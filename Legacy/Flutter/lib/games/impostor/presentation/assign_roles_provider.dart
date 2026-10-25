/// Coordinación entre el dominio puro del Impostor y la capa de datos.
///
/// El use case [AssignRolesUseCase] es puro y no sabe nada de la BD: recibe las
/// palabras ya cargadas. Aquí vive el "pegamento" que lee `wordRepositoryProvider`
/// (palabras desde SQLite) y `randomProvider` (aleatoriedad de la app) para armar
/// una partida real a partir de una [GameConfig].
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/data/impostor_word.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/assign_roles_use_case.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';

/// Error lanzado cuando no hay ninguna palabra en la BD para iniciar partida.
class NoWordsAvailableException implements Exception {
  const NoWordsAvailableException();

  @override
  String toString() =>
      'No hay palabras disponibles para iniciar la partida del Impostor.';
}

/// Provider del use case puro de asignación de roles.
///
/// Inyecta el [RandomProvider] de la app. En tests se puede sobreescribir
/// `randomProvider` con una semilla fija para resultados deterministas.
final assignRolesUseCaseProvider = Provider<AssignRolesUseCase>(
  (ref) => AssignRolesUseCase(ref.watch(randomProvider)),
);

/// Coordinador que arma una partida real desde la base de datos.
///
/// Lee todas las palabras vía [wordRepositoryProvider], las convierte al value
/// object de dominio [Word] y delega en [AssignRolesUseCase] aplicando las
/// reglas 10/10/80. Devuelve la [GameSession] resultante.
///
/// Lanza [NoWordsAvailableException] si la BD no tiene palabras.
class AssignRolesCoordinator {
  AssignRolesCoordinator({
    required WordRepository repository,
    required AssignRolesUseCase useCase,
  }) : _repository = repository,
       _useCase = useCase;

  final WordRepository _repository;
  final AssignRolesUseCase _useCase;

  /// Construye una partida para [config] eligiendo palabra de la BD.
  Future<GameSession> assign(GameConfig config) async {
    final words = await _repository.getAll();
    if (words.isEmpty) {
      throw const NoWordsAvailableException();
    }
    final domainWords = words
        .map((word) => word.toDomain())
        .toList(growable: false);
    return _useCase(config, domainWords);
  }
}

/// Provider del coordinador que arma la partida real desde la BD.
final assignRolesCoordinatorProvider = Provider<AssignRolesCoordinator>(
  (ref) => AssignRolesCoordinator(
    repository: ref.watch(wordRepositoryProvider),
    useCase: ref.watch(assignRolesUseCaseProvider),
  ),
);
