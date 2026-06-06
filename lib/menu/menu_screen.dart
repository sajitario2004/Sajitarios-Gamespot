import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../core/assets/assets.dart';
import '../core/i18n/language_selector.dart';
import '../core/theme/app_theme.dart';
import '../core/widgets/neon.dart';
import '../games/_shared/game_descriptor.dart';
import '../games/_shared/game_registry.dart';
import '../l10n/app_localizations.dart';

/// Pantalla principal: hub de minijuegos.
///
/// Lee la lista de juegos desde `gameRegistryProvider` y pinta una grid.
/// No conoce ningún juego concreto (regla de extensibilidad).
class MenuScreen extends ConsumerWidget {
  const MenuScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final games = ref.watch(gameRegistryProvider);

    return Scaffold(
      appBar: AppBar(
        // Título de la app con resplandor neón cian pulsante. Sigue siendo un
        // Text por dentro (find.text(appTitle) funciona).
        title: PulseGlow(
          minIntensity: 0.8,
          maxIntensity: 1.3,
          builder: (context, intensity) => NeonText(
            AppLocalizations.of(context)!.appTitle,
            intensity: intensity,
            style: Theme.of(context).appBarTheme.titleTextStyle,
          ),
        ),
        actions: const [LanguageSelectorButton()],
      ),
      // Fondo neón oscuro con degradado radial y rejilla tenue.
      body: NeonBackground(
        child: SafeArea(
          child: Column(
            children: [
              const _MenuHeader(),
              Expanded(
                child: games.isEmpty
                    ? const _EmptyCatalog()
                    : _GameGrid(games: games),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

/// Cabecera decorativa (opcional) del menú. Muestra `menu_header.png` como
/// banner; si la imagen no carga, se oculta por completo (fallback) sin afectar
/// al resto de la pantalla.
class _MenuHeader extends StatelessWidget {
  const _MenuHeader();

  @override
  Widget build(BuildContext context) {
    return ExcludeSemantics(
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxHeight: 140),
        child: Image.asset(
          Assets.images.menuHeader,
          width: double.infinity,
          fit: BoxFit.cover,
          // Fallback: si el PNG falta o no decodifica, no mostramos nada.
          errorBuilder: (context, error, stackTrace) => const SizedBox.shrink(),
        ),
      ),
    );
  }
}

class _GameGrid extends StatelessWidget {
  const _GameGrid({required this.games});

  final List<GameDescriptor> games;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final crossAxisCount = constraints.maxWidth > 600 ? 3 : 2;
        // Con texto muy grande las tarjetas necesitan más alto vertical para
        // no desbordar; reducimos el aspect ratio según el textScaler.
        final textScale = MediaQuery.textScalerOf(
          context,
        ).scale(1).clamp(1.0, 2.0);
        // Con texto grande damos celdas más altas (suelo más bajo) para que el
        // contenido de la tarjeta nunca desborde a textScaler 2.0.
        final childAspectRatio = (0.9 / textScale).clamp(0.46, 0.9);
        return Center(
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 900),
            child: GridView.builder(
              padding: const EdgeInsets.all(16),
              gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: crossAxisCount,
                crossAxisSpacing: 16,
                mainAxisSpacing: 16,
                childAspectRatio: childAspectRatio,
              ),
              itemCount: games.length,
              itemBuilder: (context, index) =>
                  _GameCard(game: games[index], index: index),
            ),
          ),
        );
      },
    );
  }
}

class _GameCard extends StatelessWidget {
  const _GameCard({required this.game, required this.index});

  final GameDescriptor game;

  /// Posición en la grid; se usa para alternar el color de acento neón
  /// (cian / magenta) de cada tarjeta.
  final int index;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    // Acento alterno: par -> cian, impar -> magenta. Da el efecto "full neon"
    // con tarjetas que brillan en los dos colores principales.
    final accent = index.isEven ? AppTheme.neonCyan : AppTheme.neonMagenta;
    const radius = BorderRadius.all(Radius.circular(20));

    // Textos localizados del juego (el descriptor decide su traducción; el menú
    // sigue sin conocer juegos concretos).
    final title = game.localizedTitle(context);
    final description = game.localizedDescription(context);

    // El icono se encoge con el tamaño de texto del sistema para que la tarjeta
    // (en celda de aspecto fijo) no desborde a textScaler alto.
    final textScale = MediaQuery.textScalerOf(context).scale(1).clamp(1.0, 2.0);
    final iconSize = (56.0 / textScale).clamp(34.0, 56.0);
    final gap = (16.0 / textScale).clamp(8.0, 16.0);

    return Semantics(
      button: true,
      label: AppLocalizations.of(context)!.jugarA(title, description),
      // Glow exterior del color de acento alrededor de la tarjeta tipada.
      child: NeonGlowWrapper(
        color: accent,
        borderRadius: radius,
        child: Card(
          margin: EdgeInsets.zero,
          color: AppTheme.surface,
          shape: RoundedRectangleBorder(
            borderRadius: radius,
            side: BorderSide(color: accent, width: 1.5),
          ),
          child: InkWell(
            borderRadius: radius,
            onTap: () {
              // Los juegos que declaran un `routeName` gestionan su flujo con
              // go_router y se entran navegando por nombre de ruta (para que su
              // navegación interna opere sobre el árbol del router). El resto se
              // empuja imperativamente con su pantalla de entrada. El menú sigue
              // sin conocer juegos concretos: solo lee el descriptor.
              final routeName = game.routeName;
              if (routeName != null) {
                context.goNamed(routeName);
              } else {
                Navigator.of(context).push(
                  MaterialPageRoute<void>(
                    builder: (context) => game.buildEntryScreen(context),
                  ),
                );
              }
            },
            child: Padding(
              padding: const EdgeInsets.all(16),
              // El texto y el icono ya se anuncian por el `label` del Semantics
              // padre; los excluimos para no duplicar la lectura del lector de
              // pantalla.
              child: ExcludeSemantics(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    // Icono del juego con resplandor del color de acento.
                    DecoratedBox(
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        boxShadow: neonGlow(color: accent, intensity: 0.9),
                      ),
                      child: Icon(
                        game.icon,
                        size: iconSize,
                        color: accent,
                        shadows: neonTextShadows(color: accent, intensity: 0.8),
                      ),
                    ),
                    SizedBox(height: gap),
                    Flexible(
                      // Título del juego con glow del color de acento. Sigue
                      // siendo un Text por dentro (find.text funciona).
                      child: NeonText(
                        title,
                        glowColor: accent,
                        intensity: 0.8,
                        textAlign: TextAlign.center,
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                        style: theme.textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.w800,
                          color: AppTheme.textPrimary,
                        ),
                      ),
                    ),
                    const SizedBox(height: 8),
                    Flexible(
                      child: Text(
                        description,
                        textAlign: TextAlign.center,
                        maxLines: 3,
                        overflow: TextOverflow.ellipsis,
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: theme.colorScheme.onSurfaceVariant,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _EmptyCatalog extends StatelessWidget {
  const _EmptyCatalog();

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: NeonPanel(
          padding: const EdgeInsets.all(28),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(
                Icons.videogame_asset_outlined,
                size: 72,
                color: theme.colorScheme.primary,
                shadows: neonTextShadows(color: AppTheme.neonCyan),
              ),
              const SizedBox(height: 16),
              NeonText(
                AppLocalizations.of(context)!.menuVacioTitulo,
                style: theme.textTheme.titleLarge,
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 8),
              Text(
                AppLocalizations.of(context)!.menuVacioMensaje,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      ),
    );
  }
}
