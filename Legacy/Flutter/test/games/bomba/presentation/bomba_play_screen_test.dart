/// Tests de la BombaPlayScreen.
///
/// Los tests de renderizado y PASAR usan la pantalla directamente (sin
/// GoRouter) con ProviderScope y estado pre-inicializado. GoRouter introduce
/// latencia async de navegación que hace inestables los pumps. Para esas
/// verificaciones solo necesitamos que la pantalla reciba el estado correcto.
///
/// Los tests de controller-driven (explotar/gameOver) verifican el estado del
/// controlador directamente — son 100% deterministas, sin depender del Timer.
///
/// CRÍTICO: nunca usar pumpAndSettle mientras el fuse Timer o PulseGlow estén
/// activos — usa pump(Duration) fija para drenar timers al final.
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/bomba/data/bomba_prompt_repository.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_flow_controller.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_play_screen.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_repositories_provider.dart';

import '../../../support/localized_app.dart';
import 'support/fake_bomba_prompt_repository.dart';

BombaConfig _makeConfig(List<String> players) {
  final r = BombaConfig.create(
    mode: BombaMode.silaba,
    playerNames: players,
    minSegundos: 10,
    maxSegundos: 11,
  );
  assert(r.isSuccess, 'BombaConfig inválida: ${r.error}');
  return r.config!;
}

/// Construye un [ProviderContainer] con el flujo pre-inicializado en jugando.
/// Llama a [iniciar] FUERA de testWidgets (real async, no fake-async).
Future<ProviderContainer> _makePlayingContainer(
  List<String> players,
  BombaPromptRepository repo,
) async {
  final config = _makeConfig(players);
  final container = ProviderContainer(
    overrides: [
      randomProvider.overrideWithValue(RandomProvider.seeded(0)),
      bombaPromptRepositoryProvider.overrideWith((_) => Future.value(repo)),
    ],
  );
  await container.read(bombaFlowControllerProvider.notifier).iniciar(config);
  assert(
    container.read(bombaFlowControllerProvider).fase == BombaFase.jugando,
    'iniciar() debe dejar el estado en jugando',
  );
  return container;
}

/// Harness minimalista: [BombaPlayScreen] envuelta en [UncontrolledProviderScope]
/// + [localizedApp]. Sin GoRouter → la pantalla no puede navegar pero sí
/// renderiza y acepta taps.
Widget _makeWidget(ProviderContainer container) => UncontrolledProviderScope(
  container: container,
  child: localizedApp(const BombaPlayScreen()),
);

/// Desmonta la pantalla para que dispose() cancele el fuse Timer y el
/// AnimationController de PulseGlow. Esto es más seguro que avanzar un
/// gran bloque de tiempo fake-async porque el Timer periódico de la mecha
/// + el Future.delayed del overlay pueden encadenarse indefinidamente
/// (explosión → ronda nueva → nueva mecha) dentro del mismo pump window,
/// generando un bucle sin fin y miles de frames de animación.
Future<void> _drain(WidgetTester tester) async {
  await tester.pumpWidget(const SizedBox());
  await tester.pump();
}

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('BombaPlayScreen — renderizado', () {
    late ProviderContainer container;

    setUp(() async {
      final repo = await makeFakeRepo();
      container = await _makePlayingContainer(['Ana', 'Luis', 'Marta'], repo);
    });

    tearDown(() => container.dispose());

    testWidgets('muestra el portador inicial', (tester) async {
      await tester.pumpWidget(_makeWidget(container));
      await tester.pump(); // primer frame
      await tester.pump(); // PulseGlow / Timer start

      final holder = container.read(bombaFlowControllerProvider).currentHolder!;
      // El portador se muestra en mayúsculas.
      expect(find.text(holder.toUpperCase()), findsOneWidget);
      expect(find.text('PASAR'), findsOneWidget);

      await _drain(tester);
    });

    testWidgets('NO muestra ningún número de cuenta atrás', (tester) async {
      await tester.pumpWidget(_makeWidget(container));
      await tester.pump();
      await tester.pump();

      for (final t in tester.widgetList<Text>(find.byType(Text))) {
        final data = t.data ?? '';
        // Un número de 2+ dígitos suelto indicaría countdown visible.
        expect(
          RegExp(r'^\d{2,3}$').hasMatch(data),
          isFalse,
          reason: 'No debe mostrarse cuenta atrás numérica: "$data"',
        );
      }

      await _drain(tester);
    });
  });

  group('BombaPlayScreen — PASAR', () {
    late ProviderContainer container;

    setUp(() async {
      final repo = await makeFakeRepo();
      container = await _makePlayingContainer(['Ana', 'Luis', 'Marta'], repo);
    });

    tearDown(() => container.dispose());

    testWidgets('avanza el portador al siguiente jugador', (tester) async {
      await tester.pumpWidget(_makeWidget(container));
      await tester.pump();
      await tester.pump();

      final initial = container
          .read(bombaFlowControllerProvider)
          .currentHolder!
          .toUpperCase();
      expect(find.text(initial), findsOneWidget);

      await tester.tap(find.text('PASAR'));
      await tester.pump();

      final next = container
          .read(bombaFlowControllerProvider)
          .currentHolder!
          .toUpperCase();
      expect(next, isNot(initial));
      expect(find.text(next), findsOneWidget);

      await _drain(tester);
    });

    testWidgets('múltiples PASAR rotan correctamente', (tester) async {
      await tester.pumpWidget(_makeWidget(container));
      await tester.pump();
      await tester.pump();

      await tester.tap(find.text('PASAR'));
      await tester.pump();
      await tester.tap(find.text('PASAR'));
      await tester.pump();

      // Después de 2 pases desde Ana: Ana→Luis→Marta
      final holder = container.read(bombaFlowControllerProvider).currentHolder;
      expect(holder, 'Marta');
      expect(find.text('MARTA'), findsOneWidget);

      await _drain(tester);
    });
  });

  // ── Explosión — controller-driven ─────────────────────────────────────────
  // Llama a explotar()/continuarTrasExplosion() directamente — no depende del
  // Timer de la pantalla, 100% determinista.
  //
  // NOTA: makeFakeRepo() y _makePlayingContainer() usan sqflite_ffi, que
  // internamente despacha operaciones vía dart:isolate. Esas operaciones no
  // pueden resolverse dentro de la zona FakeAsync de testWidgets — deben
  // ejecutarse con tester.runAsync() para escapar al event loop real.
  group('BombaPlayScreen — explosión (controller-driven)', () {
    testWidgets('explotar() pone el controlador en fase explotando', (
      tester,
    ) async {
      final c = await tester.runAsync(() async {
        final repo = await makeFakeRepo();
        return _makePlayingContainer(['Ana', 'Luis'], repo);
      });
      addTearDown(c!.dispose);

      await tester.pumpWidget(_makeWidget(c));
      await tester.pump();
      await tester.pump();

      c.read(bombaFlowControllerProvider.notifier).explotar();
      await tester.pump();

      final state = c.read(bombaFlowControllerProvider);
      expect(state.fase, BombaFase.explotando);
      expect(state.eliminado, isNotNull);

      await _drain(tester);
    });

    testWidgets('explotar()+continuar() con 3 jugadores → jugando', (
      tester,
    ) async {
      final c = await tester.runAsync(() async {
        final repo = await makeFakeRepo();
        return _makePlayingContainer(['Ana', 'Luis', 'Marta'], repo);
      });
      addTearDown(c!.dispose);

      await tester.pumpWidget(_makeWidget(c));
      await tester.pump();
      await tester.pump();

      c.read(bombaFlowControllerProvider.notifier).explotar();
      await tester.pump();
      expect(c.read(bombaFlowControllerProvider).fase, BombaFase.explotando);

      c.read(bombaFlowControllerProvider.notifier).continuarTrasExplosion();
      await tester.pump();

      expect(
        c.read(bombaFlowControllerProvider).fase,
        BombaFase.jugando,
        reason: 'Con 2 jugadores vivos debe continuar jugando',
      );
      expect(find.text('PASAR'), findsOneWidget);

      await _drain(tester);
    });

    testWidgets('explotar()+continuar() con 2 jugadores → gameOver', (
      tester,
    ) async {
      final c = await tester.runAsync(() async {
        final repo = await makeFakeRepo();
        return _makePlayingContainer(['Ana', 'Luis'], repo);
      });
      addTearDown(c!.dispose);

      await tester.pumpWidget(_makeWidget(c));
      await tester.pump();
      await tester.pump();

      c.read(bombaFlowControllerProvider.notifier).explotar();
      await tester.pump();
      c.read(bombaFlowControllerProvider.notifier).continuarTrasExplosion();
      await tester.pump();

      expect(
        c.read(bombaFlowControllerProvider).fase,
        BombaFase.gameOver,
        reason: 'Con 1 jugador vivo debe pasar a gameOver',
      );

      await _drain(tester);
    });
  });
}
