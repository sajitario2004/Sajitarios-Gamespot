/// Tests for [BombaConfig] validation rules.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_config.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_mode.dart';

BombaConfigResult _create({
  BombaMode mode = BombaMode.silaba,
  List<String> players = const ['Ana', 'Bob'],
  int minSeg = 15,
  int maxSeg = 45,
}) => BombaConfig.create(
  mode: mode,
  playerNames: players,
  minSegundos: minSeg,
  maxSegundos: maxSeg,
);

void main() {
  group('BombaConfig — jugadores', () {
    test('acepta el minimo de jugadores (2)', () {
      final r = _create(players: ['Ana', 'Bob']);
      expect(r.isSuccess, isTrue);
    });

    test('acepta el maximo de jugadores (12)', () {
      final names = List.generate(12, (i) => 'J${i + 1}');
      final r = _create(players: names);
      expect(r.isSuccess, isTrue);
    });

    test('falla con menos de 2 jugadores', () {
      final r = _create(players: ['Solo']);
      expect(r.isSuccess, isFalse);
      expect(r.error, BombaConfigError.pocosJugadores);
    });

    test('falla con mas de 12 jugadores', () {
      final names = List.generate(13, (i) => 'J${i + 1}');
      final r = _create(players: names);
      expect(r.isSuccess, isFalse);
      expect(r.error, BombaConfigError.demasiadosJugadores);
    });

    test('falla con nombre vacio', () {
      final r = _create(players: ['Ana', '']);
      expect(r.isSuccess, isFalse);
      expect(r.error, BombaConfigError.nombreVacio);
    });

    test('falla con nombre solo espacios', () {
      final r = _create(players: ['Ana', '   ']);
      expect(r.isSuccess, isFalse);
      expect(r.error, BombaConfigError.nombreVacio);
    });

    test('falla con nombres duplicados (case-insensitive)', () {
      final r = _create(players: ['Ana', 'ana']);
      expect(r.isSuccess, isFalse);
      expect(r.error, BombaConfigError.nombresDuplicados);
    });

    test('recorta espacios de los nombres', () {
      final r = _create(players: ['  Ana  ', '  Bob  ']);
      expect(r.isSuccess, isTrue);
      expect(r.config!.playerNames, ['Ana', 'Bob']);
    });
  });

  group('BombaConfig — rango de segundos', () {
    test('acepta rango valido (15..45)', () {
      final r = _create(minSeg: 15, maxSeg: 45);
      expect(r.isSuccess, isTrue);
      expect(r.config!.minSegundos, 15);
      expect(r.config!.maxSegundos, 45);
    });

    test('acepta los limites absolutos (10..60)', () {
      final r = _create(minSeg: 10, maxSeg: 60);
      expect(r.isSuccess, isTrue);
    });

    test('falla si minSegundos < 10', () {
      final r = _create(minSeg: 9, maxSeg: 45);
      expect(r.isSuccess, isFalse);
      expect(r.error, BombaConfigError.minSegundosFueraDeLimite);
    });

    test('falla si maxSegundos > 60', () {
      final r = _create(minSeg: 15, maxSeg: 61);
      expect(r.isSuccess, isFalse);
      expect(r.error, BombaConfigError.maxSegundosFueraDeLimite);
    });

    test('falla si minSegundos == maxSegundos', () {
      final r = _create(minSeg: 30, maxSeg: 30);
      expect(r.isSuccess, isFalse);
      expect(r.error, BombaConfigError.rangoSegundosInvalido);
    });

    test('falla si minSegundos > maxSegundos', () {
      final r = _create(minSeg: 40, maxSeg: 20);
      expect(r.isSuccess, isFalse);
      expect(r.error, BombaConfigError.rangoSegundosInvalido);
    });
  });

  group('BombaConfig — modo', () {
    test('acepta modo silaba', () {
      final r = _create(mode: BombaMode.silaba);
      expect(r.isSuccess, isTrue);
      expect(r.config!.mode, BombaMode.silaba);
    });

    test('acepta modo categoria', () {
      final r = _create(mode: BombaMode.categoria);
      expect(r.isSuccess, isTrue);
      expect(r.config!.mode, BombaMode.categoria);
    });
  });
}
