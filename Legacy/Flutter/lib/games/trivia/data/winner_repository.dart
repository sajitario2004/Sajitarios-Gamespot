/// Data access layer for the trivia winner leaderboard.
///
/// [WinnerRepository] operates over the `trivia_winners` table. It provides
/// an upsert-based wins counter per player name (case-insensitive NOCASE
/// collation in SQLite ensures "Nacho" and "nacho" are the same record).
library;

import 'package:sqflite_common/sqlite_api.dart';

import 'package:sajitarios_gamespot/games/trivia/data/trivia_schema.dart';

/// One entry in the winner leaderboard.
class WinnerRecord {
  const WinnerRecord({required this.name, required this.wins});

  /// Player name as stored in the database.
  final String name;

  /// Accumulated win count.
  final int wins;

  @override
  String toString() => 'WinnerRecord($name: $wins)';
}

/// Persists and queries the win counter for trivia players.
///
/// Name uniqueness is case-insensitive (NOCASE collation on the `name`
/// column): "Nacho" and "nacho" resolve to the same row.
class WinnerRepository {
  const WinnerRepository(this._db);

  final DatabaseExecutor _db;

  /// Increments the win count for [name] by 1.
  ///
  /// If no row exists for [name] yet, inserts one with `wins = 1`.
  /// The match is case-insensitive due to the NOCASE collation.
  Future<void> incrementWins(String name) async {
    // INSERT OR IGNORE ensures the row exists; then UPDATE increments.
    await _db.execute(
      'INSERT OR IGNORE INTO $kTriviaWinnersTable (name, wins) VALUES (?, 0)',
      <Object?>[name],
    );
    await _db.execute(
      'UPDATE $kTriviaWinnersTable SET wins = wins + 1 WHERE name = ?',
      <Object?>[name],
    );
  }

  /// Returns the current win count for [name], or 0 if no record exists.
  Future<int> getWins(String name) async {
    final rows = await _db.rawQuery(
      'SELECT wins FROM $kTriviaWinnersTable WHERE name = ? COLLATE NOCASE',
      <Object?>[name],
    );
    if (rows.isEmpty) return 0;
    return (rows.first['wins'] as int?) ?? 0;
  }

  /// Returns all winner records ordered by wins descending.
  ///
  /// Players with the same win count are ordered alphabetically by name
  /// (case-insensitive) as a stable tiebreaker.
  Future<List<WinnerRecord>> getAllRanked() async {
    final rows = await _db.rawQuery(
      'SELECT name, wins FROM $kTriviaWinnersTable '
      'ORDER BY wins DESC, name COLLATE NOCASE ASC',
    );
    return rows
        .map(
          (r) => WinnerRecord(
            name: r['name'] as String,
            wins: (r['wins'] as int?) ?? 0,
          ),
        )
        .toList(growable: false);
  }
}
