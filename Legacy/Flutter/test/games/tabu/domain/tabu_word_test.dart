/// Tests para [TabuWord]: validacion de invariantes y value object.
library;

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/games/tabu/domain/tabu_word.dart';

TabuWord _word({
  int id = 1,
  String palabra = 'Pirata',
  List<String>? prohibidas,
}) => TabuWord.create(
  id: id,
  palabra: palabra,
  prohibidas: prohibidas ?? ['barco', 'tesoro', 'mar', 'loro'],
);

void main() {
  group('TabuWord.create', () {
    group('construccion valida', () {
      test('acepta 4 palabras prohibidas', () {
        final w = _word();
        expect(w.palabra, 'Pirata');
        expect(w.prohibidas.length, 4);
      });

      test('acepta 5 palabras prohibidas', () {
        final w = _word(
          prohibidas: ['barco', 'tesoro', 'mar', 'loro', 'pata de palo'],
        );
        expect(w.prohibidas.length, 5);
      });

      test('recorta espacios de palabra y prohibidas', () {
        final w = TabuWord.create(
          id: 1,
          palabra: '  Pirata  ',
          prohibidas: [' barco ', 'tesoro', 'mar', 'loro'],
        );
        expect(w.palabra, 'Pirata');
        expect(w.prohibidas.first, 'barco');
      });

      test('lista de prohibidas es inmutable', () {
        final w = _word();
        expect(
          () => (w.prohibidas as dynamic).add('extra'),
          throwsUnsupportedError,
        );
      });
    });

    group('violaciones de invariantes', () {
      test('lanza ArgumentError si palabra esta vacia', () {
        expect(() => _word(palabra: '   '), throwsArgumentError);
      });

      test('lanza ArgumentError con menos de 4 prohibidas', () {
        expect(
          () => _word(prohibidas: ['uno', 'dos', 'tres']),
          throwsArgumentError,
        );
      });

      test('lanza ArgumentError con mas de 5 prohibidas', () {
        expect(
          () => _word(prohibidas: ['a', 'b', 'c', 'd', 'e', 'f']),
          throwsArgumentError,
        );
      });

      test('lanza ArgumentError si alguna prohibida esta vacia', () {
        expect(
          () => _word(prohibidas: ['barco', '', 'mar', 'loro']),
          throwsArgumentError,
        );
      });
    });

    group('igualdad y hashCode', () {
      test('dos instancias con mismos valores son iguales', () {
        final a = _word();
        final b = _word();
        expect(a, equals(b));
        expect(a.hashCode, equals(b.hashCode));
      });

      test('diferentes ids producen desigualdad', () {
        final a = _word(id: 1);
        final b = _word(id: 2);
        expect(a, isNot(equals(b)));
      });

      test('diferentes palabras producen desigualdad', () {
        final a = _word(palabra: 'Pirata');
        final b = _word(palabra: 'Vikingo');
        expect(a, isNot(equals(b)));
      });

      test('diferentes prohibidas producen desigualdad', () {
        final a = _word(prohibidas: ['barco', 'tesoro', 'mar', 'loro']);
        final b = _word(prohibidas: ['barco', 'tesoro', 'mar', 'ancla']);
        expect(a, isNot(equals(b)));
      });
    });
  });
}
