using System.Collections;
using System.Collections.Generic;
using Gamespot.Core.Runtime.Settings;
using Gamespot.Core.Runtime.UI;
using Gamespot.Menu;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gamespot.Tests.PlayMode
{
    /// <summary>V062: todo lo que se pulsa mide al menos 48 × 48 y los ajustes cambian vibración y animaciones.</summary>
    public sealed class AccessibilityTests
    {
        private AppTestHarness _app;
        private int? _savedReduceMotion;
        private bool _savedVibration;
        private bool _savedAudio;

        [SetUp]
        public void SetUp()
        {
            _savedReduceMotion = PlayerPrefs.HasKey(AppSettings.ReduceMotionKey) ? PlayerPrefs.GetInt(AppSettings.ReduceMotionKey) : (int?)null;
            _savedVibration = AppSettings.VibrationEnabled;
            _savedAudio = AppSettings.AudioEnabled;
        }

        [TearDown]
        public void TearDown()
        {
            _app?.Dispose();
            // Por AppSettings y no por PlayerPrefs: el valor se guarda también en memoria (V063).
            if (_savedReduceMotion.HasValue) AppSettings.ReduceMotion = _savedReduceMotion.Value == 1;
            else AppSettings.ClearReduceMotion();
            AppSettings.VibrationEnabled = _savedVibration;
            AppSettings.AudioEnabled = _savedAudio;
        }

        [UnityTest]
        public IEnumerator LosPulsablesMidenAlMenos48()
        {
            _app = new AppTestHarness();
            var small = new List<string>();
            foreach (var route in new[] { "/", "/impostor", "/trivia", "/wavelength", "/tabu", "/yo-nunca", "/bomba", "/es-un-10-pero", "/impostor/words", "/ajustes" })
            {
                _app.Router.Go(route);
                yield return AppTestHarness.Frames();
                foreach (var selectable in Object.FindObjectsByType<Selectable>(FindObjectsSortMode.None))
                {
                    if (!selectable.isActiveAndEnabled || !selectable.interactable || selectable is Scrollbar) continue;
                    var rect = ((RectTransform)selectable.transform).rect;
                    if (rect.width < NeonButton.MinTouchSize - 0.5f || rect.height < NeonButton.MinTouchSize - 0.5f)
                    {
                        small.Add($"{route}: {selectable.name} ({rect.width:0}×{rect.height:0})");
                    }
                }
            }
            CollectionAssert.IsEmpty(small, "Pulsables de menos de 48");
        }

        /// <summary>Como un toque: el interruptor solo avisa al pulsarlo.</summary>
        private static void Tap(NeonSwitch toggle) => toggle.GetComponentInChildren<Button>().onClick.Invoke();

        [UnityTest]
        public IEnumerator LosAjustesCambianSonidoVibracionYAnimaciones()
        {
            _app = new AppTestHarness();
            _app.Router.Go(SettingsScreen.Route);
            yield return null;
            var settings = (SettingsScreen)_app.Router.Current;
            Assert.IsNotNull(settings.Sound, "Con audio, también el sonido");
            var soundWas = _app.Bootstrap.Audio.Enabled;
            Tap(settings.Sound);
            Assert.AreEqual(!soundWas, _app.Bootstrap.Audio.Enabled, "Quitar el sonido");
            var vibrationWas = AppSettings.VibrationEnabled;
            Tap(settings.Vibration);
            Assert.AreEqual(!vibrationWas, AppSettings.VibrationEnabled, "Quitar la vibración");
            Assert.AreEqual(AppSettings.ReduceMotion, settings.ReduceMotion.Value);
            var motionWas = AppSettings.ReduceMotion;
            Tap(settings.ReduceMotion);
            Assert.AreEqual(!motionWas, AppSettings.ReduceMotion);

            _app.Router.Back();
            yield return null;
            Assert.AreEqual(Gamespot.Core.Runtime.Navigation.Router.Home, _app.Router.CurrentPath, "Atrás vuelve al menú");
        }

        [UnityTest]
        public IEnumerator LosDocumentosLegalesSeLeenYVuelvenALosAjustes()
        {
            _app = new AppTestHarness();
            _app.Router.Go(SettingsScreen.Route);
            yield return null;
            var settings = (SettingsScreen)_app.Router.Current;
            Assert.AreEqual(4, SettingsScreen.LegalDocuments.Length, "Términos, privacidad, cookies y licencias");
            foreach (var (title, body) in SettingsScreen.LegalDocuments)
            {
                var legal = settings.OpenLegal(title, body);
                yield return null;
                Assert.AreSame(legal, _app.Router.Current);
                Assert.AreEqual(Gamespot.Core.Runtime.Localization.Loc.T(body), legal.Body.text);
                Assert.That(legal.Body.text.Length, Is.GreaterThan(200), title);
                _app.Router.Back();
                yield return null;
                Assert.AreSame(settings, _app.Router.Current, "Atrás vuelve a los ajustes");
            }
        }
    }
}
