/// Tests para [TabuWordRepository]: insert, bulkInsert, count, getAll y
/// decodificacion de prohibidas_json via FFI en memoria.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/tabu/data/tabu_schema.dart';
import 'package:sajitarios_gamespot/games/tabu/data/tabu_word_repository.dart';
import 'package:sajitarios_gamespot/games/tabu/domain/tabu_word.dart';

TabuWord _word({
  int id = 0,
  String palabra = 'Pirata',
  List<String>? prohibidas,
}) => TabuWord.create(
  id: id,
  palabra: palabra,
  prohibidas: prohibidas ?? ['barco', 'tesoro', 'mar', 'loro'],
);

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('TabuWordRepository', () {
    late Database db;
    late TabuWordRepository repo;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await TabuSchema.createTables(db);
      repo = TabuWordRepository(db);
    });

    tearDown(() async {
      await db.close();
    });

    group('insert y count', () {
      test('count devuelve 0 en tabla vacia', () async {
        expect(await repo.count(), 0);
      });

      test('insert devuelve la palabra con id asignado', () async {
        final inserted = await repo.insert(_word());
        expect(inserted.id, greaterThan(0));
      });

      test('count se incrementa con cada insert', () async {
        await repo.insert(_word());
        await repo.insert(_word(palabra: 'Vikingo'));
        expect(await repo.count(), 2);
      });
    });

    group('bulkInsert', () {
      test('inserta todas las palabras y devuelve lista con ids', () async {
        final words = [
          _word(palabra: 'Pirata'),
          _word(palabra: 'Vikingo'),
          _word(palabra: 'Samurai'),
        ];
        final inserted = await repo.bulkInsert(words);
        expect(inserted.length, 3);
        for (final w in inserted) {
          expect(w.id, greaterThan(0));
        }
        expect(await repo.count(), 3);
      });

      test('los ids asignados son distintos', () async {
        final words = [
          _word(palabra: 'A'),
          _word(palabra: 'B'),
          _word(palabra: 'C'),
        ];
        final inserted = await repo.bulkInsert(words);
        final ids = inserted.map((w) => w.id).toSet();
        expect(ids.length, 3);
      });
    });

    group('getAll', () {
      test('devuelve lista vacia cuando no hay palabras', () async {
        expect(await repo.getAll(), isEmpty);
      });

      test('devuelve todas las palabras insertadas', () async {
        await repo.insert(_word(palabra: 'Pirata'));
        await repo.insert(_word(palabra: 'Vikingo'));
        final all = await repo.getAll();
        expect(all.length, 2);
      });

      test('decodifica prohibidas_json correctamente (4 prohibidas)', () async {
        final original = _word(
          palabra: 'Pirata',
          prohibidas: ['barco', 'tesoro', 'mar', 'loro'],
        );
        await repo.insert(original);
        final all = await repo.getAll();
        expect(all.first.prohibidas, ['barco', 'tesoro', 'mar', 'loro']);
      });

      test('decodifica prohibidas_json correctamente (5 prohibidas)', () async {
        final original = _word(
          palabra: 'Pirata',
          prohibidas: ['barco', 'tesoro', 'mar', 'loro', 'pata de palo'],
        );
        await repo.insert(original);
        final all = await repo.getAll();
        expect(all.first.prohibidas, [
          'barco',
          'tesoro',
          'mar',
          'loro',
          'pata de palo',
        ]);
      });

      test('preserva la palabra a traves del round-trip', () async {
        await repo.insert(_word(palabra: 'Volcan'));
        final all = await repo.getAll();
        expect(all.first.palabra, 'Volcan');
      });

      test('devuelve palabras ordenadas por id ascendente', () async {
        await repo.insert(_word(palabra: 'C'));
        await repo.insert(_word(palabra: 'A'));
        await repo.insert(_word(palabra: 'B'));
        final all = await repo.getAll();
        final ids = all.map((w) => w.id).toList();
        expect(ids, equals(ids..sort()));
      });
    });
  });
}
