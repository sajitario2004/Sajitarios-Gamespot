import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/impostor/presentation/game_over_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/history_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/pass_device_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/reveal_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/setup_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/voting_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/words_management_screen.dart';

/// Path base del flujo del Impostor.
const String kImpostorBasePath = '/impostor';

/// Nombre de la ruta de configuración (entrada del juego).
const String kImpostorSetupRouteName = 'impostor-setup';

/// Nombre de la ruta de gestión (CRUD) de palabras.
const String kImpostorWordsRouteName = 'impostor-words';

/// Nombre de la ruta "pásale el móvil".
const String kImpostorPassRouteName = 'impostor-pass';

/// Nombre de la ruta de revelación.
const String kImpostorRevealRouteName = 'impostor-reveal';

/// Nombre de la ruta de votación (tras revelar todos los roles).
const String kImpostorVotingRouteName = 'impostor-voting';

/// Nombre de la ruta de desenlace (fin de partida SIN revelar roles).
const String kImpostorGameOverRouteName = 'impostor-game-over';

/// Nombre de la ruta de historial y estadísticas.
const String kImpostorHistoryRouteName = 'impostor-history';

/// Rutas declarativas del flujo del Impostor.
///
/// Se importan desde `appRouterProvider` (en `lib/core/routing/app_router.dart`)
/// y se añaden a la lista de rutas raíz, manteniendo el menú desacoplado del
/// juego. El estado de la partida vive en `impostorFlowControllerProvider`; las
/// rutas son pantallas sin parámetros que leen ese provider.
List<RouteBase> impostorRoutes() => <RouteBase>[
  GoRoute(
    path: kImpostorBasePath,
    name: kImpostorSetupRouteName,
    builder: (context, state) => const SetupScreen(),
    routes: [
      GoRoute(
        path: 'pass',
        name: kImpostorPassRouteName,
        builder: (context, state) => const PassDeviceScreen(),
      ),
      GoRoute(
        path: 'reveal',
        name: kImpostorRevealRouteName,
        builder: (context, state) => const RevealScreen(),
      ),
      GoRoute(
        path: 'voting',
        name: kImpostorVotingRouteName,
        builder: (context, state) => const VotingScreen(),
      ),
      GoRoute(
        path: 'game-over',
        name: kImpostorGameOverRouteName,
        builder: (context, state) => const GameOverScreen(),
      ),
      GoRoute(
        path: 'words',
        name: kImpostorWordsRouteName,
        builder: (context, state) => const WordsManagementScreen(),
      ),
      GoRoute(
        path: 'history',
        name: kImpostorHistoryRouteName,
        builder: (context, state) => const HistoryScreen(),
      ),
    ],
  ),
];
