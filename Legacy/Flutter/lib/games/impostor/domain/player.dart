/// Un jugador de una partida del Impostor.
///
/// Es un objeto de valor inmutable y puro (sin dependencias de Flutter). El
/// orden en el que se introducen los jugadores es significativo: define el
/// **orden de revelación** durante la partida (ver `GameSession.revealOrder`).
/// Por eso la lista de jugadores siempre se maneja como `List<Player>`
/// ordenada, nunca como conjunto.
library;

/// Un jugador identificado por su [name].
class Player {
  const Player(this.name);

  /// El nombre del jugador, tal como se introdujo en la configuración.
  final String name;

  @override
  bool operator ==(Object other) =>
      identical(this, other) || other is Player && other.name == name;

  @override
  int get hashCode => name.hashCode;

  @override
  String toString() => 'Player($name)';
}
