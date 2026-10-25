using System;
using Gamespot.Core;

namespace Gamespot.Games.YoNunca.Domain
{
    /// <summary>
    /// Texto de una frase propia (V059q): recortado, con los espacios colapsados y empezando por el prefijo del juego
    /// («Yo nunca»). Si el usuario escribe solo el final («he ido a Ibiza») se le pone delante; si ya lo escribió (en
    /// mayúsculas o minúsculas), se deja con la grafía del prefijo.
    /// </summary>
    public static class NeverStatementText
    {
        public const int MaxLength = 140;

        /// <summary>La frase lista para guardar, o vacía si no hay nada después del prefijo.</summary>
        public static string Normalize(string text, string prefix)
        {
            var clean = NameNormalizer.Normalize(text);
            var p = NameNormalizer.Normalize(prefix);
            if (clean.Length == 0) return string.Empty;
            if (p.Length == 0) return Cut(clean);
            if (clean.StartsWith(p, StringComparison.OrdinalIgnoreCase))
            {
                var rest = clean.Substring(p.Length).TrimStart(' ', ',', '.', '…');
                return rest.Length == 0 ? string.Empty : Cut(p + " " + rest);
            }
            var lowered = char.ToLowerInvariant(clean[0]) + clean.Substring(1);
            return Cut(p + " " + lowered);
        }

        private static string Cut(string text) => text.Length > MaxLength ? text.Substring(0, MaxLength).TrimEnd() : text;
    }
}
