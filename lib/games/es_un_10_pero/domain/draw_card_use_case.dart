import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/es_un_10_pero/domain/card.dart';

/// Use case puro que saca una [Card] aleatoria válida del juego
/// "Es un 10 pero".
///
/// La carta resultante siempre tiene un [CardValue] en el rango A–10 y uno de
/// los cuatro [CardSuit]. La aleatoriedad se obtiene **exclusivamente** del
/// [RandomProvider] inyectado (nunca de `Random()` directo), de modo que en
/// tests se puede inyectar una semilla fija y obtener resultados deterministas.
class DrawCardUseCase {
  const DrawCardUseCase(this._random);

  final RandomProvider _random;

  /// Saca una carta aleatoria: un valor A-10 con uno de los cuatro palos.
  Card call() {
    final value = _random.pick(CardValue.values);
    final suit = _random.pick(CardSuit.values);
    return Card(value: value, suit: suit);
  }
}
