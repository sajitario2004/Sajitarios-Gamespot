/// Tests for [WavelengthConfig]: validation rules and equality.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_config.dart';

void main() {
  group('WavelengthConfig.create', () {
    group('player validation', () {
      test('succeeds with minimum players (2)', () {
        final result = WavelengthConfig.create(playerNames: ['Ana', 'Bob']);
        expect(result.isSuccess, isTrue);
      });

      test('succeeds with maximum players (8)', () {
        final names = List.generate(8, (i) => 'P$i');
        final result = WavelengthConfig.create(playerNames: names);
        expect(result.isSuccess, isTrue);
      });

      test('fails with fewer than minimum players (1)', () {
        final result = WavelengthConfig.create(playerNames: ['Solo']);
        expect(result.isSuccess, isFalse);
        expect(result.error, WavelengthConfigError.pocosJugadores);
      });

      test('fails with empty player list', () {
        final result = WavelengthConfig.create(playerNames: []);
        expect(result.error, WavelengthConfigError.pocosJugadores);
      });

      test('fails with more than maximum players (9)', () {
        final names = List.generate(9, (i) => 'P$i');
        final result = WavelengthConfig.create(playerNames: names);
        expect(result.error, WavelengthConfigError.demasiadosJugadores);
      });

      test('fails with empty player name', () {
        final result = WavelengthConfig.create(playerNames: ['Ana', '  ']);
        expect(result.error, WavelengthConfigError.nombreVacio);
      });

      test('fails with duplicate names (case-insensitive)', () {
        final result = WavelengthConfig.create(playerNames: ['Ana', 'ana']);
        expect(result.error, WavelengthConfigError.nombresDuplicados);
      });

      test('trims whitespace from player names', () {
        final result = WavelengthConfig.create(
          playerNames: ['  Ana  ', '  Bob  '],
        );
        expect(result.isSuccess, isTrue);
        expect(result.config!.playerNames, ['Ana', 'Bob']);
      });
    });

    group('rondas validation', () {
      test('uses default rondas when not specified', () {
        final result = WavelengthConfig.create(playerNames: ['Ana', 'Bob']);
        expect(result.config!.rondas, kWavelengthDefaultRondas);
      });

      test('succeeds with minimum rondas (1)', () {
        final result = WavelengthConfig.create(
          playerNames: ['Ana', 'Bob'],
          rondas: 1,
        );
        expect(result.isSuccess, isTrue);
        expect(result.config!.rondas, 1);
      });

      test('succeeds with maximum rondas (20)', () {
        final result = WavelengthConfig.create(
          playerNames: ['Ana', 'Bob'],
          rondas: 20,
        );
        expect(result.isSuccess, isTrue);
        expect(result.config!.rondas, 20);
      });

      test('fails with rondas below minimum (0)', () {
        final result = WavelengthConfig.create(
          playerNames: ['Ana', 'Bob'],
          rondas: 0,
        );
        expect(result.error, WavelengthConfigError.rondasFueraDeRango);
      });

      test('fails with rondas above maximum (21)', () {
        final result = WavelengthConfig.create(
          playerNames: ['Ana', 'Bob'],
          rondas: 21,
        );
        expect(result.error, WavelengthConfigError.rondasFueraDeRango);
      });
    });

    group('equality', () {
      test('equal configs with same players and rondas', () {
        final a = WavelengthConfig.create(
          playerNames: ['Ana', 'Bob'],
          rondas: 5,
        ).config!;
        final b = WavelengthConfig.create(
          playerNames: ['Ana', 'Bob'],
          rondas: 5,
        ).config!;
        expect(a, equals(b));
        expect(a.hashCode, equals(b.hashCode));
      });

      test('not equal when rondas differ', () {
        final a = WavelengthConfig.create(
          playerNames: ['Ana', 'Bob'],
          rondas: 5,
        ).config!;
        final b = WavelengthConfig.create(
          playerNames: ['Ana', 'Bob'],
          rondas: 6,
        ).config!;
        expect(a, isNot(equals(b)));
      });

      test('not equal when player order differs', () {
        final a = WavelengthConfig.create(playerNames: ['Ana', 'Bob']).config!;
        final b = WavelengthConfig.create(playerNames: ['Bob', 'Ana']).config!;
        expect(a, isNot(equals(b)));
      });
    });
  });
}
