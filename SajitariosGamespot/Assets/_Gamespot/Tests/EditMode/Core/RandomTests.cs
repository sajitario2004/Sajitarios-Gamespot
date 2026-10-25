using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Core
{
    public sealed class RandomTests
    {
        /// <summary>Guarda los argumentos de cada NextInt para comprobar el orden de llamadas.</summary>
        private sealed class RecordingRandom : IRandomProvider
        {
            public readonly List<int> Calls = new List<int>();
            public int NextInt(int max)
            {
                Calls.Add(max);
                return 0;
            }
            public double NextDouble() => 0;
            public bool NextBool() => false;
        }

        [Test]
        public void SeededRandom_MismaSemilla_MismaSecuencia()
        {
            var a = new SeededRandom(1234);
            var b = new SeededRandom(1234);
            for (var i = 0; i < 100; i++)
            {
                Assert.AreEqual(a.NextInt(1000), b.NextInt(1000));
                Assert.AreEqual(a.NextDouble(), b.NextDouble());
            }
        }

        [Test]
        public void SeededRandom_RangosCorrectos()
        {
            var rng = new SeededRandom(7);
            for (var i = 0; i < 5000; i++)
            {
                var n = rng.NextInt(10);
                Assert.That(n, Is.InRange(0, 9));
                var d = rng.NextDouble();
                Assert.That(d, Is.GreaterThanOrEqualTo(0).And.LessThan(1));
            }
        }

        [Test]
        public void NextInt_ConMaxNoPositivo_Lanza()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SeededRandom(1).NextInt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScriptedRandom(0.5).NextInt(-1));
        }

        [Test]
        public void Pick_ListaVacia_Lanza()
        {
            Assert.Throws<ArgumentException>(() => new SeededRandom(1).Pick(new List<int>()));
        }

        [Test]
        public void Pick_UsaNextIntSobreElTotal()
        {
            var rng = new ScriptedRandom(0.0, 0.99, 0.5);
            var items = new[] { "a", "b", "c", "d" };
            Assert.AreEqual("a", rng.Pick(items));
            Assert.AreEqual("d", rng.Pick(items));
            Assert.AreEqual("c", rng.Pick(items));
        }

        [Test]
        public void Shuffle_OrdenDeLlamadasComoElFlutter()
        {
            var rng = new RecordingRandom();
            var list = new List<int> { 1, 2, 3, 4, 5 };
            rng.Shuffle(list);
            // Fisher-Yates: i = 4..1 → NextInt(5), NextInt(4), NextInt(3), NextInt(2)
            CollectionAssert.AreEqual(new[] { 5, 4, 3, 2 }, rng.Calls);
        }

        [Test]
        public void Shuffle_EsUnaPermutacion()
        {
            var rng = new SeededRandom(99);
            var original = Enumerable.Range(0, 50).ToList();
            var shuffled = rng.Shuffled(original);
            CollectionAssert.AreEquivalent(original, shuffled);
            CollectionAssert.AreEqual(Enumerable.Range(0, 50), original, "La lista original no se toca");
        }

        [Test]
        public void ScriptedRandom_SeAcaba_Lanza()
        {
            var rng = new ScriptedRandom(0.1);
            rng.NextDouble();
            Assert.Throws<InvalidOperationException>(() => rng.NextDouble());
        }

        [Test]
        public void ScriptedRandom_EnBucle_Repite()
        {
            var rng = new ScriptedRandom(true, 0.1, 0.2);
            Assert.AreEqual(0.1, rng.NextDouble());
            Assert.AreEqual(0.2, rng.NextDouble());
            Assert.AreEqual(0.1, rng.NextDouble());
        }

        [Test]
        public void ScriptedRandom_RechazaValoresFueraDeRango()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScriptedRandom(1.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScriptedRandom(-0.1));
        }
    }
}
