import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:sqflite_common/sqlite_api.dart';

import 'package:sajitarios_gamespot/games/_shared/game_descriptor.dart';
import 'package:sajitarios_gamespot/games/wavelength/data/wavelength_schema.dart';
import 'package:sajitarios_gamespot/games/wavelength/data/wavelength_spectra_seed_loader.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_routes.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_setup_screen.dart';

/// Descriptor del juego "Wavelength" para el catálogo del hub.
///
/// La entrada del juego es la [WavelengthSetupScreen] (configuración de la
/// partida). El resto del flujo (clue → pass → guess → reveal → game-over)
/// está definido como rutas hijas en [wavelengthRoutes()] y se coordina con
/// [wavelengthFlowControllerProvider].
///
/// Nota: el proyecto no localiza [title]/[description] de los GameDescriptor
/// (excepción documentada — metadatos del catálogo). Se mantienen como
/// cadenas en inglés/español directas, siguiendo el precedente de los otros
/// juegos.
class WavelengthGame extends GameDescriptor {
  const WavelengthGame();

  @override
  String get id => 'wavelength';

  @override
  String get title => 'Wavelength';

  @override
  String get description =>
      'El psíquico da una pista; el grupo mueve el dial para acertar el objetivo oculto.';

  @override
  IconData get icon => Icons.tune;

  @override
  String get routeName => kWavelengthSetupRouteName;

  @override
  List<RouteBase> routes() => wavelengthRoutes();

  @override
  Widget buildEntryScreen(BuildContext context) =>
      const WavelengthSetupScreen();

  /// Crea el esquema de Wavelength (tabla `wavelength_spectra`) y carga el
  /// seed inicial. Delegado por `AppDatabase.onCreate`.
  @override
  Future<void> onCreateTables(DatabaseExecutor db) async {
    await WavelengthSchema.createTables(db);
    await const WavelengthSpectraSeedLoader().seedIfEmpty(db);
  }

  /// Aplica las migraciones de Wavelength para cada versión del rango
  /// [oldV]→[newV]. Delegado por `AppDatabase.onUpgrade`.
  @override
  Future<void> onUpgradeTables(DatabaseExecutor db, int oldV, int newV) async {
    for (var v = oldV + 1; v <= newV; v++) {
      await _onUpgradeStep(db, v);
    }
  }

  static Future<void> _onUpgradeStep(DatabaseExecutor db, int v) async {
    switch (v) {
      case 5:
        // Migración v4 → v5: añade la tabla de espectros de Wavelength a
        // instalaciones existentes, y carga el seed si la tabla está vacía
        // (idempotente).
        await WavelengthSchema.createTables(db);
        await const WavelengthSpectraSeedLoader().seedIfEmpty(db);
        break;
      default:
        break;
    }
  }
}
