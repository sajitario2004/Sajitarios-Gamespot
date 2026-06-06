/// A trivia theme / topic (tematica).
///
/// Named [Tematica] (Spanish) to match the project's domain naming style and
/// to avoid colliding with Flutter's [ThemeData]. Pure value object — no
/// Flutter or persistence imports.
library;

/// A trivia theme identified by [id] and named [nombre].
///
/// Instances are immutable. Equality is based on [id] alone (two themes with
/// the same id are the same theme regardless of their name).
class Tematica {
  const Tematica({required this.id, required this.nombre});

  /// Stable, URL-safe identifier (e.g. "historia", "ciencia", "deportes").
  final String id;

  /// Human-readable name shown in the UI.
  final String nombre;

  @override
  bool operator ==(Object other) =>
      identical(this, other) || other is Tematica && other.id == id;

  @override
  int get hashCode => id.hashCode;

  @override
  String toString() => 'Tematica($id, $nombre)';
}
