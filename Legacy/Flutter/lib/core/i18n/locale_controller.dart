/// Control del idioma (locale) elegido por el usuario.
///
/// Convenciones (Riverpod 3.x): un [Notifier] expuesto con un [NotifierProvider]
/// ([localeProvider]). El estado es un [Locale]? donde:
/// - `null` = seguir el idioma del sistema (comportamiento por defecto).
/// - un [Locale] concreto (p. ej. `Locale('es')`) = idioma fijado por el usuario.
///
/// La elección se persiste con `shared_preferences` (clave [_prefsKey]) y se
/// recarga al arrancar. `MaterialApp.router` observa `ref.watch(localeProvider)`
/// como su `locale`, de modo que cambiarlo reconstruye la app en caliente.
///
/// Tests: usa `SharedPreferences.setMockInitialValues({...})` antes de leer el
/// provider para no depender de plataforma.
library;

import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Clave de `shared_preferences` donde se guarda el código de idioma elegido.
@visibleForTesting
const String localePreferenceKey = 'app_locale';

/// Notifier del idioma elegido por el usuario.
///
/// El estado inicial es `null` (idioma del sistema). Al construirse, lanza la
/// carga asíncrona desde `shared_preferences`; si había un idioma guardado, lo
/// aplica. Mutar el idioma actualiza el estado y persiste de inmediato.
class LocaleController extends Notifier<Locale?> {
  @override
  Locale? build() {
    // Carga diferida del valor persistido. No bloquea el primer frame: arranca
    // con el idioma del sistema y, si había uno guardado, se aplica al resolver.
    _cargarGuardado();
    return null;
  }

  Future<void> _cargarGuardado() async {
    final prefs = await SharedPreferences.getInstance();
    final code = prefs.getString(localePreferenceKey);
    if (code == null || code.isEmpty) return;
    // Valida que el código persistido siga siendo un idioma soportado. Si no lo
    // es (idioma retirado, dato corrupto), se ignora y se limpia la preferencia.
    if (_esSoportado(code)) {
      state = Locale(code);
    } else {
      await prefs.remove(localePreferenceKey);
    }
  }

  /// `true` si [code] coincide con el `languageCode` de algún idioma soportado
  /// ([AppLocalizations.supportedLocales]).
  bool _esSoportado(String code) {
    for (final locale in AppLocalizations.supportedLocales) {
      if (locale.languageCode == code) return true;
    }
    return false;
  }

  /// Fija el idioma de la app a [locale] y lo persiste.
  ///
  /// Pasar `null` vuelve al idioma del sistema y borra la preferencia guardada.
  Future<void> setLocale(Locale? locale) async {
    state = locale;
    final prefs = await SharedPreferences.getInstance();
    if (locale == null) {
      await prefs.remove(localePreferenceKey);
    } else {
      await prefs.setString(localePreferenceKey, locale.languageCode);
    }
  }

  /// Vuelve a seguir el idioma del sistema (equivale a `setLocale(null)`).
  Future<void> usarIdiomaDelSistema() => setLocale(null);
}

/// Provider del idioma elegido (`null` = idioma del sistema).
///
/// La UI lee `ref.watch(localeProvider)` y muta con
/// `ref.read(localeProvider.notifier).setLocale(...)`. Sobreescribible en tests.
final localeProvider = NotifierProvider<LocaleController, Locale?>(
  LocaleController.new,
);
