import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/abandon_game_dialog.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_routes.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla intermedia "Pásale el móvil a {jugador}".
///
/// Aparece antes de cada revelación, mostrando de forma grande y clara a quién
/// le toca coger el móvil (según el orden de introducción, es decir
/// `session.revealOrder[currentIndex]`). Al continuar, llama a
/// `impostorFlowControllerProvider.notifier.revelar()` y navega a la ruta
/// `impostor-reveal` para que ese jugador revele su rol.
class PassDeviceScreen extends ConsumerWidget {
  const PassDeviceScreen({super.key});

  static const double _largeScreenMinWidth = 600.0;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final l10n = AppLocalizations.of(context)!;

    // Observamos el estado para reconstruir cuando cambia el jugador actual.
    final state = ref.watch(impostorFlowControllerProvider);
    // El jugador actual según el orden de introducción.
    final jugador = ref
        .read(impostorFlowControllerProvider.notifier)
        .jugadorActual;
    final nombre = jugador?.name ?? '';
    final posicion = state.currentIndex + 1;
    final total = state.totalPlayers;

    void continuar() {
      ref.read(impostorFlowControllerProvider.notifier).revelar();
      context.goNamed(kImpostorRevealRouteName);
    }

    // El botón "atrás" del sistema, sin interceptar, dejaría el flujo
    // incoherente (la ruta cambiaría pero el controlador seguiría apuntando al
    // jugador actual). Pedimos confirmación y, si el usuario quiere salir,
    // reiniciamos el flujo y volvemos a la configuración. El avance hacia
    // adelante ("Continuar") no se ve afectado.
    Future<void> confirmarSalida() async {
      final salir = await abandonarPartidaDialog(context);
      if (salir != true || !context.mounted) return;
      ref.read(impostorFlowControllerProvider.notifier).reiniciar();
      if (!context.mounted) return;
      context.goNamed(kImpostorSetupRouteName);
    }

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) {
        if (didPop) return;
        confirmarSalida();
      },
      child: Scaffold(
        appBar: AppBar(
          title: NeonText(
            l10n.impostorTitulo,
            style:
                theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
          ),
        ),
        body: NeonBackground(
          child: SafeArea(
            child: LayoutBuilder(
              builder: (context, constraints) {
                final esGrande = constraints.maxWidth > _largeScreenMinWidth;

                return Center(
                  child: ConstrainedBox(
                    constraints: const BoxConstraints(maxWidth: 520),
                    // Scrolleable para absorber el excedente (horizontal o texto
                    // grande) manteniendo el centrado vertical cuando sobra
                    // espacio (ConstrainedBox(minHeight) + IntrinsicHeight).
                    child: SingleChildScrollView(
                      child: ConstrainedBox(
                        constraints: BoxConstraints(
                          minHeight: constraints.maxHeight,
                        ),
                        child: IntrinsicHeight(
                          child: Padding(
                            padding: EdgeInsets.symmetric(
                              horizontal: esGrande ? 48 : 24,
                              vertical: 24,
                            ),
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              crossAxisAlignment: CrossAxisAlignment.stretch,
                              children: [
                                if (total > 0)
                                  Text(
                                    l10n.jugadorXDeY(posicion, total),
                                    textAlign: TextAlign.center,
                                    style: theme.textTheme.titleMedium
                                        ?.copyWith(
                                          color: colorScheme.onSurfaceVariant,
                                          fontWeight: FontWeight.w600,
                                        ),
                                  ),
                                const SizedBox(height: 24),
                                PulseGlow(
                                  builder: (context, intensity) => Container(
                                    padding: const EdgeInsets.all(20),
                                    decoration: BoxDecoration(
                                      shape: BoxShape.circle,
                                      border: Border.all(
                                        color: AppTheme.neonCyan,
                                        width: 1.5,
                                      ),
                                      boxShadow: neonGlow(
                                        color: AppTheme.neonCyan,
                                        intensity: intensity,
                                      ),
                                    ),
                                    child: Icon(
                                      Icons.phone_iphone,
                                      size: esGrande ? 96 : 72,
                                      color: colorScheme.primary,
                                    ),
                                  ),
                                ),
                                const SizedBox(height: 24),
                                Text(
                                  l10n.pasaleElMovilA,
                                  textAlign: TextAlign.center,
                                  style: theme.textTheme.headlineSmall
                                      ?.copyWith(
                                        color: colorScheme.onSurfaceVariant,
                                        fontWeight: FontWeight.w700,
                                      ),
                                ),
                                const SizedBox(height: 12),
                                NeonText(
                                  nombre,
                                  textAlign: TextAlign.center,
                                  glowColor: AppTheme.neonCyan,
                                  intensity: 1.2,
                                  maxLines: 2,
                                  overflow: TextOverflow.ellipsis,
                                  style: theme.textTheme.displayMedium
                                      ?.copyWith(
                                        color: AppTheme.textPrimary,
                                        fontWeight: FontWeight.w900,
                                      ),
                                ),
                                const SizedBox(height: 16),
                                Text(
                                  l10n.pasaleElMovilAyuda,
                                  textAlign: TextAlign.center,
                                  style: theme.textTheme.bodyLarge?.copyWith(
                                    color: colorScheme.onSurfaceVariant,
                                  ),
                                ),
                                const SizedBox(height: 40),
                                NeonGlowWrapper(
                                  color: AppTheme.neonCyan,
                                  borderRadius: const BorderRadius.all(
                                    Radius.circular(28),
                                  ),
                                  child: FilledButton.icon(
                                    onPressed: jugador == null
                                        ? null
                                        : continuar,
                                    icon: const Icon(Icons.visibility),
                                    label: Text(l10n.continuar),
                                    style: FilledButton.styleFrom(
                                      minimumSize: const Size.fromHeight(64),
                                      textStyle: const TextStyle(
                                        fontSize: 20,
                                        fontWeight: FontWeight.w800,
                                      ),
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                    ),
                  ),
                );
              },
            ),
          ),
        ),
      ),
    );
  }
}
