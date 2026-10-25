using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.Tabu.Domain
{
    /// <summary>Por qué no vale una tarjeta que escribe el usuario.</summary>
    public enum TabuCardError
    {
        PalabraVacia,
        PocasProhibidas,
        ProhibidaRepetida
    }

    /// <summary>
    /// Reglas de una tarjeta propia del Tabú (V059l): palabra obligatoria y de 4 a 5 prohibidas (los huecos vacíos no
    /// cuentan), sin repetir ninguna ni repetir la propia palabra, sin distinguir mayúsculas. Las del seed ya las
    /// cumplen (lo comprueba <c>SeedFilesTests</c>).
    /// </summary>
    public static class TabuCardRules
    {
        /// <summary>
        /// Limpia (recorta y colapsa espacios) y valida. Devuelve el primer error, o nulo con la tarjeta limpia en
        /// <paramref name="palabraLimpia"/> y <paramref name="prohibidasLimpias"/>.
        /// </summary>
        public static TabuCardError? Validate(string palabra, IEnumerable<string> prohibidas, out string palabraLimpia,
            out List<string> prohibidasLimpias)
        {
            palabraLimpia = NameNormalizer.Normalize(palabra);
            prohibidasLimpias = (prohibidas ?? Array.Empty<string>()).Select(NameNormalizer.Normalize).Where(p => p.Length > 0).ToList();
            if (palabraLimpia.Length == 0) return TabuCardError.PalabraVacia;
            if (prohibidasLimpias.Count < TabuWord.MinForbidden || prohibidasLimpias.Count > TabuWord.MaxForbidden) return TabuCardError.PocasProhibidas;
            var keys = new HashSet<string> { NameNormalizer.DuplicateKey(palabraLimpia) };
            foreach (var p in prohibidasLimpias)
            {
                if (!keys.Add(NameNormalizer.DuplicateKey(p))) return TabuCardError.ProhibidaRepetida;
            }
            return null;
        }
    }
}
