import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/role.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/word.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';

/// Coordinador falso: devuelve una sesión fija, o lanza una excepción concreta,
/// para ejercitar el flujo del controlador sin BD ni aleatoriedad real.
class _FakeCoordinator implements AssignRolesCoordinator {
  _FakeCoordinator({this.session, this.error});

  final GameSession? session;
  final Object? error;

  @override
  Future<GameSession> assign(GameConfig config) async {
    if (error != null) throw error!;
    if (session != null) return session!;
    final players = config.players;
    final assignments = <Player, Role>{
      for (var i = 0; i < players.length; i++)
        players[i]: i < config.nImpostores ? Role.impostor : Role.palabra,
    };
    return GameSession(
      word: Word(text: 'playa', hint: 'verano'),
      players: players,
      assignments: assignments,
    );
  }
}

GameConfig _config({int nImpostores = 1, bool hintEnabled = false}) {
  final result = GameConfig.create(
    players: const [Player('Nacho'), Player('Iker'), Player('Lucia')],
    nImpostores: nImpostores,
    hintEnabled: hintEnabled,
  );
  return result.config!;
}

/// Crea un [ProviderContainer] con el coordinador y random sobreescritos.
ProviderContainer _container(AssignRolesCoordinator coordinator) {
  final container = ProviderContainer(
    overrides: [
      assignRolesCoordinatorProvider.overrideWithValue(coordinator),
      randomProvider.overrideWithValue(RandomProvider.seeded(1)),
    ],
  );
  addTearDown(container.dispose);
  return container;
}

void main() {
  group('ImpostorFlowController', () {
    test('estado inicial es setup sin sesión', () {
      final container = _container(_FakeCoordinator());
      final state = container.read(impostorFlowControllerProvider);

      expect(state.phase, ImpostorPhase.setup);
      expect(state.session, isNull);
      expect(state.config, isNull);
      expect(state.currentIndex, 0);
      expect(state.totalPlayers, 0);
      expect(
        container.read(impostorFlowControllerProvider.notifier).jugadorActual,
        isNull,
      );
    });

    test(
      'iniciar() ok deja el flujo en pass apuntando al primer jugador',
      () async {
        final container = _container(_FakeCoordinator());
        final notifier = container.read(
          impostorFlowControllerProvider.notifier,
        );

        final config = _config();
        await notifier.iniciar(config);

        final state = container.read(impostorFlowControllerProvider);
        expect(state.phase, ImpostorPhase.pass);
        expect(state.config, config);
        expect(state.session, isNotNull);
        expect(state.currentIndex, 0);
        expect(state.totalPlayers, 3);
        expect(notifier.jugadorActual, const Player('Nacho'));
        expect(state.esUltimoJugador, isFalse);
      },
    );

    test(
      'iniciar() sin palabras (NoWordsAvailableException) -> fase error',
      () async {
        final container = _container(
          _FakeCoordinator(error: const NoWordsAvailableException()),
        );
        final notifier = container.read(
          impostorFlowControllerProvider.notifier,
        );

        await notifier.iniciar(_config());

        final state = container.read(impostorFlowControllerProvider);
        expect(state.phase, ImpostorPhase.error);
        expect(state.errorKind, ImpostorErrorKind.sinPalabras);
        expect(state.errorMessage, isNotNull);
        expect(state.session, isNull);
      },
    );

    test('iniciar() con error inesperado -> fase error desconocido', () async {
      final container = _container(_FakeCoordinator(error: StateError('boom')));
      final notifier = container.read(impostorFlowControllerProvider.notifier);

      await notifier.iniciar(_config());

      final state = container.read(impostorFlowControllerProvider);
      expect(state.phase, ImpostorPhase.error);
      expect(state.errorKind, ImpostorErrorKind.desconocido);
      expect(state.errorMessage, isNotNull);
    });

    test('secuencia revelar() -> avanzar() recorre hasta results', () async {
      final container = _container(_FakeCoordinator());
      final notifier = container.read(impostorFlowControllerProvider.notifier);

      await notifier.iniciar(_config());

      // Jugador 1 (Nacho): pass -> reveal -> avanzar (quedan jugadores).
      expect(notifier.jugadorActual, const Player('Nacho'));
      notifier.revelar();
      expect(
        container.read(impostorFlowControllerProvider).phase,
        ImpostorPhase.reveal,
      );
      var terminado = notifier.avanzar();
      expect(terminado, isFalse);
      expect(
        container.read(impostorFlowControllerProvider).phase,
        ImpostorPhase.pass,
      );
      expect(container.read(impostorFlowControllerProvider).currentIndex, 1);
      expect(notifier.jugadorActual, const Player('Iker'));

      // Jugador 2 (Iker).
      notifier.revelar();
      terminado = notifier.avanzar();
      expect(terminado, isFalse);
      expect(notifier.jugadorActual, const Player('Lucia'));
      expect(
        container.read(impostorFlowControllerProvider).esUltimoJugador,
        isTrue,
      );

      // Jugador 3 (Lucia): último -> results.
      notifier.revelar();
      terminado = notifier.avanzar();
      expect(terminado, isTrue);
      expect(
        container.read(impostorFlowControllerProvider).phase,
        ImpostorPhase.results,
      );
    });

    test('reiniciar() vuelve al estado inicial', () async {
      final container = _container(_FakeCoordinator());
      final notifier = container.read(impostorFlowControllerProvider.notifier);

      await notifier.iniciar(_config());
      notifier.revelar();
      notifier.avanzar();
      expect(container.read(impostorFlowControllerProvider).currentIndex, 1);

      notifier.reiniciar();
      final state = container.read(impostorFlowControllerProvider);
      expect(state.phase, ImpostorPhase.setup);
      expect(state.session, isNull);
      expect(state.currentIndex, 0);
      expect(notifier.jugadorActual, isNull);
    });

    test('revelar() y avanzar() sin sesión no hacen nada', () {
      final container = _container(_FakeCoordinator());
      final notifier = container.read(impostorFlowControllerProvider.notifier);

      // Sin iniciar: no hay sesión.
      notifier.revelar();
      expect(
        container.read(impostorFlowControllerProvider).phase,
        ImpostorPhase.setup,
      );

      final terminado = notifier.avanzar();
      expect(terminado, isFalse);
      expect(
        container.read(impostorFlowControllerProvider).phase,
        ImpostorPhase.setup,
      );
    });

    test(
      'jugadorActual devuelve null fuera de los límites de revealOrder',
      () async {
        // Sesión de un solo jugador para forzar el límite con un índice manual.
        final session = GameSession(
          word: Word(text: 'playa', hint: 'verano'),
          players: const [Player('Solo')],
          assignments: {const Player('Solo'): Role.impostor},
        );
        final container = _container(_FakeCoordinator(session: session));
        final notifier = container.read(
          impostorFlowControllerProvider.notifier,
        );

        final config = GameConfig.create(
          players: const [Player('A'), Player('B'), Player('C')],
          nImpostores: 1,
        ).config!;
        await notifier.iniciar(config);

        // currentIndex 0 es válido (1 jugador).
        expect(notifier.jugadorActual, const Player('Solo'));
        expect(
          container.read(impostorFlowControllerProvider).esUltimoJugador,
          isTrue,
        );

        // Avanzar siendo el último lleva a results; jugadorActual sigue siendo
        // el índice 0 (no se incrementa al terminar).
        final terminado = notifier.avanzar();
        expect(terminado, isTrue);
        expect(
          container.read(impostorFlowControllerProvider).phase,
          ImpostorPhase.results,
        );
      },
    );
  });
}
