/// Data access layer for Wavelength spectra.
///
/// [SpectrumRepository] operates over the `wavelength_spectra` table. It maps
/// database rows ↔ domain [Spectrum] objects. Random selection stays in the
/// domain use-case ([PickRoundUseCase]); this repo only returns lists.
library;

import 'package:sqflite_common/sqlite_api.dart';

import 'package:sajitarios_gamespot/games/wavelength/data/wavelength_schema.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/spectrum.dart';

/// Persists and queries Wavelength spectra in the `wavelength_spectra` table.
class SpectrumRepository {
  const SpectrumRepository(this._db);

  final DatabaseExecutor _db;

  // ── Write ─────────────────────────────────────────────────────────────────

  /// Inserts a single [spectrum] and returns it with its assigned [Spectrum.id].
  Future<Spectrum> insert(Spectrum spectrum) async {
    final id = await _db.insert(
      kWavelengthSpectraTable,
      _toRow(spectrum, isSeed: false),
    );
    return Spectrum(
      id: id,
      leftConcept: spectrum.leftConcept,
      rightConcept: spectrum.rightConcept,
    );
  }

  /// Inserts all [spectra] in sequence and returns them with assigned ids.
  ///
  /// No transaction is opened here so the caller can wrap multiple repo calls
  /// in one if needed.
  Future<List<Spectrum>> bulkInsert(List<Spectrum> spectra) async {
    final result = <Spectrum>[];
    for (final s in spectra) {
      result.add(await insert(s));
    }
    return result;
  }

  // ── Read ──────────────────────────────────────────────────────────────────

  /// Total number of spectra in the table.
  Future<int> count() async {
    final rows = await _db.rawQuery(
      'SELECT COUNT(*) AS c FROM $kWavelengthSpectraTable',
    );
    return (rows.first['c'] as int?) ?? 0;
  }

  /// Returns all spectra ordered by id ascending.
  ///
  /// Ordering is stable to make tests predictable.
  Future<List<Spectrum>> getAll() async {
    final rows = await _db.rawQuery(
      'SELECT * FROM $kWavelengthSpectraTable ORDER BY id ASC',
    );
    return rows.map(_fromRow).toList(growable: false);
  }

  // ── Mapping ───────────────────────────────────────────────────────────────

  /// Converts a database row into a domain [Spectrum].
  static Spectrum _fromRow(Map<String, Object?> row) {
    return Spectrum(
      id: row['id'] as int,
      leftConcept: row['izquierda'] as String,
      rightConcept: row['derecha'] as String,
    );
  }

  /// Converts a domain [Spectrum] to a database row map.
  static Map<String, Object?> _toRow(Spectrum s, {required bool isSeed}) {
    return <String, Object?>{
      'izquierda': s.leftConcept,
      'derecha': s.rightConcept,
      'is_seed': isSeed ? 1 : 0,
    };
  }
}
