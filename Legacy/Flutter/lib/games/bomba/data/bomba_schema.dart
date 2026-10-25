/// SQL DDL and schema helpers for the La Bomba bounded context.
///
/// Mirrors the shape of [TriviaSchema] so a future [GameDescriptor] can wire it
/// in via `onCreateTables` / `onUpgradeTables` without changes here.
/// For v0.68 tests the [createTables] helper is called directly on an FFI
/// in-memory [Database] — no [AppDatabase] or version bump needed yet.
library;

import 'package:sqflite_common/sqlite_api.dart';

/// Table name for La Bomba syllable prompts.
const String kBombaSilabasTable = 'bomba_silabas';

/// Table name for La Bomba category prompts.
const String kBombaCategoriasTable = 'bomba_categorias';

/// Schema, DDL constants and creation helpers for La Bomba.
///
/// Kept in an [abstract final class] so it cannot be instantiated —
/// identical pattern to [TriviaSchema].
abstract final class BombaSchema {
  /// Creates all La Bomba tables in [db] from scratch.
  ///
  /// Suitable as the body of `GameDescriptor.onCreateTables` once wired.
  static Future<void> createTables(DatabaseExecutor db) async {
    await createSilabasTable(db);
    await createCategoriasTable(db);
  }

  /// Creates the `bomba_silabas` table.
  static Future<void> createSilabasTable(DatabaseExecutor db) async {
    await db.execute('''
      CREATE TABLE $kBombaSilabasTable (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        silaba TEXT NOT NULL,
        is_seed INTEGER NOT NULL DEFAULT 0
      )
    ''');
  }

  /// Creates the `bomba_categorias` table.
  static Future<void> createCategoriasTable(DatabaseExecutor db) async {
    await db.execute('''
      CREATE TABLE $kBombaCategoriasTable (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        categoria TEXT NOT NULL,
        is_seed INTEGER NOT NULL DEFAULT 0
      )
    ''');
  }
}
