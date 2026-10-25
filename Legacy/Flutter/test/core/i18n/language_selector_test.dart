import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'package:sajitarios_gamespot/core/i18n/language_selector.dart';
import 'package:sajitarios_gamespot/core/i18n/locale_controller.dart';

import '../../support/localized_app.dart';

void main() {
  setUp(() {
    // Evita depender de plataforma al persistir el idioma elegido.
    SharedPreferences.setMockInitialValues({});
  });

  Widget buildHarness(ProviderContainer container) {
    return UncontrolledProviderScope(
      container: container,
      child: localizedApp(
        const Scaffold(
          appBar: null,
          body: Center(child: LanguageSelectorButton()),
        ),
      ),
    );
  }

  group('LanguageSelectorButton', () {
    testWidgets('muestra el icono de idioma', (tester) async {
      final container = ProviderContainer();
      addTearDown(container.dispose);
      await tester.pumpWidget(buildHarness(container));

      expect(find.byIcon(Icons.language), findsOneWidget);
    });

    testWidgets('al pulsar abre el diálogo con las tres opciones', (
      tester,
    ) async {
      final container = ProviderContainer();
      addTearDown(container.dispose);
      await tester.pumpWidget(buildHarness(container));

      await tester.tap(find.byIcon(Icons.language));
      await tester.pumpAndSettle();

      expect(find.text('Idioma del sistema'), findsOneWidget);
      expect(find.text('Español'), findsOneWidget);
      expect(find.text('Inglés'), findsOneWidget);
    });

    testWidgets('seleccionar "Inglés" cambia el localeProvider a en', (
      tester,
    ) async {
      final container = ProviderContainer();
      addTearDown(container.dispose);
      await tester.pumpWidget(buildHarness(container));

      // Estado inicial: idioma del sistema (null).
      expect(container.read(localeProvider), isNull);

      await tester.tap(find.byIcon(Icons.language));
      await tester.pumpAndSettle();
      await tester.tap(find.text('Inglés'));
      await tester.pumpAndSettle();

      expect(container.read(localeProvider), const Locale('en'));
    });

    testWidgets('seleccionar "Español" fija el locale a es', (tester) async {
      final container = ProviderContainer();
      addTearDown(container.dispose);
      await tester.pumpWidget(buildHarness(container));

      await tester.tap(find.byIcon(Icons.language));
      await tester.pumpAndSettle();
      await tester.tap(find.text('Español'));
      await tester.pumpAndSettle();

      expect(container.read(localeProvider), const Locale('es'));
    });

    testWidgets('seleccionar "Idioma del sistema" deja el locale en null', (
      tester,
    ) async {
      // Arranca con un idioma fijado para comprobar la vuelta al sistema.
      final container = ProviderContainer();
      addTearDown(container.dispose);
      container.read(localeProvider.notifier).setLocale(const Locale('en'));
      await tester.pumpWidget(buildHarness(container));

      await tester.tap(find.byIcon(Icons.language));
      await tester.pumpAndSettle();
      await tester.tap(find.text('Idioma del sistema'));
      await tester.pumpAndSettle();

      expect(container.read(localeProvider), isNull);
    });
  });
}
