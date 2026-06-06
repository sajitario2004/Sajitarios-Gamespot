import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_flow_controller.dart';
import 'package:sajitarios_gamespot/games/wavelength/presentation/wavelength_routes.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/rules_screen.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla de configuración previa a la partida de Wavelength.
///
/// Permite añadir/quitar hasta [kWavelengthMaxPlayers] jugadores (mín.
/// [kWavelengthMinPlayers]) y elegir el número de rondas. Muestra el diálogo
/// [wavelengthSinEspectrosTitulo] si la BD no tiene espectros.
class WavelengthSetupScreen extends ConsumerStatefulWidget {
  const WavelengthSetupScreen({super.key});

  @override
  ConsumerState<WavelengthSetupScreen> createState() =>
      _WavelengthSetupScreenState();
}

class _WavelengthSetupScreenState extends ConsumerState<WavelengthSetupScreen> {
  final List<TextEditingController> _controllers = [];
  int _rondas = kWavelengthDefaultRondas;
  bool _iniciando = false;

  @override
  void initState() {
    super.initState();
    for (var i = 0; i < kWavelengthMinPlayers; i++) {
      _controllers.add(TextEditingController());
    }
  }

  @override
  void dispose() {
    for (final c in _controllers) {
      c.dispose();
    }
    super.dispose();
  }

  void _addPlayer() {
    if (_controllers.length >= kWavelengthMaxPlayers) return;
    setState(() => _controllers.add(TextEditingController()));
  }

  void _removePlayer(int index) {
    if (_controllers.length <= kWavelengthMinPlayers) return;
    setState(() => _controllers.removeAt(index).dispose());
  }

  String _normalizarNombre(String value) =>
      value.trim().replaceAll(RegExp(r'\s+'), ' ');

  Future<void> _iniciarPartida() async {
    final l10n = AppLocalizations.of(context)!;
    final names = _controllers
        .map((c) => _normalizarNombre(c.text))
        .toList(growable: false);

    final result = WavelengthConfig.create(playerNames: names, rondas: _rondas);

    if (!result.isSuccess) {
      _mostrarError(_mensajeError(l10n, result.error!));
      return;
    }

    setState(() => _iniciando = true);

    await ref
        .read(wavelengthFlowControllerProvider.notifier)
        .iniciar(result.config!);

    if (!mounted) return;

    final flowState = ref.read(wavelengthFlowControllerProvider);
    if (flowState.fase == WavelengthFase.error) {
      setState(() => _iniciando = false);
      switch (flowState.errorKind) {
        case WavelengthErrorKind.sinEspectros:
          await _mostrarErrorSinEspectros();
        case null:
          _mostrarError(l10n.errorNoSePudoIniciar);
      }
      return;
    }

    if (mounted) {
      context.goNamed(kWavelengthClueRouteName);
    }
  }

  String _mensajeError(AppLocalizations l10n, WavelengthConfigError error) {
    switch (error) {
      case WavelengthConfigError.pocosJugadores:
        return l10n.errorPocosJugadores(kWavelengthMinPlayers);
      case WavelengthConfigError.demasiadosJugadores:
        return l10n.errorDemasiadosJugadores(kWavelengthMaxPlayers);
      case WavelengthConfigError.nombreVacio:
        return l10n.errorNombreVacio;
      case WavelengthConfigError.nombresDuplicados:
        return l10n.errorNombresDuplicados;
      case WavelengthConfigError.rondasFueraDeRango:
        return l10n.errorNoSePudoIniciar;
    }
  }

  void _mostrarError(String mensaje) {
    final messenger = ScaffoldMessenger.of(context);
    messenger.hideCurrentSnackBar();
    messenger.showSnackBar(SnackBar(content: Text(mensaje)));
  }

  Future<void> _mostrarErrorSinEspectros() async {
    if (!mounted) return;
    final l10n = AppLocalizations.of(context)!;
    await showDialog<void>(
      context: context,
      builder: (dialogContext) {
        return AlertDialog(
          title: Text(l10n.wavelengthSinEspectrosTitulo),
          content: Text(l10n.wavelengthSinEspectrosMensaje),
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
    final puedeQuitar = _controllers.length > kWavelengthMinPlayers;
    final puedeAnadir = _controllers.length < kWavelengthMaxPlayers;

    return Scaffold(
      appBar: AppBar(
        leading: VolverAlMenuButton(onPressed: () => context.go('/')),
        title: NeonText(
          l10n.wavelengthTitulo,
          style: theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
          glowColor: AppTheme.neonCyan,
        ),
        actions: [
          IconButton(
            tooltip: l10n.comoSeJuega,
            icon: const Icon(Icons.help_outline),
            onPressed: () => Navigator.of(context).push(
              MaterialPageRoute<void>(
                builder: (_) => RulesScreen(
                  gameTitle: l10n.wavelengthTitulo,
                  steps: [
                    l10n.reglasWavelength1,
                    l10n.reglasWavelength2,
                    l10n.reglasWavelength3,
                    l10n.reglasWavelength4,
                    l10n.reglasWavelength5,
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
                  // ── Jugadores ───────────────────────────────────────────
                  NeonText(
                    l10n.setupJugadores,
                    style: theme.textTheme.titleLarge,
                    glowColor: AppTheme.neonCyan,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    l10n.wavelengthSetupRangoJugadores(
                      kWavelengthMinPlayers,
                      kWavelengthMaxPlayers,
                    ),
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: colors.onSurfaceVariant,
                    ),
                  ),
                  const SizedBox(height: 12),
                  ..._buildPlayerFields(theme, l10n, puedeQuitar),
                  const SizedBox(height: 8),
                  OutlinedButton.icon(
                    onPressed: puedeAnadir ? _addPlayer : null,
                    icon: const Icon(Icons.person_add_alt_1),
                    label: Text(
                      puedeAnadir
                          ? l10n.anadirJugador
                          : l10n.maximoJugadoresAlcanzado,
                    ),
                  ),
                  const SizedBox(height: 24),

                  // ── Rondas ───────────────────────────────────────────────
                  NeonText(
                    l10n.wavelengthSetupRondas,
                    style: theme.textTheme.titleLarge,
                    glowColor: AppTheme.neonCyan,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    l10n.wavelengthSetupRondasAyuda(
                      kWavelengthMinRondas,
                      kWavelengthMaxRondas,
                    ),
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: colors.onSurfaceVariant,
                    ),
                  ),
                  const SizedBox(height: 12),
                  _buildRondasSelector(theme, l10n),
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
                            ? l10n.wavelengthIniciando
                            : l10n.wavelengthEmpezarPartida,
                      ),
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

  List<Widget> _buildPlayerFields(
    ThemeData theme,
    AppLocalizations l10n,
    bool puedeQuitar,
  ) {
    return [
      for (var i = 0; i < _controllers.length; i++)
        Padding(
          padding: const EdgeInsets.only(bottom: 12),
          child: Row(
            children: [
              ExcludeSemantics(
                child: NeonGlowWrapper(
                  color: AppTheme.neonCyan,
                  intensity: 0.7,
                  borderRadius: const BorderRadius.all(Radius.circular(20)),
                  child: CircleAvatar(
                    radius: 16,
                    backgroundColor: AppTheme.surfaceHigh,
                    child: Text(
                      '${i + 1}',
                      style: theme.textTheme.labelLarge?.copyWith(
                        color: AppTheme.neonCyan,
                        fontWeight: FontWeight.w900,
                      ),
                    ),
                  ),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: TextField(
                  controller: _controllers[i],
                  textCapitalization: TextCapitalization.words,
                  textInputAction: i == _controllers.length - 1
                      ? TextInputAction.done
                      : TextInputAction.next,
                  decoration: InputDecoration(
                    labelText: l10n.jugadorNumero(i + 1),
                    hintText: l10n.nombreDelJugadorNumero(i + 1),
                  ),
                ),
              ),
              const SizedBox(width: 4),
              IconButton(
                tooltip: l10n.quitarJugador,
                onPressed: puedeQuitar ? () => _removePlayer(i) : null,
                icon: const Icon(Icons.remove_circle_outline),
              ),
            ],
          ),
        ),
    ];
  }

  Widget _buildRondasSelector(ThemeData theme, AppLocalizations l10n) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        IconButton(
          tooltip: l10n.menosRondas,
          onPressed: _rondas > kWavelengthMinRondas
              ? () => setState(() => _rondas--)
              : null,
          icon: const Icon(Icons.remove_circle_outline),
        ),
        const SizedBox(width: 8),
        NeonText(
          '$_rondas',
          glowColor: AppTheme.neonCyan,
          style: theme.textTheme.headlineSmall?.copyWith(
            fontWeight: FontWeight.w900,
          ),
        ),
        const SizedBox(width: 8),
        IconButton(
          tooltip: l10n.masRondas,
          onPressed: _rondas < kWavelengthMaxRondas
              ? () => setState(() => _rondas++)
              : null,
          icon: const Icon(Icons.add_circle_outline),
        ),
      ],
    );
  }
}
