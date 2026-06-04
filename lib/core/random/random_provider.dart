import 'dart:math';

import 'package:flutter_riverpod/flutter_riverpod.dart';

/// Fuente de aleatoriedad inyectable de la app.
///
/// Es un *wrapper* fino sobre [Random] de `dart:math`. Ningún use case debe
/// instanciar `Random()` directamente: toda la aleatoriedad pasa por aquí para
/// poder inyectar una semilla fija en tests y obtener resultados deterministas
/// (reproducibles) — ver `test/core/random/random_provider_test.dart`.
///
/// Para producción se usa [RandomProvider.secure] (entropía del SO) o el
/// constructor por defecto. Para tests, [RandomProvider.seeded].
class RandomProvider {
  RandomProvider(this._random);

  /// Crea un proveedor con una fuente aleatoria no determinista.
  factory RandomProvider.system() => RandomProvider(Random());

  /// Crea un proveedor con una semilla fija: produce siempre la misma
  /// secuencia de valores. Pensado para tests deterministas.
  factory RandomProvider.seeded(int seed) => RandomProvider(Random(seed));

  /// Crea un proveedor respaldado por una fuente criptográficamente segura.
  factory RandomProvider.secure() => RandomProvider(Random.secure());

  final Random _random;

  /// Entero no negativo uniforme en el rango `[0, max)`.
  /// [max] debe ser positivo y `<= 2^32`.
  int nextInt(int max) => _random.nextInt(max);

  /// Double uniforme en el rango `[0.0, 1.0)`.
  double nextDouble() => _random.nextDouble();

  /// Valor booleano aleatorio.
  bool nextBool() => _random.nextBool();

  /// Devuelve un elemento aleatorio de [items].
  ///
  /// Lanza [ArgumentError] si la lista está vacía.
  T pick<T>(List<T> items) {
    if (items.isEmpty) {
      throw ArgumentError.value(
        items,
        'items',
        'La lista no puede estar vacía',
      );
    }
    return items[_random.nextInt(items.length)];
  }
}

/// Provider de Riverpod que expone el [RandomProvider] de la app.
///
/// Por defecto usa una fuente no determinista. En tests se sobreescribe con
/// `ProviderScope(overrides: [randomProvider.overrideWithValue(...)])` para
/// inyectar una semilla fija.
final randomProvider = Provider<RandomProvider>(
  (ref) => RandomProvider.system(),
);
