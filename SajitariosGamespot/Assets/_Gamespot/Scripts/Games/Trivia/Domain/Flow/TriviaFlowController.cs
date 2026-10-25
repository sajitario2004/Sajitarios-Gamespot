using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.Trivia.Domain
{
    public enum TriviaFase
    {
        Setup,
        Iniciando,
        Pass,
        Question,
        GameOver,
        Error
    }

    public enum TriviaErrorKind
    {
        SinPreguntas
    }

    /// <summary>De dónde salen las preguntas (el repositorio, en la app).</summary>
    public interface ITriviaQuestionSource
    {
        IReadOnlyList<Question> GetPool(IReadOnlyCollection<string> tematicaIds, Difficulty difficulty);
    }

    /// <summary>Dónde se suman las victorias por nombre (el repositorio, en la app).</summary>
    public interface ITriviaWinnerSink
    {
        void IncrementWins(string name);
    }

    /// <summary>
    /// Partida de trivia por rondas (port de <c>TriviaFlowController</c>). En cada ronda cada jugador vivo recibe
    /// una pregunta distinta del tramo de dificultad y responde en privado (pasando el móvil); quien falla queda
    /// eliminado. Al acabar la ronda se recargan los pools y se reparte la siguiente. Al terminar, +1 victoria a
    /// cada superviviente. Cualquier fallo al cargar preguntas lleva a <see cref="TriviaErrorKind.SinPreguntas"/>.
    /// </summary>
    public sealed class TriviaFlowController
    {
        private readonly ITriviaQuestionSource _questions;
        private readonly ITriviaWinnerSink _winners;
        private readonly DealQuestionsUseCase _deal;
        private IReadOnlyList<(TriviaPlayer player, Question question)> _roundQuestions = Array.Empty<(TriviaPlayer, Question)>();

        public TriviaFlowController(ITriviaQuestionSource questions, ITriviaWinnerSink winners, IRandomProvider random)
        {
            _questions = questions ?? throw new ArgumentNullException(nameof(questions));
            _winners = winners ?? throw new ArgumentNullException(nameof(winners));
            _deal = new DealQuestionsUseCase(random);
        }

        /// <summary>Ids de las preguntas que ya han salido en esta partida.</summary>
        private readonly HashSet<long> _asked = new HashSet<long>();

        public event Action Changed;

        public TriviaFase Fase { get; private set; } = TriviaFase.Setup;
        public TriviaConfig Config { get; private set; }
        public TriviaSession Session { get; private set; }
        public int CurrentPlayerIndex { get; private set; }
        public Question CurrentQuestion { get; private set; }
        public TriviaErrorKind? ErrorKind { get; private set; }

        public IReadOnlyList<TriviaPlayer> AlivePlayers => Session?.AlivePlayers ?? (IReadOnlyList<TriviaPlayer>)Array.Empty<TriviaPlayer>();

        public TriviaPlayer CurrentPlayer
        {
            get
            {
                var alive = AlivePlayers;
                return CurrentPlayerIndex < alive.Count ? alive[CurrentPlayerIndex] : null;
            }
        }

        public IReadOnlyList<TriviaPlayer> Winners => Session?.Winners ?? (IReadOnlyList<TriviaPlayer>)Array.Empty<TriviaPlayer>();
        public int CurrentRound => Session?.CurrentRound ?? 0;

        /// <summary>Preguntas repartidas en la ronda, en orden de turno.</summary>
        public IReadOnlyList<(TriviaPlayer player, Question question)> RoundQuestions => _roundQuestions;

        public void Iniciar(TriviaConfig config)
        {
            Reset();
            Fase = TriviaFase.Iniciando;
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Notify();
            try
            {
                var players = config.PlayerNames.Select(n => new TriviaPlayer(n)).ToList();
                var session = TriviaSession.Start(players);
                var pools = LoadPools();
                if (Difficulties.All.Any(d => pools[d].Count < players.Count))
                {
                    Fail();
                    return;
                }
                Session = session;
                _roundQuestions = Deal(session.AlivePlayers, pools[Difficulty.Facil]);
                CurrentPlayerIndex = 0;
                Fase = TriviaFase.Pass;
            }
            catch (Exception)
            {
                Fail();
                return;
            }
            Notify();
        }

        /// <summary>El jugador tiene el móvil: ve su pregunta.</summary>
        public void PasarDispositivo()
        {
            var player = CurrentPlayer;
            if (player == null || Fase != TriviaFase.Pass) return;
            var entry = _roundQuestions.FirstOrDefault(e => e.player.Equals(player));
            if (entry.question == null) return;
            CurrentQuestion = entry.question;
            Fase = TriviaFase.Question;
            Notify();
        }

        /// <summary>Responde la pregunta actual con la opción <paramref name="chosenIndex"/>.</summary>
        public void Responder(int chosenIndex)
        {
            if (Fase != TriviaFase.Question || Session == null || CurrentQuestion == null) return;
            var player = CurrentPlayer;
            if (player == null) return;

            var updated = Session.RecordAnswer(player, CurrentQuestion.IsCorrect(chosenIndex));
            var next = NextRoundPlayer(updated, player);
            CurrentQuestion = null;
            if (next != null)
            {
                Session = updated;
                CurrentPlayerIndex = IndexOf(updated.AlivePlayers, next);
                Fase = TriviaFase.Pass;
                Notify();
                return;
            }
            FinishRound(updated);
        }

        public void Reiniciar()
        {
            Reset();
            Notify();
        }

        /// <summary>El primer jugador después del actual, en el orden de la ronda, que siga vivo.</summary>
        private TriviaPlayer NextRoundPlayer(TriviaSession updated, TriviaPlayer current)
        {
            var alive = new HashSet<TriviaPlayer>(updated.AlivePlayers);
            var order = _roundQuestions.Select(e => e.player).ToList();
            for (var i = order.IndexOf(current) + 1; i < order.Count; i++)
            {
                if (alive.Contains(order[i])) return order[i];
            }
            return null;
        }

        private void FinishRound(TriviaSession session)
        {
            if (session.AlivePlayers.Count == 0)
            {
                GameOver(session);
                return;
            }
            var nextSession = session.AdvanceRound();
            if (nextSession.IsOver)
            {
                GameOver(nextSession);
                return;
            }
            try
            {
                var pools = LoadPools();
                Session = nextSession;
                _roundQuestions = Deal(nextSession.AlivePlayers, pools[TriviaSession.DifficultyForRound(nextSession.CurrentRound)]);
                CurrentPlayerIndex = 0;
                Fase = TriviaFase.Pass;
                Notify();
            }
            catch (Exception)
            {
                Fail();
            }
        }

        private void GameOver(TriviaSession session)
        {
            Session = session;
            foreach (var winner in session.Winners)
            {
                try
                {
                    _winners.IncrementWins(winner.Name);
                }
                catch (Exception)
                {
                    // El contador de victorias nunca rompe el final de la partida.
                }
            }
            Fase = TriviaFase.GameOver;
            Notify();
        }

        /// <summary>
        /// Reparte con las preguntas que aún no han salido en la partida (T1; en el Flutter podían repetirse entre
        /// rondas). Si no quedan suficientes para todos los vivos, se reparte del pool completo.
        /// </summary>
        private IReadOnlyList<(TriviaPlayer player, Question question)> Deal(IReadOnlyList<TriviaPlayer> alive, IReadOnlyList<Question> pool)
        {
            var fresh = pool.Where(q => !_asked.Contains(q.Id)).ToList();
            var dealt = _deal.Execute(alive, fresh.Count >= alive.Count ? fresh : pool);
            foreach (var entry in dealt) _asked.Add(entry.question.Id);
            return dealt;
        }

        private Dictionary<Difficulty, IReadOnlyList<Question>> LoadPools() =>
            Difficulties.All.ToDictionary(d => d, d => _questions.GetPool(Config.TematicaIds, d) ?? Array.Empty<Question>());

        private void Fail()
        {
            Fase = TriviaFase.Error;
            Session = null;
            ErrorKind = TriviaErrorKind.SinPreguntas;
            Notify();
        }

        private void Reset()
        {
            Fase = TriviaFase.Setup;
            Config = null;
            Session = null;
            CurrentPlayerIndex = 0;
            CurrentQuestion = null;
            ErrorKind = null;
            _roundQuestions = Array.Empty<(TriviaPlayer, Question)>();
            _asked.Clear();
        }

        private static int IndexOf(IReadOnlyList<TriviaPlayer> list, TriviaPlayer player)
        {
            for (var i = 0; i < list.Count; i++) if (list[i].Equals(player)) return i;
            return 0;
        }

        private void Notify() => Changed?.Invoke();
    }
}
