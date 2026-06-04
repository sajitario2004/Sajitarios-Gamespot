/// Test de integración end-to-end del flujo de "El Impostor".
///
/// Recorre la partida completa **a través de la UI real y el router real**:
/// menú -> setup -> pass -> reveal (de cada jugador) -> results, con datos
/// deterministas. La aleatoriedad se fija con `randomProvider.seeded(...)` y la
/// capa de datos se sustituye por un [FakeWordRepository] con una sola palabra,
/// de modo que la rama probabilística de `AssignRolesUseCase` queda totalmente
/// determinada por la semilla (sin tocar SQLite ni `path_provider`).
///
/// Se ejecuta con `flutter test integration_test` (binding de integración sobre
/// el host de test, sin necesidad de dispositivo) o con `flutter drive` en un
/// dispositivo/simulador.
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/core/routing/app_router.dart';
import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

import 'support/fake_word_repository.dart';

/// Construye la app real (router + theme) con la aleatoriedad y el repositorio
/// de palabras sobreescritos para que la partida sea determinista.
///
/// [seed] fija la semilla de `randomProvider`. Con una sola palabra disponible,
/// `pick` consume una posición fija y la siguiente tirada (`nextDouble`) decide
/// la rama 10/10/80; con `seed: 1` cae en la rama **normal** (un impostor).
Widget _app({required int seed}) {
  return ProviderScope(
    overrides: [
      randomProvider.overrideWithValue(RandomProvider.seeded(seed)),
      wordRepositoryProvider.overrideWithValue(FakeWordRepository.single()),
    ],
    child: Consumer(
      builder: (context, ref, _) {
        final router = ref.watch(appRouterProvider);
        return MaterialApp.router(
          debugShowCheckedModeBanner: false,
          theme: AppTheme.light,
          locale: const Locale('es'),
          localizationsDelegates: AppLocalizations.localizationsDelegates,
          supportedLocales: AppLocalizations.supportedLocales,
          routerConfig: router,
        );
      },
    ),
  );
}

/// Rellena los tres campos de jugador en la [SetupScreen] con [nombres].
Future<void> _rellenarJugadores(
  WidgetTester tester,
  List<String> nombres,
) async {
  final campos = find.byType(TextField);
  expect(campos, findsNWidgets(nombres.length));
  for (var i = 0; i < nombres.length; i++) {
    await tester.enterText(campos.at(i), nombres[i]);
  }
  await tester.pumpAndSettle();
}

/// Avanza una pantalla de revelación: pulsa "Continuar" (pass) -> "Revelar" y
/// devuelve si el rol mostrado fue IMPOSTOR.
Future<bool> _revelarJugadorActual(WidgetTester tester) async {
  // PassDeviceScreen: "Continuar".
  await tester.tap(find.widgetWithText(FilledButton, 'Continuar'));
  await tester.pumpAndSettle();

  // RevealScreen: "Revelar".
  await tester.tap(find.widgetWithText(FilledButton, 'Revelar'));
  await tester.pumpAndSettle();

  final esImpostor = find.text('IMPOSTOR').evaluate().isNotEmpty;

  // "Ocultar y pasar" / "Ocultar y ver resultados": el texto varía en el último
  // jugador, así que buscamos el botón por icono de ocultar.
  final ocultar = find.widgetWithIcon(FilledButton, Icons.visibility_off);
  expect(ocultar, findsOneWidget);
  await tester.tap(ocultar);
  await tester.pumpAndSettle();

  return esImpostor;
}

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('Flujo e2e de El Impostor', () {
    testWidgets(
      'partida 3 jugadores / 1 impostor (sin pista): exactamente uno ve '
      'IMPOSTOR y llega a resultados',
      (tester) async {
        // seed=1 -> rama normal -> exactamente 1 impostor (índice 2: Lucía).
        await tester.pumpWidget(_app(seed: 1));
        await tester.pumpAndSettle();

        // Menú: entrar a "El Impostor".
        expect(find.text('El Impostor'), findsOneWidget);
        await tester.tap(find.text('El Impostor'));
        await tester.pumpAndSettle();

        // SetupScreen: 3 jugadores (el setup arranca con 3 campos), 1 impostor,
        // pista desactivada por defecto.
        await _rellenarJugadores(tester, ['Nacho', 'Iker', 'Lucía']);

        await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
        await tester.pumpAndSettle();

        // Recorremos las 3 revelaciones contando impostores vistos.
        var impostoresVistos = 0;
        for (var i = 0; i < 3; i++) {
          if (await _revelarJugadorActual(tester)) impostoresVistos++;
        }

        // EXACTAMENTE un jugador vio "IMPOSTOR".
        expect(
          impostoresVistos,
          1,
          reason: 'Con 1 impostor configurado, solo uno debe ver IMPOSTOR.',
        );

        // Llegamos a la pantalla de resultados.
        expect(find.text('Resultado de la partida'), findsOneWidget);
        expect(find.text('La palabra era'), findsOneWidget);
        expect(find.text('playa'), findsOneWidget);
        expect(find.text('Había 1 impostor.'), findsOneWidget);
      },
    );

    testWidgets(
      'variante con pista activada: el impostor ve "IMPOSTOR" + la pista',
      (tester) async {
        await tester.pumpWidget(_app(seed: 1));
        await tester.pumpAndSettle();

        await tester.tap(find.text('El Impostor'));
        await tester.pumpAndSettle();

        await _rellenarJugadores(tester, ['Nacho', 'Iker', 'Lucía']);

        // Activar la pista.
        await tester.tap(find.text('Dar pista al impostor'));
        await tester.pumpAndSettle();

        await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
        await tester.pumpAndSettle();

        var vioPista = false;
        var impostoresVistos = 0;
        for (var i = 0; i < 3; i++) {
          // Pass -> Reveal.
          await tester.tap(find.widgetWithText(FilledButton, 'Continuar'));
          await tester.pumpAndSettle();
          await tester.tap(find.widgetWithText(FilledButton, 'Revelar'));
          await tester.pumpAndSettle();

          if (find.text('IMPOSTOR').evaluate().isNotEmpty) {
            impostoresVistos++;
            // Con la pista activada, el impostor ve el rótulo "Pista" y el valor
            // de la pista de la palabra (verano).
            expect(find.text('Pista'), findsOneWidget);
            expect(find.text('verano'), findsOneWidget);
            // Y nunca la palabra.
            expect(find.text('playa'), findsNothing);
            vioPista = true;
          }

          final ocultar = find.widgetWithIcon(
            FilledButton,
            Icons.visibility_off,
          );
          await tester.tap(ocultar);
          await tester.pumpAndSettle();
        }

        expect(impostoresVistos, 1);
        expect(
          vioPista,
          isTrue,
          reason: 'El impostor debe ver la pista cuando está activada.',
        );
        expect(find.text('Resultado de la partida'), findsOneWidget);
      },
    );
  });
}
