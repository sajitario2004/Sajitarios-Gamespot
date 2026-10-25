using System;
using Gamespot.Core.Runtime.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Campo de texto del tema: relleno <c>surfaceHigh</c>, radio 14, borde cian al 30 % que pasa a cian
    /// de 2 unidades al enfocar. Etiqueta opcional encima y texto de ayuda dentro.
    /// </summary>
    public sealed class NeonTextField : MonoBehaviour
    {
        private Image _border;
        private Image _focusBorder;

        public TMP_InputField Input { get; private set; }
        public TextMeshProUGUI Label { get; private set; }

        public string Value
        {
            get => Input.text;
            set => Input.SetTextWithoutNotify(value ?? string.Empty);
        }

        /// <summary>Cambia el texto (cada tecla).</summary>
        public event Action<string> Changed;

        /// <summary>Pulsar "Hecho"/Intro en el teclado.</summary>
        public event Action<string> Submitted;

        public static NeonTextField Create(Transform parent, string label = null, string placeholder = null, string value = null,
            TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, int characterLimit = 0, bool capitalizeWords = false)
        {
            var root = Ui.Column(parent, 6f, name: "TextField");
            var field = root.gameObject.AddComponent<NeonTextField>();

            if (!string.IsNullOrEmpty(label))
            {
                field.Label = Ui.Text(root, label, TextRole.LabelLarge, Palette.TextMuted);
            }

            var box = new GameObject("Box", typeof(RectTransform));
            box.transform.SetParent(root, false);
            var boxRect = (RectTransform)box.transform;
            boxRect.Sized(height: 52f);
            var background = box.AddComponent<Image>();
            background.sprite = NeonSprites.RoundedRect(14f);
            background.type = Image.Type.Sliced;
            background.color = Palette.SurfaceHigh;

            field._border = Overlay(box.transform, "Border", NeonSprites.RoundedBorder(14f, 1.2f), Palette.NeonCyan.WithAlpha(0.3f));
            field._focusBorder = Overlay(box.transform, "FocusBorder", NeonSprites.RoundedBorder(14f, 2f), Palette.NeonCyan);
            field._focusBorder.enabled = false;

            var area = UiLayout.CreateStretched("TextArea", box.transform);
            area.offsetMin = new Vector2(16f, 6f);
            area.offsetMax = new Vector2(-16f, -6f);
            area.gameObject.AddComponent<RectMask2D>();

            var placeholderText = CreateText(area, placeholder ?? string.Empty, Palette.TextMuted.WithAlpha(0.7f));
            placeholderText.fontStyle = FontStyles.Italic;
            var text = CreateText(area, string.Empty, Palette.TextPrimary);

            var input = box.AddComponent<TMP_InputField>();
            input.textViewport = area;
            input.textComponent = text;
            input.placeholder = placeholderText;
            input.targetGraphic = background;
            input.fontAsset = TMP_Settings.defaultFontAsset;
            input.pointSize = 16f;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.contentType = contentType;
            input.characterLimit = characterLimit;
            input.caretColor = Palette.NeonCyan;
            input.customCaretColor = true;
            input.caretWidth = 2;
            input.selectionColor = Palette.NeonCyan.WithAlpha(0.3f);
            input.richText = false;
            input.onFocusSelectAll = false;
            if (capitalizeWords) input.characterValidation = TMP_InputField.CharacterValidation.None;
            input.text = value ?? string.Empty;

            input.onSelect.AddListener(_ => field.SetFocused(true));
            input.onDeselect.AddListener(_ => field.SetFocused(false));
            input.onValueChanged.AddListener(v => field.Changed?.Invoke(v));
            input.onSubmit.AddListener(v => field.Submitted?.Invoke(v));
            field.Input = input;
            return field;
        }

        public void Focus()
        {
            Input.Select();
            Input.ActivateInputField();
        }

        private void SetFocused(bool focused)
        {
            _border.enabled = !focused;
            _focusBorder.enabled = focused;
        }

        private static Image Overlay(Transform parent, string name, Sprite sprite, Color color)
        {
            var rect = UiLayout.CreateStretched(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string value, Color color)
        {
            var rect = UiLayout.CreateStretched("Text", parent);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = 16f;
            text.color = color;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Masking;
            text.raycastTarget = false;
            return text;
        }
    }
}
