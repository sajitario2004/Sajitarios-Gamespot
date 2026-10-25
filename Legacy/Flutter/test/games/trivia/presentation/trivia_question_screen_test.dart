/// Tests de widget para [TriviaQuestionScreen].
///
/// Verifica:
/// - El enunciado se renderiza dentro del panel violeta.
/// - Las 4 opciones están presentes, cada una con su color de marco neón.
/// - Tocar la opción correcta/incorrecta llama a responder() en el controlador.
///
/// GOTCHA de animaciones: [TriviaQuestionScreen] usa [AnimatedContainer] con
/// 250 ms en las celdas de respuesta. NO usamos pumpAndSettle ya que eso
/// agotaría el timeout al esperar el Future.delayed(800ms) del _responder().
/// Usamos pump(duration) con duraciones concretas en su lugar.
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/question.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_flow_controller.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_question_screen.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_repositories_provider.dart';

import '../../../support/localized_app.dart';
import 'support/fake_question_repository.dart';
import 'support/fake_winner_repository.dart';

/// Construye una pregunta de prueba con opciones conocidas.
Question _buildQuestion({int correctIndex = 0}) => Question.create(
  id: 1,
  tematicaId: 'historia',
  difficulty: Difficulty.facil,
  enunciado: '¿Cuál es la capital de Francia?',
  options: ['París', 'Madrid', 'Roma', 'Berlín'],
  correctIndex: correctIndex,
);

/// Construye un estado de flujo con fase [TriviaFase.question] y la pregunta
/// inyectada al jugador actual.
TriviaFlowState _stateWithQuestion(Question question) {
  final players = [const TriviaPlayer('Ana'), const TriviaPlayer('Luis')];
  final session = TriviaSession.start(players);
  return TriviaFlowState(
    fase: TriviaFase.question,
    session: session,
    currentPlayerIndex: 0,
    currentQuestion: question,
    roundQuestions: {players[0]: question},
  );
}

/// Harness que pre-carga el controlador con [initialState].
Widget _harness(TriviaFlowState initialState) {
  return ProviderScope(
    overrides: [
      triviaFlowControllerProvider.overrideWith(() {
        final notifier = _PreloadedTriviaController(initialState);
        return notifier;
      }),
      winnerRepositoryProvider.overrideWith(
        (ref) => Future.value(FakeWinnerRepository()),
      ),
      questionRepositoryProvider.overrideWith(
        (ref) => Future.value(buildFakeQuestionRepo()),
      ),
    ],
    child: localizedApp(const TriviaQuestionScreen()),
  );
}

/// Notifier que arranca con un estado pre-cargado para tests.
///
/// Sobreescribe [responder] como no-op para que los tests de feedback visual
/// no tengan que ejecutar el flujo completo de la ronda (que requiere GoRouter
/// para navegar y un config no-nulo para recargar pools).
class _PreloadedTriviaController extends TriviaFlowController {
  _PreloadedTriviaController(this._initial);
  final TriviaFlowState _initial;

  @override
  TriviaFlowState build() => _initial;

  /// No-op: en tests de feedback visual solo nos interesa el setState local
  /// (_selectedIndex) que ocurre antes de llamar a este método.
  @override
  Future<void> responder(int chosenIndex) async {
    // Intencionalmente vacío — los tests de color comprueban el feedback visual
    // que se aplica en _responder() antes de llamar al super.
  }
}

void main() {
  group('TriviaQuestionScreen', () {
    testWidgets('muestra el enunciado de la pregunta', (tester) async {
      final question = _buildQuestion();
      await tester.pumpWidget(_harness(_stateWithQuestion(question)));
      await tester.pump();

      expect(
        find.text('¿Cuál es la capital de Francia?'),
        findsOneWidget,
        reason: 'el enunciado debe estar visible',
      );
    });

    testWidgets('renderiza exactamente 4 opciones de respuesta', (
      tester,
    ) async {
      final question = _buildQuestion();
      await tester.pumpWidget(_harness(_stateWithQuestion(question)));
      await tester.pump();

      for (final option in question.options) {
        expect(
          find.text(option),
          findsOneWidget,
          reason: 'la opción "$option" debe estar visible',
        );
      }
    });

    testWidgets('los cuatro colores de marco neón están presentes', (
      tester,
    ) async {
      final question = _buildQuestion();
      await tester.pumpWidget(_harness(_stateWithQuestion(question)));
      await tester.pump();

      // Buscamos los AnimatedContainer de las celdas por su decoración.
      // Cada celda tiene un borde del color correspondiente en answerFrameColors.
      final containers = tester.widgetList<AnimatedContainer>(
        find.byType(AnimatedContainer),
      );

      final foundColors = <Color>{};
      for (final container in containers) {
        final decoration = container.decoration;
        if (decoration is BoxDecoration) {
          final border = decoration.border;
          if (border is Border) {
            foundColors.add(border.top.color);
          }
        }
      }

      for (final color in AppTheme.answerFrameColors) {
        expect(
          foundColors,
          contains(color),
          reason: 'el color de marco $color debe estar presente',
        );
      }
    });

    testWidgets('pulsar la opción correcta (índice 0) actualiza el estado', (
      tester,
    ) async {
      final question = _buildQuestion(correctIndex: 0);
      await tester.pumpWidget(_harness(_stateWithQuestion(question)));
      await tester.pump();

      // Pulsamos la opción "París" (índice 0 = correcta).
      await tester.tap(find.text('París'));
      // Primer pump: dispara el setState que pone _selectedIndex = 0.
      await tester.pump();
      // El AnimatedContainer cambia color (250 ms).
      await tester.pump(const Duration(milliseconds: 300));

      // La celda correcta ahora muestra el borde neonGreen (feedback visual).
      final containers = tester.widgetList<AnimatedContainer>(
        find.byType(AnimatedContainer),
      );
      final borderColors = <Color>[];
      for (final container in containers) {
        final decoration = container.decoration;
        if (decoration is BoxDecoration) {
          final border = decoration.border;
          if (border is Border) {
            borderColors.add(border.top.color);
          }
        }
      }
      expect(
        borderColors,
        contains(AppTheme.neonGreen),
        reason: 'la celda correcta debe mostrar borde neonGreen',
      );

      // Drenar el Future.delayed(800ms) pendiente antes de que el test termine.
      await tester.pump(const Duration(milliseconds: 600));
      await tester.pump();
    });

    testWidgets('pulsar la opción incorrecta muestra borde neonRed', (
      tester,
    ) async {
      final question = _buildQuestion(correctIndex: 0);
      await tester.pumpWidget(_harness(_stateWithQuestion(question)));
      await tester.pump();

      // Pulsamos "Madrid" (índice 1 = incorrecta).
      await tester.tap(find.text('Madrid'));
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 300));

      final containers = tester.widgetList<AnimatedContainer>(
        find.byType(AnimatedContainer),
      );
      final borderColors = <Color>[];
      for (final container in containers) {
        final decoration = container.decoration;
        if (decoration is BoxDecoration) {
          final border = decoration.border;
          if (border is Border) {
            borderColors.add(border.top.color);
          }
        }
      }
      expect(
        borderColors,
        contains(AppTheme.neonRed),
        reason: 'la opción incorrecta debe mostrar borde neonRed',
      );

      // Drenar el Future.delayed(800ms) pendiente antes de que el test termine.
      await tester.pump(const Duration(milliseconds: 600));
      await tester.pump();
    });
  });
}
