using System;

namespace Gamespot.Games.Impostor.Domain
{
    /// <summary>
    /// Palabra de la partida con su pista, que es obligatoria (regla del autor: toda palabra lleva
    /// pista por si se activa la opción). Ambas se recortan y no pueden quedar vacías.
    /// </summary>
    public sealed class Word : IEquatable<Word>
    {
        public Word(string text, string hint)
        {
            var cleanText = text?.Trim() ?? string.Empty;
            var cleanHint = hint?.Trim() ?? string.Empty;
            if (cleanText.Length == 0) throw new ArgumentException("La palabra no puede estar vacía", nameof(text));
            if (cleanHint.Length == 0) throw new ArgumentException("La pista no puede estar vacía", nameof(hint));
            Text = cleanText;
            Hint = cleanHint;
        }

        public string Text { get; }
        public string Hint { get; }

        public bool Equals(Word other) => other != null && other.Text == Text && other.Hint == Hint;
        public override bool Equals(object obj) => Equals(obj as Word);
        public override int GetHashCode() => (Text.GetHashCode() * 397) ^ Hint.GetHashCode();
        public override string ToString() => $"Word({Text}, pista: {Hint})";
    }
}
