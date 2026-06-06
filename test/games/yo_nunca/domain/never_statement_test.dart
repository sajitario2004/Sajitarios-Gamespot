/// Tests for [NeverStatement]: construction, validation, equality, hashCode.
library;

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/games/yo_nunca/domain/intensidad.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/never_statement.dart';

void main() {
  group('NeverStatement.create', () {
    test('creates a valid statement', () {
      final s = NeverStatement.create(
        id: 1,
        frase: 'Yo nunca me he quedado dormido en el cine',
        intensidad: Intensidad.suave,
      );
      expect(s.id, 1);
      expect(s.frase, 'Yo nunca me he quedado dormido en el cine');
      expect(s.intensidad, Intensidad.suave);
    });

    test('trims whitespace from frase', () {
      final s = NeverStatement.create(
        id: 2,
        frase: '  Yo nunca he viajado solo  ',
        intensidad: Intensidad.picante,
      );
      expect(s.frase, 'Yo nunca he viajado solo');
    });

    test('throws ArgumentError when frase is empty', () {
      expect(
        () => NeverStatement.create(
          id: 1,
          frase: '',
          intensidad: Intensidad.suave,
        ),
        throwsArgumentError,
      );
    });

    test('throws ArgumentError when frase is only whitespace', () {
      expect(
        () => NeverStatement.create(
          id: 1,
          frase: '   ',
          intensidad: Intensidad.suave,
        ),
        throwsArgumentError,
      );
    });
  });

  group('NeverStatement equality', () {
    test('equal when all fields match', () {
      final a = NeverStatement.create(
        id: 5,
        frase: 'Yo nunca he comido pizza fría',
        intensidad: Intensidad.suave,
      );
      final b = NeverStatement.create(
        id: 5,
        frase: 'Yo nunca he comido pizza fría',
        intensidad: Intensidad.suave,
      );
      expect(a, equals(b));
      expect(a.hashCode, b.hashCode);
    });

    test('not equal when id differs', () {
      final a = NeverStatement.create(
        id: 1,
        frase: 'Yo nunca he comido pizza fría',
        intensidad: Intensidad.suave,
      );
      final b = NeverStatement.create(
        id: 2,
        frase: 'Yo nunca he comido pizza fría',
        intensidad: Intensidad.suave,
      );
      expect(a, isNot(equals(b)));
    });

    test('not equal when frase differs', () {
      final a = NeverStatement.create(
        id: 1,
        frase: 'Yo nunca he comido pizza fría',
        intensidad: Intensidad.suave,
      );
      final b = NeverStatement.create(
        id: 1,
        frase: 'Yo nunca he dormido en el trabajo',
        intensidad: Intensidad.suave,
      );
      expect(a, isNot(equals(b)));
    });

    test('not equal when intensidad differs', () {
      final a = NeverStatement.create(
        id: 1,
        frase: 'Yo nunca he besado a alguien sin saber su nombre',
        intensidad: Intensidad.suave,
      );
      final b = NeverStatement.create(
        id: 1,
        frase: 'Yo nunca he besado a alguien sin saber su nombre',
        intensidad: Intensidad.picante,
      );
      expect(a, isNot(equals(b)));
    });
  });
}
