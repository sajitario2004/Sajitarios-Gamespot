/// Modelo de dominio de una carta para el juego "Es un 10 pero".
///
/// Una carta solo puede tener valores del As (A) al 10 — **sin J/Q/K** — y uno
/// de los cuatro palos de la baraja francesa. Es un objeto de valor inmutable.
library;

/// Valor de una carta: del As (1) al 10. No existen figuras (J/Q/K).
enum CardValue {
  ace(1, 'A'),
  two(2, '2'),
  three(3, '3'),
  four(4, '4'),
  five(5, '5'),
  six(6, '6'),
  seven(7, '7'),
  eight(8, '8'),
  nine(9, '9'),
  ten(10, '10');

  const CardValue(this.number, this.label);

  /// Número de la carta en el rango `[1, 10]` (el As es 1).
  final int number;

  /// Etiqueta para mostrar: "A", "2", …, "10".
  final String label;

  /// Devuelve el [CardValue] cuyo [number] coincide con [number].
  ///
  /// Lanza [ArgumentError] si [number] está fuera del rango `[1, 10]`.
  static CardValue fromNumber(int number) {
    for (final value in CardValue.values) {
      if (value.number == number) return value;
    }
    throw ArgumentError.value(
      number,
      'number',
      'El valor de la carta debe estar entre 1 y 10',
    );
  }
}

/// Palo de una carta de la baraja francesa.
///
/// Tipo de dominio puro: solo conoce su nombre legible y si es rojo. El mapeo
/// palo -> icono de Flutter vive en la capa de presentación (ver
/// `presentation/card_flip_game.dart`, extensión `suitIcon`) para no acoplar el
/// dominio a Flutter.
enum CardSuit {
  espadas('Espadas'),
  corazones('Corazones'),
  diamantes('Diamantes'),
  treboles('Tréboles');

  const CardSuit(this.displayName);

  /// Nombre legible del palo: "Espadas", "Corazones", "Diamantes", "Tréboles".
  final String displayName;

  /// Palos rojos (corazones y diamantes); el resto son negros.
  bool get isRed => this == corazones || this == diamantes;
}

/// Una carta inmutable: un [value] (A–10) y un [suit].
class Card {
  const Card({required this.value, required this.suit});

  /// El valor de la carta (As a 10).
  final CardValue value;

  /// El palo de la carta.
  final CardSuit suit;

  /// Representación para mostrar, p. ej. "10 de Corazones" o "A de Espadas".
  String get label => '${value.label} de ${suit.displayName}';

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is Card && other.value == value && other.suit == suit;

  @override
  int get hashCode => Object.hash(value, suit);

  @override
  String toString() => 'Card($label)';
}
