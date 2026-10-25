import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/player.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_routes.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/rules_screen.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Configuración previa a la partida del Impostor.
///
/// Permite añadir, quitar y editar los nombres de los jugadores (mín. 3 /
/// máx. 15) con validación en UI (nombres no vacíos ni duplicados), ajustar el
/// nº de impostores (1..5, capado dinámicamente a
/// [GameConfig.maxImpostoresFor]) y activar/desactivar la pista.
///
/// Al confirmar, construye la [GameConfig] vía [GameConfig.create], muestra los
/// errores localizados si los hubiera y arranca el flujo con
/// `impostorFlowControllerProvider.notifier.iniciar(config)`, navegando a la
/// primera [PassDeviceScreen] (ruta `impostor-pass`).
class SetupScreen extends ConsumerStatefulWidget {
  const SetupScreen({super.key});

  @override
  ConsumerState<SetupScreen> createState() => _SetupScreenState();
}

class _SetupScreenState extends ConsumerState<SetupScreen> {
  /// Un controlador por cada campo de nombre de jugador. El orden de esta lista
  /// es el orden de introducción (= orden de revelación).
  final List<TextEditingController> _controllers = [];

  /// Nº de impostores seleccionado por el usuario (antes de capar).
  int _nImpostores = kMinImpostores;

  /// Nº de rondas (oportunidades de voto) seleccionado por el usuario (antes de
  /// capar). Arranca en el máximo posible para el mínimo de jugadores.
  int _rounds = kMinRounds;

  /// Estado del switch de pista.
  bool _hintEnabled = false;

  /// `true` mientras se está iniciando la partida (asignando roles).
  bool _iniciando = false;

  @override
  void initState() {
    super.initState();
    // Arranca con el mínimo de jugadores requeridos.
    for (var i = 0; i < kMinPlayers; i++) {
      _controllers.add(TextEditingController());
    }
    // Por defecto, todas las oportunidades de voto posibles.
    _rounds = _maxRounds;
  }

  @override
  void dispose() {
    for (final controller in _controllers) {
      controller.dispose();
    }
    super.dispose();
  }

  /// Tope efectivo de impostores para la cantidad actual de jugadores.
  int get _maxImpostores => GameConfig.maxImpostoresFor(_controllers.length);

  /// Tope efectivo de rondas (oportunidades de voto) para la cantidad actual de
  /// jugadores.
  int get _maxRounds => GameConfig.maxRoundsFor(_controllers.length);

  void _addPlayer() {
    if (_controllers.length >= kMaxPlayers) return;
    setState(() {
      _controllers.add(TextEditingController());
      _clampRounds();
    });
  }

  void _removePlayer(int index) {
    if (_controllers.length <= kMinPlayers) return;
    setState(() {
      _controllers.removeAt(index).dispose();
      _clampImpostores();
      _clampRounds();
    });
  }

  /// Reajusta el nº de impostores al rango válido cuando cambia la cantidad de
  /// jugadores.
  void _clampImpostores() {
    _nImpostores = _nImpostores.clamp(kMinImpostores, _maxImpostores);
  }

  /// Reajusta el nº de rondas al rango válido cuando cambia la cantidad de
  /// jugadores.
  void _clampRounds() {
    _rounds = _rounds.clamp(kMinRounds, _maxRounds);
  }

  /// Traduce un [GameConfigError] a un mensaje localizado para el usuario.
  String _mensajeError(AppLocalizations l10n, GameConfigError error) {
    switch (error) {
      case GameConfigError.pocosJugadores:
        return l10n.errorPocosJugadores(kMinPlayers);
      case GameConfigError.demasiadosJugadores:
        return l10n.errorDemasiadosJugadores(kMaxPlayers);
      case GameConfigError.nombresDuplicados:
        return l10n.errorNombresDuplicados;
      case GameConfigError.nombreVacio:
        return l10n.errorNombreVacio;
    }
  }

  void _mostrarError(String mensaje) {
    final messenger = ScaffoldMessenger.of(context);
    messenger.hideCurrentSnackBar();
    messenger.showSnackBar(SnackBar(content: Text(mensaje)));
  }

  /// Normaliza un nombre introducido: recorta los extremos y colapsa cualquier
  /// secuencia de espacios internos a uno solo. Así "Nacho   López" y
  /// "Nacho López" se tratan como el mismo nombre y la detección de duplicados
  /// (insensible a mayúsculas) funciona aunque el usuario teclee espacios de
  /// más.
  String _normalizarNombre(String value) =>
      value.trim().replaceAll(RegExp(r'\s+'), ' ');

  Future<void> _iniciarPartida() async {
    final l10n = AppLocalizations.of(context)!;
    final players = _controllers
        .map((c) => Player(_normalizarNombre(c.text)))
        .toList(growable: false);

    final result = GameConfig.create(
      players: players,
      nImpostores: _nImpostores,
      hintEnabled: _hintEnabled,
      rounds: _rounds,
    );

    if (!result.isSuccess) {
      _mostrarError(_mensajeError(l10n, result.error!));
      return;
    }

    setState(() => _iniciando = true);

    await ref
        .read(impostorFlowControllerProvider.notifier)
        .iniciar(result.config!);

    if (!mounted) return;

    final flowState = ref.read(impostorFlowControllerProvider);
    if (flowState.phase == ImpostorPhase.error) {
      setState(() => _iniciando = false);
      // El texto para el usuario se deriva del tipo de error (no del mensaje en
      // español del controlador, que es solo para depuración/logging).
      switch (flowState.errorKind) {
        case ImpostorErrorKind.sinPalabras:
          await _mostrarErrorSinPalabras();
        case ImpostorErrorKind.desconocido:
        case null:
          _mostrarError(l10n.errorNoSePudoIniciar);
      }
      return;
    }

    context.goNamed(kImpostorPassRouteName);
  }

  /// Cuando la BD no tiene palabras, un snackbar fugaz se queda corto: mostramos
  /// un diálogo claro que explica el problema y ofrece ir a gestionar palabras
  /// para añadir la primera.
  Future<void> _mostrarErrorSinPalabras() async {
    final irAGestionar = await showDialog<bool>(
      context: context,
      builder: (dialogContext) {
        final l10n = AppLocalizations.of(dialogContext)!;
        return AlertDialog(
          title: Text(l10n.noHayPalabrasTitulo),
          content: Text(l10n.noHayPalabrasMensaje),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(dialogContext).pop(false),
              child: Text(l10n.ahoraNo),
            ),
            FilledButton(
              onPressed: () => Navigator.of(dialogContext).pop(true),
              child: Text(l10n.gestionarPalabras),
            ),
          ],
        );
      },
    );
    if (irAGestionar != true || !mounted) return;
    context.goNamed(kImpostorWordsRouteName);
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;
    final l10n = AppLocalizations.of(context)!;
    final puedeQuitar = _controllers.length > kMinPlayers;
    final puedeAnadir = _controllers.length < kMaxPlayers;

    return Scaffold(
      appBar: AppBar(
        leading: VolverAlMenuButton(onPressed: () => context.go('/')),
        title: NeonText(
          l10n.impostorTitulo,
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
                  gameTitle: l10n.impostorTitulo,
                  steps: [
                    l10n.reglasImpostor1,
                    l10n.reglasImpostor2,
                    l10n.reglasImpostor3,
                    l10n.reglasImpostor4,
                    l10n.reglasImpostor5,
                    l10n.reglasImpostor6,
                  ],
                ),
              ),
            ),
          ),
          IconButton(
            tooltip: l10n.historial,
            icon: const Icon(Icons.history),
            onPressed: _iniciando
                ? null
                : () => context.goNamed(kImpostorHistoryRouteName),
          ),
          IconButton(
            tooltip: l10n.gestionarPalabras,
            icon: const Icon(Icons.menu_book_outlined),
            onPressed: _iniciando
                ? null
                : () => context.goNamed(kImpostorWordsRouteName),
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
                  NeonText(
                    l10n.setupJugadores,
                    style: theme.textTheme.titleLarge,
                    glowColor: AppTheme.neonCyan,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    l10n.setupRangoJugadores(kMinPlayers, kMaxPlayers),
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
                  NeonText(
                    l10n.setupImpostores,
                    style: theme.textTheme.titleLarge,
                    glowColor: AppTheme.neonMagenta,
                  ),
                  const SizedBox(height: 8),
                  _buildImpostoresStepper(theme, l10n),
                  const SizedBox(height: 24),
                  NeonText(
                    l10n.setupRondas,
                    style: theme.textTheme.titleLarge,
                    glowColor: AppTheme.neonCyan,
                  ),
                  const SizedBox(height: 8),
                  _buildRoundsStepper(theme, l10n),
                  const SizedBox(height: 24),
                  NeonText(
                    l10n.setupPista,
                    style: theme.textTheme.titleLarge,
                    glowColor: AppTheme.neonViolet,
                  ),
                  const SizedBox(height: 8),
                  _buildHintSwitch(theme, l10n),
                  const SizedBox(height: 32),
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
                            ? l10n.iniciandoPartida
                            : l10n.empezarPartida,
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

  Widget _buildImpostoresStepper(ThemeData theme, AppLocalizations l10n) {
    final max = _maxImpostores;
    final puedeBajar = _nImpostores > kMinImpostores;
    final puedeSubir = _nImpostores < max;

    return NeonPanel(
      borderColor: AppTheme.neonMagenta,
      intensity: 0.6,
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                NeonText(
                  l10n.impostoresContador(_nImpostores),
                  style: theme.textTheme.titleMedium,
                  glowColor: AppTheme.neonMagenta,
                ),
                Text(
                  l10n.maximoImpostoresPara(max, _controllers.length),
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
          ),
          IconButton.filledTonal(
            tooltip: l10n.menosImpostores,
            onPressed: puedeBajar ? () => setState(() => _nImpostores--) : null,
            icon: const Icon(Icons.remove),
          ),
          const SizedBox(width: 8),
          IconButton.filledTonal(
            tooltip: l10n.masImpostores,
            onPressed: puedeSubir ? () => setState(() => _nImpostores++) : null,
            icon: const Icon(Icons.add),
          ),
        ],
      ),
    );
  }

  Widget _buildRoundsStepper(ThemeData theme, AppLocalizations l10n) {
    final max = _maxRounds;
    final puedeBajar = _rounds > kMinRounds;
    final puedeSubir = _rounds < max;

    return NeonPanel(
      borderColor: AppTheme.neonCyan,
      intensity: 0.6,
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                NeonText(
                  l10n.rondasContador(_rounds),
                  style: theme.textTheme.titleMedium,
                  glowColor: AppTheme.neonCyan,
                ),
                Text(
                  l10n.setupRondasAyuda(kMinRounds, max),
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
              ],
            ),
          ),
          IconButton.filledTonal(
            tooltip: l10n.menosRondas,
            onPressed: puedeBajar ? () => setState(() => _rounds--) : null,
            icon: const Icon(Icons.remove),
          ),
          const SizedBox(width: 8),
          IconButton.filledTonal(
            tooltip: l10n.masRondas,
            onPressed: puedeSubir ? () => setState(() => _rounds++) : null,
            icon: const Icon(Icons.add),
          ),
        ],
      ),
    );
  }

  Widget _buildHintSwitch(ThemeData theme, AppLocalizations l10n) {
    return NeonPanel(
      borderColor: AppTheme.neonViolet,
      intensity: 0.6,
      padding: EdgeInsets.zero,
      child: SwitchListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
        title: Text(l10n.darPistaAlImpostor),
        subtitle: Text(
          l10n.darPistaAlImpostorSubtitulo,
          style: theme.textTheme.bodySmall?.copyWith(
            color: theme.colorScheme.onSurfaceVariant,
          ),
        ),
        value: _hintEnabled,
        onChanged: (value) => setState(() => _hintEnabled = value),
      ),
    );
  }
}
