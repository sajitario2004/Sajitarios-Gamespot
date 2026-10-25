/// A player in a trivia session — pure value object by name.
///
/// Mirrors the style of `Player` in the Impostor bounded context: immutable,
/// equality by name, no Flutter or persistence dependencies.
library;

/// A trivia player identified by their [name].
class TriviaPlayer {
  const TriviaPlayer(this.name);

  /// The player's name as entered in the configuration.
  final String name;

  @override
  bool operator ==(Object other) =>
      identical(this, other) || other is TriviaPlayer && other.name == name;

  @override
  int get hashCode => name.hashCode;

  @override
  String toString() => 'TriviaPlayer($name)';
}
