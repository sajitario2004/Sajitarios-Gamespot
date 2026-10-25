using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Impostor
{
    /// <summary>
    /// "Pásale el móvil a…" (port de <c>PassDeviceScreen</c>): posición, círculo cian que late con un móvil, el
    /// nombre en grande y "Continuar", que lleva a revelar.
    /// </summary>
    public sealed class PassDeviceScreen : ImpostorPlayScreen
    {
        public PassDeviceScreen(ImpostorModule module) : base(module)
        {
        }

        protected override void BuildGame(ScreenScaffold scaffold)
        {
            var flow = Module.Flow;
            var content = CenteredBody(scaffold.Body, 18f);
            Ui.Text(content, Loc.T("jugadorXDeY", ("posicion", flow.CurrentIndex + 1), ("total", flow.TotalPlayers)), TextRole.LabelLarge,
                Palette.TextMuted, TextAlignmentOptions.Center);

            var circleRow = Ui.Row(content, 0f, alignment: TextAnchor.MiddleCenter);
            // 96 en pantallas anchas (más de 600 dp, tabletas), 72 en móviles.
            var logicalWidth = Screen.dpi > 0 ? Screen.width / (Screen.dpi / 160f) : Screen.width;
            var size = logicalWidth > 600f ? 96f : 72f;
            var circle = Ui.Panel(circleRow, Palette.NeonCyan, Palette.Surface, 0.9f, new RectOffset(28, 28, 28, 28), radius: (size + 56f) / 2f,
                alignment: TextAnchor.MiddleCenter);
            circle.Sized(size + 56f, size + 56f);
            Ui.Icon(circle.Content, IconId.Phone, size, Palette.NeonCyan, 0.9f);
            PulseGlow.On(circle);

            Ui.Text(content, Loc.T("pasaleElMovilA"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Text(content, flow.JugadorActual?.Name ?? string.Empty, TextRole.DisplayMedium, Palette.TextPrimary, TextAlignmentOptions.Center, 1f,
                Palette.NeonCyan, 2, true);
            Ui.Text(content, Loc.T("pasaleElMovilAyuda"), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Space(content, 8f);
            Ui.Button(content, Loc.T("continuar"), Continue, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f).Sized(height: 64f);
        }

        /// <summary>"Continuar" (también lo usan los tests).</summary>
        public void Continue()
        {
            Module.Flow.Revelar();
            Router.Go(ImpostorModule.RevealRoute);
        }
    }
}
