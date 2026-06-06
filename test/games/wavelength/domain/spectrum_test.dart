/// Tests for [Spectrum]: construction, validation, and equality.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/spectrum.dart';

void main() {
  group('Spectrum', () {
    group('construction', () {
      test('creates with valid concepts', () {
        final s = Spectrum(
          id: 1,
          leftConcept: 'frío',
          rightConcept: 'caliente',
        );
        expect(s.id, 1);
        expect(s.leftConcept, 'frío');
        expect(s.rightConcept, 'caliente');
      });

      test('trims whitespace from concepts', () {
        final s = Spectrum(
          id: null,
          leftConcept: '  barato  ',
          rightConcept: '  caro  ',
        );
        expect(s.leftConcept, 'barato');
        expect(s.rightConcept, 'caro');
      });

      test('allows null id for unsaved instances', () {
        final s = Spectrum(id: null, leftConcept: 'a', rightConcept: 'b');
        expect(s.id, isNull);
      });

      test('throws ArgumentError for empty leftConcept', () {
        expect(
          () =>
              Spectrum(id: null, leftConcept: '   ', rightConcept: 'caliente'),
          throwsArgumentError,
        );
      });

      test('throws ArgumentError for empty rightConcept', () {
        expect(
          () => Spectrum(id: null, leftConcept: 'frío', rightConcept: ''),
          throwsArgumentError,
        );
      });
    });

    group('equality', () {
      test('equal when id and both concepts match', () {
        final a = Spectrum(
          id: 1,
          leftConcept: 'frío',
          rightConcept: 'caliente',
        );
        final b = Spectrum(
          id: 1,
          leftConcept: 'frío',
          rightConcept: 'caliente',
        );
        expect(a, equals(b));
      });

      test('not equal when id differs', () {
        final a = Spectrum(
          id: 1,
          leftConcept: 'frío',
          rightConcept: 'caliente',
        );
        final b = Spectrum(
          id: 2,
          leftConcept: 'frío',
          rightConcept: 'caliente',
        );
        expect(a, isNot(equals(b)));
      });

      test('not equal when leftConcept differs', () {
        final a = Spectrum(
          id: 1,
          leftConcept: 'frío',
          rightConcept: 'caliente',
        );
        final b = Spectrum(
          id: 1,
          leftConcept: 'barato',
          rightConcept: 'caliente',
        );
        expect(a, isNot(equals(b)));
      });

      test('hashCode is consistent with equality', () {
        final a = Spectrum(
          id: 1,
          leftConcept: 'frío',
          rightConcept: 'caliente',
        );
        final b = Spectrum(
          id: 1,
          leftConcept: 'frío',
          rightConcept: 'caliente',
        );
        expect(a.hashCode, equals(b.hashCode));
      });
    });
  });
}
