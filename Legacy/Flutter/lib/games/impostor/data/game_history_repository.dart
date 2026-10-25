import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:sqflite/sqflite.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/impostor/data/game_record.dart';
import 'package:sajitarios_gamespot/games/impostor/data/impostor_schema.dart'
    show kGameHistoryTable;
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';

/// Una palabra del historial con su número de apariciones.
class WordFrequency {
  const WordFrequency({required this.word, required this.count});

  /// El texto de la palabra.
  final String word;

  /// Cuántas veces apareció esa palabra en el historial.
  final int count;

  @override
  String toString() => 'WordFrequency($word: $count)';
}

/// Acceso a datos del historial de partidas del Impostor sobre `game_history`.
///
/// Persiste el resultado de cada partida ([insertFromSession]) y permite
/// listarlo ([getAll], orden descendente por fecha), borrarlo todo
/// ([deleteAll]) y calcular agregados para estadísticas ([count],
/// [mostFrequentWord], [impostorCountsByPlayer]).
class GameHistoryRepository {
  GameHistoryRepository({required AppDatabase database}) : _database = database;

  final AppDatabase _database;

  Future<Database> get _db => _database.database;

  /// Inserta una partida en el historial derivando los campos de [session].
  ///
  /// [hintEnabled] no forma parte de la sesión (vive en la `GameConfig`), así
  /// que se pasa aparte. Devuelve el [GameRecord] insertado con su `id`.
  Future<GameRecord> insertFromSession(
    GameSession session, {
    required bool hintEnabled,
    DateTime? createdAt,
  }) async {
    final record = GameRecord.fromSession(
      session,
      hintEnabled: hintEnabled,
      createdAt: createdAt,
    );
    return insert(record);
  }

  /// Inserta un [record] ya construido. Devuelve la copia con `id` asignado.
  Future<GameRecord> insert(GameRecord record) async {
    final db = await _db;
    final id = await db.insert(kGameHistoryTable, record.toMap());
    return GameRecord(
      id: id,
      createdAt: record.createdAt,
      word: record.word,
      hint: record.hint,
      nPlayers: record.nPlayers,
      nImpostors: record.nImpostors,
      hintEnabled: record.hintEnabled,
      players: record.players,
    );
  }

  /// Devuelve todas las partidas del historial, de la más reciente a la más
  /// antigua (orden descendente por `created_at`, desempate por `id`).
  Future<List<GameRecord>> getAll() async {
    final db = await _db;
    final rows = await db.query(
      kGameHistoryTable,
      orderBy: 'created_at DESC, id DESC',
    );
    return rows.map(GameRecord.fromMap).toList(growable: false);
  }

  /// Borra todas las partidas del historial. Devuelve cuántas filas se borraron.
  Future<int> deleteAll() async {
    final db = await _db;
    return db.delete(kGameHistoryTable);
  }

  /// Total de partidas guardadas en el historial.
  Future<int> count() async {
    final db = await _db;
    final result = await db.rawQuery(
      'SELECT COUNT(*) AS c FROM $kGameHistoryTable',
    );
    return Sqflite.firstIntValue(result) ?? 0;
  }

  /// La palabra más frecuente del historial, o `null` si está vacío.
  ///
  /// El recuento es case-insensitive en el agrupado (`NOCASE`); devuelve el
  /// texto tal como se guardó en la fila más reciente del grupo. En caso de
  /// empate gana la de mayor recuento y, a igualdad, la alfabéticamente menor.
  Future<WordFrequency?> mostFrequentWord() async {
    final db = await _db;
    final rows = await db.rawQuery('''
      SELECT word, COUNT(*) AS c
      FROM $kGameHistoryTable
      GROUP BY word COLLATE NOCASE
      ORDER BY c DESC, word COLLATE NOCASE ASC
      LIMIT 1
    ''');
    if (rows.isEmpty) return null;
    final row = rows.first;
    return WordFrequency(
      word: row['word'] as String,
      count: (row['c'] as int?) ?? 0,
    );
  }

  /// Cuántas veces cada nombre de jugador fue impostor a lo largo del historial.
  ///
  /// Recorre todas las partidas y acumula, por nombre, las veces que ese
  /// jugador fue impostor. Los nombres que nunca fueron impostor no aparecen en
  /// el mapa resultante. El recuento se hace en Dart (la lista de jugadores vive
  /// serializada en JSON, no en columnas).
  Future<Map<String, int>> impostorCountsByPlayer() async {
    final records = await getAll();
    final counts = <String, int>{};
    for (final record in records) {
      for (final player in record.players) {
        if (player.wasImpostor) {
          counts[player.name] = (counts[player.name] ?? 0) + 1;
        }
      }
    }
    return counts;
  }
}

/// Provider del repositorio de historial de partidas del Impostor.
final gameHistoryRepositoryProvider = Provider<GameHistoryRepository>((ref) {
  final database = ref.watch(appDatabaseProvider);
  return GameHistoryRepository(database: database);
});
