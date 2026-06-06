/// Fake in-memory [WinnerRepository] for unit tests.
///
/// Records win increments by player name so assertions can verify which players
/// received wins and how many times.
library;

import 'package:sajitarios_gamespot/games/trivia/data/winner_repository.dart';

/// In-memory [WinnerRepository] that accumulates wins in a plain [Map].
class FakeWinnerRepository implements WinnerRepository {
  /// Tracks how many times [incrementWins] was called per player name.
  final Map<String, int> wins = {};

  @override
  Future<void> incrementWins(String name) async {
    wins[name] = (wins[name] ?? 0) + 1;
  }

  @override
  Future<int> getWins(String name) async => wins[name] ?? 0;

  @override
  Future<List<WinnerRecord>> getAllRanked() async {
    final entries = wins.entries.toList()
      ..sort((a, b) {
        final cmp = b.value.compareTo(a.value);
        return cmp != 0 ? cmp : a.key.compareTo(b.key);
      });
    return entries
        .map((e) => WinnerRecord(name: e.key, wins: e.value))
        .toList(growable: false);
  }
}
