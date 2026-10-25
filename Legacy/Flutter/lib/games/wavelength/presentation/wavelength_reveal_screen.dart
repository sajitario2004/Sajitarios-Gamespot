import 'package:flame/game.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/abandon_game_dialog.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_concept_labels.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_dial_game.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_flow_controller.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_routes.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla de revelación: muestra el objetivo en el dial, la puntuación de la
/// ronda y la acumulada. Permite avanzar a la siguiente ronda o al fin de
/// partida.
class WavelengthRevealScreen extends ConsumerStatefulWidget {
  const WavelengthRevealScreen({super.key});

  @override
  ConsumerState<WavelengthRevealScreen> createState() =>
      _WavelengthRevealScreenState();
}

class _WavelengthRevealScreenState
    extends ConsumerState<WavelengthRevealScreen> {
  WavelengthDialGame? _game;
  bool _advancing = false;

  WavelengthDialGame _buildGame(BuildContext context) {
    final theme = Theme.of(context);
    final cs = theme.colorScheme;
    final palette = WavelengthDialPalette(
      arcColor: AppTheme.neonCyan,
      needleColor: AppTheme.neonMagenta,
      bullseyeColor: AppTheme.neonCyan,
      nearColor: AppTheme.neonCyan,
      farColor: AppTheme.neonCyan,
      labelColor: cs.onSurface,
      glowColor: AppTheme.neonCyan,
    );
    return WavelengthDialGame(palette: palette);
  }

  Future<void> _avanzar() async {
    if (_advancing) return;
    setState(() => _advancing = true);
    await ref.read(wavelengthFlowControllerProvider.notifier).next();
    if (!mounted) return;
    final nextState = ref.read(wavelengthFlowControllerProvider);
    if (nextState.fase == WavelengthFase.gameOver) {
      // ignore: use_build_context_synchronously
      context.goNamed(kWavelengthGameOverRouteName);
    } else {
      // ignore: use_build_context_synchronously
      context.goNamed(kWavelengthClueRouteName);
    }
    setState(() => _advancing = false);
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    final state = ref.watch(wavelengthFlowControllerProvider);
    final round = state.currentRound;
    final session = state.session;

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

    // Configure game in reveal mode.
    _game ??= _buildGame(context);
    if (round != null) {
      _game!.setMode(WavelengthDialMode.reveal);
      _game!.setTarget(round.targetPosition);
      _game!.setConceptLabels(
        round.spectrum.leftConcept,
        round.spectrum.rightConcept,
      );
    }

    final roundNum = (session?.currentRoundIndex ?? 0) + 1;
    final totalRounds = session?.totalRondas ?? 1;
    final roundScore = round?.score ?? 0;
    final cumScore = session?.cumulativeScore ?? 0;
    final isLastRound = roundNum >= totalRounds;

    // Score color based on band.
    final scoreColor = _scoreColor(roundScore);

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
          automaticallyImplyLeading: false,
        ),
        body: NeonBackground(
          child: SafeArea(
            child: Center(
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 600),
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Text(
                        l10n.wavelengthRevealRondaXDeY(roundNum, totalRounds),
                        textAlign: TextAlign.center,
                        style: theme.textTheme.titleMedium?.copyWith(
                          color: theme.colorScheme.onSurfaceVariant,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      const SizedBox(height: 4),
                      // Concept labels — Flutter widgets so they never clip.
                      if (round != null)
                        WavelengthConceptLabelsRow(
                          left: round.spectrum.leftConcept,
                          right: round.spectrum.rightConcept,
                        ),
                      const SizedBox(height: 4),
                      // Dial in reveal mode.
                      Expanded(
                        child: Semantics(
                          label: 'Dial de Wavelength - revelando objetivo',
                          excludeSemantics: true,
                          child: ConstrainedBox(
                            constraints: const BoxConstraints(minHeight: 140),
                            child: GameWidget(game: _game!),
                          ),
                        ),
                      ),
                      const SizedBox(height: 8),
                      // Score panel.
                      Semantics(
                        label: l10n.wavelengthRevealScoreSemantica(
                          _bandLabel(roundScore, l10n),
                          roundScore,
                        ),
                        child: NeonPanel(
                          borderColor: scoreColor,
                          borderWidth: 2.0,
                          padding: const EdgeInsets.symmetric(
                            horizontal: 20,
                            vertical: 14,
                          ),
                          child: Column(
                            children: [
                              Text(
                                _bandLabel(roundScore, l10n),
                                style: theme.textTheme.titleMedium?.copyWith(
                                  color: scoreColor,
                                  fontWeight: FontWeight.w800,
                                ),
                                textAlign: TextAlign.center,
                              ),
                              const SizedBox(height: 4),
                              NeonText(
                                l10n.wavelengthRevealPuntos(roundScore),
                                glowColor: scoreColor,
                                style: theme.textTheme.headlineSmall?.copyWith(
                                  color: AppTheme.textPrimary,
                                  fontWeight: FontWeight.w900,
                                ),
                                textAlign: TextAlign.center,
                              ),
                              const SizedBox(height: 4),
                              Text(
                                l10n.wavelengthRevealTotalPuntos(cumScore),
                                style: theme.textTheme.bodyMedium?.copyWith(
                                  color: theme.colorScheme.onSurfaceVariant,
                                ),
                                textAlign: TextAlign.center,
                              ),
                            ],
                          ),
                        ),
                      ),
                      const SizedBox(height: 16),
                      NeonGlowWrapper(
                        color: AppTheme.neonCyan,
                        borderRadius: const BorderRadius.all(
                          Radius.circular(28),
                        ),
                        child: FilledButton.icon(
                          onPressed: _advancing ? null : _avanzar,
                          icon: _advancing
                              ? const SizedBox(
                                  width: 20,
                                  height: 20,
                                  child: CircularProgressIndicator(
                                    strokeWidth: 2.5,
                                  ),
                                )
                              : Icon(
                                  isLastRound
                                      ? Icons.emoji_events_outlined
                                      : Icons.arrow_forward_rounded,
                                ),
                          label: Text(
                            isLastRound
                                ? l10n.wavelengthVerResultado
                                : l10n.wavelengthSiguienteRonda,
                          ),
                          style: FilledButton.styleFrom(
                            minimumSize: const Size.fromHeight(56),
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

  Color _scoreColor(int score) {
    switch (score) {
      case kPointsBullseye:
        return AppTheme.neonCyan;
      case kPointsNear:
        return AppTheme.neonCyan;
      case kPointsFar:
        return AppTheme.neonMagenta;
      default:
        return AppTheme.neonMagenta;
    }
  }

  String _bandLabel(int score, AppLocalizations l10n) {
    switch (score) {
      case kPointsBullseye:
        return l10n.wavelengthBandBlanco;
      case kPointsNear:
        return l10n.wavelengthBandCerca;
      case kPointsFar:
        return l10n.wavelengthBandLejos;
      default:
        return l10n.wavelengthBandFallo;
    }
  }
}
