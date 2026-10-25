using System.Collections.Generic;
using System.Linq;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.HombresLobo.Domain;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.HombresLobo
{
    /// <summary>
    /// Reparto (V078), como en el Impostor: «Pásale el móvil a…», el jugador pulsa para ver su carta (icono, nombre, lo
    /// que hace y, según el rol, sus compañeros lobos, sus hermanos o su grupo) y la oculta para pasar el móvil. Tras el
    /// último, «Dale el móvil al game master».
    /// </summary>
    public sealed class LoboRevealScreen : AppScreen
    {
        private readonly HombresLoboModule _module;

        public LoboRevealScreen(HombresLoboModule module) => _module = module;

        /// <summary>Si se está enseñando la carta (si no, se pide pasar el móvil).</summary>
        public bool Revealed { get; private set; }

        /// <summary>El reparto ha terminado y el móvil va al game master.</summary>
        public bool Handoff => _module.Game.Fase != LoboFase.Reparto;

        private LoboGame Game => _module.Game;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("loboTitulo"), Game.Fase == LoboFase.Reparto
                ? Loc.T("jugadorXDeY", ("posicion", Game.RepartoIndex + 1), ("total", Game.Jugadores.Count))
                : null);
            GameUi.HomeButton(scaffold.AppBar, Router, () => Game.Reiniciar());
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(24, 24, 24, 24));
            if (Game.Config == null)
            {
                Ui.Text(content, Loc.T("noHayPartidaEnCurso"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                Ui.Button(content, Loc.T("configurarPartida"), () => Router.Go(HombresLoboModule.SetupRoute));
                return;
            }
            if (Handoff) BuildHandoff(content);
            else if (Revealed) BuildCard(content, Game.Jugadores[Game.RepartoIndex]);
            else BuildPass(content, Game.Jugadores[Game.RepartoIndex]);
        }

        private void BuildPass(RectTransform content, LoboPlayer player)
        {
            Ui.Space(content, 24f);
            var circleRow = Ui.Row(content, 0f, alignment: TextAnchor.MiddleCenter);
            var circle = Ui.Panel(circleRow, Palette.NeonRed, Palette.Surface, 0.9f, new RectOffset(28, 28, 28, 28), radius: 64f,
                alignment: TextAnchor.MiddleCenter);
            circle.Sized(128f, 128f);
            Ui.Icon(circle.Content, IconId.Phone, 72f, Palette.NeonRed, 0.9f);
            PulseGlow.On(circle);
            Ui.Text(content, Loc.T("pasaleElMovilA"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Text(content, player.Name, TextRole.DisplayMedium, Palette.TextPrimary, TextAlignmentOptions.Center, 1f, Palette.NeonRed, 2, true);
            Ui.Text(content, Loc.T("loboQueNadieMire"), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Button(content, Loc.T("loboVerMiCarta"), Reveal, ButtonStyle.Filled, IconId.Visibility, Palette.NeonRed, 0.8f)
                .Sized(height: 64f);
        }

        private void BuildCard(RectTransform content, LoboPlayer player)
        {
            var color = LoboUi.Color(player.Rol);
            Ui.Text(content, player.Name, TextRole.TitleLarge, Palette.TextMuted, TextAlignmentOptions.Center, maxLines: 2, bold: true);
            var card = Ui.Panel(content, color, glow: 1f, padding: new RectOffset(24, 24, 28, 28), spacing: 12f, alignment: TextAnchor.UpperCenter);
            PulseGlow.On(card, 0.8f, 1.2f);
            Ui.Icon(card.Content, LoboUi.Icon(player.Rol), 88f, color, 1f);
            Ui.Text(card.Content, Loc.T("loboEres"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.FittedText(card.Content, LoboUi.Name(player.Rol), 40f, color, 1f, minSize: 18f, lines: 2);
            Ui.Text(card.Content, LoboUi.Description(player.Rol), TextRole.BodyLarge, Palette.TextPrimary, TextAlignmentOptions.Center);
            card.name = Loc.T("loboCartaSemantica", ("rol", LoboUi.Name(player.Rol)));
            foreach (var line in Extras(player)) Ui.Text(content, line, TextRole.BodyMedium, Palette.NeonYellow, TextAlignmentOptions.Center, bold: true);
            Ui.Button(content, Loc.T(Game.RepartoIndex + 1 < Game.Jugadores.Count ? "loboOcultarYPasar" : "loboOcultarUltimo"), Next,
                ButtonStyle.Filled, IconId.Check, glow: 0.8f).Sized(height: 64f);
        }

        /// <summary>Lo que sabe cada uno al ver su carta: los lobos, los suyos (según la regla); hermanos y sectario, los suyos.</summary>
        private IEnumerable<string> Extras(LoboPlayer player)
        {
            string Names(IEnumerable<int> indices) => string.Join(", ", indices.Select(i => Game.Jugadores[i].Name));
            if (LoboCatalog.EsLoboDeCarta(player.Rol) && Game.Rules.LobosSeConocenAlRepartir)
            {
                var mates = Game.CompanerosLobos(player.Index);
                yield return mates.Count == 0 ? Loc.T("loboEresElUnicoLobo") : Loc.T("loboTusCompaneros", ("nombres", Names(mates)));
            }
            if (player.Rol == LoboRol.Hermana || player.Rol == LoboRol.Hermano)
            {
                var siblings = Game.Jugadores.Where(p => p.Index != player.Index && p.Rol == player.Rol).Select(p => p.Index);
                yield return Loc.T(player.Rol == LoboRol.Hermana ? "loboTuHermana" : "loboTusHermanos", ("nombres", Names(siblings)));
            }
            if (player.Rol == LoboRol.Sectario) yield return Loc.T("loboTuGrupo", ("nombres", Names(Game.GrupoDe(player.Index))));
            if (player.Rol == LoboRol.Ladron) yield return Loc.T("loboLadronAviso");
            if (player.Rol == LoboRol.Angel) yield return Loc.T("loboAngelAviso");
        }

        private void BuildHandoff(RectTransform content)
        {
            Ui.Space(content, 24f);
            Ui.Icon(content, IconId.RecordVoiceOver, 96f, Palette.NeonViolet, 1f);
            Ui.Text(content, Loc.T("loboDaleElMovil"), TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, 0.8f, Palette.NeonViolet,
                bold: true);
            Ui.Text(content, Loc.T("loboDaleElMovilAyuda"), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Button(content, Loc.T("loboSoyElMaster"), StartNight, ButtonStyle.Filled, IconId.Bedtime, Palette.NeonViolet, 0.9f).Sized(height: 64f);
        }

        /// <summary>Enseña la carta del jugador de turno (también lo usan los tests).</summary>
        public void Reveal()
        {
            Revealed = true;
            _module.Audio?.Play(AppSound.Reveal, 0.7f);
            Rebuild();
        }

        /// <summary>Oculta la carta y pasa al siguiente (o al game master).</summary>
        public void Next()
        {
            Revealed = false;
            Game.SiguienteReparto();
            Rebuild();
        }

        public void StartNight() => _module.GoToPhase();

        public override bool OnBack()
        {
            if (Game.Config == null) return false;
            AbandonGameDialog.Show(_module.ResetToSetup);
            return true;
        }

        private void Rebuild()
        {
            Root.ClearChildren();
            Build(Root);
        }
    }
}
