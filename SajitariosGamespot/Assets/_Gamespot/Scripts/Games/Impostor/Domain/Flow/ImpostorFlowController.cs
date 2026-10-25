using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.Impostor.Domain
{
    public enum ImpostorPhase
    {
        Setup,
        Iniciando,
        Pass,
        Reveal,
        Voting,
        GameOver,
        Error
    }

    /// <summary>Desenlace de la votación.</summary>
    public enum VotingOutcome
    {
        JugadoresGanan,
        ImpostorGana
    }

    /// <summary>Resultado del último voto (para el aviso de la pantalla de votación).</summary>
    public enum LastVote
    {
        Ninguno,
        EraImpostor,
        NoEraImpostor
    }

    public enum ImpostorErrorKind
    {
        SinPalabras,
        Desconocido
    }

    /// <summary>De dónde salen las palabras de la partida (el repositorio, en la app).</summary>
    public interface IImpostorWordSource
    {
        IReadOnlyList<Word> GetAllWords();
    }

    /// <summary>
    /// Estado de una partida del Impostor (port de <c>ImpostorFlowController</c> de Riverpod).
    /// Fases: Setup → Pass → Reveal (× jugador) → Voting → GameOver. Sin textos de UI.
    /// Cada acción en una fase que no le toca no hace nada.
    /// </summary>
    public sealed class ImpostorFlowController
    {
        private readonly IImpostorWordSource _words;
        private readonly AssignRolesUseCase _assignRoles;
        private readonly HashSet<Player> _eliminados = new HashSet<Player>();

        public ImpostorFlowController(IImpostorWordSource words, IRandomProvider random)
        {
            _words = words ?? throw new ArgumentNullException(nameof(words));
            _assignRoles = new AssignRolesUseCase(random);
        }

        /// <summary>Se lanza tras cada cambio de estado.</summary>
        public event Action Changed;

        public ImpostorPhase Phase { get; private set; } = ImpostorPhase.Setup;
        public GameConfig Config { get; private set; }
        public GameSession Session { get; private set; }
        public int CurrentIndex { get; private set; }
        public ImpostorErrorKind? ErrorKind { get; private set; }
        public int RondaActual { get; private set; }
        public int RondasTotales { get; private set; }
        public VotingOutcome? Outcome { get; private set; }
        public LastVote LastVote { get; private set; } = LastVote.Ninguno;

        /// <summary>Impostores ya pillados en la votación.</summary>
        public IReadOnlyCollection<Player> Eliminados => _eliminados;

        public int TotalPlayers => Session?.RevealOrder.Count ?? 0;
        public bool EsUltimoJugador => Session != null && CurrentIndex >= TotalPlayers - 1;

        /// <summary>Jugador al que le toca revelar, o nulo.</summary>
        public Player JugadorActual =>
            Session != null && CurrentIndex >= 0 && CurrentIndex < Session.RevealOrder.Count ? Session.RevealOrder[CurrentIndex] : null;

        /// <summary>Votables: orden de revelación menos los impostores pillados.</summary>
        public IReadOnlyList<Player> Candidatos =>
            Session == null ? (IReadOnlyList<Player>)Array.Empty<Player>() : Session.RevealOrder.Where(p => !_eliminados.Contains(p)).ToList();

        public IReadOnlyList<Player> ImpostoresVivos =>
            Session == null ? (IReadOnlyList<Player>)Array.Empty<Player>() : Session.Impostores.Where(p => !_eliminados.Contains(p)).ToList();

        /// <summary>Reparte roles con una palabra al azar y pasa a la primera revelación.</summary>
        public void Iniciar(GameConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            ResetState();
            Phase = ImpostorPhase.Iniciando;
            Config = config;
            Notify();

            try
            {
                var words = _words.GetAllWords();
                if (words == null || words.Count == 0)
                {
                    Fail(ImpostorErrorKind.SinPalabras);
                    return;
                }
                Session = _assignRoles.Execute(config, words);
                Phase = ImpostorPhase.Pass;
            }
            catch (Exception)
            {
                Fail(ImpostorErrorKind.Desconocido);
                return;
            }
            Notify();
        }

        /// <summary>El jugador actual pulsa "Continuar" y ve el botón de revelar.</summary>
        public void Revelar()
        {
            if (Session == null) return;
            Phase = ImpostorPhase.Reveal;
            Notify();
        }

        /// <summary>
        /// "Ocultar y pasar": siguiente jugador o, tras el último, empieza la votación.
        /// Devuelve verdadero si empieza la votación.
        /// </summary>
        public bool Avanzar()
        {
            if (Session == null) return false;
            if (EsUltimoJugador)
            {
                Phase = ImpostorPhase.Voting;
                RondaActual = 1;
                RondasTotales = Config?.Rounds ?? 1;
                _eliminados.Clear();
                LastVote = LastVote.Ninguno;
                Notify();
                return true;
            }
            Phase = ImpostorPhase.Pass;
            CurrentIndex++;
            Notify();
            return false;
        }

        /// <summary>
        /// El grupo expulsa a <paramref name="player"/>. Si era impostor sale de los candidatos; los inocentes
        /// se quedan. Cada voto gasta una ronda. Ganan los jugadores al pillar a todos los impostores;
        /// gana el impostor si se acaban las rondas.
        /// </summary>
        public void Votar(Player player)
        {
            if (Session == null || Phase != ImpostorPhase.Voting || player == null || _eliminados.Contains(player)) return;

            var acierto = Session.IsImpostor(player);
            if (acierto) _eliminados.Add(player);
            var quedanImpostores = Session.Impostores.Any(i => !_eliminados.Contains(i));

            if (acierto && !quedanImpostores)
            {
                Phase = ImpostorPhase.GameOver;
                Outcome = VotingOutcome.JugadoresGanan;
                LastVote = LastVote.EraImpostor;
                Notify();
                return;
            }

            var feedback = acierto ? LastVote.EraImpostor : LastVote.NoEraImpostor;
            if (RondaActual >= RondasTotales)
            {
                Phase = ImpostorPhase.GameOver;
                Outcome = VotingOutcome.ImpostorGana;
                LastVote = feedback;
                Notify();
                return;
            }

            RondaActual++;
            LastVote = feedback;
            Notify();
        }

        /// <summary>Vuelve al estado inicial (setup sin partida).</summary>
        public void Reiniciar()
        {
            ResetState();
            Notify();
        }

        private void Fail(ImpostorErrorKind kind)
        {
            Phase = ImpostorPhase.Error;
            Session = null;
            ErrorKind = kind;
            Notify();
        }

        private void ResetState()
        {
            Phase = ImpostorPhase.Setup;
            Config = null;
            Session = null;
            CurrentIndex = 0;
            ErrorKind = null;
            _eliminados.Clear();
            RondaActual = 0;
            RondasTotales = 0;
            Outcome = null;
            LastVote = LastVote.Ninguno;
        }

        private void Notify() => Changed?.Invoke();
    }
}
