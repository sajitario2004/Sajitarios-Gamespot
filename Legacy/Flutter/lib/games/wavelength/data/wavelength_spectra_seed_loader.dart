/// Cargador del seed inicial de espectros de Wavelength.
///
/// Sigue exactamente el mismo patrón que [TriviaQuestionsSeedLoader]:
/// lee el JSON desde el bundle de assets, guarda solo si la tabla está vacía
/// (idempotente) e inserta con [is_seed = 1] usando un batch.
library;

import 'dart:convert';

import 'package:flutter/services.dart' show rootBundle;
import 'package:sqflite/sqflite.dart';

import 'package:sajitarios_gamespot/games/wavelength/data/wavelength_schema.dart';

/// Ruta del asset con los espectros semilla de Wavelength.
const String kWavelengthSpectraSeedAsset =
    'assets/seed/wavelength_spectra.json';

/// Un espectro del seed leído desde JSON, antes de mapearlo a la BD.
class _SeedSpectrum {
  const _SeedSpectrum({required this.izquierda, required this.derecha});

  /// Construye un [_SeedSpectrum] desde un mapa JSON.
  ///
  /// Lanza [FormatException] si algún campo obligatorio está ausente o vacío.
  factory _SeedSpectrum.fromJson(Map<String, dynamic> json) {
    final izquierda = (json['izquierda'] as String?)?.trim();
    final derecha = (json['derecha'] as String?)?.trim();

    if (izquierda == null || izquierda.isEmpty) {
      throw FormatException('Campo "izquierda" faltante o vacío: $json');
    }
    if (derecha == null || derecha.isEmpty) {
      throw FormatException('Campo "derecha" faltante o vacío: $json');
    }

    return _SeedSpectrum(izquierda: izquierda, derecha: derecha);
  }

  final String izquierda;
  final String derecha;
}

/// Carga los espectros semilla de Wavelength en la base la primera vez.
///
/// Lee el JSON de [kWavelengthSpectraSeedAsset] vía [rootBundle] y, solo si
/// la tabla [kWavelengthSpectraTable] está vacía, inserta cada espectro con
/// [is_seed = 1]. Los espectros seed son de solo lectura para la app.
class WavelengthSpectraSeedLoader {
  const WavelengthSpectraSeedLoader({
    this.assetPath = kWavelengthSpectraSeedAsset,
  });

  final String assetPath;

  /// Inserta el seed solo si la tabla [kWavelengthSpectraTable] no tiene filas.
  ///
  /// Devuelve el número de espectros insertados (0 si la tabla ya tenía datos).
  Future<int> seedIfEmpty(DatabaseExecutor db) async {
    final count =
        Sqflite.firstIntValue(
          await db.rawQuery('SELECT COUNT(*) FROM $kWavelengthSpectraTable'),
        ) ??
        0;
    if (count > 0) {
      return 0;
    }

    final spectra = await _loadSeedSpectra();
    final batch = db.batch();

    for (final s in spectra) {
      batch.insert(kWavelengthSpectraTable, <String, Object?>{
        'izquierda': s.izquierda,
        'derecha': s.derecha,
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

  Future<List<_SeedSpectrum>> _loadSeedSpectra() async {
    final raw = await rootBundle.loadString(assetPath);
    final decoded = jsonDecode(raw) as List<dynamic>;
    return decoded
        .cast<Map<String, dynamic>>()
        .map(_SeedSpectrum.fromJson)
        .toList(growable: false);
  }
}
