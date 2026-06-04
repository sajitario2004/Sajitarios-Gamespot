import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/games/impostor/data/impostor_word.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/words_management_screen.dart';

import '../../../support/localized_app.dart';

/// Repositorio de palabras falso, en memoria, para tests de widget.
///
/// Implementa el contrato público de [WordRepository] sin tocar SQLite: guarda
/// las palabras en una lista en memoria y reproduce las reglas relevantes para
/// la UI (unicidad de `word`, solo lectura de las seed, búsqueda por texto).
class FakeWordRepository implements WordRepository {
  FakeWordRepository(List<ImpostorWord> initial)
    : _words = List<ImpostorWord>.from(initial) {
    final ids = _words.map((w) => w.id ?? 0);
    _nextId = (ids.isEmpty ? 0 : ids.reduce((a, b) => a > b ? a : b)) + 1;
  }

  final List<ImpostorWord> _words;
  int _nextId = 1;

  List<ImpostorWord> get _ordered {
    final copy = List<ImpostorWord>.from(_words);
    copy.sort((a, b) {
      final byWord = a.word.toLowerCase().compareTo(b.word.toLowerCase());
      if (byWord != 0) return byWord;
      return (a.id ?? 0).compareTo(b.id ?? 0);
    });
    return List<ImpostorWord>.unmodifiable(copy);
  }

  @override
  Future<List<ImpostorWord>> getAll() async => _ordered;

  @override
  Future<List<ImpostorWord>> search(String query) async {
    final term = query.trim().toLowerCase();
    if (term.isEmpty) return _ordered;
    return List<ImpostorWord>.unmodifiable(
      _ordered.where((w) => w.word.toLowerCase().contains(term)),
    );
  }

  @override
  Future<ImpostorWord?> getById(int id) async {
    for (final w in _words) {
      if (w.id == id) return w;
    }
    return null;
  }

  @override
  Future<ImpostorWord> insert({
    required String word,
    required String hint,
  }) async {
    final cleanWord = word.trim();
    final cleanHint = hint.trim();
    if (cleanWord.isEmpty) {
      throw ArgumentError.value(cleanWord, 'word', 'No puede estar vacío');
    }
    if (cleanHint.isEmpty) {
      throw ArgumentError.value(cleanHint, 'hint', 'No puede estar vacío');
    }
    final existe = _words.any(
      (w) => w.word.toLowerCase() == cleanWord.toLowerCase(),
    );
    if (existe) {
      throw DuplicateWordException(cleanWord);
    }
    final nueva = ImpostorWord(
      id: _nextId++,
      word: cleanWord,
      hint: cleanHint,
      isSeed: false,
      createdAt: DateTime.now(),
    );
    _words.add(nueva);
    return nueva;
  }

  @override
  Future<ImpostorWord> update({
    required int id,
    required String word,
    required String hint,
  }) async {
    final cleanWord = word.trim();
    final cleanHint = hint.trim();
    if (cleanWord.isEmpty) {
      throw ArgumentError.value(cleanWord, 'word', 'No puede estar vacío');
    }
    if (cleanHint.isEmpty) {
      throw ArgumentError.value(cleanHint, 'hint', 'No puede estar vacío');
    }
    final index = _words.indexWhere((w) => w.id == id);
    if (index == -1) {
      throw WordNotFoundException(id);
    }
    if (_words[index].isSeed) {
      throw ReadOnlySeedWordException(id);
    }
    final choca = _words.any(
      (w) => w.id != id && w.word.toLowerCase() == cleanWord.toLowerCase(),
    );
    if (choca) {
      throw DuplicateWordException(cleanWord);
    }
    final actualizada = _words[index].copyWith(
      word: cleanWord,
      hint: cleanHint,
    );
    _words[index] = actualizada;
    return actualizada;
  }

  @override
  Future<void> delete(int id) async {
    final index = _words.indexWhere((w) => w.id == id);
    if (index == -1) {
      throw WordNotFoundException(id);
    }
    if (_words[index].isSeed) {
      throw ReadOnlySeedWordException(id);
    }
    _words.removeAt(index);
  }
}

/// Datos de ejemplo: dos palabras seed (solo lectura) y una de usuario.
List<ImpostorWord> _datosEjemplo() {
  final ahora = DateTime.fromMillisecondsSinceEpoch(0);
  return <ImpostorWord>[
    ImpostorWord(
      id: 1,
      word: 'pirata',
      hint: 'barco',
      isSeed: true,
      createdAt: ahora,
    ),
    ImpostorWord(
      id: 2,
      word: 'castillo',
      hint: 'rey',
      isSeed: true,
      createdAt: ahora,
    ),
    ImpostorWord(
      id: 3,
      word: 'manzana',
      hint: 'fruta',
      isSeed: false,
      createdAt: ahora,
    ),
  ];
}

/// Envuelve la [WordsManagementScreen] con un [MaterialApp] (Theme +
/// ScaffoldMessenger) y un [ProviderScope] que sobreescribe el
/// [wordRepositoryProvider] por un fake en memoria.
Widget _harness(FakeWordRepository repo) {
  return ProviderScope(
    overrides: [wordRepositoryProvider.overrideWithValue(repo)],
    child: localizedApp(const WordsManagementScreen()),
  );
}

void main() {
  group('WordsManagementScreen', () {
    testWidgets('lista las palabras seed y las propias del usuario', (
      tester,
    ) async {
      final repo = FakeWordRepository(_datosEjemplo());
      await tester.pumpWidget(_harness(repo));
      await tester.pumpAndSettle();

      expect(find.text('pirata'), findsOneWidget);
      expect(find.text('castillo'), findsOneWidget);
      expect(find.text('manzana'), findsOneWidget);

      // Las seed muestran el chip "Predefinida"; la de usuario no.
      expect(find.text('Predefinida'), findsNWidgets(2));

      // Las pistas se muestran con el prefijo "Pista: ".
      expect(find.text('Pista: barco'), findsOneWidget);
      expect(find.text('Pista: fruta'), findsOneWidget);
    });

    testWidgets('la búsqueda filtra la lista por texto', (tester) async {
      final repo = FakeWordRepository(_datosEjemplo());
      await tester.pumpWidget(_harness(repo));
      await tester.pumpAndSettle();

      await tester.enterText(find.byType(TextField), 'pirat');
      await tester.pumpAndSettle();

      expect(find.text('pirata'), findsOneWidget);
      expect(find.text('castillo'), findsNothing);
      expect(find.text('manzana'), findsNothing);
    });

    testWidgets(
      'la búsqueda sin coincidencias muestra el mensaje vacío en español',
      (tester) async {
        final repo = FakeWordRepository(_datosEjemplo());
        await tester.pumpWidget(_harness(repo));
        await tester.pumpAndSettle();

        await tester.enterText(find.byType(TextField), 'zzz-no-existe');
        await tester.pumpAndSettle();

        expect(
          find.text('No hay palabras que coincidan con tu búsqueda.'),
          findsOneWidget,
        );
      },
    );

    testWidgets('agregar una palabra duplicada muestra el error en español', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(1200, 2400);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);
      addTearDown(tester.view.resetDevicePixelRatio);

      final repo = FakeWordRepository(_datosEjemplo());
      await tester.pumpWidget(_harness(repo));
      await tester.pumpAndSettle();

      // Abre el formulario de alta.
      await tester.tap(find.widgetWithText(FloatingActionButton, 'Agregar'));
      await tester.pumpAndSettle();

      // Rellena con una palabra que ya existe ("manzana").
      await tester.enterText(
        find.widgetWithText(TextFormField, 'Palabra'),
        'manzana',
      );
      await tester.enterText(
        find.widgetWithText(TextFormField, 'Pista'),
        'otra pista',
      );
      await tester.pump();

      // Confirma el alta dentro del diálogo.
      await tester.tap(find.widgetWithText(FilledButton, 'Agregar'));
      await tester.pumpAndSettle();

      expect(find.text('Ya existe esa palabra.'), findsOneWidget);
    });

    testWidgets(
      'las palabras seed tienen editar/borrar deshabilitados; las de usuario no',
      (tester) async {
        final repo = FakeWordRepository(_datosEjemplo());
        await tester.pumpWidget(_harness(repo));
        await tester.pumpAndSettle();

        // Localiza las tarjetas por palabra para inspeccionar sus IconButton.
        final editButtons = find.widgetWithIcon(
          IconButton,
          Icons.edit_outlined,
        );
        final deleteButtons = find.widgetWithIcon(
          IconButton,
          Icons.delete_outline,
        );

        // Hay un par de botones por cada una de las 3 palabras.
        expect(editButtons, findsNWidgets(3));
        expect(deleteButtons, findsNWidgets(3));

        // 2 seed (deshabilitados) + 1 usuario (habilitado).
        final editPressed = editButtons
            .evaluate()
            .map((e) => (e.widget as IconButton).onPressed)
            .toList();
        final deletePressed = deleteButtons
            .evaluate()
            .map((e) => (e.widget as IconButton).onPressed)
            .toList();

        expect(editPressed.where((p) => p == null).length, 2);
        expect(editPressed.where((p) => p != null).length, 1);
        expect(deletePressed.where((p) => p == null).length, 2);
        expect(deletePressed.where((p) => p != null).length, 1);
      },
    );
  });
}
