import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_clue_screen.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_guess_screen.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_pass_device_screen.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_reveal_screen.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_setup_screen.dart';

/// Path base del flujo de Wavelength.
const String kWavelengthBasePath = '/wavelength';

/// Nombre de la ruta de configuración (entrada del juego).
const String kWavelengthSetupRouteName = 'wavelength-setup';

/// Nombre de la ruta de pista (psíquico).
const String kWavelengthClueRouteName = 'wavelength-clue';

/// Nombre de la ruta de paso de dispositivo.
const String kWavelengthPassRouteName = 'wavelength-pass';

/// Nombre de la ruta de adivinanza (grupo).
const String kWavelengthGuessRouteName = 'wavelength-guess';

/// Nombre de la ruta de revelación.
const String kWavelengthRevealRouteName = 'wavelength-reveal';

/// Nombre de la ruta de fin de partida.
const String kWavelengthGameOverRouteName = 'wavelength-game-over';

/// Rutas declarativas del flujo de Wavelength.
///
/// Se importan desde `appRouterProvider` y se añaden a la lista de rutas raíz,
/// manteniendo el menú desacoplado del juego. El estado de la partida vive en
/// [wavelengthFlowControllerProvider]; las rutas son pantallas sin parámetros
/// que leen ese provider.
List<RouteBase> wavelengthRoutes() => <RouteBase>[
  GoRoute(
    path: kWavelengthBasePath,
    name: kWavelengthSetupRouteName,
    builder: (context, state) => const WavelengthSetupScreen(),
    routes: [
      GoRoute(
        path: 'clue',
        name: kWavelengthClueRouteName,
        builder: (context, state) => const WavelengthClueScreen(),
      ),
      GoRoute(
        path: 'pass',
        name: kWavelengthPassRouteName,
        builder: (context, state) => const WavelengthPassDeviceScreen(),
      ),
      GoRoute(
        path: 'guess',
        name: kWavelengthGuessRouteName,
        builder: (context, state) => const WavelengthGuessScreen(),
      ),
      GoRoute(
        path: 'reveal',
        name: kWavelengthRevealRouteName,
        builder: (context, state) => const WavelengthRevealScreen(),
      ),
      GoRoute(
        path: 'game-over',
        name: kWavelengthGameOverRouteName,
        builder: (context, state) => const WavelengthGameOverScreen(),
      ),
    ],
  ),
];
