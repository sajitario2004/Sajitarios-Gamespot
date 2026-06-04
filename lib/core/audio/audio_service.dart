/// Servicio de audio (efectos de sonido) de la app.
///
/// Define un contrato ABSTRACTO ([AudioService]) e inyectable vía Riverpod
/// ([audioServiceProvider]) para que las pantallas y los juegos reproduzcan
/// efectos sin acoplarse a `flame_audio`.
///
/// Diseño:
/// - [AudioService] es la interfaz: `preload`, helpers semánticos
///   ([playCardFlip]/[playReveal]/[playGameOver]) y un [play] genérico que toma
///   un [AppSound]; más el estado on/off ([enabled]/[setEnabled]/[toggle]).
/// - [FlameAudioService] es la implementación por defecto sobre `flame_audio`.
///   TODAS sus llamadas a `flame_audio` van envueltas en try/catch: si el audio
///   falla, no hay plataforma (tests) o el fichero no existe todavía, se ignora
///   silenciosamente y la UI nunca se rompe.
/// - [NoopAudioService] es un fake no-op para tests: no toca ninguna plataforma.
///
/// El estado on/off vive en [AudioSettings] ([AudioSettingsNotifier] +
/// [audioEnabledProvider]); el servicio lo consulta antes de reproducir.
library;

import 'package:flame_audio/flame_audio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'package:sajitarios_gamespot/core/assets/assets.dart';

/// Efectos de sonido disponibles en la app.
///
/// Cada valor conoce su [fileName] relativo a `assets/audio/` (lo que espera
/// `flame_audio`), tomado de [Assets.audio] para no duplicar rutas.
enum AppSound {
  /// Volteo de carta ("Es un 10 pero").
  cardFlip,

  /// Revelación de rol / carta.
  reveal,

  /// Fin de partida.
  gameOver;

  /// Nombre del fichero relativo a `assets/audio/` para `flame_audio`.
  String get fileName {
    switch (this) {
      case AppSound.cardFlip:
        return Assets.audio.cardFlipFile;
      case AppSound.reveal:
        return Assets.audio.revealFile;
      case AppSound.gameOver:
        return Assets.audio.gameOverFile;
    }
  }
}

/// Contrato del servicio de audio de la app.
abstract interface class AudioService {
  /// `true` si el sonido está activado. Cuando es `false`, [play] (y los helpers)
  /// no reproducen nada.
  bool get enabled;

  /// Activa/desactiva el sonido.
  void setEnabled(bool value);

  /// Alterna el estado on/off del sonido y devuelve el nuevo valor.
  bool toggle();

  /// Precarga todos los efectos en la caché para que la primera reproducción no
  /// tenga latencia. Nunca lanza: ante un fallo, se ignora.
  Future<void> preload();

  /// Reproduce [sound] si [enabled] es `true`. Nunca lanza.
  void play(AppSound sound, {double volume = 1.0});

  /// Atajo semántico: reproduce [AppSound.cardFlip].
  void playCardFlip();

  /// Atajo semántico: reproduce [AppSound.reveal].
  void playReveal();

  /// Atajo semántico: reproduce [AppSound.gameOver].
  void playGameOver();
}

/// Implementación por defecto de [AudioService] basada en `flame_audio`.
///
/// Lee el estado on/off de un [AudioSettingsNotifier] (vía `ref`) para que la UI
/// pueda silenciar el audio de forma reactiva. Todas las llamadas a
/// `flame_audio` están envueltas en try/catch.
class FlameAudioService implements AudioService {
  FlameAudioService(this._ref);

  final Ref _ref;

  @override
  bool get enabled => _ref.read(audioEnabledProvider).enabled;

  @override
  void setEnabled(bool value) {
    _ref.read(audioEnabledProvider.notifier).setEnabled(value);
  }

  @override
  bool toggle() => _ref.read(audioEnabledProvider.notifier).toggle();

  // coverage:ignore-start
  // MOTIVO: `preload` y `play` invocan directamente `flame_audio`
  // (FlameAudio.audioCache.loadAll / FlameAudio.play), que requieren los canales
  // nativos de audioplayers. Bajo `flutter test` no hay plataforma de audio: la
  // pila intentaría inicializar el canal nativo y generaría ruido de errores
  // async ajeno a la lógica de la app. El comportamiento observable ("suena solo
  // si enabled", manejo de errores) se cubre con `CountingAudioService` y los
  // tests de pantalla, que usan overrides no-op. Por eso estos bloques que tocan
  // la plataforma se excluyen de cobertura en vez de cubrirse con un fake.
  @override
  Future<void> preload() async {
    try {
      await FlameAudio.audioCache.loadAll(Assets.audio.allFiles);
    } catch (error, stackTrace) {
      // Sin plataforma de audio (tests) o ficheros aún ausentes: se ignora.
      _logIgnored('preload', error, stackTrace);
    }
  }

  @override
  void play(AppSound sound, {double volume = 1.0}) {
    if (!enabled) return;
    try {
      // `FlameAudio.play` es async; lo lanzamos sin await porque los efectos son
      // "fire and forget" y no deben bloquear la UI. Capturamos el error async
      // para que un fallo de reproducción nunca propague.
      FlameAudio.play(sound.fileName, volume: volume).catchError((
        Object error,
        StackTrace stackTrace,
      ) {
        _logIgnored('play(${sound.name})', error, stackTrace);
        // `play` devuelve un AudioPlayer; en error devolvemos uno vacío para
        // satisfacer la firma sin propagar.
        return AudioPlayer();
      });
    } catch (error, stackTrace) {
      _logIgnored('play(${sound.name})', error, stackTrace);
    }
  }
  // coverage:ignore-end

  @override
  void playCardFlip() => play(AppSound.cardFlip);

  @override
  void playReveal() => play(AppSound.reveal);

  @override
  void playGameOver() => play(AppSound.gameOver);

  void _logIgnored(String op, Object error, StackTrace stackTrace) {
    if (kDebugMode) {
      debugPrint('AudioService: $op ignorado (audio no disponible): $error');
    }
  }
}

/// Implementación no-op de [AudioService] para tests.
///
/// No toca ninguna plataforma de audio: registra el estado on/off en memoria y
/// los `play*` no hacen nada. Úsalo en tests sobreescribiendo
/// [audioServiceProvider].
class NoopAudioService implements AudioService {
  NoopAudioService({bool enabled = true}) : _enabled = enabled;

  bool _enabled;

  @override
  bool get enabled => _enabled;

  @override
  void setEnabled(bool value) => _enabled = value;

  @override
  bool toggle() => _enabled = !_enabled;

  @override
  Future<void> preload() async {}

  @override
  void play(AppSound sound, {double volume = 1.0}) {}

  @override
  void playCardFlip() {}

  @override
  void playReveal() {}

  @override
  void playGameOver() {}
}

/// Estado de ajustes de audio (de momento solo on/off).
class AudioSettings {
  const AudioSettings({this.enabled = true});

  /// `true` si el sonido está activado.
  final bool enabled;

  AudioSettings copyWith({bool? enabled}) =>
      AudioSettings(enabled: enabled ?? this.enabled);
}

/// Clave de `shared_preferences` donde se persiste el toggle de silencio.
@visibleForTesting
const String audioEnabledPreferenceKey = 'audio_enabled';

/// Notifier del estado on/off del sonido.
///
/// Convenciones (Riverpod 3.x): [Notifier] expuesto con un [NotifierProvider].
/// La UI lee `ref.watch(audioEnabledProvider).enabled` y muta con
/// `ref.read(audioEnabledProvider.notifier).toggle()`.
///
/// El estado se persiste con `shared_preferences` (clave
/// [audioEnabledPreferenceKey]) replicando el patrón de `LocaleController`:
/// carga diferida al construirse y guardado en cada mutación. Toda interacción
/// con `shared_preferences` está protegida para que un fallo nunca rompa el
/// audio ni la UI.
class AudioSettingsNotifier extends Notifier<AudioSettings> {
  @override
  AudioSettings build() {
    // Carga diferida del valor persistido. No bloquea el primer frame: arranca
    // con el sonido activado y, si había un valor guardado, se aplica al
    // resolver.
    _cargarGuardado();
    return const AudioSettings();
  }

  Future<void> _cargarGuardado() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final saved = prefs.getBool(audioEnabledPreferenceKey);
      if (saved != null) {
        state = state.copyWith(enabled: saved);
      }
    } catch (error, stackTrace) {
      // shared_preferences no disponible (sin plataforma, fallo de E/S): se
      // ignora y se mantiene el valor por defecto.
      _logIgnored('cargar', error, stackTrace);
    }
  }

  Future<void> _persistir(bool value) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setBool(audioEnabledPreferenceKey, value);
    } catch (error, stackTrace) {
      // El estado en memoria ya está aplicado; si la persistencia falla solo se
      // pierde entre arranques, nunca rompe.
      _logIgnored('guardar', error, stackTrace);
    }
  }

  void _logIgnored(String op, Object error, StackTrace stackTrace) {
    if (kDebugMode) {
      debugPrint(
        'AudioSettings: $op ignorado (preferencias no disponibles): $error',
      );
    }
  }

  /// `true` si el sonido está activado.
  bool get enabled => state.enabled;

  /// Activa/desactiva el sonido y lo persiste.
  void setEnabled(bool value) {
    state = state.copyWith(enabled: value);
    _persistir(value);
  }

  /// Alterna el estado on/off, lo persiste y devuelve el nuevo valor.
  bool toggle() {
    final next = !state.enabled;
    state = state.copyWith(enabled: next);
    _persistir(next);
    return next;
  }
}

/// Provider del estado on/off del sonido. Sobreescribible en tests.
final audioEnabledProvider =
    NotifierProvider<AudioSettingsNotifier, AudioSettings>(
      AudioSettingsNotifier.new,
    );

/// Provider del [AudioService] de la app.
///
/// Por defecto usa [FlameAudioService]. En tests se sobreescribe con un
/// [NoopAudioService] vía
/// `ProviderScope(overrides: [audioServiceProvider.overrideWithValue(NoopAudioService())])`.
final audioServiceProvider = Provider<AudioService>(
  (ref) => FlameAudioService(ref),
);
