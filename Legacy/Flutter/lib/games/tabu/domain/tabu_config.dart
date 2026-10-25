/// Configuracion validada e inmutable para una partida de Tabu.
///
/// Solo obtenible via [TabuConfig.create], que valida las entradas y devuelve
/// un [TabuConfigResult] (exito o fallo) para que la capa de presentacion
/// reaccione al error especifico sin capturar excepciones.
library;

/// Segundos minimos por turno.
const int kTabuMinTurnoSegundos = 30;

/// Segundos maximos por turno.
const int kTabuMaxTurnoSegundos = 120;

/// Numero de victorias de ronda necesarias para ganar la partida por defecto.
const int kTabuDefaultObjetivoVictorias = 3;

/// Razones por las que un [TabuConfig] no pudo construirse.
enum TabuConfigError {
  /// El nombre del equipo A esta vacio tras recortar espacios.
  equipoAVacio,

  /// El nombre del equipo B esta vacio tras recortar espacios.
  equipoBVacio,

  /// Los nombres de los equipos son identicos (insensible a mayusculas).
  equiposDuplicados,

  /// [turnoSegundos] esta fuera del rango [kTabuMinTurnoSegundos, kTabuMaxTurnoSegundos].
  turnoSegundosInvalido,

  /// [objetivoVictorias] es menor o igual a cero.
  objetivoVictoriasInvalido,
}

/// Resultado de intentar construir un [TabuConfig].
///
/// Contiene un [config] valido ([TabuConfigResult.success]) o un [error] que
/// describe el motivo del fallo ([TabuConfigResult.failure]).
class TabuConfigResult {
  const TabuConfigResult._({this.config, this.error});

  /// Resultado exitoso que envuelve el [config] validado.
  const TabuConfigResult.success(TabuConfig config) : this._(config: config);

  /// Resultado fallido con el motivo de [error].
  const TabuConfigResult.failure(TabuConfigError error) : this._(error: error);

  /// La configuracion valida, o `null` en caso de fallo.
  final TabuConfig? config;

  /// El motivo del fallo, o `null` en caso de exito.
  final TabuConfigError? error;

  /// `true` cuando la configuracion se construyo exitosamente.
  bool get isSuccess => config != null;
}

/// Configuracion validada para una partida de Tabu.
///
/// Garantias:
/// - Nombres de equipo no vacios y distintos (insensible a mayusculas).
/// - [turnoSegundos] en [kTabuMinTurnoSegundos, kTabuMaxTurnoSegundos].
/// - [objetivoVictorias] > 0.
class TabuConfig {
  const TabuConfig._({
    required this.equipoA,
    required this.equipoB,
    required this.turnoSegundos,
    required this.objetivoVictorias,
  });

  /// Nombre del equipo A.
  final String equipoA;

  /// Nombre del equipo B.
  final String equipoB;

  /// Duracion de cada turno en segundos.
  final int turnoSegundos;

  /// Victorias de ronda necesarias para ganar la partida.
  final int objetivoVictorias;

  /// Construye un [TabuConfig] a partir de entradas sin procesar, devolviendo
  /// un [TabuConfigResult].
  ///
  /// Reglas de validacion:
  /// - [equipoA] no puede estar vacio.
  /// - [equipoB] no puede estar vacio.
  /// - [equipoA] y [equipoB] no pueden ser iguales (insensible a mayusculas).
  /// - [turnoSegundos] debe estar en [[kTabuMinTurnoSegundos], [kTabuMaxTurnoSegundos]].
  /// - [objetivoVictorias] debe ser mayor que cero.
  static TabuConfigResult create({
    required String equipoA,
    required String equipoB,
    int turnoSegundos = 60,
    int objetivoVictorias = kTabuDefaultObjetivoVictorias,
  }) {
    final a = equipoA.trim();
    final b = equipoB.trim();

    if (a.isEmpty) {
      return const TabuConfigResult.failure(TabuConfigError.equipoAVacio);
    }
    if (b.isEmpty) {
      return const TabuConfigResult.failure(TabuConfigError.equipoBVacio);
    }
    if (a.toLowerCase() == b.toLowerCase()) {
      return const TabuConfigResult.failure(TabuConfigError.equiposDuplicados);
    }
    if (turnoSegundos < kTabuMinTurnoSegundos ||
        turnoSegundos > kTabuMaxTurnoSegundos) {
      return const TabuConfigResult.failure(
        TabuConfigError.turnoSegundosInvalido,
      );
    }
    if (objetivoVictorias <= 0) {
      return const TabuConfigResult.failure(
        TabuConfigError.objetivoVictoriasInvalido,
      );
    }

    return TabuConfigResult.success(
      TabuConfig._(
        equipoA: a,
        equipoB: b,
        turnoSegundos: turnoSegundos,
        objetivoVictorias: objetivoVictorias,
      ),
    );
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    if (other is! TabuConfig) return false;
    return other.equipoA == equipoA &&
        other.equipoB == equipoB &&
        other.turnoSegundos == turnoSegundos &&
        other.objetivoVictorias == objetivoVictorias;
  }

  @override
  int get hashCode =>
      Object.hash(equipoA, equipoB, turnoSegundos, objetivoVictorias);

  @override
  String toString() =>
      'TabuConfig(equipoA: $equipoA, equipoB: $equipoB, '
      'turnoSegundos: $turnoSegundos, objetivoVictorias: $objetivoVictorias)';
}
