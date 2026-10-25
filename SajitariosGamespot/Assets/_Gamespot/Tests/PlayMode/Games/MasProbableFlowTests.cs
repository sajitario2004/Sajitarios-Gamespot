using System.Collections;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.MasProbable;
using Gamespot.Games.MasProbable.Domain;
using Gamespot.Games.Shared;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>V072: ¿Quién es más probable que…? con el seed real: cuenta atrás, señalar, llevar la cuenta y final.</summary>
    public sealed class MasProbableFlowTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown()
        {
            Dialogs.CloseAll();
            _app?.Dispose();
        }

        private MasProbableModule Module => _app.Bootstrap.Games.OfType<MasProbableGame>().Single().Module;

        private MasProbableSetupScreen OpenSetup()
        {
            _app = new AppTestHarness();
            _app.Router.Go(MasProbableModule.SetupRoute);
            return (MasProbableSetupScreen)_app.Router.Current;
        }

        [UnityTest]
        public IEnumerator SoloSenalar_CuentaAtrasYSiguiente()
        {
            var setup = OpenSetup();
            yield return null;
            Assert.IsFalse(setup.PlayersVisible, "Sin cuenta no hacen falta nombres");
            Module.Draft.Frases = 10;
            setup.Start();
            yield return null;
            var phrase = (MasProbablePhraseScreen)_app.Router.Current;
            StringAssert.StartsWith("…", phrase.PhraseText);
            StringAssert.EndsWith("?", phrase.PhraseText);
            Assert.IsFalse(phrase.Revealed);
            phrase.StartCount();
            Assert.IsTrue(phrase.Countdown.Running);
            Assert.AreEqual(3, phrase.Countdown.SecondsShown, "3 s por defecto");
            phrase.Tick(3.1f);
            Assert.IsTrue(phrase.Revealed, "Al acabar la cuenta, todos señalan");
            phrase.Next();
            yield return null;
            Assert.AreEqual(2, Module.Flow.Numero);
            ((MasProbablePhraseScreen)_app.Router.Current).Skip();
            yield return null;
            Assert.AreEqual(2, Module.Flow.Numero, "Saltar no cuenta");
            ((MasProbablePlayScreen)_app.Router.Current).Finish();
            yield return null;
            Assert.IsInstanceOf<MasProbableEndScreen>(_app.Router.Current);
        }

        [UnityTest]
        public IEnumerator ConCuenta_ApuntarYElMasProbable()
        {
            var setup = OpenSetup();
            yield return null;
            setup.SelectMode(MasProbableModo.ConCuenta);
            Assert.IsTrue(setup.PlayersVisible);
            Module.Draft.SegundosCuenta = 0;
            Module.Draft.Frases = 2;
            setup.Start();
            Assert.IsInstanceOf<MasProbableSetupScreen>(_app.Router.Current, "Faltan nombres");
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Bea");
            setup.Players.SetName(2, "Carlos");
            setup.Start();
            yield return null;

            var phrase = (MasProbablePhraseScreen)_app.Router.Current;
            phrase.StartCount();
            Assert.IsTrue(phrase.Revealed, "Sin cuenta atrás, directamente");
            phrase.Next();
            yield return null;
            var point = (MasProbablePointScreen)_app.Router.Current;
            point.Toggle(1);
            point.Toggle(2);
            point.Toggle(2);
            point.Confirm();
            yield return null;
            ((MasProbablePhraseScreen)_app.Router.Current).StartCount();
            ((MasProbablePhraseScreen)_app.Router.Current).Next();
            yield return null;
            point = (MasProbablePointScreen)_app.Router.Current;
            point.Toggle(1);
            point.Confirm();
            yield return null;
            Assert.IsInstanceOf<MasProbableEndScreen>(_app.Router.Current, "Dos frases: se acaba");
            CollectionAssert.AreEqual(new[] { 0, 2, 0 }, Module.Flow.Titulos);
            Assert.AreEqual("Bea", MasProbableEndScreen.Rows(Module.Flow)[0].Name);
        }

        [UnityTest]
        public IEnumerator FrasesPropias_SeNormalizanYSeJuegan()
        {
            var setup = OpenSetup();
            yield return null;
            _app.Router.Go(MasProbableModule.CardsRoute);
            yield return null;
            var cards = (PromptCardsScreen)_app.Router.Current;
            Assert.IsNull(cards.Save(null, "¿Quién es más probable que se coma la tarta entera?", null, PromptLevel.Suave, null, null));
            Assert.AreEqual("se coma la tarta entera", Module.Cards.Search(null, new PromptCardFilter { OwnOnly = true }).Single().Texto);

            _app.Router.Go(MasProbableModule.SetupRoute);
            yield return null;
            setup = (MasProbableSetupScreen)_app.Router.Current;
            Assert.IsTrue(setup.Origin.Visible);
            Module.Draft.IncluirSerie = false;
            setup.Start();
            yield return null;
            Assert.AreEqual("…se coma la tarta entera?", ((MasProbablePhraseScreen)_app.Router.Current).PhraseText);
        }
    }
}
