using System.Collections;
using System.Linq;
using Gamespot.Games.Trivia;
using Gamespot.Games.Trivia.Domain;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    public sealed class TriviaFlowTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown() => _app?.Dispose();

        private TriviaModule Module => _app.Bootstrap.Games.OfType<TriviaGame>().Single().Module;

        private TriviaSetupScreen OpenSetup(params string[] names)
        {
            _app = new AppTestHarness();
            _app.Router.Go(TriviaModule.SetupRoute);
            var setup = (TriviaSetupScreen)_app.Router.Current;
            while (setup.Players.Count < names.Length) setup.Players.AddPlayer();
            for (var i = 0; i < names.Length; i++) setup.Players.SetName(i, names[i]);
            return setup;
        }

        [UnityTest]
        public IEnumerator Tematicas_LaUltimaNoSePuedeDesmarcar()
        {
            var setup = OpenSetup("Ana", "Bea");
            yield return null;
            CollectionAssert.AreEquivalent(Tematicas.Default, Module.DraftThemes);
            setup.ToggleTheme("historia");
            setup.ToggleTheme("cultura_general");
            CollectionAssert.AreEquivalent(new[] { "cultura_general" }, Module.DraftThemes);
            setup.ToggleTheme("cine");
            CollectionAssert.AreEquivalent(new[] { "cultura_general", "cine" }, Module.DraftThemes);
        }

        [UnityTest]
        public IEnumerator Partida_SiTodosFallanNadieGana()
        {
            var setup = OpenSetup("Ana", "Bea");
            yield return null;
            setup.Start();
            yield return null;
            Assert.IsInstanceOf<TriviaPassScreen>(_app.Router.Current);

            for (var i = 0; i < 2; i++)
            {
                ((TriviaPassScreen)_app.Router.Current).Continue();
                yield return null;
                var question = (TriviaQuestionScreen)_app.Router.Current;
                var wrong = (Module.Flow.CurrentQuestion.CorrectIndex + 1) % 4;
                question.Choose(wrong);
                Assert.IsTrue(question.Locked);
                question.Choose(Module.Flow.CurrentQuestion.CorrectIndex);
                Assert.AreEqual(wrong, wrong, "La segunda pulsación no cambia nada");
                question.Tick(0.5f);
                Assert.IsInstanceOf<TriviaQuestionScreen>(_app.Router.Current, "Antes de 800 ms sigue en la pregunta");
                question.Tick(0.35f);
                yield return null;
            }
            Assert.IsInstanceOf<TriviaGameOverScreen>(_app.Router.Current);
            Assert.AreEqual(0, Module.Flow.Winners.Count);
            Assert.AreEqual(0, Module.Winners.GetAllRanked().Count);
        }

        [UnityTest]
        public IEnumerator Partida_AcertandoTodoGananLosDosYSumanVictoria()
        {
            var setup = OpenSetup("Ana", "Bea");
            yield return null;
            setup.Start();
            var answered = 0;
            while (Module.Flow.Fase != TriviaFase.GameOver && answered < 30)
            {
                ((TriviaPassScreen)_app.Router.Current).Continue();
                var question = (TriviaQuestionScreen)_app.Router.Current;
                question.Choose(Module.Flow.CurrentQuestion.CorrectIndex);
                question.Tick(1f);
                answered++;
                yield return null;
            }
            Assert.AreEqual(18, answered, "9 rondas × 2 jugadores");
            Assert.IsInstanceOf<TriviaGameOverScreen>(_app.Router.Current);
            CollectionAssert.AreEquivalent(new[] { "Ana", "Bea" }, Module.Flow.Winners.Select(w => w.Name));
            Assert.AreEqual(1, Module.Winners.GetWins("ana"));
        }
    }
}
