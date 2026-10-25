using System;
using Gamespot.Core.Runtime.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>Estilos de botón del tema del Flutter.</summary>
    public enum ButtonStyle
    {
        /// <summary>Relleno de color (por defecto cian) y texto oscuro. Botón principal.</summary>
        Filled,
        /// <summary>Borde de color y texto del mismo color.</summary>
        Outlined,
        /// <summary>Solo texto.</summary>
        Text
    }

    /// <summary>
    /// Botón neón: fondo redondeado (radio 16), etiqueta y/o icono, halo opcional. Deshabilitado baja al 38 %.
    /// Altura mínima 48 (objetivo táctil).
    /// </summary>
    public sealed class NeonButton : MonoBehaviour
    {
        public const float MinTouchSize = 48f;
        private const float DisabledAlpha = 0.38f;

        private CanvasGroup _group;
        private Image _background;
        private Image _border;
        private Image _glow;
        private ButtonStyle _style;
        private Color _accent;

        public Button Button { get; private set; }
        public TextMeshProUGUI Label { get; private set; }
        public IconView Icon { get; private set; }

        /// <summary>Se lanza al pulsar (solo si está habilitado).</summary>
        public event Action Clicked;

        public bool Interactable
        {
            get => Button.interactable;
            set
            {
                Button.interactable = value;
                _group.alpha = value ? 1f : DisabledAlpha;
                _group.blocksRaycasts = value;
            }
        }

        public static NeonButton Create(Transform parent, string label, Action onClick = null, ButtonStyle style = ButtonStyle.Filled,
            IconId? icon = null, Color? accent = null, float glow = 0f, float fontSize = 18f, float radius = 16f, string name = null,
            bool vertical = false)
        {
            var go = new GameObject(name ?? $"Button {label}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var button = go.AddComponent<NeonButton>();
            button._style = style;
            button._accent = accent ?? Palette.NeonCyan;
            button._group = go.AddComponent<CanvasGroup>();

            if (glow > 0f)
            {
                button._glow = Layer(go.transform, "Glow", NeonSprites.Glow(radius));
                var margin = NeonSprites.GlowMargin;
                button._glow.rectTransform.offsetMin = new Vector2(-margin, -margin);
                button._glow.rectTransform.offsetMax = new Vector2(margin, margin);
                button._glow.color = button._accent.WithAlpha(Mathf.Clamp01(glow) * NeonPanel.GlowOpacity);
            }

            button._background = Layer(go.transform, "Background", NeonSprites.RoundedRect(radius));
            button._border = Layer(go.transform, "Border", NeonSprites.RoundedBorder(radius, 1.5f));

            HorizontalOrVerticalLayoutGroup row = vertical ? go.AddComponent<VerticalLayoutGroup>() : (HorizontalOrVerticalLayoutGroup)go.AddComponent<HorizontalLayoutGroup>();
            row.padding = style == ButtonStyle.Text ? new RectOffset(12, 12, 8, 8) : new RectOffset(24, 24, 14, 14);
            row.spacing = 8f;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            if (icon.HasValue)
            {
                button.Icon = IconView.Create(go.transform, icon.Value, 22f);
            }

            if (!string.IsNullOrEmpty(label))
            {
                var labelGo = new GameObject("Label", typeof(RectTransform));
                labelGo.transform.SetParent(go.transform, false);
                var text = labelGo.AddComponent<TextMeshProUGUI>();
                text.text = label;
                text.fontSize = fontSize;
                text.fontStyle = FontStyles.Bold;
                text.alignment = TextAlignmentOptions.Center;
                text.textWrappingMode = TextWrappingModes.Normal;
                text.maxVisibleLines = 2;
                Typography.SetEllipsis(text);
                text.raycastTarget = false;
                button.Label = text;
            }

            button.Button = go.AddComponent<Button>();
            button.Button.targetGraphic = button._background;
            button.Button.transition = Selectable.Transition.ColorTint;
            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(1f, 0.75f, 0.95f, 1f);
            colors.disabledColor = Color.white;
            colors.fadeDuration = 0.08f;
            button.Button.colors = colors;
            button.Button.onClick.AddListener(() => button.Clicked?.Invoke());
            if (onClick != null) button.Clicked += onClick;

            button.ApplyStyle();
            button.MinHeight(MinTouchSize);
            return button;
        }

        /// <summary>Botón solo con icono (48 × 48), como los <c>IconButton</c> de la AppBar.</summary>
        public static NeonButton CreateIcon(Transform parent, IconId icon, Action onClick, Color? color = null, float iconSize = 24f)
        {
            var button = Create(parent, null, onClick, ButtonStyle.Text, icon, color, name: $"IconButton {icon}");
            var row = button.GetComponent<HorizontalOrVerticalLayoutGroup>();
            row.padding = new RectOffset(0, 0, 0, 0);
            button.Icon.Sized(iconSize, iconSize);
            button.Icon.Text.fontSize = iconSize;
            button.Sized(MinTouchSize, MinTouchSize);
            return button;
        }

        public void SetLabel(string label)
        {
            if (Label != null) Label.text = label;
        }

        /// <summary>
        /// Para botones estrechos en fila: menos margen y la etiqueta en una línea que se encoge (hasta 12) en vez
        /// de partirse a media palabra.
        /// </summary>
        public NeonButton Compact()
        {
            var layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 14, 14);
            layout.spacing = 4f;
            if (Label != null)
            {
                Label.textWrappingMode = TextWrappingModes.NoWrap;
                Label.enableAutoSizing = true;
                Label.fontSizeMax = Label.fontSize;
                Label.fontSizeMin = 12f;
            }
            return this;
        }

        /// <summary>Cambia el color de acento (relleno en Filled; borde y texto en los demás).</summary>
        public void SetAccent(Color accent)
        {
            _accent = accent;
            ApplyStyle();
        }

        private void ApplyStyle()
        {
            Color content;
            switch (_style)
            {
                case ButtonStyle.Filled:
                    _background.color = _accent;
                    _border.enabled = false;
                    content = Palette.OnPrimary;
                    break;
                case ButtonStyle.Outlined:
                    _background.color = Palette.Surface.WithAlpha(0.6f);
                    _border.enabled = true;
                    _border.color = _accent;
                    content = _accent;
                    break;
                default:
                    _background.color = Color.clear;
                    _border.enabled = false;
                    content = _accent;
                    break;
            }
            if (Label != null) Label.color = content;
            if (Icon != null) Icon.SetColor(content, _style == ButtonStyle.Filled ? 0f : 0.6f);
            if (_glow != null) _glow.color = _accent.WithAlpha(_glow.color.a);
        }

        private static Image Layer(Transform parent, string name, Sprite sprite)
        {
            var rect = UiLayout.CreateStretched(name, parent);
            rect.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.raycastTarget = name == "Background";
            return image;
        }
    }
}
