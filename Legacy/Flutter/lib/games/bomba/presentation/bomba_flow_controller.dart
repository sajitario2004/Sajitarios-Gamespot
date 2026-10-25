/// Controlador de flujo de una partida de La Bomba.
///
/// Orquesta el recorrido setup → jugando → explotando → gameOver (o
/// sinPrompts) que las pantallas consumen. No contiene UI: coordina la
/// [BombaConfig] actual, la [BombaSession] en curso, el prompt activo y la
/// fase del flujo.
///
/// Convenciones (Riverpod 3.x): se usa un [Notifier] expuesto con un
/// [NotifierProvider]. Las pantallas escuchan [bombaFlowControllerProvider]
/// para leer el estado y llaman a los métodos del notifier vía `.notifier`.
///
/// ## Separación timer / lógica
///
/// La MECHA de cada ronda es un [Timer] real propiedad de la pantalla
/// [BombaPlayScreen] (igual que el countdown de "Es un 10 pero" y de Tabú).
/// El controlador expone [fuseSeconds] (objetivo de la ronda) pero NO ejecuta
/// ningún wall-clock: permanece completamente síncrono y determinista para
/// tests. La pantalla llama a [explotar] cuando el Timer alcanza [fuseSeconds].
///
/// El tiempo restante de la mecha es OCULTO para los jugadores: la pantalla
/// no muestra una cuenta atrás numérica — la bomba explota "por sorpresa".
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_config.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_prompt.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_session.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/pick_prompt_use_case.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_repositories_provider.dart';

export 'package:sajitarios_gamespot/games/bomba/domain/bomba_config.dart';
export 'package:sajitarios_gamespot/games/bomba/domain/bomba_mode.dart';
export 'package:sajitarios_gamespot/games/bomba/domain/bomba_prompt.dart';
export 'package:sajitarios_gamespot/games/bomba/domain/bomba_session.dart';

/// Fase actual del flujo de la partida de La Bomba.
enum BombaFase {
  /// Configuración inicial (modo y nombres de jugadores).
  setup,

  /// Se está iniciando la partida (cargando prompts).
  iniciando,

  /// Ronda activa: jugadores pasan el móvil con la mecha corriendo.
  jugando,

  /// Eliminación breve: la mecha explotó, se revela quién fue eliminado.
  explotando,

  /// Fin de la partida: solo queda un jugador ganador.
  gameOver,

  /// Error al iniciar: no hay prompts disponibles para el modo elegido.
  error,
}

/// Tipo de error al iniciar la partida.
enum BombaErrorKind {
  /// El pool de prompts para el modo elegido está vacío.
  sinPrompts,
}

/// Estado inmutable del flujo de La Bomba.
class BombaFlowState {
  const BombaFlowState({
    required this.fase,
    this.config,
    this.session,
    this.pool = const [],
    this.currentPrompt,
    this.eliminado,
    this.errorKind,
  });

  /// Estado inicial: en configuración, sin partida.
  const BombaFlowState.initial() : this(fase: BombaFase.setup);

  /// Fase actual del flujo.
  final BombaFase fase;

  /// Configuración con la que se inició la partida (o `null` antes de iniciar).
  final BombaConfig? config;

  /// Sesión en curso (o `null` mientras se está en setup/iniciando).
  final BombaSession? session;

  /// Pool completo de prompts cargados al inicio.
  final List<BombaPrompt> pool;

  /// Prompt activo en la ronda actual.
  final BombaPrompt? currentPrompt;

  /// Nombre del jugador recién eliminado (solo en [BombaFase.explotando]).
  final String? eliminado;

  /// Tipo de error (solo en [BombaFase.error]).
  final BombaErrorKind? errorKind;

  // ── Accesores convenientes ─────────────────────────────────────────────────

  /// Duración de la mecha de la ronda actual en segundos.
  double get fuseSeconds => session?.fuseSeconds ?? 0.0;

  /// Nombre del jugador que actualmente sostiene el móvil.
  String? get currentHolder => session?.currentHolder;

  /// Jugadores vivos en el orden de rotación actual.
  List<String> get alivePlayers => session?.alivePlayers ?? const [];

  /// Ganador de la partida (solo en [BombaFase.gameOver]).
  String? get winner => session?.winner;

  BombaFlowState copyWith({
    BombaFase? fase,
    BombaConfig? config,
    BombaSession? session,
    List<BombaPrompt>? pool,
    BombaPrompt? currentPrompt,
    String? eliminado,
    BombaErrorKind? errorKind,
    bool clearEliminado = false,
    bool clearError = false,
  }) {
    return BombaFlowState(
      fase: fase ?? this.fase,
      config: config ?? this.config,
      session: session ?? this.session,
      pool: pool ?? this.pool,
      currentPrompt: currentPrompt ?? this.currentPrompt,
      eliminado: clearEliminado ? null : (eliminado ?? this.eliminado),
      errorKind: clearError ? null : (errorKind ?? this.errorKind),
    );
  }
}

/// Notifier que orquesta el flujo de una partida de La Bomba.
class BombaFlowController extends Notifier<BombaFlowState> {
  PickPromptUseCase? _pickPromptUseCase;

  @override
  BombaFlowState build() => const BombaFlowState.initial();

  // ── Iniciar ───────────────────────────────────────────────────────────────

  /// Inicia la partida con [config]:
  /// 1. Carga el pool completo de prompts desde [BombaPromptRepository].
  /// 2. Filtra los disponibles para el modo elegido.
  /// 3. Si no hay prompts → [BombaErrorKind.sinPrompts].
  /// 4. Arranca la sesión (mecha aleatoria) y elige el primer prompt.
  Future<void> iniciar(BombaConfig config) async {
    state = BombaFlowState(fase: BombaFase.iniciando, config: config);

    try {
      final repo = await ref.read(bombaPromptRepositoryProvider.future);
      final silabas = await repo.getAllSilabas();
      final categorias = await repo.getAllCategorias();
      final pool = [...silabas, ...categorias];

      final modePool = pool.where((p) => p.mode == config.mode).toList();
      if (modePool.isEmpty) {
        state = BombaFlowState(
          fase: BombaFase.error,
          config: config,
          errorKind: BombaErrorKind.sinPrompts,
        );
        return;
      }

      final rng = ref.read(randomProvider);
      _pickPromptUseCase = PickPromptUseCase(rng);

      final session = BombaSession.start(config, rng);
      final prompt = _pickPromptUseCase!.pick(mode: config.mode, pool: pool);

      state = BombaFlowState(
        fase: BombaFase.jugando,
        config: config,
        session: session,
        pool: pool,
        currentPrompt: prompt,
      );
    } catch (_) {
      state = BombaFlowState(
        fase: BombaFase.error,
        config: config,
        errorKind: BombaErrorKind.sinPrompts,
      );
    }
  }

  // ── Acciones de ronda ─────────────────────────────────────────────────────

  /// Pasa el móvil al siguiente jugador vivo.
  ///
  /// La mecha NO se reinicia — el Timer de la pantalla sigue corriendo. Solo
  /// avanza el puntero de holder en la sesión.
  ///
  /// No hace nada si no estamos en [BombaFase.jugando].
  void pasar() {
    if (state.fase != BombaFase.jugando) return;
    final session = state.session;
    if (session == null || session.isOver) return;

    state = state.copyWith(session: session.pasar());
  }

  /// Llamado por la pantalla cuando el Timer llega a [fuseSeconds].
  ///
  /// 1. Elimina al holder actual → [BombaFase.explotando] con el nombre del eliminado.
  /// 2. Si queda >1 jugador: arranca una nueva ronda (nueva mecha + nuevo prompt)
  ///    → [BombaFase.jugando]. La pantalla avanzará automáticamente.
  /// 3. Si queda 1 jugador: [BombaFase.gameOver] con el ganador.
  ///
  /// No hace nada si no estamos en [BombaFase.jugando].
  void explotar() {
    if (state.fase != BombaFase.jugando) return;
    final session = state.session;
    if (session == null || session.isOver) return;

    final nombreEliminado = session.currentHolder;
    final afterExplosion = session.explode();

    state = state.copyWith(
      fase: BombaFase.explotando,
      session: afterExplosion,
      eliminado: nombreEliminado,
    );
  }

  /// Continúa tras la fase de explosión.
  ///
  /// - Si queda >1 jugador: arranca una nueva ronda.
  /// - Si queda 1 jugador: va a [BombaFase.gameOver].
  ///
  /// No hace nada si no estamos en [BombaFase.explotando].
  void continuarTrasExplosion() {
    if (state.fase != BombaFase.explotando) return;
    final session = state.session;
    if (session == null) return;

    if (session.isOver) {
      state = state.copyWith(fase: BombaFase.gameOver, clearEliminado: true);
      return;
    }

    _iniciarNuevaRonda(session);
  }

  // ── Reiniciar ─────────────────────────────────────────────────────────────

  /// Reinicia el flujo al estado inicial (setup, sin partida).
  void reiniciar() {
    _pickPromptUseCase = null;
    state = const BombaFlowState.initial();
  }

  // ── Helpers privados ──────────────────────────────────────────────────────

  void _iniciarNuevaRonda(BombaSession sessionAfterExplosion) {
    final config = state.config;
    if (config == null) return;

    final rng = ref.read(randomProvider);
    // Nueva ronda: mismos jugadores vivos, nueva mecha aleatoria.
    final sessionNuevaRonda = BombaSession.newRound(
      previous: sessionAfterExplosion,
      config: config,
      rng: rng,
    );

    final prompt = _pickPromptUseCase?.pick(
      mode: config.mode,
      pool: state.pool,
    );

    state = state.copyWith(
      fase: BombaFase.jugando,
      session: sessionNuevaRonda,
      currentPrompt: prompt ?? state.currentPrompt,
      clearEliminado: true,
    );
  }
}

/// Provider del controlador de flujo de La Bomba.
///
/// Las pantallas leen el estado con `ref.watch(bombaFlowControllerProvider)`
/// y llaman a los métodos con
/// `ref.read(bombaFlowControllerProvider.notifier)`.
final bombaFlowControllerProvider =
    NotifierProvider<BombaFlowController, BombaFlowState>(
      BombaFlowController.new,
    );
