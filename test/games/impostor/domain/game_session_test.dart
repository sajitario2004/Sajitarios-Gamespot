import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/role.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/word.dart';

void main() {
  const nacho = Player('Nacho');
  const iker = Player('Iker');
  const lucia = Player('Lucía');
  final word = Word(text: 'playa', hint: 'arena');

  GameSession buildSession() => GameSession(
    word: word,
    players: const [nacho, iker, lucia],
    assignments: {
      nacho: Role.impostor,
      iker: Role.palabra,
      lucia: Role.palabra,
    },
  );

  group('GameSession construcción', () {
    test('falla si falta el rol de algún jugador', () {
      expect(
        () => GameSession(
          word: word,
          players: const [nacho, iker, lucia],
          assignments: {nacho: Role.impostor, iker: Role.palabra},
        ),
        throwsArgumentError,
      );
    });

    test('falla si hay asignaciones de más', () {
      expect(
        () => GameSession(
          word: word,
          players: const [nacho, iker],
          assignments: {
            nacho: Role.impostor,
            iker: Role.palabra,
            lucia: Role.palabra,
          },
        ),
        throwsArgumentError,
      );
    });
  });

  group('GameSession orden de revelación', () {
    test('revealOrder = orden de introducción de los jugadores', () {
      expect(buildSession().revealOrder, const [nacho, iker, lucia]);
    });

    test('la lista de jugadores es inmodificable', () {
      expect(
        () => buildSession().players.add(const Player('Extra')),
        throwsUnsupportedError,
      );
    });
  });

  group('GameSession consultas de rol', () {
    test('hint expone la pista de la palabra', () {
      expect(buildSession().hint, 'arena');
    });

    test('roleOf devuelve el rol de cada jugador', () {
      final session = buildSession();
      expect(session.roleOf(nacho), Role.impostor);
      expect(session.roleOf(iker), Role.palabra);
    });

    test('roleOf devuelve null para un jugador ajeno', () {
      expect(buildSession().roleOf(const Player('Ajeno')), isNull);
    });

    test('isImpostor identifica a los impostores', () {
      final session = buildSession();
      expect(session.isImpostor(nacho), isTrue);
      expect(session.isImpostor(iker), isFalse);
    });

    test('impostores e impostorCount cuadran', () {
      final session = buildSession();
      expect(session.impostores, const [nacho]);
      expect(session.impostorCount, 1);
    });

    test('caso todos impostores', () {
      final session = GameSession(
        word: word,
        players: const [nacho, iker],
        assignments: {nacho: Role.impostor, iker: Role.impostor},
      );
      expect(session.impostorCount, 2);
      expect(session.impostores, const [nacho, iker]);
    });

    test('caso ninguno impostor', () {
      final session = GameSession(
        word: word,
        players: const [nacho, iker],
        assignments: {nacho: Role.palabra, iker: Role.palabra},
      );
      expect(session.impostorCount, 0);
      expect(session.impostores, isEmpty);
    });
  });
}
