/// Catálogo centralizado de RUTAS de assets de la app.
///
/// Único sitio donde viven las rutas a imágenes y sonidos: el resto del código
/// nunca debe escribir strings sueltos como `'assets/images/card_back.png'`.
/// Así, cuando 0.40 (imágenes) y 0.41 (sonidos) añadan los ficheros reales, solo
/// hay que tocar este fichero.
///
/// Convenciones de rutas:
/// - [AppImages] expone rutas COMPLETAS (`assets/images/...`) listas para
///   `AssetImage` / `Image.asset` / la `Images` cache de Flame.
/// - [AppAudio] expone, además de la ruta completa ([AppAudio.cardFlip]), el
///   nombre RELATIVO a `assets/audio/` ([AppAudio.cardFlipFile]). `flame_audio`
///   resuelve los ficheros bajo el prefijo `assets/audio/`, por lo que sus APIs
///   (`FlameAudio.audioCache.load`, `FlameAudio.play`) esperan el nombre
///   relativo, no la ruta completa. Usa `*File` con flame_audio y la ruta
///   completa solo si necesitaras cargar el .wav con otra API.
library;

/// Punto de entrada del catálogo de assets.
///
/// Agrupa los dos sub-catálogos: [Assets.images] y [Assets.audio].
abstract final class Assets {
  /// Catálogo de imágenes.
  static const AppImages images = AppImages();

  /// Catálogo de sonidos.
  static const AppAudio audio = AppAudio();
}

/// Rutas COMPLETAS de las imágenes de la app (prefijo `assets/images/`).
///
/// Los ficheros reales los añade 0.40; aquí solo se fija el contrato de rutas.
class AppImages {
  const AppImages();

  /// Directorio base de las imágenes.
  static const String _base = 'assets/images/';

  /// Reverso de la carta del juego "Es un 10 pero".
  final String cardBack = '${_base}card_back.png';

  /// Anverso/plantilla de la carta (cara visible).
  final String cardFront = '${_base}card_front.png';

  /// Cabecera decorativa (opcional) de la pantalla de menú. PLACEHOLDER
  /// generado por `tool/generate_images.dart`.
  final String menuHeader = '${_base}menu_header.png';
}

/// Sonidos de la app.
///
/// Para cada efecto se expone:
/// - la ruta COMPLETA (`assets/audio/...`), p. ej. [cardFlip];
/// - el nombre RELATIVO a `assets/audio/` (sufijo `File`), p. ej. [cardFlipFile],
///   que es lo que esperan las APIs de `flame_audio`.
///
/// Los ficheros reales los añade 0.41; aquí solo se fija el contrato de rutas.
class AppAudio {
  const AppAudio();

  /// Directorio base de los sonidos (prefijo que usa la app y donde
  /// `flame_audio` resuelve sus ficheros).
  static const String _base = 'assets/audio/';

  // --- card_flip: volteo de carta ---

  /// Nombre relativo a `assets/audio/` (para `flame_audio`).
  final String cardFlipFile = 'card_flip.wav';

  /// Ruta completa (`assets/audio/card_flip.wav`).
  final String cardFlip = '${_base}card_flip.wav';

  // --- reveal: revelación de rol / carta ---

  /// Nombre relativo a `assets/audio/` (para `flame_audio`).
  final String revealFile = 'reveal.wav';

  /// Ruta completa (`assets/audio/reveal.wav`).
  final String reveal = '${_base}reveal.wav';

  // --- game_over: fin de partida ---

  /// Nombre relativo a `assets/audio/` (para `flame_audio`).
  final String gameOverFile = 'game_over.wav';

  /// Ruta completa (`assets/audio/game_over.wav`).
  final String gameOver = '${_base}game_over.wav';

  /// Todos los nombres de fichero relativos a `assets/audio/`.
  ///
  /// Útil para precargar (`preload`) todos los sonidos de una vez.
  List<String> get allFiles => <String>[cardFlipFile, revealFile, gameOverFile];
}
