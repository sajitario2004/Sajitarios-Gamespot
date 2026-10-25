import 'dart:math' as math;
import 'dart:ui' as ui;

import 'package:flame/components.dart';
import 'package:flame/events.dart';
import 'package:flame/game.dart';
import 'package:flutter/material.dart';

import 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_round.dart';

/// Modo de visualización del dial de Wavelength.
enum WavelengthDialMode {
  /// Modo psíquico: las bandas y el objetivo son visibles (solo para el psíquico).
  clue,

  /// Modo grupo: el objetivo está oculto; se mueve la aguja.
  guess,

  /// Revelación: como [clue] pero con animación de reveal.
  reveal,
}

/// Paleta de colores que la pantalla inyecta en el juego Flame.
///
/// Dentro del [FlameGame] no hay [BuildContext], así que el tema se pasa aquí.
@immutable
class WavelengthDialPalette {
  const WavelengthDialPalette({
    required this.arcColor,
    required this.needleColor,
    required this.bullseyeColor,
    required this.nearColor,
    required this.farColor,
    required this.labelColor,
    required this.glowColor,
  });

  final Color arcColor;
  final Color needleColor;
  final Color bullseyeColor;
  final Color nearColor;
  final Color farColor;
  final Color labelColor;
  final Color glowColor;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is WavelengthDialPalette &&
          other.arcColor == arcColor &&
          other.needleColor == needleColor &&
          other.bullseyeColor == bullseyeColor &&
          other.nearColor == nearColor &&
          other.farColor == farColor &&
          other.labelColor == labelColor &&
          other.glowColor == glowColor;

  @override
  int get hashCode => Object.hash(
    arcColor,
    needleColor,
    bullseyeColor,
    nearColor,
    farColor,
    labelColor,
    glowColor,
  );
}

/// Juego Flame que renderiza el dial semicircular de Wavelength con estética neón.
///
/// Sigue exactamente la disciplina de performance de [CardFlipGame]:
/// - [Paint]s reutilizables, recoloreados solo al cambiar la paleta.
/// - Estado estático cacheado en [ui.Picture]; se invalida al cambiar tamaño,
///   paleta, modo o target.
/// - Motor pausado en reposo ([pauseEngine]); reanudado solo en animaciones.
///
/// API pública (se llama desde la pantalla/controlador):
/// - [setMode] cambia el modo (clue / guess / reveal) y arranca la animación.
/// - [setTarget] inyecta la posición objetivo en [0,1].
/// - [setConceptLabels] inyecta las etiquetas del espectro.
/// - [onGuessChanged] callback cuando el usuario arrastra la aguja.
/// - [needlePosition] posición normalizada actual de la aguja (0..1).
///
/// Las llamadas a [setMode], [setTarget] y [setConceptLabels] antes de que
/// [onLoad] complete se almacenan como estado pendiente y se aplican
/// automáticamente al terminar la carga. Esto evita que el modo reveal no
/// arranque cuando la pantalla llama a esos métodos durante el primer build.
class WavelengthDialGame extends FlameGame {
  WavelengthDialGame({required this.palette, this.onGuessChanged});

  WavelengthDialPalette palette;

  /// Callback invocado cada vez que el usuario mueve la aguja (modo guess).
  void Function(double normalizedPosition)? onGuessChanged;

  late final _DialComponent _dial;

  // ── Pending state: applied once onLoad completes ──────────────────────────
  WavelengthDialMode _pendingMode = WavelengthDialMode.guess;
  double _pendingTarget = 0.5;
  String _pendingLeft = '';
  String _pendingRight = '';
  bool _pendingLabelsSet = false;

  @override
  Color backgroundColor() => const Color(0x00000000);

  @override
  Future<void> onLoad() async {
    _dial = _DialComponent(
      palette: palette,
      onGuessChanged: (pos) => onGuessChanged?.call(pos),
    );
    await world.add(_dial);
    camera.viewfinder.anchor = Anchor.center;
    _layout();

    // Apply any configuration that arrived before load completed.
    _dial.setTarget(_pendingTarget);
    if (_pendingLabelsSet) {
      _dial.setConceptLabels(_pendingLeft, _pendingRight);
    }
    _dial.setMode(_pendingMode);
    if (_pendingMode == WavelengthDialMode.reveal ||
        _pendingMode == WavelengthDialMode.clue) {
      if (paused) resumeEngine();
    }
  }

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    if (isLoaded) _layout();
  }

  void _layout() {
    _dial.size = size.clone();
    _dial.position = Vector2.zero();
  }

  // ── Public API ────────────────────────────────────────────────────────────

  /// Posición normalizada [0,1] de la aguja.
  double get needlePosition => _dial.needlePosition;

  /// Cambia el modo de visualización. En [WavelengthDialMode.reveal] arranca
  /// la animación de revelación y reanuda el motor.
  ///
  /// Si el juego todavía no ha terminado de cargar ([isLoaded] == false), el
  /// modo se almacena como pendiente y se aplica al finalizar [onLoad].
  void setMode(WavelengthDialMode mode) {
    _pendingMode = mode;
    if (isLoaded) {
      _dial.setMode(mode);
      if (mode == WavelengthDialMode.reveal ||
          mode == WavelengthDialMode.clue) {
        if (paused) resumeEngine();
      }
    }
  }

  /// Inyecta la posición objetivo (solo visible en modos clue/reveal).
  ///
  /// Si el juego todavía no ha terminado de cargar, el valor se almacena como
  /// pendiente y se aplica al finalizar [onLoad].
  void setTarget(double target) {
    _pendingTarget = target;
    if (isLoaded) _dial.setTarget(target);
  }

  /// Inyecta las etiquetas de los conceptos del espectro.
  ///
  /// Si el juego todavía no ha terminado de cargar, las etiquetas se almacenan
  /// como pendientes y se aplican al finalizar [onLoad].
  void setConceptLabels(String left, String right) {
    _pendingLeft = left;
    _pendingRight = right;
    _pendingLabelsSet = true;
    if (isLoaded) _dial.setConceptLabels(left, right);
  }

  /// Actualiza la paleta de colores (p. ej. al cambiar el tema).
  void updatePalette(WavelengthDialPalette newPalette) {
    if (palette == newPalette) return;
    palette = newPalette;
    if (isLoaded) _dial.updatePalette(newPalette);
  }
}

// ─────────────────────────────────────────────────────────────────────────────

/// Componente que dibuja el dial y gestiona la aguja draggable.
class _DialComponent extends PositionComponent
    with TapCallbacks, DragCallbacks {
  _DialComponent({required WavelengthDialPalette palette, this.onGuessChanged})
    : _palette = palette,
      super(anchor: Anchor.topLeft) {
    _applyPaletteColors();
  }

  WavelengthDialPalette _palette;
  void Function(double)? onGuessChanged;

  // ── Estado ────────────────────────────────────────────────────────────────
  WavelengthDialMode _mode = WavelengthDialMode.guess;
  double _target = 0.5;
  double _needlePosition = 0.5;
  String _leftLabel = '';
  String _rightLabel = '';

  // Animación de reveal
  double _revealProgress = 0.0; // 0..1
  static const double _revealDuration = 0.7;
  bool _revealing = false;

  // Drag state
  bool _dragging = false;

  // Guard: emite pauseEngine() solo una vez por transición, no cada frame.
  bool _pausePending = false;

  double get needlePosition => _needlePosition;

  // ── Paints reutilizables ──────────────────────────────────────────────────
  final Paint _arcPaint = Paint()..style = PaintingStyle.stroke;
  final Paint _arcGlowPaint = Paint()..style = PaintingStyle.stroke;
  final Paint _bullseyePaint = Paint();
  final Paint _nearPaint = Paint();
  final Paint _farPaint = Paint();
  final Paint _needlePaint = Paint()..style = PaintingStyle.stroke;
  final Paint _needleGlowPaint = Paint()..style = PaintingStyle.stroke;
  final Paint _needleKnobPaint = Paint();

  // Cached band render paints (fill with alpha-modulated color at render time).
  final Paint _bandBullseyeRenderPaint = Paint()..style = PaintingStyle.fill;
  final Paint _bandNearRenderPaint = Paint()..style = PaintingStyle.fill;
  final Paint _bandFarRenderPaint = Paint()..style = PaintingStyle.fill;

  // Cached band paths rebuilt in _rebuildBandPaths().
  final Path _bandBullseyePath = Path();
  final Path _bandNearPath = Path();
  final Path _bandFarPath = Path();

  // Cached target-marker paints.
  final Paint _markerPaint = Paint()..style = PaintingStyle.stroke;
  final Paint _markerGlowPaint = Paint()
    ..style = PaintingStyle.stroke
    ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 6);

  // Cached geometry — rebuilt only in onGameResize / layout.
  _DialGeometry? _cachedGeom;

  void _applyPaletteColors() {
    _arcPaint.color = _palette.arcColor;
    _arcGlowPaint.color = _palette.glowColor.withValues(alpha: 0.6);
    _arcGlowPaint.maskFilter = const MaskFilter.blur(BlurStyle.normal, 8);
    _bullseyePaint.color = _palette.bullseyeColor.withValues(alpha: 0.45);
    _nearPaint.color = _palette.nearColor.withValues(alpha: 0.30);
    _farPaint.color = _palette.farColor.withValues(alpha: 0.20);
    _needlePaint.color = _palette.needleColor;
    _needleGlowPaint.color = _palette.needleColor.withValues(alpha: 0.7);
    _needleGlowPaint.maskFilter = const MaskFilter.blur(BlurStyle.normal, 6);
    _needleKnobPaint.color = _palette.needleColor;
    // Update band render paint base colors.
    _bandBullseyeRenderPaint.color = _bullseyePaint.color;
    _bandNearRenderPaint.color = _nearPaint.color;
    _bandFarRenderPaint.color = _farPaint.color;
    // Update marker paint colors.
    _markerPaint.color = _palette.bullseyeColor;
    _markerGlowPaint.color = _palette.bullseyeColor.withValues(alpha: 0.7);
  }

  // ── Caché ─────────────────────────────────────────────────────────────────
  Vector2? _cachedSize;
  ui.Picture? _staticPicture; // arc + labels (no bands, no needle)

  void _invalidatePicture() {
    _staticPicture = null;
  }

  // ── Setters públicos ──────────────────────────────────────────────────────

  void setMode(WavelengthDialMode mode) {
    if (_mode == mode) return;
    _mode = mode;
    if (mode == WavelengthDialMode.reveal) {
      _revealProgress = 0.0;
      _revealing = true;
      _pausePending = false;
      findGame()?.resumeEngine();
    } else if (mode == WavelengthDialMode.clue) {
      _revealing = false;
      _revealProgress = 0.0;
      _pausePending = false;
      findGame()?.resumeEngine();
    } else {
      _revealing = false;
      _revealProgress = 0.0;
    }
    _invalidatePicture();
  }

  void setTarget(double target) {
    _target = target.clamp(0.0, 1.0);
    // Rebuild cached band paths for the new target.
    if (_cachedGeom != null) _rebuildBandPaths(_cachedGeom!);
    _invalidatePicture();
  }

  void setConceptLabels(String left, String right) {
    if (_leftLabel == left && _rightLabel == right) return;
    _leftLabel = left;
    _rightLabel = right;
    _invalidatePicture();
  }

  void updatePalette(WavelengthDialPalette newPalette) {
    if (newPalette == _palette) return;
    _palette = newPalette;
    _applyPaletteColors();
    _invalidatePicture();
  }

  // ── Lifecycle ─────────────────────────────────────────────────────────────

  @override
  void update(double dt) {
    super.update(dt);

    if (_revealing) {
      _revealProgress = (_revealProgress + dt / _revealDuration).clamp(
        0.0,
        1.0,
      );
      if (_revealProgress >= 1.0) {
        _revealing = false;
        if (!_pausePending) {
          _pausePending = true;
          findGame()?.pauseEngine();
        }
      }
      return;
    }

    if (!_dragging && !_pausePending) {
      _pausePending = true;
      findGame()?.pauseEngine();
    }
  }

  // ── Input ─────────────────────────────────────────────────────────────────

  @override
  void onTapDown(TapDownEvent event) {
    if (_mode != WavelengthDialMode.guess) return;
    _updateNeedleFromPosition(event.localPosition);
    event.continuePropagation = false;
  }

  @override
  void onDragStart(DragStartEvent event) {
    super.onDragStart(event);
    if (_mode != WavelengthDialMode.guess) return;
    _dragging = true;
    _pausePending = false;
    _updateNeedleFromPosition(event.localPosition);
    findGame()?.resumeEngine();
    event.continuePropagation = false;
  }

  @override
  void onDragUpdate(DragUpdateEvent event) {
    if (!_dragging) return;
    _updateNeedleFromPosition(event.localEndPosition);
  }

  @override
  void onDragEnd(DragEndEvent event) {
    super.onDragEnd(event);
    _dragging = false;
  }

  @override
  void onDragCancel(DragCancelEvent event) {
    super.onDragCancel(event);
    _dragging = false;
  }

  void _updateNeedleFromPosition(Vector2 local) {
    final geom = _geom;
    final dx = local.x - geom.cx;
    final dy = local.y - geom.cy;
    // Arc: left end = angle π (left pole), right end = angle 0 (right pole),
    // going through the top (angle = π/2 at top).
    // atan2(-dy, dx): up = +π/2, left = +π/-π, right = 0.
    final angle = math.atan2(-dy, dx);
    // Clamp to [0..π] (top half of unit circle).
    final normalAngle = angle.clamp(0.0, math.pi);
    // Map [π..0] → [0..1]: position=0 at left end (angle=π), 1 at right end.
    final newPos = 1.0 - (normalAngle / math.pi);
    _needlePosition = newPos.clamp(0.0, 1.0);
    onGuessChanged?.call(_needlePosition);
  }

  // ── Geometry helper ───────────────────────────────────────────────────────

  /// Returns the cached geometry, recomputing only when size changed.
  _DialGeometry get _geom {
    final cached = _cachedGeom;
    if (cached != null &&
        _cachedSize != null &&
        _cachedSize!.x == size.x &&
        _cachedSize!.y == size.y) {
      return cached;
    }
    final w = size.x;
    final h = size.y;
    final radius = math.min(w * 0.44, h * 0.82);
    final cx = w / 2;
    final cy = h * 0.78;
    final geom = _DialGeometry(cx: cx, cy: cy, radius: radius);
    _cachedGeom = geom;
    _rebuildBandPaths(geom);
    _markerPaint.strokeWidth = geom.radius * 0.018;
    _markerGlowPaint.strokeWidth = geom.radius * 0.028;
    return geom;
  }

  /// Rebuilds the three cached band [Path]s for [geom] and [_target].
  void _rebuildBandPaths(_DialGeometry geom) {
    _buildBandPath(_bandFarPath, geom, _target, kFarHalfWidth);
    _buildBandPath(_bandNearPath, geom, _target, kNearHalfWidth);
    _buildBandPath(_bandBullseyePath, geom, _target, kBullseyeHalfWidth);
  }

  void _buildBandPath(
    Path path,
    _DialGeometry geom,
    double center,
    double halfWidth,
  ) {
    path.reset();
    final startPos = (center - halfWidth).clamp(0.0, 1.0);
    final endPos = (center + halfWidth).clamp(0.0, 1.0);
    final startAngle = math.pi + startPos * math.pi;
    final sweepAngle = (endPos - startPos) * math.pi;
    final rect = Rect.fromCircle(
      center: Offset(geom.cx, geom.cy),
      radius: geom.radius,
    );
    path
      ..moveTo(geom.cx, geom.cy)
      ..arcTo(rect, startAngle, sweepAngle, false)
      ..close();
  }

  // ── Render ────────────────────────────────────────────────────────────────

  @override
  void render(Canvas canvas) {
    // _geom updates _cachedSize and rebuilds band paths if size changed.
    final geom = _geom;
    if (_cachedSize == null ||
        _cachedSize!.x != size.x ||
        _cachedSize!.y != size.y) {
      _cachedSize = size.clone();
      _invalidatePicture();
    }

    final showBands =
        _mode == WavelengthDialMode.clue || _mode == WavelengthDialMode.reveal;
    final bandAlpha = _mode == WavelengthDialMode.reveal
        ? _revealProgress
        : 1.0;

    canvas.drawPicture(_staticPictureCached(geom));

    if (showBands) {
      _drawBands(canvas, alpha: bandAlpha);
      _drawTargetMarker(canvas, geom, alpha: bandAlpha);
    }

    _drawNeedle(canvas, geom);
  }

  ui.Picture _staticPictureCached(_DialGeometry geom) {
    return _staticPicture ??= _recordStatic(geom);
  }

  ui.Picture _recordStatic(_DialGeometry geom) {
    final recorder = ui.PictureRecorder();
    final canvas = Canvas(recorder);
    _drawArc(canvas, geom);
    _drawLabels(canvas, geom);
    return recorder.endRecording();
  }

  void _drawArc(Canvas canvas, _DialGeometry geom) {
    final rect = Rect.fromCircle(
      center: Offset(geom.cx, geom.cy),
      radius: geom.radius,
    );

    _arcGlowPaint.strokeWidth = geom.radius * 0.055;
    canvas.drawArc(rect, math.pi, math.pi, false, _arcGlowPaint);

    _arcPaint.strokeWidth = geom.radius * 0.025;
    canvas.drawArc(rect, math.pi, math.pi, false, _arcPaint);

    for (var i = 0; i <= 4; i++) {
      final frac = i / 4.0;
      final angle = math.pi + frac * math.pi;
      final cos = math.cos(angle);
      final sin = math.sin(angle);
      final inner = geom.radius - geom.radius * 0.06;
      final outer = geom.radius + geom.radius * 0.06;
      canvas.drawLine(
        Offset(geom.cx + inner * cos, geom.cy + inner * sin),
        Offset(geom.cx + outer * cos, geom.cy + outer * sin),
        _arcPaint,
      );
    }
  }

  void _drawLabels(Canvas canvas, _DialGeometry geom) {
    if (_leftLabel.isEmpty && _rightLabel.isEmpty) return;

    // Labels are drawn BELOW the arc ends (near the baseline of the
    // semicircle) so they never overlap the arc and are never clipped by
    // the widget bounds, even on narrow screens.
    //
    // Geometry:
    //   - Arc left end  ≈ (cx - radius, cy)  → angle = π
    //   - Arc right end ≈ (cx + radius, cy)  → angle = 0
    // We draw the labels centered below each end point, inside the
    // canvas bounds.

    final fontSize = (geom.radius * 0.115).clamp(9.0, 16.0);
    final labelStyle = TextStyle(
      color: _palette.labelColor,
      fontSize: fontSize,
      fontWeight: FontWeight.w700,
      height: 1.15,
      shadows: [
        Shadow(
          color: _palette.glowColor.withValues(alpha: 0.8),
          blurRadius: fontSize * 0.5,
        ),
      ],
    );

    // Max width: from edge to center, leaving a small inset.
    final maxLabelWidth = (geom.cx - 8.0).clamp(40.0, geom.radius * 0.80);

    if (_leftLabel.isNotEmpty) {
      final painter = TextPainter(
        text: TextSpan(text: _leftLabel, style: labelStyle),
        textDirection: TextDirection.ltr,
        textAlign: TextAlign.right,
        maxLines: 3,
      )..layout(maxWidth: maxLabelWidth);

      // Right-align flush to the left arc endpoint, baseline at cy + small gap.
      final rightEdge = geom.cx - geom.radius + geom.radius * 0.06;
      final top = geom.cy + geom.radius * 0.04;
      final left = (rightEdge - painter.width).clamp(4.0, double.infinity);
      painter.paint(canvas, Offset(left, top));
    }

    if (_rightLabel.isNotEmpty) {
      final painter = TextPainter(
        text: TextSpan(text: _rightLabel, style: labelStyle),
        textDirection: TextDirection.ltr,
        textAlign: TextAlign.left,
        maxLines: 3,
      )..layout(maxWidth: maxLabelWidth);

      // Left-align starting at the right arc endpoint, baseline at cy + small gap.
      final leftEdge = geom.cx + geom.radius - geom.radius * 0.06;
      final top = geom.cy + geom.radius * 0.04;
      // Ensure right edge doesn't overflow canvas.
      final clampedLeft = (leftEdge - painter.width / 2).clamp(
        0.0,
        (geom.cx * 2 - painter.width - 4.0).clamp(0.0, double.infinity),
      );
      painter.paint(canvas, Offset(clampedLeft, top));
    }
  }

  /// Draws the three bands using cached [Path]s and cached render [Paint]s.
  /// Only the alpha channel is mutated per-frame — no allocations.
  void _drawBands(Canvas canvas, {double alpha = 1.0}) {
    _bandFarRenderPaint.color = _farPaint.color.withValues(
      alpha: _farPaint.color.a * alpha,
    );
    canvas.drawPath(_bandFarPath, _bandFarRenderPaint);

    _bandNearRenderPaint.color = _nearPaint.color.withValues(
      alpha: _nearPaint.color.a * alpha,
    );
    canvas.drawPath(_bandNearPath, _bandNearRenderPaint);

    _bandBullseyeRenderPaint.color = _bullseyePaint.color.withValues(
      alpha: _bullseyePaint.color.a * alpha,
    );
    canvas.drawPath(_bandBullseyePath, _bandBullseyeRenderPaint);
  }

  void _drawTargetMarker(
    Canvas canvas,
    _DialGeometry geom, {
    double alpha = 1.0,
  }) {
    final angle = math.pi + _target * math.pi;
    final cos = math.cos(angle);
    final sin = math.sin(angle);

    // Mutate only the alpha of the cached paints — no new Paint() allocation.
    _markerGlowPaint.color = _palette.bullseyeColor.withValues(
      alpha: 0.7 * alpha,
    );
    _markerPaint.color = _palette.bullseyeColor.withValues(alpha: alpha);

    final inner = geom.radius * 0.15;
    final outer = geom.radius * 0.98;

    canvas.drawLine(
      Offset(geom.cx + inner * cos, geom.cy + inner * sin),
      Offset(geom.cx + outer * cos, geom.cy + outer * sin),
      _markerGlowPaint,
    );
    canvas.drawLine(
      Offset(geom.cx + inner * cos, geom.cy + inner * sin),
      Offset(geom.cx + outer * cos, geom.cy + outer * sin),
      _markerPaint,
    );
  }

  void _drawNeedle(Canvas canvas, _DialGeometry geom) {
    final angle = math.pi + _needlePosition * math.pi;
    final cos = math.cos(angle);
    final sin = math.sin(angle);

    final knobRadius = geom.radius * 0.045;
    final needleLength = geom.radius * 0.92;

    _needleGlowPaint.strokeWidth = geom.radius * 0.022;
    canvas.drawLine(
      Offset(geom.cx, geom.cy),
      Offset(geom.cx + needleLength * cos, geom.cy + needleLength * sin),
      _needleGlowPaint,
    );

    _needlePaint.strokeWidth = geom.radius * 0.012;
    canvas.drawLine(
      Offset(geom.cx, geom.cy),
      Offset(geom.cx + needleLength * cos, geom.cy + needleLength * sin),
      _needlePaint,
    );

    canvas.drawCircle(Offset(geom.cx, geom.cy), knobRadius, _needleKnobPaint);
  }
}

/// Geometría computada del dial (centro y radio del semicírculo).
class _DialGeometry {
  const _DialGeometry({
    required this.cx,
    required this.cy,
    required this.radius,
  });

  final double cx;
  final double cy;
  final double radius;
}
