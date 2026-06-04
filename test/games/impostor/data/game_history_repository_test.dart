import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/impostor/impostor_game.dart';
import 'package:sajitarios_gamespot/games/impostor/data/game_history_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/data/impostor_schema.dart'
    show kGameHistoryTable;
import 'package:sajitarios_gamespot/games/impostor/data/game_record.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/role.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/word.dart';

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

/// Crea la tabla `game_history` igual que [AppDatabase] v3 en producción.
Future<void> _createGameHistorySchema(Database db) async {
  await db.execute('''
    CREATE TABLE $kGameHistoryTable (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      created_at INTEGER NOT NULL,
      word TEXT NOT NULL,
      hint TEXT,
      n_players INTEGER NOT NULL,
      n_impostors INTEGER NOT NULL,
      hint_enabled INTEGER NOT NULL,
      players_json TEXT NOT NULL
    )
  ''');
  await db.execute(
    'CREATE INDEX idx_game_history_created_at '
    'ON $kGameHistoryTable(created_at)',
  );
}

/// Construye una [GameSession] con la palabra y los jugadores/impostores dados.
///
/// Los nombres en [impostores] reciben el rol impostor; el resto, palabra.
GameSession _session({
  required String word,
  required String hint,
  required List<String> players,
  required Set<String> impostores,
}) {
  final lista = players.map(Player.new).toList();
  final assignments = <Player, Role>{
    for (final p in lista)
      p: impostores.contains(p.name) ? Role.impostor : Role.palabra,
  };
  return GameSession(
    word: Word(text: word, hint: hint),
    players: lista,
    assignments: assignments,
  );
}

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('GameHistoryRepository (FFI in-memory)', () {
    late Database db;
    late GameHistoryRepository repository;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await _createGameHistorySchema(db);
      repository = GameHistoryRepository(database: _InMemoryAppDatabase(db));
    });

    tearDown(() async {
      await db.close();
    });

    test('insertFromSession guarda y getAll lo recupera con id', () async {
      final session = _session(
        word: 'playa',
        hint: 'arena',
        players: ['Nacho', 'Iker', 'Lucia'],
        impostores: {'Nacho'},
      );

      final inserted = await repository.insertFromSession(
        session,
        hintEnabled: true,
      );

      expect(inserted.id, isNotNull);
      expect(inserted.word, 'playa');
      expect(inserted.nPlayers, 3);
      expect(inserted.nImpostors, 1);
      expect(inserted.hintEnabled, isTrue);

      final all = await repository.getAll();
      expect(all, hasLength(1));
      final record = all.first;
      expect(record.id, inserted.id);
      expect(record.word, 'playa');
      expect(record.hint, 'arena');
      // Los jugadores se conservan en orden de revelación con su rol.
      expect(record.players.map((p) => p.name), ['Nacho', 'Iker', 'Lucia']);
      expect(record.players[0].wasImpostor, isTrue);
      expect(record.players[1].wasImpostor, isFalse);
      expect(record.players[2].wasImpostor, isFalse);
    });

    test(
      'getAll devuelve las partidas de más reciente a más antigua',
      () async {
        await repository.insertFromSession(
          _session(
            word: 'vieja',
            hint: 'h',
            players: ['A', 'B', 'C'],
            impostores: {'A'},
          ),
          hintEnabled: false,
          createdAt: DateTime.fromMillisecondsSinceEpoch(1000),
        );
        await repository.insertFromSession(
          _session(
            word: 'nueva',
            hint: 'h',
            players: ['A', 'B', 'C'],
            impostores: {'B'},
          ),
          hintEnabled: false,
          createdAt: DateTime.fromMillisecondsSinceEpoch(5000),
        );

        final all = await repository.getAll();
        expect(all.map((r) => r.word), ['nueva', 'vieja']);
      },
    );

    test('deleteAll borra todo el historial', () async {
      await repository.insertFromSession(
        _session(
          word: 'a',
          hint: 'h',
          players: ['A', 'B', 'C'],
          impostores: {'A'},
        ),
        hintEnabled: false,
      );
      await repository.insertFromSession(
        _session(
          word: 'b',
          hint: 'h',
          players: ['A', 'B', 'C'],
          impostores: {'B'},
        ),
        hintEnabled: false,
      );

      expect(await repository.count(), 2);
      final borradas = await repository.deleteAll();
      expect(borradas, 2);
      expect(await repository.count(), 0);
      expect(await repository.getAll(), isEmpty);
    });

    group('agregados para estadísticas', () {
      test('count refleja el total de partidas', () async {
        expect(await repository.count(), 0);
        await repository.insertFromSession(
          _session(
            word: 'x',
            hint: 'h',
            players: ['A', 'B', 'C'],
            impostores: {'A'},
          ),
          hintEnabled: false,
        );
        expect(await repository.count(), 1);
      });

      test('mostFrequentWord devuelve la palabra más repetida', () async {
        for (var i = 0; i < 3; i++) {
          await repository.insertFromSession(
            _session(
              word: 'pirata',
              hint: 'barco',
              players: ['A', 'B', 'C'],
              impostores: {'A'},
            ),
            hintEnabled: false,
          );
        }
        await repository.insertFromSession(
          _session(
            word: 'playa',
            hint: 'arena',
            players: ['A', 'B', 'C'],
            impostores: {'B'},
          ),
          hintEnabled: false,
        );

        final top = await repository.mostFrequentWord();
        expect(top, isNotNull);
        expect(top!.word, 'pirata');
        expect(top.count, 3);
      });

      test('mostFrequentWord agrupa sin distinguir mayúsculas', () async {
        await repository.insertFromSession(
          _session(
            word: 'Pirata',
            hint: 'h',
            players: ['A', 'B', 'C'],
            impostores: {'A'},
          ),
          hintEnabled: false,
        );
        await repository.insertFromSession(
          _session(
            word: 'pirata',
            hint: 'h',
            players: ['A', 'B', 'C'],
            impostores: {'B'},
          ),
          hintEnabled: false,
        );

        final top = await repository.mostFrequentWord();
        expect(top, isNotNull);
        expect(top!.count, 2);
      });

      test('mostFrequentWord es null con historial vacío', () async {
        expect(await repository.mostFrequentWord(), isNull);
      });

      test('impostorCountsByPlayer cuenta impostores por nombre', () async {
        // Nacho impostor en dos partidas, Iker en una, Lucia nunca.
        await repository.insertFromSession(
          _session(
            word: 'a',
            hint: 'h',
            players: ['Nacho', 'Iker', 'Lucia'],
            impostores: {'Nacho'},
          ),
          hintEnabled: false,
        );
        await repository.insertFromSession(
          _session(
            word: 'b',
            hint: 'h',
            players: ['Nacho', 'Iker', 'Lucia'],
            impostores: {'Nacho', 'Iker'},
          ),
          hintEnabled: false,
        );

        final counts = await repository.impostorCountsByPlayer();
        expect(counts['Nacho'], 2);
        expect(counts['Iker'], 1);
        // Quien nunca fue impostor no aparece en el mapa.
        expect(counts.containsKey('Lucia'), isFalse);
      });
    });

    test('GameRecord round-trip toMap/fromMap conserva los datos', () async {
      final session = _session(
        word: 'playa',
        hint: 'arena',
        players: ['Nacho', 'Iker', 'Lucia'],
        impostores: {'Iker'},
      );
      final record = GameRecord.fromSession(
        session,
        hintEnabled: true,
        createdAt: DateTime.fromMillisecondsSinceEpoch(12345),
      );
      final back = GameRecord.fromMap(record.toMap());

      expect(back.word, 'playa');
      expect(back.hint, 'arena');
      expect(back.nPlayers, 3);
      expect(back.nImpostors, 1);
      expect(back.hintEnabled, isTrue);
      expect(back.createdAt, DateTime.fromMillisecondsSinceEpoch(12345));
      expect(back.players, record.players);
    });
  });

  group('migración v2 → v3 (crea game_history)', () {
    test('onUpgradeForTest v2->v3 crea la tabla game_history', () async {
      final db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);

      // Esquema v2: solo impostor_words (sin game_history todavía).
      await db.execute('''
        CREATE TABLE impostor_words (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          word TEXT NOT NULL UNIQUE COLLATE NOCASE,
          hint TEXT NOT NULL,
          is_seed INTEGER NOT NULL DEFAULT 0,
          created_at INTEGER NOT NULL
        )
      ''');

      // La tabla game_history no existe aún.
      final antes = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        <Object?>[kGameHistoryTable],
      );
      expect(antes, isEmpty);

      // Migra v2 -> v3.
      await AppDatabase(
        descriptors: const [ImpostorGame()],
      ).onUpgradeForTest(db, 2, kAppDatabaseVersion);

      final despues = await db.rawQuery(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=?",
        <Object?>[kGameHistoryTable],
      );
      expect(despues, hasLength(1));

      // La tabla es usable por el repositorio tras migrar.
      final repo = GameHistoryRepository(database: _InMemoryAppDatabase(db));
      await repo.insertFromSession(
        _session(
          word: 'migrada',
          hint: 'h',
          players: ['A', 'B', 'C'],
          impostores: {'A'},
        ),
        hintEnabled: false,
      );
      expect(await repo.count(), 1);

      await db.close();
    });
  });
}
