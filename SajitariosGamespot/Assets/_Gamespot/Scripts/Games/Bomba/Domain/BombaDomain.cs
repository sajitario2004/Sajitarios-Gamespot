using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.Bomba.Domain
{
    /// <summary>Qué se muestra en pantalla: una sílaba (por defecto) o una categoría.</summary>
    public enum BombaMode
    {
        Silaba,
        Categoria
    }

    /// <summary>Sílaba o categoría a mostrar (texto recortado y no vacío). El id es único dentro de su modo.</summary>
    public sealed class BombaPrompt : IEquatable<BombaPrompt>
    {
        public BombaPrompt(long id, string texto, BombaMode mode)
        {
            Texto = texto?.Trim() ?? string.Empty;
            if (Texto.Length == 0) throw new ArgumentException("El texto no puede estar vacío", nameof(texto));
            Id = id;
            Mode = mode;
        }

        public long Id { get; }
        public string Texto { get; }
        public BombaMode Mode { get; }

        public bool Equals(BombaPrompt other) => other != null && other.Id == Id && other.Texto == Texto && other.Mode == Mode;
        public override bool Equals(object obj) => Equals(obj as BombaPrompt);
        public override int GetHashCode() => (Id.GetHashCode() * 397) ^ ((int)Mode * 31) ^ Texto.GetHashCode();
    }

    public enum BombaConfigError
    {
        PocosJugadores,
        DemasiadosJugadores,
        NombreVacio,
        NombresDuplicados,
        MinSegundosFueraDeLimite,
        MaxSegundosFueraDeLimite,
        RangoSegundosInvalido
    }

    /// <summary>
    /// Modo, 2–12 jugadores en orden de juego y rango de la mecha (dentro de [10, 60] y min &lt; max; el setup
    /// siempre pasa 10 y 60).
    /// </summary>
    public sealed class BombaConfig
    {
        public const int MinPlayers = 2;
        public const int MaxPlayers = 12;
        public const int AbsMinSeconds = 10;
        public const int AbsMaxSeconds = 60;

        private BombaConfig(BombaMode mode, IReadOnlyList<string> names, int min, int max)
        {
            Mode = mode;
            PlayerNames = names;
            MinSegundos = min;
            MaxSegundos = max;
        }

        public BombaMode Mode { get; }
        public IReadOnlyList<string> PlayerNames { get; }
        public int MinSegundos { get; }
        public int MaxSegundos { get; }

        public static (BombaConfig config, BombaConfigError? error) Create(BombaMode mode, IReadOnlyList<string> playerNames,
            int minSegundos = AbsMinSeconds, int maxSegundos = AbsMaxSeconds)
        {
            var names = (playerNames ?? Array.Empty<string>()).Select(n => n?.Trim() ?? string.Empty).ToList();
            if (names.Count < MinPlayers) return (null, BombaConfigError.PocosJugadores);
            if (names.Count > MaxPlayers) return (null, BombaConfigError.DemasiadosJugadores);
            if (names.Any(n => n.Length == 0)) return (null, BombaConfigError.NombreVacio);
            if (names.Select(n => n.ToLowerInvariant()).Distinct().Count() != names.Count) return (null, BombaConfigError.NombresDuplicados);
            if (minSegundos < AbsMinSeconds) return (null, BombaConfigError.MinSegundosFueraDeLimite);
            if (maxSegundos > AbsMaxSeconds) return (null, BombaConfigError.MaxSegundosFueraDeLimite);
            if (minSegundos >= maxSegundos) return (null, BombaConfigError.RangoSegundosInvalido);
            return (new BombaConfig(mode, names.AsReadOnly(), minSegundos, maxSegundos), null);
        }
    }

    /// <summary>
    /// Partida de La Bomba (port de <c>BombaSession</c>, inmutable). La bomba empieza en el primer jugador;
    /// "pasar" la da al siguiente vivo sin tocar la mecha; al explotar queda eliminado quien la tiene y pasa al
    /// siguiente. Gana el último en pie.
    /// </summary>
    public sealed class BombaSession
    {
        private BombaSession(IReadOnlyList<string> ordered, IReadOnlyList<string> alive, int holder, double fuse)
        {
            OrderedPlayers = ordered;
            AlivePlayers = alive;
            CurrentHolderIndex = holder;
            FuseSeconds = fuse;
        }

        /// <summary>Mecha oculta: <c>min + NextDouble() · (max − min)</c>, en [min, max).</summary>
        public static double PickFuseSeconds(IRandomProvider rng, BombaConfig config)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));
            if (config == null) throw new ArgumentNullException(nameof(config));
            return config.MinSegundos + rng.NextDouble() * (config.MaxSegundos - config.MinSegundos);
        }

        public static BombaSession Start(BombaConfig config, IRandomProvider rng)
        {
            var fuse = PickFuseSeconds(rng, config);
            return new BombaSession(config.PlayerNames, config.PlayerNames, 0, fuse);
        }

        /// <summary>Ronda nueva tras una explosión: mismos vivos, mismo portador y mecha nueva.</summary>
        public static BombaSession NewRound(BombaSession previous, BombaConfig config, IRandomProvider rng)
        {
            if (previous == null) throw new ArgumentNullException(nameof(previous));
            var fuse = PickFuseSeconds(rng, config);
            return new BombaSession(previous.OrderedPlayers, previous.AlivePlayers, previous.CurrentHolderIndex % previous.AlivePlayers.Count, fuse);
        }

        public IReadOnlyList<string> OrderedPlayers { get; }
        public IReadOnlyList<string> AlivePlayers { get; }
        public int CurrentHolderIndex { get; }
        public double FuseSeconds { get; }

        public string CurrentHolder => AlivePlayers[CurrentHolderIndex];
        public bool IsOver => AlivePlayers.Count <= 1;
        public string Winner => IsOver && AlivePlayers.Count > 0 ? AlivePlayers[0] : null;

        public BombaSession Pasar()
        {
            AssertNotOver();
            return new BombaSession(OrderedPlayers, AlivePlayers, (CurrentHolderIndex + 1) % AlivePlayers.Count, FuseSeconds);
        }

        /// <summary>Elimina al portador; el índice se queda igual, así que la bomba pasa al siguiente (o al primero).</summary>
        public BombaSession Explode()
        {
            AssertNotOver();
            var alive = AlivePlayers.ToList();
            alive.RemoveAt(CurrentHolderIndex);
            var index = alive.Count == 0 ? 0 : CurrentHolderIndex % alive.Count;
            return new BombaSession(OrderedPlayers, alive.AsReadOnly(), index, FuseSeconds);
        }

        private void AssertNotOver()
        {
            if (IsOver) throw new InvalidOperationException("La partida de La Bomba ya ha terminado");
        }
    }

    /// <summary>
    /// Elige sílaba o categoría al azar sin repetir dentro de cada modo hasta agotarlas; entonces vuelve a
    /// empezar. Cada modo lleva su propia cuenta.
    /// </summary>
    public sealed class PickPromptUseCase
    {
        private readonly IRandomProvider _random;
        private readonly Dictionary<BombaMode, HashSet<long>> _seen = new Dictionary<BombaMode, HashSet<long>>();

        public PickPromptUseCase(IRandomProvider random) => _random = random ?? throw new ArgumentNullException(nameof(random));

        public BombaPrompt Pick(BombaMode mode, IReadOnlyList<BombaPrompt> pool)
        {
            var candidates = (pool ?? Array.Empty<BombaPrompt>()).Where(p => p.Mode == mode).ToList();
            if (candidates.Count == 0) throw new ArgumentException($"No hay prompts para el modo {mode}", nameof(pool));
            if (!_seen.TryGetValue(mode, out var seen))
            {
                seen = new HashSet<long>();
                _seen[mode] = seen;
            }
            if (seen.Count >= candidates.Count) seen.Clear();
            var chosen = _random.Pick(candidates.Where(p => !seen.Contains(p.Id)).ToList());
            seen.Add(chosen.Id);
            return chosen;
        }

        public void ResetMode(BombaMode mode) => _seen.Remove(mode);

        public void ResetAll() => _seen.Clear();
    }
}
