using System.Linq;
using Gamespot.Core;
using Gamespot.Games.Shared;
using Gamespot.Tests.EditMode.Db;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Shared
{
    /// <summary>V059l: categorías propias (la tabla es de cada juego; el código, común).</summary>
    public sealed class CustomCategoryRepositoryTests
    {
        private const string Table = "test_categories";

        private TempDatabase _temp;
        private CustomCategoryRepository _repo;
        private long _clock;

        [SetUp]
        public void SetUp()
        {
            // NUnit reutiliza la instancia entre tests: el reloj falso se reinicia aquí.
            _clock = 0;
            _temp = new TempDatabase();
            var db = _temp.OpenRaw();
            CustomCategoryRepository.CreateTable(db, Table);
            CustomCategoryRepository.CreateTable(db, Table);
            _repo = new CustomCategoryRepository(db, Table, () => ++_clock);
        }

        [TearDown]
        public void TearDown() => _temp.Dispose();

        [Test]
        public void Insert_LimpiaElNombreYDaSuId()
        {
            var created = _repo.Insert("  Mi   pueblo ");
            Assert.AreEqual("Mi pueblo", created.Name);
            Assert.AreEqual(WordCategories.CustomId(created.Id), created.CategoryId);
            Assert.IsTrue(WordCategories.IsCustom(created.CategoryId));
            Assert.AreEqual(1, created.CreatedAt);
            Assert.AreEqual(CustomCategoryRepository.MaxNameLength, CustomCategoryRepository.Clean(new string('a', 40)).Length, "Se recorta al máximo");
        }

        [Test]
        public void Insert_VacioORepetidoSinMayusculasNiConLasDeSerie()
        {
            Assert.AreEqual(CategoryNameError.Vacio, Assert.Throws<CategoryNameException>(() => _repo.Insert("   ")).Error);
            _repo.Insert("Ñoños");
            Assert.AreEqual(CategoryNameError.Duplicado, Assert.Throws<CategoryNameException>(() => _repo.Insert("ñoños")).Error,
                "Las mayúsculas no ASCII también cuentan");
            Assert.AreEqual(CategoryNameError.Duplicado, Assert.Throws<CategoryNameException>(() => _repo.Insert("fiesta", new[] { "Fiesta" })).Error,
                "No puede llamarse como una de serie");
            Assert.AreEqual(1, _repo.GetAll().Count);
        }

        [Test]
        public void Rename_PuedeCambiarMayusculasPeroNoChocarConOtra()
        {
            var a = _repo.Insert("Clase");
            _repo.Insert("Trabajo");
            _repo.Rename(a.Id, "CLASE");
            Assert.AreEqual("CLASE", _repo.GetById(a.Id).Name);
            Assert.AreEqual(CategoryNameError.Duplicado, Assert.Throws<CategoryNameException>(() => _repo.Rename(a.Id, "trabajo")).Error);
            Assert.AreEqual(CategoryNameError.Vacio, Assert.Throws<CategoryNameException>(() => _repo.Rename(a.Id, " ")).Error);
        }

        [Test]
        public void GetAll_PorNombreYDelete()
        {
            var b = _repo.Insert("bares");
            _repo.Insert("Amigos");
            _repo.Insert("Clase");
            CollectionAssert.AreEqual(new[] { "Amigos", "bares", "Clase" }, _repo.GetAll().Select(c => c.Name));
            _repo.Delete(b.Id);
            CollectionAssert.AreEqual(new[] { "Amigos", "Clase" }, _repo.GetAll().Select(c => c.Name));
            Assert.IsNull(_repo.GetById(b.Id));
        }
    }
}
