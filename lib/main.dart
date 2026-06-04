import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'core/i18n/locale_controller.dart';
import 'core/routing/app_router.dart';
import 'core/theme/app_theme.dart';
import 'l10n/app_localizations.dart';

void main() {
  runApp(const ProviderScope(child: SajitariosGamespotApp()));
}

class SajitariosGamespotApp extends ConsumerWidget {
  const SajitariosGamespotApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final router = ref.watch(appRouterProvider);
    final locale = ref.watch(localeProvider);

    return MaterialApp.router(
      title: 'Sajitarios Gamespot',
      debugShowCheckedModeBanner: false,
      // v0.46: tema neón oscuro forzado como experiencia principal. El neón
      // solo luce sobre oscuro, así que `theme`/`darkTheme` son el mismo tema
      // neón y `themeMode` queda fijado en oscuro.
      theme: AppTheme.neon,
      darkTheme: AppTheme.neon,
      themeMode: ThemeMode.dark,
      // i18n (v0.44): delegados y locales soportados generados por gen-l10n.
      // `supportedLocales` lista español (es) primero (idioma por defecto).
      // `locale` sigue al usuario; null = idioma del sistema.
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      locale: locale,
      routerConfig: router,
    );
  }
}
