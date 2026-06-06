/// SQL DDL and schema helpers for the Wavelength bounded context.
///
/// Mirrors the shape of [TriviaSchema] so a future [GameDescriptor] can wire
/// it in via `onCreateTables` / `onUpgradeTables` without changes here.
/// For v0.60 tests [createTables] is called directly on an FFI in-memory
/// [Database] — no [AppDatabase] or version bump needed yet.
library;

import 'package:sqflite_common/sqlite_api.dart';

/// Table name for Wavelength spectra.
const String kWavelengthSpectraTable = 'wavelength_spectra';

/// Schema, DDL constants and creation helpers for the Wavelength game.
///
/// Kept in an [abstract final class] so it cannot be instantiated —
/// identical pattern to [TriviaSchema] and [ImpostorSchema].
abstract final class WavelengthSchema {
  /// Creates all Wavelength tables in [db] from scratch.
  ///
  /// Suitable as the body of `GameDescriptor.onCreateTables` once wired.
  static Future<void> createTables(DatabaseExecutor db) async {
    await createSpectraTable(db);
  }

  /// Creates the `wavelength_spectra` table.
  static Future<void> createSpectraTable(DatabaseExecutor db) async {
    await db.execute('''
      CREATE TABLE $kWavelengthSpectraTable (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        izquierda TEXT NOT NULL,
        derecha TEXT NOT NULL,
        is_seed INTEGER NOT NULL DEFAULT 0
      )
    ''');
  }
}
