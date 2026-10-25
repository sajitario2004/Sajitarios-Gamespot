using System.Collections;
using System.Linq;
using Gamespot.Core.Runtime.Settings;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Bomba;
using Gamespot.Games.Tabu;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework.Constraints;
using UnityEngine.TestTools.Constraints;
using Is = NUnit.Framework.Is;

namespace Gamespot.Tests.PlayMode
{
    /// <summary>
    /// V063: en reposo no se genera basura cada fotograma (ni en el menú ni con una partida en marcha), los brillos que
    /// laten rehacen el Canvas pocas veces por segundo y sin tocar la pantalla se dibuja a 30 fps.
    /// </summary>
    public sealed class PerformanceTests
    {
        private AppTestHarness _app;
        private bool _reduceMotionWas;

        [SetUp]
        public void SetUp()
        {
            _reduceMotionWas = AppSettings.ReduceMotion;
            AppSettings.ReduceMotion = false;
        }

        [TearDown]
        public void TearDown()
        {
            _app?.Dispose();
            AppSettings.ReduceMotion = _reduceMotionWas;
        }

        [Test]
        public void ElBrilloSeAplicaPocasVecesPorSegundo()
        {
            var go = new GameObject("Pulse");
            try
            {
                var applied = 0;
                var pulse = PulseGlow.Attach(go, 0.7f, 1.2f, _ => applied++);
                var before = pulse.ApplyCount;
                for (var frame = 0; frame < 60; frame++) pulse.Advance(1f / 60f);
                var perSecond = pulse.ApplyCount - before;
                Assert.That(perSecond, Is.InRange(5, 25), "Antes eran 60 por segundo");
                Assert.AreEqual(pulse.ApplyCount, applied);
                Assert.AreEqual(0f, pulse.Intensity % PulseGlow.Step, 1e-4f, "Siempre en escalones");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EnReposoSeDibujaAMitad()
        {
            Assert.AreEqual(1, IdleRendering.IntervalFor(0f));
            Assert.AreEqual(1, IdleRendering.IntervalFor(IdleRendering.IdleSeconds - 0.1f));
            Assert.AreEqual(IdleRendering.IdleInterval, IdleRendering.IntervalFor(IdleRendering.IdleSeconds));
        }

        /// <summary>Lo que corre en cada fotograma de la pantalla actual: su Tick y los brillos que laten.</summary>
        private void AssertFrameDoesNotAllocate(string where)
        {
            var pulses = Object.FindObjectsByType<PulseGlow>(FindObjectsSortMode.None);
            // Una vuelta entera primero: los materiales del halo se crean la primera vez que hacen falta.
            for (var i = 0; i < 80; i++)
            {
                _app.Router.Current.Tick(0.001f);
                foreach (var pulse in pulses) pulse.Advance(0.05f);
            }
            Assert.That(() =>
            {
                _app.Router.Current.Tick(0.001f);
                foreach (var pulse in pulses) pulse.Advance(1f / 60f);
            }, new NotConstraint(new AllocatingGCMemoryConstraint()), where);
        }

        [UnityTest]
        public IEnumerator UnFotogramaEnReposoNoGeneraBasura()
        {
            _app = new AppTestHarness();
            foreach (var route in new[] { "/", "/impostor", "/trivia", "/tabu", "/yo-nunca", "/bomba", "/es-un-10-pero", "/impostor/words" })
            {
                _app.Router.Go(route);
                yield return AppTestHarness.Frames();
                AssertFrameDoesNotAllocate(route);
            }

            _app.Router.Go(TabuModule.SetupRoute);
            var tabu = (TabuSetupScreen)_app.Router.Current;
            tabu.TeamA.Value = "Rojos";
            tabu.TeamB.Value = "Azules";
            tabu.Start();
            yield return AppTestHarness.Frames();
            Assert.IsInstanceOf<TabuTurnScreen>(_app.Router.Current);
            AssertFrameDoesNotAllocate("turno del Tabú");

            _app.Router.Go(BombaModule.SetupRoute);
            var bomba = (BombaSetupScreen)_app.Router.Current;
            bomba.Players.SetName(0, "Ana");
            bomba.Players.SetName(1, "Bea");
            bomba.Start();
            yield return AppTestHarness.Frames();
            Assert.IsInstanceOf<BombaPlayScreen>(_app.Router.Current);
            AssertFrameDoesNotAllocate("mecha de La Bomba");
        }
    }
}
