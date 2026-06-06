/// Tests de widget para [TabuSetupScreen].
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_repositories_provider.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_setup_screen.dart';

import '../../../support/localized_app.dart';
import 'support/fake_tabu_word_repository.dart';

/// Construye el harness mínimo: ProviderScope con repo fake + app localizada.
Widget _harness({FakeTabuWordRepository? repo}) {
  final wordRepo = repo ?? buildFakeTabuRepo();
  return ProviderScope(
    overrides: [
      tabuWordRepositoryProvider.overrideWith((ref) => Future.value(wordRepo)),
    ],
    child: localizedApp(const TabuSetupScreen()),
  );
}

void main() {
  group('TabuSetupScreen', () {
    testWidgets('muestra los campos de equipo A y equipo B', (tester) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      await tester.pump();

      expect(find.byType(TextField), findsNWidgets(2));
    });

    testWidgets('con equipo A vacío muestra error de validación', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      await tester.pump();

      // Equipo A vacío, equipo B relleno.
      await tester.enterText(find.byType(TextField).at(1), 'Los Creativos');
      await tester.pump();

      await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
      await tester.pump();

      expect(
        find.text('El nombre del equipo A no puede estar vacío.'),
        findsOneWidget,
      );
    });

    testWidgets('con nombres de equipo iguales muestra error de duplicados', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      await tester.pump();

      await tester.enterText(find.byType(TextField).at(0), 'Rojos');
      await tester.enterText(find.byType(TextField).at(1), 'rojos');
      await tester.pump();

      await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
      await tester.pump();

      expect(
        find.text('Los equipos deben tener nombres distintos.'),
        findsOneWidget,
      );
    });

    testWidgets('sin palabras muestra el diálogo sinPalabras', (tester) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness(repo: buildEmptyTabuRepo()));
      await tester.pump();

      await tester.enterText(find.byType(TextField).at(0), 'Rojos');
      await tester.enterText(find.byType(TextField).at(1), 'Azules');
      await tester.pump();

      await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      expect(find.text('Sin palabras'), findsOneWidget);
    });

    testWidgets('con config válida navega fuera del setup', (tester) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      final router = GoRouter(
        initialLocation: '/tabu',
        routes: [
          GoRoute(
            path: '/tabu',
            name: 'tabu-setup',
            builder: (ctx, st) => const TabuSetupScreen(),
            routes: [
              GoRoute(
                path: 'turn',
                name: 'tabu-turn',
                builder: (ctx, st) => const Scaffold(body: Text('turno')),
              ),
            ],
          ),
        ],
      );

      await tester.pumpWidget(
        ProviderScope(
          overrides: [
            tabuWordRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeTabuRepo()),
            ),
          ],
          child: localizedRouterApp(router),
        ),
      );
      await tester.pump();

      await tester.enterText(find.byType(TextField).at(0), 'Rojos');
      await tester.enterText(find.byType(TextField).at(1), 'Azules');
      await tester.pump();

      await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 200));

      expect(find.text('Empezar partida'), findsNothing);
      expect(find.text('turno'), findsOneWidget);
    });
  });
}
