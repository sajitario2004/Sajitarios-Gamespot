/// Fake in-memory [TabuWordRepository] for unit and widget tests.
///
/// Returns a pre-loaded list of [TabuWord] objects without touching any real
/// database. Mirrors the fake repo pattern used by the Trivia and Impostor
/// tests.
library;

import 'package:sajitarios_gamespot/games/tabu/data/tabu_word_repository.dart';
import 'package:sajitarios_gamespot/games/tabu/domain/tabu_word.dart';

/// In-memory [TabuWordRepository] that serves words from a pre-loaded list.
class FakeTabuWordRepository implements TabuWordRepository {
  FakeTabuWordRepository(this._words);

  final List<TabuWord> _words;

  @override
  Future<List<TabuWord>> getAll() async => List<TabuWord>.unmodifiable(_words);

  @override
  Future<int> count() async => _words.length;

  @override
  Future<TabuWord> insert(TabuWord word) async => word;

  @override
  Future<List<TabuWord>> bulkInsert(List<TabuWord> words) async => words;
}

// ─── Factory helpers ─────────────────────────────────────────────────────────

/// Builds a [TabuWord] with sensible defaults for tests.
TabuWord fakeTabuWord({
  int id = 1,
  String palabra = 'Ordenador',
  List<String>? prohibidas,
}) {
  return TabuWord.create(
    id: id,
    palabra: palabra,
    prohibidas: prohibidas ?? ['computadora', 'pantalla', 'teclado', 'ratón'],
  );
}

/// Builds a [FakeTabuWordRepository] with [count] distinct fake words.
FakeTabuWordRepository buildFakeTabuRepo({int count = 10}) {
  final words = <TabuWord>[];
  for (var i = 1; i <= count; i++) {
    words.add(
      TabuWord.create(
        id: i,
        palabra: 'Palabra$i',
        prohibidas: [
          'prohibida${i}a',
          'prohibida${i}b',
          'prohibida${i}c',
          'prohibida${i}d',
        ],
      ),
    );
  }
  return FakeTabuWordRepository(words);
}

/// Builds an empty [FakeTabuWordRepository] (triggers sinPalabras error).
FakeTabuWordRepository buildEmptyTabuRepo() => FakeTabuWordRepository([]);
