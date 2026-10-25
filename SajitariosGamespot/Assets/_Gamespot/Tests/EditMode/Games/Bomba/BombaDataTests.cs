using System.Linq;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Bomba;
using Gamespot.Games.Bomba.Data;
using Gamespot.Games.Bomba.Domain;
using Gamespot.Tests.EditMode.Db;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Bomba
{
    public sealed class BombaDataTests
    {
        [Test]
        public void Seed_SilabasYCategoriasSinDuplicados()
        {
            using (var temp = new TempDatabase())
            {
                var db = temp.Open(new IDbSchema[] { new BombaGame() }, new ResourcesSeedSource());
                var repo = new BombaPromptRepository(db);
                var silabas = repo.GetAllSilabas();
                var categorias = repo.GetAllCategorias();
                Assert.That(silabas.Count, Is.GreaterThanOrEqualTo(70));
                Assert.That(categorias.Count, Is.GreaterThanOrEqualTo(70));
                Assert.IsTrue(silabas.All(p => p.Mode == BombaMode.Silaba));
                Assert.IsTrue(categorias.All(p => p.Mode == BombaMode.Categoria));
                Assert.AreEqual(silabas.Count, silabas.Select(p => p.Texto.ToLowerInvariant()).Distinct().Count(), "Sílabas sin repetir");
                Assert.AreEqual(categorias.Count, categorias.Select(p => p.Texto.ToLowerInvariant()).Distinct().Count(), "Categorías sin repetir");
                Assert.AreEqual(silabas.Count, db.Scalar<long>("SELECT COUNT(*) FROM bomba_silabas WHERE is_seed = 1"));
            }
        }

        [Test]
        public void Seed_SoloSiLaTablaEstaVacia()
        {
            using (var temp = new TempDatabase())
            {
                var db = temp.Open(new IDbSchema[] { new BombaGame() }, new ResourcesSeedSource());
                Assert.AreEqual(0, BombaSchema.SeedIfEmpty(db, new ResourcesSeedSource()), "Ya sembrada: no inserta nada");
            }
        }

        [Test]
        public void Seed_MalFormado_Lanza()
        {
            Assert.Throws<System.FormatException>(() => BombaSchema.Parse("[\"CA\", \"  \"]"));
            Assert.Throws<System.FormatException>(() => BombaSchema.Parse("[\"CA\", 3]"));
            CollectionAssert.AreEqual(new[] { "CA", "PA" }, BombaSchema.Parse("[\" CA \", \"PA\"]"));
        }
    }
}
