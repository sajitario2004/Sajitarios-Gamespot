import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/role.dart';

void main() {
  group('Role', () {
    test('solo existen dos casos: palabra e impostor', () {
      expect(Role.values, <Role>[Role.palabra, Role.impostor]);
    });

    test('impostor.esImpostor es true y sabePalabra es false', () {
      expect(Role.impostor.esImpostor, isTrue);
      expect(Role.impostor.sabePalabra, isFalse);
    });

    test('palabra.sabePalabra es true y esImpostor es false', () {
      expect(Role.palabra.sabePalabra, isTrue);
      expect(Role.palabra.esImpostor, isFalse);
    });
  });
}
