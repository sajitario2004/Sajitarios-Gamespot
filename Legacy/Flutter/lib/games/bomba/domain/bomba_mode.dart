/// Modes for La Bomba: a syllable prompt or a category prompt.
library;

/// The type of prompt shown to players during a round of La Bomba.
enum BombaMode {
  /// Players must say a word that contains the given syllable/fragment.
  silaba,

  /// Players must say a word that belongs to the given category.
  categoria,
}
