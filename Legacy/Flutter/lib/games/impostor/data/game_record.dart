import 'dart:convert';

import 'package:flutter/foundation.dart';

import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/role.dart';

/// Una entrada del historial de partidas del Impostor ya jugadas.
///
/// Es un registro de solo lectura para estadísticas: la palabra de la partida,
/// su pista, el recuento de jugadores e impostores, si la pista estaba activada
/// y la lista de jugadores con su rol (en orden de revelación). La lista de
/// jugadores se persiste serializada en JSON ([playersJson]) en la columna
/// `players_json` de la tabla `game_history`.
///
/// [id] es `null` antes de insertar la fila (SQLite asigna el autoincrement).
@immutable
class GameRecord {
  const GameRecord({
    required this.createdAt,
    required this.word,
    required this.hint,
    required this.nPlayers,
    required this.nImpostors,
    required this.hintEnabled,
    required this.players,
    this.id,
  });

  /// Construye un [GameRecord] a partir del resultado de una partida.
  ///
  /// Deriva el recuento de jugadores/impostores y la lista de roles de la
  /// [session]. [hintEnabled] no forma parte de la [GameSession] (vive en la
  /// [GameConfig]), así que se pasa aparte. [createdAt] por defecto es ahora.
  factory GameRecord.fromSession(
    GameSession session, {
    required bool hintEnabled,
    DateTime? createdAt,
  }) {
    return GameRecord(
      createdAt: createdAt ?? DateTime.now(),
      word: session.word.text,
      hint: session.word.hint,
      nPlayers: session.revealOrder.length,
      nImpostors: session.impostorCount,
      hintEnabled: hintEnabled,
      players: [
        for (final player in session.revealOrder)
          GameRecordPlayer(
            name: player.name,
            wasImpostor: session.isImpostor(player),
          ),
      ],
    );
  }

  /// Construye un [GameRecord] desde una fila de la base de datos.
  factory GameRecord.fromMap(Map<String, Object?> map) {
    final decoded = jsonDecode(map['players_json'] as String) as List<dynamic>;
    return GameRecord(
      id: map['id'] as int?,
      createdAt: DateTime.fromMillisecondsSinceEpoch(map['created_at'] as int),
      word: map['word'] as String,
      hint: map['hint'] as String?,
      nPlayers: map['n_players'] as int,
      nImpostors: map['n_impostors'] as int,
      hintEnabled: (map['hint_enabled'] as int? ?? 0) != 0,
      players: [
        for (final entry in decoded)
          GameRecordPlayer.fromJson(entry as Map<String, dynamic>),
      ],
    );
  }

  /// Identificador de la fila. `null` si todavía no se ha insertado.
  final int? id;

  /// Momento en el que se jugó/guardó la partida.
  final DateTime createdAt;

  /// La palabra de la partida.
  final String word;

  /// La pista de la palabra. Puede ser `null` (columna `hint` opcional).
  final String? hint;

  /// Nº de jugadores de la partida.
  final int nPlayers;

  /// Nº de impostores efectivos de la partida.
  final int nImpostors;

  /// Si la opción de pista estaba activada en la partida.
  final bool hintEnabled;

  /// Jugadores con su rol, en orden de revelación.
  final List<GameRecordPlayer> players;

  /// Serializa la lista de jugadores a JSON para la columna `players_json`.
  String get playersJson => jsonEncode([for (final p in players) p.toJson()]);

  /// Convierte el registro en un mapa para la base de datos.
  ///
  /// Omite `id` cuando es `null` para que SQLite asigne el autoincrement.
  Map<String, Object?> toMap() {
    return <String, Object?>{
      if (id != null) 'id': id,
      'created_at': createdAt.millisecondsSinceEpoch,
      'word': word,
      'hint': hint,
      'n_players': nPlayers,
      'n_impostors': nImpostors,
      'hint_enabled': hintEnabled ? 1 : 0,
      'players_json': playersJson,
    };
  }

  @override
  String toString() =>
      'GameRecord(id: $id, word: $word, nPlayers: $nPlayers, '
      'nImpostors: $nImpostors, hintEnabled: $hintEnabled)';
}

/// Un jugador dentro de un [GameRecord]: su nombre y si fue impostor.
@immutable
class GameRecordPlayer {
  const GameRecordPlayer({required this.name, required this.wasImpostor});

  /// Construye un jugador desde su forma JSON `{name, role}`.
  ///
  /// `role` se serializa como el nombre del [Role] (`palabra` / `impostor`).
  factory GameRecordPlayer.fromJson(Map<String, dynamic> json) {
    return GameRecordPlayer(
      name: json['name'] as String,
      wasImpostor: (json['role'] as String?) == Role.impostor.name,
    );
  }

  /// Nombre del jugador.
  final String name;

  /// `true` si el jugador fue impostor en esa partida.
  final bool wasImpostor;

  /// Forma JSON `{name, role}` para serializar en `players_json`.
  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      'name': name,
      'role': (wasImpostor ? Role.impostor : Role.palabra).name,
    };
  }

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is GameRecordPlayer &&
          other.name == name &&
          other.wasImpostor == wasImpostor;

  @override
  int get hashCode => Object.hash(name, wasImpostor);

  @override
  String toString() =>
      'GameRecordPlayer(name: $name, wasImpostor: $wasImpostor)';
}
