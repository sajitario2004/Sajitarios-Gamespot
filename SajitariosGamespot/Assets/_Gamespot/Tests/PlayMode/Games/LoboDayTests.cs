using System.Collections;
using System.Linq;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.HombresLobo;
using Gamespot.Games.HombresLobo.Domain;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>V080: el amanecer, el día (capitán, debate, votación, empate) y el final de Hombres Lobo.</summary>
    public sealed class LoboDayTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown()
        {
            Dialogs.CloseAll();
            _app?.Dispose();
        }

        private HombresLoboModule Module => _app.Bootstrap.Games.OfType<HombresLoboGame>().Single().Module;

        private T Current<T>() where T : class => _app.Router.Current as T;

        /// <summary>La noche con solo el paso de los lobos: matan a <paramref name="victim"/>.</summary>
        private IEnumerator WolvesKill(int victim)
        {
            var night = Current<LoboNightScreen>();
            Assert.IsNotNull(night);
            Assert.AreEqual(LoboPaso.Lobos, night.Step.Paso);
            night.Pick(victim);
            night.Confirm();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Amanecer_ElCazadorDisparaAntesDeEmpezarElDia()
        {
            _app = new AppTestHarness();
            LoboNightTests.StartNight(_app, Module, LoboRol.Lobo, LoboRol.Cazador, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano);
            yield return null;
            yield return WolvesKill(1);
            var dawn = Current<LoboDawnScreen>();
            Assert.IsNotNull(dawn, "Tras la noche, el amanecer");
            Assert.IsFalse(dawn.CanStartDay, "Primero dispara el cazador");
            dawn.Shoot(2);
            yield return null;
            dawn = Current<LoboDawnScreen>();
            Assert.IsNotNull(dawn);
            Assert.IsFalse(Module.Game.Jugadores[2].Vivo);
            Assert.IsTrue(dawn.CanStartDay);
            dawn.StartDay();
            yield return null;
            Assert.IsNotNull(Current<LoboDayScreen>());
            Assert.AreEqual(LoboFase.Dia, Module.Game.Fase);
        }

        [UnityTest]
        public IEnumerator Dia_CapitanDebateVotacionYVictoria()
        {
            _app = new AppTestHarness();
            LoboNightTests.StartNight(_app, Module, LoboRol.Lobo, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano);
            yield return null;
            yield return WolvesKill(1);
            Current<LoboDawnScreen>().StartDay();
            yield return null;
            var day = Current<LoboDayScreen>();
            Assert.IsTrue(Module.Game.NecesitaCapitan, "El primer día se elige capitán");
            Assert.AreEqual(0, day.Candidates.Count, "Sin capitán aún no se vota");
            day.ElectCaptain(2);
            yield return null;
            day = Current<LoboDayScreen>();
            Assert.IsTrue(Module.Game.Jugadores[2].Capitan);

            day.ToggleDebate();
            Assert.IsTrue(day.Debate.Running);
            day.AddTime(30);
            Assert.Greater(day.Debate.Remaining, Module.Game.Rules.DebateSegundos);
            day.ToggleDebate();
            Assert.IsFalse(day.Debate.Running, "Pausado");

            CollectionAssert.AreEquivalent(new[] { 0, 2, 3, 4 }, day.Candidates);
            Assert.AreEqual(0, day.Leaders().Count);
            day.SetVotes(0, 3);
            day.SetVotes(3, 1);
            CollectionAssert.AreEqual(new[] { 0 }, day.Leaders());
            day.Lynch();
            yield return null;
            var end = Current<LoboEndScreen>();
            Assert.IsNotNull(end, "Linchado el único lobo, gana el pueblo");
            Assert.AreEqual(LoboGanador.Pueblo, Module.Game.Victoria.Ganador);
            end.PlayAgain();
            yield return null;
            Assert.AreEqual(LoboFase.Reparto, Module.Game.Fase, "Jugar otra: se reparte otra vez");
            Assert.IsNotNull(Current<LoboRevealScreen>());
        }

        [UnityTest]
        public IEnumerator Dia_EmpateSegundaVotacionYNoche()
        {
            _app = new AppTestHarness();
            LoboNightTests.StartNight(_app, Module, LoboRol.Lobo, LoboRol.Lobo, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano,
                LoboRol.Aldeano);
            yield return null;
            yield return WolvesKill(2);
            Current<LoboDawnScreen>().StartDay();
            yield return null;
            Current<LoboDayScreen>().ElectCaptain(3);
            yield return null;
            var day = Current<LoboDayScreen>();
            day.SetVotes(4, 2);
            day.SetVotes(5, 2);
            Assert.AreEqual(2, day.Leaders().Count);
            day.Lynch();
            Assert.AreEqual(LoboFase.Dia, Module.Game.Fase, "Con empate no se lincha a uno");
            day.DeclareTie();
            yield return null;
            day = Current<LoboDayScreen>();
            CollectionAssert.AreEquivalent(new[] { 4, 5 }, day.Candidates, "Segunda votación entre los empatados");
            day.SetVotes(5, 1);
            day.Lynch();
            yield return null;
            Assert.IsFalse(Module.Game.Jugadores[5].Vivo);
            Assert.IsTrue(Module.Game.LinchamientoHecho);
            Current<LoboDayScreen>().StartNight();
            yield return null;
            Assert.IsNotNull(Current<LoboNightScreen>());
            Assert.AreEqual(2, Module.Game.Ronda);
        }

        [UnityTest]
        public IEnumerator Final_ElMasterLaTerminaSinGanador()
        {
            _app = new AppTestHarness();
            LoboNightTests.StartNight(_app, Module, LoboRol.Lobo, LoboRol.Lobo, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano);
            yield return null;
            var players = new LoboPlayersScreen(Module, Module.GoToPhase);
            _app.Router.Push(players);
            yield return null;
            players.End();
            yield return null;
            Assert.AreEqual(LoboFase.Final, Module.Game.Fase);
            Assert.IsNull(Module.Game.Victoria);
            Assert.IsNotNull(Current<LoboEndScreen>());
        }
    }
}
