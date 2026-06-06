/// Test de integración end-to-end del flujo de "El Impostor".
///
/// Recorre la partida completa **a través de la UI real y el router real**:
/// menú -> setup -> pass -> reveal (de cada jugador) -> VOTACIÓN -> desenlace,
/// con datos deterministas. La aleatoriedad se fija con `randomProvider.seeded`
/// y la capa de datos se sustituye por dobles in-memory (palabras e historial),
/// de modo que la rama probabilística de `AssignRolesUseCase` queda totalmente
/// determinada por la semilla (sin tocar SQLite ni `path_provider`).
///
/// El nuevo flujo de votación NO muestra una pantalla de resultados con los
/// roles: tras revelar a todos se pasa a la votación y, según el desenlace, se
/// llega a una pantalla de fin de partida que nunca revela las identidades.
///
/// IMPORTANTE sobre las animaciones: el tema NEÓN usa `PulseGlow` (animación
/// infinita) en casi todas las pantallas (menú, setup, pass, reveal...). Por eso
/// NUNCA se usa `pumpAndSettle` (que nunca asentaría): se bombea con duraciones
/// fijas vía [_settle].
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/core/routing/app_router.dart';
import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/games/impostor/data/game_history_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

import 'support/fake_game_history_repository.dart';
import 'support/fake_word_repository.dart';

/// Construye la app real (router + theme) con la aleatoriedad, el repositorio de
/// palabras y el historial sobreescritos para que la partida sea determinista y
/// no toque la plataforma.
Widget _app({required int seed}) {
  return ProviderScope(
    overrides: [
      randomProvider.overrideWithValue(RandomProvider.seeded(seed)),
      wordRepositoryProvider.overrideWithValue(FakeWordRepository.single()),
      gameHistoryRepositoryProvider.overrideWithValue(
        FakeGameHistoryRepository(),
      ),
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

/// Bombea unos cuantos frames con duración fija para dejar avanzar las
/// transiciones de navegación y los `setState` sin esperar a que se "asienten"
/// (imposible con PulseGlow, que anima de forma infinita).
Future<void> _settle(WidgetTester tester) async {
  await tester.pump();
  await tester.pump(const Duration(milliseconds: 350));
  await tester.pump(const Duration(milliseconds: 350));
}

/// Fija una ventana alta y estrecha (tipo móvil pero muy alta) para que el
/// contenido scrollable del setup y de la votación quepa entero en pantalla, de
/// modo que los `tap` no caigan sobre widgets fuera del viewport.
void _usarVentanaAlta(WidgetTester tester) {
  tester.view.physicalSize = const Size(1200, 4000);
  tester.view.devicePixelRatio = 1.0;
  addTearDown(tester.view.resetPhysicalSize);
  addTearDown(tester.view.resetDevicePixelRatio);
}

/// Rellena los campos de jugador en la [SetupScreen] con [nombres].
Future<void> _rellenarJugadores(
  WidgetTester tester,
  List<String> nombres,
) async {
  final campos = find.byType(TextField);
  expect(campos, findsNWidgets(nombres.length));
  for (var i = 0; i < nombres.length; i++) {
    await tester.enterText(campos.at(i), nombres[i]);
  }
  await _settle(tester);
}

/// Añade jugadores con el botón "+" hasta tener [total] campos.
Future<void> _anadirJugadoresHasta(WidgetTester tester, int total) async {
  while (tester.widgetList(find.byType(TextField)).length < total) {
    await tester.tap(
      find.widgetWithIcon(OutlinedButton, Icons.person_add_alt_1),
    );
    await _settle(tester);
  }
}

/// Nombre del jugador cuyo turno es ahora, leído de la pantalla de revelación
/// (aparece "Es el turno de" + nombre). Se llama ya en la RevealScreen, antes de
/// pulsar "Revelar".
String _nombreJugadorEnReveal(WidgetTester tester, List<String> candidatos) {
  for (final nombre in candidatos) {
    if (find.text(nombre).evaluate().isNotEmpty) return nombre;
  }
  fail('No se encontró el nombre del jugador en la pantalla de revelación.');
}

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  group('Flujo e2e de El Impostor', () {
    testWidgets(
      'partida 6 jugadores / 1 impostor: tras revelar pasa a la VOTACIÓN; '
      'pillar al impostor lleva al desenlace "¡Habéis ganado!" sin revelar '
      'roles',
      (tester) async {
        _usarVentanaAlta(tester);
        // seed=1 -> rama normal -> exactamente 1 impostor.
        await tester.pumpWidget(_app(seed: 1));
        await _settle(tester);

        // Menú: entrar a "El Impostor".
        expect(find.text('El Impostor'), findsOneWidget);
        await tester.tap(find.text('El Impostor'));
        await _settle(tester);

        // SetupScreen: 6 jugadores (para tener varias rondas de votación), 1
        // impostor, pista desactivada por defecto.
        final nombres = ['Nacho', 'Iker', 'Lucía', 'Ana', 'Leo', 'Sara'];
        await _anadirJugadoresHasta(tester, nombres.length);
        await _rellenarJugadores(tester, nombres);

        // Subimos las RONDAS al máximo posible (3 para 6 jugadores). El stepper
        // de rondas es el segundo "+" del formulario (el primero es el de
        // impostores). Por defecto el selector arranca en el mínimo.
        for (var i = 0; i < 3; i++) {
          final masRondas = find.widgetWithIcon(IconButton, Icons.add).at(1);
          final boton = tester.widget<IconButton>(masRondas);
          if (boton.onPressed == null) break;
          await tester.tap(masRondas);
          await _settle(tester);
        }
        expect(find.text('Empezar partida'), findsOneWidget);

        await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
        await _settle(tester);

        // Recorremos las 6 revelaciones, anotando el nombre del impostor (lo
        // leemos de la pantalla antes de revelar el rol).
        String? impostor;
        var impostoresVistos = 0;
        for (var i = 0; i < nombres.length; i++) {
          // PassDeviceScreen -> RevealScreen.
          await tester.tap(find.widgetWithText(FilledButton, 'Continuar'));
          await _settle(tester);

          final jugadorActual = _nombreJugadorEnReveal(tester, nombres);

          await tester.tap(find.widgetWithText(FilledButton, 'Revelar'));
          await _settle(tester);

          if (find.text('IMPOSTOR').evaluate().isNotEmpty) {
            impostoresVistos++;
            impostor = jugadorActual;
          }

          final ocultar = find.widgetWithIcon(
            FilledButton,
            Icons.visibility_off,
          );
          await tester.tap(ocultar);
          await _settle(tester);
        }
        expect(
          impostoresVistos,
          1,
          reason: 'Con 1 impostor configurado, solo uno debe ver IMPOSTOR.',
        );
        expect(impostor, isNotNull);

        // Tras revelar a todos NO hay pantalla de resultados con roles: estamos
        // en la VOTACIÓN.
        expect(find.text('Votación'), findsOneWidget);
        expect(find.text('Ronda 1 de 3'), findsOneWidget);
        expect(find.text('Resultado de la partida'), findsNothing);

        // Votamos directamente al impostor real (identificado durante las
        // revelaciones): se le expulsa y, al no quedar impostores, ganan los
        // jugadores y se navega al desenlace.
        await tester.tap(find.text(impostor!));
        await _settle(tester);
        await tester.tap(find.widgetWithText(FilledButton, 'Expulsar'));
        await _settle(tester);

        // En el desenlace NO se revelan roles ni la palabra.
        expect(find.text('¡Habéis ganado!'), findsOneWidget);
        expect(find.text('IMPOSTOR'), findsNothing);
        expect(find.text('playa'), findsNothing);
        expect(find.text('Resultado de la partida'), findsNothing);
      },
    );

    testWidgets(
      'variante con pista activada: el impostor ve "IMPOSTOR" + la pista '
      'durante la revelación; tras revelar a todos se pasa a la votación',
      (tester) async {
        _usarVentanaAlta(tester);
        await tester.pumpWidget(_app(seed: 1));
        await _settle(tester);

        await tester.tap(find.text('El Impostor'));
        await _settle(tester);

        await _rellenarJugadores(tester, ['Nacho', 'Iker', 'Lucía']);

        // Activar la pista.
        await tester.tap(find.text('Dar pista al impostor'));
        await _settle(tester);

        await tester.tap(find.widgetWithText(FilledButton, 'Empezar partida'));
        await _settle(tester);

        var vioPista = false;
        var impostoresVistos = 0;
        for (var i = 0; i < 3; i++) {
          await tester.tap(find.widgetWithText(FilledButton, 'Continuar'));
          await _settle(tester);
          await tester.tap(find.widgetWithText(FilledButton, 'Revelar'));
          await _settle(tester);

          if (find.text('IMPOSTOR').evaluate().isNotEmpty) {
            impostoresVistos++;
            // Con la pista activada, el impostor ve el rótulo "Pista" y el valor
            // de la pista de la palabra (verano). Y nunca la palabra.
            expect(find.text('Pista'), findsOneWidget);
            expect(find.text('verano'), findsOneWidget);
            expect(find.text('playa'), findsNothing);
            vioPista = true;
          }

          final ocultar = find.widgetWithIcon(
            FilledButton,
            Icons.visibility_off,
          );
          await tester.tap(ocultar);
          await _settle(tester);
        }

        expect(impostoresVistos, 1);
        expect(
          vioPista,
          isTrue,
          reason: 'El impostor debe ver la pista cuando está activada.',
        );

        // Tras revelar a todos se pasa a la votación (sin pantalla de roles).
        expect(find.text('Votación'), findsOneWidget);
        expect(find.text('Resultado de la partida'), findsNothing);
      },
    );
  });
}
