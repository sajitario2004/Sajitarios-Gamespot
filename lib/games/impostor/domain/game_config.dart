/// Configuración de una partida del Impostor, con sus reglas de validación y
/// normalización.
///
/// Reglas (de `context.md` / `plan.md`):
/// - Jugadores: mínimo 3, máximo 15, en orden de introducción.
/// - Nº de impostores: elegido entre 1 y 5, **capado a `players.length - 1`**
///   en modo normal (siempre debe poder haber al menos un jugador que sepa la
///   palabra en la asignación normal).
/// - Pista: activable/desactivable ([hintEnabled]).
///
/// El tipo es inmutable y puro (sin Flutter). La construcción se hace vía
/// [GameConfig.create], que **normaliza** el nº de impostores y devuelve un
/// resultado estructurado ([GameConfigResult]) en vez de lanzar excepciones,
/// para que la capa de presentación pueda mostrar el motivo del fallo.
library;

import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';

/// Número mínimo de jugadores de una partida.
const int kMinPlayers = 3;

/// Número máximo de jugadores de una partida.
const int kMaxPlayers = 15;

/// Número mínimo de impostores seleccionable.
const int kMinImpostores = 1;

/// Número máximo de impostores seleccionable.
const int kMaxImpostores = 5;

/// Motivo por el que una [GameConfig] no se pudo construir.
enum GameConfigError {
  /// Hay menos de [kMinPlayers] jugadores.
  pocosJugadores,

  /// Hay más de [kMaxPlayers] jugadores.
  demasiadosJugadores,

  /// Dos o más jugadores comparten el mismo nombre.
  nombresDuplicados,

  /// Algún jugador tiene el nombre vacío.
  nombreVacio,
}

/// Resultado de intentar construir una [GameConfig].
///
/// O bien contiene una [config] válida ([GameConfigResult.success]), o bien un
/// [error] que explica por qué no se pudo crear ([GameConfigResult.failure]).
class GameConfigResult {
  const GameConfigResult._({this.config, this.error});

  /// Resultado correcto con la [config] ya normalizada.
  const GameConfigResult.success(GameConfig config) : this._(config: config);

  /// Resultado fallido con el [error] correspondiente.
  const GameConfigResult.failure(GameConfigError error) : this._(error: error);

  /// La configuración válida, o `null` si hubo error.
  final GameConfig? config;

  /// El motivo del fallo, o `null` si fue correcto.
  final GameConfigError? error;

  /// `true` si la configuración se construyó correctamente.
  bool get isSuccess => config != null;
}

/// Configuración validada y normalizada de una partida del Impostor.
///
/// Solo se puede obtener a través de [GameConfig.create], de modo que cualquier
/// instancia existente cumple las reglas (jugadores en rango y sin duplicados,
/// nº de impostores normalizado en `[1, min(5, players-1)]`).
class GameConfig {
  const GameConfig._({
    required this.players,
    required this.nImpostores,
    required this.hintEnabled,
  });

  /// Jugadores en orden de introducción (= orden de revelación).
  final List<Player> players;

  /// Nº de impostores ya normalizado para esta partida.
  final int nImpostores;

  /// Si está activada la opción de pista para los impostores.
  final bool hintEnabled;

  /// Tope efectivo de impostores para [players]: `min(kMaxImpostores,
  /// players.length - 1)`. Garantiza que en modo normal siempre quede al menos
  /// un jugador que sepa la palabra.
  static int maxImpostoresFor(int playerCount) {
    final cap = playerCount - 1;
    return cap < kMaxImpostores ? cap : kMaxImpostores;
  }

  /// Construye una [GameConfig] validando los jugadores y **normalizando** el
  /// nº de impostores.
  ///
  /// Validaciones sobre [players] (devuelven [GameConfigResult.failure]):
  /// - Cantidad en `[kMinPlayers, kMaxPlayers]`.
  /// - Ningún nombre vacío (tras recortar espacios).
  /// - Sin nombres duplicados (ignorando mayúsculas/minúsculas).
  ///
  /// Normalización de [nImpostores]: se recorta (clamp) al rango
  /// `[kMinImpostores, maxImpostoresFor(players.length)]`. Es decir, valores
  /// fuera de 1..5 o por encima de `players - 1` se ajustan en silencio en vez
  /// de fallar (la regla solo capa, no rechaza la partida).
  static GameConfigResult create({
    required List<Player> players,
    required int nImpostores,
    bool hintEnabled = false,
  }) {
    if (players.length < kMinPlayers) {
      return const GameConfigResult.failure(GameConfigError.pocosJugadores);
    }
    if (players.length > kMaxPlayers) {
      return const GameConfigResult.failure(
        GameConfigError.demasiadosJugadores,
      );
    }
    if (players.any((p) => p.name.trim().isEmpty)) {
      return const GameConfigResult.failure(GameConfigError.nombreVacio);
    }
    final seen = <String>{};
    for (final player in players) {
      if (!seen.add(player.name.trim().toLowerCase())) {
        return const GameConfigResult.failure(
          GameConfigError.nombresDuplicados,
        );
      }
    }

    final cap = maxImpostoresFor(players.length);
    final normalized = nImpostores.clamp(kMinImpostores, cap);

    return GameConfigResult.success(
      GameConfig._(
        // Copia inmodificable: la lista no puede mutarse desde fuera.
        players: List<Player>.unmodifiable(players),
        nImpostores: normalized,
        hintEnabled: hintEnabled,
      ),
    );
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    if (other is! GameConfig) return false;
    if (other.nImpostores != nImpostores) return false;
    if (other.hintEnabled != hintEnabled) return false;
    if (other.players.length != players.length) return false;
    for (var i = 0; i < players.length; i++) {
      if (other.players[i] != players[i]) return false;
    }
    return true;
  }

  @override
  int get hashCode =>
      Object.hash(Object.hashAll(players), nImpostores, hintEnabled);

  @override
  String toString() =>
      'GameConfig(players: $players, nImpostores: $nImpostores, '
      'hintEnabled: $hintEnabled)';
}
