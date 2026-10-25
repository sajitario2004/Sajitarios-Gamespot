import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_play_screen.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_setup_screen.dart';

/// Path base del flujo de Yo Nunca.
const String kYoNuncaBasePath = '/yo-nunca';

/// Nombre de la ruta de configuración (entrada del juego).
const String kYoNuncaSetupRouteName = 'yo-nunca-setup';

/// Nombre de la ruta de juego activo.
const String kYoNuncaPlayRouteName = 'yo-nunca-play';

/// Rutas declarativas del flujo de Yo Nunca.
///
/// Se importan desde `appRouterProvider` y se añaden a la lista de rutas raíz,
/// manteniendo el menú desacoplado del juego. El estado de la partida vive en
/// [yoNuncaFlowControllerProvider]; las rutas son pantallas sin parámetros que
/// leen ese provider.
List<RouteBase> yoNuncaRoutes() => <RouteBase>[
  GoRoute(
    path: kYoNuncaBasePath,
    name: kYoNuncaSetupRouteName,
    builder: (context, state) => const YoNuncaSetupScreen(),
    routes: [
      GoRoute(
        path: 'play',
        name: kYoNuncaPlayRouteName,
        builder: (context, state) => const YoNuncaPlayScreen(),
      ),
    ],
  ),
];
