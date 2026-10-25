import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/setup_screen.dart';

import '../../../support/localized_app.dart';
import 'support/fake_assign_roles_coordinator.dart';

/// Envuelve la [SetupScreen] en lo mínimo para testearla de forma aislada:
/// un [MaterialApp] (provee [Theme] y [ScaffoldMessenger]) y un [ProviderScope]
/// que sobreescribe el coordinador de asignación para no tocar la BD real.
Widget _harness() {
  return ProviderScope(
    overrides: [
      assignRolesCoordinatorProvider.overrideWithValue(
        FakeAssignRolesCoordinator(),
      ),
    ],
    child: localizedApp(const SetupScreen()),
  );
}

void main() {
  group('SetupScreen', () {
    testWidgets(
      'arranca con el mínimo de jugadores y no permite bajar de $kMinPlayers',
      (tester) async {
        await tester.pumpWidget(_harness());

        // Arranca exactamente con kMinPlayers campos de nombre.
        expect(find.byType(TextField), findsNWidgets(kMinPlayers));

        // El botón "quitar jugador" está deshabilitado para todos cuando se
        // está en el mínimo (no se puede bajar de 3).
        final quitarFinder = find.widgetWithIcon(
          IconButton,
          Icons.remove_circle_outline,
        );
        expect(quitarFinder, findsNWidgets(kMinPlayers));
        for (final element in quitarFinder.evaluate()) {
          final boton = element.widget as IconButton;
          expect(
            boton.onPressed,
            isNull,
            reason: 'no debe poder quitarse jugador en el mínimo',
          );
        }
      },
    );

    testWidgets(
      'al añadir un jugador ya se pueden quitar (vuelve a haber margen)',
      (tester) async {
        await tester.pumpWidget(_harness());

        await tester.tap(
          find.widgetWithIcon(OutlinedButton, Icons.person_add_alt_1),
        );
        await tester.pump();

        expect(find.byType(TextField), findsNWidgets(kMinPlayers + 1));

        final quitarFinder = find.widgetWithIcon(
          IconButton,
          Icons.remove_circle_outline,
        );
        // Con 4 jugadores, los botones de quitar deben estar habilitados.
        for (final element in quitarFinder.evaluate()) {
          final boton = element.widget as IconButton;
          expect(boton.onPressed, isNotNull);
        }
      },
    );

    testWidgets(
      'el nº de impostores está capado a maxImpostoresFor (players - 1)',
      (tester) async {
        await tester.pumpWidget(_harness());

        // Con 3 jugadores, el máximo de impostores es 2 (players - 1).
        final maxEsperado = GameConfig.maxImpostoresFor(kMinPlayers);
        expect(maxEsperado, 2);

        final subir = find.widgetWithIcon(IconButton, Icons.add);
        // Sube impostores hasta intentar pasarse del tope.
        for (var i = 0; i < kMaxImpostores; i++) {
          final boton = tester.widget<IconButton>(subir);
          if (boton.onPressed == null) break;
          await tester.tap(subir);
          await tester.pump();
        }

        // El contador no supera el tope (2 para 3 jugadores).
        expect(find.text('$maxEsperado impostores'), findsOneWidget);

        // En el tope, el botón "más impostores" queda deshabilitado.
        final botonSubir = tester.widget<IconButton>(subir);
        expect(botonSubir.onPressed, isNull);
      },
    );

    testWidgets('el nº de rondas está capado a maxRoundsFor (jugadores - 3)', (
      tester,
    ) async {
      // Superficie alta para que el formulario completo (incluido el selector
      // de rondas) quepa sin scroll.
      tester.view.physicalSize = const Size(1200, 3000);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      await tester.pumpWidget(_harness());
      await tester.pump();

      // Añadimos jugadores hasta llegar a 6 (arranca en 3). Con 6 jugadores,
      // el máximo de rondas es 3 = max(1, 6 - 3).
      for (var i = kMinPlayers; i < 6; i++) {
        await tester.tap(
          find.widgetWithIcon(OutlinedButton, Icons.person_add_alt_1),
        );
        await tester.pump();
      }
      expect(find.byType(TextField), findsNWidgets(6));

      final maxRondas = GameConfig.maxRoundsFor(6);
      expect(maxRondas, 3);

      // El selector de rondas arranca en el mínimo (kMinRounds = 1) al subir
      // jugadores (el clamp mantiene el valor dentro del rango sin elevarlo).
      expect(find.text('1 ronda'), findsOneWidget);

      // El stepper de rondas comparte iconos +/- con el de impostores; el de
      // rondas es el SEGUNDO en el árbol (impostores va antes). Subimos las
      // rondas hasta intentar pasar del tope (3).
      final masRondas = find.widgetWithIcon(IconButton, Icons.add).at(1);
      for (var i = 0; i < 5; i++) {
        final boton = tester.widget<IconButton>(masRondas);
        if (boton.onPressed == null) break;
        await tester.tap(masRondas);
        await tester.pump();
      }

      // El contador no supera el tope (3 rondas para 6 jugadores).
      expect(find.text('$maxRondas rondas'), findsOneWidget);

      // En el tope, el botón "más rondas" queda deshabilitado.
      final botonTope = tester.widget<IconButton>(masRondas);
      expect(botonTope.onPressed, isNull);
    });

    testWidgets(
      'con un nombre vacío muestra el error de validación en español',
      (tester) async {
        // Superficie alta para que todo el formulario quepa sin scroll y el
        // botón "Empezar partida" esté presente en el árbol.
        tester.view.physicalSize = const Size(1200, 3000);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);
        addTearDown(tester.view.resetDevicePixelRatio);

        await tester.pumpWidget(_harness());

        // Rellena solo dos de los tres campos: queda uno vacío.
        final campos = find.byType(TextField);
        await tester.enterText(campos.at(0), 'Nacho');
        await tester.enterText(campos.at(1), 'Iker');
        await tester.pump();

        await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
        await tester.pump();

        expect(
          find.text('Todos los jugadores deben tener un nombre.'),
          findsOneWidget,
        );
      },
    );

    testWidgets('el icono ¿Cómo se juega? está presente y abre la RulesScreen', (
      tester,
    ) async {
      await tester.pumpWidget(_harness());
      await tester.pump();

      // El icono de ayuda existe en la AppBar.
      final helpBtn = find.widgetWithIcon(IconButton, Icons.help_outline);
      expect(helpBtn, findsOneWidget);

      await tester.tap(helpBtn);
      // Pump suficiente para que el MaterialPageRoute se instale sin entrar en
      // PulseGlow ni animaciones de la pantalla de juego.
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 400));

      // La RulesScreen muestra la cabecera localizada.
      expect(find.text('¿Cómo se juega?'), findsOneWidget);
      // Y al menos el primer paso de las reglas del Impostor.
      expect(
        find.text('Decide cuántos jugadores, impostores y rondas habrá.'),
        findsOneWidget,
      );
    });
  });
}
