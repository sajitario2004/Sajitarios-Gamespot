import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:sqflite_common/sqlite_api.dart';

import 'package:sajitarios_gamespot/games/_shared/game_descriptor.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/data/yo_nunca_schema.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/data/yo_nunca_statements_seed_loader.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_routes.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_setup_screen.dart';

/// Descriptor del juego "Yo Nunca" para el catálogo del hub.
///
/// La entrada del juego es la [YoNuncaSetupScreen] (selector de intensidad).
/// El resto del flujo (frases → siguiente) está definido como rutas hijas en
/// [yoNuncaRoutes()] y se coordina con [yoNuncaFlowControllerProvider].
///
/// Nota: el proyecto no localiza [title]/[description] de los GameDescriptor
/// (excepción documentada — metadatos del catálogo). Se mantienen como
/// cadenas españolas directas, siguiendo el precedente de los otros juegos.
class YoNuncaGame extends GameDescriptor {
  const YoNuncaGame();

  @override
  String get id => 'yo_nunca';

  @override
  String get title => 'Yo Nunca';

  @override
  String get description =>
      'Di "yo nunca…" y descubre qué han hecho tus amigos. Bebe si lo has hecho.';

  @override
  IconData get icon => Icons.front_hand_outlined;

  @override
  String get routeName => kYoNuncaSetupRouteName;

  @override
  List<RouteBase> routes() => yoNuncaRoutes();

  @override
  Widget buildEntryScreen(BuildContext context) => const YoNuncaSetupScreen();

  /// Crea el esquema de Yo Nunca (tabla `yo_nunca_statements`) y carga el seed.
  /// Delegado por `AppDatabase.onCreate`.
  @override
  Future<void> onCreateTables(DatabaseExecutor db) async {
    await YoNuncaSchema.createTables(db);
    await const YoNuncaStatementsSeedLoader().seedIfEmpty(db);
  }

  /// Aplica las migraciones de Yo Nunca para cada versión del rango
  /// [oldV]→[newV]. Delegado por `AppDatabase.onUpgrade`.
  @override
  Future<void> onUpgradeTables(DatabaseExecutor db, int oldV, int newV) async {
    for (var v = oldV + 1; v <= newV; v++) {
      await _onUpgradeStep(db, v);
    }
  }

  static Future<void> _onUpgradeStep(DatabaseExecutor db, int v) async {
    switch (v) {
      case 7:
        // Migración v6 → v7: añade la tabla de declaraciones de Yo Nunca a
        // instalaciones existentes, y carga el seed si la tabla está vacía
        // (idempotente).
        await YoNuncaSchema.createTables(db);
        await const YoNuncaStatementsSeedLoader().seedIfEmpty(db);
        break;
      default:
        break;
    }
  }
}
