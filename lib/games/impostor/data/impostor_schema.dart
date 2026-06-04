import 'package:sqflite_common/sqlite_api.dart';

import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart';
import 'package:sajitarios_gamespot/core/db/impostor_words_seed_loader.dart';

/// Nombre de la tabla que guarda el historial de partidas del Impostor.
const String kGameHistoryTable = 'game_history';

/// Esquema, migraciones y seed de la base de datos del juego El Impostor.
///
/// Vive dentro del bounded context del Impostor (no en `core/db`): `core`
/// orquesta la apertura/versionado genérico y delega aquí la aportación del
/// esquema vía `GameDescriptor.onCreateTables`/`onUpgradeTables`. De este modo
/// `core` no depende de ningún juego concreto.
///
/// El comportamiento de BD y las migraciones son idénticos a los que antes
/// vivían en `AppDatabase`.
abstract final class ImpostorSchema {
  /// Crea el esquema del Impostor desde cero (primer arranque) y carga el seed.
  static Future<void> onCreate(
    DatabaseExecutor db, {
    ImpostorWordsSeedLoader seedLoader = const ImpostorWordsSeedLoader(),
  }) async {
    await createImpostorWordsTable(db);
    await createGameHistoryTable(db);
    await seedLoader.seedIfEmpty(db);
  }

  /// Aplica las migraciones incrementales del Impostor para la versión [v].
  ///
  /// `AppDatabase` recorre las versiones [oldVersion]+1..[newVersion] y llama
  /// aquí por cada una; este método aplica la migración correspondiente a [v].
  static Future<void> onUpgradeStep(DatabaseExecutor db, int v) async {
    switch (v) {
      case 2:
        await _migrateToV2NoCase(db);
        break;
      case 3:
        await _migrateToV3GameHistory(db);
        break;
      default:
        break;
    }
  }

  /// Migración v1 → v2: unicidad de `word` case-insensitive.
  ///
  /// En v1 la columna `word` era `UNIQUE` sensible a mayúsculas, así que podían
  /// coexistir "Pirata" y "pirata". En v2 pasa a `UNIQUE COLLATE NOCASE`. SQLite
  /// no permite alterar la colación de una columna in situ, así que recreamos la
  /// tabla con el nuevo esquema, **deduplicando** las palabras que solo difieren
  /// en mayúsculas/minúsculas y conservando una sola por cada grupo.
  ///
  /// Criterio de desempate al deduplicar: se conserva la fila preferentemente
  /// del seed (`is_seed = 1`) y, dentro de eso, la más antigua (`created_at` y
  /// luego `id` menores). Así "Pirata" del seed gana frente a un "pirata" de
  /// usuario añadido después.
  static Future<void> _migrateToV2NoCase(DatabaseExecutor db) async {
    Future<void> run(DatabaseExecutor txn) async {
      // 1) Tabla nueva con la colación NOCASE en la restricción UNIQUE.
      await txn.execute('''
        CREATE TABLE ${kImpostorWordsTable}_v2 (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          word TEXT NOT NULL UNIQUE COLLATE NOCASE,
          hint TEXT NOT NULL,
          is_seed INTEGER NOT NULL DEFAULT 0,
          created_at INTEGER NOT NULL
        )
      ''');

      // 2) Copiar deduplicando NOCASE: por cada grupo de palabras iguales
      // ignorando mayúsculas, quedarse con una sola fila. Preferimos las del
      // seed (is_seed DESC) y, a igualdad, la más antigua (created_at, id ASC).
      await txn.execute('''
        INSERT INTO ${kImpostorWordsTable}_v2 (id, word, hint, is_seed, created_at)
        SELECT id, word, hint, is_seed, created_at
        FROM $kImpostorWordsTable AS w
        WHERE id = (
          SELECT id FROM $kImpostorWordsTable AS w2
          WHERE w2.word = w.word COLLATE NOCASE
          ORDER BY w2.is_seed DESC, w2.created_at ASC, w2.id ASC
          LIMIT 1
        )
      ''');

      // 3) Reemplazar la tabla vieja por la nueva.
      await txn.execute('DROP TABLE $kImpostorWordsTable');
      await txn.execute(
        'ALTER TABLE ${kImpostorWordsTable}_v2 RENAME TO $kImpostorWordsTable',
      );

      // 4) Recrear el índice de búsqueda/orden por `word`.
      await txn.execute(
        'CREATE INDEX idx_impostor_words_word ON $kImpostorWordsTable(word)',
      );
    }

    if (db is Database) {
      await db.transaction(run);
    } else {
      await run(db);
    }
  }

  /// Migración v2 → v3: añade la tabla `game_history`.
  ///
  /// Persiste el resultado de cada partida del Impostor (fecha, palabra, pista,
  /// nº de jugadores/impostores, si la pista estaba activa y la lista de
  /// jugadores con su rol serializada en JSON). No toca `impostor_words`.
  static Future<void> _migrateToV3GameHistory(DatabaseExecutor db) async {
    await createGameHistoryTable(db);
  }

  static Future<void> createGameHistoryTable(DatabaseExecutor db) async {
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

  static Future<void> createImpostorWordsTable(DatabaseExecutor db) async {
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
}
