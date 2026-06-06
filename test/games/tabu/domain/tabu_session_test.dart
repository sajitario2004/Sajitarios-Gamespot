/// Tests para [TabuSession]: transiciones de estado, acumulacion de victorias
/// de ronda y deteccion del fin de partida.
library;

import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/games/tabu/domain/tabu_config.dart';
import 'package:sajitarios_gamespot/games/tabu/domain/tabu_session.dart';
import 'package:sajitarios_gamespot/games/tabu/domain/tabu_word.dart';

TabuConfig _config({int objetivoVictorias = 3}) => TabuConfig.create(
  equipoA: 'Equipo A',
  equipoB: 'Equipo B',
  turnoSegundos: 60,
  objetivoVictorias: objetivoVictorias,
).config!;

TabuWord _word(int id) => TabuWord.create(
  id: id,
  palabra: 'Palabra $id',
  prohibidas: ['a', 'b', 'c', 'd'],
);

void main() {
  group('TabuSession', () {
    group('inicio de sesion', () {
      test('empieza con el equipo A describiendo', () {
        final session = TabuSession.start(config: _config(), primera: _word(1));
        expect(session.equipoActual, TabuEquipo.a);
      });

      test('empieza con 0 victorias para ambos equipos', () {
        final session = TabuSession.start(config: _config(), primera: _word(1));
        expect(session.victoriasA, 0);
        expect(session.victoriasB, 0);
      });

      test('empieza con 0 aciertos en el turno', () {
        final session = TabuSession.start(config: _config(), primera: _word(1));
        expect(session.aciertosTurnoActual, 0);
      });

      test('isOver es false al inicio', () {
        final session = TabuSession.start(config: _config(), primera: _word(1));
        expect(session.isOver, isFalse);
      });
    });

    group('registrar acierto', () {
      test('incrementa aciertosTurnoActual en 1', () {
        final session = TabuSession.start(config: _config(), primera: _word(1));
        final next = session.registrarAcierto();
        expect(next.aciertosTurnoActual, 1);
      });

      test('acumula multiples aciertos', () {
        var session = TabuSession.start(config: _config(), primera: _word(1));
        session = session
            .registrarAcierto()
            .registrarAcierto()
            .registrarAcierto();
        expect(session.aciertosTurnoActual, 3);
      });

      test('lanza StateError si la partida ya termino', () {
        var session = TabuSession.start(
          config: _config(objetivoVictorias: 1),
          primera: _word(1),
        );
        session = session.registrarAcierto();
        session = session.terminarTurno(_word(2));
        expect(session.isOver, isTrue);
        expect(() => session.registrarAcierto(), throwsStateError);
      });
    });

    group('registrar salto', () {
      test('incrementa saltosUsados en 1', () {
        final session = TabuSession.start(config: _config(), primera: _word(1));
        final next = session.registrarSalto();
        expect(next.saltosUsados, 1);
      });

      test('no afecta los aciertos', () {
        var session = TabuSession.start(config: _config(), primera: _word(1));
        session = session.registrarAcierto().registrarSalto();
        expect(session.aciertosTurnoActual, 1);
        expect(session.saltosUsados, 1);
      });
    });

    group('registrar falta', () {
      test('descuenta un acierto cuando hay aciertos', () {
        var session = TabuSession.start(config: _config(), primera: _word(1));
        session = session.registrarAcierto().registrarAcierto();
        session = session.registrarFalta();
        expect(session.aciertosTurnoActual, 1);
      });

      test('no baja de 0 aciertos', () {
        final session = TabuSession.start(config: _config(), primera: _word(1));
        final next = session.registrarFalta();
        expect(next.aciertosTurnoActual, 0);
      });
    });

    group('avanzar palabra', () {
      test('cambia la palabra actual y la agrega a usadas', () {
        final session = TabuSession.start(config: _config(), primera: _word(1));
        final next = session.avanzarPalabra(_word(2));
        expect(next.palabraActual.id, 2);
        expect(next.palabrasUsadas, containsAll([1, 2]));
      });

      test('lanza ArgumentError si la palabra ya fue usada', () {
        final session = TabuSession.start(config: _config(), primera: _word(1));
        expect(() => session.avanzarPalabra(_word(1)), throwsArgumentError);
      });
    });

    group('terminar turno', () {
      test('el equipo A gana ronda si tuvo >= 1 acierto', () {
        var session = TabuSession.start(config: _config(), primera: _word(1));
        session = session.registrarAcierto();
        session = session.terminarTurno(_word(2));
        expect(session.victoriasA, 1);
        expect(session.victoriasB, 0);
      });

      test('el equipo A NO gana ronda con 0 aciertos', () {
        final session = TabuSession.start(config: _config(), primera: _word(1));
        final next = session.terminarTurno(_word(2));
        expect(next.victoriasA, 0);
        expect(next.victoriasB, 0);
      });

      test('pasa el turno al equipo B despues del turno A', () {
        final session = TabuSession.start(config: _config(), primera: _word(1));
        final next = session.terminarTurno(_word(2));
        expect(next.equipoActual, TabuEquipo.b);
      });

      test('pasa el turno al equipo A despues del turno B', () {
        var session = TabuSession.start(config: _config(), primera: _word(1));
        session = session.terminarTurno(_word(2)); // turno B
        session = session.terminarTurno(_word(3)); // turno A
        expect(session.equipoActual, TabuEquipo.a);
      });

      test('reinicia aciertos y saltos del turno', () {
        var session = TabuSession.start(config: _config(), primera: _word(1));
        session = session
            .registrarAcierto()
            .registrarAcierto()
            .registrarSalto();
        session = session.terminarTurno(_word(2));
        expect(session.aciertosTurnoActual, 0);
        expect(session.saltosUsados, 0);
      });

      test('el equipo B gana ronda correctamente', () {
        var session = TabuSession.start(config: _config(), primera: _word(1));
        session = session.terminarTurno(_word(2)); // turno B sin aciertos
        session = session.registrarAcierto().registrarAcierto();
        session = session.terminarTurno(_word(3));
        expect(session.victoriasA, 0);
        expect(session.victoriasB, 1);
      });
    });

    group('fin de partida', () {
      test('la partida termina cuando el equipo A llega al objetivo', () {
        var session = TabuSession.start(
          config: _config(objetivoVictorias: 2),
          primera: _word(1),
        );
        // Turno 1 A: 1 acierto -> A gana ronda (A:1, B:0), turno B
        session = session.registrarAcierto().terminarTurno(_word(2));
        // Turno 2 B: sin aciertos -> nadie gana ronda, turno A
        session = session.terminarTurno(_word(3));
        // Turno 3 A: 1 acierto -> A gana ronda (A:2, B:0)
        session = session.registrarAcierto().terminarTurno(_word(4));
        expect(session.isOver, isTrue);
        expect(session.ganador, TabuEquipo.a);
      });

      test('la partida termina cuando el equipo B llega al objetivo', () {
        var session = TabuSession.start(
          config: _config(objetivoVictorias: 1),
          primera: _word(1),
        );
        // Turno A: sin aciertos -> nadie gana, turno B
        session = session.terminarTurno(_word(2));
        // Turno B: 1 acierto -> B gana ronda
        session = session.registrarAcierto().terminarTurno(_word(3));
        expect(session.isOver, isTrue);
        expect(session.ganador, TabuEquipo.b);
      });

      test('ganador es null cuando la partida no ha terminado', () {
        final session = TabuSession.start(config: _config(), primera: _word(1));
        expect(session.ganador, isNull);
      });

      test('lanza StateError al terminar turno cuando ya termino', () {
        var session = TabuSession.start(
          config: _config(objetivoVictorias: 1),
          primera: _word(1),
        );
        session = session.registrarAcierto().terminarTurno(_word(2));
        expect(session.isOver, isTrue);
        expect(() => session.terminarTurno(_word(3)), throwsStateError);
      });

      test(
        'acumula victorias correctamente a lo largo de multiples rondas',
        () {
          var session = TabuSession.start(
            config: _config(objetivoVictorias: 3),
            primera: _word(1),
          );
          var nextId = 2;
          TabuWord nextWord() => _word(nextId++);

          // Ronda 1: A gana
          session = session.registrarAcierto().terminarTurno(nextWord());
          expect(session.victoriasA, 1);
          // B no gana
          session = session.terminarTurno(nextWord());
          expect(session.victoriasB, 0);

          // Ronda 2: A gana
          session = session.registrarAcierto().terminarTurno(nextWord());
          expect(session.victoriasA, 2);
          // B gana
          session = session.registrarAcierto().terminarTurno(nextWord());
          expect(session.victoriasB, 1);

          // Ronda 3: A gana -> partida terminada
          session = session.registrarAcierto().terminarTurno(nextWord());
          expect(session.victoriasA, 3);
          expect(session.isOver, isTrue);
          expect(session.ganador, TabuEquipo.a);
        },
      );
    });
  });
}
