import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/es_un_10_pero/domain/card.dart';
import 'package:sajitarios_gamespot/games/es_un_10_pero/domain/draw_card_use_case.dart';

void main() {
  group('DrawCardUseCase', () {
    test('siempre devuelve una carta con valor en el rango [1, 10]', () {
      final useCase = DrawCardUseCase(RandomProvider.seeded(42));

      for (var i = 0; i < 2000; i++) {
        final card = useCase();
        expect(card.value.number, greaterThanOrEqualTo(1));
        expect(card.value.number, lessThanOrEqualTo(10));
      }
    });

    test('siempre devuelve un valor perteneciente a CardValue.values', () {
      final useCase = DrawCardUseCase(RandomProvider.seeded(7));

      for (var i = 0; i < 2000; i++) {
        expect(CardValue.values, contains(useCase().value));
      }
    });

    test('siempre devuelve uno de los 4 palos válidos', () {
      final useCase = DrawCardUseCase(RandomProvider.seeded(123));

      for (var i = 0; i < 2000; i++) {
        expect(CardSuit.values, contains(useCase().suit));
      }
      // Confirmamos que solo existen 4 palos.
      expect(CardSuit.values, hasLength(4));
    });

    test('la etiqueta del valor es correcta, incluido "A" para el As', () {
      const labelByNumber = {
        1: 'A',
        2: '2',
        3: '3',
        4: '4',
        5: '5',
        6: '6',
        7: '7',
        8: '8',
        9: '9',
        10: '10',
      };

      final useCase = DrawCardUseCase(RandomProvider.seeded(55));

      for (var i = 0; i < 2000; i++) {
        final card = useCase();
        expect(card.value.label, equals(labelByNumber[card.value.number]));
        if (card.value == CardValue.ace) {
          expect(card.value.label, equals('A'));
          expect(card.value.number, equals(1));
        }
      }
    });

    test('la etiqueta de la carta combina valor y nombre del palo', () {
      final useCase = DrawCardUseCase(RandomProvider.seeded(88));

      for (var i = 0; i < 1000; i++) {
        final card = useCase();
        expect(
          card.label,
          equals('${card.value.label} de ${card.suit.displayName}'),
        );
      }
    });

    test('nunca devuelve figuras J/Q/K', () {
      final useCase = DrawCardUseCase(RandomProvider.seeded(2024));

      const figuras = {'J', 'Q', 'K'};
      for (var i = 0; i < 5000; i++) {
        final card = useCase();
        expect(figuras.contains(card.value.label), isFalse);
        // Las figuras nunca aparecen porque el dominio no las define.
        expect(card.value.number, inInclusiveRange(1, 10));
      }
    });

    test('es reproducible con la misma semilla', () {
      final a = DrawCardUseCase(RandomProvider.seeded(2025));
      final b = DrawCardUseCase(RandomProvider.seeded(2025));

      final cardsA = List.generate(200, (_) => a());
      final cardsB = List.generate(200, (_) => b());

      expect(cardsA, equals(cardsB));
    });

    test('semillas distintas producen secuencias de cartas distintas', () {
      final a = DrawCardUseCase(RandomProvider.seeded(1));
      final b = DrawCardUseCase(RandomProvider.seeded(2));

      final cardsA = List.generate(200, (_) => a());
      final cardsB = List.generate(200, (_) => b());

      expect(cardsA, isNot(equals(cardsB)));
    });

    test('cubre razonablemente el espacio de valores y palos', () {
      final useCase = DrawCardUseCase(RandomProvider.seeded(31337));

      final seenValues = <CardValue>{};
      final seenSuits = <CardSuit>{};

      for (var i = 0; i < 5000; i++) {
        final card = useCase();
        seenValues.add(card.value);
        seenSuits.add(card.suit);
      }

      // Con 5000 tiradas sobre 40 combinaciones, deben aparecer
      // los 10 valores y los 4 palos.
      expect(seenValues, hasLength(CardValue.values.length));
      expect(seenSuits, hasLength(CardSuit.values.length));
    });

    test('genera la combinación completa de 40 cartas posibles', () {
      final useCase = DrawCardUseCase(RandomProvider.seeded(909090));

      final combos = <String>{};
      for (var i = 0; i < 20000; i++) {
        final card = useCase();
        combos.add('${card.value.name}-${card.suit.name}');
      }

      // 10 valores x 4 palos = 40 combinaciones únicas posibles.
      expect(combos, hasLength(40));
    });
  });
}
