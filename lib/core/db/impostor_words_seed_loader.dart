import 'dart:convert';

import 'package:flutter/services.dart' show rootBundle;
import 'package:sqflite/sqflite.dart';

/// Ruta del asset con las palabras semilla del Impostor.
const String kImpostorWordsSeedAsset = 'assets/seed/impostor_words.json';

/// Una palabra del seed inicial: `word` y su `hint` (pista), ambas obligatorias.
///
/// Toda palabra del juego lleva pista asociada (regla del Impostor): el JSON de
/// seed debe traer siempre las dos claves.
class SeedWord {
  const SeedWord({required this.word, required this.hint});

  /// Construye una [SeedWord] desde un mapa JSON.
  ///
  /// Lanza [FormatException] si faltan las claves `word`/`hint` o si están
  /// vacías tras recortar espacios.
  factory SeedWord.fromJson(Map<String, dynamic> json) {
    final word = (json['word'] as String?)?.trim();
    final hint = (json['hint'] as String?)?.trim();
    if (word == null || word.isEmpty || hint == null || hint.isEmpty) {
      throw FormatException('Palabra de seed inválida: $json');
    }
    return SeedWord(word: word, hint: hint);
  }

  final String word;
  final String hint;
}

/// Carga las palabras semilla del Impostor en la base la primera vez.
///
/// Lee el JSON de `assets/seed/impostor_words.json` vía [rootBundle] y, solo si
/// la tabla `impostor_words` está vacía, inserta cada palabra con `is_seed = 1`
/// y `created_at` (epoch ms). Las palabras seed son de solo lectura para el
/// resto de la app.
class ImpostorWordsSeedLoader {
  const ImpostorWordsSeedLoader({this.assetPath = kImpostorWordsSeedAsset});

  final String assetPath;

  /// Inserta el seed solo si la tabla `impostor_words` no tiene filas.
  ///
  /// Devuelve el número de palabras insertadas (0 si la tabla ya tenía datos).
  Future<int> seedIfEmpty(DatabaseExecutor db) async {
    final count =
        Sqflite.firstIntValue(
          await db.rawQuery('SELECT COUNT(*) FROM impostor_words'),
        ) ??
        0;
    if (count > 0) {
      return 0;
    }

    final words = await _loadSeedWords();
    final createdAt = DateTime.now().millisecondsSinceEpoch;

    final batch = db.batch();
    for (final word in words) {
      batch.insert('impostor_words', <String, Object?>{
        'word': word.word,
        'hint': word.hint,
        'is_seed': 1,
        'created_at': createdAt,
      }, conflictAlgorithm: ConflictAlgorithm.ignore);
    }

    // Con ConflictAlgorithm.ignore, cada insert que choca con una palabra ya
    // existente (incluidos duplicados NOCASE dentro del propio seed) NO crea
    // fila pero igual queda encolado, por lo que contar los encolados
    // sobre-cuenta. Pedimos los resultados del batch: cada insert devuelve el
    // rowid de la fila creada (> 0) o 0/null si fue ignorado, así que sumamos
    // solo las inserciones reales.
    final results = await batch.commit(noResult: false);
    var inserted = 0;
    for (final result in results) {
      if (result is int && result > 0) {
        inserted++;
      }
    }
    return inserted;
  }

  Future<List<SeedWord>> _loadSeedWords() async {
    final raw = await rootBundle.loadString(assetPath);
    final decoded = jsonDecode(raw) as List<dynamic>;
    return decoded
        .cast<Map<String, dynamic>>()
        .map(SeedWord.fromJson)
        .toList(growable: false);
  }
}
