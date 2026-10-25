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
        }
    }
}
