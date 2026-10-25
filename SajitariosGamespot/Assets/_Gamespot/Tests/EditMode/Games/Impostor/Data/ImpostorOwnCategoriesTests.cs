using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Impostor;
using Gamespot.Games.Impostor.Data;
using Gamespot.Games.Shared;
using Gamespot.Tests.EditMode.Db;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Impostor
{
    /// <summary>V059l: las palabras propias van a la categoría que se elija, también a las que crea el usuario.</summary>
    public sealed class ImpostorOwnCategoriesTests
    {
        private static readonly DictionarySeedSource Seed = new DictionarySeedSource(new Dictionary<string, string>
        {
            ["impostor_words"] = "[{\"word\": \"Pirata\", \"hint\": \"barco\", \"category\": \"general\"}, " +
                                 "{\"word\": \"Calimocho\", \"hint\": \"bebida\", \"category\": \"fiesta\"}]"
        });

        private TempDatabase _temp;
        private WordRepository _words;
        private CustomCategoryRepository _categories;

        [SetUp]
        public void SetUp()
        {
            _temp = new TempDatabase();
            var db = _temp.Open(new IDbSchema[] { new ImpostorGame() }, Seed);
            _words = new WordRepository(db, () => 1);
            _categories = new CustomCategoryRepository(db, ImpostorSchema.CategoriesTable);
        }

        [TearDown]
        public void TearDown() => _temp.Dispose();

        [Test]
        public void ElEsquemaCreaLaTablaDeCategorias()
        {
            Assert.AreEqual(1L, _temp.Db.Scalar<long>("SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = ?", ImpostorSchema.CategoriesTable));
        }

        [Test]
        public void Insert_EnLaCategoriaElegidaYUpdateLaCambiaOLaConserva()
        {
            var pueblo = _categories.Insert("Mi pueblo");
            var id = _words.Insert("La plaza", "fuente", pueblo.CategoryId);
            Assert.AreEqual(pueblo.CategoryId, _words.GetById(id).Category);
            var own = _words.Insert("El bar", "cañas", " ");
            Assert.AreEqual(WordCategories.Propias, _words.GetById(own).Category, "Sin categoría, a Mis palabras");

            _words.Update(id, "La plaza mayor", "fuente");
            Assert.AreEqual(pueblo.CategoryId, _words.GetById(id).Category, "Sin categoría nueva, se queda la que tenía");
            _words.Update(id, "La plaza mayor", "fuente", WordCategories.Fiesta);
            Assert.AreEqual(WordCategories.Fiesta, _words.GetById(id).Category, "También a una de serie");
            CollectionAssert.AreEquivalent(new[] { "Calimocho", "La plaza mayor" }, _words.GetWords(new[] { WordCategories.Fiesta }).Select(w => w.Text));
        }

        [Test]
        public void Search_PorTextoYPorCategoria()
        {
            var pueblo = _categories.Insert("Mi pueblo");
            _words.Insert("La plaza", "fuente", pueblo.CategoryId);
            _words.Insert("La iglesia", "campanas", pueblo.CategoryId);
            _words.Insert("La playa", "arena");
            CollectionAssert.AreEqual(new[] { "La iglesia", "La plaza" }, _words.Search(null, pueblo.CategoryId).Select(w => w.Word));
            CollectionAssert.AreEqual(new[] { "La plaza" }, _words.Search("plaz", pueblo.CategoryId).Select(w => w.Word));
            CollectionAssert.AreEqual(new[] { "La playa", "La plaza" }, _words.Search("pla").Select(w => w.Word));
            CollectionAssert.AreEqual(new[] { "Calimocho" }, _words.Search("", WordCategories.Fiesta).Select(w => w.Word));
        }

        [Test]
        public void MoveCategory_SoloMueveLasDelUsuario()
        {
            var pueblo = _categories.Insert("Mi pueblo");
            _words.Insert("La plaza", "fuente", pueblo.CategoryId);
            _words.Insert("Garrafón", "malo", WordCategories.Fiesta);
            Assert.AreEqual(1, _words.MoveCategory(pueblo.CategoryId, WordCategories.Propias));
            Assert.AreEqual(1, _words.MoveCategory(WordCategories.Fiesta, WordCategories.Propias), "Calimocho (seed) no se mueve");
            var counts = _words.CountByCategory();
            Assert.AreEqual(2, counts[WordCategories.Propias]);
            Assert.AreEqual(1, counts[WordCategories.Fiesta]);
            Assert.IsFalse(counts.ContainsKey(pueblo.CategoryId));
        }
    }
}
