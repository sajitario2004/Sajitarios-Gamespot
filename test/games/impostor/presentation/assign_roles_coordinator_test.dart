import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/assign_roles_use_case.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';

/// [AppDatabase] de prueba que expone una [Database] ya abierta (en memoria).
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

GameConfig _config({int nImpostores = 1, bool hintEnabled = false}) {
  return GameConfig.create(
    players: const [Player('Nacho'), Player('Iker'), Player('Lucia')],
    nImpostores: nImpostores,
    hintEnabled: hintEnabled,
  ).config!;
}

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('AssignRolesCoordinator (real) sobre WordRepository FFI in-memory', () {
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

    AssignRolesCoordinator coordinator(int seed) => AssignRolesCoordinator(
      repository: repository,
      useCase: AssignRolesUseCase(RandomProvider.seeded(seed)),
    );

    test(
      'arma una partida usando una palabra real de la BD (glue BD->dominio)',
      () async {
        // Una sola palabra: el pick del use case está forzado a ella, lo que
        // verifica el mapeo ImpostorWord -> Word en la frontera de datos.
        await repository.insert(word: 'pirata', hint: 'barco');

        final session = await coordinator(1).assign(_config());

        // La palabra de dominio proviene de la fila de la BD (text + hint).
        expect(session.word.text, 'pirata');
        expect(session.word.hint, 'barco');
        // El orden de revelación es el de introducción.
        expect(session.revealOrder.map((p) => p.name), [
          'Nacho',
          'Iker',
          'Lucia',
        ]);
        // Cada jugador tiene rol asignado.
        expect(session.assignments.length, 3);
      },
    );

    test('elige solo entre las palabras de la BD', () async {
      await repository.insert(word: 'unica', hint: 'sola');

      final session = await coordinator(7).assign(_config());
      expect(session.word.text, 'unica');
      expect(session.word.hint, 'sola');
    });

    test('con BD vacía lanza NoWordsAvailableException', () async {
      expect(await repository.getAll(), isEmpty);
      await expectLater(
        () => coordinator(1).assign(_config()),
        throwsA(isA<NoWordsAvailableException>()),
      );
    });
  });
}
