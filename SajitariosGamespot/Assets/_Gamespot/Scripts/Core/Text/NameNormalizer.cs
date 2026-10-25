using System.Text.RegularExpressions;

namespace Gamespot.Core
{
    /// <summary>
    /// Normalización de nombres de jugadores y equipos (igual en todos los setups):
    /// se recorta y los espacios internos seguidos se dejan en uno ("Nacho   López" → "Nacho López").
    /// </summary>
    public static class NameNormalizer
    {
        private static readonly Regex Whitespace = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>Nombre recortado y con los espacios colapsados. Nulo → cadena vacía.</summary>
        public static string Normalize(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            return Whitespace.Replace(name.Trim(), " ");
        }

        /// <summary>Clave para detectar duplicados sin distinguir mayúsculas ("Ana" = "ana").</summary>
        public static string DuplicateKey(string name) => Normalize(name).ToLowerInvariant();
    }
}
