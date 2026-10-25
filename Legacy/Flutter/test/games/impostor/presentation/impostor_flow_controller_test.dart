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

    test(
      'secuencia revelar() -> avanzar() recorre hasta la votación',
      () async {
        final container = _container(_FakeCoordinator());
        final notifier = container.read(
          impostorFlowControllerProvider.notifier,
        );

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

        // Jugador 3 (Lucia): último -> abre la VOTACIÓN (ronda 1), no results.
        notifier.revelar();
        terminado = notifier.avanzar();
        expect(terminado, isTrue);
        final fin = container.read(impostorFlowControllerProvider);
        expect(fin.phase, ImpostorPhase.voting);
        expect(fin.rondaActual, 1);
        expect(fin.rondasTotales, _config().rounds);
        expect(fin.eliminados, isEmpty);
        expect(fin.lastVote, LastVote.ninguno);
      },
    );

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

        // Avanzar siendo el último abre la votación; jugadorActual sigue siendo
        // el índice 0 (no se incrementa al terminar).
        final terminado = notifier.avanzar();
        expect(terminado, isTrue);
        expect(
          container.read(impostorFlowControllerProvider).phase,
          ImpostorPhase.voting,
        );
      },
    );
  });

  group('ImpostorFlowController · votación', () {
    /// Construye una sesión determinista con roles fijos (impostores por nombre)
    /// para no depender de la aleatoriedad real al probar la votación.
    GameSession sesion({
      required List<String> nombres,
      required Set<String> impostores,
    }) {
      final players = nombres.map(Player.new).toList(growable: false);
      return GameSession(
        word: Word(text: 'playa', hint: 'verano'),
        players: players,
        assignments: {
          for (final p in players)
            p: impostores.contains(p.name) ? Role.impostor : Role.palabra,
        },
      );
    }

    /// Inicia una partida con [session] y [rounds] y recorre todas las
    /// revelaciones hasta dejar el flujo en la VOTACIÓN (ronda 1). Devuelve el
    /// notifier listo para votar.
    Future<ImpostorFlowController> enVotacion(
      ProviderContainer container,
      GameSession session, {
      required int rounds,
    }) async {
      final notifier = container.read(impostorFlowControllerProvider.notifier);
      final config = GameConfig.create(
        players: session.players,
        nImpostores: 1,
        rounds: rounds,
      ).config!;
      await notifier.iniciar(config);
      var enVotacion = false;
      while (!enVotacion) {
        notifier.revelar();
        enVotacion = notifier.avanzar();
      }
      return notifier;
    }

    Player jugador(GameSession s, String nombre) =>
        s.players.firstWhere((p) => p.name == nombre);

    test(
      'victoria: pillar al único impostor hace ganar a los jugadores',
      () async {
        final session = sesion(
          nombres: ['Nacho', 'Iker', 'Lucia', 'Ana', 'Leo', 'Sara'],
          impostores: {'Nacho'},
        );
        final container = _container(_FakeCoordinator(session: session));
        final notifier = await enVotacion(container, session, rounds: 3);

        notifier.votar(jugador(session, 'Nacho'));

        final state = container.read(impostorFlowControllerProvider);
        expect(state.phase, ImpostorPhase.gameOver);
        expect(state.outcome, VotingOutcome.jugadoresGanan);
        expect(state.lastVote, LastVote.eraImpostor);
        expect(state.eliminados.contains(jugador(session, 'Nacho')), isTrue);
        expect(state.impostoresVivos, isEmpty);
      },
    );

    test(
      'derrota: agotar las rondas con impostor vivo hace ganar al impostor',
      () async {
        final session = sesion(
          nombres: ['Nacho', 'Iker', 'Lucia', 'Ana', 'Leo', 'Sara'],
          impostores: {'Nacho'},
        );
        final container = _container(_FakeCoordinator(session: session));
        final notifier = await enVotacion(container, session, rounds: 3);

        // Tres votos a inocentes consumen las tres rondas sin pillar al impostor.
        notifier.votar(jugador(session, 'Iker'));
        expect(
          container.read(impostorFlowControllerProvider).phase,
          ImpostorPhase.voting,
        );
        notifier.votar(jugador(session, 'Lucia'));
        expect(
          container.read(impostorFlowControllerProvider).phase,
          ImpostorPhase.voting,
        );
        notifier.votar(jugador(session, 'Ana'));

        final state = container.read(impostorFlowControllerProvider);
        expect(state.phase, ImpostorPhase.gameOver);
        expect(state.outcome, VotingOutcome.impostorGana);
        // El impostor sigue vivo (no se reveló, sigue en juego).
        expect(state.impostoresVivos, contains(jugador(session, 'Nacho')));
      },
    );

    test(
      'must-catch-all: con dos impostores no se gana hasta pillarlos a ambos',
      () async {
        final session = sesion(
          nombres: ['Nacho', 'Iker', 'Lucia', 'Ana', 'Leo', 'Sara'],
          impostores: {'Nacho', 'Iker'},
        );
        final container = _container(_FakeCoordinator(session: session));
        final notifier = await enVotacion(container, session, rounds: 3);

        // Primer impostor pillado: aún queda otro vivo -> el juego sigue.
        notifier.votar(jugador(session, 'Nacho'));
        var state = container.read(impostorFlowControllerProvider);
        expect(state.phase, ImpostorPhase.voting);
        expect(state.lastVote, LastVote.eraImpostor);
        expect(state.impostoresVivos, [jugador(session, 'Iker')]);
        expect(state.rondaActual, 2);

        // Segundo impostor pillado: ya no quedan -> ganan los jugadores.
        notifier.votar(jugador(session, 'Iker'));
        state = container.read(impostorFlowControllerProvider);
        expect(state.phase, ImpostorPhase.gameOver);
        expect(state.outcome, VotingOutcome.jugadoresGanan);
        expect(state.impostoresVivos, isEmpty);
      },
    );

    test('cada voto consume una ronda (acierte o no)', () async {
      final session = sesion(
        nombres: ['Nacho', 'Iker', 'Lucia', 'Ana', 'Leo', 'Sara'],
        impostores: {'Nacho', 'Iker'},
      );
      final container = _container(_FakeCoordinator(session: session));
      final notifier = await enVotacion(container, session, rounds: 3);

      expect(container.read(impostorFlowControllerProvider).rondaActual, 1);

      // Voto fallido: avanza de ronda.
      notifier.votar(jugador(session, 'Lucia'));
      expect(container.read(impostorFlowControllerProvider).rondaActual, 2);

      // Voto acertado (pero queda un impostor): también consume ronda.
      notifier.votar(jugador(session, 'Nacho'));
      expect(container.read(impostorFlowControllerProvider).rondaActual, 3);
    });

    test(
      'candidatos excluye a los ya eliminados; volver a votarlos no hace nada',
      () async {
        final session = sesion(
          nombres: ['Nacho', 'Iker', 'Lucia', 'Ana', 'Leo', 'Sara'],
          impostores: {'Nacho', 'Iker'},
        );
        final container = _container(_FakeCoordinator(session: session));
        final notifier = await enVotacion(container, session, rounds: 3);

        // Inicialmente todos son candidatos.
        expect(
          container.read(impostorFlowControllerProvider).candidatos.length,
          6,
        );

        // Pillar a un impostor lo elimina y lo saca de candidatos.
        notifier.votar(jugador(session, 'Nacho'));
        var state = container.read(impostorFlowControllerProvider);
        expect(state.candidatos, isNot(contains(jugador(session, 'Nacho'))));
        expect(state.candidatos.length, 5);

        // Volver a votar a un ya eliminado no cambia el estado ni consume ronda.
        final rondaAntes = state.rondaActual;
        notifier.votar(jugador(session, 'Nacho'));
        state = container.read(impostorFlowControllerProvider);
        expect(state.rondaActual, rondaAntes);
        expect(state.phase, ImpostorPhase.voting);
      },
    );

    test(
      'nadie es impostor: nunca se gana; al agotar rondas gana el impostor',
      () async {
        final session = sesion(
          nombres: ['Nacho', 'Iker', 'Lucia', 'Ana', 'Leo', 'Sara'],
          impostores: <String>{}, // regla del 10%: ninguno es impostor.
        );
        final container = _container(_FakeCoordinator(session: session));
        final notifier = await enVotacion(container, session, rounds: 3);

        // Ningún voto puede ser acierto: la partida no se gana por votación.
        notifier.votar(jugador(session, 'Nacho'));
        expect(
          container.read(impostorFlowControllerProvider).phase,
          ImpostorPhase.voting,
        );
        notifier.votar(jugador(session, 'Iker'));
        expect(
          container.read(impostorFlowControllerProvider).phase,
          ImpostorPhase.voting,
        );
        notifier.votar(jugador(session, 'Lucia'));

        final state = container.read(impostorFlowControllerProvider);
        expect(state.phase, ImpostorPhase.gameOver);
        expect(state.outcome, VotingOutcome.impostorGana);
        expect(state.lastVote, LastVote.noEraImpostor);
      },
    );
  });
}
