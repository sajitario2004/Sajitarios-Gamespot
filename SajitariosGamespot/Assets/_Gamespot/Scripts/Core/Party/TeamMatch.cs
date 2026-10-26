using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamespot.Core
{
    /// <summary>
    /// Marcador de una partida por turnos entre equipos (o jugadores sueltos) (V074): juega cada uno por orden; al cerrar
    /// el turno del último se acaba la ronda. Se gana al acabar una ronda con alguien en el objetivo de puntos o tras la
    /// última ronda; si en ese momento hay empate arriba, se juega otra ronda de desempate. Los puntos no bajan de 0.
    /// Sin objetivo ni rondas, se juega hasta <see cref="Terminar"/>.
    /// </summary>
    public sealed class TeamMatch
    {
        private readonly int[] _points;

        public TeamMatch(IReadOnlyList<string> teams, int objetivo = 0, int rondas = 0)
        {
            if (teams == null || teams.Count == 0) throw new ArgumentException("Hace falta al menos un equipo", nameof(teams));
            Equipos = teams;
            Objetivo = Math.Max(0, objetivo);
            Rondas = Math.Max(0, rondas);
            _points = new int[teams.Count];
            Ronda = 1;
        }

        public IReadOnlyList<string> Equipos { get; }
        public int Objetivo { get; }
        public int Rondas { get; }
        public IReadOnlyList<int> Puntos => _points;

        /// <summary>A quién le toca.</summary>
        public int Actual { get; private set; }

        /// <summary>Ronda en curso (desde 1).</summary>
        public int Ronda { get; private set; }

        /// <summary>Se está jugando una ronda de desempate.</summary>
        public bool Desempate { get; private set; }

        public bool Terminada { get; private set; }

        /// <summary>Los que van primeros (al terminar, los ganadores; varios si se terminó a mano con empate).</summary>
        public IReadOnlyList<int> Ganadores
        {
            get
            {
                var top = _points.Max();
                return Enumerable.Range(0, _points.Length).Where(i => _points[i] == top).ToList();
            }
        }

        /// <summary>Por puntos (más primero) y, a igualdad, por orden.</summary>
        public IReadOnlyList<int> Clasificacion => Enumerable.Range(0, _points.Length).OrderByDescending(i => _points[i]).ThenBy(i => i).ToList();

        /// <summary>Suma (o resta) los puntos del turno al que juega y pasa al siguiente.</summary>
        public void CerrarTurno(int puntos)
        {
            if (Terminada) return;
            _points[Actual] = Math.Max(0, _points[Actual] + puntos);
            Actual++;
            if (Actual < _points.Length) return;
            Actual = 0;
            var reached = (Objetivo > 0 && _points.Max() >= Objetivo) || (Rondas > 0 && Ronda >= Rondas);
            if (reached && Ganadores.Count == 1)
            {
                Terminada = true;
                return;
            }
            Desempate = reached;
            Ronda++;
        }

        public void Terminar() => Terminada = true;
    }
}
