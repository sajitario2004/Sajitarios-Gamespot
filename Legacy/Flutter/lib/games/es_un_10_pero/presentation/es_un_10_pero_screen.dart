import 'dart:async';

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
import 'package:sajitarios_gamespot/games/_shared/presentation/rules_screen.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
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

  /// Duración total de la cuenta atrás (en segundos) antes de revelar la carta.
  static const int _countdownStart = 5;

  /// Timer de la cuenta atrás. `null` cuando no hay cuenta en curso.
  Timer? _countdownTimer;

  /// Segundos restantes de la cuenta atrás (5..1). `null` = sin cuenta activa.
  int? _countdown;

  /// `true` mientras la cuenta atrás está en curso: el botón queda
  /// deshabilitado hasta que termina y se revela la carta.
  bool get _countingDown => _countdown != null;

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

  @override
  void dispose() {
    _countdownTimer?.cancel();
    super.dispose();
  }

  /// Inicia la cuenta atrás de [_countdownStart] segundos. El botón queda
  /// deshabilitado durante la cuenta; al llegar a 0 se revela la carta.
  void _startCountdown() {
    if (_countingDown) return;
    setState(() => _countdown = _countdownStart);
    _countdownTimer?.cancel();
    _countdownTimer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (!mounted) {
        timer.cancel();
        return;
      }
      final next = (_countdown ?? 0) - 1;
      if (next <= 0) {
        timer.cancel();
        setState(() => _countdown = null);
        _revealCard();
      } else {
        setState(() => _countdown = next);
      }
    });
  }

  /// Ejecuta el caso de uso de robo, anima el volteo y reproduce el SFX. Se
  /// invoca al terminar la cuenta atrás (segundo 0).
  void _revealCard() {
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
        leading: VolverAlMenuButton(
          onPressed: () => Navigator.of(context).pop(),
        ),
        title: Text(l10n.esUn10PeroTitulo),
        actions: [
          IconButton(
            tooltip: l10n.comoSeJuega,
            icon: const Icon(Icons.help_outline),
            onPressed: () => Navigator.of(context).push(
              MaterialPageRoute<void>(
                builder: (_) => RulesScreen(
                  gameTitle: l10n.esUn10PeroTitulo,
                  steps: [
                    l10n.reglasEsUn10Pero1,
                    l10n.reglasEsUn10Pero2,
                    l10n.reglasEsUn10Pero3,
                    l10n.reglasEsUn10Pero4,
                  ],
                ),
              ),
            ),
          ),
          const MuteButton(),
        ],
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
                            label: _countingDown
                                ? l10n.sacandoCartaEn
                                : _semanticLabel(context, _card),
                            child: Stack(
                              children: [
                                Positioned.fill(
                                  child: GameWidget(
                                    game: _game ??= CardFlipGame(
                                      palette: palette,
                                    ),
                                  ),
                                ),
                                if (_card == null && !_countingDown)
                                  const Positioned.fill(
                                    child: _EmptyCardHint(),
                                  ),
                                if (_countingDown)
                                  Positioned.fill(
                                    child: _CountdownOverlay(
                                      seconds: _countdown!,
                                    ),
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
                          onPressed: _countingDown ? null : _startCountdown,
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
              maxLines: 3,
              overflow: TextOverflow.ellipsis,
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

/// Superposición de cuenta atrás sobre el área de la carta. Atenúa la carta y
/// muestra, en grande y con estilo neón, los segundos restantes (5..1) junto a
/// la etiqueta "Sacando carta en...".
class _CountdownOverlay extends StatelessWidget {
  const _CountdownOverlay({required this.seconds});

  /// Segundos restantes que se muestran en grande (5..1).
  final int seconds;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final glow = theme.colorScheme.primary;
    return IgnorePointer(
      child: DecoratedBox(
        decoration: BoxDecoration(
          color: AppTheme.background.withValues(alpha: 0.72),
          borderRadius: const BorderRadius.all(Radius.circular(24)),
        ),
        child: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              NeonText(
                AppLocalizations.of(context)!.sacandoCartaEn,
                textAlign: TextAlign.center,
                glowColor: glow,
                style: theme.textTheme.titleMedium?.copyWith(
                  color: AppTheme.textPrimary,
                  fontWeight: FontWeight.w700,
                ),
              ),
              const SizedBox(height: 16),
              FittedBox(
                fit: BoxFit.scaleDown,
                child: NeonText(
                  '$seconds',
                  textAlign: TextAlign.center,
                  glowColor: glow,
                  style: theme.textTheme.displayLarge?.copyWith(
                    color: AppTheme.textPrimary,
                    fontWeight: FontWeight.w900,
                    fontSize: 96,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
