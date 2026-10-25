using System.Collections;
using System.Linq;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Bomba;
using Gamespot.Games.Bomba.Domain;
using Gamespot.Games.Shared;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    public sealed class BombaFlowTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown() => _app?.Dispose();

        private BombaModule Module => _app.Bootstrap.Games.OfType<BombaGame>().Single().Module;

        private BombaSetupScreen OpenSetup()
        {
            _app = new AppTestHarness();
            _app.Router.Go(BombaModule.SetupRoute);
            return (BombaSetupScreen)_app.Router.Current;
        }

        private BombaPlayScreen Start(BombaMode mode, params string[] names)
        {
            var setup = OpenSetup();
            setup.SelectMode(mode);
            for (var i = 2; i < names.Length; i++) setup.Players.AddPlayer();
            for (var i = 0; i < names.Length; i++) setup.Players.SetName(i, names[i]);
            setup.Start();
            return (BombaPlayScreen)_app.Router.Current;
        }

        [UnityTest]
        public IEnumerator Setup_NombresRepetidosYPocosJugadores()
        {
            var setup = OpenSetup();
            yield return null;
            setup.Players.SetName(0, "Ana");
            setup.Start();
            Assert.AreEqual(Loc.T("errorNombreVacio"), Snackbar.CurrentText);
            setup.Players.SetName(1, " ana ");
            setup.Start();
            Assert.AreEqual(Loc.T("errorNombresDuplicados"), Snackbar.CurrentText);
            Assert.IsInstanceOf<BombaSetupScreen>(_app.Router.Current);
        }

        [UnityTest]
        public IEnumerator Partida_PasarExplotarYGanaElUltimo()
        {
            var play = Start(BombaMode.Silaba, "Ana", "Bob  Ruiz", "Carlos");
            yield return null;
            Assert.AreEqual(BombaFase.Jugando, Module.Flow.Fase);
            Assert.AreEqual("ANA", play.HolderText);
            Assert.AreEqual(Module.Flow.CurrentPrompt.Texto.ToUpper(Loc.Culture), play.PromptText);
            CollectionAssert.AreEqual(new[] { "Ana", "Bob Ruiz", "Carlos" }, Module.Flow.Config.PlayerNames, "Nombres normalizados");

            play.Pass();
            Assert.AreEqual("BOB RUIZ", play.HolderText);
            var fuse = (float)Module.Flow.FuseSeconds;
            play.Tick(fuse - 0.5f);
            Assert.AreEqual(BombaFase.Jugando, Module.Flow.Fase, "Aún no explota");
            Assert.IsFalse(play.ExplosionVisible);

            play.Tick(1f);
            Assert.AreEqual(BombaFase.Explotando, Module.Flow.Fase);
            Assert.IsTrue(play.ExplosionVisible);
            Assert.AreEqual("Bob Ruiz", play.EliminatedText);
            play.Tick(1f);
            Assert.IsTrue(play.ExplosionVisible, "El aviso dura 1,5 s");
            play.Tick(0.6f);
            Assert.IsFalse(play.ExplosionVisible);
            Assert.AreEqual(BombaFase.Jugando, Module.Flow.Fase);
            Assert.AreEqual("CARLOS", play.HolderText, "La bomba sigue en el siguiente");
            Assert.AreEqual(0f, play.Elapsed, "Mecha nueva");

            play.Tick(61f);
            play.Tick(2f);
            yield return null;
            Assert.IsInstanceOf<BombaGameOverScreen>(_app.Router.Current);
            Assert.AreEqual("Ana", Module.Flow.Winner);
            _app.Router.Back();
            Assert.AreEqual(BombaFase.Setup, Module.Flow.Fase, "Salir del final reinicia");
        }

        [UnityTest]
        public IEnumerator Jugando_LaMechaSePausaConElDialogoDeSalir()
        {
            var play = Start(BombaMode.Categoria, "Ana", "Bob");
            yield return null;
            Assert.AreEqual(BombaMode.Categoria, Module.Flow.CurrentPrompt.Mode);
            _app.Router.Back();
            Assert.IsTrue(Dialogs.IsOpen, "Atrás pide confirmación");
            // El router también llama a Tick en cada frame: se compara contra lo que llevaba consumido.
            var before = play.Elapsed;
            play.Tick(120f);
            Assert.AreEqual(BombaFase.Jugando, Module.Flow.Fase, "Con el diálogo abierto la bomba no explota");
            Assert.AreEqual(before, play.Elapsed);
            Dialogs.CloseAll();
            play.Tick(5f);
            Assert.AreEqual(before + 5f, play.Elapsed, 1e-4f);
        }
    }
}
