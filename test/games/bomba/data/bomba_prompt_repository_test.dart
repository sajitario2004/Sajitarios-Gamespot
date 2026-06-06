/// Tests for [BombaPromptRepository]: insert, bulkInsert, count, getAll
/// for both silabas and categorias.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/bomba/data/bomba_prompt_repository.dart';
import 'package:sajitarios_gamespot/games/bomba/data/bomba_schema.dart';
import 'package:sajitarios_gamespot/games/bomba/domain/bomba_mode.dart';

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('BombaPromptRepository', () {
    late Database db;
    late BombaPromptRepository repo;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await BombaSchema.createTables(db);
      repo = BombaPromptRepository(db);
    });

    tearDown(() async {
      await db.close();
    });

    // ── Silabas ──────────────────────────────────────────────────────────────

    group('silabas — insert y count', () {
      test('countSilabas devuelve 0 en tabla vacia', () async {
        expect(await repo.countSilabas(), 0);
      });

      test('insertSilaba devuelve prompt con id asignado', () async {
        final p = await repo.insertSilaba('CA');
        expect(p.id, greaterThan(0));
        expect(p.texto, 'CA');
        expect(p.mode, BombaMode.silaba);
      });

      test('countSilabas incrementa tras insert', () async {
        await repo.insertSilaba('CA');
        await repo.insertSilaba('PA');
        expect(await repo.countSilabas(), 2);
      });

      test('bulkInsertSilabas inserta todos y devuelve la lista', () async {
        final prompts = await repo.bulkInsertSilabas(['CA', 'PA', 'RA']);
        expect(prompts.length, 3);
        for (final p in prompts) {
          expect(p.id, greaterThan(0));
          expect(p.mode, BombaMode.silaba);
        }
        expect(await repo.countSilabas(), 3);
      });
    });

    group('silabas — getAllSilabas', () {
      test('devuelve lista vacia cuando no hay filas', () async {
        expect(await repo.getAllSilabas(), isEmpty);
      });

      test('devuelve todos los prompts en orden por id', () async {
        await repo.bulkInsertSilabas(['CA', 'PA', 'RA']);
        final all = await repo.getAllSilabas();
        expect(all.length, 3);
        expect(all.map((p) => p.texto).toList(), ['CA', 'PA', 'RA']);
      });

      test('texto se preserva en round-trip', () async {
        await repo.insertSilaba('TRA');
        final all = await repo.getAllSilabas();
        expect(all.first.texto, 'TRA');
      });
    });

    // ── Categorias ───────────────────────────────────────────────────────────

    group('categorias — insert y count', () {
      test('countCategorias devuelve 0 en tabla vacia', () async {
        expect(await repo.countCategorias(), 0);
      });

      test('insertCategoria devuelve prompt con id asignado', () async {
        final p = await repo.insertCategoria('nombres de animales');
        expect(p.id, greaterThan(0));
        expect(p.texto, 'nombres de animales');
        expect(p.mode, BombaMode.categoria);
      });

      test('countCategorias incrementa tras insert', () async {
        await repo.insertCategoria('frutas');
        await repo.insertCategoria('paises');
        expect(await repo.countCategorias(), 2);
      });

      test('bulkInsertCategorias inserta todos y devuelve la lista', () async {
        final prompts = await repo.bulkInsertCategorias([
          'frutas',
          'deportes',
          'colores',
        ]);
        expect(prompts.length, 3);
        for (final p in prompts) {
          expect(p.id, greaterThan(0));
          expect(p.mode, BombaMode.categoria);
        }
        expect(await repo.countCategorias(), 3);
      });
    });

    group('categorias — getAllCategorias', () {
      test('devuelve lista vacia cuando no hay filas', () async {
        expect(await repo.getAllCategorias(), isEmpty);
      });

      test('devuelve todos los prompts en orden por id', () async {
        await repo.bulkInsertCategorias(['frutas', 'deportes', 'colores']);
        final all = await repo.getAllCategorias();
        expect(all.length, 3);
        expect(all.map((p) => p.texto).toList(), [
          'frutas',
          'deportes',
          'colores',
        ]);
      });
    });

    // ── Aislamiento entre tablas ──────────────────────────────────────────────

    test('silabas y categorias no se mezclan', () async {
      await repo.insertSilaba('CA');
      await repo.insertCategoria('frutas');

      expect(await repo.countSilabas(), 1);
      expect(await repo.countCategorias(), 1);

      final silabas = await repo.getAllSilabas();
      final categorias = await repo.getAllCategorias();

      expect(silabas.single.mode, BombaMode.silaba);
      expect(categorias.single.mode, BombaMode.categoria);
    });
  });
}
