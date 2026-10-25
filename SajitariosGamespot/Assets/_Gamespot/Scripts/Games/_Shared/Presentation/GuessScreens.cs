using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Shared
{
    /// <summary>
    /// Lo que necesitan las pantallas comunes de los juegos de adivinar por turnos (V074, Mímica y ¿Quién soy?): el
    /// controlador, las rutas y los textos propios de cada juego.
    /// </summary>
    public sealed class GuessGameHost
    {
        public GuessGameHost(GuessGameController flow, Router router, AudioService audio, string titleKey, string setupRoute, string readyRoute,
            string turnRoute, string summaryRoute, string endRoute)
        {
            Flow = flow;
            Router = router;
            Audio = audio;
            TitleKey = titleKey;
            SetupRoute = setupRoute;
            ReadyRoute = readyRoute;
            TurnRoute = turnRoute;
            SummaryRoute = summaryRoute;
            EndRoute = endRoute;
        }

        public GuessGameController Flow { get; }
        public Router Router { get; }
        public AudioService Audio { get; }
        public string TitleKey { get; }
        public string SetupRoute { get; }
        public string ReadyRoute { get; }
        public string TurnRoute { get; }
        public string SummaryRoute { get; }
        public string EndRoute { get; }

        /// <summary>Ayuda de la pantalla de antes del turno («uno hace la mímica, el resto adivina»).</summary>
        public string ReadyHelpKey { get; set; }

        /// <summary>Texto del botón de empezar el turno.</summary>
        public string StartTurnKey { get; set; } = "empezarTurno";

        /// <summary>Nombre de la etiqueta de una carta (el estilo de la Mímica), o nulo.</summary>
        public Func<int, string> TagLabel { get; set; }

        /// <summary>«Jugar otra»: el juego vuelve a empezar con la configuración de antes.</summary>
        public Action PlayAgain { get; set; }

        public string RouteForPhase()
        {
            switch (Flow.Fase)
            {
                case GuessFase.Preparado: return ReadyRoute;
                case GuessFase.Turno: return TurnRoute;
                case GuessFase.Resumen: return SummaryRoute;
                case GuessFase.Final: return EndRoute;
                default: return SetupRoute;
            }
        }

        public void GoToPhase() => Router.Go(RouteForPhase());

        public void ResetToMenu()
        {
            Flow.Reiniciar();
            Router.Go(Router.Home);
        }

        public void ResetToSetup()
        {
            Flow.Reiniciar();
            Router.Go(SetupRoute);
        }

        /// <summary>Las filas del marcador por puntos (los primeros, en cian).</summary>
        public List<ScoreRow> ScoreRows()
        {
            var match = Flow.Match;
            var order = match.Clasificacion;
            var positions = ScoreList.Positions(order, i => match.Puntos[i]);
            return order.Select((team, k) => new ScoreRow(positions[k], match.Equipos[team], Loc.T("puntosCorto", ("n", match.Puntos[team])),
                match.Objetivo > 0 ? Loc.T("objetivoPuntos", ("n", match.Objetivo)) : null,
                positions[k] == 1 ? Palette.NeonCyan : Palette.NeonViolet)).ToList();
        }
    }

    /// <summary>Base de las pantallas con partida en curso: casa y atrás confirman; la barra lleva «Terminar».</summary>
    public abstract class GuessPlayScreen : AppScreen
    {
        protected GuessPlayScreen(GuessGameHost host) => Host = host;

        protected GuessGameHost Host { get; }
        protected GuessGameController Flow => Host.Flow;
        protected abstract GuessFase Phase { get; }

        public sealed override void Build(RectTransform root)
        {
            var playing = Flow.Match != null && Flow.Fase == Phase;
            var scaffold = ScreenScaffold.Create(root, Loc.T(Host.TitleKey), playing && Phase != GuessFase.Final ? Subtitle() : null);
            if (!playing)
            {
                GameUi.HomeButton(scaffold.AppBar, Router);
                var column = Ui.Column(scaffold.Body, 16f, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter);
                column.Sized(flexHeight: 1f);
                Ui.Text(column, Loc.T("noHayPartidaEnCurso"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                Ui.Button(column, Loc.T("configurarPartida"), () => Router.Go(Host.SetupRoute));
                return;
            }
            if (Phase == GuessFase.Final)
            {
                GameUi.HomeButton(scaffold.AppBar, Router);
            }
            else
            {
                GameUi.HomeButton(scaffold.AppBar, Router, () => Flow.Reiniciar());
                if (Phase != GuessFase.Turno) GameUi.FinishButton(scaffold.AppBar, Finish);
            }
            BuildPhase(scaffold);
        }

        protected virtual string Subtitle()
        {
            var match = Flow.Match;
            if (match.Desempate) return Loc.T("rondaDesempate", ("ronda", match.Ronda));
            return match.Rondas > 0 ? Loc.T("rondaXDeY", ("ronda", match.Ronda), ("total", match.Rondas)) : Loc.T("rondaX", ("ronda", match.Ronda));
        }

        protected abstract void BuildPhase(ScreenScaffold scaffold);

        public override bool OnBack()
        {
            if (Flow.Match == null || Flow.Fase == GuessFase.Setup) return false;
            if (Flow.Fase == GuessFase.Final)
            {
                Host.ResetToMenu();
                return true;
            }
            AbandonGameDialog.Show(Host.ResetToSetup);
            return true;
        }

        public void Finish()
        {
            Flow.Terminar();
            Host.GoToPhase();
        }

        protected string TeamName => Flow.Match.Equipos[Flow.Match.Actual];
    }

    /// <summary>Antes del turno: a quién le toca, el marcador y «Empezar turno» (se pasa el móvil a ese equipo).</summary>
    public sealed class GuessReadyScreen : GuessPlayScreen
    {
        public GuessReadyScreen(GuessGameHost host) : base(host)
        {
        }

        protected override GuessFase Phase => GuessFase.Preparado;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var content = Ui.Scroll(scaffold.Body, 14f, new RectOffset(20, 20, 20, 24));
            Ui.Text(content, Loc.T("turnoDe"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Text(content, TeamName, TextRole.DisplayMedium, Palette.TextPrimary, TextAlignmentOptions.Center, 1f, Palette.NeonViolet, 2, true);
            if (Flow.Match.Desempate) Ui.Text(content, Loc.T("desempateAyuda"), TextRole.BodyMedium, Palette.NeonYellow, TextAlignmentOptions.Center);
            if (Host.ReadyHelpKey != null) Ui.Text(content, Loc.T(Host.ReadyHelpKey), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Text(content, Loc.T("segundosTurno", ("n", Flow.Options.Segundos)), TextRole.LabelLarge, Palette.NeonCyan, TextAlignmentOptions.Center);
            Ui.Button(content, Loc.T(Host.StartTurnKey), StartTurn, ButtonStyle.Filled, IconId.PlayArrow, Palette.NeonCyan, 0.9f).Sized(height: 72f);
            Ui.Text(content, Loc.T("marcador"), TextRole.TitleMedium, Palette.NeonCyan, glow: 0.6f, bold: true);
            ScoreList.Create(content, Host.ScoreRows());
        }

        public void StartTurn()
        {
            Flow.EmpezarTurno();
            Host.Audio?.Play(AppSound.CardFlip, 0.7f);
            Host.GoToPhase();
        }
    }

    /// <summary>
    /// Fin del turno: las cartas que salieron, acertadas (verde) o pasadas (rojo); se tocan para corregir lo que se
    /// marcó mal, y «Apuntar» suma los puntos del turno.
    /// </summary>
    public sealed class GuessSummaryScreen : GuessPlayScreen
    {
        private TMP_Text _total;
        private readonly List<NeonChip> _chips = new List<NeonChip>();

        public GuessSummaryScreen(GuessGameHost host) : base(host)
        {
        }

        protected override GuessFase Phase => GuessFase.Resumen;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var content = Ui.Scroll(scaffold.Body, 12f, new RectOffset(20, 20, 16, 24));
            Ui.Text(content, Loc.T("finDelTurno"), TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, 0.6f, Palette.NeonViolet,
                bold: true);
            Ui.Text(content, TeamName, TextRole.TitleLarge, Palette.NeonVioletText, TextAlignmentOptions.Center, maxLines: 2, bold: true);
            _total = Ui.Text(content, string.Empty, TextRole.TitleMedium, Palette.NeonCyan, TextAlignmentOptions.Center, 0.6f, bold: true);
            if (Flow.Turno.Count == 0)
            {
                Ui.Text(content, Loc.T("turnoSinCartas"), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            }
            else
            {
                Ui.Text(content, Loc.T("corregirAyuda"), TextRole.BodySmall, Palette.TextMuted, TextAlignmentOptions.Center);
            }
            _chips.Clear();
            for (var i = 0; i < Flow.Turno.Count; i++)
            {
                var index = i;
                var entry = Flow.Turno[i];
                var label = entry.Card.Text;
                if (Host.TagLabel != null && entry.Etiqueta >= 0) label = Loc.T("cartaConEstilo", ("carta", label), ("estilo", Host.TagLabel(entry.Etiqueta)));
                var chip = NeonChip.Create(content, label, entry.Acertada, entry.Acertada ? Palette.NeonGreen : Palette.NeonRed);
                chip.ToggleOnClick = false;
                chip.Clicked += () => Fix(index);
                _chips.Add(chip);
            }
            Ui.Button(content, Loc.T("apuntarTurno"), Confirm, ButtonStyle.Filled, IconId.Check, glow: 0.8f).Sized(height: 64f);
            Refresh();
        }

        /// <summary>Corrige una carta (también lo usan los tests).</summary>
        public void Fix(int index)
        {
            Flow.Corregir(index);
            Refresh();
        }

        public void Confirm()
        {
            Flow.ConfirmarTurno();
            Host.Audio?.Play(Flow.Fase == GuessFase.Final ? AppSound.GameOver : AppSound.Swipe, 0.7f);
            Host.GoToPhase();
        }

        private void Refresh()
        {
            for (var i = 0; i < _chips.Count; i++)
            {
                var ok = Flow.Turno[i].Acertada;
                _chips[i].Selected = ok;
                _chips[i].SetAccent(ok ? Palette.NeonGreen : Palette.NeonRed);
                _chips[i].name = Loc.T(ok ? "cartaAcertada" : "cartaPasada", ("carta", Flow.Turno[i].Card.Text));
            }
            _total.text = Loc.T("resumenTurno", ("aciertos", Flow.Aciertos), ("pases", Flow.Fallos), ("puntos", Flow.PuntosTurno));
        }
    }

    /// <summary>Final: el ganador (o el empate), el marcador y jugar otra, ajustes o menú.</summary>
    public sealed class GuessEndScreen : GuessPlayScreen
    {
        public GuessEndScreen(GuessGameHost host) : base(host)
        {
        }

        protected override GuessFase Phase => GuessFase.Final;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var match = Flow.Match;
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(20, 20, 20, 24));
            var panel = Ui.Panel(content, Palette.NeonCyan, glow: 1f, padding: new RectOffset(24, 24, 24, 24), spacing: 10f, alignment: TextAnchor.UpperCenter);
            PulseGlow.On(panel, 0.8f, 1.3f);
            Ui.Icon(panel.Content, IconId.Trophy, 64f, Palette.NeonCyan, 1f);
            var winners = match.Ganadores;
            Ui.Text(panel.Content, Loc.T(winners.Count > 1 ? "empateEntre" : "ganador"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.FittedText(panel.Content, string.Join(" · ", winners.Select(i => match.Equipos[i])), 36f, Palette.TextPrimary, 1f, lines: 2);
            Ui.Text(content, Loc.T("cartasJugadas", ("n", Flow.Jugadas)), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            ScoreList.Create(content, Host.ScoreRows());
            Ui.Button(content, Loc.T("jugarOtra"), () => Host.PlayAgain?.Invoke(), ButtonStyle.Filled, IconId.Refresh, glow: 0.8f);
            Ui.Button(content, Loc.T("cambiarAjustes"), Host.ResetToSetup, ButtonStyle.Outlined, IconId.Tune);
            Ui.Button(content, Loc.T("volverAlMenu"), Host.ResetToMenu, ButtonStyle.Text, IconId.Home);
        }
    }
}
