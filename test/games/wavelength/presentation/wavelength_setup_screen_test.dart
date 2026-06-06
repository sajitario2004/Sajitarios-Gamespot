/// Tests de widget para [WavelengthSetupScreen].
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_config.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_repositories_provider.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_setup_screen.dart';

import '../../../support/localized_app.dart';
import 'support/fake_spectrum_repository.dart';

Widget _harness({FakeSpectrumRepository? spectrumRepo, int seed = 42}) {
  final sRepo = spectrumRepo ?? buildFakeSpectrumRepo();
  return ProviderScope(
    overrides: [
      spectrumRepositoryProvider.overrideWith((ref) => Future.value(sRepo)),
      randomProvider.overrideWithValue(RandomProvider.seeded(seed)),
    ],
    child: localizedApp(const WavelengthSetupScreen()),
  );
}

void main() {
  group('WavelengthSetupScreen', () {
    testWidgets('arranca con $kWavelengthMinPlayers jugadores', (tester) async {
      await tester.pumpWidget(_harness());
      await tester.pump();

      expect(find.byType(TextField), findsNWidgets(kWavelengthMinPlayers));
    });

    testWidgets('no permite quitar jugadores por debajo del mínimo', (
      tester,
    ) async {
      await tester.pumpWidget(_harness());
      await tester.pump();

      // Find only the "Quitar jugador" buttons by tooltip (distinct from the
      // Rondas decrement button which also uses remove_circle_outline).
      final quitarFinder = find.byWidgetPredicate(
        (w) => w is IconButton && w.tooltip == 'Quitar jugador',
      );
      expect(quitarFinder, findsNWidgets(kWavelengthMinPlayers));
      for (final element in quitarFinder.evaluate()) {
        final btn = element.widget as IconButton;
        expect(
          btn.onPressed,
          isNull,
          reason: 'no debe permitir quitar con el mínimo',
        );
      }
    });

    testWidgets('puede añadir hasta $kWavelengthMaxPlayers jugadores', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1080, 4000);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      await tester.pump();

      for (var i = kWavelengthMinPlayers; i < kWavelengthMaxPlayers; i++) {
        await tester.tap(
          find.widgetWithIcon(OutlinedButton, Icons.person_add_alt_1),
        );
        await tester.pump();
      }

      expect(find.byType(TextField), findsNWidgets(kWavelengthMaxPlayers));

      // El botón Añadir queda deshabilitado al llegar al máximo.
      final addBtn = tester.widget<OutlinedButton>(
        find.widgetWithIcon(OutlinedButton, Icons.person_add_alt_1),
      );
      expect(addBtn.onPressed, isNull);
    });

    testWidgets('validación: nombre vacío muestra snackbar de error', (
      tester,
    ) async {
      await tester.pumpWidget(_harness());
      await tester.pump();

      // No rellenamos ningún nombre — pulsamos Empezar.
      await tester.tap(
        find.widgetWithIcon(FilledButton, Icons.play_arrow_rounded),
      );
      await tester.pump();

      expect(find.byType(SnackBar), findsOneWidget);
    });

    testWidgets('validación: nombres duplicados muestra snackbar de error', (
      tester,
    ) async {
      await tester.pumpWidget(_harness());
      await tester.pump();

      final fields = find.byType(TextField);
      await tester.enterText(fields.at(0), 'Ana');
      await tester.enterText(fields.at(1), 'Ana');

      await tester.tap(
        find.widgetWithIcon(FilledButton, Icons.play_arrow_rounded),
      );
      await tester.pump();

      expect(find.byType(SnackBar), findsOneWidget);
    });

    testWidgets('sinEspectros: muestra diálogo cuando el pool está vacío', (
      tester,
    ) async {
      await tester.pumpWidget(_harness(spectrumRepo: buildEmptySpectrumRepo()));
      await tester.pump();

      final fields = find.byType(TextField);
      await tester.enterText(fields.at(0), 'Ana');
      await tester.enterText(fields.at(1), 'Luis');

      await tester.tap(
        find.widgetWithIcon(FilledButton, Icons.play_arrow_rounded),
      );
      // Pump through the async iniciar() call.
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 100));

      expect(find.byType(AlertDialog), findsOneWidget);
    });

    testWidgets('selector de rondas incrementa y decrementa el contador', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1080, 4000);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      await tester.pump();

      // Defaults to kWavelengthDefaultRondas.
      expect(find.text('$kWavelengthDefaultRondas'), findsOneWidget);

      // Increment once.
      await tester.tap(
        find.widgetWithIcon(IconButton, Icons.add_circle_outline),
      );
      await tester.pump();
      expect(find.text('${kWavelengthDefaultRondas + 1}'), findsOneWidget);

      // Decrement twice (back to default - 1).
      await tester.tap(
        find.widgetWithIcon(IconButton, Icons.remove_circle_outline).last,
      );
      await tester.pump();
      await tester.tap(
        find.widgetWithIcon(IconButton, Icons.remove_circle_outline).last,
      );
      await tester.pump();
      expect(find.text('${kWavelengthDefaultRondas - 1}'), findsOneWidget);
    });
  });
}
