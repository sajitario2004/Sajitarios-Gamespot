/// Fake in-memory [SpectrumRepository] for unit tests.
///
/// Stores spectra in a pre-loaded list and returns them without touching any
/// real database. Mirrors the shape of the trivia fake repositories.
library;

import 'package:sajitarios_gamespot/games/wavelength/data/spectrum_repository.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/spectrum.dart';

/// In-memory [SpectrumRepository] that serves spectra from a pre-loaded list.
class FakeSpectrumRepository implements SpectrumRepository {
  FakeSpectrumRepository(this._spectra);

  final List<Spectrum> _spectra;

  @override
  Future<List<Spectrum>> getAll() async =>
      List<Spectrum>.unmodifiable(_spectra);

  @override
  Future<int> count() async => _spectra.length;

  @override
  Future<Spectrum> insert(Spectrum spectrum) async => spectrum;

  @override
  Future<List<Spectrum>> bulkInsert(List<Spectrum> spectra) async => spectra;
}

// ─── Factory helpers ──────────────────────────────────────────────────────────

/// Builds a [Spectrum] with sequential ids.
Spectrum fakeSpectrum({required int id, String left = '', String right = ''}) =>
    Spectrum(
      id: id,
      leftConcept: left.isEmpty ? 'left$id' : left,
      rightConcept: right.isEmpty ? 'right$id' : right,
    );

/// Builds a [FakeSpectrumRepository] with [count] spectra.
FakeSpectrumRepository buildFakeSpectrumRepo({int count = 5}) {
  return FakeSpectrumRepository(
    List.generate(count, (i) => fakeSpectrum(id: i + 1)),
  );
}

/// Empty repository for testing sinEspectros error path.
FakeSpectrumRepository buildEmptySpectrumRepo() =>
    FakeSpectrumRepository(const []);
