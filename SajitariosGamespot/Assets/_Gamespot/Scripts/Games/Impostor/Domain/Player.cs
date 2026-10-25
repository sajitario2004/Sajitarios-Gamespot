using System;

namespace Gamespot.Games.Impostor.Domain
{
    /// <summary>Jugador del Impostor. Igualdad por el nombre exacto (distingue mayúsculas).</summary>
    public sealed class Player : IEquatable<Player>
    {
        public Player(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }

        public bool Equals(Player other) => other != null && string.Equals(other.Name, Name, StringComparison.Ordinal);
        public override bool Equals(object obj) => Equals(obj as Player);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Name);
        public override string ToString() => $"Player({Name})";
    }
}
