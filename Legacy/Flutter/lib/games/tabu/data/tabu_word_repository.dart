/// Capa de acceso a datos para palabras de Tabu.
///
/// [TabuWordRepository] opera sobre la tabla `tabu_words`. Mapea filas de BD
/// <-> objetos de dominio [TabuWord] usando [dart:convert] para la columna
/// `prohibidas_json`. La seleccion aleatoria queda en el caso de uso de dominio
/// ([PickTabuWordUseCase]); este repo solo devuelve colecciones.
library;

import 'dart:convert';

import 'package:sqflite_common/sqlite_api.dart';

import 'package:sajitarios_gamespot/games/tabu/data/tabu_schema.dart';
import 'package:sajitarios_gamespot/games/tabu/domain/tabu_word.dart';

/// Persiste y consulta palabras de Tabu en la tabla `tabu_words`.
class TabuWordRepository {
  const TabuWordRepository(this._db);

  final DatabaseExecutor _db;

  // ── Escritura ─────────────────────────────────────────────────────────────

  /// Inserta una [word] y la devuelve con su [TabuWord.id] asignado.
  Future<TabuWord> insert(TabuWord word) async {
    final id = await _db.insert(kTabuWordsTable, _toRow(word, isSeed: false));
    return _withId(word, id);
  }

  /// Inserta todas las [words] en un batch unico.
  Future<List<TabuWord>> bulkInsert(List<TabuWord> words) async {
    final batch = _db.batch();
    for (final w in words) {
      batch.insert(kTabuWordsTable, _toRow(w, isSeed: false));
    }
    final results = await batch.commit(noResult: false);
    return [
      for (var i = 0; i < words.length; i++)
        _withId(words[i], results[i] as int),
    ];
  }

  // ── Lectura ───────────────────────────────────────────────────────────────

  /// Numero total de palabras en la tabla.
  Future<int> count() async {
    final rows = await _db.rawQuery(
      'SELECT COUNT(*) AS c FROM $kTabuWordsTable',
    );
    return (rows.first['c'] as int?) ?? 0;
  }

  /// Devuelve todas las palabras ordenadas por id ascendente.
  Future<List<TabuWord>> getAll() async {
    final rows = await _db.rawQuery(
      'SELECT * FROM $kTabuWordsTable ORDER BY id ASC',
    );
    return rows.map(_fromRow).toList(growable: false);
  }

  // ── Mapeo ─────────────────────────────────────────────────────────────────

  /// Convierte una fila de BD en un [TabuWord] de dominio.
  static TabuWord _fromRow(Map<String, Object?> row) {
    final prohibidasRaw =
        jsonDecode(row['prohibidas_json'] as String) as List<dynamic>;
    return TabuWord.create(
      id: row['id'] as int,
      palabra: row['palabra'] as String,
      prohibidas: prohibidasRaw.cast<String>(),
    );
  }

  /// Convierte un [TabuWord] de dominio en un mapa de fila de BD.
  static Map<String, Object?> _toRow(TabuWord w, {required bool isSeed}) {
    return <String, Object?>{
      'palabra': w.palabra,
      'prohibidas_json': jsonEncode(w.prohibidas),
      'is_seed': isSeed ? 1 : 0,
    };
  }

  /// Devuelve un [TabuWord] identico a [w] pero con [id] asignado.
  static TabuWord _withId(TabuWord w, int id) => TabuWord.create(
    id: id,
    palabra: w.palabra,
    prohibidas: w.prohibidas.toList(),
  );
}
