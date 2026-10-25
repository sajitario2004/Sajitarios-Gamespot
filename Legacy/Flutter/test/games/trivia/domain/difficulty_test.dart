/// Tests for [Difficulty] enum helpers and round → difficulty mapping.
library;

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/trivia_session.dart';

void main() {
  group('Difficulty', () {
    group('fromOpenTdb', () {
      test('maps "easy" → facil', () {
        expect(Difficulty.fromOpenTdb('easy'), Difficulty.facil);
      });

      test('maps "medium" → dificil', () {
        expect(Difficulty.fromOpenTdb('medium'), Difficulty.dificil);
      });

      test('maps "hard" → muyDificil', () {
        expect(Difficulty.fromOpenTdb('hard'), Difficulty.muyDificil);
      });

      test('throws ArgumentError for unknown string', () {
        expect(() => Difficulty.fromOpenTdb('unknown'), throwsArgumentError);
      });
    });

    group('toOpenTdb', () {
      test('facil → "easy"', () {
        expect(Difficulty.facil.toOpenTdb(), 'easy');
      });

      test('dificil → "medium"', () {
        expect(Difficulty.dificil.toOpenTdb(), 'medium');
      });

      test('muyDificil → "hard"', () {
        expect(Difficulty.muyDificil.toOpenTdb(), 'hard');
      });
    });

    group('displayName', () {
      test('returns non-empty string for every tier', () {
        for (final d in Difficulty.values) {
          expect(d.displayName, isNotEmpty);
        }
      });
    });

    group('stable values order', () {
      test('facil < dificil < muyDificil by index', () {
        expect(Difficulty.facil.index, lessThan(Difficulty.dificil.index));
        expect(Difficulty.dificil.index, lessThan(Difficulty.muyDificil.index));
      });
    });
  });

  group('difficultyForRound', () {
    test('rounds 0–2 → facil', () {
      for (var i = 0; i < 3; i++) {
        expect(difficultyForRound(i), Difficulty.facil, reason: 'round $i');
      }
    });

    test('rounds 3–5 → dificil', () {
      for (var i = 3; i < 6; i++) {
        expect(difficultyForRound(i), Difficulty.dificil, reason: 'round $i');
      }
    });

    test('rounds 6–8 → muyDificil', () {
      for (var i = 6; i < 9; i++) {
        expect(
          difficultyForRound(i),
          Difficulty.muyDificil,
          reason: 'round $i',
        );
      }
    });

    test('throws for negative index', () {
      expect(() => difficultyForRound(-1), throwsArgumentError);
    });

    test('throws for index == kTriviaRoundCount', () {
      expect(() => difficultyForRound(kTriviaRoundCount), throwsArgumentError);
    });
  });
}
