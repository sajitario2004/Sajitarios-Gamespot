using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamespot.Games.Impostor.Domain
{
    /// <summary>Por qué no se puede crear la configuración (se comprueban en este orden).</summary>
    public enum GameConfigError
    {
        PocosJugadores,
        DemasiadosJugadores,
        NombreVacio,
        NombresDuplicados
    }

    /// <summary>Resultado de <see cref="GameConfig.Create"/>: la configuración o el error, sin excepciones.</summary>
    public sealed class GameConfigResult
    {
        private GameConfigResult(GameConfig config, GameConfigError? error)
        {
            Config = config;
            Error = error;
        }

        public GameConfig Config { get; }
        public GameConfigError? Error { get; }
        public bool IsSuccess => Config != null;

        internal static GameConfigResult Success(GameConfig config) => new GameConfigResult(config, null);
        internal static GameConfigResult Failure(GameConfigError error) => new GameConfigResult(null, error);
    }

    /// <summary>
    /// Configuración de una partida del Impostor. Los límites vienen de las reglas del autor:
    /// 3–15 jugadores, 1–5 impostores capados a jugadores − 1, y rondas de votación 1..max(1, jugadores − 3).
    /// </summary>
    public sealed class GameConfig
    {
        public const int MinPlayers = 3;
        public const int MaxPlayers = 15;
        public const int MinImpostores = 1;
        public const int MaxImpostores = 5;
        public const int MinRounds = 1;

        private GameConfig(IReadOnlyList<Player> players, int nImpostores, bool hintEnabled, int rounds)
        {
            Players = players;
            NImpostores = nImpostores;
            HintEnabled = hintEnabled;
            Rounds = rounds;
        }

        /// <summary>Jugadores en el orden en que se introdujeron (= orden de revelación).</summary>
        public IReadOnlyList<Player> Players { get; }
        public int NImpostores { get; }
        public bool HintEnabled { get; }
        /// <summary>Oportunidades de votación (rondas).</summary>
        public int Rounds { get; }

        /// <summary>Máximo de rondas para ese número de jugadores: max(1, n − 3).</summary>
        public static int MaxRoundsFor(int playerCount) => Math.Max(1, playerCount - 3);

        /// <summary>Máximo de impostores en el reparto normal: min(5, n − 1).</summary>
        public static int MaxImpostoresFor(int playerCount) => Math.Min(MaxImpostores, playerCount - 1);

        /// <summary>
        /// Valida y normaliza. Los impostores y las rondas se recortan a su rango; si <paramref name="rounds"/>
        /// es nulo se usa el máximo. Los nombres se guardan tal cual (el setup ya los normaliza).
        /// </summary>
        public static GameConfigResult Create(IReadOnlyList<Player> players, int nImpostores, bool hintEnabled = false, int? rounds = null)
        {
            if (players == null) throw new ArgumentNullException(nameof(players));
            if (players.Count < MinPlayers) return GameConfigResult.Failure(GameConfigError.PocosJugadores);
            if (players.Count > MaxPlayers) return GameConfigResult.Failure(GameConfigError.DemasiadosJugadores);
            if (players.Any(p => p.Name.Trim().Length == 0)) return GameConfigResult.Failure(GameConfigError.NombreVacio);

            var seen = new HashSet<string>();
            foreach (var player in players)
            {
                if (!seen.Add(player.Name.Trim().ToLowerInvariant())) return GameConfigResult.Failure(GameConfigError.NombresDuplicados);
            }

            var impostores = Clamp(nImpostores, MinImpostores, MaxImpostoresFor(players.Count));
            var roundsCap = MaxRoundsFor(players.Count);
            var normalizedRounds = Clamp(rounds ?? roundsCap, MinRounds, roundsCap);
            return GameConfigResult.Success(new GameConfig(players.ToList().AsReadOnly(), impostores, hintEnabled, normalizedRounds));
        }

        private static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
    }
}
