/// Botón reutilizable para silenciar/activar el sonido de la app.
///
/// Lee el estado on/off de `audioEnabledProvider` y lo alterna al pulsar (vía
/// `audioServiceProvider.toggle()`, que mantiene sincronizados servicio y
/// estado). Muestra [Icons.volume_up] cuando el sonido está activo y
/// [Icons.volume_off] cuando está silenciado.
///
/// NO está integrado todavía en ninguna pantalla concreta: eso corresponde a
/// 0.40 / 0.41. Aquí solo se ofrece el control listo para reutilizar (p. ej. en
/// el `AppBar` de las pantallas de juego).
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/audio/audio_service.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// IconButton que alterna el sonido de la app.
class MuteButton extends ConsumerWidget {
  const MuteButton({super.key, this.color});

  /// Color opcional del icono (por defecto, el del tema).
  final Color? color;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final enabled = ref.watch(audioEnabledProvider).enabled;
    final l10n = AppLocalizations.of(context)!;
    return IconButton(
      color: color,
      icon: Icon(enabled ? Icons.volume_up : Icons.volume_off),
      tooltip: enabled ? l10n.silenciarSonido : l10n.activarSonido,
      onPressed: () => ref.read(audioServiceProvider).toggle(),
    );
  }
}
