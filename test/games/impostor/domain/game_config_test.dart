import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';

List<Player> _players(int n) =>
    List<Player>.generate(n, (i) => Player('Jugador$i'));

void main() {
  group('GameConfig.maxImpostoresFor', () {
    test('capa a players - 1 cuando es menor que 5', () {
      expect(GameConfig.maxImpostoresFor(3), 2);
      expect(GameConfig.maxImpostoresFor(4), 3);
    });

    test('capa a 5 cuando players - 1 es mayor', () {
      expect(GameConfig.maxImpostoresFor(10), 5);
      expect(GameConfig.maxImpostoresFor(15), 5);
    });
  });

  group('GameConfig.create validación de jugadores', () {
    test('falla con menos de 3 jugadores', () {
      final result = GameConfig.create(players: _players(2), nImpostores: 1);
      expect(result.isSuccess, isFalse);
      expect(result.error, GameConfigError.pocosJugadores);
    });

    test('falla con más de 15 jugadores', () {
      final result = GameConfig.create(players: _players(16), nImpostores: 1);
      expect(result.isSuccess, isFalse);
      expect(result.error, GameConfigError.demasiadosJugadores);
    });

    test('falla con nombre vacío', () {
      final result = GameConfig.create(
        players: const [Player('Nacho'), Player('  '), Player('Lucía')],
        nImpostores: 1,
      );
      expect(result.error, GameConfigError.nombreVacio);
    });

    test('falla con nombres duplicados (ignora mayúsculas)', () {
      final result = GameConfig.create(
        players: const [Player('Nacho'), Player('nacho'), Player('Lucía')],
        nImpostores: 1,
      );
      expect(result.error, GameConfigError.nombresDuplicados);
    });

    test('acepta exactamente 3 jugadores', () {
      final result = GameConfig.create(players: _players(3), nImpostores: 1);
      expect(result.isSuccess, isTrue);
      expect(result.config!.players, hasLength(3));
    });

    test('acepta exactamente 15 jugadores', () {
      final result = GameConfig.create(players: _players(15), nImpostores: 1);
      expect(result.isSuccess, isTrue);
    });
  });

  group('GameConfig.create normalización de nImpostores', () {
    test('capa a players - 1 en partidas pequeñas', () {
      // 3 jugadores -> tope 2; pedir 5 se ajusta a 2.
      final result = GameConfig.create(players: _players(3), nImpostores: 5);
      expect(result.config!.nImpostores, 2);
    });

    test('capa a 5 como máximo absoluto', () {
      final result = GameConfig.create(players: _players(10), nImpostores: 99);
      expect(result.config!.nImpostores, 5);
    });

    test('eleva a 1 como mínimo', () {
      final result = GameConfig.create(players: _players(5), nImpostores: 0);
      expect(result.config!.nImpostores, 1);
    });

    test('respeta un valor dentro del rango', () {
      final result = GameConfig.create(players: _players(6), nImpostores: 3);
      expect(result.config!.nImpostores, 3);
    });
  });

  group('GameConfig propiedades', () {
    test('hintEnabled por defecto es false y se puede activar', () {
      final off = GameConfig.create(players: _players(3), nImpostores: 1);
      expect(off.config!.hintEnabled, isFalse);

      final on = GameConfig.create(
        players: _players(3),
        nImpostores: 1,
        hintEnabled: true,
      );
      expect(on.config!.hintEnabled, isTrue);
    });

    test('preserva el orden de introducción de los jugadores', () {
      const players = [Player('Nacho'), Player('Iker'), Player('Lucía')];
      final result = GameConfig.create(players: players, nImpostores: 1);
      expect(result.config!.players, players);
    });

    test('la lista de jugadores es inmodificable', () {
      final result = GameConfig.create(players: _players(3), nImpostores: 1);
      expect(
        () => result.config!.players.add(const Player('Extra')),
        throwsUnsupportedError,
      );
    });

    test('igualdad por valor', () {
      final a = GameConfig.create(players: _players(3), nImpostores: 1).config!;
      final b = GameConfig.create(players: _players(3), nImpostores: 1).config!;
      expect(a, b);
      expect(a.hashCode, b.hashCode);
    });
  });
}
