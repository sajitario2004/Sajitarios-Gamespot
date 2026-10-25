/// Tests for [YoNuncaStatementsSeedLoader] and the seed JSON integrity.
///
/// Two concerns:
/// 1. JSON content validation — parses assets/seed/yo_nunca_statements.json
///    from disk (dart:io) and asserts structural + coverage invariants.
/// 2. Loader behavior — creates an FFI in-memory DB, runs the loader, asserts
///    row count and is_seed=1; runs again and asserts no duplicates (idempotent).
library;

import 'dart:convert';
import 'dart:io';

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/yo_nunca/data/yo_nunca_schema.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/data/yo_nunca_statements_seed_loader.dart';

// ── Helpers ───────────────────────────────────────────────────────────────────

const _allowedIntensidades = {'suave', 'picante'};

/// Reads the seed JSON from disk (avoids rootBundle in plain flutter test).
List<Map<String, dynamic>> _loadSeedJson() {
  final file = File('assets/seed/yo_nunca_statements.json');
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

  // ── JSON content tests ─────────────────────────────────────────────────────

  group('yo_nunca_statements.json — contenido del seed', () {
    late List<Map<String, dynamic>> statements;

    setUpAll(() {
      statements = _loadSeedJson();
    });

    test('tiene al menos 140 declaraciones', () {
      expect(statements.length, greaterThanOrEqualTo(140));
    });

    test('cada declaracion tiene frase e intensidad', () {
      for (final s in statements) {
        expect(s['frase'], isA<String>(), reason: 'frase debe ser String: $s');
        expect(
          s['intensidad'],
          isA<String>(),
          reason: 'intensidad debe ser String: $s',
        );
      }
    });

    test('frase no esta vacia en ninguna declaracion', () {
      for (final s in statements) {
        final frase = (s['frase'] as String).trim();
        expect(frase, isNotEmpty, reason: 'frase vacia en: $s');
      }
    });

    test('intensidad tiene valor valido en todas las declaraciones', () {
      for (final s in statements) {
        expect(
          _allowedIntensidades,
          contains(s['intensidad']),
          reason: 'Intensidad invalida en: $s',
        );
      }
    });

    test('ambos niveles de intensidad estan representados', () {
      final intensidades = statements
          .map((s) => s['intensidad'] as String)
          .toSet();
      for (final i in _allowedIntensidades) {
        expect(intensidades, contains(i), reason: 'Falta intensidad: $i');
      }
    });

    test('hay al menos 50 declaraciones suave', () {
      final suave = statements.where((s) => s['intensidad'] == 'suave').length;
      expect(suave, greaterThanOrEqualTo(50));
    });

    test('hay al menos 30 declaraciones picante', () {
      final picante = statements
          .where((s) => s['intensidad'] == 'picante')
          .length;
      expect(picante, greaterThanOrEqualTo(30));
    });
  });

  // ── Loader behavior tests ──────────────────────────────────────────────────

  group(
    'YoNuncaStatementsSeedLoader — comportamiento del loader (FFI in-memory)',
    () {
      late Database db;

      setUp(() async {
        db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
        await YoNuncaSchema.createTables(db);
      });

      tearDown(() async {
        await db.close();
      });

      test('seedIfEmpty inserta todas las declaraciones del JSON', () async {
        final expected = _loadSeedJson().length;
        final loader = const YoNuncaStatementsSeedLoader();
        final inserted = await loader.seedIfEmpty(db);

        expect(inserted, expected);

        final rows = await db.rawQuery(
          'SELECT COUNT(*) AS c FROM $kYoNuncaStatementsTable',
        );
        expect((rows.first['c'] as int), expected);
      });

      test('todas las filas insertadas tienen is_seed = 1', () async {
        await const YoNuncaStatementsSeedLoader().seedIfEmpty(db);

        final seed = await db.rawQuery(
          'SELECT COUNT(*) AS c FROM $kYoNuncaStatementsTable WHERE is_seed = 1',
        );
        final total = await db.rawQuery(
          'SELECT COUNT(*) AS c FROM $kYoNuncaStatementsTable',
        );
        expect(
          (seed.first['c'] as int),
          (total.first['c'] as int),
          reason: 'Todas las filas del seed deben tener is_seed = 1',
        );
      });

      test(
        'seedIfEmpty es idempotente: segunda llamada no duplica filas',
        () async {
          final loader = const YoNuncaStatementsSeedLoader();

          final firstInserted = await loader.seedIfEmpty(db);
          expect(firstInserted, greaterThan(0));

          final secondInserted = await loader.seedIfEmpty(db);
          expect(secondInserted, 0);

          final rows = await db.rawQuery(
            'SELECT COUNT(*) AS c FROM $kYoNuncaStatementsTable',
          );
          expect((rows.first['c'] as int), firstInserted);
        },
      );

      test(
        'seedIfEmpty devuelve 0 sobre tabla ya poblada sin tocarla',
        () async {
          await db.insert(kYoNuncaStatementsTable, <String, Object?>{
            'frase': 'Yo nunca he probado el mate',
            'intensidad': 'suave',
            'is_seed': 0,
          });

          final inserted = await const YoNuncaStatementsSeedLoader()
              .seedIfEmpty(db);

          expect(inserted, 0);

          final rows = await db.rawQuery(
            'SELECT COUNT(*) AS c FROM $kYoNuncaStatementsTable',
          );
          expect((rows.first['c'] as int), 1);
        },
      );
    },
  );
}
