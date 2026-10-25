/// SQL DDL and schema helpers for the Trivia bounded context.
///
/// Mirrors the shape of [ImpostorSchema] so a future [GameDescriptor] can
/// wire it in via `onCreateTables` / `onUpgradeTables` without changes here.
/// For v0.51 tests the [createTables] helper is called directly on an FFI
/// in-memory [Database] — no [AppDatabase] or version bump needed yet.
library;

import 'package:sqflite_common/sqlite_api.dart';

/// Table name for trivia questions.
const String kTriviaQuestionsTable = 'trivia_questions';

/// Table name for trivia winners (win counter per player name).
const String kTriviaWinnersTable = 'trivia_winners';

/// Schema, DDL constants and creation helpers for the Trivia game.
///
/// Kept in an [abstract final class] so it cannot be instantiated —
/// identical pattern to [ImpostorSchema].
abstract final class TriviaSchema {
  /// Creates all Trivia tables in [db] from scratch.
  ///
  /// Suitable as the body of `GameDescriptor.onCreateTables` once wired.
  static Future<void> createTables(DatabaseExecutor db) async {
    await createQuestionsTable(db);
    await createWinnersTable(db);
  }

  /// Creates the `trivia_questions` table and its index.
  static Future<void> createQuestionsTable(DatabaseExecutor db) async {
    await db.execute('''
      CREATE TABLE $kTriviaQuestionsTable (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        tematica_id TEXT NOT NULL,
        difficulty TEXT NOT NULL,
        enunciado TEXT NOT NULL,
        options_json TEXT NOT NULL,
        correct_index INTEGER NOT NULL,
        is_seed INTEGER NOT NULL DEFAULT 0
      )
    ''');
    await db.execute(
      'CREATE INDEX idx_trivia_questions_tema_diff '
      'ON $kTriviaQuestionsTable(tematica_id, difficulty)',
    );
  }

  /// Creates the `trivia_winners` table.
  static Future<void> createWinnersTable(DatabaseExecutor db) async {
    await db.execute('''
      CREATE TABLE $kTriviaWinnersTable (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        name TEXT NOT NULL UNIQUE COLLATE NOCASE,
        wins INTEGER NOT NULL DEFAULT 0
      )
    ''');
  }
}
