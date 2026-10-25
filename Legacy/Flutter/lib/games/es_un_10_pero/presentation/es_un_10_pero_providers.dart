import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/es_un_10_pero/domain/draw_card_use_case.dart';

/// Provider del [DrawCardUseCase] del juego "Es un 10 pero".
///
/// Inyecta el [RandomProvider] global de la app, de modo que toda la
/// aleatoriedad pasa por la misma fuente y puede sobreescribirse en tests con
/// `ProviderScope(overrides: [randomProvider.overrideWithValue(...)])`.
final drawCardUseCaseProvider = Provider<DrawCardUseCase>(
  (ref) => DrawCardUseCase(ref.watch(randomProvider)),
);
