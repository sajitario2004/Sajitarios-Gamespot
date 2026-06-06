/// Trivia session model and elimination rules — pure domain, no Flutter or
/// persistence imports.
///
/// A session runs for exactly 9 rounds (3 facil → 3 dificil → 3 muyDificil).
/// Each round every alive player answers one question; a wrong answer
/// eliminates the player. Survivors after round 9 tie and win together. If
/// all players are eliminated in the same round, [winners] is empty.
///
/// All state transitions are immutable: each method returns a new
/// [TriviaSession] rather than mutating the current one.
library;

import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/trivia_player.dart';

/// Total number of rounds in a trivia session.
const int kTriviaRoundCount = 9;

/// Number of rounds per difficulty tier.
const int kRoundsPerTier = 3;

/// Returns the [Difficulty] tier that corresponds to [roundIndex] (0-based).
///
/// - Rounds 0–2 → [Difficulty.facil]
/// - Rounds 3–5 → [Difficulty.dificil]
/// - Rounds 6–8 → [Difficulty.muyDificil]
///
/// Throws [ArgumentError] for round indices outside [0, kTriviaRoundCount).
Difficulty difficultyForRound(int roundIndex) {
  if (roundIndex < 0 || roundIndex >= kTriviaRoundCount) {
    throw ArgumentError.value(
      roundIndex,
      'roundIndex',
      'roundIndex must be in [0, $kTriviaRoundCount), got $roundIndex',
    );
  }
  final tier = roundIndex ~/ kRoundsPerTier;
  return Difficulty.values[tier];
}

/// An immutable snapshot of a trivia session's state.
///
/// Obtain the initial session via [TriviaSession.start] and advance it with
/// [recordAnswer] and [advanceRound].
class TriviaSession {
  const TriviaSession._({
    required this.orderedPlayers,
    required this.eliminatedPlayers,
    required this.currentRound,
  });

  /// Starts a new session from [players] (introduction order).
  ///
  /// [players] must not be empty.
  factory TriviaSession.start(List<TriviaPlayer> players) {
    if (players.isEmpty) {
      throw ArgumentError.value(
        players,
        'players',
        'A trivia session requires at least one player',
      );
    }
    return TriviaSession._(
      orderedPlayers: List<TriviaPlayer>.unmodifiable(players),
      eliminatedPlayers: const <TriviaPlayer>{},
      currentRound: 0,
    );
  }

  /// All players in introduction order (never changes).
  final List<TriviaPlayer> orderedPlayers;

  /// Players who have been eliminated so far.
  final Set<TriviaPlayer> eliminatedPlayers;

  /// Current round index (0-based). Range: [0, kTriviaRoundCount].
  ///
  /// A value of [kTriviaRoundCount] means all rounds have been played and
  /// [isOver] is `true`.
  final int currentRound;

  /// Players still alive (not eliminated) in introduction order.
  List<TriviaPlayer> get alivePlayers => orderedPlayers
      .where((p) => !eliminatedPlayers.contains(p))
      .toList(growable: false);

  /// Difficulty tier for the current round.
  ///
  /// Returns `null` when [isOver] is `true` (no active round).
  Difficulty? get currentDifficulty =>
      isOver ? null : difficultyForRound(currentRound);

  /// `true` when all rounds are done OR all players are eliminated.
  bool get isOver => currentRound >= kTriviaRoundCount || alivePlayers.isEmpty;

  /// Players who survived all 9 rounds. Empty if everyone was eliminated.
  ///
  /// Meaningful only when [isOver] is `true`; returns the current alive list
  /// at any earlier point as a snapshot.
  List<TriviaPlayer> get winners => isOver ? alivePlayers : [];

  /// Records [player]'s answer for the current round.
  ///
  /// - [correct] = `true` → player survives this round (no state change for
  ///   that player).
  /// - [correct] = `false` → player is eliminated.
  ///
  /// Throws [ArgumentError] if [player] is not in [alivePlayers] or the
  /// session [isOver].
  TriviaSession recordAnswer(TriviaPlayer player, {required bool correct}) {
    if (isOver) {
      throw StateError('Cannot record an answer when the session is over');
    }
    if (!alivePlayers.contains(player)) {
      throw ArgumentError.value(
        player,
        'player',
        'Player "${player.name}" is not alive in this session',
      );
    }
    if (correct) {
      return this; // No change — player stays alive.
    }
    return TriviaSession._(
      orderedPlayers: orderedPlayers,
      eliminatedPlayers: {...eliminatedPlayers, player},
      currentRound: currentRound,
    );
  }

  /// Advances to the next round.
  ///
  /// Throws [StateError] if [isOver] is already `true`.
  TriviaSession advanceRound() {
    if (isOver) {
      throw StateError('Cannot advance round when the session is over');
    }
    return TriviaSession._(
      orderedPlayers: orderedPlayers,
      eliminatedPlayers: eliminatedPlayers,
      currentRound: currentRound + 1,
    );
  }

  @override
  String toString() =>
      'TriviaSession(round: $currentRound/$kTriviaRoundCount, '
      'alive: ${alivePlayers.length}, eliminated: ${eliminatedPlayers.length})';
}
