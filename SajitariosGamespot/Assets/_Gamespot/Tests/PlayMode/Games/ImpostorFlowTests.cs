using System.Collections;
using System.Linq;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Settings;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Impostor;
using Gamespot.Games.Impostor.Domain;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    public sealed class ImpostorFlowTests
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

        private ImpostorModule Module => _app.Bootstrap.Games.OfType<ImpostorGame>().Single().Module;

        private ImpostorSetupScreen OpenSetup(params string[] names)
        {
            _app = new AppTestHarness();
            _app.Router.Go(ImpostorModule.SetupRoute);
            var setup = (ImpostorSetupScreen)_app.Router.Current;
            while (setup.Players.Count < names.Length) setup.Players.AddPlayer();
            for (var i = 0; i < names.Length; i++) setup.Players.SetName(i, names[i]);
            return setup;
        }

        [UnityTest]
        public IEnumerator PartidaCompleta_RevelarAtodosVotarYGuardarHistorial()
        {
            var setup = OpenSetup("Nacho", "Iker", "Lucía", "Ana", "Bea");
            yield return null;
            Assert.AreEqual(2, setup.RoundsStepper.Max, "5 jugadores → máx. 2 rondas");
            Assert.AreEqual(2, setup.RoundsStepper.Value, "Empieza en el máximo (I1; en el Flutter empezaba en 1)");
            setup.Start();
            yield return null;
            Assert.IsInstanceOf<PassDeviceScreen>(_app.Router.Current);
            Assert.AreEqual(2060, Module.Words.Count(), "El seed se cargó en la BD temporal");

            var revealSounds = 0;
            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(i, Module.Flow.CurrentIndex);
                ((PassDeviceScreen)_app.Router.Current).Continue();
                yield return null;
                var reveal = (RevealScreen)_app.Router.Current;
                Assert.IsFalse(reveal.Revealed, "El rol está oculto hasta pulsar Revelar");
                var before = _app.Bootstrap.Audio.PlayCount;
                reveal.Reveal();
                revealSounds += _app.Bootstrap.Audio.PlayCount - before;
                Assert.IsNotNull(reveal.Announcement);
                reveal.HideAndPass();
                yield return null;
            }
            Assert.AreEqual(5, revealSounds);
            Assert.IsInstanceOf<VotingScreen>(_app.Router.Current);

            // Se vota a los impostores que haya (si no hay ninguno, gana el impostor al gastar la ronda).
            var voting = (VotingScreen)_app.Router.Current;
            var target = Module.Flow.ImpostoresVivos.FirstOrDefault() ?? Module.Flow.Candidatos.First();
            while (Module.Flow.Phase == ImpostorPhase.Voting)
            {
                voting.Vote(Module.Flow.ImpostoresVivos.FirstOrDefault() ?? target);
                yield return null;
                if (_app.Router.Current is VotingScreen next) voting = next;
            }
            Assert.IsInstanceOf<GameOverScreen>(_app.Router.Current);
            Assert.AreEqual(AppSound.GameOver, _app.Bootstrap.Audio.LastPlayed);
            Assert.AreEqual(1L, Module.History.Count(), "La partida terminada se guarda una vez");

            _app.Router.Back();
            yield return null;
            Assert.IsInstanceOf<ImpostorSetupScreen>(_app.Router.Current, "Atrás en el fin vuelve al setup");
            Assert.AreEqual(ImpostorPhase.Setup, Module.Flow.Phase);
            Assert.AreEqual(1L, Module.History.Count());
        }

        [UnityTest]
        public IEnumerator Setup_ErroresDeValidacionEnAviso()
        {
            var setup = OpenSetup("Ana", "ana", "Bea");
            yield return null;
            setup.Start();
            Assert.IsInstanceOf<ImpostorSetupScreen>(_app.Router.Current);
            Assert.AreEqual(Gamespot.Core.Runtime.Localization.Loc.T("errorNombresDuplicados"), Snackbar.CurrentText);
        }

        [UnityTest]
        public IEnumerator Setup_QuitarJugadoresRecortaLosContadores()
        {
            var setup = OpenSetup("A", "B", "C", "D", "E", "F", "G");
            yield return null;
            setup.ImpostorsStepper.Set(5, true);
            setup.RoundsStepper.Set(4, true);
            for (var i = 0; i < 3; i++) setup.Players.RemovePlayer(0);
            Assert.AreEqual(4, setup.Players.Count);
            Assert.AreEqual(3, setup.ImpostorsStepper.Value, "min(5, 4−1)");
            Assert.AreEqual(1, setup.RoundsStepper.Value, "max(1, 4−3)");
        }

        [UnityTest]
        public IEnumerator Setup_LasRondasSiguenAlMaximoHastaQueSeTocan()
        {
            var setup = OpenSetup("A", "B", "C", "D", "E");
            yield return null;
            Assert.AreEqual(2, setup.RoundsStepper.Value);
            setup.Players.AddPlayer();
            setup.Players.AddPlayer();
            Assert.AreEqual(4, setup.RoundsStepper.Value, "7 jugadores: sube solo a max(1, 7−3)");
            setup.RoundsStepper.Set(3, true);
            setup.Players.AddPlayer();
            Assert.AreEqual(5, setup.RoundsStepper.Max);
            Assert.AreEqual(3, setup.RoundsStepper.Value, "Tocadas: ya no suben solas");
            for (var i = 0; i < 4; i++) setup.Players.RemovePlayer(0);
            Assert.AreEqual(1, setup.RoundsStepper.Value, "Pero sí se recortan");
        }

        [UnityTest]
        public IEnumerator PartidaEnCurso_AtrasPideConfirmarYVuelveAlSetup()
        {
            var setup = OpenSetup("Nacho", "Iker", "Lucía");
            yield return null;
            setup.Start();
            yield return null;
            _app.Router.Back();
            Assert.IsTrue(Dialogs.IsOpen, "Atrás con partida en curso pide confirmación");
            _app.Router.Back();
            Assert.IsFalse(Dialogs.IsOpen);
            Assert.IsInstanceOf<PassDeviceScreen>(_app.Router.Current, "Cancelar sigue en la partida");
        }

        [UnityTest]
        public IEnumerator Borrador_SeConservaAlIrAPalabrasYVolver()
        {
            OpenSetup("Nacho", "Iker", "Lucía", "Ana");
            yield return null;
            _app.Router.Go(ImpostorModule.WordsRoute);
            yield return AppTestHarness.Frames();
            var words = (WordsScreen)_app.Router.Current;
            Assert.AreEqual(2060, words.Items.Count);
            Assert.Greater(words.List.CreatedRows, 0, "Pinta las filas visibles");
            Assert.Less(words.List.CreatedRows, 40, "La lista solo crea las filas visibles");
            _app.Router.Back();
            yield return null;
            var setup = (ImpostorSetupScreen)_app.Router.Current;
            CollectionAssert.AreEqual(new[] { "Nacho", "Iker", "Lucía", "Ana" }, setup.Players.RawNames);
        }
    }
}
