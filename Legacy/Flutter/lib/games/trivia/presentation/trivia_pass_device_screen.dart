import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/abandon_game_dialog.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_flow_controller.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_routes.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla intermedia "Pásale el móvil a {jugador}" de Trivia.
///
/// Aparece antes de cada pregunta, mostrando de forma grande y clara a quién
/// le toca coger el móvil. Al continuar, llama a
/// `triviaFlowControllerProvider.notifier.pasarDispositivo()` y navega a la
/// ruta `trivia-question` para que ese jugador vea su pregunta.
class TriviaPassDeviceScreen extends ConsumerWidget {
  const TriviaPassDeviceScreen({super.key});

  static const double _largeScreenMinWidth = 600.0;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final l10n = AppLocalizations.of(context)!;

    final state = ref.watch(triviaFlowControllerProvider);
    final player = state.currentPlayer;
    final nombre = player?.name ?? '';
    final posicion = state.alivePlayers.isEmpty
        ? 0
        : state.currentPlayerIndex + 1;
    final total = state.alivePlayers.length;

    void continuar() {
      ref.read(triviaFlowControllerProvider.notifier).pasarDispositivo();
      context.goNamed(kTriviaQuestionRouteName);
    }

    Future<void> confirmarSalida() async {
      final salir = await abandonarPartidaDialog(context);
      if (salir != true || !context.mounted) return;
      ref.read(triviaFlowControllerProvider.notifier).reiniciar();
      if (!context.mounted) return;
      context.goNamed(kTriviaSetupRouteName);
    }

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) {
        if (didPop) return;
        confirmarSalida();
      },
      child: Scaffold(
        appBar: AppBar(
          leading: VolverAlMenuButton(
            onPressed: () async {
              final salir = await abandonarPartidaDialog(context);
              if (salir != true || !context.mounted) return;
              ref.read(triviaFlowControllerProvider.notifier).reiniciar();
              if (!context.mounted) return;
              context.go('/');
            },
          ),
          title: NeonText(
            l10n.triviaTitulo,
            style:
                theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
            glowColor: AppTheme.neonViolet,
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
                                Icon(
                                  Icons.phone_iphone,
                                  size: esGrande ? 96 : 72,
                                  color: AppTheme.neonViolet,
                                ),
                                const SizedBox(height: 24),
                                Text(
                                  l10n.triviaPasaleElMovilA,
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
                                  glowColor: AppTheme.neonViolet,
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
                                  l10n.triviaPasaleElMovilAyuda,
                                  textAlign: TextAlign.center,
                                  style: theme.textTheme.bodyLarge?.copyWith(
                                    color: colorScheme.onSurfaceVariant,
                                  ),
                                ),
                                const SizedBox(height: 40),
                                NeonGlowWrapper(
                                  color: AppTheme.neonViolet,
                                  borderRadius: const BorderRadius.all(
                                    Radius.circular(28),
                                  ),
                                  child: FilledButton.icon(
                                    onPressed: player == null
                                        ? null
                                        : continuar,
                                    icon: const Icon(Icons.quiz),
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
