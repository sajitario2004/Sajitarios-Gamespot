/// Tests para [TabuWordsSeedLoader] e integridad del seed JSON.
///
/// Dos aspectos:
/// 1. Contenido JSON — parsea assets/seed/tabu_words.json desde disco (dart:io)
///    y verifica invariantes estructurales.
/// 2. Comportamiento del loader — crea una BD FFI en memoria, ejecuta el loader,
///    verifica el conteo y is_seed=1; ejecuta de nuevo para confirmar idempotencia.
library;

import 'dart:convert';
import 'dart:io';

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/tabu/data/tabu_schema.dart';
import 'package:sajitarios_gamespot/games/tabu/data/tabu_words_seed_loader.dart';

// ── Helpers ───────────────────────────────────────────────────────────────────

/// Lee el seed JSON desde disco (evita rootBundle en flutter test puro).
List<Map<String, dynamic>> _loadSeedJson() {
  final file = File('assets/seed/tabu_words.json');
  final raw = file.readAsStringSync();
  final decoded = jsonDecode(raw) as List<dynamic>;
  return decoded.cast<Map<String, dynamic>>();
}

// ── Tests ─────────────────────────────────────────────────────────────────────

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  // ── Contenido JSON ─────────────────────────────────────────────────────────

  group('tabu_words.json — contenido del seed', () {
    late List<Map<String, dynamic>> words;

    setUpAll(() {
      words = _loadSeedJson();
    });

    test('tiene al menos 100 palabras', () {
      expect(words.length, greaterThanOrEqualTo(100));
    });

    test('cada entrada tiene el campo "palabra" no vacio', () {
      for (final w in words) {
        final palabra = (w['palabra'] as String?)?.trim() ?? '';
        expect(palabra, isNotEmpty, reason: '"palabra" vacia en: $w');
      }
    });

    test('cada entrada tiene entre 4 y 5 prohibidas', () {
      for (final w in words) {
        final prohibidas = w['prohibidas'];
        expect(
          prohibidas,
          isA<List>(),
          reason: '"prohibidas" debe ser lista en: $w',
        );
        final len = (prohibidas as List).length;
        expect(
          len,
          allOf(greaterThanOrEqualTo(4), lessThanOrEqualTo(5)),
          reason: 'Debe haber 4 o 5 prohibidas en: $w',
        );
      }
    });

    test('ninguna palabra prohibida esta vacia', () {
      for (final w in words) {
        final prohibidas = (w['prohibidas'] as List).cast<String>();
        for (final p in prohibidas) {
          expect(p.trim(), isNotEmpty, reason: 'Prohibida vacia en: $w');
        }
      }
    });

    test('no hay palabras duplicadas (insensible a mayusculas)', () {
      final seen = <String>{};
      for (final w in words) {
        final palabra = (w['palabra'] as String).trim().toLowerCase();
        expect(
          seen.add(palabra),
          isTrue,
          reason: 'Palabra duplicada: $palabra',
        );
      }
    });
  });

  // ── Comportamiento del loader ──────────────────────────────────────────────

  group('TabuWordsSeedLoader — comportamiento del loader (FFI en memoria)', () {
    late Database db;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await TabuSchema.createTables(db);
    });

    tearDown(() async {
      await db.close();
    });

    test('seedIfEmpty inserta todas las palabras del JSON', () async {
      final expected = _loadSeedJson().length;
      final loader = const TabuWordsSeedLoader();
      final inserted = await loader.seedIfEmpty(db);

      expect(inserted, expected);

      final rows = await db.rawQuery(
        'SELECT COUNT(*) AS c FROM $kTabuWordsTable',
      );
      expect((rows.first['c'] as int), expected);
    });

    test('todas las filas insertadas tienen is_seed = 1', () async {
      await const TabuWordsSeedLoader().seedIfEmpty(db);

      final seedRows = await db.rawQuery(
        'SELECT COUNT(*) AS c FROM $kTabuWordsTable WHERE is_seed = 1',
      );
      final totalRows = await db.rawQuery(
        'SELECT COUNT(*) AS c FROM $kTabuWordsTable',
      );
      expect(
        (seedRows.first['c'] as int),
        (totalRows.first['c'] as int),
        reason: 'Todas las filas del seed deben tener is_seed = 1',
      );
    });

    test(
      'seedIfEmpty es idempotente: segunda llamada no duplica filas',
      () async {
        final loader = const TabuWordsSeedLoader();

        final firstInserted = await loader.seedIfEmpty(db);
        expect(firstInserted, greaterThan(0));

        final secondInserted = await loader.seedIfEmpty(db);
        expect(secondInserted, 0);

        final rows = await db.rawQuery(
          'SELECT COUNT(*) AS c FROM $kTabuWordsTable',
        );
        expect((rows.first['c'] as int), firstInserted);
      },
    );

    test('seedIfEmpty devuelve 0 sobre tabla ya poblada sin tocarla', () async {
      // Inserta una fila manual para simular tabla no vacia.
      await db.rawInsert(
        'INSERT INTO $kTabuWordsTable (palabra, prohibidas_json, is_seed) '
        'VALUES (?, ?, ?)',
        ['Manual', '["a","b","c","d"]', 0],
      );

      final inserted = await const TabuWordsSeedLoader().seedIfEmpty(db);
      expect(inserted, 0);

      final rows = await db.rawQuery(
        'SELECT COUNT(*) AS c FROM $kTabuWordsTable',
      );
      expect((rows.first['c'] as int), 1);
    });
  });
}
