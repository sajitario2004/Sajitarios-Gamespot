using System;
using System.Collections.Generic;

namespace Gamespot.Games.EsUn10Pero.Domain
{
    /// <summary>Valores de la carta: del As al 10 (sin J, Q ni K).</summary>
    public enum CardValue
    {
        Ace = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10
    }

    /// <summary>Palos de la baraja francesa. El nombre visible lo pone la presentación (localizado).</summary>
    public enum CardSuit
    {
        Espadas,
        Corazones,
        Diamantes,
        Treboles
    }

    /// <summary>Listas ordenadas y ayudas de <see cref="CardValue"/> y <see cref="CardSuit"/>.</summary>
    public static class Cards
    {
        /// <summary>Los 10 valores en orden: A, 2 … 10.</summary>
        public static readonly IReadOnlyList<CardValue> Values = new[]
        {
            CardValue.Ace, CardValue.Two, CardValue.Three, CardValue.Four, CardValue.Five,
            CardValue.Six, CardValue.Seven, CardValue.Eight, CardValue.Nine, CardValue.Ten
        };

        /// <summary>Los 4 palos en orden: espadas, corazones, diamantes, tréboles.</summary>
        public static readonly IReadOnlyList<CardSuit> Suits = new[]
        {
            CardSuit.Espadas, CardSuit.Corazones, CardSuit.Diamantes, CardSuit.Treboles
        };

        /// <summary>Número de la carta (1–10).</summary>
        public static int Number(this CardValue value) => (int)value;

        /// <summary>Etiqueta de la esquina: "A" para el As y el número para el resto.</summary>
        public static string Label(this CardValue value) => value == CardValue.Ace ? "A" : ((int)value).ToString();

        /// <summary>Valor a partir de su número. Lanza fuera de 1–10.</summary>
        public static CardValue FromNumber(int number)
        {
            if (number < 1 || number > 10) throw new ArgumentOutOfRangeException(nameof(number), number, "La carta tiene que estar entre A (1) y 10");
            return (CardValue)number;
        }

        /// <summary>Corazones y diamantes son rojos.</summary>
        public static bool IsRed(this CardSuit suit) => suit == CardSuit.Corazones || suit == CardSuit.Diamantes;
    }

    /// <summary>Una carta: valor + palo. Igualdad por valor.</summary>
    public sealed class Card : IEquatable<Card>
    {
        public Card(CardValue value, CardSuit suit)
        {
            Value = value;
            Suit = suit;
        }

        public CardValue Value { get; }
        public CardSuit Suit { get; }

        public bool Equals(Card other) => other != null && other.Value == Value && other.Suit == Suit;
        public override bool Equals(object obj) => Equals(obj as Card);
        public override int GetHashCode() => ((int)Value * 397) ^ (int)Suit;
        public override string ToString() => $"{Value.Label()} de {Suit}";
    }
}
