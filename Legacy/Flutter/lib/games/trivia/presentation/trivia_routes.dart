import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_game_over_screen.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_pass_device_screen.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_question_screen.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_setup_screen.dart';

/// Path base del flujo de Trivia.
const String kTriviaBasePath = '/trivia';

/// Nombre de la ruta de configuración (entrada del juego).
const String kTriviaSetupRouteName = 'trivia-setup';

/// Nombre de la ruta "pásale el móvil".
const String kTriviaPassRouteName = 'trivia-pass';

/// Nombre de la ruta de la pregunta activa.
const String kTriviaQuestionRouteName = 'trivia-question';

/// Nombre de la ruta de fin de partida.
const String kTriviaGameOverRouteName = 'trivia-game-over';

/// Rutas declarativas del flujo de Trivia.
///
/// Se importan desde `appRouterProvider` y se añaden a la lista de rutas raíz,
/// manteniendo el menú desacoplado del juego. El estado de la partida vive en
/// `triviaFlowControllerProvider`; las rutas son pantallas sin parámetros que
/// leen ese provider.
List<RouteBase> triviaRoutes() => <RouteBase>[
  GoRoute(
    path: kTriviaBasePath,
    name: kTriviaSetupRouteName,
    builder: (context, state) => const TriviaSetupScreen(),
    routes: [
      GoRoute(
        path: 'pass',
        name: kTriviaPassRouteName,
        builder: (context, state) => const TriviaPassDeviceScreen(),
      ),
      GoRoute(
        path: 'question',
        name: kTriviaQuestionRouteName,
        builder: (context, state) => const TriviaQuestionScreen(),
      ),
      GoRoute(
        path: 'game-over',
        name: kTriviaGameOverRouteName,
        builder: (context, state) => const TriviaGameOverScreen(),
      ),
    ],
  ),
];
