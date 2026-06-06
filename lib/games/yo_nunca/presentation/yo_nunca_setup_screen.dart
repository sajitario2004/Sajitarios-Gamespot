import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_flow_controller.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_routes.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/rules_screen.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla de configuración previa a la sesión de Yo Nunca.
///
/// Permite seleccionar una o más intensidades ([Intensidad.suave] / [Intensidad.picante]).
/// Cuando se activa la opción picante se muestra una advertencia de contenido adulto.
class YoNuncaSetupScreen extends ConsumerStatefulWidget {
  const YoNuncaSetupScreen({super.key});

  @override
  ConsumerState<YoNuncaSetupScreen> createState() => _YoNuncaSetupScreenState();
}

class _YoNuncaSetupScreenState extends ConsumerState<YoNuncaSetupScreen> {
  final Set<Intensidad> _intensidades = {Intensidad.suave};
  bool _iniciando = false;

  Future<void> _iniciar() async {
    final l10n = AppLocalizations.of(context)!;

    final result = YoNuncaConfig.create(intensidades: Set.of(_intensidades));

    if (!result.isSuccess) {
      _mostrarError(l10n.yoNuncaErrorSinIntensidades);
      return;
    }

    setState(() => _iniciando = true);

    await ref
        .read(yoNuncaFlowControllerProvider.notifier)
        .iniciar(result.config!);

    if (!mounted) return;

    final flowState = ref.read(yoNuncaFlowControllerProvider);
    if (flowState.fase == YoNuncaFase.error) {
      setState(() => _iniciando = false);
      if (flowState.errorKind == YoNuncaErrorKind.sinFrases) {
        await _mostrarErrorSinFrases();
      } else {
        _mostrarError(l10n.errorNoSePudoIniciar);
      }
      return;
    }

    if (mounted) {
      context.goNamed(kYoNuncaPlayRouteName);
    }
  }

  void _mostrarError(String mensaje) {
    final messenger = ScaffoldMessenger.of(context);
    messenger.hideCurrentSnackBar();
    messenger.showSnackBar(SnackBar(content: Text(mensaje)));
  }

  Future<void> _mostrarErrorSinFrases() async {
    if (!mounted) return;
    final l10n = AppLocalizations.of(context)!;
    await showDialog<void>(
      context: context,
      builder: (dialogContext) {
        return AlertDialog(
          title: Text(l10n.yoNuncaSinFrasesTitulo),
          content: Text(l10n.yoNuncaSinFrasesMensaje),
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

  void _toggleIntensidad(Intensidad intensidad, bool selected) {
    setState(() {
      if (selected) {
        _intensidades.add(intensidad);
      } else {
        _intensidades.remove(intensidad);
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;
    final l10n = AppLocalizations.of(context)!;

    final picanteSel = _intensidades.contains(Intensidad.picante);

    return Scaffold(
      appBar: AppBar(
        title: NeonText(
          l10n.yoNuncaTitulo,
          style: theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
          glowColor: AppTheme.neonCyan,
        ),
        leading: VolverAlMenuButton(onPressed: () => context.go('/')),
        actions: [
          IconButton(
            tooltip: l10n.comoSeJuega,
            icon: const Icon(Icons.help_outline),
            onPressed: () => Navigator.of(context).push(
              MaterialPageRoute<void>(
                builder: (_) => RulesScreen(
                  gameTitle: l10n.yoNuncaTitulo,
                  steps: [
                    l10n.reglasYoNunca1,
                    l10n.reglasYoNunca2,
                    l10n.reglasYoNunca3,
                    l10n.reglasYoNunca4,
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
                  // ── Intensidades ─────────────────────────────────────────────
                  NeonText(
                    l10n.yoNuncaSetupIntensidades,
                    style: theme.textTheme.titleLarge,
                    glowColor: AppTheme.neonCyan,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    l10n.yoNuncaSetupIntensidadesAyuda,
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: colors.onSurfaceVariant,
                    ),
                  ),
                  const SizedBox(height: 12),

                  // Chip suave
                  _IntensidadChip(
                    label: l10n.yoNuncaIntensidadSuave,
                    selected: _intensidades.contains(Intensidad.suave),
                    color: AppTheme.neonCyan,
                    onSelected: (v) => _toggleIntensidad(Intensidad.suave, v),
                  ),
                  const SizedBox(height: 8),

                  // Chip picante
                  _IntensidadChip(
                    label: l10n.yoNuncaIntensidadPicante,
                    selected: picanteSel,
                    color: AppTheme.neonMagenta,
                    onSelected: (v) => _toggleIntensidad(Intensidad.picante, v),
                  ),

                  // Advertencia picante
                  if (picanteSel) ...[
                    const SizedBox(height: 12),
                    NeonPanel(
                      borderColor: AppTheme.neonMagenta,
                      child: Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Icon(
                            Icons.warning_amber_rounded,
                            color: AppTheme.neonMagenta,
                            size: 20,
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              l10n.yoNuncaAdvertenciaPicante,
                              style: theme.textTheme.bodySmall?.copyWith(
                                color: colors.onSurface,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],

                  const SizedBox(height: 32),

                  // ── Botón Empezar ─────────────────────────────────────────────
                  NeonGlowWrapper(
                    color: AppTheme.neonCyan,
                    borderRadius: const BorderRadius.all(Radius.circular(28)),
                    child: FilledButton.icon(
                      onPressed: _iniciando ? null : _iniciar,
                      icon: _iniciando
                          ? const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(
                                strokeWidth: 2.5,
                              ),
                            )
                          : const Icon(Icons.play_arrow_rounded),
                      label: Text(l10n.yoNuncaEmpezar),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

/// Chip de selección de intensidad con estilos neón.
class _IntensidadChip extends StatelessWidget {
  const _IntensidadChip({
    required this.label,
    required this.selected,
    required this.color,
    required this.onSelected,
  });

  final String label;
  final bool selected;
  final Color color;
  final ValueChanged<bool> onSelected;

  @override
  Widget build(BuildContext context) {
    return FilterChip(
      label: Text(label),
      selected: selected,
      onSelected: onSelected,
      selectedColor: color.withValues(alpha: 0.25),
      checkmarkColor: color,
      side: BorderSide(
        color: selected ? color : AppTheme.neonCyan.withValues(alpha: 0.4),
      ),
    );
  }
}
