using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using UnityEngine;

namespace Gamespot.Games.Shared
{
    /// <summary>Panel de un setup con su título en color y, si hay, una ayuda debajo (el patrón de todos los setups).</summary>
    public static class SetupSection
    {
        public static NeonPanel Create(Transform parent, string title, string help, Color accent)
        {
            var panel = Ui.Panel(parent, accent.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(panel.Content, title, TextRole.TitleMedium, accent, glow: 0.6f, bold: true);
            if (!string.IsNullOrEmpty(help)) Ui.Text(panel.Content, help, TextRole.BodySmall, Palette.TextMuted);
            return panel;
        }

        /// <summary>Aviso fijo en magenta con el icono de aviso (+18, alcohol…).</summary>
        public static GameObject Warning(Transform parent, string text)
        {
            var warning = Ui.Panel(parent, Palette.NeonMagenta, glow: 0.8f, padding: new RectOffset(14, 14, 12, 12));
            var row = Ui.Row(warning.Content, 10f, alignment: TextAnchor.UpperLeft);
            Ui.Icon(row, IconId.Warning, 24f, Palette.NeonMagenta, 0.8f);
            Ui.Text(row, text, TextRole.BodySmall, Palette.TextPrimary).Sized(width: 0f, flexWidth: 1f);
            return warning.gameObject;
        }
    }

    /// <summary>
    /// Fila (o rejilla, con más de cuatro) de chips de opción única: segundos del turno, rondas, modos… Siempre hay
    /// una elegida. Trabaja con valores: el setup guarda el valor en su borrador desde <c>changed</c>.
    /// </summary>
    public sealed class ChoiceChips<T>
    {
        private readonly List<(T value, NeonChip chip)> _chips = new List<(T, NeonChip)>();
        private readonly Action<T> _changed;

        private ChoiceChips(T current, Action<T> changed)
        {
            Value = current;
            _changed = changed;
        }

        public T Value { get; private set; }

        public IReadOnlyList<T> Values => _chips.Select(c => c.value).ToList();

        public static ChoiceChips<T> Create(Transform parent, IReadOnlyList<(T value, string label)> options, T current, Color accent,
            Action<T> changed = null)
        {
            var choice = new ChoiceChips<T>(current, changed);
            var container = options.Count > 4 ? ResponsiveChipGrid.Create(parent, 3) : Ui.Row(parent, 8f, alignment: TextAnchor.MiddleCenter);
            foreach (var (value, label) in options)
            {
                var v = value;
                var chip = NeonChip.Create(container, label, EqualityComparer<T>.Default.Equals(v, current), accent);
                chip.ToggleOnClick = false;
                chip.Clicked += () => choice.Select(v);
                chip.name = label;
                if (options.Count <= 4) chip.Sized(flexWidth: 1f);
                choice._chips.Add((v, chip));
            }
            return choice;
        }

        /// <summary>Elige un valor (también lo usan los tests). Los que no están en la fila no hacen nada.</summary>
        public void Select(T value)
        {
            if (!_chips.Any(c => EqualityComparer<T>.Default.Equals(c.value, value))) return;
            Value = value;
            foreach (var (v, chip) in _chips) chip.Selected = EqualityComparer<T>.Default.Equals(v, value);
            _changed?.Invoke(value);
        }

        public NeonChip ChipOf(T value) => _chips.FirstOrDefault(c => EqualityComparer<T>.Default.Equals(c.value, value)).chip;
    }

    /// <summary>
    /// Panel «Nivel» de los juegos de cartas (V071): chips Suave y Picante +18 sobre el conjunto del borrador. Marcar
    /// el picante pide la confirmación de edad común (<see cref="AdultGate"/>) y, mientras está marcado, se ve un aviso
    /// fijo. Siempre queda al menos un nivel.
    /// </summary>
    public sealed class PromptLevelPicker : MonoBehaviour
    {
        private readonly Dictionary<PromptLevel, NeonChip> _chips = new Dictionary<PromptLevel, NeonChip>();
        private ISet<PromptLevel> _selected;
        private GameObject _warning;

        public IReadOnlyCollection<PromptLevel> Selected => PromptLevels.All.Where(_selected.Contains).ToList();

        public bool WarningVisible => _warning != null && _warning.activeSelf;

        public NeonChip ChipOf(PromptLevel level) => _chips[level];

        public static PromptLevelPicker Create(Transform parent, ISet<PromptLevel> selected, string helpKey = "nivelAyuda",
            string warningKey = "nivelPicanteAviso")
        {
            var panel = SetupSection.Create(parent, Loc.T("nivelTitulo"), Loc.T(helpKey), Palette.NeonCyan);
            var picker = panel.gameObject.AddComponent<PromptLevelPicker>();
            picker._selected = selected ?? throw new ArgumentNullException(nameof(selected));
            if (selected.Count == 0) selected.Add(PromptLevel.Suave);
            var row = Ui.Row(panel.Content, 10f, alignment: TextAnchor.MiddleCenter);
            foreach (var level in PromptLevels.All)
            {
                var l = level;
                var spicy = l == PromptLevel.Picante;
                var chip = NeonChip.Create(row, Loc.T(spicy ? "nivelPicante" : "nivelSuave"), selected.Contains(l),
                    spicy ? Palette.NeonMagenta : Palette.NeonCyan);
                chip.Sized(flexWidth: 1f);
                chip.ToggleOnClick = false;
                chip.Clicked += () => picker.Request(l, !picker._selected.Contains(l));
                picker._chips[l] = chip;
            }
            picker._warning = SetupSection.Warning(panel.Content, Loc.T(warningKey));
            picker.Refresh();
            return picker;
        }

        /// <summary>Lo que hace un chip: el picante, la primera vez en la sesión, pregunta antes si todos son mayores.</summary>
        public void Request(PromptLevel level, bool selected)
        {
            if (selected && level == PromptLevel.Picante) AdultGate.Request(() => Set(level, true));
            else Set(level, selected);
        }

        /// <summary>Marca o desmarca sin preguntar (también lo usan los tests). El último nivel no se puede quitar.</summary>
        public void Set(PromptLevel level, bool selected)
        {
            if (selected) _selected.Add(level);
            else if (_selected.Count > 1) _selected.Remove(level);
            Refresh();
        }

        private void Refresh()
        {
            foreach (var pair in _chips) pair.Value.Selected = _selected.Contains(pair.Key);
            _warning.SetActive(_selected.Contains(PromptLevel.Picante));
        }
    }

    /// <summary>
    /// De dónde salen las cartas de la partida (V071, como en Yo Nunca): de serie y/o mías. Solo se enseña si hay
    /// alguna propia; sin propias, salen las de serie. Siempre queda al menos uno.
    /// </summary>
    public sealed class PromptOriginPicker
    {
        private NeonChip _seed;
        private NeonChip _own;

        public bool IncludeSeed { get; private set; } = true;
        public bool IncludeOwn { get; private set; } = true;

        /// <summary>Si se enseña (hay cartas propias).</summary>
        public bool Visible => _own != null;

        /// <param name="ownCount">Cuántas propias hay (con 0 no se pinta nada).</param>
        public static PromptOriginPicker Create(Transform parent, long ownCount, bool seed, bool own, Action<bool, bool> changed)
        {
            var picker = new PromptOriginPicker { IncludeSeed = seed || !own, IncludeOwn = own };
            if (ownCount <= 0)
            {
                picker.IncludeSeed = true;
                picker.IncludeOwn = false;
                return picker;
            }
            var panel = SetupSection.Create(parent, Loc.T("origenTitulo"), Loc.T("origenAyuda"), Palette.NeonViolet);
            var row = Ui.Row(panel.Content, 10f, alignment: TextAnchor.MiddleCenter);
            picker._seed = NeonChip.Create(row, Loc.T("origenSerie"), picker.IncludeSeed, Palette.NeonViolet);
            picker._seed.Sized(flexWidth: 1f);
            picker._seed.ToggleOnClick = false;
            picker._own = NeonChip.Create(row, Loc.T("origenPropias", ("n", ownCount)), picker.IncludeOwn, Palette.NeonViolet);
            picker._own.Sized(flexWidth: 1f);
            picker._own.ToggleOnClick = false;
            picker._seed.Clicked += () =>
            {
                picker.Set(!picker.IncludeSeed, picker.IncludeOwn);
                changed?.Invoke(picker.IncludeSeed, picker.IncludeOwn);
            };
            picker._own.Clicked += () =>
            {
                picker.Set(picker.IncludeSeed, !picker.IncludeOwn);
                changed?.Invoke(picker.IncludeSeed, picker.IncludeOwn);
            };
            return picker;
        }

        public void Set(bool seed, bool own)
        {
            if (!seed && !own) return;
            if (_own == null) return;
            IncludeSeed = seed;
            IncludeOwn = own;
            _seed.Selected = seed;
            _own.Selected = own;
        }
    }
}
