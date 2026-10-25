using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.QuePrefieres.Domain
{
    /// <summary>Solo se leen las preguntas y se vota a mano, o además se apuntan los votos de cada opción.</summary>
    public enum QuePrefieresModo
    {
        SoloPreguntas,
        ContarVotos
    }

    public enum QuePrefieresOpcion
    {
        A,
        B
    }

    /// <summary>Borrador de las reglas del setup.</summary>
    public sealed class QuePrefieresOptions
    {
        public QuePrefieresModo Modo { get; set; } = QuePrefieresModo.SoloPreguntas;

        /// <summary>Tiempo para pensarlo antes de votar. 0 = sin cuenta atrás.</summary>
        public int SegundosPensar { get; set; }

        /// <summary>Contando votos, la minoría paga (bebe o prenda).</summary>
        public bool PagaMinoria { get; set; } = true;

        /// <summary>Preguntas de la partida. 0 = sin límite.</summary>
        public int Preguntas { get; set; }

        public bool IncluirSerie { get; set; } = true;
        public bool IncluirPropias { get; set; } = true;

        public QuePrefieresOptions Clone() => (QuePrefieresOptions)MemberwiseClone();
    }

    /// <summary>Partida de ¿Qué prefieres? (V073): al menos un nivel y las reglas del setup.</summary>
    public sealed class QuePrefieresConfig
    {
        public const int MaxVotos = 99;
        public static readonly IReadOnlyList<int> PensarOptions = new[] { 0, 5, 10 };
        public static readonly IReadOnlyList<int> PreguntasOptions = new[] { 0, 10, 20, 30 };

        private QuePrefieresConfig(IReadOnlyCollection<PromptLevel> niveles, QuePrefieresOptions options)
        {
            Niveles = niveles;
            Options = options;
        }

        public IReadOnlyCollection<PromptLevel> Niveles { get; }
        public QuePrefieresOptions Options { get; }

        /// <summary>Nulo sin niveles.</summary>
        public static QuePrefieresConfig Create(IEnumerable<PromptLevel> niveles, QuePrefieresOptions options = null)
        {
            var levels = new HashSet<PromptLevel>(niveles ?? Array.Empty<PromptLevel>());
            if (levels.Count == 0) return null;
            var copy = (options ?? new QuePrefieresOptions()).Clone();
            if (!copy.IncluirSerie && !copy.IncluirPropias) copy.IncluirSerie = true;
            copy.SegundosPensar = Math.Max(0, copy.SegundosPensar);
            copy.Preguntas = Math.Max(0, copy.Preguntas);
            return new QuePrefieresConfig(levels, copy);
        }
    }

    /// <summary>Una pregunta jugada con sus votos (cero y cero si no se contaron).</summary>
    public sealed class QuePrefieresRound
    {
        public QuePrefieresRound(PromptCard card, int votosA, int votosB)
        {
            Card = card;
            VotosA = votosA;
            VotosB = votosB;
        }

        public PromptCard Card { get; }
        public int VotosA { get; }
        public int VotosB { get; }
        public int Total => VotosA + VotosB;

        /// <summary>Porcentaje de A redondeado (B es el resto, para que sumen 100). Sin votos, 0.</summary>
        public int PorcentajeA => Total == 0 ? 0 : (int)Math.Round(100.0 * VotosA / Total, MidpointRounding.AwayFromZero);

        public int PorcentajeB => Total == 0 ? 0 : 100 - PorcentajeA;

        /// <summary>La opción con menos votos (nula con empate o sin votos).</summary>
        public QuePrefieresOpcion? Minoria => Total == 0 || VotosA == VotosB ? (QuePrefieresOpcion?)null
            : VotosA < VotosB ? QuePrefieresOpcion.A : QuePrefieresOpcion.B;

        /// <summary>Lo repartido que estuvo el grupo: 0 = unanimidad, 1 = mitad y mitad.</summary>
        public double Division => Total == 0 ? 0 : 1.0 - Math.Abs(VotosA - VotosB) / (double)Total;
    }

    public enum QuePrefieresFase
    {
        Setup,
        Pregunta,

        /// <summary>Contando votos: porcentajes de la pregunta.</summary>
        Resultado,

        Final,
        Error
    }

    /// <summary>
    /// Partida de ¿Qué prefieres? (V073). Preguntas sin repetir hasta agotarlas (una llamada al random por pregunta).
    /// Contando votos, se suman o se quitan votos a cada opción y «Ver resultado» enseña los porcentajes y quién es
    /// la minoría; si no, se pasa directamente a la siguiente. Con límite, se acaba tras la última.
    /// </summary>
    public sealed class QuePrefieresFlowController
    {
        private readonly IPromptSource _source;
        private readonly NoRepeatDeck _deck;
        private readonly List<QuePrefieresRound> _rounds = new List<QuePrefieresRound>();
        private IReadOnlyList<PromptCard> _pool = Array.Empty<PromptCard>();
        private bool _deckNotice;

        public QuePrefieresFlowController(IPromptSource source, IRandomProvider random)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _deck = new NoRepeatDeck(random);
        }

        public event Action Changed;

        public QuePrefieresFase Fase { get; private set; } = QuePrefieresFase.Setup;
        public QuePrefieresConfig Config { get; private set; }
        public PromptCard Current { get; private set; }
        public int Numero { get; private set; }
        public int VotosA { get; private set; }
        public int VotosB { get; private set; }
        public IReadOnlyList<QuePrefieresRound> Rondas => _rounds;

        /// <summary>La pregunta recién votada (en <see cref="QuePrefieresFase.Resultado"/>).</summary>
        public QuePrefieresRound Ultima => _rounds.Count > 0 ? _rounds[_rounds.Count - 1] : null;

        public bool ContarVotos => Config != null && Config.Options.Modo == QuePrefieresModo.ContarVotos;

        public bool UltimaPregunta => Config != null && Config.Options.Preguntas > 0 && Numero >= Config.Options.Preguntas;

        /// <summary>La más repartida de las votadas (la primera si hay varias iguales), o nula.</summary>
        public QuePrefieresRound MasDividida => _rounds.Where(r => r.Total > 0).OrderByDescending(r => r.Division).FirstOrDefault();

        /// <summary>La más unánime de las votadas, o nula.</summary>
        public QuePrefieresRound MasUnanime => _rounds.Where(r => r.Total > 0).OrderBy(r => r.Division).ThenByDescending(r => r.Total).FirstOrDefault();

        public void Iniciar(QuePrefieresConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _deck.Reset();
            _rounds.Clear();
            Numero = 0;
            Current = null;
            _deckNotice = false;
            try
            {
                _pool = (_source.GetCards(new PromptQuery
                {
                    Levels = config.Niveles,
                    IncludeSeed = config.Options.IncluirSerie,
                    IncludeOwn = config.Options.IncluirPropias
                }) ?? Array.Empty<PromptCard>()).Where(c => c.TextB != null).ToList();
            }
            catch (Exception)
            {
                _pool = Array.Empty<PromptCard>();
            }
            if (_pool.Count == 0)
            {
                Fase = QuePrefieresFase.Error;
                Changed?.Invoke();
                return;
            }
            Draw();
        }

        /// <summary>Suma (o quita, con <paramref name="delta"/> negativo) votos a una opción, sin bajar de 0.</summary>
        public void Votar(QuePrefieresOpcion opcion, int delta = 1)
        {
            if (Fase != QuePrefieresFase.Pregunta || !ContarVotos) return;
            if (opcion == QuePrefieresOpcion.A) VotosA = Clamp(VotosA + delta);
            else VotosB = Clamp(VotosB + delta);
            Changed?.Invoke();
        }

        /// <summary>Contando votos, a los porcentajes; si no, la siguiente pregunta (o el final).</summary>
        public void Revelar()
        {
            if (Fase != QuePrefieresFase.Pregunta) return;
            _rounds.Add(new QuePrefieresRound(Current, VotosA, VotosB));
            if (ContarVotos)
            {
                Fase = QuePrefieresFase.Resultado;
                Changed?.Invoke();
                return;
            }
            Next();
        }

        public void Siguiente()
        {
            if (Fase != QuePrefieresFase.Resultado) return;
            Next();
        }

        /// <summary>Cambia la pregunta sin jugarla (no cuenta para el límite).</summary>
        public void Saltar()
        {
            if (Fase != QuePrefieresFase.Pregunta) return;
            Numero--;
            Draw();
        }

        public void Terminar()
        {
            if (Fase != QuePrefieresFase.Pregunta && Fase != QuePrefieresFase.Resultado) return;
            Fase = QuePrefieresFase.Final;
            Changed?.Invoke();
        }

        public void Reiniciar()
        {
            Fase = QuePrefieresFase.Setup;
            Config = null;
            Current = null;
            Numero = 0;
            VotosA = VotosB = 0;
            _rounds.Clear();
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

        private static int Clamp(int votes) => Math.Max(0, Math.Min(QuePrefieresConfig.MaxVotos, votes));

        private void Next()
        {
            if (UltimaPregunta)
            {
                Fase = QuePrefieresFase.Final;
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
            VotosA = VotosB = 0;
            Fase = QuePrefieresFase.Pregunta;
            Changed?.Invoke();
        }
    }
}
