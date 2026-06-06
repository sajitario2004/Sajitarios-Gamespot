import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_flow_controller.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_routes.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla de fin de partida de Wavelength.
///
/// Muestra la puntuación total acumulada. "Volver al menú" llama a
/// `reiniciar()` antes de salir para dejar el flujo limpio.
class WavelengthGameOverScreen extends ConsumerWidget {
  const WavelengthGameOverScreen({super.key});

  static const double _kMaxContentWidth = 560.0;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    final state = ref.watch(wavelengthFlowControllerProvider);
    final session = state.session;
    final totalScore = session?.cumulativeScore ?? 0;
    final totalRounds = session?.totalRondas ?? 0;

    void volverAlMenu() {
      ref.read(wavelengthFlowControllerProvider.notifier).reiniciar();
      context.goNamed('menu');
    }

    void jugarOtra() {
      ref.read(wavelengthFlowControllerProvider.notifier).reiniciar();
      context.goNamed(kWavelengthSetupRouteName);
    }

    return Scaffold(
      appBar: AppBar(
        leading: VolverAlMenuButton(
          onPressed: () {
            ref.read(wavelengthFlowControllerProvider.notifier).reiniciar();
            context.goNamed('menu');
          },
        ),
        title: NeonText(
          l10n.wavelengthFinDePartida,
          style: theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
          glowColor: AppTheme.neonCyan,
        ),
        automaticallyImplyLeading: false,
      ),
      body: NeonBackground(
        child: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: _kMaxContentWidth),
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Expanded(
                      child: Center(
                        child: SingleChildScrollView(
                          child: NeonPanel(
                            borderColor: AppTheme.neonCyan,
                            borderWidth: 2.0,
                            padding: const EdgeInsets.all(28),
                            child: Column(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Icon(
                                  Icons.tune,
                                  size: 72,
                                  color: AppTheme.neonCyan,
                                  shadows: neonTextShadows(
                                    color: AppTheme.neonCyan,
                                  ),
                                ),
                                const SizedBox(height: 20),
                                NeonText(
                                  l10n.wavelengthPuntuacionFinal,
                                  glowColor: AppTheme.neonCyan,
                                  style: theme.textTheme.headlineSmall
                                      ?.copyWith(
                                        color: AppTheme.textPrimary,
                                        fontWeight: FontWeight.w900,
                                      ),
                                  textAlign: TextAlign.center,
                                ),
                                const SizedBox(height: 12),
                                NeonText(
                                  l10n.wavelengthPuntosTotales(totalScore),
                                  glowColor: AppTheme.neonCyan,
                                  intensity: 1.4,
                                  style: theme.textTheme.displaySmall?.copyWith(
                                    color: AppTheme.textPrimary,
                                    fontWeight: FontWeight.w900,
                                  ),
                                  textAlign: TextAlign.center,
                                ),
                                if (totalRounds > 0) ...[
                                  const SizedBox(height: 8),
                                  Text(
                                    '${totalRounds > 0 ? (totalScore / totalRounds).toStringAsFixed(1) : '0.0'} puntos/ronda',
                                    style: theme.textTheme.bodyMedium?.copyWith(
                                      color: theme.colorScheme.onSurfaceVariant,
                                    ),
                                    textAlign: TextAlign.center,
                                  ),
                                ],
                              ],
                            ),
                          ),
                        ),
                      ),
                    ),
                    Padding(
                      padding: const EdgeInsets.fromLTRB(0, 16, 0, 0),
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          SizedBox(
                            width: double.infinity,
                            child: NeonGlowWrapper(
                              color: AppTheme.neonCyan,
                              borderRadius: const BorderRadius.all(
                                Radius.circular(24),
                              ),
                              child: FilledButton.icon(
                                onPressed: jugarOtra,
                                icon: const Icon(Icons.replay),
                                label: Text(l10n.wavelengthJugarOtra),
                              ),
                            ),
                          ),
                          const SizedBox(height: 8),
                          SizedBox(
                            width: double.infinity,
                            child: OutlinedButton.icon(
                              onPressed: volverAlMenu,
                              icon: const Icon(Icons.home_outlined),
                              label: Text(l10n.volverAlMenu),
                            ),
                          ),
                        ],
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
  }
}
