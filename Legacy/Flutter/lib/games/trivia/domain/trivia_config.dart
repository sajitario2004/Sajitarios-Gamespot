/// Validated, immutable configuration for a trivia session.
///
/// Only obtainable via [TriviaConfig.create], which validates the inputs and
/// returns a [TriviaConfigResult] (success or failure) so the presentation
/// layer can react to the specific error without catching exceptions. Mirrors
/// the style of `GameConfig` in the Impostor bounded context.
library;

/// Minimum number of players allowed in a trivia session.
const int kTriviaMinPlayers = 2;

/// Maximum number of players allowed in a trivia session.
const int kTriviaMaxPlayers = 6;

/// Reasons why a [TriviaConfig] could not be built.
enum TriviaConfigError {
  /// Fewer than [kTriviaMinPlayers] player names were provided.
  pocosJugadores,

  /// More than [kTriviaMaxPlayers] player names were provided.
  demasiadosJugadores,

  /// At least one player name is empty after trimming whitespace.
  nombreVacio,

  /// Two or more players share the same name (case-insensitive).
  nombresDuplicados,

  /// No [Tematica] id was selected.
  sinTematicas,
}

/// Result of attempting to build a [TriviaConfig].
///
/// Either holds a valid [config] ([TriviaConfigResult.success]) or an
/// [error] that describes why construction failed ([TriviaConfigResult.failure]).
class TriviaConfigResult {
  const TriviaConfigResult._({this.config, this.error});

  /// Successful result wrapping the validated [config].
  const TriviaConfigResult.success(TriviaConfig config)
    : this._(config: config);

  /// Failed result carrying the [error] reason.
  const TriviaConfigResult.failure(TriviaConfigError error)
    : this._(error: error);

  /// The valid configuration, or `null` on failure.
  final TriviaConfig? config;

  /// The failure reason, or `null` on success.
  final TriviaConfigError? error;

  /// `true` when the configuration was built successfully.
  bool get isSuccess => config != null;
}

/// Validated configuration for a trivia session.
///
/// Guarantees: 2–6 non-empty unique player names and at least one selected
/// tematica id.
class TriviaConfig {
  const TriviaConfig._({
    required this.playerNames,
    required this.selectedTematicaIds,
  });

  /// Player names in introduction order (2..6). Immutable, trimmed, unique.
  final List<String> playerNames;

  /// The set of tematica ids chosen for this session. At least one element.
  final Set<String> selectedTematicaIds;

  /// Builds a [TriviaConfig] from raw inputs, returning a [TriviaConfigResult].
  ///
  /// Validation rules (return [TriviaConfigResult.failure] on violation):
  /// - [playerNames] count must be in [kTriviaMinPlayers, kTriviaMaxPlayers].
  /// - No name may be empty after trimming whitespace.
  /// - Names are unique (case-insensitive).
  /// - [selectedTematicaIds] must contain at least one id.
  static TriviaConfigResult create({
    required List<String> playerNames,
    required Set<String> selectedTematicaIds,
  }) {
    if (playerNames.length < kTriviaMinPlayers) {
      return const TriviaConfigResult.failure(TriviaConfigError.pocosJugadores);
    }
    if (playerNames.length > kTriviaMaxPlayers) {
      return const TriviaConfigResult.failure(
        TriviaConfigError.demasiadosJugadores,
      );
    }
    if (playerNames.any((n) => n.trim().isEmpty)) {
      return const TriviaConfigResult.failure(TriviaConfigError.nombreVacio);
    }
    final seen = <String>{};
    for (final name in playerNames) {
      if (!seen.add(name.trim().toLowerCase())) {
        return const TriviaConfigResult.failure(
          TriviaConfigError.nombresDuplicados,
        );
      }
    }
    if (selectedTematicaIds.isEmpty) {
      return const TriviaConfigResult.failure(TriviaConfigError.sinTematicas);
    }

    return TriviaConfigResult.success(
      TriviaConfig._(
        playerNames: List<String>.unmodifiable(
          playerNames.map((n) => n.trim()).toList(),
        ),
        selectedTematicaIds: Set<String>.unmodifiable(selectedTematicaIds),
      ),
    );
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    if (other is! TriviaConfig) return false;
    if (other.playerNames.length != playerNames.length) return false;
    for (var i = 0; i < playerNames.length; i++) {
      if (other.playerNames[i] != playerNames[i]) return false;
    }
    if (other.selectedTematicaIds.length != selectedTematicaIds.length) {
      return false;
    }
    return other.selectedTematicaIds.containsAll(selectedTematicaIds);
  }

  @override
  int get hashCode => Object.hash(
    Object.hashAll(playerNames),
    Object.hashAll(selectedTematicaIds),
  );

  @override
  String toString() =>
      'TriviaConfig(players: $playerNames, tematicas: $selectedTematicaIds)';
}
