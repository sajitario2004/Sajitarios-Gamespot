import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/impostor/data/impostor_word.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Pantalla de gestión (CRUD) del banco de palabras del Impostor.
///
/// Versiones cubiertas:
/// - 0.22 Listar palabras (seed + propias) con búsqueda por texto.
/// - 0.23 Agregar nueva palabra (`word` + `hint` requeridos, unicidad).
/// - 0.24 Editar/borrar solo las del usuario (`isSeed = false`); las seed son de
///   solo lectura (acciones deshabilitadas y marcadas visualmente).
///
/// Lee el [wordRepositoryProvider] para todas las operaciones. Es responsive:
/// el contenido se centra y se limita a un ancho cómodo de lectura.
class WordsManagementScreen extends ConsumerStatefulWidget {
  const WordsManagementScreen({super.key});

  @override
  ConsumerState<WordsManagementScreen> createState() =>
      _WordsManagementScreenState();
}

class _WordsManagementScreenState extends ConsumerState<WordsManagementScreen> {
  final TextEditingController _searchController = TextEditingController();

  /// Texto de búsqueda aplicado actualmente.
  String _query = '';

  /// Retardo (debounce) entre la última pulsación y la consulta a sqflite.
  static const Duration _debounceDuration = Duration(milliseconds: 280);

  /// Temporizador de debounce de la búsqueda; se cancela en cada pulsación y en
  /// [dispose] para no disparar consultas sobre un estado ya desmontado.
  Timer? _searchDebounce;

  /// Futuro de la carga actual de palabras; se reemplaza al refrescar o buscar.
  late Future<List<ImpostorWord>> _wordsFuture;

  @override
  void initState() {
    super.initState();
    _wordsFuture = _loadWords();
  }

  @override
  void dispose() {
    _searchDebounce?.cancel();
    _searchController.dispose();
    super.dispose();
  }

  Future<List<ImpostorWord>> _loadWords() {
    final repo = ref.read(wordRepositoryProvider);
    return _query.isEmpty ? repo.getAll() : repo.search(_query);
  }

  /// Vuelve a leer las palabras (tras alta/edición/borrado o cambio de filtro).
  void _refresh() {
    setState(() {
      _wordsFuture = _loadWords();
    });
  }

  void _onSearchChanged(String value) {
    final nuevaQuery = value.trim();
    // Refleja de inmediato el cambio de texto en la UI (p. ej. el botón de
    // limpiar depende de `_query.isEmpty`), pero difiere la consulta a sqflite.
    setState(() {
      _query = nuevaQuery;
    });
    _searchDebounce?.cancel();
    _searchDebounce = Timer(_debounceDuration, () {
      if (!mounted) return;
      _refresh();
    });
  }

  void _mostrarMensaje(String mensaje) {
    final messenger = ScaffoldMessenger.of(context);
    messenger.hideCurrentSnackBar();
    messenger.showSnackBar(SnackBar(content: Text(mensaje)));
  }

  // ---------------------------------------------------------------------------
  // 0.23 — Agregar
  // ---------------------------------------------------------------------------

  Future<void> _agregarPalabra() async {
    final l10n = AppLocalizations.of(context)!;
    final resultado = await showDialog<_WordFormResult>(
      context: context,
      builder: (_) => const _WordFormDialog(),
    );
    if (resultado == null || !mounted) return;

    final repo = ref.read(wordRepositoryProvider);
    try {
      await repo.insert(word: resultado.word, hint: resultado.hint);
      if (!mounted) return;
      _mostrarMensaje(l10n.palabraAnadida);
      _refresh();
    } on DuplicateWordException {
      if (!mounted) return;
      _mostrarMensaje(l10n.yaExisteEsaPalabra);
    } on ArgumentError {
      if (!mounted) return;
      _mostrarMensaje(l10n.palabraYPistaObligatorias);
    }
  }

  // ---------------------------------------------------------------------------
  // 0.24 — Editar / borrar (solo palabras de usuario)
  // ---------------------------------------------------------------------------

  Future<void> _editarPalabra(ImpostorWord word) async {
    if (word.isSeed || word.id == null) return;

    final l10n = AppLocalizations.of(context)!;
    final resultado = await showDialog<_WordFormResult>(
      context: context,
      builder: (_) => _WordFormDialog(initial: word),
    );
    if (resultado == null || !mounted) return;

    final repo = ref.read(wordRepositoryProvider);
    try {
      await repo.update(
        id: word.id!,
        word: resultado.word,
        hint: resultado.hint,
      );
      if (!mounted) return;
      _mostrarMensaje(l10n.palabraActualizada);
      _refresh();
    } on DuplicateWordException {
      if (!mounted) return;
      _mostrarMensaje(l10n.yaExisteEsaPalabra);
    } on ReadOnlySeedWordException {
      if (!mounted) return;
      _mostrarMensaje(l10n.palabrasPredefinidasSoloLectura);
    } on WordNotFoundException {
      if (!mounted) return;
      _mostrarMensaje(l10n.esaPalabraYaNoExiste);
      _refresh();
    } on ArgumentError {
      if (!mounted) return;
      _mostrarMensaje(l10n.palabraYPistaObligatorias);
    }
  }

  Future<void> _borrarPalabra(ImpostorWord word) async {
    if (word.isSeed || word.id == null) return;

    final l10n = AppLocalizations.of(context)!;
    final confirmar = await showDialog<bool>(
      context: context,
      builder: (dialogContext) {
        final dl10n = AppLocalizations.of(dialogContext)!;
        return AlertDialog(
          title: Text(dl10n.borrarPalabraTitulo),
          content: Text(dl10n.borrarPalabraMensaje(word.word)),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(dialogContext).pop(false),
              child: Text(dl10n.cancelar),
            ),
            NeonGlowWrapper(
              color: AppTheme.neonError,
              borderRadius: const BorderRadius.all(Radius.circular(12)),
              child: FilledButton(
                style: FilledButton.styleFrom(
                  backgroundColor: AppTheme.neonError,
                  foregroundColor: AppTheme.background,
                ),
                onPressed: () => Navigator.of(dialogContext).pop(true),
                child: Text(dl10n.borrar),
              ),
            ),
          ],
        );
      },
    );
    if (confirmar != true || !mounted) return;

    final repo = ref.read(wordRepositoryProvider);
    try {
      await repo.delete(word.id!);
      if (!mounted) return;
      _mostrarMensaje(l10n.palabraBorrada);
      _refresh();
    } on ReadOnlySeedWordException {
      if (!mounted) return;
      _mostrarMensaje(l10n.palabrasPredefinidasSoloLectura);
    } on WordNotFoundException {
      if (!mounted) return;
      _mostrarMensaje(l10n.esaPalabraYaNoExiste);
      _refresh();
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;

    return Scaffold(
      appBar: AppBar(title: Text(l10n.gestionarPalabrasTitulo)),
      floatingActionButton: NeonGlowWrapper(
        color: AppTheme.neonCyan,
        borderRadius: const BorderRadius.all(Radius.circular(16)),
        child: FloatingActionButton.extended(
          onPressed: _agregarPalabra,
          icon: const Icon(Icons.add),
          label: Text(l10n.agregar),
        ),
      ),
      body: NeonBackground(
        child: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 640),
              child: Column(
                children: [
                  Padding(
                    padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                    child: TextField(
                      controller: _searchController,
                      onChanged: _onSearchChanged,
                      textInputAction: TextInputAction.search,
                      decoration: InputDecoration(
                        hintText: l10n.buscarPalabra,
                        prefixIcon: const Icon(Icons.search),
                        suffixIcon: _query.isEmpty
                            ? null
                            : IconButton(
                                tooltip: l10n.limpiarBusqueda,
                                icon: const Icon(Icons.clear),
                                onPressed: () {
                                  _searchController.clear();
                                  _onSearchChanged('');
                                },
                              ),
                      ),
                    ),
                  ),
                  Expanded(
                    child: FutureBuilder<List<ImpostorWord>>(
                      future: _wordsFuture,
                      builder: (context, snapshot) {
                        if (snapshot.connectionState ==
                            ConnectionState.waiting) {
                          return const Center(
                            child: CircularProgressIndicator(),
                          );
                        }
                        if (snapshot.hasError) {
                          return _MensajeCentral(
                            icono: Icons.error_outline,
                            texto: l10n.noSePudieronCargarPalabras,
                          );
                        }
                        final words = snapshot.data ?? const <ImpostorWord>[];
                        if (words.isEmpty) {
                          return _MensajeCentral(
                            icono: Icons.search_off,
                            texto: _query.isEmpty
                                ? l10n.sinPalabrasAun
                                : l10n.sinCoincidencias,
                          );
                        }
                        return ListView.separated(
                          padding: const EdgeInsets.fromLTRB(16, 8, 16, 96),
                          itemCount: words.length,
                          separatorBuilder: (_, _) => const SizedBox(height: 8),
                          itemBuilder: (context, index) {
                            return _WordTile(
                              word: words[index],
                              theme: theme,
                              onEdit: _editarPalabra,
                              onDelete: _borrarPalabra,
                            );
                          },
                        );
                      },
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

/// Tarjeta de una palabra en la lista.
///
/// Muestra `word` + `hint`. Para las palabras seed muestra un chip
/// "Predefinida" con un candado y deshabilita editar/borrar.
class _WordTile extends StatelessWidget {
  const _WordTile({
    required this.word,
    required this.theme,
    required this.onEdit,
    required this.onDelete,
  });

  final ImpostorWord word;
  final ThemeData theme;
  final ValueChanged<ImpostorWord> onEdit;
  final ValueChanged<ImpostorWord> onDelete;

  @override
  Widget build(BuildContext context) {
    final colors = theme.colorScheme;
    final l10n = AppLocalizations.of(context)!;
    final esSeed = word.isSeed;
    // Las seed se marcan en violeta de apoyo; las del usuario en cian acento.
    final borderColor = esSeed ? AppTheme.neonViolet : AppTheme.neonCyan;

    return NeonPanel(
      borderColor: borderColor,
      intensity: 0.55,
      borderWidth: 1.2,
      borderRadius: const BorderRadius.all(Radius.circular(16)),
      padding: const EdgeInsets.fromLTRB(16, 12, 8, 12),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Flexible(
                      child: NeonText(
                        word.word,
                        glowColor: borderColor,
                        intensity: 0.6,
                        style: theme.textTheme.titleMedium,
                      ),
                    ),
                    if (esSeed) ...[
                      const SizedBox(width: 8),
                      _SeedChip(theme: theme),
                    ],
                  ],
                ),
                const SizedBox(height: 2),
                Text(
                  l10n.pistaConValor(word.hint),
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: colors.onSurfaceVariant,
                  ),
                ),
              ],
            ),
          ),
          IconButton(
            tooltip: esSeed ? l10n.palabrasPredefinidasNoEditar : l10n.editar,
            onPressed: esSeed ? null : () => onEdit(word),
            icon: const Icon(Icons.edit_outlined),
            color: esSeed ? null : AppTheme.neonCyan,
          ),
          IconButton(
            tooltip: esSeed ? l10n.palabrasPredefinidasNoBorrar : l10n.borrar,
            onPressed: esSeed ? null : () => onDelete(word),
            icon: const Icon(Icons.delete_outline),
            color: esSeed ? null : AppTheme.neonError,
          ),
        ],
      ),
    );
  }
}

/// Chip visual que marca una palabra como predefinida (solo lectura).
class _SeedChip extends StatelessWidget {
  const _SeedChip({required this.theme});

  final ThemeData theme;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: AppTheme.neonViolet.withValues(alpha: 0.14),
        borderRadius: BorderRadius.circular(8),
        border: Border.all(
          color: AppTheme.neonViolet.withValues(alpha: 0.8),
          width: 1.0,
        ),
        boxShadow: neonGlow(color: AppTheme.neonViolet, intensity: 0.4),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.lock_outline, size: 14, color: AppTheme.neonViolet),
          const SizedBox(width: 4),
          Text(
            AppLocalizations.of(context)!.predefinida,
            style: theme.textTheme.labelSmall?.copyWith(
              color: AppTheme.textPrimary,
              fontWeight: FontWeight.w700,
              shadows: neonTextShadows(
                color: AppTheme.neonViolet,
                intensity: 0.5,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

/// Mensaje central reutilizable para los estados vacío y de error.
class _MensajeCentral extends StatelessWidget {
  const _MensajeCentral({required this.icono, required this.texto});

  final IconData icono;
  final String texto;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icono, size: 56, color: theme.colorScheme.onSurfaceVariant),
            const SizedBox(height: 12),
            Text(
              texto,
              textAlign: TextAlign.center,
              style: theme.textTheme.bodyLarge?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Resultado validado del formulario de alta/edición de palabra.
class _WordFormResult {
  const _WordFormResult({required this.word, required this.hint});

  final String word;
  final String hint;
}

/// Diálogo con formulario para crear (0.23) o editar (0.24) una palabra.
///
/// Si [initial] es `null` es un alta; si trae una palabra es una edición y
/// precarga los campos. Ambos campos son obligatorios (validados en UI).
class _WordFormDialog extends StatefulWidget {
  const _WordFormDialog({this.initial});

  final ImpostorWord? initial;

  @override
  State<_WordFormDialog> createState() => _WordFormDialogState();
}

class _WordFormDialogState extends State<_WordFormDialog> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _wordController;
  late final TextEditingController _hintController;

  bool get _isEditing => widget.initial != null;

  @override
  void initState() {
    super.initState();
    _wordController = TextEditingController(text: widget.initial?.word ?? '');
    _hintController = TextEditingController(text: widget.initial?.hint ?? '');
  }

  @override
  void dispose() {
    _wordController.dispose();
    _hintController.dispose();
    super.dispose();
  }

  String? _validarRequerido(String? value) {
    if (value == null || value.trim().isEmpty) {
      return AppLocalizations.of(context)!.campoObligatorio;
    }
    return null;
  }

  void _guardar() {
    if (!_formKey.currentState!.validate()) return;
    Navigator.of(context).pop(
      _WordFormResult(
        word: _wordController.text.trim(),
        hint: _hintController.text.trim(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return AlertDialog(
      title: Text(_isEditing ? l10n.editarPalabra : l10n.nuevaPalabra),
      content: Form(
        key: _formKey,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextFormField(
              controller: _wordController,
              autofocus: true,
              textCapitalization: TextCapitalization.none,
              textInputAction: TextInputAction.next,
              decoration: InputDecoration(
                labelText: l10n.campoPalabra,
                hintText: l10n.campoPalabraHint,
              ),
              validator: _validarRequerido,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _hintController,
              textInputAction: TextInputAction.done,
              onFieldSubmitted: (_) => _guardar(),
              decoration: InputDecoration(
                labelText: l10n.campoPista,
                hintText: l10n.campoPistaHint,
              ),
              validator: _validarRequerido,
            ),
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text(l10n.cancelar),
        ),
        NeonGlowWrapper(
          borderRadius: const BorderRadius.all(Radius.circular(12)),
          child: FilledButton(
            onPressed: _guardar,
            child: Text(_isEditing ? l10n.guardar : l10n.agregar),
          ),
        ),
      ],
    );
  }
}
