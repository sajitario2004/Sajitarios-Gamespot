import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/reveal_screen.dart';

import '../../../support/localized_app.dart';
import 'support/fake_assign_roles_coordinator.dart';

/// Crea un [ProviderContainer] con el coordinador falso devolviendo [session],
/// inicia la partida (fase pass) y la lleva a fase reveal del primer jugador.
Future<ProviderContainer> _containerEnReveal(GameSession session) async {
  final container = ProviderContainer(
    overrides: [
      assignRolesCoordinatorProvider.overrideWithValue(
        FakeAssignRolesCoordinator(session: session),
      ),
    ],
  );
  final config = GameConfig.create(
    players: session.players,
    nImpostores: 1,
    hintEnabled: session.isImpostor(session.players.first),
  ).config!;
  final notifier = container.read(impostorFlowControllerProvider.notifier);
  await notifier.iniciar(config);
  notifier.revelar();
  return container;
}

Widget _harness(ProviderContainer container) {
  return UncontrolledProviderScope(
    container: container,
    child: localizedApp(const RevealScreen()),
  );
}

/// Harness con router real para ejercitar el botón "atrás" del sistema
/// (interceptado con `PopScope`) durante la revelación.
Widget _routerHarness(ProviderContainer container) {
  final router = GoRouter(
    initialLocation: '/reveal',
    routes: [
      GoRoute(
        path: '/setup',
        name: 'impostor-setup',
        builder: (_, _) => const Scaffold(body: Text('SETUP')),
      ),
      GoRoute(
        path: '/reveal',
        name: 'impostor-reveal',
        builder: (_, _) => const RevealScreen(),
      ),
    ],
  );
  return UncontrolledProviderScope(
    container: container,
    child: localizedRouterApp(router),
  );
}

void main() {
  group('RevealScreen', () {
    testWidgets('no muestra la palabra hasta pulsar "Revelar" (caso palabra)', (
      tester,
    ) async {
      // El primer jugador (Iker) NO es impostor: debe ver la palabra.
      final session = buildSession(
        nombres: ['Iker', 'Nacho', 'Lucía'],
        impostores: {'Nacho'},
        palabra: 'pirata',
      );
      final container = await _containerEnReveal(session);
      addTearDown(container.dispose);

      await tester.pumpWidget(_harness(container));

      // Antes de revelar: turno del jugador, botón "Revelar" visible y la
      // palabra OCULTA.
      expect(find.text('Iker'), findsOneWidget);
      expect(find.widgetWithText(FilledButton, 'Revelar'), findsOneWidget);
      expect(find.text('pirata'), findsNothing);
      expect(find.text('IMPOSTOR'), findsNothing);

      // Pulsa "Revelar".
      await tester.tap(find.widgetWithText(FilledButton, 'Revelar'));
      await tester.pump();

      // Ahora sí se ve la palabra y NO "IMPOSTOR".
      expect(find.text('pirata'), findsOneWidget);
      expect(find.text('IMPOSTOR'), findsNothing);
    });

    testWidgets(
      'el impostor ve "IMPOSTOR" sin pista cuando hintEnabled es false',
      (tester) async {
        // El primer jugador (Nacho) es impostor; pista desactivada.
        final session = buildSession(
          nombres: ['Nacho', 'Iker', 'Lucía'],
          impostores: {'Nacho'},
          palabra: 'pirata',
          pista: 'barco',
        );
        final container = ProviderContainer(
          overrides: [
            assignRolesCoordinatorProvider.overrideWithValue(
              FakeAssignRolesCoordinator(session: session),
            ),
          ],
        );
        addTearDown(container.dispose);
        final config = GameConfig.create(
          players: session.players,
          nImpostores: 1,
          hintEnabled: false,
        ).config!;
        final notifier = container.read(
          impostorFlowControllerProvider.notifier,
        );
        await notifier.iniciar(config);
        notifier.revelar();

        await tester.pumpWidget(_harness(container));

        await tester.tap(find.widgetWithText(FilledButton, 'Revelar'));
        await tester.pump();

        expect(find.text('IMPOSTOR'), findsOneWidget);
        // Sin pista: ni el texto "Pista" ni el valor de la pista.
        expect(find.text('Pista'), findsNothing);
        expect(find.text('barco'), findsNothing);
        // El impostor nunca ve la palabra.
        expect(find.text('pirata'), findsNothing);
      },
    );

    testWidgets(
      'el impostor ve "IMPOSTOR" + pista cuando hintEnabled es true',
      (tester) async {
        final session = buildSession(
          nombres: ['Nacho', 'Iker', 'Lucía'],
          impostores: {'Nacho'},
          palabra: 'pirata',
          pista: 'barco',
        );
        final container = ProviderContainer(
          overrides: [
            assignRolesCoordinatorProvider.overrideWithValue(
              FakeAssignRolesCoordinator(session: session),
            ),
          ],
        );
        addTearDown(container.dispose);
        final config = GameConfig.create(
          players: session.players,
          nImpostores: 1,
          hintEnabled: true,
        ).config!;
        final notifier = container.read(
          impostorFlowControllerProvider.notifier,
        );
        await notifier.iniciar(config);
        notifier.revelar();

        await tester.pumpWidget(_harness(container));

        await tester.tap(find.widgetWithText(FilledButton, 'Revelar'));
        await tester.pump();

        expect(find.text('IMPOSTOR'), findsOneWidget);
        expect(find.text('Pista'), findsOneWidget);
        expect(find.text('barco'), findsOneWidget);
        expect(find.text('pirata'), findsNothing);
      },
    );

    testWidgets(
      'el botón "atrás" del sistema pide confirmación; al cancelar sigue en la '
      'partida (no deja un estado raro)',
      (tester) async {
        final session = buildSession(
          nombres: ['Iker', 'Nacho', 'Lucía'],
          impostores: {'Nacho'},
          palabra: 'pirata',
        );
        final container = await _containerEnReveal(session);
        addTearDown(container.dispose);

        await tester.pumpWidget(_routerHarness(container));
        await tester.pumpAndSettle();

        // Simula el botón "atrás" del sistema.
        final handled = await tester.binding.handlePopRoute();
        await tester.pumpAndSettle();

        // El PopScope intercepta el pop y muestra la confirmación.
        expect(handled, isTrue);
        expect(find.text('¿Salir de la partida?'), findsOneWidget);

        // Al elegir seguir jugando, vuelve a la revelación y el flujo intacto.
        await tester.tap(find.widgetWithText(TextButton, 'Seguir jugando'));
        await tester.pumpAndSettle();

        expect(find.text('¿Salir de la partida?'), findsNothing);
        expect(find.text('Iker'), findsOneWidget);
        expect(
          container.read(impostorFlowControllerProvider).phase,
          ImpostorPhase.reveal,
        );
      },
    );

    testWidgets(
      'el botón "atrás" del sistema, al confirmar, reinicia el flujo y vuelve a '
      'configuración',
      (tester) async {
        final session = buildSession(
          nombres: ['Iker', 'Nacho', 'Lucía'],
          impostores: {'Nacho'},
          palabra: 'pirata',
        );
        final container = await _containerEnReveal(session);
        addTearDown(container.dispose);

        await tester.pumpWidget(_routerHarness(container));
        await tester.pumpAndSettle();

        await tester.binding.handlePopRoute();
        await tester.pumpAndSettle();

        await tester.tap(find.widgetWithText(FilledButton, 'Salir'));
        await tester.pumpAndSettle();

        expect(find.text('SETUP'), findsOneWidget);
        final estado = container.read(impostorFlowControllerProvider);
        expect(estado.phase, ImpostorPhase.setup);
        expect(estado.session, isNull);
      },
    );
  });
}
