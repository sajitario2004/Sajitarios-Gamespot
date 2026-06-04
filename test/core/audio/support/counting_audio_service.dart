import 'package:sajitarios_gamespot/core/audio/audio_service.dart';

/// Fake de [AudioService] que CUENTA las reproducciones, para tests.
///
/// Respeta la semántica del contrato: si [enabled] es `false`, [play] (y los
/// helpers) NO registran ninguna reproducción (igual que el servicio real no
/// emite sonido). Nunca toca ninguna plataforma de audio.
class CountingAudioService implements AudioService {
  CountingAudioService({bool enabled = true}) : _enabled = enabled;

  bool _enabled;

  /// Nº de reproducciones EFECTIVAS por sonido (solo cuando estaba `enabled`).
  final Map<AppSound, int> plays = <AppSound, int>{
    for (final s in AppSound.values) s: 0,
  };

  /// Nº de veces que se invocó [preload].
  int preloadCount = 0;

  /// Total de reproducciones efectivas.
  int get totalPlays => plays.values.fold(0, (a, b) => a + b);

  @override
  bool get enabled => _enabled;

  @override
  void setEnabled(bool value) => _enabled = value;

  @override
  bool toggle() => _enabled = !_enabled;

  @override
  Future<void> preload() async => preloadCount++;

  @override
  void play(AppSound sound, {double volume = 1.0}) {
    if (!_enabled) return;
    plays[sound] = plays[sound]! + 1;
  }

  @override
  void playCardFlip() => play(AppSound.cardFlip);

  @override
  void playReveal() => play(AppSound.reveal);

  @override
  void playGameOver() => play(AppSound.gameOver);
}
