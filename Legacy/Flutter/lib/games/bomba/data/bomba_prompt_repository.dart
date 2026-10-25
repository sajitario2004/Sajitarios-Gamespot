/// Data access layer for La Bomba prompts (silabas + categorias).
///
/// [BombaPromptRepository] operates over [kBombaSilabasTable] and
/// [kBombaCategoriasTable]. It maps database rows to domain [BombaPrompt]
/// objects. Random selection stays in the domain use-case
/// ([PickPromptUseCase]); this repo only returns full pools.
library;

import 'package:sqflite_common/sqlite_api.dart';

import 'package:sajitarios_gamespot/games/bomba/data/bomba_schema.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_mode.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_prompt.dart';

/// Persists and queries La Bomba prompts in the two prompt tables.
class BombaPromptRepository {
  const BombaPromptRepository(this._db);

  final DatabaseExecutor _db;

  // ── Silabas ──────────────────────────────────────────────────────────────

  /// Inserts a single syllable [texto] and returns the resulting [BombaPrompt].
  Future<BombaPrompt> insertSilaba(String texto, {bool isSeed = false}) async {
    final id = await _db.insert(kBombaSilabasTable, <String, Object?>{
      'silaba': texto.trim(),
      'is_seed': isSeed ? 1 : 0,
    });
    return BombaPrompt.create(id: id, texto: texto, mode: BombaMode.silaba);
  }

  /// Inserts all syllables in [textos] and returns the resulting prompts.
  Future<List<BombaPrompt>> bulkInsertSilabas(
    List<String> textos, {
    bool isSeed = false,
  }) async {
    final result = <BombaPrompt>[];
    for (final t in textos) {
      result.add(await insertSilaba(t, isSeed: isSeed));
    }
    return result;
  }

  /// Total number of syllable rows in the table.
  Future<int> countSilabas() async {
    final rows = await _db.rawQuery(
      'SELECT COUNT(*) AS c FROM $kBombaSilabasTable',
    );
    return (rows.first['c'] as int?) ?? 0;
  }

  /// Returns all syllable prompts ordered by id.
  Future<List<BombaPrompt>> getAllSilabas() async {
    final rows = await _db.rawQuery(
      'SELECT * FROM $kBombaSilabasTable ORDER BY id ASC',
    );
    return rows.map(_silabaFromRow).toList(growable: false);
  }

  // ── Categorias ───────────────────────────────────────────────────────────

  /// Inserts a single category [texto] and returns the resulting [BombaPrompt].
  Future<BombaPrompt> insertCategoria(
    String texto, {
    bool isSeed = false,
  }) async {
    final id = await _db.insert(kBombaCategoriasTable, <String, Object?>{
      'categoria': texto.trim(),
      'is_seed': isSeed ? 1 : 0,
    });
    return BombaPrompt.create(id: id, texto: texto, mode: BombaMode.categoria);
  }

  /// Inserts all categories in [textos] and returns the resulting prompts.
  Future<List<BombaPrompt>> bulkInsertCategorias(
    List<String> textos, {
    bool isSeed = false,
  }) async {
    final result = <BombaPrompt>[];
    for (final t in textos) {
      result.add(await insertCategoria(t, isSeed: isSeed));
    }
    return result;
  }

  /// Total number of category rows in the table.
  Future<int> countCategorias() async {
    final rows = await _db.rawQuery(
      'SELECT COUNT(*) AS c FROM $kBombaCategoriasTable',
    );
    return (rows.first['c'] as int?) ?? 0;
  }

  /// Returns all category prompts ordered by id.
  Future<List<BombaPrompt>> getAllCategorias() async {
    final rows = await _db.rawQuery(
      'SELECT * FROM $kBombaCategoriasTable ORDER BY id ASC',
    );
    return rows.map(_categoriaFromRow).toList(growable: false);
  }

  // ── Mapping ───────────────────────────────────────────────────────────────

  static BombaPrompt _silabaFromRow(Map<String, Object?> row) =>
      BombaPrompt.create(
        id: row['id'] as int,
        texto: row['silaba'] as String,
        mode: BombaMode.silaba,
      );

  static BombaPrompt _categoriaFromRow(Map<String, Object?> row) =>
      BombaPrompt.create(
        id: row['id'] as int,
        texto: row['categoria'] as String,
        mode: BombaMode.categoria,
      );
}
