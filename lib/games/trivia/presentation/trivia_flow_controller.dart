/// Controlador de flujo de una partida de Trivia (Preguntas por puntos).
///
/// Orquesta el recorrido setup → pass → question → ... → gameOver que las
/// pantallas consumen. No contiene UI: coordina la [TriviaConfig] actual, la
/// [TriviaSession] en curso, las preguntas repartidas para la ronda presente
/// y la fase del flujo.
///
/// Convenciones (Riverpod 3.x): se usa un [Notifier] expuesto con un
/// [NotifierProvider]. Las pantallas escuchan [triviaFlowControllerProvider]
/// para leer el estado y llaman a los métodos del notifier vía `.notifier`.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/trivia/data/question_repository.dart';
import 'package:sajitarios_gamespot/games/trivia/data/winner_repository.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/deal_questions_use_case.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/question.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/trivia_config.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/trivia_player.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/trivia_session.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_repositories_provider.dart';

export 'package:sajitarios_gamespot/games/trivia/domain/trivia_config.dart';
export 'package:sajitarios_gamespot/games/trivia/domain/trivia_player.dart';
export 'package:sajitarios_gamespot/games/trivia/domain/trivia_session.dart';

/// Fase actual del flujo de la partida de Trivia.
enum TriviaFase {
  /// Configuración inicial (nombres de jugadores, temáticas).
  setup,

  /// Se está iniciando la partida (cargando preguntas); operación asíncrona.
  iniciando,

  /// Pantalla intermedia "pásale el móvil a {jugador}".
  pass,

  /// El jugador actual ve su pregunta y elige una opción.
  question,

  /// Fin de la partida: muestra los ganadores.
  gameOver,

  /// Ocurrió un error al iniciar la partida (p. ej. pool insuficiente).
  error,
}

/// Tipo de error al iniciar la partida, para que la UI pueda reaccionar de
/// forma específica sin depender de strings.
enum TriviaErrorKind {
  /// El pool de preguntas de alguna dificultad no tiene suficientes preguntas
  /// para repartir una distinta a cada jugador en al menos una ronda.
  sinPreguntas,
}

/// Estado inmutable del flujo de Trivia.
///
/// - En [TriviaFase.setup] / [TriviaFase.iniciando]: [session] es `null`.
/// - A partir de [TriviaFase.pass]: [session] está disponible y
///   [currentPlayerIndex] apunta al jugador en curso dentro de
///   `session.alivePlayers`.
class TriviaFlowState {
  const TriviaFlowState({
    required this.fase,
    this.config,
    this.session,
    this.currentPlayerIndex = 0,
    this.currentQuestion,
    this.roundQuestions = const {},
    this.errorKind,
  });

  /// Estado inicial: en configuración, sin partida.
  const TriviaFlowState.initial() : this(fase: TriviaFase.setup);

  /// Fase actual del flujo.
  final TriviaFase fase;

  /// Configuración con la que se inició la partida (o `null` antes de iniciar).
  final TriviaConfig? config;

  /// Sesión en curso (o `null` mientras se está en setup/iniciando).
  final TriviaSession? session;

  /// Índice del jugador actual dentro de `session.alivePlayers` (0-based).
  final int currentPlayerIndex;

  /// Pregunta asignada al jugador actual en esta ronda, o `null` fuera de la
  /// fase [TriviaFase.question].
  final Question? currentQuestion;

  /// Mapa de jugador → pregunta asignada para todos los jugadores vivos de la
  /// ronda en curso. Vacío fuera de las fases pass / question.
  final Map<TriviaPlayer, Question> roundQuestions;

  /// Tipo de error (solo en [TriviaFase.error]), para reacción específica
  /// de la UI.
  final TriviaErrorKind? errorKind;

  /// Lista ordenada de jugadores aún vivos, o vacía si no hay sesión.
  List<TriviaPlayer> get alivePlayers => session?.alivePlayers ?? const [];

  /// Jugador al que le toca responder ahora, o `null` si no hay sesión.
  TriviaPlayer? get currentPlayer {
    final alive = alivePlayers;
    if (alive.isEmpty || currentPlayerIndex >= alive.length) return null;
    return alive[currentPlayerIndex];
  }

  /// Ganadores de la partida (vacío si no ha terminado o todos eliminados).
  List<TriviaPlayer> get winners => session?.winners ?? const [];

  /// Índice de la ronda actual (0-based), o 0 si no hay sesión.
  int get currentRound => session?.currentRound ?? 0;

  TriviaFlowState copyWith({
    TriviaFase? fase,
    TriviaConfig? config,
    TriviaSession? session,
    int? currentPlayerIndex,
    Question? currentQuestion,
    Map<TriviaPlayer, Question>? roundQuestions,
    TriviaErrorKind? errorKind,
    bool clearQuestion = false,
    bool clearError = false,
  }) {
    return TriviaFlowState(
      fase: fase ?? this.fase,
      config: config ?? this.config,
      session: session ?? this.session,
      currentPlayerIndex: currentPlayerIndex ?? this.currentPlayerIndex,
      currentQuestion: clearQuestion
          ? null
          : (currentQuestion ?? this.currentQuestion),
      roundQuestions: roundQuestions ?? this.roundQuestions,
      errorKind: clearError ? null : (errorKind ?? this.errorKind),
    );
  }
}

/// Notifier que orquesta el flujo de una partida de Trivia.
class TriviaFlowController extends Notifier<TriviaFlowState> {
  @override
  TriviaFlowState build() => const TriviaFlowState.initial();

  // ── Iniciar ──────────────────────────────────────────────────────────────

  /// Inicia la partida con [config]:
  /// 1. Carga los pools de preguntas por dificultad desde [QuestionRepository].
  /// 2. Valida que cada tier tenga suficientes preguntas para todos los
  ///    jugadores en las rondas correspondientes.
  /// 3. Reparte las preguntas de la ronda 0 (dificultad [Difficulty.facil]).
  /// 4. Deja el flujo en [TriviaFase.pass] apuntando al primer jugador.
  ///
  /// En caso de pool insuficiente deja el flujo en [TriviaFase.error] con
  /// [TriviaErrorKind.sinPreguntas].
  Future<void> iniciar(TriviaConfig config) async {
    state = TriviaFlowState(fase: TriviaFase.iniciando, config: config);

    try {
      final questionRepo = await ref.read(questionRepositoryProvider.future);
      final players = config.playerNames.map(TriviaPlayer.new).toList();
      final session = TriviaSession.start(players);

      // Cargar pools para los tres tiers.
      final pools = await _loadPools(questionRepo, config.selectedTematicaIds);

      // Validar que cada tier tenga suficientes preguntas.
      // Rondas 0-2 → facil, 3-5 → dificil, 6-8 → muyDificil.
      // Cada ronda necesita >= alivePlayers.length preguntas (worst case: todos
      // vivos hasta el final). Validamos con el recuento inicial (= all alive).
      if (!_poolsSufficient(pools, players.length)) {
        state = TriviaFlowState(
          fase: TriviaFase.error,
          config: config,
          errorKind: TriviaErrorKind.sinPreguntas,
        );
        return;
      }

      // Repartir ronda 0 (facil).
      final dealt = _dealRound(session, pools[Difficulty.facil]!);

      state = TriviaFlowState(
        fase: TriviaFase.pass,
        config: config,
        session: session,
        currentPlayerIndex: 0,
        roundQuestions: dealt,
      );
    } catch (_) {
      state = TriviaFlowState(
        fase: TriviaFase.error,
        config: config,
        errorKind: TriviaErrorKind.sinPreguntas,
      );
    }
  }

  // ── Pasar el dispositivo ─────────────────────────────────────────────────

  /// El jugador actual ya tiene el dispositivo: muestra su pregunta.
  ///
  /// Transición [TriviaFase.pass] → [TriviaFase.question]. No hace nada si
  /// no hay sesión activa.
  void pasarDispositivo() {
    final player = state.currentPlayer;
    if (player == null) return;
    final question = state.roundQuestions[player];
    if (question == null) return;

    state = state.copyWith(
      fase: TriviaFase.question,
      currentQuestion: question,
    );
  }

  // ── Responder ────────────────────────────────────────────────────────────

  /// El jugador actual responde con [chosenIndex].
  ///
  /// - Si es correcto: el jugador sobrevive esta ronda.
  /// - Si es incorrecto: el jugador es eliminado.
  ///
  /// Después avanza al siguiente jugador vivo de la ronda, o —si la ronda
  /// terminó— avanza a la siguiente ronda (recomputando la dificultad y
  /// repartiendo nuevas preguntas). Al acabar la ronda 8 (la novena) o cuando
  /// todos son eliminados, va a [TriviaFase.gameOver] e incrementa las victorias
  /// de los supervivientes en [WinnerRepository].
  ///
  /// No hace nada si no estamos en [TriviaFase.question].
  Future<void> responder(int chosenIndex) async {
    if (state.fase != TriviaFase.question) return;
    final session = state.session;
    if (session == null) return;
    final player = state.currentPlayer;
    if (player == null) return;
    final question = state.currentQuestion;
    if (question == null) return;

    final correct = question.isCorrect(chosenIndex);
    final updatedSession = session.recordAnswer(player, correct: correct);

    // El orden de los jugadores en la ronda es el orden de las claves en
    // roundQuestions (= alivePlayers al inicio de la ronda en orden de
    // introducción). Buscamos el siguiente jugador que siga vivo DESPUÉS del
    // actual en ese orden.
    final roundOrder = state.roundQuestions.keys.toList(growable: false);
    final nextPlayer = _nextRoundPlayer(
      updatedSession: updatedSession,
      roundOrder: roundOrder,
      currentPlayer: player,
    );

    if (nextPlayer != null) {
      // Quedan jugadores en la ronda: pasar al siguiente.
      // Calculamos su índice en updatedSession.alivePlayers para mantener
      // currentPlayerIndex coherente con el getter currentPlayer.
      final newAlive = updatedSession.alivePlayers;
      final nextIndex = newAlive.indexOf(nextPlayer);
      state = state.copyWith(
        fase: TriviaFase.pass,
        session: updatedSession,
        currentPlayerIndex: nextIndex,
        clearQuestion: true,
        roundQuestions: state.roundQuestions,
      );
    } else {
      // Ronda terminada: avanzar.
      await _finishRound(updatedSession);
    }
  }

  // ── Reiniciar ─────────────────────────────────────────────────────────────

  /// Reinicia el flujo al estado inicial (setup, sin partida).
  void reiniciar() {
    state = const TriviaFlowState.initial();
  }

  // ── Helpers privados ──────────────────────────────────────────────────────

  /// Carga los pools de preguntas para cada dificultad.
  Future<Map<Difficulty, List<Question>>> _loadPools(
    QuestionRepository repo,
    Set<String> tematicaIds,
  ) async {
    final Map<Difficulty, List<Question>> pools = {};
    for (final difficulty in Difficulty.values) {
      pools[difficulty] = await repo.getPool(
        tematicaIds: tematicaIds,
        difficulty: difficulty,
      );
    }
    return pools;
  }

  /// Verifica que cada tier tenga al menos [playerCount] preguntas.
  bool _poolsSufficient(
    Map<Difficulty, List<Question>> pools,
    int playerCount,
  ) {
    for (final difficulty in Difficulty.values) {
      final pool = pools[difficulty];
      if (pool == null || pool.length < playerCount) return false;
    }
    return true;
  }

  /// Reparte una pregunta distinta a cada jugador vivo usando
  /// [DealQuestionsUseCase].
  Map<TriviaPlayer, Question> _dealRound(
    TriviaSession session,
    List<Question> pool,
  ) {
    final useCase = DealQuestionsUseCase(ref.read(randomProvider));
    return useCase(alivePlayers: session.alivePlayers, pool: pool);
  }

  /// Avanza a la siguiente ronda o termina la partida.
  Future<void> _finishRound(TriviaSession session) async {
    // Si no quedan jugadores vivos: game over inmediato.
    if (session.alivePlayers.isEmpty) {
      await _goGameOver(session);
      return;
    }

    // Avanzar la ronda en el modelo de dominio.
    final nextSession = session.advanceRound();

    if (nextSession.isOver) {
      // Todas las rondas completadas.
      await _goGameOver(nextSession);
      return;
    }

    // Cargar pool para la nueva ronda y repartir preguntas.
    try {
      final config = state.config!;
      final questionRepo = await ref.read(questionRepositoryProvider.future);
      final pools = await _loadPools(questionRepo, config.selectedTematicaIds);
      final difficulty = difficultyForRound(nextSession.currentRound);
      final dealt = _dealRound(nextSession, pools[difficulty]!);

      state = state.copyWith(
        fase: TriviaFase.pass,
        session: nextSession,
        currentPlayerIndex: 0,
        roundQuestions: dealt,
        clearQuestion: true,
      );
    } catch (_) {
      state = TriviaFlowState(
        fase: TriviaFase.error,
        config: state.config,
        errorKind: TriviaErrorKind.sinPreguntas,
      );
    }
  }

  /// Finaliza la partida: incrementa victorias y va a [TriviaFase.gameOver].
  Future<void> _goGameOver(TriviaSession session) async {
    final survivors = session.winners;
    if (survivors.isNotEmpty) {
      final winnerRepo = await ref.read(winnerRepositoryProvider.future);
      for (final winner in survivors) {
        await winnerRepo.incrementWins(winner.name);
      }
    }

    state = state.copyWith(
      fase: TriviaFase.gameOver,
      session: session,
      clearQuestion: true,
      roundQuestions: const {},
    );
  }

  /// Devuelve el siguiente jugador en el orden de la ronda que siga vivo, o
  /// `null` si no quedan más jugadores para esta ronda.
  ///
  /// [roundOrder] es la lista de jugadores al inicio de la ronda (en orden de
  /// introducción). [currentPlayer] es quien acaba de responder. Busca el
  /// primero posterior a [currentPlayer] que esté en [updatedSession.alivePlayers].
  TriviaPlayer? _nextRoundPlayer({
    required TriviaSession updatedSession,
    required List<TriviaPlayer> roundOrder,
    required TriviaPlayer currentPlayer,
  }) {
    final stillAlive = updatedSession.alivePlayers.toSet();
    final currentPos = roundOrder.indexOf(currentPlayer);
    for (var i = currentPos + 1; i < roundOrder.length; i++) {
      if (stillAlive.contains(roundOrder[i])) return roundOrder[i];
    }
    return null;
  }
}

/// Provider del controlador de flujo de Trivia.
///
/// Las pantallas leen el estado con `ref.watch(triviaFlowControllerProvider)`
/// y llaman a los métodos con
/// `ref.read(triviaFlowControllerProvider.notifier)`.
final triviaFlowControllerProvider =
    NotifierProvider<TriviaFlowController, TriviaFlowState>(
      TriviaFlowController.new,
    );
