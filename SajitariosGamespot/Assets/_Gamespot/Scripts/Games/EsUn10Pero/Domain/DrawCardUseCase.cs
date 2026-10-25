using System;
using Gamespot.Core;

namespace Gamespot.Games.EsUn10Pero.Domain
{
    /// <summary>
    /// Saca una carta al azar entre A y 10 de cualquier palo. Dos llamadas al random, en este orden:
    /// primero el valor y después el palo (cada una de las 40 cartas tiene probabilidad 1/40).
    /// </summary>
    public sealed class DrawCardUseCase
    {
        private readonly IRandomProvider _random;

        public DrawCardUseCase(IRandomProvider random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public Card Execute()
        {
            var value = _random.Pick(Cards.Values);
            var suit = _random.Pick(Cards.Suits);
            return new Card(value, suit);
        }
    }
}
