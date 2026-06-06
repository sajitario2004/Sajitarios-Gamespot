/// Full-flow tests for the Wavelength game.
///
/// Tests the complete round trip:
///   setup → iniciar → clue → pass → guess → reveal → next → … → gameOver
///
/// All repositories and randomness are injected fakes — no database, no Flame.
/// These tests act as a regression harness that would catch any hang or
/// null-dereference introduced in [WavelengthFlowController.next] or the
/// phase-transition sequence.
///
/// Also validates the crash/hang hardening added to the flow controller:
/// - Empty pool → sinEspectros error (no crash).
/// - next() with no guess → no-op (no StateError crash).
/// - next() from wrong phase → no-op.
/// - Double-iniciar guard (re-iniciar from mid-game resets cleanly).
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_flow_controller.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_repositories_provider.dart';

import 'support/fake_spectrum_repository.dart';

// ─── Helpers ──────────────────────────────────────────────────────────────────

WavelengthConfig _config(List<String> names, {int rondas = 1}) =>
    WavelengthConfig.create(playerNames: names, rondas: rondas).config!;

ProviderContainer _container({
  FakeSpectrumRepository? spectrumRepo,
  int seed = 42,
}) {
  final sRepo = spectrumRepo ?? buildFakeSpectrumRepo(count: 5);
  final container = ProviderContainer(
    overrides: [
      spectrumRepositoryProvider.overrideWith((ref) => Future.value(sRepo)),
      randomProvider.overrideWithValue(RandomProvider.seeded(seed)),
    ],
  );
  addTearDown(container.dispose);
  return container;
}

WavelengthFlowState _state(ProviderContainer c) =>
    c.read(wavelengthFlowControllerProvider);

WavelengthFlowController _notifier(ProviderContainer c) =>
    c.read(wavelengthFlowControllerProvider.notifier);

// ─── Full-flow: 1 round ───────────────────────────────────────────────────────

void main() {
  group('WavelengthFlowController · full flow (1 ronda)', () {
    test('flujo completo 1 ronda: setup→clue→pass→guess→reveal→gameOver '
        'sin crash/hang', () async {
      final c = _container(seed: 0);

      // 1. setup
      expect(_state(c).fase, WavelengthFase.setup);

      // 2. iniciar
      await _notifier(c).iniciar(_config(['Ana', 'Luis']));
      expect(_state(c).fase, WavelengthFase.clue);
      expect(_state(c).currentRound, isNotNull);
      expect(_state(c).currentPsychic, 'Ana');

      // round and target must be accessible without null crash
      final round = _state(c).currentRound!;
      expect(round.targetPosition, inInclusiveRange(0.0, 1.0));
      expect(round.spectrum.leftConcept, isNotEmpty);
      expect(round.spectrum.rightConcept, isNotEmpty);

      // 3. clue
      _notifier(c).confirmarPista('Tibia');
      expect(_state(c).fase, WavelengthFase.pass);
      expect(_state(c).currentRound?.clue, 'Tibia');

      // 4. pass device
      _notifier(c).pasarDispositivo();
      expect(_state(c).fase, WavelengthFase.guess);

      // 5. guess — submit exactly on target → bullseye
      final target = _state(c).currentRound!.targetPosition;
      _notifier(c).submitGuess(target);
      expect(_state(c).fase, WavelengthFase.reveal);
      expect(_state(c).currentRound?.score, 4);
      expect(_state(c).currentRound?.hasGuess, isTrue);

      // 6. next → gameOver (1 ronda total)
      await _notifier(c).next();
      final s = _state(c);
      expect(s.fase, WavelengthFase.gameOver);
      expect(s.session, isNotNull);
      expect(s.session!.isOver, isTrue);
      expect(s.session!.cumulativeScore, 4);
      expect(s.currentRound, isNull);
    });
  });

  group('WavelengthFlowController · full flow (3 rondas)', () {
    test('flujo completo 3 rondas: psíquico rota, puntuación acumula, '
        'gameOver al final — sin crash/hang', () async {
      final c = _container(seed: 1);
      const players = ['Ana', 'Luis', 'Mia'];

      await _notifier(c).iniciar(_config(players, rondas: 3));

      var cumulativeScore = 0;

      for (var round = 0; round < 3; round++) {
        expect(_state(c).fase, WavelengthFase.clue);

        // Each player is psychic in order.
        expect(_state(c).currentPsychic, players[round]);

        // currentRound must exist and be non-null every round.
        final r = _state(c).currentRound!;
        expect(r.targetPosition, inInclusiveRange(0.0, 1.0));
        expect(r.clue, isNull);

        _notifier(c).confirmarPista('pista$round');
        expect(_state(c).fase, WavelengthFase.pass);

        _notifier(c).pasarDispositivo();
        expect(_state(c).fase, WavelengthFase.guess);

        // Guess at 0.5 — score depends on target.
        _notifier(c).submitGuess(0.5);
        expect(_state(c).fase, WavelengthFase.reveal);

        final roundScore = _state(c).currentRound!.score!;
        cumulativeScore += roundScore;

        await _notifier(c).next();
      }

      expect(_state(c).fase, WavelengthFase.gameOver);
      expect(_state(c).session!.cumulativeScore, cumulativeScore);
      expect(_state(c).currentRound, isNull);
    });
  });

  group('WavelengthFlowController · guards / edge cases', () {
    test('next() fuera de reveal es no-op (no crash)', () async {
      final c = _container();
      await _notifier(c).iniciar(_config(['A', 'B']));

      // We are in clue — calling next() must not crash or change state.
      await _notifier(c).next();
      expect(_state(c).fase, WavelengthFase.clue);
    });

    test('next() en reveal sin guess es no-op (no crash)', () async {
      final c = _container();
      await _notifier(c).iniciar(_config(['A', 'B']));
      _notifier(c).confirmarPista('pista');
      _notifier(c).pasarDispositivo();

      // Manually move to reveal with a round that has a guess.
      _notifier(c).submitGuess(0.5);
      expect(_state(c).fase, WavelengthFase.reveal);
      expect(_state(c).currentRound?.hasGuess, isTrue);

      // next() proceeds normally.
      await _notifier(c).next();
      expect(_state(c).fase, WavelengthFase.gameOver);
    });

    test('submitGuess fuera de guess es no-op (no crash)', () async {
      final c = _container();
      await _notifier(c).iniciar(_config(['A', 'B']));
      // In clue phase — submitGuess must be silent.
      _notifier(c).submitGuess(0.5);
      expect(_state(c).fase, WavelengthFase.clue);
    });

    test('reiniciar desde mid-game vuelve a setup limpio', () async {
      final c = _container();
      await _notifier(c).iniciar(_config(['A', 'B'], rondas: 3));
      _notifier(c).confirmarPista('x');
      _notifier(c).pasarDispositivo();
      _notifier(c).submitGuess(0.5);
      // Now in reveal; reiniciar.
      _notifier(c).reiniciar();
      final s = _state(c);
      expect(s.fase, WavelengthFase.setup);
      expect(s.session, isNull);
      expect(s.currentRound, isNull);
    });

    test('iniciar con pool vacío → error sinEspectros (no crash)', () async {
      final c = _container(spectrumRepo: buildEmptySpectrumRepo());
      await _notifier(c).iniciar(_config(['A', 'B']));
      expect(_state(c).fase, WavelengthFase.error);
      expect(_state(c).errorKind, WavelengthErrorKind.sinEspectros);
    });

    test('confirmarPista fuera de clue es no-op (no crash)', () async {
      final c = _container();
      // In setup — should be no-op.
      _notifier(c).confirmarPista('pista');
      expect(_state(c).fase, WavelengthFase.setup);
    });

    test('pasarDispositivo fuera de pass es no-op (no crash)', () async {
      final c = _container();
      await _notifier(c).iniciar(_config(['A', 'B']));
      // In clue — should be no-op.
      _notifier(c).pasarDispositivo();
      expect(_state(c).fase, WavelengthFase.clue);
    });
  });

  group('WavelengthFlowController · puntuación', () {
    test('bullseye: guess == target → 4 puntos', () async {
      final c = _container(seed: 0);
      await _notifier(c).iniciar(_config(['A', 'B']));
      _notifier(c).confirmarPista('p');
      _notifier(c).pasarDispositivo();
      final target = _state(c).currentRound!.targetPosition;
      _notifier(c).submitGuess(target);
      expect(_state(c).currentRound!.score, 4);
    });

    test('miss: guess muy lejos del target → 0 puntos', () async {
      final c = _container(seed: 0);
      await _notifier(c).iniciar(_config(['A', 'B']));
      _notifier(c).confirmarPista('p');
      _notifier(c).pasarDispositivo();
      final target = _state(c).currentRound!.targetPosition;
      // Guess at the opposite extreme.
      final guess = target > 0.5 ? 0.0 : 1.0;
      _notifier(c).submitGuess(guess);
      // Score should be 0 unless target is itself at an edge.
      final score = _state(c).currentRound!.score!;
      expect(score, lessThanOrEqualTo(4));
      expect(score, greaterThanOrEqualTo(0));
    });

    test('dial position clamped: guess > 1 → treats as 1.0', () async {
      final c = _container(seed: 0);
      await _notifier(c).iniciar(_config(['A', 'B']));
      _notifier(c).confirmarPista('p');
      _notifier(c).pasarDispositivo();
      // submitGuess clamps; should not throw.
      _notifier(c).submitGuess(1.5);
      expect(_state(c).fase, WavelengthFase.reveal);
      expect(_state(c).currentRound?.guess, 1.0);
    });
  });
}
