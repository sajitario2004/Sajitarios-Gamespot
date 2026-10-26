using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.YoNunca;
using Gamespot.Games.YoNunca.Domain;
using Gamespot.Tests.EditMode.Db;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.YoNunca
{
    /// <summary>V059q: frases propias de Yo Nunca y el seed con el picante +18 nuevo.</summary>
    public sealed class YoNuncaPhrasesTests
    {
        private TempDatabase _temp;
        private NeverStatementRepository _repo;

        [SetUp]
        public void SetUp()
        {
            _temp = new TempDatabase();
            _repo = new NeverStatementRepository(_temp.Open(new IDbSchema[] { new YoNuncaGame() }, new ResourcesSeedSource()));
        }

        [TearDown]
        public void TearDown() => _temp.Dispose();

        private static YoNuncaPhraseProblem Problem(TestDelegate action) => Assert.Throws<YoNuncaPhraseException>(action).Problem;

        private static string Plain(string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s.ToLowerInvariant().Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            return sb.ToString().Trim(' ', '.');
        }

        [Test]
        public void Seed_PicanteMuchoMasGrandeYSinFrasesQueNoTocan()
        {
            var all = YoNuncaSchema.Parse(new ResourcesSeedSource().Load(YoNuncaSchema.SeedName));
            Assert.That(all.Count(s => s.Intensidad == Intensidad.Picante), Is.GreaterThanOrEqualTo(250));
            Assert.That(all.Count(s => s.Intensidad == Intensidad.Suave), Is.GreaterThanOrEqualTo(90));
            Assert.IsTrue(all.All(s => s.Frase.StartsWith("Yo nunca ")), "Todas empiezan por «Yo nunca»");
            Assert.AreEqual(all.Count, all.Select(s => Plain(s.Frase)).Distinct().Count(), "Sin repetidas");
            foreach (var s in all)
            {
                Assert.IsFalse(Regex.IsMatch(Plain(s.Frase), @"\b(menor|menores|nin[oa]s?|adolescente|colegio|instituto)\b"), s.Frase);
                StringAssert.DoesNotContain("espiado a alguien", s.Frase, "Nada sin consentimiento");
            }
        }

        [Test]
        public void Propias_AltaEdicionBorradoYSoloLasSuyas()
        {
            var id = _repo.Insert("Yo nunca he ido a Ibiza", Intensidad.Suave);
            Assert.AreEqual(1, _repo.CountOwn());
            var row = _repo.GetById(id);
            Assert.IsFalse(row.IsSeed);
            Assert.AreEqual(Intensidad.Suave, row.Level);

            _repo.Update(id, "Yo nunca he ido a Ibiza en invierno", Intensidad.Picante);
            Assert.AreEqual(Intensidad.Picante, _repo.GetById(id).Level);
            Assert.AreEqual("Yo nunca he ido a Ibiza en invierno", _repo.Search(null, YoNuncaPhraseFilter.Propias).Single().Frase);

            var seed = _repo.Search(null, YoNuncaPhraseFilter.Suaves).First(r => r.IsSeed);
            Assert.AreEqual(YoNuncaPhraseProblem.Predefinida, Problem(() => _repo.Update(seed.Id, "Yo nunca algo", Intensidad.Suave)));
            Assert.AreEqual(YoNuncaPhraseProblem.Predefinida, Problem(() => _repo.Delete(seed.Id)));
            Assert.AreEqual(YoNuncaPhraseProblem.Duplicada, Problem(() => _repo.Insert(seed.Frase.ToUpperInvariant() + ".", Intensidad.Suave)),
                "Ni con otras mayúsculas ni con punto final");
            Assert.AreEqual(YoNuncaPhraseProblem.Vacia, Problem(() => _repo.Insert("  ", Intensidad.Suave)));

            _repo.Delete(id);
            Assert.AreEqual(0, _repo.CountOwn());
            Assert.AreEqual(YoNuncaPhraseProblem.NoExiste, Problem(() => _repo.Delete(id)));
        }

        [Test]
        public void Origen_DeSerieMiasOLasDos()
        {
            _repo.Insert("Yo nunca he cantado en un karaoke de Benidorm", Intensidad.Suave);
            var source = new OriginNeverStatementSource(_repo);
            var suave = new[] { Intensidad.Suave };
            var both = source.GetByIntensidades(suave).Count;
            source.IncludeSeed = false;
            Assert.AreEqual("Yo nunca he cantado en un karaoke de Benidorm", source.GetByIntensidades(suave).Single().Frase);
            source.IncludeSeed = true;
            source.IncludeOwn = false;
            Assert.AreEqual(both - 1, source.GetByIntensidades(suave).Count);
            Assert.AreEqual(0, source.GetByIntensidades(new[] { Intensidad.Picante }).Count(s => s.Frase.Contains("Benidorm")));
        }

        [Test]
        public void MigracionA7_RenuevaElSeedYConservaLasPropias()
        {
            using (var temp = new TempDatabase())
            {
                var db = temp.Open(new IDbSchema[] { new YoNuncaGame() }, new ResourcesSeedSource(), version: 6);
                db.Execute("DELETE FROM yo_nunca_statements WHERE is_seed = 1");
                db.Execute("INSERT INTO yo_nunca_statements (frase, intensidad, is_seed) VALUES ('Yo nunca vieja', 'picante', 1)");
                db.Execute("INSERT INTO yo_nunca_statements (frase, intensidad, is_seed) VALUES ('Yo nunca mía', 'suave', 0)");
                temp.Close();

                db = temp.Open(new IDbSchema[] { new YoNuncaGame() }, new ResourcesSeedSource());
                var repo = new NeverStatementRepository(db);
                Assert.IsFalse(repo.Search("vieja").Any(), "El seed viejo se va");
                Assert.AreEqual("Yo nunca mía", repo.Search(null, YoNuncaPhraseFilter.Propias).Single().Frase, "La propia se queda");
                Assert.That(repo.GetByIntensidades(new[] { Intensidad.Picante }).Count, Is.GreaterThanOrEqualTo(250), "Y entra el nuevo");
                Assert.AreEqual(0, YoNuncaSchema.SeedIfEmpty(db, new ResourcesSeedSource()), "No se siembra dos veces");
            }
        }
    }
}
