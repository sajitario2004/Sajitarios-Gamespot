/// Tests for [WavelengthSpectraSeedLoader] and seed JSON integrity.
///
/// Two concerns:
/// 1. JSON content validation — parses assets/seed/wavelength_spectra.json
///    from disk (dart:io) and asserts structural invariants.
/// 2. Loader behavior — creates an FFI in-memory DB, runs the loader, asserts
///    row count and is_seed=1; runs again to verify idempotence.
library;

import 'dart:convert';
import 'dart:io';

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/wavelength/data/wavelength_schema.dart';
import 'package:sajitarios_gamespot/games/wavelength/data/wavelength_spectra_seed_loader.dart';

// ── Helpers ───────────────────────────────────────────────────────────────────

/// Reads the seed JSON from disk (avoids rootBundle in plain flutter test).
List<Map<String, dynamic>> _loadSeedJson() {
  final file = File('assets/seed/wavelength_spectra.json');
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

  group('wavelength_spectra.json — contenido del seed', () {
    late List<Map<String, dynamic>> spectra;

    setUpAll(() {
      spectra = _loadSeedJson();
    });

    test('tiene al menos 70 pares de espectros', () {
      expect(spectra.length, greaterThanOrEqualTo(70));
    });

    test('cada espectro tiene campo "izquierda" no vacío', () {
      for (final s in spectra) {
        final izquierda = (s['izquierda'] as String?)?.trim() ?? '';
        expect(izquierda, isNotEmpty, reason: '"izquierda" vacío en: $s');
      }
    });

    test('cada espectro tiene campo "derecha" no vacío', () {
      for (final s in spectra) {
        final derecha = (s['derecha'] as String?)?.trim() ?? '';
        expect(derecha, isNotEmpty, reason: '"derecha" vacío en: $s');
      }
    });

    test('izquierda y derecha son distintos en cada par', () {
      for (final s in spectra) {
        final izquierda = (s['izquierda'] as String).trim().toLowerCase();
        final derecha = (s['derecha'] as String).trim().toLowerCase();
        expect(
          izquierda,
          isNot(equals(derecha)),
          reason: 'Concepto izquierdo y derecho son iguales en: $s',
        );
      }
    });

    test('no hay pares con conceptos invertidos duplicados', () {
      final seen = <String>{};
      for (final s in spectra) {
        final left = (s['izquierda'] as String).trim().toLowerCase();
        final right = (s['derecha'] as String).trim().toLowerCase();
        final key = '${left}_$right';
        final reverseKey = '${right}_$left';
        expect(seen, isNot(contains(key)), reason: 'Par duplicado en: $s');
        expect(
          seen,
          isNot(contains(reverseKey)),
          reason: 'Par invertido duplicado en: $s',
        );
        seen.add(key);
      }
    });
  });

  // ── Loader behavior tests ──────────────────────────────────────────────────

  group(
    'WavelengthSpectraSeedLoader — comportamiento del loader (FFI in-memory)',
    () {
      late Database db;

      setUp(() async {
        db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
        await WavelengthSchema.createTables(db);
      });

      tearDown(() async {
        await db.close();
      });

      test('seedIfEmpty inserta todos los espectros del JSON', () async {
        final expected = _loadSeedJson().length;

        final loader = const WavelengthSpectraSeedLoader();
        final inserted = await loader.seedIfEmpty(db);

        expect(inserted, expected);

        final rows = await db.rawQuery(
          'SELECT COUNT(*) AS c FROM $kWavelengthSpectraTable',
        );
        expect((rows.first['c'] as int), expected);
      });

      test('todas las filas insertadas tienen is_seed = 1', () async {
        await const WavelengthSpectraSeedLoader().seedIfEmpty(db);

        final seedRows = await db.rawQuery(
          'SELECT COUNT(*) AS c FROM $kWavelengthSpectraTable WHERE is_seed = 1',
        );
        final totalRows = await db.rawQuery(
          'SELECT COUNT(*) AS c FROM $kWavelengthSpectraTable',
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
          final loader = const WavelengthSpectraSeedLoader();

          final firstInserted = await loader.seedIfEmpty(db);
          expect(firstInserted, greaterThan(0));

          final secondInserted = await loader.seedIfEmpty(db);
          expect(secondInserted, 0);

          final rows = await db.rawQuery(
            'SELECT COUNT(*) AS c FROM $kWavelengthSpectraTable',
          );
          expect((rows.first['c'] as int), firstInserted);
        },
      );

      test(
        'seedIfEmpty devuelve 0 sobre tabla ya poblada sin tocarla',
        () async {
          await db.insert(kWavelengthSpectraTable, <String, Object?>{
            'izquierda': 'manual',
            'derecha': 'test',
            'is_seed': 0,
          });

          final inserted = await const WavelengthSpectraSeedLoader()
              .seedIfEmpty(db);

          expect(inserted, 0);

          final rows = await db.rawQuery(
            'SELECT COUNT(*) AS c FROM $kWavelengthSpectraTable',
          );
          expect((rows.first['c'] as int), 1);
        },
      );
    },
  );
}
