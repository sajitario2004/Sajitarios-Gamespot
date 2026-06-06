/// Capa de acceso a datos para las declaraciones "Yo nunca".
///
/// [NeverStatementRepository] opera sobre la tabla [kYoNuncaStatementsTable].
/// Mapea filas de la BD ↔ objetos de dominio [NeverStatement].
/// La aleatoriedad queda en el use-case ([DrawStatementUseCase]); este
/// repositorio solo devuelve pools.
library;

import 'package:sqflite_common/sqlite_api.dart';

import 'package:sajitarios_gamespot/games/yo_nunca/data/yo_nunca_schema.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/intensidad.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/never_statement.dart';

/// Persiste y consulta declaraciones "Yo nunca" en la tabla
/// [kYoNuncaStatementsTable].
class NeverStatementRepository {
  const NeverStatementRepository(this._db);

  final DatabaseExecutor _db;

  // ── Escritura ────────────────────────────────────────────────────────────

  /// Inserta una [statement] y devuelve la misma con su [NeverStatement.id]
  /// asignado por la base de datos.
  Future<NeverStatement> insert(NeverStatement statement) async {
    final id = await _db.insert(
      kYoNuncaStatementsTable,
      _toRow(statement, isSeed: false),
    );
    return NeverStatement.create(
      id: id,
      frase: statement.frase,
      intensidad: statement.intensidad,
    );
  }

  /// Inserta todas las [statements] en un batch único.
  ///
  /// Devuelve la lista con los ids asignados por la base de datos.
  Future<List<NeverStatement>> bulkInsert(
    List<NeverStatement> statements,
  ) async {
    final batch = _db.batch();
    for (final s in statements) {
      batch.insert(kYoNuncaStatementsTable, _toRow(s, isSeed: false));
    }
    final results = await batch.commit(noResult: false);
    return [
      for (var i = 0; i < statements.length; i++)
        NeverStatement.create(
          id: results[i] as int,
          frase: statements[i].frase,
          intensidad: statements[i].intensidad,
        ),
    ];
  }

  // ── Lectura ──────────────────────────────────────────────────────────────

  /// Número total de declaraciones en la tabla.
  Future<int> count() async {
    final rows = await _db.rawQuery(
      'SELECT COUNT(*) AS c FROM $kYoNuncaStatementsTable',
    );
    return (rows.first['c'] as int?) ?? 0;
  }

  /// Todas las declaraciones que pertenezcan a alguna de las [intensidades].
  ///
  /// El orden es estable (por id ascendente) para que los tests sean
  /// predecibles.
  Future<List<NeverStatement>> getByIntensidades(
    Set<Intensidad> intensidades,
  ) async {
    if (intensidades.isEmpty) return const [];

    final placeholders = List<String>.filled(
      intensidades.length,
      '?',
    ).join(',');
    final rows = await _db.rawQuery(
      'SELECT * FROM $kYoNuncaStatementsTable '
      'WHERE intensidad IN ($placeholders) '
      'ORDER BY id ASC',
      intensidades.map((i) => i.name).toList(),
    );
    return rows.map(_fromRow).toList(growable: false);
  }

  /// Todas las declaraciones de la tabla, ordenadas por id ascendente.
  Future<List<NeverStatement>> getAll() async {
    final rows = await _db.rawQuery(
      'SELECT * FROM $kYoNuncaStatementsTable ORDER BY id ASC',
    );
    return rows.map(_fromRow).toList(growable: false);
  }

  // ── Mapeo ────────────────────────────────────────────────────────────────

  static NeverStatement _fromRow(Map<String, Object?> row) {
    return NeverStatement.create(
      id: row['id'] as int,
      frase: row['frase'] as String,
      intensidad: Intensidad.values.byName(row['intensidad'] as String),
    );
  }

  static Map<String, Object?> _toRow(NeverStatement s, {required bool isSeed}) {
    return <String, Object?>{
      'frase': s.frase,
      'intensidad': s.intensidad.name,
      'is_seed': isSeed ? 1 : 0,
    };
  }
}
