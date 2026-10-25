/// A spectrum: two opposite concepts that define the poles of the dial.
///
/// Pure value object — no Flutter or persistence imports.
/// Corresponds to one row in the `wavelength_spectra` table (data layer),
/// but this type only carries the conceptual identity; persistence details
/// stay in the data layer.
library;

/// An immutable spectrum with a [leftConcept] and a [rightConcept] pole.
///
/// Both concepts must be non-empty after trimming whitespace. [id] is
/// `null` for transient/unsaved instances and positive after persistence.
class Spectrum {
  /// Creates a [Spectrum], trimming whitespace from both concepts.
  ///
  /// Throws [ArgumentError] if either concept is empty after trimming.
  factory Spectrum({
    required int? id,
    required String leftConcept,
    required String rightConcept,
  }) {
    final cleanLeft = leftConcept.trim();
    final cleanRight = rightConcept.trim();
    if (cleanLeft.isEmpty) {
      throw ArgumentError.value(
        leftConcept,
        'leftConcept',
        'El concepto izquierdo no puede estar vacío',
      );
    }
    if (cleanRight.isEmpty) {
      throw ArgumentError.value(
        rightConcept,
        'rightConcept',
        'El concepto derecho no puede estar vacío',
      );
    }
    return Spectrum._(id: id, leftConcept: cleanLeft, rightConcept: cleanRight);
  }

  const Spectrum._({
    required this.id,
    required this.leftConcept,
    required this.rightConcept,
  });

  /// Database id. `null` for instances not yet persisted.
  final int? id;

  /// The left-pole concept (e.g. "frío").
  final String leftConcept;

  /// The right-pole concept (e.g. "caliente").
  final String rightConcept;

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    if (other is! Spectrum) return false;
    return other.id == id &&
        other.leftConcept == leftConcept &&
        other.rightConcept == rightConcept;
  }

  @override
  int get hashCode => Object.hash(id, leftConcept, rightConcept);

  @override
  String toString() =>
      'Spectrum(id: $id, left: $leftConcept, right: $rightConcept)';
}
