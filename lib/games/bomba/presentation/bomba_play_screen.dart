import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_flow_controller.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_routes.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/abandon_game_dialog.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla de juego activo de La Bomba.
///
/// Muestra el PROMPT grande (sílaba o categoría), el nombre del portador actual
/// y un botón "PASAR". La mecha es un [Timer] real propiedad de ESTA pantalla
/// — el controlador [BombaFlowController] expone [fuseSeconds] como objetivo
/// pero NO ejecuta ningún wall-clock (permanece determinista para tests).
///
/// ## Mecha oculta
///
/// El tiempo restante NO se muestra: la bomba explota por sorpresa. Solo se
/// muestra una animación de pulso decorativa para generar tensión.
///
/// ## Ciclo de ronda
///
/// 1. `initState` → [_startFuseTimer] arranca el [Timer].
/// 2. El portador puede pulsar "PASAR" → [BombaFlowController.pasar] avanza
///    el holder. El Timer NO se reinicia.
/// 3. El Timer expira → [_onFuseExpired] → controller.explotar() →
///    fase=explotando. La pantalla muestra brevemente al eliminado y luego
///    llama a [BombaFlowController.continuarTrasExplosion].
/// 4. Si quedan >1 jugadores la fase vuelve a jugando y la pantalla arranca
///    un nuevo Timer automáticamente.
/// 5. Si queda 1 jugador la fase pasa a gameOver y navegamos a game-over.
class BombaPlayScreen extends ConsumerStatefulWidget {
  const BombaPlayScreen({super.key});

  @override
  ConsumerState<BombaPlayScreen> createState() => _BombaPlayScreenState();
}

class _BombaPlayScreenState extends ConsumerState<BombaPlayScreen> {
  Timer? _fuseTimer;
  // Milisegundos transcurridos desde que arrancó el Timer de la ronda actual.
  int _elapsedMs = 0;
  // Target de la mecha en ms para la ronda actual.
  int _fuseMs = 0;

  // Controla si estamos en el breve estado de "explosión visible" en pantalla.
  bool _showingExplosion = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      _startFuseTimer();
    });
  }

  @override
  void dispose() {
    _fuseTimer?.cancel();
    super.dispose();
  }

  void _startFuseTimer() {
    _fuseTimer?.cancel();
    final fuseSeconds = ref.read(bombaFlowControllerProvider).fuseSeconds;
    _fuseMs = (fuseSeconds * 1000).round();
    _elapsedMs = 0;

    const tickMs = 50;
    _fuseTimer = Timer.periodic(const Duration(milliseconds: tickMs), (timer) {
      if (!mounted) {
        timer.cancel();
        return;
      }
      _elapsedMs += tickMs;
      if (_elapsedMs >= _fuseMs) {
        timer.cancel();
        _onFuseExpired();
      }
    });
  }

  void _onFuseExpired() {
    if (!mounted) return;
    final fase = ref.read(bombaFlowControllerProvider).fase;
    if (fase != BombaFase.jugando) return;

    ref.read(bombaFlowControllerProvider.notifier).explotar();

    setState(() => _showingExplosion = true);

    // Muestra la explosión durante 1.5 s y luego continúa.
    Future.delayed(const Duration(milliseconds: 1500), () {
      if (!mounted) return;
      ref.read(bombaFlowControllerProvider.notifier).continuarTrasExplosion();
      setState(() => _showingExplosion = false);

      final newFase = ref.read(bombaFlowControllerProvider).fase;
      if (newFase == BombaFase.gameOver) {
        context.goNamed(kBombaGameOverRouteName);
      } else if (newFase == BombaFase.jugando) {
        _startFuseTimer();
      }
    });
  }

  void _onPasar() {
    ref.read(bombaFlowControllerProvider.notifier).pasar();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    final flowState = ref.watch(bombaFlowControllerProvider);

    final prompt = flowState.currentPrompt;
    final holder = flowState.currentHolder;

    if (prompt == null || holder == null) {
      return Scaffold(body: Center(child: Text(l10n.bombaNoHayPartida)));
    }

    if (_showingExplosion) {
      return _ExplosionOverlay(
        eliminado: flowState.eliminado ?? holder,
        l10n: l10n,
        theme: theme,
      );
    }

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) async {
        if (didPop) return;
        final confirmed = await _confirmarSalida(context, l10n);
        if (confirmed && context.mounted) {
          _fuseTimer?.cancel();
          ref.read(bombaFlowControllerProvider.notifier).reiniciar();
          context.go('/');
        }
      },
      child: Scaffold(
        appBar: AppBar(
          leading: VolverAlMenuButton(
            onPressed: () async {
              final salir = await abandonarPartidaDialog(context);
              if (salir != true || !context.mounted) return;
              _fuseTimer?.cancel();
              ref.read(bombaFlowControllerProvider.notifier).reiniciar();
              if (!context.mounted) return;
              context.go('/');
            },
          ),
          automaticallyImplyLeading: false,
          title: NeonText(
            l10n.bombaTitulo,
            style:
                theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
            glowColor: AppTheme.neonError,
          ),
          actions: [
            Padding(
              padding: const EdgeInsets.only(right: 12),
              child: Chip(
                avatar: Icon(
                  Icons.people_outline,
                  size: 16,
                  color: AppTheme.neonCyan,
                ),
                label: Text(
                  '${flowState.alivePlayers.length}',
                  style: theme.textTheme.labelMedium?.copyWith(
                    color: AppTheme.neonCyan,
                  ),
                ),
                backgroundColor: AppTheme.surfaceHigh,
                side: BorderSide(
                  color: AppTheme.neonCyan.withValues(alpha: 0.4),
                ),
              ),
            ),
          ],
        ),
        body: NeonBackground(
          child: SafeArea(
            child: Center(
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 500),
                child: Padding(
                  padding: const EdgeInsets.all(24),
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      // ── Portador actual ─────────────────────────────────────
                      Semantics(
                        label: l10n.bombaPortadorSemantica(holder),
                        child: NeonText(
                          holder.toUpperCase(),
                          textAlign: TextAlign.center,
                          style: theme.textTheme.headlineSmall?.copyWith(
                            fontWeight: FontWeight.w700,
                            color: AppTheme.textPrimary,
                          ),
                          glowColor: AppTheme.neonCyan,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                      const SizedBox(height: 32),

                      // ── Prompt grande ─────────────────────────────────────
                      PulseGlow(
                        builder: (context, intensity) => NeonPanel(
                          borderColor: AppTheme.neonError.withValues(
                            alpha: intensity.clamp(0.0, 1.0),
                          ),
                          padding: const EdgeInsets.symmetric(
                            horizontal: 32,
                            vertical: 24,
                          ),
                          child: Semantics(
                            label: l10n.bombaPromptSemantica(prompt.texto),
                            child: FittedBox(
                              fit: BoxFit.scaleDown,
                              child: NeonText(
                                prompt.texto.toUpperCase(),
                                textAlign: TextAlign.center,
                                style: theme.textTheme.displayMedium?.copyWith(
                                  fontWeight: FontWeight.w900,
                                  color: AppTheme.textPrimary,
                                ),
                                glowColor: AppTheme.neonError,
                              ),
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(height: 16),

                      // ── Tipo de prompt ────────────────────────────────────
                      Text(
                        flowState.config?.mode == BombaMode.silaba
                            ? l10n.bombaTipoSilaba
                            : l10n.bombaTipoCategoria,
                        style: theme.textTheme.bodyMedium?.copyWith(
                          color: AppTheme.textMuted,
                        ),
                      ),
                      const SizedBox(height: 48),

                      // ── Botón PASAR ───────────────────────────────────────
                      SizedBox(
                        width: double.infinity,
                        child: NeonGlowWrapper(
                          color: AppTheme.neonCyan,
                          borderRadius: const BorderRadius.all(
                            Radius.circular(28),
                          ),
                          child: FilledButton.icon(
                            onPressed: _onPasar,
                            style: FilledButton.styleFrom(
                              minimumSize: const Size.fromHeight(64),
                            ),
                            icon: const Icon(Icons.skip_next_rounded, size: 28),
                            label: Text(
                              l10n.bombaPasar,
                              style: theme.textTheme.titleMedium?.copyWith(
                                fontWeight: FontWeight.w800,
                              ),
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
      ),
    );
  }
}

/// Overlay de explosión que se muestra brevemente cuando la mecha expira.
class _ExplosionOverlay extends StatelessWidget {
  const _ExplosionOverlay({
    required this.eliminado,
    required this.l10n,
    required this.theme,
  });

  final String eliminado;
  final AppLocalizations l10n;
  final ThemeData theme;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: NeonBackground(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(
                  Icons.local_fire_department,
                  size: 80,
                  color: AppTheme.neonError,
                ),
                const SizedBox(height: 24),
                Semantics(
                  liveRegion: true,
                  child: NeonText(
                    l10n.bombaExplosionTitulo,
                    textAlign: TextAlign.center,
                    style: theme.textTheme.headlineMedium?.copyWith(
                      fontWeight: FontWeight.w900,
                    ),
                    glowColor: AppTheme.neonError,
                  ),
                ),
                const SizedBox(height: 16),
                Semantics(
                  liveRegion: true,
                  child: NeonPanel(
                    borderColor: AppTheme.neonError,
                    padding: const EdgeInsets.all(20),
                    child: NeonText(
                      eliminado,
                      textAlign: TextAlign.center,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                      style: theme.textTheme.displaySmall?.copyWith(
                        fontWeight: FontWeight.w900,
                        color: AppTheme.textPrimary,
                      ),
                      glowColor: AppTheme.neonError,
                    ),
                  ),
                ),
                const SizedBox(height: 16),
                Text(
                  l10n.bombaEliminado,
                  style: theme.textTheme.bodyLarge?.copyWith(
                    color: AppTheme.textMuted,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

Future<bool> _confirmarSalida(
  BuildContext context,
  AppLocalizations l10n,
) async {
  final result = await showDialog<bool>(
    context: context,
    builder: (dialogContext) => AlertDialog(
      title: Text(l10n.salirDeLaPartidaTitulo),
      content: Text(l10n.salirDeLaPartidaMensaje),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(dialogContext).pop(false),
          child: Text(l10n.seguirJugando),
        ),
        FilledButton(
          onPressed: () => Navigator.of(dialogContext).pop(true),
          child: Text(l10n.salir),
        ),
      ],
    ),
  );
  return result ?? false;
}
