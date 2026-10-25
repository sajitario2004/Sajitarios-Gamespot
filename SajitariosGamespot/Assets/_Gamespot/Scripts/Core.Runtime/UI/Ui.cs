using System;
using Gamespot.Core.Runtime.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Constructor de UI por código, al estilo de los widgets de Flutter: columnas, filas, espacios, scroll,
    /// textos neón, paneles, botones e imágenes. Todo cuelga de un <see cref="Transform"/> padre.
    /// </summary>
    public static class Ui
    {
        // ------------------------------------------------------------------ Contenedores

        /// <summary>Columna (VerticalLayoutGroup): los hijos ocupan el ancho y apilan su alto preferido.</summary>
        public static RectTransform Column(Transform parent, float spacing = 0f, RectOffset padding = null,
            TextAnchor alignment = TextAnchor.UpperCenter, bool stretchChildren = true, string name = "Column")
        {
            var rect = Create(name, parent);
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset();
            layout.childAlignment = alignment;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = stretchChildren;
            layout.childForceExpandHeight = false;
            return rect;
        }

        /// <summary>Fila (HorizontalLayoutGroup): los hijos toman su ancho preferido salvo los flexibles.</summary>
        public static RectTransform Row(Transform parent, float spacing = 0f, RectOffset padding = null,
            TextAnchor alignment = TextAnchor.MiddleLeft, string name = "Row")
        {
            var rect = Create(name, parent);
            var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset();
            layout.childAlignment = alignment;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return rect;
        }

        /// <summary>Hueco fijo (alto en una columna, ancho en una fila).</summary>
        public static RectTransform Space(Transform parent, float size)
        {
            var rect = Create("Space", parent);
            rect.Sized(size, size);
            return rect;
        }

        /// <summary>
        /// Hueco que se lleva el espacio sobrante (como <c>Spacer</c>) solo en el eje del padre: ancho en una fila,
        /// alto en una columna. (Si fuera flexible en los dos ejes, la fila entera se estiraría en vertical.)
        /// </summary>
        public static RectTransform Flex(Transform parent, float weight = 1f)
        {
            var rect = Create("Flex", parent);
            if (parent.GetComponent<HorizontalLayoutGroup>() != null) rect.Sized(flexWidth: weight);
            else rect.Sized(flexHeight: weight);
            return rect;
        }

        /// <summary>
        /// Zona con scroll vertical que ocupa el espacio sobrante del padre. Devuelve la columna de contenido.
        /// </summary>
        public static RectTransform Scroll(Transform parent, float spacing = 0f, RectOffset padding = null, string name = "Scroll")
        {
            var root = Create(name, parent);
            root.Sized(flexWidth: 1f, flexHeight: 1f);
            var scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.inertia = true;
            scroll.decelerationRate = 0.135f;
            scroll.scrollSensitivity = 30f;

            var viewport = UiLayout.CreateStretched("Viewport", root);
            viewport.gameObject.AddComponent<RectMask2D>();
            // Imagen invisible para que el arrastre funcione en todo el viewport.
            var hit = viewport.gameObject.AddComponent<Image>();
            hit.color = Color.clear;

            var content = Column(viewport, spacing, padding, TextAnchor.UpperCenter, true, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        // ------------------------------------------------------------------ Texto

        /// <summary>
        /// Texto con el estilo de <paramref name="role"/>. <paramref name="glow"/> &gt; 0 añade el halo neón
        /// (como <c>NeonText</c>). <paramref name="maxLines"/> &gt; 0 corta con puntos suspensivos. Con una sola
        /// línea no se parte por palabras (si no, «Andreu Buenafuente» se quedaba en «Andreu» sin avisar) y, si la
        /// fuente no puede pintar «…» (negrita sin variante bold), primero encoge hasta el 70 % y luego corta.
        /// </summary>
        public static TextMeshProUGUI Text(Transform parent, string text, TextRole role = TextRole.BodyMedium, Color? color = null,
            TextAlignmentOptions alignment = TextAlignmentOptions.Left, float glow = 0f, Color? glowColor = null, int maxLines = 0,
            bool bold = false, string name = "Text")
        {
            var rect = Create(name, parent);
            var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = Typography.Size(role);
            tmp.fontStyle = bold || Typography.IsBold(role) ? FontStyles.Bold : FontStyles.Normal;
            var c = color ?? Palette.TextPrimary;
            tmp.color = c;
            tmp.alignment = alignment;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            if (maxLines > 0)
            {
                tmp.maxVisibleLines = maxLines;
                Typography.SetEllipsis(tmp);
                if (maxLines == 1)
                {
                    tmp.textWrappingMode = TextWrappingModes.NoWrap;
                    if (tmp.overflowMode == TextOverflowModes.Truncate)
                    {
                        tmp.enableAutoSizing = true;
                        tmp.fontSizeMax = tmp.fontSize;
                        tmp.fontSizeMin = tmp.fontSize * 0.7f;
                    }
                }
            }
            if (glow > 0f) SetGlow(tmp, glowColor ?? c, glow);
            return tmp;
        }

        /// <summary>Cambia el halo de un texto (0 lo quita).</summary>
        public static void SetGlow(TextMeshProUGUI text, Color glowColor, float intensity)
        {
            text.fontSharedMaterial = NeonTextMaterials.Get(text.font != null ? text.font : TMP_Settings.defaultFontAsset, glowColor, intensity);
        }

        /// <summary>
        /// Texto que se encoge para caber (como <c>FittedBox(scaleDown)</c>). Con <paramref name="lines"/> &gt; 1 puede
        /// partirse en varias líneas antes de encoger: lo usan las palabras de juego, que ahora pueden ser nombres
        /// largos («José Luis Rodríguez Zapatero»).
        /// </summary>
        public static TextMeshProUGUI FittedText(Transform parent, string text, float maxSize, Color? color = null, float glow = 0f,
            bool bold = true, float minSize = 10f, int lines = 1)
        {
            var tmp = Text(parent, text, TextRole.DisplayMedium, color, TextAlignmentOptions.Center, glow, bold: bold, name: "FittedText");
            tmp.textWrappingMode = lines > 1 ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMax = maxSize;
            tmp.fontSizeMin = minSize;
            tmp.fontSize = maxSize;
            tmp.Sized(height: maxSize * 1.3f * Mathf.Max(1, lines));
            return tmp;
        }

        // ------------------------------------------------------------------ Piezas

        public static NeonPanel Panel(Transform parent, Color? border = null, Color? fill = null, float glow = 0f, RectOffset padding = null,
            float spacing = 0f, float radius = 20f, float borderWidth = 1.5f, TextAnchor alignment = TextAnchor.UpperLeft) =>
            NeonPanel.Create(parent, border, fill, radius, borderWidth, glow, null, padding, spacing, alignment);

        public static NeonButton Button(Transform parent, string label, Action onClick, ButtonStyle style = ButtonStyle.Filled,
            IconId? icon = null, Color? accent = null, float glow = 0f) =>
            NeonButton.Create(parent, label, onClick, style, icon, accent, glow);

        public static NeonButton IconButton(Transform parent, IconId icon, Action onClick, Color? color = null, float size = 24f) =>
            NeonButton.CreateIcon(parent, icon, onClick, color, size);

        public static IconView Icon(Transform parent, IconId icon, float size = 24f, Color? color = null, float glow = 0f) =>
            IconView.Create(parent, icon, size, color, glow);

        /// <summary>Imagen; con <paramref name="height"/> &gt; 0 fija el alto y conserva la proporción.</summary>
        public static Image Image(Transform parent, Sprite sprite, Color? color = null, float height = -1f, bool preserveAspect = true)
        {
            var rect = Create("Image", parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color ?? Color.white;
            image.preserveAspect = preserveAspect;
            image.raycastTarget = false;
            if (height > 0f) image.Sized(height: height);
            return image;
        }

        /// <summary>Línea divisoria de 1 unidad.</summary>
        public static Image Divider(Transform parent, Color? color = null)
        {
            var rect = Create("Divider", parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color ?? Palette.Divider;
            image.raycastTarget = false;
            image.Sized(height: 1f);
            return image;
        }

        /// <summary>Círculo de color con un texto centrado (avatares numerados, pasos de las reglas).</summary>
        public static RectTransform NumberBadge(Transform parent, string text, Color accent, float diameter = 32f, Color? fill = null)
        {
            var rect = Create("Badge", parent);
            rect.Sized(diameter, diameter);
            var circle = rect.gameObject.AddComponent<Image>();
            circle.sprite = NeonSprites.Circle(diameter);
            circle.type = UnityEngine.UI.Image.Type.Sliced;
            circle.color = fill ?? accent.WithAlpha(0.2f);
            circle.raycastTarget = false;
            var label = Text(rect, text, TextRole.LabelLarge, accent, TextAlignmentOptions.Center, bold: true);
            UiLayout.Stretch(label.rectTransform);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return rect;
        }

        private static RectTransform Create(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
    }
}
