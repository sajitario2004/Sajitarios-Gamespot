/// Tests de la BombaSetupScreen.
///
/// Cubre: selección de modo, validación de jugadores (2..12), sinPrompts dialog.
///
/// IMPORTANT: makeFakeRepo() usa sqflite_ffi que necesita I/O real y no puede
/// correr dentro de la zona fake-async de testWidgets. Se llama en setUp().
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/bomba/data/bomba_prompt_repository.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_flow_controller.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_repositories_provider.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_setup_screen.dart';

import '../../../support/localized_app.dart';
import 'support/fake_bomba_prompt_repository.dart';

Widget _harness(BombaPromptRepository repo) => ProviderScope(
  overrides: [
    randomProvider.overrideWithValue(RandomProvider.seeded(42)),
    bombaPromptRepositoryProvider.overrideWith((_) async => repo),
  ],
  child: localizedApp(const BombaSetupScreen()),
);

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('BombaSetupScreen', () {
    late BombaPromptRepository repo;

    setUp(() async {
      repo = await makeFakeRepo();
    });

    testWidgets('muestra los campos mínimos al arrancar (2 jugadores)', (
      tester,
    ) async {
      await tester.pumpWidget(_harness(repo));

      expect(find.byType(TextField), findsNWidgets(kBombaMinPlayers));
    });

    testWidgets('botón quitar deshabilitado en el mínimo', (tester) async {
      await tester.pumpWidget(_harness(repo));

      final quitarFinder = find.widgetWithIcon(
        IconButton,
        Icons.remove_circle_outline,
      );
      expect(quitarFinder, findsNWidgets(kBombaMinPlayers));
      for (final el in quitarFinder.evaluate()) {
        final btn = el.widget as IconButton;
        expect(btn.onPressed, isNull, reason: 'no debe poder quitar en mínimo');
      }
    });

    testWidgets('añadir jugador habilita el botón quitar', (tester) async {
      await tester.pumpWidget(_harness(repo));

      await tester.tap(
        find.widgetWithIcon(OutlinedButton, Icons.person_add_alt_1),
      );
      await tester.pump();

      expect(find.byType(TextField), findsNWidgets(kBombaMinPlayers + 1));

      final quitarFinder = find.widgetWithIcon(
        IconButton,
        Icons.remove_circle_outline,
      );
      for (final el in quitarFinder.evaluate()) {
        final btn = el.widget as IconButton;
        expect(btn.onPressed, isNotNull);
      }
    });

    testWidgets('no puede superar kBombaMaxPlayers jugadores', (tester) async {
      // Use a tall device so the add button is always visible.
      tester.view.physicalSize = const Size(1080, 4800);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness(repo));

      // Añadir hasta kBombaMaxPlayers (10 taps desde 2 hasta 12).
      for (var i = kBombaMinPlayers; i < kBombaMaxPlayers; i++) {
        final addBtn = find.widgetWithIcon(
          OutlinedButton,
          Icons.person_add_alt_1,
        );
        await tester.ensureVisible(addBtn);
        await tester.tap(addBtn);
        await tester.pump();
      }

      expect(find.byType(TextField), findsNWidgets(kBombaMaxPlayers));

      // En el máximo el botón está deshabilitado.
      final addBtn = find.widgetWithIcon(
        OutlinedButton,
        Icons.person_add_alt_1,
      );
      final btn = tester.widget<OutlinedButton>(addBtn);
      expect(btn.onPressed, isNull);
    });

    testWidgets('error cuando hay nombres vacíos', (tester) async {
      await tester.pumpWidget(_harness(repo));

      await tester.tap(find.byType(FilledButton));
      await tester.pump();

      expect(find.byType(SnackBar), findsOneWidget);
    });

    testWidgets(
      'el icono ¿Cómo se juega? está presente y abre la RulesScreen',
      (tester) async {
        await tester.pumpWidget(_harness(repo));
        await tester.pump();

        final helpBtn = find.widgetWithIcon(IconButton, Icons.help_outline);
        expect(helpBtn, findsOneWidget);

        await tester.tap(helpBtn);
        await tester.pump();
        await tester.pump(const Duration(milliseconds: 400));

        expect(find.text('¿Cómo se juega?'), findsOneWidget);
        expect(
          find.text('Se muestra una sílaba o categoría en pantalla.'),
          findsOneWidget,
        );
      },
    );
  });

  group('BombaSetupScreen — sinPrompts', () {
    late BombaPromptRepository emptyRepo;

    setUp(() async {
      emptyRepo = await makeEmptyRepo(emptyMode: BombaMode.silaba);
    });

    testWidgets('muestra diálogo sinPrompts cuando el pool está vacío', (
      tester,
    ) async {
      await tester.pumpWidget(
        ProviderScope(
          overrides: [
            randomProvider.overrideWithValue(RandomProvider.seeded(42)),
            bombaPromptRepositoryProvider.overrideWith(
              (_) => Future.value(emptyRepo),
            ),
          ],
          child: localizedApp(const BombaSetupScreen()),
        ),
      );

      final fields = find.byType(TextField);
      await tester.enterText(fields.at(0), 'Ana');
      await tester.enterText(fields.at(1), 'Luis');

      await tester.tap(find.byType(FilledButton));
      // _iniciarPartida() llama a iniciar() que internamente awaita sqflite_ffi.
      // sqflite_ffi puede usar dart:isolate internamente, lo que no se puede
      // resolver en la zona fake-async con pump(Duration). Usamos runAsync()
      // para escaper al event loop real y que las operaciones async completen.
      await tester.runAsync(() async {
        // Ceder el control al event loop para que FutureProvider y iniciar()
        // completen sus operaciones async reales.
        await Future<void>.delayed(const Duration(milliseconds: 50));
      });
      await tester.pump(); // renderiza el diálogo
      await tester.pump(); // frame adicional para dialogo

      expect(find.byType(AlertDialog), findsOneWidget);

      // Cerrar el diálogo — pump fijo (no pumpAndSettle: hay estado error activo).
      final dialogBtn = find.descendant(
        of: find.byType(AlertDialog),
        matching: find.byType(FilledButton),
      );
      await tester.tap(dialogBtn);
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 100));
    });
  });
}
