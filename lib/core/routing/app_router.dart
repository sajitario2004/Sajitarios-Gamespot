import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../games/_shared/game_registry.dart';
import '../../l10n/app_localizations.dart';
import '../../menu/menu_screen.dart';

/// Configuración de rutas declarativas con go_router.
///
/// Provider para poder inyectar/observar el router desde la app. Las rutas de
/// cada juego se obtienen recorriendo `gameRegistryProvider` y llamando a
/// `GameDescriptor.routes()`: el router no menciona juegos concretos, solo la
/// ruta del menú. Para añadir un juego con flujo de rutas basta registrarlo y
/// sobreescribir `routes()` en su descriptor.
final appRouterProvider = Provider<GoRouter>((ref) {
  return GoRouter(
    initialLocation: '/',
    routes: [
      GoRoute(
        path: '/',
        name: 'menu',
        builder: (context, state) => const MenuScreen(),
      ),
      for (final game in ref.watch(gameRegistryProvider)) ...game.routes(),
    ],
    errorBuilder: (context, state) => const _RouteErrorScreen(),
  );
});

class _RouteErrorScreen extends StatelessWidget {
  const _RouteErrorScreen();

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      appBar: AppBar(title: Text(l10n.rutaNoEncontradaTitulo)),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.error_outline, size: 64),
              const SizedBox(height: 16),
              Text(
                l10n.rutaNoEncontradaMensaje,
                style: Theme.of(context).textTheme.titleLarge,
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 8),
              Text(
                l10n.rutaNoEncontradaAyuda,
                style: Theme.of(context).textTheme.bodyMedium,
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 24),
              FilledButton(
                onPressed: () => GoRouter.of(context).go('/'),
                child: Text(l10n.volverAlMenu),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
