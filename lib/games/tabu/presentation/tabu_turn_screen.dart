import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/abandon_game_dialog.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_flow_controller.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_routes.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla del turno activo de Tabú.
///
/// Muestra la palabra secreta, las palabras prohibidas, un COUNTDOWN real y
/// tres botones grandes: Acierto / Saltar / Falta.
///
/// El countdown es un [Timer.periodic] de 1 segundo propiedad de ESTA
/// pantalla, igual que en "Es un 10 pero". El controlador [TabuFlowController]
/// no tiene ningún Timer — es puro y determinista para tests. Al expirar el
/// timer, la pantalla llama a [TabuFlowController.terminarTurno].
class TabuTurnScreen extends ConsumerStatefulWidget {
  const TabuTurnScreen({super.key});

  @override
  ConsumerState<TabuTurnScreen> createState() => _TabuTurnScreenState();
}

class _TabuTurnScreenState extends ConsumerState<TabuTurnScreen> {
  Timer? _timer;
  int _secondsLeft = 0;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      _startTimer();
    });
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  void _startTimer() {
    final duration = ref.read(tabuFlowControllerProvider).turnDurationSeconds;
    setState(() => _secondsLeft = duration);
    _timer?.cancel();
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (!mounted) {
        timer.cancel();
        return;
      }
      final next = _secondsLeft - 1;
      if (next <= 0) {
        timer.cancel();
        setState(() => _secondsLeft = 0);
        _onTimerExpired();
      } else {
        setState(() => _secondsLeft = next);
      }
    });
  }

  void _onTimerExpired() {
    ref.read(tabuFlowControllerProvider.notifier).terminarTurno();
    _navigateAfterTurn();
  }

  void _navigateAfterTurn() {
    if (!mounted) return;
    final fase = ref.read(tabuFlowControllerProvider).fase;
    if (fase == TabuFase.gameOver) {
      context.goNamed(kTabuGameOverRouteName);
    } else {
      context.goNamed(kTabuScoreboardRouteName);
    }
  }

  void _onAcierto() {
    ref.read(tabuFlowControllerProvider.notifier).acierto();
  }

  void _onSaltar() {
    ref.read(tabuFlowControllerProvider.notifier).saltar();
  }

  void _onFalta() {
    ref.read(tabuFlowControllerProvider.notifier).falta();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    final flowState = ref.watch(tabuFlowControllerProvider);

    final palabra = flowState.palabraActual;
    final equipoNombre = flowState.nombreEquipoActual ?? '';
    final aciertos = flowState.aciertosTurnoActual;

    if (palabra == null) {
      return Scaffold(body: Center(child: Text(l10n.tabuNoHayPartida)));
    }

    // Color del countdown: verde > 10s, amarillo 6-10s, rojo <= 5s.
    final countdownColor = _secondsLeft > 10
        ? AppTheme.neonCyan
        : _secondsLeft > 5
        ? AppTheme.neonYellow
        : AppTheme.neonError;

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) async {
        if (didPop) return;
        final confirmed = await _confirmarSalida(context, l10n);
        if (confirmed && context.mounted) {
          _timer?.cancel();
          ref.read(tabuFlowControllerProvider.notifier).reiniciar();
          context.go('/');
        }
      },
      child: Scaffold(
        appBar: AppBar(
          leading: VolverAlMenuButton(
            onPressed: () async {
              final salir = await abandonarPartidaDialog(context);
              if (salir != true || !context.mounted) return;
              _timer?.cancel();
              ref.read(tabuFlowControllerProvider.notifier).reiniciar();
              if (!context.mounted) return;
              context.go('/');
            },
          ),
          automaticallyImplyLeading: false,
          title: NeonText(
            l10n.tabuTitulo,
            style:
                theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
            glowColor: AppTheme.neonCyan,
          ),
          actions: [
            // Contador de aciertos del turno.
            Padding(
              padding: const EdgeInsets.only(right: 16),
              child: Semantics(
                liveRegion: true,
                child: NeonText(
                  l10n.tabuAciertosContador(aciertos),
                  style: theme.textTheme.titleMedium,
                  glowColor: AppTheme.neonCyan,
                ),
              ),
            ),
          ],
        ),
        body: NeonBackground(
          child: SafeArea(
            child: Center(
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 600),
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    children: [
                      // ── Equipo y countdown ─────────────────────────────────
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Flexible(
                            child: NeonText(
                              equipoNombre,
                              style: theme.textTheme.titleMedium,
                              glowColor: AppTheme.neonViolet,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ),
                          Semantics(
                            label: l10n.tabuTiempoRestante(_secondsLeft),
                            child: NeonText(
                              '$_secondsLeft',
                              style: theme.textTheme.headlineLarge?.copyWith(
                                fontWeight: FontWeight.w900,
                              ),
                              glowColor: countdownColor,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),

                      // ── Palabra secreta ────────────────────────────────────
                      Expanded(
                        child: NeonPanel(
                          borderColor: AppTheme.neonViolet,
                          padding: const EdgeInsets.all(20),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              Semantics(
                                header: true,
                                child: FittedBox(
                                  fit: BoxFit.scaleDown,
                                  child: NeonText(
                                    palabra.palabra.toUpperCase(),
                                    textAlign: TextAlign.center,
                                    style: theme.textTheme.displaySmall
                                        ?.copyWith(
                                          fontWeight: FontWeight.w900,
                                          color: AppTheme.textPrimary,
                                        ),
                                    glowColor: AppTheme.neonViolet,
                                  ),
                                ),
                              ),
                              const Divider(height: 24),
                              Text(
                                l10n.tabuProhibidas,
                                style: theme.textTheme.labelLarge?.copyWith(
                                  color: AppTheme.neonError,
                                ),
                              ),
                              const SizedBox(height: 8),
                              Expanded(
                                child: ListView.separated(
                                  itemCount: palabra.prohibidas.length,
                                  separatorBuilder: (context, index) =>
                                      const SizedBox(height: 6),
                                  itemBuilder: (_, i) => Text(
                                    palabra.prohibidas[i],
                                    maxLines: 2,
                                    overflow: TextOverflow.ellipsis,
                                    style: theme.textTheme.bodyLarge?.copyWith(
                                      color: AppTheme.textPrimary,
                                      fontWeight: FontWeight.w600,
                                    ),
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                      const SizedBox(height: 16),

                      // ── Botones de acción ──────────────────────────────────
                      Row(
                        children: [
                          Expanded(
                            child: Semantics(
                              hint: l10n.tabuAciertoHint,
                              child: _ActionButton(
                                label: l10n.tabuAcierto,
                                icon: Icons.check_circle_outline,
                                color: AppTheme.neonGreen,
                                onTap: _onAcierto,
                              ),
                            ),
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Semantics(
                              hint: l10n.tabuSaltarHint,
                              child: _ActionButton(
                                label: l10n.tabuSaltar,
                                icon: Icons.skip_next_outlined,
                                color: AppTheme.neonCyan,
                                onTap: _onSaltar,
                              ),
                            ),
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Semantics(
                              hint: l10n.tabuFaltaHint,
                              child: _ActionButton(
                                label: l10n.tabuFalta,
                                icon: Icons.cancel_outlined,
                                color: AppTheme.neonError,
                                onTap: _onFalta,
                              ),
                            ),
                          ),
                        ],
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

class _ActionButton extends StatelessWidget {
  const _ActionButton({
    required this.label,
    required this.icon,
    required this.color,
    required this.onTap,
  });

  final String label;
  final IconData icon;
  final Color color;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return NeonGlowWrapper(
      color: color,
      borderRadius: const BorderRadius.all(Radius.circular(12)),
      child: ElevatedButton(
        onPressed: onTap,
        style: ElevatedButton.styleFrom(
          minimumSize: const Size.fromHeight(72),
          padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 4),
          backgroundColor: AppTheme.surfaceHigh,
          foregroundColor: color,
          side: BorderSide(color: color.withValues(alpha: 0.7)),
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 28, color: color),
            const SizedBox(height: 4),
            Text(
              label,
              textAlign: TextAlign.center,
              style: theme.textTheme.labelLarge?.copyWith(
                color: color,
                fontWeight: FontWeight.w700,
              ),
            ),
          ],
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
