using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamespot.Core
{
    /// <summary>Reglas comunes de los juegos de adivinar por turnos (V074): Mímica y ¿Quién soy?</summary>
    public sealed class GuessGameOptions
    {
        public const int SinLimite = -1;

        /// <summary>Segundos de cada turno.</summary>
        public int Segundos { get; set; } = 60;

        /// <summary>Pases por turno (<see cref="SinLimite"/> = los que se quiera).</summary>
        public int Pases { get; set; } = SinLimite;

        /// <summary>Cada pase resta un punto.</summary>
        public bool PenalizarPases { get; set; }

        /// <summary>Puntos para ganar (0 = sin objetivo).</summary>
        public int Objetivo { get; set; }

        /// <summary>Rondas de la partida (0 = sin límite).</summary>
        public int Rondas { get; set; }

        /// <summary>
        /// Si es mayor que 0, cada carta lleva además una etiqueta al azar entre 0 y <c>Etiquetas − 1</c> (el estilo de la
        /// Mímica en modo mezcla).
        /// </summary>
        public int Etiquetas { get; set; }

        public GuessGameOptions Clone() => (GuessGameOptions)MemberwiseClone();
    }

    /// <summary>Una carta jugada en un turno: si se acertó (se puede corregir en el resumen) y su etiqueta.</summary>
    public sealed class GuessEntry
    {
        public GuessEntry(PromptCard card, bool acertada, int etiqueta)
        {
            Card = card;
            Acertada = acertada;
            Etiqueta = etiqueta;
        }

        public PromptCard Card { get; }
        public bool Acertada { get; internal set; }
        public int Etiqueta { get; }
    }

    public enum GuessFase
    {
        Setup,

        /// <summary>Antes del turno: se pasa el móvil al equipo que toca.</summary>
        Preparado,

        Turno,

        /// <summary>Se acabó el tiempo: se repasan (y corrigen) las cartas del turno.</summary>
        Resumen,

        Final,
        Error
    }

    /// <summary>
    /// Partida de adivinar por equipos (V074): cartas sin repetir de las categorías elegidas, turnos con acierto (+1) y
    /// pase (limitado o no, y que puede restar), resumen corregible y el marcador de <see cref="TeamMatch"/>. Orden del
    /// random: cada carta, la llamada del mazo y, con etiquetas, <c>NextInt(Etiquetas)</c>.
    /// </summary>
    public sealed class GuessGameController
    {
        private readonly IPromptSource _source;
        private readonly IRandomProvider _random;
        private readonly NoRepeatDeck _deck;
        private readonly List<GuessEntry> _log = new List<GuessEntry>();
        private IReadOnlyList<PromptCard> _pool = Array.Empty<PromptCard>();
        private bool _deckNotice;

        public GuessGameController(IPromptSource source, IRandomProvider random)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _deck = new NoRepeatDeck(random);
        }

        public event Action Changed;

        public GuessFase Fase { get; private set; } = GuessFase.Setup;
        public GuessGameOptions Options { get; private set; }
        public TeamMatch Match { get; private set; }
        public PromptCard Current { get; private set; }

        /// <summary>Etiqueta de la carta en curso (−1 sin etiquetas).</summary>
        public int CurrentTag { get; private set; } = -1;

        /// <summary>Pases que quedan en el turno (<see cref="GuessGameOptions.SinLimite"/> = sin límite).</summary>
        public int PasesRestantes { get; private set; }

        /// <summary>Cartas del turno en curso o recién acabado.</summary>
        public IReadOnlyList<GuessEntry> Turno => _log;

        public int Aciertos => _log.Count(e => e.Acertada);
        public int Fallos => _log.Count(e => !e.Acertada);

        /// <summary>Lo que suma el turno: aciertos menos pases si se penalizan (el total no baja de 0).</summary>
        public int PuntosTurno => Aciertos - (Options != null && Options.PenalizarPases ? Fallos : 0);

        public bool PuedePasar => Fase == GuessFase.Turno && PasesRestantes != 0;

        /// <summary>Total de cartas jugadas en la partida.</summary>
        public int Jugadas { get; private set; }

        public void Iniciar(IReadOnlyList<string> equipos, PromptQuery query, GuessGameOptions options)
        {
            Options = (options ?? new GuessGameOptions()).Clone();
            Match = new TeamMatch(equipos, Options.Objetivo, Options.Rondas);
            _deck.Reset();
            _log.Clear();
            Current = null;
            CurrentTag = -1;
            Jugadas = 0;
            _deckNotice = false;
            try
            {
                _pool = _source.GetCards(query ?? new PromptQuery()) ?? Array.Empty<PromptCard>();
            }
            catch (Exception)
            {
                _pool = Array.Empty<PromptCard>();
            }
            Fase = _pool.Count == 0 ? GuessFase.Error : GuessFase.Preparado;
            Changed?.Invoke();
        }

        public void EmpezarTurno()
        {
            if (Fase != GuessFase.Preparado) return;
            _log.Clear();
            PasesRestantes = Options.Pases;
            Fase = GuessFase.Turno;
            Draw();
            Changed?.Invoke();
        }

        public void Acierto()
        {
            if (Fase != GuessFase.Turno) return;
            _log.Add(new GuessEntry(Current, true, CurrentTag));
            Jugadas++;
            Draw();
            Changed?.Invoke();
        }

        public void Pasar()
        {
            if (!PuedePasar) return;
            if (PasesRestantes > 0) PasesRestantes--;
            _log.Add(new GuessEntry(Current, false, CurrentTag));
            Jugadas++;
            Draw();
            Changed?.Invoke();
        }

        /// <summary>Se acaba el tiempo: la carta que estaba en juego no cuenta.</summary>
        public void TerminarTurno()
        {
            if (Fase != GuessFase.Turno) return;
            Current = null;
            CurrentTag = -1;
            Fase = GuessFase.Resumen;
            Changed?.Invoke();
        }

        /// <summary>En el resumen, cambia una carta de acertada a fallada o al revés.</summary>
        public void Corregir(int index)
        {
            if (Fase != GuessFase.Resumen || index < 0 || index >= _log.Count) return;
            _log[index].Acertada = !_log[index].Acertada;
            Changed?.Invoke();
        }

        /// <summary>Apunta los puntos del turno y pasa al siguiente equipo (o al final).</summary>
        public void ConfirmarTurno()
        {
            if (Fase != GuessFase.Resumen) return;
            Match.CerrarTurno(PuntosTurno);
            Fase = Match.Terminada ? GuessFase.Final : GuessFase.Preparado;
            Changed?.Invoke();
        }

        public void Terminar()
        {
            if (Match == null || Fase == GuessFase.Final || Fase == GuessFase.Error || Fase == GuessFase.Setup) return;
            Match.Terminar();
            Fase = GuessFase.Final;
            Changed?.Invoke();
        }

        public void Reiniciar()
        {
            Fase = GuessFase.Setup;
            Options = null;
            Match = null;
            Current = null;
            CurrentTag = -1;
            _log.Clear();
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

        private void Draw()
        {
            Current = _deck.Draw(_pool);
            if (_deck.Reshuffled) _deckNotice = true;
            CurrentTag = Options.Etiquetas > 0 ? _random.NextInt(Options.Etiquetas) : -1;
        }
    }
}
