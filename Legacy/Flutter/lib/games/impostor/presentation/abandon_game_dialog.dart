import 'package:flutter/material.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Diálogo de confirmación para abandonar una partida del Impostor en curso.
///
/// Se usa al interceptar el botón "atrás" del sistema durante el flujo
/// (pass/reveal): dejar salir sin avisar dejaría la sesión a medias en el
/// controlador y, sobre todo, podría filtrar el rol del jugador actual. Devuelve
/// `true` si el usuario confirma que quiere salir, `false`/`null` si cancela.
Future<bool?> abandonarPartidaDialog(BuildContext context) {
  return showDialog<bool>(
    context: context,
    builder: (dialogContext) {
      final l10n = AppLocalizations.of(dialogContext)!;
      final theme = Theme.of(dialogContext);
      return AlertDialog(
        title: NeonText(
          l10n.salirDeLaPartidaTitulo,
          glowColor: AppTheme.neonMagenta,
          style: theme.textTheme.titleLarge,
        ),
        content: Text(l10n.salirDeLaPartidaMensaje),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(false),
            child: Text(l10n.seguirJugando),
          ),
          NeonGlowWrapper(
            color: AppTheme.neonMagenta,
            borderRadius: const BorderRadius.all(Radius.circular(20)),
            child: FilledButton(
              onPressed: () => Navigator.of(dialogContext).pop(true),
              child: Text(l10n.salir),
            ),
          ),
        ],
      );
    },
  );
}
