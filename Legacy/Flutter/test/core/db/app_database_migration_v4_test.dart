/// Prueba de migración de base de datos v3 → v4.
///
/// Verifica que al actualizar una instalación existente (v3) se crean las tablas
/// de Trivia ([kTriviaQuestionsTable], [kTriviaWinnersTable]) sin tocar las
/// tablas del Impostor. El seed de preguntas no se carga en este test (requiere
/// [rootBundle]) — se prueba por separado en los tests de integración.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/impostor/data/impostor_schema.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart';
import 'package:sajitarios_gamespot/games/trivia/data/trivia_schema.dart';
import 'package:sajitarios_gamespot/games/trivia/trivia_game.dart';

/// Abre la BD en memoria con el esquema v3 (impostor_words + game_history).
Future<Database> _openV3Schema() async {
  final db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
  await ImpostorSchema.createImpostorWordsTable(db);
  await ImpostorSchema.createGameHistoryTable(db);
  return db;
}

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('Migración v3 → v4: tablas de Trivia', () {
    test('crea trivia_questions con sus columnas e índice', () async {
      final db = await _openV3Schema();

      // Solo el descriptor de Trivia participa en v4; el Impostor no aporta
      // nada nuevo en v4 (no tiene un step para esa versión).
      await AppDatabase(
        descriptors: const [TriviaGame()],
      ).onUpgradeForTest(db, 3, 4);

      // La tabla existe.
      final tables = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kTriviaQuestionsTable],
      );
      expect(tables, hasLength(1), reason: 'trivia_questions debe existir');

      // El índice existe.
      final indexes = await db.rawQuery(
        "PRAGMA index_list('$kTriviaQuestionsTable')",
      );
      final indexNames = indexes.map((r) => r['name'] as String).toList();
      expect(indexNames, contains('idx_trivia_questions_tema_diff'));

      await db.close();
    });

    test('crea trivia_winners con UNIQUE NOCASE en name', () async {
      final db = await _openV3Schema();

      await AppDatabase(
        descriptors: const [TriviaGame()],
      ).onUpgradeForTest(db, 3, 4);

      final tables = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kTriviaWinnersTable],
      );
      expect(tables, hasLength(1), reason: 'trivia_winners debe existir');

      // Unicidad funcional: dos entradas con distinta capitalización colisionan.
      await db.execute(
        'INSERT INTO $kTriviaWinnersTable (name, wins) VALUES (?, ?)',
        ['Nacho', 0],
      );
      await expectLater(
        () => db.execute(
          'INSERT INTO $kTriviaWinnersTable (name, wins) VALUES (?, ?)',
          ['nacho', 0],
        ),
        throwsA(isA<DatabaseException>()),
        reason: 'NOCASE debe impedir el duplicado "nacho"',
      );

      await db.close();
    });

    test('las tablas del Impostor no se ven afectadas', () async {
      final db = await _openV3Schema();

      // Insertar una fila para verificar que sigue ahí después.
      await db.insert(kImpostorWordsTable, <String, Object?>{
        'word': 'prueba',
        'hint': 'test',
        'is_seed': 1,
        'created_at': 1,
      });

      await AppDatabase(
        descriptors: const [TriviaGame()],
      ).onUpgradeForTest(db, 3, 4);

      final rows = await db.query(kImpostorWordsTable);
      expect(
        rows,
        hasLength(1),
        reason: 'impostor_words no debe verse alterada',
      );
      expect(rows.single['word'], 'prueba');

      await db.close();
    });

    test(
      'aplicar v4 dos veces lanza (tabla ya existe) — idempotencia en BD',
      () async {
        // Este test documenta el comportamiento: TriviaSchema.createTables no es
        // idempotente porque usa CREATE TABLE sin IF NOT EXISTS. Llamar a la
        // migración dos veces debe lanzar DatabaseException.
        final db = await _openV3Schema();

        await AppDatabase(
          descriptors: const [TriviaGame()],
        ).onUpgradeForTest(db, 3, 4);

        await expectLater(
          () => AppDatabase(
            descriptors: const [TriviaGame()],
          ).onUpgradeForTest(db, 3, 4),
          throwsA(isA<DatabaseException>()),
          reason: 'Segunda migración v4 debe lanzar porque la tabla ya existe',
        );

        await db.close();
      },
    );
  });
}
