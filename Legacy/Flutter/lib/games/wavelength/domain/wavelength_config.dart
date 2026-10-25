/// Validated, immutable configuration for a Wavelength session.
///
/// Only obtainable via [WavelengthConfig.create], which validates inputs and
/// returns a [WavelengthConfigResult] (success or failure) so the presentation
/// layer can react to the specific error without catching exceptions.
/// Mirrors the style of [TriviaConfig] and [GameConfig].
library;

/// Minimum number of players for a Wavelength session.
const int kWavelengthMinPlayers = 2;

/// Maximum number of players for a Wavelength session.
const int kWavelengthMaxPlayers = 8;

/// Minimum number of rounds (rondas) for a session.
const int kWavelengthMinRondas = 1;

/// Maximum number of rounds (rondas) for a session.
const int kWavelengthMaxRondas = 20;

/// Default number of rounds when the caller does not specify one.
const int kWavelengthDefaultRondas = 8;

/// Reasons why a [WavelengthConfig] could not be built.
enum WavelengthConfigError {
  /// Fewer than [kWavelengthMinPlayers] player names were provided.
  pocosJugadores,

  /// More than [kWavelengthMaxPlayers] player names were provided.
  demasiadosJugadores,

  /// At least one player name is empty after trimming whitespace.
  nombreVacio,

  /// Two or more players share the same name (case-insensitive).
  nombresDuplicados,

  /// Number of rounds is outside [kWavelengthMinRondas, kWavelengthMaxRondas].
  rondasFueraDeRango,
}

/// Result of attempting to build a [WavelengthConfig].
///
/// Either holds a valid [config] ([WavelengthConfigResult.success]) or an
/// [error] that describes why construction failed
/// ([WavelengthConfigResult.failure]).
class WavelengthConfigResult {
  const WavelengthConfigResult._({this.config, this.error});

  /// Successful result wrapping the validated [config].
  const WavelengthConfigResult.success(WavelengthConfig config)
    : this._(config: config);

  /// Failed result carrying the [error] reason.
  const WavelengthConfigResult.failure(WavelengthConfigError error)
    : this._(error: error);

  /// The valid configuration, or `null` on failure.
  final WavelengthConfig? config;

  /// The failure reason, or `null` on success.
  final WavelengthConfigError? error;

  /// `true` when the configuration was built successfully.
  bool get isSuccess => config != null;
}

/// Validated configuration for a Wavelength session.
///
/// Guarantees: 2–8 non-empty unique player names, rounds in
/// [kWavelengthMinRondas, kWavelengthMaxRondas].
class WavelengthConfig {
  const WavelengthConfig._({required this.playerNames, required this.rondas});

  /// Player names in introduction order. Immutable, trimmed, unique.
  final List<String> playerNames;

  /// Number of rounds for the session.
  final int rondas;

  /// Builds a [WavelengthConfig] from raw inputs.
  ///
  /// Validation rules (return [WavelengthConfigResult.failure] on violation):
  /// - [playerNames] count must be in
  ///   [kWavelengthMinPlayers, kWavelengthMaxPlayers].
  /// - No name may be empty after trimming whitespace.
  /// - Names are unique (case-insensitive).
  /// - [rondas] must be in [kWavelengthMinRondas, kWavelengthMaxRondas].
  static WavelengthConfigResult create({
    required List<String> playerNames,
    int rondas = kWavelengthDefaultRondas,
  }) {
    if (playerNames.length < kWavelengthMinPlayers) {
      return const WavelengthConfigResult.failure(
        WavelengthConfigError.pocosJugadores,
      );
    }
    if (playerNames.length > kWavelengthMaxPlayers) {
      return const WavelengthConfigResult.failure(
        WavelengthConfigError.demasiadosJugadores,
      );
    }
    if (playerNames.any((n) => n.trim().isEmpty)) {
      return const WavelengthConfigResult.failure(
        WavelengthConfigError.nombreVacio,
      );
    }
    final seen = <String>{};
    for (final name in playerNames) {
      if (!seen.add(name.trim().toLowerCase())) {
        return const WavelengthConfigResult.failure(
          WavelengthConfigError.nombresDuplicados,
        );
      }
    }
    if (rondas < kWavelengthMinRondas || rondas > kWavelengthMaxRondas) {
      return const WavelengthConfigResult.failure(
        WavelengthConfigError.rondasFueraDeRango,
      );
    }

    return WavelengthConfigResult.success(
      WavelengthConfig._(
        playerNames: List<String>.unmodifiable(
          playerNames.map((n) => n.trim()).toList(),
        ),
        rondas: rondas,
      ),
    );
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    if (other is! WavelengthConfig) return false;
    if (other.rondas != rondas) return false;
    if (other.playerNames.length != playerNames.length) return false;
    for (var i = 0; i < playerNames.length; i++) {
      if (other.playerNames[i] != playerNames[i]) return false;
    }
    return true;
  }

  @override
  int get hashCode => Object.hash(Object.hashAll(playerNames), rondas);

  @override
  String toString() =>
      'WavelengthConfig(players: $playerNames, rondas: $rondas)';
}
