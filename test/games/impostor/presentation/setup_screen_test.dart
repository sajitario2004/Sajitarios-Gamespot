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
  });
}
