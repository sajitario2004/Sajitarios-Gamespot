/// Fake in-memory [QuestionRepository] for unit tests.
///
/// Stores questions by tematica+difficulty and returns pools without touching
/// any real database. Shape mirrors the fake repos in the Impostor presentation
/// tests.
library;

import 'package:sajitarios_gamespot/games/trivia/data/question_repository.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/question.dart';

/// In-memory [QuestionRepository] that serves questions from a pre-loaded list.
class FakeQuestionRepository implements QuestionRepository {
  FakeQuestionRepository(this._questions);

  final List<Question> _questions;

  @override
  Future<List<Question>> getPool({
    required Set<String> tematicaIds,
    required Difficulty difficulty,
  }) async {
    return _questions
        .where(
          (q) =>
              tematicaIds.contains(q.tematicaId) && q.difficulty == difficulty,
        )
        .toList(growable: false);
  }

  @override
  Future<int> count() async => _questions.length;

  @override
  Future<int> countByTematicaAndDifficulty(
    String tematicaId,
    Difficulty difficulty,
  ) async => _questions
      .where((q) => q.tematicaId == tematicaId && q.difficulty == difficulty)
      .length;

  @override
  Future<Question> insert(Question question) async => question;

  @override
  Future<List<Question>> insertAll(List<Question> questions) async => questions;
}

// ─── Factory helpers ─────────────────────────────────────────────────────────

/// Builds a [Question] with sensible defaults for tests.
///
/// [correctIndex] defaults to 0 so callers can answer correctly with 0 and
/// incorrectly with any other value (1–3).
Question fakeQuestion({
  required int id,
  String tematicaId = 'historia',
  Difficulty difficulty = Difficulty.facil,
  int correctIndex = 0,
}) => Question.create(
  id: id,
  tematicaId: tematicaId,
  difficulty: difficulty,
  enunciado: 'Question $id',
  options: ['Correct', 'Wrong A', 'Wrong B', 'Wrong C'],
  correctIndex: correctIndex,
);

/// Builds a [FakeQuestionRepository] that contains [countPerTier] questions
/// per difficulty tier for [tematicaId].
///
/// Question ids are sequential starting at 1, allocated per tier in order:
/// facil 1..N, dificil N+1..2N, muyDificil 2N+1..3N.
FakeQuestionRepository buildFakeQuestionRepo({
  int countPerTier = 6,
  String tematicaId = 'historia',
}) {
  final questions = <Question>[];
  var id = 1;
  for (final difficulty in Difficulty.values) {
    for (var i = 0; i < countPerTier; i++) {
      questions.add(
        fakeQuestion(id: id++, tematicaId: tematicaId, difficulty: difficulty),
      );
    }
  }
  return FakeQuestionRepository(questions);
}

// ─── Call-count variant ───────────────────────────────────────────────────────

/// A [QuestionRepository] that returns [firstCallQuestions] on the first
/// [getPool] batch (all three difficulty calls for a single round load) and
/// [subsequentQuestions] on every subsequent batch.
///
/// Used to simulate a pool that is large enough for round 0 but shrinks
/// (or becomes insufficient) when a later round tries to reload questions.
///
/// Each call to [getPool] is counted independently; [callCount] tracks the
/// total number of calls made across all difficulties.
class CallCountQuestionRepository implements QuestionRepository {
  CallCountQuestionRepository({
    required List<Question> firstCallQuestions,
    required List<Question> subsequentQuestions,
    // How many individual getPool() calls constitute the "first batch".
    // Trivia loads 3 difficulties per round, so the default is 3.
    int firstBatchSize = 3,
  }) : _firstCallQuestions = firstCallQuestions,
       _subsequentQuestions = subsequentQuestions,
       _firstBatchSize = firstBatchSize;

  final List<Question> _firstCallQuestions;
  final List<Question> _subsequentQuestions;
  final int _firstBatchSize;

  int callCount = 0;

  @override
  Future<List<Question>> getPool({
    required Set<String> tematicaIds,
    required Difficulty difficulty,
  }) async {
    callCount++;
    final source = callCount <= _firstBatchSize
        ? _firstCallQuestions
        : _subsequentQuestions;
    return source
        .where(
          (q) =>
              tematicaIds.contains(q.tematicaId) && q.difficulty == difficulty,
        )
        .toList(growable: false);
  }

  @override
  Future<int> count() async => callCount <= _firstBatchSize
      ? _firstCallQuestions.length
      : _subsequentQuestions.length;

  @override
  Future<int> countByTematicaAndDifficulty(
    String tematicaId,
    Difficulty difficulty,
  ) async {
    final source = callCount <= _firstBatchSize
        ? _firstCallQuestions
        : _subsequentQuestions;
    return source
        .where((q) => q.tematicaId == tematicaId && q.difficulty == difficulty)
        .length;
  }

  @override
  Future<Question> insert(Question question) async => question;

  @override
  Future<List<Question>> insertAll(List<Question> questions) async => questions;
}

/// Builds a [CallCountQuestionRepository] whose first batch (the initial
/// [iniciar()] load) has [firstCountPerTier] questions per tier, and whose
/// subsequent batches (round reloads) have [laterCountPerTier] per tier.
///
/// Set [laterCountPerTier] to a value less than the number of alive players to
/// trigger the [TriviaErrorKind.sinPreguntas] path in [_finishRound].
CallCountQuestionRepository buildCallCountRepo({
  required int firstCountPerTier,
  required int laterCountPerTier,
  String tematicaId = 'historia',
}) {
  List<Question> makeQuestions(int countPerTier, {int idOffset = 0}) {
    final questions = <Question>[];
    var id = idOffset + 1;
    for (final difficulty in Difficulty.values) {
      for (var i = 0; i < countPerTier; i++) {
        questions.add(
          fakeQuestion(
            id: id++,
            tematicaId: tematicaId,
            difficulty: difficulty,
          ),
        );
      }
    }
    return questions;
  }

  return CallCountQuestionRepository(
    firstCallQuestions: makeQuestions(firstCountPerTier),
    subsequentQuestions: makeQuestions(laterCountPerTier, idOffset: 1000),
    firstBatchSize: 3, // one getPool call per difficulty tier
  );
}
