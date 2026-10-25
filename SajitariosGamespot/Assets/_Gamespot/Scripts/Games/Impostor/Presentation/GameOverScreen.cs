using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Impostor.Domain;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Impostor
{
    /// <summary>
    /// Fin de partida (port de <c>GameOverScreen</c>): solo el desenlace, NUNCA los roles ni la palabra. Al abrirse
    /// suena <see cref="AppSound.GameOver"/> y se guarda la partida en el historial (una vez). "Jugar otra" vuelve al
    /// setup y "Volver al menú", al menú; los dos reinician la partida. Atrás vuelve al setup (en el Flutter no
    /// estaba interceptado y dejaba la partida a medias).
    /// </summary>
    public sealed class GameOverScreen : AppScreen
    {
        private readonly ImpostorModule _module;
        private bool _announced;

        public GameOverScreen(ImpostorModule module) => _module = module;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("finDePartidaTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router).Clicked += () => _module.Flow.Reiniciar();

            var content = Ui.Scroll(scaffold.Body, 20f, new RectOffset(24, 24, 40, 24));
            var flow = _module.Flow;
            if (flow.Session == null || flow.Phase != ImpostorPhase.GameOver)
            {
                Ui.Text(content, Loc.T("noHayPartidaQueMostrar"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                Ui.Button(content, Loc.T("configurarPartida"), () => Router.Go(ImpostorModule.SetupRoute));
                return;
            }

            var won = flow.Outcome == VotingOutcome.JugadoresGanan;
            var accent = won ? Palette.NeonCyan : Palette.NeonMagenta;
            var panel = Ui.Panel(content, accent, glow: 1f, padding: new RectOffset(24, 24, 32, 32), spacing: 18f, alignment: TextAnchor.UpperCenter);
            PulseGlow.On(panel, 0.8f, 1.3f);
            Ui.Icon(panel.Content, won ? IconId.Trophy : IconId.TheaterComedy, 72f, accent, 1f);
            Ui.Text(panel.Content, Loc.T(won ? "votacionJugadoresGanan" : "votacionImpostorSigue"), TextRole.HeadlineSmall, Palette.TextPrimary,
                TextAlignmentOptions.Center, 1f, accent, bold: true);

            Ui.Button(content, Loc.T("jugarOtra"), _module.ResetToSetup, ButtonStyle.Filled, IconId.Refresh, glow: 0.8f);
            Ui.Button(content, Loc.T("volverAlMenu"), _module.ResetToMenu, ButtonStyle.Outlined, IconId.Home);
        }

        public override void OnShow()
        {
            if (_announced || _module.Flow.Phase != ImpostorPhase.GameOver) return;
            _announced = true;
            _module.Audio?.Play(AppSound.GameOver);
            _module.SaveFinishedGameOnce();
        }

        public override bool OnBack()
        {
            _module.ResetToSetup();
            return true;
        }
    }
}
