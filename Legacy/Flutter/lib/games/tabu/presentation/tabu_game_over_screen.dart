import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_flow_controller.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla de fin de partida de Tabú.
///
/// Anuncia el equipo ganador (el primero en llegar a [objetivoVictorias]
/// victorias de ronda) y ofrece volver al menú principal.
class TabuGameOverScreen extends ConsumerWidget {
  const TabuGameOverScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    final flowState = ref.watch(tabuFlowControllerProvider);

    final ganadorNombre = flowState.nombreGanador;
    final config = flowState.config;

    if (config == null || ganadorNombre == null) {
      return Scaffold(body: Center(child: Text(l10n.tabuNoHayPartida)));
    }

    final victoriasA = flowState.victoriasA;
    final victoriasB = flowState.victoriasB;

    return Scaffold(
      appBar: AppBar(
        leading: VolverAlMenuButton(
          onPressed: () {
            ref.read(tabuFlowControllerProvider.notifier).reiniciar();
            context.go('/');
          },
        ),
        automaticallyImplyLeading: false,
        title: NeonText(
          l10n.tabuTitulo,
          style: theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
          glowColor: AppTheme.neonCyan,
        ),
      ),
      body: NeonBackground(
        child: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 500),
              child: LayoutBuilder(
                builder: (context, constraints) {
                  return SingleChildScrollView(
                    child: ConstrainedBox(
                      constraints: BoxConstraints(
                        minHeight: constraints.maxHeight,
                      ),
                      child: IntrinsicHeight(
                        child: Padding(
                          padding: const EdgeInsets.all(24),
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              NeonText(
                                l10n.tabuFinDePartida,
                                style: theme.textTheme.headlineSmall,
                                glowColor: AppTheme.neonCyan,
                                textAlign: TextAlign.center,
                              ),
                              const SizedBox(height: 24),
                              NeonPanel(
                                borderColor: AppTheme.neonViolet,
                                padding: const EdgeInsets.all(24),
                                child: Column(
                                  children: [
                                    Text(
                                      l10n.tabuGanadorLabel,
                                      style: theme.textTheme.titleMedium
                                          ?.copyWith(
                                            color: theme
                                                .colorScheme
                                                .onSurfaceVariant,
                                          ),
                                    ),
                                    const SizedBox(height: 8),
                                    Semantics(
                                      liveRegion: true,
                                      child: NeonText(
                                        ganadorNombre,
                                        textAlign: TextAlign.center,
                                        maxLines: 2,
                                        overflow: TextOverflow.ellipsis,
                                        style: theme.textTheme.displaySmall
                                            ?.copyWith(
                                              fontWeight: FontWeight.w900,
                                              color: AppTheme.textPrimary,
                                            ),
                                        glowColor: AppTheme.neonViolet,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              const SizedBox(height: 24),
                              // Marcador final
                              Row(
                                children: [
                                  Expanded(
                                    child: _ScoreCard(
                                      nombre: config.equipoA,
                                      victorias: victoriasA,
                                      color: AppTheme.neonCyan,
                                    ),
                                  ),
                                  const SizedBox(width: 12),
                                  Expanded(
                                    child: _ScoreCard(
                                      nombre: config.equipoB,
                                      victorias: victoriasB,
                                      color: AppTheme.neonViolet,
                                    ),
                                  ),
                                ],
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
                                          .read(
                                            tabuFlowControllerProvider.notifier,
                                          )
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
                  );
                },
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _ScoreCard extends StatelessWidget {
  const _ScoreCard({
    required this.nombre,
    required this.victorias,
    required this.color,
  });

  final String nombre;
  final int victorias;
  final Color color;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return NeonPanel(
      borderColor: color,
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
      child: Column(
        children: [
          Text(
            nombre,
            style: theme.textTheme.bodyMedium?.copyWith(
              color: AppTheme.textPrimary,
              fontWeight: FontWeight.w700,
            ),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 4),
          NeonText(
            '$victorias',
            style: theme.textTheme.headlineMedium?.copyWith(
              fontWeight: FontWeight.w900,
            ),
            glowColor: color,
          ),
        ],
      ),
    );
  }
}
