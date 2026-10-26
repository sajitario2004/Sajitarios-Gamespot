using System.Collections;
using System.Linq;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.HombresLobo;
using Gamespot.Games.HombresLobo.Domain;
using NUnit.Framework;
using TMPro;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>V078: reparto de Hombres Lobo pasando el móvil, con lo que ve cada uno, hasta dárselo al game master.</summary>
    public sealed class LoboRevealTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown()
        {
            Dialogs.CloseAll();
            _app?.Dispose();
        }

        private HombresLoboModule Module => _app.Bootstrap.Games.OfType<HombresLoboGame>().Single().Module;

        private static string AllText() =>
            string.Join("\n", UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(UnityEngine.FindObjectsSortMode.None)
                .Where(t => t.isActiveAndEnabled).Select(t => t.text));

        [UnityTest]
        public IEnumerator Reparto_CadaUnoVeSuCartaYAlFinalElMaster()
        {
            _app = new AppTestHarness();
            var roles = new[] { LoboRol.Lobo, LoboRol.Vidente, LoboRol.Lobo, LoboRol.Aldeano, LoboRol.Aldeano };
            var counts = roles.GroupBy(r => r).ToDictionary(g => g.Key, g => g.Count());
            var (config, error) = LoboConfig.Create(new[] { "Ana", "Bea", "Carlos", "Dani", "Eva" }, counts);
            Assert.IsNull(error);
            Module.Game.IniciarConReparto(config, roles);
            _app.Router.Go(HombresLoboModule.RevealRoute);
            yield return null;

            var reveal = (LoboRevealScreen)_app.Router.Current;
            Assert.IsFalse(reveal.Revealed);
            StringAssert.Contains("Ana", AllText());
            StringAssert.DoesNotContain(Loc.T("loboEres"), AllText(), "Sin pulsar, no se ve la carta");
            reveal.Reveal();
            yield return null;
            StringAssert.Contains(Loc.T("loboEres"), AllText());
            StringAssert.Contains(LoboUi.Description(LoboRol.Lobo), AllText());
            StringAssert.Contains("Carlos", AllText(), "Los lobos ven a sus compañeros");
            reveal.Next();
            yield return null;
            Assert.AreEqual(1, Module.Game.RepartoIndex);
            Assert.IsFalse(reveal.Revealed, "Se oculta al pasar");
            for (var i = 1; i < 5; i++)
            {
                reveal.Reveal();
                reveal.Next();
            }
            yield return null;
            Assert.IsTrue(reveal.Handoff);
            Assert.AreEqual(LoboFase.Noche, Module.Game.Fase);
        }

        [UnityTest]
        public IEnumerator Reparto_SinReglaLosLobosNoSeVen()
        {
            _app = new AppTestHarness();
            var roles = new[] { LoboRol.Lobo, LoboRol.Lobo, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano };
            var counts = roles.GroupBy(r => r).ToDictionary(g => g.Key, g => g.Count());
            var (config, _) = LoboConfig.Create(new[] { "Ana", "Bea", "Carlos", "Dani", "Eva" }, counts,
                new LoboRules { LobosSeConocenAlRepartir = false });
            Module.Game.IniciarConReparto(config, roles);
            _app.Router.Go(HombresLoboModule.RevealRoute);
            yield return null;
            var reveal = (LoboRevealScreen)_app.Router.Current;
            reveal.Reveal();
            yield return null;
            StringAssert.DoesNotContain("Bea", AllText());
        }
    }
}
