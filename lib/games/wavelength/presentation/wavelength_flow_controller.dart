/// Controlador de flujo de una partida de Wavelength.
///
/// Orquesta el recorrido setup → clue → pass → guess → reveal → … → gameOver
/// que las pantallas consumen. No contiene UI: coordina la [WavelengthConfig]
/// actual, la [WavelengthSession] en curso y la ronda presente.
///
/// Convenciones (Riverpod 3.x): se usa un [Notifier] expuesto con un
/// [NotifierProvider]. Las pantallas escuchan
/// [wavelengthFlowControllerProvider] para leer el estado y llaman a los
/// métodos del notifier vía `.notifier`.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/pick_round_use_case.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_config.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_round.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_session.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_repositories_provider.dart';

export 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_config.dart';
export 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_round.dart';
export 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_session.dart';

/// Fase actual del flujo de la partida de Wavelength.
enum WavelengthFase {
  /// Configuración inicial (nombres de jugadores, número de rondas).
  setup,

  /// Se está iniciando la partida (cargando espectros); operación asíncrona.
  iniciando,

  /// El psíquico ve el objetivo en el dial y escribe una pista.
  clue,

  /// Pantalla intermedia: pásale el móvil al grupo.
  pass,

  /// El grupo mueve el dial con la pista visible pero sin objetivo.
  guess,

  /// Se revela el objetivo y se muestra la puntuación de la ronda.
  reveal,

  /// Fin de la partida: muestra la puntuación total.
  gameOver,

  /// Ocurrió un error al iniciar la partida.
  error,
}

/// Tipo de error al iniciar la partida.
enum WavelengthErrorKind {
  /// No hay espectros disponibles en la base de datos.
  sinEspectros,
}

/// Estado inmutable del flujo de Wavelength.
class WavelengthFlowState {
  const WavelengthFlowState({
    required this.fase,
    this.config,
    this.session,
    this.currentRound,
    this.errorKind,
  });

  /// Estado inicial: en configuración, sin partida.
  const WavelengthFlowState.initial() : this(fase: WavelengthFase.setup);

  /// Fase actual del flujo.
  final WavelengthFase fase;

  /// Configuración con la que se inició la partida (o `null` antes de iniciar).
  final WavelengthConfig? config;

  /// Sesión en curso (o `null` mientras se está en setup/iniciando).
  final WavelengthSession? session;

  /// Ronda activa en curso (o `null` fuera de las fases de juego).
  final WavelengthRound? currentRound;

  /// Tipo de error (solo en [WavelengthFase.error]).
  final WavelengthErrorKind? errorKind;

  /// Nombre del psíquico de la ronda actual, o `null` si no hay sesión.
  String? get currentPsychic => session?.currentPsychic;

  WavelengthFlowState copyWith({
    WavelengthFase? fase,
    WavelengthConfig? config,
    WavelengthSession? session,
    WavelengthRound? currentRound,
    WavelengthErrorKind? errorKind,
    bool clearRound = false,
    bool clearError = false,
  }) {
    return WavelengthFlowState(
      fase: fase ?? this.fase,
      config: config ?? this.config,
      session: session ?? this.session,
      currentRound: clearRound ? null : (currentRound ?? this.currentRound),
      errorKind: clearError ? null : (errorKind ?? this.errorKind),
    );
  }
}

/// Notifier que orquesta el flujo de una partida de Wavelength.
class WavelengthFlowController extends Notifier<WavelengthFlowState> {
  @override
  WavelengthFlowState build() => const WavelengthFlowState.initial();

  // ── Iniciar ──────────────────────────────────────────────────────────────

  /// Inicia la partida con [config]:
  /// 1. Carga los espectros desde [SpectrumRepository].
  /// 2. Valida que el pool no esté vacío; si lo está, deja el flujo en
  ///    [WavelengthFase.error] con [WavelengthErrorKind.sinEspectros].
  /// 3. Elige la primera ronda con [PickRoundUseCase].
  /// 4. Crea la sesión y deja el flujo en [WavelengthFase.clue].
  Future<void> iniciar(WavelengthConfig config) async {
    state = WavelengthFlowState(fase: WavelengthFase.iniciando, config: config);

    try {
      final spectrumRepo = await ref.read(spectrumRepositoryProvider.future);
      final pool = await spectrumRepo.getAll();

      if (pool.isEmpty) {
        state = WavelengthFlowState(
          fase: WavelengthFase.error,
          config: config,
          errorKind: WavelengthErrorKind.sinEspectros,
        );
        return;
      }

      final session = WavelengthSession.start(
        playerNames: config.playerNames,
        totalRondas: config.rondas,
      );

      final round = PickRoundUseCase(ref.read(randomProvider))(pool);

      state = WavelengthFlowState(
        fase: WavelengthFase.clue,
        config: config,
        session: session,
        currentRound: round,
      );
    } catch (_) {
      state = WavelengthFlowState(
        fase: WavelengthFase.error,
        config: config,
        errorKind: WavelengthErrorKind.sinEspectros,
      );
    }
  }

  // ── Confirmar pista ───────────────────────────────────────────────────────

  /// El psíquico confirma su [pista]. Avanza de [WavelengthFase.clue] a
  /// [WavelengthFase.pass].
  ///
  /// No hace nada si no estamos en [WavelengthFase.clue] o si [pista] está
  /// vacía tras normalizar.
  void confirmarPista(String pista) {
    if (state.fase != WavelengthFase.clue) return;
    final trimmed = pista.trim();
    if (trimmed.isEmpty) return;
    final round = state.currentRound;
    if (round == null) return;
    state = state.copyWith(
      fase: WavelengthFase.pass,
      currentRound: round.withClue(trimmed),
    );
  }

  // ── Pasar dispositivo ─────────────────────────────────────────────────────

  /// El grupo ya tiene el dispositivo. Avanza de [WavelengthFase.pass] a
  /// [WavelengthFase.guess].
  void pasarDispositivo() {
    if (state.fase != WavelengthFase.pass) return;
    state = state.copyWith(fase: WavelengthFase.guess);
  }

  // ── Adivinar ──────────────────────────────────────────────────────────────

  /// El grupo confirma su adivinanza con [position] (0..1).
  ///
  /// Registra el guess en la ronda actual → avanza a [WavelengthFase.reveal].
  /// No hace nada si no estamos en [WavelengthFase.guess].
  void submitGuess(double position) {
    if (state.fase != WavelengthFase.guess) return;
    final round = state.currentRound;
    if (round == null) return;
    final roundWithGuess = round.withGuess(position);
    state = state.copyWith(
      fase: WavelengthFase.reveal,
      currentRound: roundWithGuess,
    );
  }

  // ── Siguiente ronda / gameOver ────────────────────────────────────────────

  /// Avanza desde [WavelengthFase.reveal] a la siguiente ronda o a
  /// [WavelengthFase.gameOver] si era la última.
  ///
  /// No hace nada si no estamos en [WavelengthFase.reveal].
  Future<void> next() async {
    if (state.fase != WavelengthFase.reveal) return;
    final session = state.session;
    final round = state.currentRound;
    if (session == null || round == null || !round.hasGuess) return;

    final updatedSession = session.recordRound(round);

    if (updatedSession.isOver) {
      state = state.copyWith(
        fase: WavelengthFase.gameOver,
        session: updatedSession,
        clearRound: true,
      );
      return;
    }

    // Cargar pool y elegir ronda nueva.
    try {
      final spectrumRepo = await ref.read(spectrumRepositoryProvider.future);
      final pool = await spectrumRepo.getAll();
      final newRound = PickRoundUseCase(ref.read(randomProvider))(pool);
      state = state.copyWith(
        fase: WavelengthFase.clue,
        session: updatedSession,
        currentRound: newRound,
      );
    } catch (_) {
      state = state.copyWith(
        fase: WavelengthFase.gameOver,
        session: updatedSession,
        clearRound: true,
      );
    }
  }

  // ── Reiniciar ─────────────────────────────────────────────────────────────

  /// Reinicia el flujo al estado inicial (setup, sin partida).
  void reiniciar() {
    state = const WavelengthFlowState.initial();
  }
}

/// Provider del controlador de flujo de Wavelength.
///
/// Las pantallas leen el estado con
/// `ref.watch(wavelengthFlowControllerProvider)` y llaman a los métodos con
/// `ref.read(wavelengthFlowControllerProvider.notifier)`.
final wavelengthFlowControllerProvider =
    NotifierProvider<WavelengthFlowController, WavelengthFlowState>(
      WavelengthFlowController.new,
    );
