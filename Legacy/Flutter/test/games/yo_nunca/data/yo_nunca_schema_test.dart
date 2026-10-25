/// Tests for [YoNuncaSchema]: verifies that createTables produces the expected
/// table and index using an in-memory FFI database.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/yo_nunca/data/yo_nunca_schema.dart';

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('YoNuncaSchema.createTables', () {
    late Database db;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await YoNuncaSchema.createTables(db);
    });

    tearDown(() async {
      await db.close();
    });

    test('creates yo_nunca_statements table', () async {
      final result = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kYoNuncaStatementsTable],
      );
      expect(result, isNotEmpty);
    });

    test('creates index on intensidad', () async {
      final result = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='index' AND "
        "name='idx_yo_nunca_statements_intensidad'",
      );
      expect(result, isNotEmpty);
    });

    test('table has id, frase, intensidad and is_seed columns', () async {
      final result = await db.rawQuery(
        'PRAGMA table_info($kYoNuncaStatementsTable)',
      );
      final columns = result.map((r) => r['name'] as String).toSet();
      expect(columns, containsAll(['id', 'frase', 'intensidad', 'is_seed']));
    });
  });
}
