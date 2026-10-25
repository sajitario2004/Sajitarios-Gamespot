using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Shared;
using Gamespot.Games.Tabu;
using Gamespot.Games.Tabu.Data;
using Gamespot.Games.Tabu.Domain;
using Gamespot.Tests.EditMode.Db;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Tabu
{
    /// <summary>V059l: tarjetas propias del Tabú (alta, edición, borrado, categorías) con el seed real.</summary>
    public sealed class TabuOwnCardsTests
    {
        private static readonly string[] Five = { "plaza", "fiestas", "bar", "iglesia", "campo" };

        private TempDatabase _temp;
        private TabuWordRepository _repo;
        private CustomCategoryRepository _categories;

        [SetUp]
        public void SetUp()
        {
            _temp = new TempDatabase();
            var db = _temp.Open(new IDbSchema[] { new TabuGame() }, new ResourcesSeedSource());
            _repo = new TabuWordRepository(db);
            _categories = new CustomCategoryRepository(db, TabuSchema.CategoriesTable);
        }

        [TearDown]
        public void TearDown() => _temp.Dispose();

        private static TabuCardProblem Problem(TestDelegate action) => Assert.Throws<TabuCardException>(action).Problem;

        [Test]
        public void Insert_ConCategoriaYLaPartidaLaUsa()
        {
            var pueblo = _categories.Insert("Mi pueblo");
            var id = _repo.Insert(" Villarriba ", new[] { "plaza", "fiestas", "bar", "iglesia", "" }, pueblo.CategoryId);
            var card = _repo.GetById(id);
            Assert.AreEqual("Villarriba", card.Palabra);
            CollectionAssert.AreEqual(new[] { "plaza", "fiestas", "bar", "iglesia" }, card.Prohibidas, "La quinta es opcional");
            Assert.IsFalse(card.IsSeed);
            Assert.AreEqual(pueblo.CategoryId, card.Categoria);
            Assert.AreEqual(1, _repo.CountByCategoria()[pueblo.CategoryId]);
            var source = new CategoryTabuWordSource(_repo) { Categorias = new[] { pueblo.CategoryId } };
            CollectionAssert.AreEqual(new[] { "Villarriba" }, source.GetAll().Select(w => w.Palabra));
        }

        [Test]
        public void Insert_NoValidaORepetidaConElSeed()
        {
            var invalid = Assert.Throws<TabuCardException>(() => _repo.Insert("Villarriba", new[] { "a", "b", "c" }));
            Assert.AreEqual(TabuCardProblem.Invalida, invalid.Problem);
            Assert.AreEqual(TabuCardError.PocasProhibidas, invalid.Validation);
            Assert.AreEqual(TabuCardProblem.Duplicada, Problem(() => _repo.Insert("pirata", Five)), "Pirata ya está en el seed");
            Assert.AreEqual(TabuCardProblem.Duplicada, Problem(() => _repo.Insert("VOLCÁN", Five)), "Sin mayúsculas, también con tilde");
        }

        [Test]
        public void Update_YDelete_SoloLasPropias()
        {
            var id = _repo.Insert("Villarriba", Five);
            _repo.Update(id, "Villabajo", Five.Reverse());
            var card = _repo.GetById(id);
            Assert.AreEqual("Villabajo", card.Palabra);
            Assert.AreEqual("campo", card.Prohibidas[0]);
            Assert.AreEqual(WordCategories.Propias, card.Categoria, "Sin categoría nueva, se queda la que tenía");
            _repo.Update(id, "Villabajo", Five, WordCategories.Fiesta);
            Assert.AreEqual(WordCategories.Fiesta, _repo.GetById(id).Categoria);

            var seed = _repo.Search("Pirata").First(c => c.IsSeed);
            Assert.AreEqual(TabuCardProblem.Predefinida, Problem(() => _repo.Update(seed.Id, "Pirata", Five)));
            Assert.AreEqual(TabuCardProblem.Predefinida, Problem(() => _repo.Delete(seed.Id)));
            Assert.AreEqual(TabuCardProblem.Duplicada, Problem(() => _repo.Update(id, "Pirata", Five)));

            _repo.Delete(id);
            Assert.IsNull(_repo.GetById(id));
            Assert.AreEqual(TabuCardProblem.NoExiste, Problem(() => _repo.Delete(id)));
        }

        [Test]
        public void Search_PorTextoYCategoria()
        {
            var pueblo = _categories.Insert("Mi pueblo");
            _repo.Insert("Villarriba", Five, pueblo.CategoryId);
            _repo.Insert("Villabajo", Five, pueblo.CategoryId);
            CollectionAssert.AreEqual(new[] { "Villabajo", "Villarriba" }, _repo.Search(null, pueblo.CategoryId).Select(c => c.Palabra));
            CollectionAssert.AreEqual(new[] { "Villarriba" }, _repo.Search("rriba", pueblo.CategoryId).Select(c => c.Palabra));
            Assert.That(_repo.Search().Count, Is.GreaterThanOrEqualTo(1000), "Sin filtros, todas");
            Assert.IsTrue(_repo.Search(null, WordCategories.Sitios).All(c => c.IsSeed && c.Categoria == WordCategories.Sitios));
        }

        [Test]
        public void MoveCategory_AlBorrarLaCategoria()
        {
            var pueblo = _categories.Insert("Mi pueblo");
            _repo.Insert("Villarriba", Five, pueblo.CategoryId);
            Assert.AreEqual(1, _repo.MoveCategory(pueblo.CategoryId, WordCategories.Propias));
            Assert.AreEqual(0, _repo.MoveCategory(WordCategories.Sitios, WordCategories.Propias), "Las del seed no se mueven");
            Assert.AreEqual(1, _repo.CountByCategoria()[WordCategories.Propias]);
        }
    }
}
