using System;
using Gamespot.Core.Runtime.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Chip seleccionable (como <c>FilterChip</c>/<c>ChoiceChip</c>): marcado = relleno del color al 25 % y
    /// borde del color; sin marcar = borde del color al 40 %.
    /// </summary>
    public sealed class NeonChip : MonoBehaviour
    {
        private Image _fill;
        private Image _border;
        private TextMeshProUGUI _label;
        private Color _accent;
        private bool _selected;

        /// <summary>Si es falso, pulsar no cambia el estado solo (lo decide quien escucha <see cref="Clicked"/>).</summary>
        public bool ToggleOnClick { get; set; } = true;

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                Refresh();
            }
        }

        public event Action<bool> Changed;
        public event Action Clicked;

        public static NeonChip Create(Transform parent, string label, bool selected = false, Color? accent = null)
        {
            var go = new GameObject($"Chip {label}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var chip = go.AddComponent<NeonChip>();
            chip._accent = accent ?? Palette.NeonCyan;

            chip._fill = go.AddComponent<Image>();
            chip._fill.sprite = NeonSprites.RoundedRect(10f);
            chip._fill.type = Image.Type.Sliced;

            var border = UiLayout.CreateStretched("Border", go.transform);
            border.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            chip._border = border.gameObject.AddComponent<Image>();
            chip._border.sprite = NeonSprites.RoundedBorder(10f, 1.5f);
            chip._border.type = Image.Type.Sliced;
            chip._border.raycastTarget = false;

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 8);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Una línea que encoge si no cabe («Internet & social media» en una rejilla de dos columnas).
            chip._label = Ui.Text(go.transform, label, TextRole.LabelLarge, Palette.TextPrimary, TextAlignmentOptions.Center, maxLines: 1);

            var button = go.AddComponent<Button>();
            button.targetGraphic = chip._fill;
            button.onClick.AddListener(() =>
            {
                chip.Clicked?.Invoke();
                if (!chip.ToggleOnClick) return;
                chip.Selected = !chip.Selected;
                chip.Changed?.Invoke(chip.Selected);
            });
            // 48 de alto: el mínimo táctil (V062; antes 40).
            chip.MinHeight(NeonButton.MinTouchSize);
            chip.Selected = selected;
            return chip;
        }

        private void Refresh()
        {
            if (_fill == null) return;
            _fill.color = _selected ? _accent.WithAlpha(0.25f) : Palette.Surface.WithAlpha(0.6f);
            _border.color = _selected ? _accent : _accent.WithAlpha(0.4f);
            _label.color = _selected ? Palette.TextPrimary : Palette.TextMuted;
        }
    }
}
