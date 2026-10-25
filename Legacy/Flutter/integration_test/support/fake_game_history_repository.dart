import 'package:sajitarios_gamespot/games/impostor/data/game_history_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/data/game_record.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';

/// Repositorio de historial falso e in-memory para el e2e del Impostor.
///
/// La pantalla de DESENLACE guarda la partida al entrar. En el host de test no
/// hay SQLite (ni `path_provider`), así que sustituimos el repositorio por este
/// doble: registra en memoria las partidas guardadas y nunca toca disco.
class FakeGameHistoryRepository implements GameHistoryRepository {
  final List<GameSession> saved = <GameSession>[];

  @override
  Future<GameRecord> insertFromSession(
    GameSession session, {
    required bool hintEnabled,
    DateTime? createdAt,
  }) async {
    saved.add(session);
    return GameRecord.fromSession(
      session,
      hintEnabled: hintEnabled,
      createdAt: createdAt,
    );
  }

  @override
  dynamic noSuchMethod(Invocation invocation) => super.noSuchMethod(invocation);
}
