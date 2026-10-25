using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Games.Shared;
using Gamespot.Tests.EditMode.Db;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Shared
{
    /// <summary>V070: tabla de cartas común (cada juego tiene la suya; el código es el mismo).</summary>
    public sealed class PromptCardRepositoryTests
    {
        private const string Table = "test_cards";

        private TempDatabase _temp;
        private PromptCardRepository _repo;

        [SetUp]
        public void SetUp()
        {
            _temp = new TempDatabase();
            var db = _temp.OpenRaw();
            PromptCardRepository.CreateTable(db, Table);
            PromptCardRepository.CreateTable(db, Table);
            PromptCardRepository.SeedIfEmpty(db, Table, new[]
            {
                new PromptCard(0, "¿Cuál es tu mayor miedo?", PromptLevel.Suave, "verdad", "general"),
                new PromptCard(0, "Baila sin música 30 segundos", PromptLevel.Suave, "reto", "general"),
                new PromptCard(0, "¿Con quién de aquí te liarías?", PromptLevel.Picante, "verdad", "cachondeo")
            });
            _repo = new PromptCardRepository(db, Table);
        }

        [TearDown]
        public void TearDown() => _temp.Dispose();

        [Test]
        public void Seed_SeCargaUnaVezYLasPropiasNoLoImpiden()
        {
            Assert.AreEqual(0, PromptCardRepository.SeedIfEmpty(_temp.Db, Table, new[] { new PromptCard(0, "otra") }), "Ya hay seed");
            _repo.Insert(new PromptCard(0, "Mía"));
            PromptCardRepository.DeleteSeed(_temp.Db, Table);
            Assert.AreEqual(1, _repo.GetCards(null).Count, "Al renovar el seed las propias se quedan");
            Assert.AreEqual(1, PromptCardRepository.SeedIfEmpty(_temp.Db, Table, new[] { new PromptCard(0, "Nueva de serie") }));
            Assert.AreEqual(2, _repo.GetCards(null).Count);
        }

        [Test]
        public void ParseSeed_LeeLosCamposYRechazaLasMalFormadas()
        {
            var cards = PromptCardRepository.ParseSeed(
                "[{\"texto\":\"A\",\"texto_b\":\"B\",\"tipo\":\"reto\",\"nivel\":\"picante\",\"categoria\":\"fiesta\"},{\"texto\":\"Solo\"}]");
            Assert.AreEqual(2, cards.Count);
            Assert.AreEqual("B", cards[0].TextB);
            Assert.AreEqual("reto", cards[0].Kind);
            Assert.AreEqual(PromptLevel.Picante, cards[0].Level);
            Assert.AreEqual("fiesta", cards[0].Category);
            Assert.AreEqual(PromptLevel.Suave, cards[1].Level);
            Assert.AreEqual(string.Empty, cards[1].Kind);
            Assert.Throws<FormatException>(() => PromptCardRepository.ParseSeed("[{\"text\":\"A\"}]"));
            Assert.Throws<FormatException>(() => PromptCardRepository.ParseSeed("[{\"texto\":\"A\",\"nivel\":\"x\"}]"));
        }

        [Test]
        public void GetCards_FiltraParaLaPartida()
        {
            var own = _repo.Insert(new PromptCard(0, "Imita a alguien de aquí", PromptLevel.Suave, "reto", "propias"));
            Assert.AreEqual(4, _repo.GetCards(null).Count);
            Assert.AreEqual(2, _repo.GetCards(new PromptQuery { Kind = "reto" }).Count);
            Assert.AreEqual(1, _repo.GetCards(new PromptQuery { Levels = new[] { PromptLevel.Picante } }).Count);
            Assert.AreEqual(2, _repo.GetCards(new PromptQuery { Categories = new[] { "propias", "cachondeo" } }).Count);
            var onlyOwn = _repo.GetCards(new PromptQuery { IncludeSeed = false });
            Assert.AreEqual(own, onlyOwn.Single().Id);
            Assert.IsTrue(onlyOwn.Single().IsOwn);
            Assert.IsFalse(_repo.GetCards(new PromptQuery { IncludeOwn = false }).Any(c => c.IsOwn));
        }

        [Test]
        public void Search_PropiasPrimeroYFiltros()
        {
            _repo.Insert(new PromptCard(0, "¿Qué es lo más raro que has comido?", PromptLevel.Suave, "verdad", "general"));
            var all = _repo.Search();
            Assert.AreEqual(4, all.Count);
            Assert.IsFalse(all[0].IsSeed, "Las propias, arriba");
            Assert.AreEqual(2, _repo.Search("mayor").Count + _repo.Search("RARO").Count, "Sin distinguir mayúsculas ASCII");
            Assert.AreEqual(1, _repo.Search(null, new PromptCardFilter { OwnOnly = true }).Count);
            Assert.AreEqual(1, _repo.Search(null, new PromptCardFilter { Level = PromptLevel.Picante }).Count);
            Assert.AreEqual(1, _repo.Search(null, new PromptCardFilter { Kind = "reto" }).Count);
            Assert.AreEqual(1, _repo.Search(null, new PromptCardFilter { Category = "cachondeo" }).Count);
            Assert.AreEqual(0, _repo.Search("100%").Count, "El % se busca literal");
        }

        [Test]
        public void Search_TambienEnLaSegundaOpcion()
        {
            _repo.Insert(new PromptCard(0, "Volar", textB: "Ser invisible"), true);
            Assert.AreEqual("Volar", _repo.Search("invisible").Single().Texto);
        }

        [Test]
        public void Insert_ValidaVaciaSegundaYRepetida()
        {
            Assert.AreEqual(PromptCardProblem.FaltaSegunda,
                Assert.Throws<PromptCardException>(() => _repo.Insert(new PromptCard(0, "Volar"), true)).Problem);
            Assert.AreEqual(PromptCardProblem.Duplicada,
                Assert.Throws<PromptCardException>(() => _repo.Insert(new PromptCard(0, "¿cuál es tu mayor miedo", kind: "verdad"))).Problem,
                "Sin mayúsculas ni signos finales");
            Assert.DoesNotThrow(() => _repo.Insert(new PromptCard(0, "¿Cuál es tu mayor miedo?", kind: "reto")), "Con otro tipo sí");
            _repo.Insert(new PromptCard(0, "Volar", textB: "Ser invisible"), true);
            Assert.DoesNotThrow(() => _repo.Insert(new PromptCard(0, "Volar", textB: "Leer mentes"), true), "Con otra segunda opción sí");
            Assert.AreEqual(PromptCardProblem.Duplicada,
                Assert.Throws<PromptCardException>(() => _repo.Insert(new PromptCard(0, "VOLAR", textB: "ser invisible."), true)).Problem);
        }

        [Test]
        public void UpdateYDelete_SoloLasPropias()
        {
            var seed = _repo.Search(null, new PromptCardFilter { Kind = "reto" }).Single();
            Assert.AreEqual(PromptCardProblem.Predefinida,
                Assert.Throws<PromptCardException>(() => _repo.Update(seed.Id, new PromptCard(0, "x"))).Problem);
            Assert.AreEqual(PromptCardProblem.Predefinida, Assert.Throws<PromptCardException>(() => _repo.Delete(seed.Id)).Problem);
            Assert.AreEqual(PromptCardProblem.NoExiste, Assert.Throws<PromptCardException>(() => _repo.Delete(999)).Problem);

            var id = _repo.Insert(new PromptCard(0, "Canta", PromptLevel.Suave, "reto"));
            _repo.Update(id, new PromptCard(0, "Canta una canción", PromptLevel.Picante, "reto", "fiesta"));
            var row = _repo.GetById(id);
            Assert.AreEqual("Canta una canción", row.Texto);
            Assert.AreEqual(PromptLevel.Picante, row.Level);
            Assert.AreEqual("fiesta", row.Categoria);
            Assert.DoesNotThrow(() => _repo.Update(id, new PromptCard(0, "canta una canción.", kind: "reto")), "Consigo misma no choca");
            _repo.Delete(id);
            Assert.IsNull(_repo.GetById(id));
        }

        [Test]
        public void Categorias_ContarYMoverLasPropias()
        {
            _repo.Insert(new PromptCard(0, "Una", category: "custom_1"));
            _repo.Insert(new PromptCard(0, "Dos", category: "custom_1"));
            var counts = _repo.CountByCategory();
            Assert.AreEqual(2, counts["custom_1"]);
            Assert.AreEqual(2, counts["general"]);
            Assert.AreEqual(2, _repo.MoveCategory("custom_1", WordCategories.Propias));
            Assert.AreEqual(0, _repo.MoveCategory("general", WordCategories.Propias), "Las de serie no se mueven");
            Assert.AreEqual(2, _repo.CountByCategory()[WordCategories.Propias]);
            Assert.AreEqual(2, _repo.CountOwn());
        }
    }
}
