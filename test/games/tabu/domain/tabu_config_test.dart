/// Tests para [TabuConfig]: validacion de invariantes y value object.
library;

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/games/tabu/domain/tabu_config.dart';

TabuConfigResult _create({
  String equipoA = 'Equipo A',
  String equipoB = 'Equipo B',
  int turnoSegundos = 60,
  int objetivoVictorias = 3,
}) => TabuConfig.create(
  equipoA: equipoA,
  equipoB: equipoB,
  turnoSegundos: turnoSegundos,
  objetivoVictorias: objetivoVictorias,
);

void main() {
  group('TabuConfig.create', () {
    group('construccion valida', () {
      test('crea configuracion con valores por defecto', () {
        final result = _create();
        expect(result.isSuccess, isTrue);
        final cfg = result.config!;
        expect(cfg.equipoA, 'Equipo A');
        expect(cfg.equipoB, 'Equipo B');
        expect(cfg.turnoSegundos, 60);
        expect(cfg.objetivoVictorias, 3);
      });

      test('recorta espacios de nombres de equipo', () {
        final result = _create(equipoA: '  Rojos  ', equipoB: '  Azules  ');
        expect(result.isSuccess, isTrue);
        expect(result.config!.equipoA, 'Rojos');
        expect(result.config!.equipoB, 'Azules');
      });

      test('acepta turnoSegundos en el limite inferior (30)', () {
        final result = _create(turnoSegundos: kTabuMinTurnoSegundos);
        expect(result.isSuccess, isTrue);
      });

      test('acepta turnoSegundos en el limite superior (120)', () {
        final result = _create(turnoSegundos: kTabuMaxTurnoSegundos);
        expect(result.isSuccess, isTrue);
      });

      test('acepta objetivoVictorias = 1', () {
        final result = _create(objetivoVictorias: 1);
        expect(result.isSuccess, isTrue);
      });
    });

    group('violaciones de invariantes', () {
      test('falla si equipoA esta vacio', () {
        final result = _create(equipoA: '   ');
        expect(result.isSuccess, isFalse);
        expect(result.error, TabuConfigError.equipoAVacio);
      });

      test('falla si equipoB esta vacio', () {
        final result = _create(equipoB: '');
        expect(result.isSuccess, isFalse);
        expect(result.error, TabuConfigError.equipoBVacio);
      });

      test('falla si equipoA y equipoB son iguales (mismo caso)', () {
        final result = _create(equipoA: 'Rojos', equipoB: 'Rojos');
        expect(result.isSuccess, isFalse);
        expect(result.error, TabuConfigError.equiposDuplicados);
      });

      test('falla si equipoA y equipoB son iguales (diferente caso)', () {
        final result = _create(equipoA: 'rojos', equipoB: 'ROJOS');
        expect(result.isSuccess, isFalse);
        expect(result.error, TabuConfigError.equiposDuplicados);
      });

      test('falla si turnoSegundos es menor al minimo', () {
        final result = _create(turnoSegundos: kTabuMinTurnoSegundos - 1);
        expect(result.isSuccess, isFalse);
        expect(result.error, TabuConfigError.turnoSegundosInvalido);
      });

      test('falla si turnoSegundos excede el maximo', () {
        final result = _create(turnoSegundos: kTabuMaxTurnoSegundos + 1);
        expect(result.isSuccess, isFalse);
        expect(result.error, TabuConfigError.turnoSegundosInvalido);
      });

      test('falla si objetivoVictorias es cero', () {
        final result = _create(objetivoVictorias: 0);
        expect(result.isSuccess, isFalse);
        expect(result.error, TabuConfigError.objetivoVictoriasInvalido);
      });

      test('falla si objetivoVictorias es negativo', () {
        final result = _create(objetivoVictorias: -1);
        expect(result.isSuccess, isFalse);
        expect(result.error, TabuConfigError.objetivoVictoriasInvalido);
      });
    });

    group('igualdad y hashCode', () {
      test('dos instancias identicas son iguales', () {
        final a = _create().config!;
        final b = _create().config!;
        expect(a, equals(b));
        expect(a.hashCode, equals(b.hashCode));
      });

      test('diferente equipoA produce desigualdad', () {
        final a = _create(equipoA: 'Rojos').config!;
        final b = _create(equipoA: 'Azules').config!;
        expect(a, isNot(equals(b)));
      });

      test('diferente turnoSegundos produce desigualdad', () {
        final a = _create(turnoSegundos: 60).config!;
        final b = _create(turnoSegundos: 90).config!;
        expect(a, isNot(equals(b)));
      });
    });
  });
}
