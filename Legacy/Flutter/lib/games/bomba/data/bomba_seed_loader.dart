/// Cargador del seed inicial de prompts de La Bomba (silabas + categorias).
///
/// Sigue exactamente el mismo patrón que [TriviaQuestionsSeedLoader]:
/// lee los JSON desde el bundle de assets, guarda solo si la tabla está vacía
/// (idempotente) e inserta con [is_seed = 1] usando un batch.
library;

import 'dart:convert';

import 'package:flutter/services.dart' show rootBundle;
import 'package:sqflite/sqflite.dart';

import 'package:sajitarios_gamespot/games/bomba/data/bomba_schema.dart';

/// Ruta del asset con las silabas semilla de La Bomba.
const String kBombaSilabasSeedAsset = 'assets/seed/bomba_silabas.json';

/// Ruta del asset con las categorias semilla de La Bomba.
const String kBombaCategoriasSeedAsset = 'assets/seed/bomba_categorias.json';

/// Carga las silabas y categorias semilla de La Bomba la primera vez.
///
/// Cada tabla se semilla de forma independiente e idempotente: si la tabla ya
/// tiene filas no se inserta nada y se devuelve 0 para esa tabla.
class BombaSeedLoader {
  const BombaSeedLoader({
    this.silabasAssetPath = kBombaSilabasSeedAsset,
    this.categoriasAssetPath = kBombaCategoriasSeedAsset,
  });

  final String silabasAssetPath;
  final String categoriasAssetPath;

  /// Inserta las silabas semilla solo si [kBombaSilabasTable] no tiene filas.
  ///
  /// Devuelve el número de silabas insertadas (0 si la tabla ya tenía datos).
  Future<int> seedSilabasIfEmpty(DatabaseExecutor db) async {
    final count =
        Sqflite.firstIntValue(
          await db.rawQuery('SELECT COUNT(*) FROM $kBombaSilabasTable'),
        ) ??
        0;
    if (count > 0) return 0;

    final silabas = await _loadStringArray(silabasAssetPath);
    final batch = db.batch();
    for (final s in silabas) {
      batch.insert(kBombaSilabasTable, <String, Object?>{
        'silaba': s.trim(),
        'is_seed': 1,
      }, conflictAlgorithm: ConflictAlgorithm.ignore);
    }

    final results = await batch.commit(noResult: false);
    return _countInserted(results);
  }

  /// Inserta las categorias semilla solo si [kBombaCategoriasTable] no tiene
  /// filas.
  ///
  /// Devuelve el número de categorias insertadas (0 si la tabla ya tenía datos).
  Future<int> seedCategoriasIfEmpty(DatabaseExecutor db) async {
    final count =
        Sqflite.firstIntValue(
          await db.rawQuery('SELECT COUNT(*) FROM $kBombaCategoriasTable'),
        ) ??
        0;
    if (count > 0) return 0;

    final categorias = await _loadStringArray(categoriasAssetPath);
    final batch = db.batch();
    for (final c in categorias) {
      batch.insert(kBombaCategoriasTable, <String, Object?>{
        'categoria': c.trim(),
        'is_seed': 1,
      }, conflictAlgorithm: ConflictAlgorithm.ignore);
    }

    final results = await batch.commit(noResult: false);
    return _countInserted(results);
  }

  /// Semilla ambas tablas. Devuelve un par (silabasInsertadas, categoriasInsertadas).
  Future<(int, int)> seedAllIfEmpty(DatabaseExecutor db) async {
    final s = await seedSilabasIfEmpty(db);
    final c = await seedCategoriasIfEmpty(db);
    return (s, c);
  }

  // ── Internals ─────────────────────────────────────────────────────────────

  Future<List<String>> _loadStringArray(String assetPath) async {
    final raw = await rootBundle.loadString(assetPath);
    final decoded = jsonDecode(raw) as List<dynamic>;
    final result = <String>[];
    for (var i = 0; i < decoded.length; i++) {
      final element = decoded[i];
      if (element is! String || element.trim().isEmpty) {
        throw FormatException(
          'Element at index $i in $assetPath is not a non-empty String: $element',
        );
      }
      result.add(element.trim());
    }
    return result;
  }

  int _countInserted(List<Object?> results) {
    var inserted = 0;
    for (final result in results) {
      if (result is int && result > 0) inserted++;
    }
    return inserted;
  }
}
