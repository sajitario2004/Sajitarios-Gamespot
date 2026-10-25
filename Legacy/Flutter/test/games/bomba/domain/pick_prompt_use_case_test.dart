/// Tests for [PickPromptUseCase]: determinism, no-repeat, pool exhaustion reset.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_mode.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_prompt.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/pick_prompt_use_case.dart';

List<BombaPrompt> _silabas(int count) => List.generate(
  count,
  (i) => BombaPrompt.create(
    id: i + 1,
    texto: 'SI${i + 1}',
    mode: BombaMode.silaba,
  ),
);

List<BombaPrompt> _categorias(int count) => List.generate(
  count,
  (i) => BombaPrompt.create(
    id: i + 100,
    texto: 'Cat${i + 1}',
    mode: BombaMode.categoria,
  ),
);

void main() {
  group('PickPromptUseCase — determinismo', () {
    test('misma semilla produce el mismo prompt', () {
      final pool = _silabas(10);
      final uc1 = PickPromptUseCase(RandomProvider.seeded(42));
      final uc2 = PickPromptUseCase(RandomProvider.seeded(42));
      expect(
        uc1.pick(mode: BombaMode.silaba, pool: pool).id,
        uc2.pick(mode: BombaMode.silaba, pool: pool).id,
      );
    });
  });

  group('PickPromptUseCase — sin repeticion', () {
    test('todos los prompts se producen antes de repetir (pool de 5)', () {
      final pool = _silabas(5);
      final uc = PickPromptUseCase(RandomProvider.seeded(7));
      final seen = <int>{};
      for (var i = 0; i < pool.length; i++) {
        final p = uc.pick(mode: BombaMode.silaba, pool: pool);
        expect(seen, isNot(contains(p.id)), reason: 'Repeticion en el pick $i');
        seen.add(p.id);
      }
      expect(seen.length, pool.length);
    });

    test('el pool se reinicia al agotarse', () {
      final pool = _silabas(3);
      final uc = PickPromptUseCase(RandomProvider.seeded(1));
      // Agota el pool
      for (var i = 0; i < pool.length; i++) {
        uc.pick(mode: BombaMode.silaba, pool: pool);
      }
      // El siguiente pick no debe lanzar
      expect(
        () => uc.pick(mode: BombaMode.silaba, pool: pool),
        returnsNormally,
      );
    });

    test('los pools de silaba y categoria son independientes', () {
      final silabas = _silabas(3);
      final categorias = _categorias(3);
      final combined = [...silabas, ...categorias];
      final uc = PickPromptUseCase(RandomProvider.seeded(5));

      final seenSilabas = <int>{};
      final seenCats = <int>{};

      for (var i = 0; i < 3; i++) {
        final s = uc.pick(mode: BombaMode.silaba, pool: combined);
        expect(s.mode, BombaMode.silaba);
        expect(seenSilabas, isNot(contains(s.id)));
        seenSilabas.add(s.id);

        final c = uc.pick(mode: BombaMode.categoria, pool: combined);
        expect(c.mode, BombaMode.categoria);
        expect(seenCats, isNot(contains(c.id)));
        seenCats.add(c.id);
      }
    });
  });

  group('PickPromptUseCase — errores', () {
    test('lanza ArgumentError si no hay prompts para el modo', () {
      final pool = _silabas(3);
      final uc = PickPromptUseCase(RandomProvider.seeded(0));
      expect(
        () => uc.pick(mode: BombaMode.categoria, pool: pool),
        throwsArgumentError,
      );
    });

    test('lanza ArgumentError si el pool esta vacio', () {
      final uc = PickPromptUseCase(RandomProvider.seeded(0));
      expect(
        () => uc.pick(mode: BombaMode.silaba, pool: []),
        throwsArgumentError,
      );
    });
  });

  group('PickPromptUseCase — reset', () {
    test('resetMode reinicia el ciclo para un modo', () {
      final pool = _silabas(2);
      final uc = PickPromptUseCase(RandomProvider.seeded(0));
      uc.pick(mode: BombaMode.silaba, pool: pool);
      uc.pick(mode: BombaMode.silaba, pool: pool); // pool agotado
      uc.resetMode(BombaMode.silaba);
      // No debe lanzar — pool disponible de nuevo
      expect(
        () => uc.pick(mode: BombaMode.silaba, pool: pool),
        returnsNormally,
      );
    });

    test('resetAll reinicia todos los modos', () {
      final silabas = _silabas(2);
      final categorias = _categorias(2);
      final combined = [...silabas, ...categorias];
      final uc = PickPromptUseCase(RandomProvider.seeded(0));
      // Agota ambos modos
      for (var i = 0; i < 2; i++) {
        uc.pick(mode: BombaMode.silaba, pool: combined);
        uc.pick(mode: BombaMode.categoria, pool: combined);
      }
      uc.resetAll();
      expect(
        () => uc.pick(mode: BombaMode.silaba, pool: combined),
        returnsNormally,
      );
      expect(
        () => uc.pick(mode: BombaMode.categoria, pool: combined),
        returnsNormally,
      );
    });
  });
}
