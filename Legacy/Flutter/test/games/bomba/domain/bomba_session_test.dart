/// Tests for [BombaSession] and [pickFuseSeconds].
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_config.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_mode.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_session.dart';

BombaConfig _config({
  List<String> players = const ['Ana', 'Bob', 'Carlos'],
  int minSeg = 15,
  int maxSeg = 45,
}) {
  final r = BombaConfig.create(
    mode: BombaMode.silaba,
    playerNames: players,
    minSegundos: minSeg,
    maxSegundos: maxSeg,
  );
  return r.config!;
}

void main() {
  group('pickFuseSeconds', () {
    test('resultado dentro de [minSegundos, maxSegundos]', () {
      final config = _config(minSeg: 15, maxSeg: 45);
      for (var seed = 0; seed < 100; seed++) {
        final rng = RandomProvider.seeded(seed);
        final fuse = pickFuseSeconds(rng, config);
        expect(
          fuse,
          greaterThanOrEqualTo(config.minSegundos.toDouble()),
          reason: 'fuse debe ser >= minSegundos con semilla $seed',
        );
        expect(
          fuse,
          lessThanOrEqualTo(config.maxSegundos.toDouble()),
          reason: 'fuse debe ser <= maxSegundos con semilla $seed',
        );
      }
    });

    test('es determinista bajo semilla fija', () {
      final config = _config();
      final fuse1 = pickFuseSeconds(RandomProvider.seeded(42), config);
      final fuse2 = pickFuseSeconds(RandomProvider.seeded(42), config);
      expect(fuse1, equals(fuse2));
    });

    test('produce valores distintos con semillas distintas (sanity)', () {
      final config = _config();
      final values = {
        for (var s = 0; s < 20; s++)
          pickFuseSeconds(RandomProvider.seeded(s), config),
      };
      expect(values.length, greaterThan(1));
    });
  });

  group('BombaSession.start', () {
    test('primer holder es el primer jugador de introduccion', () {
      final session = BombaSession.start(_config(), RandomProvider.seeded(0));
      expect(session.currentHolder, 'Ana');
    });

    test('alivePlayers incluye todos los jugadores al inicio', () {
      final session = BombaSession.start(_config(), RandomProvider.seeded(0));
      expect(session.alivePlayers, ['Ana', 'Bob', 'Carlos']);
    });

    test('isOver es false con mas de un jugador', () {
      final session = BombaSession.start(_config(), RandomProvider.seeded(0));
      expect(session.isOver, isFalse);
    });

    test('fuseSeconds esta dentro del rango configurado', () {
      final cfg = _config(minSeg: 20, maxSeg: 50);
      final session = BombaSession.start(cfg, RandomProvider.seeded(7));
      expect(session.fuseSeconds, greaterThanOrEqualTo(20.0));
      expect(session.fuseSeconds, lessThanOrEqualTo(50.0));
    });
  });

  group('BombaSession.pasar', () {
    test('avanza el holder al siguiente jugador', () {
      var session = BombaSession.start(_config(), RandomProvider.seeded(0));
      session = session.pasar();
      expect(session.currentHolder, 'Bob');
    });

    test('hace wrap-around al llegar al final de alivePlayers', () {
      var session = BombaSession.start(_config(), RandomProvider.seeded(0));
      session = session.pasar(); // Bob
      session = session.pasar(); // Carlos
      session = session.pasar(); // Ana (wrap)
      expect(session.currentHolder, 'Ana');
    });

    test('no cambia la lista alivePlayers', () {
      var session = BombaSession.start(_config(), RandomProvider.seeded(0));
      final before = session.alivePlayers;
      session = session.pasar();
      expect(session.alivePlayers, before);
    });

    test('lanza StateError si la sesion ya termino', () {
      final cfg = _config(players: ['Solo', 'Dos']);
      var session = BombaSession.start(cfg, RandomProvider.seeded(0));
      session = session.explode(); // elimina Ana
      expect(session.isOver, isTrue);
      expect(() => session.pasar(), throwsStateError);
    });
  });

  group('BombaSession.explode', () {
    test('elimina al holder actual', () {
      var session = BombaSession.start(_config(), RandomProvider.seeded(0));
      // holder = Ana
      session = session.explode();
      expect(session.alivePlayers, isNot(contains('Ana')));
    });

    test('alivePlayers se reduce en uno tras explode', () {
      var session = BombaSession.start(_config(), RandomProvider.seeded(0));
      session = session.explode();
      expect(session.alivePlayers.length, 2);
    });

    test('isOver es true cuando queda un solo jugador', () {
      final cfg = _config(players: ['Ana', 'Bob']);
      var session = BombaSession.start(cfg, RandomProvider.seeded(0));
      session = session.explode(); // elimina Ana
      expect(session.isOver, isTrue);
      expect(session.winner, 'Bob');
    });

    test(
      'el siguiente holder es valido tras eliminar al ultimo de la lista',
      () {
        var session = BombaSession.start(_config(), RandomProvider.seeded(0));
        // Avanzar hasta Carlos (indice 2)
        session = session.pasar().pasar(); // holder = Carlos
        expect(session.currentHolder, 'Carlos');
        session = session.explode(); // elimina Carlos (era el ultimo)
        // El indice debe hacer wrap: 2 % 2 = 0 -> Ana
        expect(session.alivePlayers, ['Ana', 'Bob']);
        expect(session.currentHolder, 'Ana');
      },
    );

    test('lanza StateError si la sesion ya termino', () {
      final cfg = _config(players: ['Ana', 'Bob']);
      var session = BombaSession.start(cfg, RandomProvider.seeded(0));
      session = session.explode(); // termina el juego
      expect(() => session.explode(), throwsStateError);
    });

    test('secuencia completa: last-one-standing es el ganador', () {
      // 4 jugadores, eliminamos 3 uno a uno
      final cfg = _config(players: ['A', 'B', 'C', 'D']);
      var session = BombaSession.start(cfg, RandomProvider.seeded(0));
      session = session.explode(); // A eliminado
      session = session.explode(); // B eliminado (nuevo holder 0 -> B)
      session = session.explode(); // C eliminado
      expect(session.isOver, isTrue);
      expect(session.winner, isNotNull);
      expect(session.alivePlayers.length, 1);
    });
  });
}
