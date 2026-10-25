/// Tests de widget para [TriviaSetupScreen].
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/trivia/domain/trivia_config.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_repositories_provider.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_setup_screen.dart';

import '../../../support/localized_app.dart';
import 'support/fake_question_repository.dart';
import 'support/fake_winner_repository.dart';

/// Construye el harness mínimo: ProviderScope con repos fake + app localizada.
Widget _harness({
  FakeWinnerRepository? winnerRepo,
  FakeQuestionRepository? questionRepo,
}) {
  final wRepo = winnerRepo ?? FakeWinnerRepository();
  final qRepo = questionRepo ?? buildFakeQuestionRepo();
  return ProviderScope(
    overrides: [
      winnerRepositoryProvider.overrideWith((ref) => Future.value(wRepo)),
      questionRepositoryProvider.overrideWith((ref) => Future.value(qRepo)),
    ],
    child: localizedApp(const TriviaSetupScreen()),
  );
}

void main() {
  group('TriviaSetupScreen', () {
    testWidgets(
      'arranca con $kTriviaMinPlayers jugadores y no permite bajar de ese mínimo',
      (tester) async {
        await tester.pumpWidget(_harness());
        await tester.pump();

        expect(find.byType(TextField), findsNWidgets(kTriviaMinPlayers));

        final quitarFinder = find.widgetWithIcon(
          IconButton,
          Icons.remove_circle_outline,
        );
        expect(quitarFinder, findsNWidgets(kTriviaMinPlayers));
        for (final element in quitarFinder.evaluate()) {
          final btn = element.widget as IconButton;
          expect(
            btn.onPressed,
            isNull,
            reason: 'no debe permitir quitar con el mínimo de jugadores',
          );
        }
      },
    );

    testWidgets('puede añadir hasta $kTriviaMaxPlayers jugadores', (
      tester,
    ) async {
      // Pantalla alta para que el botón Añadir jugador sea siempre visible.
      tester.view.physicalSize = const Size(1080, 4000);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      await tester.pump();

      // Añadir hasta llegar al máximo.
      for (var i = kTriviaMinPlayers; i < kTriviaMaxPlayers; i++) {
        await tester.tap(
          find.widgetWithIcon(OutlinedButton, Icons.person_add_alt_1),
        );
        await tester.pump();
      }

      expect(find.byType(TextField), findsNWidgets(kTriviaMaxPlayers));

      // El botón de añadir queda deshabilitado en el máximo.
      final addBtn = tester.widget<OutlinedButton>(
        find.widgetWithIcon(OutlinedButton, Icons.person_add_alt_1),
      );
      expect(addBtn.onPressed, isNull);
    });

    testWidgets('con nombre vacío muestra error de validación', (tester) async {
      tester.view.physicalSize = const Size(1080, 4000);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      await tester.pump();

      // Solo rellena el primer campo; el segundo queda vacío.
      await tester.enterText(find.byType(TextField).at(0), 'Nacho');
      await tester.pump();

      await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
      await tester.pump();

      expect(
        find.text('Todos los jugadores deben tener un nombre.'),
        findsOneWidget,
      );
    });

    testWidgets('iniciar con config válida navega fuera del setup', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1080, 4000);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      final qRepo = buildFakeQuestionRepo(countPerTier: 6);

      // GoRouter mínimo: setup → pass (stub) para que goNamed no lance.
      final router = GoRouter(
        initialLocation: '/trivia',
        routes: [
          GoRoute(
            path: '/trivia',
            name: 'trivia-setup',
            builder: (ctx, st) => const TriviaSetupScreen(),
            routes: [
              GoRoute(
                path: 'pass',
                name: 'trivia-pass',
                builder: (ctx, st) => const Scaffold(body: Text('pass')),
              ),
            ],
          ),
        ],
      );

      await tester.pumpWidget(
        ProviderScope(
          overrides: [
            winnerRepositoryProvider.overrideWith(
              (ref) => Future.value(FakeWinnerRepository()),
            ),
            questionRepositoryProvider.overrideWith(
              (ref) => Future.value(qRepo),
            ),
          ],
          child: localizedRouterApp(router),
        ),
      );
      await tester.pump();

      await tester.enterText(find.byType(TextField).at(0), 'Nacho');
      await tester.enterText(find.byType(TextField).at(1), 'Iker');
      await tester.pump();

      await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 200));

      // Tras iniciar con éxito el router navega a pass y el setup ya no aparece.
      expect(find.text('Empezar partida'), findsNothing);
      expect(find.text('pass'), findsOneWidget);
    });

    testWidgets('iniciar sin preguntas muestra el diálogo sinPreguntas', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1080, 4000);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      // Repo vacío → sinPreguntas.
      final emptyRepo = FakeQuestionRepository([]);
      await tester.pumpWidget(_harness(questionRepo: emptyRepo));
      await tester.pump();

      await tester.enterText(find.byType(TextField).at(0), 'Ana');
      await tester.enterText(find.byType(TextField).at(1), 'Luis');
      await tester.pump();

      await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
      // Damos tiempo a la operación asíncrona.
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pump();

      expect(find.text('Sin preguntas'), findsOneWidget);
    });

    testWidgets(
      'el icono ¿Cómo se juega? está presente y abre la RulesScreen',
      (tester) async {
        await tester.pumpWidget(_harness());
        await tester.pump();

        final helpBtn = find.widgetWithIcon(IconButton, Icons.help_outline);
        expect(helpBtn, findsOneWidget);

        await tester.tap(helpBtn);
        await tester.pump();
        await tester.pump(const Duration(milliseconds: 400));

        expect(find.text('¿Cómo se juega?'), findsOneWidget);
        expect(
          find.text(
            'Hasta 6 jugadores compiten respondiendo preguntas por turnos.',
          ),
          findsOneWidget,
        );
      },
    );
  });
}
