/// Immutable domain model for a La Bomba prompt — pure domain, no Flutter or
/// persistence imports.
library;

import 'package:sajitarios_gamespot/games/bomba/domain/bomba_mode.dart';

/// A prompt shown to players during a La Bomba round.
///
/// For [BombaMode.silaba] the [texto] is a syllable or fragment (e.g. "CA").
/// For [BombaMode.categoria] the [texto] is a category description
/// (e.g. "nombres de animales").
///
/// Constructed via [BombaPrompt.create] which validates invariants and throws
/// [ArgumentError] on violations. Equality / hashCode are value-based.
class BombaPrompt {
  const BombaPrompt._({
    required this.id,
    required this.texto,
    required this.mode,
  });

  /// Unique identifier (SQLite autoincrement from the data layer).
  final int id;

  /// The syllable/fragment or category label shown to players.
  final String texto;

  /// Whether this is a syllable prompt or a category prompt.
  final BombaMode mode;

  /// Creates a [BombaPrompt] after validating that [texto] is non-empty.
  ///
  /// Throws [ArgumentError] when [texto] is empty after trimming.
  factory BombaPrompt.create({
    required int id,
    required String texto,
    required BombaMode mode,
  }) {
    final trimmed = texto.trim();
    if (trimmed.isEmpty) {
      throw ArgumentError.value(
        texto,
        'texto',
        'BombaPrompt.texto must not be empty',
      );
    }
    return BombaPrompt._(id: id, texto: trimmed, mode: mode);
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    if (other is! BombaPrompt) return false;
    return other.id == id && other.texto == texto && other.mode == mode;
  }

  @override
  int get hashCode => Object.hash(id, texto, mode);

  @override
  String toString() => 'BombaPrompt(id: $id, mode: $mode, texto: "$texto")';
}
