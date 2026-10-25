import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/abandon_game_dialog.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_flow_controller.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla de juego activo de Yo Nunca.
///
/// Muestra la frase "Yo nunca…" actual con estilo neón grande y un botón
/// "Siguiente" para sacar la próxima. Los jugadores leen y pasan el móvil
/// entre ellos (pass-the-phone implícito).
class YoNuncaPlayScreen extends ConsumerWidget {
  const YoNuncaPlayScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final flowState = ref.watch(yoNuncaFlowControllerProvider);
    final l10n = AppLocalizations.of(context)!;
    final theme = Theme.of(context);

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) {
        if (didPop) return;
        ref.read(yoNuncaFlowControllerProvider.notifier).reiniciar();
        context.go('/');
      },
      child: Scaffold(
        appBar: AppBar(
          title: NeonText(
            l10n.yoNuncaTitulo,
            style:
                theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
            glowColor: AppTheme.neonCyan,
          ),
          leading: VolverAlMenuButton(
            onPressed: () async {
              final salir = await abandonarPartidaDialog(context);
              if (salir != true || !context.mounted) return;
              ref.read(yoNuncaFlowControllerProvider.notifier).reiniciar();
              if (!context.mounted) return;
              context.go('/');
            },
          ),
        ),
        body: NeonBackground(
          child: SafeArea(
            child:
                flowState.fase != YoNuncaFase.jugando ||
                    flowState.fraseActual == null
                ? _NoSesion(l10n: l10n)
                : _PlayContent(flowState: flowState, l10n: l10n, ref: ref),
          ),
        ),
      ),
    );
  }
}

/// Contenido principal cuando hay sesión activa.
class _PlayContent extends StatelessWidget {
  const _PlayContent({
    required this.flowState,
    required this.l10n,
    required this.ref,
  });

  final YoNuncaFlowState flowState;
  final AppLocalizations l10n;
  final WidgetRef ref;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final frase = flowState.fraseActual!;

    return Center(
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 640),
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Expanded(
                child: Center(
                  child: Semantics(
                    label: l10n.yoNuncaFraseSemantica(frase.frase),
                    child: NeonPanel(
                      borderColor: AppTheme.neonCyan,
                      padding: EdgeInsets.zero,
                      child: SingleChildScrollView(
                        padding: const EdgeInsets.all(24),
                        child: NeonText(
                          frase.frase,
                          style: theme.textTheme.headlineMedium,
                          glowColor: AppTheme.neonCyan,
                          textAlign: TextAlign.center,
                        ),
                      ),
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 24),
              NeonGlowWrapper(
                color: AppTheme.neonCyan,
                borderRadius: const BorderRadius.all(Radius.circular(28)),
                child: FilledButton.icon(
                  onPressed: () => ref
                      .read(yoNuncaFlowControllerProvider.notifier)
                      .siguiente(),
                  icon: const Icon(Icons.arrow_forward_rounded),
                  label: Text(l10n.yoNuncaSiguiente),
                  style: FilledButton.styleFrom(
                    minimumSize: const Size.fromHeight(56),
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

/// Fallback cuando no hay sesión activa en la pantalla de juego.
class _NoSesion extends StatelessWidget {
  const _NoSesion({required this.l10n});

  final AppLocalizations l10n;

  @override
  Widget build(BuildContext context) {
    return Center(child: Text(l10n.yoNuncaNoHaySesion));
  }
}
