/// La Bomba session model and elimination logic — pure domain, no Flutter or
/// persistence imports.
///
/// The EXPLOSION INSTANT is chosen once at session creation via
/// [pickFuseSeconds]. The UI runs a real wall-clock timer and calls
/// [explode] when the fuse fires, or [pasar] when the holder passes the phone.
/// All state transitions are immutable: each method returns a new [BombaSession].
library;

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_config.dart';

/// Picks a fuse duration in seconds from the configured range.
///
/// Uses a single [RandomProvider.nextDouble] call scaled into
/// [config.minSegundos, config.maxSegundos].
///
/// The result is deterministic under a seeded [RandomProvider] and always
/// satisfies:
///   `config.minSegundos <= result <= config.maxSegundos`.
double pickFuseSeconds(RandomProvider rng, BombaConfig config) {
  final range = config.maxSegundos - config.minSegundos;
  return config.minSegundos + rng.nextDouble() * range;
}

/// An immutable snapshot of a La Bomba session's state.
///
/// Obtain the initial session via [BombaSession.start] and advance it with
/// [pasar] (pass the phone) or [explode] (fuse fires, holder is eliminated).
class BombaSession {
  const BombaSession._({
    required this.orderedPlayers,
    required this.alivePlayers,
    required this.currentHolderIndex,
    required this.fuseSeconds,
  });

  /// Creates a new session from [config] using [rng] to pick the fuse duration.
  ///
  /// [config.playerNames] must not be empty (enforced by [BombaConfig.create]).
  /// The first player in introduction order holds the phone initially.
  factory BombaSession.start(BombaConfig config, RandomProvider rng) {
    final names = List<String>.unmodifiable(config.playerNames);
    final fuse = pickFuseSeconds(rng, config);
    return BombaSession._(
      orderedPlayers: names,
      alivePlayers: names,
      currentHolderIndex: 0,
      fuseSeconds: fuse,
    );
  }

  /// Creates a new round for an ongoing session, keeping [alivePlayers] and
  /// [orderedPlayers] from a previous session snapshot and picking a new fuse
  /// duration via [rng] and [config].
  ///
  /// The holder resets to index 0 of the current alive players list.
  factory BombaSession.newRound({
    required BombaSession previous,
    required BombaConfig config,
    required RandomProvider rng,
  }) {
    final fuse = pickFuseSeconds(rng, config);
    return BombaSession._(
      orderedPlayers: previous.orderedPlayers,
      alivePlayers: previous.alivePlayers,
      currentHolderIndex:
          previous.currentHolderIndex % previous.alivePlayers.length,
      fuseSeconds: fuse,
    );
  }

  /// All player names in introduction order (never changes across the session).
  final List<String> orderedPlayers;

  /// Player names still in the game, in the order they rotate.
  ///
  /// When a player is eliminated their entry is removed; the list shrinks.
  final List<String> alivePlayers;

  /// Index into [alivePlayers] of the player currently holding the phone.
  final int currentHolderIndex;

  /// The chosen fuse duration in seconds (fixed for the lifetime of this session
  /// instance; the UI compares elapsed wall-clock time against this value).
  final double fuseSeconds;

  /// The name of the player currently holding the phone.
  String get currentHolder => alivePlayers[currentHolderIndex];

  /// `true` when only one player remains (they win).
  bool get isOver => alivePlayers.length <= 1;

  /// The winning player's name, or `null` if the game is not yet over.
  String? get winner => isOver ? alivePlayers.first : null;

  /// Passes the phone to the next alive player in rotation.
  ///
  /// Throws [StateError] if [isOver] is already `true`.
  BombaSession pasar() {
    if (isOver) {
      throw StateError('Cannot pass the phone when the session is over');
    }
    final nextIndex = (currentHolderIndex + 1) % alivePlayers.length;
    return BombaSession._(
      orderedPlayers: orderedPlayers,
      alivePlayers: alivePlayers,
      currentHolderIndex: nextIndex,
      fuseSeconds: fuseSeconds,
    );
  }

  /// Eliminates the current holder (the bomb explodes in their hands).
  ///
  /// Returns the updated session. If only one player was alive, [isOver] will
  /// be `true` on the returned session (no winner — edge case, but handled).
  ///
  /// Throws [StateError] if [isOver] is already `true`.
  BombaSession explode() {
    if (isOver) {
      throw StateError('Cannot explode when the session is already over');
    }
    final newAlive = List<String>.unmodifiable(
      List<String>.of(alivePlayers)..removeAt(currentHolderIndex),
    );

    // Keep the holder index in bounds; wrap if the eliminated player was last.
    final newIndex = newAlive.isEmpty
        ? 0
        : currentHolderIndex % newAlive.length;

    return BombaSession._(
      orderedPlayers: orderedPlayers,
      alivePlayers: newAlive,
      currentHolderIndex: newIndex,
      fuseSeconds: fuseSeconds,
    );
  }

  @override
  String toString() =>
      'BombaSession(alive: ${alivePlayers.length}, '
      'holder: "$currentHolder", fuse: ${fuseSeconds.toStringAsFixed(2)}s)';
}
