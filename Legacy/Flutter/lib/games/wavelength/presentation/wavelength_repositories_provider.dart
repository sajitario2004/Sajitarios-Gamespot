/// Providers de repositorios para el juego de Wavelength.
///
/// Expone [SpectrumRepository] respaldado por el [AppDatabase] de la app.
/// Usa [FutureProvider] porque obtener la [Database] abierta es asíncrono.
///
/// En tests se sobreescribe con un fake usando
/// `overrideWith((ref) => Future.value(fakeRepo))`.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/wavelength/data/spectrum_repository.dart';

/// Provider del [SpectrumRepository] de Wavelength.
final spectrumRepositoryProvider = FutureProvider<SpectrumRepository>((
  ref,
) async {
  final db = await ref.watch(databaseProvider.future);
  return SpectrumRepository(db);
});
