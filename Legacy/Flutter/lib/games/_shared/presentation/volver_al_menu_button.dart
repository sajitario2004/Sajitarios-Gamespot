import 'package:flutter/material.dart';

import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Botón de AppBar, en la esquina superior izquierda, que devuelve al menú
/// principal. Se pasa como `leading:` de un [AppBar].
///
/// El [onPressed] decide CÓMO se vuelve, porque depende del estado del juego:
/// - Pantallas de configuración / fin de partida: navegación directa al menú
///   (`context.go('/')`), sin confirmación porque no hay nada que perder.
/// - Pantallas de partida en curso: el callback debería pedir confirmación
///   (p. ej. `abandonarPartidaDialog`) antes de reiniciar el controlador y salir.
///
/// Vive en `_shared/` para que todos los juegos usen el mismo affordance sin que
/// el menú conozca juegos concretos (regla de oro de la arquitectura).
class VolverAlMenuButton extends StatelessWidget {
  const VolverAlMenuButton({super.key, required this.onPressed});

  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return IconButton(
      icon: const Icon(Icons.home_rounded),
      tooltip: l10n.volverAlMenu,
      onPressed: onPressed,
    );
  }
}
