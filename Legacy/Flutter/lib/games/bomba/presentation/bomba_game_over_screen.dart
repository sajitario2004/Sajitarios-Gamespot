import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_flow_controller.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla de fin de partida de La Bomba.
///
/// Anuncia el último jugador en pie (ganador) y ofrece volver al menú.
/// Llama a [BombaFlowController.reiniciar] antes de navegar al menú para
/// limpiar el estado de la partida.
class BombaGameOverScreen extends ConsumerWidget {
  const BombaGameOverScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    final flowState = ref.watch(bombaFlowControllerProvider);

    final winner = flowState.winner;

    if (winner == null) {
      return Scaffold(body: Center(child: Text(l10n.bombaNoHayPartida)));
    }

    return Scaffold(
      appBar: AppBar(
        leading: VolverAlMenuButton(
          onPressed: () {
            ref.read(bombaFlowControllerProvider.notifier).reiniciar();
            context.go('/');
          },
        ),
        automaticallyImplyLeading: false,
        title: NeonText(
          l10n.bombaTitulo,
          style: theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
          glowColor: AppTheme.neonError,
        ),
      ),
      body: NeonBackground(
        child: SafeArea(
          child: SingleChildScrollView(
            child: Center(
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 500),
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 24,
                    vertical: 40,
                  ),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(
                        Icons.emoji_events_outlined,
                        size: 64,
                        color: AppTheme.neonCyan,
                      ),
                      const SizedBox(height: 16),
                      NeonText(
                        l10n.bombaFinDePartida,
                        textAlign: TextAlign.center,
                        style: theme.textTheme.headlineSmall,
                        glowColor: AppTheme.neonCyan,
                      ),
                      const SizedBox(height: 24),
                      NeonPanel(
                        borderColor: AppTheme.neonCyan,
                        padding: const EdgeInsets.all(24),
                        child: Column(
                          children: [
                            Text(
                              l10n.bombaGanadorLabel,
                              style: theme.textTheme.titleMedium?.copyWith(
                                color: theme.colorScheme.onSurfaceVariant,
                              ),
                            ),
                            const SizedBox(height: 8),
                            Semantics(
                              liveRegion: true,
                              // FittedBox: un nombre larguísimo (incluso una sola
                              // palabra sin espacios) se reduce hasta caber en el
                              // ancho disponible en vez de desbordar.
                              child: FittedBox(
                                fit: BoxFit.scaleDown,
                                child: NeonText(
                                  winner,
                                  textAlign: TextAlign.center,
                                  maxLines: 1,
                                  overflow: TextOverflow.ellipsis,
                                  style: theme.textTheme.displaySmall?.copyWith(
                                    fontWeight: FontWeight.w900,
                                    color: AppTheme.textPrimary,
                                  ),
                                  glowColor: AppTheme.neonCyan,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 40),
                      SizedBox(
                        width: double.infinity,
                        child: NeonGlowWrapper(
                          color: AppTheme.neonCyan,
                          borderRadius: const BorderRadius.all(
                            Radius.circular(28),
                          ),
                          child: FilledButton.icon(
                            onPressed: () {
                              ref
                                  .read(bombaFlowControllerProvider.notifier)
                                  .reiniciar();
                              context.go('/');
                            },
                            icon: const Icon(Icons.home_outlined),
                            label: Text(l10n.volverAlMenu),
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
      ),
    );
  }
}
