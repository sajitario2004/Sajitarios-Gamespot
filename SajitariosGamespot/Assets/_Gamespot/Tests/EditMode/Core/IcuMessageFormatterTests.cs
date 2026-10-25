using System.Collections.Generic;
using Gamespot.Core;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Core
{
    public sealed class IcuMessageFormatterTests
    {
        private static Dictionary<string, object> Args(params (string, object)[] pairs)
        {
            var d = new Dictionary<string, object>();
            foreach (var (k, v) in pairs) d[k] = v;
            return d;
        }

        [Test]
        public void SustituyePlaceholders()
        {
            var text = IcuMessageFormatter.Format("Jugar a {titulo}. {descripcion}", Args(("titulo", "Tabú"), ("descripcion", "Describe sin decir")));
            Assert.AreEqual("Jugar a Tabú. Describe sin decir", text);
        }

        [Test]
        public void PlaceholderQueFalta_SeDejaTalCual()
        {
            Assert.AreEqual("Jugador {n}", IcuMessageFormatter.Format("Jugador {n}", Args()));
        }

        [TestCase(1, "1 impostor")]
        [TestCase(2, "2 impostores")]
        [TestCase(0, "0 impostores")]
        public void Plural_OneYOther(int n, string expected)
        {
            Assert.AreEqual(expected, IcuMessageFormatter.Format("{n, plural, one{{n} impostor} other{{n} impostores}}", Args(("n", n))));
        }

        [TestCase(0, "No había ningún impostor.")]
        [TestCase(1, "Había 1 impostor.")]
        [TestCase(3, "Había 3 impostores.")]
        public void Plural_CasoExactoTienePrioridad(int count, string expected)
        {
            const string pattern = "{count, plural, =0{No había ningún impostor.} one{Había 1 impostor.} other{Había {count} impostores.}}";
            Assert.AreEqual(expected, IcuMessageFormatter.Format(pattern, Args(("count", count))));
        }

        [Test]
        public void Plural_ConTextoAlrededor()
        {
            const string pattern = "{pts, plural, one{{pts} punto} other{{pts} puntos}} esta ronda";
            Assert.AreEqual("1 punto esta ronda", IcuMessageFormatter.Format(pattern, Args(("pts", 1))));
            Assert.AreEqual("4 puntos esta ronda", IcuMessageFormatter.Format(pattern, Args(("pts", 4))));
        }

        [Test]
        public void Plural_AlmohadillaEsElNumero()
        {
            Assert.AreEqual("5 rondas", IcuMessageFormatter.Format("{n, plural, one{# ronda} other{# rondas}}", Args(("n", 5))));
        }

        [Test]
        public void TextoSinArgumentos_SeQuedaIgual()
        {
            Assert.AreEqual("¿Cómo se juega?", IcuMessageFormatter.Format("¿Cómo se juega?", null));
        }
    }
}
