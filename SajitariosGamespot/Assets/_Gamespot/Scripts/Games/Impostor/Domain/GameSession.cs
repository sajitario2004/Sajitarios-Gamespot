using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamespot.Games.Impostor.Domain
{
    /// <summary>
    /// Partida en curso: palabra, jugadores y el rol de cada uno. El orden de revelación es el de
    /// introducción de los nombres (nunca se baraja para revelar).
    /// </summary>
    public sealed class GameSession
    {
        private readonly Dictionary<Player, Role> _assignments;

        public GameSession(Word word, IReadOnlyList<Player> players, IReadOnlyDictionary<Player, Role> assignments)
        {
            Word = word ?? throw new ArgumentNullException(nameof(word));
            if (players == null) throw new ArgumentNullException(nameof(players));
            if (assignments == null) throw new ArgumentNullException(nameof(assignments));
            foreach (var player in players)
            {
                if (!assignments.ContainsKey(player)) throw new ArgumentException($"Falta el rol del jugador \"{player.Name}\"", nameof(assignments));
            }
            if (assignments.Count != players.Count) throw new ArgumentException("El número de asignaciones no coincide con el de jugadores", nameof(assignments));

            Players = players.ToList().AsReadOnly();
            _assignments = assignments.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public Word Word { get; }
        public IReadOnlyList<Player> Players { get; }
        public string Hint => Word.Hint;

        /// <summary>Orden de revelación = orden de introducción.</summary>
        public IReadOnlyList<Player> RevealOrder => Players;

        /// <summary>Rol del jugador, o nulo si no es de esta partida.</summary>
        public Role? RoleOf(Player player) => player != null && _assignments.TryGetValue(player, out var role) ? role : (Role?)null;

        public bool IsImpostor(Player player) => RoleOf(player) == Role.Impostor;

        /// <summary>Impostores en orden de revelación.</summary>
        public IReadOnlyList<Player> Impostores => Players.Where(IsImpostor).ToList();

        public int ImpostorCount => _assignments.Values.Count(r => r == Role.Impostor);
    }
}
