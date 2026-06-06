/// Prueba de migración de base de datos v6 → v7.
///
/// Verifica que al actualizar una instalación existente (v6) se crea la tabla
/// [kYoNuncaStatementsTable] sin tocar las tablas del Impostor, las de Trivia,
/// las de Wavelength ni las de Tabú. El seed de declaraciones no se carga en
/// este test (requiere [rootBundle]) — se prueba por separado.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/impostor/data/impostor_schema.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart'
    show kImpostorWordsTable;
import 'package:sajitarios_gamespot/games/tabu/data/tabu_schema.dart';
import 'package:sajitarios_gamespot/games/trivia/data/trivia_schema.dart';
import 'package:sajitarios_gamespot/games/wavelength/data/wavelength_schema.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/data/yo_nunca_schema.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/yo_nunca_game.dart';

/// Abre la BD en memoria con el esquema v6 (impostor + trivia + wavelength + tabu).
Future<Database> _openV6Schema() async {
  final db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
  await ImpostorSchema.createImpostorWordsTable(db);
  await ImpostorSchema.createGameHistoryTable(db);
  await TriviaSchema.createTables(db);
  await WavelengthSchema.createTables(db);
  await TabuSchema.createTables(db);
  return db;
}

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('Migración v6 → v7: tabla yo_nunca_statements', () {
    test('crea yo_nunca_statements con sus columnas', () async {
      final db = await _openV6Schema();

      await AppDatabase(
        descriptors: const [YoNuncaGame()],
      ).onUpgradeForTest(db, 6, 7);

      final tables = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kYoNuncaStatementsTable],
      );
      expect(
        tables,
        hasLength(1),
        reason: 'yo_nunca_statements debe existir tras la migración v7',
      );

      // Columnas: id, frase, intensidad, is_seed.
      final cols = await db.rawQuery(
        'PRAGMA table_info($kYoNuncaStatementsTable)',
      );
      final colNames = cols.map((r) => r['name'] as String).toSet();
      expect(colNames, containsAll(['id', 'frase', 'intensidad', 'is_seed']));

      await db.close();
    });

    test('las tablas del Impostor no se ven afectadas', () async {
      final db = await _openV6Schema();

      await db.insert(kImpostorWordsTable, <String, Object?>{
        'word': 'prueba',
        'hint': 'test',
        'is_seed': 1,
        'created_at': 1,
      });

      await AppDatabase(
        descriptors: const [YoNuncaGame()],
      ).onUpgradeForTest(db, 6, 7);

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
      final db = await _openV6Schema();

      await db.insert(kTriviaWinnersTable, <String, Object?>{
        'name': 'Ana',
        'wins': 3,
      });

      await AppDatabase(
        descriptors: const [YoNuncaGame()],
      ).onUpgradeForTest(db, 6, 7);

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
      final db = await _openV6Schema();

      await db.insert(kWavelengthSpectraTable, <String, Object?>{
        'izquierda': 'frío',
        'derecha': 'caliente',
        'is_seed': 1,
      });

      await AppDatabase(
        descriptors: const [YoNuncaGame()],
      ).onUpgradeForTest(db, 6, 7);

      final rows = await db.query(kWavelengthSpectraTable);
      expect(
        rows,
        hasLength(1),
        reason: 'wavelength_spectra no debe verse alterada',
      );
      expect(rows.single['izquierda'], 'frío');

      await db.close();
    });

    test('las tablas de Tabú no se ven afectadas', () async {
      final db = await _openV6Schema();

      await db.insert(kTabuWordsTable, <String, Object?>{
        'palabra': 'elefante',
        'prohibidas_json': '["animal","grande","trompa"]',
        'is_seed': 1,
      });

      await AppDatabase(
        descriptors: const [YoNuncaGame()],
      ).onUpgradeForTest(db, 6, 7);

      final rows = await db.query(kTabuWordsTable);
      expect(rows, hasLength(1), reason: 'tabu_words no debe verse alterada');
      expect(rows.single['palabra'], 'elefante');

      await db.close();
    });

    test(
      'aplicar v7 dos veces lanza (tabla ya existe) — comportamiento documentado',
      () async {
        final db = await _openV6Schema();

        await AppDatabase(
          descriptors: const [YoNuncaGame()],
        ).onUpgradeForTest(db, 6, 7);

        await expectLater(
          () => AppDatabase(
            descriptors: const [YoNuncaGame()],
          ).onUpgradeForTest(db, 6, 7),
          throwsA(isA<DatabaseException>()),
          reason: 'Segunda migración v7 lanza porque la tabla ya existe',
        );

        await db.close();
      },
    );
  });
}
