import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Helpers de test para renderizar widgets bajo el locale español con los
/// delegados de [AppLocalizations].
///
/// Con i18n (v0.45) todos los textos de UI vienen de [AppLocalizations]. El
/// entorno de test suele resolver el locale a `en`, lo que rompería las
/// aserciones de texto en español. Estos helpers fijan `locale: Locale('es')`
/// y añaden los delegados para que `AppLocalizations.of(context)!` funcione y
/// los textos se rendericen en español.

/// Envuelve un widget en un [MaterialApp] localizado en español, con [child]
/// como `home`.
Widget localizedApp(Widget child) {
  return MaterialApp(
    locale: const Locale('es'),
    localizationsDelegates: AppLocalizations.localizationsDelegates,
    supportedLocales: AppLocalizations.supportedLocales,
    home: child,
  );
}

/// Igual que [localizedApp] pero usando [MaterialApp.router] con el [router]
/// dado, para tests que ejercitan la navegación de `go_router`.
Widget localizedRouterApp(GoRouter router) {
  return MaterialApp.router(
    locale: const Locale('es'),
    localizationsDelegates: AppLocalizations.localizationsDelegates,
    supportedLocales: AppLocalizations.supportedLocales,
    routerConfig: router,
  );
}
