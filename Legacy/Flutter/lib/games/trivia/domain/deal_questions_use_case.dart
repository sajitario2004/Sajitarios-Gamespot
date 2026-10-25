/// Use case: deal one distinct question per alive player for the current round.
///
/// Pure domain — no Flutter or persistence imports. All randomness is injected
/// via [RandomProvider] so tests can use a seeded instance for determinism.
library;

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/question.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/trivia_player.dart';

/// Maps each alive player to a distinct [Question] drawn from [pool].
///
/// Algorithm: shuffle a copy of [pool] using Fisher-Yates (via [RandomProvider])
/// then assign the first N shuffled questions to the N alive players. This
/// guarantees no two players receive the same question in the same round
/// without requiring the caller to pre-filter.
///
/// Throws [ArgumentError] if:
/// - [alivePlayers] is empty.
/// - [pool] has fewer questions than [alivePlayers].
class DealQuestionsUseCase {
  const DealQuestionsUseCase(this._random);

  final RandomProvider _random;

  /// Returns a map of alive player → assigned question.
  ///
  /// [alivePlayers] — players who need a question this round.
  /// [pool] — available questions for the current tier (must be ≥ alivePlayers
  ///   in length).
  Map<TriviaPlayer, Question> call({
    required List<TriviaPlayer> alivePlayers,
    required List<Question> pool,
  }) {
    if (alivePlayers.isEmpty) {
      throw ArgumentError.value(
        alivePlayers,
        'alivePlayers',
        'alivePlayers must not be empty',
      );
    }
    if (pool.length < alivePlayers.length) {
      throw ArgumentError(
        'Not enough questions in the pool: need ${alivePlayers.length}, '
        'got ${pool.length}',
      );
    }

    final shuffled = List<Question>.of(pool);
    _shuffleQuestions(shuffled);

    return {
      for (var i = 0; i < alivePlayers.length; i++)
        alivePlayers[i]: shuffled[i],
    };
  }

  /// Fisher-Yates shuffle using [RandomProvider] for determinism in tests.
  void _shuffleQuestions(List<Question> list) {
    for (var i = list.length - 1; i > 0; i--) {
      final j = _random.nextInt(i + 1);
      final tmp = list[i];
      list[i] = list[j];
      list[j] = tmp;
    }
  }
}
