import 'package:sajitarios_gamespot/games/impostor/data/game_history_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/data/game_record.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';

/// Repositorio de historial falso para tests de la pantalla de DESENLACE.
///
/// La [GameOverScreen] guarda la partida en el historial al entrar (post-frame).
/// En tests no queremos tocar SQLite ni `path_provider`, así que sustituimos el
/// repositorio por este doble: registra las partidas guardadas en memoria
/// ([saved]) y nunca toca la base de datos.
///
/// Extiende [GameHistoryRepository] (clase concreta) para encajar en el tipo del
/// `gameHistoryRepositoryProvider` sin necesitar una `AppDatabase` real: el
/// constructor base nunca llega a usarse porque sobreescribimos los métodos.
class FakeGameHistoryRepository implements GameHistoryRepository {
  /// Partidas guardadas vía [insertFromSession], en orden de inserción.
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
