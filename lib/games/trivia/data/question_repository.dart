/// Data access layer for trivia questions.
///
/// [QuestionRepository] operates over the `trivia_questions` table. It maps
/// database rows ↔ domain [Question] objects using [dart:convert] for the
/// `options_json` column. Random selection stays in the domain use-case
/// ([DealQuestionsUseCase]); this repo only returns pools.
library;

import 'dart:convert';

import 'package:sqflite_common/sqlite_api.dart';

import 'package:sajitarios_gamespot/games/trivia/data/trivia_schema.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/question.dart';

/// Persists and queries trivia questions in the `trivia_questions` table.
class QuestionRepository {
  const QuestionRepository(this._db);

  final DatabaseExecutor _db;

  // ── Write ────────────────────────────────────────────────────────────────

  /// Inserts a single [question] and returns it with its assigned [Question.id].
  Future<Question> insert(Question question) async {
    final id = await _db.insert(
      kTriviaQuestionsTable,
      _toRow(question, isSeed: false),
    );
    return _withId(question, id);
  }

  /// Inserts all [questions] in a single batch.
  ///
  /// Each question is inserted independently; no transaction is opened here so
  /// the caller can wrap multiple repo calls in one if needed.
  Future<List<Question>> insertAll(List<Question> questions) async {
    final result = <Question>[];
    for (final q in questions) {
      result.add(await insert(q));
    }
    return result;
  }

  // ── Read ─────────────────────────────────────────────────────────────────

  /// Total number of questions in the table.
  Future<int> count() async {
    final rows = await _db.rawQuery(
      'SELECT COUNT(*) AS c FROM $kTriviaQuestionsTable',
    );
    return (rows.first['c'] as int?) ?? 0;
  }

  /// Number of questions for a given [tematicaId] and [difficulty].
  Future<int> countByTematicaAndDifficulty(
    String tematicaId,
    Difficulty difficulty,
  ) async {
    final rows = await _db.rawQuery(
      'SELECT COUNT(*) AS c FROM $kTriviaQuestionsTable '
      'WHERE tematica_id = ? AND difficulty = ?',
      <Object?>[tematicaId, difficulty.name],
    );
    return (rows.first['c'] as int?) ?? 0;
  }

  /// Fetches all questions that match any of [tematicaIds] and the given
  /// [difficulty].
  ///
  /// Returns domain [Question] objects with options decoded from JSON.
  /// Ordering is stable (by id ascending) to make tests predictable.
  Future<List<Question>> getPool({
    required Set<String> tematicaIds,
    required Difficulty difficulty,
  }) async {
    if (tematicaIds.isEmpty) return const [];

    final placeholders = List<String>.filled(tematicaIds.length, '?').join(',');
    final rows = await _db.rawQuery(
      'SELECT * FROM $kTriviaQuestionsTable '
      'WHERE tematica_id IN ($placeholders) AND difficulty = ? '
      'ORDER BY id ASC',
      <Object?>[...tematicaIds, difficulty.name],
    );
    return rows.map(_fromRow).toList(growable: false);
  }

  // ── Mapping ───────────────────────────────────────────────────────────────

  /// Converts a database row into a domain [Question].
  static Question _fromRow(Map<String, Object?> row) {
    final optionsRaw =
        jsonDecode(row['options_json'] as String) as List<dynamic>;
    return Question.create(
      id: row['id'] as int,
      tematicaId: row['tematica_id'] as String,
      difficulty: Difficulty.values.byName(row['difficulty'] as String),
      enunciado: row['enunciado'] as String,
      options: optionsRaw.cast<String>(),
      correctIndex: row['correct_index'] as int,
    );
  }

  /// Converts a domain [Question] to a database row map.
  static Map<String, Object?> _toRow(Question q, {required bool isSeed}) {
    return <String, Object?>{
      'tematica_id': q.tematicaId,
      'difficulty': q.difficulty.name,
      'enunciado': q.enunciado,
      'options_json': jsonEncode(q.options),
      'correct_index': q.correctIndex,
      'is_seed': isSeed ? 1 : 0,
    };
  }

  /// Returns a new [Question] identical to [q] but with [id] set.
  static Question _withId(Question q, int id) => Question.create(
    id: id,
    tematicaId: q.tematicaId,
    difficulty: q.difficulty,
    enunciado: q.enunciado,
    options: q.options.toList(),
    correctIndex: q.correctIndex,
  );
}
