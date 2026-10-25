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

        // Al pulsar arranca la cuenta atrás de 5s: la carta todavía no se ha
        // revelado, así que hay que dejar pasar la cuenta antes de comprobar el
        // cambio de estado. Se avanza el tiempo en segundos (Timer.periodic).
        for (var i = 0; i < 5; i++) {
          await tester.pump(const Duration(seconds: 1));
        }

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

    testWidgets(
      'la cuenta atrás de 5s deshabilita el botón y oculta la carta; al '
      'terminar la revela y rehabilita el botón',
      (tester) async {
        await tester.pumpWidget(_harness());
        await tester.pump();

        // Estado inicial: botón habilitado, sin overlay de cuenta atrás.
        final botonInicial = tester.widget<FilledButton>(
          find.byType(FilledButton),
        );
        expect(botonInicial.onPressed, isNotNull);
        expect(find.text('Sacando carta en...'), findsNothing);

        // Pulsar arranca la cuenta atrás: el botón queda DESHABILITADO y la
        // carta aún NO está revelada (sigue mostrando "Sacar carta", no "otra").
        await tester.tap(find.widgetWithText(FilledButton, 'Sacar carta'));
        await tester.pump();

        final botonDurante = tester.widget<FilledButton>(
          find.byType(FilledButton),
        );
        expect(
          botonDurante.onPressed,
          isNull,
          reason: 'el botón debe estar deshabilitado durante la cuenta atrás',
        );
        expect(find.text('Sacando carta en...'), findsOneWidget);
        // La carta no se ha revelado todavía: el botón sigue siendo "Sacar
        // carta" (no "Sacar otra carta").
        expect(
          find.widgetWithText(FilledButton, 'Sacar otra carta'),
          findsNothing,
        );

        // Tras agotar los 5 segundos: la carta se revela (botón "Sacar otra
        // carta"), el overlay desaparece y el botón vuelve a estar habilitado.
        for (var i = 0; i < 5; i++) {
          await tester.pump(const Duration(seconds: 1));
        }

        expect(find.text('Sacando carta en...'), findsNothing);
        expect(
          find.widgetWithText(FilledButton, 'Sacar otra carta'),
          findsOneWidget,
        );
        final botonFinal = tester.widget<FilledButton>(
          find.byType(FilledButton),
        );
        expect(
          botonFinal.onPressed,
          isNotNull,
          reason: 'el botón debe rehabilitarse al terminar la cuenta atrás',
        );
      },
    );
  });
}
