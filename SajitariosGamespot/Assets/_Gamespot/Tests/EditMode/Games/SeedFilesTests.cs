using System.Linq;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Impostor.Data;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games
{
    /// <summary>Los seeds reales de <c>Resources/Seed</c> cumplen los mínimos de los tests del Flutter.</summary>
    public sealed class SeedFilesTests
    {
        private static readonly ResourcesSeedSource Seeds = new ResourcesSeedSource();

        [Test]
        public void Impostor_2000PalabrasConPistaYSinDuplicados()
        {
            var entries = ImpostorSchema.Parse(Seeds.Load(ImpostorSchema.SeedName));
            Assert.That(entries.Count, Is.GreaterThanOrEqualTo(2000));
            Assert.IsTrue(entries.All(e => e.word.Length > 0 && e.hint.Length > 0));
            Assert.IsTrue(entries.All(e => !string.Equals(e.word, e.hint, System.StringComparison.OrdinalIgnoreCase)), "word ≠ hint");
            var duplicates = entries.GroupBy(e => e.word.ToLowerInvariant()).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            CollectionAssert.IsEmpty(duplicates);
        }

        [Test]
        public void Impostor_CategoriasYSinLasDificiles()
        {
            var entries = ImpostorSchema.Parse(Seeds.Load(ImpostorSchema.SeedName));
            CollectionAssert.IsSubsetOf(entries.Select(e => e.category).Distinct(), Gamespot.Core.WordCategories.Seed, "Categorías conocidas");
            foreach (var category in Gamespot.Core.WordCategories.Seed)
            {
                Assert.That(entries.Count(e => e.category == category), Is.GreaterThanOrEqualTo(20), category);
            }
            var words = entries.Select(e => e.word).ToList();
            foreach (var removed in new[] { "feudo", "Eugenio", "Andreu Buenafuente", "obenque", "hamburgesa" })
            {
                CollectionAssert.DoesNotContain(words, removed);
            }
            CollectionAssert.Contains(words, "hamburguesa");
        }

        [Test]
        public void Impostor_PalabrasDeFiestaYDeEspana()
        {
            var words = ImpostorSchema.Parse(Seeds.Load(ImpostorSchema.SeedName)).Select(e => e.word).ToList();
            Assert.That(words.Count, Is.GreaterThanOrEqualTo(2100));
            foreach (var expected in new[] { "Ibai", "Pedro Sánchez", "Rosalía", "Fortnite", "calimocho", "Torre Eiffel", "Sagrada Familia", "Mercadona" })
            {
                CollectionAssert.Contains(words, expected);
            }
        }
    }
}
