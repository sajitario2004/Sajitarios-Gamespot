using System;
using System.Linq;
using Gamespot.Games.Impostor.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Impostor
{
    public sealed class ImpostorStatsTests
    {
        private static GameRecord Record(long id, string word, params (string name, bool impostor)[] players) =>
            new GameRecord(id, new DateTime(2026, 1, 1).AddMinutes(id), word, "pista", players.Length, players.Count(p => p.impostor), false,
                players.Select(p => new GameRecordPlayer(p.name, p.impostor)).ToList());

        [Test]
        public void SinPartidas()
        {
            var stats = ImpostorStats.Compute(Array.Empty<GameRecord>());
            Assert.AreEqual(0, stats.Total);
            Assert.IsNull(stats.MostRepeatedWord);
            Assert.AreEqual(0, stats.ImpostorRanking.Count);
        }

        [Test]
        public void PalabraMasRepetida_SinMayusculasYGrafiaMasReciente()
        {
            // El repositorio devuelve la más reciente primero.
            var stats = ImpostorStats.Compute(new[]
            {
                Record(3, "Playa", ("Ana", true)),
                Record(2, "pirata", ("Ana", false)),
                Record(1, "playa", ("Bea", true))
            });
            Assert.AreEqual(3, stats.Total);
            Assert.AreEqual("Playa", stats.MostRepeatedWord);
            Assert.AreEqual(2, stats.MostRepeatedCount);
        }

        [Test]
        public void PalabraMasRepetida_EmpateGanaLaAlfabeticamenteMenor()
        {
            var stats = ImpostorStats.Compute(new[] { Record(2, "zorro"), Record(1, "abeja") });
            Assert.AreEqual("abeja", stats.MostRepeatedWord);
        }

        [Test]
        public void RankingDeImpostores_MasVecesPrimeroYEmpatePorNombre()
        {
            var stats = ImpostorStats.Compute(new[]
            {
                Record(3, "a", ("bea", true), ("Ana", true), ("Carlos", false)),
                Record(2, "b", ("Carlos", true)),
                Record(1, "c", ("Carlos", true), ("ana", true))
            });
            CollectionAssert.AreEqual(new[] { ("Carlos", 2), ("Ana", 1), ("ana", 1), ("bea", 1) }, stats.ImpostorRanking.ToArray());
        }
    }
}
