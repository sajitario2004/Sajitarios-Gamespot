using System.Linq;
using Gamespot.Core;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Core
{
    public sealed class WordCategoriesTests
    {
        [Test]
        public void ClavesDeLocalizacionYOrden()
        {
            Assert.AreEqual("categoriaGeneral", WordCategories.LabelKey(WordCategories.General));
            Assert.AreEqual("categoriaEspana", WordCategories.LabelKey(WordCategories.Espana));
            Assert.AreEqual("categoriaPropias", WordCategories.LabelKey(WordCategories.Propias));
            Assert.AreEqual(WordCategories.Propias, WordCategories.All.Last(), "Mis palabras va al final");
            CollectionAssert.DoesNotContain(WordCategories.Seed, WordCategories.Propias);
            Assert.AreEqual(WordCategories.All.Count, WordCategories.All.Distinct().Count());
            Assert.IsTrue(WordCategories.IsAdult(WordCategories.Cachondeo));
            Assert.IsFalse(WordCategories.IsAdult(WordCategories.Fiesta));
            CollectionAssert.DoesNotContain(WordCategories.Default, WordCategories.Cachondeo, "Cachondeo va desmarcada por defecto");
            CollectionAssert.AreEquivalent(WordCategories.All.Where(c => !WordCategories.IsAdult(c)), WordCategories.Default);
        }

        [Test]
        public void IdsDeLasCategoriasPropias()
        {
            Assert.AreEqual("custom_7", WordCategories.CustomId(7));
            Assert.IsTrue(WordCategories.IsCustom("custom_7"));
            Assert.IsFalse(WordCategories.IsCustom(WordCategories.Propias));
            Assert.IsFalse(WordCategories.IsCustom(null));
            Assert.IsFalse(WordCategories.All.Any(WordCategories.IsCustom), "Ninguna de serie empieza por custom_");
        }
    }
}
