/// Modelo de sesion / puntuacion de Tabu — puro de dominio, sin imports de
/// Flutter ni persistencia.
///
/// ## Regla de ronda
///
/// Los equipos se alternan en turnos de descripcion. Cada turno el equipo
/// descriptor intenta acumular el mayor numero de [aciertos]. Al finalizar un
/// turno, si el marcador del turno activo es positivo (al menos 1 acierto), el
/// equipo descriptor se anota UNA victoria de ronda. Cuando un equipo llega a
/// [TabuConfig.objetivoVictorias] victorias de ronda, la partida termina y ese
/// equipo es el ganador.
///
/// Esta regla es simple y completamente independiente del otro equipo: cada
/// turno es una oportunidad para ganar una ronda; no se comparan aciertos entre
/// equipos, sino que se premia el logro del equipo descriptor (>=1 acierto en
/// el turno).
///
/// Todas las transiciones de estado son inmutables: cada metodo devuelve un
/// nuevo [TabuSession] en lugar de mutar el actual.
library;

import 'package:sajitarios_gamespot/games/tabu/domain/tabu_config.dart';
import 'package:sajitarios_gamespot/games/tabu/domain/tabu_word.dart';

/// Identifica que equipo esta describiendo en el turno actual.
enum TabuEquipo { a, b }

/// Instantanea inmutable del estado de una sesion de Tabu.
///
/// Obtener el estado inicial con [TabuSession.start] y avanzarlo con
/// [registrarAcierto], [registrarSalto], [registrarFalta] y [terminarTurno].
class TabuSession {
  const TabuSession._({
    required this.config,
    required this.victoriasA,
    required this.victoriasB,
    required this.equipoActual,
    required this.palabraActual,
    required this.aciertosTurnoActual,
    required this.saltosUsados,
    required this.palabrasUsadas,
  });

  /// Inicia una nueva sesion con la configuracion dada y la primera palabra.
  ///
  /// [primera] es la palabra con la que comienza el primer turno. El equipo A
  /// siempre describe primero.
  factory TabuSession.start({
    required TabuConfig config,
    required TabuWord primera,
  }) {
    return TabuSession._(
      config: config,
      victoriasA: 0,
      victoriasB: 0,
      equipoActual: TabuEquipo.a,
      palabraActual: primera,
      aciertosTurnoActual: 0,
      saltosUsados: 0,
      palabrasUsadas: {primera.id},
    );
  }

  /// Configuracion de la partida (inmutable durante toda la sesion).
  final TabuConfig config;

  /// Victorias de ronda acumuladas por el equipo A.
  final int victoriasA;

  /// Victorias de ronda acumuladas por el equipo B.
  final int victoriasB;

  /// Equipo que esta describiendo en el turno actual.
  final TabuEquipo equipoActual;

  /// Palabra que se esta describiendo actualmente.
  final TabuWord palabraActual;

  /// Aciertos conseguidos por el equipo descriptor en el turno actual.
  final int aciertosTurnoActual;

  /// Numero de saltos usados en el turno actual.
  final int saltosUsados;

  /// IDs de palabras ya usadas en esta partida (para no repetir).
  final Set<int> palabrasUsadas;

  /// `true` cuando algun equipo alcanzo [TabuConfig.objetivoVictorias].
  bool get isOver =>
      victoriasA >= config.objetivoVictorias ||
      victoriasB >= config.objetivoVictorias;

  /// Equipo ganador, o `null` si la partida no ha terminado.
  TabuEquipo? get ganador {
    if (victoriasA >= config.objetivoVictorias) return TabuEquipo.a;
    if (victoriasB >= config.objetivoVictorias) return TabuEquipo.b;
    return null;
  }

  /// Registra un acierto del equipo descriptor en el turno actual.
  ///
  /// Lanza [StateError] si la partida ya termino.
  TabuSession registrarAcierto() {
    _assertNotOver();
    return _copyWith(aciertosTurnoActual: aciertosTurnoActual + 1);
  }

  /// Registra un salto (palabra omitida sin penalizacion ni acierto).
  ///
  /// Lanza [StateError] si la partida ya termino.
  TabuSession registrarSalto() {
    _assertNotOver();
    return _copyWith(saltosUsados: saltosUsados + 1);
  }

  /// Registra una falta (el descriptor dijo una palabra prohibida).
  ///
  /// Una falta descuenta un acierto del turno actual (minimo 0).
  ///
  /// Lanza [StateError] si la partida ya termino.
  TabuSession registrarFalta() {
    _assertNotOver();
    final nuevoAciertos = aciertosTurnoActual > 0 ? aciertosTurnoActual - 1 : 0;
    return _copyWith(aciertosTurnoActual: nuevoAciertos);
  }

  /// Avanza a la siguiente palabra dentro del mismo turno.
  ///
  /// [siguiente] no debe pertenecer a [palabrasUsadas].
  ///
  /// Lanza [StateError] si la partida ya termino.
  /// Lanza [ArgumentError] si [siguiente] ya fue usada en esta partida.
  TabuSession avanzarPalabra(TabuWord siguiente) {
    _assertNotOver();
    if (palabrasUsadas.contains(siguiente.id)) {
      throw ArgumentError.value(
        siguiente.id,
        'siguiente',
        'La palabra con id ${siguiente.id} ya fue usada en esta partida',
      );
    }
    return _copyWith(
      palabraActual: siguiente,
      palabrasUsadas: {...palabrasUsadas, siguiente.id},
    );
  }

  /// Termina el turno actual y otorga victoria de ronda si el equipo descriptor
  /// tuvo al menos 1 acierto.
  ///
  /// El turno pasa al equipo contrario. Los contadores de aciertos y saltos
  /// del turno se reinician. La [nuevaPalabra] sera la primera del siguiente
  /// turno.
  ///
  /// Lanza [StateError] si la partida ya termino.
  TabuSession terminarTurno(TabuWord nuevaPalabra) {
    _assertNotOver();

    final ganoRonda = aciertosTurnoActual > 0;
    final nuevoVictoriasA = (equipoActual == TabuEquipo.a && ganoRonda)
        ? victoriasA + 1
        : victoriasA;
    final nuevoVictoriasB = (equipoActual == TabuEquipo.b && ganoRonda)
        ? victoriasB + 1
        : victoriasB;
    final siguienteEquipo = equipoActual == TabuEquipo.a
        ? TabuEquipo.b
        : TabuEquipo.a;

    return TabuSession._(
      config: config,
      victoriasA: nuevoVictoriasA,
      victoriasB: nuevoVictoriasB,
      equipoActual: siguienteEquipo,
      palabraActual: nuevaPalabra,
      aciertosTurnoActual: 0,
      saltosUsados: 0,
      palabrasUsadas: {...palabrasUsadas, nuevaPalabra.id},
    );
  }

  // ── Helpers privados ──────────────────────────────────────────────────────

  void _assertNotOver() {
    if (isOver) {
      throw StateError('No se puede operar sobre una sesion que ya termino');
    }
  }

  TabuSession _copyWith({
    int? aciertosTurnoActual,
    int? saltosUsados,
    TabuWord? palabraActual,
    Set<int>? palabrasUsadas,
  }) {
    return TabuSession._(
      config: config,
      victoriasA: victoriasA,
      victoriasB: victoriasB,
      equipoActual: equipoActual,
      palabraActual: palabraActual ?? this.palabraActual,
      aciertosTurnoActual: aciertosTurnoActual ?? this.aciertosTurnoActual,
      saltosUsados: saltosUsados ?? this.saltosUsados,
      palabrasUsadas: palabrasUsadas ?? this.palabrasUsadas,
    );
  }

  @override
  String toString() =>
      'TabuSession(equipoActual: $equipoActual, '
      'victoriasA: $victoriasA, victoriasB: $victoriasB, '
      'aciertos: $aciertosTurnoActual, over: $isOver)';
}
