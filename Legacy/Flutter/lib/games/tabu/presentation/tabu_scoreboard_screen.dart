import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/abandon_game_dialog.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_flow_controller.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_routes.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla de marcador entre turnos de Tabú.
///
/// Muestra las victorias de ronda acumuladas de cada equipo y un botón para
/// continuar al siguiente turno.
class TabuScoreboardScreen extends ConsumerWidget {
  const TabuScoreboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    final flowState = ref.watch(tabuFlowControllerProvider);
    final config = flowState.config;

    if (config == null) {
      return Scaffold(body: Center(child: Text(l10n.tabuNoHayPartida)));
    }

    final victoriasA = flowState.victoriasA;
    final victoriasB = flowState.victoriasB;
    final objetivo = config.objetivoVictorias;

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) async {
        if (didPop) return;
        final confirmed = await _confirmarSalida(context, l10n);
        if (confirmed && context.mounted) {
          ref.read(tabuFlowControllerProvider.notifier).reiniciar();
          context.go('/');
        }
      },
      child: Scaffold(
        appBar: AppBar(
          leading: VolverAlMenuButton(
            onPressed: () async {
              final salir = await abandonarPartidaDialog(context);
              if (salir != true || !context.mounted) return;
              ref.read(tabuFlowControllerProvider.notifier).reiniciar();
              if (!context.mounted) return;
              context.go('/');
            },
          ),
          automaticallyImplyLeading: false,
          title: NeonText(
            l10n.tabuTitulo,
            style:
                theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
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
                                  l10n.tabuMarcador,
                                  style: theme.textTheme.headlineSmall,
                                  glowColor: AppTheme.neonCyan,
                                  textAlign: TextAlign.center,
                                ),
                                const SizedBox(height: 8),
                                Text(
                                  l10n.tabuObjetivoVictorias(objetivo),
                                  textAlign: TextAlign.center,
                                  style: theme.textTheme.bodyMedium?.copyWith(
                                    color: theme.colorScheme.onSurfaceVariant,
                                  ),
                                ),
                                const SizedBox(height: 32),
                                _TeamScore(
                                  nombre: config.equipoA,
                                  victorias: victoriasA,
                                  objetivo: objetivo,
                                  color: AppTheme.neonCyan,
                                ),
                                const SizedBox(height: 16),
                                _TeamScore(
                                  nombre: config.equipoB,
                                  victorias: victoriasB,
                                  objetivo: objetivo,
                                  color: AppTheme.neonViolet,
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
                                              tabuFlowControllerProvider
                                                  .notifier,
                                            )
                                            .siguienteTurno();
                                        context.goNamed(kTabuTurnRouteName);
                                      },
                                      icon: const Icon(
                                        Icons.play_arrow_rounded,
                                      ),
                                      label: Text(l10n.tabuSiguienteTurno),
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
      ),
    );
  }
}

class _TeamScore extends StatelessWidget {
  const _TeamScore({
    required this.nombre,
    required this.victorias,
    required this.objetivo,
    required this.color,
  });

  final String nombre;
  final int victorias;
  final int objetivo;
  final Color color;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return NeonPanel(
      borderColor: color,
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
      child: Row(
        children: [
          Expanded(
            child: Text(
              nombre,
              style: theme.textTheme.titleMedium?.copyWith(
                color: AppTheme.textPrimary,
                fontWeight: FontWeight.w700,
              ),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
            ),
          ),
          const SizedBox(width: 8),
          NeonText(
            '$victorias / $objetivo',
            style: theme.textTheme.headlineSmall?.copyWith(
              fontWeight: FontWeight.w900,
            ),
            glowColor: color,
          ),
        ],
      ),
    );
  }
}

Future<bool> _confirmarSalida(
  BuildContext context,
  AppLocalizations l10n,
) async {
  final result = await showDialog<bool>(
    context: context,
    builder: (dialogContext) => AlertDialog(
      title: Text(l10n.salirDeLaPartidaTitulo),
      content: Text(l10n.salirDeLaPartidaMensaje),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(dialogContext).pop(false),
          child: Text(l10n.seguirJugando),
        ),
        FilledButton(
          onPressed: () => Navigator.of(dialogContext).pop(true),
          child: Text(l10n.salir),
        ),
      ],
    ),
  );
  return result ?? false;
}
