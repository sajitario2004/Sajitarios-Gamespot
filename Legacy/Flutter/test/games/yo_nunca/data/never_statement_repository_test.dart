/// Tests for [NeverStatementRepository]: insert, bulkInsert, count,
/// getByIntensidades filtering, and getAll — using an FFI in-memory database.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/yo_nunca/data/never_statement_repository.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/data/yo_nunca_schema.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/intensidad.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/never_statement.dart';

NeverStatement _stmt({
  int id = 0,
  String frase = 'Yo nunca he viajado solo',
  Intensidad intensidad = Intensidad.suave,
}) => NeverStatement.create(id: id, frase: frase, intensidad: intensidad);

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('NeverStatementRepository', () {
    late Database db;
    late NeverStatementRepository repo;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await YoNuncaSchema.createTables(db);
      repo = NeverStatementRepository(db);
    });

    tearDown(() async {
      await db.close();
    });

    group('insert and count', () {
      test('count returns 0 on empty table', () async {
        expect(await repo.count(), 0);
      });

      test('insert returns statement with assigned id', () async {
        final inserted = await repo.insert(_stmt());
        expect(inserted.id, greaterThan(0));
      });

      test('insert preserves frase and intensidad', () async {
        final inserted = await repo.insert(
          _stmt(
            frase: 'Yo nunca he comido pizza fría',
            intensidad: Intensidad.picante,
          ),
        );
        expect(inserted.frase, 'Yo nunca he comido pizza fría');
        expect(inserted.intensidad, Intensidad.picante);
      });

      test('count increments after insert', () async {
        await repo.insert(_stmt(frase: 'Frase 1'));
        await repo.insert(_stmt(frase: 'Frase 2'));
        expect(await repo.count(), 2);
      });
    });

    group('bulkInsert', () {
      test('inserts all statements and returns them with ids', () async {
        final stmts = [
          _stmt(frase: 'Frase A'),
          _stmt(frase: 'Frase B', intensidad: Intensidad.picante),
          _stmt(frase: 'Frase C'),
        ];
        final inserted = await repo.bulkInsert(stmts);
        expect(inserted.length, 3);
        for (final s in inserted) {
          expect(s.id, greaterThan(0));
        }
        expect(await repo.count(), 3);
      });
    });

    group('getByIntensidades', () {
      setUp(() async {
        await repo.insert(
          _stmt(frase: 'Suave 1', intensidad: Intensidad.suave),
        );
        await repo.insert(
          _stmt(frase: 'Suave 2', intensidad: Intensidad.suave),
        );
        await repo.insert(
          _stmt(frase: 'Picante 1', intensidad: Intensidad.picante),
        );
      });

      test('returns empty list when intensidades is empty', () async {
        final result = await repo.getByIntensidades({});
        expect(result, isEmpty);
      });

      test('filters by suave only', () async {
        final result = await repo.getByIntensidades({Intensidad.suave});
        expect(result.length, 2);
        expect(result.every((s) => s.intensidad == Intensidad.suave), isTrue);
      });

      test('filters by picante only', () async {
        final result = await repo.getByIntensidades({Intensidad.picante});
        expect(result.length, 1);
        expect(result.first.intensidad, Intensidad.picante);
      });

      test('returns all when both intensidades requested', () async {
        final result = await repo.getByIntensidades({
          Intensidad.suave,
          Intensidad.picante,
        });
        expect(result.length, 3);
      });
    });

    group('getAll', () {
      test('returns all statements ordered by id', () async {
        await repo.insert(_stmt(frase: 'Primera'));
        await repo.insert(_stmt(frase: 'Segunda'));
        await repo.insert(_stmt(frase: 'Tercera'));

        final all = await repo.getAll();
        expect(all.length, 3);
        for (var i = 1; i < all.length; i++) {
          expect(all[i].id, greaterThan(all[i - 1].id));
        }
      });

      test('returns empty list on empty table', () async {
        final all = await repo.getAll();
        expect(all, isEmpty);
      });
    });
  });
}
