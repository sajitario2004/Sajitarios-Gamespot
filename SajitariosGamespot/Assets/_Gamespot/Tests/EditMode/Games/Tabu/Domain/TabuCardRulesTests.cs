using Gamespot.Games.Tabu.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Tabu
{
    /// <summary>V059l: reglas de las tarjetas que escribe el usuario.</summary>
    public sealed class TabuCardRulesTests
    {
        [Test]
        public void TarjetaValida_SeLimpiaYLosHuecosNoCuentan()
        {
            var error = TabuCardRules.Validate("  Mi  pueblo ", new[] { " plaza", "fiestas ", "", "bar", "  iglesia  " }, out var palabra, out var prohibidas);
            Assert.IsNull(error);
            Assert.AreEqual("Mi pueblo", palabra);
            CollectionAssert.AreEqual(new[] { "plaza", "fiestas", "bar", "iglesia" }, prohibidas);
        }

        [Test]
        public void PalabraVaciaYPocasProhibidas()
        {
            Assert.AreEqual(TabuCardError.PalabraVacia, TabuCardRules.Validate(" ", new[] { "a", "b", "c", "d" }, out _, out _));
            Assert.AreEqual(TabuCardError.PocasProhibidas, TabuCardRules.Validate("sol", new[] { "a", "b", "", "c" }, out _, out _));
            Assert.AreEqual(TabuCardError.PocasProhibidas, TabuCardRules.Validate("sol", null, out _, out _));
            Assert.AreEqual(TabuCardError.PocasProhibidas, TabuCardRules.Validate("sol", new[] { "a", "b", "c", "d", "e", "f" }, out _, out _));
        }

        [Test]
        public void ProhibidasRepetidasOIgualesALaPalabra()
        {
            Assert.AreEqual(TabuCardError.ProhibidaRepetida, TabuCardRules.Validate("sol", new[] { "Luz", "calor", "luz", "día" }, out _, out _));
            Assert.AreEqual(TabuCardError.ProhibidaRepetida, TabuCardRules.Validate("Sol", new[] { "SOL", "calor", "luz", "día" }, out _, out _));
            Assert.AreEqual(TabuCardError.ProhibidaRepetida, TabuCardRules.Validate("ñandú", new[] { "Ñandú", "ave", "plumas", "correr" }, out _, out _));
        }
    }
}
