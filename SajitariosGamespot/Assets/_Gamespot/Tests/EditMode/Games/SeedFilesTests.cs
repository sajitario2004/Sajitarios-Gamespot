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
    }
}
