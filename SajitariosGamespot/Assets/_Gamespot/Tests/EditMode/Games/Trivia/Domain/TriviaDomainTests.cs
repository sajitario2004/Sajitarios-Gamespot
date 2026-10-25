using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Games.Trivia.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Trivia
{
    /// <summary>Preguntas de prueba: la correcta siempre es la opción 0.</summary>
    internal sealed class FakeQuestionSource : ITriviaQuestionSource
    {
        public readonly Dictionary<Difficulty, int> Sizes = new Dictionary<Difficulty, int>
        {
            [Difficulty.Facil] = 20, [Difficulty.Dificil] = 20, [Difficulty.MuyDificil] = 20
        };
        public readonly List<Difficulty> Requested = new List<Difficulty>();
        public bool FailAfterFirstLoad;
        private int _loads;

        public IReadOnlyList<Question> GetPool(IReadOnlyCollection<string> tematicaIds, Difficulty difficulty)
        {
            if (difficulty == Difficulty.Facil) _loads++;
            if (FailAfterFirstLoad && _loads > 1) throw new InvalidOperationException("BD caída");
            Requested.Add(difficulty);
            return Enumerable.Range(1, Sizes[difficulty])
                .Select(i => new Question(((int)difficulty + 1) * 1000 + i, "historia", difficulty, $"P{difficulty}{i}", new[] { "ok", "b", "c", "d" }, 0))
                .ToList();
        }
    }

    internal sealed class FakeWinnerSink : ITriviaWinnerSink
    {
        public readonly List<string> Wins = new List<string>();
        public void IncrementWins(string name) => Wins.Add(name);
    }

    public sealed class TriviaDomainTests
    {
        private static TriviaConfig Config(params string[] names) => TriviaConfig.Create(names, new[] { "historia" }).config;

        [TestCase(0, Difficulty.Facil)]
        [TestCase(2, Difficulty.Facil)]
        [TestCase(3, Difficulty.Dificil)]
        [TestCase(5, Difficulty.Dificil)]
        [TestCase(6, Difficulty.MuyDificil)]
        [TestCase(8, Difficulty.MuyDificil)]
        public void TramosDeDificultad(int round, Difficulty expected)
        {
            Assert.AreEqual(expected, TriviaSession.DifficultyForRound(round));
        }

        [Test]
        public void RondaFueraDeRango_Lanza()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TriviaSession.DifficultyForRound(9));
            Assert.Throws<ArgumentOutOfRangeException>(() => TriviaSession.DifficultyForRound(-1));
        }

        [Test]
        public void Dificultades_NombresDeLaBDYOpenTdb()
        {
            Assert.AreEqual("muyDificil", Difficulty.MuyDificil.StorageName());
            Assert.AreEqual(Difficulty.Dificil, Difficulties.FromStorageName("dificil"));
            Assert.AreEqual(Difficulty.MuyDificil, Difficulties.FromOpenTdb("hard"));
            Assert.Throws<ArgumentException>(() => Difficulties.FromOpenTdb("legend"));
        }

        [Test]
        public void Pregunta_Validacion()
        {
            Assert.Throws<ArgumentException>(() => new Question(1, "t", Difficulty.Facil, "¿?", new[] { "a", "b", "c" }, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Question(1, "t", Difficulty.Facil, "¿?", new[] { "a", "b", "c", "d" }, 4));
            Assert.IsTrue(new Question(1, "t", Difficulty.Facil, "¿?", new[] { "a", "b", "c", "d" }, 2).IsCorrect(2));
        }

        [Test]
        public void Config_LimitesYNormalizacion()
        {
            Assert.AreEqual(TriviaConfigError.PocosJugadores, TriviaConfig.Create(new[] { "Ana" }, new[] { "x" }).error);
            Assert.AreEqual(TriviaConfigError.DemasiadosJugadores, TriviaConfig.Create(Enumerable.Range(0, 7).Select(i => $"J{i}").ToList(), new[] { "x" }).error);
            Assert.AreEqual(TriviaConfigError.NombreVacio, TriviaConfig.Create(new[] { "Ana", "  " }, new[] { "x" }).error);
            Assert.AreEqual(TriviaConfigError.NombresDuplicados, TriviaConfig.Create(new[] { "Ana", " ana" }, new[] { "x" }).error);
            Assert.AreEqual(TriviaConfigError.SinTematicas, TriviaConfig.Create(new[] { "Ana", "Bea" }, new string[0]).error);
            var ok = TriviaConfig.Create(new[] { " Ana ", "Bea" }, new[] { "x" }).config;
            CollectionAssert.AreEqual(new[] { "Ana", "Bea" }, ok.PlayerNames);
        }

        [Test]
        public void Sesion_EliminacionYEmpateDeSupervivientes()
        {
            var ana = new TriviaPlayer("Ana");
            var bea = new TriviaPlayer("Bea");
            var session = TriviaSession.Start(new[] { ana, bea });
            session = session.RecordAnswer(ana, true).RecordAnswer(bea, false);
            CollectionAssert.AreEqual(new[] { ana }, session.AlivePlayers);
            for (var i = 0; i < 9; i++) session = session.AdvanceRound();
            Assert.IsTrue(session.IsOver);
            CollectionAssert.AreEqual(new[] { ana }, session.Winners);
            Assert.Throws<InvalidOperationException>(() => session.AdvanceRound());
        }

        [Test]
        public void Sesion_SiCaenTodosNoHayGanadores()
        {
            var ana = new TriviaPlayer("Ana");
            var session = TriviaSession.Start(new[] { ana }).RecordAnswer(ana, false);
            Assert.IsTrue(session.IsOver);
            Assert.AreEqual(0, session.Winners.Count);
        }

        [Test]
        public void Reparto_PreguntasDistintasYDeterminista()
        {
            var players = new[] { new TriviaPlayer("A"), new TriviaPlayer("B"), new TriviaPlayer("C") };
            var pool = new FakeQuestionSource().GetPool(new[] { "x" }, Difficulty.Facil);
            var a = new DealQuestionsUseCase(new SeededRandom(99)).Execute(players, pool);
            var b = new DealQuestionsUseCase(new SeededRandom(99)).Execute(players, pool);
            Assert.AreEqual(3, a.Select(e => e.question).Distinct().Count());
            CollectionAssert.AreEqual(a.Select(e => e.question.Id), b.Select(e => e.question.Id));
            CollectionAssert.AreEqual(players, a.Select(e => e.player));
            Assert.Throws<ArgumentException>(() => new DealQuestionsUseCase(new SeededRandom(1)).Execute(players, pool.Take(2).ToList()));
            Assert.DoesNotThrow(() => new DealQuestionsUseCase(new SeededRandom(1)).Execute(players, pool.Take(3).ToList()));
        }

        [Test]
        public void Flow_PoolInsuficiente_ErrorSinPreguntas()
        {
            var source = new FakeQuestionSource();
            source.Sizes[Difficulty.MuyDificil] = 1;
            var flow = new TriviaFlowController(source, new FakeWinnerSink(), new SeededRandom(1));
            flow.Iniciar(Config("Ana", "Bea"));
            Assert.AreEqual(TriviaFase.Error, flow.Fase);
            Assert.AreEqual(TriviaErrorKind.SinPreguntas, flow.ErrorKind);
        }

        [Test]
        public void Flow_PartidaCompleta_DificultadPorRondaYVictorias()
        {
            var source = new FakeQuestionSource();
            var winners = new FakeWinnerSink();
            var flow = new TriviaFlowController(source, winners, new SeededRandom(3));
            flow.Iniciar(Config("Ana", "Bea", "Carlos"));
            Assert.AreEqual(TriviaFase.Pass, flow.Fase);

            var seenDifficulties = new List<Difficulty>();
            while (flow.Fase != TriviaFase.GameOver)
            {
                flow.PasarDispositivo();
                Assert.AreEqual(TriviaFase.Question, flow.Fase);
                seenDifficulties.Add(flow.CurrentQuestion.Difficulty);
                // Bea falla en la ronda 4 (índice 3); los demás aciertan siempre.
                var wrong = flow.CurrentPlayer.Name == "Bea" && flow.CurrentRound == 3;
                flow.Responder(wrong ? 1 : 0);
            }

            CollectionAssert.AreEqual(new[] { "Ana", "Carlos" }, flow.Winners.Select(p => p.Name));
            CollectionAssert.AreEquivalent(new[] { "Ana", "Carlos" }, winners.Wins);
            // Rondas 0–2 con 3 jugadores, ronda 3 con 3 (Bea cae) y 4–8 con 2: 9 + 3 + 10.
            Assert.AreEqual(3 * 3 + 3 + 5 * 2, seenDifficulties.Count, "Preguntas respondidas");
            Assert.AreEqual(Difficulty.Facil, seenDifficulties.First());
            Assert.AreEqual(Difficulty.MuyDificil, seenDifficulties.Last());
        }

        [Test]
        public void Flow_EliminadoAMitadDeRonda_NoRecibeMasPreguntas()
        {
            var flow = new TriviaFlowController(new FakeQuestionSource(), new FakeWinnerSink(), new SeededRandom(4));
            flow.Iniciar(Config("Ana", "Bea", "Carlos"));
            flow.PasarDispositivo();
            flow.Responder(1); // Ana falla
            Assert.AreEqual("Bea", flow.CurrentPlayer.Name);
            flow.PasarDispositivo();
            flow.Responder(0);
            Assert.AreEqual("Carlos", flow.CurrentPlayer.Name);
            flow.PasarDispositivo();
            flow.Responder(0);
            Assert.AreEqual(1, flow.CurrentRound);
            CollectionAssert.AreEqual(new[] { "Bea", "Carlos" }, flow.RoundQuestions.Select(e => e.player.Name));
        }

        [Test]
        public void Flow_SiCaenTodos_NadieGanaYNoSeSumanVictorias()
        {
            var winners = new FakeWinnerSink();
            var flow = new TriviaFlowController(new FakeQuestionSource(), winners, new SeededRandom(5));
            flow.Iniciar(Config("Ana", "Bea"));
            flow.PasarDispositivo();
            flow.Responder(1);
            flow.PasarDispositivo();
            flow.Responder(1);
            Assert.AreEqual(TriviaFase.GameOver, flow.Fase);
            Assert.AreEqual(0, flow.Winners.Count);
            Assert.AreEqual(0, winners.Wins.Count);
        }

        [Test]
        public void Flow_ErrorAlRecargarLosPools_ErrorSinPreguntas()
        {
            var source = new FakeQuestionSource { FailAfterFirstLoad = true };
            var flow = new TriviaFlowController(source, new FakeWinnerSink(), new SeededRandom(6));
            flow.Iniciar(Config("Ana", "Bea"));
            for (var i = 0; i < 2; i++)
            {
                flow.PasarDispositivo();
                flow.Responder(0);
            }
            Assert.AreEqual(TriviaFase.Error, flow.Fase);
            Assert.AreEqual(TriviaErrorKind.SinPreguntas, flow.ErrorKind);
        }

        [Test]
        public void Flow_AccionesFueraDeFaseNoHacenNada()
        {
            var flow = new TriviaFlowController(new FakeQuestionSource(), new FakeWinnerSink(), new SeededRandom(7));
            flow.Responder(0);
            flow.PasarDispositivo();
            Assert.AreEqual(TriviaFase.Setup, flow.Fase);
            flow.Iniciar(Config("Ana", "Bea"));
            flow.Responder(0);
            Assert.AreEqual(TriviaFase.Pass, flow.Fase, "Responder en Pass no hace nada");
        }
    }
}
