using System.Linq;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Tabu;
using Gamespot.Games.Tabu.Data;
using Gamespot.Tests.EditMode.Db;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Tabu
{
    public sealed class TabuDataTests
    {
        [Test]
        public void Seed_100PalabrasCon4o5ProhibidasSinDuplicados()
        {
            using (var temp = new TempDatabase())
            {
                var db = temp.Open(new IDbSchema[] { new TabuGame() }, new ResourcesSeedSource());
                var all = new TabuWordRepository(db).GetAll();
                Assert.That(all.Count, Is.GreaterThanOrEqualTo(100));
                Assert.IsTrue(all.All(w => w.Prohibidas.Count >= 4 && w.Prohibidas.Count <= 5));
                Assert.AreEqual(all.Count, all.Select(w => w.Palabra.ToLowerInvariant()).Distinct().Count(), "Sin palabras repetidas");
            }
        }

        [Test]
        public void Seed_MalFormado_Lanza()
        {
            Assert.Throws<System.FormatException>(() => TabuSchema.Parse("[{\"palabra\": \"sol\", \"prohibidas\": [\"a\"]}]"));
        }
    }
}
