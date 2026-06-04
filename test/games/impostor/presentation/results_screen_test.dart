import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/results_screen.dart';

import '../../../support/localized_app.dart';
import 'support/fake_assign_roles_coordinator.dart';

/// Lleva el flujo hasta la fase de resultados con la [session] dada.
Future<ProviderContainer> _containerEnResultados(GameSession session) async {
  final container = ProviderContainer(
    overrides: [
      assignRolesCoordinatorProvider.overrideWithValue(
        FakeAssignRolesCoordinator(session: session),
      ),
    ],
  );
  final config = GameConfig.create(
    players: session.players,
    nImpostores: session.impostorCount.clamp(1, session.players.length - 1),
    hintEnabled: false,
  ).config!;
  final notifier = container.read(impostorFlowControllerProvider.notifier);
  await notifier.iniciar(config);
  // Recorre a todos los jugadores hasta llegar a resultados.
  var terminado = false;
  while (!terminado) {
    notifier.revelar();
    terminado = notifier.avanzar();
  }
  return container;
}

Widget _harness(ProviderContainer container) {
  return UncontrolledProviderScope(
    container: container,
    child: localizedApp(const ResultsScreen()),
  );
}

/// Harness con router real (rutas `menu` y `impostor-results`) para poder
/// ejercitar las acciones de navegación (volver al menú / jugar otra).
Widget _routerHarness(ProviderContainer container) {
  final router = GoRouter(
    initialLocation: '/results',
    routes: [
      GoRoute(
        path: '/',
        name: 'menu',
        builder: (_, _) => const Scaffold(body: Text('MENU')),
      ),
      GoRoute(
        path: '/results',
        name: 'impostor-results',
        builder: (_, _) => const ResultsScreen(),
      ),
      GoRoute(
        path: '/setup',
        name: 'impostor-setup',
        builder: (_, _) => const Scaffold(body: Text('SETUP')),
      ),
    ],
  );
  return UncontrolledProviderScope(
    container: container,
    child: localizedRouterApp(router),
  );
}

void main() {
  group('ResultsScreen', () {
    testWidgets('lista a TODOS los jugadores con su rol y muestra la palabra', (
      tester,
    ) async {
      final session = buildSession(
        nombres: ['Nacho', 'Iker', 'Lucía'],
        impostores: {'Nacho'},
        palabra: 'playa',
        pista: 'verano',
      );
      final container = await _containerEnResultados(session);
      addTearDown(container.dispose);

      await tester.pumpWidget(_harness(container));
      await tester.pump();

      // Todos los jugadores aparecen.
      expect(find.text('Nacho'), findsOneWidget);
      expect(find.text('Iker'), findsOneWidget);
      expect(find.text('Lucía'), findsOneWidget);

      // El impostor está etiquetado como tal y los demás como que sabían.
      expect(find.text('IMPOSTOR'), findsOneWidget);
      expect(find.text('Sabía la palabra'), findsNWidgets(2));

      // Se muestra la palabra de la ronda.
      expect(find.text('playa'), findsOneWidget);

      // Acciones de fin de partida.
      expect(find.widgetWithText(FilledButton, 'Jugar otra'), findsOneWidget);
      expect(
        find.widgetWithText(OutlinedButton, 'Volver al menú'),
        findsOneWidget,
      );
    });

    testWidgets('con varios impostores los etiqueta a todos correctamente', (
      tester,
    ) async {
      final session = buildSession(
        nombres: ['A', 'B', 'C', 'D'],
        impostores: {'A', 'C'},
        palabra: 'pirata',
      );
      final container = await _containerEnResultados(session);
      addTearDown(container.dispose);

      // Superficie alta para que la lista de 4 jugadores quepa sin scroll.
      tester.view.physicalSize = const Size(1200, 3000);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness(container));
      await tester.pump();

      expect(find.text('IMPOSTOR'), findsNWidgets(2));
      expect(find.text('Sabía la palabra'), findsNWidgets(2));
      expect(find.text('Había 2 impostores.'), findsOneWidget);
    });

    testWidgets(
      '"Volver al menú" reinicia el flujo antes de salir (no deja la sesión '
      'terminada viva)',
      (tester) async {
        final session = buildSession(
          nombres: ['Nacho', 'Iker', 'Lucía'],
          impostores: {'Nacho'},
          palabra: 'playa',
        );
        final container = await _containerEnResultados(session);
        addTearDown(container.dispose);

        // Antes de salir, el flujo está en fase results con la sesión cargada.
        final estadoFinal = container.read(impostorFlowControllerProvider);
        expect(estadoFinal.phase, ImpostorPhase.results);
        expect(estadoFinal.session, isNotNull);

        await tester.pumpWidget(_routerHarness(container));
        await tester.pumpAndSettle();

        await tester.tap(find.widgetWithText(OutlinedButton, 'Volver al menú'));
        await tester.pumpAndSettle();

        // Navegó al menú y el flujo quedó reiniciado (setup, sin sesión).
        expect(find.text('MENU'), findsOneWidget);
        final estadoTrasVolver = container.read(impostorFlowControllerProvider);
        expect(estadoTrasVolver.phase, ImpostorPhase.setup);
        expect(estadoTrasVolver.session, isNull);
      },
    );
  });
}
