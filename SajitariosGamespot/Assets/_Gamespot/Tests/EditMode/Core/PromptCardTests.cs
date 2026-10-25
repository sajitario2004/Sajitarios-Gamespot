using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Core
{
    /// <summary>V070: cartas comunes, filtro de la partida y mazo sin repeticiones.</summary>
    public sealed class PromptCardTests
    {
        private static List<PromptCard> Cards(int n) => Enumerable.Range(1, n).Select(i => new PromptCard(i, $"Carta {i}")).ToList();

        [Test]
        public void Niveles_SeGuardanComoTexto()
        {
            Assert.AreEqual("suave", PromptLevel.Suave.StorageName());
            Assert.AreEqual("picante", PromptLevel.Picante.StorageName());
            Assert.AreEqual(PromptLevel.Picante, PromptLevels.FromStorageName("picante"));
            Assert.AreEqual(PromptLevel.Suave, PromptLevels.FromStorageName(null), "Sin nivel = suave");
            Assert.AreEqual(PromptLevel.Suave, PromptLevels.FromStorageName(""));
            Assert.Throws<FormatException>(() => PromptLevels.FromStorageName("fuerte"));
        }

        [Test]
        public void Carta_RecortaYNoAdmiteVacia()
        {
            var card = new PromptCard(3, "  Hola ", PromptLevel.Picante, " reto ", " general ", "   ", true);
            Assert.AreEqual("Hola", card.Text);
            Assert.AreEqual("reto", card.Kind);
            Assert.AreEqual("general", card.Category);
            Assert.IsNull(card.TextB, "Segunda opción en blanco = sin segunda opción");
            Assert.IsTrue(card.IsOwn);
            Assert.Throws<ArgumentException>(() => new PromptCard(1, "  "));
        }

        [Test]
        public void Consulta_FiltraPorNivelTipoCategoriaYOrigen()
        {
            var seed = new PromptCard(1, "a", PromptLevel.Suave, "verdad", "x");
            var own = new PromptCard(2, "b", PromptLevel.Picante, "reto", "y", isOwn: true);
            Assert.IsTrue(new PromptQuery().Matches(seed) && new PromptQuery().Matches(own), "Sin filtros entra todo");
            Assert.IsFalse(new PromptQuery { Levels = new[] { PromptLevel.Suave } }.Matches(own));
            Assert.IsTrue(new PromptQuery { Kind = "reto" }.Matches(own));
            Assert.IsFalse(new PromptQuery { Kind = "reto" }.Matches(seed));
            Assert.IsFalse(new PromptQuery { Categories = new[] { "y" } }.Matches(seed));
            Assert.IsFalse(new PromptQuery { IncludeSeed = false }.Matches(seed));
            Assert.IsFalse(new PromptQuery { IncludeOwn = false }.Matches(own));
        }

        [Test]
        public void Mazo_NoRepiteHastaAgotarYDespuesNoRepiteLaUltima()
        {
            var pool = Cards(5);
            var deck = new NoRepeatDeck(new SeededRandom(7));
            var first = Enumerable.Range(0, 5).Select(_ => deck.Draw(pool).Id).ToList();
            CollectionAssert.AreEquivalent(pool.Select(c => c.Id), first, "Salen las cinco sin repetir");
            Assert.IsFalse(deck.Reshuffled);
            for (var round = 0; round < 50; round++)
            {
                var last = deck.Draw(pool);
                var next = deck.Draw(pool);
                Assert.AreNotEqual(last.Id, next.Id, "Nunca dos veces seguidas la misma");
            }
        }

        [Test]
        public void Mazo_AvisaAlVolverAEmpezarYConUnaSolaCartaLaRepite()
        {
            var pool = Cards(2);
            var deck = new NoRepeatDeck(new SeededRandom(1));
            deck.Draw(pool);
            deck.Draw(pool);
            Assert.AreEqual(2, deck.SeenCount);
            deck.Draw(pool);
            Assert.IsTrue(deck.Reshuffled);
            Assert.AreEqual(1, deck.SeenCount);
            var single = new NoRepeatDeck(new SeededRandom(1));
            var one = Cards(1);
            Assert.AreEqual(1, single.Draw(one).Id);
            Assert.AreEqual(1, single.Draw(one).Id);
            Assert.Throws<ArgumentException>(() => single.Draw(new List<PromptCard>()));
        }

        [Test]
        public void Mazo_UnaSolaLlamadaAlRandomPorCarta()
        {
            var pool = Cards(4);
            var rng = new ScriptedRandom(0.0, 0.0, 0.99, 0.0);
            var deck = new NoRepeatDeck(rng);
            // Restantes en orden de id: [1,2,3,4] → 1; [2,3,4] → 2; [3,4] → 4; [3] → 3.
            CollectionAssert.AreEqual(new long[] { 1, 2, 4, 3 }, Enumerable.Range(0, 4).Select(_ => deck.Draw(pool).Id).ToList());
            Assert.AreEqual(4, rng.Consumed);
        }

        [Test]
        public void Mazo_ReiniciarOlvidaLasVistas()
        {
            var pool = Cards(3);
            var deck = new NoRepeatDeck(new SeededRandom(3));
            deck.Draw(pool);
            deck.Draw(pool);
            deck.Reset();
            Assert.AreEqual(0, deck.SeenCount);
            Assert.IsFalse(deck.Reshuffled);
        }
    }
}
