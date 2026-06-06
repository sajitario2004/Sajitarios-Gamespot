import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/voting_screen.dart';

import '../../../support/localized_app.dart';
import 'support/fake_assign_roles_coordinator.dart';

/// Inicia una partida con [session] y [rounds] y la deja en la VOTACIÓN
/// (ronda 1), sin tocar BD ni aleatoriedad real.
Future<ProviderContainer> _containerEnVotacion(
  GameSession session, {
  required int rounds,
}) async {
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
    rounds: rounds,
  ).config!;
  final notifier = container.read(impostorFlowControllerProvider.notifier);
  await notifier.iniciar(config);
  var enVotacion = false;
  while (!enVotacion) {
    notifier.revelar();
    enVotacion = notifier.avanzar();
  }
  return container;
}

/// Harness con router real: la [VotingScreen] navega a la pantalla de desenlace
/// al alcanzar gameOver, así que necesitamos una ruta destino para esa
/// transición (representada aquí por un placeholder).
Widget _harness(ProviderContainer container) {
  final router = GoRouter(
    initialLocation: '/voting',
    routes: [
      GoRoute(
        path: '/voting',
        name: 'impostor-voting',
        builder: (_, _) => const VotingScreen(),
      ),
      GoRoute(
        path: '/game-over',
        name: 'impostor-game-over',
        builder: (_, _) => const Scaffold(body: Text('GAME-OVER')),
      ),
      GoRoute(
        path: '/',
        name: 'menu',
        builder: (_, _) => const Scaffold(body: Text('MENU')),
      ),
    ],
  );
  return UncontrolledProviderScope(
    container: container,
    child: localizedRouterApp(router),
  );
}

void main() {
  group('VotingScreen', () {
    testWidgets('muestra "Ronda 1 de 3" y la lista de candidatos', (
      tester,
    ) async {
      final session = buildSession(
        nombres: ['Nacho', 'Iker', 'Lucía', 'Ana', 'Leo', 'Sara'],
        impostores: {'Nacho'},
      );
      final container = await _containerEnVotacion(session, rounds: 3);
      addTearDown(container.dispose);

      await tester.pumpWidget(_harness(container));
      await tester.pump();

      expect(find.text('Ronda 1 de 3'), findsOneWidget);
      expect(find.text('Votad a quien creáis impostor'), findsOneWidget);
      // Todos los jugadores aparecen como candidatos.
      for (final nombre in ['Nacho', 'Iker', 'Lucía', 'Ana', 'Leo', 'Sara']) {
        expect(find.text(nombre), findsOneWidget);
      }
    });

    testWidgets(
      'votar a un impostor: confirma la expulsión y termina la partida '
      '(navega al desenlace)',
      (tester) async {
        final session = buildSession(
          nombres: ['Nacho', 'Iker', 'Lucía', 'Ana', 'Leo', 'Sara'],
          impostores: {'Nacho'},
        );
        final container = await _containerEnVotacion(session, rounds: 3);
        addTearDown(container.dispose);

        await tester.pumpWidget(_harness(container));
        await tester.pump();

        // Pulsar al candidato Nacho abre el diálogo de confirmación.
        await tester.tap(find.text('Nacho'));
        await tester.pumpAndSettle();
        expect(find.text('¿Expulsar a Nacho?'), findsOneWidget);

        // Confirmar la expulsión: como Nacho es el único impostor, ganan los
        // jugadores y se navega al desenlace.
        await tester.tap(find.widgetWithText(FilledButton, 'Expulsar'));
        await tester.pumpAndSettle();

        final state = container.read(impostorFlowControllerProvider);
        expect(state.phase, ImpostorPhase.gameOver);
        expect(state.outcome, VotingOutcome.jugadoresGanan);
        expect(find.text('GAME-OVER'), findsOneWidget);
      },
    );

    testWidgets(
      'votar a un inocente: avanza de ronda y muestra "El impostor sigue entre '
      'vosotros"',
      (tester) async {
        final session = buildSession(
          nombres: ['Nacho', 'Iker', 'Lucía', 'Ana', 'Leo', 'Sara'],
          impostores: {'Nacho'},
        );
        final container = await _containerEnVotacion(session, rounds: 3);
        addTearDown(container.dispose);

        await tester.pumpWidget(_harness(container));
        await tester.pump();

        await tester.tap(find.text('Iker'));
        await tester.pumpAndSettle();
        await tester.tap(find.widgetWithText(FilledButton, 'Expulsar'));
        await tester.pumpAndSettle();

        // Sigue en votación, ahora en la ronda 2, con el mensaje de feedback.
        final state = container.read(impostorFlowControllerProvider);
        expect(state.phase, ImpostorPhase.voting);
        expect(state.rondaActual, 2);
        expect(find.text('Ronda 2 de 3'), findsOneWidget);
        expect(find.text('El impostor sigue entre vosotros'), findsOneWidget);
      },
    );

    testWidgets('cancelar la confirmación no emite el voto', (tester) async {
      final session = buildSession(
        nombres: ['Nacho', 'Iker', 'Lucía', 'Ana', 'Leo', 'Sara'],
        impostores: {'Nacho'},
      );
      final container = await _containerEnVotacion(session, rounds: 3);
      addTearDown(container.dispose);

      await tester.pumpWidget(_harness(container));
      await tester.pump();

      await tester.tap(find.text('Nacho'));
      await tester.pumpAndSettle();
      await tester.tap(find.widgetWithText(TextButton, 'Cancelar'));
      await tester.pumpAndSettle();

      // Sigue en la ronda 1 sin nadie eliminado.
      final state = container.read(impostorFlowControllerProvider);
      expect(state.phase, ImpostorPhase.voting);
      expect(state.rondaActual, 1);
      expect(state.eliminados, isEmpty);
    });
  });
}
