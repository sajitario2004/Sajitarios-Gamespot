using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using UnityEngine;

namespace Gamespot.Games.Shared
{
    /// <summary>
    /// Panel "Categorías" de los setups del Impostor y del Tabú (V059i): un chip por categoría con palabras, en
    /// dos columnas. Siempre queda al menos una marcada. Trabaja sobre el conjunto del borrador del setup, así
    /// la elección se conserva al ir y volver. Con una categoría +18 marcada (Cachondeo) se ve un aviso fijo,
    /// como el del modo picante de Yo Nunca.
    /// </summary>
    public sealed class CategoryPicker : MonoBehaviour
    {
        private readonly Dictionary<string, NeonChip> _chips = new Dictionary<string, NeonChip>();
        private ISet<string> _selected;
        private GameObject _adultWarning;

        /// <summary>Se lanza al marcar o desmarcar.</summary>
        public event Action Changed;

        /// <summary>Las que se enseñan (las que tienen palabras), en orden.</summary>
        public IReadOnlyList<string> Shown { get; private set; } = Array.Empty<string>();

        /// <summary>Las marcadas entre las que se enseñan: lo que se pasa a la partida.</summary>
        public IReadOnlyCollection<string> Selected => Shown.Where(_selected.Contains).ToList();

        public bool IsSelected(string id) => _selected.Contains(id) && _chips.ContainsKey(id);

        /// <summary>Si se ve el aviso +18.</summary>
        public bool AdultWarningVisible => _adultWarning != null && _adultWarning.activeSelf;

        public static CategoryPicker Create(Transform parent, IReadOnlyList<string> shown, ISet<string> selected, Color accent)
        {
            var panel = Ui.Panel(parent, accent.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            var picker = panel.gameObject.AddComponent<CategoryPicker>();
            picker._selected = selected ?? throw new ArgumentNullException(nameof(selected));
            picker.Shown = shown ?? Array.Empty<string>();
            Ui.Text(panel.Content, Loc.T("categoriasTitulo"), TextRole.TitleMedium, accent, glow: 0.6f, bold: true);
            Ui.Text(panel.Content, Loc.T("categoriasAyuda"), TextRole.BodySmall, Palette.TextMuted);

            // Si no queda marcada ninguna de las que se ven (p. ej. solo "Mis palabras" y ya no hay), se marcan todas
            // menos las +18 (y, si solo hay +18, esas).
            if (!picker.Shown.Any(selected.Contains))
            {
                var normal = picker.Shown.Where(id => !WordCategories.IsAdult(id)).ToList();
                foreach (var id in normal.Count > 0 ? normal : picker.Shown) selected.Add(id);
            }
            var grid = ResponsiveChipGrid.Create(panel.Content);
            foreach (var id in picker.Shown)
            {
                var category = id;
                var chip = NeonChip.Create(grid, Loc.T(WordCategories.LabelKey(category)), selected.Contains(category), accent);
                chip.ToggleOnClick = false;
                chip.Clicked += () => picker.Toggle(category);
                chip.name = Loc.T(WordCategories.LabelKey(category));
                picker._chips[category] = chip;
            }

            var warning = Ui.Panel(panel.Content, Palette.NeonMagenta, glow: 0.8f, padding: new RectOffset(14, 14, 12, 12));
            var row = Ui.Row(warning.Content, 10f, alignment: TextAnchor.UpperLeft);
            Ui.Icon(row, IconId.Warning, 24f, Palette.NeonMagenta, 0.8f);
            Ui.Text(row, Loc.T("categoriaCachondeoAviso"), TextRole.BodySmall, Palette.TextPrimary).Sized(flexWidth: 1f);
            picker._adultWarning = warning.gameObject;
            picker.RefreshWarning();
            return picker;
        }

        /// <summary>Marca o desmarca una categoría; la última marcada no se puede quitar.</summary>
        public void Toggle(string id)
        {
            if (!_chips.ContainsKey(id)) return;
            if (_selected.Contains(id))
            {
                if (Selected.Count <= 1) return;
                _selected.Remove(id);
            }
            else
            {
                _selected.Add(id);
            }
            Refresh();
        }

        /// <summary>Deja marcada solo <paramref name="id"/> (atajo para los tests).</summary>
        public void SelectOnly(string id)
        {
            if (!_chips.ContainsKey(id)) return;
            foreach (var other in Shown) _selected.Remove(other);
            _selected.Add(id);
            Refresh();
        }

        private void Refresh()
        {
            foreach (var pair in _chips) pair.Value.Selected = _selected.Contains(pair.Key);
            RefreshWarning();
            Changed?.Invoke();
        }

        private void RefreshWarning() => _adultWarning.SetActive(Selected.Any(WordCategories.IsAdult));
    }
}
