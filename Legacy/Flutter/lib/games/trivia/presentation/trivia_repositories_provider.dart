/// Providers de repositorios para el juego de Trivia.
///
/// Expone [QuestionRepository] y [WinnerRepository] respaldados por el
/// [AppDatabase] de la app. Los providers usan [FutureProvider] porque obtener
/// la [Database] abierta es una operación asíncrona.
///
/// En tests se sobreescriben con fakes síncronos usando
/// `overrideWith((ref) => Future.value(fakeRepo))`.
///
/// NOTA: las tablas de trivia se crean en la próxima migración (slice 0.57).
/// Estos providers ya existen y funcionarán en cuanto el esquema esté presente.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/trivia/data/question_repository.dart';
import 'package:sajitarios_gamespot/games/trivia/data/winner_repository.dart';

/// Provider del [QuestionRepository] de Trivia.
///
/// Espera a que [databaseProvider] entregue la [Database] abierta antes de
/// construir el repositorio — el mismo patrón de acceso diferido que usa
/// [WordRepository] internamente con [AppDatabase].
final questionRepositoryProvider = FutureProvider<QuestionRepository>((
  ref,
) async {
  final db = await ref.watch(databaseProvider.future);
  return QuestionRepository(db);
});

/// Provider del [WinnerRepository] de Trivia.
final winnerRepositoryProvider = FutureProvider<WinnerRepository>((ref) async {
  final db = await ref.watch(databaseProvider.future);
  return WinnerRepository(db);
});
