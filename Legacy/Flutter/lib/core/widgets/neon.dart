/// Sistema de diseño neón (Fase F11, v0.46).
///
/// Widgets y helpers reutilizables para la estética cyberpunk cian + magenta
/// sobre fondo oscuro. El glow es **decorativo**: el texto mantiene buen
/// contraste (claro sobre oscuro) y los resplandores se construyen como
/// `BoxShadow`/`Shadow` del color de acento.
///
/// API pública (de la que dependen las pantallas):
/// - `neonGlow({color, intensity, spread})` → `List<BoxShadow>`.
/// - `neonTextShadows({color, intensity})` → `List<Shadow>`.
/// - `NeonText` — Text con glow.
/// - `NeonPanel` — contenedor con fondo oscuro, borde brillante y glow.
/// - `NeonGlowWrapper` — envuelve cualquier widget (botón, icono) con glow.
/// - `NeonBackground` — fondo oscuro con rejilla/grid sutil y degradado radial.
/// - `PulseGlow` — anima suavemente la intensidad del glow de su hijo.
library;

import 'package:flutter/material.dart';

import '../theme/app_theme.dart';

/// Caché de memoización para listas de glow generadas con los mismos
/// parámetros (color/intensity/spread). Evita asignar nuevas `List<BoxShadow>`
/// en cada `build` cuando los valores se repiten (caso muy frecuente: glow
/// estático de paneles, wrappers e iconos). Tamaño acotado para no crecer
/// indefinidamente.
final Map<int, List<BoxShadow>> _glowCache = <int, List<BoxShadow>>{};

/// Caché de memoización para sombras de texto neón frecuentes.
final Map<int, List<Shadow>> _textShadowCache = <int, List<Shadow>>{};

const int _maxCacheEntries = 64;

/// Construye una lista de [BoxShadow] de color para efecto resplandor neón.
///
/// El resultado se cachea (memoiza) por combinación de
/// `color`/`intensity`/`spread`, de modo que builds repetidos con los mismos
/// parámetros reutilizan la misma lista inmutable en vez de asignar una nueva.
/// No mutes la lista devuelta.
///
/// - [color]: color del glow (por defecto cian neón).
/// - [intensity]: multiplicador del blur/opacidad (1.0 = estándar).
/// - [spread]: spreadRadius base.
List<BoxShadow> neonGlow({
  Color color = AppTheme.neonCyan,
  double intensity = 1.0,
  double spread = 0.0,
}) {
  final key = Object.hash(color, intensity, spread);
  final cached = _glowCache[key];
  if (cached != null) return cached;

  final shadows = List<BoxShadow>.unmodifiable([
    BoxShadow(
      color: color.withValues(alpha: 0.55 * intensity.clamp(0.0, 1.0)),
      blurRadius: 12 * intensity,
      spreadRadius: spread,
    ),
    BoxShadow(
      color: color.withValues(alpha: 0.30 * intensity.clamp(0.0, 1.0)),
      blurRadius: 28 * intensity,
      spreadRadius: spread + 1,
    ),
  ]);
  if (_glowCache.length >= _maxCacheEntries) _glowCache.clear();
  _glowCache[key] = shadows;
  return shadows;
}

/// Construye sombras de texto ([Shadow]) para un efecto glow del mismo color.
///
/// Igual que [neonGlow], el resultado se memoiza por `color`/`intensity`.
/// No mutes la lista devuelta.
List<Shadow> neonTextShadows({
  Color color = AppTheme.neonCyan,
  double intensity = 1.0,
}) {
  final key = Object.hash(color, intensity);
  final cached = _textShadowCache[key];
  if (cached != null) return cached;

  final shadows = List<Shadow>.unmodifiable([
    Shadow(color: color.withValues(alpha: 0.9), blurRadius: 8 * intensity),
    Shadow(color: color.withValues(alpha: 0.6), blurRadius: 18 * intensity),
    Shadow(color: color.withValues(alpha: 0.4), blurRadius: 32 * intensity),
  ]);
  if (_textShadowCache.length >= _maxCacheEntries) _textShadowCache.clear();
  _textShadowCache[key] = shadows;
  return shadows;
}

/// Texto con resplandor neón. Acepta un [style] que se fusiona con el glow.
///
/// El color del texto se mantiene claro/legible; el glow usa [glowColor].
class NeonText extends StatelessWidget {
  const NeonText(
    this.data, {
    super.key,
    this.style,
    this.glowColor = AppTheme.neonCyan,
    this.intensity = 1.0,
    this.textAlign,
    this.maxLines,
    this.overflow,
  });

  final String data;
  final TextStyle? style;
  final Color glowColor;
  final double intensity;
  final TextAlign? textAlign;
  final int? maxLines;
  final TextOverflow? overflow;

  @override
  Widget build(BuildContext context) {
    final base = style ?? DefaultTextStyle.of(context).style;
    return Text(
      data,
      textAlign: textAlign,
      maxLines: maxLines,
      overflow: overflow,
      style: base.copyWith(
        shadows: neonTextShadows(color: glowColor, intensity: intensity),
      ),
    );
  }
}

/// Contenedor con fondo oscuro, borde de color brillante y glow exterior.
///
/// Usable como tarjeta/panel neón. El [child] vive sobre [backgroundColor].
class NeonPanel extends StatelessWidget {
  const NeonPanel({
    super.key,
    required this.child,
    this.borderColor = AppTheme.neonCyan,
    this.backgroundColor = AppTheme.surface,
    this.intensity = 1.0,
    this.borderWidth = 1.5,
    this.borderRadius = const BorderRadius.all(Radius.circular(20)),
    this.padding = const EdgeInsets.all(16),
    this.margin,
    this.glow = true,
  });

  final Widget child;
  final Color borderColor;
  final Color backgroundColor;
  final double intensity;
  final double borderWidth;
  final BorderRadius borderRadius;
  final EdgeInsetsGeometry padding;
  final EdgeInsetsGeometry? margin;
  final bool glow;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: margin,
      padding: padding,
      decoration: BoxDecoration(
        color: backgroundColor,
        borderRadius: borderRadius,
        border: Border.all(color: borderColor, width: borderWidth),
        boxShadow: glow
            ? neonGlow(color: borderColor, intensity: intensity)
            : null,
      ),
      child: child,
    );
  }
}

/// Envuelve cualquier widget (p.ej. un FilledButton o IconButton) con un
/// resplandor neón sin alterar el widget interno ni el árbol tipado.
class NeonGlowWrapper extends StatelessWidget {
  const NeonGlowWrapper({
    super.key,
    required this.child,
    this.color = AppTheme.neonCyan,
    this.intensity = 1.0,
    this.borderRadius = const BorderRadius.all(Radius.circular(16)),
  });

  final Widget child;
  final Color color;
  final double intensity;
  final BorderRadius borderRadius;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        borderRadius: borderRadius,
        boxShadow: neonGlow(color: color, intensity: intensity),
      ),
      child: child,
    );
  }
}

/// Fondo neón: color base oscuro con un degradado radial tenue y una rejilla
/// (grid) sutil dibujada por encima. Pensado para usarse como fondo de un
/// `Scaffold` (p.ej. envolviendo el body) o como `Stack` de fondo.
class NeonBackground extends StatelessWidget {
  const NeonBackground({
    super.key,
    this.child,
    this.baseColor = AppTheme.background,
    this.gridColor = AppTheme.neonCyan,
    this.glowColor = AppTheme.neonMagenta,
    this.gridSpacing = 36.0,
    this.showGrid = true,
  });

  final Widget? child;
  final Color baseColor;
  final Color gridColor;
  final Color glowColor;
  final double gridSpacing;
  final bool showGrid;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        gradient: RadialGradient(
          center: const Alignment(-0.6, -0.8),
          radius: 1.4,
          colors: [
            Color.alphaBlend(glowColor.withValues(alpha: 0.10), baseColor),
            baseColor,
          ],
        ),
      ),
      child: showGrid
          // La rejilla es estática; aislarla en su propia capa con un
          // RepaintBoundary evita que un repaint del contenido animado del
          // [child] vuelva a pintar el grid (y viceversa).
          ? Stack(
              fit: StackFit.passthrough,
              children: [
                Positioned.fill(
                  child: RepaintBoundary(
                    child: CustomPaint(
                      painter: _NeonGridPainter(
                        color: gridColor,
                        spacing: gridSpacing,
                      ),
                    ),
                  ),
                ),
                ?child,
              ],
            )
          : child,
    );
  }
}

class _NeonGridPainter extends CustomPainter {
  const _NeonGridPainter({required this.color, required this.spacing});

  final Color color;
  final double spacing;

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = color.withValues(alpha: 0.05)
      ..strokeWidth = 1.0;
    for (double x = 0; x <= size.width; x += spacing) {
      canvas.drawLine(Offset(x, 0), Offset(x, size.height), paint);
    }
    for (double y = 0; y <= size.height; y += spacing) {
      canvas.drawLine(Offset(0, y), Offset(size.width, y), paint);
    }
  }

  @override
  bool shouldRepaint(_NeonGridPainter oldDelegate) =>
      oldDelegate.color != color || oldDelegate.spacing != spacing;
}

/// Anima suavemente la intensidad del glow de su hijo (pulso sutil),
/// ideal para títulos o acentos. Usa un [AnimatedBuilder] con un controlador
/// en repeat reverse. El hijo se reconstruye con la intensidad animada vía
/// [builder].
///
/// Rendimiento: opcionalmente, aporta el contenido estático caro
/// (Column/Icon/Text) vía [child]; el `AnimatedBuilder` lo construye una sola
/// vez y lo reenvía sin reconstruirlo cada frame. En ese caso usa el
/// [childBuilder] (que recibe la intensidad y el [child] ya construido) solo
/// para envolverlo con la decoración de glow animada — Column/Icon/Text NO se
/// reconstruyen por frame. Si no usas [childBuilder]/[child], el [builder]
/// clásico (`context, intensity`) sigue funcionando igual (compatible).
///
/// Accesibilidad: respeta *reduce motion*. Si el sistema pide desactivar
/// animaciones ([MediaQuery.maybeDisableAnimationsOf]), no se llama a
/// `repeat()` y se emite una intensidad fija (la media entre min y max).
class PulseGlow extends StatefulWidget {
  const PulseGlow({
    super.key,
    required this.builder,
    this.minIntensity = 0.7,
    this.maxIntensity = 1.2,
    this.duration = const Duration(milliseconds: 1600),
  }) : childBuilder = null,
       child = null;

  /// Variante optimizada: separa el contenido estático ([child]) de la
  /// decoración de glow animada ([childBuilder]). El [child] se construye una
  /// vez y se reusa en cada frame sin reconstruirse.
  const PulseGlow.withChild({
    super.key,
    required this.childBuilder,
    required this.child,
    this.minIntensity = 0.7,
    this.maxIntensity = 1.2,
    this.duration = const Duration(milliseconds: 1600),
  }) : builder = null;

  /// Construye el hijo dado el valor de intensidad animado (entre min y max).
  /// API clásica/compatible.
  final Widget Function(BuildContext context, double intensity)? builder;

  /// Variante de [builder] que recibe además el [child] estático ya
  /// construido (no reconstruido por frame).
  final Widget Function(BuildContext context, double intensity, Widget child)?
  childBuilder;

  /// Contenido estático opcional usado por [PulseGlow.withChild]. Se construye
  /// una vez y se reusa en cada frame de la animación.
  final Widget? child;

  final double minIntensity;
  final double maxIntensity;
  final Duration duration;

  @override
  State<PulseGlow> createState() => _PulseGlowState();
}

class _PulseGlowState extends State<PulseGlow>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller;
  late Animation<double> _animation;
  bool _disableAnimations = false;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(vsync: this, duration: widget.duration);
    _animation = _buildAnimation();
  }

  Animation<double> _buildAnimation() {
    return Tween<double>(
      begin: widget.minIntensity,
      end: widget.maxIntensity,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeInOut));
  }

  /// Intensidad fija usada cuando *reduce motion* está activo.
  double get _fixedIntensity => (widget.minIntensity + widget.maxIntensity) / 2;

  void _syncAnimationState() {
    if (_disableAnimations) {
      if (_controller.isAnimating) _controller.stop();
    } else {
      if (!_controller.isAnimating) {
        _controller.repeat(reverse: true);
      }
    }
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    _disableAnimations = MediaQuery.maybeDisableAnimationsOf(context) ?? false;
    _syncAnimationState();
  }

  @override
  void didUpdateWidget(PulseGlow oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.duration != widget.duration) {
      _controller.duration = widget.duration;
    }
    if (oldWidget.minIntensity != widget.minIntensity ||
        oldWidget.maxIntensity != widget.maxIntensity) {
      _animation = _buildAnimation();
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    // Variante optimizada: child estático + childBuilder para el glow.
    if (widget.childBuilder != null) {
      final staticChild = widget.child!;
      if (_disableAnimations) {
        // Sin animación: una sola construcción con intensidad fija media.
        return widget.childBuilder!(context, _fixedIntensity, staticChild);
      }
      return AnimatedBuilder(
        animation: _animation,
        // El [child] estático se pasa aquí: el AnimatedBuilder NO lo
        // reconstruye por frame, solo re-ejecuta el builder que aplica el
        // glow animado. Column/Icon/Text no se reconstruyen cada frame.
        child: staticChild,
        builder: (context, child) =>
            widget.childBuilder!(context, _animation.value, child!),
      );
    }

    // API clásica/compatible: builder(context, intensity).
    final builder = widget.builder!;
    if (_disableAnimations) {
      return builder(context, _fixedIntensity);
    }
    return AnimatedBuilder(
      animation: _animation,
      builder: (context, _) => builder(context, _animation.value),
    );
  }
}
