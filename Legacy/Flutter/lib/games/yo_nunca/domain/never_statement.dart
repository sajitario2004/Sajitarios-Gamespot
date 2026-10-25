/// Declaración inmutable "Yo nunca…" — puro dominio, sin imports de Flutter
/// ni persistencia.
library;

import 'package:sajitarios_gamespot/games/yo_nunca/domain/intensidad.dart';

/// Una declaración "Yo nunca…" con su nivel de [intensidad].
///
/// Construida vía [NeverStatement.create] que valida los invariantes y lanza
/// [ArgumentError] en caso de violación. Igualdad / hashCode basados en todos
/// los campos (value object).
class NeverStatement {
  const NeverStatement._({
    required this.id,
    required this.frase,
    required this.intensidad,
  });

  /// Identificador único (autoincrement de SQLite asignado por la capa de datos).
  final int id;

  /// Texto de la declaración. No puede estar vacío.
  final String frase;

  /// Nivel de intensidad de la declaración.
  final Intensidad intensidad;

  /// Construye un [NeverStatement] tras validar [frase].
  ///
  /// Lanza [ArgumentError] cuando:
  /// - [frase] está vacía tras recortar espacios.
  factory NeverStatement.create({
    required int id,
    required String frase,
    required Intensidad intensidad,
  }) {
    final trimmed = frase.trim();
    if (trimmed.isEmpty) {
      throw ArgumentError.value(
        frase,
        'frase',
        'La frase no puede estar vacía',
      );
    }
    return NeverStatement._(id: id, frase: trimmed, intensidad: intensidad);
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    if (other is! NeverStatement) return false;
    return other.id == id &&
        other.frase == frase &&
        other.intensidad == intensidad;
  }

  @override
  int get hashCode => Object.hash(id, frase, intensidad);

  @override
  String toString() =>
      'NeverStatement(id: $id, intensidad: $intensidad, frase: $frase)';
}
