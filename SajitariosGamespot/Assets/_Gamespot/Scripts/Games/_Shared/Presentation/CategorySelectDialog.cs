using System;
using System.Collections.Generic;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using UnityEngine;

namespace Gamespot.Games.Shared
{
    /// <summary>
    /// Diálogo para elegir una categoría (V059l): chips en dos columnas dentro de un scroll (con muchas categorías
    /// propias no cabrían) y, opcionalmente, "Todas" arriba y "Nueva categoría" abajo. Tocar un chip elige y cierra.
    /// </summary>
    public static class CategorySelectDialog
    {
        public const float RowHeight = 56f;
        public const float MaxListHeight = 340f;

        /// <param name="current">La elegida ahora (nulo = "Todas").</param>
        /// <param name="onPick">Recibe el id elegido (nulo = "Todas").</param>
        /// <param name="allLabel">Si no es nulo, añade la opción "Todas" con ese texto.</param>
        /// <param name="onCreate">
        /// Si no es nulo, enseña "Nueva categoría": lo llama con una función a la que pasar la categoría creada, que
        /// queda elegida.
        /// </param>
        public static GameObject Show(string title, IReadOnlyList<CategoryOption> options, string current, Action<string> onPick,
            string allLabel = null, Action<Action<CategoryOption>> onCreate = null)
        {
            GameObject dialog = null;
            void Pick(string id)
            {
                Dialogs.Close(dialog);
                onPick?.Invoke(id);
            }

            dialog = Dialogs.Show(title, null, new[]
            {
                new DialogAction(Loc.T("cancelar"), (Action)null, ButtonStyle.Text, Palette.NeonCyan)
            }, buildContent: content =>
            {
                var entries = new List<CategoryOption>();
                if (allLabel != null) entries.Add(new CategoryOption(null, allLabel));
                entries.AddRange(options);

                // Con margen dentro del scroll: si no, la máscara se come el borde de los chips de la derecha.
                var list = Ui.Scroll(content, 0f, new RectOffset(4, 4, 4, 4), "Categories");
                var rows = (entries.Count + 1) / 2;
                ((RectTransform)list.parent.parent).Sized(height: Mathf.Min(MaxListHeight, rows * RowHeight + 8f), flexHeight: 0f);
                var grid = ResponsiveChipGrid.Create(list);
                foreach (var entry in entries)
                {
                    var id = entry.Id;
                    var chip = NeonChip.Create(grid, entry.Label, id == current, Palette.NeonCyan);
                    chip.ToggleOnClick = false;
                    chip.Clicked += () => Pick(id);
                    chip.name = entry.Label;
                }

                if (onCreate != null)
                {
                    Ui.Button(content, Loc.T("nuevaCategoria"), () => onCreate(created => Pick(created.Id)), ButtonStyle.Outlined, IconId.Add);
                }
            });
            return dialog;
        }
    }
}
