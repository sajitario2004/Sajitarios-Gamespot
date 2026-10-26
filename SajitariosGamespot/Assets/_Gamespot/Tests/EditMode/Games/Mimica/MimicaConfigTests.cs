using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Mimica;
using Gamespot.Games.Mimica.Domain;
using Gamespot.Games.Shared;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Mimica
{
    /// <summary>V074: configuración de Mímica y su seed.</summary>
    public sealed class MimicaConfigTests
    {
        [Test]
        public void Config_ValidaEquiposYCategorias()
        {
            var cats = new[] { MimicaCategories.Animales };
            Assert.AreEqual(MimicaConfigError.PocosEquipos, MimicaConfig.Create(new[] { "A" }, cats).error);
            Assert.AreEqual(MimicaConfigError.DemasiadosEquipos, MimicaConfig.Create(new[] { "A", "B", "C", "D", "E" }, cats).error);
            Assert.AreEqual(MimicaConfigError.NombreVacio, MimicaConfig.Create(new[] { "A", " " }, cats).error);
            Assert.AreEqual(MimicaConfigError.NombresRepetidos, MimicaConfig.Create(new[] { "Rojos", "rojos" }, cats).error);
            Assert.AreEqual(MimicaConfigError.SinCategorias, MimicaConfig.Create(new[] { "A", "B" }, new string[0]).error);
            var (config, error) = MimicaConfig.Create(new[] { " Los  Rojos ", "Azules" }, cats,
                new MimicaOptions { Modo = MimicaModo.Mezcla, Pases = 1, PenalizarPases = true, Objetivo = 15, Segundos = 45 });
            Assert.IsNull(error);
            Assert.AreEqual("Los Rojos", config.Equipos[0]);
            var guess = config.GuessOptions;
            Assert.AreEqual(4, guess.Etiquetas, "En mezcla, un estilo por tarjeta");
            Assert.AreEqual(15, guess.Objetivo);
            Assert.AreEqual(1, guess.Pases);
            Assert.IsTrue(guess.PenalizarPases);
            Assert.AreEqual(45, guess.Segundos);
            Assert.AreEqual(0, MimicaConfig.Create(new[] { "A", "B" }, cats).config.GuessOptions.Etiquetas, "Solo mímica: sin estilos");
            CollectionAssert.AreEqual(cats, config.Query.Categories);
        }

        [Test]
        public void Categorias_PorDefectoSinLaMasDieciocho()
        {
            CollectionAssert.DoesNotContain(MimicaCategories.Default, MimicaCategories.Cachondeo);
            CollectionAssert.Contains(MimicaCategories.Default, WordCategories.Propias);
            Assert.IsTrue(WordCategories.IsAdult(MimicaCategories.Cachondeo));
        }

        [Test]
        public void Seed_TodasLasCategoriasConTarjetasSinRepetir()
        {
            var cards = PromptCardRepository.ParseSeed(new ResourcesSeedSource().Load(MimicaSchema.SeedName));
            PromptSeedFilesTests.CheckCommon(cards);
            Assert.That(cards.Count, Is.GreaterThanOrEqualTo(500));
            CollectionAssert.IsSubsetOf(cards.Select(c => c.Category).Distinct(), MimicaCategories.Seed);
            foreach (var category in MimicaCategories.Seed)
            {
                Assert.That(cards.Count(c => c.Category == category), Is.GreaterThanOrEqualTo(45), category);
            }
            Assert.IsTrue(cards.All(c => c.Level == PromptLevel.Suave && c.Kind.Length == 0), "Sin nivel ni tipo: el +18 va por categoría");
            Assert.IsTrue(cards.All(c => c.Text.Length <= 60));
        }
    }
}
