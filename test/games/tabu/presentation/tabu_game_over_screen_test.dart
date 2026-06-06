/// Tests de widget para [TabuGameOverScreen].
///
/// Verifica que al llegar al objetivo de victorias se muestra la pantalla de
/// fin de partida con el nombre del equipo ganador.
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_flow_controller.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_repositories_provider.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

import '../../../support/localized_app.dart';
import 'support/fake_tabu_word_repository.dart';

// ─── Helpers ──────────────────────────────────────────────────────────────────

/// Construye una [TabuConfig] con objetivo de 1 victoria para que la partida
/// termine rápidamente en los tests.
TabuConfig _config() => TabuConfig.create(
  equipoA: 'Rojos',
  equipoB: 'Azules',
  turnoSegundos: 30,
  objetivoVictorias: 1,
).config!;

/// Avanza el controlador hasta [TabuFase.gameOver] simulando:
/// 1. iniciar() — carga palabras y va a [TabuFase.turno].
/// 2. acierto() — equipo A acumula >= 1 acierto en el turno.
/// 3. terminarTurno() — aplica la regla de ronda; con objetivo=1, la partida
///    termina y el equipo A gana.
Future<void> _llegarAGameOver(WidgetRef ref) async {
  await ref.read(tabuFlowControllerProvider.notifier).iniciar(_config());
  ref.read(tabuFlowControllerProvider.notifier).acierto();
  ref.read(tabuFlowControllerProvider.notifier).terminarTurno();
}

void main() {
  group('TabuGameOverScreen', () {
    testWidgets(
      'muestra el equipo ganador cuando el equipo A llega al objetivo',
      (tester) async {
        late WidgetRef capturedRef;

        await tester.pumpWidget(
          ProviderScope(
            overrides: [
              tabuWordRepositoryProvider.overrideWith(
                (ref) => Future.value(buildFakeTabuRepo(count: 20)),
              ),
              randomProvider.overrideWithValue(RandomProvider.seeded(42)),
            ],
            child: Consumer(
              builder: (context, ref, _) {
                capturedRef = ref;
                return localizedApp(const TabuGameOverScreen());
              },
            ),
          ),
        );

        await tester.pump();

        // Llevar el controlador a gameOver.
        await _llegarAGameOver(capturedRef);
        await tester.pump();

        // La pantalla debe mostrar la etiqueta del ganador.
        expect(find.text('Equipo ganador'), findsOneWidget);
        // "Rojos" aparece como ganador (puede ser en múltiples widgets).
        expect(find.text('Rojos'), findsAtLeastNWidgets(1));
      },
    );

    testWidgets('"Volver al menú" llama reiniciar() y navega al menú', (
      tester,
    ) async {
      late WidgetRef capturedRef;

      // GoRouter mínimo para que context.go('/') no lance.
      final router = GoRouter(
        initialLocation: '/tabu/game-over',
        routes: [
          GoRoute(
            path: '/',
            builder: (ctx, st) => const Scaffold(body: Text('menu')),
          ),
          GoRoute(
            path: '/tabu',
            name: 'tabu-setup',
            builder: (ctx, st) => const Scaffold(body: Text('setup')),
            routes: [
              GoRoute(
                path: 'game-over',
                name: 'tabu-game-over',
                builder: (ctx, st) => const TabuGameOverScreen(),
              ),
            ],
          ),
        ],
      );

      await tester.pumpWidget(
        ProviderScope(
          overrides: [
            tabuWordRepositoryProvider.overrideWith(
              (ref) => Future.value(buildFakeTabuRepo(count: 20)),
            ),
            randomProvider.overrideWithValue(RandomProvider.seeded(42)),
          ],
          child: Consumer(
            builder: (context, ref, _) {
              capturedRef = ref;
              return MaterialApp.router(
                locale: const Locale('es'),
                localizationsDelegates: AppLocalizations.localizationsDelegates,
                supportedLocales: AppLocalizations.supportedLocales,
                routerConfig: router,
              );
            },
          ),
        ),
      );

      await tester.pump();

      // Llevar el controlador a gameOver.
      await _llegarAGameOver(capturedRef);
      await tester.pump();

      // Confirmar que estamos en gameOver.
      expect(
        capturedRef.read(tabuFlowControllerProvider).fase,
        TabuFase.gameOver,
      );

      await tester.tap(find.text('Volver al menú'));
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 100));

      // El controlador debe haber vuelto a setup tras reiniciar().
      expect(capturedRef.read(tabuFlowControllerProvider).fase, TabuFase.setup);
    });
  });
}
