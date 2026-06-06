/// Unit tests for [WavelengthFlowController].
///
/// All repositories are replaced with in-memory fakes; randomProvider is
/// seeded for determinism. No database or Flutter widgets needed.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_flow_controller.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_repositories_provider.dart';

import 'support/fake_spectrum_repository.dart';

// ─── Helpers ──────────────────────────────────────────────────────────────────

WavelengthConfig _config(List<String> names, {int rondas = 3}) {
  return WavelengthConfig.create(playerNames: names, rondas: rondas).config!;
}

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

// ─── Tests ────────────────────────────────────────────────────────────────────

void main() {
  group('WavelengthFlowController · estado inicial', () {
    test('estado inicial es setup sin sesión', () {
      final c = _container();
      final s = _state(c);
      expect(s.fase, WavelengthFase.setup);
      expect(s.session, isNull);
      expect(s.config, isNull);
      expect(s.currentRound, isNull);
    });
  });

  group('WavelengthFlowController · iniciar', () {
    test('iniciar con espectros disponibles pasa a fase clue', () async {
      final c = _container();
      await _notifier(c).iniciar(_config(['Ana', 'Luis']));
      final s = _state(c);
      expect(s.fase, WavelengthFase.clue);
      expect(s.session, isNotNull);
      expect(s.currentRound, isNotNull);
      expect(s.currentPsychic, 'Ana');
    });

    test('iniciar sin espectros pasa a fase error sinEspectros', () async {
      final c = _container(spectrumRepo: buildEmptySpectrumRepo());
      await _notifier(c).iniciar(_config(['Ana', 'Luis']));
      final s = _state(c);
      expect(s.fase, WavelengthFase.error);
      expect(s.errorKind, WavelengthErrorKind.sinEspectros);
    });

    test('el psíquico de la primera ronda es el primer jugador', () async {
      final c = _container();
      await _notifier(c).iniciar(_config(['Jugador1', 'Jugador2', 'Jugador3']));
      expect(_state(c).currentPsychic, 'Jugador1');
    });
  });

  group('WavelengthFlowController · flujo completo', () {
    test('confirmarPista sin texto no avanza', () async {
      final c = _container();
      await _notifier(c).iniciar(_config(['Ana', 'Luis']));
      _notifier(c).confirmarPista('   ');
      expect(_state(c).fase, WavelengthFase.clue);
    });

    test('confirmarPista con texto avanza a pass', () async {
      final c = _container();
      await _notifier(c).iniciar(_config(['Ana', 'Luis']));
      _notifier(c).confirmarPista('Mi pista');
      expect(_state(c).fase, WavelengthFase.pass);
      expect(_state(c).currentRound?.clue, 'Mi pista');
    });

    test('pasarDispositivo avanza de pass a guess', () async {
      final c = _container();
      await _notifier(c).iniciar(_config(['Ana', 'Luis']));
      _notifier(c).confirmarPista('pista');
      _notifier(c).pasarDispositivo();
      expect(_state(c).fase, WavelengthFase.guess);
    });

    test('submitGuess avanza a reveal y asigna puntuación', () async {
      final c = _container(seed: 0);
      await _notifier(c).iniciar(_config(['Ana', 'Luis']));
      _notifier(c).confirmarPista('algo');
      _notifier(c).pasarDispositivo();
      final target = _state(c).currentRound!.targetPosition;
      // Guess exactamente en el target → bullseye (4 pts).
      _notifier(c).submitGuess(target);
      final s = _state(c);
      expect(s.fase, WavelengthFase.reveal);
      expect(s.currentRound?.score, 4);
    });

    test(
      'next() desde reveal avanza a siguiente ronda (clue) con nuevo psíquico',
      () async {
        final c = _container();
        await _notifier(c).iniciar(_config(['Ana', 'Luis'], rondas: 3));
        _notifier(c).confirmarPista('pista');
        _notifier(c).pasarDispositivo();
        _notifier(c).submitGuess(0.5);
        await _notifier(c).next();
        final s = _state(c);
        expect(s.fase, WavelengthFase.clue);
        // Psíquico rota: ronda 1 → jugador índice 1 (Luis).
        expect(s.currentPsychic, 'Luis');
      },
    );

    test('next() tras última ronda pasa a gameOver', () async {
      final c = _container();
      await _notifier(c).iniciar(_config(['Ana', 'Luis'], rondas: 1));
      _notifier(c).confirmarPista('pista');
      _notifier(c).pasarDispositivo();
      _notifier(c).submitGuess(0.5);
      await _notifier(c).next();
      expect(_state(c).fase, WavelengthFase.gameOver);
    });

    test('puntuación acumulada se suma correctamente', () async {
      final c = _container(seed: 0);
      await _notifier(c).iniciar(_config(['Ana', 'Luis'], rondas: 2));

      // Ronda 1: bullseye.
      final target1 = _state(c).currentRound!.targetPosition;
      _notifier(c).confirmarPista('pista');
      _notifier(c).pasarDispositivo();
      _notifier(c).submitGuess(target1);
      final score1 = _state(c).currentRound!.score!;
      await _notifier(c).next();

      // Ronda 2: miss.
      _notifier(c).confirmarPista('pista2');
      _notifier(c).pasarDispositivo();
      _notifier(c).submitGuess(0.0);
      await _notifier(c).next();

      final s = _state(c);
      expect(s.fase, WavelengthFase.gameOver);
      expect(s.session!.cumulativeScore, greaterThanOrEqualTo(score1));
    });
  });

  group('WavelengthFlowController · reiniciar', () {
    test('reiniciar tras partida vuelve a setup', () async {
      final c = _container();
      await _notifier(c).iniciar(_config(['Ana', 'Luis'], rondas: 1));
      _notifier(c).confirmarPista('pista');
      _notifier(c).pasarDispositivo();
      _notifier(c).submitGuess(0.5);
      await _notifier(c).next();
      expect(_state(c).fase, WavelengthFase.gameOver);

      _notifier(c).reiniciar();
      final s = _state(c);
      expect(s.fase, WavelengthFase.setup);
      expect(s.session, isNull);
      expect(s.currentRound, isNull);
    });
  });
}
