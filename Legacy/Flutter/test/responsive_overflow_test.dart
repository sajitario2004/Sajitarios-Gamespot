/// Regression tests: pantallas clave no desbordan con textScaler 2.0 en
/// superficie pequeña (320x600). Cubre los casos de mayor riesgo:
/// - RevealScreen (impostor con pista larga + palabra larga)
/// - TriviaGameOverScreen (lista de ganadores larga)
/// - BombaGameOverScreen (nombre de ganador largo)
/// - MenuScreen (todas las tarjetas de juego)
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/_shared/game_descriptor.dart';
import 'package:sajitarios_gamespot/games/_shared/game_registry.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_flow_controller.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/role.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/word.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/reveal_screen.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_flow_controller.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_repositories_provider.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';
import 'package:sajitarios_gamespot/menu/menu_screen.dart';

import 'games/impostor/presentation/support/fake_assign_roles_coordinator.dart';
import 'games/trivia/presentation/support/fake_question_repository.dart';
import 'games/trivia/presentation/support/fake_winner_repository.dart';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/// Envuelve [child] en una superficie pequeña (320×600) con textScaler 2.0.
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

// ---------------------------------------------------------------------------
// Fake game descriptor para el menú
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
// Fake Trivia controller preloaded
// ---------------------------------------------------------------------------

TriviaFlowState _gameOverStateWith(List<String> survivors) {
  final all = [
    'Ana',
    'Luis',
    'Marta',
    'Pedro',
    'Sofía',
  ].map(TriviaPlayer.new).toList();
  final survSet = survivors.map(TriviaPlayer.new).toSet();
  var session = TriviaSession.start(all);
  for (final p in all) {
    if (!survSet.contains(p)) {
      session = session.recordAnswer(p, correct: false);
    }
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
// Fake Bomba controller preloaded to gameOver
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

  // ── RevealScreen ─────────────────────────────────────────────────────────

  group('RevealScreen textScaler 2.0 — no overflow', () {
    Future<ProviderContainer> containerWithSession(GameSession session) async {
      final container = ProviderContainer(
        overrides: [
          assignRolesCoordinatorProvider.overrideWithValue(
            FakeAssignRolesCoordinator(session: session),
          ),
        ],
      );
      final config = GameConfig.create(
        players: session.players,
        nImpostores: 1,
        hintEnabled: true,
      ).config!;
      final notifier = container.read(impostorFlowControllerProvider.notifier);
      await notifier.iniciar(config);
      notifier.revelar();
      return container;
    }

    testWidgets(
      'impostor con palabra y pista muy largas no desborda (320x600)',
      (tester) async {
        final session = GameSession(
          word: Word(
            text: 'Palabra muy larga que podría desbordarse en pantalla',
            hint: 'Pista también bastante larga con muchas palabras seguidas',
          ),
          players: [
            Player('NombreLargoQueDesbordaPantallaPequena'),
            Player('B'),
            Player('C'),
          ],
          assignments: {
            Player('NombreLargoQueDesbordaPantallaPequena'): Role.impostor,
            Player('B'): Role.palabra,
            Player('C'): Role.palabra,
          },
        );
        final container = await containerWithSession(session);
        addTearDown(container.dispose);

        await tester.pumpWidget(
          UncontrolledProviderScope(
            container: container,
            child: _scaledSmall(const RevealScreen()),
          ),
        );
        // PulseGlow: no usar pumpAndSettle (animación infinita).
        await tester.pump();
        await tester.pump(const Duration(milliseconds: 400));

        expect(tester.takeException(), isNull);
      },
    );
  });

  // ── TriviaGameOverScreen ─────────────────────────────────────────────────

  group('TriviaGameOverScreen textScaler 2.0 — no overflow', () {
    testWidgets('lista de ganadores larga no desborda (320x600)', (
      tester,
    ) async {
      final state = _gameOverStateWith(['Ana', 'Marta', 'Pedro']);

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

      expect(tester.takeException(), isNull);
    });
  });

  // ── BombaGameOverScreen ──────────────────────────────────────────────────

  group('BombaGameOverScreen textScaler 2.0 — no overflow', () {
    testWidgets('nombre de ganador muy largo no desborda (textScaler 2.0)', (
      tester,
    ) async {
      // La pantalla de fin de Bomba solo lee el flow controller (estado fijo
      // de gameOver). No necesita el repositorio: evitamos llamar a sqflite-ffi
      // dentro de testWidgets (provoca "guarded function conflict").
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

      expect(tester.takeException(), isNull);
    });
  });

  // ── MenuScreen ───────────────────────────────────────────────────────────

  group('MenuScreen textScaler 2.0 — no overflow', () {
    testWidgets(
      'todas las tarjetas de juego visibles sin desbordamiento (320x600)',
      (tester) async {
        const games = <GameDescriptor>[
          _FakeGame('g1', 'Juego Uno', 'Descripción uno'),
          _FakeGame(
            'g2',
            'Juego Dos Nombre Largo',
            'Descripción dos bastante extensa que podría desbordarse',
          ),
          _FakeGame('g3', 'Juego Tres', 'Descripción tres'),
          _FakeGame('g4', 'Juego Cuatro', 'Descripción cuatro'),
        ];

        await tester.pumpWidget(
          ProviderScope(
            overrides: [gameRegistryProvider.overrideWithValue(games)],
            child: _scaledSmall(const MenuScreen()),
          ),
        );
        // PulseGlow del título: no pumpAndSettle.
        await tester.pump();
        await tester.pump(const Duration(milliseconds: 300));

        expect(tester.takeException(), isNull);
        expect(find.text('Juego Uno'), findsOneWidget);
      },
    );
  });
}
