import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/audio/audio_service.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/reveal_screen.dart';

import '../../../core/audio/support/counting_audio_service.dart';
import '../../../support/localized_app.dart';
import 'support/fake_assign_roles_coordinator.dart';

Future<ProviderContainer> _containerEnReveal(
  GameSession session,
  CountingAudioService audio,
) async {
  final container = ProviderContainer(
    overrides: [
      assignRolesCoordinatorProvider.overrideWithValue(
        FakeAssignRolesCoordinator(session: session),
      ),
      audioServiceProvider.overrideWithValue(audio),
    ],
  );
  final config = GameConfig.create(
    players: session.players,
    nImpostores: 1,
    hintEnabled: false,
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

void main() {
  group('RevealScreen audio', () {
    testWidgets('reproduce reveal al pulsar "Revelar" (sonido activo)', (
      tester,
    ) async {
      final audio = CountingAudioService();
      final session = buildSession(
        nombres: ['Iker', 'Nacho', 'Lucía'],
        impostores: {'Nacho'},
        palabra: 'pirata',
      );
      final container = await _containerEnReveal(session, audio);
      addTearDown(container.dispose);

      await tester.pumpWidget(_harness(container));
      await tester.tap(find.widgetWithText(FilledButton, 'Revelar'));
      await tester.pump();

      expect(audio.plays[AppSound.reveal], 1);
    });

    testWidgets('NO reproduce si está silenciado', (tester) async {
      final audio = CountingAudioService(enabled: false);
      final session = buildSession(
        nombres: ['Iker', 'Nacho', 'Lucía'],
        impostores: {'Nacho'},
        palabra: 'pirata',
      );
      final container = await _containerEnReveal(session, audio);
      addTearDown(container.dispose);

      await tester.pumpWidget(_harness(container));
      await tester.tap(find.widgetWithText(FilledButton, 'Revelar'));
      await tester.pump();

      expect(audio.totalPlays, 0);
    });
  });
}
