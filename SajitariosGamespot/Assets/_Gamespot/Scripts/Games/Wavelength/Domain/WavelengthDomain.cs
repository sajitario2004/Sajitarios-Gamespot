using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.Wavelength.Domain
{
    /// <summary>Par de conceptos opuestos (frío ↔ caliente). Recortados y no vacíos.</summary>
    public sealed class Spectrum : IEquatable<Spectrum>
    {
        public Spectrum(long id, string left, string right)
        {
            Left = left?.Trim() ?? string.Empty;
            Right = right?.Trim() ?? string.Empty;
            if (Left.Length == 0 || Right.Length == 0) throw new ArgumentException("Los dos conceptos son obligatorios");
            Id = id;
        }

        public long Id { get; }
        public string Left { get; }
        public string Right { get; }

        public bool Equals(Spectrum other) => other != null && other.Id == Id && other.Left == Left && other.Right == Right;
        public override bool Equals(object obj) => Equals(obj as Spectrum);
        public override int GetHashCode() => (Left.GetHashCode() * 397) ^ Right.GetHashCode();
        public override string ToString() => $"{Left} ↔ {Right}";
    }

    /// <summary>
    /// Puntos por cercanía sobre el eje 0..1 (bordes inclusivos, en <c>double</c>): d ≤ 0.06 → 4, d ≤ 0.12 → 3,
    /// d ≤ 0.20 → 2, si no 0. La respuesta se recorta a [0, 1]; el objetivo no.
    /// </summary>
    public static class WavelengthScoring
    {
        public const double BullseyeHalfWidth = 0.06;
        public const double NearHalfWidth = 0.12;
        public const double FarHalfWidth = 0.20;
        public const int PointsBullseye = 4;
        public const int PointsNear = 3;
        public const int PointsFar = 2;
        public const int PointsMiss = 0;

        /// <summary>Tolerancia para que los bordes sean inclusivos también en coma flotante (0.5 + 0.06 da 0.56000000000000005).</summary>
        private const double Epsilon = 1e-9;

        public static int ScoreFor(double guess, double target)
        {
            var clamped = Math.Max(0.0, Math.Min(1.0, guess));
            var distance = Math.Abs(clamped - target);
            if (distance <= BullseyeHalfWidth + Epsilon) return PointsBullseye;
            if (distance <= NearHalfWidth + Epsilon) return PointsNear;
            if (distance <= FarHalfWidth + Epsilon) return PointsFar;
            return PointsMiss;
        }
    }

    /// <summary>Una ronda: espectro, objetivo oculto en [0, 1], pista del psíquico y respuesta del grupo.</summary>
    public sealed class WavelengthRound
    {
        private WavelengthRound(Spectrum spectrum, double target, string clue, double? guess)
        {
            Spectrum = spectrum;
            Target = target;
            Clue = clue;
            Guess = guess;
        }

        public static WavelengthRound Start(Spectrum spectrum, double target)
        {
            if (spectrum == null) throw new ArgumentNullException(nameof(spectrum));
            if (target < 0.0 || target > 1.0) throw new ArgumentOutOfRangeException(nameof(target), target, "El objetivo tiene que estar en [0, 1]");
            return new WavelengthRound(spectrum, target, null, null);
        }

        public Spectrum Spectrum { get; }
        public double Target { get; }
        public string Clue { get; }
        public double? Guess { get; }
        public bool HasClue => Clue != null;
        public bool HasGuess => Guess.HasValue;

        /// <summary>Puntos de la ronda (nulo hasta que hay respuesta).</summary>
        public int? Score => Guess.HasValue ? WavelengthScoring.ScoreFor(Guess.Value, Target) : (int?)null;

        public WavelengthRound WithClue(string clue)
        {
            var trimmed = clue?.Trim() ?? string.Empty;
            if (trimmed.Length == 0) throw new ArgumentException("La pista no puede estar vacía", nameof(clue));
            return new WavelengthRound(Spectrum, Target, trimmed, Guess);
        }

        public WavelengthRound WithGuess(double guess) => new WavelengthRound(Spectrum, Target, Clue, Math.Max(0.0, Math.Min(1.0, guess)));
    }

    public enum WavelengthConfigError
    {
        PocosJugadores,
        DemasiadosJugadores,
        NombreVacio,
        NombresDuplicados,
        RondasFueraDeRango
    }

    /// <summary>Configuración: 2–8 jugadores (nombres recortados) y 1–20 rondas (8 por defecto).</summary>
    public sealed class WavelengthConfig
    {
        public const int MinPlayers = 2;
        public const int MaxPlayers = 8;
        public const int MinRounds = 1;
        public const int MaxRounds = 20;
        public const int DefaultRounds = 8;

        private WavelengthConfig(IReadOnlyList<string> names, int rounds)
        {
            PlayerNames = names;
            Rounds = rounds;
        }

        public IReadOnlyList<string> PlayerNames { get; }
        public int Rounds { get; }

        public static (WavelengthConfig config, WavelengthConfigError? error) Create(IReadOnlyList<string> playerNames, int rounds = DefaultRounds)
        {
            var names = (playerNames ?? Array.Empty<string>()).Select(n => n?.Trim() ?? string.Empty).ToList();
            if (names.Count < MinPlayers) return (null, WavelengthConfigError.PocosJugadores);
            if (names.Count > MaxPlayers) return (null, WavelengthConfigError.DemasiadosJugadores);
            if (names.Any(n => n.Length == 0)) return (null, WavelengthConfigError.NombreVacio);
            if (names.Select(n => n.ToLowerInvariant()).Distinct().Count() != names.Count) return (null, WavelengthConfigError.NombresDuplicados);
            if (rounds < MinRounds || rounds > MaxRounds) return (null, WavelengthConfigError.RondasFueraDeRango);
            return (new WavelengthConfig(names.AsReadOnly(), rounds), null);
        }
    }

    /// <summary>
    /// Partida cooperativa con un solo marcador: el psíquico rota (<c>jugadores[ronda % n]</c>) y cada ronda
    /// registrada suma sus puntos.
    /// </summary>
    public sealed class WavelengthSession
    {
        private WavelengthSession(IReadOnlyList<string> names, int totalRounds, IReadOnlyList<WavelengthRound> completed, int score)
        {
            PlayerNames = names;
            TotalRounds = totalRounds;
            CompletedRounds = completed;
            CumulativeScore = score;
        }

        public static WavelengthSession Start(IReadOnlyList<string> playerNames, int totalRounds)
        {
            if (playerNames == null || playerNames.Count == 0) throw new ArgumentException("Hace falta al menos un jugador", nameof(playerNames));
            if (totalRounds < 1) throw new ArgumentOutOfRangeException(nameof(totalRounds));
            return new WavelengthSession(playerNames.ToList().AsReadOnly(), totalRounds, Array.Empty<WavelengthRound>(), 0);
        }

        public IReadOnlyList<string> PlayerNames { get; }
        public int TotalRounds { get; }
        public IReadOnlyList<WavelengthRound> CompletedRounds { get; }
        public int CumulativeScore { get; }

        /// <summary>Índice de la ronda en curso (= rondas completadas).</summary>
        public int CurrentRoundIndex => CompletedRounds.Count;

        public bool IsOver => CompletedRounds.Count >= TotalRounds;

        public string CurrentPsychic => IsOver ? null : PlayerNames[CurrentRoundIndex % PlayerNames.Count];

        public WavelengthSession RecordRound(WavelengthRound round)
        {
            if (IsOver) throw new InvalidOperationException("La partida ya ha terminado");
            if (round == null || !round.HasGuess) throw new ArgumentException("La ronda no tiene respuesta", nameof(round));
            var completed = CompletedRounds.Concat(new[] { round }).ToList().AsReadOnly();
            return new WavelengthSession(PlayerNames, TotalRounds, completed, CumulativeScore + (round.Score ?? 0));
        }
    }

    /// <summary>Espectro al azar del pool (puede repetirse) y objetivo = <c>NextDouble()</c>.</summary>
    public sealed class PickRoundUseCase
    {
        private readonly IRandomProvider _random;

        public PickRoundUseCase(IRandomProvider random) => _random = random ?? throw new ArgumentNullException(nameof(random));

        public WavelengthRound Execute(IReadOnlyList<Spectrum> pool)
        {
            var spectrum = _random.Pick(pool);
            var target = _random.NextDouble();
            return WavelengthRound.Start(spectrum, target);
        }
    }
}
