/// Reglas probabilísticas del Impostor: el núcleo crítico del proyecto.
///
/// [AssignRolesUseCase] es **puro** (sin Flutter, sin base de datos): recibe la
/// configuración de la partida y las palabras disponibles como parámetros, y la
/// aleatoriedad vía un [RandomProvider] inyectado. Así la suite de ~10.000
/// iteraciones con semilla fija (versión 0.15) es totalmente determinista.
///
/// La obtención de palabras desde la BD **no** ocurre aquí: la hace el provider
/// coordinador `assignRolesCoordinatorProvider`, que lee `wordRepositoryProvider`
/// y `randomProvider` y luego delega en este use case.
library;

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/role.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/word.dart';

/// Umbral de probabilidad del caso especial "todos impostores": `< 0.10`.
const double kTodosImpostoresThreshold = 0.10;

/// Umbral de probabilidad del caso especial "ninguno impostor": `[0.10, 0.20)`.
const double kNingunoImpostorThreshold = 0.20;

/// Aplica las reglas de asignación de roles del Impostor.
///
/// Con **una sola** tirada de [RandomProvider.nextDouble]:
/// - `< 0.10` → TODOS los jugadores son impostores.
/// - `< 0.20` → NINGUNO es impostor (todos saben la palabra).
/// - resto (80%) → asignación normal: `nImpostores` jugadores al azar como
///   impostores (capado por [GameConfig.create] a `players - 1`).
///
/// La palabra se elige al azar con [RandomProvider.pick]. El orden de revelación
/// es siempre el de introducción de los jugadores ([GameConfig.players]): la
/// baraja solo se usa para **asignar** los roles, no para revelarlos.
class AssignRolesUseCase {
  /// Crea el use case con la fuente de aleatoriedad inyectada.
  const AssignRolesUseCase(this._random);

  final RandomProvider _random;

  /// Asigna roles y devuelve la [GameSession] resultante.
  ///
  /// [config] es la configuración ya validada/normalizada de la partida.
  /// [words] es la lista de palabras candidatas (no vacía); se elige una al azar.
  ///
  /// Lanza [ArgumentError] si [words] está vacía.
  GameSession call(GameConfig config, List<Word> words) {
    if (words.isEmpty) {
      throw ArgumentError.value(
        words,
        'words',
        'No hay palabras disponibles para la partida',
      );
    }

    final word = _random.pick(words);
    final players = config.players;

    // UNA sola tirada decide el caso (10 / 10 / 80).
    final roll = _random.nextDouble();

    final Map<Player, Role> assignments;
    if (roll < kTodosImpostoresThreshold) {
      assignments = _allWith(players, Role.impostor);
    } else if (roll < kNingunoImpostorThreshold) {
      assignments = _allWith(players, Role.palabra);
    } else {
      assignments = _assignNormal(players, config.nImpostores);
    }

    return GameSession(word: word, players: players, assignments: assignments);
  }

  /// Asignación normal: elige [nImpostores] jugadores al azar como impostores y
  /// el resto saben la palabra.
  ///
  /// Baraja una copia de [players] (sin tocar el orden original de revelación) y
  /// toma los primeros [nImpostores] como impostores.
  Map<Player, Role> _assignNormal(List<Player> players, int nImpostores) {
    final shuffled = List<Player>.of(players);
    _shuffle(shuffled);

    final impostores = shuffled.take(nImpostores).toSet();
    return <Player, Role>{
      for (final player in players)
        player: impostores.contains(player) ? Role.impostor : Role.palabra,
    };
  }

  /// Asigna [role] a todos los [players].
  Map<Player, Role> _allWith(List<Player> players, Role role) => <Player, Role>{
    for (final player in players) player: role,
  };

  /// Baraja [list] en sitio con Fisher-Yates usando [RandomProvider] (para que
  /// el barajado también sea determinista con semilla fija).
  void _shuffle(List<Player> list) {
    for (var i = list.length - 1; i > 0; i--) {
      final j = _random.nextInt(i + 1);
      final tmp = list[i];
      list[i] = list[j];
      list[j] = tmp;
    }
  }
}
