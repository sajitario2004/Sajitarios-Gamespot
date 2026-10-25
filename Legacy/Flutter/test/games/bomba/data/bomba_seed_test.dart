/// Tests for [BombaSeedLoader] and the seed JSON integrity.
///
/// Two concerns:
/// 1. JSON content validation — parses both seed files from disk (dart:io) and
///    asserts structural + coverage invariants.
/// 2. Loader behavior — creates an FFI in-memory DB, runs the loader, asserts
///    row counts and is_seed=1; runs again and asserts no duplicates (idempotent).
library;

import 'dart:convert';
import 'dart:io';

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/bomba/data/bomba_schema.dart';
import 'package:sajitarios_gamespot/games/bomba/data/bomba_seed_loader.dart';

// ── Helpers ───────────────────────────────────────────────────────────────────

List<String> _loadStringArrayFromDisk(String path) {
  final raw = File(path).readAsStringSync();
  return (jsonDecode(raw) as List<dynamic>).cast<String>();
}

// ── Tests ─────────────────────────────────────────────────────────────────────

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  // ── JSON content — silabas ─────────────────────────────────────────────────

  group('bomba_silabas.json — contenido del seed', () {
    late List<String> silabas;

    setUpAll(() {
      silabas = _loadStringArrayFromDisk('assets/seed/bomba_silabas.json');
    });

    test('tiene al menos 70 silabas', () {
      expect(silabas.length, greaterThanOrEqualTo(70));
    });

    test('ninguna silaba esta vacia', () {
      for (final s in silabas) {
        expect(s.trim(), isNotEmpty, reason: 'Silaba vacia encontrada: "$s"');
      }
    });

    test('no hay silabas duplicadas (case-insensitive)', () {
      final seen = <String>{};
      for (final s in silabas) {
        final key = s.trim().toUpperCase();
        expect(seen, isNot(contains(key)), reason: 'Duplicado: "$s"');
        seen.add(key);
      }
    });
  });

  // ── JSON content — categorias ──────────────────────────────────────────────

  group('bomba_categorias.json — contenido del seed', () {
    late List<String> categorias;

    setUpAll(() {
      categorias = _loadStringArrayFromDisk(
        'assets/seed/bomba_categorias.json',
      );
    });

    test('tiene al menos 70 categorias', () {
      expect(categorias.length, greaterThanOrEqualTo(70));
    });

    test('ninguna categoria esta vacia', () {
      for (final c in categorias) {
        expect(
          c.trim(),
          isNotEmpty,
          reason: 'Categoria vacia encontrada: "$c"',
        );
      }
    });

    test('no hay categorias duplicadas (case-insensitive)', () {
      final seen = <String>{};
      for (final c in categorias) {
        final key = c.trim().toLowerCase();
        expect(seen, isNot(contains(key)), reason: 'Duplicado: "$c"');
        seen.add(key);
      }
    });
  });

  // ── Loader behavior — silabas ──────────────────────────────────────────────

  group('BombaSeedLoader — silabas (FFI in-memory)', () {
    late Database db;
    late BombaSeedLoader loader;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await BombaSchema.createTables(db);
      loader = const BombaSeedLoader();
    });

    tearDown(() async {
      await db.close();
    });

    test('seedSilabasIfEmpty inserta todas las silabas del JSON', () async {
      final expected = _loadStringArrayFromDisk(
        'assets/seed/bomba_silabas.json',
      ).length;
      final inserted = await loader.seedSilabasIfEmpty(db);
      expect(inserted, expected);
      final rows = await db.rawQuery(
        'SELECT COUNT(*) AS c FROM $kBombaSilabasTable',
      );
      expect(rows.first['c'] as int, expected);
    });

    test('todas las filas de silabas tienen is_seed = 1', () async {
      await loader.seedSilabasIfEmpty(db);
      final total = await db.rawQuery(
        'SELECT COUNT(*) AS c FROM $kBombaSilabasTable',
      );
      final seeded = await db.rawQuery(
        'SELECT COUNT(*) AS c FROM $kBombaSilabasTable WHERE is_seed = 1',
      );
      expect(seeded.first['c'] as int, total.first['c'] as int);
    });

    test(
      'seedSilabasIfEmpty es idempotente (segunda llamada devuelve 0)',
      () async {
        final first = await loader.seedSilabasIfEmpty(db);
        expect(first, greaterThan(0));
        final second = await loader.seedSilabasIfEmpty(db);
        expect(second, 0);
        final rows = await db.rawQuery(
          'SELECT COUNT(*) AS c FROM $kBombaSilabasTable',
        );
        expect(rows.first['c'] as int, first);
      },
    );
  });

  // ── Loader behavior — categorias ───────────────────────────────────────────

  group('BombaSeedLoader — categorias (FFI in-memory)', () {
    late Database db;
    late BombaSeedLoader loader;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await BombaSchema.createTables(db);
      loader = const BombaSeedLoader();
    });

    tearDown(() async {
      await db.close();
    });

    test(
      'seedCategoriasIfEmpty inserta todas las categorias del JSON',
      () async {
        final expected = _loadStringArrayFromDisk(
          'assets/seed/bomba_categorias.json',
        ).length;
        final inserted = await loader.seedCategoriasIfEmpty(db);
        expect(inserted, expected);
        final rows = await db.rawQuery(
          'SELECT COUNT(*) AS c FROM $kBombaCategoriasTable',
        );
        expect(rows.first['c'] as int, expected);
      },
    );

    test('todas las filas de categorias tienen is_seed = 1', () async {
      await loader.seedCategoriasIfEmpty(db);
      final total = await db.rawQuery(
        'SELECT COUNT(*) AS c FROM $kBombaCategoriasTable',
      );
      final seeded = await db.rawQuery(
        'SELECT COUNT(*) AS c FROM $kBombaCategoriasTable WHERE is_seed = 1',
      );
      expect(seeded.first['c'] as int, total.first['c'] as int);
    });

    test(
      'seedCategoriasIfEmpty es idempotente (segunda llamada devuelve 0)',
      () async {
        final first = await loader.seedCategoriasIfEmpty(db);
        expect(first, greaterThan(0));
        final second = await loader.seedCategoriasIfEmpty(db);
        expect(second, 0);
        final rows = await db.rawQuery(
          'SELECT COUNT(*) AS c FROM $kBombaCategoriasTable',
        );
        expect(rows.first['c'] as int, first);
      },
    );
  });

  // ── seedAllIfEmpty ─────────────────────────────────────────────────────────

  group('BombaSeedLoader.seedAllIfEmpty', () {
    late Database db;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await BombaSchema.createTables(db);
    });

    tearDown(() async {
      await db.close();
    });

    test('siembra ambas tablas y devuelve conteos correctos', () async {
      final (s, c) = await const BombaSeedLoader().seedAllIfEmpty(db);
      final expectedS = _loadStringArrayFromDisk(
        'assets/seed/bomba_silabas.json',
      ).length;
      final expectedC = _loadStringArrayFromDisk(
        'assets/seed/bomba_categorias.json',
      ).length;
      expect(s, expectedS);
      expect(c, expectedC);
    });

    test('segunda llamada a seedAllIfEmpty devuelve (0, 0)', () async {
      await const BombaSeedLoader().seedAllIfEmpty(db);
      final (s, c) = await const BombaSeedLoader().seedAllIfEmpty(db);
      expect(s, 0);
      expect(c, 0);
    });
  });
}
