using System;
using System.Collections.Generic;
using Gamespot.Core;
using Gamespot.Games.EsUn10Pero.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.EsUn10Pero
{
    public sealed class DrawCardUseCaseTests
    {
        [Test]
        public void SaleCualquieraDeLas40Cartas()
        {
            var useCase = new DrawCardUseCase(new SeededRandom(909090));
            var seen = new HashSet<Card>();
            for (var i = 0; i < 20000 && seen.Count < 40; i++) seen.Add(useCase.Execute());
            Assert.AreEqual(40, seen.Count, "En 20.000 tiradas salen las 40 combinaciones");
        }

        [Test]
        public void NuncaSaleJQK()
        {
            var useCase = new DrawCardUseCase(new SeededRandom(5));
            for (var i = 0; i < 5000; i++)
            {
                Assert.That(useCase.Execute().Value.Number(), Is.InRange(1, 10));
            }
        }

        [Test]
        public void MismaSemilla_MismaSecuencia()
        {
            var a = new DrawCardUseCase(new SeededRandom(42));
            var b = new DrawCardUseCase(new SeededRandom(42));
            for (var i = 0; i < 200; i++) Assert.AreEqual(a.Execute(), b.Execute());
        }

        [Test]
        public void PrimeroElValorDespuesElPalo()
        {
            // 0.0 → primer valor (A); 0.99 → último palo (tréboles)
            Assert.AreEqual(new Card(CardValue.Ace, CardSuit.Treboles), new DrawCardUseCase(new ScriptedRandom(0.0, 0.99)).Execute());
            // 0.99 → último valor (10); 0.0 → primer palo (espadas)
            Assert.AreEqual(new Card(CardValue.Ten, CardSuit.Espadas), new DrawCardUseCase(new ScriptedRandom(0.99, 0.0)).Execute());
        }

        [Test]
        public void EtiquetasYColores()
        {
            Assert.AreEqual("A", CardValue.Ace.Label());
            Assert.AreEqual("10", CardValue.Ten.Label());
            Assert.AreEqual(CardValue.Seven, Cards.FromNumber(7));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cards.FromNumber(11));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cards.FromNumber(0));
            Assert.IsTrue(CardSuit.Corazones.IsRed());
            Assert.IsTrue(CardSuit.Diamantes.IsRed());
            Assert.IsFalse(CardSuit.Espadas.IsRed());
            Assert.IsFalse(CardSuit.Treboles.IsRed());
        }
    }
}
