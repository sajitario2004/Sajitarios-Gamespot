import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/audio/audio_service.dart';
import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/impostor/data/game_history_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_routes.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Ancho máximo del contenido en pantallas grandes (tablet/desktop) para que
/// las listas y textos no se estiren de forma incómoda.
const double _kMaxContentWidth = 560.0;

/// Fin de partida: muestra a todos los jugadores con su rol y la palabra.
///
/// Lee `impostorFlowControllerProvider` para obtener la [GameSession]. Lista a
/// TODOS los jugadores en orden de revelación con su rol (sabía la palabra /
/// IMPOSTOR), destacando a los impostores. Ofrece volver al menú y jugar otra
/// partida (`reiniciar()` + navegación a `impostor-setup`).
class ResultsScreen extends ConsumerStatefulWidget {
  const ResultsScreen({super.key});

  @override
  ConsumerState<ResultsScreen> createState() => _ResultsScreenState();
}

class _ResultsScreenState extends ConsumerState<ResultsScreen> {
  /// Guard para guardar la partida en el historial una sola vez (la pantalla
  /// puede reconstruirse, pero el registro debe insertarse solo al llegar).
  bool _guardada = false;

  @override
  void initState() {
    super.initState();
    // SFX de fin de partida al llegar a resultados (una sola vez al entrar).
    // Respeta el toggle de silencio y va envuelto en el propio servicio para
    // no romper la UI. Tras el primer frame para tener el `ref` listo.
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      final state = ref.read(impostorFlowControllerProvider);
      // Solo suena/guarda si hay una partida que mostrar (estado válido).
      if (state.session != null) {
        ref.read(audioServiceProvider).playGameOver();
        _guardarPartida(state);
      }
    });
  }

  /// Persiste la partida en el historial UNA sola vez. Va envuelto para que un
  /// fallo de persistencia nunca rompa la pantalla de resultados.
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
    final session = state.session;

    final theme = Theme.of(context);
    return Scaffold(
      appBar: AppBar(
        leading: VolverAlMenuButton(
          onPressed: () {
            ref.read(impostorFlowControllerProvider.notifier).reiniciar();
            context.goNamed('menu');
          },
        ),
        title: NeonText(
          AppLocalizations.of(context)!.resultadoDeLaPartida,
          style: theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
        ),
        automaticallyImplyLeading: false,
      ),
      body: NeonBackground(
        child: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: _kMaxContentWidth),
              child: session == null
                  ? _SinPartida(onVolverAlMenu: _volverAlMenu)
                  : _Resultados(
                      session: session,
                      onVolverAlMenu: _volverAlMenu,
                      onJugarOtra: _jugarOtra,
                    ),
            ),
          ),
        ),
      ),
    );
  }

  /// Vuelve al menú dejando el flujo limpio: se llama a `reiniciar()` para no
  /// dejar la sesión terminada viva en el controlador (si no, al volver a
  /// entrar al Impostor seguiría con la partida ya jugada).
  void _volverAlMenu() {
    ref.read(impostorFlowControllerProvider.notifier).reiniciar();
    context.goNamed('menu');
  }

  void _jugarOtra() {
    ref.read(impostorFlowControllerProvider.notifier).reiniciar();
    context.goNamed(kImpostorSetupRouteName);
  }
}

/// Estado defensivo cuando se llega a resultados sin una sesión válida.
class _SinPartida extends StatelessWidget {
  const _SinPartida({required this.onVolverAlMenu});

  final VoidCallback onVolverAlMenu;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final l10n = AppLocalizations.of(context)!;
    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Icon(Icons.inbox_outlined, size: 64),
          const SizedBox(height: 16),
          Text(
            l10n.noHayPartidaQueMostrar,
            style: textTheme.titleLarge,
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 24),
          FilledButton.icon(
            onPressed: onVolverAlMenu,
            icon: const Icon(Icons.home_outlined),
            label: Text(l10n.volverAlMenu),
          ),
        ],
      ),
    );
  }
}

/// Contenido principal de resultados: palabra + lista de jugadores + acciones.
class _Resultados extends StatelessWidget {
  const _Resultados({
    required this.session,
    required this.onVolverAlMenu,
    required this.onJugarOtra,
  });

  final GameSession session;
  final VoidCallback onVolverAlMenu;
  final VoidCallback onJugarOtra;

  @override
  Widget build(BuildContext context) {
    final jugadores = session.revealOrder;
    return Column(
      children: [
        Expanded(
          child: ListView(
            padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
            children: [
              _PalabraCard(palabra: session.word.text, pista: session.hint),
              const SizedBox(height: 16),
              _ResumenImpostores(count: session.impostorCount),
              const SizedBox(height: 16),
              NeonText(
                AppLocalizations.of(context)!.setupJugadores,
                glowColor: AppTheme.neonCyan,
                style: Theme.of(context).textTheme.titleMedium,
              ),
              const SizedBox(height: 8),
              for (final jugador in jugadores)
                _JugadorTile(
                  jugador: jugador,
                  esImpostor: session.isImpostor(jugador),
                ),
            ],
          ),
        ),
        _Acciones(onVolverAlMenu: onVolverAlMenu, onJugarOtra: onJugarOtra),
      ],
    );
  }
}

/// Tarjeta destacada con la palabra de la ronda y su pista.
class _PalabraCard extends StatelessWidget {
  const _PalabraCard({required this.palabra, required this.pista});

  final String palabra;
  final String pista;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    return NeonPanel(
      borderColor: AppTheme.neonCyan,
      borderWidth: 2.0,
      padding: const EdgeInsets.all(20),
      child: Column(
        children: [
          Text(
            l10n.laPalabraEra,
            style: theme.textTheme.labelLarge?.copyWith(
              color: AppTheme.textMuted,
            ),
          ),
          const SizedBox(height: 8),
          FittedBox(
            fit: BoxFit.scaleDown,
            child: NeonText(
              palabra,
              glowColor: AppTheme.neonCyan,
              style: theme.textTheme.displaySmall?.copyWith(
                color: AppTheme.textPrimary,
                fontWeight: FontWeight.w900,
              ),
              textAlign: TextAlign.center,
            ),
          ),
          const SizedBox(height: 12),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                Icons.lightbulb_outline,
                size: 18,
                color: AppTheme.neonViolet,
              ),
              const SizedBox(width: 6),
              Flexible(
                child: Text(
                  l10n.pistaConValor(pista),
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: AppTheme.textPrimary,
                  ),
                  textAlign: TextAlign.center,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

/// Resumen del número de impostores de la partida.
class _ResumenImpostores extends StatelessWidget {
  const _ResumenImpostores({required this.count});

  final int count;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final texto = AppLocalizations.of(context)!.resumenImpostores(count);
    return Row(
      children: [
        Icon(
          Icons.theater_comedy_outlined,
          color: AppTheme.neonMagenta,
          shadows: neonTextShadows(color: AppTheme.neonMagenta, intensity: 0.7),
        ),
        const SizedBox(width: 8),
        Expanded(
          child: NeonText(
            texto,
            glowColor: AppTheme.neonMagenta,
            intensity: 0.7,
            style: theme.textTheme.titleSmall,
          ),
        ),
      ],
    );
  }
}

/// Fila de un jugador con su rol, destacando a los impostores.
class _JugadorTile extends StatelessWidget {
  const _JugadorTile({required this.jugador, required this.esImpostor});

  final Player jugador;
  final bool esImpostor;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;

    final Color accent = esImpostor ? AppTheme.neonMagenta : AppTheme.neonCyan;
    final IconData icono;
    final String etiqueta;

    if (esImpostor) {
      icono = Icons.theater_comedy;
      etiqueta = l10n.impostorMayus;
    } else {
      icono = Icons.check_circle_outline;
      etiqueta = l10n.sabiaLaPalabra;
    }

    return NeonGlowWrapper(
      color: accent,
      intensity: esImpostor ? 0.9 : 0.5,
      borderRadius: const BorderRadius.all(Radius.circular(12)),
      child: Card(
        margin: const EdgeInsets.symmetric(vertical: 4),
        color: AppTheme.surface,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
          side: BorderSide(color: accent, width: esImpostor ? 2.0 : 1.0),
        ),
        // Toda la fila se anuncia como un solo nodo "{nombre}: {rol}" en vez de
        // leerse el nombre, el icono y la etiqueta por separado.
        child: Semantics(
          label: esImpostor
              ? l10n.jugadorEraImpostor(jugador.name)
              : l10n.jugadorSabiaLaPalabra(jugador.name),
          child: ExcludeSemantics(
            child: ListTile(
              leading: CircleAvatar(
                backgroundColor: AppTheme.surfaceHigh,
                foregroundColor: accent,
                child: Icon(icono),
              ),
              title: Text(
                jugador.name,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: theme.textTheme.titleMedium?.copyWith(
                  color: AppTheme.textPrimary,
                  fontWeight: esImpostor ? FontWeight.w800 : FontWeight.w600,
                ),
              ),
              trailing: Text(
                etiqueta,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: theme.textTheme.labelLarge?.copyWith(
                  color: accent,
                  fontWeight: esImpostor ? FontWeight.w900 : FontWeight.w600,
                  shadows: neonTextShadows(
                    color: accent,
                    intensity: esImpostor ? 0.8 : 0.4,
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

/// Botones de fin de partida: volver al menú o jugar otra.
class _Acciones extends StatelessWidget {
  const _Acciones({required this.onVolverAlMenu, required this.onJugarOtra});

  final VoidCallback onVolverAlMenu;
  final VoidCallback onJugarOtra;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
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
