/// Tests for [DrawStatementUseCase]: intensity filtering, no-repeat-until-exhausted,
/// determinism under seeded RandomProvider, and pool-reset behavior.
library;

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/draw_statement_use_case.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/intensidad.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/never_statement.dart';

NeverStatement _stmt(int id, Intensidad intensidad) =>
    NeverStatement.create(id: id, frase: 'Frase $id', intensidad: intensidad);

List<NeverStatement> _pool({int suave = 3, int picante = 3}) => [
  for (var i = 1; i <= suave; i++) _stmt(i, Intensidad.suave),
  for (var i = suave + 1; i <= suave + picante; i++)
    _stmt(i, Intensidad.picante),
];

void main() {
  group('DrawStatementUseCase', () {
    test(
      'only returns statements matching allowed intensidades (suave only)',
      () {
        final useCase = DrawStatementUseCase(RandomProvider.seeded(42));
        final pool = _pool(suave: 5, picante: 5);
        final seen = <int>{};

        for (var i = 0; i < 5; i++) {
          final result = useCase(
            pool: pool,
            intensidades: {Intensidad.suave},
            seen: seen,
          );
          expect(result.intensidad, Intensidad.suave);
        }
      },
    );

    test(
      'only returns statements matching allowed intensidades (picante only)',
      () {
        final useCase = DrawStatementUseCase(RandomProvider.seeded(7));
        final pool = _pool(suave: 5, picante: 5);
        final seen = <int>{};

        for (var i = 0; i < 5; i++) {
          final result = useCase(
            pool: pool,
            intensidades: {Intensidad.picante},
            seen: seen,
          );
          expect(result.intensidad, Intensidad.picante);
        }
      },
    );

    test('does not repeat until pool is exhausted', () {
      final useCase = DrawStatementUseCase(RandomProvider.seeded(1));
      final pool = _pool(suave: 4, picante: 0);
      final seen = <int>{};
      final drawn = <int>[];

      for (var i = 0; i < 4; i++) {
        final result = useCase(
          pool: pool,
          intensidades: {Intensidad.suave},
          seen: seen,
        );
        expect(drawn, isNot(contains(result.id)));
        drawn.add(result.id);
      }
      expect(drawn.length, 4);
    });

    test('resets seen set when pool is exhausted and allows re-draw', () {
      final useCase = DrawStatementUseCase(RandomProvider.seeded(99));
      final pool = _pool(suave: 2, picante: 0);
      final seen = <int>{};

      // Exhaust the 2-item pool.
      useCase(pool: pool, intensidades: {Intensidad.suave}, seen: seen);
      useCase(pool: pool, intensidades: {Intensidad.suave}, seen: seen);

      // seen should have both ids now; next draw must still work (reshuffles).
      expect(
        () => useCase(pool: pool, intensidades: {Intensidad.suave}, seen: seen),
        returnsNormally,
      );
    });

    test('is deterministic with a fixed seed', () {
      final pool = _pool(suave: 5, picante: 5);

      NeverStatement draw(int seed) {
        return DrawStatementUseCase(RandomProvider.seeded(seed))(
          pool: pool,
          intensidades: {Intensidad.suave, Intensidad.picante},
          seen: <int>{},
        );
      }

      expect(draw(42), equals(draw(42)));
    });

    test('different seeds may produce different results', () {
      final pool = _pool(suave: 6, picante: 6);

      final results = <int>{};
      for (var seed = 0; seed < 20; seed++) {
        final result = DrawStatementUseCase(RandomProvider.seeded(seed))(
          pool: pool,
          intensidades: {Intensidad.suave, Intensidad.picante},
          seen: <int>{},
        );
        results.add(result.id);
      }
      // With 12 distinct items and 20 different seeds, we expect more than 1
      // unique id to be drawn across all runs.
      expect(results.length, greaterThan(1));
    });

    test('throws when no statements match the given intensidades', () {
      final useCase = DrawStatementUseCase(RandomProvider.seeded(1));
      // Pool has only suave, but we request picante.
      final pool = _pool(suave: 3, picante: 0);
      expect(
        () => useCase(
          pool: pool,
          intensidades: {Intensidad.picante},
          seen: <int>{},
        ),
        throwsArgumentError,
      );
    });

    test('seen is updated after each draw', () {
      final useCase = DrawStatementUseCase(RandomProvider.seeded(5));
      final pool = _pool(suave: 3, picante: 0);
      final seen = <int>{};

      final first = useCase(
        pool: pool,
        intensidades: {Intensidad.suave},
        seen: seen,
      );
      expect(seen, contains(first.id));

      final second = useCase(
        pool: pool,
        intensidades: {Intensidad.suave},
        seen: seen,
      );
      expect(seen, contains(second.id));
      expect(first.id, isNot(equals(second.id)));
    });

    test('accepts both intensidades when pool contains both', () {
      final useCase = DrawStatementUseCase(RandomProvider.seeded(13));
      final pool = _pool(suave: 3, picante: 3);
      final seen = <int>{};
      final drawn = <NeverStatement>[];

      for (var i = 0; i < 6; i++) {
        drawn.add(
          useCase(
            pool: pool,
            intensidades: {Intensidad.suave, Intensidad.picante},
            seen: seen,
          ),
        );
      }

      final ids = drawn.map((s) => s.id).toSet();
      expect(ids.length, 6);
    });
  });
}
