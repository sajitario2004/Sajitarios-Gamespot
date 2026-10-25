using System.Collections.Generic;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.Wavelength.Data;
using Gamespot.Games.Wavelength.Domain;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Wavelength
{
    /// <summary>Estado compartido de las pantallas de Wavelength.</summary>
    public sealed class WavelengthModule
    {
        public const string SetupRoute = "/wavelength";
        public const string ClueRoute = "/wavelength/clue";
        public const string PassRoute = "/wavelength/pass";
        public const string GuessRoute = "/wavelength/guess";
        public const string RevealRoute = "/wavelength/reveal";
        public const string GameOverRoute = "/wavelength/game-over";

        public WavelengthModule(WavelengthFlowController flow, SpectrumRepository spectra, Router router)
        {
            Flow = flow;
            Spectra = spectra;
            Router = router;
        }

        public WavelengthFlowController Flow { get; }
        public SpectrumRepository Spectra { get; }
        public Router Router { get; }
        public List<string> DraftNames { get; } = new List<string>();
        public int DraftRounds { get; set; } = WavelengthConfig.DefaultRounds;

        public void ResetToSetup()
        {
            Flow.Reiniciar();
            Router.Go(SetupRoute);
        }

        public void ResetToMenu()
        {
            Flow.Reiniciar();
            Router.Go(Router.Home);
        }

        /// <summary>Texto y color de la banda conseguida.</summary>
        public static (string text, Color color) Band(int points)
        {
            switch (points)
            {
                case WavelengthScoring.PointsBullseye: return (Loc.T("wavelengthBandBlanco"), Palette.NeonCyan);
                case WavelengthScoring.PointsNear: return (Loc.T("wavelengthBandCerca"), Palette.NeonCyan);
                case WavelengthScoring.PointsFar: return (Loc.T("wavelengthBandLejos"), Palette.NeonMagenta);
                default: return (Loc.T("wavelengthBandFallo"), Palette.NeonMagenta);
            }
        }
    }

    /// <summary>Setup: 2–8 jugadores (el psíquico rota en este orden) y 1–20 rondas (8 por defecto).</summary>
    public sealed class WavelengthSetupScreen : AppScreen
    {
        private readonly WavelengthModule _module;
        private PlayerListEditor _players;
        private NeonStepper _rounds;

        public WavelengthSetupScreen(WavelengthModule module) => _module = module;

        public PlayerListEditor Players => _players;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("wavelengthTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "wavelengthTitulo", GameUi.RuleKeys("reglasWavelength", 5));
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 8, 32));

            var playersPanel = Ui.Panel(content, Palette.NeonCyan.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(playersPanel.Content, Loc.T("setupJugadores"), TextRole.TitleMedium, Palette.NeonCyan, glow: 0.6f, bold: true);
            Ui.Text(playersPanel.Content, Loc.T("wavelengthSetupRangoJugadores", ("min", WavelengthConfig.MinPlayers), ("max", WavelengthConfig.MaxPlayers)),
                TextRole.BodySmall, Palette.TextMuted);
            _players = PlayerListEditor.Create(playersPanel.Content, WavelengthConfig.MinPlayers, WavelengthConfig.MaxPlayers, Palette.NeonCyan,
                _module.DraftNames);
            _players.NamesChanged += SaveDraft;
            _players.CountChanged += SaveDraft;

            var roundsPanel = Ui.Panel(content, Palette.NeonMagenta.WithAlpha(0.5f), glow: 0.3f, spacing: 6f);
            Ui.Text(roundsPanel.Content, Loc.T("wavelengthSetupRondas"), TextRole.TitleMedium, Palette.NeonMagenta, glow: 0.6f, bold: true);
            _rounds = NeonStepper.Create(roundsPanel.Content, _module.DraftRounds, WavelengthConfig.MinRounds, WavelengthConfig.MaxRounds,
                n => Loc.T("rondasContador", ("n", n)), Palette.NeonMagenta);
            _rounds.SetHelper(Loc.T("wavelengthSetupRondasAyuda", ("min", WavelengthConfig.MinRounds), ("max", WavelengthConfig.MaxRounds)));
            _rounds.Changed += v => _module.DraftRounds = v;

            Ui.Button(content, Loc.T("wavelengthEmpezarPartida"), Start, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f);
        }

        private void SaveDraft()
        {
            _module.DraftNames.Clear();
            _module.DraftNames.AddRange(_players.RawNames);
        }

        public void Start()
        {
            SaveDraft();
            var (config, error) = WavelengthConfig.Create(_players.NormalizedNames, _rounds.Value);
            if (error.HasValue)
            {
                Snackbar.Show(ErrorText(error.Value), Palette.NeonError);
                return;
            }
            _module.Flow.Iniciar(config);
            if (_module.Flow.Fase == WavelengthFase.Clue)
            {
                Router.Go(WavelengthModule.ClueRoute);
                return;
            }
            Dialogs.Show(Loc.T("wavelengthSinEspectrosTitulo"), Loc.T("wavelengthSinEspectrosMensaje"), new[]
            {
                new DialogAction(Loc.T("aceptar"), (System.Action)null, ButtonStyle.Filled, Palette.NeonCyan)
            });
        }

        private static string ErrorText(WavelengthConfigError error)
        {
            switch (error)
            {
                case WavelengthConfigError.PocosJugadores: return Loc.T("errorPocosJugadores", ("min", WavelengthConfig.MinPlayers));
                case WavelengthConfigError.DemasiadosJugadores: return Loc.T("errorDemasiadosJugadores", ("max", WavelengthConfig.MaxPlayers));
                case WavelengthConfigError.NombreVacio: return Loc.T("errorNombreVacio");
                case WavelengthConfigError.NombresDuplicados: return Loc.T("errorNombresDuplicados");
                default: return Loc.T("errorNoSePudoIniciar");
            }
        }
    }

    /// <summary>
    /// Base de las pantallas con partida en curso: casa y atrás piden confirmación. Las del dial usan una
    /// columna SIN scroll: el dial ocupa el espacio que sobra (mín. 180) y nunca compite con un arrastre de scroll
    /// (uno de los fallos del Flutter).
    /// </summary>
    public abstract class WavelengthPlayScreen : AppScreen
    {
        public const float MinDialHeight = 180f;

        protected WavelengthPlayScreen(WavelengthModule module) => Module = module;

        protected WavelengthModule Module { get; }

        public sealed override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("wavelengthTitulo"), RoundText());
            GameUi.HomeButton(scaffold.AppBar, Router, () => Module.Flow.Reiniciar());
            if (Module.Flow.Session == null || Module.Flow.CurrentRound == null)
            {
                var column = Ui.Column(scaffold.Body, 16f, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter);
                column.Sized(flexHeight: 1f);
                Ui.Text(column, Loc.T("noHayPartidaEnCurso"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                Ui.Button(column, Loc.T("configurarPartida"), () => Router.Go(WavelengthModule.SetupRoute));
                return;
            }
            var body = Ui.Column(scaffold.Body, 12f, new RectOffset(16, 16, 8, 16));
            body.Sized(flexHeight: 1f);
            BuildGame(body);
        }

        protected abstract void BuildGame(RectTransform body);

        public override bool OnBack()
        {
            if (Module.Flow.Session == null) return false;
            AbandonGameDialog.Show(Module.ResetToSetup);
            return true;
        }

        private string RoundText()
        {
            var session = Module.Flow.Session;
            return session == null ? null
                : Loc.T("wavelengthRondaXDeY", ("actual", System.Math.Min(session.CurrentRoundIndex + 1, session.TotalRounds)), ("total", session.TotalRounds));
        }

        /// <summary>Fila con los dos conceptos (izquierda y derecha), en vez de pintarlos también sobre el dial.</summary>
        protected static void ConceptLabels(RectTransform parent, Spectrum spectrum)
        {
            var row = Ui.Row(parent, 8f, name: "Concepts");
            var left = Ui.Text(row, spectrum.Left, TextRole.TitleMedium, Palette.NeonCyan, TextAlignmentOptions.Left, 0.6f, maxLines: 2, bold: true);
            left.Sized(flexWidth: 1f);
            var right = Ui.Text(row, spectrum.Right, TextRole.TitleMedium, Palette.NeonCyan, TextAlignmentOptions.Right, 0.6f, maxLines: 2, bold: true);
            right.Sized(flexWidth: 1f);
        }

        /// <summary>Zona flexible con el dial.</summary>
        protected static WavelengthDial DialArea(RectTransform parent)
        {
            var area = Ui.Column(parent, 0f, name: "DialArea");
            area.Sized(flexHeight: 1f);
            area.MinHeight(MinDialHeight);
            var dial = WavelengthDial.Create(area);
            dial.Sized(flexWidth: 1f, flexHeight: 1f);
            return dial;
        }
    }

    /// <summary>El psíquico ve el objetivo y escribe una pista.</summary>
    public sealed class WavelengthClueScreen : WavelengthPlayScreen
    {
        private NeonTextField _clue;

        public WavelengthClueScreen(WavelengthModule module) : base(module)
        {
        }

        public WavelengthDial Dial { get; private set; }

        protected override void BuildGame(RectTransform body)
        {
            var flow = Module.Flow;
            Ui.Text(body, $"{Loc.T("wavelengthCluePsicoEtiqueta")}: {flow.CurrentPsychic}", TextRole.TitleLarge, Palette.TextPrimary,
                TextAlignmentOptions.Center, 0.8f, Palette.NeonCyan, 1, true);
            var info = Ui.Panel(body, Palette.NeonCyan.WithAlpha(0.6f), padding: new RectOffset(14, 14, 10, 10));
            Ui.Text(info.Content, Loc.T("wavelengthClueScreenInstruccion"), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            ConceptLabels(body, flow.CurrentRound.Spectrum);
            Dial = DialArea(body);
            Dial.Setup(DialMode.Clue, flow.CurrentRound.Target, 0.5f);
            _clue = NeonTextField.Create(body, Loc.T("wavelengthClueFieldLabel"), Loc.T("wavelengthClueFieldHint"), characterLimit: 60);
            _clue.Submitted += _ => Confirm();
            Ui.Button(body, Loc.T("wavelengthConfirmarPista"), Confirm, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f);
        }

        /// <summary>Confirma la pista escrita (vacía: aviso y se queda).</summary>
        public void Confirm() => Confirm(_clue.Value);

        public void Confirm(string clue)
        {
            if (string.IsNullOrWhiteSpace(clue))
            {
                Snackbar.Show(Loc.T("campoObligatorio"), Palette.NeonError);
                return;
            }
            Module.Flow.ConfirmarPista(clue);
            Router.Go(WavelengthModule.PassRoute);
        }
    }

    /// <summary>"Pásale el móvil al GRUPO".</summary>
    public sealed class WavelengthPassScreen : WavelengthPlayScreen
    {
        public WavelengthPassScreen(WavelengthModule module) : base(module)
        {
        }

        protected override void BuildGame(RectTransform body)
        {
            Ui.Flex(body);
            var row = Ui.Row(body, 0f, alignment: TextAnchor.MiddleCenter);
            var circle = Ui.Panel(row, Palette.NeonMagenta, Palette.Surface, 0.9f, new RectOffset(28, 28, 28, 28), radius: 64f, alignment: TextAnchor.MiddleCenter);
            circle.Sized(128f, 128f);
            Ui.Icon(circle.Content, IconId.Phone, 72f, Palette.NeonMagenta, 0.9f);
            PulseGlow.On(circle);
            Ui.Text(body, Loc.T("wavelengthPassDeviceInstruccion"), TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, 0.9f,
                Palette.NeonMagenta, bold: true);
            Ui.Text(body, Loc.T("wavelengthPassDeviceAyuda"), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Flex(body);
            Ui.Button(body, Loc.T("continuar"), Continue, ButtonStyle.Filled, IconId.ArrowForward, Palette.NeonMagenta, 0.8f).Sized(height: 64f);
        }

        public void Continue()
        {
            Module.Flow.PasarDispositivo();
            Router.Go(WavelengthModule.GuessRoute);
        }
    }

    /// <summary>El grupo mueve la aguja (empieza en el centro) y confirma.</summary>
    public sealed class WavelengthGuessScreen : WavelengthPlayScreen
    {
        public WavelengthGuessScreen(WavelengthModule module) : base(module)
        {
        }

        public WavelengthDial Dial { get; private set; }

        protected override void BuildGame(RectTransform body)
        {
            var round = Module.Flow.CurrentRound;
            var cluePanel = Ui.Panel(body, Palette.NeonMagenta, glow: 0.6f, padding: new RectOffset(16, 16, 10, 10), alignment: TextAnchor.MiddleCenter);
            Ui.Text(cluePanel.Content, Loc.T("wavelengthPistaEtiqueta"), TextRole.LabelLarge, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Text(cluePanel.Content, round.Clue ?? string.Empty, TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, 0.9f,
                Palette.NeonMagenta, 2, true);
            Ui.Text(body, Loc.T("wavelengthGuessInstruccion"), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            ConceptLabels(body, round.Spectrum);
            Dial = DialArea(body);
            Dial.Setup(DialMode.Guess, round.Target, 0.5f);
            Ui.Button(body, Loc.T("wavelengthConfirmarAdivinanza"), Confirm, ButtonStyle.Filled, IconId.Check, glow: 0.8f);
        }

        public void Confirm()
        {
            Module.Flow.SubmitGuess(Dial.Needle);
            Router.Go(WavelengthModule.RevealRoute);
        }
    }

    /// <summary>
    /// Revelado: bandas y objetivo aparecen en 0,7 s con la aguja donde la dejó el grupo (en el Flutter salía en el
    /// centro), banda conseguida, puntos de la ronda y total CON esta ronda (en el Flutter no la incluía).
    /// </summary>
    public sealed class WavelengthRevealScreen : WavelengthPlayScreen
    {
        private bool _advancing;

        public WavelengthRevealScreen(WavelengthModule module) : base(module)
        {
        }

        public WavelengthDial Dial { get; private set; }
        public string BandText { get; private set; }
        public string TotalText { get; private set; }

        protected override void BuildGame(RectTransform body)
        {
            var flow = Module.Flow;
            var round = flow.CurrentRound;
            var points = round.Score ?? 0;
            ConceptLabels(body, round.Spectrum);
            Dial = DialArea(body);
            Dial.Setup(DialMode.Reveal, round.Target, (float)(round.Guess ?? 0.5));

            var (band, color) = WavelengthModule.Band(points);
            BandText = band;
            TotalText = Loc.T("wavelengthRevealTotalPuntos", ("pts", flow.ScoreIncludingCurrent));
            var panel = Ui.Panel(body, color, glow: 0.9f, padding: new RectOffset(16, 16, 12, 12), spacing: 4f, alignment: TextAnchor.UpperCenter);
            panel.name = Loc.T("wavelengthRevealScoreSemantica", ("banda", band), ("puntos", points));
            Ui.Text(panel.Content, band, TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, 1f, color, bold: true);
            Ui.Text(panel.Content, Loc.T("wavelengthRevealPuntos", ("pts", points)), TextRole.TitleMedium, color, TextAlignmentOptions.Center, bold: true);
            Ui.Text(panel.Content, TotalText, TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);

            var last = flow.Session.CurrentRoundIndex + 1 >= flow.Session.TotalRounds;
            Ui.Button(body, Loc.T(last ? "wavelengthVerResultado" : "wavelengthSiguienteRonda"), Next, ButtonStyle.Filled,
                last ? IconId.Trophy : IconId.ArrowForward, glow: 0.8f);
        }

        public void Next()
        {
            if (_advancing) return;
            _advancing = true;
            Module.Flow.Next();
            Router.Go(Module.Flow.Fase == WavelengthFase.GameOver ? WavelengthModule.GameOverRoute : WavelengthModule.ClueRoute);
        }
    }

    /// <summary>Fin: puntuación final y media por ronda (localizada).</summary>
    public sealed class WavelengthGameOverScreen : AppScreen
    {
        private readonly WavelengthModule _module;

        public WavelengthGameOverScreen(WavelengthModule module) => _module = module;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("wavelengthFinDePartida"));
            GameUi.HomeButton(scaffold.AppBar, Router).Clicked += () => _module.Flow.Reiniciar();
            var content = Ui.Scroll(scaffold.Body, 20f, new RectOffset(24, 24, 40, 24));
            var session = _module.Flow.Session;
            var total = session?.CumulativeScore ?? 0;
            var rounds = session?.CompletedRounds.Count ?? 0;
            var average = rounds > 0 ? (double)total / rounds : 0.0;

            var panel = Ui.Panel(content, Palette.NeonCyan, glow: 1f, padding: new RectOffset(24, 24, 32, 32), spacing: 12f, alignment: TextAnchor.UpperCenter);
            PulseGlow.On(panel, 0.8f, 1.3f);
            Ui.Icon(panel.Content, IconId.Trophy, 72f, Palette.NeonCyan, 1f);
            Ui.Text(panel.Content, Loc.T("wavelengthPuntuacionFinal"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Text(panel.Content, Loc.T("wavelengthPuntosTotales", ("pts", total)), TextRole.DisplaySmall, Palette.TextPrimary, TextAlignmentOptions.Center,
                1f, Palette.NeonCyan, bold: true);
            Ui.Text(panel.Content, Loc.T("wavelengthPuntosPorRonda", ("avg", average.ToString("0.0", Loc.Culture))), TextRole.TitleMedium, Palette.NeonCyan,
                TextAlignmentOptions.Center);
            Ui.Button(content, Loc.T("wavelengthJugarOtra"), _module.ResetToSetup, ButtonStyle.Filled, IconId.Refresh, glow: 0.8f);
            Ui.Button(content, Loc.T("volverAlMenu"), _module.ResetToMenu, ButtonStyle.Outlined, IconId.Home);
        }

        public override bool OnBack()
        {
            _module.ResetToSetup();
            return true;
        }
    }
}
