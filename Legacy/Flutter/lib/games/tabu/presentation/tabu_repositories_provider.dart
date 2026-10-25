/// Providers de repositorios para el juego de Tabú.
///
/// Expone [TabuWordRepository] respaldado por el [AppDatabase] de la app.
/// El provider usa [FutureProvider] porque obtener la [Database] abierta es
/// una operación asíncrona.
///
/// En tests se sobreescribe con un fake síncrono usando
/// `overrideWith((ref) => Future.value(fakeRepo))`.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/tabu/data/tabu_word_repository.dart';

/// Provider del [TabuWordRepository] de Tabú.
///
/// Espera a que [databaseProvider] entregue la [Database] abierta antes de
/// construir el repositorio — el mismo patrón de acceso diferido que usan
/// los repositorios de Trivia y Wavelength.
final tabuWordRepositoryProvider = FutureProvider<TabuWordRepository>((
  ref,
) async {
  final db = await ref.watch(databaseProvider.future);
  return TabuWordRepository(db);
});
