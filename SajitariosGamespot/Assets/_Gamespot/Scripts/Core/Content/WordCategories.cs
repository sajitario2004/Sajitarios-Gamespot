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

        /// <summary>+18 (V059j): humor bruto, sexo y política. Va desmarcada por defecto y el setup avisa al marcarla.</summary>
        public const string Cachondeo = "cachondeo";

        public const string Propias = "propias";

        /// <summary>Las del seed, en el orden de los chips del setup.</summary>
        public static readonly IReadOnlyList<string> Seed = new[] { General, Famosos, Politica, Internet, Tele, Ficcion, Juegos, Fiesta, Espana, Sitios, Cachondeo };

        /// <summary>Todas, con las propias al final.</summary>
        public static readonly IReadOnlyList<string> All = new[] { General, Famosos, Politica, Internet, Tele, Ficcion, Juegos, Fiesta, Espana, Sitios, Cachondeo, Propias };

        /// <summary>Las que van marcadas al abrir el setup: todas menos las +18.</summary>
        public static readonly IReadOnlyList<string> Default = new[] { General, Famosos, Politica, Internet, Tele, Ficcion, Juegos, Fiesta, Espana, Sitios, Propias };

        /// <summary>Si la categoría es solo para mayores de edad (el setup avisa al marcarla).</summary>
        public static bool IsAdult(string id) => id == Cachondeo;

        /// <summary>Clave de localización del nombre: <c>categoriaGeneral</c>, <c>categoriaFamosos</c>…</summary>
        public static string LabelKey(string id) =>
            string.IsNullOrEmpty(id) ? "categoriaGeneral" : "categoria" + char.ToUpperInvariant(id[0]) + id.Substring(1);
    }
}
