/// Use case: pick a random spectrum and target position for a new round.
///
/// Pure domain — no Flutter or persistence imports. All randomness is injected
/// via [RandomProvider] so tests can use a seeded instance for determinism.
library;

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/spectrum.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_round.dart';

/// Picks a random [Spectrum] from [pool] and generates a random target
/// position, returning a fresh [WavelengthRound].
///
/// All randomness is delegated to an injected [RandomProvider]:
/// - [RandomProvider.pick] selects the spectrum.
/// - [RandomProvider.nextDouble] generates the target position in [0.0, 1.0).
///
/// This makes the use case fully deterministic under a seeded [RandomProvider],
/// enabling reproducible test scenarios.
class PickRoundUseCase {
  const PickRoundUseCase(this._random);

  final RandomProvider _random;

  /// Returns a new [WavelengthRound] with a randomly chosen spectrum and target.
  ///
  /// [pool] must not be empty; throws [ArgumentError] (propagated from
  /// [RandomProvider.pick]) otherwise.
  WavelengthRound call(List<Spectrum> pool) {
    final spectrum = _random.pick(pool);
    final targetPosition = _random.nextDouble();
    return WavelengthRound.start(
      spectrum: spectrum,
      targetPosition: targetPosition,
    );
  }
}
