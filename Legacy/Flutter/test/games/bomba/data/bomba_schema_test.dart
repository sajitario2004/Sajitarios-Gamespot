/// Tests for [BombaSchema]: verifies that createTables produces the expected
/// tables using an in-memory FFI database.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/bomba/data/bomba_schema.dart';

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('BombaSchema.createTables', () {
    late Database db;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await BombaSchema.createTables(db);
    });

    tearDown(() async {
      await db.close();
    });

    test('crea la tabla bomba_silabas', () async {
      final result = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kBombaSilabasTable],
      );
      expect(result, isNotEmpty);
    });

    test('crea la tabla bomba_categorias', () async {
      final result = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kBombaCategoriasTable],
      );
      expect(result, isNotEmpty);
    });

    test('bomba_silabas tiene columnas id, silaba, is_seed', () async {
      final result = await db.rawQuery(
        'PRAGMA table_info($kBombaSilabasTable)',
      );
      final cols = result.map((r) => r['name'] as String).toSet();
      expect(cols, containsAll(['id', 'silaba', 'is_seed']));
    });

    test('bomba_categorias tiene columnas id, categoria, is_seed', () async {
      final result = await db.rawQuery(
        'PRAGMA table_info($kBombaCategoriasTable)',
      );
      final cols = result.map((r) => r['name'] as String).toSet();
      expect(cols, containsAll(['id', 'categoria', 'is_seed']));
    });
  });
}
