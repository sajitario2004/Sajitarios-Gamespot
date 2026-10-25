/// Tests de widget para [TabuTurnScreen].
///
/// El countdown es un [Timer.periodic] real propiedad de la pantalla. Para
/// avanzar o expirar el timer en tests usamos [tester.pump(Duration)] con
/// duraciones fijas — NUNCA [pumpAndSettle] mientras el timer esté activo,
/// ya que [pumpAndSettle] cuelga al no haber frame final con un timer vivo.
/// Este es exactamente el mismo patrón que usa es_un_10_pero_screen_test.dart.
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_flow_controller.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_repositories_provider.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_turn_screen.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

import 'support/fake_tabu_word_repository.dart';

// ─── Helpers ──────────────────────────────────────────────────────────────────

/// Construye la config válida mínima de Tabú.
TabuConfig _config({int turnoSegundos = 30}) {
  return TabuConfig.create(
    equipoA: 'Rojos',
    equipoB: 'Azules',
    turnoSegundos: turnoSegundos,
  ).config!;
}

/// Harness que pone el controlador directamente en estado [TabuFase.turno].
///
/// Usa un GoRouter mínimo para que [context.goNamed] no falle al terminar
/// el turno.
Widget _harness({FakeTabuWordRepository? repo, int turnoSegundos = 30}) {
  final wordRepo = repo ?? buildFakeTabuRepo(count: 20);

  final router = GoRouter(
    initialLocation: '/tabu/turn',
    routes: [
      GoRoute(
        path: '/tabu',
        name: 'tabu-setup',
        builder: (ctx, st) => const Scaffold(body: Text('setup')),
        routes: [
          GoRoute(
            path: 'turn',
            name: 'tabu-turn',
            builder: (ctx, st) => const TabuTurnScreen(),
          ),
          GoRoute(
            path: 'scoreboard',
            name: 'tabu-scoreboard',
            builder: (ctx, st) => const Scaffold(body: Text('marcador')),
          ),
          GoRoute(
            path: 'game-over',
            name: 'tabu-game-over',
            builder: (ctx, st) => const Scaffold(body: Text('fin')),
          ),
        ],
      ),
    ],
  );

  return ProviderScope(
    overrides: [
      tabuWordRepositoryProvider.overrideWith((ref) => Future.value(wordRepo)),
      randomProvider.overrideWithValue(RandomProvider.seeded(42)),
    ],
    child: Consumer(
      builder: (context, ref, _) {
        // Pre-carga el estado turno antes de renderizar la pantalla.
        WidgetsBinding.instance.addPostFrameCallback((_) async {
          await ref
              .read(tabuFlowControllerProvider.notifier)
              .iniciar(_config(turnoSegundos: turnoSegundos));
        });
        return MaterialApp.router(
          locale: const Locale('es'),
          localizationsDelegates: AppLocalizations.localizationsDelegates,
          supportedLocales: AppLocalizations.supportedLocales,
          routerConfig: router,
        );
      },
    ),
  );
}

void main() {
  group('TabuTurnScreen', () {
    testWidgets('muestra la palabra secreta y los tres botones de acción', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      // Dar tiempo al iniciar() async y al postFrameCallback.
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      // Los tres botones deben estar presentes.
      expect(find.text('Acierto'), findsOneWidget);
      expect(find.text('Saltar'), findsOneWidget);
      expect(find.text('Falta'), findsOneWidget);
    });

    testWidgets('acierto incrementa el contador de aciertos del turno', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      // Inicialmente 0 aciertos.
      expect(find.text('0 aciertos'), findsOneWidget);

      await tester.tap(find.text('Acierto'));
      await tester.pump();

      // Después de un acierto: 1 acierto.
      expect(find.text('1 acierto'), findsOneWidget);
    });

    testWidgets('al expirar el timer navega al marcador (scoreboard)', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      // Usamos turno muy corto (30s mínimo — avanzamos el tiempo manualmente).
      await tester.pumpWidget(_harness(turnoSegundos: 30));
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      // Avanzamos el tiempo para que expiren los 30 segundos del turno.
      // Cada tick del timer es 1 segundo; bombeamos 31 ticks.
      for (var i = 0; i < 31; i++) {
        await tester.pump(const Duration(seconds: 1));
      }

      // Tras expirar, debe estar en el marcador.
      expect(find.text('marcador'), findsOneWidget);
    });
  });
}
