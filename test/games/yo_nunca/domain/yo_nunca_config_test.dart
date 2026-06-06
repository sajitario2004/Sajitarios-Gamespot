/// Tests for [YoNuncaConfig]: validation and equality.
library;

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/games/yo_nunca/domain/intensidad.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/yo_nunca_config.dart';

void main() {
  group('YoNuncaConfig.create', () {
    test('succeeds with only suave', () {
      final result = YoNuncaConfig.create(intensidades: {Intensidad.suave});
      expect(result.isSuccess, isTrue);
      expect(result.config!.intensidades, {Intensidad.suave});
    });

    test('succeeds with only picante', () {
      final result = YoNuncaConfig.create(intensidades: {Intensidad.picante});
      expect(result.isSuccess, isTrue);
      expect(result.config!.intensidades, {Intensidad.picante});
    });

    test('succeeds with both intensidades', () {
      final result = YoNuncaConfig.create(
        intensidades: {Intensidad.suave, Intensidad.picante},
      );
      expect(result.isSuccess, isTrue);
      expect(
        result.config!.intensidades,
        containsAll([Intensidad.suave, Intensidad.picante]),
      );
    });

    test('fails with empty intensidades', () {
      final result = YoNuncaConfig.create(intensidades: {});
      expect(result.isSuccess, isFalse);
      expect(result.error, YoNuncaConfigError.sinIntensidades);
    });

    test('intensidades set is unmodifiable', () {
      final result = YoNuncaConfig.create(intensidades: {Intensidad.suave});
      expect(
        () => result.config!.intensidades.add(Intensidad.picante),
        throwsUnsupportedError,
      );
    });
  });

  group('YoNuncaConfig equality', () {
    test('equal configs with same intensidades', () {
      final a = YoNuncaConfig.create(
        intensidades: {Intensidad.suave, Intensidad.picante},
      ).config!;
      final b = YoNuncaConfig.create(
        intensidades: {Intensidad.picante, Intensidad.suave},
      ).config!;
      expect(a, equals(b));
      expect(a.hashCode, b.hashCode);
    });

    test('not equal when intensidades differ', () {
      final a = YoNuncaConfig.create(intensidades: {Intensidad.suave}).config!;
      final b = YoNuncaConfig.create(
        intensidades: {Intensidad.picante},
      ).config!;
      expect(a, isNot(equals(b)));
    });
  });
}
