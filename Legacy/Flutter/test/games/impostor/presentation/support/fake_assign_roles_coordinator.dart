import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/role.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/word.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';

/// Coordinador de asignación de roles falso y determinista para tests de widget.
///
/// Implementa el contrato de [AssignRolesCoordinator] sin tocar la BD ni la
/// aleatoriedad real: devuelve una [GameSession] controlada por el test. Así las
/// pantallas (Reveal / Results) reciben siempre la misma partida y se pueden
/// hacer aserciones exactas sobre roles y palabra.
class FakeAssignRolesCoordinator implements AssignRolesCoordinator {
  FakeAssignRolesCoordinator({this.session});

  /// Sesión fija a devolver. Si es `null`, se construye una por defecto a partir
  /// de la [GameConfig] recibida (todos saben la palabra, salvo el primero, que
  /// es impostor).
  final GameSession? session;

  @override
  Future<GameSession> assign(GameConfig config) async {
    if (session != null) return session!;
    final players = config.players;
    final assignments = <Player, Role>{
      for (var i = 0; i < players.length; i++)
        players[i]: i < config.nImpostores ? Role.impostor : Role.palabra,
    };
    return GameSession(
      word: Word(text: 'playa', hint: 'verano'),
      players: players,
      assignments: assignments,
    );
  }
}

/// Construye una [GameSession] determinista para tests, indicando explícitamente
/// el orden de revelación y qué jugadores son impostores.
GameSession buildSession({
  required List<String> nombres,
  required Set<String> impostores,
  String palabra = 'playa',
  String pista = 'verano',
}) {
  final players = nombres.map(Player.new).toList(growable: false);
  final assignments = <Player, Role>{
    for (final p in players)
      p: impostores.contains(p.name) ? Role.impostor : Role.palabra,
  };
  return GameSession(
    word: Word(text: palabra, hint: pista),
    players: players,
    assignments: assignments,
  );
}
