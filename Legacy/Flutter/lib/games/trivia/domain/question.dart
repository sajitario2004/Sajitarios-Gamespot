/// An immutable trivia question — pure domain, no Flutter or persistence
/// imports.
library;

import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';

/// A trivia question with exactly 4 answer options.
///
/// Constructed via [Question.create] which validates the invariants and
/// throws [ArgumentError] on violations. Equality / hashCode are based on
/// all fields (value object).
class Question {
  const Question._({
    required this.id,
    required this.tematicaId,
    required this.difficulty,
    required this.enunciado,
    required this.options,
    required this.correctIndex,
  });

  /// Unique identifier (SQLite autoincrement from the data layer).
  final int id;

  /// The id of the [Tematica] this question belongs to.
  final String tematicaId;

  /// Difficulty tier.
  final Difficulty difficulty;

  /// The question text.
  final String enunciado;

  /// Exactly 4 answer options. Index 0..3.
  final List<String> options;

  /// Index of the correct answer in [options]. Must be in [0, 4).
  final int correctIndex;

  /// Creates a [Question] after validating [options] length and
  /// [correctIndex] bounds.
  ///
  /// Throws [ArgumentError] when:
  /// - [options] does not have exactly 4 elements.
  /// - [correctIndex] is not in the range [0, 4).
  factory Question.create({
    required int id,
    required String tematicaId,
    required Difficulty difficulty,
    required String enunciado,
    required List<String> options,
    required int correctIndex,
  }) {
    if (options.length != 4) {
      throw ArgumentError.value(
        options.length,
        'options',
        'A question must have exactly 4 options, got ${options.length}',
      );
    }
    if (correctIndex < 0 || correctIndex >= 4) {
      throw ArgumentError.value(
        correctIndex,
        'correctIndex',
        'correctIndex must be in [0, 4), got $correctIndex',
      );
    }
    return Question._(
      id: id,
      tematicaId: tematicaId,
      difficulty: difficulty,
      enunciado: enunciado,
      options: List<String>.unmodifiable(options),
      correctIndex: correctIndex,
    );
  }

  /// Returns `true` if [chosenIndex] matches the [correctIndex].
  bool isCorrect(int chosenIndex) => chosenIndex == correctIndex;

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    if (other is! Question) return false;
    if (other.id != id) return false;
    if (other.tematicaId != tematicaId) return false;
    if (other.difficulty != difficulty) return false;
    if (other.enunciado != enunciado) return false;
    if (other.correctIndex != correctIndex) return false;
    if (other.options.length != options.length) return false;
    for (var i = 0; i < options.length; i++) {
      if (other.options[i] != options[i]) return false;
    }
    return true;
  }

  @override
  int get hashCode => Object.hash(
    id,
    tematicaId,
    difficulty,
    enunciado,
    Object.hashAll(options),
    correctIndex,
  );

  @override
  String toString() =>
      'Question(id: $id, tematicaId: $tematicaId, '
      'difficulty: $difficulty, enunciado: $enunciado, '
      'correctIndex: $correctIndex)';
}
