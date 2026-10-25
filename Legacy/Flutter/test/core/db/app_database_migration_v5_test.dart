/// Prueba de migración de base de datos v4 → v5.
///
/// Verifica que al actualizar una instalación existente (v4) se crea la tabla
/// [kWavelengthSpectraTable] sin tocar las tablas del Impostor ni las de Trivia.
/// El seed de espectros no se carga en este test (requiere [rootBundle]) —
/// se prueba por separado en los tests de integración.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/impostor/data/impostor_schema.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart'
    show kImpostorWordsTable;
import 'package:sajitarios_gamespot/games/trivia/data/trivia_schema.dart';
import 'package:sajitarios_gamespot/games/wavelength/data/wavelength_schema.dart';
import 'package:sajitarios_gamespot/games/wavelength/wavelength_game.dart';

/// Abre la BD en memoria con el esquema v4 (impostor + trivia).
Future<Database> _openV4Schema() async {
  final db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
  await ImpostorSchema.createImpostorWordsTable(db);
  await ImpostorSchema.createGameHistoryTable(db);
  await TriviaSchema.createTables(db);
  return db;
}

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('Migración v4 → v5: tabla wavelength_spectra', () {
    test('crea wavelength_spectra con sus columnas', () async {
      final db = await _openV4Schema();

      await AppDatabase(
        descriptors: const [WavelengthGame()],
      ).onUpgradeForTest(db, 4, 5);

      final tables = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kWavelengthSpectraTable],
      );
      expect(
        tables,
        hasLength(1),
        reason: 'wavelength_spectra debe existir tras la migración v5',
      );

      // Columnas: id, izquierda, derecha, is_seed.
      final cols = await db.rawQuery(
        'PRAGMA table_info($kWavelengthSpectraTable)',
      );
      final colNames = cols.map((r) => r['name'] as String).toSet();
      expect(colNames, containsAll(['id', 'izquierda', 'derecha', 'is_seed']));

      await db.close();
    });

    test('las tablas del Impostor no se ven afectadas', () async {
      final db = await _openV4Schema();

      await db.insert(kImpostorWordsTable, <String, Object?>{
        'word': 'prueba',
        'hint': 'test',
        'is_seed': 1,
        'created_at': 1,
      });

      await AppDatabase(
        descriptors: const [WavelengthGame()],
      ).onUpgradeForTest(db, 4, 5);

      final rows = await db.query(kImpostorWordsTable);
      expect(
        rows,
        hasLength(1),
        reason: 'impostor_words no debe verse alterada',
      );
      expect(rows.single['word'], 'prueba');

      await db.close();
    });

    test('las tablas de Trivia no se ven afectadas', () async {
      final db = await _openV4Schema();

      await db.insert(kTriviaWinnersTable, <String, Object?>{
        'name': 'Ana',
        'wins': 3,
      });

      await AppDatabase(
        descriptors: const [WavelengthGame()],
      ).onUpgradeForTest(db, 4, 5);

      final rows = await db.query(kTriviaWinnersTable);
      expect(
        rows,
        hasLength(1),
        reason: 'trivia_winners no debe verse alterada',
      );
      expect(rows.single['name'], 'Ana');

      await db.close();
    });

    test(
      'aplicar v5 dos veces lanza (tabla ya existe) — comportamiento documentado',
      () async {
        final db = await _openV4Schema();

        await AppDatabase(
          descriptors: const [WavelengthGame()],
        ).onUpgradeForTest(db, 4, 5);

        await expectLater(
          () => AppDatabase(
            descriptors: const [WavelengthGame()],
          ).onUpgradeForTest(db, 4, 5),
          throwsA(isA<DatabaseException>()),
          reason: 'Segunda migración v5 lanza porque la tabla ya existe',
        );

        await db.close();
      },
    );
  });
}
