/// Providers de repositorios para el juego La Bomba.
///
/// Expone [BombaPromptRepository] respaldado por el [AppDatabase] de la app.
/// El provider usa [FutureProvider] porque obtener la [Database] abierta es
/// una operación asíncrona.
///
/// En tests se sobreescribe con un fake síncrono usando
/// `overrideWith((ref) => Future.value(fakeRepo))`.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/bomba/data/bomba_prompt_repository.dart';

/// Provider del [BombaPromptRepository] de La Bomba.
///
/// Espera a que [databaseProvider] entregue la [Database] abierta antes de
/// construir el repositorio — el mismo patrón de acceso diferido que usan
/// los repositorios de Tabú y Trivia.
final bombaPromptRepositoryProvider = FutureProvider<BombaPromptRepository>((
  ref,
) async {
  final db = await ref.watch(databaseProvider.future);
  return BombaPromptRepository(db);
});
