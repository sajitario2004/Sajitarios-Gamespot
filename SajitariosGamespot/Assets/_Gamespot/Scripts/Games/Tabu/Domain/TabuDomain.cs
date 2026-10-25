using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.Tabu.Domain
{
    /// <summary>Palabra a describir y sus 4 o 5 palabras prohibidas (recortadas y no vacías).</summary>
    public sealed class TabuWord : IEquatable<TabuWord>
    {
        public const int MinForbidden = 4;
        public const int MaxForbidden = 5;

        public TabuWord(long id, string palabra, IReadOnlyList<string> prohibidas)
        {
            Palabra = palabra?.Trim() ?? string.Empty;
            if (Palabra.Length == 0) throw new ArgumentException("La palabra no puede estar vacía", nameof(palabra));
            var forbidden = (prohibidas ?? Array.Empty<string>()).Select(p => p?.Trim() ?? string.Empty).ToList();
            if (forbidden.Count < MinForbidden || forbidden.Count > MaxForbidden) throw new ArgumentException("Hacen falta de 4 a 5 palabras prohibidas", nameof(prohibidas));
            if (forbidden.Any(p => p.Length == 0)) throw new ArgumentException("Una palabra prohibida está vacía", nameof(prohibidas));
            Id = id;
            Prohibidas = forbidden.AsReadOnly();
        }

        public long Id { get; }
        public string Palabra { get; }
        public IReadOnlyList<string> Prohibidas { get; }

        public bool Equals(TabuWord other) => other != null && other.Id == Id && other.Palabra == Palabra && other.Prohibidas.SequenceEqual(Prohibidas);
        public override bool Equals(object obj) => Equals(obj as TabuWord);
        public override int GetHashCode() => (Id.GetHashCode() * 397) ^ Palabra.GetHashCode();
    }

    public enum TabuEquipo
    {
        A,
        B
    }

    public enum TabuConfigError
    {
        EquipoAVacio,
        EquipoBVacio,
        EquiposDuplicados,
        TurnoSegundosInvalido,
        ObjetivoVictoriasInvalido
    }

    /// <summary>Dos equipos con nombre, turno de 30–120 s (el setup ofrece 30/60/90, 60 por defecto) y objetivo de victorias (3).</summary>
    public sealed class TabuConfig
    {
        public const int MinTurnSeconds = 30;
        public const int MaxTurnSeconds = 120;
        public const int DefaultTurnSeconds = 60;
        public const int DefaultTargetWins = 3;
        public static readonly IReadOnlyList<int> TurnOptions = new[] { 30, 60, 90 };

        private TabuConfig(string a, string b, int seconds, int target)
        {
            EquipoA = a;
            EquipoB = b;
            TurnoSegundos = seconds;
            ObjetivoVictorias = target;
        }

        public string EquipoA { get; }
        public string EquipoB { get; }
        public int TurnoSegundos { get; }
        public int ObjetivoVictorias { get; }

        public string NameOf(TabuEquipo team) => team == TabuEquipo.A ? EquipoA : EquipoB;

        public static (TabuConfig config, TabuConfigError? error) Create(string equipoA, string equipoB, int turnoSegundos = DefaultTurnSeconds,
            int objetivoVictorias = DefaultTargetWins)
        {
            var a = equipoA?.Trim() ?? string.Empty;
            var b = equipoB?.Trim() ?? string.Empty;
            if (a.Length == 0) return (null, TabuConfigError.EquipoAVacio);
            if (b.Length == 0) return (null, TabuConfigError.EquipoBVacio);
            if (string.Equals(a.ToLowerInvariant(), b.ToLowerInvariant(), StringComparison.Ordinal)) return (null, TabuConfigError.EquiposDuplicados);
            if (turnoSegundos < MinTurnSeconds || turnoSegundos > MaxTurnSeconds) return (null, TabuConfigError.TurnoSegundosInvalido);
            if (objetivoVictorias <= 0) return (null, TabuConfigError.ObjetivoVictoriasInvalido);
            return (new TabuConfig(a, b, turnoSegundos, objetivoVictorias), null);
        }
    }

    /// <summary>
    /// Partida de Tabú por equipos (port de <c>TabuSession</c>, inmutable). Acierto +1; saltar no puntúa (sin
    /// límite); falta −1 sin bajar de 0 y sin cortar el turno. Al terminar el turno, si hubo algún acierto el equipo
    /// gana una ronda; luego juega el otro. Gana el primero en llegar al objetivo. No se repiten palabras hasta
    /// que se agotan (entonces <see cref="ReiniciarPalabras"/>).
    /// </summary>
    public sealed class TabuSession
    {
        private readonly HashSet<long> _used;

        private TabuSession(TabuConfig config, int winsA, int winsB, TabuEquipo current, TabuWord word, int correct, int skips, HashSet<long> used)
        {
            Config = config;
            VictoriasA = winsA;
            VictoriasB = winsB;
            EquipoActual = current;
            PalabraActual = word;
            AciertosTurno = correct;
            Saltos = skips;
            _used = used;
        }

        public static TabuSession Start(TabuConfig config, TabuWord first)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (first == null) throw new ArgumentNullException(nameof(first));
            return new TabuSession(config, 0, 0, TabuEquipo.A, first, 0, 0, new HashSet<long> { first.Id });
        }

        public TabuConfig Config { get; }
        public int VictoriasA { get; }
        public int VictoriasB { get; }
        public TabuEquipo EquipoActual { get; }
        public TabuWord PalabraActual { get; }
        public int AciertosTurno { get; }
        public int Saltos { get; }
        public IReadOnlyCollection<long> PalabrasUsadas => _used;

        public bool IsOver => VictoriasA >= Config.ObjetivoVictorias || VictoriasB >= Config.ObjetivoVictorias;

        public TabuEquipo? Ganador => VictoriasA >= Config.ObjetivoVictorias ? TabuEquipo.A
            : VictoriasB >= Config.ObjetivoVictorias ? TabuEquipo.B : (TabuEquipo?)null;

        public TabuSession RegistrarAcierto() => With(correct: AciertosTurno + 1);

        public TabuSession RegistrarSalto() => With(skips: Saltos + 1);

        public TabuSession RegistrarFalta() => With(correct: Math.Max(0, AciertosTurno - 1));

        public TabuSession AvanzarPalabra(TabuWord next)
        {
            AssertNotOver();
            if (next == null) throw new ArgumentNullException(nameof(next));
            if (_used.Contains(next.Id)) throw new ArgumentException($"La palabra {next.Id} ya salió en esta partida", nameof(next));
            var used = new HashSet<long>(_used) { next.Id };
            return new TabuSession(Config, VictoriasA, VictoriasB, EquipoActual, next, AciertosTurno, Saltos, used);
        }

        /// <summary>Cierra el turno: +1 victoria si hubo aciertos, cambia de equipo y empieza con <paramref name="newWord"/>.</summary>
        public TabuSession TerminarTurno(TabuWord newWord)
        {
            AssertNotOver();
            var won = AciertosTurno > 0;
            var winsA = EquipoActual == TabuEquipo.A && won ? VictoriasA + 1 : VictoriasA;
            var winsB = EquipoActual == TabuEquipo.B && won ? VictoriasB + 1 : VictoriasB;
            var next = EquipoActual == TabuEquipo.A ? TabuEquipo.B : TabuEquipo.A;
            var used = new HashSet<long>(_used) { newWord.Id };
            return new TabuSession(Config, winsA, winsB, next, newWord, 0, 0, used);
        }

        /// <summary>Se acabaron las palabras: vuelven a estar disponibles todas menos la actual.</summary>
        public TabuSession ReiniciarPalabras()
        {
            AssertNotOver();
            return new TabuSession(Config, VictoriasA, VictoriasB, EquipoActual, PalabraActual, AciertosTurno, Saltos, new HashSet<long> { PalabraActual.Id });
        }

        private TabuSession With(int? correct = null, int? skips = null)
        {
            AssertNotOver();
            return new TabuSession(Config, VictoriasA, VictoriasB, EquipoActual, PalabraActual, correct ?? AciertosTurno, skips ?? Saltos,
                new HashSet<long>(_used));
        }

        private void AssertNotOver()
        {
            if (IsOver) throw new InvalidOperationException("La partida de Tabú ya ha terminado");
        }
    }

    /// <summary>Elige al azar una palabra que no haya salido. Lanza si no queda ninguna.</summary>
    public sealed class PickTabuWordUseCase
    {
        private readonly IRandomProvider _random;

        public PickTabuWordUseCase(IRandomProvider random) => _random = random ?? throw new ArgumentNullException(nameof(random));

        public TabuWord Execute(IReadOnlyList<TabuWord> pool, IReadOnlyCollection<long> used)
        {
            if (pool == null || pool.Count == 0) throw new ArgumentException("No hay palabras", nameof(pool));
            var available = pool.Where(w => used == null || !used.Contains(w.Id)).ToList();
            if (available.Count == 0) throw new ArgumentException("Ya han salido todas las palabras");
            return available[_random.NextInt(available.Count)];
        }
    }
}
