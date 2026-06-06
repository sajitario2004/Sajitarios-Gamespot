import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';

import 'package:sajitarios_gamespot/main.dart';
import 'package:sajitarios_gamespot/core/routing/app_router.dart';
import 'package:sajitarios_gamespot/games/es_un_10_pero/presentation/es_un_10_pero_screen.dart';

/// Auditoría visual: arranca la app real en el simulador y captura una pantalla
/// por cada superficie clave navegando por el router (sin depender de toques).
/// Las capturas las guarda el driver host en `/tmp/audit/NOMBRE.png`.
void main() {
  final binding = IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  testWidgets('auditoría visual: captura de pantallas', (tester) async {
    final container = ProviderContainer();
    addTearDown(container.dispose);

    await tester.pumpWidget(
      UncontrolledProviderScope(
        container: container,
        child: const SajitariosGamespotApp(),
      ),
    );
    // La app siembra la BD en el primer arranque; damos margen.
    await tester.pump(const Duration(milliseconds: 1200));

    final router = container.read(appRouterProvider);

    Future<void> settle() async {
      // PulseGlow es una animación infinita: nunca pumpAndSettle. Bombeamos
      // tiempos fijos generosos para que la navegación, los providers async
      // (FutureProvider de repos) y el primer frame asienten antes de capturar.
      for (var i = 0; i < 6; i++) {
        await tester.pump(const Duration(milliseconds: 400));
      }
    }

    Future<void> capture(String name) async {
      // En iOS hay que convertir la superficie a imagen ANTES de cada captura,
      // si no se captura un frame anterior (desfase).
      await binding.convertFlutterSurfaceToImage();
      await tester.pump();
      await binding.takeScreenshot(name);
    }

    Future<void> shot(String name, String route) async {
      try {
        router.go(route);
        await settle();
        await capture(name);
      } catch (_) {
        // No abortar la auditoría por una pantalla concreta.
      }
    }

    await settle();
    await capture('01_menu');

    await shot('02_impostor_setup', '/impostor');
    await shot('03_impostor_words', '/impostor/words');
    await shot('04_impostor_history', '/impostor/history');
    await shot('05_trivia_setup', '/trivia');
    await shot('06_wavelength_setup', '/wavelength');
    await shot('07_tabu_setup', '/tabu');
    await shot('08_yo_nunca_setup', '/yo-nunca');
    await shot('09_bomba_setup', '/bomba');

    // "Es un 10 pero" no declara ruta (entra vía buildEntryScreen): lo
    // empujamos directamente sobre el navigator raíz del router.
    try {
      router.go('/');
      await settle();
      router.routerDelegate.navigatorKey.currentState?.push(
        MaterialPageRoute<void>(builder: (_) => const EsUn10PeroScreen()),
      );
      await settle();
      await capture('10_es_un_10_pero');
    } catch (_) {}
  });
}
