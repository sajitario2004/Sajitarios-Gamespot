/// Tests de widget para [WavelengthGameOverScreen].
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/spectrum.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_flow_controller.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_repositories_provider.dart';

import '../../../support/localized_app.dart';
import 'support/fake_spectrum_repository.dart';

// ─── Helpers ──────────────────────────────────────────────────────────────────

final _testSpectrum = Spectrum(
  id: 1,
  leftConcept: 'frío',
  rightConcept: 'caliente',
);

/// Construye una [WavelengthSession] con [rondas] rondas completadas, todas
/// con guess = target → bullseye (4 pts cada una).
WavelengthSession _completedSession(int rondas) {
  var session = WavelengthSession.start(
    playerNames: ['Ana', 'Luis'],
    totalRondas: rondas,
  );
  for (var i = 0; i < rondas; i++) {
    final round = WavelengthRound.start(
      spectrum: _testSpectrum,
      targetPosition: 0.5,
    ).withClue('pista').withGuess(0.5); // bullseye = 4 pts
    session = session.recordRound(round);
  }
  return session;
}

WavelengthFlowState _gameOverState({int rondas = 2}) {
  return WavelengthFlowState(
    fase: WavelengthFase.gameOver,
    config: WavelengthConfig.create(
      playerNames: ['Ana', 'Luis'],
      rondas: rondas,
    ).config,
    session: _completedSession(rondas),
  );
}

class _PreloadedController extends WavelengthFlowController {
  _PreloadedController(this._initial);

  final WavelengthFlowState _initial;

  @override
  WavelengthFlowState build() => _initial;
}

Widget _harness(WavelengthFlowState state) {
  return ProviderScope(
    overrides: [
      wavelengthFlowControllerProvider.overrideWith(
        () => _PreloadedController(state),
      ),
      spectrumRepositoryProvider.overrideWith(
        (ref) => Future.value(buildFakeSpectrumRepo()),
      ),
      randomProvider.overrideWithValue(RandomProvider.seeded(42)),
    ],
    child: localizedApp(const WavelengthGameOverScreen()),
  );
}

// ─── Tests ────────────────────────────────────────────────────────────────────

void main() {
  group('WavelengthGameOverScreen', () {
    testWidgets('muestra el título de fin de partida', (tester) async {
      await tester.pumpWidget(_harness(_gameOverState()));
      await tester.pump();

      expect(find.text('Fin de la partida'), findsOneWidget);
    });

    testWidgets('muestra la puntuación acumulada correcta', (tester) async {
      // 2 rondas × bullseye (4 pts) = 8 pts.
      await tester.pumpWidget(_harness(_gameOverState(rondas: 2)));
      await tester.pump();

      expect(find.text('8 puntos'), findsOneWidget);
    });

    testWidgets('muestra la etiqueta de puntuación final', (tester) async {
      await tester.pumpWidget(_harness(_gameOverState()));
      await tester.pump();

      expect(find.text('Puntuación final'), findsOneWidget);
    });

    testWidgets('muestra el botón "Jugar otra"', (tester) async {
      await tester.pumpWidget(_harness(_gameOverState()));
      await tester.pump();

      expect(find.text('Jugar otra'), findsOneWidget);
    });

    testWidgets('muestra el botón "Volver al menú"', (tester) async {
      await tester.pumpWidget(_harness(_gameOverState()));
      await tester.pump();

      expect(find.text('Volver al menú'), findsOneWidget);
    });

    testWidgets('con 1 ronda muestra 4 puntos (bullseye)', (tester) async {
      await tester.pumpWidget(_harness(_gameOverState(rondas: 1)));
      await tester.pump();

      expect(find.text('4 puntos'), findsOneWidget);
    });
  });
}
