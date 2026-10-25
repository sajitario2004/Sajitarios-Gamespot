/// Comprehensive overflow audit — every screen at textScaler 2.0 on 320x600.
///
/// Purpose: find screens where text or layout overflows at large text scales
/// and/or small surfaces (worst-case: 320x600 + textScaler 2.0). Screens that
/// embed a Flame [GameWidget] are skipped (see SKIPPED notes below) because the
/// Flame game loop is incompatible with a plain widget test environment.
///
/// Rules:
/// - NEVER use pumpAndSettle (infinite animations like PulseGlow will hang).
/// - Use await tester.pump() then await tester.pump(const Duration(ms: 400)).
/// - sqflite-ffi is initialized in setUpAll; never call sqflite inside testWidgets.
/// - Preloaded controllers avoid async repo calls inside testWidgets bodies.
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/audio/audio_service.dart';
import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/_shared/game_descriptor.dart';
import 'package:sajitarios_gamespot/games/_shared/game_registry.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/rules_screen.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_flow_controller.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_repositories_provider.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_setup_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/data/game_history_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/role.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/word.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/game_over_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/history_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/pass_device_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/reveal_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/setup_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/voting_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/words_management_screen.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_flow_controller.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_repositories_provider.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_scoreboard_screen.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_setup_screen.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_turn_screen.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/question.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_flow_controller.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_pass_device_screen.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_question_screen.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_repositories_provider.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_setup_screen.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/spectrum.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_clue_screen.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_flow_controller.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_guess_screen.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_pass_device_screen.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_repositories_provider.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_reveal_screen.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_setup_screen.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_flow_controller.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_play_screen.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_repositories_provider.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_setup_screen.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';
import 'package:sajitarios_gamespot/menu/menu_screen.dart';

import 'games/bomba/presentation/support/fake_bomba_prompt_repository.dart';
import 'games/impostor/presentation/support/fake_assign_roles_coordinator.dart';
import 'games/impostor/presentation/support/fake_game_history_repository.dart';
import 'games/tabu/presentation/support/fake_tabu_word_repository.dart';
import 'games/trivia/presentation/support/fake_question_repository.dart';
import 'games/trivia/presentation/support/fake_winner_repository.dart';
import 'games/wavelength/presentation/support/fake_spectrum_repository.dart';
import 'games/yo_nunca/presentation/support/fake_never_statement_repository.dart';

// ---------------------------------------------------------------------------
// Surface helpers
// ---------------------------------------------------------------------------

// ProviderOverride is the concrete type returned by provider.overrideWith(...).
// flutter_riverpod exposes it, but the public API name for the list element is
// just `Override` from package:riverpod_annotation or we can use dynamic. In
// practice the accepted type on ProviderScope.overrides is List<Override> where
// Override is the riverpod internal typedef; we use the same trick as the
// existing test file: pass typed lists where needed and avoid the alias here by
// using the concrete ProviderScope directly in each helper.

/// Wraps [child] in a 320×600 surface with textScaler 2.0 and Spanish locale.
Widget _scaledSmall(Widget child) {
  return MaterialApp(
    locale: const Locale('es'),
    localizationsDelegates: AppLocalizations.localizationsDelegates,
    supportedLocales: AppLocalizations.supportedLocales,
    home: MediaQuery(
      data: const MediaQueryData(
        size: Size(320, 600),
        textScaler: TextScaler.linear(2.0),
      ),
      child: SizedBox(width: 320, height: 600, child: child),
    ),
  );
}

/// Same as [_scaledSmall] but wrapped in [ProviderScope] with [overrides].
Widget _scaledSmallScoped(Widget child, {List<Object?> overrides = const []}) {
  // `Override` is not publicly importable in riverpod 3.x; the context-typed
  // `.cast()` infers `List<Override>` from ProviderScope.overrides.
  return ProviderScope(overrides: overrides.cast(), child: _scaledSmall(child));
}

/// Wraps [child] in a router-backed MaterialApp with 320×600 + textScaler 2.0.
/// The router renders [child] at the root route `/`.
Widget _scaledSmallRouted(Widget child, {List<Object?> overrides = const []}) {
  final router = GoRouter(
    initialLocation: '/',
    routes: [
      GoRoute(path: '/', builder: (_, _) => child),
      // Stub routes so go_router does not throw for navigation calls
      GoRoute(path: '/menu', name: 'menu', builder: (_, _) => const Scaffold()),
      GoRoute(
        path: '/impostor/setup',
        name: 'impostor-setup',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/impostor/pass',
        name: 'impostor-pass',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/impostor/reveal',
        name: 'impostor-reveal',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/impostor/voting',
        name: 'impostor-voting',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/impostor/game-over',
        name: 'impostor-game-over',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/trivia/pass',
        name: 'trivia-pass',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/trivia/question',
        name: 'trivia-question',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/trivia/game-over',
        name: 'trivia-game-over',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/wavelength/setup',
        name: 'wavelength-setup',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/wavelength/clue',
        name: 'wavelength-clue',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/wavelength/pass',
        name: 'wavelength-pass',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/wavelength/guess',
        name: 'wavelength-guess',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/wavelength/reveal',
        name: 'wavelength-reveal',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/wavelength/game-over',
        name: 'wavelength-game-over',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/tabu/setup',
        name: 'tabu-setup',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/tabu/turn',
        name: 'tabu-turn',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/tabu/scoreboard',
        name: 'tabu-scoreboard',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/tabu/game-over',
        name: 'tabu-game-over',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/yo-nunca/setup',
        name: 'yo-nunca-setup',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/bomba/setup',
        name: 'bomba-setup',
        builder: (_, _) => const Scaffold(),
      ),
      GoRoute(
        path: '/bomba/game-over',
        name: 'bomba-game-over',
        builder: (_, _) => const Scaffold(),
      ),
    ],
  );
  return ProviderScope(
    overrides: overrides.cast(),
    child: MaterialApp.router(
      locale: const Locale('es'),
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      routerConfig: router,
      builder: (context, child) {
        return MediaQuery(
          data: const MediaQueryData(
            size: Size(320, 600),
            textScaler: TextScaler.linear(2.0),
          ),
          child: SizedBox(width: 320, height: 600, child: child!),
        );
      },
    ),
  );
}

// ---------------------------------------------------------------------------
// Noop audio service — prevents AudioService from touching platform channels
// ---------------------------------------------------------------------------

class _NoopAudio implements AudioService {
  @override
  Future<void> preload() async {}
  @override
  bool get enabled => false;
  @override
  void setEnabled(bool value) {}
  @override
  bool toggle() => false;
  @override
  void playCardFlip() {}
  @override
  void playReveal() {}
  @override
  void playGameOver() {}
  @override
  void play(AppSound sound, {double volume = 1.0}) {}
}

// ---------------------------------------------------------------------------
// Fake game descriptor for MenuScreen
// ---------------------------------------------------------------------------

class _FakeGame extends GameDescriptor {
  const _FakeGame(this._id, this._title, this._desc);
  final String _id;
  final String _title;
  final String _desc;
  @override
  String get id => _id;
  @override
  String get title => _title;
  @override
  String get description => _desc;
  @override
  IconData get icon => Icons.videogame_asset;
  @override
  Widget buildEntryScreen(BuildContext context) =>
      const Scaffold(body: Text('ENTRADA'));
}

// ---------------------------------------------------------------------------
// Impostor preloaded controllers
// ---------------------------------------------------------------------------

/// Builds a deterministic GameSession from named players.
GameSession _buildSession({
  List<String>? nombres,
  String palabra = 'Palabra extremadamente larga que podría desbordarse',
  String pista = 'Pista bastante larga con muchas palabras consecutivas',
}) {
  final players =
      (nombres ??
              [
                'JugadorConNombreMuyLargoQueDesborda',
                'OtroNombreQueEsTambienLargo',
                'Tercero',
              ])
          .map(Player.new)
          .toList(growable: false);
  final assignments = <Player, Role>{
    for (var i = 0; i < players.length; i++)
      players[i]: i == 0 ? Role.impostor : Role.palabra,
  };
  return GameSession(
    word: Word(text: palabra, hint: pista),
    players: players,
    assignments: assignments,
  );
}

/// Builds an ImpostorFlowState in the given [phase] with a pre-built session.
class _PreloadedImpostorController extends ImpostorFlowController {
  _PreloadedImpostorController(this._state);
  final ImpostorFlowState _state;
  @override
  ImpostorFlowState build() => _state;
}

ImpostorFlowState _impostorPassState(GameSession session) {
  final config = GameConfig.create(
    players: session.players,
    nImpostores: 1,
    hintEnabled: true,
  ).config!;
  return ImpostorFlowState(
    phase: ImpostorPhase.pass,
    config: config,
    session: session,
    currentIndex: 0,
  );
}

ImpostorFlowState _impostorRevealState(GameSession session) =>
    _impostorPassState(session).copyWith(phase: ImpostorPhase.reveal);

ImpostorFlowState _impostorVotingState(GameSession session) {
  final config = GameConfig.create(
    players: session.players,
    nImpostores: 1,
    hintEnabled: true,
  ).config!;
  return ImpostorFlowState(
    phase: ImpostorPhase.voting,
    config: config,
    session: session,
    currentIndex: 0,
    rondaActual: 1,
    rondasTotales: config.rounds,
  );
}

ImpostorFlowState _impostorGameOverState(GameSession session) {
  final config = GameConfig.create(
    players: session.players,
    nImpostores: 1,
    hintEnabled: true,
  ).config!;
  return ImpostorFlowState(
    phase: ImpostorPhase.gameOver,
    config: config,
    session: session,
    outcome: VotingOutcome.jugadoresGanan,
  );
}

// ---------------------------------------------------------------------------
// Trivia preloaded controller
// ---------------------------------------------------------------------------

TriviaFlowState _triviaPassState() {
  final players = [
    'JugadorNombreLargo',
    'OtroJugadorLargo',
    'Tercero',
  ].map(TriviaPlayer.new).toList();
  final session = TriviaSession.start(players);
  final q = Question.create(
    id: 1,
    tematicaId: 'historia',
    difficulty: Difficulty.facil,
    enunciado:
        'Enunciado de pregunta bastante largo que podría desbordarse en pantallas pequeñas con texto escalado',
    options: [
      'Respuesta correcta bastante larga',
      'Respuesta incorrecta larga también',
      'Otra respuesta equivocada',
      'Cuarta opción también larga',
    ],
    correctIndex: 0,
  );
  return TriviaFlowState(
    fase: TriviaFase.pass,
    session: session,
    currentQuestion: q,
    currentPlayerIndex: 0,
  );
}

TriviaFlowState _triviaQuestionState() {
  final players = [
    'JugadorNombreLargo',
    'OtroJugadorLargo',
    'Tercero',
  ].map(TriviaPlayer.new).toList();
  final session = TriviaSession.start(players);
  final q = Question.create(
    id: 1,
    tematicaId: 'historia',
    difficulty: Difficulty.facil,
    enunciado:
        'Enunciado de pregunta bastante largo que podría desbordarse en pantallas pequeñas con texto escalado',
    options: [
      'Respuesta correcta bastante larga',
      'Respuesta incorrecta larga también',
      'Otra respuesta equivocada',
      'Cuarta opción también larga',
    ],
    correctIndex: 0,
  );
  return TriviaFlowState(
    fase: TriviaFase.question,
    session: session,
    currentQuestion: q,
    currentPlayerIndex: 0,
  );
}

TriviaFlowState _triviaGameOverState() {
  final all = [
    'Ana',
    'Luis',
    'Marta',
    'Pedro',
    'Sofía',
  ].map(TriviaPlayer.new).toList();
  var session = TriviaSession.start(all);
  // Eliminate everyone except first two.
  for (final p in all.skip(2)) {
    session = session.recordAnswer(p, correct: false);
  }
  while (!session.isOver) {
    session = session.advanceRound();
  }
  return TriviaFlowState(fase: TriviaFase.gameOver, session: session);
}

class _PreloadedTriviaController extends TriviaFlowController {
  _PreloadedTriviaController(this._initial);
  final TriviaFlowState _initial;
  @override
  TriviaFlowState build() => _initial;
}

// ---------------------------------------------------------------------------
// Wavelength preloaded controller
// ---------------------------------------------------------------------------

final _testSpectrum = Spectrum(
  id: 1,
  leftConcept: 'Concepto izquierdo bastante largo',
  rightConcept: 'Concepto derecho también largo',
);

WavelengthFlowState _wavelengthPassState() {
  final session = WavelengthSession.start(
    playerNames: ['JugadorPsicoLargo', 'OtroJugador', 'Tercero'],
    totalRondas: 3,
  );
  final round = WavelengthRound.start(
    spectrum: _testSpectrum,
    targetPosition: 0.5,
  ).withClue('Pista bastante larga del psíquico');
  return WavelengthFlowState(
    fase: WavelengthFase.pass,
    config: WavelengthConfig.create(
      playerNames: ['JugadorPsicoLargo', 'OtroJugador', 'Tercero'],
      rondas: 3,
    ).config,
    session: session,
    currentRound: round,
  );
}

WavelengthFlowState _wavelengthRevealState() {
  final session = WavelengthSession.start(
    playerNames: ['JugadorPsicoLargo', 'OtroJugador', 'Tercero'],
    totalRondas: 3,
  );
  final round = WavelengthRound.start(
    spectrum: _testSpectrum,
    targetPosition: 0.5,
  ).withClue('Pista bastante larga').withGuess(0.6);
  return WavelengthFlowState(
    fase: WavelengthFase.reveal,
    config: WavelengthConfig.create(
      playerNames: ['JugadorPsicoLargo', 'OtroJugador', 'Tercero'],
      rondas: 3,
    ).config,
    session: session,
    currentRound: round,
  );
}

WavelengthFlowState _wavelengthGameOverState() {
  var session = WavelengthSession.start(
    playerNames: ['JugadorPsicoLargo', 'OtroJugador', 'Tercero'],
    totalRondas: 2,
  );
  final round1 = WavelengthRound.start(
    spectrum: _testSpectrum,
    targetPosition: 0.5,
  ).withClue('pista').withGuess(0.5);
  session = session.recordRound(round1);
  final round2 = WavelengthRound.start(
    spectrum: _testSpectrum,
    targetPosition: 0.3,
  ).withClue('otra pista').withGuess(0.3);
  session = session.recordRound(round2);
  return WavelengthFlowState(
    fase: WavelengthFase.gameOver,
    config: WavelengthConfig.create(
      playerNames: ['JugadorPsicoLargo', 'OtroJugador', 'Tercero'],
      rondas: 2,
    ).config,
    session: session,
  );
}

WavelengthFlowState _wavelengthClueState() {
  final session = WavelengthSession.start(
    playerNames: ['JugadorPsicoLargo', 'OtroJugador', 'Tercero'],
    totalRondas: 3,
  );
  final round = WavelengthRound.start(
    spectrum: _testSpectrum,
    targetPosition: 0.5,
  );
  return WavelengthFlowState(
    fase: WavelengthFase.clue,
    config: WavelengthConfig.create(
      playerNames: ['JugadorPsicoLargo', 'OtroJugador', 'Tercero'],
      rondas: 3,
    ).config,
    session: session,
    currentRound: round,
  );
}

WavelengthFlowState _wavelengthGuessState() {
  final session = WavelengthSession.start(
    playerNames: ['JugadorPsicoLargo', 'OtroJugador', 'Tercero'],
    totalRondas: 3,
  );
  final round = WavelengthRound.start(
    spectrum: _testSpectrum,
    targetPosition: 0.5,
  ).withClue('Pista bastante larga del psíquico para forzar el ancho');
  return WavelengthFlowState(
    fase: WavelengthFase.guess,
    config: WavelengthConfig.create(
      playerNames: ['JugadorPsicoLargo', 'OtroJugador', 'Tercero'],
      rondas: 3,
    ).config,
    session: session,
    currentRound: round,
  );
}

class _PreloadedWavelengthController extends WavelengthFlowController {
  _PreloadedWavelengthController(this._initial);
  final WavelengthFlowState _initial;
  @override
  WavelengthFlowState build() => _initial;
}

// ---------------------------------------------------------------------------
// Tabu preloaded controller
// ---------------------------------------------------------------------------

TabuFlowState _tabuTurnState() {
  final config = TabuConfig.create(
    equipoA: 'Equipo Rojo Nombre Largo',
    equipoB: 'Equipo Azul Nombre Largo',
    turnoSegundos: 60,
  ).config!;
  final word = TabuWord.create(
    id: 1,
    palabra: 'PalabraTabuLarga',
    prohibidas: [
      'prohibida1larga',
      'prohibida2larga',
      'prohibida3larga',
      'prohibida4larga',
    ],
  );
  final session = TabuSession.start(config: config, primera: word);
  return TabuFlowState(
    fase: TabuFase.turno,
    config: config,
    session: session,
    pool: [word],
  );
}

TabuFlowState _tabuScoreboardState() {
  final config = TabuConfig.create(
    equipoA: 'Equipo Rojo Nombre Largo',
    equipoB: 'Equipo Azul Nombre Largo',
    turnoSegundos: 60,
  ).config!;
  final word = TabuWord.create(
    id: 1,
    palabra: 'PalabraTabu',
    prohibidas: ['prohibida1', 'prohibida2', 'prohibida3', 'prohibida4'],
  );
  var session = TabuSession.start(config: config, primera: word);
  session = session.registrarAcierto();
  session = session.terminarTurno(word);
  return TabuFlowState(
    fase: TabuFase.finRonda,
    config: config,
    session: session,
    pool: [word],
  );
}

TabuFlowState _tabuGameOverState() {
  final config = TabuConfig.create(
    equipoA: 'Equipo Rojo Nombre Largo',
    equipoB: 'Equipo Azul Nombre Largo',
    turnoSegundos: 60,
    objetivoVictorias: 1,
  ).config!;
  final word = TabuWord.create(
    id: 1,
    palabra: 'PalabraTabu',
    prohibidas: ['prohibida1', 'prohibida2', 'prohibida3', 'prohibida4'],
  );
  var session = TabuSession.start(config: config, primera: word);
  session = session.registrarAcierto();
  session = session.terminarTurno(word);
  return TabuFlowState(
    fase: TabuFase.gameOver,
    config: config,
    session: session,
    pool: [word],
  );
}

class _PreloadedTabuController extends TabuFlowController {
  _PreloadedTabuController(this._initial);
  final TabuFlowState _initial;
  @override
  TabuFlowState build() => _initial;
}

// ---------------------------------------------------------------------------
// Yo Nunca preloaded controller
// ---------------------------------------------------------------------------

YoNuncaFlowState _yoNuncaJugandoState() {
  final statement = NeverStatement.create(
    id: 1,
    frase:
        'Yo nunca he hecho algo extremadamente interesante que requiere texto muy largo para probar el overflow',
    intensidad: Intensidad.suave,
  );
  return YoNuncaFlowState(
    fase: YoNuncaFase.jugando,
    config: YoNuncaConfig.create(intensidades: {Intensidad.suave}).config,
    pool: [statement],
    fraseActual: statement,
    seen: {1},
  );
}

class _PreloadedYoNuncaController extends YoNuncaFlowController {
  _PreloadedYoNuncaController(this._initial);
  final YoNuncaFlowState _initial;
  @override
  YoNuncaFlowState build() => _initial;
}

// ---------------------------------------------------------------------------
// Bomba preloaded controller (reuse from responsive_overflow_test pattern)
// ---------------------------------------------------------------------------

class _BombaGameOverController extends BombaFlowController {
  _BombaGameOverController(this._winnerName);
  final String _winnerName;

  @override
  BombaFlowState build() {
    final config = BombaConfig.create(
      mode: BombaMode.silaba,
      playerNames: [_winnerName, 'Eliminado'],
      minSegundos: 10,
      maxSegundos: 60,
    ).config!;
    final rng = RandomProvider.seeded(0);
    final initial = BombaSession.start(config, rng);
    final afterPass = initial.pasar();
    final afterExplosion = afterPass.explode();
    return BombaFlowState(
      fase: BombaFase.gameOver,
      config: config,
      session: afterExplosion,
    );
  }
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  // ══════════════════════════════════════════════════════════════════════════
  // SETUP SCREENS
  // ══════════════════════════════════════════════════════════════════════════

  group('Setup screens — textScaler 2.0 — no overflow', () {
    testWidgets('impostor SetupScreen (320x600)', (tester) async {
      await tester.pumpWidget(
        _scaledSmallRouted(
          const SetupScreen(),
          overrides: [
            assignRolesCoordinatorProvider.overrideWithValue(
              FakeAssignRolesCoordinator(),
            ),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('trivia TriviaSetupScreen (320x600)', (tester) async {
      await tester.pumpWidget(
        _scaledSmallScoped(
          const TriviaSetupScreen(),
          overrides: [
            questionRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeQuestionRepo()),
            ),
            winnerRepositoryProvider.overrideWith(
              (ref) => Future.value(FakeWinnerRepository()),
            ),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('wavelength WavelengthSetupScreen (320x600)', (tester) async {
      await tester.pumpWidget(
        _scaledSmallScoped(
          const WavelengthSetupScreen(),
          overrides: [
            spectrumRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeSpectrumRepo()),
            ),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('tabu TabuSetupScreen (320x600)', (tester) async {
      await tester.pumpWidget(
        _scaledSmallScoped(
          const TabuSetupScreen(),
          overrides: [
            tabuWordRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeTabuRepo()),
            ),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('yo_nunca YoNuncaSetupScreen (320x600)', (tester) async {
      await tester.pumpWidget(
        _scaledSmallScoped(
          const YoNuncaSetupScreen(),
          overrides: [
            neverStatementRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeRepo()),
            ),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('bomba BombaSetupScreen (320x600)', (tester) async {
      // makeFakeRepo() uses sqflite-ffi (real I/O) and cannot run inside the
      // testWidgets fake-async zone: build it via runAsync, then inject it.
      final repo = await tester.runAsync(makeFakeRepo);
      await tester.pumpWidget(
        _scaledSmallScoped(
          const BombaSetupScreen(),
          overrides: [
            bombaPromptRepositoryProvider.overrideWith((ref) async => repo!),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });
  });

  // ══════════════════════════════════════════════════════════════════════════
  // IMPOSTOR FLOW SCREENS
  // ══════════════════════════════════════════════════════════════════════════

  group('Impostor flow screens — textScaler 2.0 — no overflow', () {
    final session = _buildSession();

    testWidgets('PassDeviceScreen — long player name (320x600)', (
      tester,
    ) async {
      await tester.pumpWidget(
        _scaledSmallRouted(
          const PassDeviceScreen(),
          overrides: [
            impostorFlowControllerProvider.overrideWith(
              () => _PreloadedImpostorController(_impostorPassState(session)),
            ),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('RevealScreen — impostor with long word+hint (320x600)', (
      tester,
    ) async {
      await tester.pumpWidget(
        _scaledSmallScoped(
          const RevealScreen(),
          overrides: [
            impostorFlowControllerProvider.overrideWith(
              () => _PreloadedImpostorController(_impostorRevealState(session)),
            ),
            assignRolesCoordinatorProvider.overrideWithValue(
              FakeAssignRolesCoordinator(session: session),
            ),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('VotingScreen — long player names (320x600)', (tester) async {
      final router = GoRouter(
        initialLocation: '/',
        routes: [
          GoRoute(path: '/', builder: (_, _) => const VotingScreen()),
          GoRoute(
            path: '/game-over',
            name: 'impostor-game-over',
            builder: (_, _) => const Scaffold(body: Text('FIN')),
          ),
          GoRoute(
            path: '/menu',
            name: 'menu',
            builder: (_, _) => const Scaffold(),
          ),
        ],
      );
      await tester.pumpWidget(
        ProviderScope(
          overrides: [
            impostorFlowControllerProvider.overrideWith(
              () => _PreloadedImpostorController(_impostorVotingState(session)),
            ),
          ],
          child: MaterialApp.router(
            locale: const Locale('es'),
            localizationsDelegates: AppLocalizations.localizationsDelegates,
            supportedLocales: AppLocalizations.supportedLocales,
            routerConfig: router,
            builder: (context, child) => MediaQuery(
              data: const MediaQueryData(
                size: Size(320, 600),
                textScaler: TextScaler.linear(2.0),
              ),
              child: SizedBox(width: 320, height: 600, child: child!),
            ),
          ),
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('GameOverScreen — jugadores ganan (320x600)', (tester) async {
      await tester.pumpWidget(
        _scaledSmallRouted(
          const GameOverScreen(),
          overrides: [
            impostorFlowControllerProvider.overrideWith(
              () =>
                  _PreloadedImpostorController(_impostorGameOverState(session)),
            ),
            gameHistoryRepositoryProvider.overrideWithValue(
              FakeGameHistoryRepository(),
            ),
            audioServiceProvider.overrideWithValue(_NoopAudio()),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('WordsManagementScreen — empty state (320x600)', (
      tester,
    ) async {
      // WordsManagementScreen calls wordRepositoryProvider which needs sqflite.
      // We use sqflite-ffi (initialized in setUpAll) so this is safe.
      await tester.pumpWidget(
        _scaledSmallScoped(const WordsManagementScreen()),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      // WordsManagementScreen loads words via FutureBuilder — allow async
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('HistoryScreen — empty state (320x600)', (tester) async {
      await tester.pumpWidget(
        _scaledSmallScoped(
          const HistoryScreen(),
          overrides: [
            gameHistoryRepositoryProvider.overrideWithValue(
              FakeGameHistoryRepository(),
            ),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });
  });

  // ══════════════════════════════════════════════════════════════════════════
  // TRIVIA FLOW SCREENS
  // ══════════════════════════════════════════════════════════════════════════

  group('Trivia flow screens — textScaler 2.0 — no overflow', () {
    testWidgets('TriviaPassDeviceScreen — long player name (320x600)', (
      tester,
    ) async {
      final state = _triviaPassState();
      await tester.pumpWidget(
        _scaledSmallRouted(
          const TriviaPassDeviceScreen(),
          overrides: [
            triviaFlowControllerProvider.overrideWith(
              () => _PreloadedTriviaController(state),
            ),
            questionRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeQuestionRepo()),
            ),
            winnerRepositoryProvider.overrideWith(
              (ref) => Future.value(FakeWinnerRepository()),
            ),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets(
      'TriviaQuestionScreen — long enunciado + long answers (320x600)',
      (tester) async {
        final state = _triviaQuestionState();
        await tester.pumpWidget(
          _scaledSmallRouted(
            const TriviaQuestionScreen(),
            overrides: [
              triviaFlowControllerProvider.overrideWith(
                () => _PreloadedTriviaController(state),
              ),
              questionRepositoryProvider.overrideWith(
                (ref) => Future.value(buildFakeQuestionRepo()),
              ),
              winnerRepositoryProvider.overrideWith(
                (ref) => Future.value(FakeWinnerRepository()),
              ),
              randomProvider.overrideWithValue(RandomProvider.seeded(0)),
            ],
          ),
        );
        await tester.pump();
        await tester.pump(const Duration(milliseconds: 400));
        expect(tester.takeException(), isNull);
      },
    );

    testWidgets('TriviaGameOverScreen — multiple winners (320x600)', (
      tester,
    ) async {
      final state = _triviaGameOverState();
      await tester.pumpWidget(
        ProviderScope(
          overrides: [
            triviaFlowControllerProvider.overrideWith(
              () => _PreloadedTriviaController(state),
            ),
            winnerRepositoryProvider.overrideWith(
              (ref) => Future.value(FakeWinnerRepository()),
            ),
            questionRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeQuestionRepo()),
            ),
          ],
          child: _scaledSmall(const TriviaGameOverScreen()),
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });
  });

  // ══════════════════════════════════════════════════════════════════════════
  // WAVELENGTH FLOW SCREENS
  // Note: WavelengthClueScreen, WavelengthGuessScreen embed a Flame GameWidget.
  // Those surfaces are SKIPPED (see report). We audit the Flutter-only screens.
  // ══════════════════════════════════════════════════════════════════════════

  group('Wavelength flow screens — textScaler 2.0 — no overflow', () {
    testWidgets('WavelengthPassDeviceScreen — long player name (320x600)', (
      tester,
    ) async {
      final state = _wavelengthPassState();
      await tester.pumpWidget(
        _scaledSmallRouted(
          const WavelengthPassDeviceScreen(),
          overrides: [
            wavelengthFlowControllerProvider.overrideWith(
              () => _PreloadedWavelengthController(state),
            ),
            spectrumRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeSpectrumRepo()),
            ),
            randomProvider.overrideWithValue(RandomProvider.seeded(0)),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    // The clue/guess screens embed the Flame dial (GameWidget). The dial sits
    // inside an Expanded, so the surrounding Flutter chrome (round indicator,
    // psychic label, instruction panel, concept labels, clue field, button)
    // is what can overflow at textScaler 2.0 — that is exactly what we assert.
    // GameWidget renders an empty surface in widget tests without crashing.
    testWidgets('WavelengthClueScreen — chrome at textScaler 2.0 (320x600)', (
      tester,
    ) async {
      final state = _wavelengthClueState();
      await tester.pumpWidget(
        _scaledSmallRouted(
          const WavelengthClueScreen(),
          overrides: [
            wavelengthFlowControllerProvider.overrideWith(
              () => _PreloadedWavelengthController(state),
            ),
            spectrumRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeSpectrumRepo()),
            ),
            randomProvider.overrideWithValue(RandomProvider.seeded(0)),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('WavelengthGuessScreen — chrome at textScaler 2.0 (320x600)', (
      tester,
    ) async {
      final state = _wavelengthGuessState();
      await tester.pumpWidget(
        _scaledSmallRouted(
          const WavelengthGuessScreen(),
          overrides: [
            wavelengthFlowControllerProvider.overrideWith(
              () => _PreloadedWavelengthController(state),
            ),
            spectrumRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeSpectrumRepo()),
            ),
            randomProvider.overrideWithValue(RandomProvider.seeded(0)),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('WavelengthRevealScreen — long concepts (320x600)', (
      tester,
    ) async {
      final state = _wavelengthRevealState();
      await tester.pumpWidget(
        _scaledSmallRouted(
          const WavelengthRevealScreen(),
          overrides: [
            wavelengthFlowControllerProvider.overrideWith(
              () => _PreloadedWavelengthController(state),
            ),
            spectrumRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeSpectrumRepo()),
            ),
            randomProvider.overrideWithValue(RandomProvider.seeded(0)),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('WavelengthGameOverScreen — final score (320x600)', (
      tester,
    ) async {
      final state = _wavelengthGameOverState();
      await tester.pumpWidget(
        _scaledSmallRouted(
          const WavelengthGameOverScreen(),
          overrides: [
            wavelengthFlowControllerProvider.overrideWith(
              () => _PreloadedWavelengthController(state),
            ),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });
  });

  // ══════════════════════════════════════════════════════════════════════════
  // TABU FLOW SCREENS
  // Note: TabuTurnScreen has an active Timer. We use _scaledSmallRouted and
  // pump briefly — the timer is started post-frame so pump(400ms) is safe.
  // ══════════════════════════════════════════════════════════════════════════

  group('Tabu flow screens — textScaler 2.0 — no overflow', () {
    testWidgets('TabuTurnScreen — long team + word + prohibidas (320x600)', (
      tester,
    ) async {
      final state = _tabuTurnState();
      await tester.pumpWidget(
        _scaledSmallRouted(
          const TabuTurnScreen(),
          overrides: [
            tabuFlowControllerProvider.overrideWith(
              () => _PreloadedTabuController(state),
            ),
            tabuWordRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeTabuRepo()),
            ),
            randomProvider.overrideWithValue(RandomProvider.seeded(0)),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('TabuScoreboardScreen — long team names + scores (320x600)', (
      tester,
    ) async {
      final state = _tabuScoreboardState();
      await tester.pumpWidget(
        _scaledSmallRouted(
          const TabuScoreboardScreen(),
          overrides: [
            tabuFlowControllerProvider.overrideWith(
              () => _PreloadedTabuController(state),
            ),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });

    testWidgets('TabuGameOverScreen — winner team long name (320x600)', (
      tester,
    ) async {
      final state = _tabuGameOverState();
      await tester.pumpWidget(
        _scaledSmallRouted(
          const TabuGameOverScreen(),
          overrides: [
            tabuFlowControllerProvider.overrideWith(
              () => _PreloadedTabuController(state),
            ),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });
  });

  // ══════════════════════════════════════════════════════════════════════════
  // YO NUNCA FLOW SCREEN
  // ══════════════════════════════════════════════════════════════════════════

  group('Yo Nunca flow screens — textScaler 2.0 — no overflow', () {
    testWidgets('YoNuncaPlayScreen — long statement (320x600)', (tester) async {
      final state = _yoNuncaJugandoState();
      await tester.pumpWidget(
        _scaledSmallRouted(
          const YoNuncaPlayScreen(),
          overrides: [
            yoNuncaFlowControllerProvider.overrideWith(
              () => _PreloadedYoNuncaController(state),
            ),
            neverStatementRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeRepo()),
            ),
          ],
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });
  });

  // ══════════════════════════════════════════════════════════════════════════
  // BOMBA FLOW SCREENS
  // Note: BombaPlayScreen has an active fuse Timer. Same approach as Tabu.
  // ══════════════════════════════════════════════════════════════════════════

  group('Bomba flow screens — textScaler 2.0 — no overflow', () {
    // BombaPlayScreen — SKIPPED: embeds Flame GameWidget and active fuse Timer
    // (see report for full rationale).

    testWidgets('BombaGameOverScreen — long winner name (320x600)', (
      tester,
    ) async {
      await tester.pumpWidget(
        ProviderScope(
          overrides: [
            randomProvider.overrideWithValue(RandomProvider.seeded(0)),
            bombaFlowControllerProvider.overrideWith(
              () => _BombaGameOverController(
                'NombreDeJugadorExtremadamenteLargoQuePodriaCortarse',
              ),
            ),
          ],
          child: _scaledSmall(const BombaGameOverScreen()),
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });
  });

  // ══════════════════════════════════════════════════════════════════════════
  // SHARED SCREENS
  // ══════════════════════════════════════════════════════════════════════════

  group('Shared screens — textScaler 2.0 — no overflow', () {
    testWidgets('RulesScreen — long steps list (320x600)', (tester) async {
      const steps = [
        'Paso uno: una descripción bastante larga que podría truncarse en pantallas con textScaler grande',
        'Paso dos: otra instrucción larga con muchas palabras para probar wrapping',
        'Paso tres: tercer paso con contenido extenso',
        'Paso cuatro: cuarto paso con instrucciones detalladas y texto largo',
        'Paso cinco: quinto y último paso con descripción extensa',
      ];
      await tester.pumpWidget(
        _scaledSmall(
          const RulesScreen(
            gameTitle: 'Nombre del Juego Bastante Largo',
            steps: steps,
          ),
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));
      expect(tester.takeException(), isNull);
    });
  });

  // ══════════════════════════════════════════════════════════════════════════
  // ES UN 10 PERO
  // Note: EsUn10PeroScreen embeds a Flame CardFlipGame. SKIPPED — see report.
  // ══════════════════════════════════════════════════════════════════════════

  // ══════════════════════════════════════════════════════════════════════════
  // MENU SCREEN
  // ══════════════════════════════════════════════════════════════════════════

  group('MenuScreen — textScaler 2.0 — no overflow', () {
    testWidgets('all game cards visible without overflow (320x600)', (
      tester,
    ) async {
      const games = <GameDescriptor>[
        _FakeGame(
          'g1',
          'Juego Uno Nombre Bastante Largo',
          'Descripción uno muy extensa',
        ),
        _FakeGame(
          'g2',
          'Juego Dos',
          'Descripción dos bastante extensa que podría desbordarse',
        ),
        _FakeGame('g3', 'Juego Tres', 'Descripción tres'),
        _FakeGame('g4', 'Juego Cuatro', 'Descripción cuatro'),
        _FakeGame('g5', 'Juego Cinco', 'Descripción cinco'),
        _FakeGame('g6', 'Juego Seis', 'Descripción seis'),
      ];
      await tester.pumpWidget(
        ProviderScope(
          overrides: [gameRegistryProvider.overrideWithValue(games)],
          child: _scaledSmall(const MenuScreen()),
        ),
      );
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 300));
      expect(tester.takeException(), isNull);
    });
  });
}
