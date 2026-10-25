/// DDL y helpers de esquema para el bounded context Yo Nunca.
///
/// Sigue la misma forma que [TriviaSchema] e [ImpostorSchema]: expone
/// [createTable] para que un futuro [GameDescriptor] lo llame desde
/// `onCreateTables` / `onUpgradeTables` sin cambios aquí.
/// Para los tests de v0.66 el helper se llama directamente sobre una [Database]
/// FFI en memoria — sin [AppDatabase] ni bump de versión todavía.
library;

import 'package:sqflite_common/sqlite_api.dart';

/// Nombre de la tabla de declaraciones "Yo nunca".
const String kYoNuncaStatementsTable = 'yo_nunca_statements';

/// Esquema, constantes DDL y helpers de creación para el juego Yo Nunca.
///
/// Clase [abstract final] para que no pueda instanciarse —
/// patrón idéntico a [TriviaSchema] e [ImpostorSchema].
abstract final class YoNuncaSchema {
  /// Crea todas las tablas de Yo Nunca en [db] desde cero.
  ///
  /// Apto como cuerpo de `GameDescriptor.onCreateTables` una vez cableado.
  static Future<void> createTables(DatabaseExecutor db) async {
    await createStatementsTable(db);
  }

  /// Crea la tabla `yo_nunca_statements` y su índice por intensidad.
  static Future<void> createStatementsTable(DatabaseExecutor db) async {
    await db.execute('''
      CREATE TABLE $kYoNuncaStatementsTable (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        frase TEXT NOT NULL,
        intensidad TEXT NOT NULL,
        is_seed INTEGER NOT NULL DEFAULT 0
      )
    ''');
    await db.execute(
      'CREATE INDEX idx_yo_nunca_statements_intensidad '
      'ON $kYoNuncaStatementsTable(intensidad)',
    );
  }
}
