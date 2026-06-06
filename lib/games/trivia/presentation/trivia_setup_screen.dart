import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/trivia/data/winner_repository.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/tematica.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_flow_controller.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_repositories_provider.dart';
import 'package:sajitarios_gamespot/games/trivia/presentation/trivia_routes.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/rules_screen.dart';
import 'package:sajitarios_gamespot/games/_shared/presentation/volver_al_menu_button.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Temáticas disponibles con sus etiquetas en español.
const List<Tematica> kTriviaTematicas = [
  Tematica(id: 'cultura_general', nombre: 'Cultura general'),
  Tematica(id: 'videojuegos', nombre: 'Videojuegos'),
  Tematica(id: 'cocina', nombre: 'Cocina'),
  Tematica(id: 'cine', nombre: 'Cine'),
  Tematica(id: 'ciencia', nombre: 'Ciencia'),
  Tematica(id: 'geografia', nombre: 'Geografía'),
  Tematica(id: 'historia', nombre: 'Historia'),
  Tematica(id: 'deportes', nombre: 'Deportes'),
  Tematica(id: 'musica', nombre: 'Música'),
];

/// Pantalla de configuración previa a la partida de Trivia.
///
/// Permite añadir/quitar hasta [kTriviaMaxPlayers] jugadores (mín.
/// [kTriviaMinPlayers]), seleccionar las temáticas (multi-select chips) y
/// consultar el ranking de victorias antes de empezar.
class TriviaSetupScreen extends ConsumerStatefulWidget {
  const TriviaSetupScreen({super.key});

  @override
  ConsumerState<TriviaSetupScreen> createState() => _TriviaSetupScreenState();
}

class _TriviaSetupScreenState extends ConsumerState<TriviaSetupScreen> {
  final List<TextEditingController> _controllers = [];
  final Set<String> _selectedTematicaIds = {'cultura_general', 'historia'};
  bool _iniciando = false;

  @override
  void initState() {
    super.initState();
    for (var i = 0; i < kTriviaMinPlayers; i++) {
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
    if (_controllers.length >= kTriviaMaxPlayers) return;
    setState(() => _controllers.add(TextEditingController()));
  }

  void _removePlayer(int index) {
    if (_controllers.length <= kTriviaMinPlayers) return;
    setState(() => _controllers.removeAt(index).dispose());
  }

  String _normalizarNombre(String value) =>
      value.trim().replaceAll(RegExp(r'\s+'), ' ');

  void _toggleTematica(String id) {
    setState(() {
      if (_selectedTematicaIds.contains(id)) {
        if (_selectedTematicaIds.length > 1) {
          _selectedTematicaIds.remove(id);
        }
      } else {
        _selectedTematicaIds.add(id);
      }
    });
  }

  Future<void> _iniciarPartida() async {
    final l10n = AppLocalizations.of(context)!;
    final names = _controllers
        .map((c) => _normalizarNombre(c.text))
        .toList(growable: false);

    final result = TriviaConfig.create(
      playerNames: names,
      selectedTematicaIds: Set<String>.from(_selectedTematicaIds),
    );

    if (!result.isSuccess) {
      _mostrarError(_mensajeError(l10n, result.error!));
      return;
    }

    setState(() => _iniciando = true);

    await ref
        .read(triviaFlowControllerProvider.notifier)
        .iniciar(result.config!);

    if (!mounted) return;

    final flowState = ref.read(triviaFlowControllerProvider);
    if (flowState.fase == TriviaFase.error) {
      setState(() => _iniciando = false);
      switch (flowState.errorKind) {
        case TriviaErrorKind.sinPreguntas:
          await _mostrarErrorSinPreguntas();
        case null:
          _mostrarError(l10n.errorNoSePudoIniciar);
      }
      return;
    }

    if (mounted) {
      context.goNamed(kTriviaPassRouteName);
    }
  }

  String _mensajeError(AppLocalizations l10n, TriviaConfigError error) {
    switch (error) {
      case TriviaConfigError.pocosJugadores:
        return l10n.errorPocosJugadores(kTriviaMinPlayers);
      case TriviaConfigError.demasiadosJugadores:
        return l10n.errorDemasiadosJugadores(kTriviaMaxPlayers);
      case TriviaConfigError.nombreVacio:
        return l10n.errorNombreVacio;
      case TriviaConfigError.nombresDuplicados:
        return l10n.errorNombresDuplicados;
      case TriviaConfigError.sinTematicas:
        return l10n.triviaSetupTematicasAyuda;
    }
  }

  void _mostrarError(String mensaje) {
    final messenger = ScaffoldMessenger.of(context);
    messenger.hideCurrentSnackBar();
    messenger.showSnackBar(SnackBar(content: Text(mensaje)));
  }

  Future<void> _mostrarErrorSinPreguntas() async {
    if (!mounted) return;
    final l10n = AppLocalizations.of(context)!;
    await showDialog<void>(
      context: context,
      builder: (dialogContext) {
        return AlertDialog(
          title: Text(l10n.triviaNoHayPreguntasTitulo),
          content: Text(l10n.triviaNoHayPreguntasMensaje),
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
    final puedeQuitar = _controllers.length > kTriviaMinPlayers;
    final puedeAnadir = _controllers.length < kTriviaMaxPlayers;

    return Scaffold(
      appBar: AppBar(
        leading: VolverAlMenuButton(onPressed: () => context.go('/')),
        title: NeonText(
          l10n.triviaTitulo,
          style: theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
          glowColor: AppTheme.neonViolet,
        ),
        actions: [
          IconButton(
            tooltip: l10n.comoSeJuega,
            icon: const Icon(Icons.help_outline),
            onPressed: () => Navigator.of(context).push(
              MaterialPageRoute<void>(
                builder: (_) => RulesScreen(
                  gameTitle: l10n.triviaTitulo,
                  steps: [
                    l10n.reglasTrivia1,
                    l10n.reglasTrivia2,
                    l10n.reglasTrivia3,
                    l10n.reglasTrivia4,
                    l10n.reglasTrivia5,
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
                    l10n.triviaSetupRangoJugadores(
                      kTriviaMinPlayers,
                      kTriviaMaxPlayers,
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

                  // ── Temáticas ────────────────────────────────────────────
                  NeonText(
                    l10n.triviaSetupTematicas,
                    style: theme.textTheme.titleLarge,
                    glowColor: AppTheme.neonViolet,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    l10n.triviaSetupTematicasAyuda,
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: colors.onSurfaceVariant,
                    ),
                  ),
                  const SizedBox(height: 12),
                  _buildTematicasChips(theme),
                  const SizedBox(height: 24),

                  // ── Ranking de victorias ─────────────────────────────────
                  _TriviaRankingWidget(iniciando: _iniciando),
                  const SizedBox(height: 24),

                  // ── Atribución OpenTDB ───────────────────────────────────
                  Text(
                    l10n.triviaAtribucion,
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: colors.onSurfaceVariant,
                    ),
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 16),

                  // ── Botón Empezar ────────────────────────────────────────
                  NeonGlowWrapper(
                    color: AppTheme.neonViolet,
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
                            ? l10n.triviaIniciando
                            : l10n.triviaEmpezarPartida,
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
                  color: AppTheme.neonViolet,
                  intensity: 0.7,
                  borderRadius: const BorderRadius.all(Radius.circular(20)),
                  child: CircleAvatar(
                    radius: 16,
                    backgroundColor: AppTheme.surfaceHigh,
                    child: Text(
                      '${i + 1}',
                      style: theme.textTheme.labelLarge?.copyWith(
                        color: AppTheme.neonVioletText,
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

  Widget _buildTematicasChips(ThemeData theme) {
    return Wrap(
      spacing: 8,
      runSpacing: 8,
      children: [
        for (final tematica in kTriviaTematicas)
          FilterChip(
            label: Text(tematica.nombre),
            selected: _selectedTematicaIds.contains(tematica.id),
            onSelected: (_) => _toggleTematica(tematica.id),
            selectedColor: AppTheme.neonViolet.withValues(alpha: 0.25),
            checkmarkColor: AppTheme.neonVioletText,
            side: BorderSide(
              color: _selectedTematicaIds.contains(tematica.id)
                  ? AppTheme.neonViolet
                  : AppTheme.neonCyan.withValues(alpha: 0.4),
            ),
          ),
      ],
    );
  }
}

/// Widget que muestra el ranking de victorias de trivia (lectura asíncrona).
class _TriviaRankingWidget extends ConsumerWidget {
  const _TriviaRankingWidget({required this.iniciando});

  final bool iniciando;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    final theme = Theme.of(context);
    final winnerRepoAsync = ref.watch(winnerRepositoryProvider);

    return NeonPanel(
      borderColor: AppTheme.neonViolet,
      intensity: 0.5,
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          NeonText(
            l10n.triviaRankingVictorias,
            style: theme.textTheme.titleMedium,
            glowColor: AppTheme.neonViolet,
          ),
          const SizedBox(height: 8),
          winnerRepoAsync.when(
            loading: () => const SizedBox(
              height: 24,
              child: Center(
                child: SizedBox(
                  width: 16,
                  height: 16,
                  child: CircularProgressIndicator(strokeWidth: 2),
                ),
              ),
            ),
            error: (e, st) => const SizedBox.shrink(),
            data: (repo) => _RankingList(repo: repo),
          ),
        ],
      ),
    );
  }
}

class _RankingList extends StatefulWidget {
  const _RankingList({required this.repo});

  final dynamic repo;

  @override
  State<_RankingList> createState() => _RankingListState();
}

class _RankingListState extends State<_RankingList> {
  List<WinnerRecord>? _records;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final records =
        await (widget.repo as dynamic).getAllRanked() as List<WinnerRecord>;
    if (mounted) setState(() => _records = records);
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final theme = Theme.of(context);
    final records = _records;

    if (records == null) {
      return const SizedBox(
        height: 24,
        child: Center(
          child: SizedBox(
            width: 16,
            height: 16,
            child: CircularProgressIndicator(strokeWidth: 2),
          ),
        ),
      );
    }

    if (records.isEmpty) {
      return Text(
        l10n.triviaSinVictorias,
        style: theme.textTheme.bodySmall?.copyWith(
          color: theme.colorScheme.onSurfaceVariant,
        ),
      );
    }

    return Column(
      children: [
        for (final record in records)
          Padding(
            padding: const EdgeInsets.only(bottom: 4),
            child: Row(
              children: [
                Expanded(
                  child: Text(
                    record.name,
                    style: theme.textTheme.bodyMedium,
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
                NeonText(
                  l10n.triviaVictorias(record.wins),
                  style: theme.textTheme.bodySmall,
                  glowColor: AppTheme.neonViolet,
                ),
              ],
            ),
          ),
      ],
    );
  }
}
