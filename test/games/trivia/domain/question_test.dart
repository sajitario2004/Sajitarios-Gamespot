/// Tests for [Question] value object validation and helpers.
library;

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/question.dart';

List<String> _fourOptions() => ['A', 'B', 'C', 'D'];

Question _validQuestion({int correctIndex = 0}) => Question.create(
  id: 1,
  tematicaId: 'historia',
  difficulty: Difficulty.facil,
  enunciado: '¿Quién pintó la Mona Lisa?',
  options: _fourOptions(),
  correctIndex: correctIndex,
);

void main() {
  group('Question.create validation', () {
    test('accepts exactly 4 options', () {
      expect(() => _validQuestion(), returnsNormally);
    });

    test('rejects fewer than 4 options', () {
      expect(
        () => Question.create(
          id: 1,
          tematicaId: 'x',
          difficulty: Difficulty.facil,
          enunciado: 'Q',
          options: ['A', 'B', 'C'],
          correctIndex: 0,
        ),
        throwsArgumentError,
      );
    });

    test('rejects more than 4 options', () {
      expect(
        () => Question.create(
          id: 1,
          tematicaId: 'x',
          difficulty: Difficulty.facil,
          enunciado: 'Q',
          options: ['A', 'B', 'C', 'D', 'E'],
          correctIndex: 0,
        ),
        throwsArgumentError,
      );
    });

    test('rejects correctIndex < 0', () {
      expect(
        () => Question.create(
          id: 1,
          tematicaId: 'x',
          difficulty: Difficulty.facil,
          enunciado: 'Q',
          options: _fourOptions(),
          correctIndex: -1,
        ),
        throwsArgumentError,
      );
    });

    test('rejects correctIndex == 4 (out of range)', () {
      expect(
        () => Question.create(
          id: 1,
          tematicaId: 'x',
          difficulty: Difficulty.facil,
          enunciado: 'Q',
          options: _fourOptions(),
          correctIndex: 4,
        ),
        throwsArgumentError,
      );
    });

    test('accepts correctIndex at boundary 0', () {
      expect(() => _validQuestion(correctIndex: 0), returnsNormally);
    });

    test('accepts correctIndex at boundary 3', () {
      expect(() => _validQuestion(correctIndex: 3), returnsNormally);
    });
  });

  group('Question.isCorrect', () {
    test('returns true when chosen index matches correctIndex', () {
      final q = _validQuestion(correctIndex: 2);
      expect(q.isCorrect(2), isTrue);
    });

    test('returns false for wrong index', () {
      final q = _validQuestion(correctIndex: 2);
      expect(q.isCorrect(1), isFalse);
    });
  });

  group('Question equality and hashCode', () {
    test('identical instances are equal', () {
      final q = _validQuestion();
      expect(q, equals(q));
    });

    test('two questions with same fields are equal', () {
      final a = _validQuestion();
      final b = _validQuestion();
      expect(a, equals(b));
      expect(a.hashCode, equals(b.hashCode));
    });

    test('different id makes questions unequal', () {
      final a = _validQuestion();
      final b = Question.create(
        id: 2,
        tematicaId: 'historia',
        difficulty: Difficulty.facil,
        enunciado: '¿Quién pintó la Mona Lisa?',
        options: _fourOptions(),
        correctIndex: 0,
      );
      expect(a, isNot(equals(b)));
    });
  });
}
