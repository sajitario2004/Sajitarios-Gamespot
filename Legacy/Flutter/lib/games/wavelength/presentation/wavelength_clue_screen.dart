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

/// Pantalla del psíquico: ve el dial con bandas+objetivo y escribe su pista.
class WavelengthClueScreen extends ConsumerStatefulWidget {
  const WavelengthClueScreen({super.key});

  @override
  ConsumerState<WavelengthClueScreen> createState() =>
      _WavelengthClueScreenState();
}

class _WavelengthClueScreenState extends ConsumerState<WavelengthClueScreen> {
  WavelengthDialGame? _game;
  final TextEditingController _clueController = TextEditingController();

  @override
  void dispose() {
    _clueController.dispose();
    super.dispose();
  }

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
    final game = WavelengthDialGame(palette: palette);
    return game;
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

    void confirmar() {
      final pista = _clueController.text;
      if (pista.trim().isEmpty) return;
      ref.read(wavelengthFlowControllerProvider.notifier).confirmarPista(pista);
      context.goNamed(kWavelengthPassRouteName);
    }

    // Build or configure the game.
    _game ??= _buildGame(context);
    if (round != null) {
      _game!.setMode(WavelengthDialMode.clue);
      _game!.setTarget(round.targetPosition);
      _game!.setConceptLabels(
        round.spectrum.leftConcept,
        round.spectrum.rightConcept,
      );
    }

    final roundNum = (session?.currentRoundIndex ?? 0) + 1;
    final totalRounds = session?.totalRondas ?? 1;

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
                              // Ronda indicator
                              Text(
                                l10n.wavelengthRondaXDeY(roundNum, totalRounds),
                                textAlign: TextAlign.center,
                                style: theme.textTheme.titleMedium?.copyWith(
                                  color: theme.colorScheme.onSurfaceVariant,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                              const SizedBox(height: 4),
                              // Psíquico label
                              if (state.currentPsychic != null) ...[
                                Row(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  children: [
                                    Flexible(
                                      child: Text(
                                        '${l10n.wavelengthCluePsicoEtiqueta}: ',
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
                                    Flexible(
                                      child: NeonText(
                                        state.currentPsychic!,
                                        glowColor: AppTheme.neonCyan,
                                        style: theme.textTheme.bodyMedium
                                            ?.copyWith(
                                              fontWeight: FontWeight.w800,
                                            ),
                                        maxLines: 1,
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                    ),
                                  ],
                                ),
                                const SizedBox(height: 4),
                              ],
                              // Role instruction — explicit, prominent.
                              NeonPanel(
                                borderColor: AppTheme.neonCyan,
                                intensity: 0.6,
                                padding: const EdgeInsets.symmetric(
                                  horizontal: 14,
                                  vertical: 10,
                                ),
                                child: Text(
                                  l10n.wavelengthClueScreenInstruccion,
                                  textAlign: TextAlign.center,
                                  style: theme.textTheme.bodyMedium?.copyWith(
                                    color: theme.colorScheme.onSurface,
                                    fontWeight: FontWeight.w600,
                                  ),
                                ),
                              ),
                              const SizedBox(height: 8),
                              // Concept labels — rendered as Flutter widgets so they
                              // never get clipped regardless of screen width.
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
                                  label: l10n.wavelengthDialSemanticsClue,
                                  excludeSemantics: true,
                                  child: GameWidget(game: _game!),
                                ),
                              ),
                              const SizedBox(height: 8),
                              // Clue field
                              TextField(
                                controller: _clueController,
                                textCapitalization:
                                    TextCapitalization.sentences,
                                textInputAction: TextInputAction.done,
                                onSubmitted: (_) => confirmar(),
                                decoration: InputDecoration(
                                  labelText: l10n.wavelengthClueFieldLabel,
                                  hintText: l10n.wavelengthClueFieldHint,
                                ),
                              ),
                              const SizedBox(height: 16),
                              NeonGlowWrapper(
                                color: AppTheme.neonCyan,
                                borderRadius: const BorderRadius.all(
                                  Radius.circular(28),
                                ),
                                child: FilledButton.icon(
                                  onPressed: confirmar,
                                  icon: const Icon(Icons.send_rounded),
                                  label: Text(l10n.wavelengthConfirmarPista),
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
