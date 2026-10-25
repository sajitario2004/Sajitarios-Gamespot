/// Tests for [WavelengthSchema]: verifies that createTables produces the
/// expected table using an in-memory FFI database.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/wavelength/data/wavelength_schema.dart';

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('WavelengthSchema.createTables', () {
    late Database db;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await WavelengthSchema.createTables(db);
    });

    tearDown(() async {
      await db.close();
    });

    test('creates wavelength_spectra table', () async {
      final result = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kWavelengthSpectraTable],
      );
      expect(result, isNotEmpty);
    });

    test('table has expected columns', () async {
      final result = await db.rawQuery(
        'PRAGMA table_info($kWavelengthSpectraTable)',
      );
      final columnNames = result.map((r) => r['name'] as String).toSet();
      expect(
        columnNames,
        containsAll(['id', 'izquierda', 'derecha', 'is_seed']),
      );
    });
  });
}
