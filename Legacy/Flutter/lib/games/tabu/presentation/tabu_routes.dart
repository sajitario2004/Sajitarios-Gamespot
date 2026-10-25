import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_scoreboard_screen.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_setup_screen.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_turn_screen.dart';

/// Path base del flujo de Tabú.
const String kTabuBasePath = '/tabu';

/// Nombre de la ruta de configuración (entrada del juego).
const String kTabuSetupRouteName = 'tabu-setup';

/// Nombre de la ruta del turno activo.
const String kTabuTurnRouteName = 'tabu-turn';

/// Nombre de la ruta del marcador entre turnos.
const String kTabuScoreboardRouteName = 'tabu-scoreboard';

/// Nombre de la ruta de fin de partida.
const String kTabuGameOverRouteName = 'tabu-game-over';

/// Rutas declarativas del flujo de Tabú.
///
/// Se importan desde `appRouterProvider` y se añaden a la lista de rutas raíz,
/// manteniendo el menú desacoplado del juego. El estado de la partida vive en
/// `tabuFlowControllerProvider`; las rutas son pantallas sin parámetros que
/// leen ese provider.
List<RouteBase> tabuRoutes() => <RouteBase>[
  GoRoute(
    path: kTabuBasePath,
    name: kTabuSetupRouteName,
    builder: (context, state) => const TabuSetupScreen(),
    routes: [
      GoRoute(
        path: 'turn',
        name: kTabuTurnRouteName,
        builder: (context, state) => const TabuTurnScreen(),
      ),
      GoRoute(
        path: 'scoreboard',
        name: kTabuScoreboardRouteName,
        builder: (context, state) => const TabuScoreboardScreen(),
      ),
      GoRoute(
        path: 'game-over',
        name: kTabuGameOverRouteName,
        builder: (context, state) => const TabuGameOverScreen(),
      ),
    ],
  ),
];
