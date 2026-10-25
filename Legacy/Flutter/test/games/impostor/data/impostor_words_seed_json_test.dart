import 'dart:convert';

import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  group('assets/seed/impostor_words.json', () {
    late List<Map<String, dynamic>> words;

    setUpAll(() async {
      final raw = await rootBundle.loadString(
        'assets/seed/impostor_words.json',
      );
      words = (jsonDecode(raw) as List<dynamic>).cast<Map<String, dynamic>>();
    });

    test('tiene al menos 2000 entradas', () {
      expect(words.length, greaterThanOrEqualTo(2000));
    });

    test('cada entrada tiene word y hint no vacíos', () {
      for (final entry in words) {
        final word = (entry['word'] as String? ?? '').trim();
        final hint = (entry['hint'] as String? ?? '').trim();
        expect(word, isNotEmpty, reason: 'word vacía en entrada: $entry');
        expect(hint, isNotEmpty, reason: 'hint vacía en entrada: $entry');
      }
    });

    test('word != hint (insensible a mayúsculas)', () {
      for (final entry in words) {
        final word = (entry['word'] as String).toLowerCase().trim();
        final hint = (entry['hint'] as String).toLowerCase().trim();
        expect(
          word,
          isNot(equals(hint)),
          reason: 'word igual a hint en entrada: $entry',
        );
      }
    });

    test('no hay words duplicadas (insensible a mayúsculas)', () {
      final seen = <String>{};
      final duplicates = <String>[];
      for (final entry in words) {
        final word = (entry['word'] as String).toLowerCase().trim();
        if (!seen.add(word)) {
          duplicates.add(word);
        }
      }
      expect(duplicates, isEmpty, reason: 'Palabras duplicadas: $duplicates');
    });
  });
}
