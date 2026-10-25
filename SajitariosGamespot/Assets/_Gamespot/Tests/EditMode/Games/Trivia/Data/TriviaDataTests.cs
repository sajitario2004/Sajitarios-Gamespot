using System.Collections.Generic;
using System.Linq;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Trivia;
using Gamespot.Games.Trivia.Data;
using Gamespot.Games.Trivia.Domain;
using Gamespot.Tests.EditMode.Db;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Trivia
{
    public sealed class TriviaDataTests
    {
        private TempDatabase _temp;
        private QuestionRepository _questions;
        private WinnerRepository _winners;

        [SetUp]
        public void SetUp()
        {
            _temp = new TempDatabase();
            var db = _temp.Open(new IDbSchema[] { new TriviaGame() }, new ResourcesSeedSource());
            _questions = new QuestionRepository(db);
            _winners = new WinnerRepository(db);
        }

        [TearDown]
        public void TearDown() => _temp.Dispose();

        [Test]
        public void MigracionA6_RenuevaLasPreguntasYConservaLasVictorias()
        {
            using (var temp = new TempDatabase())
            {
                var db = temp.Open(new IDbSchema[] { new TriviaGame() }, new ResourcesSeedSource(), version: 5);
                db.Execute("UPDATE trivia_questions SET enunciado = 'vieja' WHERE id = 1");
                new WinnerRepository(db).IncrementWins("Ana");
                temp.Close();

                db = temp.Open(new IDbSchema[] { new TriviaGame() }, new ResourcesSeedSource());
                Assert.AreEqual(0L, db.Scalar<long>("SELECT COUNT(*) FROM trivia_questions WHERE enunciado = 'vieja'"), "El seed viejo se va");
                Assert.That(new QuestionRepository(db).Count(), Is.GreaterThanOrEqualTo(1000), "Y entra el nuevo");
                Assert.AreEqual(1, new WinnerRepository(db).GetWins("Ana"), "Las victorias se quedan");
            }
        }

        [Test]
        public void Seed_ExponentesConEtiquetas()
        {
            var all = TriviaSchema.Parse(new ResourcesSeedSource().Load(TriviaSchema.SeedName));
            var texts = all.SelectMany(q => q.Options.Prepend(q.Enunciado)).ToList();
            Assert.IsFalse(texts.Any(t => t.Contains('\u207B') || t.Contains('\u2082')), "Los ⁻ y ₂ que la fuente no tiene van como <sup>/<sub>");
            Assert.IsTrue(texts.Any(t => t.Contains("10<sup>-11</sup>")));
            Assert.IsTrue(texts.Any(t => t == "H<sub>2</sub>O"));
        }

        [Test]
        public void Seed_1000PreguntasValidasEnLasTresDificultades()
        {
            Assert.That(_questions.Count(), Is.GreaterThanOrEqualTo(1000));
            foreach (var theme in Tematicas.All)
            {
                foreach (var d in Difficulties.All)
                {
                    Assert.That(_questions.CountBy(theme, d), Is.GreaterThan(0), $"{theme} {d}");
                }
            }
        }

        [Test]
        public void Pool_FiltraPorTematicasYDificultad()
        {
            var pool = _questions.GetPool(new[] { "cocina", "cine" }, Difficulty.MuyDificil);
            Assert.That(pool.Count, Is.GreaterThan(0));
            Assert.IsTrue(pool.All(q => (q.TematicaId == "cocina" || q.TematicaId == "cine") && q.Difficulty == Difficulty.MuyDificil));
            Assert.IsTrue(pool.All(q => q.Options.Count == 4));
            CollectionAssert.IsOrdered(pool.Select(q => q.Id));
            Assert.AreEqual(0, _questions.GetPool(new string[0], Difficulty.Facil).Count);
        }

        [Test]
        public void Victorias_PorNombreSinMayusculasYRanking()
        {
            _winners.IncrementWins("Ana");
            _winners.IncrementWins("ana");
            _winners.IncrementWins("Bea");
            _winners.IncrementWins("carlos");
            _winners.IncrementWins("Carlos");
            Assert.AreEqual(2, _winners.GetWins("ANA"));
            Assert.AreEqual(0, _winners.GetWins("Desconocido"));
            CollectionAssert.AreEqual(new[] { "Ana", "carlos", "Bea" }, _winners.GetAllRanked().Select(e => e.Name));
        }

        [Test]
        public void ClaveDeTematica()
        {
            Assert.AreEqual("triviaTematicaCulturaGeneral", TriviaModule.ThemeKey("cultura_general"));
            Assert.AreEqual("triviaTematicaMusica", TriviaModule.ThemeKey("musica"));
        }
    }
}
