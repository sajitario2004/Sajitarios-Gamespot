/// La palabra elegida para una partida del Impostor, con su pista.
///
/// Es un *value object* de dominio puro: solo el texto y la pista, sin los
/// detalles de persistencia (id, is_seed, created_at) que sí lleva
/// `ImpostorWord` en la capa de datos. Mantenerlo separado evita que el dominio
/// dependa de cómo se guardan las palabras.
///
/// La pista es **obligatoria** (regla del juego: toda palabra lleva pista) y se
/// muestra al impostor solo si la opción de pista está activada en la partida.
///
/// El dominio no conoce la capa de datos: la conversión desde `ImpostorWord`
/// vive en la frontera de datos (ver `ImpostorWordX.toDomain` en `data/`).
library;

/// Una palabra de la partida ([text]) con su [hint] (pista) obligatoria.
class Word {
  /// Crea una palabra de dominio.
  ///
  /// Recorta espacios de [text] y [hint] y lanza [ArgumentError] si alguno
  /// queda vacío (toda palabra debe tener texto y pista).
  factory Word({required String text, required String hint}) {
    final cleanText = text.trim();
    final cleanHint = hint.trim();
    if (cleanText.isEmpty) {
      throw ArgumentError.value(
        text,
        'text',
        'La palabra no puede estar vacía',
      );
    }
    if (cleanHint.isEmpty) {
      throw ArgumentError.value(hint, 'hint', 'La pista no puede estar vacía');
    }
    return Word._(cleanText, cleanHint);
  }

  const Word._(this.text, this.hint);

  /// El texto de la palabra.
  final String text;

  /// La pista asociada a la palabra (siempre presente).
  final String hint;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is Word && other.text == text && other.hint == hint;

  @override
  int get hashCode => Object.hash(text, hint);

  @override
  String toString() => 'Word($text, hint: $hint)';
}
