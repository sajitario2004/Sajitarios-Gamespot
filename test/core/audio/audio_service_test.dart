import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/assets/assets.dart';
import 'package:sajitarios_gamespot/core/audio/audio_service.dart';

import 'support/counting_audio_service.dart';

void main() {
  group('AppSound.fileName', () {
    test(
      'delega en Assets.audio.*File (nombres relativos para flame_audio)',
      () {
        expect(AppSound.cardFlip.fileName, Assets.audio.cardFlipFile);
        expect(AppSound.reveal.fileName, Assets.audio.revealFile);
        expect(AppSound.gameOver.fileName, Assets.audio.gameOverFile);
        // Son nombres relativos, NO rutas completas.
        for (final s in AppSound.values) {
          expect(s.fileName, isNot(startsWith('assets/')));
        }
      },
    );
  });

  group('NoopAudioService', () {
    test('no rompe con enabled on/off y respeta toggle', () {
      final audio = NoopAudioService();
      expect(audio.enabled, isTrue);
      expect(audio.toggle(), isFalse);
      expect(audio.enabled, isFalse);
      audio.setEnabled(true);
      expect(audio.enabled, isTrue);
      // Ninguna llamada lanza.
      expect(audio.preload(), completes);
      audio.play(AppSound.cardFlip);
      audio.playCardFlip();
      audio.playReveal();
      audio.playGameOver();
    });
  });

  group('CountingAudioService (semántica enabled)', () {
    test('reproduce SOLO cuando enabled es true', () {
      final audio = CountingAudioService();

      audio.playCardFlip();
      audio.playReveal();
      audio.playGameOver();
      expect(audio.plays[AppSound.cardFlip], 1);
      expect(audio.plays[AppSound.reveal], 1);
      expect(audio.plays[AppSound.gameOver], 1);
      expect(audio.totalPlays, 3);

      // Silenciado: las reproducciones NO cuentan.
      audio.setEnabled(false);
      audio.playCardFlip();
      audio.play(AppSound.reveal);
      audio.playGameOver();
      expect(audio.totalPlays, 3); // sin cambios

      // Reactivado: vuelve a contar.
      audio.toggle();
      expect(audio.enabled, isTrue);
      audio.playReveal();
      expect(audio.plays[AppSound.reveal], 2);
      expect(audio.totalPlays, 4);
    });
  });

  group('FlameAudioService (sin plataforma de audio)', () {
    // NOTA: no ejercitamos aquí `preload`/`play` del FlameAudioService real
    // porque bajo `flutter test` no hay plataforma de audio y la pila de
    // flame_audio/audioplayers intenta inicializar el canal nativo, generando
    // ruido de errores async ajeno a esta fase. El comportamiento "play solo si
    // enabled" y el manejo de errores se cubre con CountingAudioService y con
    // los tests de pantalla, que usan overrides no-op. Aquí solo verificamos el
    // puente con el estado on/off, que no toca ninguna plataforma.
    test('el estado on/off se refleja en audioEnabledProvider', () {
      final container = ProviderContainer();
      addTearDown(container.dispose);
      final audio = container.read(audioServiceProvider);

      expect(audio.enabled, isTrue);
      expect(container.read(audioEnabledProvider).enabled, isTrue);

      final next = audio.toggle();
      expect(next, isFalse);
      expect(audio.enabled, isFalse);
      expect(container.read(audioEnabledProvider).enabled, isFalse);

      audio.setEnabled(true);
      expect(container.read(audioEnabledProvider).enabled, isTrue);
    });
  });

  group('AudioSettingsNotifier', () {
    test('toggle alterna y devuelve el nuevo estado', () {
      final container = ProviderContainer();
      addTearDown(container.dispose);
      final notifier = container.read(audioEnabledProvider.notifier);

      expect(notifier.enabled, isTrue);
      expect(notifier.toggle(), isFalse);
      expect(container.read(audioEnabledProvider).enabled, isFalse);
      notifier.setEnabled(true);
      expect(notifier.enabled, isTrue);
    });
  });
}
