import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_flow_controller.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_routes.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla de fin de partida de Trivia.
///
/// Muestra el resultado SIN revelar roles: los supervivientes que empataron
/// ("¡Habéis ganado!") o un mensaje de "nadie ganó" si todos fueron
/// eliminados. Las victorias ya fueron persistidas por el controlador en
/// [TriviaFlowController._goGameOver].
///
/// "Volver al menú" llama a `reiniciar()` antes de salir — igual que el
/// fix análogo en el Impostor — para dejar el flujo limpio.
class TriviaGameOverScreen extends ConsumerWidget {
  const TriviaGameOverScreen({super.key});

  static const double _kMaxContentWidth = 560.0;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    final state = ref.watch(triviaFlowControllerProvider);

    final winners = state.winners;
    final hayGanadores = winners.isNotEmpty;

    final accent = hayGanadores ? AppTheme.neonCyan : AppTheme.neonMagenta;
    final icono = hayGanadores
        ? Icons.emoji_events_outlined
        : Icons.sentiment_dissatisfied_outlined;
    final mensaje = hayGanadores ? l10n.triviaGanadores : l10n.triviaNadieGano;

    void volverAlMenu() {
      ref.read(triviaFlowControllerProvider.notifier).reiniciar();
      context.goNamed('menu');
    }

    void jugarOtra() {
      ref.read(triviaFlowControllerProvider.notifier).reiniciar();
      context.goNamed(kTriviaSetupRouteName);
    }

    return Scaffold(
      appBar: AppBar(
        leading: VolverAlMenuButton(
          onPressed: () {
            ref.read(triviaFlowControllerProvider.notifier).reiniciar();
            context.goNamed('menu');
          },
        ),
        title: NeonText(
          l10n.triviaFinDePartida,
          style: theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
          glowColor: AppTheme.neonViolet,
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
                            borderColor: accent,
                            borderWidth: 2.0,
                            padding: const EdgeInsets.all(28),
                            child: Column(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Icon(
                                  icono,
                                  size: 72,
                                  color: accent,
                                  shadows: neonTextShadows(color: accent),
                                ),
                                const SizedBox(height: 20),
                                NeonText(
                                  mensaje,
                                  glowColor: accent,
                                  style: theme.textTheme.headlineSmall
                                      ?.copyWith(
                                        color: AppTheme.textPrimary,
                                        fontWeight: FontWeight.w900,
                                      ),
                                  textAlign: TextAlign.center,
                                ),
                                if (hayGanadores) ...[
                                  const SizedBox(height: 16),
                                  Text(
                                    l10n.triviaGanadoresList(
                                      winners.map((p) => p.name).join(', '),
                                    ),
                                    style: theme.textTheme.bodyLarge?.copyWith(
                                      color: AppTheme.textPrimary,
                                    ),
                                    textAlign: TextAlign.center,
                                    maxLines: 4,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                ],
                              ],
                            ),
                          ),
                        ),
                      ),
                    ),
                    _Acciones(
                      onVolverAlMenu: volverAlMenu,
                      onJugarOtra: jugarOtra,
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

class _Acciones extends StatelessWidget {
  const _Acciones({required this.onVolverAlMenu, required this.onJugarOtra});

  final VoidCallback onVolverAlMenu;
  final VoidCallback onJugarOtra;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Padding(
      padding: const EdgeInsets.fromLTRB(0, 16, 0, 0),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          SizedBox(
            width: double.infinity,
            child: NeonGlowWrapper(
              color: AppTheme.neonViolet,
              borderRadius: const BorderRadius.all(Radius.circular(24)),
              child: FilledButton.icon(
                onPressed: onJugarOtra,
                icon: const Icon(Icons.replay),
                label: Text(l10n.triviaJugarOtra),
              ),
            ),
          ),
          const SizedBox(height: 8),
          SizedBox(
            width: double.infinity,
            child: OutlinedButton.icon(
              onPressed: onVolverAlMenu,
              icon: const Icon(Icons.home_outlined),
              label: Text(l10n.volverAlMenu),
            ),
          ),
        ],
      ),
    );
  }
}
