import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:sqflite_common/sqlite_api.dart';

import 'package:sajitarios_gamespot/games/_shared/game_descriptor.dart';
import 'package:sajitarios_gamespot/games/impostor/data/impostor_schema.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_routes.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/setup_screen.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Descriptor del juego "El Impostor" para el catálogo del hub.
///
/// La entrada del juego es la [SetupScreen] (configuración de la partida). El
/// resto del flujo (pass -> reveal -> results) está definido como rutas hijas en
/// `impostorRoutes()` y se coordina con `impostorFlowControllerProvider`.
class ImpostorGame extends GameDescriptor {
  const ImpostorGame();

  @override
  String get id => 'impostor';

  @override
  String get title => 'El Impostor';

  @override
  String get description =>
      'Todos conocen la palabra... menos los impostores. ¿Los descubrirás?';

  @override
  String localizedTitle(BuildContext context) =>
      AppLocalizations.of(context)!.impostorMenuTitulo;

  @override
  String localizedDescription(BuildContext context) =>
      AppLocalizations.of(context)!.impostorMenuDescripcion;

  @override
  IconData get icon => Icons.theater_comedy;

  /// El Impostor gestiona su flujo (setup -> pass -> reveal -> results) con
  /// `go_router`, así que el menú entra por nombre de ruta y no por push
  /// imperativo. Así `context.goNamed(...)` dentro de las pantallas opera sobre
  /// el árbol correcto del router.
  @override
  String get routeName => kImpostorSetupRouteName;

  /// Aporta el flujo completo (setup -> pass -> reveal -> results + CRUD de
  /// palabras) al árbol de `go_router`. `appRouterProvider` concatena estas
  /// rutas sin conocer el juego.
  @override
  List<RouteBase> routes() => impostorRoutes();

  @override
  Widget buildEntryScreen(BuildContext context) => const SetupScreen();

  /// Crea el esquema del Impostor (tablas `impostor_words` + `game_history`,
  /// sus índices) y carga el seed inicial. Delegado por `AppDatabase.onCreate`.
  @override
  Future<void> onCreateTables(DatabaseExecutor db) =>
      ImpostorSchema.onCreate(db);

  /// Aplica las migraciones del Impostor para cada versión del rango
  /// [oldV]→[newV]. Delegado por `AppDatabase.onUpgrade`.
  @override
  Future<void> onUpgradeTables(DatabaseExecutor db, int oldV, int newV) async {
    for (var v = oldV + 1; v <= newV; v++) {
      await ImpostorSchema.onUpgradeStep(db, v);
    }
  }
}
