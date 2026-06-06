/// Tests de widget para [WavelengthClueScreen].
///
/// La pantalla embebe un [GameWidget] (Flame); NO se usa pumpAndSettle sobre
/// la superficie animada — se bombea con duraciones fijas.
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/spectrum.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_clue_screen.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_flow_controller.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_repositories_provider.dart';

import '../../../support/localized_app.dart';
import 'support/fake_spectrum_repository.dart';

// ─── Helpers ──────────────────────────────────────────────────────────────────

final _testSpectrum = Spectrum(
  id: 1,
  leftConcept: 'frío',
  rightConcept: 'caliente',
);

WavelengthFlowState _clueState() {
  final session = WavelengthSession.start(
    playerNames: ['Ana', 'Luis'],
    totalRondas: 3,
  );
  final round = WavelengthRound.start(
    spectrum: _testSpectrum,
    targetPosition: 0.5,
  );
  return WavelengthFlowState(
    fase: WavelengthFase.clue,
    config: WavelengthConfig.create(
      playerNames: ['Ana', 'Luis'],
      rondas: 3,
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
    child: localizedApp(const WavelengthClueScreen()),
  );
}

// ─── Tests ────────────────────────────────────────────────────────────────────

void main() {
  group('WavelengthClueScreen', () {
    testWidgets('muestra el título Wavelength', (tester) async {
      await tester.pumpWidget(_harness(_clueState()));
      // Pump once — no pumpAndSettle sobre la superficie Flame.
      await tester.pump();

      expect(find.text('Wavelength'), findsOneWidget);
    });

    testWidgets('muestra el indicador de ronda', (tester) async {
      await tester.pumpWidget(_harness(_clueState()));
      await tester.pump();

      // "Ronda 1 de 3"
      expect(find.text('Ronda 1 de 3'), findsOneWidget);
    });

    testWidgets('muestra el nombre del psíquico', (tester) async {
      await tester.pumpWidget(_harness(_clueState()));
      await tester.pump();

      expect(find.text('Ana'), findsOneWidget);
    });

    testWidgets('muestra el campo de texto para la pista', (tester) async {
      await tester.pumpWidget(_harness(_clueState()));
      await tester.pump();

      expect(find.byType(TextField), findsOneWidget);
    });

    testWidgets('muestra el botón de confirmar pista', (tester) async {
      await tester.pumpWidget(_harness(_clueState()));
      await tester.pump();

      expect(find.text('Confirmar pista y pasar el movil'), findsOneWidget);
    });

    testWidgets('puede introducir texto en el campo de pista', (tester) async {
      await tester.pumpWidget(_harness(_clueState()));
      await tester.pump();

      await tester.enterText(find.byType(TextField), 'ni frío ni caliente');
      expect(find.text('ni frío ni caliente'), findsOneWidget);
    });
  });
}
