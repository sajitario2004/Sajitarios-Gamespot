import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/impostor/impostor_game.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart';

/// Lee la fila de metadatos del índice de la tabla `impostor_words`.
Future<List<Map<String, Object?>>> _indexList(Database db) {
  return db.rawQuery("PRAGMA index_list('$kImpostorWordsTable')");
}

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('AppDatabase onCreate (esquema v2 vía FFI)', () {
    late Database db;

    setUp(() async {
      // Reproduce el esquema que crea AppDatabase._onCreate (sin path_provider
      // ni el seed loader, que dependen de rootBundle). Se construye con el
      // mismo DDL que producción para validar tabla + índice + colación NOCASE.
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await db.execute('''
        CREATE TABLE $kImpostorWordsTable (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          word TEXT NOT NULL UNIQUE COLLATE NOCASE,
          hint TEXT NOT NULL,
          is_seed INTEGER NOT NULL DEFAULT 0,
          created_at INTEGER NOT NULL
        )
      ''');
      await db.execute(
        'CREATE INDEX idx_impostor_words_word ON $kImpostorWordsTable(word)',
      );
    });

    tearDown(() async {
      await db.close();
    });

    test('crea la tabla impostor_words', () async {
      final tables = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kImpostorWordsTable],
      );
      expect(tables, hasLength(1));
    });

    test('crea el índice idx_impostor_words_word', () async {
      final indexes = await _indexList(db);
      final names = indexes.map((r) => r['name'] as String);
      expect(names, contains('idx_impostor_words_word'));
    });

    test('la columna word es UNIQUE con colación NOCASE', () async {
      final sql =
          (await db.rawQuery(
                "SELECT sql FROM sqlite_master WHERE type='table' AND name=?",
                [kImpostorWordsTable],
              )).first['sql']
              as String;
      expect(sql.toUpperCase(), contains('UNIQUE'));
      expect(sql.toUpperCase(), contains('COLLATE NOCASE'));

      // Verificación funcional: distinta capitalización colisiona.
      await db.insert(kImpostorWordsTable, <String, Object?>{
        'word': 'Pirata',
        'hint': 'barco',
        'is_seed': 0,
        'created_at': 0,
      });
      await expectLater(
        () => db.insert(kImpostorWordsTable, <String, Object?>{
          'word': 'pirata',
          'hint': 'otra',
          'is_seed': 0,
          'created_at': 0,
        }),
        throwsA(isA<DatabaseException>()),
      );
    });
  });

  group('AppDatabase migración v1 -> v2 (onUpgradeForTest)', () {
    test('recrea la tabla con UNIQUE NOCASE y conserva el índice', () async {
      final db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      // Esquema v1: UNIQUE sensible a mayúsculas.
      await db.execute('''
        CREATE TABLE $kImpostorWordsTable (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          word TEXT NOT NULL UNIQUE,
          hint TEXT NOT NULL,
          is_seed INTEGER NOT NULL DEFAULT 0,
          created_at INTEGER NOT NULL
        )
      ''');
      await db.execute(
        'CREATE INDEX idx_impostor_words_word ON $kImpostorWordsTable(word)',
      );
      await db.insert(kImpostorWordsTable, <String, Object?>{
        'word': 'gato',
        'hint': 'felino',
        'is_seed': 1,
        'created_at': 100,
      });

      await AppDatabase(
        descriptors: const [ImpostorGame()],
      ).onUpgradeForTest(db, 1, kAppDatabaseVersion);

      // Datos conservados.
      final rows = await db.query(kImpostorWordsTable);
      expect(rows.map((r) => r['word']), contains('gato'));

      // Índice recreado tras la migración.
      final indexes = await _indexList(db);
      expect(
        indexes.map((r) => r['name'] as String),
        contains('idx_impostor_words_word'),
      );

      // La unicidad ahora es NOCASE.
      await expectLater(
        () => db.insert(kImpostorWordsTable, <String, Object?>{
          'word': 'GATO',
          'hint': 'otra',
          'is_seed': 0,
          'created_at': 200,
        }),
        throwsA(isA<DatabaseException>()),
      );

      await db.close();
    });

    test(
      'deduplica NOCASE: "Pirata"(seed) + "pirata"(usuario) preexistentes en v1 '
      'conservan una sola fila (gana la seed más antigua)',
      () async {
        final db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
        // Esquema v1: UNIQUE sensible a mayúsculas, así que "Pirata" y "pirata"
        // pueden coexistir.
        await db.execute('''
          CREATE TABLE $kImpostorWordsTable (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            word TEXT NOT NULL UNIQUE,
            hint TEXT NOT NULL,
            is_seed INTEGER NOT NULL DEFAULT 0,
            created_at INTEGER NOT NULL
          )
        ''');
        await db.execute(
          'CREATE INDEX idx_impostor_words_word ON $kImpostorWordsTable(word)',
        );
        // "Pirata" del seed (más antigua) y "pirata" del usuario (posterior).
        await db.insert(kImpostorWordsTable, <String, Object?>{
          'word': 'Pirata',
          'hint': 'barco',
          'is_seed': 1,
          'created_at': 100,
        });
        await db.insert(kImpostorWordsTable, <String, Object?>{
          'word': 'pirata',
          'hint': 'usuario',
          'is_seed': 0,
          'created_at': 200,
        });

        await AppDatabase(
          descriptors: const [ImpostorGame()],
        ).onUpgradeForTest(db, 1, kAppDatabaseVersion);

        // Tras migrar a NOCASE solo queda UNA fila para el grupo "pirata".
        final rows = await db.query(
          kImpostorWordsTable,
          where: 'word = ? COLLATE NOCASE',
          whereArgs: ['pirata'],
        );
        expect(rows, hasLength(1));
        // Gana la del seed más antigua: "Pirata" / pista "barco" / is_seed 1.
        expect(rows.single['word'], 'Pirata');
        expect(rows.single['hint'], 'barco');
        expect(rows.single['is_seed'], 1);

        // Y volver a insertar la misma palabra con otra capitalización falla.
        await expectLater(
          () => db.insert(kImpostorWordsTable, <String, Object?>{
            'word': 'PIRATA',
            'hint': 'otra',
            'is_seed': 0,
            'created_at': 300,
          }),
          throwsA(isA<DatabaseException>()),
        );

        await db.close();
      },
    );

    test('onUpgrade desde la misma versión no toca nada', () async {
      final db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await db.execute('''
        CREATE TABLE $kImpostorWordsTable (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          word TEXT NOT NULL UNIQUE COLLATE NOCASE,
          hint TEXT NOT NULL,
          is_seed INTEGER NOT NULL DEFAULT 0,
          created_at INTEGER NOT NULL
        )
      ''');
      // oldVersion == newVersion: el bucle no entra y no lanza.
      await AppDatabase(
        descriptors: const [ImpostorGame()],
      ).onUpgradeForTest(db, 2, 2);
      final tables = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        [kImpostorWordsTable],
      );
      expect(tables, hasLength(1));
      await db.close();
    });
  });

  group('AppDatabase comportamiento singleton', () {
    test('close() sin abrir no lanza y es idempotente', () async {
      final appDb = AppDatabase(descriptors: const [ImpostorGame()]);
      await appDb.close();
      await appDb.close();
    });

    test(
      'database getter comparte una única operación de apertura concurrente',
      () async {
        // _open() real usa path_provider (no disponible en tests puros), así que
        // la apertura falla; lo que validamos es el contrato del getter: dos
        // llamadas concurrentes NO disparan dos aperturas, comparten el mismo
        // Future (in-flight) — ambas fallan de forma idéntica.
        final appDb = AppDatabase(descriptors: const [ImpostorGame()]);
        final first = appDb.database;
        final second = appDb.database;
        expect(identical(first, second), isTrue);

        await expectLater(first, throwsA(anything));
        await expectLater(second, throwsA(anything));
      },
    );

    test('constantes de versión y nombre de fichero', () {
      expect(kAppDatabaseVersion, 8);
      expect(kAppDatabaseFileName, isNotEmpty);
    });
  });
}
