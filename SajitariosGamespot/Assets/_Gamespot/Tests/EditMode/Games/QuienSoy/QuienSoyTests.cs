using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.QuienSoy;
using Gamespot.Games.QuienSoy.Domain;
using Gamespot.Games.Shared;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.QuienSoy
{
    /// <summary>V075: configuración de ¿Quién soy?, detector de inclinación y seed.</summary>
    public sealed class QuienSoyTests
    {
        private static readonly string[] Cats = { QuienSoyCategories.Animales };

        [Test]
        public void Config_LimitesSegunElModo()
        {
            var teams = new QuienSoyOptions { Modo = QuienSoyModo.PorEquipos };
            var solo = new QuienSoyOptions { Modo = QuienSoyModo.CadaUno };
            var five = new[] { "A", "B", "C", "D", "E" };
            Assert.AreEqual(QuienSoyConfigError.DemasiadosNombres, QuienSoyConfig.Create(five, Cats, teams).error, "Equipos: hasta 4");
            Assert.IsNull(QuienSoyConfig.Create(five, Cats, solo).error, "Cada uno: hasta 12");
            Assert.AreEqual(QuienSoyConfigError.DemasiadosNombres,
                QuienSoyConfig.Create(Enumerable.Range(0, 13).Select(i => $"J{i}"), Cats, solo).error);
            Assert.AreEqual(QuienSoyConfigError.PocosNombres, QuienSoyConfig.Create(new[] { "A" }, Cats, solo).error);
            Assert.AreEqual(QuienSoyConfigError.NombresRepetidos, QuienSoyConfig.Create(new[] { "Ana", "ANA" }, Cats, solo).error);
            Assert.AreEqual(QuienSoyConfigError.SinCategorias, QuienSoyConfig.Create(new[] { "A", "B" }, new string[0]).error);
            var (config, _) = QuienSoyConfig.Create(new[] { "A", "B" }, Cats, new QuienSoyOptions { Rondas = 2, Pases = 1, Segundos = 45 });
            Assert.AreEqual(2, config.GuessOptions.Rondas);
            Assert.AreEqual(0, config.GuessOptions.Objetivo, "Por rondas, sin objetivo de puntos");
            Assert.AreEqual(0, config.GuessOptions.Etiquetas);
            Assert.AreEqual(1, config.GuessOptions.Pases);
        }

        [Test]
        public void Inclinacion_AbajoAciertoArribaPasaYHayQueVolver()
        {
            var tilt = new TiltDetector();
            tilt.Calibrar(0.05f);
            Assert.AreEqual(TiltGesture.Ninguno, tilt.Leer(0.4f), "Poco: nada");
            Assert.AreEqual(TiltGesture.Abajo, tilt.Leer(0.8f));
            Assert.AreEqual(TiltGesture.Ninguno, tilt.Leer(0.9f), "Sigue abajo: no vuelve a contar");
            Assert.AreEqual(TiltGesture.Ninguno, tilt.Leer(-0.9f), "Sin volver a la postura no cuenta");
            Assert.AreEqual(TiltGesture.Ninguno, tilt.Leer(0.1f));
            Assert.AreEqual(TiltGesture.Arriba, tilt.Leer(-0.7f));
            Assert.AreEqual(TiltGesture.Ninguno, tilt.Leer(0f));
            Assert.AreEqual(TiltGesture.Abajo, tilt.Leer(0.7f));
        }

        [Test]
        public void Inclinacion_AlRevesYReferenciaVertical()
        {
            var tilt = new TiltDetector { Invertir = true };
            tilt.Calibrar(0f);
            Assert.AreEqual(TiltGesture.Arriba, tilt.Leer(0.8f), "Al revés, abajo es pasar");
            var flat = new TiltDetector();
            flat.Calibrar(-0.95f);
            Assert.AreEqual(0f, flat.Base, "Si al empezar estaba tumbado, la referencia es la vertical");
            Assert.AreEqual(TiltGesture.Arriba, flat.Leer(-0.95f));
            var auto = new TiltDetector();
            Assert.AreEqual(TiltGesture.Ninguno, auto.Leer(0.2f), "Sin calibrar, la primera lectura es la referencia");
            Assert.AreEqual(0.2f, auto.Base, 1e-5f);
        }

        [Test]
        public void Giro_SegunElLadoQueQuedaAbajo()
        {
            Assert.AreEqual(90f, TiltDetector.Giro(0.9f, -90f));
            Assert.AreEqual(-90f, TiltDetector.Giro(-0.9f, 90f));
            Assert.AreEqual(90f, TiltDetector.Giro(0.1f, 90f), "Casi vertical: se queda como estaba");
        }

        [Test]
        public void Seed_CategoriasConTarjetasSinRepetir()
        {
            var cards = PromptCardRepository.ParseSeed(new ResourcesSeedSource().Load(QuienSoySchema.SeedName));
            PromptSeedFilesTests.CheckCommon(cards);
            Assert.That(cards.Count, Is.GreaterThanOrEqualTo(480));
            CollectionAssert.IsSubsetOf(cards.Select(c => c.Category).Distinct(), QuienSoyCategories.Seed);
            foreach (var category in QuienSoyCategories.Seed)
            {
                Assert.That(cards.Count(c => c.Category == category), Is.GreaterThanOrEqualTo(45), category);
            }
            Assert.IsTrue(cards.All(c => c.Text.Length <= 50));
            CollectionAssert.DoesNotContain(QuienSoyCategories.Default, QuienSoyCategories.Cachondeo);
        }
    }
}
