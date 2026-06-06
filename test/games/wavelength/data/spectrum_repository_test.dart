/// Tests for [SpectrumRepository]: insert, bulkInsert, count, getAll round-trip.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sqflite_common_ffi/sqflite_ffi.dart';

import 'package:sajitarios_gamespot/games/wavelength/data/spectrum_repository.dart';
import 'package:sajitarios_gamespot/games/wavelength/data/wavelength_schema.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/spectrum.dart';

Spectrum _spectrum({String left = 'frío', String right = 'caliente'}) =>
    Spectrum(id: null, leftConcept: left, rightConcept: right);

void main() {
  setUpAll(() {
    sqfliteFfiInit();
    databaseFactory = databaseFactoryFfi;
  });

  group('SpectrumRepository', () {
    late Database db;
    late SpectrumRepository repo;

    setUp(() async {
      db = await databaseFactoryFfi.openDatabase(inMemoryDatabasePath);
      await WavelengthSchema.createTables(db);
      repo = SpectrumRepository(db);
    });

    tearDown(() async {
      await db.close();
    });

    group('count', () {
      test('returns 0 on empty table', () async {
        expect(await repo.count(), 0);
      });

      test('increments after insert', () async {
        await repo.insert(_spectrum());
        await repo.insert(_spectrum(left: 'barato', right: 'caro'));
        expect(await repo.count(), 2);
      });
    });

    group('insert', () {
      test('returns spectrum with assigned id', () async {
        final inserted = await repo.insert(_spectrum());
        expect(inserted.id, greaterThan(0));
        expect(inserted.leftConcept, 'frío');
        expect(inserted.rightConcept, 'caliente');
      });

      test('concepts are preserved through insert', () async {
        final inserted = await repo.insert(
          _spectrum(left: 'pequeño', right: 'grande'),
        );
        expect(inserted.leftConcept, 'pequeño');
        expect(inserted.rightConcept, 'grande');
      });
    });

    group('bulkInsert', () {
      test('inserts all and returns spectra with ids', () async {
        final spectra = [
          _spectrum(left: 'frío', right: 'caliente'),
          _spectrum(left: 'barato', right: 'caro'),
          _spectrum(left: 'lento', right: 'rápido'),
        ];
        final inserted = await repo.bulkInsert(spectra);
        expect(inserted.length, 3);
        for (final s in inserted) {
          expect(s.id, greaterThan(0));
        }
        expect(await repo.count(), 3);
      });

      test('ids are distinct', () async {
        final spectra = [
          _spectrum(left: 'a', right: 'b'),
          _spectrum(left: 'c', right: 'd'),
        ];
        final inserted = await repo.bulkInsert(spectra);
        final ids = inserted.map((s) => s.id).toSet();
        expect(ids.length, 2);
      });
    });

    group('getAll', () {
      test('returns empty list when table is empty', () async {
        expect(await repo.getAll(), isEmpty);
      });

      test('returns all spectra ordered by id', () async {
        await repo.insert(_spectrum(left: 'frío', right: 'caliente'));
        await repo.insert(_spectrum(left: 'barato', right: 'caro'));

        final all = await repo.getAll();
        expect(all.length, 2);
        expect(all[0].leftConcept, 'frío');
        expect(all[1].leftConcept, 'barato');
      });

      test('round-trips leftConcept and rightConcept', () async {
        await repo.insert(_spectrum(left: 'oscuro', right: 'luminoso'));
        final all = await repo.getAll();
        expect(all.first.leftConcept, 'oscuro');
        expect(all.first.rightConcept, 'luminoso');
      });

      test('returned spectra have positive ids', () async {
        await repo.insert(_spectrum());
        final all = await repo.getAll();
        expect(all.first.id, greaterThan(0));
      });
    });
  });
}
