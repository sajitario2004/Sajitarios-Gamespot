import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/core/db/impostor_words_seed_loader.dart';
import 'package:sajitarios_gamespot/games/impostor/impostor_game.dart';
import 'package:sajitarios_gamespot/games/impostor/data/impostor_word.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/word.dart';

/// [AppDatabase] de prueba que expone una [Database] ya abierta (en memoria).
///
/// [WordRepository] solo necesita el getter [database], así que inyectamos una
/// base FFI en memoria sin pasar por la apertura real de [AppDatabase]
/// (path_provider + canales de plataforma), que no funciona en tests puros.
class _InMemoryAppDatabase implements AppDatabase {
  _InMemoryAppDatabase(this._db);

  final Database _db;

  @override
  Future<Database> get database => Future<Database>.value(_db);

  @override
  Future<void> close() => _db.close();

  @override
  dynamic noSuchMethod(Invocation invocation) => super.noSuchMethod(invocation);
}

/// Crea la tabla `impostor_words` igual que [AppDatabase] v2 en producción
/// (unicidad de `word` con colación NOCASE).
Future<void> _createSchema(Database db) async {
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
}

void main() {
  // Necesario para que rootBundle (carga del seed desde assets) y los canales
  // de plataforma funcionen en el entorno de test.
  TestWidgetsFlutterBinding.ensureInitialized();

  // Backend de SQLite por FFI: permite abrir bases reales en memoria
  // (inMemoryDatabasePath) sin emulador ni dispositivo.
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('WordRepository con seed cargado (FFI in-memory)', () {
    late Database db;
    late WordRepository repository;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await _createSchema(db);
      // Carga el seed real desde el asset, igual que AppDatabase.onCreate.
      await const ImpostorWordsSeedLoader().seedIfEmpty(db);

      repository = WordRepository(database: _InMemoryAppDatabase(db));
    });

    tearDown(() async {
      await db.close();
    });

    test('carga el seed y todas las palabras quedan con is_seed = 1', () async {
      final all = await repository.getAll();

      // Debe haber palabras (el seed no está vacío).
      expect(all, isNotEmpty);
      // Todas las palabras iniciales son del seed (solo lectura).
      expect(all.every((w) => w.isSeed), isTrue);
      // Cada palabra del seed tiene id asignado, texto y pista no vacíos.
      for (final word in all) {
        expect(word.id, isNotNull);
        expect(word.word, isNotEmpty);
        expect(word.hint, isNotEmpty);
      }
    });

    test('seedIfEmpty no duplica sobre una base ya poblada', () async {
      final all = await repository.getAll();
      expect(all.where((w) => w.isSeed).length, all.length);

      // Reintentar el seed devuelve 0 (la tabla ya tiene filas) y no duplica.
      final insertedAgain = await const ImpostorWordsSeedLoader().seedIfEmpty(
        db,
      );
      expect(insertedAgain, 0);
      expect((await repository.getAll()).length, all.length);
    });

    test('getAll devuelve un orden estable (alfabético, NOCASE)', () async {
      final all = await repository.getAll();
      final words = all.map((w) => w.word.toLowerCase()).toList();
      final sorted = [...words]..sort();
      expect(words, equals(sorted));
    });

    group('CRUD de palabras de usuario', () {
      test('insert crea una palabra de usuario (is_seed = 0) con id', () async {
        final created = await repository.insert(word: 'zorro', hint: 'astuto');

        expect(created.id, isNotNull);
        expect(created.word, 'zorro');
        expect(created.hint, 'astuto');
        expect(created.isSeed, isFalse);

        // Queda persistida y recuperable por id.
        final fetched = await repository.getById(created.id!);
        expect(fetched, isNotNull);
        expect(fetched!.word, 'zorro');
        expect(fetched.isSeed, isFalse);
      });

      test('insert recorta espacios de word y hint', () async {
        final created = await repository.insert(
          word: '  ardilla  ',
          hint: '  nueces  ',
        );
        expect(created.word, 'ardilla');
        expect(created.hint, 'nueces');
      });

      test('insert con word vacío lanza ArgumentError', () async {
        expect(
          () => repository.insert(word: '   ', hint: 'pista'),
          throwsA(isA<ArgumentError>()),
        );
      });

      test('insert con hint vacío lanza ArgumentError', () async {
        expect(
          () => repository.insert(word: 'palabra', hint: '   '),
          throwsA(isA<ArgumentError>()),
        );
      });

      test('getById devuelve null para un id inexistente', () async {
        expect(await repository.getById(999999), isNull);
      });

      test('getAll incluye la palabra de usuario insertada', () async {
        await repository.insert(word: 'zorro', hint: 'astuto');
        final all = await repository.getAll();
        expect(all.where((w) => w.word == 'zorro'), hasLength(1));
        expect(all.firstWhere((w) => w.word == 'zorro').isSeed, isFalse);
      });

      test('update modifica word y hint de una palabra de usuario', () async {
        final created = await repository.insert(word: 'zorro', hint: 'astuto');
        final updated = await repository.update(
          id: created.id!,
          word: 'lobo',
          hint: 'manada',
        );

        expect(updated.id, created.id);
        expect(updated.word, 'lobo');
        expect(updated.hint, 'manada');
        expect(updated.isSeed, isFalse);

        final fetched = await repository.getById(created.id!);
        expect(fetched!.word, 'lobo');
        expect(fetched.hint, 'manada');
      });

      test('update de un id inexistente lanza WordNotFoundException', () async {
        expect(
          () => repository.update(id: 888888, word: 'x', hint: 'y'),
          throwsA(isA<WordNotFoundException>()),
        );
      });

      test('delete elimina una palabra de usuario', () async {
        final created = await repository.insert(word: 'zorro', hint: 'astuto');
        await repository.delete(created.id!);
        expect(await repository.getById(created.id!), isNull);
      });

      test('delete de un id inexistente lanza WordNotFoundException', () async {
        expect(
          () => repository.delete(777777),
          throwsA(isA<WordNotFoundException>()),
        );
      });
    });

    group('unicidad de word', () {
      test('insert duplicado exacto lanza DuplicateWordException', () async {
        await repository.insert(word: 'zorro', hint: 'astuto');
        expect(
          () => repository.insert(word: 'zorro', hint: 'otra pista'),
          throwsA(isA<DuplicateWordException>()),
        );
      });

      test('insert duplicado contra una palabra del seed falla', () async {
        // 'pirata' está en el seed; insertarla de nuevo debe fallar por la
        // restricción UNIQUE de word.
        expect(
          () => repository.insert(word: 'pirata', hint: 'otra'),
          throwsA(isA<DuplicateWordException>()),
        );
      });

      test(
        'update a un texto ya existente lanza DuplicateWordException',
        () async {
          await repository.insert(word: 'zorro', hint: 'astuto');
          final otra = await repository.insert(word: 'lobo', hint: 'manada');

          expect(
            () => repository.update(id: otra.id!, word: 'zorro', hint: 'm'),
            throwsA(isA<DuplicateWordException>()),
          );
        },
      );
    });

    group('palabras seed son de solo lectura', () {
      test(
        'update de una palabra seed lanza ReadOnlySeedWordException',
        () async {
          final seed = (await repository.getAll()).firstWhere((w) => w.isSeed);

          expect(
            () => repository.update(
              id: seed.id!,
              word: 'cambiada',
              hint: 'nueva',
            ),
            throwsA(isA<ReadOnlySeedWordException>()),
          );

          // La palabra seed no cambió.
          final fetched = await repository.getById(seed.id!);
          expect(fetched!.word, seed.word);
          expect(fetched.hint, seed.hint);
        },
      );

      test(
        'delete de una palabra seed lanza ReadOnlySeedWordException',
        () async {
          final seed = (await repository.getAll()).firstWhere((w) => w.isSeed);

          expect(
            () => repository.delete(seed.id!),
            throwsA(isA<ReadOnlySeedWordException>()),
          );

          // La palabra seed sigue existiendo.
          expect(await repository.getById(seed.id!), isNotNull);
        },
      );

      test(
        'sí se puede editar y borrar una palabra de usuario (is_seed = 0)',
        () async {
          final created = await repository.insert(
            word: 'zorro',
            hint: 'astuto',
          );

          // Editar funciona.
          final updated = await repository.update(
            id: created.id!,
            word: 'zorro2',
            hint: 'astuto2',
          );
          expect(updated.word, 'zorro2');

          // Borrar funciona.
          await repository.delete(created.id!);
          expect(await repository.getById(created.id!), isNull);
        },
      );
    });

    group('search', () {
      test(
        'search encuentra coincidencias sin distinguir mayúsculas',
        () async {
          await repository.insert(word: 'Zorro', hint: 'astuto');
          final results = await repository.search('zorro');
          expect(results.map((w) => w.word), contains('Zorro'));
        },
      );

      test('search por subcadena devuelve las coincidencias', () async {
        await repository.insert(word: 'gatopardo', hint: 'felino');
        final results = await repository.search('opar');
        expect(results.map((w) => w.word), contains('gatopardo'));
      });

      test('search con query vacío devuelve todas las palabras', () async {
        final all = await repository.getAll();
        final results = await repository.search('   ');
        expect(results.length, all.length);
      });

      test(
        'search escapa los comodines de LIKE y los trata literalmente',
        () async {
          await repository.insert(word: '100%natural', hint: 'puro');
          await repository.insert(word: '100xnatural', hint: 'mezcla');

          // '%' debe tratarse literal: solo coincide la que contiene '%'.
          final results = await repository.search('100%natural');
          final palabras = results.map((w) => w.word).toList();
          expect(palabras, contains('100%natural'));
          expect(palabras, isNot(contains('100xnatural')));
        },
      );
    });

    group('unicidad NOCASE (case-insensitive)', () {
      test(
        'insert con distinta capitalización lanza DuplicateWordException',
        () async {
          await repository.insert(word: 'Murcielago', hint: 'cueva');
          expect(
            () => repository.insert(word: 'murcielago', hint: 'otra'),
            throwsA(isA<DuplicateWordException>()),
          );
          expect(
            () => repository.insert(word: 'MURCIELAGO', hint: 'otra'),
            throwsA(isA<DuplicateWordException>()),
          );
        },
      );

      test(
        'update a un texto que solo difiere en mayúsculas también colisiona',
        () async {
          await repository.insert(word: 'Murcielago', hint: 'cueva');
          final otra = await repository.insert(word: 'lobo', hint: 'manada');
          expect(
            () =>
                repository.update(id: otra.id!, word: 'MURCIELAGO', hint: 'm'),
            throwsA(isA<DuplicateWordException>()),
          );
        },
      );
    });
  });

  group('WordRepository sobre base en memoria vacía (FFI)', () {
    // Verifica el CRUD partiendo de una tabla vacía (sin seed).
    late Database db;
    late WordRepository repository;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await _createSchema(db);
      repository = WordRepository(database: _InMemoryAppDatabase(db));
    });

    tearDown(() async {
      await db.close();
    });

    test('CRUD completo funciona sobre base en memoria', () async {
      expect(await repository.getAll(), isEmpty);

      final created = await repository.insert(word: 'mar', hint: 'olas');
      expect(created.id, isNotNull);
      expect(created.isSeed, isFalse);
      expect(await repository.getAll(), hasLength(1));

      final updated = await repository.update(
        id: created.id!,
        word: 'oceano',
        hint: 'profundo',
      );
      expect(updated.word, 'oceano');

      // search encuentra la palabra editada.
      final found = await repository.search('cean');
      expect(found.map((w) => w.word), contains('oceano'));

      await repository.delete(created.id!);
      expect(await repository.getAll(), isEmpty);
    });
  });

  group('ImpostorWordX.toDomain (frontera de datos → dominio)', () {
    test('mapea word/hint y descarta los campos de persistencia', () {
      final source = ImpostorWord(
        id: 42,
        word: 'pirata',
        hint: 'barco',
        isSeed: true,
        createdAt: DateTime.fromMillisecondsSinceEpoch(0),
      );
      expect(source.toDomain(), Word(text: 'pirata', hint: 'barco'));
    });
  });

  group('migración v1 → v2 (UNIQUE COLLATE NOCASE + dedup)', () {
    // Crea el esquema v1 (UNIQUE sensible a mayúsculas) para poder insertar
    // duplicados lógicos antes de migrar.
    Future<Database> openV1() async {
      final db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
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
      return db;
    }

    test(
      'deduplica conservando la seed/más antigua y aplica UNIQUE NOCASE',
      () async {
        // Nota: la base FFI en memoria no persiste entre open/close, así que la
        // migración se ejecuta sobre la MISMA conexión vía onUpgradeForTest,
        // que es exactamente la lógica que AppDatabase pasa a onUpgrade.
        final db = await openV1();

        // 'Pirata' del seed (más antigua) y 'pirata' del usuario (posterior):
        // tras migrar debe quedar solo la del seed.
        await db.insert(kImpostorWordsTable, <String, Object?>{
          'word': 'Pirata',
          'hint': 'barco',
          'is_seed': 1,
          'created_at': 1000,
        });
        await db.insert(kImpostorWordsTable, <String, Object?>{
          'word': 'pirata',
          'hint': 'usuario',
          'is_seed': 0,
          'created_at': 2000,
        });
        // Una palabra sin colisión sobrevive intacta.
        await db.insert(kImpostorWordsTable, <String, Object?>{
          'word': 'gato',
          'hint': 'felino',
          'is_seed': 0,
          'created_at': 1500,
        });

        // Ejecuta la migración v1 -> v2.
        await AppDatabase(
          descriptors: const [ImpostorGame()],
        ).onUpgradeForTest(db, 1, kAppDatabaseVersion);

        final rows = await db.query(
          kImpostorWordsTable,
          orderBy: 'word COLLATE NOCASE ASC',
        );
        final words = rows.map((r) => r['word'] as String).toList();

        // Una sola fila para el grupo 'pirata' y se conserva la del seed.
        expect(words.where((w) => w.toLowerCase() == 'pirata'), hasLength(1));
        final pirata = rows.firstWhere(
          (r) => (r['word'] as String).toLowerCase() == 'pirata',
        );
        expect(pirata['word'], 'Pirata');
        expect(pirata['hint'], 'barco');
        expect(pirata['is_seed'], 1);
        // La palabra sin colisión sigue presente.
        expect(words, contains('gato'));

        // Tras migrar, insertar la misma palabra con otra capitalización falla.
        final repo = WordRepository(database: _InMemoryAppDatabase(db));
        await expectLater(
          () => repo.insert(word: 'PIRATA', hint: 'otra'),
          throwsA(isA<DuplicateWordException>()),
        );

        await db.close();
      },
    );
  });
}
