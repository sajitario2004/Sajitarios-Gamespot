using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.QuienSoy.Domain
{
    /// <summary>Categorías de ¿Quién soy? (V075). Famosos, Sitios y Cachondeo (+18) comparten id con el Impostor.</summary>
    public static class QuienSoyCategories
    {
        public const string Famosos = WordCategories.Famosos;
        public const string Personajes = "personajes";
        public const string Animales = "animales";
        public const string Oficios = "oficios";
        public const string Objetos = "objetos";
        public const string Sitios = WordCategories.Sitios;
        public const string Comida = "comida";
        public const string Cachondeo = WordCategories.Cachondeo;

        public static readonly IReadOnlyList<string> Seed = new[] { Famosos, Personajes, Animales, Oficios, Objetos, Sitios, Comida, Cachondeo };

        public static readonly IReadOnlyList<string> Default = Seed.Where(c => !WordCategories.IsAdult(c)).Concat(new[] { WordCategories.Propias })
            .ToList();
    }

    /// <summary>Por equipos (uno del equipo adivina, los suyos describen) o cada uno a lo suyo (describen todos los demás).</summary>
    public enum QuienSoyModo
    {
        PorEquipos,
        CadaUno
    }

    public enum QuienSoyConfigError
    {
        PocosNombres,
        DemasiadosNombres,
        NombreVacio,
        NombresRepetidos,
        SinCategorias
    }

    /// <summary>Borrador de las reglas del setup.</summary>
    public sealed class QuienSoyOptions
    {
        public QuienSoyModo Modo { get; set; } = QuienSoyModo.PorEquipos;
        public int Segundos { get; set; } = 60;
        public int Rondas { get; set; } = 3;
        public int Pases { get; set; } = GuessGameOptions.SinLimite;
        public bool PenalizarPases { get; set; }

        /// <summary>Acertar y pasar inclinando el móvil (si no, solo con los botones).</summary>
        public bool Inclinar { get; set; } = true;

        /// <summary>Cambia arriba por abajo (por si en algún móvil va al revés).</summary>
        public bool InvertirInclinacion { get; set; }

        public QuienSoyOptions Clone() => (QuienSoyOptions)MemberwiseClone();
    }

    /// <summary>
    /// Partida de ¿Quién soy? (V075): 2–4 equipos o 2–12 jugadores con nombre (sin repetir), al menos una categoría y
    /// las reglas. Se juega por rondas: gana quien más aciertos tenga al acabar la última (con empate, desempate).
    /// </summary>
    public sealed class QuienSoyConfig
    {
        public const int MinNombres = 2;
        public const int MaxEquipos = 4;
        public const int MaxJugadores = 12;
        public static readonly IReadOnlyList<int> SegundosOptions = new[] { 30, 45, 60, 90, 120 };
        public static readonly IReadOnlyList<int> RondasOptions = new[] { 1, 2, 3, 5 };
        public static readonly IReadOnlyList<int> PasesOptions = new[] { GuessGameOptions.SinLimite, 0, 1, 3 };

        private QuienSoyConfig(IReadOnlyList<string> nombres, IReadOnlyCollection<string> categorias, QuienSoyOptions options)
        {
            Nombres = nombres;
            Categorias = categorias;
            Options = options;
        }

        /// <summary>Los equipos o los jugadores, en el orden en que juegan.</summary>
        public IReadOnlyList<string> Nombres { get; }

        public IReadOnlyCollection<string> Categorias { get; }
        public QuienSoyOptions Options { get; }

        public PromptQuery Query => new PromptQuery { Categories = Categorias };

        public GuessGameOptions GuessOptions => new GuessGameOptions
        {
            Segundos = Options.Segundos,
            Pases = Options.Pases,
            PenalizarPases = Options.PenalizarPases,
            Rondas = Options.Rondas
        };

        public static int MaxNombresPara(QuienSoyModo modo) => modo == QuienSoyModo.PorEquipos ? MaxEquipos : MaxJugadores;

        public static (QuienSoyConfig config, QuienSoyConfigError? error) Create(IEnumerable<string> nombres, IEnumerable<string> categorias,
            QuienSoyOptions options = null)
        {
            var copy = (options ?? new QuienSoyOptions()).Clone();
            var names = (nombres ?? Array.Empty<string>()).Select(NameNormalizer.Normalize).ToList();
            if (names.Count < MinNombres) return (null, QuienSoyConfigError.PocosNombres);
            if (names.Count > MaxNombresPara(copy.Modo)) return (null, QuienSoyConfigError.DemasiadosNombres);
            if (names.Any(n => n.Length == 0)) return (null, QuienSoyConfigError.NombreVacio);
            if (names.Select(NameNormalizer.DuplicateKey).Distinct().Count() != names.Count) return (null, QuienSoyConfigError.NombresRepetidos);
            var categories = new HashSet<string>(categorias ?? Array.Empty<string>());
            if (categories.Count == 0) return (null, QuienSoyConfigError.SinCategorias);
            copy.Segundos = Math.Max(5, copy.Segundos);
            copy.Rondas = Math.Max(1, copy.Rondas);
            if (copy.Pases < GuessGameOptions.SinLimite) copy.Pases = GuessGameOptions.SinLimite;
            return (new QuienSoyConfig(names.AsReadOnly(), categories, copy), null);
        }
    }

    public enum TiltGesture
    {
        Ninguno,

        /// <summary>La pantalla mira al suelo: acierto.</summary>
        Abajo,

        /// <summary>La pantalla mira al techo: pasar.</summary>
        Arriba
    }

    /// <summary>
    /// Detecta las inclinaciones del móvil en la frente (V075) con el eje z del acelerómetro (sale de la pantalla): con
    /// el móvil vertical en la frente z ≈ 0; mirando al suelo, z → +1 (acierto); mirando al techo, z → −1 (pasar).
    /// Se mide respecto a la postura del inicio del turno (<see cref="Calibrar"/>) y, tras un gesto, hay que volver
    /// cerca de esa postura antes del siguiente.
    /// </summary>
    public sealed class TiltDetector
    {
        /// <summary>Cuánto hay que inclinar (en g) para que cuente.</summary>
        public const float Umbral = 0.6f;

        /// <summary>Lo cerca de la postura inicial que hay que volver para el siguiente gesto.</summary>
        public const float Rearme = 0.3f;

        /// <summary>Si al calibrar el móvil no está casi vertical, se toma la vertical (z = 0) como referencia.</summary>
        public const float MaxBase = 0.5f;

        private bool _armed = true;
        private bool _calibrated;

        public bool Invertir { get; set; }
        public float Base { get; private set; }

        public void Calibrar(float z)
        {
            Base = Math.Abs(z) <= MaxBase ? z : 0f;
            _armed = true;
            _calibrated = true;
        }

        public TiltGesture Leer(float z)
        {
            if (!_calibrated) Calibrar(z);
            var delta = z - Base;
            if (Invertir) delta = -delta;
            if (!_armed)
            {
                if (Math.Abs(delta) < Rearme) _armed = true;
                return TiltGesture.Ninguno;
            }
            if (delta > Umbral)
            {
                _armed = false;
                return TiltGesture.Abajo;
            }
            if (delta < -Umbral)
            {
                _armed = false;
                return TiltGesture.Arriba;
            }
            return TiltGesture.Ninguno;
        }

        /// <summary>
        /// Hacia dónde girar el texto para que se lea con el móvil en horizontal, con el eje x: si el lado derecho (en
        /// vertical) queda abajo, x → +1 y se gira +90°; si queda arriba, −90°. Cerca de 0 no cambia.
        /// </summary>
        public static float Giro(float x, float actual) => x > 0.5f ? 90f : x < -0.5f ? -90f : actual;
    }
}
