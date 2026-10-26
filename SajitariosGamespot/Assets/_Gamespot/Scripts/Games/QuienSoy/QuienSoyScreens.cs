using System;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Sensors;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.QuienSoy.Domain;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Games.QuienSoy
{
    /// <summary>
    /// Setup de ¿Quién soy? (V075): por equipos (2–4) o cada uno a lo suyo (2–12), categorías (Cachondeo +18 con la
    /// confirmación de edad; también las propias), tiempo del turno, rondas, pases (y si restan) y el control: inclinar
    /// el móvil (y al revés, por si acaso) o solo los botones.
    /// </summary>
    public sealed class QuienSoySetupScreen : AppScreen
    {
        private readonly QuienSoyModule _module;
        private GameObject _teamsPanel;
        private GameObject _playersPanel;

        public QuienSoySetupScreen(QuienSoyModule module) => _module = module;

        public PlayerListEditor Teams { get; private set; }
        public PlayerListEditor Players { get; private set; }
        public CategoryPicker Categories { get; private set; }
        public ChoiceChips<QuienSoyModo> Mode { get; private set; }

        public bool TeamsVisible => _teamsPanel != null && _teamsPanel.activeSelf;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("quienSoyTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "quienSoyTitulo", GameUi.RuleKeys("reglasQuienSoy", 6));
            scaffold.AppBar.AddAction(IconId.MenuBook, () => Router.Go(QuienSoyModule.CardsRoute)).name = Loc.T("gestionarTarjetas");
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 8, 32));
            var draft = _module.Draft;

            var mode = SetupSection.Create(content, Loc.T("quienSoyComoSeJuega"), Loc.T("quienSoyModoAyuda"), Palette.NeonViolet);
            Mode = ChoiceChips<QuienSoyModo>.Create(mode.Content, new[]
            {
                (QuienSoyModo.PorEquipos, Loc.T("quienSoyModoEquipos")),
                (QuienSoyModo.CadaUno, Loc.T("quienSoyModoCadaUno"))
            }, draft.Modo, Palette.NeonViolet, SelectMode);

            var teams = SetupSection.Create(content, Loc.T("equipos"), Loc.T("quienSoyEquiposAyuda"), Palette.NeonCyan);
            _teamsPanel = teams.gameObject;
            Teams = PlayerListEditor.Create(teams.Content, QuienSoyConfig.MinNombres, QuienSoyConfig.MaxEquipos, Palette.NeonCyan, _module.DraftTeams, false,
                "equipoNumero", "nombreDelEquipoNumero", "anadirEquipo", "maximoEquiposAlcanzado", "quitarEquipo");
            Teams.NamesChanged += SaveNames;
            Teams.CountChanged += SaveNames;
            var players = SetupSection.Create(content, Loc.T("jugadores"), Loc.T("quienSoyJugadoresAyuda"), Palette.NeonCyan);
            _playersPanel = players.gameObject;
            Players = PlayerListEditor.Create(players.Content, QuienSoyConfig.MinNombres, QuienSoyConfig.MaxJugadores, Palette.NeonCyan, _module.DraftPlayers);
            Players.NamesChanged += SaveNames;
            Players.CountChanged += SaveNames;
            ShowNames(draft.Modo);

            Categories = CategoryPicker.Create(content, _module.AvailableCategories(), _module.DraftCategories, Palette.NeonViolet,
                _module.KnownCategories, true);

            var turn = SetupSection.Create(content, Loc.T("turno"), null, Palette.NeonMagenta);
            Ui.Text(turn.Content, Loc.T("duracionTurno"), TextRole.LabelLarge, Palette.TextMuted);
            ChoiceChips<int>.Create(turn.Content, QuienSoyConfig.SegundosOptions.Select(n => (n, Loc.T("segundosCorto", ("n", n)))).ToList(), draft.Segundos,
                Palette.NeonMagenta, v => draft.Segundos = v);
            Ui.Text(turn.Content, Loc.T("rondasPartida"), TextRole.LabelLarge, Palette.TextMuted);
            ChoiceChips<int>.Create(turn.Content, QuienSoyConfig.RondasOptions.Select(n => (n, n.ToString())).ToList(), draft.Rondas, Palette.NeonMagenta,
                v => draft.Rondas = v);
            Ui.Text(turn.Content, Loc.T("pasesPorTurno"), TextRole.LabelLarge, Palette.TextMuted);
            ChoiceChips<int>.Create(turn.Content, QuienSoyConfig.PasesOptions.Select(n => (n, PassesLabel(n))).ToList(), draft.Pases, Palette.NeonMagenta,
                v => draft.Pases = v);
            var penalty = NeonSwitch.Create(turn.Content, Loc.T("pasarResta"), Loc.T("pasarRestaAyuda"), draft.PenalizarPases);
            penalty.Changed += v => draft.PenalizarPases = v;

            var control = SetupSection.Create(content, Loc.T("quienSoyControl"), Loc.T("quienSoyControlAyuda"), Palette.NeonCyan);
            var tilt = NeonSwitch.Create(control.Content, Loc.T("quienSoyInclinar"), Loc.T("quienSoyInclinarAyuda"), draft.Inclinar);
            tilt.Changed += v => draft.Inclinar = v;
            var invert = NeonSwitch.Create(control.Content, Loc.T("quienSoyInvertir"), Loc.T("quienSoyInvertirAyuda"), draft.InvertirInclinacion);
            invert.Changed += v => draft.InvertirInclinacion = v;

            Ui.Button(content, Loc.T("empezarPartida"), Start, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f);
        }

        private static string PassesLabel(int passes) =>
            passes == GuessGameOptions.SinLimite ? Loc.T("sinLimite") : passes == 0 ? Loc.T("ninguno") : passes.ToString();

        public void SelectMode(QuienSoyModo mode)
        {
            _module.Draft.Modo = mode;
            if (Mode != null && Mode.Value != mode) Mode.Select(mode);
            ShowNames(mode);
        }

        private void ShowNames(QuienSoyModo mode)
        {
            if (_teamsPanel != null) _teamsPanel.SetActive(mode == QuienSoyModo.PorEquipos);
            if (_playersPanel != null) _playersPanel.SetActive(mode == QuienSoyModo.CadaUno);
        }

        private void SaveNames()
        {
            if (Teams != null)
            {
                _module.DraftTeams.Clear();
                _module.DraftTeams.AddRange(Teams.RawNames);
            }
            if (Players != null)
            {
                _module.DraftPlayers.Clear();
                _module.DraftPlayers.AddRange(Players.RawNames);
            }
        }

        public void Start()
        {
            SaveNames();
            var byTeams = _module.Draft.Modo == QuienSoyModo.PorEquipos;
            var (config, error) = QuienSoyConfig.Create((byTeams ? Teams : Players).NormalizedNames, Categories.Selected, _module.Draft);
            if (error.HasValue)
            {
                Snackbar.Show(ErrorText(error.Value, byTeams), Palette.NeonError);
                return;
            }
            if (!byTeams) Players.Remember();
            _module.Start(config);
            if (_module.Flow.Fase == GuessFase.Preparado)
            {
                _module.Host.GoToPhase();
                return;
            }
            Dialogs.Show(Loc.T("sinTarjetasTitulo"), Loc.T("sinTarjetasMensaje"), new[]
            {
                new DialogAction(Loc.T("aceptar"), (Action)null, ButtonStyle.Filled, Palette.NeonCyan)
            });
        }

        public static string ErrorText(QuienSoyConfigError error, bool teams)
        {
            switch (error)
            {
                case QuienSoyConfigError.PocosNombres:
                    return Loc.T(teams ? "errorMinEquipos" : "errorMinJugadores", ("n", QuienSoyConfig.MinNombres));
                case QuienSoyConfigError.DemasiadosNombres:
                    return teams ? Loc.T("errorMaxEquipos", ("n", QuienSoyConfig.MaxEquipos)) : Loc.T("errorMaxJugadores", ("n", QuienSoyConfig.MaxJugadores));
                case QuienSoyConfigError.NombreVacio: return Loc.T(teams ? "errorEquipoSinNombre" : "errorNombreVacio");
                case QuienSoyConfigError.NombresRepetidos: return Loc.T(teams ? "errorEquiposRepetidos" : "errorNombresDuplicados");
                default: return Loc.T("errorSinCategorias");
            }
        }
    }

    /// <summary>
    /// El turno, en horizontal para leerlo con el móvil en la frente (el marco gira según cómo se sujete): primero 3 s
    /// para colocárselo; después la cuenta atrás, la palabra en grande y Pasar / Acierto a los lados. Inclinar hacia
    /// abajo es acierto y hacia arriba, pasar (respecto a la postura del principio); cada gesto suena, vibra y destella.
    /// </summary>
    public sealed class QuienSoyTurnScreen : GuessPlayScreen
    {
        public const float PrepSeconds = 3f;
        public const float FlashSeconds = 0.6f;

        private readonly QuienSoyModule _module;
        private readonly Countdown _prep = new Countdown();
        private readonly Countdown _timer = new Countdown();
        private readonly TiltDetector _tilt = new TiltDetector();
        private LandscapeFrame _frame;
        private GameObject _prepOverlay;
        private TMP_Text _prepCount;
        private TMP_Text _timerText;
        private TMP_Text _word;
        private TMP_Text _category;
        private TMP_Text _score;
        private TMP_Text _flashText;
        private NeonPanel _panel;
        private NeonButton _pass;
        private float _flash;

        public QuienSoyTurnScreen(QuienSoyModule module) : base(module.Host)
        {
            _module = module;
            _prep.SecondChanged += s =>
            {
                if (_prepCount != null && s > 0) _prepCount.text = s.ToString();
                if (s > 0 && _prep.Running) Host.Audio?.Play(AppSound.Beep, 0.7f);
            };
            _prep.Finished += BeginGuessing;
            _timer.SecondChanged += ShowSeconds;
            _timer.Finished += TimeUp;
        }

        protected override GuessFase Phase => GuessFase.Turno;

        public bool Preparing => _prep.Running;
        public Countdown Timer => _timer;
        public string Word => _word != null ? _word.text : null;
        public LandscapeFrame Frame => _frame;

        protected override string Subtitle() => TeamName;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            _frame = LandscapeFrame.Create(scaffold.Body);
            var row = Ui.Row(_frame.Content, 12f, new RectOffset(12, 12, 8, 8), TextAnchor.MiddleCenter, "Turn");
            UiLayout.Stretch(row);
            _pass = Ui.Button(row, Loc.T("pasar"), Pass, ButtonStyle.Outlined, IconId.ArrowUpward, Palette.NeonRed);
            _pass.Sized(width: 190f, flexHeight: 1f);
            var center = Ui.Column(row, 4f, alignment: TextAnchor.MiddleCenter);
            center.Sized(width: 0f, flexWidth: 1f, flexHeight: 1f);
            _timerText = Ui.FittedText(center, string.Empty, 40f, Palette.NeonCyan, 1f);
            _category = Ui.Text(center, string.Empty, TextRole.LabelLarge, Palette.NeonVioletText, TextAlignmentOptions.Center, maxLines: 1);
            _panel = Ui.Panel(center, Palette.NeonCyan, glow: 1f, padding: new RectOffset(14, 14, 14, 14), alignment: TextAnchor.MiddleCenter);
            _panel.Sized(flexHeight: 1f);
            _word = Ui.FittedText(_panel.Content, string.Empty, 64f, Palette.TextPrimary, 1f, minSize: 20f, lines: 2);
            _score = Ui.Text(center, string.Empty, TextRole.TitleSmall, Palette.NeonGreen, TextAlignmentOptions.Center, bold: true, maxLines: 1);
            Ui.Button(row, Loc.T("acierto"), Correct, ButtonStyle.Filled, IconId.ArrowDownward, Palette.NeonGreen, 0.7f).Sized(width: 190f, flexHeight: 1f);

            // El destello del gesto y la pantalla de colocarse el móvil van encima de todo.
            _flashText = Ui.Text(_frame.Content, string.Empty, TextRole.DisplayMedium, Palette.TextPrimary, TextAlignmentOptions.Center, 1f, bold: true);
            UiLayout.Stretch(_flashText.rectTransform);
            _flashText.gameObject.GetOrAdd<LayoutElement>().ignoreLayout = true;
            _flashText.gameObject.SetActive(false);

            var overlay = Ui.Panel(_frame.Content, Palette.NeonViolet, Palette.Background, 1f, new RectOffset(24, 24, 24, 24), 8f,
                alignment: TextAnchor.MiddleCenter);
            overlay.gameObject.GetOrAdd<LayoutElement>().ignoreLayout = true;
            UiLayout.Stretch((RectTransform)overlay.transform);
            Ui.Icon(overlay.Content, IconId.ScreenRotation, 48f, Palette.NeonViolet, 0.8f);
            Ui.Text(overlay.Content, Loc.T("quienSoyPonteloEnLaFrente"), TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, bold: true);
            Ui.Text(overlay.Content, Loc.T(Flow.Options != null && _module.Config.Options.Inclinar ? "quienSoyComoInclinar" : "quienSoyComoBotones"),
                TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            _prepCount = Ui.FittedText(overlay.Content, string.Empty, 72f, Palette.NeonYellow, 1f);
            _prepOverlay = overlay.gameObject;

            _tilt.Invertir = _module.Config.Options.InvertirInclinacion;
            Refresh();
            _prep.Start(PrepSeconds);
        }

        public override void Tick(float deltaTime)
        {
            if (Dialogs.IsOpen) return;
            if (_flash > 0f)
            {
                _flash -= deltaTime;
                if (_flash <= 0f) _flashText.gameObject.SetActive(false);
            }
            var hasSensor = MotionSensor.TryRead(out var gravity);
            if (hasSensor && _frame != null) _frame.Angle = TiltDetector.Giro(gravity.x, _frame.Angle);
            if (_prep.Running)
            {
                _prep.Tick(deltaTime);
                return;
            }
            _timer.Tick(deltaTime);
            if (!hasSensor || !_timer.Running || !_module.Config.Options.Inclinar) return;
            switch (_tilt.Leer(gravity.z))
            {
                case TiltGesture.Abajo:
                    Correct();
                    break;
                case TiltGesture.Arriba:
                    Pass();
                    break;
            }
        }

        public override void OnHide()
        {
            _prep.Stop();
            _timer.Stop();
            MotionSensor.Release();
        }

        /// <summary>Se acaba la preparación: se toma la postura de referencia y empieza el tiempo.</summary>
        public void BeginGuessing()
        {
            if (_prepOverlay != null) _prepOverlay.SetActive(false);
            if (_prep.Running) _prep.Stop();
            _tilt.Calibrar(MotionSensor.TryRead(out var gravity) ? gravity.z : 0f);
            Host.Audio?.Play(AppSound.CardFlip, 0.7f);
            Haptics.Vibrate();
            _timer.Start(Flow.Options.Segundos);
        }

        public void Correct()
        {
            if (_prep.Running || Flow.Fase != GuessFase.Turno) return;
            Flow.Acierto();
            Host.Audio?.Play(AppSound.Correct, 0.7f);
            Haptics.Vibrate();
            Flash(Loc.T("quienSoyAcierto"), Palette.NeonGreen);
            Refresh();
        }

        public void Pass()
        {
            if (_prep.Running || !Flow.PuedePasar) return;
            Flow.Pasar();
            Host.Audio?.Play(AppSound.Wrong, 0.6f);
            Flash(Loc.T("quienSoyPaso"), Palette.NeonRed);
            Refresh();
        }

        public void TimeUp()
        {
            if (Flow.Fase != GuessFase.Turno) return;
            Host.Audio?.Play(AppSound.Buzzer);
            Haptics.Vibrate();
            Flow.TerminarTurno();
            Host.GoToPhase();
        }

        private void Flash(string text, Color color)
        {
            _flash = FlashSeconds;
            _flashText.text = text;
            _flashText.color = color;
            Ui.SetGlow((TextMeshProUGUI)_flashText, color, 1f);
            _flashText.gameObject.SetActive(true);
            _flashText.transform.SetAsLastSibling();
            if (_prepOverlay != null) _prepOverlay.transform.SetAsLastSibling();
        }

        private void Refresh()
        {
            var card = Flow.Current;
            if (card == null) return;
            _word.text = card.Text.ToUpper(Loc.Culture);
            _panel.name = Loc.T("cartaSemantica", ("carta", card.Text));
            _category.text = CategoryOption.LabelOf(card.Category, _module.CustomCategoriesOrEmpty());
            _panel.SetColors(WordCategories.IsAdult(card.Category) ? Palette.NeonMagenta : Palette.NeonCyan);
            _score.text = Loc.T("aciertosTurno", ("n", Flow.Aciertos));
            _pass.Interactable = Flow.PuedePasar;
            _pass.SetLabel(Flow.PasesRestantes > 0 ? Loc.T("pasarQuedan", ("n", Flow.PasesRestantes)) : Loc.T("pasar"));
            if (Flow.ConsumirAvisoMazo()) Snackbar.Show(Loc.T("tarjetasAgotadas"), Palette.NeonYellow);
        }

        private void ShowSeconds(int seconds)
        {
            if (_timerText == null) return;
            _timerText.text = seconds.ToString();
            _timerText.color = seconds > 10 ? Palette.NeonCyan : seconds > 5 ? Palette.NeonYellow : Palette.NeonRed;
            Ui.SetGlow((TextMeshProUGUI)_timerText, _timerText.color, 1f);
            _timerText.name = Loc.T("tiempoRestante", ("n", seconds));
            if (seconds <= 5 && seconds > 0 && _timer.Running) Host.Audio?.Play(AppSound.Beep, 0.8f);
        }
    }
}
