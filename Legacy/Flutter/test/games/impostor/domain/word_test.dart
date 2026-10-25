import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/word.dart';

void main() {
  group('Word', () {
    test('guarda texto y pista', () {
      final word = Word(text: 'pirata', hint: 'barco');
      expect(word.text, 'pirata');
      expect(word.hint, 'barco');
    });

    test('recorta espacios de texto y pista', () {
      final word = Word(text: '  playa  ', hint: '  arena  ');
      expect(word.text, 'playa');
      expect(word.hint, 'arena');
    });

    test('texto vacío lanza ArgumentError', () {
      expect(() => Word(text: '   ', hint: 'pista'), throwsArgumentError);
    });

    test('pista vacía lanza ArgumentError', () {
      expect(() => Word(text: 'palabra', hint: '   '), throwsArgumentError);
    });

    test('igualdad por valor', () {
      expect(Word(text: 'mar', hint: 'olas'), Word(text: 'mar', hint: 'olas'));
      expect(
        Word(text: 'mar', hint: 'olas').hashCode,
        Word(text: 'mar', hint: 'olas').hashCode,
      );
    });

    test('palabras distintas no son iguales', () {
      expect(
        Word(text: 'mar', hint: 'olas'),
        isNot(Word(text: 'sol', hint: 'olas')),
      );
    });
  });
}
