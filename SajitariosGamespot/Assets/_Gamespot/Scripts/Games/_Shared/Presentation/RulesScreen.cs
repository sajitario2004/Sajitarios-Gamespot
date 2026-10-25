using System.Collections.Generic;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Shared
{
    /// <summary>
    /// "¿Cómo se juega?" (port de <c>RulesScreen</c>): nombre del juego con halo cian y un panel con los pasos
    /// numerados. Se apila sobre el setup; atrás o la casa la quitan.
    /// </summary>
    public sealed class RulesScreen : AppScreen
    {
        private readonly string _gameTitleKey;
        private readonly IReadOnlyList<string> _stepKeys;

        public RulesScreen(string gameTitleKey, IReadOnlyList<string> stepKeys)
        {
            _gameTitleKey = gameTitleKey;
            _stepKeys = stepKeys;
        }

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("comoSeJuega"));
            scaffold.AppBar.SetLeading(IconId.Home, () => Router.Pop()).name = Loc.T("volverAlMenu");

            var content = Ui.Scroll(scaffold.Body, 20f, new RectOffset(16, 16, 16, 32));
            Ui.Text(content, Loc.T(_gameTitleKey), TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, 1f,
                Palette.NeonCyan);

            var panel = Ui.Panel(content, Palette.NeonCyan, glow: 0.6f, padding: new RectOffset(20, 20, 20, 20), spacing: 16f);
            for (var i = 0; i < _stepKeys.Count; i++)
            {
                var row = Ui.Row(panel.Content, 12f, alignment: TextAnchor.UpperLeft);
                Ui.NumberBadge(row, (i + 1).ToString(), Palette.NeonCyan, 28f, Palette.SurfaceHigh);
                var text = Ui.Text(row, Loc.T(_stepKeys[i]), TextRole.BodyMedium);
                text.lineSpacing = 12f;
                text.Sized(flexWidth: 1f);
            }
        }
    }
}
