using System.Collections;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.HombresLobo;
using Gamespot.Games.HombresLobo.Domain;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>V081: los eventos de luna en la consola del game master y la guía de cartas.</summary>
    public sealed class LoboMoonTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown()
        {
            Dialogs.CloseAll();
            _app?.Dispose();
        }

        private HombresLoboModule Module => _app.Bootstrap.Games.OfType<HombresLoboGame>().Single().Module;

        private T Current<T>() where T : class => _app.Router.Current as T;

        /// <summary>
        /// Cada mañana sale la luna <paramref name="luna"/>: un <c>NextDouble()</c> por debajo de la probabilidad y
        /// después <c>NextInt(8)</c> (el guion se repite).
        /// </summary>
        private void StartWithMoon(LoboLuna luna, params LoboRol[] roles)
        {
            _app = new AppTestHarness(random: new ScriptedRandom(true, 0.0, ((int)luna + 0.5) / LoboLunas.All.Count));
            var names = roles.Select((_, i) => $"J{i}").ToList();
            var counts = roles.GroupBy(r => r).ToDictionary(g => g.Key, g => g.Count());
            var (config, error) = LoboConfig.Create(names, counts, new LoboRules { EventosLuna = true });
            Assert.IsNull(error);
            Module.Game.IniciarConReparto(config, roles);
            for (var i = 0; i < roles.Length; i++) Module.Game.SiguienteReparto();
            _app.Router.Go(HombresLoboModule.NightRoute);
        }

        /// <summary>Los lobos matan a <paramref name="victim"/>, se empieza el día y se elige capitán.</summary>
        private IEnumerator ToDay(int victim, int captain)
        {
            var night = Current<LoboNightScreen>();
            night.Pick(victim);
            night.Confirm();
            yield return null;
            var dawn = Current<LoboDawnScreen>();
            Assert.IsNotNull(dawn);
            Assert.IsNotNull(_app.Router.Current.Root.GetComponentsInChildren<NeonPanel>(true)
                .FirstOrDefault(p => p.name == Loc.T("loboLunaHoy", ("luna", Loc.T("loboLuna" + Module.Game.Luna)))), "El amanecer enseña la luna");
            dawn.StartDay();
            yield return null;
            Current<LoboDayScreen>().ElectCaptain(captain);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Tormenta_HoySeVotaDosVeces()
        {
            StartWithMoon(LoboLuna.Tormenta, LoboRol.Lobo, LoboRol.Lobo, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano,
                LoboRol.Aldeano);
            yield return null;
            yield return ToDay(2, 3);
            Assert.AreEqual(LoboLuna.Tormenta, Module.Game.Luna);
            var day = Current<LoboDayScreen>();
            day.SetVotes(4, 1);
            day.Lynch();
            yield return null;
            day = Current<LoboDayScreen>();
            Assert.IsFalse(Module.Game.LinchamientoHecho, "Queda la segunda votación");
            CollectionAssert.DoesNotContain(day.Candidates, 4);
            day.SetVotes(5, 1);
            day.Lynch();
            yield return null;
            Assert.IsTrue(Module.Game.LinchamientoHecho);
            Assert.IsFalse(Module.Game.Jugadores[4].Vivo);
            Assert.IsFalse(Module.Game.Jugadores[5].Vivo);
        }

        [UnityTest]
        public IEnumerator Eclipse_SinDebateYLaNocheEnsenaLaLuna()
        {
            StartWithMoon(LoboLuna.Eclipse, LoboRol.Lobo, LoboRol.Lobo, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano,
                LoboRol.Aldeano);
            yield return null;
            yield return ToDay(2, 3);
            var day = Current<LoboDayScreen>();
            Assert.IsFalse(day.ShowsDebate, "Con eclipse se vota directamente");
            day.NoLynch();
            yield return null;
            Current<LoboDayScreen>().StartNight();
            yield return null;
            Assert.IsNotNull(Current<LoboNightScreen>());
            Assert.IsNotNull(_app.Router.Current.Root.GetComponentsInChildren<NeonPanel>(true)
                .FirstOrDefault(p => p.name == Loc.T("loboLunaHoy", ("luna", Loc.T("loboLunaEclipse")))), "La primera pantalla de la noche recuerda la luna");
        }

        [UnityTest]
        public IEnumerator Guia_CartasDeLaPartidaYLunas()
        {
            StartWithMoon(LoboLuna.Niebla, LoboRol.Lobo, LoboRol.Vidente, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano);
            yield return null;
            _app.Router.Push(new LoboGuideScreen(Module));
            yield return null;
            var panels = _app.Router.Current.Root.GetComponentsInChildren<NeonPanel>(true).Select(p => p.name).ToList();
            CollectionAssert.Contains(panels, LoboUi.Name(LoboRol.Vidente));
            CollectionAssert.Contains(panels, Loc.T("loboRolesVarios", ("n", 3), ("rol", LoboUi.Name(LoboRol.Aldeano))));
            CollectionAssert.DoesNotContain(panels, LoboUi.Name(LoboRol.Bruja), "Solo las cartas de la partida");
            CollectionAssert.Contains(panels, Loc.T("loboLunaNiebla"));
        }
    }
}
