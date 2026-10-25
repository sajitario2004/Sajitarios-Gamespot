using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.VerdadReto.Domain
{
    /// <summary>Tipos de carta de Verdad o Reto, tal como se guardan en la columna <c>tipo</c>.</summary>
    public static class VerdadRetoKinds
    {
        public const string Verdad = "verdad";
        public const string Reto = "reto";

        /// <summary>Prenda de quien se raja.</summary>
        public const string Castigo = "castigo";

        public static readonly IReadOnlyList<string> All = new[] { Verdad, Reto, Castigo };
    }

    /// <summary>Cómo se pasa el turno: en el orden de los nombres o al azar (todos juegan una vez por ronda).</summary>
    public enum OrdenTurnos
    {
        EnOrden,
        AlAzar
    }

    /// <summary>Quién decide entre verdad y reto: el jugador o el móvil.</summary>
    public enum ModoEleccion
    {
        ElJugador,
        AlAzar
    }

    public enum VerdadRetoConfigError
    {
        PocosJugadores,
        DemasiadosJugadores,
        NombreVacio,
        NombresRepetidos,
        SinNiveles
    }

    /// <summary>Borrador de las reglas que se eligen en el setup (con los valores por defecto).</summary>
    public sealed class VerdadRetoOptions
    {
        public OrdenTurnos Orden { get; set; } = OrdenTurnos.EnOrden;
        public ModoEleccion Eleccion { get; set; } = ModoEleccion.ElJugador;

        /// <summary>Se puede decir «Me rajo» y cumplir una prenda.</summary>
        public bool PermitirRajarse { get; set; } = true;

        /// <summary>Se apuntan puntos (verdad 1, reto 2) y al final hay clasificación.</summary>
        public bool Puntos { get; set; } = true;

        /// <summary>Rondas de la partida (todos juegan una vez por ronda). 0 = sin límite.</summary>
        public int Rondas { get; set; }

        /// <summary>Segundos para cumplir un reto. 0 = sin tiempo.</summary>
        public int SegundosReto { get; set; }

        /// <summary>Máximo de verdades seguidas de un mismo jugador. 0 = sin límite.</summary>
        public int MaxVerdadesSeguidas { get; set; }

        /// <summary>Cuántas veces por turno se puede pedir otra carta.</summary>
        public int CambiosPorTurno { get; set; } = 1;

        public bool IncluirSerie { get; set; } = true;
        public bool IncluirPropias { get; set; } = true;

        public VerdadRetoOptions Clone() => (VerdadRetoOptions)MemberwiseClone();
    }

    /// <summary>
    /// Partida de Verdad o Reto (V071): 2–20 jugadores con nombre (sin repetir, sin distinguir mayúsculas), al menos
    /// un nivel y las reglas del setup.
    /// </summary>
    public sealed class VerdadRetoConfig
    {
        public const int MinJugadores = 2;
        public const int MaxJugadores = 20;
        public const int PuntosVerdad = 1;
        public const int PuntosReto = 2;

        public static readonly IReadOnlyList<int> RondasOptions = new[] { 0, 3, 5, 10 };
        public static readonly IReadOnlyList<int> SegundosRetoOptions = new[] { 0, 30, 60, 90 };
        public static readonly IReadOnlyList<int> MaxVerdadesOptions = new[] { 0, 2, 3 };
        public static readonly IReadOnlyList<int> CambiosOptions = new[] { 0, 1, 2 };

        private VerdadRetoConfig(IReadOnlyList<string> jugadores, IReadOnlyCollection<PromptLevel> niveles, VerdadRetoOptions options)
        {
            Jugadores = jugadores;
            Niveles = niveles;
            Options = options;
        }

        public IReadOnlyList<string> Jugadores { get; }
        public IReadOnlyCollection<PromptLevel> Niveles { get; }

        /// <summary>Copia de las reglas (cambiar el borrador después no la toca).</summary>
        public VerdadRetoOptions Options { get; }

        public static (VerdadRetoConfig config, VerdadRetoConfigError? error) Create(IEnumerable<string> jugadores, IEnumerable<PromptLevel> niveles,
            VerdadRetoOptions options = null)
        {
            var names = (jugadores ?? Array.Empty<string>()).Select(NameNormalizer.Normalize).ToList();
            if (names.Count < MinJugadores) return (null, VerdadRetoConfigError.PocosJugadores);
            if (names.Count > MaxJugadores) return (null, VerdadRetoConfigError.DemasiadosJugadores);
            if (names.Any(n => n.Length == 0)) return (null, VerdadRetoConfigError.NombreVacio);
            if (names.Select(NameNormalizer.DuplicateKey).Distinct().Count() != names.Count) return (null, VerdadRetoConfigError.NombresRepetidos);
            var levels = new HashSet<PromptLevel>(niveles ?? Array.Empty<PromptLevel>());
            if (levels.Count == 0) return (null, VerdadRetoConfigError.SinNiveles);
            var copy = (options ?? new VerdadRetoOptions()).Clone();
            if (!copy.IncluirSerie && !copy.IncluirPropias) copy.IncluirSerie = true;
            copy.Rondas = Math.Max(0, copy.Rondas);
            copy.SegundosReto = Math.Max(0, copy.SegundosReto);
            copy.MaxVerdadesSeguidas = Math.Max(0, copy.MaxVerdadesSeguidas);
            copy.CambiosPorTurno = Math.Max(0, copy.CambiosPorTurno);
            return (new VerdadRetoConfig(names.AsReadOnly(), levels, copy), null);
        }
    }
}
