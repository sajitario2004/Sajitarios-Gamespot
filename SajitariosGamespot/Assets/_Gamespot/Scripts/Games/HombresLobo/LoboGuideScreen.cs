using System.Linq;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.HombresLobo.Domain;
using UnityEngine;

namespace Gamespot.Games.HombresLobo
{
    /// <summary>
    /// Guía para el game master (V081): qué hace cada carta de esta partida y, con la regla activada, los eventos de
    /// luna. Se abre desde la lista de jugadores.
    /// </summary>
    public sealed class LoboGuideScreen : AppScreen
    {
        private readonly HombresLoboModule _module;

        public LoboGuideScreen(HombresLoboModule module) => _module = module;

        public override void Build(RectTransform root)
        {
            var game = _module.Game;
            var scaffold = ScreenScaffold.Create(root, Loc.T("loboGuia"));
            scaffold.AppBar.SetLeading(IconId.Check, () => Router.Pop()).name = Loc.T("volver");
            var content = Ui.Scroll(scaffold.Body, 10f, new RectOffset(16, 16, 12, 24));
            if (game.Config == null) return;

            Ui.Text(content, Loc.T("loboGuiaCartas"), TextRole.LabelLarge, Palette.TextMuted);
            foreach (var def in LoboCatalog.All.Where(d => game.Config.Count(d.Rol) > 0))
            {
                var count = game.Config.Count(def.Rol);
                var name = count > 1 ? Loc.T("loboRolesVarios", ("n", count), ("rol", LoboUi.Name(def.Rol))) : LoboUi.Name(def.Rol);
                Entry(content, LoboUi.Icon(def.Rol), LoboUi.Color(def.Rol), name, LoboUi.Description(def.Rol));
            }
            if (!game.Rules.EventosLuna) return;
            Ui.Text(content, Loc.T("loboGuiaLunas"), TextRole.LabelLarge, Palette.TextMuted);
            foreach (var luna in LoboLunas.All)
            {
                Entry(content, IconId.NightsStay, Palette.NeonVioletText, Loc.T("loboLuna" + luna), Loc.T("loboLunaDesc" + luna));
            }
        }

        private static void Entry(Transform parent, IconId icon, Color color, string title, string text)
        {
            var panel = Ui.Panel(parent, color, glow: 0.3f, padding: new RectOffset(12, 12, 10, 10), spacing: 4f);
            panel.name = title;
            var row = Ui.Row(panel.Content, 10f, alignment: TextAnchor.MiddleLeft);
            Ui.Icon(row, icon, 26f, color, 0.5f);
            Ui.Text(row, title, TextRole.TitleSmall, color, bold: true, maxLines: 2).Sized(width: 0f, flexWidth: 1f);
            Ui.Text(panel.Content, text, TextRole.BodyMedium, Palette.TextPrimary);
        }
    }
}
