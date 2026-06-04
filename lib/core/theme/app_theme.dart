import 'package:flutter/material.dart';

/// Tema Material 3 de Sajitarios Gamespot — estética **cyberpunk neón**.
///
/// Dirección estética (Fase F11): cian eléctrico + magenta sobre fondo casi
/// negro azulado, "full neón" (resplandores, bordes brillantes, texto con
/// glow). El neón solo luce sobre oscuro, así que el tema oscuro neón es la
/// experiencia principal y la única que se sirve.
///
/// API pública conservada para no romper imports: `AppTheme.light`,
/// `AppTheme.dark` y `AppTheme.neon` devuelven todos el tema neón oscuro.
class AppTheme {
  AppTheme._();

  // --- Paleta neón (constantes públicas) -----------------------------------

  /// Fondo base de la app: casi negro azulado.
  static const Color background = Color(0xFF0A0A14);

  /// Superficie oscura (cards, paneles).
  static const Color surface = Color(0xFF14141F);

  /// Superficie un poco más clara (elementos elevados / contenedores).
  static const Color surfaceHigh = Color(0xFF1B1B2A);

  /// Cian eléctrico — acento primario (con glow).
  static const Color neonCyan = Color(0xFF00F0FF);

  /// Magenta — acento secundario (con glow).
  static const Color neonMagenta = Color(0xFFFF00E5);

  /// Violeta de apoyo — acento terciario.
  ///
  /// CONTRASTE / A11y (WCAG 2.2): `#B026FF` sobre las superficies oscuras del
  /// tema NO alcanza 4.5:1 para texto pequeño (≈3.97:1 sobre [surface],
  /// ≈4.28:1 sobre [background]). Por tanto su uso queda RESTRINGIDO a:
  ///   - elementos **decorativos** (glow, bordes, degradados),
  ///   - **iconos** (componentes gráficos, exentos del 4.5:1),
  ///   - **texto grande** (≥18pt normal o ≥14pt bold, umbral 3:1).
  /// Para texto pequeño usa [neonVioletText] (violeta más claro y legible) o
  /// [textPrimary]. El [ColorScheme] de este tema aplica esta regla: los roles
  /// de violeta destinados a *texto* usan [neonVioletText], no [neonViolet].
  static const Color neonViolet = Color(0xFFB026FF);

  /// Violeta legible para **texto** sobre superficies oscuras.
  ///
  /// Variante más clara de [neonViolet] que sí cumple contraste para texto
  /// pequeño (≈6.8:1 sobre [surface], ≈7.3:1 sobre [background]) manteniendo la
  /// coherencia visual neón. Úsalo cuando el violeta deba aplicarse a texto.
  static const Color neonVioletText = Color(0xFFC77DFF);

  /// Texto base claro con buen contraste sobre el fondo oscuro.
  static const Color textPrimary = Color(0xFFE6F7FF);

  /// Texto secundario / atenuado (sigue siendo legible).
  static const Color textMuted = Color(0xFFA8B6C8);

  /// Color de error neón (rosa/rojo brillante).
  static const Color neonError = Color(0xFFFF3D71);

  // --- Compatibilidad con la API anterior ----------------------------------

  /// Color semilla (ahora el cian neón). Conservado por compatibilidad.
  static const Color seedColor = neonCyan;

  /// Acento secundario (ahora magenta). Conservado por compatibilidad.
  static const Color accentColor = neonMagenta;

  /// Tema principal de la app (neón oscuro).
  static ThemeData get neon => _build();

  /// Conservado por compatibilidad: devuelve el mismo tema neón oscuro.
  static ThemeData get light => _build();

  /// Conservado por compatibilidad: devuelve el mismo tema neón oscuro.
  static ThemeData get dark => _build();

  static ThemeData _build() {
    const colorScheme = ColorScheme(
      brightness: Brightness.dark,
      primary: neonCyan,
      onPrimary: Color(0xFF001417),
      primaryContainer: Color(0xFF003A40),
      onPrimaryContainer: neonCyan,
      secondary: neonMagenta,
      onSecondary: Color(0xFF1A0017),
      secondaryContainer: Color(0xFF3D0037),
      onSecondaryContainer: neonMagenta,
      // `tertiary` se reserva como acento decorativo/grande (glow, iconos,
      // contenedores). Los roles que Material pinta como TEXTO usan el
      // violeta legible [neonVioletText] para cumplir contraste 4.5:1.
      tertiary: neonViolet,
      onTertiary: Color(0xFF14001F),
      tertiaryContainer: Color(0xFF2E0A47),
      // Texto sobre `tertiaryContainer`: legible (≈cumple) con el violeta claro.
      onTertiaryContainer: neonVioletText,
      error: neonError,
      onError: Color(0xFF1A0009),
      errorContainer: Color(0xFF45000F),
      onErrorContainer: neonError,
      surface: surface,
      onSurface: textPrimary,
      onSurfaceVariant: textMuted,
      surfaceContainerLowest: background,
      surfaceContainerLow: Color(0xFF11111B),
      surfaceContainer: surface,
      surfaceContainerHigh: surfaceHigh,
      surfaceContainerHighest: Color(0xFF222234),
      inverseSurface: textPrimary,
      onInverseSurface: background,
      inversePrimary: neonViolet,
      outline: Color(0xFF3A3A55),
      outlineVariant: Color(0xFF26263A),
      shadow: Color(0xFF000000),
      scrim: Color(0xCC000000),
      surfaceTint: neonCyan,
    );

    final base = ThemeData(
      useMaterial3: true,
      colorScheme: colorScheme,
      brightness: Brightness.dark,
    );

    return base.copyWith(
      scaffoldBackgroundColor: background,
      canvasColor: background,
      textTheme: _textTheme(base.textTheme, colorScheme),
      iconTheme: const IconThemeData(color: neonCyan),
      dividerTheme: const DividerThemeData(
        color: Color(0xFF26263A),
        thickness: 1,
      ),
      appBarTheme: const AppBarTheme(
        centerTitle: true,
        backgroundColor: background,
        foregroundColor: textPrimary,
        surfaceTintColor: Colors.transparent,
        elevation: 0,
        scrolledUnderElevation: 0,
        iconTheme: IconThemeData(color: neonCyan),
        titleTextStyle: TextStyle(
          color: textPrimary,
          fontSize: 22,
          fontWeight: FontWeight.w800,
          letterSpacing: 0.6,
          shadows: [
            Shadow(color: neonCyan, blurRadius: 12),
            Shadow(color: neonCyan, blurRadius: 24),
          ],
        ),
      ),
      cardTheme: CardThemeData(
        color: surface,
        elevation: 0,
        clipBehavior: Clip.antiAlias,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
          side: BorderSide(color: neonCyan.withValues(alpha: 0.35), width: 1.2),
        ),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style:
            FilledButton.styleFrom(
              backgroundColor: neonCyan,
              foregroundColor: colorScheme.onPrimary,
              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
              textStyle: const TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.w800,
              ),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(16),
                side: const BorderSide(color: neonCyan, width: 1.5),
              ),
            ).copyWith(
              overlayColor: WidgetStatePropertyAll(
                neonMagenta.withValues(alpha: 0.18),
              ),
            ),
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: surfaceHigh,
          foregroundColor: neonCyan,
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
          textStyle: const TextStyle(fontSize: 18, fontWeight: FontWeight.w800),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
            side: const BorderSide(color: neonCyan, width: 1.5),
          ),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          foregroundColor: neonCyan,
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
          textStyle: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700),
          side: const BorderSide(color: neonCyan, width: 1.5),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
        ),
      ),
      textButtonTheme: TextButtonThemeData(
        style: TextButton.styleFrom(
          foregroundColor: neonCyan,
          textStyle: const TextStyle(fontWeight: FontWeight.w700),
        ),
      ),
      iconButtonTheme: IconButtonThemeData(
        style: IconButton.styleFrom(foregroundColor: neonCyan),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: surfaceHigh,
        labelStyle: const TextStyle(color: textMuted),
        floatingLabelStyle: const TextStyle(color: neonCyan),
        hintStyle: const TextStyle(color: textMuted),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: BorderSide(color: neonCyan.withValues(alpha: 0.3)),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: BorderSide(color: neonCyan.withValues(alpha: 0.3)),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: const BorderSide(color: neonCyan, width: 2),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: const BorderSide(color: neonError, width: 1.5),
        ),
        focusedErrorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: const BorderSide(color: neonError, width: 2),
        ),
      ),
      switchTheme: SwitchThemeData(
        thumbColor: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) return neonCyan;
          return textMuted;
        }),
        trackColor: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) {
            return neonCyan.withValues(alpha: 0.4);
          }
          return surfaceHigh;
        }),
        trackOutlineColor: WidgetStateProperty.resolveWith((states) {
          if (states.contains(WidgetState.selected)) return neonCyan;
          return const Color(0xFF3A3A55);
        }),
      ),
      sliderTheme: SliderThemeData(
        activeTrackColor: neonCyan,
        inactiveTrackColor: surfaceHigh,
        thumbColor: neonCyan,
        overlayColor: neonCyan.withValues(alpha: 0.18),
      ),
      snackBarTheme: SnackBarThemeData(
        behavior: SnackBarBehavior.floating,
        backgroundColor: surfaceHigh,
        contentTextStyle: const TextStyle(color: textPrimary),
        actionTextColor: neonCyan,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
          side: BorderSide(color: neonCyan.withValues(alpha: 0.5)),
        ),
      ),
      dialogTheme: DialogThemeData(
        backgroundColor: surface,
        surfaceTintColor: Colors.transparent,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
          side: BorderSide(color: neonMagenta.withValues(alpha: 0.45)),
        ),
        titleTextStyle: const TextStyle(
          color: textPrimary,
          fontSize: 20,
          fontWeight: FontWeight.w800,
        ),
        contentTextStyle: const TextStyle(color: textPrimary, fontSize: 16),
      ),
      bottomSheetTheme: const BottomSheetThemeData(
        backgroundColor: surface,
        surfaceTintColor: Colors.transparent,
      ),
      listTileTheme: const ListTileThemeData(
        iconColor: neonCyan,
        textColor: textPrimary,
      ),
      expansionTileTheme: const ExpansionTileThemeData(
        iconColor: neonCyan,
        collapsedIconColor: textMuted,
        textColor: neonCyan,
        collapsedTextColor: textPrimary,
      ),
      chipTheme: ChipThemeData(
        backgroundColor: surfaceHigh,
        side: BorderSide(color: neonCyan.withValues(alpha: 0.4)),
        labelStyle: const TextStyle(color: textPrimary),
      ),
      progressIndicatorTheme: const ProgressIndicatorThemeData(color: neonCyan),
    );
  }

  static TextTheme _textTheme(TextTheme base, ColorScheme scheme) {
    return base
        .apply(bodyColor: textPrimary, displayColor: textPrimary)
        .copyWith(
          displayLarge: base.displayLarge?.copyWith(
            color: textPrimary,
            fontWeight: FontWeight.w900,
            letterSpacing: -0.5,
          ),
          headlineLarge: base.headlineLarge?.copyWith(
            color: textPrimary,
            fontWeight: FontWeight.w800,
          ),
          headlineMedium: base.headlineMedium?.copyWith(
            color: textPrimary,
            fontWeight: FontWeight.w800,
          ),
          titleLarge: base.titleLarge?.copyWith(
            color: textPrimary,
            fontWeight: FontWeight.w700,
          ),
          labelLarge: base.labelLarge?.copyWith(
            fontWeight: FontWeight.w700,
            letterSpacing: 0.3,
          ),
        );
  }
}
