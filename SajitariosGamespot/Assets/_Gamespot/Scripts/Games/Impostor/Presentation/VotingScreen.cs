using System.Collections.Generic;
using System.Linq;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Impostor.Domain;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Impostor
{
    /// <summary>
    /// Votación por rondas (port de <c>VotingScreen</c>): "Ronda X de Y", la lista de candidatos (cian) y, debajo,
    /// los impostores ya pillados (tachados). Tras cada voto avisa "El impostor sigue entre vosotros" (acierte o
    /// no, como el Flutter). Votar pide confirmación. Al acabar va al fin de partida.
    /// </summary>
    public sealed class VotingScreen : ImpostorPlayScreen
    {
        private RectTransform _list;
        private TMP_Text _roundText;
        private GameObject _feedback;

        public VotingScreen(ImpostorModule module) : base(module)
        {
        }

        protected override string TitleKey => "votacionTitulo";

        /// <summary>Botones de candidatos (para los tests).</summary>
        public List<NeonButton> CandidateButtons { get; } = new List<NeonButton>();

        protected override void BuildGame(ScreenScaffold scaffold)
        {
            var content = CenteredBody(scaffold.Body, 14f);
            _roundText = Ui.Text(content, string.Empty, TextRole.TitleLarge, Palette.NeonMagenta, TextAlignmentOptions.Center, 0.9f, bold: true);
            Ui.Text(content, Loc.T("votacionInstruccion"), TextRole.BodyLarge, Palette.TextMuted, TextAlignmentOptions.Center);
            var feedback = Ui.Panel(content, Palette.NeonMagenta.WithAlpha(0.7f), glow: 0.5f, padding: new RectOffset(16, 16, 12, 12),
                alignment: TextAnchor.MiddleCenter);
            Ui.Text(feedback.Content, Loc.T("votacionImpostorSigue"), TextRole.TitleMedium, Palette.NeonMagenta, TextAlignmentOptions.Center, 0.6f,
                bold: true);
            _feedback = feedback.gameObject;
            _list = Ui.Column(content, 10f, name: "Candidates");
            Refresh();
        }

        /// <summary>Expulsa a <paramref name="player"/> sin el diálogo (lo usa el diálogo y los tests).</summary>
        public void Vote(Player player)
        {
            Module.Flow.Votar(player);
            if (Module.Flow.Phase == ImpostorPhase.GameOver)
            {
                Router.Go(ImpostorModule.GameOverRoute);
                return;
            }
            Refresh();
        }

        private void Refresh()
        {
            var flow = Module.Flow;
            _roundText.text = Loc.T("votacionRondaXDeY", ("actual", flow.RondaActual), ("total", flow.RondasTotales));
            _feedback.SetActive(flow.LastVote != LastVote.Ninguno);
            _list.ClearChildren();
            CandidateButtons.Clear();

            foreach (var candidate in flow.Candidatos)
            {
                var player = candidate;
                var button = Ui.Button(_list, player.Name, () => Confirm(player), ButtonStyle.Outlined, IconId.Vote, Palette.NeonCyan, 0.4f);
                if (button.Label != null) button.Label.alignment = TextAlignmentOptions.Left;
                CandidateButtons.Add(button);
            }

            foreach (var caught in flow.Session.Impostores.Where(p => flow.Eliminados.Contains(p)))
            {
                var row = Ui.Panel(_list, Palette.Outline, Palette.Surface.WithAlpha(0.5f), padding: new RectOffset(16, 16, 12, 12));
                var line = Ui.Row(row.Content, 10f);
                Ui.Icon(line, IconId.Block, 22f, Palette.TextMuted);
                var name = Ui.Text(line, caught.Name, TextRole.TitleMedium, Palette.TextMuted);
                name.fontStyle |= FontStyles.Strikethrough;
            }
        }

        private void Confirm(Player player)
        {
            Dialogs.Confirm(Loc.T("votacionTitulo"), Loc.T("votacionConfirmarExpulsion", ("nombre", player.Name)), Loc.T("cancelar"),
                Loc.T("votacionExpulsar"), ok =>
                {
                    if (ok) Vote(player);
                }, Palette.NeonMagenta);
        }
    }
}
