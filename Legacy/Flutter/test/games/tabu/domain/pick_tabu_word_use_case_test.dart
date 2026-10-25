/// Tests para [PickTabuWordUseCase]: seleccion determinista y sin repeticion
/// usando [RandomProvider] con semilla fija.
library;

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/tabu/domain/pick_tabu_word_use_case.dart';
import 'package:sajitarios_gamespot/games/tabu/domain/tabu_word.dart';

TabuWord _word(int id) => TabuWord.create(
  id: id,
  palabra: 'Palabra $id',
  prohibidas: ['a', 'b', 'c', 'd'],
);

List<TabuWord> _pool(int count) =>
    List.generate(count, (i) => _word(i + 1), growable: false);

void main() {
  group('PickTabuWordUseCase', () {
    test('devuelve una palabra del pool cuando no hay usadas', () {
      final useCase = PickTabuWordUseCase(RandomProvider.seeded(42));
      final pool = _pool(5);
      final picked = useCase(pool: pool, usadas: {});
      expect(pool, contains(picked));
    });

    test('nunca devuelve una palabra ya usada', () {
      final useCase = PickTabuWordUseCase(RandomProvider.seeded(42));
      final pool = _pool(5);
      // Marcamos las primeras 4 como usadas; solo queda la id=5
      final picked = useCase(pool: pool, usadas: {1, 2, 3, 4});
      expect(picked.id, 5);
    });

    test('es determinista con la misma semilla', () {
      final pool = _pool(10);
      final r1 = PickTabuWordUseCase(RandomProvider.seeded(7))(
        pool: pool,
        usadas: {},
      );
      final r2 = PickTabuWordUseCase(RandomProvider.seeded(7))(
        pool: pool,
        usadas: {},
      );
      expect(r1, equals(r2));
    });

    test('semillas distintas pueden producir resultados distintos', () {
      final pool = _pool(20);
      final results = <int>{};
      for (var seed = 0; seed < 20; seed++) {
        final picked = PickTabuWordUseCase(RandomProvider.seeded(seed))(
          pool: pool,
          usadas: {},
        );
        results.add(picked.id);
      }
      // Con 20 semillas sobre un pool de 20 palabras esperamos al menos 2 ids
      // distintos (probabilidad de que todas caigan en el mismo es negligible).
      expect(results.length, greaterThan(1));
    });

    test('lanza ArgumentError si el pool esta vacio', () {
      final useCase = PickTabuWordUseCase(RandomProvider.seeded(1));
      expect(() => useCase(pool: [], usadas: {}), throwsArgumentError);
    });

    test('lanza ArgumentError si todas las palabras del pool estan usadas', () {
      final useCase = PickTabuWordUseCase(RandomProvider.seeded(1));
      final pool = _pool(3);
      expect(() => useCase(pool: pool, usadas: {1, 2, 3}), throwsArgumentError);
    });

    test('no consume palabras ya usadas con pool grande', () {
      final useCase = PickTabuWordUseCase(RandomProvider.seeded(99));
      final pool = _pool(10);
      final usadas = <int>{};

      // Simula escoger todas las palabras una a una sin repeticion
      for (var i = 0; i < 10; i++) {
        final picked = useCase(pool: pool, usadas: usadas);
        expect(usadas, isNot(contains(picked.id)));
        usadas.add(picked.id);
      }
      expect(usadas.length, 10);
    });
  });
}
