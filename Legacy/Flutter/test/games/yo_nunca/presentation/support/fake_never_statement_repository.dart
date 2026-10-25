/// Fake in-memory [NeverStatementRepository] para tests unitarios y de widget.
///
/// Devuelve una lista precargada de [NeverStatement] sin tocar ninguna base de
/// datos real. Espeja el patrón del fake de Tabú y Trivia.
library;

import 'package:sajitarios_gamespot/games/yo_nunca/data/never_statement_repository.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/intensidad.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/never_statement.dart';

/// [NeverStatementRepository] en memoria que sirve declaraciones de una lista
/// prefijada.
class FakeNeverStatementRepository implements NeverStatementRepository {
  FakeNeverStatementRepository(this._statements);

  final List<NeverStatement> _statements;

  @override
  Future<List<NeverStatement>> getAll() async =>
      List<NeverStatement>.unmodifiable(_statements);

  @override
  Future<List<NeverStatement>> getByIntensidades(
    Set<Intensidad> intensidades,
  ) async {
    if (intensidades.isEmpty) return const [];
    return _statements
        .where((s) => intensidades.contains(s.intensidad))
        .toList(growable: false);
  }

  @override
  Future<int> count() async => _statements.length;

  @override
  Future<NeverStatement> insert(NeverStatement statement) async => statement;

  @override
  Future<List<NeverStatement>> bulkInsert(
    List<NeverStatement> statements,
  ) async => statements;
}

// ─── Factory helpers ─────────────────────────────────────────────────────────

/// Construye un [NeverStatement] con valores por defecto para tests.
NeverStatement fakeStatement({
  int id = 1,
  String frase = 'Yo nunca he comido pizza fría',
  Intensidad intensidad = Intensidad.suave,
}) {
  return NeverStatement.create(id: id, frase: frase, intensidad: intensidad);
}

/// Builds a [FakeNeverStatementRepository] with [count] distinct suave statements.
FakeNeverStatementRepository buildFakeRepo({int count = 10}) {
  final statements = <NeverStatement>[];
  for (var i = 1; i <= count; i++) {
    statements.add(
      NeverStatement.create(
        id: i,
        frase: 'Yo nunca he hecho la declaración número $i',
        intensidad: Intensidad.suave,
      ),
    );
  }
  return FakeNeverStatementRepository(statements);
}

/// Builds a [FakeNeverStatementRepository] with mixed suave and picante statements.
FakeNeverStatementRepository buildFakeMixedRepo({int countEach = 5}) {
  final statements = <NeverStatement>[];
  for (var i = 1; i <= countEach; i++) {
    statements.add(
      NeverStatement.create(
        id: i,
        frase: 'Yo nunca he hecho algo suave $i',
        intensidad: Intensidad.suave,
      ),
    );
    statements.add(
      NeverStatement.create(
        id: countEach + i,
        frase: 'Yo nunca he hecho algo picante $i',
        intensidad: Intensidad.picante,
      ),
    );
  }
  return FakeNeverStatementRepository(statements);
}

/// Builds an empty [FakeNeverStatementRepository] (sin frases path).
FakeNeverStatementRepository buildEmptyRepo() =>
    FakeNeverStatementRepository(const []);
