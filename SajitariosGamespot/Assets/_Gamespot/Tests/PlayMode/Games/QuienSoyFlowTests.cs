using System.Collections;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Sensors;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.QuienSoy;
using Gamespot.Games.QuienSoy.Domain;
using Gamespot.Games.Shared;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>V075: ¿Quién soy? con el seed real y el acelerómetro simulado: preparación, inclinar, botones y rondas.</summary>
    public sealed class QuienSoyFlowTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown()
        {
            MotionSensor.Override = null;
            Dialogs.CloseAll();
            _app?.Dispose();
        }

        private QuienSoyModule Module => _app.Bootstrap.Games.OfType<QuienSoyGame>().Single().Module;

        private QuienSoySetupScreen OpenSetup()
        {
            _app = new AppTestHarness();
            _app.Router.Go(QuienSoyModule.SetupRoute);
            return (QuienSoySetupScreen)_app.Router.Current;
        }

        [UnityTest]
        public IEnumerator Turno_InclinandoElMovil()
        {
            var setup = OpenSetup();
            yield return null;
            Assert.IsTrue(setup.TeamsVisible, "Por equipos por defecto");
            Module.Draft.Rondas = 1;
            Module.Draft.Pases = 1;
            setup.Teams.SetName(0, "Rojos");
            setup.Teams.SetName(1, "Azules");
            setup.Start();
            yield return null;
            ((GuessReadyScreen)_app.Router.Current).StartTurn();
            yield return null;

            var turn = (QuienSoyTurnScreen)_app.Router.Current;
            Assert.IsTrue(turn.Preparing, "Primero, 3 s para ponérselo");
            MotionSensor.Override = new Vector3(0.95f, 0f, 0.05f);
            turn.Correct();
            Assert.AreEqual(0, Module.Flow.Aciertos, "Durante la preparación no cuenta");
            turn.Tick(QuienSoyTurnScreen.PrepSeconds + 0.1f);
            Assert.IsFalse(turn.Preparing);
            Assert.IsTrue(turn.Timer.Running);
            Assert.AreEqual(90f, turn.Frame.Angle, "Lado derecho abajo: texto girado a +90°");

            MotionSensor.Override = new Vector3(0.95f, 0f, 0.85f);
            turn.Tick(0.1f);
            Assert.AreEqual(1, Module.Flow.Aciertos, "Abajo: acierto");
            turn.Tick(0.1f);
            Assert.AreEqual(1, Module.Flow.Aciertos, "Hasta volver no cuenta otra");
            MotionSensor.Override = new Vector3(0.95f, 0f, 0f);
            turn.Tick(0.1f);
            MotionSensor.Override = new Vector3(0.95f, 0f, -0.85f);
            turn.Tick(0.1f);
            Assert.AreEqual(1, Module.Flow.Fallos, "Arriba: pasar");
            MotionSensor.Override = new Vector3(0.95f, 0f, 0f);
            turn.Tick(0.1f);
            MotionSensor.Override = new Vector3(0.95f, 0f, -0.85f);
            turn.Tick(0.1f);
            Assert.AreEqual(1, Module.Flow.Fallos, "Solo un pase por turno");
            turn.Correct();
            Assert.AreEqual(2, Module.Flow.Aciertos, "Los botones también");
            turn.Timer.Tick(61f);
            yield return null;

            var summary = (GuessSummaryScreen)_app.Router.Current;
            summary.Confirm();
            yield return null;
            ((GuessReadyScreen)_app.Router.Current).StartTurn();
            yield return null;
            turn = (QuienSoyTurnScreen)_app.Router.Current;
            turn.BeginGuessing();
            turn.Correct();
            turn.TimeUp();
            yield return null;
            ((GuessSummaryScreen)_app.Router.Current).Confirm();
            yield return null;
            Assert.IsInstanceOf<GuessEndScreen>(_app.Router.Current, "Una ronda: se acaba");
            CollectionAssert.AreEqual(new[] { 0 }, Module.Flow.Match.Ganadores);
        }

        [UnityTest]
        public IEnumerator CadaUno_SinInclinarSoloBotones()
        {
            var setup = OpenSetup();
            yield return null;
            setup.SelectMode(QuienSoyModo.CadaUno);
            Assert.IsFalse(setup.TeamsVisible);
            Module.Draft.Inclinar = false;
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Bea");
            setup.Players.AddPlayer();
            setup.Players.SetName(2, "Carlos");
            setup.Start();
            yield return null;
            Assert.AreEqual(3, Module.Flow.Match.Equipos.Count, "Cada jugador juega solo");
            ((GuessReadyScreen)_app.Router.Current).StartTurn();
            yield return null;
            var turn = (QuienSoyTurnScreen)_app.Router.Current;
            turn.BeginGuessing();
            MotionSensor.Override = new Vector3(0f, 0f, 0.9f);
            turn.Tick(0.1f);
            Assert.AreEqual(0, Module.Flow.Aciertos, "Sin inclinar, el gesto no cuenta");
            turn.Pass();
            turn.Pass();
            Assert.AreEqual(2, Module.Flow.Fallos, "Pases sin límite por defecto");
            StringAssert.IsMatch("[A-ZÁÉÍÓÚÑÜ0-9]", turn.Word);
        }
    }
}
