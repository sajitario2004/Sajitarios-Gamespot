import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/audio/audio_service.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/results_screen.dart';

import '../../../core/audio/support/counting_audio_service.dart';
import '../../../support/localized_app.dart';
import 'support/fake_assign_roles_coordinator.dart';

Future<ProviderContainer> _containerEnResultados(
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
    nImpostores: session.impostorCount.clamp(1, session.players.length - 1),
    hintEnabled: false,
  ).config!;
  final notifier = container.read(impostorFlowControllerProvider.notifier);
  await notifier.iniciar(config);
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

void main() {
  group('ResultsScreen audio', () {
    testWidgets('reproduce gameOver al llegar a resultados (sonido activo)', (
      tester,
    ) async {
      final audio = CountingAudioService();
      final session = buildSession(
        nombres: ['Iker', 'Nacho', 'Lucía'],
        impostores: {'Nacho'},
        palabra: 'pirata',
      );
      final container = await _containerEnResultados(session, audio);
      addTearDown(container.dispose);

      await tester.pumpWidget(_harness(container));
      await tester.pump(); // ejecuta el post-frame callback

      expect(audio.plays[AppSound.gameOver], 1);
    });

    testWidgets('NO reproduce si está silenciado', (tester) async {
      final audio = CountingAudioService(enabled: false);
      final session = buildSession(
        nombres: ['Iker', 'Nacho', 'Lucía'],
        impostores: {'Nacho'},
        palabra: 'pirata',
      );
      final container = await _containerEnResultados(session, audio);
      addTearDown(container.dispose);

      await tester.pumpWidget(_harness(container));
      await tester.pump();

      expect(audio.totalPlays, 0);
    });
  });
}
