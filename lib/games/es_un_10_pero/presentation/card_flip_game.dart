import 'dart:math' as math;
import 'dart:ui' as ui;

import 'package:flame/components.dart';
import 'package:flame/game.dart';
import 'package:flutter/cupertino.dart' show CupertinoIcons;
import 'package:flutter/material.dart';

import 'package:sajitarios_gamespot/core/assets/assets.dart';
import 'package:sajitarios_gamespot/games/es_un_10_pero/domain/card.dart'
    as domain;

/// Mapeo palo -> icono de Flutter en la capa de **presentación**.
///
/// El dominio ([domain.CardSuit]) es puro y no conoce Flutter; aquí asociamos
/// cada palo con su [IconData] de [CupertinoIcons] para renderizarlo. Se
/// conservan los mismos iconos `CupertinoIcons.suit_*_fill` que antes.
extension CardSuitIcon on domain.CardSuit {
  /// Icono de Flutter ([CupertinoIcons]) que representa este palo.
  IconData get suitIcon {
    switch (this) {
      case domain.CardSuit.espadas:
        return CupertinoIcons.suit_spade_fill;
      case domain.CardSuit.corazones:
        return CupertinoIcons.suit_heart_fill;
      case domain.CardSuit.diamantes:
        return CupertinoIcons.suit_diamond_fill;
      case domain.CardSuit.treboles:
        return CupertinoIcons.suit_club_fill;
    }
  }
}

/// Paleta de colores que la pantalla (que sí tiene acceso al [Theme]) inyecta
/// en el juego Flame, porque dentro del `FlameGame` no hay `BuildContext`.
@immutable
class CardFlipPalette {
  const CardFlipPalette({
    required this.cardSurface,
    required this.frameColor,
    required this.backPrimary,
    required this.backAccent,
    required this.redSuit,
    required this.blackSuit,
    required this.shadow,
  });

  /// Color de fondo de la cara de la carta.
  final Color cardSurface;

  /// Color del marco/borde de la cara revelada.
  final Color frameColor;

  /// Color principal del dorso de la carta.
  final Color backPrimary;

  /// Color de acento del dorso de la carta.
  final Color backAccent;

  /// Color de los palos rojos (corazones y diamantes).
  final Color redSuit;

  /// Color de los palos negros (espadas y tréboles).
  final Color blackSuit;

  /// Color de la sombra proyectada bajo la carta.
  final Color shadow;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is CardFlipPalette &&
          other.cardSurface == cardSurface &&
          other.frameColor == frameColor &&
          other.backPrimary == backPrimary &&
          other.backAccent == backAccent &&
          other.redSuit == redSuit &&
          other.blackSuit == blackSuit &&
          other.shadow == shadow;

  @override
  int get hashCode => Object.hash(
    cardSurface,
    frameColor,
    backPrimary,
    backAccent,
    redSuit,
    blackSuit,
    shadow,
  );
}

/// Juego Flame que renderiza una única carta con animación de volteo
/// (dorso → cara) sobre el eje vertical (efecto 3D simulado por escala en X).
///
/// El estado "carta actual" vive en Riverpod (en la pantalla). Este juego solo
/// recibe órdenes: [flipTo] reproduce el volteo revelando la nueva carta.
class CardFlipGame extends FlameGame {
  CardFlipGame({required this.palette});

  /// Paleta de colores inyectada desde el [Theme] de Flutter.
  CardFlipPalette palette;

  late final _CardComponent _card;

  @override
  Color backgroundColor() => const Color(0x00000000);

  @override
  Future<void> onLoad() async {
    // Creamos el componente de inmediato (sin esperar a la imagen) para que
    // `_card` esté disponible cuanto antes y `flipTo` nunca lo encuentre sin
    // inicializar.
    _card = _CardComponent(palette: palette);
    await world.add(_card);

    camera.viewfinder.anchor = Anchor.center;
    _layout();

    // Cargamos (con fallback) la imagen del dorso de forma asíncrona. Si carga,
    // se inyecta en el componente; si falla, sigue el dorso programático.
    final backImage = await _tryLoadCardBack();
    if (backImage != null) _card.setBackImage(backImage);
  }

  /// Intenta cargar el dorso de carta desde [Assets.images.cardBack] usando la
  /// caché de imágenes de Flame (prefijo `assets/images/`, por lo que se le
  /// pasa solo el nombre de fichero). Devuelve `null` si no se puede cargar,
  /// para que el componente recurra al dibujo programático.
  Future<ui.Image?> _tryLoadCardBack() async {
    try {
      final fileName = Assets.images.cardBack.split('/').last;
      return await images.load(fileName);
    } catch (_) {
      return null;
    }
  }

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    if (isLoaded) _layout();
  }

  void _layout() {
    // La carta mantiene proporción 5/7 y se ajusta al espacio disponible.
    final available = size;
    var w = available.x;
    var h = w * 7 / 5;
    if (h > available.y) {
      h = available.y;
      w = h * 5 / 7;
    }
    _card.size = Vector2(w, h);
    _card.position = Vector2.zero();
  }

  /// Reproduce la animación de volteo y, en mitad del giro, cambia la carta
  /// mostrada por [card]. Si la carta estaba bocabajo, voltea a la cara.
  ///
  /// Rendimiento: el motor se pausa en reposo (ver [_CardComponent.update]) para
  /// no gastar frames a ~60fps cuando no hay animación. Aquí lo reanudamos para
  /// que el volteo se anime; al terminar, el componente vuelve a pausarlo.
  void flipTo(domain.Card card) {
    if (paused) resumeEngine();
    _card.flipTo(card);
  }

  /// Actualiza la paleta (p. ej. al cambiar el tema claro/oscuro).
  void updatePalette(CardFlipPalette newPalette) {
    if (palette == newPalette) return;
    palette = newPalette;
    if (isLoaded) _card.updatePalette(newPalette);
  }
}

/// Componente de la carta. Dibuja el dorso o la cara según el progreso del
/// volteo y aplica una escala horizontal para simular el giro 3D.
///
/// Rendimiento: los [Paint] son instancias reutilizables (recoloreadas solo
/// cuando cambia la paleta) y los estados estáticos (dorso y cara) se cachean
/// en objetos [ui.Picture] que se regraban únicamente cuando cambia el tamaño,
/// la paleta o la carta mostrada. En reposo, `render` pinta directamente la
/// `Picture` cacheada sin reconstruir `Path`/`TextPainter` ni reasignar nada.
class _CardComponent extends PositionComponent {
  _CardComponent({required CardFlipPalette palette})
    : _palette = palette,
      super(anchor: Anchor.center) {
    _applyPaletteColors();
  }

  CardFlipPalette _palette;

  /// Imagen del dorso (placeholder generado por tool/generate_images.dart). Si
  /// es `null` (carga fallida o aún no cargada), el dorso se dibuja
  /// programáticamente.
  ui.Image? _backImage;

  /// Inyecta la imagen del dorso una vez cargada e invalida la `Picture`
  /// cacheada del dorso para que se regrabe con la imagen.
  void setBackImage(ui.Image image) {
    _backImage = image;
    _backPicture = null;
  }

  /// Carta que se está mostrando (null = dorso visible, sin cara aún).
  domain.Card? _current;

  /// Carta objetivo que se revelará al pasar el ecuador del volteo.
  domain.Card? _pending;

  /// Progreso del volteo en `[0, 1]`. 0 = reposo. En 0.5 está de canto.
  double _flipT = 0;
  bool _flipping = false;

  /// Duración total del volteo en segundos. Algo más largo que antes para que
  /// el giro respire y se perciba más elegante.
  static const double _flipDuration = 0.55;

  // --- Paints reutilizables (configurados una vez; recoloreados en cambios) --
  final Paint _shadowFill = Paint();
  final Paint _backFill = Paint();
  final Paint _backStroke = Paint()..style = PaintingStyle.stroke;
  final Paint _diamondFill = Paint();
  final Paint _faceFill = Paint();
  final Paint _faceStroke = Paint()..style = PaintingStyle.stroke;

  // Trazos "glow" (desenfocados) para el resplandor neón del marco. Se pintan
  // bajo el trazo nítido para simular el halo de luz característico del neón.
  final Paint _faceGlowStroke = Paint()..style = PaintingStyle.stroke;
  final Paint _backGlowStroke = Paint()..style = PaintingStyle.stroke;

  void _applyPaletteColors() {
    _shadowFill.color = _palette.shadow;
    _backFill.color = _palette.backPrimary;
    _backStroke.color = _palette.backAccent;
    _diamondFill.color = _palette.backAccent.withValues(alpha: 0.45);
    _faceFill.color = _palette.cardSurface;
    _faceStroke.color = _palette.frameColor;
    _faceGlowStroke.color = _palette.frameColor.withValues(alpha: 0.75);
    _backGlowStroke.color = _palette.backAccent.withValues(alpha: 0.7);
  }

  // --- Cachés de geometría estática y de estados pintados (Picture) ---------
  Vector2? _cachedSize;
  ui.Picture? _backPicture;
  ui.Picture? _facePicture;
  domain.Card? _facePictureCard;

  void updatePalette(CardFlipPalette newPalette) {
    if (newPalette == _palette) return;
    _palette = newPalette;
    _applyPaletteColors();
    _invalidatePictures();
  }

  void _invalidatePictures() {
    _backPicture = null;
    _facePicture = null;
    _facePictureCard = null;
  }

  void flipTo(domain.Card card) {
    _pending = card;
    _flipT = 0;
    _flipping = true;
  }

  /// Curva de easing suave (ease-in-out cúbica) sobre el progreso lineal.
  ///
  /// Hace que el giro arranque y termine despacio y acelere en el centro,
  /// dando una sensación mucho más fluida y agradable que el avance lineal.
  static double _easeInOutCubic(double t) {
    if (t < 0.5) return 4 * t * t * t;
    final f = (2 * t) - 2;
    return 0.5 * f * f * f + 1;
  }

  @override
  void update(double dt) {
    super.update(dt);
    if (!_flipping) {
      // En reposo (sin animación) pausamos el motor para no consumir ~60fps.
      // Lo hacemos desde `update` (no en `onLoad`) para que el game loop haya
      // ejecutado al menos un `render` del estado actual antes de detenerse, y
      // así el `GameWidget` muestre el frame correcto. `flipTo` lo reanuda.
      findGame()?.pauseEngine();
      return;
    }

    _flipT += dt / _flipDuration;

    if (_flipT >= 1) {
      _flipT = 1;
      _flipping = false;
      _current = _pending;
      // Volteo terminado: pausamos el motor para no renderizar a ~60fps en
      // reposo. Esta misma vuelta del game loop todavía ejecuta `render` con el
      // estado final (`_flipping = false`, carta revelada), por lo que el
      // `GameWidget` muestra el frame correcto antes de detenerse.
      findGame()?.pauseEngine();
      return;
    }

    // Al cruzar el ecuador del giro intercambiamos a la nueva carta para que el
    // cambio ocurra cuando la carta está "de canto" y no se vea el corte.
    // Usamos el progreso ya suavizado para que el cambio coincida exactamente
    // con el punto donde la escala horizontal es mínima (carta de canto).
    if (_easeInOutCubic(_flipT) >= 0.5 && _current != _pending) {
      _current = _pending;
    }
  }

  @override
  void render(Canvas canvas) {
    // Asegura que las cachés correspondan al tamaño actual.
    if (_cachedSize == null ||
        _cachedSize!.x != size.x ||
        _cachedSize!.y != size.y) {
      _cachedSize = size.clone();
      _invalidatePictures();
    }

    // --- Early-out: en reposo pintamos el estado estático cacheado sin
    // transformaciones de canvas ni reconstrucción de geometría/texto. ---
    if (!_flipping) {
      // La sombra en reposo (lift = 0) ya está horneada dentro de la Picture.
      final picture = _current != null
          ? _facePictureFor(_current!)
          : _backPictureCached();
      canvas.drawPicture(picture);
      return;
    }

    // --- Animación en curso: progreso suavizado y factores de escala. -------
    final eased = _easeInOutCubic(_flipT);

    // Factor de escala horizontal: 1 en reposo, ~0 de canto (ecuador del giro).
    // El clamp inferior evita que la carta llegue a ancho 0 (lo que produciría
    // un parpadeo / línea invisible) y mantiene un fino canto siempre visible.
    final scaleX = math.cos(eased * math.pi).abs().clamp(0.06, 1.0);

    // Sutil "respiro" vertical: la carta se eleva un poco al voltearse y vuelve
    // a su tamaño en reposo. Es máximo en el ecuador (sin(eased·π)).
    final lift = math.sin(eased * math.pi);
    final scaleY = 1.0 + 0.06 * lift;

    // El cambio de cara ocurre exactamente en el ecuador del progreso suavizado.
    final showFace = eased >= 0.5;

    canvas.save();
    // Escalamos respecto al centro de la carta (X para el giro, Y para el lift).
    canvas.translate(size.x / 2, size.y / 2);
    canvas.scale(scaleX, scaleY);
    canvas.translate(-size.x / 2, -size.y / 2);

    // La Picture cacheada incluye la sombra con lift = 0. Durante el giro la
    // sombra "elevada" se dibuja por separado (barata, sin texto ni paths) para
    // reforzar la profundidad, y luego pintamos el cuerpo cacheado encima.
    _renderShadow(canvas, lift);

    if (showFace && _current != null) {
      canvas.drawPicture(_facePictureFor(_current!, includeShadow: false));
    } else {
      canvas.drawPicture(_backPictureCached(includeShadow: false));
    }
    canvas.restore();
  }

  Rect get _bounds => Rect.fromLTWH(0, 0, size.x, size.y);
  double get _radius => size.x * 0.10;

  /// Devuelve (creando si hace falta) la `Picture` del dorso. La variante con
  /// sombra (lift = 0) se cachea; la variante sin sombra se graba al vuelo solo
  /// durante el giro (se invoca a lo sumo una vez por frame de animación).
  ui.Picture _backPictureCached({bool includeShadow = true}) {
    if (includeShadow) {
      return _backPicture ??= _recordBack(includeShadow: true);
    }
    return _recordBack(includeShadow: false);
  }

  ui.Picture _facePictureFor(domain.Card card, {bool includeShadow = true}) {
    if (includeShadow) {
      if (_facePicture == null || _facePictureCard != card) {
        _facePicture = _recordFace(card, includeShadow: true);
        _facePictureCard = card;
      }
      return _facePicture!;
    }
    return _recordFace(card, includeShadow: false);
  }

  ui.Picture _recordBack({required bool includeShadow}) {
    final recorder = ui.PictureRecorder();
    final canvas = Canvas(recorder);
    if (includeShadow) _renderShadow(canvas, 0);
    _paintBack(canvas);
    return recorder.endRecording();
  }

  ui.Picture _recordFace(domain.Card card, {required bool includeShadow}) {
    final recorder = ui.PictureRecorder();
    final canvas = Canvas(recorder);
    if (includeShadow) _renderShadow(canvas, 0);
    _paintFace(canvas, card);
    return recorder.endRecording();
  }

  /// Dibuja la sombra proyectada. Durante el giro la carta "se eleva": la
  /// sombra se desplaza y difumina más ([lift] en `[0, 1]`), reforzando la
  /// sensación de profundidad y elevación.
  void _renderShadow(Canvas canvas, double lift) {
    final dy = 8.0 + 18.0 * lift;
    final blur = 12.0 + 16.0 * lift;
    final rrect = RRect.fromRectAndRadius(
      _bounds.shift(Offset(0, dy)),
      Radius.circular(_radius),
    );
    _shadowFill.maskFilter = MaskFilter.blur(BlurStyle.normal, blur);
    canvas.drawRRect(rrect, _shadowFill);
  }

  void _paintBack(Canvas canvas) {
    final rrect = RRect.fromRectAndRadius(_bounds, Radius.circular(_radius));

    // Si hay imagen de dorso disponible, la pintamos recortada al rect
    // redondeado. Ante cualquier fallo se recurre al dibujo programático.
    if (_backImage != null && _tryPaintBackImage(canvas, rrect)) return;

    canvas.drawRRect(rrect, _backFill);

    // Marco exterior neón: trazo cian brillante con halo desenfocado, igual que
    // la cara, para que el dorso comparta el look "full neon".
    _backGlowStroke.strokeWidth = size.x * 0.018;
    _backGlowStroke.color = _palette.frameColor.withValues(alpha: 0.7);
    _backGlowStroke.maskFilter = MaskFilter.blur(
      BlurStyle.normal,
      size.x * 0.04,
    );
    canvas.drawRRect(rrect, _backGlowStroke);
    _faceStroke.strokeWidth = size.x * 0.012;
    canvas.drawRRect(rrect, _faceStroke);

    // Marco interior magenta con resplandor.
    final inset = _bounds.deflate(size.x * 0.06);
    final innerRRect = RRect.fromRectAndRadius(
      inset,
      Radius.circular(_radius * 0.7),
    );
    _backGlowStroke.color = _palette.backAccent.withValues(alpha: 0.7);
    _backGlowStroke.strokeWidth = size.x * 0.022;
    _backGlowStroke.maskFilter = MaskFilter.blur(
      BlurStyle.normal,
      size.x * 0.03,
    );
    canvas.drawRRect(innerRRect, _backGlowStroke);
    _backStroke.strokeWidth = size.x * 0.014;
    _backStroke.maskFilter = null;
    canvas.drawRRect(innerRRect, _backStroke);

    // Patrón de rombos neón en el centro (con un sutil halo magenta).
    canvas.save();
    canvas.clipRRect(innerRRect);
    _diamondFill.maskFilter = MaskFilter.blur(BlurStyle.normal, size.x * 0.012);
    final step = size.x * 0.16;
    for (double y = inset.top; y < inset.bottom; y += step) {
      for (double x = inset.left; x < inset.right; x += step) {
        final cx = x + step / 2;
        final cy = y + step / 2;
        final d = step * 0.32;
        final path = Path()
          ..moveTo(cx, cy - d)
          ..lineTo(cx + d, cy)
          ..lineTo(cx, cy + d)
          ..lineTo(cx - d, cy)
          ..close();
        canvas.drawPath(path, _diamondFill);
      }
    }
    canvas.restore();
  }

  /// Pinta [_backImage] cubriendo el rect de la carta (BoxFit.cover) recortado
  /// al rect redondeado [rrect]. Devuelve `true` si pintó algo; `false` ante
  /// cualquier fallo, para que el llamante recurra al dibujo programático.
  bool _tryPaintBackImage(Canvas canvas, RRect rrect) {
    final image = _backImage;
    if (image == null) return false;
    final saveCount = canvas.getSaveCount();
    try {
      final src = Rect.fromLTWH(
        0,
        0,
        image.width.toDouble(),
        image.height.toDouble(),
      );
      canvas.save();
      canvas.clipRRect(rrect);
      canvas.drawImageRect(image, src, _bounds, Paint());
      canvas.restore();
      return true;
    } catch (_) {
      // Restauramos cualquier save() pendiente para no corromper el canvas.
      canvas.restoreToCount(saveCount);
      return false;
    }
  }

  void _paintFace(Canvas canvas, domain.Card card) {
    final rrect = RRect.fromRectAndRadius(_bounds, Radius.circular(_radius));
    canvas.drawRRect(rrect, _faceFill);

    // Marco neón: primero un trazo cian desenfocado (halo) y encima el trazo
    // nítido, para que el borde de la carta "brille" como un tubo de neón.
    _faceGlowStroke.strokeWidth = size.x * 0.03;
    _faceGlowStroke.maskFilter = MaskFilter.blur(
      BlurStyle.normal,
      size.x * 0.04,
    );
    canvas.drawRRect(rrect, _faceGlowStroke);
    _faceStroke.strokeWidth = size.x * 0.014;
    _faceStroke.maskFilter = null;
    canvas.drawRRect(rrect, _faceStroke);

    final suitColor = card.suit.isRed ? _palette.redSuit : _palette.blackSuit;
    final margin = size.x * 0.08;

    // Valor + palo en la esquina superior izquierda.
    _drawCornerLabel(
      canvas,
      card,
      suitColor,
      Offset(margin, margin),
      flipped: false,
    );

    // Valor + palo en la esquina inferior derecha (rotado 180°).
    _drawCornerLabel(
      canvas,
      card,
      suitColor,
      Offset(size.x - margin, size.y - margin),
      flipped: true,
    );

    // Palo grande al centro.
    final centerPainter = _iconPainter(
      card.suit.suitIcon,
      suitColor,
      size.x * 0.40,
    );
    centerPainter.paint(
      canvas,
      Offset(
        (size.x - centerPainter.width) / 2,
        (size.y - centerPainter.height) / 2,
      ),
    );
  }

  void _drawCornerLabel(
    Canvas canvas,
    domain.Card card,
    Color color,
    Offset anchor, {
    required bool flipped,
  }) {
    final valuePainter = _textPainter(
      card.value.label,
      color,
      size.x * 0.16,
      FontWeight.w900,
    );
    final suitPainter = _iconPainter(card.suit.suitIcon, color, size.x * 0.11);

    canvas.save();
    if (flipped) {
      canvas.translate(anchor.dx, anchor.dy);
      canvas.rotate(math.pi);
      valuePainter.paint(canvas, Offset.zero);
      suitPainter.paint(canvas, Offset(0, valuePainter.height));
    } else {
      valuePainter.paint(canvas, anchor);
      suitPainter.paint(
        canvas,
        Offset(anchor.dx, anchor.dy + valuePainter.height),
      );
    }
    canvas.restore();
  }

  /// Sombras de glow neón para texto/iconos: varios halos del color del palo
  /// para que números y palos "brillen" como neón sobre la superficie oscura.
  List<Shadow> _neonShadows(Color color, double fontSize) {
    return [
      Shadow(color: color.withValues(alpha: 0.9), blurRadius: fontSize * 0.20),
      Shadow(color: color.withValues(alpha: 0.6), blurRadius: fontSize * 0.45),
    ];
  }

  TextPainter _textPainter(
    String text,
    Color color,
    double fontSize,
    FontWeight weight,
  ) {
    final painter = TextPainter(
      text: TextSpan(
        text: text,
        style: TextStyle(
          color: color,
          fontSize: fontSize,
          fontWeight: weight,
          height: 1,
          shadows: _neonShadows(color, fontSize),
        ),
      ),
      textDirection: TextDirection.ltr,
    );
    painter.layout();
    return painter;
  }

  /// Pinta el glifo de un [IconData] de Flutter usando su fuente, en vez de un
  /// símbolo Unicode. Así los palos son iconos de Flutter (CupertinoIcons).
  TextPainter _iconPainter(IconData icon, Color color, double fontSize) {
    final painter = TextPainter(
      text: TextSpan(
        text: String.fromCharCode(icon.codePoint),
        style: TextStyle(
          color: color,
          fontSize: fontSize,
          height: 1,
          fontFamily: icon.fontFamily,
          package: icon.fontPackage,
          shadows: _neonShadows(color, fontSize),
        ),
      ),
      textDirection: TextDirection.ltr,
    );
    painter.layout();
    return painter;
  }
}
