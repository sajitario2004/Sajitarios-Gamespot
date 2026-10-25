using System.Globalization;
using System.Linq;
using System.Text;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Tabu;
using Gamespot.Games.Tabu.Data;
using Gamespot.Tests.EditMode.Db;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Tabu
{
    public sealed class TabuDataTests
    {
        /// <summary>Minúsculas y sin tildes, para comparar como lo haría alguien jugando.</summary>
        private static string Plain(string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s.ToLowerInvariant().Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            return sb.ToString().Trim();
        }

        [Test]
        public void Seed_500PalabrasSinDuplicadosNiProhibidasRepetidas()
        {
            using (var temp = new TempDatabase())
            {
                var db = temp.Open(new IDbSchema[] { new TabuGame() }, new ResourcesSeedSource());
                var all = new TabuWordRepository(db).GetAll();
                Assert.That(all.Count, Is.GreaterThanOrEqualTo(500));
                Assert.IsTrue(all.All(w => w.Prohibidas.Count == 5), "5 prohibidas en cada tarjeta");
                Assert.AreEqual(all.Count, all.Select(w => Plain(w.Palabra)).Distinct().Count(), "Sin palabras repetidas");
                foreach (var word in all)
                {
                    var forbidden = word.Prohibidas.Select(Plain).ToList();
                    Assert.AreEqual(5, forbidden.Distinct().Count(), $"Prohibidas repetidas en {word.Palabra}");
                    CollectionAssert.DoesNotContain(forbidden, Plain(word.Palabra), $"La palabra {word.Palabra} está entre sus prohibidas");
                }
            }
        }

        [Test]
        public void Seed_ConFamososSitiosYFiesta()
        {
            var words = TabuSchema.Parse(new ResourcesSeedSource().Load(TabuSchema.SeedName)).Select(w => w.Palabra).ToList();
            Assert.That(words.Count, Is.GreaterThanOrEqualTo(1000));
            foreach (var expected in new[] { "Ibai", "Rosalía", "Torre Eiffel", "Calimocho", "Mercadona", "Harry Potter" })
            {
                CollectionAssert.Contains(words, expected);
            }
        }

        [Test]
        public void Categorias_FiltranYCuentan()
        {
            using (var temp = new TempDatabase())
            {
                var db = temp.Open(new IDbSchema[] { new TabuGame() }, new ResourcesSeedSource());
                var repo = new TabuWordRepository(db);
                var counts = repo.CountByCategoria();
                CollectionAssert.IsSubsetOf(counts.Keys, Gamespot.Core.WordCategories.Seed);
                var sitios = repo.GetByCategorias(new[] { Gamespot.Core.WordCategories.Sitios });
                Assert.AreEqual(counts[Gamespot.Core.WordCategories.Sitios], sitios.Count);
                Assert.IsTrue(sitios.Any(w => w.Palabra == "Torre Eiffel"));
                Assert.IsFalse(sitios.Any(w => w.Palabra == "Pirata"));
                var source = new CategoryTabuWordSource(repo) { Categorias = new[] { Gamespot.Core.WordCategories.Politica } };
                Assert.IsTrue(source.GetAll().All(w => w.Palabra != "Pirata"));
                source.Categorias = null;
                Assert.AreEqual(repo.GetAll().Count, source.GetAll().Count, "Sin categorías: todas");
            }
        }

        [Test]
        public void TablaAnteriorSinCategoria_RecibeLaColumna()
        {
            using (var temp = new TempDatabase())
            {
                var raw = temp.OpenRaw();
                raw.Execute("CREATE TABLE tabu_words (id INTEGER PRIMARY KEY AUTOINCREMENT, palabra TEXT NOT NULL, prohibidas_json TEXT NOT NULL, " +
                            "is_seed INTEGER NOT NULL DEFAULT 0)");
                raw.Execute("INSERT INTO tabu_words (palabra, prohibidas_json, is_seed) VALUES ('Candil', '[\"a\",\"b\",\"c\",\"d\"]', 1)");
                raw.UserVersion = 3;
                temp.Close();

                var db = temp.Open(new IDbSchema[] { new TabuGame() }, new ResourcesSeedSource());
                Assert.IsTrue(AppDatabase.HasColumn(db, TabuSchema.Table, "categoria"));
                var all = new TabuWordRepository(db).GetAll();
                Assert.IsFalse(all.Any(w => w.Palabra == "Candil"), "La difícil se va con el seed viejo");
                Assert.That(all.Count, Is.GreaterThanOrEqualTo(1000));
            }
        }

        [Test]
        public void Seed_ConTildes()
        {
            var words = TabuSchema.Parse(new ResourcesSeedSource().Load(TabuSchema.SeedName));
            var byWord = words.ToDictionary(w => w.Palabra);
            Assert.IsTrue(byWord.ContainsKey("Volcán"));
            Assert.IsTrue(byWord.ContainsKey("Brújula"));
            CollectionAssert.Contains(byWord["Volcán"].Prohibidas, "erupción");
            Assert.IsFalse(byWord.ContainsKey("Volcan"), "La versión sin tilde ya no está");
        }

        [Test]
        public void MigracionA2_RenuevaElSeedYConservaLasPropias()
        {
            using (var temp = new TempDatabase())
            {
                var db = temp.Open(new IDbSchema[] { new TabuGame() }, new ResourcesSeedSource(), version: 1);
                // Así estaba una BD con el seed heredado (sin tildes) y, por si algún día existen, una palabra propia.
                db.Execute("DELETE FROM tabu_words");
                db.Execute("INSERT INTO tabu_words (palabra, prohibidas_json, is_seed) VALUES ('Volcan', '[\"lava\",\"erupcion\",\"magma\",\"humo\"]', 1)");
                db.Execute("INSERT INTO tabu_words (palabra, prohibidas_json, is_seed) VALUES ('Mi palabra', '[\"a\",\"b\",\"c\",\"d\"]', 0)");
                temp.Close();

                db = temp.Open(new IDbSchema[] { new TabuGame() }, new ResourcesSeedSource());
                Assert.AreEqual(AppDatabase.CurrentVersion, db.UserVersion);
                var all = new TabuWordRepository(db).GetAll();
                Assert.IsFalse(all.Any(w => w.Palabra == "Volcan"), "El seed viejo se borra");
                Assert.IsTrue(all.Any(w => w.Palabra == "Volcán"), "Y entra el nuevo");
                Assert.IsTrue(all.Any(w => w.Palabra == "Mi palabra"), "Las propias se quedan");
                Assert.That(db.Scalar<long>("SELECT COUNT(*) FROM tabu_words WHERE is_seed = 1"), Is.GreaterThanOrEqualTo(500));
            }
        }

        [Test]
        public void Seed_SoloSiNoHayFilasDeSeed()
        {
            using (var temp = new TempDatabase())
            {
                var db = temp.Open(new IDbSchema[] { new TabuGame() }, new ResourcesSeedSource());
                Assert.AreEqual(0, TabuSchema.SeedIfEmpty(db, new ResourcesSeedSource()));
            }
        }

        [Test]
        public void Seed_MalFormado_Lanza()
        {
            Assert.Throws<System.FormatException>(() => TabuSchema.Parse("[{\"palabra\": \"sol\", \"prohibidas\": [\"a\"]}]"));
        }
    }
}
