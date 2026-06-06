/// Tests for [WinnerRepository]: upsert, win counts, case-insensitive
/// uniqueness, and ranked retrieval.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/trivia/data/trivia_schema.dart';
import 'package:sajitarios_gamespot/games/trivia/data/winner_repository.dart';

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('WinnerRepository', () {
    late Database db;
    late WinnerRepository repo;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await TriviaSchema.createTables(db);
      repo = WinnerRepository(db);
    });

    tearDown(() async {
      await db.close();
    });

    group('getWins', () {
      test('returns 0 for unknown player', () async {
        expect(await repo.getWins('Unknown'), 0);
      });
    });

    group('incrementWins', () {
      test('first call creates record with wins = 1', () async {
        await repo.incrementWins('Nacho');
        expect(await repo.getWins('Nacho'), 1);
      });

      test('subsequent calls increment the counter', () async {
        await repo.incrementWins('Nacho');
        await repo.incrementWins('Nacho');
        await repo.incrementWins('Nacho');
        expect(await repo.getWins('Nacho'), 3);
      });

      test(
        'name uniqueness is case-insensitive — "Nacho" == "nacho"',
        () async {
          await repo.incrementWins('Nacho');
          await repo.incrementWins('nacho');
          // Both map to the same row; total should be 2.
          expect(await repo.getWins('Nacho'), 2);
          expect(await repo.getWins('nacho'), 2);
        },
      );

      test('different names have independent counters', () async {
        await repo.incrementWins('Alice');
        await repo.incrementWins('Alice');
        await repo.incrementWins('Bob');
        expect(await repo.getWins('Alice'), 2);
        expect(await repo.getWins('Bob'), 1);
      });
    });

    group('getAllRanked', () {
      test('returns empty list when no records', () async {
        expect(await repo.getAllRanked(), isEmpty);
      });

      test('returns records ordered by wins descending', () async {
        await repo.incrementWins('Charlie');
        await repo.incrementWins('Alice');
        await repo.incrementWins('Alice');
        await repo.incrementWins('Bob');
        await repo.incrementWins('Bob');
        await repo.incrementWins('Bob');

        final ranked = await repo.getAllRanked();
        expect(ranked.map((r) => r.name).toList(), ['Bob', 'Alice', 'Charlie']);
        expect(ranked.map((r) => r.wins).toList(), [3, 2, 1]);
      });

      test(
        'tiebreaker: same wins ordered alphabetically (case-insensitive)',
        () async {
          await repo.incrementWins('Zara');
          await repo.incrementWins('Alice');

          final ranked = await repo.getAllRanked();
          expect(ranked[0].name.toLowerCase(), 'alice');
          expect(ranked[1].name.toLowerCase(), 'zara');
        },
      );
    });
  });
}
