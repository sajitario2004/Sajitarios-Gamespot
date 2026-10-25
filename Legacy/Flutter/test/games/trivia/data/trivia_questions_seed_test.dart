/// Tests for [TriviaQuestionsSeedLoader] and the seed JSON integrity.
///
/// Two concerns:
/// 1. JSON content validation — parses assets/seed/trivia_questions.json from
///    disk (dart:io) and asserts structural + coverage invariants.
/// 2. Loader behavior — creates an FFI in-memory DB, runs the loader, asserts
///    row count and is_seed=1; runs again and asserts no duplicates (idempotent).
library;

import 'dart:convert';
import 'dart:io';

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/trivia/data/trivia_questions_seed_loader.dart';
import 'package:sajitarios_gamespot/games/trivia/data/trivia_schema.dart';

// ── Helpers ───────────────────────────────────────────────────────────────────

const _allowedDifficulties = {'facil', 'dificil', 'muyDificil'};
const _allowedTematicas = {
  'cultura_general',
  'videojuegos',
  'cocina',
  'cine',
  'ciencia',
  'geografia',
  'historia',
  'deportes',
  'musica',
};
const _priorityTematicas = {'cultura_general', 'videojuegos', 'cocina'};

/// Reads the seed JSON from disk (avoids rootBundle in plain flutter test).
List<Map<String, dynamic>> _loadSeedJson() {
  final file = File('assets/seed/trivia_questions.json');
  final raw = file.readAsStringSync();
  final decoded = jsonDecode(raw) as List<dynamic>;
  return decoded.cast<Map<String, dynamic>>();
}

// ── Tests ─────────────────────────────────────────────────────────────────────

void main() {
  // Required so rootBundle (used by the loader in production path) works if
  // called; for these tests we read from disk, but keeping the binding init
  // is consistent with other seed tests in this project.
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  // ── JSON content tests ─────────────────────────────────────────────────────

  group('trivia_questions.json — contenido del seed', () {
    late List<Map<String, dynamic>> questions;

    setUpAll(() {
      questions = _loadSeedJson();
    });

    test('tiene al menos 1000 preguntas', () {
      expect(questions.length, greaterThanOrEqualTo(1000));
    });

    test('cada pregunta tiene exactamente 4 opciones', () {
      for (final q in questions) {
        final opciones = q['opciones'];
        expect(
          opciones,
          isA<List>(),
          reason: 'opciones debe ser una lista: $q',
        );
        expect(
          (opciones as List).length,
          4,
          reason: 'Debe tener exactamente 4 opciones: $q',
        );
      }
    });

    test('correcta está en el rango [0, 4) en todas las preguntas', () {
      for (final q in questions) {
        final correcta = q['correcta'];
        expect(correcta, isA<int>(), reason: '"correcta" debe ser int: $q');
        expect(
          correcta as int,
          allOf(greaterThanOrEqualTo(0), lessThan(4)),
          reason: '"correcta" fuera de rango [0,4): $q',
        );
      }
    });

    test('difficulty tiene valor válido en todas las preguntas', () {
      for (final q in questions) {
        expect(
          _allowedDifficulties,
          contains(q['difficulty']),
          reason: 'Dificultad inválida en: $q',
        );
      }
    });

    test('enunciado no está vacío en ninguna pregunta', () {
      for (final q in questions) {
        final enunciado = (q['enunciado'] as String?)?.trim() ?? '';
        expect(enunciado, isNotEmpty, reason: 'enunciado vacío en: $q');
      }
    });

    test('tematica pertenece al conjunto permitido en todas las preguntas', () {
      for (final q in questions) {
        expect(
          _allowedTematicas,
          contains(q['tematica']),
          reason: 'Temática no permitida en: $q',
        );
      }
    });

    test('las 3 dificultades están representadas', () {
      final difficulties = questions
          .map((q) => q['difficulty'] as String)
          .toSet();
      for (final d in _allowedDifficulties) {
        expect(difficulties, contains(d), reason: 'Falta dificultad: $d');
      }
    });

    test('las 3 temáticas prioritarias están presentes', () {
      final tematicas = questions.map((q) => q['tematica'] as String).toSet();
      for (final t in _priorityTematicas) {
        expect(
          tematicas,
          contains(t),
          reason: 'Falta temática prioritaria: $t',
        );
      }
    });

    test('no hay opciones vacías en ninguna pregunta', () {
      for (final q in questions) {
        final opciones = (q['opciones'] as List).cast<String>();
        for (final opcion in opciones) {
          expect(opcion.trim(), isNotEmpty, reason: 'Opción vacía en: $q');
        }
      }
    });
  });

  // ── Loader behavior tests ──────────────────────────────────────────────────

  group(
    'TriviaQuestionsSeedLoader — comportamiento del loader (FFI in-memory)',
    () {
      late Database db;

      setUp(() async {
        db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
        await TriviaSchema.createTables(db);
      });

      tearDown(() async {
        await db.close();
      });

      test('seedIfEmpty inserta todas las preguntas del JSON', () async {
        // Read expected count directly from disk so the test doesn't hardcode it.
        final expected = _loadSeedJson().length;

        // The loader uses rootBundle, which needs the asset binding.
        // We test it via the disk-based path using a custom assetPath-overriding
        // subclass isn't needed — the loader is tested with the real rootBundle
        // because TestWidgetsFlutterBinding.ensureInitialized() is active.
        // However, for this in-memory DB test we use the loader directly so
        // rootBundle is required. Since this is a flutter test (not dart test)
        // the binding is active and assets are accessible.
        final loader = const TriviaQuestionsSeedLoader();
        final inserted = await loader.seedIfEmpty(db);

        expect(inserted, expected);

        final rows = await db.rawQuery(
          'SELECT COUNT(*) AS c FROM $kTriviaQuestionsTable',
        );
        expect((rows.first['c'] as int), expected);
      });

      test('todas las filas insertadas tienen is_seed = 1', () async {
        await const TriviaQuestionsSeedLoader().seedIfEmpty(db);

        final rows = await db.rawQuery(
          'SELECT COUNT(*) AS c FROM $kTriviaQuestionsTable WHERE is_seed = 1',
        );
        final totalRows = await db.rawQuery(
          'SELECT COUNT(*) AS c FROM $kTriviaQuestionsTable',
        );
        expect(
          (rows.first['c'] as int),
          (totalRows.first['c'] as int),
          reason: 'Todas las filas del seed deben tener is_seed = 1',
        );
      });

      test(
        'seedIfEmpty es idempotente: segunda llamada no duplica filas',
        () async {
          final loader = const TriviaQuestionsSeedLoader();

          final firstInserted = await loader.seedIfEmpty(db);
          expect(firstInserted, greaterThan(0));

          // Segunda ejecución: la tabla ya tiene datos, debe devolver 0.
          final secondInserted = await loader.seedIfEmpty(db);
          expect(secondInserted, 0);

          // El conteo total no cambió.
          final rows = await db.rawQuery(
            'SELECT COUNT(*) AS c FROM $kTriviaQuestionsTable',
          );
          expect((rows.first['c'] as int), firstInserted);
        },
      );

      test(
        'seedIfEmpty devuelve 0 sobre tabla ya poblada sin tocarla',
        () async {
          // Inserta una fila manual para simular tabla no vacía.
          await db.insert(kTriviaQuestionsTable, <String, Object?>{
            'tematica_id': 'historia',
            'difficulty': 'facil',
            'enunciado': 'Pregunta manual de prueba',
            'options_json': '["A","B","C","D"]',
            'correct_index': 0,
            'is_seed': 0,
          });

          final inserted = await const TriviaQuestionsSeedLoader().seedIfEmpty(
            db,
          );

          // No debe insertar nada porque la tabla ya tiene al menos una fila.
          expect(inserted, 0);

          // Solo la fila manual existe.
          final rows = await db.rawQuery(
            'SELECT COUNT(*) AS c FROM $kTriviaQuestionsTable',
          );
          expect((rows.first['c'] as int), 1);
        },
      );
    },
  );
}
