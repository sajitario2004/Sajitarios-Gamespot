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

        [SetUp]
        public void SetUp()
        {
            _savedReduceMotion = PlayerPrefs.HasKey(AppSettings.ReduceMotionKey) ? PlayerPrefs.GetInt(AppSettings.ReduceMotionKey) : (int?)null;
            _savedVibration = AppSettings.VibrationEnabled;
        }

        [TearDown]
        public void TearDown()
        {
            _app?.Dispose();
            if (_savedReduceMotion.HasValue) PlayerPrefs.SetInt(AppSettings.ReduceMotionKey, _savedReduceMotion.Value);
            else PlayerPrefs.DeleteKey(AppSettings.ReduceMotionKey);
            AppSettings.VibrationEnabled = _savedVibration;
        }

        [UnityTest]
        public IEnumerator LosPulsablesMidenAlMenos48()
        {
            _app = new AppTestHarness();
            var small = new List<string>();
            foreach (var route in new[] { "/", "/impostor", "/trivia", "/wavelength", "/tabu", "/yo-nunca", "/bomba", "/es-un-10-pero", "/impostor/words" })
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
        public IEnumerator LosAjustesCambianVibracionYAnimaciones()
        {
            _app = new AppTestHarness();
            _app.Router.Go("/");
            yield return null;
            var (sound, vibration, reduceMotion) = SettingsDialog.Show(_app.Bootstrap.Audio);
            Assert.IsNotNull(sound, "Con audio, también el sonido");
            Assert.AreEqual(AppSettings.ReduceMotion, reduceMotion.Value);
            var motionWas = AppSettings.ReduceMotion;
            Tap(reduceMotion);
            Assert.AreEqual(!motionWas, AppSettings.ReduceMotion);
            var vibrationWas = AppSettings.VibrationEnabled;
            Tap(vibration);
            Assert.AreEqual(!vibrationWas, AppSettings.VibrationEnabled);
            Assert.IsTrue(Dialogs.IsOpen);
            Dialogs.CloseAll();
        }
    }
}
