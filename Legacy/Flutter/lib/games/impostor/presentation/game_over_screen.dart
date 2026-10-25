import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/audio/audio_service.dart';
import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/impostor/data/game_history_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_routes.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Ancho máximo del contenido en pantallas grandes (tablet/desktop).
const double _kMaxContentWidth = 560.0;

/// Pantalla de DESENLACE del Impostor (estética neón).
///
/// Muestra el resultado de la votación SIN revelar los roles: o bien los
/// jugadores ganaron (pillaron a todos los impostores), o bien el impostor
/// sigue entre ellos (se agotaron las rondas). Lee el [VotingOutcome] de
/// `impostorFlowControllerProvider`.
///
/// Guarda la partida en el historial UNA sola vez (post-frame, con guard,
/// fire-and-forget) y reproduce el SFX de fin de partida una vez, sin revelar
/// identidades en la UI.
class GameOverScreen extends ConsumerStatefulWidget {
  const GameOverScreen({super.key});

  @override
  ConsumerState<GameOverScreen> createState() => _GameOverScreenState();
}

class _GameOverScreenState extends ConsumerState<GameOverScreen> {
  /// Guard para guardar la partida en el historial una sola vez (la pantalla
  /// puede reconstruirse, pero el registro debe insertarse solo al llegar).
  bool _guardada = false;

  @override
  void initState() {
    super.initState();
    // SFX de fin de partida y guardado en historial al llegar al desenlace
    // (una sola vez al entrar). Tras el primer frame para tener el `ref` listo.
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      final state = ref.read(impostorFlowControllerProvider);
      // Solo suena/guarda si hay una partida válida y terminada.
      if (state.session != null) {
        ref.read(audioServiceProvider).playGameOver();
        _guardarPartida(state);
      }
    });
  }

  /// Persiste la partida en el historial UNA sola vez. Va envuelto para que un
  /// fallo de persistencia nunca rompa la pantalla. El historial se guarda con
  /// el desenlace; la UI no revela los roles.
  Future<void> _guardarPartida(ImpostorFlowState state) async {
    if (_guardada) return;
    final session = state.session;
    if (session == null) return;
    _guardada = true;
    final hintEnabled = state.config?.hintEnabled ?? false;
    try {
      await ref
          .read(gameHistoryRepositoryProvider)
          .insertFromSession(session, hintEnabled: hintEnabled);
    } catch (_) {
      // Si falla la persistencia se ignora (no debe afectar a la UI) y se
      // permite reintentar en una futura reconstrucción de la pantalla.
      _guardada = false;
    }
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(impostorFlowControllerProvider);
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;

    final bool ganan = state.outcome == VotingOutcome.jugadoresGanan;
    final String mensaje = ganan
        ? l10n.votacionJugadoresGanan
        : l10n.votacionImpostorSigue;
    final Color accent = ganan ? AppTheme.neonCyan : AppTheme.neonMagenta;
    final IconData icono = ganan
        ? Icons.emoji_events_outlined
        : Icons.theater_comedy_outlined;

    return Scaffold(
      appBar: AppBar(
        leading: VolverAlMenuButton(
          onPressed: () {
            ref.read(impostorFlowControllerProvider.notifier).reiniciar();
            context.goNamed('menu');
          },
        ),
        title: NeonText(
          l10n.finDePartidaTitulo,
          style: theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
        ),
        automaticallyImplyLeading: false,
      ),
      body: NeonBackground(
        child: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: _kMaxContentWidth),
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Expanded(
                      child: Center(
                        child: SingleChildScrollView(
                          child: NeonPanel(
                            borderColor: accent,
                            borderWidth: 2.0,
                            padding: const EdgeInsets.all(28),
                            child: Column(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Icon(
                                  icono,
                                  size: 72,
                                  color: accent,
                                  shadows: neonTextShadows(color: accent),
                                ),
                                const SizedBox(height: 20),
                                NeonText(
                                  mensaje,
                                  glowColor: accent,
                                  style: theme.textTheme.headlineSmall
                                      ?.copyWith(
                                        color: AppTheme.textPrimary,
                                        fontWeight: FontWeight.w900,
                                      ),
                                  textAlign: TextAlign.center,
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                    ),
                    _Acciones(
                      onVolverAlMenu: _volverAlMenu,
                      onJugarOtra: _jugarOtra,
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

  /// Vuelve al menú dejando el flujo limpio (`reiniciar()`), para no dejar la
  /// partida terminada viva en el controlador.
  void _volverAlMenu() {
    ref.read(impostorFlowControllerProvider.notifier).reiniciar();
    context.goNamed('menu');
  }

  void _jugarOtra() {
    ref.read(impostorFlowControllerProvider.notifier).reiniciar();
    context.goNamed(kImpostorSetupRouteName);
  }
}

/// Botones de fin de partida: volver al menú o jugar otra.
class _Acciones extends StatelessWidget {
  const _Acciones({required this.onVolverAlMenu, required this.onJugarOtra});

  final VoidCallback onVolverAlMenu;
  final VoidCallback onJugarOtra;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Padding(
      padding: const EdgeInsets.fromLTRB(0, 16, 0, 0),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          SizedBox(
            width: double.infinity,
            child: NeonGlowWrapper(
              color: AppTheme.neonCyan,
              borderRadius: const BorderRadius.all(Radius.circular(24)),
              child: FilledButton.icon(
                onPressed: onJugarOtra,
                icon: const Icon(Icons.replay),
                label: Text(l10n.jugarOtra),
              ),
            ),
          ),
          const SizedBox(height: 8),
          SizedBox(
            width: double.infinity,
            child: OutlinedButton.icon(
              onPressed: onVolverAlMenu,
              icon: const Icon(Icons.home_outlined),
              label: Text(l10n.volverAlMenu),
            ),
          ),
        ],
      ),
    );
  }
}
