using System.Collections;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Mimica;
using Gamespot.Games.Mimica.Domain;
using Gamespot.Games.Shared;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>V074: Mímica por equipos con el seed real: setup, turnos con tiempo, resumen corregible, final y tarjetas propias.</summary>
    public sealed class MimicaFlowTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown()
        {
            Dialogs.CloseAll();
            _app?.Dispose();
        }

        private MimicaModule Module => _app.Bootstrap.Games.OfType<MimicaGame>().Single().Module;

        private MimicaSetupScreen OpenSetup()
        {
            _app = new AppTestHarness();
            _app.Router.Go(MimicaModule.SetupRoute);
            return (MimicaSetupScreen)_app.Router.Current;
        }

        [UnityTest]
        public IEnumerator Partida_DosEquiposHastaElObjetivo()
        {
            var setup = OpenSetup();
            yield return null;
            Assert.IsFalse(setup.Teams.CanUseLastPlayers, "Los equipos no usan los jugadores de la última partida");
            Assert.IsFalse(setup.Categories.IsSelected(MimicaCategories.Cachondeo), "La +18 empieza sin marcar");
            Module.Draft.Objetivo = 5;
            Module.Draft.Segundos = 30;
            Module.Draft.Pases = 1;
            setup.Start();
            Assert.IsInstanceOf<MimicaSetupScreen>(_app.Router.Current, "Sin nombres de equipo no empieza");
            setup.Teams.SetName(0, "Rojos");
            setup.Teams.SetName(1, "Azules");
            setup.Start();
            yield return null;

            var ready = (GuessReadyScreen)_app.Router.Current;
            Assert.AreEqual("Rojos", Module.Flow.Match.Equipos[Module.Flow.Match.Actual]);
            ready.StartTurn();
            yield return null;
            var turn = (MimicaTurnScreen)_app.Router.Current;
            Assert.IsFalse(string.IsNullOrEmpty(turn.Word));
            Assert.IsFalse(WordCategories.IsAdult(Module.Flow.Current.Category));
            for (var i = 0; i < 5; i++) turn.Correct();
            turn.Pass();
            turn.Pass();
            Assert.AreEqual(1, Module.Flow.Fallos, "Un pase por turno");
            turn.Tick(31f);
            yield return null;
            var summary = (GuessSummaryScreen)_app.Router.Current;
            Assert.AreEqual(5, Module.Flow.Aciertos);
            summary.Fix(0);
            Assert.AreEqual(4, Module.Flow.PuntosTurno, "Corregida una");
            summary.Confirm();
            yield return null;

            Assert.IsInstanceOf<GuessReadyScreen>(_app.Router.Current);
            Assert.AreEqual(1, Module.Flow.Match.Actual, "Ahora los Azules");
            ((GuessReadyScreen)_app.Router.Current).StartTurn();
            yield return null;
            turn = (MimicaTurnScreen)_app.Router.Current;
            for (var i = 0; i < 6; i++) turn.Correct();
            turn.TimeUp();
            yield return null;
            ((GuessSummaryScreen)_app.Router.Current).Confirm();
            yield return null;
            Assert.IsInstanceOf<GuessEndScreen>(_app.Router.Current, "Los Azules llegan a 6 al acabar la ronda");
            CollectionAssert.AreEqual(new[] { 1 }, Module.Flow.Match.Ganadores);

            Module.PlayAgain();
            yield return null;
            Assert.IsInstanceOf<GuessReadyScreen>(_app.Router.Current);
            Assert.AreEqual(0, Module.Flow.Match.Puntos.Sum());
        }

        [UnityTest]
        public IEnumerator Mezcla_CadaTarjetaConEstiloYCachondeoPideEdad()
        {
            var setup = OpenSetup();
            yield return null;
            setup.Categories.Toggle(MimicaCategories.Cachondeo);
            Assert.IsTrue(Dialogs.IsOpen, "Cachondeo pide confirmar la edad");
            Dialogs.HandleBack();
            Assert.IsFalse(setup.Categories.IsSelected(MimicaCategories.Cachondeo));
            AdultGate.Confirmed = true;
            setup.Categories.SelectOnly(MimicaCategories.Cachondeo);
            Assert.IsTrue(setup.Categories.AdultWarningVisible);
            Module.Draft.Modo = MimicaModo.Mezcla;
            setup.Teams.SetName(0, "Rojos");
            setup.Teams.SetName(1, "Azules");
            setup.Start();
            yield return null;
            ((GuessReadyScreen)_app.Router.Current).StartTurn();
            yield return null;
            var turn = (MimicaTurnScreen)_app.Router.Current;
            for (var i = 0; i < 8; i++)
            {
                Assert.AreEqual(MimicaCategories.Cachondeo, Module.Flow.Current.Category);
                Assert.That(Module.Flow.CurrentTag, Is.InRange(0, 3), "En mezcla cada tarjeta trae su estilo");
                turn.Correct();
            }
        }

        [UnityTest]
        public IEnumerator TarjetasPropias_EnUnaCategoriaPropia()
        {
            OpenSetup();
            yield return null;
            Assert.IsNull(CategoryNameDialog.TryCreate(Module.Categories, "Mi pueblo", out var pueblo));
            _app.Router.Go(MimicaModule.CardsRoute);
            yield return null;
            var cards = (PromptCardsScreen)_app.Router.Current;
            Assert.IsNull(cards.Save(null, "Bailar la jota", null, PromptLevel.Suave, null, pueblo.CategoryId));
            _app.Router.Go(MimicaModule.SetupRoute);
            yield return null;
            var setup = (MimicaSetupScreen)_app.Router.Current;
            Assert.IsTrue(setup.Categories.IsSelected(pueblo.CategoryId), "La categoría propia nueva sale marcada");
            setup.Categories.SelectOnly(pueblo.CategoryId);
            setup.Teams.SetName(0, "Rojos");
            setup.Teams.SetName(1, "Azules");
            setup.Start();
            yield return null;
            ((GuessReadyScreen)_app.Router.Current).StartTurn();
            yield return null;
            Assert.AreEqual("Bailar la jota", Module.Flow.Current.Text);
            Assert.AreEqual("BAILAR LA JOTA", ((MimicaTurnScreen)_app.Router.Current).Word);
        }
    }
}
