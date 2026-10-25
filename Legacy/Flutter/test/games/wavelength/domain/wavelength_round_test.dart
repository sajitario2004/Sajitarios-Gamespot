/// Tests for [WavelengthRound] scoring bands, transitions, and clamping.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/spectrum.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_round.dart';

Spectrum _spectrum() =>
    Spectrum(id: 1, leftConcept: 'frío', rightConcept: 'caliente');

void main() {
  group('scoreFor', () {
    const target = 0.5;

    test('bullseye: exact hit scores 4', () {
      expect(scoreFor(target, target), kPointsBullseye);
    });

    test('bullseye: within half-width scores 4', () {
      // Use a value clearly inside the band (not at the floating-point edge).
      expect(
        scoreFor(target + kBullseyeHalfWidth - 0.001, target),
        kPointsBullseye,
      );
      expect(
        scoreFor(target - kBullseyeHalfWidth + 0.001, target),
        kPointsBullseye,
      );
    });

    test('near band scores 3', () {
      // Just past bullseye edge, still within near band.
      final nearGuess = target + kBullseyeHalfWidth + 0.001;
      expect(scoreFor(nearGuess, target), kPointsNear);
      expect(scoreFor(target + kNearHalfWidth, target), kPointsNear);
    });

    test('far band scores 2', () {
      final farGuess = target + kNearHalfWidth + 0.001;
      expect(scoreFor(farGuess, target), kPointsFar);
      expect(scoreFor(target + kFarHalfWidth, target), kPointsFar);
    });

    test('miss scores 0', () {
      final missGuess = target + kFarHalfWidth + 0.001;
      expect(scoreFor(missGuess, target), kPointsMiss);
    });

    test('scoring is symmetric around target', () {
      final offset = kNearHalfWidth - 0.001;
      expect(
        scoreFor(target + offset, target),
        scoreFor(target - offset, target),
      );
    });

    test('clamps guess below 0.0 to 0.0', () {
      // Target near 0 so that clamped-to-0 lands in bullseye.
      expect(scoreFor(-0.5, 0.0), kPointsBullseye);
    });

    test('clamps guess above 1.0 to 1.0', () {
      expect(scoreFor(1.5, 1.0), kPointsBullseye);
    });

    test('target at 0.0 edge: bullseye hit', () {
      expect(scoreFor(0.0, 0.0), kPointsBullseye);
    });

    test('target at 1.0 edge: bullseye hit', () {
      expect(scoreFor(1.0, 1.0), kPointsBullseye);
    });
  });

  group('WavelengthRound', () {
    group('start', () {
      test('creates round with no clue and no guess', () {
        final round = WavelengthRound.start(
          spectrum: _spectrum(),
          targetPosition: 0.5,
        );
        expect(round.hasClue, isFalse);
        expect(round.hasGuess, isFalse);
        expect(round.score, isNull);
      });

      test('throws ArgumentError for targetPosition < 0', () {
        expect(
          () => WavelengthRound.start(
            spectrum: _spectrum(),
            targetPosition: -0.1,
          ),
          throwsArgumentError,
        );
      });

      test('throws ArgumentError for targetPosition > 1', () {
        expect(
          () =>
              WavelengthRound.start(spectrum: _spectrum(), targetPosition: 1.1),
          throwsArgumentError,
        );
      });

      test('accepts boundary values 0.0 and 1.0', () {
        expect(
          () =>
              WavelengthRound.start(spectrum: _spectrum(), targetPosition: 0.0),
          returnsNormally,
        );
        expect(
          () =>
              WavelengthRound.start(spectrum: _spectrum(), targetPosition: 1.0),
          returnsNormally,
        );
      });
    });

    group('withClue', () {
      test('sets clue and returns new instance', () {
        final round = WavelengthRound.start(
          spectrum: _spectrum(),
          targetPosition: 0.5,
        );
        final withClue = round.withClue('tibio');
        expect(withClue.clue, 'tibio');
        expect(withClue.hasClue, isTrue);
        // Original unchanged.
        expect(round.hasClue, isFalse);
      });

      test('trims whitespace from clue', () {
        final round = WavelengthRound.start(
          spectrum: _spectrum(),
          targetPosition: 0.5,
        );
        expect(round.withClue('  tibio  ').clue, 'tibio');
      });

      test('throws ArgumentError for empty clue', () {
        final round = WavelengthRound.start(
          spectrum: _spectrum(),
          targetPosition: 0.5,
        );
        expect(() => round.withClue('   '), throwsArgumentError);
      });
    });

    group('withGuess', () {
      test('sets guess and returns new instance', () {
        final round = WavelengthRound.start(
          spectrum: _spectrum(),
          targetPosition: 0.5,
        ).withClue('tibio');
        final withGuess = round.withGuess(0.48);
        expect(withGuess.hasGuess, isTrue);
        expect(withGuess.guess, closeTo(0.48, 0.0001));
      });

      test('clamps guess to [0.0, 1.0]', () {
        final round = WavelengthRound.start(
          spectrum: _spectrum(),
          targetPosition: 0.5,
        );
        expect(round.withGuess(-0.5).guess, 0.0);
        expect(round.withGuess(1.5).guess, 1.0);
      });

      test('score is non-null after guess', () {
        final round = WavelengthRound.start(
          spectrum: _spectrum(),
          targetPosition: 0.5,
        ).withGuess(0.5);
        expect(round.score, isNotNull);
        expect(round.score, kPointsBullseye);
      });
    });
  });
}
