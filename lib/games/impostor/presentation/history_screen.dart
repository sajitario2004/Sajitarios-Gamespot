import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/impostor/data/game_history_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/data/game_record.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Ancho máximo del contenido en pantallas grandes (tablet/desktop) para que
/// las listas y textos no se estiren de forma incómoda.
const double _kMaxContentWidth = 640.0;

/// Datos agregados que la pantalla muestra de una sola carga.
///
/// Reúne la lista de partidas y los agregados de estadísticas (total, palabra
/// más repetida y ranking de impostores por jugador) para resolverlos en un
/// único [FutureBuilder] y mantener la UI coherente.
class _HistoryData {
  const _HistoryData({
    required this.records,
    required this.total,
    required this.mostFrequentWord,
    required this.impostorCounts,
  });

  final List<GameRecord> records;
  final int total;
  final WordFrequency? mostFrequentWord;

  /// Nombre de jugador -> veces que fue impostor (solo nombres con >= 1).
  final Map<String, int> impostorCounts;
}

/// Historial de partidas y estadísticas del Impostor (versión 0.43).
///
/// Lee `gameHistoryRepositoryProvider` y muestra:
/// - Un resumen de estadísticas: total de partidas, palabra más repetida y
///   ranking de cuántas veces cada jugador fue impostor.
/// - La lista de partidas (fecha, palabra, nº de jugadores/impostores, pista
///   on/off), expandible para ver el detalle de roles.
/// - Un botón para borrar todo el historial (con confirmación).
///
/// Cubre los estados de carga, vacío y error en español.
class HistoryScreen extends ConsumerStatefulWidget {
  const HistoryScreen({super.key});

  @override
  ConsumerState<HistoryScreen> createState() => _HistoryScreenState();
}

class _HistoryScreenState extends ConsumerState<HistoryScreen> {
  /// Futuro de la carga actual; se reemplaza al refrescar o tras borrar.
  late Future<_HistoryData> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<_HistoryData> _load() async {
    final repo = ref.read(gameHistoryRepositoryProvider);
    // Una sola lectura de la tabla; los agregados (total, palabra más repetida
    // y recuento de impostores por jugador) se derivan en Dart para no
    // reconsultar la base de datos.
    final records = await repo.getAll();
    return _HistoryData(
      records: records,
      total: records.length,
      mostFrequentWord: _computeMostFrequentWord(records),
      impostorCounts: _computeImpostorCounts(records),
    );
  }

  /// Palabra más repetida del historial, o `null` si no hay partidas.
  ///
  /// El recuento es case-insensitive; en caso de empate gana la de mayor
  /// recuento y, a igualdad, la alfabéticamente menor. Como [records] viene
  /// ordenado de la partida más reciente a la más antigua, se conserva el texto
  /// tal como se guardó en la fila más reciente del grupo.
  static WordFrequency? _computeMostFrequentWord(List<GameRecord> records) {
    if (records.isEmpty) return null;
    final counts = <String, int>{};
    final display = <String, String>{};
    for (final record in records) {
      final key = record.word.toLowerCase();
      counts[key] = (counts[key] ?? 0) + 1;
      // El primer registro visto (más reciente) fija el texto mostrado.
      display.putIfAbsent(key, () => record.word);
    }
    final best = counts.entries.toList()
      ..sort((a, b) {
        final byCount = b.value.compareTo(a.value);
        if (byCount != 0) return byCount;
        return a.key.compareTo(b.key);
      });
    final top = best.first;
    return WordFrequency(word: display[top.key]!, count: top.value);
  }

  /// Cuántas veces cada nombre de jugador fue impostor a lo largo del historial.
  ///
  /// Los nombres que nunca fueron impostor no aparecen en el mapa.
  static Map<String, int> _computeImpostorCounts(List<GameRecord> records) {
    final counts = <String, int>{};
    for (final record in records) {
      for (final player in record.players) {
        if (player.wasImpostor) {
          counts[player.name] = (counts[player.name] ?? 0) + 1;
        }
      }
    }
    return counts;
  }

  void _refresh() {
    setState(() {
      _future = _load();
    });
  }

  Future<void> _confirmarBorrado() async {
    final l10n = AppLocalizations.of(context)!;
    final confirmar = await showDialog<bool>(
      context: context,
      builder: (dialogContext) {
        final dl10n = AppLocalizations.of(dialogContext)!;
        return AlertDialog(
          title: Text(dl10n.borrarHistorialTitulo),
          content: Text(dl10n.borrarHistorialMensaje),
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
                child: Text(dl10n.borrarTodo),
              ),
            ),
          ],
        );
      },
    );
    if (confirmar != true || !mounted) return;
    final messenger = ScaffoldMessenger.of(context);
    try {
      await ref.read(gameHistoryRepositoryProvider).deleteAll();
      if (!mounted) return;
      messenger.hideCurrentSnackBar();
      messenger.showSnackBar(SnackBar(content: Text(l10n.historialBorrado)));
      _refresh();
    } catch (_) {
      if (!mounted) return;
      messenger.hideCurrentSnackBar();
      messenger.showSnackBar(
        SnackBar(content: Text(l10n.noSePudoBorrarHistorial)),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return Scaffold(
      appBar: AppBar(
        title: Text(l10n.historial),
        actions: [
          IconButton(
            tooltip: l10n.borrarHistorialTitulo,
            icon: const Icon(Icons.delete_outline),
            onPressed: _confirmarBorrado,
          ),
        ],
      ),
      body: NeonBackground(
        child: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: _kMaxContentWidth),
              child: FutureBuilder<_HistoryData>(
                future: _future,
                builder: (context, snapshot) {
                  if (snapshot.connectionState == ConnectionState.waiting) {
                    return const _Cargando();
                  }
                  if (snapshot.hasError) {
                    return _Error(onReintentar: _refresh);
                  }
                  final data = snapshot.data;
                  if (data == null || data.records.isEmpty) {
                    return const _Vacio();
                  }
                  return _Contenido(data: data);
                },
              ),
            ),
          ),
        ),
      ),
    );
  }
}

/// Estado de carga.
class _Cargando extends StatelessWidget {
  const _Cargando();

  @override
  Widget build(BuildContext context) {
    return const Center(child: CircularProgressIndicator());
  }
}

/// Estado de error con opción de reintentar.
class _Error extends StatelessWidget {
  const _Error({required this.onReintentar});

  final VoidCallback onReintentar;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final l10n = AppLocalizations.of(context)!;
    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.error_outline, size: 64, color: AppTheme.neonError),
          const SizedBox(height: 16),
          NeonText(
            l10n.noSePudoCargarHistorial,
            glowColor: AppTheme.neonError,
            intensity: 0.6,
            style: textTheme.titleLarge,
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 24),
          NeonGlowWrapper(
            borderRadius: const BorderRadius.all(Radius.circular(12)),
            child: FilledButton.icon(
              onPressed: onReintentar,
              icon: const Icon(Icons.refresh),
              label: Text(l10n.reintentar),
            ),
          ),
        ],
      ),
    );
  }
}

/// Estado vacío (sin partidas guardadas).
class _Vacio extends StatelessWidget {
  const _Vacio();

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;
    final colors = Theme.of(context).colorScheme;
    final l10n = AppLocalizations.of(context)!;
    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.history_toggle_off, size: 64, color: AppTheme.neonCyan),
          const SizedBox(height: 16),
          NeonText(
            l10n.historialVacioTitulo,
            intensity: 0.6,
            style: textTheme.titleLarge,
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 8),
          Text(
            l10n.historialVacioMensaje,
            style: textTheme.bodyMedium?.copyWith(
              color: colors.onSurfaceVariant,
            ),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }
}

/// Contenido principal: resumen de estadísticas + lista de partidas.
class _Contenido extends StatelessWidget {
  const _Contenido({required this.data});

  final _HistoryData data;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return ListView(
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 24),
      children: [
        _ResumenEstadisticas(data: data),
        const SizedBox(height: 24),
        NeonText(
          AppLocalizations.of(context)!.partidas,
          intensity: 0.6,
          style: theme.textTheme.titleLarge,
        ),
        const SizedBox(height: 8),
        for (final record in data.records) _PartidaTile(record: record),
      ],
    );
  }
}

/// Resumen de estadísticas: total, palabra más repetida y ranking de impostores.
class _ResumenEstadisticas extends StatelessWidget {
  const _ResumenEstadisticas({required this.data});

  final _HistoryData data;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;
    final l10n = AppLocalizations.of(context)!;

    // Ranking de impostores: nombre desc por veces, desempate alfabético.
    final ranking = data.impostorCounts.entries.toList()
      ..sort((a, b) {
        final byCount = b.value.compareTo(a.value);
        if (byCount != 0) return byCount;
        return a.key.toLowerCase().compareTo(b.key.toLowerCase());
      });

    final masRepetida = data.mostFrequentWord;

    return NeonPanel(
      borderColor: AppTheme.neonCyan,
      backgroundColor: AppTheme.surfaceHigh,
      borderRadius: const BorderRadius.all(Radius.circular(18)),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.insights_outlined, color: AppTheme.neonCyan),
              const SizedBox(width: 8),
              NeonText(
                l10n.estadisticas,
                intensity: 0.6,
                style: theme.textTheme.titleMedium,
              ),
            ],
          ),
          const SizedBox(height: 12),
          _EstadisticaLinea(
            icon: Icons.sports_esports_outlined,
            etiqueta: l10n.partidasJugadas,
            valor: '${data.total}',
          ),
          const SizedBox(height: 8),
          _EstadisticaLinea(
            icon: Icons.star_outline,
            etiqueta: l10n.palabraMasRepetida,
            valor: masRepetida == null
                ? '—'
                : l10n.palabraMasRepetidaValor(
                    masRepetida.word,
                    masRepetida.count,
                  ),
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              Icon(
                Icons.theater_comedy_outlined,
                size: 18,
                color: colors.error,
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  l10n.vecesQueFueImpostor,
                  style: theme.textTheme.titleSmall,
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          if (ranking.isEmpty)
            Text(
              l10n.nadieFueImpostor,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: colors.onSurfaceVariant,
              ),
            )
          else
            for (final entry in ranking)
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 2),
                child: Row(
                  children: [
                    Expanded(
                      child: Text(entry.key, style: theme.textTheme.bodyMedium),
                    ),
                    Text(
                      '${entry.value}',
                      style: theme.textTheme.bodyMedium?.copyWith(
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ],
                ),
              ),
        ],
      ),
    );
  }
}

/// Una línea de estadística con icono, etiqueta y valor.
class _EstadisticaLinea extends StatelessWidget {
  const _EstadisticaLinea({
    required this.icon,
    required this.etiqueta,
    required this.valor,
  });

  final IconData icon;
  final String etiqueta;
  final String valor;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Row(
      children: [
        Icon(icon, size: 18, color: theme.colorScheme.onSurfaceVariant),
        const SizedBox(width: 8),
        Expanded(child: Text(etiqueta, style: theme.textTheme.bodyMedium)),
        Text(
          valor,
          style: theme.textTheme.bodyMedium?.copyWith(
            fontWeight: FontWeight.w700,
          ),
        ),
      ],
    );
  }
}

/// Fila de una partida del historial, expandible para ver el detalle de roles.
class _PartidaTile extends StatelessWidget {
  const _PartidaTile({required this.record});

  final GameRecord record;

  /// Formatea una fecha (fecha corta + hora) según el [localeName] activo,
  /// usando `intl` en vez de un patrón `dd/MM/yyyy` fijo.
  String _formatearFecha(DateTime fecha, String localeName) {
    final local = fecha.toLocal();
    return DateFormat.yMd(localeName).add_Hm().format(local);
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;
    final l10n = AppLocalizations.of(context)!;
    final localeName = Localizations.localeOf(context).toString();
    final impostoresTexto = l10n.impostoresTexto(record.nImpostors);
    final subtitulo = l10n.partidaSubtitulo(
      _formatearFecha(record.createdAt, localeName),
      record.nPlayers,
      impostoresTexto,
    );

    return NeonPanel(
      borderColor: AppTheme.neonMagenta,
      intensity: 0.45,
      borderWidth: 1.2,
      padding: EdgeInsets.zero,
      borderRadius: const BorderRadius.all(Radius.circular(16)),
      margin: const EdgeInsets.symmetric(vertical: 4),
      child: ExpansionTile(
        leading: Icon(Icons.casino_outlined, color: AppTheme.neonMagenta),
        shape: const Border(),
        collapsedShape: const Border(),
        title: NeonText(
          record.word,
          glowColor: AppTheme.neonMagenta,
          intensity: 0.5,
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.w700,
          ),
        ),
        subtitle: Text(subtitulo, style: theme.textTheme.bodySmall),
        trailing: Icon(
          record.hintEnabled
              ? Icons.lightbulb_outline
              : Icons.lightbulb_outlined,
          color: record.hintEnabled
              ? colors.primary
              : colors.onSurfaceVariant.withValues(alpha: 0.4),
        ),
        childrenPadding: const EdgeInsets.fromLTRB(16, 0, 16, 12),
        children: [
          Align(
            alignment: Alignment.centerLeft,
            child: Text(
              record.hintEnabled
                  ? (record.hint != null
                        ? l10n.pistaActivadaConValor(record.hint!)
                        : l10n.pistaActivada)
                  : l10n.pistaDesactivada,
              style: theme.textTheme.bodySmall?.copyWith(
                color: colors.onSurfaceVariant,
              ),
            ),
          ),
          const SizedBox(height: 8),
          for (final player in record.players)
            _RolLinea(name: player.name, wasImpostor: player.wasImpostor),
        ],
      ),
    );
  }
}

/// Línea de un jugador con su rol dentro del detalle de una partida.
class _RolLinea extends StatelessWidget {
  const _RolLinea({required this.name, required this.wasImpostor});

  final String name;
  final bool wasImpostor;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;
    final l10n = AppLocalizations.of(context)!;
    final icono = wasImpostor
        ? Icons.theater_comedy
        : Icons.check_circle_outline;
    final color = wasImpostor ? colors.error : colors.onSurfaceVariant;
    final etiqueta = wasImpostor ? l10n.impostorMayus : l10n.sabiaLaPalabra;

    return Semantics(
      label: wasImpostor
          ? l10n.jugadorEraImpostor(name)
          : l10n.jugadorSabiaLaPalabra(name),
      child: ExcludeSemantics(
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 4),
          child: Row(
            children: [
              Icon(icono, size: 18, color: color),
              const SizedBox(width: 8),
              Expanded(child: Text(name, style: theme.textTheme.bodyMedium)),
              Text(
                etiqueta,
                style: theme.textTheme.labelMedium?.copyWith(
                  color: color,
                  fontWeight: wasImpostor ? FontWeight.w800 : FontWeight.w500,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
