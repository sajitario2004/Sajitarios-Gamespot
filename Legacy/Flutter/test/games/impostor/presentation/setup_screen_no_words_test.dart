import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/setup_screen.dart';

import '../../../support/localized_app.dart';

/// Coordinador falso que simula que la BD no tiene ninguna palabra: siempre
/// lanza [NoWordsAvailableException] al intentar armar la partida.
class _SinPalabrasCoordinator implements AssignRolesCoordinator {
  @override
  Future<GameSession> assign(GameConfig config) async {
    throw const NoWordsAvailableException();
  }
}

/// Harness con router real para poder verificar la navegación a la pantalla de
/// gestión de palabras desde el diálogo de error.
Widget _harness() {
  final router = GoRouter(
    initialLocation: '/setup',
    routes: [
      GoRoute(
        path: '/setup',
        name: 'impostor-setup',
        builder: (_, _) => const SetupScreen(),
      ),
      GoRoute(
        path: '/words',
        name: 'impostor-words',
        builder: (_, _) => const Scaffold(body: Text('GESTION_PALABRAS')),
      ),
    ],
  );
  return ProviderScope(
    overrides: [
      assignRolesCoordinatorProvider.overrideWithValue(
        _SinPalabrasCoordinator(),
      ),
    ],
    child: localizedRouterApp(router),
  );
}

void main() {
  group('SetupScreen sin palabras (edge case)', () {
    testWidgets(
      'al iniciar sin palabras en la BD muestra un diálogo claro y no crashea',
      (tester) async {
        // Superficie alta para que el botón "Empezar partida" esté presente.
        tester.view.physicalSize = const Size(1200, 3000);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);
        addTearDown(tester.view.resetDevicePixelRatio);

        await tester.pumpWidget(_harness());

        // Rellena los 3 jugadores mínimos.
        final campos = find.byType(TextField);
        await tester.enterText(campos.at(0), 'Nacho');
        await tester.enterText(campos.at(1), 'Iker');
        await tester.enterText(campos.at(2), 'Lucía');
        await tester.pump();

        await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
        await tester.pumpAndSettle();

        // En vez de crashear, aparece un diálogo guía en español.
        expect(find.text('No hay palabras'), findsOneWidget);
        expect(
          find.widgetWithText(FilledButton, 'Gestionar palabras'),
          findsOneWidget,
        );
      },
    );

    testWidgets(
      'desde el diálogo de "No hay palabras" se navega a gestionar palabras',
      (tester) async {
        tester.view.physicalSize = const Size(1200, 3000);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);
        addTearDown(tester.view.resetDevicePixelRatio);

        await tester.pumpWidget(_harness());

        final campos = find.byType(TextField);
        await tester.enterText(campos.at(0), 'Nacho');
        await tester.enterText(campos.at(1), 'Iker');
        await tester.enterText(campos.at(2), 'Lucía');
        await tester.pump();

        await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
        await tester.pumpAndSettle();

        await tester.tap(
          find.widgetWithText(FilledButton, 'Gestionar palabras'),
        );
        await tester.pumpAndSettle();

        expect(find.text('GESTION_PALABRAS'), findsOneWidget);
      },
    );

    testWidgets(
      'nombres con espacios internos repetidos se tratan como duplicados',
      (tester) async {
        tester.view.physicalSize = const Size(1200, 3000);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);
        addTearDown(tester.view.resetDevicePixelRatio);

        await tester.pumpWidget(_harness());

        // "Nacho  Lopez" (doble espacio) y "Nacho Lopez" deben colisionar tras
        // normalizar los espacios internos.
        final campos = find.byType(TextField);
        await tester.enterText(campos.at(0), 'Nacho  Lopez');
        await tester.enterText(campos.at(1), 'Nacho Lopez');
        await tester.enterText(campos.at(2), 'Iker');
        await tester.pump();

        await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
        await tester.pump();

        expect(find.text('Hay nombres de jugador repetidos.'), findsOneWidget);
      },
    );
  });
}
