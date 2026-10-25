import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'package:sajitarios_gamespot/core/i18n/locale_controller.dart';

void main() {
  group('LocaleController', () {
    setUp(() {
      SharedPreferences.setMockInitialValues(<String, Object>{});
    });

    test('por defecto el idioma es null (sigue al sistema)', () {
      final container = ProviderContainer();
      addTearDown(container.dispose);

      expect(container.read(localeProvider), isNull);
    });

    test('setLocale fija el idioma y lo persiste', () async {
      final container = ProviderContainer();
      addTearDown(container.dispose);

      await container
          .read(localeProvider.notifier)
          .setLocale(const Locale('en'));

      expect(container.read(localeProvider), const Locale('en'));
      final prefs = await SharedPreferences.getInstance();
      expect(prefs.getString(localePreferenceKey), 'en');
    });

    test('usarIdiomaDelSistema vuelve a null y borra la preferencia', () async {
      SharedPreferences.setMockInitialValues(<String, Object>{
        localePreferenceKey: 'es',
      });
      final container = ProviderContainer();
      addTearDown(container.dispose);

      await container
          .read(localeProvider.notifier)
          .setLocale(const Locale('es'));
      await container.read(localeProvider.notifier).usarIdiomaDelSistema();

      expect(container.read(localeProvider), isNull);
      final prefs = await SharedPreferences.getInstance();
      expect(prefs.getString(localePreferenceKey), isNull);
    });

    test('carga el idioma guardado al construirse', () async {
      SharedPreferences.setMockInitialValues(<String, Object>{
        localePreferenceKey: 'en',
      });
      final container = ProviderContainer();
      addTearDown(container.dispose);

      // Fuerza la construcción del provider (lanza la carga diferida).
      expect(container.read(localeProvider), isNull);
      // La carga es asíncrona; tras resolverla el estado se aplica.
      await Future<void>.delayed(Duration.zero);

      expect(container.read(localeProvider), const Locale('en'));
    });
  });
}
