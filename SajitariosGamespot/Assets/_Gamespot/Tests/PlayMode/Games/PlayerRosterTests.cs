using System.Collections;
using System.Linq;
using Gamespot.Core.Runtime.Settings;
using Gamespot.Games.Bomba;
using Gamespot.Games.Impostor;
using Gamespot.Games.Shared;
using Gamespot.Games.Trivia;
using Gamespot.Games.Wavelength;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>C2: los nombres de la última partida se comparten entre los setups con lista de jugadores.</summary>
    public sealed class PlayerRosterTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown() => _app?.Dispose();

        private static void Fill(PlayerListEditor players, params string[] names)
        {
            while (players.Count < names.Length) players.AddPlayer();
            for (var i = 0; i < names.Length; i++) players.SetName(i, names[i]);
        }

        [UnityTest]
        public IEnumerator SinPartidaPrevia_SetupVacioYSinBoton()
        {
            _app = new AppTestHarness();
            _app.Router.Go(WavelengthModule.SetupRoute);
            yield return null;
            var players = ((WavelengthSetupScreen)_app.Router.Current).Players;
            CollectionAssert.AreEqual(new[] { "", "" }, players.RawNames);
            Assert.IsFalse(players.CanUseLastPlayers);
        }

        [UnityTest]
        public IEnumerator JugarEnUnJuego_RellenaElSetupDeOtro()
        {
            _app = new AppTestHarness();
            _app.Router.Go(BombaModule.SetupRoute);
            var bomba = (BombaSetupScreen)_app.Router.Current;
            Fill(bomba.Players, "Ana", "Bob  Ruiz", "Carlos");
            bomba.Start();
            yield return null;
            CollectionAssert.AreEqual(new[] { "Ana", "Bob Ruiz", "Carlos" }, AppSettings.LastPlayers, "Se guardan normalizados al empezar");

            _app.Router.Go(WavelengthModule.SetupRoute);
            yield return null;
            var players = ((WavelengthSetupScreen)_app.Router.Current).Players;
            CollectionAssert.AreEqual(new[] { "Ana", "Bob Ruiz", "Carlos" }, players.NormalizedNames);
            Assert.IsFalse(players.CanUseLastPlayers, "Ya son los mismos");
        }

        [UnityTest]
        public IEnumerator MasNombresQueElMaximo_SeRecortan()
        {
            _app = new AppTestHarness();
            _app.Router.Go(ImpostorModule.SetupRoute);
            var impostor = (ImpostorSetupScreen)_app.Router.Current;
            var seven = Enumerable.Range(1, 7).Select(i => "Jugador" + i).ToArray();
            Fill(impostor.Players, seven);
            impostor.Start();
            yield return null;
            Assert.AreEqual(7, AppSettings.LastPlayers.Count);

            _app.Router.Go(TriviaModule.SetupRoute);
            yield return null;
            var trivia = ((TriviaSetupScreen)_app.Router.Current).Players;
            CollectionAssert.AreEqual(seven.Take(6), trivia.NormalizedNames, "La trivia admite 6");
        }

        [UnityTest]
        public IEnumerator NombresCambiados_ElBotonRecuperaLosGuardados()
        {
            _app = new AppTestHarness();
            AppSettings.LastPlayers = new[] { "Ana", "Bob", "Carlos" };
            _app.Router.Go(BombaModule.SetupRoute);
            yield return null;
            var players = ((BombaSetupScreen)_app.Router.Current).Players;
            players.RemovePlayer(2);
            players.SetName(0, "Zoe");
            Assert.IsTrue(players.CanUseLastPlayers);
            players.UseLastPlayers();
            CollectionAssert.AreEqual(new[] { "Ana", "Bob", "Carlos" }, players.NormalizedNames);
            Assert.IsFalse(players.CanUseLastPlayers);
        }
    }
}
