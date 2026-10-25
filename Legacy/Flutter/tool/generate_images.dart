// Generador de imágenes PLACEHOLDER de la app — Sajitarios Gamespot.
//
// PLACEHOLDER: estas imágenes son marcadores de posición decentes generados
// programáticamente (no arte final). Sustituye los PNG de assets/images/ por
// arte profesional cuando esté disponible (mismos nombres y proporciones) y
// vuelve a ejecutar este generador si quieres regenerar los placeholders:
//
//   dart run tool/generate_images.dart
//
// Produce:
//   assets/images/card_back.png    (500x700, dorso de carta: morado + rombos
//                                    ámbar, mismo lenguaje visual que el dibujo
//                                    de _paintBack en card_flip_game.dart)
//   assets/images/menu_header.png  (1200x600, cabecera decorativa del menú:
//                                    degradado morado + cartas/rombos ámbar)
//
// Colores de marca (ver lib/core/theme/app_theme.dart):
//   morado #7C3AED + ámbar #F59E0B.

import 'dart:io';
import 'dart:math' as math;

import 'package:image/image.dart' as img;

// Colores de marca.
final purple = img.ColorRgb8(0x7C, 0x3A, 0xED); // #7C3AED
final purpleDark = img.ColorRgb8(0x4C, 0x1D, 0x95); // morado profundo
final amber = img.ColorRgb8(0xF5, 0x9E, 0x0B); // #F59E0B
final white = img.ColorRgb8(0xFF, 0xFF, 0xFF);

void main() {
  final dir = Directory('assets/images');
  if (!dir.existsSync()) dir.createSync(recursive: true);

  _generateCardBack();
  _generateMenuHeader();

  stdout.writeln('Imágenes generadas en assets/images/ (PLACEHOLDER).');
}

/// Dorso de carta 5:7 (proporción de la carta del juego). Fondo morado, marco
/// interior ámbar y patrón de rombos ámbar translúcidos, replicando el
/// `_paintBack` dibujado por Flame para mantener coherencia visual.
void _generateCardBack() {
  const w = 500;
  const h = 700;
  final image = img.Image(width: w, height: h, numChannels: 4);

  // Fondo morado con esquinas redondeadas (sobre transparente).
  final radius = (w * 0.10).round();
  img.fillRect(
    image,
    x1: 0,
    y1: 0,
    x2: w - 1,
    y2: h - 1,
    radius: radius,
    color: purple,
  );

  // Marco interior ámbar.
  final inset = (w * 0.06).round();
  img.drawRect(
    image,
    x1: inset,
    y1: inset,
    x2: w - 1 - inset,
    y2: h - 1 - inset,
    radius: (radius * 0.7).round(),
    color: amber,
    thickness: math.max(2, (w * 0.02)).round(),
  );

  // Patrón de rombos ámbar translúcidos en una rejilla, recortado al marco.
  final diamond = img.ColorRgba8(0xF5, 0x9E, 0x0B, (0.35 * 255).round());
  final step = (w * 0.16);
  final left = inset.toDouble();
  final top = inset.toDouble();
  final right = (w - inset).toDouble();
  final bottom = (h - inset).toDouble();
  for (double y = top; y < bottom; y += step) {
    for (double x = left; x < right; x += step) {
      final cx = x + step / 2;
      final cy = y + step / 2;
      final d = step * 0.32;
      _fillDiamond(image, cx, cy, d, diamond, left, top, right, bottom);
    }
  }

  File('assets/images/card_back.png').writeAsBytesSync(img.encodePng(image));
}

/// Cabecera decorativa del menú: degradado vertical morado + rombos ámbar
/// dispersos. Se muestra (con fallback) sobre el AppBar / cabecera del menú.
void _generateMenuHeader() {
  const w = 1200;
  const h = 600;
  final image = img.Image(width: w, height: h, numChannels: 4);

  // Degradado vertical de morado oscuro (arriba) a morado de marca (abajo).
  for (var y = 0; y < h; y++) {
    final t = y / (h - 1);
    final r = _lerp(purpleDark.r, purple.r, t);
    final g = _lerp(purpleDark.g, purple.g, t);
    final b = _lerp(purpleDark.b, purple.b, t);
    img.drawLine(
      image,
      x1: 0,
      y1: y,
      x2: w - 1,
      y2: y,
      color: img.ColorRgb8(r, g, b),
    );
  }

  // Rombos ámbar dispersos (semilla fija -> reproducible).
  final rnd = math.Random(42);
  for (var i = 0; i < 40; i++) {
    final cx = rnd.nextDouble() * w;
    final cy = rnd.nextDouble() * h;
    final d = 14.0 + rnd.nextDouble() * 34.0;
    final alpha = (40 + rnd.nextInt(120));
    final c = img.ColorRgba8(0xF5, 0x9E, 0x0B, alpha);
    _fillDiamond(image, cx, cy, d, c, 0, 0, w.toDouble(), h.toDouble());
  }

  File('assets/images/menu_header.png').writeAsBytesSync(img.encodePng(image));
}

/// Rellena un rombo centrado en (cx, cy) con semidiagonal [d], recortado al
/// rectángulo [clipL, clipT, clipR, clipB] (en coordenadas de píxel).
void _fillDiamond(
  img.Image image,
  double cx,
  double cy,
  double d,
  img.Color color,
  double clipL,
  double clipT,
  double clipR,
  double clipB,
) {
  final minY = (cy - d).floor();
  final maxY = (cy + d).ceil();
  for (var y = minY; y <= maxY; y++) {
    final dy = (y - cy).abs();
    if (dy > d) continue;
    final halfW = d - dy; // ancho del rombo a esta altura
    final minX = (cx - halfW).floor();
    final maxX = (cx + halfW).ceil();
    for (var x = minX; x <= maxX; x++) {
      if (x < clipL || x >= clipR || y < clipT || y >= clipB) continue;
      if (x < 0 || x >= image.width || y < 0 || y >= image.height) continue;
      _blendPixel(image, x, y, color);
    }
  }
}

/// Mezcla [color] sobre el píxel destino respetando su alfa (alpha over).
void _blendPixel(img.Image image, int x, int y, img.Color color) {
  final a = color.a / 255.0;
  if (a >= 1.0) {
    image.setPixel(x, y, color);
    return;
  }
  final dst = image.getPixel(x, y);
  final r = _lerp(dst.r, color.r, a);
  final g = _lerp(dst.g, color.g, a);
  final b = _lerp(dst.b, color.b, a);
  image.setPixelRgb(x, y, r, g, b);
}

int _lerp(num a, num b, double t) =>
    (a + (b - a) * t).round().clamp(0, 255).toInt();
