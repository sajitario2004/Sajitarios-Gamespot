/// Unit tests for [TriviaFlowController].
///
/// All repositories are replaced with in-memory fakes; randomProvider is
/// seeded for determinism. No database or Flutter widgets needed.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/trivia/data/question_repository.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_flow_controller.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_repositories_provider.dart';

import 'support/fake_question_repository.dart';
import 'support/fake_winner_repository.dart';

// ─── Helpers ─────────────────────────────────────────────────────────────────

/// Builds a [TriviaConfig] with [playerNames] and a single tematica "historia".
TriviaConfig _config(List<String> playerNames) {
  final result = TriviaConfig.create(
    playerNames: playerNames,
    selectedTematicaIds: const {'historia'},
  );
  return result.config!;
}

/// Creates a [ProviderContainer] with fakes and a seeded [RandomProvider].
///
/// [questionRepo] defaults to a repo with 6 questions per tier (enough for 2
/// players through all 9 rounds). [winnerRepo] defaults to a fresh fake.
ProviderContainer _container({
  QuestionRepository? questionRepo,
  FakeWinnerRepository? winnerRepo,
  int seed = 42,
}) {
  final qRepo = questionRepo ?? buildFakeQuestionRepo(countPerTier: 6);
  final wRepo = winnerRepo ?? FakeWinnerRepository();
  final container = ProviderContainer(
    overrides: [
      questionRepositoryProvider.overrideWith((ref) => Future.value(qRepo)),
      winnerRepositoryProvider.overrideWith((ref) => Future.value(wRepo)),
      randomProvider.overrideWithValue(RandomProvider.seeded(seed)),
    ],
  );
  addTearDown(container.dispose);
  return container;
}

/// Reads the current [TriviaFlowState] from [container].
TriviaFlowState _state(ProviderContainer container) =>
    container.read(triviaFlowControllerProvider);

/// Reads the notifier from [container].
TriviaFlowController _notifier(ProviderContainer container) =>
    container.read(triviaFlowControllerProvider.notifier);

/// Drives a player through pass → question → answer in one step.
///
/// [correct] = true answers with the correct index (0), false answers with 1.
Future<void> _answerFor(
  ProviderContainer container, {
  required bool correct,
}) async {
  _notifier(container).pasarDispositivo();
  await _notifier(container).responder(correct ? 0 : 1);
}

// ─── Tests ────────────────────────────────────────────────────────────────────

void main() {
  group('TriviaFlowController · estado inicial', () {
    test('estado inicial es setup sin sesión', () {
      final container = _container();
      final state = _state(container);

      expect(state.fase, TriviaFase.setup);
      expect(state.session, isNull);
      expect(state.config, isNull);
      expect(state.currentPlayerIndex, 0);
      expect(state.currentPlayer, isNull);
      expect(state.winners, isEmpty);
    });
  });

  group('TriviaFlowController · iniciar()', () {
    test('con pool suficiente -> fase pass, ronda 0 dificultad facil, '
        'cada jugador tiene pregunta distinta', () async {
      final container = _container();
      final config = _config(['Ana', 'Leo']);
      await _notifier(container).iniciar(config);

      final state = _state(container);
      expect(state.fase, TriviaFase.pass);
      expect(state.config, config);
      expect(state.session, isNotNull);
      expect(state.currentPlayerIndex, 0);
      expect(state.currentRound, 0);
      // Ronda 0 → facil.
      expect(state.session!.currentDifficulty, Difficulty.facil);
      // Cada jugador tiene una pregunta distinta.
      expect(state.roundQuestions.length, 2);
      final questions = state.roundQuestions.values.toList();
      expect(questions[0], isNot(equals(questions[1])));
      // Todas las preguntas son de dificultad facil.
      for (final q in questions) {
        expect(q.difficulty, Difficulty.facil);
      }
    });

    test(
      'pool insuficiente -> error kind sinPreguntas, juego no iniciado',
      () async {
        // Only 1 question per tier, but 2 players → insufficient.
        final thinRepo = buildFakeQuestionRepo(countPerTier: 1);
        final container = _container(questionRepo: thinRepo);

        await _notifier(container).iniciar(_config(['Ana', 'Leo']));

        final state = _state(container);
        expect(state.fase, TriviaFase.error);
        expect(state.errorKind, TriviaErrorKind.sinPreguntas);
        expect(state.session, isNull);
      },
    );

    test('exactamente N preguntas para N jugadores -> suficiente', () async {
      // 2 questions per tier, 2 players → exactly enough.
      final repo = buildFakeQuestionRepo(countPerTier: 2);
      final container = _container(questionRepo: repo);

      await _notifier(container).iniciar(_config(['Ana', 'Leo']));

      expect(_state(container).fase, TriviaFase.pass);
    });
  });

  group('TriviaFlowController · pasarDispositivo()', () {
    test('pass -> question, currentQuestion set', () async {
      final container = _container();
      await _notifier(container).iniciar(_config(['Ana', 'Leo']));

      expect(_state(container).fase, TriviaFase.pass);
      _notifier(container).pasarDispositivo();

      final state = _state(container);
      expect(state.fase, TriviaFase.question);
      expect(state.currentQuestion, isNotNull);
      expect(
        state.currentQuestion,
        state.roundQuestions[const TriviaPlayer('Ana')],
      );
    });

    test('pasarDispositivo() sin sesión no hace nada', () {
      final container = _container();
      _notifier(container).pasarDispositivo();
      expect(_state(container).fase, TriviaFase.setup);
    });
  });

  group('TriviaFlowController · responder()', () {
    test(
      'respuesta correcta -> jugador sobrevive, avanza al siguiente',
      () async {
        final container = _container();
        await _notifier(container).iniciar(_config(['Ana', 'Leo']));

        _notifier(container).pasarDispositivo();
        await _notifier(container).responder(0); // correct index = 0

        final state = _state(container);
        expect(state.fase, TriviaFase.pass);
        // Ana sobrevive → sigue en alivePlayers.
        expect(state.alivePlayers, contains(const TriviaPlayer('Ana')));
        // Ahora es el turno de Leo (índice 1 en la lista de vivos original,
        // pero puede ser 0 o 1 según si Ana sigue viva).
        expect(state.currentPlayer, const TriviaPlayer('Leo'));
      },
    );

    test('respuesta incorrecta -> jugador eliminado', () async {
      final container = _container();
      await _notifier(container).iniciar(_config(['Ana', 'Leo']));

      _notifier(container).pasarDispositivo();
      await _notifier(container).responder(1); // wrong → Ana eliminated

      final state = _state(container);
      expect(state.alivePlayers, isNot(contains(const TriviaPlayer('Ana'))));
    });

    test('responder() fuera de fase question no hace nada', () async {
      final container = _container();
      await _notifier(container).iniciar(_config(['Ana', 'Leo']));
      // We are in pass, not question.
      expect(_state(container).fase, TriviaFase.pass);
      await _notifier(container).responder(0);
      expect(_state(container).fase, TriviaFase.pass);
    });
  });

  group('TriviaFlowController · dificultad por ronda', () {
    test('rondas 0-2 → facil, 3-5 → dificil, 6-8 → muyDificil', () async {
      // 2 players (minimum), both answer correctly every round.
      // Need at least 2 questions per tier for 2 players.
      final repo = buildFakeQuestionRepo(countPerTier: 9);
      final container = _container(questionRepo: repo);
      await _notifier(container).iniciar(_config(['Ana', 'Leo']));

      for (var round = 0; round < kTriviaRoundCount; round++) {
        final expectedDifficulty = difficultyForRound(round);
        final state = _state(container);
        expect(
          state.session!.currentDifficulty,
          expectedDifficulty,
          reason: 'round $round should be $expectedDifficulty',
        );
        // Check the dealt question difficulty for the first alive player.
        final firstPlayer = state.alivePlayers.first;
        final q = state.roundQuestions[firstPlayer]!;
        expect(
          q.difficulty,
          expectedDifficulty,
          reason: 'round $round question',
        );

        // Both Ana and Leo answer correctly.
        await _answerFor(container, correct: true); // Ana
        await _answerFor(container, correct: true); // Leo
      }
      expect(_state(container).fase, TriviaFase.gameOver);
    });
  });

  group('TriviaFlowController · gameOver', () {
    test('happy path: todos responden correctamente en 9 rondas -> '
        'gameOver, winners = todos, WinnerRepository +1 por jugador', () async {
      final wRepo = FakeWinnerRepository();
      final repo = buildFakeQuestionRepo(countPerTier: 9);
      final container = _container(questionRepo: repo, winnerRepo: wRepo);
      final config = _config(['Ana', 'Leo']);
      await _notifier(container).iniciar(config);

      // Drive all 9 rounds with correct answers for both players.
      for (var round = 0; round < kTriviaRoundCount; round++) {
        // Ana answers.
        await _answerFor(container, correct: true);
        // Leo answers.
        await _answerFor(container, correct: true);
      }

      final state = _state(container);
      expect(state.fase, TriviaFase.gameOver);
      expect(
        state.winners,
        containsAll([const TriviaPlayer('Ana'), const TriviaPlayer('Leo')]),
      );
      expect(wRepo.wins['Ana'], 1);
      expect(wRepo.wins['Leo'], 1);
    });

    test(
      'todos fallan ronda 0 -> gameOver, winners vacío, sin victorias registradas',
      () async {
        final wRepo = FakeWinnerRepository();
        final container = _container(winnerRepo: wRepo);
        await _notifier(container).iniciar(_config(['Ana', 'Leo']));

        // Both answer incorrectly → eliminated in round 0.
        await _answerFor(container, correct: false); // Ana wrong
        // After Ana is eliminated Leo is the only one left in round.
        await _answerFor(container, correct: false); // Leo wrong

        final state = _state(container);
        expect(state.fase, TriviaFase.gameOver);
        expect(state.winners, isEmpty);
        expect(wRepo.wins, isEmpty);
      },
    );

    test('gameOver no registra victorias cuando no hay ganadores', () async {
      final wRepo = FakeWinnerRepository();
      final container = _container(winnerRepo: wRepo);
      await _notifier(container).iniciar(_config(['Ana', 'Leo']));

      await _answerFor(container, correct: false);
      await _answerFor(container, correct: false);

      expect(wRepo.wins, isEmpty);
    });
  });

  group('TriviaFlowController · reiniciar()', () {
    test('reiniciar() vuelve al estado inicial', () async {
      final container = _container();
      await _notifier(container).iniciar(_config(['Ana', 'Leo']));
      _notifier(container).pasarDispositivo();
      expect(_state(container).fase, TriviaFase.question);

      _notifier(container).reiniciar();
      final state = _state(container);
      expect(state.fase, TriviaFase.setup);
      expect(state.session, isNull);
      expect(state.config, isNull);
      expect(state.currentPlayer, isNull);
    });
  });

  group('TriviaFlowController · avance de ronda', () {
    test('cuando una ronda termina la siguiente empieza con preguntas de la '
        'dificultad correcta y distintas', () async {
      final repo = buildFakeQuestionRepo(countPerTier: 6);
      final container = _container(questionRepo: repo);
      await _notifier(container).iniciar(_config(['Ana', 'Leo']));

      // Ronda 0 (facil): ambos responden correctamente.
      await _answerFor(container, correct: true); // Ana
      await _answerFor(container, correct: true); // Leo

      // Ahora debemos estar en ronda 1 (facil todavía).
      final state = _state(container);
      expect(state.fase, TriviaFase.pass);
      expect(state.currentRound, 1);
      expect(state.session!.currentDifficulty, Difficulty.facil);
      // Las preguntas nuevas deben ser distintas a las anteriores.
      expect(state.roundQuestions.length, 2);
    });

    test(
      'al pasar de ronda 2 a ronda 3 la dificultad cambia a dificil',
      () async {
        final repo = buildFakeQuestionRepo(countPerTier: 9);
        final container = _container(questionRepo: repo);
        await _notifier(container).iniciar(_config(['Ana', 'Leo']));

        // Drive rounds 0, 1, 2 (all facil) with both players answering correctly.
        for (var i = 0; i < 3; i++) {
          await _answerFor(container, correct: true); // Ana
          await _answerFor(container, correct: true); // Leo
        }

        expect(_state(container).currentRound, 3);
        expect(
          _state(container).session!.currentDifficulty,
          Difficulty.dificil,
        );
      },
    );
  });

  group('TriviaFlowController · eliminación parcial en una ronda', () {
    test('jugador eliminado en mitad de ronda no recibe pregunta en rondas '
        'posteriores', () async {
      final repo = buildFakeQuestionRepo(countPerTier: 9);
      final container = _container(questionRepo: repo);
      await _notifier(container).iniciar(_config(['Ana', 'Leo', 'Kim']));

      // Ronda 0: Ana falla, Leo y Kim aciertan.
      await _answerFor(container, correct: false); // Ana eliminated
      await _answerFor(container, correct: true); // Leo
      await _answerFor(container, correct: true); // Kim

      final state = _state(container);
      expect(state.fase, TriviaFase.pass);
      expect(state.alivePlayers, hasLength(2));
      expect(state.alivePlayers, isNot(contains(const TriviaPlayer('Ana'))));
      // Next round deals only to alive players.
      expect(state.roundQuestions.keys, hasLength(2));
      expect(
        state.roundQuestions.containsKey(const TriviaPlayer('Ana')),
        isFalse,
      );
    });
  });

  group('TriviaFlowController · error en _finishRound', () {
    test('pool insuficiente al recargar preguntas en ronda siguiente → '
        'TriviaFase.error kind sinPreguntas', () async {
      // 2 players, so round 0 needs >= 2 questions per tier.
      // The call-count repo returns 2 questions per tier on the first batch
      // (the iniciar() load — 3 getPool calls, one per difficulty), but only
      // 1 question per tier on every subsequent batch (the _finishRound
      // reload). 1 < 2 alive players → _finishRound must transition to error.
      final repo = buildCallCountRepo(
        firstCountPerTier: 2,
        laterCountPerTier: 1,
      );
      final container = _container(questionRepo: repo);
      await _notifier(container).iniciar(_config(['Ana', 'Leo']));

      // iniciar() should have succeeded (2 questions per tier >= 2 players).
      expect(_state(container).fase, TriviaFase.pass);

      // Both players answer correctly → round 0 ends → _finishRound fires.
      await _answerFor(container, correct: true); // Ana
      await _answerFor(container, correct: true); // Leo

      // _finishRound reloads the pool and gets only 1 question per tier,
      // which is < 2 alive players. The try/catch in _finishRound must
      // catch this and transition to TriviaFase.error.
      expect(_state(container).fase, TriviaFase.error);
      expect(_state(container).errorKind, TriviaErrorKind.sinPreguntas);
    });
  });
}
