using System.Collections;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Settings;
using Gamespot.Games.EsUn10Pero;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    public sealed class EsUn10PeroScreenTests
    {
        private AppTestHarness _app;
        private bool _audioWasEnabled;

        [SetUp]
        public void SetUp()
        {
            _audioWasEnabled = AppSettings.AudioEnabled;
            AppSettings.AudioEnabled = true;
        }

        [TearDown]
        public void TearDown()
        {
            _app?.Dispose();
            AppSettings.AudioEnabled = _audioWasEnabled;
        }

        private EsUn10PeroScreen Open()
        {
            _app = new AppTestHarness();
            _app.Router.Go(EsUn10PeroGame.Route);
            return (EsUn10PeroScreen)_app.Router.Current;
        }

        [UnityTest]
        public IEnumerator EnElMenuApareceYAbreElJuego()
        {
            _app = new AppTestHarness();
            yield return AppTestHarness.Frames();
            var menu = (Gamespot.Menu.MenuScreen)_app.Router.Current;
            Assert.AreEqual(_app.Bootstrap.Games.Count, menu.Cards.Count);
            menu.Cards[0].Button.onClick.Invoke();
            yield return null;
            Assert.IsInstanceOf<EsUn10PeroScreen>(_app.Router.Current);
        }

        [UnityTest]
        public IEnumerator CuentaAtras_DeshabilitaElBotonYSacaLaCartaALos5Segundos()
        {
            var screen = Open();
            yield return AppTestHarness.Frames();
            var audio = _app.Bootstrap.Audio;
            var played = audio.PlayCount;

            Assert.AreEqual(Loc.T("sacarCarta"), screen.DrawButton.Label.text);
            screen.OnDrawPressed();
            Assert.AreEqual(5, screen.Countdown);
            Assert.IsFalse(screen.DrawButton.Interactable, "Deshabilitado durante la cuenta atrás");
            Assert.AreEqual(Loc.T("sacarCarta"), screen.DrawButton.Label.text, "Durante la primera cuenta atrás sigue diciendo «Sacar carta»");

            for (var i = 0; i < 4; i++) screen.Tick(1f);
            Assert.AreEqual(1, screen.Countdown);
            Assert.IsNull(screen.Card, "La carta se saca al terminar, no al pulsar");
            Assert.AreEqual(played, audio.PlayCount, "No suena antes de los 5 s");

            screen.Tick(1f);
            Assert.AreEqual(0, screen.Countdown);
            Assert.IsNotNull(screen.Card);
            Assert.IsTrue(screen.DrawButton.Interactable);
            Assert.AreEqual(Loc.T("sacarOtraCarta"), screen.DrawButton.Label.text);
            Assert.AreEqual(played + 1, audio.PlayCount);
            Assert.AreEqual(AppSound.CardFlip, audio.LastPlayed);
        }

        [UnityTest]
        public IEnumerator PulsarDuranteLaCuentaAtras_NoHaceNada()
        {
            var screen = Open();
            yield return null;
            screen.OnDrawPressed();
            screen.Tick(1f);
            screen.OnDrawPressed();
            Assert.AreEqual(4, screen.Countdown);
        }

        [UnityTest]
        public IEnumerator Silenciado_NoSuena()
        {
            var screen = Open();
            yield return null;
            AppSettings.AudioEnabled = false;
            var played = _app.Bootstrap.Audio.PlayCount;
            screen.OnDrawPressed();
            for (var i = 0; i < 5; i++) screen.Tick(1f);
            Assert.IsNotNull(screen.Card);
            Assert.AreEqual(played, _app.Bootstrap.Audio.PlayCount);
        }
    }
}
