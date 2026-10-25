/// Tests for [TriviaSchema]: verifies that createTables produces the expected
/// tables using an in-memory FFI database.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/trivia/data/trivia_schema.dart';

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('TriviaSchema.createTables', () {
    late Database db;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await TriviaSchema.createTables(db);
    });

    tearDown(() async {
      await db.close();
    });

    test('creates trivia_questions table', () async {
      final result = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kTriviaQuestionsTable],
      );
      expect(result, isNotEmpty);
    });

    test('creates trivia_winners table', () async {
      final result = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kTriviaWinnersTable],
      );
      expect(result, isNotEmpty);
    });

    test('creates index on (tematica_id, difficulty)', () async {
      final result = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='index' AND "
        "name='idx_trivia_questions_tema_diff'",
      );
      expect(result, isNotEmpty);
    });
  });
}
