using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Gamespot.Core;

namespace Gamespot.Games.MasProbable.Domain
{
    /// <summary>Cómo se juega: todos señalan a la vez (el móvil solo saca frases) o, además, se apunta al más señalado.</summary>
    public enum MasProbableModo
    {
        Senalar,
        ConCuenta
    }

    public enum MasProbableConfigError
    {
        SinNiveles,
        PocosJugadores,
        DemasiadosJugadores,
        NombreVacio,
        NombresRepetidos
    }

    /// <summary>Borrador de las reglas del setup.</summary>
    public sealed class MasProbableOptions
    {
        public MasProbableModo Modo { get; set; } = MasProbableModo.Senalar;

        /// <summary>Cuenta atrás antes de señalar. 0 = sin cuenta atrás.</summary>
        public int SegundosCuenta { get; set; } = 3;

        /// <summary>Frases de la partida. 0 = sin límite.</summary>
        public int Frases { get; set; }

        public bool IncluirSerie { get; set; } = true;
        public bool IncluirPropias { get; set; } = true;

        public MasProbableOptions Clone() => (MasProbableOptions)MemberwiseClone();
    }

    /// <summary>
    /// Partida de ¿Quién es más probable que…? (V072): al menos un nivel; con cuenta, 3–20 jugadores con nombre (sin
    /// repetir). Sin cuenta no hacen falta nombres.
    /// </summary>
    public sealed class MasProbableConfig
    {
        public const int MinJugadores = 3;
        public const int MaxJugadores = 20;
        public static readonly IReadOnlyList<int> CuentaOptions = new[] { 0, 3, 5 };
        public static readonly IReadOnlyList<int> FrasesOptions = new[] { 0, 10, 20, 30 };

        private MasProbableConfig(IReadOnlyCollection<PromptLevel> niveles, IReadOnlyList<string> jugadores, MasProbableOptions options)
        {
            Niveles = niveles;
            Jugadores = jugadores;
            Options = options;
        }

        public IReadOnlyCollection<PromptLevel> Niveles { get; }

        /// <summary>Solo con <see cref="MasProbableModo.ConCuenta"/>; si no, vacía.</summary>
        public IReadOnlyList<string> Jugadores { get; }

        public MasProbableOptions Options { get; }

        public static (MasProbableConfig config, MasProbableConfigError? error) Create(IEnumerable<PromptLevel> niveles, IEnumerable<string> jugadores,
            MasProbableOptions options = null)
        {
            var levels = new HashSet<PromptLevel>(niveles ?? Array.Empty<PromptLevel>());
            if (levels.Count == 0) return (null, MasProbableConfigError.SinNiveles);
            var copy = (options ?? new MasProbableOptions()).Clone();
            if (!copy.IncluirSerie && !copy.IncluirPropias) copy.IncluirSerie = true;
            copy.SegundosCuenta = Math.Max(0, copy.SegundosCuenta);
            copy.Frases = Math.Max(0, copy.Frases);
            var names = new List<string>();
            if (copy.Modo == MasProbableModo.ConCuenta)
            {
                names = (jugadores ?? Array.Empty<string>()).Select(NameNormalizer.Normalize).ToList();
                if (names.Count < MinJugadores) return (null, MasProbableConfigError.PocosJugadores);
                if (names.Count > MaxJugadores) return (null, MasProbableConfigError.DemasiadosJugadores);
                if (names.Any(n => n.Length == 0)) return (null, MasProbableConfigError.NombreVacio);
                if (names.Select(NameNormalizer.DuplicateKey).Distinct().Count() != names.Count) return (null, MasProbableConfigError.NombresRepetidos);
            }
            return (new MasProbableConfig(levels, names.AsReadOnly(), copy), null);
        }
    }

    /// <summary>
    /// Texto de las frases (V072): se guarda solo lo que va detrás de «¿Quién es más probable que…»; si el usuario lo
    /// escribe entero, se quita (con o sin tildes ni signos) y también la interrogación final.
    /// </summary>
    public static class MasProbableText
    {
        public const int MaxLength = 120;

        private const string Prefix = "quien es mas probable que";

        public static string Normalize(string text)
        {
            var clean = NameNormalizer.Normalize(text ?? string.Empty).TrimStart('¿', ' ').TrimEnd('?', ' ', '.');
            var plain = RemoveAccents(clean).ToLowerInvariant();
            if (plain.StartsWith(Prefix, StringComparison.Ordinal)) clean = clean.Substring(Prefix.Length).TrimStart(' ', '.', '…', ',');
            clean = clean.TrimStart('.', '…', ' ');
            if (clean.Length > MaxLength) clean = clean.Substring(0, MaxLength).TrimEnd();
            return clean;
        }

        private static string RemoveAccents(string text)
        {
            var sb = new StringBuilder(text.Length);
            foreach (var c in text.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }

    public enum MasProbableFase
    {
        Setup,

        /// <summary>La frase en pantalla: cuenta atrás y todos señalan.</summary>
        Frase,

        /// <summary>Con cuenta: se toca al más señalado (o a los empatados).</summary>
        Apuntar,

        Final,
        Error
    }

    /// <summary>Una frase jugada y a quién señalaron más (con cuenta).</summary>
    public sealed class MasProbableRound
    {
        public MasProbableRound(PromptCard card, IReadOnlyList<int> winners)
        {
            Card = card;
            Winners = winners;
        }

        public PromptCard Card { get; }
        public IReadOnlyList<int> Winners { get; }
    }

    /// <summary>
    /// Partida de ¿Quién es más probable que…? (V072). Frases sin repetir hasta agotarlas (una llamada al random por
    /// frase, <see cref="NoRepeatDeck"/>). Con cuenta, tras señalar se apunta al más señalado: cada uno suma un
    /// «título»; los empatados suman todos. Con límite de frases, se acaba al jugar la última.
    /// </summary>
    public sealed class MasProbableFlowController
    {
        private readonly IPromptSource _source;
        private readonly NoRepeatDeck _deck;
        private readonly List<MasProbableRound> _rounds = new List<MasProbableRound>();
        private IReadOnlyList<PromptCard> _pool = Array.Empty<PromptCard>();
        private int[] _titles = Array.Empty<int>();
        private bool _deckNotice;

        public MasProbableFlowController(IPromptSource source, IRandomProvider random)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _deck = new NoRepeatDeck(random);
        }

        public event Action Changed;

        public MasProbableFase Fase { get; private set; } = MasProbableFase.Setup;
        public MasProbableConfig Config { get; private set; }
        public PromptCard Current { get; private set; }

        /// <summary>Número de la frase en curso (desde 1).</summary>
        public int Numero { get; private set; }

        public IReadOnlyList<MasProbableRound> Rondas => _rounds;

        /// <summary>Títulos de cada jugador (con cuenta), en el orden de los nombres.</summary>
        public IReadOnlyList<int> Titulos => _titles;

        public bool ConCuenta => Config != null && Config.Options.Modo == MasProbableModo.ConCuenta;

        /// <summary>Si la frase en curso es la última (con límite de frases).</summary>
        public bool UltimaFrase => Config != null && Config.Options.Frases > 0 && Numero >= Config.Options.Frases;

        /// <summary>Jugadores por títulos (más primero; a igualdad, el orden de los nombres): (índice, títulos).</summary>
        public IReadOnlyList<(int index, int titulos)> Ranking =>
            _titles.Select((t, i) => (i, t)).OrderByDescending(x => x.t).ThenBy(x => x.i).ToList();

        public void Iniciar(MasProbableConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _deck.Reset();
            _rounds.Clear();
            _titles = new int[config.Jugadores.Count];
            Numero = 0;
            Current = null;
            _deckNotice = false;
            try
            {
                _pool = _source.GetCards(new PromptQuery
                {
                    Levels = config.Niveles,
                    IncludeSeed = config.Options.IncluirSerie,
                    IncludeOwn = config.Options.IncluirPropias
                }) ?? Array.Empty<PromptCard>();
            }
            catch (Exception)
            {
                _pool = Array.Empty<PromptCard>();
            }
            if (_pool.Count == 0)
            {
                Fase = MasProbableFase.Error;
                Changed?.Invoke();
                return;
            }
            Draw();
        }

        /// <summary>Ya han señalado: con cuenta, a apuntar; sin cuenta, la siguiente frase (o el final).</summary>
        public void Senalado()
        {
            if (Fase != MasProbableFase.Frase) return;
            if (ConCuenta)
            {
                Fase = MasProbableFase.Apuntar;
                Changed?.Invoke();
                return;
            }
            _rounds.Add(new MasProbableRound(Current, Array.Empty<int>()));
            Next();
        }

        /// <summary>Apunta a los más señalados (vacío = nadie, p. ej. si no hubo acuerdo).</summary>
        public void Apuntar(IEnumerable<int> masSenalados)
        {
            if (Fase != MasProbableFase.Apuntar) return;
            var winners = (masSenalados ?? Array.Empty<int>()).Where(i => i >= 0 && i < _titles.Length).Distinct().OrderBy(i => i).ToList();
            foreach (var i in winners) _titles[i]++;
            _rounds.Add(new MasProbableRound(Current, winners));
            Next();
        }

        /// <summary>Salta la frase sin jugarla (no cuenta para el límite).</summary>
        public void Saltar()
        {
            if (Fase != MasProbableFase.Frase) return;
            Numero--;
            Draw();
        }

        public void Terminar()
        {
            if (Fase != MasProbableFase.Frase && Fase != MasProbableFase.Apuntar) return;
            Fase = MasProbableFase.Final;
            Changed?.Invoke();
        }

        public void Reiniciar()
        {
            Fase = MasProbableFase.Setup;
            Config = null;
            Current = null;
            Numero = 0;
            _rounds.Clear();
            _titles = Array.Empty<int>();
            _pool = Array.Empty<PromptCard>();
            _deck.Reset();
            Changed?.Invoke();
        }

        public bool ConsumirAvisoMazo()
        {
            var notice = _deckNotice;
            _deckNotice = false;
            return notice;
        }

        private void Next()
        {
            if (UltimaFrase)
            {
                Fase = MasProbableFase.Final;
                Changed?.Invoke();
                return;
            }
            Draw();
        }

        private void Draw()
        {
            Current = _deck.Draw(_pool);
            if (_deck.Reshuffled) _deckNotice = true;
            Numero++;
            Fase = MasProbableFase.Frase;
            Changed?.Invoke();
        }
    }
}
