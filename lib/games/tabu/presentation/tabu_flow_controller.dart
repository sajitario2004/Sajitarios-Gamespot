/// Controlador de flujo de una partida de Tabú.
///
/// Orquesta el recorrido setup → turno → marcador → ... → gameOver que las
/// pantallas consumen. No contiene UI: coordina la [TabuConfig] actual, la
/// [TabuSession] en curso, la [TabuWord] actual y la fase del flujo.
///
/// Convenciones (Riverpod 3.x): se usa un [Notifier] expuesto con un
/// [NotifierProvider]. Las pantallas escuchan [tabuFlowControllerProvider]
/// para leer el estado y llaman a los métodos del notifier vía `.notifier`.
///
/// ## Separación timer / lógica
///
/// El COUNTDOWN de cada turno es un `Timer.periodic` real propiedad de la
/// pantalla [TabuTurnScreen] (igual que el countdown de "Es un 10 pero"). El
/// controlador expone [turnDurationSeconds] y [aciertosTurnoActual] para que la
/// pantalla sepa cuánto tiempo mostrar y los aciertos acumulados, pero NO
/// ejecuta ningún wall-clock: permanece completamente síncrono y determinista
/// para tests. La pantalla llama a [terminarTurno] al expirar el timer.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/tabu/data/tabu_word_repository.dart';
import 'package:sajitarios_gamespot/games/tabu/domain/pick_tabu_word_use_case.dart';
import 'package:sajitarios_gamespot/games/tabu/domain/tabu_config.dart';
import 'package:sajitarios_gamespot/games/tabu/domain/tabu_session.dart';
import 'package:sajitarios_gamespot/games/tabu/domain/tabu_word.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_repositories_provider.dart';

export 'package:sajitarios_gamespot/games/tabu/domain/tabu_config.dart';
export 'package:sajitarios_gamespot/games/tabu/domain/tabu_session.dart';
export 'package:sajitarios_gamespot/games/tabu/domain/tabu_word.dart';

/// Fase actual del flujo de la partida de Tabú.
enum TabuFase {
  /// Configuración inicial (nombres de equipos, duración del turno).
  setup,

  /// Se está iniciando la partida (cargando palabras); operación asíncrona.
  iniciando,

  /// Turno activo: el equipo descriptor juega con el countdown en pantalla.
  turno,

  /// Entre turnos: marcador de victorias de ronda.
  finRonda,

  /// Fin de la partida: muestra el equipo ganador.
  gameOver,

  /// Ocurrió un error al iniciar la partida (p. ej. pool vacío).
  error,
}

/// Tipo de error al iniciar la partida, para que la UI pueda reaccionar de
/// forma específica sin depender de strings.
enum TabuErrorKind {
  /// El pool de palabras está vacío — no hay palabras para jugar.
  sinPalabras,
}

/// Estado inmutable del flujo de Tabú.
class TabuFlowState {
  const TabuFlowState({
    required this.fase,
    this.config,
    this.session,
    this.pool = const [],
    this.errorKind,
  });

  /// Estado inicial: en configuración, sin partida.
  const TabuFlowState.initial() : this(fase: TabuFase.setup);

  /// Fase actual del flujo.
  final TabuFase fase;

  /// Configuración con la que se inició la partida (o `null` antes de iniciar).
  final TabuConfig? config;

  /// Sesión en curso (o `null` mientras se está en setup/iniciando).
  final TabuSession? session;

  /// Pool de palabras disponibles (cargado al iniciar, filtrado a lo largo de
  /// la partida).
  final List<TabuWord> pool;

  /// Tipo de error (solo en [TabuFase.error]).
  final TabuErrorKind? errorKind;

  // ── Accesores convenientes ─────────────────────────────────────────────────

  /// Duración del turno en segundos (desde la config), o 60 por defecto.
  int get turnDurationSeconds => config?.turnoSegundos ?? 60;

  /// Equipo que describe en el turno actual.
  TabuEquipo? get equipoActual => session?.equipoActual;

  /// Nombre del equipo que describe actualmente.
  String? get nombreEquipoActual {
    final conf = config;
    final equipo = equipoActual;
    if (conf == null || equipo == null) return null;
    return equipo == TabuEquipo.a ? conf.equipoA : conf.equipoB;
  }

  /// Palabra secreta del turno actual.
  TabuWord? get palabraActual => session?.palabraActual;

  /// Aciertos del equipo descriptor en el turno actual.
  int get aciertosTurnoActual => session?.aciertosTurnoActual ?? 0;

  /// Victorias acumuladas del equipo A.
  int get victoriasA => session?.victoriasA ?? 0;

  /// Victorias acumuladas del equipo B.
  int get victoriasB => session?.victoriasB ?? 0;

  /// Equipo ganador (solo en [TabuFase.gameOver]).
  TabuEquipo? get ganador => session?.ganador;

  /// Nombre del equipo ganador.
  String? get nombreGanador {
    final conf = config;
    final gan = ganador;
    if (conf == null || gan == null) return null;
    return gan == TabuEquipo.a ? conf.equipoA : conf.equipoB;
  }

  TabuFlowState copyWith({
    TabuFase? fase,
    TabuConfig? config,
    TabuSession? session,
    List<TabuWord>? pool,
    TabuErrorKind? errorKind,
    bool clearError = false,
  }) {
    return TabuFlowState(
      fase: fase ?? this.fase,
      config: config ?? this.config,
      session: session ?? this.session,
      pool: pool ?? this.pool,
      errorKind: clearError ? null : (errorKind ?? this.errorKind),
    );
  }
}

/// Notifier que orquesta el flujo de una partida de Tabú.
class TabuFlowController extends Notifier<TabuFlowState> {
  @override
  TabuFlowState build() => const TabuFlowState.initial();

  // ── Iniciar ──────────────────────────────────────────────────────────────

  /// Inicia la partida con [config]:
  /// 1. Carga el pool de palabras desde [TabuWordRepository].
  /// 2. Valida que el pool no esté vacío (→ [TabuErrorKind.sinPalabras]).
  /// 3. Elige la primera palabra y deja el flujo en [TabuFase.turno].
  Future<void> iniciar(TabuConfig config) async {
    state = TabuFlowState(fase: TabuFase.iniciando, config: config);

    try {
      final wordRepo = await ref.read(tabuWordRepositoryProvider.future);
      final pool = await wordRepo.getAll();

      if (pool.isEmpty) {
        state = TabuFlowState(
          fase: TabuFase.error,
          config: config,
          errorKind: TabuErrorKind.sinPalabras,
        );
        return;
      }

      final primera = _pickWord(pool, {});
      final session = TabuSession.start(config: config, primera: primera);

      state = TabuFlowState(
        fase: TabuFase.turno,
        config: config,
        session: session,
        pool: pool,
      );
    } catch (_) {
      state = TabuFlowState(
        fase: TabuFase.error,
        config: config,
        errorKind: TabuErrorKind.sinPalabras,
      );
    }
  }

  // ── Acciones de turno ─────────────────────────────────────────────────────

  /// El equipo descriptor acertó: registra el acierto y avanza a la siguiente
  /// palabra.
  ///
  /// No hace nada si no estamos en [TabuFase.turno].
  void acierto() {
    if (state.fase != TabuFase.turno) return;
    final session = state.session;
    if (session == null || session.isOver) return;

    final updatedSession = session.registrarAcierto();
    final siguiente = _pickNextWord(updatedSession);

    state = state.copyWith(
      session: siguiente != null
          ? updatedSession.avanzarPalabra(siguiente)
          : updatedSession,
    );
  }

  /// El equipo descriptor saltó la palabra: avanza sin anotar punto.
  ///
  /// No hace nada si no estamos en [TabuFase.turno].
  void saltar() {
    if (state.fase != TabuFase.turno) return;
    final session = state.session;
    if (session == null || session.isOver) return;

    final updatedSession = session.registrarSalto();
    final siguiente = _pickNextWord(updatedSession);

    state = state.copyWith(
      session: siguiente != null
          ? updatedSession.avanzarPalabra(siguiente)
          : updatedSession,
    );
  }

  /// El equipo descriptor cometió una falta: descuenta un acierto y avanza.
  ///
  /// No hace nada si no estamos en [TabuFase.turno].
  void falta() {
    if (state.fase != TabuFase.turno) return;
    final session = state.session;
    if (session == null || session.isOver) return;

    final updatedSession = session.registrarFalta();
    final siguiente = _pickNextWord(updatedSession);

    state = state.copyWith(
      session: siguiente != null
          ? updatedSession.avanzarPalabra(siguiente)
          : updatedSession,
    );
  }

  /// Termina el turno actual (llamado por la pantalla cuando expira el timer).
  ///
  /// Aplica la regla de ronda (>=1 acierto → victoria), pasa el turno al
  /// equipo contrario y navega a [TabuFase.finRonda]. Si la partida terminó
  /// (equipo alcanzó [TabuConfig.objetivoVictorias]) va a [TabuFase.gameOver].
  ///
  /// No hace nada si no estamos en [TabuFase.turno].
  void terminarTurno() {
    if (state.fase != TabuFase.turno) return;
    final session = state.session;
    if (session == null) return;

    // Elegir la siguiente palabra para el nuevo turno.
    // Si ya no quedan palabras reutilizamos la actual (borde extremo de pool
    // pequeño — la partida sigue igualmente).
    final siguientePalabra = _pickNextWord(session) ?? session.palabraActual;

    final updated = session.terminarTurno(siguientePalabra);

    if (updated.isOver) {
      state = state.copyWith(fase: TabuFase.gameOver, session: updated);
    } else {
      state = state.copyWith(fase: TabuFase.finRonda, session: updated);
    }
  }

  /// Avanza del marcador al siguiente turno.
  ///
  /// No hace nada si no estamos en [TabuFase.finRonda].
  void siguienteTurno() {
    if (state.fase != TabuFase.finRonda) return;
    state = state.copyWith(fase: TabuFase.turno);
  }

  // ── Reiniciar ─────────────────────────────────────────────────────────────

  /// Reinicia el flujo al estado inicial (setup, sin partida).
  void reiniciar() {
    state = const TabuFlowState.initial();
  }

  // ── Helpers privados ──────────────────────────────────────────────────────

  /// Elige una palabra del pool que no esté en las [usadas] de [session].
  TabuWord _pickWord(List<TabuWord> pool, Set<int> usadas) {
    return PickTabuWordUseCase(ref.read(randomProvider))(
      pool: pool,
      usadas: usadas,
    );
  }

  /// Intenta elegir la siguiente palabra no usada. Devuelve `null` si el pool
  /// está agotado (se usa la palabra actual para continuar el turno).
  TabuWord? _pickNextWord(TabuSession session) {
    final pool = state.pool;
    final usadas = session.palabrasUsadas;
    final disponibles = pool.where((w) => !usadas.contains(w.id)).toList();
    if (disponibles.isEmpty) return null;
    return PickTabuWordUseCase(ref.read(randomProvider))(
      pool: disponibles,
      usadas: {},
    );
  }
}

/// Provider del controlador de flujo de Tabú.
///
/// Las pantallas leen el estado con `ref.watch(tabuFlowControllerProvider)`
/// y llaman a los métodos con
/// `ref.read(tabuFlowControllerProvider.notifier)`.
final tabuFlowControllerProvider =
    NotifierProvider<TabuFlowController, TabuFlowState>(TabuFlowController.new);
