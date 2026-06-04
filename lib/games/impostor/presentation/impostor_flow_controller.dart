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

  /// Fin de partida: se muestran todos los roles.
  results,

  /// Ocurrió un error al iniciar la partida (p. ej. sin palabras en la BD).
  error,
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

  /// Total de jugadores de la partida (0 si todavía no hay sesión).
  int get totalPlayers => session?.revealOrder.length ?? 0;

  /// `true` si el jugador actual es el último en revelar.
  bool get esUltimoJugador =>
      session != null && currentIndex >= totalPlayers - 1;

  ImpostorFlowState copyWith({
    ImpostorPhase? phase,
    GameConfig? config,
    GameSession? session,
    int? currentIndex,
    String? errorMessage,
    ImpostorErrorKind? errorKind,
  }) {
    return ImpostorFlowState(
      phase: phase ?? this.phase,
      config: config ?? this.config,
      session: session ?? this.session,
      currentIndex: currentIndex ?? this.currentIndex,
      errorMessage: errorMessage ?? this.errorMessage,
      errorKind: errorKind ?? this.errorKind,
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
  /// - Si el actual era el último: pasa a [ImpostorPhase.results].
  ///
  /// Devuelve `true` si la partida ha terminado (se llegó a results).
  bool avanzar() {
    final session = state.session;
    if (session == null) return false;
    if (state.esUltimoJugador) {
      state = state.copyWith(phase: ImpostorPhase.results);
      return true;
    }
    state = state.copyWith(
      phase: ImpostorPhase.pass,
      currentIndex: state.currentIndex + 1,
    );
    return false;
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
