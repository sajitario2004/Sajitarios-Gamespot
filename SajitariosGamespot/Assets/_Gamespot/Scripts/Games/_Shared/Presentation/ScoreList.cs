using System;
using System.Collections.Generic;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Shared
{
    /// <summary>Una fila de una clasificación: posición, nombre, detalle opcional y valor a la derecha.</summary>
    public sealed class ScoreRow
    {
        public ScoreRow(int position, string name, string value, string detail = null, Color? color = null)
        {
            Position = position;
            Name = name;
            Value = value;
            Detail = detail;
            Color = color ?? Palette.NeonCyan;
        }

        public int Position { get; }
        public string Name { get; }
        public string Value { get; }
        public string Detail { get; }
        public Color Color { get; }
    }

    /// <summary>
    /// Clasificación de los juegos nuevos (V071): un panel por fila con la posición en un círculo, el nombre (dos
    /// líneas que encogen), el detalle debajo y el valor a la derecha. El primero, con más brillo.
    /// </summary>
    public static class ScoreList
    {
        public static RectTransform Create(Transform parent, IReadOnlyList<ScoreRow> rows)
        {
            var column = Ui.Column(parent, 10f, name: "Scores");
            foreach (var row in rows) CreateRow(column, row);
            return column;
        }

        public static NeonPanel CreateRow(Transform parent, ScoreRow row)
        {
            var first = row.Position == 1;
            var panel = Ui.Panel(parent, row.Color, glow: first ? 0.9f : 0.4f, padding: new RectOffset(14, 16, 10, 10));
            var line = Ui.Row(panel.Content, 12f, alignment: TextAnchor.MiddleLeft);
            Ui.NumberBadge(line, row.Position.ToString(), row.Color, 36f);
            var texts = Ui.Column(line, 2f, alignment: TextAnchor.MiddleLeft);
            texts.Sized(width: 0f, flexWidth: 1f);
            var name = Ui.Text(texts, row.Name, TextRole.TitleMedium, Palette.TextPrimary, maxLines: 2, bold: true);
            name.enableAutoSizing = true;
            name.fontSizeMax = name.fontSize;
            name.fontSizeMin = 12f;
            if (!string.IsNullOrEmpty(row.Detail)) Ui.Text(texts, row.Detail, TextRole.BodySmall, Palette.TextMuted, maxLines: 2);
            if (!string.IsNullOrEmpty(row.Value))
            {
                Ui.Text(line, row.Value, TextRole.HeadlineSmall, row.Color, TextAlignmentOptions.Right, 0.8f, bold: true, maxLines: 1);
            }
            panel.name = Loc.T("puestoSemantica", ("puesto", row.Position), ("nombre", row.Name), ("valor", row.Value ?? string.Empty));
            return panel;
        }

        /// <summary>Puestos con empates: el mismo puesto a los que tienen la misma puntuación (1, 1, 3…).</summary>
        public static List<int> Positions<T>(IReadOnlyList<T> sorted, Func<T, int> score)
        {
            var result = new List<int>(sorted.Count);
            for (var i = 0; i < sorted.Count; i++)
            {
                result.Add(i > 0 && score(sorted[i]) == score(sorted[i - 1]) ? result[i - 1] : i + 1);
            }
            return result;
        }
    }
}
