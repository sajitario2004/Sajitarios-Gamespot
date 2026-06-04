import 'package:flutter/foundation.dart';

import 'package:sajitarios_gamespot/games/impostor/domain/word.dart';

/// Una palabra del juego del Impostor con su pista asociada.
///
/// Cada palabra lleva siempre una `hint` (pista) obligatoria, según la regla del
/// Impostor. El campo [isSeed] indica si la palabra viene del seed inicial: las
/// palabras seed son de solo lectura (no se pueden editar ni borrar desde la
/// app). [id] es `null` solo antes de insertar la fila (sin id asignado aún).
@immutable
class ImpostorWord {
  const ImpostorWord({
    required this.word,
    required this.hint,
    required this.createdAt,
    this.id,
    this.isSeed = false,
  });

  /// Construye una [ImpostorWord] desde una fila de la base de datos.
  factory ImpostorWord.fromMap(Map<String, Object?> map) {
    return ImpostorWord(
      id: map['id'] as int?,
      word: map['word'] as String,
      hint: map['hint'] as String,
      isSeed: (map['is_seed'] as int? ?? 0) != 0,
      createdAt: DateTime.fromMillisecondsSinceEpoch(map['created_at'] as int),
    );
  }

  /// Identificador de la fila. `null` si todavía no se ha insertado.
  final int? id;

  /// La palabra. Es única en la tabla `impostor_words`.
  final String word;

  /// La pista asociada a la palabra.
  final String hint;

  /// `true` si la palabra proviene del seed inicial (solo lectura).
  final bool isSeed;

  /// Momento de creación de la fila.
  final DateTime createdAt;

  /// Convierte la palabra en un mapa para la base de datos.
  ///
  /// Omite `id` cuando es `null` para que SQLite asigne el autoincrement.
  Map<String, Object?> toMap() {
    return <String, Object?>{
      if (id != null) 'id': id,
      'word': word,
      'hint': hint,
      'is_seed': isSeed ? 1 : 0,
      'created_at': createdAt.millisecondsSinceEpoch,
    };
  }

  /// Devuelve una copia con los campos indicados sustituidos.
  ImpostorWord copyWith({
    int? id,
    String? word,
    String? hint,
    bool? isSeed,
    DateTime? createdAt,
  }) {
    return ImpostorWord(
      id: id ?? this.id,
      word: word ?? this.word,
      hint: hint ?? this.hint,
      isSeed: isSeed ?? this.isSeed,
      createdAt: createdAt ?? this.createdAt,
    );
  }

  @override
  bool operator ==(Object other) {
    return other is ImpostorWord &&
        other.id == id &&
        other.word == word &&
        other.hint == hint &&
        other.isSeed == isSeed &&
        other.createdAt == createdAt;
  }

  @override
  int get hashCode => Object.hash(id, word, hint, isSeed, createdAt);

  @override
  String toString() {
    return 'ImpostorWord(id: $id, word: $word, hint: $hint, '
        'isSeed: $isSeed, createdAt: $createdAt)';
  }
}

/// Conversión desde la capa de datos hacia el dominio puro.
///
/// La inversión de dependencia es de un solo sentido: `data/` conoce `domain/`,
/// pero `domain/` nunca importa `data/`. Por eso el mapeo
/// [ImpostorWord] → [Word] vive aquí, en la frontera de datos.
extension ImpostorWordX on ImpostorWord {
  /// Construye una [Word] de dominio descartando los campos de persistencia
  /// (id, isSeed, createdAt).
  Word toDomain() => Word(text: word, hint: hint);
}
