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

/// Pantalla de adivinanza: el grupo mueve el dial sin ver el objetivo.
class WavelengthGuessScreen extends ConsumerStatefulWidget {
  const WavelengthGuessScreen({super.key});

  @override
  ConsumerState<WavelengthGuessScreen> createState() =>
      _WavelengthGuessScreenState();
}

class _WavelengthGuessScreenState extends ConsumerState<WavelengthGuessScreen> {
  WavelengthDialGame? _game;

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

    void confirmar() {
      final pos = _game?.needlePosition ?? 0.5;
      ref.read(wavelengthFlowControllerProvider.notifier).submitGuess(pos);
      context.goNamed(kWavelengthRevealRouteName);
    }

    _game ??= _buildGame(context);
    if (round != null) {
      _game!.setMode(WavelengthDialMode.guess);
      _game!.setTarget(round.targetPosition);
      _game!.setConceptLabels(
        round.spectrum.leftConcept,
        round.spectrum.rightConcept,
      );
    }

    final roundNum = (session?.currentRoundIndex ?? 0) + 1;
    final totalRounds = session?.totalRondas ?? 1;
    final clue = round?.clue ?? '';

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) {
        if (didPop) return;
        confirmarSalida();
      },
      child: Scaffold(
        appBar: AppBar(
          title: NeonText(
            l10n.wavelengthTitulo,
            style:
                theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
            glowColor: AppTheme.neonCyan,
          ),
        ),
        body: NeonBackground(
          child: SafeArea(
            child: Center(
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 600),
                child: LayoutBuilder(
                  builder: (context, constraints) {
                    final dialHeight = (constraints.maxHeight * 0.42).clamp(
                      180.0,
                      320.0,
                    );
                    return SingleChildScrollView(
                      child: ConstrainedBox(
                        constraints: BoxConstraints(
                          minHeight: constraints.maxHeight,
                        ),
                        child: Padding(
                          padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              Text(
                                l10n.wavelengthRondaXDeY(roundNum, totalRounds),
                                textAlign: TextAlign.center,
                                style: theme.textTheme.titleMedium?.copyWith(
                                  color: theme.colorScheme.onSurfaceVariant,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                              const SizedBox(height: 4),
                              // Pista del psíquico
                              if (clue.isNotEmpty)
                                NeonPanel(
                                  borderColor: AppTheme.neonCyan,
                                  intensity: 0.5,
                                  padding: const EdgeInsets.symmetric(
                                    horizontal: 16,
                                    vertical: 10,
                                  ),
                                  child: Row(
                                    children: [
                                      Flexible(
                                        child: Text(
                                          l10n.wavelengthPistaEtiqueta,
                                          style: theme.textTheme.bodyMedium
                                              ?.copyWith(
                                                color: theme
                                                    .colorScheme
                                                    .onSurfaceVariant,
                                              ),
                                          maxLines: 1,
                                          overflow: TextOverflow.ellipsis,
                                        ),
                                      ),
                                      const SizedBox(width: 8),
                                      Expanded(
                                        child: NeonText(
                                          clue,
                                          glowColor: AppTheme.neonCyan,
                                          style: theme.textTheme.bodyLarge
                                              ?.copyWith(
                                                fontWeight: FontWeight.w800,
                                              ),
                                          overflow: TextOverflow.ellipsis,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              const SizedBox(height: 4),
                              // Instruction — explicit group role.
                              Text(
                                l10n.wavelengthGuessInstruccion,
                                textAlign: TextAlign.center,
                                style: theme.textTheme.bodyMedium?.copyWith(
                                  color: theme.colorScheme.onSurface,
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
                              // Dial — fixed responsive height so the page can scroll at
                              // large text scales instead of overflowing. The needle is
                              // settable by tap, so scrolling never blocks input.
                              SizedBox(
                                height: dialHeight,
                                child: Semantics(
                                  label: l10n.wavelengthDialSemanticsGuess,
                                  excludeSemantics: true,
                                  child: GameWidget(game: _game!),
                                ),
                              ),
                              const SizedBox(height: 8),
                              NeonGlowWrapper(
                                color: AppTheme.neonCyan,
                                borderRadius: const BorderRadius.all(
                                  Radius.circular(28),
                                ),
                                child: FilledButton.icon(
                                  onPressed: confirmar,
                                  icon: const Icon(Icons.check_circle_outline),
                                  label: Text(
                                    l10n.wavelengthConfirmarAdivinanza,
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
