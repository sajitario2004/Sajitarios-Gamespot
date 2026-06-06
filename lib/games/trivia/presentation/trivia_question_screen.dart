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

/// Letras de las opciones de respuesta (A-D).
const List<String> _kOptionLetters = ['A', 'B', 'C', 'D'];

/// Pantalla de pregunta activa en el flujo de Trivia.
///
/// Muestra el enunciado en una caja neón violeta ([AppTheme.neonViolet]) y las
/// cuatro opciones en un grid 2x2, cada una con el marco de su color neón
/// correspondiente ([AppTheme.answerFrameColors]). Al pulsar una opción llama
/// a `triviaFlowControllerProvider.notifier.responder(index)` y navega al
/// siguiente paso (pass o game-over según el resultado).
class TriviaQuestionScreen extends ConsumerStatefulWidget {
  const TriviaQuestionScreen({super.key});

  @override
  ConsumerState<TriviaQuestionScreen> createState() =>
      _TriviaQuestionScreenState();
}

class _TriviaQuestionScreenState extends ConsumerState<TriviaQuestionScreen> {
  /// Índice de la opción seleccionada, o -1 si aún no se respondió.
  int _selectedIndex = -1;

  /// Evita doble-tap durante la transición de respuesta.
  bool _respondiendo = false;

  Future<void> _responder(int index) async {
    if (_respondiendo) return;
    setState(() {
      _selectedIndex = index;
      _respondiendo = true;
    });

    // Pausa breve para que el jugador vea el feedback visual antes de avanzar.
    await Future<void>.delayed(const Duration(milliseconds: 800));

    if (!mounted) return;

    await ref.read(triviaFlowControllerProvider.notifier).responder(index);

    if (!mounted) return;

    final fase = ref.read(triviaFlowControllerProvider).fase;
    switch (fase) {
      case TriviaFase.pass:
        context.goNamed(kTriviaPassRouteName);
      case TriviaFase.gameOver:
        context.goNamed(kTriviaGameOverRouteName);
      default:
        break;
    }
  }

  Future<void> _confirmarSalida() async {
    final salir = await abandonarPartidaDialog(context);
    if (salir != true || !mounted) return;
    ref.read(triviaFlowControllerProvider.notifier).reiniciar();
    if (!mounted) return;
    context.goNamed(kTriviaSetupRouteName);
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    final state = ref.watch(triviaFlowControllerProvider);
    final question = state.currentQuestion;
    final player = state.currentPlayer;

    // Pantalla de guardia: si no hay pregunta activa, mostramos fallback.
    if (question == null || player == null) {
      return Scaffold(
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
          child: Center(
            child: Text(
              l10n.triviaNoHayPartida,
              style: theme.textTheme.bodyLarge,
            ),
          ),
        ),
      );
    }

    final round = state.currentRound + 1;
    final totalRounds = 9;

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) {
        if (didPop) return;
        _confirmarSalida();
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
          bottom: PreferredSize(
            preferredSize: const Size.fromHeight(24),
            child: Padding(
              padding: const EdgeInsets.only(bottom: 6),
              child: Text(
                l10n.triviaRondaXDeY(round, totalRounds),
                style: theme.textTheme.bodySmall?.copyWith(
                  color: AppTheme.textMuted,
                ),
              ),
            ),
          ),
        ),
        body: NeonBackground(
          child: SafeArea(
            child: LayoutBuilder(
              builder: (context, constraints) {
                return SingleChildScrollView(
                  padding: const EdgeInsets.fromLTRB(16, 16, 16, 24),
                  child: ConstrainedBox(
                    constraints: BoxConstraints(
                      minHeight: constraints.maxHeight,
                      maxWidth: 640,
                    ),
                    child: Center(
                      child: ConstrainedBox(
                        constraints: const BoxConstraints(maxWidth: 640),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.stretch,
                          children: [
                            // ── Nombre del jugador ─────────────────────────
                            NeonText(
                              player.name,
                              textAlign: TextAlign.center,
                              glowColor: AppTheme.neonViolet,
                              style: theme.textTheme.titleMedium?.copyWith(
                                color: AppTheme.neonVioletText,
                                fontWeight: FontWeight.w700,
                              ),
                            ),
                            const SizedBox(height: 12),

                            // ── Caja de pregunta (neón violeta) ────────────
                            Semantics(
                              label:
                                  '${l10n.triviaPregunta}: ${question.enunciado}',
                              child: NeonPanel(
                                borderColor: AppTheme.neonViolet,
                                backgroundColor: AppTheme.surface,
                                intensity: 0.9,
                                borderWidth: 2.0,
                                padding: const EdgeInsets.all(20),
                                child: Text(
                                  question.enunciado,
                                  style: theme.textTheme.titleMedium?.copyWith(
                                    color: AppTheme.textPrimary,
                                    fontWeight: FontWeight.w700,
                                    height: 1.4,
                                  ),
                                  textAlign: TextAlign.center,
                                ),
                              ),
                            ),
                            const SizedBox(height: 20),

                            // ── Grid 2x2 de opciones ────────────────────────
                            _AnswerGrid(
                              options: question.options,
                              correctIndex: question.correctIndex,
                              selectedIndex: _selectedIndex,
                              onTap: _respondiendo ? null : _responder,
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
    );
  }
}

/// Grid 2x2 con las cuatro opciones de respuesta.
///
/// Cada celda usa [AppTheme.answerFrameColors] en orden para su borde neón.
/// Tras elegir, muestra feedback visual: verde brillante si correcta, rojo si
/// incorrecta.
class _AnswerGrid extends StatelessWidget {
  const _AnswerGrid({
    required this.options,
    required this.correctIndex,
    required this.selectedIndex,
    required this.onTap,
  });

  final List<String> options;
  final int correctIndex;

  /// Índice seleccionado por el jugador (-1 = ninguno).
  final int selectedIndex;

  /// Callback al pulsar una opción. `null` mientras se espera la respuesta.
  final void Function(int index)? onTap;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final textScale = MediaQuery.textScalerOf(context).scale(1).clamp(1.0, 2.0);
    final childAspectRatio = (1.3 / textScale).clamp(0.7, 1.3);
    return GridView.count(
      crossAxisCount: 2,
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      crossAxisSpacing: 12,
      mainAxisSpacing: 12,
      childAspectRatio: childAspectRatio,
      children: [
        for (var i = 0; i < options.length && i < 4; i++)
          _AnswerCell(
            letter: _kOptionLetters[i],
            text: options[i],
            frameColor: AppTheme.answerFrameColors[i],
            state: _cellState(i),
            semanticLabel: l10n.triviaOpcionLetra(
              _kOptionLetters[i],
              options[i],
            ),
            onTap: onTap == null ? null : () => onTap!(i),
          ),
      ],
    );
  }

  _CellState _cellState(int index) {
    if (selectedIndex < 0) return _CellState.idle;
    if (index == correctIndex) return _CellState.correct;
    if (index == selectedIndex) return _CellState.wrong;
    return _CellState.dimmed;
  }
}

enum _CellState { idle, correct, wrong, dimmed }

/// Una opción de respuesta individual con marco neón de color.
class _AnswerCell extends StatelessWidget {
  const _AnswerCell({
    required this.letter,
    required this.text,
    required this.frameColor,
    required this.state,
    required this.semanticLabel,
    required this.onTap,
  });

  final String letter;
  final String text;
  final Color frameColor;
  final _CellState state;
  final String semanticLabel;
  final VoidCallback? onTap;

  Color _resolvedBorderColor() {
    return switch (state) {
      _CellState.correct => AppTheme.neonGreen,
      _CellState.wrong => AppTheme.neonRed,
      _CellState.dimmed => frameColor.withValues(alpha: 0.25),
      _CellState.idle => frameColor,
    };
  }

  Color _resolvedBackground() {
    return switch (state) {
      _CellState.correct => AppTheme.neonGreen.withValues(alpha: 0.18),
      _CellState.wrong => AppTheme.neonRed.withValues(alpha: 0.18),
      _CellState.dimmed => AppTheme.surface.withValues(alpha: 0.5),
      _CellState.idle => AppTheme.surface,
    };
  }

  double _resolvedGlowIntensity() {
    return switch (state) {
      _CellState.correct || _CellState.wrong => 1.2,
      _CellState.dimmed => 0.0,
      _CellState.idle => 0.7,
    };
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final borderColor = _resolvedBorderColor();
    final bgColor = _resolvedBackground();
    final glowIntensity = _resolvedGlowIntensity();

    return Semantics(
      label: semanticLabel,
      button: true,
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(14),
          child: AnimatedContainer(
            duration: const Duration(milliseconds: 250),
            decoration: BoxDecoration(
              color: bgColor,
              borderRadius: BorderRadius.circular(14),
              border: Border.all(color: borderColor, width: 2.0),
              boxShadow: glowIntensity > 0
                  ? neonGlow(color: borderColor, intensity: glowIntensity)
                  : null,
            ),
            child: ConstrainedBox(
              constraints: const BoxConstraints(minHeight: 56),
              child: Padding(
                padding: const EdgeInsets.symmetric(
                  horizontal: 10,
                  vertical: 8,
                ),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    // Letra de la opción (A/B/C/D)
                    Text(
                      letter,
                      style: theme.textTheme.labelLarge?.copyWith(
                        color: borderColor,
                        fontWeight: FontWeight.w900,
                        fontSize: 13,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Flexible(
                      child: Text(
                        text,
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: state == _CellState.dimmed
                              ? AppTheme.textMuted
                              : AppTheme.textPrimary,
                          fontWeight: FontWeight.w600,
                          height: 1.3,
                        ),
                        textAlign: TextAlign.center,
                        maxLines: 3,
                        overflow: TextOverflow.ellipsis,
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
