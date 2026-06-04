import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/pass_device_screen.dart';

import '../../../support/localized_app.dart';
import 'support/fake_assign_roles_coordinator.dart';

/// Crea un [ProviderContainer] con el coordinador falso devolviendo [session] e
/// inicia la partida, dejándola en fase pass apuntando al primer jugador.
Future<ProviderContainer> _containerEnPass(GameSession session) async {
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
  ).config!;
  await container.read(impostorFlowControllerProvider.notifier).iniciar(config);
  return container;
}

Widget _harness(ProviderContainer container) {
  return UncontrolledProviderScope(
    container: container,
    child: localizedApp(const PassDeviceScreen()),
  );
}

void main() {
  group('PassDeviceScreen', () {
    testWidgets('muestra el jugador actual (primero del orden) y su posición', (
      tester,
    ) async {
      final session = buildSession(
        nombres: ['Nacho', 'Iker', 'Lucía'],
        impostores: {'Nacho'},
      );
      final container = await _containerEnPass(session);
      addTearDown(container.dispose);

      await tester.pumpWidget(_harness(container));

      expect(find.text('Pásale el móvil a'), findsOneWidget);
      expect(find.text('Nacho'), findsOneWidget);
      expect(find.text('Jugador 1 de 3'), findsOneWidget);
      expect(find.widgetWithText(FilledButton, 'Continuar'), findsOneWidget);
      // No filtra el rol: ni la palabra ni "IMPOSTOR" aparecen aquí.
      expect(find.text('IMPOSTOR'), findsNothing);
      expect(find.text('playa'), findsNothing);
    });

    testWidgets('refleja el jugador correcto tras avanzar de jugador', (
      tester,
    ) async {
      final session = buildSession(
        nombres: ['Nacho', 'Iker', 'Lucía'],
        impostores: {'Nacho'},
      );
      final container = await _containerEnPass(session);
      addTearDown(container.dispose);

      // Avanza al segundo jugador (revelar + avanzar deja en pass index 1).
      final notifier = container.read(impostorFlowControllerProvider.notifier);
      notifier.revelar();
      notifier.avanzar();

      await tester.pumpWidget(_harness(container));

      expect(find.text('Iker'), findsOneWidget);
      expect(find.text('Jugador 2 de 3'), findsOneWidget);
      expect(find.text('Nacho'), findsNothing);
    });
  });
}
