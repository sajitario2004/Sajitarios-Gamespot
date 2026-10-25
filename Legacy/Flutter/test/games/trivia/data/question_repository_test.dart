/// Tests for [QuestionRepository]: insert, count, filtered pool fetch,
/// options decoding, and correctIndex preservation.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/trivia/data/question_repository.dart';
import 'package:sajitarios_gamespot/games/trivia/data/trivia_schema.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/difficulty.dart';
import 'package:sajitarios_gamespot/games/trivia/domain/question.dart';

Question _q({
  int id = 0,
  String tematicaId = 'historia',
  Difficulty difficulty = Difficulty.facil,
  String enunciado = 'Test question',
  int correctIndex = 2,
}) => Question.create(
  id: id,
  tematicaId: tematicaId,
  difficulty: difficulty,
  enunciado: enunciado,
  options: ['Opt A', 'Opt B', 'Opt C', 'Opt D'],
  correctIndex: correctIndex,
);

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('QuestionRepository', () {
    late Database db;
    late QuestionRepository repo;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await TriviaSchema.createTables(db);
      repo = QuestionRepository(db);
    });

    tearDown(() async {
      await db.close();
    });

    group('insert and count', () {
      test('count returns 0 on empty table', () async {
        expect(await repo.count(), 0);
      });

      test('insert returns question with assigned id', () async {
        final inserted = await repo.insert(_q());
        expect(inserted.id, greaterThan(0));
      });

      test('count increments after insert', () async {
        await repo.insert(_q());
        await repo.insert(_q(enunciado: 'Another question'));
        expect(await repo.count(), 2);
      });

      test('insertAll inserts all and returns questions with ids', () async {
        final qs = [
          _q(enunciado: 'Q1'),
          _q(enunciado: 'Q2'),
          _q(enunciado: 'Q3'),
        ];
        final inserted = await repo.insertAll(qs);
        expect(inserted.length, 3);
        for (final q in inserted) {
          expect(q.id, greaterThan(0));
        }
        expect(await repo.count(), 3);
      });
    });

    group('countByTematicaAndDifficulty', () {
      test('returns 0 when no matching rows', () async {
        expect(
          await repo.countByTematicaAndDifficulty('ciencia', Difficulty.facil),
          0,
        );
      });

      test('counts only matching tematica and difficulty', () async {
        await repo.insert(
          _q(tematicaId: 'historia', difficulty: Difficulty.facil),
        );
        await repo.insert(
          _q(tematicaId: 'historia', difficulty: Difficulty.dificil),
        );
        await repo.insert(
          _q(tematicaId: 'ciencia', difficulty: Difficulty.facil),
        );

        expect(
          await repo.countByTematicaAndDifficulty('historia', Difficulty.facil),
          1,
        );
        expect(
          await repo.countByTematicaAndDifficulty(
            'historia',
            Difficulty.dificil,
          ),
          1,
        );
        expect(
          await repo.countByTematicaAndDifficulty('ciencia', Difficulty.facil),
          1,
        );
      });
    });

    group('getPool', () {
      test('returns empty list when tematicaIds is empty', () async {
        await repo.insert(_q());
        final pool = await repo.getPool(
          tematicaIds: {},
          difficulty: Difficulty.facil,
        );
        expect(pool, isEmpty);
      });

      test('filters by tematicaId and difficulty', () async {
        await repo.insert(
          _q(tematicaId: 'historia', difficulty: Difficulty.facil),
        );
        await repo.insert(
          _q(tematicaId: 'historia', difficulty: Difficulty.dificil),
        );
        await repo.insert(
          _q(tematicaId: 'ciencia', difficulty: Difficulty.facil),
        );

        final pool = await repo.getPool(
          tematicaIds: {'historia'},
          difficulty: Difficulty.facil,
        );
        expect(pool.length, 1);
        expect(pool.first.tematicaId, 'historia');
        expect(pool.first.difficulty, Difficulty.facil);
      });

      test('returns questions from multiple tematicas', () async {
        await repo.insert(
          _q(tematicaId: 'historia', difficulty: Difficulty.facil),
        );
        await repo.insert(
          _q(tematicaId: 'ciencia', difficulty: Difficulty.facil),
        );

        final pool = await repo.getPool(
          tematicaIds: {'historia', 'ciencia'},
          difficulty: Difficulty.facil,
        );
        expect(pool.length, 2);
      });

      test('decoded options list matches original', () async {
        final original = Question.create(
          id: 0,
          tematicaId: 'historia',
          difficulty: Difficulty.facil,
          enunciado: 'Options test',
          options: ['Alpha', 'Beta', 'Gamma', 'Delta'],
          correctIndex: 3,
        );
        await repo.insert(original);
        final pool = await repo.getPool(
          tematicaIds: {'historia'},
          difficulty: Difficulty.facil,
        );
        expect(pool.first.options, ['Alpha', 'Beta', 'Gamma', 'Delta']);
      });

      test('correctIndex is preserved through round-trip', () async {
        await repo.insert(_q(correctIndex: 3));
        final pool = await repo.getPool(
          tematicaIds: {'historia'},
          difficulty: Difficulty.facil,
        );
        expect(pool.first.correctIndex, 3);
      });
    });
  });
}
