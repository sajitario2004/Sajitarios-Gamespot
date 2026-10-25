using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Impostor.Domain;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Impostor
{
    /// <summary>
    /// Revelar el rol (port de <c>RevealScreen</c>): el rol está oculto hasta pulsar "Revelar" (suena
    /// <see cref="AppSound.Reveal"/>). Impostor: panel magenta que late, "IMPOSTOR" y, con pista, la pista en violeta.
    /// Palabra: panel cian que late con "Tu palabra es" y la palabra. "Ocultar y pasar" va al siguiente jugador o,
    /// tras el último, a la votación.
    /// </summary>
    public sealed class RevealScreen : ImpostorPlayScreen
    {
        private bool _revealed;
        private RectTransform _cardSlot;
        private NeonButton _revealButton;
        private NeonButton _nextButton;

        public RevealScreen(ImpostorModule module) : base(module)
        {
        }

        public bool Revealed => _revealed;

        /// <summary>Texto que se lee en la tarjeta revelada (para los tests y el lector de pantalla).</summary>
        public string Announcement { get; private set; }

        protected override void BuildGame(ScreenScaffold scaffold)
        {
            var flow = Module.Flow;
            var content = CenteredBody(scaffold.Body, 18f);
            Ui.Text(content, Loc.T("esElTurnoDe"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Text(content, flow.JugadorActual?.Name ?? string.Empty, TextRole.HeadlineMedium, Palette.TextPrimary, TextAlignmentOptions.Center, 0.9f,
                Palette.NeonCyan, 2, true);

            _cardSlot = Ui.Column(content, 0f, name: "CardSlot");
            _revealButton = Ui.Button(content, Loc.T("revelar"), Reveal, ButtonStyle.Filled, IconId.Visibility, Palette.NeonMagenta, 0.9f);
            _nextButton = Ui.Button(content, Loc.T(flow.EsUltimoJugador ? "ocultarYVerResultados" : "ocultarYPasar"), HideAndPass,
                ButtonStyle.Outlined, IconId.ArrowForward);
            Refresh(false);
        }

        /// <summary>Pulsar "Revelar".</summary>
        public void Reveal()
        {
            if (_revealed) return;
            _revealed = true;
            Module.Audio?.Play(AppSound.Reveal);
            Refresh(true);
        }

        /// <summary>"Ocultar y pasar" / "Ocultar y ver resultados".</summary>
        public void HideAndPass()
        {
            var votingStarts = Module.Flow.Avanzar();
            Router.Go(votingStarts ? ImpostorModule.VotingRoute : ImpostorModule.PassRoute);
        }

        private void Refresh(bool animate)
        {
            _cardSlot.ClearChildren();
            _revealButton.gameObject.SetActive(!_revealed);
            _nextButton.gameObject.SetActive(_revealed);
            var panel = _revealed ? BuildRole() : BuildHidden();
            if (animate) FadeScaleIn.Play(panel.gameObject);
        }

        private NeonPanel BuildHidden()
        {
            Announcement = null;
            var panel = Ui.Panel(_cardSlot, Palette.NeonCyan.WithAlpha(0.6f), glow: 0.4f, padding: new RectOffset(24, 24, 32, 32), spacing: 16f,
                alignment: TextAnchor.UpperCenter);
            Ui.Icon(panel.Content, IconId.Help, 72f, Palette.NeonCyan, 0.8f);
            Ui.Text(panel.Content, Loc.T("pulsaRevelar"), TextRole.BodyLarge, Palette.TextMuted, TextAlignmentOptions.Center);
            return panel;
        }

        private NeonPanel BuildRole()
        {
            var flow = Module.Flow;
            var player = flow.JugadorActual;
            var session = flow.Session;
            if (session.IsImpostor(player))
            {
                var hintEnabled = flow.Config?.HintEnabled ?? false;
                Announcement = hintEnabled ? Loc.T("tuRolEsImpostorConPista", ("pista", session.Hint)) : Loc.T("tuRolEsImpostor");
                var panel = Ui.Panel(_cardSlot, Palette.NeonMagenta, glow: 1f, padding: new RectOffset(24, 24, 32, 32), spacing: 14f,
                    alignment: TextAnchor.UpperCenter);
                PulseGlow.On(panel, 0.85f, 1.4f);
                Ui.Icon(panel.Content, IconId.TheaterComedy, 56f, Palette.NeonMagenta, 1f);
                var title = Ui.FittedText(panel.Content, Loc.T("impostorMayus"), 44f, Palette.NeonMagenta, 1.2f);
                title.characterSpacing = 12f;
                if (hintEnabled)
                {
                    Ui.Text(panel.Content, Loc.T("setupPista"), TextRole.LabelLarge, Palette.TextMuted, TextAlignmentOptions.Center);
                    Ui.Text(panel.Content, session.Hint, TextRole.HeadlineSmall, Palette.NeonVioletText, TextAlignmentOptions.Center, 1f,
                        Palette.NeonViolet, bold: true);
                }
                panel.name = Announcement;
                return panel;
            }

            Announcement = Loc.T("tuPalabraEsAnuncio", ("palabra", session.Word.Text));
            var wordPanel = Ui.Panel(_cardSlot, Palette.NeonCyan, glow: 1f, padding: new RectOffset(24, 24, 32, 32), spacing: 14f,
                alignment: TextAnchor.UpperCenter);
            PulseGlow.On(wordPanel, 0.8f, 1.2f);
            Ui.Text(wordPanel.Content, Loc.T("tuPalabraEs"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.FittedText(wordPanel.Content, session.Word.Text, 45f, Palette.TextPrimary, 1f, minSize: 18f, lines: 2).fontSharedMaterial =
                NeonTextMaterials.Get(TMP_Settings.defaultFontAsset, Palette.NeonCyan, 1f);
            wordPanel.name = Announcement;
            return wordPanel;
        }
    }
}
