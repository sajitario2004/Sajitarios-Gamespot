using System;
using Gamespot.Core.Runtime.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Fila con título, subtítulo opcional y un interruptor cian (como <c>SwitchListTile</c>).
    /// Toda la fila es pulsable.
    /// </summary>
    public sealed class NeonSwitch : MonoBehaviour
    {
        private const float TrackWidth = 52f;
        private const float TrackHeight = 32f;
        private const float KnobSize = 24f;

        private Image _track;
        private Image _trackBorder;
        private RectTransform _knob;
        private Image _knobImage;
        private bool _value;

        public bool Value
        {
            get => _value;
            set
            {
                _value = value;
                Refresh();
            }
        }

        public event Action<bool> Changed;

        public static NeonSwitch Create(Transform parent, string title, string subtitle = null, bool value = false)
        {
            var row = Ui.Row(parent, 12f, new RectOffset(4, 4, 6, 6), TextAnchor.MiddleLeft, "Switch");
            row.MinHeight(56f);
            var view = row.gameObject.AddComponent<NeonSwitch>();

            var texts = Ui.Column(row, 2f, alignment: TextAnchor.MiddleLeft);
            texts.Sized(flexWidth: 1f);
            Ui.Text(texts, title, TextRole.TitleMedium, bold: true);
            if (!string.IsNullOrEmpty(subtitle)) Ui.Text(texts, subtitle, TextRole.BodySmall, Palette.TextMuted);

            var track = new GameObject("Track", typeof(RectTransform));
            track.transform.SetParent(row, false);
            ((RectTransform)track.transform).Sized(TrackWidth, TrackHeight);
            view._track = track.AddComponent<Image>();
            view._track.sprite = NeonSprites.RoundedRect(TrackHeight * 0.5f);
            view._track.type = Image.Type.Sliced;
            view._track.raycastTarget = false;

            var border = UiLayout.CreateStretched("Border", track.transform);
            view._trackBorder = border.gameObject.AddComponent<Image>();
            view._trackBorder.sprite = NeonSprites.RoundedBorder(TrackHeight * 0.5f, 2f);
            view._trackBorder.type = Image.Type.Sliced;
            view._trackBorder.raycastTarget = false;

            var knob = new GameObject("Knob", typeof(RectTransform));
            knob.transform.SetParent(track.transform, false);
            view._knob = (RectTransform)knob.transform;
            view._knob.sizeDelta = new Vector2(KnobSize, KnobSize);
            view._knob.anchorMin = view._knob.anchorMax = new Vector2(0f, 0.5f);
            view._knobImage = knob.AddComponent<Image>();
            view._knobImage.sprite = NeonSprites.Circle(KnobSize);
            view._knobImage.type = Image.Type.Sliced;
            view._knobImage.raycastTarget = false;

            // Fondo transparente para que toda la fila reciba el toque.
            var hit = row.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            var button = row.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() =>
            {
                view.Value = !view.Value;
                view.Changed?.Invoke(view.Value);
            });

            view.Value = value;
            return view;
        }

        private void Refresh()
        {
            if (_track == null) return;
            _track.color = _value ? Palette.NeonCyan.WithAlpha(0.45f) : Palette.SurfaceHigh;
            _trackBorder.color = _value ? Palette.NeonCyan : Palette.Outline;
            _knobImage.color = _value ? Palette.NeonCyan : Palette.TextMuted;
            var x = _value ? TrackWidth - KnobSize * 0.5f - 4f : KnobSize * 0.5f + 4f;
            _knob.anchoredPosition = new Vector2(x, 0f);
        }
    }
}
