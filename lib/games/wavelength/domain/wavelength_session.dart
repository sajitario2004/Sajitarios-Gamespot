/// Wavelength session model — pure domain, no Flutter or persistence imports.
///
/// A session runs for exactly [WavelengthConfig.rondas] rounds. Each round one
/// player acts as the psychic (clue-giver); the role rotates in introduction
/// order across rounds. All state transitions are immutable: each method
/// returns a new [WavelengthSession] rather than mutating the current one.
library;

import 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_round.dart';

/// An immutable snapshot of a Wavelength session's state.
///
/// Obtain the initial session via [WavelengthSession.start] and advance it
/// with [recordRound].
class WavelengthSession {
  const WavelengthSession._({
    required this.playerNames,
    required this.totalRondas,
    required this.completedRounds,
    required this.cumulativeScore,
  });

  /// Starts a new session.
  ///
  /// [playerNames] must not be empty.
  /// [totalRondas] must be >= 1.
  factory WavelengthSession.start({
    required List<String> playerNames,
    required int totalRondas,
  }) {
    if (playerNames.isEmpty) {
      throw ArgumentError.value(
        playerNames,
        'playerNames',
        'A Wavelength session requires at least one player',
      );
    }
    if (totalRondas < 1) {
      throw ArgumentError.value(
        totalRondas,
        'totalRondas',
        'totalRondas must be >= 1',
      );
    }
    return WavelengthSession._(
      playerNames: List<String>.unmodifiable(playerNames),
      totalRondas: totalRondas,
      completedRounds: const [],
      cumulativeScore: 0,
    );
  }

  /// Player names in introduction order (never changes).
  final List<String> playerNames;

  /// Total number of rounds to play (from config).
  final int totalRondas;

  /// Rounds that have been completed (have a guess + score).
  final List<WavelengthRound> completedRounds;

  /// Sum of all scores for completed rounds.
  final int cumulativeScore;

  /// Index of the current round (0-based). Equals [completedRounds.length]
  /// before the session ends, or [totalRondas] when [isOver] is `true`.
  int get currentRoundIndex => completedRounds.length;

  /// `true` when all [totalRondas] rounds have been completed.
  bool get isOver => completedRounds.length >= totalRondas;

  /// The name of the player who acts as psychic in the current round.
  ///
  /// Players rotate in introduction order: round 0 → player 0,
  /// round 1 → player 1, …, wrapping around when needed.
  ///
  /// Returns `null` when [isOver] is `true` (no active round).
  String? get currentPsychic =>
      isOver ? null : playerNames[currentRoundIndex % playerNames.length];

  /// Records a completed [round] (must have a guess) and advances the session.
  ///
  /// Throws [StateError] if the session [isOver].
  /// Throws [ArgumentError] if [round] has no guess.
  WavelengthSession recordRound(WavelengthRound round) {
    if (isOver) {
      throw StateError('Cannot record a round when the session is over');
    }
    if (!round.hasGuess) {
      throw ArgumentError.value(
        round,
        'round',
        'Cannot record a round without a guess',
      );
    }
    final roundScore = round.score ?? 0;
    return WavelengthSession._(
      playerNames: playerNames,
      totalRondas: totalRondas,
      completedRounds: List<WavelengthRound>.unmodifiable([
        ...completedRounds,
        round,
      ]),
      cumulativeScore: cumulativeScore + roundScore,
    );
  }

  @override
  String toString() =>
      'WavelengthSession(round: $currentRoundIndex/$totalRondas, '
      'score: $cumulativeScore)';
}
