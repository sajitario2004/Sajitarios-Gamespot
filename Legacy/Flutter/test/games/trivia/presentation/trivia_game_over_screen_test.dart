/// Tests de widget para [TriviaGameOverScreen].
///
/// Verifica:
/// - Con supervivientes muestra "¡Habéis ganado!" y los nombres.
/// - Sin supervivientes muestra "Nadie ganó esta partida".
/// - Los botones de acción están presentes.
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_flow_controller.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_repositories_provider.dart';

import '../../../support/localized_app.dart';
import 'support/fake_question_repository.dart';
import 'support/fake_winner_repository.dart';

/// Construye un estado [TriviaFase.gameOver] cuya sesión tiene [survivors]
/// como jugadores vivos al llegar a la ronda 9 (todos los demás eliminados).
///
/// [allNames] son todos los jugadores; [survivors] son quienes sobrevivieron.
TriviaFlowState _gameOverState({
  required List<String> allNames,
  required List<String> survivors,
}) {
  final all = allNames.map(TriviaPlayer.new).toList();
  final survSet = survivors.map(TriviaPlayer.new).toSet();

  // Eliminamos todos los que no están en survSet.
  var session = TriviaSession.start(all);
  for (final p in all) {
    if (!survSet.contains(p)) {
      session = session.recordAnswer(p, correct: false);
    }
  }
  // Avanzar a la ronda 9 (isOver = true) si quedan supervivientes.
  // Si todos están eliminados, isOver ya es true desde la primera ronda.
  while (!session.isOver) {
    session = session.advanceRound();
  }

  return TriviaFlowState(fase: TriviaFase.gameOver, session: session);
}

Widget _harness(TriviaFlowState initialState) {
  return ProviderScope(
    overrides: [
      triviaFlowControllerProvider.overrideWith(
        () => _PreloadedController(initialState),
      ),
      winnerRepositoryProvider.overrideWith(
        (ref) => Future.value(FakeWinnerRepository()),
      ),
      questionRepositoryProvider.overrideWith(
        (ref) => Future.value(buildFakeQuestionRepo()),
      ),
    ],
    child: localizedApp(const TriviaGameOverScreen()),
  );
}

class _PreloadedController extends TriviaFlowController {
  _PreloadedController(this._initial);
  final TriviaFlowState _initial;

  @override
  TriviaFlowState build() => _initial;
}

void main() {
  group('TriviaGameOverScreen — con ganadores', () {
    testWidgets('muestra el mensaje de victoria', (tester) async {
      final state = _gameOverState(
        allNames: ['Ana', 'Luis', 'Marta'],
        survivors: ['Ana', 'Marta'],
      );
      await tester.pumpWidget(_harness(state));
      await tester.pump();

      expect(find.text('¡Habéis ganado!'), findsOneWidget);
    });

    testWidgets('muestra los nombres de los ganadores', (tester) async {
      final state = _gameOverState(
        allNames: ['Ana', 'Luis', 'Marta'],
        survivors: ['Ana', 'Marta'],
      );
      await tester.pumpWidget(_harness(state));
      await tester.pump();

      expect(find.textContaining('Ana'), findsWidgets);
      expect(find.textContaining('Marta'), findsWidgets);
    });

    testWidgets('muestra el botón "Jugar otra"', (tester) async {
      final state = _gameOverState(
        allNames: ['Ana', 'Luis'],
        survivors: ['Ana'],
      );
      await tester.pumpWidget(_harness(state));
      await tester.pump();

      expect(find.text('Jugar otra'), findsOneWidget);
    });
  });

  group('TriviaGameOverScreen — sin ganadores (todos eliminados)', () {
    testWidgets('muestra "Nadie ganó esta partida"', (tester) async {
      // Todos eliminados en ronda 0 → isOver = true, winners = [].
      final state = _gameOverState(allNames: ['Ana', 'Luis'], survivors: []);
      await tester.pumpWidget(_harness(state));
      await tester.pump();

      expect(find.text('Nadie ganó esta partida'), findsOneWidget);
    });

    testWidgets('no muestra el mensaje de victoria', (tester) async {
      final state = _gameOverState(allNames: ['Ana', 'Luis'], survivors: []);
      await tester.pumpWidget(_harness(state));
      await tester.pump();

      expect(find.text('¡Habéis ganado!'), findsNothing);
    });
  });

  group('TriviaGameOverScreen — acciones', () {
    testWidgets('"Volver al menú" está presente', (tester) async {
      final state = _gameOverState(
        allNames: ['Ana', 'Luis'],
        survivors: ['Ana'],
      );
      await tester.pumpWidget(_harness(state));
      await tester.pump();

      expect(find.text('Volver al menú'), findsOneWidget);
    });
  });
}
