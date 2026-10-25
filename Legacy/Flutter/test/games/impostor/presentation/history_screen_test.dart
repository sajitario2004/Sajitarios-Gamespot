import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/impostor/data/game_history_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/data/game_record.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/history_screen.dart';

import '../../../support/localized_app.dart';

/// Repositorio de historial falso, en memoria, para tests de widget.
///
/// Subclasea [GameHistoryRepository] y sobreescribe todo el contrato público
/// que usa la pantalla, así que la [AppDatabase] del constructor nunca se toca
/// (no hay SQLite ni path_provider en el test).
class _FakeGameHistoryRepository extends GameHistoryRepository {
  _FakeGameHistoryRepository(this._records)
    : super(database: AppDatabase(descriptors: const []));

  List<GameRecord> _records;

  @override
  Future<List<GameRecord>> getAll() async => List<GameRecord>.from(_records);

  @override
  Future<int> count() async => _records.length;

  @override
  Future<WordFrequency?> mostFrequentWord() async {
    if (_records.isEmpty) return null;
    final counts = <String, int>{};
    for (final r in _records) {
      final key = r.word.toLowerCase();
      counts[key] = (counts[key] ?? 0) + 1;
    }
    final top = counts.entries.reduce((a, b) => a.value >= b.value ? a : b);
    final word = _records
        .firstWhere((r) => r.word.toLowerCase() == top.key)
        .word;
    return WordFrequency(word: word, count: top.value);
  }

  @override
  Future<Map<String, int>> impostorCountsByPlayer() async {
    final counts = <String, int>{};
    for (final r in _records) {
      for (final p in r.players) {
        if (p.wasImpostor) {
          counts[p.name] = (counts[p.name] ?? 0) + 1;
        }
      }
    }
    return counts;
  }

  @override
  Future<int> deleteAll() async {
    final n = _records.length;
    _records = <GameRecord>[];
    return n;
  }
}

GameRecord _record({
  required DateTime createdAt,
  required String word,
  String? hint,
  required bool hintEnabled,
  required List<GameRecordPlayer> players,
}) {
  final nImpostors = players.where((p) => p.wasImpostor).length;
  return GameRecord(
    id: createdAt.millisecondsSinceEpoch,
    createdAt: createdAt,
    word: word,
    hint: hint,
    nPlayers: players.length,
    nImpostors: nImpostors,
    hintEnabled: hintEnabled,
    players: players,
  );
}

Widget _app(GameHistoryRepository repo) {
  return ProviderScope(
    overrides: [gameHistoryRepositoryProvider.overrideWithValue(repo)],
    child: localizedApp(const HistoryScreen()),
  );
}

void main() {
  final partidasDeEjemplo = <GameRecord>[
    _record(
      createdAt: DateTime(2026, 1, 2, 18, 30),
      word: 'pirata',
      hint: 'barco',
      hintEnabled: true,
      players: const [
        GameRecordPlayer(name: 'Nacho', wasImpostor: true),
        GameRecordPlayer(name: 'Iker', wasImpostor: false),
        GameRecordPlayer(name: 'Lucia', wasImpostor: false),
      ],
    ),
    _record(
      createdAt: DateTime(2026, 1, 1, 10, 0),
      word: 'pirata',
      hint: 'barco',
      hintEnabled: false,
      players: const [
        GameRecordPlayer(name: 'Nacho', wasImpostor: true),
        GameRecordPlayer(name: 'Iker', wasImpostor: true),
        GameRecordPlayer(name: 'Lucia', wasImpostor: false),
      ],
    ),
  ];

  group('HistoryScreen', () {
    testWidgets('lista las partidas guardadas', (tester) async {
      await tester.pumpWidget(
        _app(_FakeGameHistoryRepository(partidasDeEjemplo)),
      );
      await tester.pumpAndSettle();

      // Aparece la sección de partidas y cada palabra.
      expect(find.text('Partidas'), findsOneWidget);
      expect(find.text('pirata'), findsNWidgets(2));
      // El subtítulo refleja jugadores e impostores.
      expect(find.textContaining('3 jugadores'), findsNWidgets(2));
      expect(find.textContaining('1 impostor'), findsOneWidget);
      expect(find.textContaining('2 impostores'), findsOneWidget);
    });

    testWidgets('muestra el resumen de estadísticas', (tester) async {
      await tester.pumpWidget(
        _app(_FakeGameHistoryRepository(partidasDeEjemplo)),
      );
      await tester.pumpAndSettle();

      expect(find.text('Estadísticas'), findsOneWidget);
      expect(find.text('Partidas jugadas'), findsOneWidget);
      // Total de partidas.
      expect(find.text('2'), findsWidgets);
      // Palabra más repetida.
      expect(find.text('pirata (2)'), findsOneWidget);
      // Ranking de impostores: Nacho 2 veces, Iker 1.
      expect(find.text('Veces que cada jugador fue impostor'), findsOneWidget);
      expect(find.text('Nacho'), findsOneWidget);
      expect(find.text('Iker'), findsOneWidget);
    });

    testWidgets('expande el detalle de roles de una partida', (tester) async {
      await tester.pumpWidget(
        _app(_FakeGameHistoryRepository(partidasDeEjemplo)),
      );
      await tester.pumpAndSettle();

      // Antes de expandir no se ve el detalle de roles.
      expect(find.text('IMPOSTOR'), findsNothing);

      await tester.tap(find.text('pirata').first);
      await tester.pumpAndSettle();

      // Tras expandir aparece el rol de los jugadores.
      expect(find.text('IMPOSTOR'), findsWidgets);
      expect(find.text('Sabía la palabra'), findsWidgets);
    });

    testWidgets('muestra el estado vacío sin partidas', (tester) async {
      await tester.pumpWidget(
        _app(_FakeGameHistoryRepository(const <GameRecord>[])),
      );
      await tester.pumpAndSettle();

      expect(find.text('Todavía no hay partidas guardadas.'), findsOneWidget);
      expect(find.text('Partidas'), findsNothing);
      expect(find.text('Estadísticas'), findsNothing);
    });

    testWidgets('borra el historial tras confirmar', (tester) async {
      await tester.pumpWidget(
        _app(_FakeGameHistoryRepository(List.of(partidasDeEjemplo))),
      );
      await tester.pumpAndSettle();

      expect(find.text('pirata'), findsNWidgets(2));

      // Abre el diálogo de confirmación.
      await tester.tap(find.byTooltip('Borrar historial'));
      await tester.pumpAndSettle();
      expect(find.text('Borrar historial'), findsWidgets);

      await tester.tap(find.text('Borrar todo'));
      await tester.pumpAndSettle();

      // Tras borrar, queda el estado vacío.
      expect(find.text('Todavía no hay partidas guardadas.'), findsOneWidget);
    });
  });
}
