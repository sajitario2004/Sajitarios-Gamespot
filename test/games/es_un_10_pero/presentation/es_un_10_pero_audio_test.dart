import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/audio/audio_service.dart';
import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/es_un_10_pero/presentation/es_un_10_pero_screen.dart';

import '../../../core/audio/support/counting_audio_service.dart';
import '../../../support/localized_app.dart';

Widget _harness(CountingAudioService audio, {int seed = 7}) {
  return ProviderScope(
    overrides: [
      randomProvider.overrideWithValue(RandomProvider.seeded(seed)),
      audioServiceProvider.overrideWithValue(audio),
    ],
    child: localizedApp(const EsUn10PeroScreen()),
  );
}

void main() {
  group('EsUn10PeroScreen audio', () {
    testWidgets('precarga los SFX al entrar a la pantalla', (tester) async {
      final audio = CountingAudioService();
      await tester.pumpWidget(_harness(audio));
      await tester.pump(); // ejecuta el post-frame callback

      expect(audio.preloadCount, 1);
    });

    testWidgets('reproduce cardFlip al sacar carta (sonido activo)', (
      tester,
    ) async {
      final audio = CountingAudioService();
      await tester.pumpWidget(_harness(audio));
      await tester.pump();

      await tester.tap(find.widgetWithText(FilledButton, 'Sacar carta'));
      await tester.pump();

      expect(audio.plays[AppSound.cardFlip], 1);
    });

    testWidgets('NO reproduce si está silenciado', (tester) async {
      final audio = CountingAudioService(enabled: false);
      await tester.pumpWidget(_harness(audio));
      await tester.pump();

      await tester.tap(find.widgetWithText(FilledButton, 'Sacar carta'));
      await tester.pump();

      expect(audio.totalPlays, 0);
    });
  });
}
