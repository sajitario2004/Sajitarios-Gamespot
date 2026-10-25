/// Controlador de flujo de una partida del Impostor.
///
/// Mantiene en un único sitio el estado del recorrido setup -> pass -> reveal
/// -> ... -> results que las pantallas consumen. No contiene UI: solo coordina
/// la [GameConfig] actual, la [GameSession] resultante (tras asignar roles vía
/// `assignRolesCoordinatorProvider`), el índice del jugador actual y la fase del
/// flujo.
///
/// Convenciones (Riverpod 3.x): se usa un [Notifier] expuesto con un
/// [NotifierProvider]. Las pantallas escuchan `impostorFlowControllerProvider`
/// para leer el estado y llaman a los métodos del notifier vía `.notifier`.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';

/// Fase actual del flujo de la partida.
enum ImpostorPhase {
  /// Configuración de la partida (jugadores, nº impostores, pista).
  setup,

  /// Se está iniciando la partida (asignando roles); operación asíncrona.
  iniciando,

  /// Pantalla intermedia "pásale el móvil a {jugador}".
  pass,

  /// Revelación del rol del jugador actual.
  reveal,

  /// Fin de la revelación: el grupo vota por rondas a quién expulsar.
  voting,

  /// Fin de partida (desenlace de la votación): **no** se revelan los roles.
  gameOver,

  /// Ocurrió un error al iniciar la partida (p. ej. sin palabras en la BD).
  error,
}

/// Desenlace de la votación, una vez la partida ha terminado.
enum VotingOutcome {
  /// Los jugadores pillaron a todos los impostores antes de agotar las rondas.
  jugadoresGanan,

  /// Se agotaron las rondas con impostores aún vivos: gana(n) el/los impostor(es).
  impostorGana,
}

/// Resultado del último voto emitido, para que la UI muestre feedback.
enum LastVote {
  /// Todavía no se ha votado en la ronda actual (estado inicial de la votación).
  ninguno,

  /// El último jugador votado era impostor (acierto): queda eliminado.
  eraImpostor,

  /// El último jugador votado no era impostor (fallo).
  noEraImpostor,
}

/// Tipo de error al iniciar la partida, para que la UI pueda reaccionar de
/// forma específica (p. ej. guiar a gestionar palabras si la BD está vacía).
enum ImpostorErrorKind {
  /// La base de datos no tiene ninguna palabra para jugar.
  sinPalabras,

  /// Cualquier otro error inesperado al iniciar.
  desconocido,
}

/// Estado inmutable del flujo del Impostor.
///
/// - En [ImpostorPhase.setup] / [ImpostorPhase.iniciando]: [session] es `null`.
/// - A partir de [ImpostorPhase.pass]: [session] está disponible y
///   [currentIndex] apunta al jugador en curso dentro de `session.revealOrder`.
class ImpostorFlowState {
  const ImpostorFlowState({
    required this.phase,
    this.config,
    this.session,
    this.currentIndex = 0,
    this.errorMessage,
    this.errorKind,
    this.eliminados = const {},
    this.rondaActual = 0,
    this.rondasTotales = 0,
    this.outcome,
    this.lastVote = LastVote.ninguno,
  });

  /// Estado inicial: en configuración, sin partida.
  const ImpostorFlowState.initial() : this(phase: ImpostorPhase.setup);

  /// Fase actual del flujo.
  final ImpostorPhase phase;

  /// Configuración con la que se inició la partida (o `null` antes de iniciar).
  final GameConfig? config;

  /// Partida ya resuelta (o `null` mientras se está en setup/iniciando).
  final GameSession? session;

  /// Índice del jugador actual dentro de `session.revealOrder`.
  final int currentIndex;

  /// Mensaje de error técnico (solo en [ImpostorPhase.error]), pensado para
  /// depuración/logging. NO debe usarse como texto de UI: la pantalla deriva el
  /// mensaje visible del usuario a partir de [errorKind] vía `AppLocalizations`.
  final String? errorMessage;

  /// Tipo de error (solo en [ImpostorPhase.error]), para reacción específica
  /// de la UI.
  final ImpostorErrorKind? errorKind;

  /// Jugadores ya expulsados durante la votación. Vacío fuera de la votación.
  final Set<Player> eliminados;

  /// Ronda de votación en curso (1-based). 0 mientras no hay votación.
  final int rondaActual;

  /// Total de rondas (oportunidades de voto) de la partida (= `config.rounds`).
  /// 0 mientras no hay votación.
  final int rondasTotales;

  /// Desenlace de la partida, o `null` mientras no ha terminado la votación.
  final VotingOutcome? outcome;

  /// Feedback del último voto emitido, para que la UI lo muestre.
  final LastVote lastVote;

  /// Total de jugadores de la partida (0 si todavía no hay sesión).
  int get totalPlayers => session?.revealOrder.length ?? 0;

  /// `true` si el jugador actual es el último en revelar.
  bool get esUltimoJugador =>
      session != null && currentIndex >= totalPlayers - 1;

  /// Candidatos a expulsar en la votación: jugadores aún no eliminados, en
  /// orden de revelación. Vacío si no hay sesión.
  List<Player> get candidatos {
    final order = session?.revealOrder;
    if (order == null) return const [];
    return order.where((p) => !eliminados.contains(p)).toList(growable: false);
  }

  /// Impostores que siguen vivos (no eliminados). Vacío si no hay sesión.
  List<Player> get impostoresVivos {
    final s = session;
    if (s == null) return const [];
    return s.impostores
        .where((p) => !eliminados.contains(p))
        .toList(growable: false);
  }

  ImpostorFlowState copyWith({
    ImpostorPhase? phase,
    GameConfig? config,
    GameSession? session,
    int? currentIndex,
    String? errorMessage,
    ImpostorErrorKind? errorKind,
    Set<Player>? eliminados,
    int? rondaActual,
    int? rondasTotales,
    VotingOutcome? outcome,
    LastVote? lastVote,
  }) {
    return ImpostorFlowState(
      phase: phase ?? this.phase,
      config: config ?? this.config,
      session: session ?? this.session,
      currentIndex: currentIndex ?? this.currentIndex,
      errorMessage: errorMessage ?? this.errorMessage,
      errorKind: errorKind ?? this.errorKind,
      eliminados: eliminados ?? this.eliminados,
      rondaActual: rondaActual ?? this.rondaActual,
      rondasTotales: rondasTotales ?? this.rondasTotales,
      outcome: outcome ?? this.outcome,
      lastVote: lastVote ?? this.lastVote,
    );
  }
}

/// Notifier que orquesta el flujo de una partida del Impostor.
class ImpostorFlowController extends Notifier<ImpostorFlowState> {
  @override
  ImpostorFlowState build() => const ImpostorFlowState.initial();

  /// Jugador actual en el orden de revelación, o `null` si todavía no hay
  /// partida iniciada.
  Player? get jugadorActual {
    final session = state.session;
    if (session == null) return null;
    final order = session.revealOrder;
    if (state.currentIndex < 0 || state.currentIndex >= order.length) {
      return null;
    }
    return order[state.currentIndex];
  }

  /// Inicia una partida con [config]: asigna roles vía
  /// `assignRolesCoordinatorProvider` y deja el flujo en [ImpostorPhase.pass]
  /// apuntando al primer jugador.
  ///
  /// En caso de error (p. ej. [NoWordsAvailableException]) deja el flujo en
  /// [ImpostorPhase.error] con un [ImpostorFlowState.errorKind]. El texto que ve
  /// el usuario lo deriva la UI de ese [ImpostorErrorKind] vía
  /// `AppLocalizations`; el [ImpostorFlowState.errorMessage] que se fija aquí es
  /// solo técnico (depuración/logging), no para mostrar.
  Future<void> iniciar(GameConfig config) async {
    state = ImpostorFlowState(phase: ImpostorPhase.iniciando, config: config);
    try {
      final coordinator = ref.read(assignRolesCoordinatorProvider);
      final session = await coordinator.assign(config);
      state = ImpostorFlowState(
        phase: ImpostorPhase.pass,
        config: config,
        session: session,
      );
    } on NoWordsAvailableException catch (error) {
      state = ImpostorFlowState(
        phase: ImpostorPhase.error,
        config: config,
        errorMessage: error.toString(),
        errorKind: ImpostorErrorKind.sinPalabras,
      );
    } catch (error) {
      state = ImpostorFlowState(
        phase: ImpostorPhase.error,
        config: config,
        errorMessage: error.toString(),
        errorKind: ImpostorErrorKind.desconocido,
      );
    }
  }

  /// Marca que el jugador actual va a revelar su rol: pasa de
  /// [ImpostorPhase.pass] a [ImpostorPhase.reveal] sin cambiar de jugador.
  void revelar() {
    if (state.session == null) return;
    state = state.copyWith(phase: ImpostorPhase.reveal);
  }

  /// Avanza al siguiente jugador tras ocultar el rol del actual.
  ///
  /// - Si quedan jugadores: incrementa [ImpostorFlowState.currentIndex] y vuelve
  ///   a [ImpostorPhase.pass].
  /// - Si el actual era el último: termina la revelación y abre la
  ///   [ImpostorPhase.voting] (ronda 1, sin nadie eliminado). **No** se pasa a
  ///   results: los roles no se revelan; el desenlace lo decide la votación.
  ///
  /// Devuelve `true` si la revelación ha terminado (se abrió la votación).
  bool avanzar() {
    final session = state.session;
    if (session == null) return false;
    if (state.esUltimoJugador) {
      state = state.copyWith(
        phase: ImpostorPhase.voting,
        rondaActual: 1,
        rondasTotales: state.config?.rounds ?? 1,
        eliminados: const {},
        lastVote: LastVote.ninguno,
      );
      return true;
    }
    state = state.copyWith(
      phase: ImpostorPhase.pass,
      currentIndex: state.currentIndex + 1,
    );
    return false;
  }

  /// Aplica un voto del grupo expulsando a [p] en la ronda actual.
  ///
  /// Reglas (un voto = una ronda consumida, acierte o no):
  /// - Si [p] es impostor: queda eliminado. Si ya no quedan impostores vivos ->
  ///   los jugadores ganan ([VotingOutcome.jugadoresGanan], fase
  ///   [ImpostorPhase.gameOver]); el feedback es [LastVote.eraImpostor].
  /// - Si [p] no es impostor (o aún quedan impostores tras eliminar uno): se
  ///   consume la ronda. Si se agotan las rondas con impostores vivos -> gana el
  ///   impostor ([VotingOutcome.impostorGana], fase [ImpostorPhase.gameOver],
  ///   **sin** revelar roles). Si quedan rondas, [rondaActual] avanza y el flujo
  ///   sigue en [ImpostorPhase.voting].
  ///
  /// No hace nada si no hay votación en curso o [p] ya fue eliminado.
  void votar(Player p) {
    final session = state.session;
    if (session == null) return;
    if (state.phase != ImpostorPhase.voting) return;
    if (state.eliminados.contains(p)) return;

    final acierto = session.isImpostor(p);
    final eliminados = acierto ? {...state.eliminados, p} : state.eliminados;

    // Impostores vivos tras aplicar esta eliminación.
    final quedanImpostores = session.impostores.any(
      (i) => !eliminados.contains(i),
    );

    if (acierto && !quedanImpostores) {
      // Pillados todos los impostores: ganan los jugadores.
      state = state.copyWith(
        phase: ImpostorPhase.gameOver,
        eliminados: eliminados,
        outcome: VotingOutcome.jugadoresGanan,
        lastVote: LastVote.eraImpostor,
      );
      return;
    }

    final feedback = acierto ? LastVote.eraImpostor : LastVote.noEraImpostor;

    // Cada voto consume una ronda (acierte o no).
    if (state.rondaActual >= state.rondasTotales) {
      // Se agotaron las rondas con impostores vivos: gana el impostor (sin
      // revelar identidades).
      state = state.copyWith(
        phase: ImpostorPhase.gameOver,
        eliminados: eliminados,
        outcome: VotingOutcome.impostorGana,
        lastVote: feedback,
      );
      return;
    }

    // Quedan rondas: avanzar a la siguiente y seguir votando.
    state = state.copyWith(
      phase: ImpostorPhase.voting,
      eliminados: eliminados,
      rondaActual: state.rondaActual + 1,
      lastVote: feedback,
    );
  }

  /// Reinicia el flujo al estado inicial (setup, sin partida).
  void reiniciar() {
    state = const ImpostorFlowState.initial();
  }
}

/// Provider del controlador de flujo del Impostor.
///
/// Las pantallas leen el estado con `ref.watch(impostorFlowControllerProvider)`
/// y llaman a los métodos con
/// `ref.read(impostorFlowControllerProvider.notifier)`.
final impostorFlowControllerProvider =
    NotifierProvider<ImpostorFlowController, ImpostorFlowState>(
      ImpostorFlowController.new,
    );
