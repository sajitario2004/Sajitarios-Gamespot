/// El rol que le toca a un jugador en una partida del Impostor.
///
/// Solo existen dos casos:
/// - [Role.palabra]: el jugador conoce la palabra (no es impostor).
/// - [Role.impostor]: el jugador es impostor (no conoce la palabra; puede ver
///   la pista si la opción de pista está activada).
library;

/// Rol de un jugador: sabe la palabra o es impostor.
enum Role {
  /// El jugador conoce la palabra (no es impostor).
  palabra,

  /// El jugador es impostor (no conoce la palabra).
  impostor;

  /// `true` si este rol corresponde a un impostor.
  bool get esImpostor => this == Role.impostor;

  /// `true` si este rol corresponde a quien sabe la palabra.
  bool get sabePalabra => this == Role.palabra;
}
