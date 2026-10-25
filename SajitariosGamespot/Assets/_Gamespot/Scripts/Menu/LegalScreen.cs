using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using TMPro;
using UnityEngine;

namespace Gamespot.Menu
{
    /// <summary>
    /// Un documento legal de los Ajustes (V059p): título en la barra y el texto, con sus apartados en negrita, dentro de
    /// un panel con scroll. Se apila sobre los Ajustes: atrás o la casa lo quitan.
    /// </summary>
    public sealed class LegalScreen : AppScreen
    {
        private readonly string _titleKey;
        private readonly string _bodyKey;

        public LegalScreen(string titleKey, string bodyKey)
        {
            _titleKey = titleKey;
            _bodyKey = bodyKey;
        }

        /// <summary>El texto que se enseña (para los tests).</summary>
        public TMP_Text Body { get; private set; }

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T(_titleKey));
            scaffold.AppBar.SetLeading(IconId.Home, () => Router.Pop()).name = Loc.T("volver");
            var content = Ui.Scroll(scaffold.Body, 0f, new RectOffset(16, 16, 12, 32));
            var panel = Ui.Panel(content, Palette.NeonMagenta.WithAlpha(0.4f), glow: 0.2f, padding: new RectOffset(20, 20, 20, 20));
            Body = Ui.Text(panel.Content, Loc.T(_bodyKey), TextRole.BodyMedium, Palette.TextPrimary);
            Body.lineSpacing = 10f;
            Body.paragraphSpacing = 12f;
        }
    }
}
