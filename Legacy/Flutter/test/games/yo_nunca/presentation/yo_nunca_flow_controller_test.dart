/// Unit tests for [YoNuncaFlowController].
///
/// All repositories are replaced with in-memory fakes; randomProvider is
/// seeded for determinism. No database or Flutter widgets needed.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_flow_controller.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_repositories_provider.dart';

import 'support/fake_never_statement_repository.dart';

// ─── Helpers ─────────────────────────────────────────────────────────────────

/// Builds a [YoNuncaConfig] selecting only [Intensidad.suave].
YoNuncaConfig _configSuave() {
  final result = YoNuncaConfig.create(intensidades: {Intensidad.suave});
  assert(result.isSuccess, 'YoNuncaConfig inválida: ${result.error}');
  return result.config!;
}

/// Creates a [ProviderContainer] with the given repo and a seeded RNG.
ProviderContainer _container({
  FakeNeverStatementRepository? repo,
  int seed = 42,
}) {
  final r = repo ?? buildFakeRepo(count: 10);
  final container = ProviderContainer(
    overrides: [
      neverStatementRepositoryProvider.overrideWith((_) => Future.value(r)),
      randomProvider.overrideWithValue(RandomProvider.seeded(seed)),
    ],
  );
  addTearDown(container.dispose);
  return container;
}

YoNuncaFlowState _state(ProviderContainer c) =>
    c.read(yoNuncaFlowControllerProvider);

YoNuncaFlowController _notifier(ProviderContainer c) =>
    c.read(yoNuncaFlowControllerProvider.notifier);

// ─── Tests ────────────────────────────────────────────────────────────────────

void main() {
  group('YoNuncaFlowController · estado inicial', () {
    test('fase setup, sin frase ni config', () {
      final container = _container();
      final state = _state(container);

      expect(state.fase, YoNuncaFase.setup);
      expect(state.fraseActual, isNull);
      expect(state.config, isNull);
    });
  });

  group('YoNuncaFlowController · iniciar()', () {
    test('con pool válido → jugando con una frase', () async {
      final container = _container();
      await _notifier(container).iniciar(_configSuave());

      final state = _state(container);
      expect(state.fase, YoNuncaFase.jugando);
      expect(state.fraseActual, isNotNull);
      expect(state.pool, isNotEmpty);
      expect(state.seen, contains(state.fraseActual!.id));
    });

    test(
      'pool vacío para las intensidades elegidas → error kind sinFrases',
      () async {
        // Empty repo has no statements at all.
        final container = _container(repo: buildEmptyRepo());
        await _notifier(container).iniciar(_configSuave());

        final state = _state(container);
        expect(state.fase, YoNuncaFase.error);
        expect(state.errorKind, YoNuncaErrorKind.sinFrases);
      },
    );

    test(
      'pool con intensidad diferente a la seleccionada → error sinFrases',
      () async {
        // Repo only has picante statements, but config selects suave.
        final purePickante = FakeNeverStatementRepository([
          NeverStatement.create(
            id: 1,
            frase: 'Yo nunca he hecho algo picante',
            intensidad: Intensidad.picante,
          ),
        ]);
        final container = _container(repo: purePickante);
        await _notifier(container).iniciar(_configSuave());

        final state = _state(container);
        expect(state.fase, YoNuncaFase.error);
        expect(state.errorKind, YoNuncaErrorKind.sinFrases);
      },
    );
  });

  group('YoNuncaFlowController · siguiente()', () {
    test(
      'avanza a una frase distinta cuando hay más de una en el pool',
      () async {
        final container = _container(repo: buildFakeRepo(count: 10));
        await _notifier(container).iniciar(_configSuave());

        final fraseAntes = _state(container).fraseActual!;
        _notifier(container).siguiente();
        final fraseDespues = _state(container).fraseActual!;

        // Con un pool de 10 y seed fija, la segunda frase debería ser distinta.
        // (El use case garantiza no-repeat hasta agotar el pool.)
        expect(fraseDespues.id, isNot(equals(fraseAntes.id)));
      },
    );

    test('seen acumula las frases vistas', () async {
      final container = _container(repo: buildFakeRepo(count: 10));
      await _notifier(container).iniciar(_configSuave());

      // Después de iniciar, seen tiene 1 entrada.
      expect(_state(container).seen.length, 1);

      _notifier(container).siguiente();
      expect(_state(container).seen.length, 2);

      _notifier(container).siguiente();
      expect(_state(container).seen.length, 3);
    });

    test(
      'al agotar el pool seen se reinicia y se sigue devolviendo frases',
      () async {
        // Pool de solo 2 frases.
        final container = _container(repo: buildFakeRepo(count: 2));
        await _notifier(container).iniciar(_configSuave());

        // Después de iniciar (1 vista) + siguiente (2 vistas) → pool agotado.
        _notifier(container).siguiente();

        // El use case reinicia seen internamente; el estado debería tener
        // seen.length == 1 (la nueva frase recién sacada tras el rebaraje).
        _notifier(container).siguiente();

        final state = _state(container);
        expect(state.fase, YoNuncaFase.jugando);
        expect(state.fraseActual, isNotNull);
        // seen no puede tener más entradas que el tamaño del pool filtrado.
        expect(state.seen.length, lessThanOrEqualTo(2));
      },
    );

    test('siguiente() no hace nada fuera de fase jugando', () async {
      final container = _container();
      // Still in setup.
      _notifier(container).siguiente();
      expect(_state(container).fase, YoNuncaFase.setup);
    });
  });

  group('YoNuncaFlowController · reiniciar()', () {
    test('vuelve al estado inicial (setup)', () async {
      final container = _container();
      await _notifier(container).iniciar(_configSuave());
      expect(_state(container).fase, YoNuncaFase.jugando);

      _notifier(container).reiniciar();
      final state = _state(container);
      expect(state.fase, YoNuncaFase.setup);
      expect(state.fraseActual, isNull);
      expect(state.config, isNull);
      expect(state.pool, isEmpty);
      expect(state.seen, isEmpty);
    });
  });
}
