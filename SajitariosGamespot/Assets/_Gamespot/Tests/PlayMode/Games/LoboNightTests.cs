using System.Collections;
using System.Linq;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.HombresLobo;
using Gamespot.Games.HombresLobo.Domain;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>V079: la consola nocturna del game master (pasos, resultado de la vidente, deshacer y jugadores).</summary>
    public sealed class LoboNightTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown()
        {
            Dialogs.CloseAll();
            _app?.Dispose();
        }

        private HombresLoboModule Module => _app.Bootstrap.Games.OfType<HombresLoboGame>().Single().Module;

        /// <summary>Partida con los roles fijados, ya repartida, en la pantalla de la noche.</summary>
        internal static void StartNight(AppTestHarness app, HombresLoboModule module, params LoboRol[] roles)
        {
            var names = roles.Select((_, i) => $"J{i}").ToList();
            var counts = roles.GroupBy(r => r).ToDictionary(g => g.Key, g => g.Count());
            var (config, error) = LoboConfig.Create(names, counts);
            Assert.IsNull(error);
            module.Game.IniciarConReparto(config, roles);
            for (var i = 0; i < roles.Length; i++) module.Game.SiguienteReparto();
            app.Router.Go(HombresLoboModule.NightRoute);
        }

        [UnityTest]
        public IEnumerator Noche_PasosConResultadoYDeshacer()
        {
            _app = new AppTestHarness();
            StartNight(_app, Module, LoboRol.Lobo, LoboRol.Vidente, LoboRol.Bruja, LoboRol.Aldeano, LoboRol.Aldeano);
            yield return null;
            var night = (LoboNightScreen)_app.Router.Current;
            Assert.AreEqual(LoboPaso.Vidente, night.Step.Paso);
            night.Confirm();
            Assert.AreEqual(LoboPaso.Vidente, Module.Game.Paso.Paso, "Sin señalar a nadie no se puede");
            night.Pick(0);
            night.Confirm();
            yield return null;
            Assert.IsTrue(Dialogs.IsOpen, "Se enseña lo que ve la vidente");
            Dialogs.CloseAll();
            night = (LoboNightScreen)_app.Router.Current;
            Assert.AreEqual(LoboPaso.Lobos, night.Step.Paso);
            night.Pick(3);
            night.Confirm();
            yield return null;
            night = (LoboNightScreen)_app.Router.Current;
            Assert.AreEqual(LoboPaso.BrujaSalva, night.Step.Paso);
            night.Undo();
            yield return null;
            night = (LoboNightScreen)_app.Router.Current;
            Assert.AreEqual(LoboPaso.Lobos, night.Step.Paso, "Deshacer vuelve a los lobos");
            night.Pick(4);
            night.Confirm();
            yield return null;
            ((LoboNightScreen)_app.Router.Current).Choose(LoboOpcion.No);
            yield return null;
            night = (LoboNightScreen)_app.Router.Current;
            Assert.AreEqual(LoboPaso.BrujaEnvenena, night.Step.Paso);
            night.Confirm();
            yield return null;
            Assert.AreEqual(LoboFase.Amanecer, Module.Game.Fase, "A nadie: se acaba la noche");
            Assert.IsFalse(Module.Game.Jugadores[4].Vivo);
            Assert.IsTrue(Module.Game.Jugadores[3].Vivo);
        }

        [UnityTest]
        public IEnumerator Jugadores_MatarYRevivirAMano()
        {
            _app = new AppTestHarness();
            StartNight(_app, Module, LoboRol.Lobo, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano);
            yield return null;
            var players = new LoboPlayersScreen(Module, null);
            _app.Router.Push(players);
            yield return null;
            Module.Game.MatarManual(2);
            players.Revive(2);
            Assert.IsTrue(Module.Game.Jugadores[2].Vivo);
            _app.Router.Push(new LoboLogScreen(Module));
            yield return null;
            Assert.IsTrue(Module.Game.Registro.Any(e => e.Tipo == LoboEventoTipo.Revive));
        }
    }
}
