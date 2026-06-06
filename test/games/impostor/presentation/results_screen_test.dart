import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/impostor/data/game_history_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/game_over_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';

import '../../../support/localized_app.dart';
import 'support/fake_assign_roles_coordinator.dart';
import 'support/fake_game_history_repository.dart';

/// Lleva el flujo hasta el DESENLACE (gameOver) a partir de la [session] dada.
///
/// Recorre todas las revelaciones (reveal -> voting) y luego vota a los
/// jugadores en [votar] uno a uno hasta que la partida termina. El historial se
/// sustituye por un [FakeGameHistoryRepository] para no tocar SQLite.
///
/// El desenlace concreto lo determinan los votos: votar a los impostores hasta
/// pillarlos a todos da [VotingOutcome.jugadoresGanan]; votar a inocentes hasta
/// agotar las rondas da [VotingOutcome.impostorGana].
Future<ProviderContainer> _containerEnDesenlace(
  GameSession session, {
  required List<String> votar,
  int? rounds,
}) async {
  final container = ProviderContainer(
    overrides: [
      assignRolesCoordinatorProvider.overrideWithValue(
        FakeAssignRolesCoordinator(session: session),
      ),
      gameHistoryRepositoryProvider.overrideWithValue(
        FakeGameHistoryRepository(),
      ),
    ],
  );
  final config = GameConfig.create(
    players: session.players,
    nImpostores: session.impostorCount.clamp(1, session.players.length - 1),
    hintEnabled: false,
    rounds: rounds,
  ).config!;
  final notifier = container.read(impostorFlowControllerProvider.notifier);
  await notifier.iniciar(config);
  // Recorre a todos los jugadores hasta abrir la votación.
  var enVotacion = false;
  while (!enVotacion) {
    notifier.revelar();
    enVotacion = notifier.avanzar();
  }
  // Emite los votos indicados hasta que la partida termina.
  final porNombre = {for (final p in session.players) p.name: p};
  for (final nombre in votar) {
    if (container.read(impostorFlowControllerProvider).phase !=
        ImpostorPhase.voting) {
      break;
    }
    notifier.votar(porNombre[nombre]!);
  }
  return container;
}

Widget _harness(ProviderContainer container) {
  return UncontrolledProviderScope(
    container: container,
    child: localizedApp(const GameOverScreen()),
  );
}

/// Harness con router real (rutas `menu` y `impostor-setup`) para ejercitar las
/// acciones de fin de partida (volver al menú / jugar otra).
Widget _routerHarness(ProviderContainer container) {
  final router = GoRouter(
    initialLocation: '/game-over',
    routes: [
      GoRoute(
        path: '/',
        name: 'menu',
        builder: (_, _) => const Scaffold(body: Text('MENU')),
      ),
      GoRoute(
        path: '/game-over',
        name: 'impostor-game-over',
        builder: (_, _) => const GameOverScreen(),
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
  group('GameOverScreen (desenlace sin revelar roles)', () {
    testWidgets(
      'cuando los jugadores pillan al impostor muestra "¡Habéis ganado!" y NO '
      'revela roles ni palabra',
      (tester) async {
        final session = buildSession(
          nombres: ['Nacho', 'Iker', 'Lucía'],
          impostores: {'Nacho'},
          palabra: 'playa',
          pista: 'verano',
        );
        // Votar a Nacho (el único impostor) -> los jugadores ganan.
        final container = await _containerEnDesenlace(
          session,
          votar: ['Nacho'],
        );
        addTearDown(container.dispose);

        await tester.pumpWidget(_harness(container));
        await tester.pump();

        expect(find.text('¡Habéis ganado!'), findsOneWidget);

        // No se revelan roles ni la palabra: la UI de desenlace nunca los muestra.
        expect(find.text('IMPOSTOR'), findsNothing);
        expect(find.text('Sabía la palabra'), findsNothing);
        expect(find.text('playa'), findsNothing);

        // Acciones de fin de partida.
        expect(find.widgetWithText(FilledButton, 'Jugar otra'), findsOneWidget);
        expect(
          find.widgetWithText(OutlinedButton, 'Volver al menú'),
          findsOneWidget,
        );
      },
    );

    testWidgets(
      'cuando se agotan las rondas con impostor vivo muestra "El impostor sigue '
      'entre vosotros" sin revelar roles',
      (tester) async {
        final session = buildSession(
          nombres: ['A', 'B', 'C', 'D', 'E', 'F'],
          impostores: {'A'},
          palabra: 'pirata',
        );
        // 6 jugadores -> max rondas = 3. Votar a inocentes 3 veces agota las
        // rondas con el impostor (A) aún vivo -> gana el impostor.
        final container = await _containerEnDesenlace(
          session,
          votar: ['B', 'C', 'D'],
          rounds: 3,
        );
        addTearDown(container.dispose);

        await tester.pumpWidget(_harness(container));
        await tester.pump();

        expect(find.text('El impostor sigue entre vosotros'), findsOneWidget);
        expect(find.text('¡Habéis ganado!'), findsNothing);
        // No revela quién era el impostor.
        expect(find.text('IMPOSTOR'), findsNothing);
        expect(find.text('pirata'), findsNothing);
      },
    );

    testWidgets(
      '"Volver al menú" reinicia el flujo antes de salir (no deja la partida '
      'terminada viva)',
      (tester) async {
        final session = buildSession(
          nombres: ['Nacho', 'Iker', 'Lucía'],
          impostores: {'Nacho'},
          palabra: 'playa',
        );
        final container = await _containerEnDesenlace(
          session,
          votar: ['Nacho'],
        );
        addTearDown(container.dispose);

        // Antes de salir, el flujo está en fase gameOver con la sesión cargada.
        final estadoFinal = container.read(impostorFlowControllerProvider);
        expect(estadoFinal.phase, ImpostorPhase.gameOver);
        expect(estadoFinal.session, isNotNull);

        await tester.pumpWidget(_routerHarness(container));
        await tester.pump();

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
