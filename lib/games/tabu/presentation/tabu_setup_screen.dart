import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_flow_controller.dart';
import 'package:sajitarios_gamespot/games/tabu/presentation/tabu_routes.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/rules_screen.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla de configuración previa a la partida de Tabú.
///
/// Permite introducir los nombres de los dos equipos y elegir la duración del
/// turno (30 / 60 / 90 segundos). Valida los campos antes de iniciar.
class TabuSetupScreen extends ConsumerStatefulWidget {
  const TabuSetupScreen({super.key});

  @override
  ConsumerState<TabuSetupScreen> createState() => _TabuSetupScreenState();
}

class _TabuSetupScreenState extends ConsumerState<TabuSetupScreen> {
  final _equipoAController = TextEditingController();
  final _equipoBController = TextEditingController();
  int _turnoSegundos = 60;
  bool _iniciando = false;

  static const List<int> _opcionesTurno = [30, 60, 90];

  @override
  void dispose() {
    _equipoAController.dispose();
    _equipoBController.dispose();
    super.dispose();
  }

  String _normalize(String value) =>
      value.trim().replaceAll(RegExp(r'\s+'), ' ');

  Future<void> _iniciarPartida() async {
    final l10n = AppLocalizations.of(context)!;
    final a = _normalize(_equipoAController.text);
    final b = _normalize(_equipoBController.text);

    final result = TabuConfig.create(
      equipoA: a,
      equipoB: b,
      turnoSegundos: _turnoSegundos,
    );

    if (!result.isSuccess) {
      _mostrarError(_mensajeError(l10n, result.error!));
      return;
    }

    setState(() => _iniciando = true);

    await ref.read(tabuFlowControllerProvider.notifier).iniciar(result.config!);

    if (!mounted) return;

    final flowState = ref.read(tabuFlowControllerProvider);
    if (flowState.fase == TabuFase.error) {
      setState(() => _iniciando = false);
      if (flowState.errorKind == TabuErrorKind.sinPalabras) {
        await _mostrarErrorSinPalabras();
      } else {
        _mostrarError(l10n.errorNoSePudoIniciar);
      }
      return;
    }

    if (mounted) {
      context.goNamed(kTabuTurnRouteName);
    }
  }

  String _mensajeError(AppLocalizations l10n, TabuConfigError error) {
    switch (error) {
      case TabuConfigError.equipoAVacio:
        return l10n.tabuErrorEquipoAVacio;
      case TabuConfigError.equipoBVacio:
        return l10n.tabuErrorEquipoBVacio;
      case TabuConfigError.equiposDuplicados:
        return l10n.tabuErrorEquiposDuplicados;
      case TabuConfigError.turnoSegundosInvalido:
        return l10n.tabuErrorTurnoInvalido;
      case TabuConfigError.objetivoVictoriasInvalido:
        return l10n.errorNoSePudoIniciar;
    }
  }

  void _mostrarError(String mensaje) {
    final messenger = ScaffoldMessenger.of(context);
    messenger.hideCurrentSnackBar();
    messenger.showSnackBar(SnackBar(content: Text(mensaje)));
  }

  Future<void> _mostrarErrorSinPalabras() async {
    if (!mounted) return;
    final l10n = AppLocalizations.of(context)!;
    await showDialog<void>(
      context: context,
      builder: (dialogContext) {
        return AlertDialog(
          title: Text(l10n.tabuSinPalabrasTitulo),
          content: Text(l10n.tabuSinPalabrasMensaje),
          actions: [
            FilledButton(
              onPressed: () => Navigator.of(dialogContext).pop(),
              child: Text(l10n.aceptar),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;
    final l10n = AppLocalizations.of(context)!;

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) async {
        if (didPop) return;
        final confirmed = await _confirmarSalida(context, l10n);
        if (confirmed && context.mounted) {
          context.go('/');
        }
      },
      child: Scaffold(
        appBar: AppBar(
          leading: VolverAlMenuButton(onPressed: () => context.go('/')),
          title: NeonText(
            l10n.tabuTitulo,
            style:
                theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
            glowColor: AppTheme.neonCyan,
          ),
          actions: [
            IconButton(
              tooltip: l10n.comoSeJuega,
              icon: const Icon(Icons.help_outline),
              onPressed: () => Navigator.of(context).push(
                MaterialPageRoute<void>(
                  builder: (_) => RulesScreen(
                    gameTitle: l10n.tabuTitulo,
                    steps: [
                      l10n.reglasTabu1,
                      l10n.reglasTabu2,
                      l10n.reglasTabu3,
                      l10n.reglasTabu4,
                      l10n.reglasTabu5,
                    ],
                  ),
                ),
              ),
            ),
          ],
        ),
        body: NeonBackground(
          child: SafeArea(
            child: Center(
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 640),
                child: ListView(
                  padding: const EdgeInsets.fromLTRB(16, 16, 16, 24),
                  children: [
                    // ── Equipos ──────────────────────────────────────────────
                    NeonText(
                      l10n.tabuSetupEquipos,
                      style: theme.textTheme.titleLarge,
                      glowColor: AppTheme.neonCyan,
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: _equipoAController,
                      textCapitalization: TextCapitalization.words,
                      textInputAction: TextInputAction.next,
                      decoration: InputDecoration(
                        labelText: l10n.tabuEquipoA,
                        hintText: l10n.tabuEquipoAHint,
                      ),
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: _equipoBController,
                      textCapitalization: TextCapitalization.words,
                      textInputAction: TextInputAction.done,
                      decoration: InputDecoration(
                        labelText: l10n.tabuEquipoB,
                        hintText: l10n.tabuEquipoBHint,
                      ),
                    ),
                    const SizedBox(height: 24),

                    // ── Duración del turno ───────────────────────────────────
                    NeonText(
                      l10n.tabuSetupTurno,
                      style: theme.textTheme.titleLarge,
                      glowColor: AppTheme.neonViolet,
                    ),
                    const SizedBox(height: 4),
                    Text(
                      l10n.tabuSetupTurnoAyuda,
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: colors.onSurfaceVariant,
                      ),
                    ),
                    const SizedBox(height: 12),
                    Wrap(
                      spacing: 8,
                      children: [
                        for (final secs in _opcionesTurno)
                          ChoiceChip(
                            label: Text(l10n.tabuSegundos(secs)),
                            selected: _turnoSegundos == secs,
                            onSelected: (_) =>
                                setState(() => _turnoSegundos = secs),
                            selectedColor: AppTheme.neonViolet.withValues(
                              alpha: 0.25,
                            ),
                            checkmarkColor: AppTheme.neonVioletText,
                            side: BorderSide(
                              color: _turnoSegundos == secs
                                  ? AppTheme.neonViolet
                                  : AppTheme.neonCyan.withValues(alpha: 0.4),
                            ),
                          ),
                      ],
                    ),
                    const SizedBox(height: 32),

                    // ── Botón Empezar ────────────────────────────────────────
                    NeonGlowWrapper(
                      color: AppTheme.neonCyan,
                      borderRadius: const BorderRadius.all(Radius.circular(28)),
                      child: FilledButton.icon(
                        onPressed: _iniciando ? null : _iniciarPartida,
                        icon: _iniciando
                            ? const SizedBox(
                                width: 20,
                                height: 20,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2.5,
                                ),
                              )
                            : const Icon(Icons.play_arrow_rounded),
                        label: Text(
                          _iniciando
                              ? l10n.tabuIniciando
                              : l10n.tabuEmpezarPartida,
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
