/// Tests de widget para [WavelengthRevealScreen].
///
/// La pantalla embebe un [GameWidget] (Flame); NO se usa pumpAndSettle sobre
/// la superficie animada — se bombea con duraciones fijas.
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/spectrum.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_flow_controller.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_repositories_provider.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_reveal_screen.dart';

import '../../../support/localized_app.dart';
import 'support/fake_spectrum_repository.dart';

// ─── Helpers ──────────────────────────────────────────────────────────────────

final _testSpectrum = Spectrum(
  id: 1,
  leftConcept: 'frío',
  rightConcept: 'caliente',
);

/// Builds a reveal state with a completed round (has guess → has score).
WavelengthFlowState _revealState({
  double target = 0.5,
  double guess = 0.5,
  int rondas = 3,
  int completedRounds = 0,
}) {
  var session = WavelengthSession.start(
    playerNames: ['Ana', 'Luis'],
    totalRondas: rondas,
  );
  // Record completedRounds with bullseye to build cumulative score.
  for (var i = 0; i < completedRounds; i++) {
    final r = WavelengthRound.start(
      spectrum: _testSpectrum,
      targetPosition: 0.5,
    ).withClue('pista').withGuess(0.5);
    session = session.recordRound(r);
  }
  final round = WavelengthRound.start(
    spectrum: _testSpectrum,
    targetPosition: target,
  ).withClue('pista').withGuess(guess);

  return WavelengthFlowState(
    fase: WavelengthFase.reveal,
    config: WavelengthConfig.create(
      playerNames: ['Ana', 'Luis'],
      rondas: rondas,
    ).config,
    session: session,
    currentRound: round,
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
    child: localizedApp(const WavelengthRevealScreen()),
  );
}

// ─── Tests ────────────────────────────────────────────────────────────────────

void main() {
  group('WavelengthRevealScreen', () {
    testWidgets('muestra el título Wavelength', (tester) async {
      await tester.pumpWidget(_harness(_revealState()));
      await tester.pump();

      expect(find.text('Wavelength'), findsOneWidget);
    });

    testWidgets('muestra el indicador de ronda', (tester) async {
      await tester.pumpWidget(_harness(_revealState(rondas: 3)));
      await tester.pump();

      expect(find.text('Ronda 1 de 3'), findsOneWidget);
    });

    testWidgets('muestra la puntuación de la ronda — bullseye (4 pts)', (
      tester,
    ) async {
      // target = guess = 0.5 → bullseye.
      await tester.pumpWidget(_harness(_revealState(target: 0.5, guess: 0.5)));
      await tester.pump();

      expect(find.text('4 puntos esta ronda'), findsOneWidget);
    });

    testWidgets('muestra la puntuación total acumulada', (tester) async {
      // 0 completed rounds + current bullseye → display shows 0 (from session).
      await tester.pumpWidget(
        _harness(_revealState(target: 0.5, guess: 0.5, completedRounds: 0)),
      );
      await tester.pump();

      // Session cumulative is 0 (round not yet recorded).
      expect(find.text('Total: 0 puntos'), findsOneWidget);
    });

    testWidgets('muestra "Siguiente ronda" en ronda intermedia', (
      tester,
    ) async {
      await tester.pumpWidget(_harness(_revealState(rondas: 3)));
      await tester.pump();

      expect(find.text('Siguiente ronda'), findsOneWidget);
    });

    testWidgets('muestra "Ver resultado" en la última ronda', (tester) async {
      // rondas = 1, completedRounds = 0 → roundNum = 1 = totalRounds.
      await tester.pumpWidget(_harness(_revealState(rondas: 1)));
      await tester.pump();

      expect(find.text('Ver resultado'), findsOneWidget);
    });
  });
}
