using System;
using System.Linq;
using Gamespot.Core;
using Gamespot.Games.Impostor.Domain;
using NUnit.Framework;
using static Gamespot.Tests.EditMode.Games.Impostor.ImpostorTestData;

namespace Gamespot.Tests.EditMode.Games.Impostor
{
    /// <summary>
    /// Reglas probabilísticas del Impostor (CLAUDE.md §5). No cambiar estos tests sin entender por qué:
    /// son el núcleo de las reglas del autor.
    /// </summary>
    public sealed class AssignRolesUseCaseTests
    {
        private const int Iterations = 10000;
        private const int DistributionSeed = 20240615;

        private static (int todos, int ninguno, int normal) Distribution(int seed)
        {
            var useCase = new AssignRolesUseCase(new SeededRandom(seed));
            var config = Config(Players(5), 2);
            int todos = 0, ninguno = 0, normal = 0;
            for (var i = 0; i < Iterations; i++)
            {
                var session = useCase.Execute(config, Words);
                if (session.ImpostorCount == 5) todos++;
                else if (session.ImpostorCount == 0) ninguno++;
                else normal++;
            }
            return (todos, ninguno, normal);
        }

        [Test]
        public void SinPalabras_Lanza()
        {
            var useCase = new AssignRolesUseCase(new SeededRandom(1));
            Assert.Throws<ArgumentException>(() => useCase.Execute(Config(Players(3), 1), Array.Empty<Word>()));
        }

        [Test]
        public void Distribucion_10_10_80_En10000Iteraciones()
        {
            var (todos, ninguno, normal) = Distribution(DistributionSeed);
            Assert.AreEqual(Iterations, todos + ninguno + normal);
            Assert.That(todos / (double)Iterations, Is.EqualTo(0.10).Within(0.02), "TODOS impostores ≈ 10 %");
            Assert.That(ninguno / (double)Iterations, Is.EqualTo(0.10).Within(0.02), "NINGUNO impostor ≈ 10 %");
            Assert.That(normal / (double)Iterations, Is.EqualTo(0.80).Within(0.03), "Reparto normal ≈ 80 %");
        }

        [Test]
        public void MismaSemilla_MismosConteos()
        {
            Assert.AreEqual(Distribution(99), Distribution(99));
        }

        [Test]
        public void TiradaMenorQue010_TodosImpostores()
        {
            // palabra (1 valor) + tirada 0.05
            var session = new AssignRolesUseCase(new ScriptedRandom(0.0, 0.05)).Execute(Config(Players(5), 2), Words);
            Assert.IsTrue(session.Players.All(session.IsImpostor));
            Assert.AreEqual(5, session.ImpostorCount);
        }

        [Test]
        public void TiradaEntre010Y020_NingunImpostor()
        {
            var session = new AssignRolesUseCase(new ScriptedRandom(0.0, 0.15)).Execute(Config(Players(5), 2), Words);
            Assert.IsFalse(session.Players.Any(session.IsImpostor));
            Assert.AreEqual(0, session.ImpostorCount);
        }

        [TestCase(0.10)]
        [TestCase(0.0999)]
        [TestCase(0.20)]
        [TestCase(0.1999)]
        public void BordesDeLosUmbrales(double roll)
        {
            var session = new AssignRolesUseCase(new ScriptedRandom(true, 0.0, roll, 0.5)).Execute(Config(Players(5), 2), Words);
            var expected = roll < 0.10 ? 5 : roll < 0.20 ? 0 : 2;
            Assert.AreEqual(expected, session.ImpostorCount, $"tirada {roll}");
        }

        [Test]
        public void TiradaNormal_ExactamenteNImpostores()
        {
            var useCase = new AssignRolesUseCase(new SeededRandom(3));
            var config = Config(Players(8), 3);
            for (var i = 0; i < 500; i++)
            {
                var session = useCase.Execute(config, Words);
                Assert.That(session.ImpostorCount, Is.EqualTo(3).Or.EqualTo(0).Or.EqualTo(8));
            }
        }

        [Test]
        public void RepartoNormal_PrimerosDeLaCopiaBarajada()
        {
            // 3 jugadores, 1 impostor: palabra, tirada 0.5, barajado sin cambios (j = i en cada paso) → impostor = J1
            var players = Players("Nacho", "Iker", "Lucía");
            var session = new AssignRolesUseCase(new ScriptedRandom(0.0, 0.5, 0.99, 0.99)).Execute(Config(players, 1), Words);
            Assert.IsTrue(session.IsImpostor(players[0]));
            Assert.IsFalse(session.IsImpostor(players[1]));
            Assert.IsFalse(session.IsImpostor(players[2]));
        }

        [Test]
        public void ImpostoresCapadosAJugadoresMenosUno()
        {
            var config = Config(Players(4), 5);
            Assert.AreEqual(3, config.NImpostores);
            var useCase = new AssignRolesUseCase(new SeededRandom(11));
            for (var i = 0; i < 300; i++)
            {
                var session = useCase.Execute(config, Words);
                if (session.ImpostorCount != 4 && session.ImpostorCount != 0) Assert.AreEqual(3, session.ImpostorCount);
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(7)]
        [TestCase(9999)]
        public void OrdenDeRevelacion_EsElDeIntroduccion(int seed)
        {
            var players = Players("Nacho", "Iker", "Lucía", "Ana", "Bea");
            var session = new AssignRolesUseCase(new SeededRandom(seed)).Execute(Config(players, 2), Words);
            CollectionAssert.AreEqual(players, session.RevealOrder);
        }

        [Test]
        public void LaPalabraEsDelBancoYLlevaSuPista()
        {
            var useCase = new AssignRolesUseCase(new SeededRandom(5));
            for (var i = 0; i < 100; i++)
            {
                var session = useCase.Execute(Config(Players(3), 1), Words);
                CollectionAssert.Contains(Words, session.Word);
                Assert.AreEqual(session.Word.Hint, session.Hint);
            }
        }

        [Test]
        public void PrimeroLaPalabraDespuesLaTirada()
        {
            // 0.99 elige la última palabra; 0.05 → todos impostores
            var session = new AssignRolesUseCase(new ScriptedRandom(0.99, 0.05)).Execute(Config(Players(3), 1), Words);
            Assert.AreEqual(Words[2], session.Word);
            Assert.AreEqual(3, session.ImpostorCount);
        }
    }
}
