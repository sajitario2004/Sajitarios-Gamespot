import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_play_screen.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_setup_screen.dart';

/// Path base del flujo de La Bomba.
const String kBombaBasePath = '/bomba';

/// Nombre de la ruta de configuración (entrada del juego).
const String kBombaSetupRouteName = 'bomba-setup';

/// Nombre de la ruta de juego activo.
const String kBombaPlayRouteName = 'bomba-play';

/// Nombre de la ruta de fin de partida.
const String kBombaGameOverRouteName = 'bomba-game-over';

/// Rutas declarativas del flujo de La Bomba.
///
/// Se importan desde `appRouterProvider` y se añaden a la lista de rutas raíz,
/// manteniendo el menú desacoplado del juego. El estado de la partida vive en
/// `bombaFlowControllerProvider`; las rutas son pantallas sin parámetros que
/// leen ese provider.
List<RouteBase> bombaRoutes() => <RouteBase>[
  GoRoute(
    path: kBombaBasePath,
    name: kBombaSetupRouteName,
    builder: (context, state) => const BombaSetupScreen(),
    routes: [
      GoRoute(
        path: 'play',
        name: kBombaPlayRouteName,
        builder: (context, state) => const BombaPlayScreen(),
      ),
      GoRoute(
        path: 'game-over',
        name: kBombaGameOverRouteName,
        builder: (context, state) => const BombaGameOverScreen(),
      ),
    ],
  ),
];
