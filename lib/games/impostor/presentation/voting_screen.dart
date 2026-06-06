import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/abandon_game_dialog.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_routes.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Ancho máximo del contenido en pantallas grandes (tablet/desktop) para que
/// la lista de candidatos no se estire de forma incómoda.
const double _kMaxContentWidth = 560.0;

/// Pantalla de VOTACIÓN del Impostor (estética neón).
///
/// Pantalla compartida en la que el grupo vota, por rondas, a quién expulsar.
/// Muestra "Ronda X de Y", la lista de CANDIDATOS (jugadores aún no eliminados)
/// como tarjetas neón pulsables y el feedback del último voto. Al pulsar un
/// candidato se pide confirmación y luego se llama a
/// `impostorFlowControllerProvider.notifier.votar(jugador)`.
///
/// Cuando el flujo llega a [ImpostorPhase.gameOver] navega a la pantalla de
/// DESENLACE ([kImpostorGameOverRouteName]), que NO revela los roles.
class VotingScreen extends ConsumerWidget {
  const VotingScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // Al alcanzar gameOver, navegar al desenlace (post-frame para no navegar
    // durante el build). El propio gameOver guarda la partida y suena el SFX.
    ref.listen<ImpostorFlowState>(impostorFlowControllerProvider, (
      previous,
      next,
    ) {
      if (next.phase == ImpostorPhase.gameOver &&
          previous?.phase != ImpostorPhase.gameOver) {
        WidgetsBinding.instance.addPostFrameCallback((_) {
          if (!context.mounted) return;
          context.goNamed(kImpostorGameOverRouteName);
        });
      }
    });

    final state = ref.watch(impostorFlowControllerProvider);
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;

    return Scaffold(
      appBar: AppBar(
        leading: VolverAlMenuButton(
          onPressed: () async {
            final salir = await abandonarPartidaDialog(context);
            if (salir != true || !context.mounted) return;
            ref.read(impostorFlowControllerProvider.notifier).reiniciar();
            if (!context.mounted) return;
            context.go('/');
          },
        ),
        title: NeonText(
          l10n.votacionTitulo,
          style: theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
        ),
        automaticallyImplyLeading: false,
      ),
      body: NeonBackground(
        child: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: _kMaxContentWidth),
              child: state.session == null
                  ? _SinVotacion(
                      onVolverAlMenu: () => _volverAlMenu(context, ref),
                    )
                  : _Votacion(state: state),
            ),
          ),
        ),
      ),
    );
  }

  void _volverAlMenu(BuildContext context, WidgetRef ref) {
    ref.read(impostorFlowControllerProvider.notifier).reiniciar();
    context.goNamed('menu');
  }
}

/// Estado defensivo cuando se llega a la votación sin una sesión válida.
class _SinVotacion extends StatelessWidget {
  const _SinVotacion({required this.onVolverAlMenu});

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

/// Contenido principal de la votación: ronda + instrucción + feedback + lista.
class _Votacion extends ConsumerWidget {
  const _Votacion({required this.state});

  final ImpostorFlowState state;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    final candidatos = state.candidatos;
    final eliminados =
        state.session?.revealOrder
            .where((p) => state.eliminados.contains(p))
            .toList(growable: false) ??
        const <Player>[];

    return Column(
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
          child: Column(
            children: [
              NeonText(
                l10n.votacionRondaXDeY(state.rondaActual, state.rondasTotales),
                glowColor: AppTheme.neonMagenta,
                style: theme.textTheme.titleLarge,
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 8),
              Text(
                l10n.votacionInstruccion,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: AppTheme.textMuted,
                ),
                textAlign: TextAlign.center,
              ),
              if (state.lastVote != LastVote.ninguno) ...[
                const SizedBox(height: 12),
                _Feedback(lastVote: state.lastVote),
              ],
            ],
          ),
        ),
        Expanded(
          child: ListView(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
            children: [
              for (final jugador in candidatos)
                _CandidatoTile(
                  jugador: jugador,
                  onVotar: () => _confirmarVoto(context, ref, jugador),
                ),
              for (final jugador in eliminados)
                _EliminadoTile(jugador: jugador),
            ],
          ),
        ),
      ],
    );
  }

  Future<void> _confirmarVoto(
    BuildContext context,
    WidgetRef ref,
    Player jugador,
  ) async {
    final confirmado = await showDialog<bool>(
      context: context,
      builder: (dialogContext) {
        final dl10n = AppLocalizations.of(dialogContext)!;
        return AlertDialog(
          title: Text(dl10n.votacionTitulo),
          content: Text(dl10n.votacionConfirmarExpulsion(jugador.name)),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(dialogContext).pop(false),
              child: Text(dl10n.cancelar),
            ),
            FilledButton(
              onPressed: () => Navigator.of(dialogContext).pop(true),
              child: Text(dl10n.votacionExpulsar),
            ),
          ],
        );
      },
    );
    if (confirmado != true) return;
    ref.read(impostorFlowControllerProvider.notifier).votar(jugador);
  }
}

/// Mensaje de feedback del último voto. Nunca revela roles concretos: solo
/// indica si la partida continúa ("El impostor sigue entre vosotros").
class _Feedback extends StatelessWidget {
  const _Feedback({required this.lastVote});

  final LastVote lastVote;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    // En la votación, mientras se sigue votando, el impostor aún no ha caído del
    // todo: el mensaje es siempre "el impostor sigue entre vosotros".
    return NeonPanel(
      borderColor: AppTheme.neonViolet,
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.warning_amber_outlined, color: AppTheme.neonViolet),
          const SizedBox(width: 8),
          Flexible(
            child: NeonText(
              l10n.votacionImpostorSigue,
              glowColor: AppTheme.neonViolet,
              intensity: 0.7,
              style: theme.textTheme.titleSmall,
              textAlign: TextAlign.center,
            ),
          ),
        ],
      ),
    );
  }
}

/// Tarjeta neón de un candidato pulsable para expulsar.
class _CandidatoTile extends StatelessWidget {
  const _CandidatoTile({required this.jugador, required this.onVotar});

  final Player jugador;
  final VoidCallback onVotar;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return NeonGlowWrapper(
      color: AppTheme.neonCyan,
      intensity: 0.6,
      borderRadius: const BorderRadius.all(Radius.circular(12)),
      child: Card(
        margin: const EdgeInsets.symmetric(vertical: 4),
        color: AppTheme.surface,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
          side: const BorderSide(color: AppTheme.neonCyan, width: 1.0),
        ),
        child: ListTile(
          onTap: onVotar,
          leading: CircleAvatar(
            backgroundColor: AppTheme.surfaceHigh,
            foregroundColor: AppTheme.neonCyan,
            child: const Icon(Icons.person_outline),
          ),
          title: Text(
            jugador.name,
            style: theme.textTheme.titleMedium?.copyWith(
              color: AppTheme.textPrimary,
              fontWeight: FontWeight.w600,
            ),
          ),
          trailing: const Icon(
            Icons.how_to_vote_outlined,
            color: AppTheme.neonCyan,
          ),
        ),
      ),
    );
  }
}

/// Fila de un jugador ya expulsado (no pulsable), atenuada.
class _EliminadoTile extends StatelessWidget {
  const _EliminadoTile({required this.jugador});

  final Player jugador;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Card(
      margin: const EdgeInsets.symmetric(vertical: 4),
      color: AppTheme.surface,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
        side: BorderSide(color: AppTheme.textMuted.withValues(alpha: 0.4)),
      ),
      child: ListTile(
        enabled: false,
        leading: CircleAvatar(
          backgroundColor: AppTheme.surfaceHigh,
          foregroundColor: AppTheme.textMuted,
          child: const Icon(Icons.person_off_outlined),
        ),
        title: Text(
          jugador.name,
          style: theme.textTheme.titleMedium?.copyWith(
            color: AppTheme.textMuted,
            decoration: TextDecoration.lineThrough,
          ),
        ),
        trailing: Icon(Icons.block, color: AppTheme.textMuted),
      ),
    );
  }
}
