/// Cargador del seed inicial de palabras de Tabu.
///
/// Sigue exactamente el mismo patron que [TriviaQuestionsSeedLoader] e
/// [ImpostorWordsSeedLoader]: lee el JSON desde el bundle de assets, guarda
/// solo si la tabla esta vacia (idempotente) e inserta con [is_seed = 1]
/// usando un batch.
library;

import 'dart:convert';

import 'package:flutter/services.dart' show rootBundle;
import 'package:sqflite/sqflite.dart';

import 'package:sajitarios_gamespot/games/tabu/data/tabu_schema.dart';

/// Ruta del asset con las palabras semilla de Tabu.
const String kTabuWordsSeedAsset = 'assets/seed/tabu_words.json';

/// Una palabra del seed leida desde JSON, antes de mapearla a la BD.
class _SeedWord {
  const _SeedWord({required this.palabra, required this.prohibidas});

  /// Construye un [_SeedWord] desde un mapa JSON.
  ///
  /// Lanza [FormatException] si algun campo obligatorio esta ausente o invalido.
  factory _SeedWord.fromJson(Map<String, dynamic> json) {
    final palabra = (json['palabra'] as String?)?.trim();
    final prohibidasRaw = json['prohibidas'];

    if (palabra == null || palabra.isEmpty) {
      throw FormatException('Campo "palabra" faltante o vacio: $json');
    }
    if (prohibidasRaw is! List ||
        prohibidasRaw.length < 4 ||
        prohibidasRaw.length > 5) {
      throw FormatException(
        '"prohibidas" debe ser una lista de 4 o 5 elementos: $json',
      );
    }
    final prohibidas = prohibidasRaw.cast<String>();
    for (final p in prohibidas) {
      if (p.trim().isEmpty) {
        throw FormatException('Palabra prohibida vacia en: $json');
      }
    }

    return _SeedWord(palabra: palabra, prohibidas: prohibidas);
  }

  final String palabra;
  final List<String> prohibidas;
}

/// Carga las palabras semilla de Tabu en la base la primera vez.
///
/// Lee el JSON de [kTabuWordsSeedAsset] via [rootBundle] y, solo si la tabla
/// [kTabuWordsTable] esta vacia, inserta cada palabra con [is_seed = 1]. Las
/// palabras seed son de solo lectura para la app.
class TabuWordsSeedLoader {
  const TabuWordsSeedLoader({this.assetPath = kTabuWordsSeedAsset});

  final String assetPath;

  /// Inserta el seed solo si la tabla [kTabuWordsTable] no tiene filas.
  ///
  /// Devuelve el numero de palabras insertadas (0 si la tabla ya tenia datos).
  Future<int> seedIfEmpty(DatabaseExecutor db) async {
    final count =
        Sqflite.firstIntValue(
          await db.rawQuery('SELECT COUNT(*) FROM $kTabuWordsTable'),
        ) ??
        0;
    if (count > 0) {
      return 0;
    }

    final words = await _loadSeedWords();
    final batch = db.batch();

    for (final w in words) {
      batch.insert(kTabuWordsTable, <String, Object?>{
        'palabra': w.palabra,
        'prohibidas_json': jsonEncode(w.prohibidas),
        'is_seed': 1,
      }, conflictAlgorithm: ConflictAlgorithm.ignore);
    }

    final results = await batch.commit(noResult: false);
    var inserted = 0;
    for (final result in results) {
      if (result is int && result > 0) {
        inserted++;
      }
    }
    return inserted;
  }

  Future<List<_SeedWord>> _loadSeedWords() async {
    final raw = await rootBundle.loadString(assetPath);
    final decoded = jsonDecode(raw) as List<dynamic>;
    return decoded
        .cast<Map<String, dynamic>>()
        .map(_SeedWord.fromJson)
        .toList(growable: false);
  }
}
