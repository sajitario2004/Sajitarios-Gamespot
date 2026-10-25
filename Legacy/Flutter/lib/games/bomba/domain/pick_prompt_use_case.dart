/// Use case: pick a random prompt for the active [BombaMode], cycling without
/// repetition until the pool is exhausted.
///
/// Pure domain — no Flutter or persistence imports. All randomness injected via
/// [RandomProvider] for determinism in tests.
library;

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_mode.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_prompt.dart';

/// Picks prompts from a pool one at a time without repetition.
///
/// Maintains separate seen-sets per [BombaMode] so that switching modes
/// within a session always draws from an independent pool. When all prompts
/// for a given mode have been shown the set resets and the full pool is
/// available again.
///
/// Usage:
/// ```dart
/// final uc = PickPromptUseCase(rng);
/// final prompt = uc.pick(mode: BombaMode.silaba, pool: silabas);
/// ```
class PickPromptUseCase {
  PickPromptUseCase(this._random);

  final RandomProvider _random;

  final Map<BombaMode, Set<int>> _seen = {};

  /// Returns a random [BombaPrompt] from [pool] for the given [mode].
  ///
  /// Guarantees no repetition until all prompts in [pool] have been picked;
  /// then the seen-set resets and the full pool becomes available again.
  ///
  /// [pool] must not be empty and must contain at least one prompt whose
  /// [BombaPrompt.mode] matches [mode].
  ///
  /// Throws [ArgumentError] if [pool] is empty or contains no prompt for
  /// [mode].
  BombaPrompt pick({required BombaMode mode, required List<BombaPrompt> pool}) {
    final candidates = pool.where((p) => p.mode == mode).toList();
    if (candidates.isEmpty) {
      throw ArgumentError.value(
        pool,
        'pool',
        'No hay prompts disponibles para el modo $mode',
      );
    }

    final seen = _seen.putIfAbsent(mode, () => {});

    // Reset when the full pool has been exhausted.
    if (seen.length >= candidates.length) {
      seen.clear();
    }

    // Collect unseen candidates and pick one at random.
    final unseen = candidates.where((p) => !seen.contains(p.id)).toList();
    final chosen = _random.pick(unseen);
    seen.add(chosen.id);
    return chosen;
  }

  /// Resets the no-repeat tracking for [mode], making the full pool available
  /// again immediately.
  void resetMode(BombaMode mode) => _seen.remove(mode);

  /// Resets tracking for all modes.
  void resetAll() => _seen.clear();
}
