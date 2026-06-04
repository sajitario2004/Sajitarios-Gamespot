import 'package:flame/game.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/audio/audio_service.dart';
import 'package:sajitarios_gamespot/core/audio/mute_button.dart';
import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/es_un_10_pero/domain/card.dart'
    as domain;
import 'package:sajitarios_gamespot/games/es_un_10_pero/presentation/card_flip_game.dart';
import 'package:sajitarios_gamespot/games/es_un_10_pero/presentation/es_un_10_pero_providers.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla del juego "Es un 10 pero".
///
/// La carta se renderiza como un componente **Flame** dentro de un
/// [GameWidget] embebido ([CardFlipGame]). Al pulsar "Sacar carta" se ejecuta
/// el [DrawCardUseCase] (vía `drawCardUseCaseProvider`, que lee `randomProvider`)
/// y se reproduce una animación de volteo (dorso → cara) que revela la nueva
/// carta. El estado de la carta actual vive en Riverpod/Flutter (`_card`) y se
/// mantiene coherente con lo que muestra el juego Flame.
class EsUn10PeroScreen extends ConsumerStatefulWidget {
  const EsUn10PeroScreen({super.key});

  @override
  ConsumerState<EsUn10PeroScreen> createState() => _EsUn10PeroScreenState();
}

class _EsUn10PeroScreenState extends ConsumerState<EsUn10PeroScreen> {
  CardFlipGame? _game;
  domain.Card? _card;

  @override
  void initState() {
    super.initState();
    // Precarga los SFX al entrar a la pantalla (nunca lanza). Tras el primer
    // frame, para tener el `ref` listo y no bloquear el build.
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      ref.read(audioServiceProvider).preload();
    });
  }

  void _drawCard() {
    final drawCard = ref.read(drawCardUseCaseProvider);
    final card = drawCard();
    setState(() => _card = card);
    _game?.flipTo(card);
    // SFX de volteo (respeta el toggle de silencio; nunca rompe la UI).
    ref.read(audioServiceProvider).playCardFlip();
  }

  /// Etiqueta semántica dinámica para la carta renderizada en Flame/Canvas.
  ///
  /// El [GameWidget] pinta sobre un canvas y no genera nodo semántico, así que
  /// describimos manualmente la carta actual (o la ausencia de carta) para los
  /// lectores de pantalla.
  String _semanticLabel(BuildContext context, domain.Card? card) {
    final l10n = AppLocalizations.of(context)!;
    if (card == null) {
      return l10n.cartaSinSacarSemantica;
    }
    return l10n.cartaSemantica(card.value.label, card.suit.displayName);
  }

  CardFlipPalette _paletteFor(ThemeData theme) {
    final scheme = theme.colorScheme;
    // Paleta neón cyberpunk: superficie oscura, marco cian brillante, dorso con
    // patrón magenta y palos en cian/magenta. El "glow" de la carta se deriva
    // del color del marco (cian). El nombre [redSuit]/[blackSuit] se conserva
    // por compatibilidad de la API: aquí mapean a magenta y cian neón.
    return CardFlipPalette(
      cardSurface: AppTheme.surface,
      frameColor: scheme.primary, // cian neón
      backPrimary: AppTheme.surfaceHigh,
      backAccent: scheme.secondary, // magenta neón
      redSuit: scheme.secondary, // palos "rojos" -> magenta neón
      blackSuit: scheme.primary, // palos "negros" -> cian neón
      shadow: scheme.primary.withValues(alpha: 0.55),
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    final palette = _paletteFor(theme);
    // Reaplica la paleta si cambia el tema (claro/oscuro) sin recrear el juego.
    _game?.updatePalette(palette);

    return Scaffold(
      appBar: AppBar(
        title: Text(l10n.esUn10PeroTitulo),
        actions: const [MuteButton()],
      ),
      body: NeonBackground(
        child: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 480),
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Expanded(
                      child: Center(
                        child: AspectRatio(
                          aspectRatio: 5 / 7,
                          child: Semantics(
                            container: true,
                            liveRegion: true,
                            label: _semanticLabel(context, _card),
                            child: Stack(
                              children: [
                                Positioned.fill(
                                  child: GameWidget(
                                    game: _game ??= CardFlipGame(
                                      palette: palette,
                                    ),
                                  ),
                                ),
                                if (_card == null)
                                  const Positioned.fill(
                                    child: _EmptyCardHint(),
                                  ),
                              ],
                            ),
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(height: 24),
                    SizedBox(
                      width: double.infinity,
                      child: NeonGlowWrapper(
                        borderRadius: const BorderRadius.all(
                          Radius.circular(12),
                        ),
                        child: FilledButton.icon(
                          onPressed: _drawCard,
                          icon: const Icon(Icons.casino),
                          label: Text(
                            _card == null
                                ? l10n.sacarCarta
                                : l10n.sacarOtraCarta,
                          ),
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

/// Pista superpuesta sobre el dorso de la carta mientras no se ha sacado
/// ninguna. Se oculta en cuanto aparece la primera carta.
class _EmptyCardHint extends StatelessWidget {
  const _EmptyCardHint();

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return IgnorePointer(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.end,
          children: [
            NeonText(
              AppLocalizations.of(context)!.pistaCartaVacia,
              textAlign: TextAlign.center,
              glowColor: theme.colorScheme.primary,
              style: theme.textTheme.titleMedium?.copyWith(
                color: AppTheme.textPrimary,
                fontWeight: FontWeight.w700,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
