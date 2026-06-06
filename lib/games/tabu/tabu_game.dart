import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:sqflite_common/sqlite_api.dart';

import 'package:sajitarios_gamespot/games/_shared/game_descriptor.dart';
import 'package:sajitarios_gamespot/games/tabu/data/tabu_schema.dart';
import 'package:sajitarios_gamespot/games/tabu/data/tabu_words_seed_loader.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_routes.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_setup_screen.dart';

/// Descriptor del juego "Tabú" para el catálogo del hub.
///
/// La entrada del juego es la [TabuSetupScreen] (configuración de la partida).
/// El resto del flujo (turno → marcador → fin) está definido como rutas hijas
/// en [tabuRoutes()] y se coordina con [tabuFlowControllerProvider].
///
/// Nota: el proyecto no localiza [title]/[description] de los GameDescriptor
/// (excepción documentada — metadatos del catálogo). Se mantienen como
/// cadenas españolas directas, siguiendo el precedente de los otros juegos.
class TabuGame extends GameDescriptor {
  const TabuGame();

  @override
  String get id => 'tabu';

  @override
  String get title => 'Tabú';

  @override
  String get description =>
      'Describe la palabra sin decir las palabras prohibidas. ¡Haz que tu equipo adivine!';

  @override
  IconData get icon => Icons.do_not_disturb_on_outlined;

  @override
  String get routeName => kTabuSetupRouteName;

  @override
  List<RouteBase> routes() => tabuRoutes();

  @override
  Widget buildEntryScreen(BuildContext context) => const TabuSetupScreen();

  /// Crea el esquema de Tabú (tabla `tabu_words`) y carga el seed inicial.
  /// Delegado por `AppDatabase.onCreate`.
  @override
  Future<void> onCreateTables(DatabaseExecutor db) async {
    await TabuSchema.createTables(db);
    await const TabuWordsSeedLoader().seedIfEmpty(db);
  }

  /// Aplica las migraciones de Tabú para cada versión del rango
  /// [oldV]→[newV]. Delegado por `AppDatabase.onUpgrade`.
  @override
  Future<void> onUpgradeTables(DatabaseExecutor db, int oldV, int newV) async {
    for (var v = oldV + 1; v <= newV; v++) {
      await _onUpgradeStep(db, v);
    }
  }

  static Future<void> _onUpgradeStep(DatabaseExecutor db, int v) async {
    switch (v) {
      case 6:
        // Migración v5 → v6: añade la tabla de palabras de Tabú a
        // instalaciones existentes, y carga el seed si la tabla está vacía
        // (idempotente).
        await TabuSchema.createTables(db);
        await const TabuWordsSeedLoader().seedIfEmpty(db);
        break;
      default:
        break;
    }
  }
}
