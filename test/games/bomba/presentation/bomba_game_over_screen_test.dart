/// Tests de la BombaGameOverScreen.
///
/// Cubre: muestra el ganador, el botón "Volver al menú", estado sin partida.
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/bomba/data/bomba_prompt_repository.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_flow_controller.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_repositories_provider.dart';

import '../../../support/localized_app.dart';
import 'support/fake_bomba_prompt_repository.dart';

/// Controlador que arranca directamente en gameOver para tests.
/// La sesión tiene un único jugador vivo — ese es el ganador.
class _GameOverController extends BombaFlowController {
  _GameOverController({required this.winnerName});

  final String winnerName;

  @override
  BombaFlowState build() {
    final configResult = BombaConfig.create(
      mode: BombaMode.silaba,
      playerNames: [winnerName, 'Eliminado'],
      minSegundos: 10,
      maxSegundos: 60,
    );
    final config = configResult.config!;
    final rng = RandomProvider.seeded(0);
    // Creamos la sesión con 2 jugadores y eliminamos al segundo para que
    // solo quede el ganador.
    final initial = BombaSession.start(config, rng);
    // holder=0 (winnerName), pasar → holder=1 (Eliminado), explode → queda winnerName.
    final afterPass = initial.pasar();
    final afterExplosion = afterPass.explode();

    return BombaFlowState(
      fase: BombaFase.gameOver,
      config: config,
      session: afterExplosion,
    );
  }
}

Widget _harness(String winner, BombaPromptRepository repo) {
  return ProviderScope(
    overrides: [
      randomProvider.overrideWithValue(RandomProvider.seeded(0)),
      bombaPromptRepositoryProvider.overrideWith((_) async => repo),
      bombaFlowControllerProvider.overrideWith(
        () => _GameOverController(winnerName: winner),
      ),
    ],
    child: localizedApp(const BombaGameOverScreen()),
  );
}

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('BombaGameOverScreen', () {
    late BombaPromptRepository repo;
    setUp(() async => repo = await makeFakeRepo());

    testWidgets('muestra el nombre del ganador', (tester) async {
      await tester.pumpWidget(_harness('Marta', repo));
      await tester.pump();

      expect(find.text('Marta'), findsOneWidget);
    });

    testWidgets('muestra el título de fin de partida', (tester) async {
      await tester.pumpWidget(_harness('Carlos', repo));
      await tester.pump();

      expect(find.text('Fin de la partida'), findsOneWidget);
    });

    testWidgets('muestra la etiqueta Ganador', (tester) async {
      await tester.pumpWidget(_harness('Pedro', repo));
      await tester.pump();

      expect(find.text('Ganador'), findsOneWidget);
    });

    testWidgets('tiene el botón Volver al menú', (tester) async {
      await tester.pumpWidget(_harness('Ana', repo));
      await tester.pump();

      expect(find.text('Volver al menú'), findsOneWidget);
    });

    testWidgets('sin partida activa muestra mensaje de no hay partida', (
      tester,
    ) async {
      // ProviderScope sin override del controller → estado inicial = setup,
      // winner == null → la pantalla muestra el mensaje de guardia.
      await tester.pumpWidget(
        ProviderScope(
          overrides: [
            randomProvider.overrideWithValue(RandomProvider.seeded(0)),
            bombaPromptRepositoryProvider.overrideWith((_) async => repo),
          ],
          child: localizedApp(const BombaGameOverScreen()),
        ),
      );
      await tester.pump();

      expect(find.text('No hay ninguna partida en curso.'), findsOneWidget);
    });
  });
}
