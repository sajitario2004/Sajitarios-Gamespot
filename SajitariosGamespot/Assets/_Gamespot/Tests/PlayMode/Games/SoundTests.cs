using System.Collections;
using System.Linq;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Settings;
using Gamespot.Games.Bomba;
using Gamespot.Games.Tabu;
using Gamespot.Games.Trivia;
using Gamespot.Games.Wavelength;
using Gamespot.Games.YoNunca;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>C3 + L1: cada juego suena en su momento y La Bomba vibra al explotar.</summary>
    public sealed class SoundTests
    {
        private AppTestHarness _app;
        private bool _audioWas;
        private bool _vibrationWas;

        [SetUp]
        public void SetUp()
        {
            _audioWas = AppSettings.AudioEnabled;
            _vibrationWas = AppSettings.VibrationEnabled;
            AppSettings.AudioEnabled = true;
            AppSettings.VibrationEnabled = true;
            _app = new AppTestHarness();
        }

        [TearDown]
        public void TearDown()
        {
            _app?.Dispose();
            AppSettings.AudioEnabled = _audioWas;
            AppSettings.VibrationEnabled = _vibrationWas;
        }

        private AudioService Audio => _app.Bootstrap.Audio;

        [UnityTest]
        public IEnumerator Bomba_TicTacQueAceleraYExplosionConVibracion()
        {
            _app.Router.Go(BombaModule.SetupRoute);
            var setup = (BombaSetupScreen)_app.Router.Current;
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Bob");
            setup.Start();
            var play = (BombaPlayScreen)_app.Router.Current;
            yield return null;
            var ticks = Audio.CountOf(AppSound.Tick) + Audio.CountOf(AppSound.Tock);
            Assert.That(ticks, Is.GreaterThanOrEqualTo(1), "Suena nada más empezar");
            for (var i = 0; i < 5; i++) play.Tick(BombaPlayScreen.SlowestTick);
            Assert.That(Audio.CountOf(AppSound.Tick), Is.GreaterThan(0));
            Assert.That(Audio.CountOf(AppSound.Tock), Is.GreaterThan(0), "Alterna tic y tac");
            Assert.Greater(BombaPlayScreen.TickInterval(0f), BombaPlayScreen.TickInterval(0.5f));
            Assert.Greater(BombaPlayScreen.TickInterval(0.5f), BombaPlayScreen.TickInterval(1f));

            var vibrations = Haptics.VibrateCount;
            play.Tick(61f);
            Assert.AreEqual(1, Audio.CountOf(AppSound.Explosion));
            Assert.AreEqual(vibrations + 1, Haptics.VibrateCount);
        }

        [UnityTest]
        public IEnumerator Tabu_PitidosEnLosUltimos5SegundosYBocina()
        {
            _app.Router.Go(TabuModule.SetupRoute);
            var setup = (TabuSetupScreen)_app.Router.Current;
            setup.TeamA.Value = "Rojos";
            setup.TeamB.Value = "Azules";
            setup.SelectSeconds(30);
            setup.Start();
            var turn = (TabuTurnScreen)_app.Router.Current;
            yield return null;
            for (var i = 0; i < 24; i++) turn.Tick(1f);
            Assert.AreEqual(0, Audio.CountOf(AppSound.Beep), "Con 6 s o más no pita");
            for (var i = 0; i < 5; i++) turn.Tick(1f);
            Assert.AreEqual(5, Audio.CountOf(AppSound.Beep), "5, 4, 3, 2 y 1");
            Assert.AreEqual(0, Audio.CountOf(AppSound.Buzzer));
            turn.Tick(1f);
            Assert.AreEqual(1, Audio.CountOf(AppSound.Buzzer));
        }

        [UnityTest]
        public IEnumerator Trivia_AciertoYFallo()
        {
            _app.Router.Go(TriviaModule.SetupRoute);
            var setup = (TriviaSetupScreen)_app.Router.Current;
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Bea");
            setup.Start();
            yield return null;
            var flow = _app.Bootstrap.Games.OfType<TriviaGame>().Single().Module.Flow;
            ((TriviaPassScreen)_app.Router.Current).Continue();
            yield return null;
            ((TriviaQuestionScreen)_app.Router.Current).Choose(flow.CurrentQuestion.CorrectIndex);
            Assert.AreEqual(1, Audio.CountOf(AppSound.Correct));
            ((TriviaQuestionScreen)_app.Router.Current).Tick(1f);
            yield return null;
            ((TriviaPassScreen)_app.Router.Current).Continue();
            yield return null;
            ((TriviaQuestionScreen)_app.Router.Current).Choose((flow.CurrentQuestion.CorrectIndex + 1) % 4);
            Assert.AreEqual(1, Audio.CountOf(AppSound.Wrong));
        }

        [UnityTest]
        public IEnumerator Wavelength_RevelaConCampanas_YYoNuncaPasaPagina()
        {
            _app.Router.Go(WavelengthModule.SetupRoute);
            var setup = (WavelengthSetupScreen)_app.Router.Current;
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Luis");
            setup.Start();
            ((WavelengthClueScreen)_app.Router.Current).Confirm("pista");
            yield return null;
            ((WavelengthPassScreen)_app.Router.Current).Continue();
            yield return AppTestHarness.Frames();
            ((WavelengthGuessScreen)_app.Router.Current).Confirm();
            Assert.AreEqual(1, Audio.CountOf(AppSound.Chime));

            _app.Router.Go(YoNuncaModule.SetupRoute);
            ((YoNuncaSetupScreen)_app.Router.Current).Start();
            yield return null;
            ((YoNuncaPlayScreen)_app.Router.Current).Next();
            Assert.AreEqual(1, Audio.CountOf(AppSound.Swipe));
        }

        [UnityTest]
        public IEnumerator Silenciado_NoSuenaPeroLaBombaVibra()
        {
            AppSettings.AudioEnabled = false;
            _app.Router.Go(BombaModule.SetupRoute);
            var setup = (BombaSetupScreen)_app.Router.Current;
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Bob");
            setup.Start();
            yield return null;
            var vibrations = Haptics.VibrateCount;
            ((BombaPlayScreen)_app.Router.Current).Tick(61f);
            Assert.AreEqual(0, Audio.CountOf(AppSound.Explosion));
            Assert.AreEqual(vibrations + 1, Haptics.VibrateCount, "La vibración va aparte del sonido");
        }
    }
}
