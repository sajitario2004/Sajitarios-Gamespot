using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Shared
{
    /// <summary>Ruta desconocida (el <c>errorBuilder</c> del router del Flutter), con "Volver al menú".</summary>
    public sealed class NotFoundScreen : AppScreen
    {
        private readonly string _path;

        public NotFoundScreen(string path) => _path = path;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("rutaNoEncontradaTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            var column = Ui.Column(scaffold.Body, 16f, new RectOffset(24, 24, 32, 24), TextAnchor.MiddleCenter);
            column.Sized(flexHeight: 1f);
            Ui.Icon(column, IconId.Error, 64f, Palette.NeonError, 0.8f);
            Ui.Text(column, Loc.T("rutaNoEncontradaMensaje"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
            Ui.Text(column, Loc.T("rutaNoEncontradaAyuda"), TextRole.BodySmall, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Text(column, _path ?? string.Empty, TextRole.BodySmall, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Button(column, Loc.T("volverAlMenu"), () => Router.Go(Router.Home));
        }
    }
}
