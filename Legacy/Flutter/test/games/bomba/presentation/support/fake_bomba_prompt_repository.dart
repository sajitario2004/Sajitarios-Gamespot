import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/bomba/data/bomba_prompt_repository.dart';
import 'package:sajitarios_gamespot/games/bomba/data/bomba_schema.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_mode.dart';

/// Abre una base de datos FFI en memoria completamente aislada por llamada.
///
/// sqflite FFI tiene un caché de instancias por path. Usar siempre
/// `singleInstance: false` garantiza que cada llamada abra una BD nueva,
/// evitando conflictos entre tests que comparten el mismo `:memory:` path.
Future<Database> _openFreshDb() => databaseFactoryFfi.openDatabase(
  inMemoryDatabasePath,
  options: OpenDatabaseOptions(singleInstance: false),
);

/// Crea un [BombaPromptRepository] respaldado por una BD FFI en memoria fresca,
/// con prompts de prueba predeterminados.
///
/// Las sílabas de test (por defecto "CA", "RA", "TA") y categorías ("animales",
/// "colores", "comidas") son suficientes para ejercitar la lógica de selección
/// sin necesitar [rootBundle] ni assets reales.
Future<BombaPromptRepository> makeFakeRepo({
  List<String> silabas = const ['CA', 'RA', 'TA'],
  List<String> categorias = const ['animales', 'colores', 'comidas'],
}) async {
  final db = await _openFreshDb();
  await BombaSchema.createTables(db);
  final repo = BombaPromptRepository(db);
  for (final s in silabas) {
    await repo.insertSilaba(s, isSeed: true);
  }
  for (final c in categorias) {
    await repo.insertCategoria(c, isSeed: true);
  }
  return repo;
}

/// Versión con pool vacío para el modo [emptyMode].
Future<BombaPromptRepository> makeEmptyRepo({
  BombaMode emptyMode = BombaMode.silaba,
}) async {
  final db = await _openFreshDb();
  await BombaSchema.createTables(db);
  final repo = BombaPromptRepository(db);
  // Solo cargamos prompts del OTRO modo para que el modo elegido quede vacío.
  if (emptyMode == BombaMode.silaba) {
    await repo.insertCategoria('animales', isSeed: true);
  } else {
    await repo.insertSilaba('CA', isSeed: true);
  }
  return repo;
}
