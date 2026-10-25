using System;
using Gamespot.Core.Runtime.Theme;
using TMPro;
using UnityEngine;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Contador con botones − y + (impostores, rondas…). El texto central lo pone quien lo usa
    /// (normalmente un plural localizado) y una línea de ayuda opcional debajo.
    /// </summary>
    public sealed class NeonStepper : MonoBehaviour
    {
        private NeonButton _minus;
        private NeonButton _plus;
        private TextMeshProUGUI _label;
        private TextMeshProUGUI _helper;
        private Func<int, string> _format;

        public int Value { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }

        public event Action<int> Changed;

        public static NeonStepper Create(Transform parent, int value, int min, int max, Func<int, string> format, Color? accent = null)
        {
            var column = Ui.Column(parent, 4f, name: "Stepper");
            var stepper = column.gameObject.AddComponent<NeonStepper>();
            stepper._format = format;
            var color = accent ?? Palette.NeonCyan;

            var row = Ui.Row(column, 8f, alignment: TextAnchor.MiddleCenter);
            stepper._minus = NeonButton.CreateIcon(row, IconId.Remove, () => stepper.Set(stepper.Value - 1, true), color);
            stepper._label = Ui.Text(row, string.Empty, TextRole.TitleMedium, Palette.TextPrimary, TextAlignmentOptions.Center, bold: true);
            stepper._label.Sized(flexWidth: 1f);
            stepper._plus = NeonButton.CreateIcon(row, IconId.Add, () => stepper.Set(stepper.Value + 1, true), color);

            stepper._helper = Ui.Text(column, string.Empty, TextRole.BodySmall, Palette.TextMuted, TextAlignmentOptions.Center);
            stepper._helper.gameObject.SetActive(false);

            stepper.SetRange(min, max, value);
            return stepper;
        }

        /// <summary>Accesibilidad: etiquetas de los botones − y + (se leen como tooltip).</summary>
        public void SetButtonNames(string minus, string plus)
        {
            _minus.name = minus;
            _plus.name = plus;
        }

        /// <summary>Cambia el rango; el valor se recorta sin avisar.</summary>
        public void SetRange(int min, int max, int? value = null)
        {
            Min = min;
            Max = Math.Max(min, max);
            Set(value ?? Value, false);
        }

        public void SetHelper(string text)
        {
            _helper.text = text ?? string.Empty;
            _helper.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        public void Set(int value, bool notify)
        {
            var clamped = Math.Max(Min, Math.Min(Max, value));
            var changed = clamped != Value;
            Value = clamped;
            _label.text = _format != null ? _format(Value) : Value.ToString();
            _minus.Interactable = Value > Min;
            _plus.Interactable = Value < Max;
            if (notify && changed) Changed?.Invoke(Value);
        }
    }
}
