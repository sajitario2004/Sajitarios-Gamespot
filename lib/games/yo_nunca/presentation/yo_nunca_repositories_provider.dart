/// Providers de repositorios para el juego Yo Nunca.
///
/// Expone [NeverStatementRepository] respaldado por el [AppDatabase] de la app.
/// El provider usa [FutureProvider] porque obtener la [Database] abierta es
/// una operación asíncrona.
///
/// En tests se sobreescribe con un fake síncrono usando
/// `overrideWith((ref) => Future.value(fakeRepo))`.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/data/never_statement_repository.dart';

/// Provider del [NeverStatementRepository] de Yo Nunca.
///
/// Espera a que [databaseProvider] entregue la [Database] abierta antes de
/// construir el repositorio — el mismo patrón de acceso diferido que usan
/// los repositorios de Trivia, Wavelength y Tabú.
final neverStatementRepositoryProvider =
    FutureProvider<NeverStatementRepository>((ref) async {
      final db = await ref.watch(databaseProvider.future);
      return NeverStatementRepository(db);
    });
