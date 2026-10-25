/// Wavelength round model and scoring — pure domain, no Flutter or persistence.
///
/// A round ties a [Spectrum] to a hidden [targetPosition] on a normalised
/// 0.0–1.0 axis. One player gives a [clue]; the group submits a [guess].
/// Scoring is determined by concentric bands around the target (like the board
/// game): bullseye = 4 pts, next band = 3, outer band = 2, miss = 0.
///
/// Band widths are exposed as named constants so the UI/dial can render them
/// without duplicating magic numbers.
library;

import 'package:sajitarios_gamespot/games/wavelength/domain/spectrum.dart';

// ── Scoring band constants ────────────────────────────────────────────────────

/// Half-width of the bullseye band (4 pts). Full width = 2× this value.
/// On a [0,1] axis this covers ± 0.06 around the target center.
const double kBullseyeHalfWidth = 0.06;

/// Half-width of the "near" band (3 pts). Full width = 2× this value.
/// Extends from [kBullseyeHalfWidth] to this value on each side of the target.
const double kNearHalfWidth = 0.12;

/// Half-width of the "far" band (2 pts). Full width = 2× this value.
/// Extends from [kNearHalfWidth] to this value on each side of the target.
const double kFarHalfWidth = 0.20;

// Outside [kFarHalfWidth] on either side = 0 pts (complete miss).

// ── Points per band ───────────────────────────────────────────────────────────

/// Points awarded for landing in the bullseye band.
const int kPointsBullseye = 4;

/// Points awarded for landing in the near band.
const int kPointsNear = 3;

/// Points awarded for landing in the far band.
const int kPointsFar = 2;

/// Points awarded for a miss (outside all bands).
const int kPointsMiss = 0;

// ── Pure scoring function ─────────────────────────────────────────────────────

/// Returns the score for a [guess] given a [targetCenter], both in [0.0, 1.0].
///
/// [guess] is clamped to [0.0, 1.0] before scoring so out-of-range inputs
/// are treated as edge hits rather than errors.
///
/// Scoring bands (symmetric around [targetCenter]):
/// - |guess - target| <= [kBullseyeHalfWidth] → [kPointsBullseye] (4)
/// - |guess - target| <= [kNearHalfWidth]     → [kPointsNear]     (3)
/// - |guess - target| <= [kFarHalfWidth]      → [kPointsFar]      (2)
/// - otherwise                                 → [kPointsMiss]     (0)
int scoreFor(double guess, double targetCenter) {
  final clampedGuess = guess.clamp(0.0, 1.0);
  final distance = (clampedGuess - targetCenter).abs();
  if (distance <= kBullseyeHalfWidth) return kPointsBullseye;
  if (distance <= kNearHalfWidth) return kPointsNear;
  if (distance <= kFarHalfWidth) return kPointsFar;
  return kPointsMiss;
}

// ── Round model ───────────────────────────────────────────────────────────────

/// An immutable snapshot of a Wavelength round.
///
/// Created via [WavelengthRound.start] (no clue/guess yet) and advanced with
/// [withClue] and [withGuess]. All transitions return new instances.
class WavelengthRound {
  const WavelengthRound._({
    required this.spectrum,
    required this.targetPosition,
    required this.clue,
    required this.guess,
  });

  /// Creates a new round with a [spectrum] and a hidden [targetPosition].
  ///
  /// [targetPosition] must be in [0.0, 1.0].
  factory WavelengthRound.start({
    required Spectrum spectrum,
    required double targetPosition,
  }) {
    if (targetPosition < 0.0 || targetPosition > 1.0) {
      throw ArgumentError.value(
        targetPosition,
        'targetPosition',
        'targetPosition must be in [0.0, 1.0]',
      );
    }
    return WavelengthRound._(
      spectrum: spectrum,
      targetPosition: targetPosition,
      clue: null,
      guess: null,
    );
  }

  /// The spectrum for this round.
  final Spectrum spectrum;

  /// The hidden target position on the normalised [0.0, 1.0] axis.
  final double targetPosition;

  /// The clue given by the psychic. `null` until [withClue] is called.
  final String? clue;

  /// The group's dial guess. `null` until [withGuess] is called.
  final double? guess;

  /// `true` once a clue has been given.
  bool get hasClue => clue != null;

  /// `true` once a guess has been submitted.
  bool get hasGuess => guess != null;

  /// Returns a new round identical to this one but with [clue] set.
  ///
  /// Throws [ArgumentError] if [clue] is empty after trimming.
  WavelengthRound withClue(String clue) {
    final trimmed = clue.trim();
    if (trimmed.isEmpty) {
      throw ArgumentError.value(clue, 'clue', 'La pista no puede estar vacía');
    }
    return WavelengthRound._(
      spectrum: spectrum,
      targetPosition: targetPosition,
      clue: trimmed,
      guess: guess,
    );
  }

  /// Returns a new round identical to this one but with [guess] set.
  ///
  /// [guess] is clamped to [0.0, 1.0].
  WavelengthRound withGuess(double guess) {
    return WavelengthRound._(
      spectrum: spectrum,
      targetPosition: targetPosition,
      clue: clue,
      guess: guess.clamp(0.0, 1.0),
    );
  }

  /// Score for the current [guess].
  ///
  /// Returns `null` if no [guess] has been submitted yet.
  int? get score => guess == null ? null : scoreFor(guess!, targetPosition);

  @override
  String toString() =>
      'WavelengthRound(spectrum: $spectrum, target: $targetPosition, '
      'clue: $clue, guess: $guess, score: $score)';
}
