import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/games/bomba/bomba_game.dart';
import 'package:sajitarios_gamespot/games/es_un_10_pero/es_un_10_pero_game.dart';
import 'package:sajitarios_gamespot/games/impostor/impostor_game.dart';
import 'package:sajitarios_gamespot/games/tabu/tabu_game.dart';
import 'package:sajitarios_gamespot/games/trivia/trivia_game.dart';
import 'package:sajitarios_gamespot/games/wavelength/wavelength_game.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/yo_nunca_game.dart';

import 'game_descriptor.dart';

/// Catálogo de juegos disponibles en el hub.
///
/// Para añadir un juego nuevo basta con crear su `GameDescriptor` en
/// `lib/games/<juego>/` y agregarlo a esta lista. El `MenuScreen` se actualiza
/// solo: nunca conoce juegos concretos, únicamente lee este provider.
final gameRegistryProvider = Provider<List<GameDescriptor>>((ref) {
  return const <GameDescriptor>[
    EsUn10PeroGame(),
    ImpostorGame(),
    TriviaGame(),
    WavelengthGame(),
    TabuGame(),
    YoNuncaGame(),
    BombaGame(),
  ];
});
