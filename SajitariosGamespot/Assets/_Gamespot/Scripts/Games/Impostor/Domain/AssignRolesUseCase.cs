using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.Impostor.Domain
{
    /// <summary>
    /// Reparto de roles del Impostor: el núcleo testeado del proyecto (CLAUDE.md §5).
    /// Orden exacto de llamadas al random:
    /// 1. <c>Pick(words)</c> elige la palabra.
    /// 2. Un solo <c>NextDouble()</c> decide el caso: &lt; 0.10 todos impostores, &lt; 0.20 ninguno,
    ///    resto reparto normal.
    /// 3. Solo en el reparto normal: se baraja una copia de los jugadores (Fisher-Yates) y los primeros
    ///    <c>NImpostores</c> son impostores. La lista original (orden de revelación) no se toca.
    /// </summary>
    public sealed class AssignRolesUseCase
    {
        /// <summary>Por debajo de este valor, TODOS son impostores (10 %).</summary>
        public const double TodosImpostoresThreshold = 0.10;

        /// <summary>Entre el anterior y este valor, NINGUNO es impostor (otro 10 %).</summary>
        public const double NingunoImpostorThreshold = 0.20;

        private readonly IRandomProvider _random;

        public AssignRolesUseCase(IRandomProvider random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public GameSession Execute(GameConfig config, IReadOnlyList<Word> words)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (words == null || words.Count == 0) throw new ArgumentException("No hay palabras disponibles para la partida", nameof(words));

            var word = _random.Pick(words);
            var players = config.Players;

            // UNA sola tirada decide el caso (10 / 10 / 80).
            var roll = _random.NextDouble();
            Dictionary<Player, Role> assignments;
            if (roll < TodosImpostoresThreshold) assignments = AllWith(players, Role.Impostor);
            else if (roll < NingunoImpostorThreshold) assignments = AllWith(players, Role.Palabra);
            else assignments = AssignNormal(players, config.NImpostores);

            return new GameSession(word, players, assignments);
        }

        private Dictionary<Player, Role> AssignNormal(IReadOnlyList<Player> players, int nImpostores)
        {
            var shuffled = _random.Shuffled(players);
            var impostores = new HashSet<Player>(shuffled.Take(nImpostores));
            return players.ToDictionary(p => p, p => impostores.Contains(p) ? Role.Impostor : Role.Palabra);
        }

        private static Dictionary<Player, Role> AllWith(IReadOnlyList<Player> players, Role role) => players.ToDictionary(p => p, _ => role);
    }
}
