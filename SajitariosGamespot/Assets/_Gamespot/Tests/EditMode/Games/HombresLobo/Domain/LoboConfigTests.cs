using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Games.HombresLobo.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.HombresLobo.Domain
{
    /// <summary>V076: catálogo de roles, configuración, presets y reparto de Hombres Lobo.</summary>
    public sealed class LoboConfigTests
    {
        private static readonly string[] Five = { "Ana", "Bea", "Carlos", "Dani", "Eva" };

        private static Dictionary<LoboRol, int> Roles(params (LoboRol rol, int n)[] roles) => roles.ToDictionary(r => r.rol, r => r.n);

        [Test]
        public void Catalogo_TodosLosRolesConPaqueteYBando()
        {
            Assert.AreEqual(28, LoboCatalog.All.Count);
            Assert.AreEqual(8, LoboCatalog.Of(LoboPaquete.Basico).Count);
            Assert.IsTrue(LoboCatalog.EsLoboDeCarta(LoboRol.GranLoboMalo));
            Assert.IsTrue(LoboCatalog.EsLoboDeCarta(LoboRol.LoboBlanco));
            Assert.IsFalse(LoboCatalog.EsLoboDeCarta(LoboRol.PerroLobo), "El perro lobo elige");
            Assert.AreEqual(2, LoboCatalog.Get(LoboRol.Hermana).Grupo);
            Assert.AreEqual("loboRolVidente", LoboCatalog.Get(LoboRol.Vidente).NameKey);
        }

        [Test]
        public void Config_Errores()
        {
            var ok = Roles((LoboRol.Lobo, 1), (LoboRol.Aldeano, 4));
            Assert.AreEqual(LoboConfigError.PocosJugadores, LoboConfig.Create(Five.Take(4), ok).error);
            Assert.AreEqual(LoboConfigError.NombresRepetidos, LoboConfig.Create(new[] { "Ana", "ana", "C", "D", "E" }, ok).error);
            Assert.AreEqual(LoboConfigError.NombreVacio, LoboConfig.Create(new[] { "Ana", " ", "C", "D", "E" }, ok).error);
            Assert.AreEqual(LoboConfigError.SinLobos, LoboConfig.Create(Five, Roles((LoboRol.Aldeano, 5))).error);
            Assert.AreEqual(LoboConfigError.CartasNoCuadran, LoboConfig.Create(Five, Roles((LoboRol.Lobo, 1), (LoboRol.Aldeano, 3))).error);
            Assert.AreEqual(LoboConfigError.GrupoIncompleto,
                LoboConfig.Create(Five, Roles((LoboRol.Lobo, 1), (LoboRol.Hermana, 1), (LoboRol.Aldeano, 3))).error);
            Assert.AreEqual(LoboConfigError.DemasiadasCopias,
                LoboConfig.Create(Five, Roles((LoboRol.Lobo, 1), (LoboRol.Vidente, 2), (LoboRol.Aldeano, 2))).error);
            Assert.AreEqual(LoboConfigError.CartasNoCuadran,
                LoboConfig.Create(Five, Roles((LoboRol.Lobo, 1), (LoboRol.Ladron, 1), (LoboRol.Aldeano, 3))).error, "El ladrón necesita dos cartas más");
            var (config, error) = LoboConfig.Create(Five, Roles((LoboRol.Lobo, 1), (LoboRol.Ladron, 1), (LoboRol.Aldeano, 5)));
            Assert.IsNull(error);
            Assert.AreEqual(7, LoboConfig.TotalCartas(config.Roles));
        }

        [Test]
        public void Presets_SiempreCuadranConLobosSuficientes()
        {
            foreach (var preset in LoboPresets.All)
            {
                for (var n = LoboConfig.MinJugadores; n <= LoboConfig.MaxJugadores; n++)
                {
                    var roles = LoboPresets.Roles(preset, n);
                    var names = Enumerable.Range(0, n).Select(i => $"J{i}");
                    var (config, error) = LoboConfig.Create(names, roles);
                    Assert.IsNull(error, $"{preset} con {n}: {error}");
                    var wolves = roles.Where(r => LoboCatalog.EsLoboDeCarta(r.Key)).Sum(r => r.Value);
                    Assert.That(wolves, Is.InRange(1, n / 3 + 1), $"{preset} con {n}");
                    Assert.IsNotNull(config);
                }
            }
            CollectionAssert.IsSubsetOf(new[] { LoboRol.Vidente, LoboRol.Bruja, LoboRol.Cazador }, LoboPresets.Roles(LoboPreset.Iniciacion, 8).Keys);
            Assert.IsTrue(LoboPresets.Roles(LoboPreset.Caos, 16).ContainsKey(LoboRol.LoboBlanco));
        }

        [Test]
        public void Reparto_FisherYatesDelMazoEnElOrdenDelCatalogo()
        {
            var roles = Roles((LoboRol.Aldeano, 3), (LoboRol.Lobo, 1), (LoboRol.Vidente, 1));
            var (config, _) = LoboConfig.Create(Five, roles);
            // Mazo: [Aldeano, Aldeano, Aldeano, Lobo, Vidente]; i = 4..1 con j = NextInt(i + 1) = 0 siempre:
            // [V,A,A,L,A] → [L,A,A,V,A] → [A,A,L,V,A] → [A,A,L,V,A].
            var game = new LoboGame(new ScriptedRandom(0.0, 0.0, 0.0, 0.0));
            game.Iniciar(config);
            CollectionAssert.AreEqual(new[] { LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Lobo, LoboRol.Vidente, LoboRol.Aldeano },
                game.Jugadores.Select(p => p.Rol));
            Assert.AreEqual(LoboFase.Reparto, game.Fase);
            Assert.AreEqual(0, game.RepartoIndex);
        }

        [Test]
        public void Reparto_ConLadronSobranDosYSectarioConGrupos()
        {
            var names = Enumerable.Range(0, 7).Select(i => $"J{i}").ToList();
            var roles = Roles((LoboRol.Lobo, 1), (LoboRol.Ladron, 1), (LoboRol.Sectario, 1), (LoboRol.Aldeano, 6));
            var (config, error) = LoboConfig.Create(names, roles);
            Assert.IsNull(error);
            var game = new LoboGame(new SeededRandom(5));
            game.Iniciar(config);
            Assert.AreEqual(2, game.CartasLadron.Count);
            var all = game.Jugadores.Select(p => p.Rol).Concat(game.CartasLadron).GroupBy(r => r).ToDictionary(g => g.Key, g => g.Count());
            CollectionAssert.AreEquivalent(roles, all, "Están todas las cartas");
            var sect = game.Jugadores.FirstOrDefault(p => p.Rol == LoboRol.Sectario);
            if (sect != null)
            {
                Assert.AreEqual(0, sect.Grupo);
                Assert.That(game.Jugadores.Count(p => p.Grupo == 1), Is.GreaterThanOrEqualTo(3), "Los demás, repartidos en dos grupos");
            }
        }

        [Test]
        public void Reparto_LobosSeConocen()
        {
            var game = LoboTestKit.Game(LoboRol.Lobo, LoboRol.Aldeano, LoboRol.Lobo, LoboRol.Aldeano, LoboRol.Aldeano, LoboRol.Aldeano);
            CollectionAssert.AreEqual(new[] { 2 }, game.CompanerosLobos(0));
        }
    }
}
