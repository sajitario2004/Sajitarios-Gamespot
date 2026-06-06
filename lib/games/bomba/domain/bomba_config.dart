/// Validated, immutable configuration for a La Bomba session.
///
/// Only obtainable via [BombaConfig.create], which validates the inputs and
/// returns a [BombaConfigResult]. Mirrors the style of [TriviaConfig].
library;

import 'package:sajitarios_gamespot/games/bomba/domain/bomba_mode.dart';

/// Minimum number of players allowed in a La Bomba session.
const int kBombaMinPlayers = 2;

/// Maximum number of players allowed in a La Bomba session.
const int kBombaMaxPlayers = 12;

/// Minimum allowed value for [BombaConfig.minSegundos].
const int kBombaAbsMinSegundos = 10;

/// Maximum allowed value for [BombaConfig.maxSegundos].
const int kBombaAbsMaxSegundos = 60;

/// Reasons why a [BombaConfig] could not be built.
enum BombaConfigError {
  /// Fewer than [kBombaMinPlayers] player names were provided.
  pocosJugadores,

  /// More than [kBombaMaxPlayers] player names were provided.
  demasiadosJugadores,

  /// At least one player name is empty after trimming whitespace.
  nombreVacio,

  /// Two or more players share the same name (case-insensitive).
  nombresDuplicados,

  /// [BombaConfig.minSegundos] is below [kBombaAbsMinSegundos].
  minSegundosFueraDeLimite,

  /// [BombaConfig.maxSegundos] exceeds [kBombaAbsMaxSegundos].
  maxSegundosFueraDeLimite,

  /// [BombaConfig.minSegundos] >= [BombaConfig.maxSegundos].
  rangoSegundosInvalido,
}

/// Result of attempting to build a [BombaConfig].
///
/// Either holds a valid [config] ([BombaConfigResult.success]) or an [error]
/// that describes why construction failed ([BombaConfigResult.failure]).
class BombaConfigResult {
  const BombaConfigResult._({this.config, this.error});

  /// Successful result wrapping the validated [config].
  const BombaConfigResult.success(BombaConfig config) : this._(config: config);

  /// Failed result carrying the [error] reason.
  const BombaConfigResult.failure(BombaConfigError error)
    : this._(error: error);

  /// The valid configuration, or `null` on failure.
  final BombaConfig? config;

  /// The failure reason, or `null` on success.
  final BombaConfigError? error;

  /// `true` when the configuration was built successfully.
  bool get isSuccess => config != null;
}

/// Validated configuration for a La Bomba session.
///
/// Guarantees: 2–12 non-empty unique player names, a valid [BombaMode],
/// and a valid fuse range [minSegundos, maxSegundos] within [10, 60] with
/// minSegundos < maxSegundos.
class BombaConfig {
  const BombaConfig._({
    required this.mode,
    required this.playerNames,
    required this.minSegundos,
    required this.maxSegundos,
  });

  /// The prompt mode for this session.
  final BombaMode mode;

  /// Player names in introduction order (2..12). Immutable, trimmed, unique.
  final List<String> playerNames;

  /// Minimum fuse duration in seconds (inclusive). Range: [10, maxSegundos).
  final int minSegundos;

  /// Maximum fuse duration in seconds (inclusive). Range: (minSegundos, 60].
  final int maxSegundos;

  /// Builds a [BombaConfig] from raw inputs, returning a [BombaConfigResult].
  ///
  /// Validation rules (return [BombaConfigResult.failure] on violation):
  /// - [playerNames] count must be in [kBombaMinPlayers, kBombaMaxPlayers].
  /// - No name may be empty after trimming.
  /// - Names must be unique (case-insensitive).
  /// - [minSegundos] >= [kBombaAbsMinSegundos].
  /// - [maxSegundos] <= [kBombaAbsMaxSegundos].
  /// - [minSegundos] < [maxSegundos].
  static BombaConfigResult create({
    required BombaMode mode,
    required List<String> playerNames,
    required int minSegundos,
    required int maxSegundos,
  }) {
    if (playerNames.length < kBombaMinPlayers) {
      return const BombaConfigResult.failure(BombaConfigError.pocosJugadores);
    }
    if (playerNames.length > kBombaMaxPlayers) {
      return const BombaConfigResult.failure(
        BombaConfigError.demasiadosJugadores,
      );
    }
    if (playerNames.any((n) => n.trim().isEmpty)) {
      return const BombaConfigResult.failure(BombaConfigError.nombreVacio);
    }
    final seen = <String>{};
    for (final name in playerNames) {
      if (!seen.add(name.trim().toLowerCase())) {
        return const BombaConfigResult.failure(
          BombaConfigError.nombresDuplicados,
        );
      }
    }
    if (minSegundos < kBombaAbsMinSegundos) {
      return const BombaConfigResult.failure(
        BombaConfigError.minSegundosFueraDeLimite,
      );
    }
    if (maxSegundos > kBombaAbsMaxSegundos) {
      return const BombaConfigResult.failure(
        BombaConfigError.maxSegundosFueraDeLimite,
      );
    }
    if (minSegundos >= maxSegundos) {
      return const BombaConfigResult.failure(
        BombaConfigError.rangoSegundosInvalido,
      );
    }

    return BombaConfigResult.success(
      BombaConfig._(
        mode: mode,
        playerNames: List<String>.unmodifiable(
          playerNames.map((n) => n.trim()).toList(),
        ),
        minSegundos: minSegundos,
        maxSegundos: maxSegundos,
      ),
    );
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    if (other is! BombaConfig) return false;
    if (other.mode != mode) return false;
    if (other.minSegundos != minSegundos) return false;
    if (other.maxSegundos != maxSegundos) return false;
    if (other.playerNames.length != playerNames.length) return false;
    for (var i = 0; i < playerNames.length; i++) {
      if (other.playerNames[i] != playerNames[i]) return false;
    }
    return true;
  }

  @override
  int get hashCode =>
      Object.hash(mode, Object.hashAll(playerNames), minSegundos, maxSegundos);

  @override
  String toString() =>
      'BombaConfig(mode: $mode, players: $playerNames, '
      'fuse: ${minSegundos}s..${maxSegundos}s)';
}
