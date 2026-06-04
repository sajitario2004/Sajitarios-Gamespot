import 'package:sajitarios_gamespot/games/impostor/data/impostor_word.dart';
import 'package:sajitarios_gamespot/games/impostor/data/word_repository.dart';

/// [WordRepository] falso e in-memory para los tests de integración.
///
/// El flujo del Impostor solo consume `getAll()` (vía
/// `AssignRolesCoordinator`). Aquí devolvemos un conjunto fijo de palabras sin
/// tocar SQLite ni `path_provider` (que requieren plataforma). El resto de
/// operaciones CRUD no se ejercitan en el e2e y lanzan [UnimplementedError]
/// para dejar claro que no forman parte del escenario.
class FakeWordRepository implements WordRepository {
  FakeWordRepository(this._words);

  /// Repositorio con una única palabra (`playa`/`verano`).
  ///
  /// Útil cuando el test inyecta una semilla fija: al haber una sola palabra, el
  /// `pick` consume una sola posición y la rama probabilística queda totalmente
  /// determinada por la semilla.
  factory FakeWordRepository.single() => FakeWordRepository(<ImpostorWord>[
    ImpostorWord(
      id: 1,
      word: 'playa',
      hint: 'verano',
      isSeed: true,
      createdAt: DateTime.fromMillisecondsSinceEpoch(0),
    ),
  ]);

  final List<ImpostorWord> _words;

  @override
  Future<List<ImpostorWord>> getAll() async => List.unmodifiable(_words);

  @override
  dynamic noSuchMethod(Invocation invocation) => throw UnimplementedError(
    'FakeWordRepository solo implementa getAll() para el e2e del Impostor.',
  );
}
