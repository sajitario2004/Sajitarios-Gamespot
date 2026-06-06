import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/abandon_game_dialog.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_flow_controller.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_routes.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla intermedia "Pásale el móvil al grupo" de Wavelength.
class WavelengthPassDeviceScreen extends ConsumerWidget {
  const WavelengthPassDeviceScreen({super.key});

  static const double _largeScreenMinWidth = 600.0;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final l10n = AppLocalizations.of(context)!;

    final state = ref.watch(wavelengthFlowControllerProvider);
    final session = state.session;
    final roundNum = (session?.currentRoundIndex ?? 0) + 1;
    final totalRounds = session?.totalRondas ?? 1;

    void continuar() {
      ref.read(wavelengthFlowControllerProvider.notifier).pasarDispositivo();
      context.goNamed(kWavelengthGuessRouteName);
    }

    Future<void> confirmarSalida() async {
      final salir = await abandonarPartidaDialog(context);
      if (salir != true || !context.mounted) return;
      ref.read(wavelengthFlowControllerProvider.notifier).reiniciar();
      if (!context.mounted) return;
      context.goNamed(kWavelengthSetupRouteName);
    }

    Future<void> confirmarSalidaAlMenu() async {
      final salir = await abandonarPartidaDialog(context);
      if (salir != true || !context.mounted) return;
      ref.read(wavelengthFlowControllerProvider.notifier).reiniciar();
      if (!context.mounted) return;
      context.go('/');
    }

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) {
        if (didPop) return;
        confirmarSalida();
      },
      child: Scaffold(
        appBar: AppBar(
          leading: VolverAlMenuButton(onPressed: confirmarSalidaAlMenu),
          title: NeonText(
            l10n.wavelengthTitulo,
            style:
                theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
            glowColor: AppTheme.neonCyan,
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
                                Text(
                                  l10n.wavelengthRondaXDeY(
                                    roundNum,
                                    totalRounds,
                                  ),
                                  textAlign: TextAlign.center,
                                  style: theme.textTheme.titleMedium?.copyWith(
                                    color: colorScheme.onSurfaceVariant,
                                    fontWeight: FontWeight.w600,
                                  ),
                                ),
                                const SizedBox(height: 24),
                                Icon(
                                  Icons.phone_iphone,
                                  size: esGrande ? 96 : 72,
                                  color: AppTheme.neonCyan,
                                ),
                                const SizedBox(height: 24),
                                NeonText(
                                  l10n.wavelengthPassDeviceInstruccion,
                                  textAlign: TextAlign.center,
                                  glowColor: AppTheme.neonCyan,
                                  intensity: 1.2,
                                  style: theme.textTheme.headlineSmall
                                      ?.copyWith(
                                        color: AppTheme.textPrimary,
                                        fontWeight: FontWeight.w900,
                                      ),
                                ),
                                const SizedBox(height: 16),
                                Text(
                                  l10n.wavelengthPassDeviceAyuda,
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
                                    onPressed: continuar,
                                    icon: const Icon(Icons.tune),
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
