using System.Collections;
using System.Linq;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Wavelength;
using Gamespot.Games.Wavelength.Domain;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>
    /// Wavelength estaba roto en el Flutter: este test juega una partida completa como lo haría el grupo, arrastrando
    /// el dial con eventos de puntero reales (pantalla → punto local) y comprobando el revelado.
    /// </summary>
    public sealed class WavelengthFlowTests
    {
        private AppTestHarness _app;
        private GameObject _eventSystem;

        [SetUp]
        public void SetUp()
        {
            if (EventSystem.current == null) _eventSystem = new GameObject("EventSystem", typeof(EventSystem));
        }

        [TearDown]
        public void TearDown()
        {
            _app?.Dispose();
            if (_eventSystem != null) Object.Destroy(_eventSystem);
        }

        private WavelengthModule Module => _app.Bootstrap.Games.OfType<WavelengthGame>().Single().Module;

        /// <summary>Arrastra la aguja hasta la posición <paramref name="p"/> con eventos de puntero en coordenadas de pantalla.</summary>
        private void DragTo(WavelengthDial dial, float p)
        {
            var camera = _app.Bootstrap.Ui.Canvas.worldCamera;
            var local = dial.LocalPointOf(p, 60f);
            var screen = RectTransformUtility.WorldToScreenPoint(camera, dial.transform.TransformPoint(local));
            // pressEventCamera sale del raycaster del Canvas, como cuando toca un dedo de verdad.
            var raycaster = _app.Bootstrap.Ui.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            var data = new PointerEventData(EventSystem.current)
            {
                position = screen,
                pointerPressRaycast = new RaycastResult { module = raycaster, gameObject = dial.gameObject }
            };
            ExecuteEvents.Execute(dial.gameObject, data, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(dial.gameObject, data, ExecuteEvents.beginDragHandler);
            ExecuteEvents.Execute(dial.gameObject, data, ExecuteEvents.dragHandler);
        }

        [UnityTest]
        public IEnumerator PartidaCompleta_ArrastrandoElDial()
        {
            _app = new AppTestHarness();
            _app.Router.Go(WavelengthModule.SetupRoute);
            var setup = (WavelengthSetupScreen)_app.Router.Current;
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Luis");
            Module.DraftRounds = 2;
            _app.Router.Go(WavelengthModule.SetupRoute);
            setup = (WavelengthSetupScreen)_app.Router.Current;
            setup.Start();
            yield return AppTestHarness.Frames();

            for (var round = 0; round < 2; round++)
            {
                var clue = (WavelengthClueScreen)_app.Router.Current;
                Assert.AreEqual(DialMode.Clue, clue.Dial.Mode, "El psíquico ve el objetivo");
                Assert.AreEqual(round == 0 ? "Ana" : "Luis", Module.Flow.CurrentPsychic);
                clue.Confirm("  ");
                Assert.IsInstanceOf<WavelengthClueScreen>(_app.Router.Current, "Pista vacía: se queda y avisa");
                clue.Confirm("una pista");
                yield return null;

                ((WavelengthPassScreen)_app.Router.Current).Continue();
                yield return AppTestHarness.Frames();
                var guess = (WavelengthGuessScreen)_app.Router.Current;
                Assert.AreEqual(0.5f, guess.Dial.Needle, "La aguja empieza en el centro");
                var target = (float)Module.Flow.CurrentRound.Target;
                DragTo(guess.Dial, target);
                Assert.AreEqual(target, guess.Dial.Needle, 0.01f, "El arrastre mueve la aguja");
                guess.Confirm();
                yield return AppTestHarness.Frames();

                var reveal = (WavelengthRevealScreen)_app.Router.Current;
                Assert.AreEqual(DialMode.Reveal, reveal.Dial.Mode);
                Assert.AreEqual(target, reveal.Dial.Needle, 0.01f, "La aguja del revelado es la del grupo (en el Flutter salía en 0,5)");
                Assert.AreEqual(Loc.T("wavelengthBandBlanco"), reveal.BandText);
                Assert.AreEqual(Loc.T("wavelengthRevealTotalPuntos", ("pts", 4 * (round + 1))), reveal.TotalText, "El total incluye esta ronda");
                yield return new WaitForSecondsRealtime(WavelengthDial.RevealSeconds + 0.1f);
                Assert.AreEqual(1f, reveal.Dial.RevealProgress, "La animación de revelado termina");
                reveal.Next();
                reveal.Next();
                yield return null;
            }

            Assert.IsInstanceOf<WavelengthGameOverScreen>(_app.Router.Current);
            Assert.AreEqual(8, Module.Flow.Session.CumulativeScore);
            Assert.AreEqual(2, Module.Flow.Session.CompletedRounds.Count, "El doble toque en «Siguiente» no salta rondas");
        }

        [UnityTest]
        public IEnumerator EnElDial_ElArrastreNoLoRobaNingunScroll()
        {
            _app = new AppTestHarness();
            _app.Router.Go(WavelengthModule.SetupRoute);
            var setup = (WavelengthSetupScreen)_app.Router.Current;
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Luis");
            setup.Start();
            ((WavelengthClueScreen)_app.Router.Current).Confirm("pista");
            ((WavelengthPassScreen)_app.Router.Current).Continue();
            yield return AppTestHarness.Frames();
            var guess = (WavelengthGuessScreen)_app.Router.Current;
            Assert.IsNull(guess.Dial.GetComponentInParent<UnityEngine.UI.ScrollRect>(), "El dial no está dentro de un scroll");
            var handler = ExecuteEvents.GetEventHandler<IDragHandler>(guess.Dial.gameObject);
            Assert.AreEqual(guess.Dial.gameObject, handler, "El arrastre lo recibe el dial");
        }
    }
}
