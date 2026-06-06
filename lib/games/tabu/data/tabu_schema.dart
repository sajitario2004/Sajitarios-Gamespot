/// DDL SQL y helpers de esquema para el bounded context de Tabu.
///
/// Sigue exactamente el mismo patron que [TriviaSchema] e [ImpostorSchema] para
/// que un futuro [GameDescriptor] pueda invocar [TabuSchema.createTables] desde
/// su `onCreateTables` sin cambios aqui.
/// Para los tests de v0.64 se llama directamente sobre una [Database] FFI
/// en memoria — sin [AppDatabase] ni version bump por ahora.
library;

import 'package:sqflite_common/sqlite_api.dart';

/// Nombre de la tabla de palabras de Tabu.
const String kTabuWordsTable = 'tabu_words';

/// Esquema, constantes DDL y helpers de creacion para el juego Tabu.
///
/// Clase [abstract final] para que no pueda instanciarse — mismo patron que
/// [TriviaSchema] e [ImpostorSchema].
abstract final class TabuSchema {
  /// Crea todas las tablas de Tabu en [db] desde cero.
  ///
  /// Adecuado como cuerpo de `GameDescriptor.onCreateTables` una vez conectado.
  static Future<void> createTables(DatabaseExecutor db) async {
    await createWordsTable(db);
  }

  /// Crea la tabla `tabu_words`.
  static Future<void> createWordsTable(DatabaseExecutor db) async {
    await db.execute('''
      CREATE TABLE $kTabuWordsTable (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        palabra TEXT NOT NULL,
        prohibidas_json TEXT NOT NULL,
        is_seed INTEGER NOT NULL DEFAULT 0
      )
    ''');
  }
}
