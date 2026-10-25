using System.Collections;
using System.Linq;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.Tabu;
using Gamespot.Games.Tabu.Domain;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    public sealed class TabuFlowTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown() => _app?.Dispose();

        private TabuModule Module => _app.Bootstrap.Games.OfType<TabuGame>().Single().Module;

        private TabuTurnScreen Start(int seconds = 30)
        {
            _app = new AppTestHarness();
            _app.Router.Go(TabuModule.SetupRoute);
            var setup = (TabuSetupScreen)_app.Router.Current;
            setup.TeamA.Value = "Rojos";
            setup.TeamB.Value = "Azules";
            setup.SelectSeconds(seconds);
            setup.Start();
            return (TabuTurnScreen)_app.Router.Current;
        }

        [UnityTest]
        public IEnumerator Setup_EquiposIgualesDaAviso()
        {
            _app = new AppTestHarness();
            _app.Router.Go(TabuModule.SetupRoute);
            var setup = (TabuSetupScreen)_app.Router.Current;
            setup.TeamA.Value = "Rojos";
            setup.TeamB.Value = " rojos ";
            setup.Start();
            yield return null;
            Assert.AreEqual(Loc.T("tabuErrorEquiposDuplicados"), Snackbar.CurrentText);
        }

        [UnityTest]
        public IEnumerator Turno_TemporizadorTerminaElTurnoYVaAlMarcador()
        {
            var turn = Start(30);
            yield return null;
            Assert.AreEqual(30, turn.SecondsShown);
            Assert.That(Module.Flow.Session.PalabraActual, Is.Not.Null);
            turn.Correct();
            Assert.AreEqual(1, Module.Flow.Session.AciertosTurno);
            for (var i = 0; i < 29; i++) turn.Tick(1f);
            Assert.IsInstanceOf<TabuTurnScreen>(_app.Router.Current, "A falta de 1 s sigue el turno");
            turn.Tick(1.1f);
            yield return null;
            Assert.IsInstanceOf<TabuScoreboardScreen>(_app.Router.Current);
            Assert.AreEqual(1, Module.Flow.Session.VictoriasA);
            Assert.AreEqual(TabuEquipo.B, Module.Flow.Session.EquipoActual);
        }

        [UnityTest]
        public IEnumerator Turno_ElTemporizadorSePausaConElDialogoDeSalir()
        {
            var turn = Start(30);
            yield return null;
            AbandonGameDialog.Show(() => { });
            turn.Tick(5f);
            Assert.AreEqual(30, turn.SecondsShown, "Con el diálogo abierto el tiempo no corre");
            Dialogs.CloseAll();
            turn.Tick(5f);
            Assert.AreEqual(25, turn.SecondsShown);
        }

        [UnityTest]
        public IEnumerator Partida_PrimeroATresGana()
        {
            var turn = Start(30);
            yield return null;
            for (var t = 0; t < 5 && Module.Flow.Fase != TabuFase.GameOver; t++)
            {
                turn = (TabuTurnScreen)_app.Router.Current;
                if (Module.Flow.Session.EquipoActual == TabuEquipo.A) turn.Correct();
                turn.Tick(31f);
                yield return null;
                if (_app.Router.Current is TabuScoreboardScreen board) board.Next();
            }
            Assert.IsInstanceOf<TabuGameOverScreen>(_app.Router.Current);
            Assert.AreEqual(TabuEquipo.A, Module.Flow.Session.Ganador);
        }
    }
}
