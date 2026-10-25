using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Localization;

namespace Gamespot.Games.Shared
{
    /// <summary>Una categoría tal como se enseña: id guardado en la BD y nombre ya traducido (o el que puso el usuario).</summary>
    public sealed class CategoryOption
    {
        public CategoryOption(string id, string label)
        {
            Id = id;
            Label = label;
        }

        public string Id { get; }
        public string Label { get; }

        /// <summary>
        /// Nombre de una categoría: las de serie salen de la localización; las propias, de su fila. Una propia que
        /// ya no existe se enseña como "Mis palabras", que es adonde van sus palabras al borrarla.
        /// </summary>
        public static string LabelOf(string id, IEnumerable<CustomCategory> custom)
        {
            if (!WordCategories.IsCustom(id)) return Loc.T(WordCategories.LabelKey(id));
            var match = custom?.FirstOrDefault(c => c.CategoryId == id);
            return match != null ? match.Name : Loc.T(WordCategories.LabelKey(WordCategories.Propias));
        }

        /// <summary>Las opciones de <paramref name="ids"/>, en ese orden.</summary>
        public static List<CategoryOption> For(IEnumerable<string> ids, IReadOnlyList<CustomCategory> custom) =>
            ids.Select(id => new CategoryOption(id, LabelOf(id, custom))).ToList();

        /// <summary>
        /// Las categorías a las que el usuario puede llevar sus palabras: "Mis palabras", las suyas y las de serie
        /// (también Cachondeo, +18).
        /// </summary>
        public static List<CategoryOption> Assignable(IReadOnlyList<CustomCategory> custom)
        {
            var ids = new List<string> { WordCategories.Propias };
            ids.AddRange(custom.Select(c => c.CategoryId));
            ids.AddRange(WordCategories.Seed);
            return For(ids, custom);
        }

        /// <summary>Todas las que pueden salir en un setup, en el orden de los chips: las de serie, "Mis palabras" y las propias.</summary>
        public static List<string> AllIds(IReadOnlyList<CustomCategory> custom) =>
            WordCategories.All.Concat(custom.Select(c => c.CategoryId)).ToList();

        /// <summary>Nombres de las categorías de serie en el idioma actual: una propia no puede llamarse igual.</summary>
        public static List<string> BuiltInNames() => WordCategories.All.Select(id => Loc.T(WordCategories.LabelKey(id))).ToList();
    }
}
