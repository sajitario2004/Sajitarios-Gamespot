/// Tests for [TriviaConfig] validation.
library;

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/games/trivia/domain/trivia_config.dart';

TriviaConfigResult _create({List<String>? players, Set<String>? tematicas}) =>
    TriviaConfig.create(
      playerNames: players ?? ['Ana', 'Bob'],
      selectedTematicaIds: tematicas ?? {'historia'},
    );

void main() {
  group('TriviaConfig.create', () {
    group('player count validation', () {
      test('accepts minimum 2 players', () {
        final result = _create(players: ['Ana', 'Bob']);
        expect(result.isSuccess, isTrue);
      });

      test('accepts maximum 6 players', () {
        final result = _create(players: ['P1', 'P2', 'P3', 'P4', 'P5', 'P6']);
        expect(result.isSuccess, isTrue);
      });

      test('rejects 1 player — pocosJugadores', () {
        final result = _create(players: ['Solo']);
        expect(result.isSuccess, isFalse);
        expect(result.error, TriviaConfigError.pocosJugadores);
      });

      test('rejects 7 players — demasiadosJugadores', () {
        final result = _create(
          players: ['P1', 'P2', 'P3', 'P4', 'P5', 'P6', 'P7'],
        );
        expect(result.isSuccess, isFalse);
        expect(result.error, TriviaConfigError.demasiadosJugadores);
      });
    });

    group('name validation', () {
      test('rejects empty name — nombreVacio', () {
        final result = _create(players: ['Ana', '']);
        expect(result.isSuccess, isFalse);
        expect(result.error, TriviaConfigError.nombreVacio);
      });

      test('rejects whitespace-only name — nombreVacio', () {
        final result = _create(players: ['Ana', '   ']);
        expect(result.isSuccess, isFalse);
        expect(result.error, TriviaConfigError.nombreVacio);
      });

      test('rejects duplicate names case-insensitive — nombresDuplicados', () {
        final result = _create(players: ['Ana', 'ana']);
        expect(result.isSuccess, isFalse);
        expect(result.error, TriviaConfigError.nombresDuplicados);
      });
    });

    group('tematicas validation', () {
      test('rejects empty tematica set — sinTematicas', () {
        final result = _create(tematicas: {});
        expect(result.isSuccess, isFalse);
        expect(result.error, TriviaConfigError.sinTematicas);
      });

      test('accepts one or more tematica ids', () {
        final result = _create(tematicas: {'historia', 'ciencia'});
        expect(result.isSuccess, isTrue);
      });
    });

    group('successful construction', () {
      test('trims whitespace from player names', () {
        final result = _create(players: [' Ana ', ' Bob ']);
        expect(result.isSuccess, isTrue);
        expect(result.config!.playerNames, ['Ana', 'Bob']);
      });

      test('resulting config is immutable (playerNames unmodifiable)', () {
        final result = _create();
        expect(
          () => result.config!.playerNames.add('Extra'),
          throwsUnsupportedError,
        );
      });
    });
  });
}
