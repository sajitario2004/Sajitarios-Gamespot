/// Tests for [DealQuestionsUseCase]: distinct-question-per-player guarantee
/// and edge cases, using a seeded [RandomProvider] for determinism.
library;

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/deal_questions_use_case.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/question.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/trivia_player.dart';

Question _q(int id) => Question.create(
  id: id,
  tematicaId: 'historia',
  difficulty: Difficulty.facil,
  enunciado: 'Question $id',
  options: ['A', 'B', 'C', 'D'],
  correctIndex: 0,
);

List<Question> _pool(int count) =>
    List.generate(count, (i) => _q(i + 1), growable: false);

List<TriviaPlayer> _players(int count) =>
    List.generate(count, (i) => TriviaPlayer('P$i'), growable: false);

void main() {
  group('DealQuestionsUseCase', () {
    test('assigns exactly one distinct question per alive player', () {
      final useCase = DealQuestionsUseCase(RandomProvider.seeded(42));
      final players = _players(4);
      final pool = _pool(10);

      final result = useCase(alivePlayers: players, pool: pool);

      expect(result.keys, containsAll(players));
      expect(result.keys.length, players.length);
      // All assigned questions are distinct.
      final assigned = result.values.toSet();
      expect(assigned.length, players.length);
    });

    test('works with pool size exactly equal to player count', () {
      final useCase = DealQuestionsUseCase(RandomProvider.seeded(7));
      final players = _players(3);
      final pool = _pool(3);

      final result = useCase(alivePlayers: players, pool: pool);
      expect(result.length, 3);
      expect(result.values.toSet().length, 3);
    });

    test('is deterministic with a fixed seed', () {
      final players = _players(3);
      final pool = _pool(6);

      final r1 = DealQuestionsUseCase(RandomProvider.seeded(99))(
        alivePlayers: players,
        pool: pool,
      );
      final r2 = DealQuestionsUseCase(RandomProvider.seeded(99))(
        alivePlayers: players,
        pool: pool,
      );

      for (final player in players) {
        expect(r1[player], equals(r2[player]));
      }
    });

    test('throws when alivePlayers is empty', () {
      final useCase = DealQuestionsUseCase(RandomProvider.seeded(1));
      expect(
        () => useCase(alivePlayers: [], pool: _pool(4)),
        throwsArgumentError,
      );
    });

    test('throws when pool has fewer questions than players', () {
      final useCase = DealQuestionsUseCase(RandomProvider.seeded(1));
      expect(
        () => useCase(alivePlayers: _players(4), pool: _pool(3)),
        throwsArgumentError,
      );
    });

    test('different seeds produce different assignments for the same input', () {
      final players = _players(3);
      final pool = _pool(10);

      final r1 = DealQuestionsUseCase(RandomProvider.seeded(1))(
        alivePlayers: players,
        pool: pool,
      );
      final r2 = DealQuestionsUseCase(RandomProvider.seeded(2))(
        alivePlayers: players,
        pool: pool,
      );

      // With different seeds the assignment should differ for at least one player
      // across a pool of 10 questions (the probability of identical results is
      // negligibly small).
      final allSame = players.every((p) => r1[p] == r2[p]);
      expect(allSame, isFalse);
    });
  });
}
