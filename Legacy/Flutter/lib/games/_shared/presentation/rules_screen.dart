import 'package:flutter/material.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla genérica de reglas "¿Cómo se juega?".
///
/// Recibe el título del juego y una lista de pasos localizados.
/// Se abre con [Navigator.push] desde el botón de reglas de cada pantalla
/// de configuración; no necesita ruta en go_router.
///
/// Los pasos se renderizan como filas numeradas, son accesibles y se adaptan
/// a textScaler grandes sin desbordarse.
class RulesScreen extends StatelessWidget {
  const RulesScreen({super.key, required this.gameTitle, required this.steps});

  /// Título del juego (localizado).
  final String gameTitle;

  /// Lista de pasos de las reglas (ya localizados).
  final List<String> steps;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;

    return Scaffold(
      appBar: AppBar(
        title: NeonText(
          l10n.comoSeJuega,
          style: theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
          glowColor: AppTheme.neonCyan,
        ),
      ),
      body: NeonBackground(
        child: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 640),
              child: ListView(
                padding: const EdgeInsets.fromLTRB(16, 16, 16, 32),
                children: [
                  NeonText(
                    gameTitle,
                    style: theme.textTheme.headlineSmall?.copyWith(
                      color: AppTheme.textPrimary,
                      fontWeight: FontWeight.w800,
                    ),
                    glowColor: AppTheme.neonCyan,
                  ),
                  const SizedBox(height: 20),
                  NeonPanel(
                    borderColor: AppTheme.neonCyan,
                    intensity: 0.6,
                    padding: const EdgeInsets.all(20),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        for (var i = 0; i < steps.length; i++) ...[
                          if (i > 0) const SizedBox(height: 16),
                          _RuleStep(number: i + 1, text: steps[i]),
                        ],
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

/// Una fila de regla numerada.
class _RuleStep extends StatelessWidget {
  const _RuleStep({required this.number, required this.text});

  final int number;
  final String text;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Semantics(
      label: '$number. $text',
      child: ExcludeSemantics(
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            NeonGlowWrapper(
              color: AppTheme.neonCyan,
              intensity: 0.7,
              borderRadius: const BorderRadius.all(Radius.circular(20)),
              child: CircleAvatar(
                radius: 14,
                backgroundColor: AppTheme.surfaceHigh,
                child: Text(
                  '$number',
                  style: theme.textTheme.labelMedium?.copyWith(
                    color: AppTheme.neonCyan,
                    fontWeight: FontWeight.w900,
                  ),
                ),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                text,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: AppTheme.textPrimary,
                  height: 1.45,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
