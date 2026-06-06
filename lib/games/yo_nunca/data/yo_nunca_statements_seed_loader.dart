/// Cargador del seed inicial de declaraciones "Yo nunca".
///
/// Sigue exactamente el mismo patrón que [TriviaQuestionsSeedLoader] e
/// [ImpostorWordsSeedLoader]: lee el JSON desde el bundle de assets, guarda
/// solo si la tabla está vacía (idempotente) e inserta con [is_seed = 1]
/// usando un batch.
library;

import 'dart:convert';

import 'package:flutter/services.dart' show rootBundle;
import 'package:sqflite/sqflite.dart';

import 'package:sajitarios_gamespot/games/yo_nunca/data/yo_nunca_schema.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/intensidad.dart';

/// Ruta del asset con las declaraciones semilla de Yo Nunca.
const String kYoNuncaStatementsSeedAsset =
    'assets/seed/yo_nunca_statements.json';

/// Una declaración del seed leída desde JSON, antes de mapearla a la BD.
class _SeedStatement {
  const _SeedStatement({required this.frase, required this.intensidad});

  /// Construye un [_SeedStatement] desde un mapa JSON.
  ///
  /// Lanza [FormatException] si algún campo obligatorio está ausente o inválido.
  factory _SeedStatement.fromJson(Map<String, dynamic> json) {
    final frase = (json['frase'] as String?)?.trim();
    final intensidadStr = (json['intensidad'] as String?)?.trim();

    if (frase == null || frase.isEmpty) {
      throw FormatException('Campo "frase" faltante o vacío: $json');
    }
    if (intensidadStr == null || intensidadStr.isEmpty) {
      throw FormatException('Campo "intensidad" faltante o vacío: $json');
    }

    final Intensidad intensidad;
    try {
      intensidad = Intensidad.values.byName(intensidadStr);
    } on ArgumentError {
      throw FormatException(
        '"intensidad" no reconocida ("$intensidadStr"): $json',
      );
    }

    return _SeedStatement(frase: frase, intensidad: intensidad);
  }

  final String frase;
  final Intensidad intensidad;
}

/// Carga las declaraciones semilla de Yo Nunca en la base la primera vez.
///
/// Lee el JSON de [kYoNuncaStatementsSeedAsset] vía [rootBundle] y, solo si
/// la tabla [kYoNuncaStatementsTable] está vacía, inserta cada declaración con
/// [is_seed = 1]. Las declaraciones seed son de solo lectura para la app.
class YoNuncaStatementsSeedLoader {
  const YoNuncaStatementsSeedLoader({
    this.assetPath = kYoNuncaStatementsSeedAsset,
  });

  final String assetPath;

  /// Inserta el seed solo si la tabla [kYoNuncaStatementsTable] no tiene filas.
  ///
  /// Devuelve el número de declaraciones insertadas (0 si la tabla ya tenía
  /// datos).
  Future<int> seedIfEmpty(DatabaseExecutor db) async {
    final count =
        Sqflite.firstIntValue(
          await db.rawQuery('SELECT COUNT(*) FROM $kYoNuncaStatementsTable'),
        ) ??
        0;
    if (count > 0) {
      return 0;
    }

    final statements = await _loadSeedStatements();
    final batch = db.batch();

    for (final s in statements) {
      batch.insert(kYoNuncaStatementsTable, <String, Object?>{
        'frase': s.frase,
        'intensidad': s.intensidad.name,
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

  Future<List<_SeedStatement>> _loadSeedStatements() async {
    final raw = await rootBundle.loadString(assetPath);
    final decoded = jsonDecode(raw) as List<dynamic>;
    return decoded
        .cast<Map<String, dynamic>>()
        .map(_SeedStatement.fromJson)
        .toList(growable: false);
  }
}
