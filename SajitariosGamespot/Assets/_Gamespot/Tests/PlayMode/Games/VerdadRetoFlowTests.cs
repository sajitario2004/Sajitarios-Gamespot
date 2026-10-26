using System.Collections;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.VerdadReto;
using Gamespot.Games.VerdadReto.Domain;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>V071: Verdad o Reto de principio a fin con el seed real (setup, turnos, rajarse, final y jugar otra).</summary>
    public sealed class VerdadRetoFlowTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown()
        {
            Dialogs.CloseAll();
            _app?.Dispose();
        }

        private VerdadRetoModule Module => _app.Bootstrap.Games.OfType<VerdadRetoGame>().Single().Module;

        private VerdadRetoSetupScreen OpenSetup()
        {
            _app = new AppTestHarness();
            _app.Router.Go(VerdadRetoModule.SetupRoute);
            return (VerdadRetoSetupScreen)_app.Router.Current;
        }

        [UnityTest]
        public IEnumerator Partida_CompletaConVerdadRetoYRajada()
        {
            var setup = OpenSetup();
            yield return null;
            Module.Draft.Rondas = 1;
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Bea");
            setup.Start();
            yield return null;

            var turn = (VerdadRetoTurnScreen)_app.Router.Current;
            Assert.AreEqual("Ana", Module.Flow.JugadorActual.Name);
            Assert.IsTrue(turn.VerdadButton.Interactable && turn.RetoButton.Interactable);
            turn.ChooseVerdad();
            yield return null;
            var card = (VerdadRetoCardScreen)_app.Router.Current;
            Assert.IsFalse(string.IsNullOrEmpty(card.CardText));
            Assert.AreEqual(PromptLevel.Suave, Module.Flow.CartaActual.Level, "Por defecto solo suave");
            StringAssert.DoesNotContain(VerdadRetoFlowController.Otro, card.CardText, "El {otro} sale con nombre");
            card.Change();
            Assert.AreEqual(0, Module.Flow.CambiosRestantes, "Un cambio por turno por defecto");
            card.Done();
            yield return null;

            turn = (VerdadRetoTurnScreen)_app.Router.Current;
            Assert.AreEqual("Bea", Module.Flow.JugadorActual.Name);
            turn.ChooseReto();
            yield return null;
            ((VerdadRetoCardScreen)_app.Router.Current).Chicken();
            yield return null;
            var penalty = (VerdadRetoPenaltyScreen)_app.Router.Current;
            Assert.IsFalse(string.IsNullOrEmpty(penalty.PenaltyText));
            penalty.Done();
            yield return null;

            Assert.IsInstanceOf<VerdadRetoEndScreen>(_app.Router.Current, "Una ronda: se acaba");
            CollectionAssert.AreEqual(new[] { "Ana", "Bea" }, Module.Flow.Ranking.Select(p => p.Name));
            Assert.AreEqual(1, Module.Flow.Jugadores[1].Rajadas);

            Module.PlayAgain();
            yield return null;
            Assert.IsInstanceOf<VerdadRetoTurnScreen>(_app.Router.Current);
            Assert.IsTrue(Module.Flow.Jugadores.All(p => p.Puntos == 0), "Jugar otra empieza de cero");
        }

        [UnityTest]
        public IEnumerator Picante_PideConfirmarLaEdadYSoloSalenPicantes()
        {
            var setup = OpenSetup();
            yield return null;
            setup.Levels.Request(PromptLevel.Picante, true);
            Assert.IsTrue(Dialogs.IsOpen, "Pide confirmar la edad");
            Dialogs.HandleBack();
            Assert.IsFalse(setup.Levels.Selected.Contains(PromptLevel.Picante));
            AdultGate.Confirmed = true;
            setup.Levels.Request(PromptLevel.Picante, true);
            setup.Levels.Set(PromptLevel.Suave, false);
            Assert.IsTrue(setup.Levels.WarningVisible);
            CollectionAssert.AreEqual(new[] { PromptLevel.Picante }, setup.Levels.Selected);
            setup.Levels.Set(PromptLevel.Picante, false);
            CollectionAssert.AreEqual(new[] { PromptLevel.Picante }, setup.Levels.Selected, "El último nivel no se quita");

            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Bea");
            setup.Start();
            yield return null;
            for (var i = 0; i < 6; i++)
            {
                Module.Flow.Elegir(i % 2 == 0 ? VerdadRetoKinds.Verdad : VerdadRetoKinds.Reto);
                Assert.AreEqual(PromptLevel.Picante, Module.Flow.CartaActual.Level);
                Module.Flow.Cumplir();
            }
        }

        [UnityTest]
        public IEnumerator Setup_ErroresYCartasPropias()
        {
            var setup = OpenSetup();
            yield return null;
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "ana");
            setup.Start();
            Assert.IsInstanceOf<VerdadRetoSetupScreen>(_app.Router.Current, "Nombres repetidos: no empieza");
            Assert.IsFalse(setup.Origin.Visible, "Sin cartas propias no se elige el origen");

            Module.Cards.Insert(new PromptCard(0, "Cuenta tu mejor anécdota con {otro}", kind: VerdadRetoKinds.Verdad));
            Module.Cards.Insert(new PromptCard(0, "Baila con {otro}", kind: VerdadRetoKinds.Reto));
            _app.Router.Go(VerdadRetoModule.CardsRoute);
            yield return null;
            var cards = (PromptCardsScreen)_app.Router.Current;
            cards.SetFilter("own");
            Assert.AreEqual(2, cards.Items.Count);

            _app.Router.Go(VerdadRetoModule.SetupRoute);
            yield return null;
            setup = (VerdadRetoSetupScreen)_app.Router.Current;
            Assert.IsTrue(setup.Origin.Visible);
            setup.Origin.Set(false, true);
            Module.Draft.IncluirSerie = false;
            Module.Draft.IncluirPropias = true;
            setup.Players.SetName(1, "Bea");
            setup.Start();
            yield return null;
            Module.Flow.Elegir(VerdadRetoKinds.Reto);
            Assert.AreEqual("Baila con Bea", Module.Flow.TextoCarta, "Solo las propias, con el nombre del otro");
        }
    }
}
