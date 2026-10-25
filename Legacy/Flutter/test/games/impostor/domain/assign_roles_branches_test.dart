/// Endurecimiento de la suite de `AssignRolesUseCase` (recoge la observación del
/// revisor de F4): además de comprobar el **resultado** (cuántos impostores),
/// fija cada **rama** de la regla 10/10/80 con una semilla concreta y verifica
/// que se ejecutó esa rama y no otra.
///
/// La clave para distinguir ramas de forma determinista: con **una sola**
/// palabra disponible, `pick(words)` consume exactamente una posición y la
/// siguiente tirada (`nextDouble`) decide la rama. Conociendo el valor exacto de
/// esa tirada por semilla, sabemos qué rama *debe* tomarse:
/// - `< 0.10` -> TODOS impostores.
/// - `[0.10, 0.20)` -> NINGUNO impostor.
/// - resto -> asignación normal.
///
/// Así "todos" y "ninguno" ya no se confunden por casualidad: cada test ancla su
/// rama por el `roll` además del recuento de roles.
library;

import 'dart:math';

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/assign_roles_use_case.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/role.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/word.dart';

/// Una sola palabra: hace que la única fuente de variación tras `pick` sea la
/// tirada que decide la rama.
List<Word> _singleWord() => <Word>[Word(text: 'playa', hint: 'verano')];

GameConfig _config({required int playerCount, required int nImpostores}) {
  final players = List<Player>.generate(
    playerCount,
    (i) => Player('j$i'),
    growable: false,
  );
  final result = GameConfig.create(players: players, nImpostores: nImpostores);
  expect(result.isSuccess, isTrue, reason: '${result.error}');
  return result.config!;
}

/// Reproduce la tirada que `AssignRolesUseCase` usaría para decidir la rama con
/// [seed] y **una** palabra: la primera tirada es el `pick` (nextInt(1)), la
/// segunda (`nextDouble`) decide la rama. Devuelve ese `nextDouble`.
double _rollForSeed(int seed) {
  final rng = Random(seed);
  rng.nextInt(1); // pick de la única palabra
  return rng.nextDouble();
}

void main() {
  group('AssignRolesUseCase — ramas ancladas por seed (todos vs ninguno)', () {
    final words = _singleWord();

    test(
      'rama TODOS (roll < 0.10): el roll cae en el tramo y todos son impostores',
      () {
        // seed=8 -> roll ~ 0.017 (< 0.10): debe tomarse la rama "todos".
        const seed = 8;
        final roll = _rollForSeed(seed);
        expect(
          roll,
          lessThan(kTodosImpostoresThreshold),
          reason: 'El seed debe caer en la rama TODOS (roll=$roll).',
        );

        final config = _config(playerCount: 4, nImpostores: 1);
        final session = AssignRolesUseCase(RandomProvider.seeded(seed))(
          config,
          words,
        );

        // Distintivo de RAMA, no solo del recuento: todos con Role.impostor.
        expect(
          session.assignments.values.every((r) => r == Role.impostor),
          isTrue,
        );
        expect(session.impostorCount, config.players.length);
      },
    );

    test(
      'rama NINGUNO (0.10 <= roll < 0.20): el roll cae en el tramo y nadie es '
      'impostor',
      () {
        // seed=0 -> roll ~ 0.161 ([0.10, 0.20)): debe tomarse la rama "ninguno".
        const seed = 0;
        final roll = _rollForSeed(seed);
        expect(
          roll,
          inInclusiveRange(
            kTodosImpostoresThreshold,
            kNingunoImpostorThreshold,
          ),
        );
        expect(roll, lessThan(kNingunoImpostorThreshold));

        final config = _config(playerCount: 4, nImpostores: 1);
        final session = AssignRolesUseCase(RandomProvider.seeded(seed))(
          config,
          words,
        );

        // Distintivo de RAMA: todos con Role.palabra (ninguno impostor).
        expect(
          session.assignments.values.every((r) => r == Role.palabra),
          isTrue,
        );
        expect(session.impostorCount, 0);
      },
    );

    test(
      'rama NORMAL (roll >= 0.20): exactamente nImpostores impostores y el resto '
      'sabe la palabra',
      () {
        // seed=1 -> roll ~ 0.311 (>= 0.20): rama normal.
        const seed = 1;
        final roll = _rollForSeed(seed);
        expect(roll, greaterThanOrEqualTo(kNingunoImpostorThreshold));

        final config = _config(playerCount: 3, nImpostores: 1);
        final session = AssignRolesUseCase(RandomProvider.seeded(seed))(
          config,
          words,
        );

        expect(session.impostorCount, config.nImpostores);
        expect(
          session.assignments.values.where((r) => r == Role.palabra).length,
          config.players.length - config.nImpostores,
        );
      },
    );

    test('todos y ninguno son ramas distintas con el MISMO recuento extremo no '
        'colisionan: el recuento 0 implica palabra para todos y el recuento '
        'total implica impostor para todos', () {
      // Refuerzo explícito de la observación: el caso "todos" (impostorCount ==
      // N) y "ninguno" (impostorCount == 0) son mutuamente excluyentes y se
      // identifican por el rol concreto de cada jugador, no por un único número.
      final config = _config(playerCount: 5, nImpostores: 2);

      final todos = AssignRolesUseCase(RandomProvider.seeded(8))(config, words);
      final ninguno = AssignRolesUseCase(RandomProvider.seeded(0))(
        config,
        words,
      );

      expect(todos.impostorCount, config.players.length);
      expect(ninguno.impostorCount, 0);
      expect(todos.assignments.values.toSet(), equals(<Role>{Role.impostor}));
      expect(ninguno.assignments.values.toSet(), equals(<Role>{Role.palabra}));
    });
  });
}
