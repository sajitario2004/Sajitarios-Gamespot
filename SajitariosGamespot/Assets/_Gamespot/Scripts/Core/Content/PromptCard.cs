using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamespot.Core
{
    /// <summary>Nivel de una carta: para todos o solo para mayores de 18 (V070).</summary>
    public enum PromptLevel
    {
        Suave,
        Picante
    }

    public static class PromptLevels
    {
        public static readonly IReadOnlyList<PromptLevel> All = new[] { PromptLevel.Suave, PromptLevel.Picante };

        public static string StorageName(this PromptLevel level) => level == PromptLevel.Picante ? "picante" : "suave";

        public static PromptLevel FromStorageName(string name)
        {
            switch (name)
            {
                case "suave":
                case "":
                case null:
                    return PromptLevel.Suave;
                case "picante": return PromptLevel.Picante;
                default: throw new FormatException($"Nivel desconocido: {name}");
            }
        }
    }

    /// <summary>
    /// Carta de los juegos de frases y palabras (V070): Verdad o Reto, ¿Quién es más probable que…?, ¿Qué prefieres?,
    /// Mímica y ¿Quién soy?. <see cref="TextB"/> es la segunda opción de ¿Qué prefieres?; <see cref="Kind"/>, el tipo
    /// (verdad / reto); <see cref="Category"/>, la categoría (vacía si el juego no tiene).
    /// </summary>
    public sealed class PromptCard
    {
        public PromptCard(long id, string text, PromptLevel level = PromptLevel.Suave, string kind = "", string category = "", string textB = null,
            bool isOwn = false)
        {
            Text = text?.Trim() ?? string.Empty;
            if (Text.Length == 0) throw new ArgumentException("La carta no puede estar vacía", nameof(text));
            Id = id;
            Level = level;
            Kind = kind?.Trim() ?? string.Empty;
            Category = category?.Trim() ?? string.Empty;
            TextB = string.IsNullOrWhiteSpace(textB) ? null : textB.Trim();
            IsOwn = isOwn;
        }

        public long Id { get; }
        public string Text { get; }
        public string TextB { get; }
        public PromptLevel Level { get; }
        public string Kind { get; }
        public string Category { get; }
        public bool IsOwn { get; }
    }

    /// <summary>Qué cartas quiere una partida. Nulo en una lista = sin filtrar por eso.</summary>
    public sealed class PromptQuery
    {
        public IReadOnlyCollection<PromptLevel> Levels { get; set; }
        public string Kind { get; set; }
        public IReadOnlyCollection<string> Categories { get; set; }
        public bool IncludeSeed { get; set; } = true;
        public bool IncludeOwn { get; set; } = true;

        public bool Matches(PromptCard card) =>
            (Levels == null || Levels.Contains(card.Level))
            && (string.IsNullOrEmpty(Kind) || card.Kind == Kind)
            && (Categories == null || Categories.Contains(card.Category))
            && (card.IsOwn ? IncludeOwn : IncludeSeed);
    }

    /// <summary>De dónde salen las cartas de una partida (el repositorio del juego, en la app).</summary>
    public interface IPromptSource
    {
        IReadOnlyList<PromptCard> GetCards(PromptQuery query);
    }

    /// <summary>
    /// Mazo sin repeticiones (V070): saca cartas al azar sin repetir hasta agotar las del montón que se le pasa; al
    /// agotarse vuelve a empezar, sin repetir la última si hay más de una. Una sola llamada a <c>NextInt</c> por carta.
    /// </summary>
    public sealed class NoRepeatDeck
    {
        private readonly HashSet<long> _seen = new HashSet<long>();
        private readonly IRandomProvider _random;
        private long? _last;

        public NoRepeatDeck(IRandomProvider random) => _random = random ?? throw new ArgumentNullException(nameof(random));

        /// <summary>Cuántas han salido desde el último reinicio.</summary>
        public int SeenCount => _seen.Count;

        /// <summary>Si en el último <see cref="Draw"/> se acabó el montón y se volvió a empezar.</summary>
        public bool Reshuffled { get; private set; }

        public PromptCard Draw(IReadOnlyList<PromptCard> pool)
        {
            if (pool == null || pool.Count == 0) throw new ArgumentException("No hay cartas", nameof(pool));
            var remaining = pool.Where(c => !_seen.Contains(c.Id)).ToList();
            Reshuffled = remaining.Count == 0;
            if (Reshuffled)
            {
                _seen.Clear();
                remaining = pool.Count > 1 ? pool.Where(c => c.Id != _last).ToList() : pool.ToList();
            }
            var card = remaining[_random.NextInt(remaining.Count)];
            _seen.Add(card.Id);
            _last = card.Id;
            return card;
        }

        public void Reset()
        {
            _seen.Clear();
            _last = null;
            Reshuffled = false;
        }
    }
}
