/// Caso de uso: sacar una declaración aleatoria sin repetir hasta agotar el pool.
///
/// Puro dominio — sin imports de Flutter ni persistencia. Toda la aleatoriedad
/// se inyecta vía [RandomProvider] para que los tests puedan usar una semilla
/// fija y obtener resultados deterministas.
library;

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/intensidad.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/never_statement.dart';

/// Extrae una declaración aleatoria del [pool] filtrada por [intensidades],
/// garantizando que no se repita ninguna hasta agotar todas las disponibles.
///
/// Algoritmo:
/// 1. Filtra [pool] por las [intensidades] permitidas.
/// 2. Elimina de la lista filtrada las que ya aparecen en [seen].
/// 3. Si el pool restante queda vacío (se agotó) se re-inicializa [seen] y
///    se usa el pool filtrado completo (rebaraje implícito via [RandomProvider]).
/// 4. Selecciona un índice aleatorio dentro del pool restante.
///
/// Lanza [ArgumentError] si el pool filtrado está vacío (no hay declaraciones
/// para las intensidades solicitadas).
class DrawStatementUseCase {
  const DrawStatementUseCase(this._random);

  final RandomProvider _random;

  /// Devuelve una [NeverStatement] aleatoria y actualiza [seen] en el acto.
  ///
  /// [pool] — todas las declaraciones disponibles (sin filtrar).
  /// [intensidades] — niveles permitidos para esta sesión.
  /// [seen] — conjunto mutable de ids ya mostrados; se modifica in-place
  ///   para registrar la declaración sacada (y se limpia cuando se agota el pool).
  ///
  /// Lanza [ArgumentError] si no hay ninguna declaración para las
  /// [intensidades] solicitadas.
  NeverStatement call({
    required List<NeverStatement> pool,
    required Set<Intensidad> intensidades,
    required Set<int> seen,
  }) {
    final filtered = pool
        .where((s) => intensidades.contains(s.intensidad))
        .toList(growable: false);

    if (filtered.isEmpty) {
      throw ArgumentError(
        'No hay declaraciones para las intensidades: $intensidades',
      );
    }

    var remaining = filtered
        .where((s) => !seen.contains(s.id))
        .toList(growable: false);

    // Pool agotado: reiniciar seen y usar el pool filtrado completo.
    if (remaining.isEmpty) {
      seen.clear();
      remaining = filtered;
    }

    final index = _random.nextInt(remaining.length);
    final statement = remaining[index];
    seen.add(statement.id);
    return statement;
  }
}
