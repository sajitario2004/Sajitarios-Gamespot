import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:sqflite_common/sqlite_api.dart';

import 'package:sajitarios_gamespot/games/_shared/game_descriptor.dart';
import 'package:sajitarios_gamespot/games/bomba/data/bomba_schema.dart';
import 'package:sajitarios_gamespot/games/bomba/data/bomba_seed_loader.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_routes.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_setup_screen.dart';

/// Descriptor del juego "La Bomba" para el catálogo del hub.
///
/// La entrada del juego es la [BombaSetupScreen] (configuración de la partida).
/// El resto del flujo (juego → game-over) está definido como rutas hijas en
/// [bombaRoutes()] y se coordina con [bombaFlowControllerProvider].
///
/// Nota: el proyecto no localiza [title]/[description] de los GameDescriptor
/// (excepción documentada — metadatos del catálogo). Se mantienen como
/// cadenas españolas directas, siguiendo el precedente de los otros juegos.
class BombaGame extends GameDescriptor {
  const BombaGame();

  @override
  String get id => 'bomba';

  @override
  String get title => 'La Bomba';

  @override
  String get description =>
      'Di palabras con la sílaba o categoría antes de que explote la bomba. '
      '¡El que la tiene cuando explota queda eliminado!';

  @override
  IconData get icon => Icons.local_fire_department;

  @override
  String get routeName => kBombaSetupRouteName;

  @override
  List<RouteBase> routes() => bombaRoutes();

  @override
  Widget buildEntryScreen(BuildContext context) => const BombaSetupScreen();

  /// Crea el esquema de La Bomba (tablas `bomba_silabas` y `bomba_categorias`)
  /// y carga el seed inicial. Delegado por `AppDatabase.onCreate`.
  @override
  Future<void> onCreateTables(DatabaseExecutor db) async {
    await BombaSchema.createTables(db);
    await const BombaSeedLoader().seedAllIfEmpty(db);
  }

  /// Aplica las migraciones de La Bomba para cada versión del rango
  /// [oldV]→[newV]. Delegado por `AppDatabase.onUpgrade`.
  @override
  Future<void> onUpgradeTables(DatabaseExecutor db, int oldV, int newV) async {
    for (var v = oldV + 1; v <= newV; v++) {
      await _onUpgradeStep(db, v);
    }
  }

  static Future<void> _onUpgradeStep(DatabaseExecutor db, int v) async {
    switch (v) {
      case 8:
        // Migración v7 → v8: añade las tablas de La Bomba a instalaciones
        // existentes y carga el seed si las tablas están vacías (idempotente).
        await BombaSchema.createTables(db);
        await const BombaSeedLoader().seedAllIfEmpty(db);
        break;
      default:
        break;
    }
  }
}
