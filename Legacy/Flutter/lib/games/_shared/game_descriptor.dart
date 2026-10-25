import 'package:flutter/widgets.dart';
import 'package:go_router/go_router.dart';
import 'package:sqflite_common/sqlite_api.dart';

/// Contrato que describe un juego dentro del hub.
///
/// Cada juego del catálogo implementa esta interfaz y se registra en el
/// `GameRegistry`. Es la única dependencia que el menú conoce: el menú nunca
/// importa juegos concretos, solo lee descriptores.
abstract class GameDescriptor {
  const GameDescriptor();

  /// Identificador único y estable del juego (usado en rutas y persistencia).
  String get id;

  /// Título no localizado del juego. Sirve como identificador legible y como
  /// fallback (p. ej. para logs). La UI debe usar [localizedTitle].
  String get title;

  /// Descripción no localizada del juego. Sirve como fallback (p. ej. para
  /// logs). La UI debe usar [localizedDescription].
  String get description;

  /// Título localizado para mostrar en el menú.
  ///
  /// Por defecto devuelve [title]; los juegos que tengan claves en el catálogo
  /// de traducciones sobreescriben este método leyendo `AppLocalizations`.
  String localizedTitle(BuildContext context) => title;

  /// Descripción localizada para la tarjeta del menú.
  ///
  /// Por defecto devuelve [description]; los juegos que tengan claves en el
  /// catálogo de traducciones sobreescriben este método leyendo
  /// `AppLocalizations`.
  String localizedDescription(BuildContext context) => description;

  /// Icono representativo del juego.
  IconData get icon;

  /// Nombre de ruta `go_router` con el que se entra al juego, si el juego
  /// gestiona su flujo mediante rutas declarativas.
  ///
  /// Si es `null` (valor por defecto), el menú entra al juego empujando
  /// imperativamente [buildEntryScreen]. Si no es `null`, el menú navega con
  /// `context.goNamed(routeName)`, integrando el juego en el árbol de
  /// `go_router` (necesario para juegos cuyo flujo navega entre varias rutas,
  /// como el Impostor). El menú sigue sin conocer juegos concretos: solo lee
  /// este campo del descriptor.
  ///
  /// Cuando un juego declara [routeName] debe aportar la ruta correspondiente
  /// (y las del resto de su flujo) sobreescribiendo [routes]: ese es el único
  /// origen de la entrada. En ese caso [buildEntryScreen] no debe ofrecer un
  /// camino divergente (el `builder` de la ruta de entrada construye la misma
  /// pantalla), evitando dos fuentes de verdad.
  String? get routeName => null;

  /// Rutas `go_router` que el juego aporta al árbol de la app.
  ///
  /// `appRouterProvider` recorre el registry y concatena estas rutas, de modo
  /// que el router no conoce juegos concretos. Por defecto vacío: los juegos
  /// que se entran por push imperativo ([routeName] `null`) no aportan rutas.
  List<RouteBase> routes() => const [];

  /// Construye la pantalla de entrada del juego.
  ///
  /// Es la única fuente de verdad de la pantalla de entrada: se usa tanto en el
  /// push imperativo del menú (cuando [routeName] es `null`) como en el
  /// `builder` de la ruta de entrada declarada en [routes] (cuando el juego usa
  /// `go_router`). De este modo no hay dos caminos distintos de construcción.
  Widget buildEntryScreen(BuildContext context);

  /// Aporta el esquema (tablas/índices/seed) que el juego necesita al crear la
  /// base de datos por primera vez.
  ///
  /// `AppDatabase` recorre los descriptores del registry en `onCreate` y llama
  /// a este método de cada uno, de modo que `core` no depende de ningún juego
  /// concreto. Por defecto no-op: los juegos sin persistencia no aportan nada.
  Future<void> onCreateTables(DatabaseExecutor db) async {}

  /// Aplica las migraciones de esquema del juego al subir la versión de la base.
  ///
  /// `AppDatabase` recorre los descriptores del registry en `onUpgrade` y llama
  /// a este método de cada uno con el rango de versiones [oldV]→[newV]. El juego
  /// es responsable de aplicar únicamente los pasos que le correspondan. Por
  /// defecto no-op: los juegos sin persistencia no migran nada.
  Future<void> onUpgradeTables(DatabaseExecutor db, int oldV, int newV) async {}
}
