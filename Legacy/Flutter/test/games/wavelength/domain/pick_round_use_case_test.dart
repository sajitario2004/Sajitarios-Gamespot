/// Tests for [PickRoundUseCase]: determinism under seeded RandomProvider.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/pick_round_use_case.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/spectrum.dart';

List<Spectrum> _pool() => [
  Spectrum(id: 1, leftConcept: 'frío', rightConcept: 'caliente'),
  Spectrum(id: 2, leftConcept: 'barato', rightConcept: 'caro'),
  Spectrum(id: 3, leftConcept: 'lento', rightConcept: 'rápido'),
];

void main() {
  group('PickRoundUseCase', () {
    test('same seed produces same spectrum and target', () {
      const seed = 42;
      final pool = _pool();

      final round1 = PickRoundUseCase(RandomProvider.seeded(seed))(pool);
      final round2 = PickRoundUseCase(RandomProvider.seeded(seed))(pool);

      expect(round1.spectrum, equals(round2.spectrum));
      expect(round1.targetPosition, closeTo(round2.targetPosition, 1e-12));
    });

    test('different seeds may produce different results', () {
      final pool = _pool();
      // Run many seeds until we find two that differ (very likely immediately).
      var foundDifference = false;
      for (var seed = 0; seed < 20; seed++) {
        final r1 = PickRoundUseCase(RandomProvider.seeded(seed))(pool);
        final r2 = PickRoundUseCase(RandomProvider.seeded(seed + 100))(pool);
        if (r1.spectrum != r2.spectrum ||
            (r1.targetPosition - r2.targetPosition).abs() > 1e-12) {
          foundDifference = true;
          break;
        }
      }
      expect(foundDifference, isTrue);
    });

    test('target position is in [0.0, 1.0)', () {
      final pool = _pool();
      for (var seed = 0; seed < 50; seed++) {
        final round = PickRoundUseCase(RandomProvider.seeded(seed))(pool);
        expect(round.targetPosition, inInclusiveRange(0.0, 1.0));
      }
    });

    test('selected spectrum always comes from the pool', () {
      final pool = _pool();
      for (var seed = 0; seed < 50; seed++) {
        final round = PickRoundUseCase(RandomProvider.seeded(seed))(pool);
        expect(pool, contains(round.spectrum));
      }
    });

    test('round has no clue and no guess after creation', () {
      final round = PickRoundUseCase(RandomProvider.seeded(1))(_pool());
      expect(round.hasClue, isFalse);
      expect(round.hasGuess, isFalse);
    });

    test('throws when pool is empty', () {
      expect(
        () => PickRoundUseCase(RandomProvider.seeded(0))([]),
        throwsArgumentError,
      );
    });
  });
}
