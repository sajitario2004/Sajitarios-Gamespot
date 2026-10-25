using System.Collections;
using System.Linq;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.HombresLobo;
using Gamespot.Games.HombresLobo.Domain;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>V077: setup de Hombres Lobo (presets que siguen a los jugadores, roles a medida, grupos y validación).</summary>
    public sealed class LoboSetupTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown()
        {
            Dialogs.CloseAll();
            _app?.Dispose();
        }

        private HombresLoboModule Module => _app.Bootstrap.Games.OfType<HombresLoboGame>().Single().Module;

        private LoboSetupScreen OpenSetup()
        {
            _app = new AppTestHarness();
            _app.Router.Go(HombresLoboModule.SetupRoute);
            return (LoboSetupScreen)_app.Router.Current;
        }

        private static void Name(LoboSetupScreen setup, int count)
        {
            while (setup.Players.Count < count) setup.Players.AddPlayer();
            for (var i = 0; i < count; i++) setup.Players.SetName(i, $"Jugador {i + 1}");
        }

        [UnityTest]
        public IEnumerator Preset_SigueAlNumeroDeJugadores()
        {
            var setup = OpenSetup();
            yield return null;
            Assert.AreEqual(LoboPreset.Clasica, Module.DraftPreset, "Clásica por defecto");
            Assert.AreEqual(5, setup.Players.Count);
            Assert.IsTrue(setup.CardsOk);
            setup.Players.AddPlayer();
            setup.Players.AddPlayer();
            setup.Players.AddPlayer();
            setup.Players.AddPlayer();
            Assert.AreEqual(9, setup.Players.Count);
            Assert.IsTrue(setup.CardsOk, "El preset se rehace para 9");
            Assert.IsTrue(Module.DraftRoles.ContainsKey(LoboRol.Ladron), "Con 9, la clásica lleva ladrón (y dos cartas más)");
            Assert.AreEqual(11, LoboConfig.TotalCartas(Module.DraftRoles));
            setup.SelectPreset(LoboPreset.Caos);
            Assert.IsTrue(setup.CardsOk);
            Assert.IsTrue(Module.DraftRoles.ContainsKey(LoboRol.GranLoboMalo));
        }

        [UnityTest]
        public IEnumerator Roles_AMedidaConGruposYAjusteConAldeanos()
        {
            var setup = OpenSetup();
            yield return null;
            Name(setup, 8);
            // Iniciación para 8: dos lobos, vidente, bruja, cazador y tres aldeanos (sitio para tres hermanos).
            setup.SelectPreset(LoboPreset.Iniciacion);
            var roles = new LoboRolesScreen(Module, 8, setup.RefreshRoles);
            _app.Router.Push(roles);
            yield return null;
            roles.SetCount(LoboCatalog.Get(LoboRol.Hermano), 1);
            Assert.AreEqual(3, Module.DraftRoles[LoboRol.Hermano], "Los hermanos van de tres en tres");
            Assert.IsNull(Module.DraftPreset, "Ya es a medida");
            Assert.IsFalse(setup.CardsOk);
            roles.Fill();
            Assert.IsTrue(setup.CardsOk, "Cuadrado con aldeanos");
            roles.SetCount(LoboCatalog.Get(LoboRol.Hermano), 2);
            Assert.IsFalse(Module.DraftRoles.ContainsKey(LoboRol.Hermano), "Al bajar, fuera los tres");
            _app.Router.Pop();
            yield return null;
            setup.Fill();
            Assert.IsTrue(setup.CardsOk);
        }

        [UnityTest]
        public IEnumerator Empezar_ValidaYReparte()
        {
            var setup = OpenSetup();
            yield return null;
            setup.Start();
            Assert.AreEqual(LoboFase.Setup, Module.Game.Fase, "Sin nombres no empieza");
            Name(setup, 6);
            Module.SetRole(LoboRol.Lobo, 0);
            Module.SetRole(LoboRol.Aldeano, 6);
            setup.RefreshRoles();
            setup.Start();
            Assert.AreEqual(LoboFase.Setup, Module.Game.Fase, "Sin lobos no empieza");
            setup.SelectPreset(LoboPreset.Iniciacion);
            setup.Start();
            yield return null;
            Assert.AreEqual(LoboFase.Reparto, Module.Game.Fase);
            Assert.AreEqual(6, Module.Game.Jugadores.Count);
            Assert.AreEqual("Jugador 1", Module.Game.Jugadores[0].Name);
        }
    }
}
