/// Difficulty tiers for a trivia question.
///
/// Pure domain enum — no Flutter or persistence imports. Maps to the
/// Open Trivia DB difficulty strings (easy / medium / hard) so the
/// repository can round-trip the value without depending on this enum's
/// name directly.
library;

/// The three difficulty tiers used in a trivia session.
///
/// Declared in increasing order so [Difficulty.values] gives the natural
/// progression: [facil] → [dificil] → [muyDificil].
enum Difficulty {
  /// Easy — Open Trivia DB: "easy".
  facil,

  /// Medium — Open Trivia DB: "medium".
  dificil,

  /// Hard — Open Trivia DB: "hard".
  muyDificil;

  /// Human-readable Spanish label.
  String get displayName => switch (this) {
    Difficulty.facil => 'Fácil',
    Difficulty.dificil => 'Difícil',
    Difficulty.muyDificil => 'Muy difícil',
  };

  /// Builds a [Difficulty] from an Open Trivia DB difficulty string.
  ///
  /// Throws [ArgumentError] for unrecognized values.
  static Difficulty fromOpenTdb(String value) => switch (value) {
    'easy' => Difficulty.facil,
    'medium' => Difficulty.dificil,
    'hard' => Difficulty.muyDificil,
    _ => throw ArgumentError.value(
      value,
      'value',
      'Open Trivia DB difficulty not recognized: $value',
    ),
  };

  /// Returns the Open Trivia DB string for this difficulty.
  String toOpenTdb() => switch (this) {
    Difficulty.facil => 'easy',
    Difficulty.dificil => 'medium',
    Difficulty.muyDificil => 'hard',
  };
}
