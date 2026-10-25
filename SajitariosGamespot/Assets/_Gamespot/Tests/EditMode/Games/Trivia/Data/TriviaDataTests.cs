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
