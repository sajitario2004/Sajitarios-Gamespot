using System.Collections.Generic;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Games.Impostor.Data;
using Gamespot.Games.Impostor.Domain;

namespace Gamespot.Games.Impostor
{
    /// <summary>Lo que se había escrito en el setup, para recuperarlo al volver de palabras o historial.</summary>
    public sealed class ImpostorSetupDraft
    {
        public List<string> Names { get; } = new List<string>();
        public int Impostores { get; set; } = 1;
        public int Rounds { get; set; } = GameConfig.MinRounds;

        /// <summary>Si se han tocado las rondas: hasta entonces siguen al máximo según los jugadores (I1).</summary>
        public bool RoundsTouched { get; set; }
        public bool HintEnabled { get; set; }

        /// <summary>Categorías marcadas (por defecto todas menos Cachondeo, +18). El setup solo enseña las que tienen palabras.</summary>
        public HashSet<string> Categories { get; } = new HashSet<string>(WordCategories.Default);
    }

    /// <summary>
    /// Estado del juego que comparten sus pantallas (el equivalente a los providers de Riverpod): el flow
    /// controller, los repositorios, el borrador del setup y las rutas.
    /// </summary>
    public sealed class ImpostorModule
    {
        public const string SetupRoute = "/impostor";
        public const string PassRoute = "/impostor/pass";
        public const string RevealRoute = "/impostor/reveal";
        public const string VotingRoute = "/impostor/voting";
        public const string GameOverRoute = "/impostor/game-over";
        public const string WordsRoute = "/impostor/words";
        public const string HistoryRoute = "/impostor/history";

        private GameSession _savedSession;

        public ImpostorModule(ImpostorFlowController flow, WordRepository words, GameHistoryRepository history, AudioService audio, Router router,
            CategoryWordSource source)
        {
            WordSource = source;
            Flow = flow;
            Words = words;
            History = history;
            Audio = audio;
            Router = router;
        }

        public ImpostorFlowController Flow { get; }
        public WordRepository Words { get; }
        public GameHistoryRepository History { get; }
        public AudioService Audio { get; }
        public Router Router { get; }
        public ImpostorSetupDraft Draft { get; } = new ImpostorSetupDraft();

        /// <summary>De dónde saca palabras la partida (con las categorías del setup).</summary>
        public CategoryWordSource WordSource { get; }

        /// <summary>
        /// Guarda la partida terminada en el historial una sola vez (si falla, se podrá reintentar). Las partidas
        /// abandonadas no se guardan.
        /// </summary>
        public bool SaveFinishedGameOnce()
        {
            var session = Flow.Session;
            if (session == null || Flow.Phase != ImpostorPhase.GameOver || ReferenceEquals(session, _savedSession)) return false;
            try
            {
                History.InsertFromSession(session, Flow.Config?.HintEnabled ?? false);
                _savedSession = session;
                return true;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"[Impostor] No se pudo guardar el historial: {e.Message}");
                return false;
            }
        }

        /// <summary>Reinicia la partida y vuelve al setup del juego.</summary>
        public void ResetToSetup()
        {
            Flow.Reiniciar();
            Router.Go(SetupRoute);
        }

        /// <summary>Reinicia la partida y vuelve al menú principal.</summary>
        public void ResetToMenu()
        {
            Flow.Reiniciar();
            Router.Go(Router.Home);
        }
    }
}
