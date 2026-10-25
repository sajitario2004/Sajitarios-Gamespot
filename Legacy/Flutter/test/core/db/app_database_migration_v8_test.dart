/// Prueba de migración de base de datos v7 → v8.
///
/// Verifica que al actualizar una instalación existente (v7) se crean las
/// tablas [kBombaSilabasTable] y [kBombaCategoriasTable] sin tocar las tablas
/// del Impostor, las de Trivia, las de Wavelength, las de Tabú ni las de
/// Yo Nunca. El seed de La Bomba no se carga en este test (requiere
/// [rootBundle]) — se prueba por separado.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/bomba/bomba_game.dart';
import 'package:sajitarios_gamespot/games/bomba/data/bomba_schema.dart';
import 'package:sajitarios_gamespot/games/impostor/data/impostor_schema.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart'
    show kImpostorWordsTable;
import 'package:sajitarios_gamespot/games/tabu/data/tabu_schema.dart';
import 'package:sajitarios_gamespot/games/trivia/data/trivia_schema.dart';
import 'package:sajitarios_gamespot/games/wavelength/data/wavelength_schema.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/data/yo_nunca_schema.dart';

/// Abre la BD en memoria con el esquema v7 (impostor + trivia + wavelength +
/// tabu + yo_nunca).
Future<Database> _openV7Schema() async {
  final db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
  await ImpostorSchema.createImpostorWordsTable(db);
  await ImpostorSchema.createGameHistoryTable(db);
  await TriviaSchema.createTables(db);
  await WavelengthSchema.createTables(db);
  await TabuSchema.createTables(db);
  await YoNuncaSchema.createTables(db);
  return db;
}

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('Migración v7 → v8: tablas bomba_silabas y bomba_categorias', () {
    test('crea bomba_silabas con sus columnas', () async {
      final db = await _openV7Schema();

      await AppDatabase(
        descriptors: const [BombaGame()],
      ).onUpgradeForTest(db, 7, 8);

      final tables = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kBombaSilabasTable],
      );
      expect(
        tables,
        hasLength(1),
        reason: 'bomba_silabas debe existir tras la migración v8',
      );

      final cols = await db.rawQuery('PRAGMA table_info($kBombaSilabasTable)');
      final colNames = cols.map((r) => r['name'] as String).toSet();
      expect(colNames, containsAll(['id', 'silaba', 'is_seed']));

      await db.close();
    });

    test('crea bomba_categorias con sus columnas', () async {
      final db = await _openV7Schema();

      await AppDatabase(
        descriptors: const [BombaGame()],
      ).onUpgradeForTest(db, 7, 8);

      final tables = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kBombaCategoriasTable],
      );
      expect(
        tables,
        hasLength(1),
        reason: 'bomba_categorias debe existir tras la migración v8',
      );

      final cols = await db.rawQuery(
        'PRAGMA table_info($kBombaCategoriasTable)',
      );
      final colNames = cols.map((r) => r['name'] as String).toSet();
      expect(colNames, containsAll(['id', 'categoria', 'is_seed']));

      await db.close();
    });

    test('las tablas del Impostor no se ven afectadas', () async {
      final db = await _openV7Schema();

      await db.insert(kImpostorWordsTable, <String, Object?>{
        'word': 'prueba',
        'hint': 'test',
        'is_seed': 1,
        'created_at': 1,
      });

      await AppDatabase(
        descriptors: const [BombaGame()],
      ).onUpgradeForTest(db, 7, 8);

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
      final db = await _openV7Schema();

      await db.insert(kTriviaWinnersTable, <String, Object?>{
        'name': 'Ana',
        'wins': 3,
      });

      await AppDatabase(
        descriptors: const [BombaGame()],
      ).onUpgradeForTest(db, 7, 8);

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
      final db = await _openV7Schema();

      await db.insert(kWavelengthSpectraTable, <String, Object?>{
        'izquierda': 'frío',
        'derecha': 'caliente',
        'is_seed': 1,
      });

      await AppDatabase(
        descriptors: const [BombaGame()],
      ).onUpgradeForTest(db, 7, 8);

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
      final db = await _openV7Schema();

      await db.insert(kTabuWordsTable, <String, Object?>{
        'palabra': 'elefante',
        'prohibidas_json': '["animal","grande","trompa"]',
        'is_seed': 1,
      });

      await AppDatabase(
        descriptors: const [BombaGame()],
      ).onUpgradeForTest(db, 7, 8);

      final rows = await db.query(kTabuWordsTable);
      expect(rows, hasLength(1), reason: 'tabu_words no debe verse alterada');
      expect(rows.single['palabra'], 'elefante');

      await db.close();
    });

    test('las tablas de Yo Nunca no se ven afectadas', () async {
      final db = await _openV7Schema();

      await db.insert(kYoNuncaStatementsTable, <String, Object?>{
        'frase': 'Yo nunca he viajado fuera del país',
        'intensidad': 'suave',
        'is_seed': 1,
      });

      await AppDatabase(
        descriptors: const [BombaGame()],
      ).onUpgradeForTest(db, 7, 8);

      final rows = await db.query(kYoNuncaStatementsTable);
      expect(
        rows,
        hasLength(1),
        reason: 'yo_nunca_statements no debe verse alterada',
      );

      await db.close();
    });

    test(
      'aplicar v8 dos veces lanza (tablas ya existen) — comportamiento documentado',
      () async {
        final db = await _openV7Schema();

        await AppDatabase(
          descriptors: const [BombaGame()],
        ).onUpgradeForTest(db, 7, 8);

        await expectLater(
          () => AppDatabase(
            descriptors: const [BombaGame()],
          ).onUpgradeForTest(db, 7, 8),
          throwsA(isA<DatabaseException>()),
          reason: 'Segunda migración v8 lanza porque las tablas ya existen',
        );

        await db.close();
      },
    );
  });
}
