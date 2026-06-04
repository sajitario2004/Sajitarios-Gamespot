import 'dart:math';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/core/random/random_provider.dart';

void main() {
  group('RandomProvider', () {
    test('con semilla fija reproduce la misma secuencia de nextInt', () {
      final a = RandomProvider.seeded(42);
      final b = RandomProvider.seeded(42);

      final seqA = List.generate(100, (_) => a.nextInt(1000));
      final seqB = List.generate(100, (_) => b.nextInt(1000));

      expect(seqA, equals(seqB));
    });

    test('con semilla fija reproduce la misma secuencia de nextDouble', () {
      final a = RandomProvider.seeded(7);
      final b = RandomProvider.seeded(7);

      final seqA = List.generate(100, (_) => a.nextDouble());
      final seqB = List.generate(100, (_) => b.nextDouble());

      expect(seqA, equals(seqB));
    });

    test('semillas distintas producen secuencias distintas', () {
      final a = RandomProvider.seeded(1);
      final b = RandomProvider.seeded(2);

      final seqA = List.generate(50, (_) => a.nextInt(1 << 30));
      final seqB = List.generate(50, (_) => b.nextInt(1 << 30));

      expect(seqA, isNot(equals(seqB)));
    });

    test('coincide con dart:math Random usando la misma semilla', () {
      const seed = 12345;
      final provider = RandomProvider.seeded(seed);
      final raw = Random(seed);

      for (var i = 0; i < 50; i++) {
        expect(provider.nextInt(500), equals(raw.nextInt(500)));
      }
    });

    test('nextInt siempre devuelve valores en [0, max)', () {
      final provider = RandomProvider.seeded(99);
      for (var i = 0; i < 1000; i++) {
        final value = provider.nextInt(10);
        expect(value, greaterThanOrEqualTo(0));
        expect(value, lessThan(10));
      }
    });

    test('nextDouble siempre devuelve valores en [0.0, 1.0)', () {
      final provider = RandomProvider.seeded(99);
      for (var i = 0; i < 1000; i++) {
        final value = provider.nextDouble();
        expect(value, greaterThanOrEqualTo(0.0));
        expect(value, lessThan(1.0));
      }
    });

    group('pick', () {
      test(
        'con semilla fija elige el mismo elemento de forma reproducible',
        () {
          final items = ['a', 'b', 'c', 'd', 'e'];
          final a = RandomProvider.seeded(33);
          final b = RandomProvider.seeded(33);

          final picksA = List.generate(20, (_) => a.pick(items));
          final picksB = List.generate(20, (_) => b.pick(items));

          expect(picksA, equals(picksB));
        },
      );

      test('siempre devuelve un elemento de la lista', () {
        final items = [10, 20, 30];
        final provider = RandomProvider.seeded(5);
        for (var i = 0; i < 100; i++) {
          expect(items, contains(provider.pick(items)));
        }
      });

      test('lanza ArgumentError con lista vacía', () {
        final provider = RandomProvider.seeded(0);
        expect(() => provider.pick<int>([]), throwsArgumentError);
      });
    });

    group('randomProvider (Riverpod)', () {
      test('puede sobreescribirse con una instancia con semilla fija', () {
        final container = ProviderContainer(
          overrides: [
            randomProvider.overrideWithValue(RandomProvider.seeded(2024)),
          ],
        );
        addTearDown(container.dispose);

        final injected = container.read(randomProvider);
        final reference = RandomProvider.seeded(2024);

        final seqInjected = List.generate(50, (_) => injected.nextInt(1000));
        final seqReference = List.generate(50, (_) => reference.nextInt(1000));

        expect(seqInjected, equals(seqReference));
      });

      test('por defecto expone una instancia de RandomProvider', () {
        final container = ProviderContainer();
        addTearDown(container.dispose);

        expect(container.read(randomProvider), isA<RandomProvider>());
      });
    });
  });
}
