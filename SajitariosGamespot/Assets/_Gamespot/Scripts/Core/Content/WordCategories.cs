using System.Collections.Generic;

namespace Gamespot.Core
{
    /// <summary>
    /// Categorías de las palabras del Impostor y de las tarjetas del Tabú (V059i). El seed trae la categoría de
    /// cada palabra; las que crea el usuario en el Impostor son <see cref="Propias"/>. Los ids se guardan en la BD:
    /// no se cambian.
    /// </summary>
    public static class WordCategories
    {
        public const string General = "general";
        public const string Famosos = "famosos";
        public const string Politica = "politica";
        public const string Internet = "internet";
        public const string Tele = "tele";
        public const string Ficcion = "ficcion";
        public const string Juegos = "juegos";
        public const string Fiesta = "fiesta";
        public const string Espana = "espana";
        public const string Sitios = "sitios";
        public const string Propias = "propias";

        /// <summary>Las del seed, en el orden de los chips del setup.</summary>
        public static readonly IReadOnlyList<string> Seed = new[] { General, Famosos, Politica, Internet, Tele, Ficcion, Juegos, Fiesta, Espana, Sitios };

        /// <summary>Todas, con las propias al final.</summary>
        public static readonly IReadOnlyList<string> All = new[] { General, Famosos, Politica, Internet, Tele, Ficcion, Juegos, Fiesta, Espana, Sitios, Propias };

        /// <summary>Clave de localización del nombre: <c>categoriaGeneral</c>, <c>categoriaFamosos</c>…</summary>
        public static string LabelKey(string id) =>
            string.IsNullOrEmpty(id) ? "categoriaGeneral" : "categoria" + char.ToUpperInvariant(id[0]) + id.Substring(1);
    }
}
