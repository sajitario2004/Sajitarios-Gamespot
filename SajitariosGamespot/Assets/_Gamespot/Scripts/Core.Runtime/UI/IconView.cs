using Gamespot.Core.Runtime.Theme;
using TMPro;
using UnityEngine;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>Un icono como texto de TextMeshPro (fuente de iconos o sustituto), con halo opcional.</summary>
    public sealed class IconView : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private IconId _icon;
        private float _glow;
        private Color _color;

        public TextMeshProUGUI Text => _text;

        public static IconView Create(Transform parent, IconId icon, float size = 24f, Color? color = null, float glow = 0f)
        {
            var go = new GameObject($"Icon {icon}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<IconView>();
            view._text = go.AddComponent<TextMeshProUGUI>();
            view._text.alignment = TextAlignmentOptions.Center;
            view._text.textWrappingMode = TextWrappingModes.NoWrap;
            view._text.overflowMode = TextOverflowModes.Overflow;
            view._text.raycastTarget = false;
            view._text.fontSize = size;
            view._color = color ?? Palette.NeonCyan;
            view._glow = glow;
            view.Set(icon);
            view.Sized(size, size);
            return view;
        }

        public void Set(IconId icon)
        {
            _icon = icon;
            var (glyph, usesIconFont) = Icons.Glyph(icon);
            var font = usesIconFont ? Icons.IconFont : TMP_Settings.defaultFontAsset;
            _text.font = font;
            _text.fontStyle = usesIconFont ? FontStyles.Normal : FontStyles.Bold;
            _text.text = glyph;
            // El sustituto de texto es más ancho que un icono: se encoge para caber.
            _text.enableAutoSizing = !usesIconFont;
            _text.fontSizeMax = _text.fontSize;
            _text.fontSizeMin = 6f;
            SetColor(_color, _glow);
        }

        public void SetColor(Color color, float glow = -1f)
        {
            _color = color;
            if (glow >= 0f) _glow = glow;
            _text.color = color;
            _text.fontSharedMaterial = NeonTextMaterials.Get(_text.font, color, _glow);
        }

        public IconId Icon => _icon;
    }
}
