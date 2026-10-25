using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.Trivia.Domain
{
    /// <summary>Jugador de la trivia. Igualdad por nombre exacto.</summary>
    public sealed class TriviaPlayer : IEquatable<TriviaPlayer>
    {
        public TriviaPlayer(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));

        public string Name { get; }

        public bool Equals(TriviaPlayer other) => other != null && string.Equals(other.Name, Name, StringComparison.Ordinal);
        public override bool Equals(object obj) => Equals(obj as TriviaPlayer);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Name);
        public override string ToString() => Name;
    }

    public enum TriviaConfigError
    {
        PocosJugadores,
        DemasiadosJugadores,
        NombreVacio,
        NombresDuplicados,
        SinTematicas
    }

    /// <summary>Configuración: 2–6 jugadores (nombres recortados) y al menos una temática.</summary>
    public sealed class TriviaConfig
    {
        public const int MinPlayers = 2;
        public const int MaxPlayers = 6;

        private TriviaConfig(IReadOnlyList<string> playerNames, IReadOnlyCollection<string> tematicaIds)
        {
            PlayerNames = playerNames;
            TematicaIds = tematicaIds;
        }

        public IReadOnlyList<string> PlayerNames { get; }
        public IReadOnlyCollection<string> TematicaIds { get; }

        /// <summary>Errores en orden: pocos, demasiados, nombre vacío, duplicados (sin mayúsculas), sin temáticas.</summary>
        public static (TriviaConfig config, TriviaConfigError? error) Create(IReadOnlyList<string> playerNames, IEnumerable<string> tematicaIds)
        {
            var names = (playerNames ?? Array.Empty<string>()).Select(n => n?.Trim() ?? string.Empty).ToList();
            if (names.Count < MinPlayers) return (null, TriviaConfigError.PocosJugadores);
            if (names.Count > MaxPlayers) return (null, TriviaConfigError.DemasiadosJugadores);
            if (names.Any(n => n.Length == 0)) return (null, TriviaConfigError.NombreVacio);
            if (names.Select(n => n.ToLowerInvariant()).Distinct().Count() != names.Count) return (null, TriviaConfigError.NombresDuplicados);
            var themes = new HashSet<string>(tematicaIds ?? Array.Empty<string>());
            if (themes.Count == 0) return (null, TriviaConfigError.SinTematicas);
            return (new TriviaConfig(names.AsReadOnly(), themes), null);
        }
    }

    /// <summary>
    /// Partida de trivia (port de <c>TriviaSession</c>, inmutable): 9 rondas en tramos de 3 (fácil, difícil, muy
    /// difícil); fallar elimina. Termina tras la ronda 9 o si no queda nadie; ganan los que siguen vivos (empate).
    /// </summary>
    public sealed class TriviaSession
    {
        public const int RoundCount = 9;
        public const int RoundsPerTier = 3;

        private readonly HashSet<TriviaPlayer> _eliminated;

        private TriviaSession(IReadOnlyList<TriviaPlayer> ordered, HashSet<TriviaPlayer> eliminated, int currentRound)
        {
            OrderedPlayers = ordered;
            _eliminated = eliminated;
            CurrentRound = currentRound;
        }

        public static TriviaSession Start(IReadOnlyList<TriviaPlayer> players)
        {
            if (players == null || players.Count == 0) throw new ArgumentException("La partida necesita al menos un jugador", nameof(players));
            return new TriviaSession(players.ToList().AsReadOnly(), new HashSet<TriviaPlayer>(), 0);
        }

        public IReadOnlyList<TriviaPlayer> OrderedPlayers { get; }
        public int CurrentRound { get; }
        public IReadOnlyCollection<TriviaPlayer> Eliminated => _eliminated;

        public IReadOnlyList<TriviaPlayer> AlivePlayers => OrderedPlayers.Where(p => !_eliminated.Contains(p)).ToList();

        public bool IsOver => CurrentRound >= RoundCount || AlivePlayers.Count == 0;

        public Difficulty? CurrentDifficulty => IsOver ? (Difficulty?)null : DifficultyForRound(CurrentRound);

        /// <summary>Supervivientes al terminar (vacío mientras se juega o si cayeron todos).</summary>
        public IReadOnlyList<TriviaPlayer> Winners => IsOver ? AlivePlayers : (IReadOnlyList<TriviaPlayer>)Array.Empty<TriviaPlayer>();

        /// <summary>Rondas 0–2 fácil, 3–5 difícil, 6–8 muy difícil. Lanza fuera de [0, 9).</summary>
        public static Difficulty DifficultyForRound(int round)
        {
            if (round < 0 || round >= RoundCount) throw new ArgumentOutOfRangeException(nameof(round), round, "Ronda fuera de [0, 9)");
            return Difficulties.All[round / RoundsPerTier];
        }

        public TriviaSession RecordAnswer(TriviaPlayer player, bool correct)
        {
            if (IsOver) throw new InvalidOperationException("La partida ya ha terminado");
            if (!AlivePlayers.Contains(player)) throw new ArgumentException($"{player?.Name} no sigue vivo", nameof(player));
            if (correct) return this;
            var eliminated = new HashSet<TriviaPlayer>(_eliminated) { player };
            return new TriviaSession(OrderedPlayers, eliminated, CurrentRound);
        }

        public TriviaSession AdvanceRound()
        {
            if (IsOver) throw new InvalidOperationException("La partida ya ha terminado");
            return new TriviaSession(OrderedPlayers, new HashSet<TriviaPlayer>(_eliminated), CurrentRound + 1);
        }
    }

    /// <summary>
    /// Reparte una pregunta DISTINTA a cada jugador vivo: baraja una copia del pool (Fisher-Yates) y da
    /// <c>shuffled[i]</c> al jugador <c>i</c>. Devuelve una lista ordenada (el orden importa para el turno).
    /// </summary>
    public sealed class DealQuestionsUseCase
    {
        private readonly IRandomProvider _random;

        public DealQuestionsUseCase(IRandomProvider random) => _random = random ?? throw new ArgumentNullException(nameof(random));

        public IReadOnlyList<(TriviaPlayer player, Question question)> Execute(IReadOnlyList<TriviaPlayer> alivePlayers, IReadOnlyList<Question> pool)
        {
            if (alivePlayers == null || alivePlayers.Count == 0) throw new ArgumentException("No hay jugadores vivos", nameof(alivePlayers));
            if (pool == null || pool.Count < alivePlayers.Count) throw new ArgumentException($"No hay preguntas suficientes: hacen falta {alivePlayers.Count}", nameof(pool));
            var shuffled = _random.Shuffled(pool);
            return alivePlayers.Select((p, i) => (p, shuffled[i])).ToList();
        }
    }
}
