import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/es_un_10_pero/presentation/es_un_10_pero_screen.dart';

import '../../../support/localized_app.dart';

/// Tests de widget de la pantalla "Es un 10 pero".
///
/// Verifican que sacar una carta **cambia el estado** observable de la pantalla
/// (la pista de carta vacía desaparece y el botón pasa de "Sacar carta" a
/// "Sacar otra carta"). La aleatoriedad se fija con `randomProvider.seeded` para
/// que el comportamiento sea determinista. La animación Flame de la carta vive
/// dentro de un `GameWidget` y no se inspecciona aquí: lo que se comprueba es el
/// estado de la UI Flutter que envuelve al juego.
Widget _harness({int seed = 7}) {
  return ProviderScope(
    overrides: [randomProvider.overrideWithValue(RandomProvider.seeded(seed))],
    child: localizedApp(const EsUn10PeroScreen()),
  );
}

void main() {
  group('EsUn10PeroScreen', () {
    testWidgets(
      'arranca sin carta: muestra la pista y el botón "Sacar carta"',
      (tester) async {
        await tester.pumpWidget(_harness());
        await tester.pump();

        expect(
          find.widgetWithText(FilledButton, 'Sacar carta'),
          findsOneWidget,
        );
        expect(
          find.widgetWithText(FilledButton, 'Sacar otra carta'),
          findsNothing,
        );
        // Pista de carta vacía visible mientras no se ha sacado ninguna.
        expect(
          find.text('Pulsa "Sacar carta"\npara revelar una carta'),
          findsOneWidget,
        );
      },
    );

    testWidgets(
      'sacar carta cambia el estado: oculta la pista y cambia el botón',
      (tester) async {
        await tester.pumpWidget(_harness());
        await tester.pump();

        await tester.tap(find.widgetWithText(FilledButton, 'Sacar carta'));
        await tester.pump();

        // El estado cambió: el botón ahora invita a sacar OTRA carta y la pista de
        // carta vacía ha desaparecido.
        expect(
          find.widgetWithText(FilledButton, 'Sacar otra carta'),
          findsOneWidget,
        );
        expect(find.widgetWithText(FilledButton, 'Sacar carta'), findsNothing);
        expect(
          find.text('Pulsa "Sacar carta"\npara revelar una carta'),
          findsNothing,
        );
      },
    );
  });
}
