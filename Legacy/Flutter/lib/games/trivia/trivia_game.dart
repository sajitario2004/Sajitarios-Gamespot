import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:sqflite_common/sqlite_api.dart';

import 'package:sajitarios_gamespot/games/_shared/game_descriptor.dart';
import 'package:sajitarios_gamespot/games/trivia/data/trivia_schema.dart';
import 'package:sajitarios_gamespot/games/trivia/data/trivia_questions_seed_loader.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_routes.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_setup_screen.dart';

/// Descriptor del juego "Preguntas por puntos" (Trivia) para el catálogo del hub.
///
/// La entrada del juego es la [TriviaSetupScreen] (configuración de la partida).
/// El resto del flujo (pass → question → game-over) está definido como rutas
/// hijas en `triviaRoutes()` y se coordina con `triviaFlowControllerProvider`.
///
/// Nota: el proyecto no localiza [title]/[description] de los GameDescriptor
/// (excepción documentada — metadatos del catálogo). Se mantienen como
/// cadenas españolas directas, siguiendo el precedente de [ImpostorGame].
class TriviaGame extends GameDescriptor {
  const TriviaGame();

  @override
  String get id => 'trivia';

  @override
  String get title => 'Preguntas por puntos';

  @override
  String get description =>
      'Responde preguntas de cultura general y otras temáticas. ¡El que más acierte gana!';

  @override
  IconData get icon => Icons.quiz;

  @override
  String get routeName => kTriviaSetupRouteName;

  @override
  List<RouteBase> routes() => triviaRoutes();

  @override
  Widget buildEntryScreen(BuildContext context) => const TriviaSetupScreen();

  /// Crea el esquema de Trivia (tablas `trivia_questions` + `trivia_winners`,
  /// sus índices) y carga el seed inicial. Delegado por `AppDatabase.onCreate`.
  @override
  Future<void> onCreateTables(DatabaseExecutor db) async {
    await TriviaSchema.createTables(db);
    await const TriviaQuestionsSeedLoader().seedIfEmpty(db);
  }

  /// Aplica las migraciones de Trivia para cada versión del rango
  /// [oldV]→[newV]. Delegado por `AppDatabase.onUpgrade`.
  @override
  Future<void> onUpgradeTables(DatabaseExecutor db, int oldV, int newV) async {
    for (var v = oldV + 1; v <= newV; v++) {
      await _onUpgradeStep(db, v);
    }
  }

  static Future<void> _onUpgradeStep(DatabaseExecutor db, int v) async {
    switch (v) {
      case 4:
        // Migración v3 → v4: añade las tablas de Trivia a instalaciones
        // existentes, y carga el seed si la tabla está vacía (idempotente).
        await TriviaSchema.createTables(db);
        await const TriviaQuestionsSeedLoader().seedIfEmpty(db);
        break;
      default:
        break;
    }
  }
}
