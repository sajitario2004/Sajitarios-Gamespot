using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.Mimica.Domain
{
    /// <summary>
    /// Categorías de las tarjetas de Mímica (V074). Famosos y Cachondeo (+18) usan los mismos ids que el Impostor y el
    /// Tabú (comparten nombre traducido); las propias del usuario son <c>custom_N</c> y «Mis palabras».
    /// </summary>
    public static class MimicaCategories
    {
        public const string Peliculas = "peliculas";
        public const string Acciones = "acciones";
        public const string Famosos = WordCategories.Famosos;
        public const string Animales = "animales";
        public const string Oficios = "oficios";
        public const string Objetos = "objetos";
        public const string Deportes = "deportes";
        public const string Cachondeo = WordCategories.Cachondeo;

        public static readonly IReadOnlyList<string> Seed = new[] { Peliculas, Acciones, Famosos, Animales, Oficios, Objetos, Deportes, Cachondeo };

        /// <summary>Las que van marcadas al abrir el setup: todas menos la +18.</summary>
        public static readonly IReadOnlyList<string> Default = Seed.Where(c => !WordCategories.IsAdult(c)).Concat(new[] { WordCategories.Propias })
            .ToList();
    }

    /// <summary>Solo mímica, o una mezcla en la que cada tarjeta dice cómo hay que hacerla.</summary>
    public enum MimicaModo
    {
        Mimica,
        Mezcla
    }

    /// <summary>Cómo hay que hacer la tarjeta en el modo mezcla (es la etiqueta de <see cref="GuessGameController"/>).</summary>
    public enum MimicaEstilo
    {
        Mimica,
        Sonidos,
        DibujarEnElAire,
        UnaPalabra
    }

    public enum MimicaConfigError
    {
        PocosEquipos,
        DemasiadosEquipos,
        NombreVacio,
        NombresRepetidos,
        SinCategorias
    }

    /// <summary>Borrador de las reglas del setup.</summary>
    public sealed class MimicaOptions
    {
        public int Segundos { get; set; } = 60;
        public int Objetivo { get; set; } = 10;

        /// <summary>Pases por turno (<see cref="GuessGameOptions.SinLimite"/> = los que se quiera).</summary>
        public int Pases { get; set; } = GuessGameOptions.SinLimite;

        public bool PenalizarPases { get; set; }
        public MimicaModo Modo { get; set; } = MimicaModo.Mimica;

        public MimicaOptions Clone() => (MimicaOptions)MemberwiseClone();
    }

    /// <summary>Partida de Mímica (V074): 2–4 equipos con nombre (sin repetir), al menos una categoría y las reglas.</summary>
    public sealed class MimicaConfig
    {
        public const int MinEquipos = 2;
        public const int MaxEquipos = 4;
        public static readonly IReadOnlyList<int> SegundosOptions = new[] { 30, 45, 60, 90, 120 };
        public static readonly IReadOnlyList<int> ObjetivoOptions = new[] { 5, 10, 15, 20, 30 };
        public static readonly IReadOnlyList<int> PasesOptions = new[] { GuessGameOptions.SinLimite, 0, 1, 3 };

        private MimicaConfig(IReadOnlyList<string> equipos, IReadOnlyCollection<string> categorias, MimicaOptions options)
        {
            Equipos = equipos;
            Categorias = categorias;
            Options = options;
        }

        public IReadOnlyList<string> Equipos { get; }
        public IReadOnlyCollection<string> Categorias { get; }
        public MimicaOptions Options { get; }

        /// <summary>Las tarjetas de la partida: las de las categorías elegidas, de serie y propias.</summary>
        public PromptQuery Query => new PromptQuery { Categories = Categorias };

        /// <summary>Las reglas para el controlador común (en mezcla, cada tarjeta lleva un estilo).</summary>
        public GuessGameOptions GuessOptions => new GuessGameOptions
        {
            Segundos = Options.Segundos,
            Pases = Options.Pases,
            PenalizarPases = Options.PenalizarPases,
            Objetivo = Options.Objetivo,
            Etiquetas = Options.Modo == MimicaModo.Mezcla ? Enum.GetValues(typeof(MimicaEstilo)).Length : 0
        };

        public static (MimicaConfig config, MimicaConfigError? error) Create(IEnumerable<string> equipos, IEnumerable<string> categorias,
            MimicaOptions options = null)
        {
            var names = (equipos ?? Array.Empty<string>()).Select(NameNormalizer.Normalize).ToList();
            if (names.Count < MinEquipos) return (null, MimicaConfigError.PocosEquipos);
            if (names.Count > MaxEquipos) return (null, MimicaConfigError.DemasiadosEquipos);
            if (names.Any(n => n.Length == 0)) return (null, MimicaConfigError.NombreVacio);
            if (names.Select(NameNormalizer.DuplicateKey).Distinct().Count() != names.Count) return (null, MimicaConfigError.NombresRepetidos);
            var categories = new HashSet<string>(categorias ?? Array.Empty<string>());
            if (categories.Count == 0) return (null, MimicaConfigError.SinCategorias);
            var copy = (options ?? new MimicaOptions()).Clone();
            copy.Segundos = Math.Max(5, copy.Segundos);
            copy.Objetivo = Math.Max(1, copy.Objetivo);
            if (copy.Pases < GuessGameOptions.SinLimite) copy.Pases = GuessGameOptions.SinLimite;
            return (new MimicaConfig(names.AsReadOnly(), categories, copy), null);
        }
    }
}
