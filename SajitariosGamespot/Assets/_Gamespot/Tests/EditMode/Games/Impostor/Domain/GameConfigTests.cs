using System;
using System.Collections.Generic;
using Gamespot.Games.Impostor.Domain;
using NUnit.Framework;
using static Gamespot.Tests.EditMode.Games.Impostor.ImpostorTestData;

namespace Gamespot.Tests.EditMode.Games.Impostor
{
    public sealed class GameConfigTests
    {
        [TestCase(3, 2)]
        [TestCase(4, 3)]
        [TestCase(6, 5)]
        [TestCase(15, 5)]
        public void MaxImpostoresFor(int players, int expected)
        {
            Assert.AreEqual(expected, GameConfig.MaxImpostoresFor(players));
        }

        [TestCase(3, 1)]
        [TestCase(4, 1)]
        [TestCase(5, 2)]
        [TestCase(6, 3)]
        [TestCase(15, 12)]
        public void MaxRoundsFor(int players, int expected)
        {
            Assert.AreEqual(expected, GameConfig.MaxRoundsFor(players));
        }

        [Test]
        public void MenosDe3Jugadores_Falla()
        {
            Assert.AreEqual(GameConfigError.PocosJugadores, GameConfig.Create(Players(2), 1).Error);
        }

        [Test]
        public void MasDe15Jugadores_Falla()
        {
            Assert.AreEqual(GameConfigError.DemasiadosJugadores, GameConfig.Create(Players(16), 1).Error);
        }

        [Test]
        public void NombreVacio_Falla()
        {
            Assert.AreEqual(GameConfigError.NombreVacio, GameConfig.Create(Players("Ana", "  ", "Bea"), 1).Error);
        }

        [Test]
        public void NombresDuplicadosSinDistinguirMayusculas_Falla()
        {
            Assert.AreEqual(GameConfigError.NombresDuplicados, GameConfig.Create(Players("Ana", "ana", "Bea"), 1).Error);
            Assert.AreEqual(GameConfigError.NombresDuplicados, GameConfig.Create(Players("Ana", " ANA ", "Bea"), 1).Error);
        }

        [Test]
        public void ElOrdenDeLosErrores_PocosAntesQueVacios()
        {
            Assert.AreEqual(GameConfigError.PocosJugadores, GameConfig.Create(Players("", ""), 1).Error);
        }

        [TestCase(3)]
        [TestCase(15)]
        public void AceptaLosLimites(int count)
        {
            Assert.IsTrue(GameConfig.Create(Players(count), 1).IsSuccess);
        }

        [TestCase(3, 5, 2)]
        [TestCase(10, 9, 5)]
        [TestCase(5, 0, 1)]
        [TestCase(5, -3, 1)]
        [TestCase(8, 3, 3)]
        public void NormalizaLosImpostores(int players, int requested, int expected)
        {
            Assert.AreEqual(expected, Config(Players(players), requested).NImpostores);
        }

        [Test]
        public void Rondas_PorDefectoElMaximoYRecortadas()
        {
            Assert.AreEqual(3, Config(Players(6), 1).Rounds);
            Assert.AreEqual(3, Config(Players(6), 1, rounds: 10).Rounds);
            Assert.AreEqual(1, Config(Players(6), 1, rounds: 0).Rounds);
            Assert.AreEqual(2, Config(Players(6), 1, rounds: 2).Rounds);
        }

        [Test]
        public void Pista_PorDefectoDesactivada()
        {
            Assert.IsFalse(Config(Players(3), 1).HintEnabled);
            Assert.IsTrue(Config(Players(3), 1, hint: true).HintEnabled);
        }

        [Test]
        public void ConservaElOrdenYEsInmutable()
        {
            var players = Players("Nacho", "Iker", "Lucía");
            var config = Config(players, 1);
            players.Add(new Player("Intruso"));
            CollectionAssert.AreEqual(new[] { "Nacho", "Iker", "Lucía" }, ToNames(config.Players));
            Assert.Throws<NotSupportedException>(() => ((IList<Player>)config.Players).Add(new Player("X")));
        }

        private static List<string> ToNames(IReadOnlyList<Player> players)
        {
            var names = new List<string>();
            foreach (var p in players) names.Add(p.Name);
            return names;
        }
    }
}
