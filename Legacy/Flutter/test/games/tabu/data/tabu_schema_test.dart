/// Tests para [TabuSchema]: verifica que createTables produce las tablas
/// esperadas usando una base de datos FFI en memoria.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/tabu/data/tabu_schema.dart';

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('TabuSchema.createTables', () {
    late Database db;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await TabuSchema.createTables(db);
    });

    tearDown(() async {
      await db.close();
    });

    test('crea la tabla tabu_words', () async {
      final result = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kTabuWordsTable],
      );
      expect(result, isNotEmpty);
    });

    test('la tabla tiene las columnas esperadas', () async {
      final rows = await db.rawQuery('PRAGMA table_info($kTabuWordsTable)');
      final columns = rows.map((r) => r['name'] as String).toSet();
      expect(
        columns,
        containsAll(['id', 'palabra', 'prohibidas_json', 'is_seed']),
      );
    });

    test('id es PRIMARY KEY AUTOINCREMENT', () async {
      // Insertar dos filas y verificar que los ids se autoincrementan
      await db.rawInsert(
        'INSERT INTO $kTabuWordsTable (palabra, prohibidas_json, is_seed) '
        'VALUES (?, ?, ?)',
        ['PalabraA', '["a","b","c","d"]', 0],
      );
      await db.rawInsert(
        'INSERT INTO $kTabuWordsTable (palabra, prohibidas_json, is_seed) '
        'VALUES (?, ?, ?)',
        ['PalabraB', '["a","b","c","d"]', 0],
      );
      final rows = await db.rawQuery(
        'SELECT id FROM $kTabuWordsTable ORDER BY id ASC',
      );
      expect(rows[0]['id'] as int, 1);
      expect(rows[1]['id'] as int, 2);
    });

    test('is_seed tiene valor por defecto 0', () async {
      await db.rawInsert(
        'INSERT INTO $kTabuWordsTable (palabra, prohibidas_json) VALUES (?, ?)',
        ['PalabraC', '["a","b","c","d"]'],
      );
      final rows = await db.rawQuery(
        'SELECT is_seed FROM $kTabuWordsTable LIMIT 1',
      );
      expect(rows.first['is_seed'] as int, 0);
    });
  });
}
