/// Prueba de migración de base de datos v5 → v6.
///
/// Verifica que al actualizar una instalación existente (v5) se crea la tabla
/// [kTabuWordsTable] sin tocar las tablas del Impostor, las de Trivia ni las
/// de Wavelength. El seed de palabras no se carga en este test (requiere
/// [rootBundle]) — se prueba por separado en los tests de integración.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/impostor/data/impostor_schema.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart'
    show kImpostorWordsTable;
import 'package:sajitarios_gamespot/games/tabu/data/tabu_schema.dart';
import 'package:sajitarios_gamespot/games/tabu/tabu_game.dart';
import 'package:sajitarios_gamespot/games/trivia/data/trivia_schema.dart';
import 'package:sajitarios_gamespot/games/wavelength/data/wavelength_schema.dart';

/// Abre la BD en memoria con el esquema v5 (impostor + trivia + wavelength).
Future<Database> _openV5Schema() async {
  final db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
  await ImpostorSchema.createImpostorWordsTable(db);
  await ImpostorSchema.createGameHistoryTable(db);
  await TriviaSchema.createTables(db);
  await WavelengthSchema.createTables(db);
  return db;
}

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('Migración v5 → v6: tabla tabu_words', () {
    test('crea tabu_words con sus columnas', () async {
      final db = await _openV5Schema();

      await AppDatabase(
        descriptors: const [TabuGame()],
      ).onUpgradeForTest(db, 5, 6);

      final tables = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kTabuWordsTable],
      );
      expect(
        tables,
        hasLength(1),
        reason: 'tabu_words debe existir tras la migración v6',
      );

      // Columnas: id, palabra, prohibidas_json, is_seed.
      final cols = await db.rawQuery('PRAGMA table_info($kTabuWordsTable)');
      final colNames = cols.map((r) => r['name'] as String).toSet();
      expect(
        colNames,
        containsAll(['id', 'palabra', 'prohibidas_json', 'is_seed']),
      );

      await db.close();
    });

    test('las tablas del Impostor no se ven afectadas', () async {
      final db = await _openV5Schema();

      await db.insert(kImpostorWordsTable, <String, Object?>{
        'word': 'prueba',
        'hint': 'test',
        'is_seed': 1,
        'created_at': 1,
      });

      await AppDatabase(
        descriptors: const [TabuGame()],
      ).onUpgradeForTest(db, 5, 6);

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
      final db = await _openV5Schema();

      await db.insert(kTriviaWinnersTable, <String, Object?>{
        'name': 'Ana',
        'wins': 3,
      });

      await AppDatabase(
        descriptors: const [TabuGame()],
      ).onUpgradeForTest(db, 5, 6);

      final rows = await db.query(kTriviaWinnersTable);
      expect(
        rows,
        hasLength(1),
        reason: 'trivia_winners no debe verse alterada',
      );
      expect(rows.single['name'], 'Ana');

      await db.close();
    });

    test('las tablas de Wavelength no se ven afectadas', () async {
      final db = await _openV5Schema();

      await db.insert(kWavelengthSpectraTable, <String, Object?>{
        'izquierda': 'frío',
        'derecha': 'caliente',
        'is_seed': 1,
      });

      await AppDatabase(
        descriptors: const [TabuGame()],
      ).onUpgradeForTest(db, 5, 6);

      final rows = await db.query(kWavelengthSpectraTable);
      expect(
        rows,
        hasLength(1),
        reason: 'wavelength_spectra no debe verse alterada',
      );
      expect(rows.single['izquierda'], 'frío');

      await db.close();
    });

    test(
      'aplicar v6 dos veces lanza (tabla ya existe) — comportamiento documentado',
      () async {
        final db = await _openV5Schema();

        await AppDatabase(
          descriptors: const [TabuGame()],
        ).onUpgradeForTest(db, 5, 6);

        await expectLater(
          () => AppDatabase(
            descriptors: const [TabuGame()],
          ).onUpgradeForTest(db, 5, 6),
          throwsA(isA<DatabaseException>()),
          reason: 'Segunda migración v6 lanza porque la tabla ya existe',
        );

        await db.close();
      },
    );
  });
}
