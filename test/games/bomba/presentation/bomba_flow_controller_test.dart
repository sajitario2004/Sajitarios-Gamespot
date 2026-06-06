/// Unit tests for [BombaFlowController].
///
/// Everything is driven through the controller directly — no widget mounting,
/// no real Timer. The fake repo is the existing FFI-backed helper so we do
/// not need a new in-memory fake. randomProvider is seeded for determinism.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_flow_controller.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_repositories_provider.dart';

import 'support/fake_bomba_prompt_repository.dart';

// ─── Helpers ─────────────────────────────────────────────────────────────────

BombaConfig _config(
  List<String> players, {
  BombaMode mode = BombaMode.silaba,
  int minSegundos = 10,
  int maxSegundos = 11,
}) {
  final result = BombaConfig.create(
    mode: mode,
    playerNames: players,
    minSegundos: minSegundos,
    maxSegundos: maxSegundos,
  );
  assert(result.isSuccess, 'BombaConfig inválida: ${result.error}');
  return result.config!;
}

/// Creates a [ProviderContainer] with the given repo and a seeded RNG.
///
/// [repoFuture] is an already-resolved future so the override never touches
/// the real database provider.
Future<ProviderContainer> _container({
  required Future<dynamic> repoFuture,
  int seed = 0,
}) async {
  final repo = await repoFuture;
  final container = ProviderContainer(
    overrides: [
      bombaPromptRepositoryProvider.overrideWith((_) => Future.value(repo)),
      randomProvider.overrideWithValue(RandomProvider.seeded(seed)),
    ],
  );
  addTearDown(container.dispose);
  return container;
}

BombaFlowState _state(ProviderContainer c) =>
    c.read(bombaFlowControllerProvider);

BombaFlowController _notifier(ProviderContainer c) =>
    c.read(bombaFlowControllerProvider.notifier);

// ─── Tests ────────────────────────────────────────────────────────────────────

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('BombaFlowController · iniciar()', () {
    test('happy path → jugando con prompt y primer portador', () async {
      final container = await _container(repoFuture: makeFakeRepo());
      final config = _config(['Ana', 'Luis', 'Marta']);

      await _notifier(container).iniciar(config);

      final state = _state(container);
      expect(state.fase, BombaFase.jugando);
      expect(state.currentPrompt, isNotNull);
      expect(state.currentHolder, isNotNull);
      expect(state.alivePlayers, hasLength(3));
    });

    test('pool vacío para el modo elegido → error kind sinPrompts', () async {
      // makeEmptyRepo() deja vacío el modo silaba por defecto.
      final container = await _container(
        repoFuture: makeEmptyRepo(emptyMode: BombaMode.silaba),
      );
      final config = _config(['Ana', 'Luis'], mode: BombaMode.silaba);

      await _notifier(container).iniciar(config);

      final state = _state(container);
      expect(state.fase, BombaFase.error);
      expect(state.errorKind, BombaErrorKind.sinPrompts);
      expect(state.session, isNull);
    });
  });

  group('BombaFlowController · pasar()', () {
    test('rota el portador al siguiente jugador vivo', () async {
      final container = await _container(repoFuture: makeFakeRepo());
      final config = _config(['Ana', 'Luis', 'Marta']);
      await _notifier(container).iniciar(config);

      final holderAntes = _state(container).currentHolder!;
      _notifier(container).pasar();
      final holderDespues = _state(container).currentHolder!;

      // El portador debe haber cambiado (rotación circular).
      expect(holderDespues, isNot(equals(holderAntes)));
    });

    test('pasar() no hace nada fuera de fase jugando', () async {
      final container = await _container(repoFuture: makeFakeRepo());
      // Still in setup.
      _notifier(container).pasar();
      expect(_state(container).fase, BombaFase.setup);
    });
  });

  group('BombaFlowController · explotar()', () {
    test('elimina al portador y transiciona a fase explotando', () async {
      final container = await _container(repoFuture: makeFakeRepo());
      final config = _config(['Ana', 'Luis', 'Marta']);
      await _notifier(container).iniciar(config);

      final eliminado = _state(container).currentHolder!;
      _notifier(container).explotar();

      final state = _state(container);
      expect(state.fase, BombaFase.explotando);
      expect(state.eliminado, eliminado);
      // El jugador eliminado ya no está en la lista de vivos.
      expect(state.alivePlayers, isNot(contains(eliminado)));
      expect(state.alivePlayers, hasLength(2));
    });

    test('explotar() no hace nada fuera de fase jugando', () async {
      final container = await _container(repoFuture: makeFakeRepo());
      _notifier(container).explotar();
      expect(_state(container).fase, BombaFase.setup);
    });
  });

  group('BombaFlowController · continuarTrasExplosion()', () {
    test(
      'con >1 jugador vivo → jugando (nueva ronda con nuevo prompt)',
      () async {
        final container = await _container(repoFuture: makeFakeRepo());
        await _notifier(container).iniciar(_config(['Ana', 'Luis', 'Marta']));

        _notifier(container).explotar(); // elimina al portador, quedan 2
        expect(_state(container).fase, BombaFase.explotando);

        _notifier(container).continuarTrasExplosion();

        final state = _state(container);
        expect(state.fase, BombaFase.jugando);
        expect(state.currentPrompt, isNotNull);
        expect(state.eliminado, isNull);
        expect(state.alivePlayers, hasLength(2));
      },
    );

    test(
      'con exactamente 1 jugador vivo → gameOver con el último en pie',
      () async {
        final container = await _container(repoFuture: makeFakeRepo());
        await _notifier(container).iniciar(_config(['Ana', 'Luis']));

        _notifier(container).explotar(); // elimina a uno, queda 1
        expect(_state(container).alivePlayers, hasLength(1));

        _notifier(container).continuarTrasExplosion();

        final state = _state(container);
        expect(state.fase, BombaFase.gameOver);
        expect(state.winner, isNotNull);
        expect(state.alivePlayers, hasLength(1));
      },
    );

    test(
      'continuarTrasExplosion() no hace nada fuera de fase explotando',
      () async {
        final container = await _container(repoFuture: makeFakeRepo());
        _notifier(container).continuarTrasExplosion();
        expect(_state(container).fase, BombaFase.setup);
      },
    );
  });

  group('BombaFlowController · reiniciar()', () {
    test('vuelve al estado inicial (setup)', () async {
      final container = await _container(repoFuture: makeFakeRepo());
      await _notifier(container).iniciar(_config(['Ana', 'Luis']));
      expect(_state(container).fase, BombaFase.jugando);

      _notifier(container).reiniciar();
      final state = _state(container);
      expect(state.fase, BombaFase.setup);
      expect(state.session, isNull);
      expect(state.config, isNull);
      expect(state.currentPrompt, isNull);
      expect(state.pool, isEmpty);
    });
  });
}
