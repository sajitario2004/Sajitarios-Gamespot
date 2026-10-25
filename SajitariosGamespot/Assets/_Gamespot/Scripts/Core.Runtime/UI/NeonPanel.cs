using Gamespot.Core.Runtime.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Panel neón (port de <c>NeonPanel</c>): relleno, borde de color y halo opcional, con radio 20 y
    /// padding 16 por defecto. Los hijos se añaden a <see cref="Content"/> y se apilan en columna.
    /// </summary>
    public sealed class NeonPanel : MonoBehaviour
    {
        /// <summary>Escala de opacidad del halo: en espacio lineal el brillo sale más fuerte que en Flutter.</summary>
        public const float GlowOpacity = 0.75f;

        private Image _glow;
        private Image _fill;
        private Image _border;
        private float _glowIntensity;
        private Color _glowColor;

        /// <summary>Contenedor de los hijos (una columna con el padding del panel).</summary>
        public RectTransform Content { get; private set; }

        public VerticalLayoutGroup Layout { get; private set; }

        /// <summary>Intensidad del halo (0 = sin halo). <see cref="PulseGlow"/> la anima.</summary>
        public float GlowIntensity
        {
            get => _glowIntensity;
            set
            {
                _glowIntensity = value;
                if (_glow == null) return;
                _glow.enabled = value > 0.01f;
                _glow.color = _glowColor.WithAlpha(Mathf.Clamp01(value) * GlowOpacity);
            }
        }

        public static NeonPanel Create(Transform parent, Color? border = null, Color? fill = null, float radius = 20f,
            float borderWidth = 1.5f, float glow = 0f, Color? glowColor = null, RectOffset padding = null, float spacing = 0f,
            TextAnchor alignment = TextAnchor.UpperLeft, string name = "NeonPanel")
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var panel = go.AddComponent<NeonPanel>();
            var borderColor = border ?? Palette.NeonCyan;
            panel._glowColor = glowColor ?? borderColor;

            panel._glow = Layer(go.transform, "Glow", NeonSprites.Glow(radius), panel._glowColor);
            var margin = NeonSprites.GlowMargin;
            panel._glow.rectTransform.offsetMin = new Vector2(-margin, -margin);
            panel._glow.rectTransform.offsetMax = new Vector2(margin, margin);

            panel._fill = Layer(go.transform, "Fill", NeonSprites.RoundedRect(radius), fill ?? Palette.Surface);
            panel._border = Layer(go.transform, "Border", NeonSprites.RoundedBorder(radius, borderWidth), borderColor);
            if (borderWidth <= 0f) panel._border.enabled = false;

            panel.Layout = go.AddComponent<VerticalLayoutGroup>();
            panel.Layout.padding = padding ?? new RectOffset(16, 16, 16, 16);
            panel.Layout.spacing = spacing;
            panel.Layout.childAlignment = alignment;
            panel.Layout.childControlWidth = true;
            panel.Layout.childControlHeight = true;
            panel.Layout.childForceExpandWidth = true;
            panel.Layout.childForceExpandHeight = false;

            panel.Content = (RectTransform)go.transform;
            panel.GlowIntensity = glow;
            return panel;
        }

        /// <summary>Cambia los colores (por ejemplo, al marcar una respuesta).</summary>
        public void SetColors(Color border, Color? fill = null, Color? glowColor = null)
        {
            _border.color = border;
            if (fill.HasValue) _fill.color = fill.Value;
            _glowColor = glowColor ?? border;
            GlowIntensity = _glowIntensity;
        }

        private static Image Layer(Transform parent, string name, Sprite sprite, Color color)
        {
            var rect = UiLayout.CreateStretched(name, parent);
            rect.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }
    }
}
