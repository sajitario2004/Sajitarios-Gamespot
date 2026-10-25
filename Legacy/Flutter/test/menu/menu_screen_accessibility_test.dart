import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/games/_shared/game_descriptor.dart';
import 'package:sajitarios_gamespot/games/_shared/game_registry.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';
import 'package:sajitarios_gamespot/menu/menu_screen.dart';

/// Descriptor de juego mínimo para tests del menú sin depender del catálogo
/// real (que arrastra Flame / SQLite).
class _FakeGame extends GameDescriptor {
  const _FakeGame();

  @override
  String get id => 'fake';

  @override
  String get title => 'Juego de Prueba';

  @override
  String get description => 'Una descripción de prueba.';

  @override
  IconData get icon => Icons.videogame_asset;

  @override
  Widget buildEntryScreen(BuildContext context) =>
      const Scaffold(body: Text('ENTRADA'));
}

Widget _harness() {
  return ProviderScope(
    overrides: [
      gameRegistryProvider.overrideWithValue(const <GameDescriptor>[
        _FakeGame(),
      ]),
    ],
    child: const MaterialApp(
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      locale: Locale('es'),
      home: MenuScreen(),
    ),
  );
}

void main() {
  group('MenuScreen accesibilidad', () {
    testWidgets(
      'cada tarjeta de juego expone un Semantics de botón con título y '
      'descripción',
      (tester) async {
        await tester.pumpWidget(_harness());
        // El título del menú usa un PulseGlow (animación infinita decorativa);
        // `pumpAndSettle` no terminaría. Bombeamos unos frames en su lugar.
        await tester.pump();
        await tester.pump(const Duration(milliseconds: 300));

        // El lector de pantalla anuncia un único nodo accionable por juego.
        expect(
          find.bySemanticsLabel(
            'Jugar a Juego de Prueba. Una descripción de prueba.',
          ),
          findsOneWidget,
        );
      },
    );

    testWidgets('el menú no desborda con texto grande (textScaler 2.0)', (
      tester,
    ) async {
      await tester.pumpWidget(
        ProviderScope(
          overrides: [
            gameRegistryProvider.overrideWithValue(const <GameDescriptor>[
              _FakeGame(),
              _FakeGame(),
            ]),
          ],
          child: const MaterialApp(
            localizationsDelegates: AppLocalizations.localizationsDelegates,
            supportedLocales: AppLocalizations.supportedLocales,
            locale: Locale('es'),
            home: MediaQuery(
              data: MediaQueryData(textScaler: TextScaler.linear(2.0)),
              child: MenuScreen(),
            ),
          ),
        ),
      );
      // PulseGlow del título: animación infinita decorativa, no se asienta.
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 300));

      // Si hubiera overflow, el test fallaría con un error de render.
      expect(tester.takeException(), isNull);
      expect(find.text('Juego de Prueba'), findsNWidgets(2));
    });
  });
}
