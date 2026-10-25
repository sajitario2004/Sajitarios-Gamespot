/// Caso de uso: seleccionar una palabra de Tabu no usada en la partida actual.
///
/// Puro de dominio — sin imports de Flutter ni persistencia. Toda la
/// aleatoriedad se inyecta via [RandomProvider] para que los tests puedan usar
/// una semilla fija.
library;

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/tabu/domain/tabu_word.dart';

/// Elige aleatoriamente una [TabuWord] del [pool] que no este en [usadas].
///
/// Lanza [ArgumentError] si:
/// - [pool] esta vacio.
/// - Todas las palabras del [pool] ya estan en [usadas] (sin palabras
///   disponibles).
class PickTabuWordUseCase {
  const PickTabuWordUseCase(this._random);

  final RandomProvider _random;

  /// Devuelve una palabra aleatoria de [pool] que no pertenezca a [usadas].
  ///
  /// [pool] — todas las palabras disponibles para esta partida.
  /// [usadas] — IDs de palabras ya utilizadas en la partida actual.
  TabuWord call({required List<TabuWord> pool, required Set<int> usadas}) {
    if (pool.isEmpty) {
      throw ArgumentError.value(
        pool,
        'pool',
        'El pool de palabras no puede estar vacio',
      );
    }

    final disponibles = pool.where((w) => !usadas.contains(w.id)).toList();

    if (disponibles.isEmpty) {
      throw ArgumentError(
        'No hay palabras disponibles: todas las palabras del pool '
        '(${pool.length}) ya fueron usadas',
      );
    }

    return disponibles[_random.nextInt(disponibles.length)];
  }
}
