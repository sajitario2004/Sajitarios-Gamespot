using System;
using System.Collections.Generic;
using Gamespot.Core;

namespace Gamespot.Games.Shared
{
    /// <summary>
    /// Cómo es la gestión de cartas de un juego (V070): qué campos tiene cada carta (segundo texto, tipo, nivel,
    /// categoría) y con qué textos se enseñan. La usa <see cref="PromptCardsScreen"/>.
    /// </summary>
    public sealed class PromptCardsConfig
    {
        public string TitleKey { get; set; } = "cartasTitulo";
        public string SearchHintKey { get; set; } = "buscarCarta";
        public string TextLabelKey { get; set; } = "campoCarta";
        public string TextHintKey { get; set; }

        /// <summary>Si no es nulo, cada carta tiene segunda opción (¿Qué prefieres?) y es obligatoria.</summary>
        public string TextBLabelKey { get; set; }
        public string TextBHintKey { get; set; }

        /// <summary>Tipos de carta (id guardado, clave del nombre), p. ej. verdad y reto. Vacío = sin tipo.</summary>
        public IReadOnlyList<(string id, string labelKey)> Kinds { get; set; } = Array.Empty<(string, string)>();

        /// <summary>Si las cartas tienen nivel suave / +18.</summary>
        public bool HasLevels { get; set; } = true;

        /// <summary>
        /// Categorías de serie (ids; las +18 según <see cref="WordCategories.IsAdult"/>). Nulo = sin categorías. Las
        /// propias del usuario se añaden con <see cref="CustomCategories"/>.
        /// </summary>
        public IReadOnlyList<string> SeedCategories { get; set; }
        public CustomCategoryRepository CustomCategories { get; set; }

        /// <summary>Arreglo del texto antes de guardar (p. ej. el prefijo de la frase). Nulo = solo recortar.</summary>
        public Func<string, string> Normalize { get; set; }

        public int MaxLength { get; set; } = 140;

        public bool HasTextB => TextBLabelKey != null;
        public bool HasKinds => Kinds != null && Kinds.Count > 0;
        public bool HasCategories => SeedCategories != null;
    }
}
