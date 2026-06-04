import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';

void main() {
  group('Player', () {
    test('guarda el nombre tal cual', () {
      expect(const Player('Nacho').name, 'Nacho');
    });

    test('dos jugadores con el mismo nombre son iguales', () {
      expect(const Player('Iker'), const Player('Iker'));
      expect(const Player('Iker').hashCode, const Player('Iker').hashCode);
    });

    test('jugadores con distinto nombre no son iguales', () {
      expect(const Player('Nacho'), isNot(const Player('Lucía')));
    });

    test('toString incluye el nombre', () {
      expect(const Player('Lucía').toString(), contains('Lucía'));
    });
  });
}
