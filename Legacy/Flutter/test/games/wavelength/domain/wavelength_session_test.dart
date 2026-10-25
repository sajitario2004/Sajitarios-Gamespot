/// Tests for [WavelengthSession]: advance, cumulative score, clue-giver rotation.
library;

import 'package:flutter_test/flutter_test.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/spectrum.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_round.dart';
import 'package:sajitarios_gamespot/games/wavelength/domain/wavelength_session.dart';

WavelengthRound _completedRound({double target = 0.5, double guess = 0.5}) {
  final spectrum = Spectrum(
    id: 1,
    leftConcept: 'frío',
    rightConcept: 'caliente',
  );
  return WavelengthRound.start(
    spectrum: spectrum,
    targetPosition: target,
  ).withClue('tibio').withGuess(guess);
}

void main() {
  group('WavelengthSession', () {
    group('start', () {
      test('creates session at round 0 with zero score', () {
        final session = WavelengthSession.start(
          playerNames: ['Ana', 'Bob'],
          totalRondas: 5,
        );
        expect(session.currentRoundIndex, 0);
        expect(session.cumulativeScore, 0);
        expect(session.isOver, isFalse);
      });

      test('throws when playerNames is empty', () {
        expect(
          () => WavelengthSession.start(playerNames: [], totalRondas: 5),
          throwsArgumentError,
        );
      });

      test('throws when totalRondas < 1', () {
        expect(
          () => WavelengthSession.start(playerNames: ['Ana'], totalRondas: 0),
          throwsArgumentError,
        );
      });
    });

    group('currentPsychic rotation', () {
      test('first round psychic is first player', () {
        final session = WavelengthSession.start(
          playerNames: ['Ana', 'Bob', 'Carlos'],
          totalRondas: 6,
        );
        expect(session.currentPsychic, 'Ana');
      });

      test('psychic rotates in introduction order', () {
        var session = WavelengthSession.start(
          playerNames: ['Ana', 'Bob', 'Carlos'],
          totalRondas: 6,
        );
        expect(session.currentPsychic, 'Ana');
        session = session.recordRound(_completedRound());
        expect(session.currentPsychic, 'Bob');
        session = session.recordRound(_completedRound());
        expect(session.currentPsychic, 'Carlos');
      });

      test('psychic wraps around after all players have gone', () {
        var session = WavelengthSession.start(
          playerNames: ['Ana', 'Bob'],
          totalRondas: 4,
        );
        session = session.recordRound(_completedRound()); // Ana
        session = session.recordRound(_completedRound()); // Bob
        expect(session.currentPsychic, 'Ana'); // wraps
      });

      test('currentPsychic is null when session is over', () {
        var session = WavelengthSession.start(
          playerNames: ['Ana'],
          totalRondas: 1,
        );
        session = session.recordRound(_completedRound());
        expect(session.isOver, isTrue);
        expect(session.currentPsychic, isNull);
      });
    });

    group('recordRound and cumulative score', () {
      test('cumulative score accumulates across rounds', () {
        var session = WavelengthSession.start(
          playerNames: ['Ana', 'Bob'],
          totalRondas: 3,
        );
        // Bullseye (4 pts).
        session = session.recordRound(_completedRound(target: 0.5, guess: 0.5));
        expect(session.cumulativeScore, 4);

        // Miss (0 pts).
        session = session.recordRound(_completedRound(target: 0.5, guess: 0.0));
        expect(session.cumulativeScore, 4);

        // Near band (3 pts): target 0.5, guess at near edge.
        session = session.recordRound(
          _completedRound(target: 0.5, guess: 0.5 + kNearHalfWidth),
        );
        expect(session.cumulativeScore, 7);
      });

      test('recordRound is immutable — original session unchanged', () {
        final session = WavelengthSession.start(
          playerNames: ['Ana'],
          totalRondas: 2,
        );
        session.recordRound(_completedRound());
        expect(session.currentRoundIndex, 0);
        expect(session.cumulativeScore, 0);
      });

      test('isOver becomes true after all rounds', () {
        var session = WavelengthSession.start(
          playerNames: ['Ana'],
          totalRondas: 2,
        );
        expect(session.isOver, isFalse);
        session = session.recordRound(_completedRound());
        expect(session.isOver, isFalse);
        session = session.recordRound(_completedRound());
        expect(session.isOver, isTrue);
      });

      test('recordRound throws StateError when session is over', () {
        var session = WavelengthSession.start(
          playerNames: ['Ana'],
          totalRondas: 1,
        );
        session = session.recordRound(_completedRound());
        expect(() => session.recordRound(_completedRound()), throwsStateError);
      });

      test('recordRound throws ArgumentError when round has no guess', () {
        final session = WavelengthSession.start(
          playerNames: ['Ana'],
          totalRondas: 2,
        );
        final roundNoGuess = WavelengthRound.start(
          spectrum: Spectrum(
            id: 1,
            leftConcept: 'frío',
            rightConcept: 'caliente',
          ),
          targetPosition: 0.5,
        ).withClue('tibio');
        expect(() => session.recordRound(roundNoGuess), throwsArgumentError);
      });
    });
  });
}
