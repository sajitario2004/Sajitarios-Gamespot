using System;
using Gamespot.Core.Runtime.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Barra superior (port de la <c>AppBar</c> del tema): botón a la izquierda (normalmente "volver al menú"),
    /// título centrado de 22 w800 con halo cian y acciones a la derecha. Alto 56.
    /// </summary>
    public sealed class AppBar : MonoBehaviour
    {
        public const float Height = 56f;

        private float _actionsWidth;

        public TextMeshProUGUI Title { get; private set; }
        public TextMeshProUGUI Subtitle { get; private set; }
        public RectTransform Actions { get; private set; }
        public RectTransform Leading { get; private set; }

        public static AppBar Create(Transform parent, string title, string subtitle = null)
        {
            var root = Ui.Row(parent, 4f, new RectOffset(4, 4, 4, 4), TextAnchor.MiddleCenter, "AppBar");
            root.Sized(height: subtitle == null ? Height : Height + 16f);
            var bar = root.gameObject.AddComponent<AppBar>();

            bar.Leading = Ui.Row(root, 0f, alignment: TextAnchor.MiddleLeft, name: "Leading");
            bar.Leading.Sized(NeonButton.MinTouchSize, NeonButton.MinTouchSize);

            var center = Ui.Column(root, 0f, alignment: TextAnchor.MiddleCenter, name: "TitleBlock");
            center.Sized(flexWidth: 1f);
            bar.Title = Ui.Text(center, title, TextRole.TitleLarge, Palette.TextPrimary, TextAlignmentOptions.Center, glow: 0.8f,
                glowColor: Palette.NeonCyan, maxLines: 1);
            bar.Title.fontSize = 22f;
            bar.Title.characterSpacing = 3f;
            // Si no cabe, se encoge hasta 14 antes de cortarse.
            bar.Title.textWrappingMode = TextWrappingModes.NoWrap;
            bar.Title.enableAutoSizing = true;
            bar.Title.fontSizeMax = 22f;
            bar.Title.fontSizeMin = 14f;
            if (!string.IsNullOrEmpty(subtitle))
            {
                bar.Subtitle = Ui.Text(center, subtitle, TextRole.BodySmall, Palette.TextMuted, TextAlignmentOptions.Center, maxLines: 1);
            }

            bar.Actions = Ui.Row(root, 0f, alignment: TextAnchor.MiddleRight, name: "Actions");
            bar.Actions.Sized(NeonButton.MinTouchSize, NeonButton.MinTouchSize);
            return bar;
        }

        /// <summary>Pone el botón de la izquierda.</summary>
        public NeonButton SetLeading(IconId icon, Action onClick)
        {
            Leading.ClearChildren();
            return NeonButton.CreateIcon(Leading, icon, onClick);
        }

        /// <summary>Añade una acción a la derecha. La zona de acciones crece con cada una.</summary>
        public NeonButton AddAction(IconId icon, Action onClick, Color? color = null)
        {
            var button = NeonButton.CreateIcon(Actions, icon, onClick, color);
            GrowActions(NeonButton.MinTouchSize);
            return button;
        }

        /// <summary>
        /// Añade a la derecha una pastilla informativa que no se pulsa (icono + texto, como el <c>Chip</c> de
        /// jugadores vivos de La Bomba). Devuelve el texto para actualizarlo.
        /// </summary>
        public TextMeshProUGUI AddInfo(IconId icon, string text, Color color)
        {
            const float width = 64f;
            var chip = Ui.Panel(Actions, color.WithAlpha(0.4f), Palette.SurfaceHigh, padding: new RectOffset(8, 10, 4, 4), radius: 16f,
                borderWidth: 1f, alignment: TextAnchor.MiddleCenter);
            chip.Sized(width - 8f, 32f);
            var row = Ui.Row(chip.Content, 4f, alignment: TextAnchor.MiddleCenter);
            Ui.Icon(row, icon, 16f, color);
            var label = Ui.Text(row, text, TextRole.LabelMedium, color, maxLines: 1, bold: true);
            GrowActions(width);
            return label;
        }

        private void GrowActions(float added)
        {
            _actionsWidth += added;
            Actions.Sized(_actionsWidth, NeonButton.MinTouchSize);
            // Se equilibra la izquierda para centrar el título, pero sin pasar de dos botones: con más acciones el
            // título se corre un poco a la izquierda en vez de encogerse (como Flutter cuando no cabe centrado).
            Leading.Sized(Mathf.Clamp(_actionsWidth, NeonButton.MinTouchSize, NeonButton.MinTouchSize * 2f), NeonButton.MinTouchSize);
        }
    }
}
