/// Tests for [TriviaSession] elimination rules and winner detection.
library;

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/trivia_player.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/trivia_session.dart';

TriviaPlayer _p(String name) => TriviaPlayer(name);

List<TriviaPlayer> _players(int n) =>
    List.generate(n, (i) => _p('P$i'), growable: false);

void main() {
  group('TriviaSession.start', () {
    test('initialises with round 0 and no eliminations', () {
      final session = TriviaSession.start(_players(3));
      expect(session.currentRound, 0);
      expect(session.eliminatedPlayers, isEmpty);
      expect(session.alivePlayers.length, 3);
    });

    test('throws when player list is empty', () {
      expect(() => TriviaSession.start([]), throwsArgumentError);
    });
  });

  group('currentDifficulty', () {
    test('round 0 → facil', () {
      final s = TriviaSession.start(_players(2));
      expect(s.currentDifficulty, Difficulty.facil);
    });

    test('round 3 → dificil', () {
      var s = TriviaSession.start(_players(2));
      for (var i = 0; i < 3; i++) {
        s = s.advanceRound();
      }
      expect(s.currentDifficulty, Difficulty.dificil);
    });

    test('round 6 → muyDificil', () {
      var s = TriviaSession.start(_players(2));
      for (var i = 0; i < 6; i++) {
        s = s.advanceRound();
      }
      expect(s.currentDifficulty, Difficulty.muyDificil);
    });

    test('returns null when session is over', () {
      var s = TriviaSession.start(_players(2));
      for (var i = 0; i < kTriviaRoundCount; i++) {
        s = s.advanceRound();
      }
      expect(s.currentDifficulty, isNull);
    });
  });

  group('recordAnswer elimination', () {
    test('correct answer keeps the player alive', () {
      final p = _p('Ana');
      final s = TriviaSession.start([p, _p('Bob')]);
      final updated = s.recordAnswer(p, correct: true);
      expect(updated.alivePlayers, contains(p));
      expect(updated.eliminatedPlayers, isEmpty);
    });

    test('wrong answer eliminates the player', () {
      final p = _p('Ana');
      final s = TriviaSession.start([p, _p('Bob')]);
      final updated = s.recordAnswer(p, correct: false);
      expect(updated.alivePlayers, isNot(contains(p)));
      expect(updated.eliminatedPlayers, contains(p));
    });

    test('eliminating a player does not change the round', () {
      final p = _p('Ana');
      final s = TriviaSession.start([p, _p('Bob')]);
      final updated = s.recordAnswer(p, correct: false);
      expect(updated.currentRound, 0);
    });

    test('throws when player is not alive', () {
      final p = _p('Ana');
      var s = TriviaSession.start([p, _p('Bob')]);
      s = s.recordAnswer(p, correct: false); // eliminate Ana
      expect(() => s.recordAnswer(p, correct: true), throwsArgumentError);
    });

    test('throws when session is already over', () {
      final p = _p('Ana');
      var s = TriviaSession.start([p]);
      s = s.recordAnswer(p, correct: false); // all eliminated → isOver
      expect(() => s.recordAnswer(p, correct: false), throwsStateError);
    });
  });

  group('advanceRound', () {
    test('increments currentRound by 1', () {
      final s = TriviaSession.start(_players(2));
      expect(s.advanceRound().currentRound, 1);
    });

    test('throws when session is already over', () {
      var s = TriviaSession.start(_players(2));
      for (var i = 0; i < kTriviaRoundCount; i++) {
        s = s.advanceRound();
      }
      expect(s.isOver, isTrue);
      expect(() => s.advanceRound(), throwsStateError);
    });
  });

  group('isOver and winners', () {
    test('isOver is false before round 9', () {
      final s = TriviaSession.start(_players(2));
      expect(s.isOver, isFalse);
    });

    test('isOver is true after all 9 rounds', () {
      var s = TriviaSession.start(_players(2));
      for (var i = 0; i < kTriviaRoundCount; i++) {
        s = s.advanceRound();
      }
      expect(s.isOver, isTrue);
    });

    test('survivors of all 9 rounds tie as winners', () {
      final players = _players(3);
      var s = TriviaSession.start(players);
      // Eliminate P0 in round 0, keep P1 and P2 alive.
      s = s.recordAnswer(players[0], correct: false);
      for (var i = 0; i < kTriviaRoundCount; i++) {
        s = s.advanceRound();
      }
      expect(s.isOver, isTrue);
      expect(s.winners, containsAll([players[1], players[2]]));
      expect(s.winners.length, 2);
    });

    test('all eliminated in same round → empty winners', () {
      final players = _players(2);
      var s = TriviaSession.start(players);
      s = s.recordAnswer(players[0], correct: false);
      s = s.recordAnswer(players[1], correct: false);
      expect(s.isOver, isTrue);
      expect(s.winners, isEmpty);
    });

    test('isOver immediately when all players eliminated before round 9', () {
      final p = _p('Solo');
      var s = TriviaSession.start([p]);
      s = s.recordAnswer(p, correct: false);
      expect(s.isOver, isTrue);
    });
  });
}
