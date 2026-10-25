import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/widgets/neon.dart';

/// Tests de [PulseGlow] (sistema de diseño neón).
///
/// IMPORTANTE: `PulseGlow` anima de forma infinita (`repeat(reverse: true)`),
/// así que NUNCA se usa `pumpAndSettle` (colgaría). Se bombea con `pump()` de
/// duración fija para muestrear la intensidad animada.
void main() {
  /// Monta un [PulseGlow] que captura la última intensidad emitida por el
  /// builder en [out]. [disableAnimations] simula *reduce motion* vía MediaQuery.
  Widget buildHarness(
    List<double> out, {
    double minIntensity = 0.7,
    double maxIntensity = 1.2,
    Duration duration = const Duration(milliseconds: 1600),
    bool disableAnimations = false,
    Key? pulseKey,
  }) {
    return MaterialApp(
      home: MediaQuery(
        data: MediaQueryData(disableAnimations: disableAnimations),
        child: Scaffold(
          body: PulseGlow(
            key: pulseKey,
            minIntensity: minIntensity,
            maxIntensity: maxIntensity,
            duration: duration,
            builder: (context, intensity) {
              out.add(intensity);
              return const SizedBox(width: 10, height: 10);
            },
          ),
        ),
      ),
    );
  }

  group('PulseGlow (animado)', () {
    testWidgets('emite una intensidad dentro de [min, max] a mitad de ciclo', (
      tester,
    ) async {
      final values = <double>[];
      await tester.pumpWidget(buildHarness(values));
      // Muestra a mitad del ciclo de la animación (la curva easeInOut mantiene
      // el valor dentro de los extremos del Tween).
      await tester.pump(const Duration(milliseconds: 800));

      expect(values, isNotEmpty);
      final last = values.last;
      expect(last, greaterThanOrEqualTo(0.7));
      expect(last, lessThanOrEqualTo(1.2));
    });

    testWidgets('la intensidad cambia entre frames (está animando)', (
      tester,
    ) async {
      final values = <double>[];
      await tester.pumpWidget(buildHarness(values));
      await tester.pump(const Duration(milliseconds: 200));
      final a = values.last;
      await tester.pump(const Duration(milliseconds: 400));
      final b = values.last;
      expect(a, isNot(equals(b)));
    });
  });

  group('PulseGlow.withChild', () {
    testWidgets('reenvía el child estático y emite intensidad en [min, max]', (
      tester,
    ) async {
      final values = <double>[];
      var childBuilds = 0;
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: PulseGlow.withChild(
              child: Builder(
                builder: (_) {
                  childBuilds++;
                  return const Text('estatico');
                },
              ),
              childBuilder: (context, intensity, child) {
                values.add(intensity);
                return child;
              },
            ),
          ),
        ),
      );
      await tester.pump(const Duration(milliseconds: 800));

      expect(find.text('estatico'), findsOneWidget);
      // El child estático se construye una sola vez aunque haya varios frames.
      expect(childBuilds, 1);
      expect(values.last, inInclusiveRange(0.7, 1.2));
    });
  });

  group('PulseGlow reduce-motion (disableAnimations)', () {
    testWidgets('emite intensidad fija (media) y no anima', (tester) async {
      final values = <double>[];
      await tester.pumpWidget(buildHarness(values, disableAnimations: true));
      await tester.pump(const Duration(milliseconds: 800));

      // Media entre 0.7 y 1.2 = 0.95, constante en cada frame.
      expect(values, isNotEmpty);
      for (final v in values) {
        expect(v, closeTo(0.95, 1e-9));
      }
    });

    testWidgets('withChild también respeta reduce-motion (intensidad fija)', (
      tester,
    ) async {
      final values = <double>[];
      await tester.pumpWidget(
        MaterialApp(
          home: MediaQuery(
            data: const MediaQueryData(disableAnimations: true),
            child: Scaffold(
              body: PulseGlow.withChild(
                child: const Text('x'),
                childBuilder: (context, intensity, child) {
                  values.add(intensity);
                  return child;
                },
              ),
            ),
          ),
        ),
      );
      await tester.pump(const Duration(milliseconds: 400));
      expect(values, everyElement(closeTo(0.95, 1e-9)));
    });
  });

  group('PulseGlow didUpdateWidget', () {
    testWidgets('cambiar min/max reconstruye la animación con nuevo rango', (
      tester,
    ) async {
      final values = <double>[];

      Widget build(double min, double max) {
        return MaterialApp(
          home: Scaffold(
            body: PulseGlow(
              minIntensity: min,
              maxIntensity: max,
              builder: (context, intensity) {
                values.add(intensity);
                return const SizedBox.shrink();
              },
            ),
          ),
        );
      }

      await tester.pumpWidget(build(0.7, 1.2));
      await tester.pump(const Duration(milliseconds: 100));

      // Cambia el rango: didUpdateWidget reconstruye el Tween.
      values.clear();
      await tester.pumpWidget(build(0.2, 0.4));
      await tester.pump(const Duration(milliseconds: 100));

      expect(values, isNotEmpty);
      for (final v in values) {
        expect(v, inInclusiveRange(0.2, 0.4));
      }
    });

    testWidgets('cambiar la duración no rompe la animación', (tester) async {
      final values = <double>[];

      Widget build(Duration d) {
        return MaterialApp(
          home: Scaffold(
            body: PulseGlow(
              duration: d,
              builder: (context, intensity) {
                values.add(intensity);
                return const SizedBox.shrink();
              },
            ),
          ),
        );
      }

      await tester.pumpWidget(build(const Duration(milliseconds: 1600)));
      await tester.pump(const Duration(milliseconds: 100));
      await tester.pumpWidget(build(const Duration(milliseconds: 800)));
      await tester.pump(const Duration(milliseconds: 100));

      expect(tester.takeException(), isNull);
      expect(values.last, inInclusiveRange(0.7, 1.2));
    });
  });
}
