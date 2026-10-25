/// Cargador del seed inicial de preguntas de trivia.
///
/// Sigue exactamente el mismo patrón que [ImpostorWordsSeedLoader]:
/// lee el JSON desde el bundle de assets, guarda solo si la tabla está vacía
/// (idempotente) e inserta con [is_seed = 1] usando un batch.
library;

import 'dart:convert';

import 'package:flutter/services.dart' show rootBundle;
import 'package:sqflite/sqflite.dart';

import 'package:sajitarios_gamespot/games/trivia/data/trivia_schema.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';

/// Ruta del asset con las preguntas semilla de Trivia.
const String kTriviaQuestionsSeedAsset = 'assets/seed/trivia_questions.json';

/// Una pregunta del seed leída desde JSON, antes de mapearla a la BD.
class _SeedQuestion {
  const _SeedQuestion({
    required this.tematicaId,
    required this.difficulty,
    required this.enunciado,
    required this.options,
    required this.correctIndex,
  });

  /// Construye un [_SeedQuestion] desde un mapa JSON.
  ///
  /// Lanza [FormatException] si algún campo obligatorio está ausente o inválido.
  factory _SeedQuestion.fromJson(Map<String, dynamic> json) {
    final tematicaId = (json['tematica'] as String?)?.trim();
    final difficultyStr = (json['difficulty'] as String?)?.trim();
    final enunciado = (json['enunciado'] as String?)?.trim();
    final opcionesRaw = json['opciones'];
    final correcta = json['correcta'];

    if (tematicaId == null || tematicaId.isEmpty) {
      throw FormatException('Campo "tematica" faltante o vacío: $json');
    }
    if (difficultyStr == null || difficultyStr.isEmpty) {
      throw FormatException('Campo "difficulty" faltante o vacío: $json');
    }
    if (enunciado == null || enunciado.isEmpty) {
      throw FormatException('Campo "enunciado" faltante o vacío: $json');
    }
    if (opcionesRaw is! List || opcionesRaw.length != 4) {
      throw FormatException(
        '"opciones" debe ser una lista de 4 elementos: $json',
      );
    }
    if (correcta is! int || correcta < 0 || correcta >= 4) {
      throw FormatException('"correcta" debe ser un entero en [0, 4): $json');
    }

    final Difficulty difficulty;
    try {
      difficulty = Difficulty.values.byName(difficultyStr);
    } on ArgumentError {
      throw FormatException(
        '"difficulty" no reconocida ("$difficultyStr"): $json',
      );
    }

    return _SeedQuestion(
      tematicaId: tematicaId,
      difficulty: difficulty,
      enunciado: enunciado,
      options: opcionesRaw.cast<String>(),
      correctIndex: correcta,
    );
  }

  final String tematicaId;
  final Difficulty difficulty;
  final String enunciado;
  final List<String> options;
  final int correctIndex;
}

/// Carga las preguntas semilla de Trivia en la base la primera vez.
///
/// Lee el JSON de [kTriviaQuestionsSeedAsset] vía [rootBundle] y, solo si
/// la tabla [kTriviaQuestionsTable] está vacía, inserta cada pregunta con
/// [is_seed = 1]. Las preguntas seed son de solo lectura para la app.
class TriviaQuestionsSeedLoader {
  const TriviaQuestionsSeedLoader({this.assetPath = kTriviaQuestionsSeedAsset});

  final String assetPath;

  /// Inserta el seed solo si la tabla [kTriviaQuestionsTable] no tiene filas.
  ///
  /// Devuelve el número de preguntas insertadas (0 si la tabla ya tenía datos).
  Future<int> seedIfEmpty(DatabaseExecutor db) async {
    final count =
        Sqflite.firstIntValue(
          await db.rawQuery('SELECT COUNT(*) FROM $kTriviaQuestionsTable'),
        ) ??
        0;
    if (count > 0) {
      return 0;
    }

    final questions = await _loadSeedQuestions();
    final batch = db.batch();

    for (final q in questions) {
      batch.insert(kTriviaQuestionsTable, <String, Object?>{
        'tematica_id': q.tematicaId,
        'difficulty': q.difficulty.name,
        'enunciado': q.enunciado,
        'options_json': jsonEncode(q.options),
        'correct_index': q.correctIndex,
        'is_seed': 1,
      }, conflictAlgorithm: ConflictAlgorithm.ignore);
    }

    // Contamos solo las inserciones reales (rowid > 0) por si alguna
    // fila choca con ConflictAlgorithm.ignore.
    final results = await batch.commit(noResult: false);
    var inserted = 0;
    for (final result in results) {
      if (result is int && result > 0) {
        inserted++;
      }
    }
    return inserted;
  }

  Future<List<_SeedQuestion>> _loadSeedQuestions() async {
    final raw = await rootBundle.loadString(assetPath);
    final decoded = jsonDecode(raw) as List<dynamic>;
    return decoded
        .cast<Map<String, dynamic>>()
        .map(_SeedQuestion.fromJson)
        .toList(growable: false);
  }
}
