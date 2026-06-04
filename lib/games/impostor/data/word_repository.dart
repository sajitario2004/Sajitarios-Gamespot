import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:sqflite/sqflite.dart';

import 'package:sajitarios_gamespot/core/db/app_database.dart';
import 'package:sajitarios_gamespot/games/impostor/data/impostor_word.dart';

/// Nombre de la tabla de palabras del Impostor.
const String kImpostorWordsTable = 'impostor_words';

/// Error lanzado al intentar insertar/actualizar una palabra que ya existe.
///
/// La columna `word` es única en la tabla con colación `NOCASE`: dos palabras no
/// pueden tener el mismo texto ignorando mayúsculas/minúsculas ("Pirata" y
/// "pirata" colisionan).
class DuplicateWordException implements Exception {
  const DuplicateWordException(this.word);

  /// El texto de la palabra duplicada.
  final String word;

  @override
  String toString() => 'Ya existe una palabra con el texto "$word".';
}

/// Error lanzado al intentar editar o borrar una palabra del seed.
///
/// Las palabras seed (`is_seed = 1`) son de solo lectura: vienen con la app y no
/// se pueden modificar ni eliminar desde la interfaz.
class ReadOnlySeedWordException implements Exception {
  const ReadOnlySeedWordException(this.id);

  /// El id de la palabra seed que se intentó modificar/borrar.
  final int id;

  @override
  String toString() =>
      'La palabra con id $id es del seed y es de solo lectura.';
}

/// Error lanzado cuando una operación referencia una palabra inexistente.
class WordNotFoundException implements Exception {
  const WordNotFoundException(this.id);

  /// El id buscado.
  final int id;

  @override
  String toString() => 'No existe ninguna palabra con id $id.';
}

/// Acceso a datos (CRUD) de las palabras del Impostor sobre `impostor_words`.
///
/// Reglas de negocio aplicadas aquí:
/// - `word` y `hint` son obligatorios (no vacíos tras recortar espacios).
/// - `word` es único sin distinguir mayúsculas (colación `NOCASE`):
///   insertar/actualizar a un texto ya existente lanza [DuplicateWordException].
/// - Las palabras seed (`is_seed = 1`) son de solo lectura: editarlas o
///   borrarlas lanza [ReadOnlySeedWordException].
class WordRepository {
  WordRepository({required AppDatabase database}) : _database = database;

  final AppDatabase _database;

  Future<Database> get _db => _database.database;

  /// Devuelve todas las palabras con un orden estable.
  ///
  /// El orden es por `word` (ascendente, sin distinguir mayúsculas) y, en caso
  /// de empate improbable, por `id`. Así la lista es reproducible entre
  /// llamadas.
  Future<List<ImpostorWord>> getAll() async {
    final db = await _db;
    final rows = await db.query(
      kImpostorWordsTable,
      orderBy: 'word COLLATE NOCASE ASC, id ASC',
    );
    return rows.map(ImpostorWord.fromMap).toList(growable: false);
  }

  /// Busca palabras cuyo texto contenga [query] (búsqueda `LIKE`).
  ///
  /// La búsqueda no distingue mayúsculas/minúsculas. Si [query] queda vacío tras
  /// recortar espacios, devuelve todas las palabras (igual que [getAll]).
  Future<List<ImpostorWord>> search(String query) async {
    final term = query.trim();
    if (term.isEmpty) {
      return getAll();
    }
    final db = await _db;
    final pattern = '%${_escapeLike(term)}%';
    final rows = await db.query(
      kImpostorWordsTable,
      where: "word LIKE ? ESCAPE '\\'",
      whereArgs: <Object?>[pattern],
      orderBy: 'word COLLATE NOCASE ASC, id ASC',
    );
    return rows.map(ImpostorWord.fromMap).toList(growable: false);
  }

  /// Devuelve la palabra con [id] o `null` si no existe.
  Future<ImpostorWord?> getById(int id) async {
    final db = await _db;
    final rows = await db.query(
      kImpostorWordsTable,
      where: 'id = ?',
      whereArgs: <Object?>[id],
      limit: 1,
    );
    if (rows.isEmpty) {
      return null;
    }
    return ImpostorWord.fromMap(rows.first);
  }

  /// Inserta una nueva palabra de usuario (`is_seed = 0`).
  ///
  /// [word] y [hint] son obligatorios (no vacíos). Lanza [ArgumentError] si
  /// alguno está vacío y [DuplicateWordException] si ya existe una palabra con
  /// el mismo texto. Devuelve la palabra insertada con su `id` asignado.
  Future<ImpostorWord> insert({
    required String word,
    required String hint,
  }) async {
    final cleanWord = word.trim();
    final cleanHint = hint.trim();
    _requireNotEmpty(cleanWord, 'word');
    _requireNotEmpty(cleanHint, 'hint');

    final db = await _db;
    final createdAt = DateTime.now();
    try {
      final id = await db.insert(kImpostorWordsTable, <String, Object?>{
        'word': cleanWord,
        'hint': cleanHint,
        'is_seed': 0,
        'created_at': createdAt.millisecondsSinceEpoch,
      }, conflictAlgorithm: ConflictAlgorithm.abort);
      return ImpostorWord(
        id: id,
        word: cleanWord,
        hint: cleanHint,
        isSeed: false,
        createdAt: createdAt,
      );
    } on DatabaseException catch (error) {
      if (error.isUniqueConstraintError()) {
        throw DuplicateWordException(cleanWord);
      }
      rethrow;
    }
  }

  /// Actualiza el texto y/o la pista de una palabra de usuario.
  ///
  /// Solo afecta a filas con `is_seed = 0`. Lanza:
  /// - [WordNotFoundException] si [id] no existe.
  /// - [ReadOnlySeedWordException] si la palabra es del seed.
  /// - [ArgumentError] si `word`/`hint` quedan vacíos.
  /// - [DuplicateWordException] si el nuevo texto choca con otra palabra.
  ///
  /// Devuelve la palabra ya actualizada.
  Future<ImpostorWord> update({
    required int id,
    required String word,
    required String hint,
  }) async {
    final cleanWord = word.trim();
    final cleanHint = hint.trim();
    _requireNotEmpty(cleanWord, 'word');
    _requireNotEmpty(cleanHint, 'hint');

    final existing = await getById(id);
    if (existing == null) {
      throw WordNotFoundException(id);
    }
    if (existing.isSeed) {
      throw ReadOnlySeedWordException(id);
    }

    final db = await _db;
    try {
      await db.update(
        kImpostorWordsTable,
        <String, Object?>{'word': cleanWord, 'hint': cleanHint},
        // Doble guarda: aunque ya validamos isSeed, restringimos el UPDATE a
        // filas de usuario para no tocar nunca el seed.
        where: 'id = ? AND is_seed = 0',
        whereArgs: <Object?>[id],
        conflictAlgorithm: ConflictAlgorithm.abort,
      );
    } on DatabaseException catch (error) {
      if (error.isUniqueConstraintError()) {
        throw DuplicateWordException(cleanWord);
      }
      rethrow;
    }

    return existing.copyWith(word: cleanWord, hint: cleanHint);
  }

  /// Borra una palabra de usuario.
  ///
  /// Solo afecta a filas con `is_seed = 0`. Lanza [WordNotFoundException] si
  /// [id] no existe y [ReadOnlySeedWordException] si la palabra es del seed.
  Future<void> delete(int id) async {
    final existing = await getById(id);
    if (existing == null) {
      throw WordNotFoundException(id);
    }
    if (existing.isSeed) {
      throw ReadOnlySeedWordException(id);
    }

    final db = await _db;
    // Doble guarda: el WHERE excluye filas seed por si acaso.
    await db.delete(
      kImpostorWordsTable,
      where: 'id = ? AND is_seed = 0',
      whereArgs: <Object?>[id],
    );
  }

  void _requireNotEmpty(String value, String name) {
    if (value.isEmpty) {
      throw ArgumentError.value(value, name, 'No puede estar vacío');
    }
  }

  /// Escapa los comodines de `LIKE` (`%`, `_`) y el propio carácter de escape
  /// (`\`) para tratar [term] como texto literal en la búsqueda.
  String _escapeLike(String term) {
    return term
        .replaceAll('\\', '\\\\')
        .replaceAll('%', '\\%')
        .replaceAll('_', '\\_');
  }
}

/// Provider de Riverpod que expone el [WordRepository] del Impostor.
///
/// En tests se puede sobreescribir inyectando un [AppDatabase] en memoria
/// (vía [appDatabaseProvider]).
final wordRepositoryProvider = Provider<WordRepository>((ref) {
  return WordRepository(database: ref.watch(appDatabaseProvider));
});
