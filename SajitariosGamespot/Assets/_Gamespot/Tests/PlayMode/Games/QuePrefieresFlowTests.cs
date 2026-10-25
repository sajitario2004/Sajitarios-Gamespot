using System.Collections;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.QuePrefieres;
using Gamespot.Games.QuePrefieres.Domain;
using Gamespot.Games.Shared;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>V073: ¿Qué prefieres? con el seed real: solo preguntas, contando votos con la minoría y preguntas propias.</summary>
    public sealed class QuePrefieresFlowTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown()
        {
            Dialogs.CloseAll();
            _app?.Dispose();
        }

        private QuePrefieresModule Module => _app.Bootstrap.Games.OfType<QuePrefieresGame>().Single().Module;

        private QuePrefieresSetupScreen OpenSetup()
        {
            _app = new AppTestHarness();
            _app.Router.Go(QuePrefieresModule.SetupRoute);
            return (QuePrefieresSetupScreen)_app.Router.Current;
        }

        [UnityTest]
        public IEnumerator SoloPreguntas_ConTiempoParaPensar()
        {
            var setup = OpenSetup();
            yield return null;
            Module.Draft.SegundosPensar = 5;
            Module.Draft.Preguntas = 2;
            setup.Start();
            yield return null;
            var question = (QuePrefieresQuestionScreen)_app.Router.Current;
            Assert.IsNull(question.StepperA, "Sin contar votos no hay contadores");
            Assert.IsTrue(question.Countdown.Running);
            question.Tick(5.1f);
            Assert.IsTrue(question.Countdown.IsFinished);
            question.Reveal();
            yield return null;
            Assert.AreEqual(2, Module.Flow.Numero);
            ((QuePrefieresQuestionScreen)_app.Router.Current).Reveal();
            yield return null;
            Assert.IsInstanceOf<QuePrefieresEndScreen>(_app.Router.Current);
        }

        [UnityTest]
        public IEnumerator ContarVotos_PagaLaMinoria()
        {
            var setup = OpenSetup();
            yield return null;
            Module.Draft.Modo = QuePrefieresModo.ContarVotos;
            setup.Start();
            yield return null;
            var question = (QuePrefieresQuestionScreen)_app.Router.Current;
            question.AddVote(QuePrefieresOpcion.A, 4);
            question.StepperB.Set(1, true);
            Assert.AreEqual(1, Module.Flow.VotosB, "El contador de pantalla vota");
            question.Reveal();
            yield return null;
            var result = (QuePrefieresResultScreen)_app.Router.Current;
            Assert.AreEqual(80, Module.Flow.Ultima.PorcentajeA);
            StringAssert.Contains(Module.Flow.Ultima.Card.TextB, result.Verdict, "Paga la B");
            result.Next();
            yield return null;
            ((QuePrefieresPlayScreen)_app.Router.Current).Finish();
            yield return null;
            Assert.IsInstanceOf<QuePrefieresEndScreen>(_app.Router.Current);
            Assert.AreEqual(1, QuePrefieresEndScreen.Rows(Module.Flow).Count);
            Assert.AreEqual("A", QuePrefieresEndScreen.Rows(Module.Flow)[0].Value);
        }

        [UnityTest]
        public IEnumerator PreguntasPropias_ConLasDosOpciones()
        {
            OpenSetup();
            yield return null;
            _app.Router.Go(QuePrefieresModule.CardsRoute);
            yield return null;
            var cards = (PromptCardsScreen)_app.Router.Current;
            Assert.AreEqual(PromptCardProblem.FaltaSegunda, cards.Save(null, "Comer tortilla con cebolla", "", PromptLevel.Suave, null, null));
            Assert.IsNull(cards.Save(null, "Comer tortilla con cebolla", "Comer tortilla sin cebolla", PromptLevel.Suave, null, null));
            _app.Router.Go(QuePrefieresModule.SetupRoute);
            yield return null;
            var setup = (QuePrefieresSetupScreen)_app.Router.Current;
            Assert.IsTrue(setup.Origin.Visible);
            Module.Draft.IncluirSerie = false;
            setup.Start();
            yield return null;
            Assert.AreEqual("Comer tortilla sin cebolla", Module.Flow.Current.TextB);
            Assert.AreEqual(Loc.T("quePrefieresParejaOpciones", ("a", "x"), ("b", "y")), "x o y");
        }
    }
}
