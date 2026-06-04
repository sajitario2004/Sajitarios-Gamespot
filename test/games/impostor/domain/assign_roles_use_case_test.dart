/// Suite dedicada del [AssignRolesUseCase] (versión 0.15): blinda las reglas
/// probabilísticas críticas del Impostor.
///
/// Toda la aleatoriedad se inyecta con [RandomProvider.seeded] para que la
/// distribución de ~10.000 iteraciones y los casos especiales sean
/// **deterministas** (reproducibles) en cada ejecución.
library;

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/assign_roles_use_case.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/role.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/word.dart';

/// Construye una lista de [Player] con nombres incrementales (j0, j1, ...).
List<Player> _players(int count) =>
    List<Player>.generate(count, (i) => Player('j$i'), growable: false);

/// Crea una [GameConfig] válida o falla el test si la construcción no lo es.
GameConfig _config({
  required int playerCount,
  required int nImpostores,
  bool hintEnabled = false,
}) {
  final result = GameConfig.create(
    players: _players(playerCount),
    nImpostores: nImpostores,
    hintEnabled: hintEnabled,
  );
  expect(
    result.isSuccess,
    isTrue,
    reason: 'La configuración de prueba debería ser válida: ${result.error}',
  );
  return result.config!;
}

/// Conjunto de palabras candidatas con su pista obligatoria.
List<Word> _words() => <Word>[
  Word(text: 'pirata', hint: 'barco'),
  Word(text: 'playa', hint: 'arena'),
  Word(text: 'montaña', hint: 'nieve'),
  Word(text: 'cocina', hint: 'fuego'),
];

void main() {
  group('AssignRolesUseCase — reglas probabilísticas (seed fija)', () {
    final words = _words();

    test('lanza ArgumentError si la lista de palabras está vacía', () {
      final useCase = AssignRolesUseCase(RandomProvider.seeded(1));
      final config = _config(playerCount: 4, nImpostores: 1);

      expect(
        () => useCase(config, const <Word>[]),
        throwsA(isA<ArgumentError>()),
      );
    });

    group('distribución ~10/10/80 sobre 10.000 iteraciones', () {
      const iterations = 10000;

      test('las proporciones caen ~10% / 10% / 80% con tolerancia', () {
        // Semilla fija => secuencia de tiradas determinista => conteos estables.
        final useCase = AssignRolesUseCase(RandomProvider.seeded(20240615));
        final gameConfig = _config(playerCount: 5, nImpostores: 2);

        var todos = 0;
        var ninguno = 0;
        var normal = 0;

        for (var i = 0; i < iterations; i++) {
          final session = useCase(gameConfig, words);
          final impostores = session.impostorCount;
          if (impostores == gameConfig.players.length) {
            todos++;
          } else if (impostores == 0) {
            ninguno++;
          } else {
            normal++;
          }
        }

        // Suma íntegra: cada partida cae en exactamente un cubo.
        expect(todos + ninguno + normal, iterations);

        final pTodos = todos / iterations;
        final pNinguno = ninguno / iterations;
        final pNormal = normal / iterations;

        // Tolerancia razonable de ±2 puntos porcentuales sobre 10.000 muestras.
        expect(
          pTodos,
          closeTo(0.10, 0.02),
          reason: 'Esperado ~10% "todos impostores", obtenido $pTodos',
        );
        expect(
          pNinguno,
          closeTo(0.10, 0.02),
          reason: 'Esperado ~10% "ninguno", obtenido $pNinguno',
        );
        expect(
          pNormal,
          closeTo(0.80, 0.03),
          reason: 'Esperado ~80% "normal", obtenido $pNormal',
        );
      });

      test('mismo seed produce exactamente los mismos conteos', () {
        List<int> contar(int seed) {
          final useCase = AssignRolesUseCase(RandomProvider.seeded(seed));
          final gameConfig = _config(playerCount: 5, nImpostores: 2);
          var todos = 0;
          var ninguno = 0;
          var normal = 0;
          for (var i = 0; i < iterations; i++) {
            final impostores = useCase(gameConfig, words).impostorCount;
            if (impostores == gameConfig.players.length) {
              todos++;
            } else if (impostores == 0) {
              ninguno++;
            } else {
              normal++;
            }
          }
          return <int>[todos, ninguno, normal];
        }

        expect(contar(99), equals(contar(99)));
      });
    });

    group('casos especiales', () {
      test('TODOS impostores: todos los jugadores tienen Role.impostor', () {
        // Buscamos una semilla cuya primera tirada sea < 0.10. Como nextDouble
        // se consume tras pick(words), iteramos semillas hasta encontrar el caso
        // y comprobamos la invariante (determinista por seed).
        final config = _config(playerCount: 6, nImpostores: 3);
        final session = _firstSessionWhere(
          config,
          words,
          (s) => s.impostorCount == config.players.length,
        );

        expect(
          session.assignments.values.every((r) => r == Role.impostor),
          isTrue,
        );
        expect(session.impostores.length, config.players.length);
      });

      test('NINGUNO impostor: ningún jugador tiene Role.impostor', () {
        final config = _config(playerCount: 6, nImpostores: 3);
        final session = _firstSessionWhere(
          config,
          words,
          (s) => s.impostorCount == 0,
        );

        expect(
          session.assignments.values.every((r) => r == Role.palabra),
          isTrue,
        );
        expect(session.impostores, isEmpty);
      });

      test('NORMAL: exactamente nImpostores impostores', () {
        final config = _config(playerCount: 6, nImpostores: 2);
        final session = _firstSessionWhere(
          config,
          words,
          (s) => s.impostorCount > 0 && s.impostorCount < config.players.length,
        );

        expect(session.impostorCount, config.nImpostores);
        // El resto sabe la palabra.
        expect(
          session.assignments.values.where((r) => r == Role.palabra).length,
          config.players.length - config.nImpostores,
        );
      });
    });

    group('capado de nImpostores a players - 1', () {
      test('nImpostores >= players.length se capa a players - 1', () {
        // GameConfig.create normaliza nImpostores a min(5, players-1). Con 4
        // jugadores el tope es 3, así que pedir 5 (o más) debe quedar en 3.
        final config = _config(playerCount: 4, nImpostores: 5);
        expect(config.nImpostores, 3, reason: 'Cap = players - 1 = 3');

        final session = _firstSessionWhere(
          config,
          words,
          (s) => s.impostorCount > 0 && s.impostorCount < config.players.length,
        );

        // En modo normal nunca todos son impostores: queda al menos uno con la
        // palabra.
        expect(session.impostorCount, 3);
        expect(session.impostorCount, lessThan(config.players.length));
        expect(
          session.assignments.values.where((r) => r == Role.palabra),
          isNotEmpty,
        );
      });
    });

    group('orden de revelación = orden de introducción', () {
      test('session.players conserva el orden de config.players', () {
        final config = _config(playerCount: 7, nImpostores: 3);
        final useCase = AssignRolesUseCase(RandomProvider.seeded(42));

        final session = useCase(config, words);

        expect(session.players, equals(config.players));
        expect(session.revealOrder, equals(config.players));
      });

      test(
        'aunque la baraja interna asigne roles, el orden no se altera nunca',
        () {
          final config = _config(playerCount: 8, nImpostores: 4);
          // Varias semillas distintas: el orden de revelación es siempre el de
          // introducción, independientemente de cómo se barajen los roles.
          for (final seed in <int>[1, 2, 3, 7, 11, 100, 9999]) {
            final useCase = AssignRolesUseCase(RandomProvider.seeded(seed));
            final session = useCase(config, words);
            expect(
              session.players,
              equals(config.players),
              reason: 'El orden se rompió con seed $seed',
            );
          }
        },
      );
    });

    group('elección de palabra y pista', () {
      test('la palabra elegida pertenece al conjunto disponible', () {
        final config = _config(playerCount: 5, nImpostores: 2);
        for (final seed in <int>[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]) {
          final useCase = AssignRolesUseCase(RandomProvider.seeded(seed));
          final session = useCase(config, words);
          expect(words, contains(session.word));
        }
      });

      test('la pista de la palabra viaja en la GameSession', () {
        final config = _config(playerCount: 5, nImpostores: 2);
        final useCase = AssignRolesUseCase(RandomProvider.seeded(123));
        final session = useCase(config, words);

        expect(session.hint, session.word.hint);
        expect(session.hint, isNotEmpty);
      });
    });
  });
}

/// Devuelve la primera [GameSession] (probando semillas crecientes) que cumple
/// [matcher]. Como cada semilla es determinista, el resultado es reproducible.
///
/// Falla el test si no se encuentra ninguna en un número razonable de intentos.
GameSession _firstSessionWhere(
  GameConfig config,
  List<Word> words,
  bool Function(GameSession) matcher,
) {
  for (var seed = 0; seed < 5000; seed++) {
    final useCase = AssignRolesUseCase(RandomProvider.seeded(seed));
    final session = useCase(config, words);
    if (matcher(session)) return session;
  }
  fail('No se encontró ninguna sesión que cumpla el criterio en 5000 semillas');
}
