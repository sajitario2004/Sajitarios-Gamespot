import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/i18n/locale_controller.dart';
import 'package:sajitarios_gamespot/main.dart';

void main() {
  testWidgets('La app arranca y muestra el menú principal', (tester) async {
    // Los títulos/descripciones de los juegos ahora se localizan (v0.47). El
    // entorno de test resuelve el locale a `en` por defecto, lo que rompería las
    // aserciones en español; fijamos el idioma a español sobreescribiendo
    // `localeProvider`.
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          localeProvider.overrideWith(() => _FixedLocaleController()),
        ],
        child: const SajitariosGamespotApp(),
      ),
    );
    // El título del menú lleva un resplandor neón pulsante (PulseGlow), una
    // animación infinita y decorativa; `pumpAndSettle` nunca terminaría, así
    // que bombeamos unos frames para asentar el primer build.
    await tester.pump();
    await tester.pump(const Duration(milliseconds: 300));

    expect(find.text('Sajitarios Gamespot'), findsOneWidget);
    // El catálogo ya incluye el juego "Es un 10 pero" (fase F2).
    expect(find.text('Es un 10 pero'), findsOneWidget);
  });
}

/// Controlador de locale fijado en español para tests que arrancan la app real.
class _FixedLocaleController extends LocaleController {
  @override
  Locale? build() => const Locale('es');
}
