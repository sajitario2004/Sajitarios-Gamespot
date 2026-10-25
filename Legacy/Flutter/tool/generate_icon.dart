// Generador del icono fuente y del logo del splash — Sajitarios Gamespot.
//
// PLACEHOLDER: este icono es un marcador de posición decente generado
// programáticamente (no arte final). Sustituye los PNG de assets/icon/ por
// arte profesional cuando esté disponible y vuelve a ejecutar:
//   dart run flutter_launcher_icons
//   dart run flutter_native_splash:create
//
// Ejecuta este generador con:  dart run tool/generate_icon.dart
//
// Produce:
//   assets/icon/app_icon.png            (1024x1024, fondo morado + símbolo)
//   assets/icon/app_icon_foreground.png (1024x1024, transparente + símbolo,
//                                         con padding seguro para Android)
//   assets/icon/splash_logo.png         (1024x1024, transparente + símbolo)
//
// El símbolo es una carta de juego estilizada con un pip de pica, un guiño a
// los party games de la app (morado de marca #7C3AED + ámbar de acento).

import 'dart:io';
import 'dart:math' as math;

import 'package:image/image.dart' as img;

const int size = 1024;

// Colores de marca (ver lib/core/theme/app_theme.dart).
final purple = img.ColorRgb8(0x7C, 0x3A, 0xED); // #7C3AED
final amber = img.ColorRgb8(0xF5, 0x9E, 0x0B); // #F59E0B
final white = img.ColorRgb8(0xFF, 0xFF, 0xFF);
final darkInk = img.ColorRgb8(0x2A, 0x10, 0x55);

void main() {
  final dir = Directory('assets/icon');
  if (!dir.existsSync()) dir.createSync(recursive: true);

  // app_icon.png — fondo morado sólido + símbolo centrado.
  final appIcon = img.Image(width: size, height: size, numChannels: 4);
  img.fill(appIcon, color: purple);
  _drawSymbol(appIcon, scale: 1.0);
  File('assets/icon/app_icon.png').writeAsBytesSync(img.encodePng(appIcon));

  // app_icon_foreground.png — transparente, símbolo con padding seguro
  // (el sistema recorta ~25% en los bordes del icono adaptativo).
  final fg = img.Image(width: size, height: size, numChannels: 4);
  _drawSymbol(fg, scale: 0.66);
  File(
    'assets/icon/app_icon_foreground.png',
  ).writeAsBytesSync(img.encodePng(fg));

  // splash_logo.png — transparente + símbolo (se muestra sobre el morado).
  final splash = img.Image(width: size, height: size, numChannels: 4);
  _drawSymbol(splash, scale: 0.9);
  File('assets/icon/splash_logo.png').writeAsBytesSync(img.encodePng(splash));

  stdout.writeln('Iconos generados en assets/icon/ (PLACEHOLDER).');
}

/// Dibuja una carta de juego con un pip de pica ámbar, centrada y escalada.
void _drawSymbol(img.Image image, {required double scale}) {
  final cx = size / 2;
  final cy = size / 2;

  // Dimensiones de la carta.
  final cardW = (size * 0.50 * scale);
  final cardH = (size * 0.70 * scale);
  final left = (cx - cardW / 2).round();
  final top = (cy - cardH / 2).round();
  final right = (cx + cardW / 2).round();
  final bottom = (cy + cardH / 2).round();
  final radius = (cardW * 0.12).round();

  // Sombra suave de la carta (offset).
  img.fillRect(
    image,
    x1: left + 14,
    y1: top + 18,
    x2: right + 14,
    y2: bottom + 18,
    radius: radius,
    color: img.ColorRgba8(0, 0, 0, 60),
  );

  // Cuerpo de la carta (blanco).
  img.fillRect(
    image,
    x1: left,
    y1: top,
    x2: right,
    y2: bottom,
    radius: radius,
    color: white,
  );

  // Borde morado fino.
  img.drawRect(
    image,
    x1: left,
    y1: top,
    x2: right,
    y2: bottom,
    radius: radius,
    color: purple,
    thickness: math.max(2, (cardW * 0.02)).round(),
  );

  // Pip de pica (ámbar) centrado en la carta.
  _drawSpade(image, cx, cy + cardH * 0.04, cardW * 0.46, amber);
}

/// Dibuja un símbolo de pica relleno: dos lóbulos circulares, un triángulo
/// superior y un tallo inferior.
void _drawSpade(img.Image image, double cx, double cy, double w, img.Color c) {
  final lobeR = w * 0.27;
  final lobeY = cy + w * 0.08;

  // Lóbulos inferiores (dos círculos).
  img.fillCircle(
    image,
    x: (cx - lobeR * 0.85).round(),
    y: lobeY.round(),
    radius: lobeR.round(),
    color: c,
  );
  img.fillCircle(
    image,
    x: (cx + lobeR * 0.85).round(),
    y: lobeY.round(),
    radius: lobeR.round(),
    color: c,
  );

  // Punta superior (triángulo) que une los lóbulos hacia arriba.
  final apexY = cy - w * 0.55;
  _fillTriangle(
    image,
    cx,
    apexY, // ápice
    cx - lobeR * 1.7,
    lobeY + lobeR * 0.1, // base izq
    cx + lobeR * 1.7,
    lobeY + lobeR * 0.1, // base der
    c,
  );

  // Tallo inferior (trapecio): base ancha que se estrecha hacia los lóbulos.
  final stemTop = lobeY + lobeR * 0.6;
  final stemBottom = cy + w * 0.62;
  _fillTriangle(
    image,
    cx,
    stemTop,
    cx - w * 0.22,
    stemBottom,
    cx + w * 0.22,
    stemBottom,
    c,
  );
}

/// Relleno de triángulo por rasterizado de barrido (scanline).
void _fillTriangle(
  img.Image image,
  double ax,
  double ay,
  double bx,
  double by,
  double cx,
  double cy,
  img.Color color,
) {
  final minY = [ay, by, cy].reduce(math.min).floor();
  final maxY = [ay, by, cy].reduce(math.max).ceil();
  final minX = [ax, bx, cx].reduce(math.min).floor();
  final maxX = [ax, bx, cx].reduce(math.max).ceil();

  for (var y = minY; y <= maxY; y++) {
    for (var x = minX; x <= maxX; x++) {
      if (_pointInTriangle(
        x.toDouble(),
        y.toDouble(),
        ax,
        ay,
        bx,
        by,
        cx,
        cy,
      )) {
        if (x >= 0 && x < image.width && y >= 0 && y < image.height) {
          image.setPixel(x, y, color);
        }
      }
    }
  }
}

bool _pointInTriangle(
  double px,
  double py,
  double ax,
  double ay,
  double bx,
  double by,
  double cx,
  double cy,
) {
  final d1 = _sign(px, py, ax, ay, bx, by);
  final d2 = _sign(px, py, bx, by, cx, cy);
  final d3 = _sign(px, py, cx, cy, ax, ay);
  final hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
  final hasPos = d1 > 0 || d2 > 0 || d3 > 0;
  return !(hasNeg && hasPos);
}

double _sign(double px, double py, double ax, double ay, double bx, double by) {
  return (px - bx) * (ay - by) - (ax - bx) * (py - by);
}
