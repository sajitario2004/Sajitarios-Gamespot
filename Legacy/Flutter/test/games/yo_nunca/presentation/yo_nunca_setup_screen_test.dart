/// Tests de widget para [YoNuncaSetupScreen].
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_repositories_provider.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_setup_screen.dart';

import '../../../support/localized_app.dart';
import 'support/fake_never_statement_repository.dart';

/// Construye el harness mínimo: ProviderScope con repo fake + app localizada.
Widget _harness({FakeNeverStatementRepository? repo}) {
  final stmtRepo = repo ?? buildFakeRepo();
  return ProviderScope(
    overrides: [
      neverStatementRepositoryProvider.overrideWith(
        (ref) => Future.value(stmtRepo),
      ),
    ],
    child: localizedApp(const YoNuncaSetupScreen()),
  );
}

void main() {
  group('YoNuncaSetupScreen', () {
    testWidgets('muestra chips de intensidad suave y picante', (tester) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      await tester.pump();

      expect(find.text('Suave'), findsOneWidget);
      expect(find.text('Picante'), findsOneWidget);
    });

    testWidgets('sin ninguna intensidad seleccionada muestra error', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      await tester.pump();

      // Deseleccionar suave (viene seleccionado por defecto).
      await tester.tap(find.text('Suave'));
      await tester.pump();

      await tester.tap(find.widgetWithText(FilledButton, 'Empezar'));
      await tester.pump();

      expect(
        find.text('Elige al menos una intensidad para jugar.'),
        findsOneWidget,
      );
    });

    testWidgets('activar picante muestra la advertencia de contenido adulto', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      await tester.pump();

      // Activar picante.
      await tester.tap(find.text('Picante'));
      await tester.pump();

      expect(find.textContaining('Contenido explícito'), findsOneWidget);
    });

    testWidgets('desactivar picante oculta la advertencia', (tester) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      await tester.pump();

      // Activar y luego desactivar picante.
      await tester.tap(find.text('Picante'));
      await tester.pump();
      await tester.tap(find.text('Picante'));
      await tester.pump();

      expect(find.textContaining('Contenido adulto'), findsNothing);
    });

    testWidgets('sin frases disponibles muestra el diálogo sinFrases', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness(repo: buildEmptyRepo()));
      await tester.pump();

      await tester.tap(find.widgetWithText(FilledButton, 'Empezar'));
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      expect(find.text('Sin frases'), findsOneWidget);
    });

    testWidgets('con config válida navega a la pantalla de juego', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1080, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      final router = GoRouter(
        initialLocation: '/yo-nunca',
        routes: [
          GoRoute(
            path: '/yo-nunca',
            name: 'yo-nunca-setup',
            builder: (ctx, st) => const YoNuncaSetupScreen(),
            routes: [
              GoRoute(
                path: 'play',
                name: 'yo-nunca-play',
                builder: (ctx, st) =>
                    const Scaffold(body: Text('pantalla de juego')),
              ),
            ],
          ),
        ],
      );

      await tester.pumpWidget(
        ProviderScope(
          overrides: [
            neverStatementRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeRepo()),
            ),
          ],
          child: localizedRouterApp(router),
        ),
      );
      await tester.pump();

      await tester.tap(find.widgetWithText(FilledButton, 'Empezar'));
      // Procesar la cadena asíncrona: FutureProvider → flow controller → GoRouter.
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 500));

      expect(find.text('pantalla de juego'), findsOneWidget);
    });
  });
}
