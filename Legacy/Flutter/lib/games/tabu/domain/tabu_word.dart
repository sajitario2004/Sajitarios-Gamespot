/// Palabra de Tabu: inmutable, pura de dominio, sin imports de Flutter ni
/// persistencia.
///
/// Cada [TabuWord] tiene una [palabra] secreta que el descriptor no puede
/// mencionar, y entre 4 y 5 [prohibidas] (las palabras que tampoco puede
/// decir).
library;

/// Palabra de Tabu con sus palabras prohibidas.
///
/// Solo se puede construir a través de [TabuWord.create], que valida los
/// invariantes y lanza [ArgumentError] en caso de violacion. Igualdad /
/// hashCode basados en todos los campos (value object).
class TabuWord {
  const TabuWord._({
    required this.id,
    required this.palabra,
    required this.prohibidas,
  });

  /// Identificador unico (AUTOINCREMENT de SQLite en la capa de datos).
  final int id;

  /// La palabra secreta que el descriptor debe hacer adivinar.
  final String palabra;

  /// Lista de palabras prohibidas (4 o 5 elementos, ninguna vacia).
  final List<String> prohibidas;

  /// Construye un [TabuWord] validando los invariantes.
  ///
  /// Lanza [ArgumentError] cuando:
  /// - [palabra] esta vacia tras recortar espacios.
  /// - [prohibidas] no tiene entre 4 y 5 elementos.
  /// - Alguna palabra prohibida esta vacia tras recortar espacios.
  factory TabuWord.create({
    required int id,
    required String palabra,
    required List<String> prohibidas,
  }) {
    final palabraTrimmed = palabra.trim();
    if (palabraTrimmed.isEmpty) {
      throw ArgumentError.value(
        palabra,
        'palabra',
        'La palabra no puede estar vacia',
      );
    }
    if (prohibidas.length < 4 || prohibidas.length > 5) {
      throw ArgumentError.value(
        prohibidas.length,
        'prohibidas',
        'Debe haber entre 4 y 5 palabras prohibidas, hay ${prohibidas.length}',
      );
    }
    for (final p in prohibidas) {
      if (p.trim().isEmpty) {
        throw ArgumentError.value(
          p,
          'prohibidas',
          'Ninguna palabra prohibida puede estar vacia',
        );
      }
    }
    return TabuWord._(
      id: id,
      palabra: palabraTrimmed,
      prohibidas: List<String>.unmodifiable(
        prohibidas.map((p) => p.trim()).toList(),
      ),
    );
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    if (other is! TabuWord) return false;
    if (other.id != id) return false;
    if (other.palabra != palabra) return false;
    if (other.prohibidas.length != prohibidas.length) return false;
    for (var i = 0; i < prohibidas.length; i++) {
      if (other.prohibidas[i] != prohibidas[i]) return false;
    }
    return true;
  }

  @override
  int get hashCode => Object.hash(id, palabra, Object.hashAll(prohibidas));

  @override
  String toString() =>
      'TabuWord(id: $id, palabra: $palabra, prohibidas: $prohibidas)';
}
