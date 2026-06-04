import 'package:flutter/foundation.dart' show visibleForTesting;
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:path/path.dart' as p;
import 'package:path_provider/path_provider.dart';
import 'package:sqflite/sqflite.dart';

import 'package:sajitarios_gamespot/games/_shared/game_descriptor.dart';
import 'package:sajitarios_gamespot/games/_shared/game_registry.dart';

/// Versión actual del esquema de la base de datos.
///
/// Subir este número cuando algún juego cambie su esquema. Las migraciones
/// concretas viven en cada juego (`GameDescriptor.onUpgradeTables`), no aquí:
/// `AppDatabase` solo orquesta el rango de versiones y delega.
const int kAppDatabaseVersion = 3;

/// Nombre del fichero físico de la base de datos en el dispositivo.
const String kAppDatabaseFileName = 'sajitarios_gamespot.db';

/// Acceso central a la base de datos local (SQLite vía `sqflite`).
///
/// Mantiene una única instancia abierta (singleton) de [Database] durante toda
/// la vida de la app: la primera llamada a [database] crea/abre el fichero,
/// ejecuta `onCreate` la primera vez y `onUpgrade` cuando la versión instalada
/// es menor que [kAppDatabaseVersion]. El resto del código debe pedir la base
/// por aquí (o vía [appDatabaseProvider]) y nunca abrir el fichero por su
/// cuenta.
///
/// `AppDatabase` no conoce el esquema de ningún juego: en `onCreate`/`onUpgrade`
/// recorre los [GameDescriptor] que recibe y delega en
/// `onCreateTables`/`onUpgradeTables`. Así cada juego aporta su propio esquema
/// y migraciones desde su bounded context, sin que `core` dependa de un juego
/// concreto.
class AppDatabase {
  AppDatabase({
    required List<GameDescriptor> descriptors,
    String? fileName,
    int version = kAppDatabaseVersion,
  }) : _descriptors = descriptors,
       _fileName = fileName ?? kAppDatabaseFileName,
       _version = version;

  final List<GameDescriptor> _descriptors;
  final String _fileName;
  final int _version;

  Database? _database;
  Future<Database>? _opening;

  /// Devuelve la base abierta, creándola/migrándola la primera vez.
  ///
  /// Es seguro llamar a este getter de forma concurrente: las llamadas
  /// simultáneas comparten la misma operación de apertura.
  Future<Database> get database {
    final existing = _database;
    if (existing != null) {
      return Future<Database>.value(existing);
    }
    return _opening ??= _open();
  }

  Future<Database> _open() async {
    final directory = await getApplicationDocumentsDirectory();
    final path = p.join(directory.path, _fileName);
    final db = await openDatabase(
      path,
      version: _version,
      onConfigure: _onConfigure,
      onCreate: _onCreate,
      onUpgrade: _onUpgrade,
    );
    _database = db;
    _opening = null;
    return db;
  }

  /// Cierra la base si está abierta. Útil en tests y en teardown.
  Future<void> close() async {
    final db = _database;
    _database = null;
    _opening = null;
    if (db != null) {
      await db.close();
    }
  }

  Future<void> _onConfigure(Database db) async {
    // Activa la verificación de claves foráneas para futuras tablas.
    await db.execute('PRAGMA foreign_keys = ON');
  }

  /// Crea el esquema desde cero (primer arranque) delegando en cada juego.
  Future<void> _onCreate(Database db, int version) async {
    for (final descriptor in _descriptors) {
      await descriptor.onCreateTables(db);
    }
  }

  /// Punto de entrada de [_onUpgrade] para tests de migración con sqflite_ffi.
  ///
  /// Permite pasar la lógica de migración a `OpenDatabaseOptions.onUpgrade` sin
  /// abrir la base por el camino real (path_provider).
  @visibleForTesting
  Future<void> onUpgradeForTest(Database db, int oldVersion, int newVersion) =>
      _onUpgrade(db, oldVersion, newVersion);

  /// Aplica migraciones incrementales desde [oldVersion] hasta [newVersion]
  /// delegando en cada juego, que aplica solo los pasos que le corresponden.
  Future<void> _onUpgrade(Database db, int oldVersion, int newVersion) async {
    for (final descriptor in _descriptors) {
      await descriptor.onUpgradeTables(db, oldVersion, newVersion);
    }
  }
}

/// Provider de Riverpod que expone el [AppDatabase] singleton de la app.
///
/// Inyecta los [GameDescriptor] del `gameRegistryProvider` para que la apertura
/// recorra los juegos y delegue el esquema/migraciones, sin que `core` conozca
/// juegos concretos. En tests se puede sobreescribir con
/// `ProviderScope(overrides: [...])` para inyectar una base en memoria
/// (`databaseFactoryFfi`).
final appDatabaseProvider = Provider<AppDatabase>((ref) {
  final db = AppDatabase(descriptors: ref.watch(gameRegistryProvider));
  ref.onDispose(db.close);
  return db;
});

/// Provider asíncrono que entrega la [Database] ya abierta y migrada.
final databaseProvider = FutureProvider<Database>((ref) {
  return ref.watch(appDatabaseProvider).database;
});
