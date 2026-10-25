import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_flow_controller.dart';
import 'package:sajitarios_gamespot/games/bomba/presentation/bomba_routes.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/rules_screen.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla de configuración previa a la partida de La Bomba.
///
/// Permite elegir el modo (sílaba / categoría) e introducir los nombres de los
/// jugadores (2..12). Valida la configuración antes de iniciar la partida.
class BombaSetupScreen extends ConsumerStatefulWidget {
  const BombaSetupScreen({super.key});

  @override
  ConsumerState<BombaSetupScreen> createState() => _BombaSetupScreenState();
}

class _BombaSetupScreenState extends ConsumerState<BombaSetupScreen> {
  BombaMode _mode = BombaMode.silaba;
  final List<TextEditingController> _playerControllers = [
    TextEditingController(),
    TextEditingController(),
  ];
  bool _iniciando = false;

  @override
  void dispose() {
    for (final c in _playerControllers) {
      c.dispose();
    }
    super.dispose();
  }

  List<String> get _playerNames =>
      _playerControllers.map((c) => c.text.trim()).toList();

  void _addPlayer() {
    if (_playerControllers.length >= kBombaMaxPlayers) return;
    setState(() => _playerControllers.add(TextEditingController()));
  }

  void _removePlayer(int index) {
    if (_playerControllers.length <= kBombaMinPlayers) return;
    setState(() {
      _playerControllers[index].dispose();
      _playerControllers.removeAt(index);
    });
  }

  Future<void> _iniciarPartida() async {
    final l10n = AppLocalizations.of(context)!;
    final result = BombaConfig.create(
      mode: _mode,
      playerNames: _playerNames,
      minSegundos: kBombaAbsMinSegundos,
      maxSegundos: kBombaAbsMaxSegundos,
    );

    if (!result.isSuccess) {
      _mostrarError(_mensajeError(l10n, result.error!));
      return;
    }

    setState(() => _iniciando = true);

    await ref
        .read(bombaFlowControllerProvider.notifier)
        .iniciar(result.config!);

    if (!mounted) return;

    final flowState = ref.read(bombaFlowControllerProvider);
    if (flowState.fase == BombaFase.error) {
      setState(() => _iniciando = false);
      if (flowState.errorKind == BombaErrorKind.sinPrompts) {
        await _mostrarErrorSinPrompts();
      } else {
        _mostrarError(l10n.errorNoSePudoIniciar);
      }
      return;
    }

    if (mounted) {
      context.goNamed(kBombaPlayRouteName);
    }
  }

  String _mensajeError(AppLocalizations l10n, BombaConfigError error) {
    switch (error) {
      case BombaConfigError.pocosJugadores:
        return l10n.errorPocosJugadores(kBombaMinPlayers);
      case BombaConfigError.demasiadosJugadores:
        return l10n.errorDemasiadosJugadores(kBombaMaxPlayers);
      case BombaConfigError.nombreVacio:
        return l10n.errorNombreVacio;
      case BombaConfigError.nombresDuplicados:
        return l10n.errorNombresDuplicados;
      case BombaConfigError.minSegundosFueraDeLimite:
      case BombaConfigError.maxSegundosFueraDeLimite:
      case BombaConfigError.rangoSegundosInvalido:
        return l10n.errorNoSePudoIniciar;
    }
  }

  void _mostrarError(String mensaje) {
    final messenger = ScaffoldMessenger.of(context);
    messenger.hideCurrentSnackBar();
    messenger.showSnackBar(SnackBar(content: Text(mensaje)));
  }

  Future<void> _mostrarErrorSinPrompts() async {
    if (!mounted) return;
    final l10n = AppLocalizations.of(context)!;
    await showDialog<void>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: Text(l10n.bombaSinPromptsTitulo),
        content: Text(l10n.bombaSinPromptsMensaje),
        actions: [
          FilledButton(
            onPressed: () => Navigator.of(dialogContext).pop(),
            child: Text(l10n.aceptar),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;
    final l10n = AppLocalizations.of(context)!;
    final canAddPlayer = _playerControllers.length < kBombaMaxPlayers;

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
            l10n.bombaTitulo,
            style:
                theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
            glowColor: AppTheme.neonError,
          ),
          actions: [
            IconButton(
              tooltip: l10n.comoSeJuega,
              icon: const Icon(Icons.help_outline),
              onPressed: () => Navigator.of(context).push(
                MaterialPageRoute<void>(
                  builder: (_) => RulesScreen(
                    gameTitle: l10n.bombaTitulo,
                    steps: [
                      l10n.reglasBomba1,
                      l10n.reglasBomba2,
                      l10n.reglasBomba3,
                      l10n.reglasBomba4,
                      l10n.reglasBomba5,
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
                    // ── Modo ─────────────────────────────────────────────────
                    NeonText(
                      l10n.bombaSetupModo,
                      style: theme.textTheme.titleLarge,
                      glowColor: AppTheme.neonError,
                    ),
                    const SizedBox(height: 12),
                    Wrap(
                      spacing: 8,
                      children: [
                        ChoiceChip(
                          label: Text(l10n.bombaModoSilaba),
                          selected: _mode == BombaMode.silaba,
                          onSelected: (_) =>
                              setState(() => _mode = BombaMode.silaba),
                          selectedColor: AppTheme.neonError.withValues(
                            alpha: 0.22,
                          ),
                          checkmarkColor: AppTheme.neonError,
                          side: BorderSide(
                            color: _mode == BombaMode.silaba
                                ? AppTheme.neonError
                                : AppTheme.neonCyan.withValues(alpha: 0.4),
                          ),
                        ),
                        ChoiceChip(
                          label: Text(l10n.bombaModoCategoria),
                          selected: _mode == BombaMode.categoria,
                          onSelected: (_) =>
                              setState(() => _mode = BombaMode.categoria),
                          selectedColor: AppTheme.neonError.withValues(
                            alpha: 0.22,
                          ),
                          checkmarkColor: AppTheme.neonError,
                          side: BorderSide(
                            color: _mode == BombaMode.categoria
                                ? AppTheme.neonError
                                : AppTheme.neonCyan.withValues(alpha: 0.4),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 24),

                    // ── Jugadores ────────────────────────────────────────────
                    NeonText(
                      l10n.setupJugadores,
                      style: theme.textTheme.titleLarge,
                      glowColor: AppTheme.neonCyan,
                    ),
                    const SizedBox(height: 4),
                    Text(
                      l10n.bombaSetupRangoJugadores(
                        kBombaMinPlayers,
                        kBombaMaxPlayers,
                      ),
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: colors.onSurfaceVariant,
                      ),
                    ),
                    const SizedBox(height: 12),

                    for (var i = 0; i < _playerControllers.length; i++) ...[
                      Row(
                        children: [
                          Expanded(
                            child: TextField(
                              controller: _playerControllers[i],
                              textCapitalization: TextCapitalization.words,
                              textInputAction: i < _playerControllers.length - 1
                                  ? TextInputAction.next
                                  : TextInputAction.done,
                              decoration: InputDecoration(
                                labelText: l10n.jugadorNumero(i + 1),
                                hintText: l10n.nombreDelJugadorNumero(i + 1),
                              ),
                            ),
                          ),
                          const SizedBox(width: 8),
                          IconButton(
                            tooltip: l10n.quitarJugador,
                            icon: const Icon(Icons.remove_circle_outline),
                            color: AppTheme.neonError,
                            onPressed:
                                _playerControllers.length > kBombaMinPlayers
                                ? () => _removePlayer(i)
                                : null,
                          ),
                        ],
                      ),
                      const SizedBox(height: 8),
                    ],

                    const SizedBox(height: 4),
                    OutlinedButton.icon(
                      onPressed: canAddPlayer ? _addPlayer : null,
                      icon: const Icon(Icons.person_add_alt_1),
                      label: Text(
                        canAddPlayer
                            ? l10n.anadirJugador
                            : l10n.maximoJugadoresAlcanzado,
                      ),
                    ),
                    const SizedBox(height: 32),

                    // ── Botón Empezar ────────────────────────────────────────
                    NeonGlowWrapper(
                      color: AppTheme.neonError,
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
                            : const Icon(Icons.local_fire_department),
                        label: Text(
                          _iniciando
                              ? l10n.bombaIniciando
                              : l10n.bombaEmpezarPartida,
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
