using System.Collections;
using System.Linq;
using Gamespot.Games.Bomba;
using Gamespot.Games.Bomba.Domain;
using Gamespot.Games.Tabu;
using Gamespot.Games.Tabu.Domain;
using Gamespot.Games.Trivia;
using Gamespot.Games.Trivia.Domain;
using Gamespot.Games.Wavelength;
using Gamespot.Games.Wavelength.Domain;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>C4: "Jugar otra" empieza al momento con los mismos ajustes; "Cambiar ajustes" vuelve al setup.</summary>
    public sealed class PlayAgainTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown() => _app?.Dispose();

        private T Game<T>() => _app.Bootstrap.Games.OfType<T>().Single();

        [UnityTest]
        public IEnumerator Bomba_MismosJugadoresYModo()
        {
            _app = new AppTestHarness();
            _app.Router.Go(BombaModule.SetupRoute);
            var setup = (BombaSetupScreen)_app.Router.Current;
            setup.SelectMode(BombaMode.Categoria);
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Bob");
            setup.Start();
            var play = (BombaPlayScreen)_app.Router.Current;
            play.Tick(61f);
            play.Tick(2f);
            yield return null;
            Assert.IsInstanceOf<BombaGameOverScreen>(_app.Router.Current);

            var module = Game<BombaGame>().Module;
            module.PlayAgain();
            yield return null;
            Assert.IsInstanceOf<BombaPlayScreen>(_app.Router.Current);
            Assert.AreEqual(BombaFase.Jugando, module.Flow.Fase);
            CollectionAssert.AreEqual(new[] { "Ana", "Bob" }, module.Flow.AlivePlayers, "Vuelven a estar todos");
            Assert.AreEqual(BombaMode.Categoria, module.Flow.CurrentPrompt.Mode);
            Assert.AreEqual(0f, ((BombaPlayScreen)_app.Router.Current).Elapsed, 0.2f, "Mecha nueva");

            module.ResetToSetup();
            yield return null;
            var again = (BombaSetupScreen)_app.Router.Current;
            CollectionAssert.AreEqual(new[] { "Ana", "Bob" }, again.Players.NormalizedNames, "Cambiar ajustes conserva el borrador");
            Assert.AreEqual(BombaFase.Setup, module.Flow.Fase);
        }

        [UnityTest]
        public IEnumerator Tabu_MismosEquiposYTiempo()
        {
            _app = new AppTestHarness();
            _app.Router.Go(TabuModule.SetupRoute);
            var setup = (TabuSetupScreen)_app.Router.Current;
            setup.TeamA.Value = "Rojos";
            setup.TeamB.Value = "Azules";
            setup.SelectSeconds(90);
            setup.Start();
            var module = Game<TabuGame>().Module;
            while (module.Flow.Fase != TabuFase.GameOver)
            {
                if (module.Flow.Session.EquipoActual == TabuEquipo.A) module.Flow.Acierto();
                module.Flow.TerminarTurno();
                module.Flow.SiguienteTurno();
            }
            _app.Router.Go(TabuModule.GameOverRoute);
            yield return null;

            module.PlayAgain();
            yield return null;
            var turn = (TabuTurnScreen)_app.Router.Current;
            Assert.AreEqual(TabuFase.Turno, module.Flow.Fase);
            Assert.AreEqual(0, module.Flow.Session.VictoriasA, "Marcador a cero");
            Assert.AreEqual("Rojos", module.Flow.CurrentTeamName);
            Assert.AreEqual(90, turn.SecondsShown, "Mismo tiempo de turno");
        }

        [UnityTest]
        public IEnumerator Trivia_MismosJugadoresYTematicas()
        {
            _app = new AppTestHarness();
            _app.Router.Go(TriviaModule.SetupRoute);
            var setup = (TriviaSetupScreen)_app.Router.Current;
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Bea");
            setup.Start();
            var module = Game<TriviaGame>().Module;
            var themes = module.Flow.Config.TematicaIds.ToList();
            while (module.Flow.Fase != TriviaFase.GameOver)
            {
                module.Flow.PasarDispositivo();
                module.Flow.Responder((module.Flow.CurrentQuestion.CorrectIndex + 1) % 4);
            }
            _app.Router.Go(TriviaModule.GameOverRoute);
            yield return null;

            module.PlayAgain();
            yield return null;
            Assert.IsInstanceOf<TriviaPassScreen>(_app.Router.Current);
            CollectionAssert.AreEqual(new[] { "Ana", "Bea" }, module.Flow.Config.PlayerNames);
            CollectionAssert.AreEquivalent(themes, module.Flow.Config.TematicaIds);
        }

        [UnityTest]
        public IEnumerator Wavelength_MismosJugadoresYRondas()
        {
            _app = new AppTestHarness();
            var module = Game<WavelengthGame>().Module;
            module.DraftRounds = 1;
            _app.Router.Go(WavelengthModule.SetupRoute);
            var setup = (WavelengthSetupScreen)_app.Router.Current;
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Luis");
            setup.Start();
            ((WavelengthClueScreen)_app.Router.Current).Confirm("pista");
            ((WavelengthPassScreen)_app.Router.Current).Continue();
            yield return AppTestHarness.Frames();
            ((WavelengthGuessScreen)_app.Router.Current).Confirm();
            yield return AppTestHarness.Frames();
            ((WavelengthRevealScreen)_app.Router.Current).Next();
            yield return null;
            Assert.IsInstanceOf<WavelengthGameOverScreen>(_app.Router.Current);

            module.PlayAgain();
            yield return null;
            Assert.IsInstanceOf<WavelengthClueScreen>(_app.Router.Current);
            Assert.AreEqual(1, module.Flow.Config.Rounds);
            CollectionAssert.AreEqual(new[] { "Ana", "Luis" }, module.Flow.Config.PlayerNames);
            Assert.AreEqual(0, module.Flow.Session.CumulativeScore);
        }
    }
}
