/// Una partida del Impostor ya resuelta: la palabra elegida y el rol de cada
/// jugador.
///
/// Es el resultado que produce `AssignRolesUseCase` (versión 0.14). Tipo
/// inmutable y puro (sin Flutter).
///
/// Punto clave de las reglas: el **orden de revelación** es el orden de
/// introducción de los jugadores ([revealOrder]). La baraja para asignar roles
/// ocurre dentro del use case y **no** afecta a este orden.
library;

import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/role.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/word.dart';

/// Resultado de una partida: [word] elegida y [assignments] (rol por jugador).
class GameSession {
  /// Crea una sesión.
  ///
  /// [players] define el orden de revelación (orden de introducción). Cada
  /// jugador de [players] debe tener una entrada en [assignments]; en caso
  /// contrario se lanza [ArgumentError].
  factory GameSession({
    required Word word,
    required List<Player> players,
    required Map<Player, Role> assignments,
  }) {
    for (final player in players) {
      if (!assignments.containsKey(player)) {
        throw ArgumentError.value(
          assignments,
          'assignments',
          'Falta el rol del jugador "${player.name}"',
        );
      }
    }
    if (assignments.length != players.length) {
      throw ArgumentError.value(
        assignments,
        'assignments',
        'El número de asignaciones no coincide con el de jugadores',
      );
    }
    return GameSession._(
      word: word,
      players: List<Player>.unmodifiable(players),
      assignments: Map<Player, Role>.unmodifiable(assignments),
    );
  }

  const GameSession._({
    required this.word,
    required this.players,
    required this.assignments,
  });

  /// La palabra elegida para la partida.
  final Word word;

  /// Jugadores en orden de introducción (= [revealOrder]).
  final List<Player> players;

  /// Rol asignado a cada jugador.
  final Map<Player, Role> assignments;

  /// La pista de la palabra (atajo a `word.hint`).
  String get hint => word.hint;

  /// Orden en el que los jugadores revelan su rol: el de introducción.
  ///
  /// **No** se baraja para revelar (la baraja solo ocurre al asignar roles).
  List<Player> get revealOrder => players;

  /// Devuelve el rol de [player], o `null` si no participa en la partida.
  Role? roleOf(Player player) => assignments[player];

  /// `true` si [player] es impostor en esta partida.
  bool isImpostor(Player player) => assignments[player]?.esImpostor ?? false;

  /// Jugadores con rol impostor, en orden de revelación.
  List<Player> get impostores =>
      players.where(isImpostor).toList(growable: false);

  /// Nº de impostores efectivos de la partida.
  int get impostorCount => assignments.values.where((r) => r.esImpostor).length;

  @override
  String toString() =>
      'GameSession(word: ${word.text}, players: $players, '
      'assignments: $assignments)';
}
