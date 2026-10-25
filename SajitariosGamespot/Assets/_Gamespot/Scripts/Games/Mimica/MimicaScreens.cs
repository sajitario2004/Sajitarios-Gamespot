using System;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Mimica.Domain;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Mimica
{
    /// <summary>
    /// Setup de Mímica (V074): 2–4 equipos, categorías (Cachondeo +18 con la confirmación de edad; también las propias),
    /// tiempo del turno, puntos para ganar, pases por turno (y si restan) y el modo (solo mímica o mezcla). La barra
    /// abre la gestión de tarjetas.
    /// </summary>
    public sealed class MimicaSetupScreen : AppScreen
    {
        private readonly MimicaModule _module;

        public MimicaSetupScreen(MimicaModule module) => _module = module;

        public PlayerListEditor Teams { get; private set; }
        public CategoryPicker Categories { get; private set; }

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("mimicaTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "mimicaTitulo", GameUi.RuleKeys("reglasMimica", 6));
            scaffold.AppBar.AddAction(IconId.MenuBook, () => Router.Go(MimicaModule.CardsRoute)).name = Loc.T("gestionarTarjetas");
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 8, 32));
            var draft = _module.Draft;

            var teams = SetupSection.Create(content, Loc.T("equipos"), Loc.T("mimicaEquiposAyuda"), Palette.NeonCyan);
            Teams = PlayerListEditor.Create(teams.Content, MimicaConfig.MinEquipos, MimicaConfig.MaxEquipos, Palette.NeonCyan, _module.DraftTeams, false,
                "equipoNumero", "nombreDelEquipoNumero", "anadirEquipo", "maximoEquiposAlcanzado", "quitarEquipo");
            Teams.NamesChanged += SaveNames;
            Teams.CountChanged += SaveNames;

            Categories = CategoryPicker.Create(content, _module.AvailableCategories(), _module.DraftCategories, Palette.NeonViolet,
                _module.KnownCategories, true);

            var turn = SetupSection.Create(content, Loc.T("turno"), null, Palette.NeonMagenta);
            Ui.Text(turn.Content, Loc.T("duracionTurno"), TextRole.LabelLarge, Palette.TextMuted);
            ChoiceChips<int>.Create(turn.Content, MimicaConfig.SegundosOptions.Select(n => (n, Loc.T("segundosCorto", ("n", n)))).ToList(), draft.Segundos,
                Palette.NeonMagenta, v => draft.Segundos = v);
            Ui.Text(turn.Content, Loc.T("puntosParaGanar"), TextRole.LabelLarge, Palette.TextMuted);
            ChoiceChips<int>.Create(turn.Content, MimicaConfig.ObjetivoOptions.Select(n => (n, n.ToString())).ToList(), draft.Objetivo,
                Palette.NeonMagenta, v => draft.Objetivo = v);
            Ui.Text(turn.Content, Loc.T("pasesPorTurno"), TextRole.LabelLarge, Palette.TextMuted);
            ChoiceChips<int>.Create(turn.Content, MimicaConfig.PasesOptions.Select(n => (n, PassesLabel(n))).ToList(), draft.Pases, Palette.NeonMagenta,
                v => draft.Pases = v);
            var penalty = NeonSwitch.Create(turn.Content, Loc.T("pasarResta"), Loc.T("pasarRestaAyuda"), draft.PenalizarPases);
            penalty.Changed += v => draft.PenalizarPases = v;

            var mode = SetupSection.Create(content, Loc.T("mimicaModo"), Loc.T("mimicaModoAyuda"), Palette.NeonViolet);
            ChoiceChips<MimicaModo>.Create(mode.Content, new[]
            {
                (MimicaModo.Mimica, Loc.T("mimicaModoMimica")),
                (MimicaModo.Mezcla, Loc.T("mimicaModoMezcla"))
            }, draft.Modo, Palette.NeonViolet, v => draft.Modo = v);

            Ui.Button(content, Loc.T("empezarPartida"), Start, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f);
        }

        public static string PassesLabel(int passes) =>
            passes == GuessGameOptions.SinLimite ? Loc.T("sinLimite") : passes == 0 ? Loc.T("ninguno") : passes.ToString();

        private void SaveNames()
        {
            _module.DraftTeams.Clear();
            _module.DraftTeams.AddRange(Teams.RawNames);
        }

        public void Start()
        {
            SaveNames();
            var (config, error) = MimicaConfig.Create(Teams.NormalizedNames, Categories.Selected, _module.Draft);
            if (error.HasValue)
            {
                Snackbar.Show(ErrorText(error.Value), Palette.NeonError);
                return;
            }
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

        public static string ErrorText(MimicaConfigError error)
        {
            switch (error)
            {
                case MimicaConfigError.PocosEquipos: return Loc.T("errorMinEquipos", ("n", MimicaConfig.MinEquipos));
                case MimicaConfigError.DemasiadosEquipos: return Loc.T("errorMaxEquipos", ("n", MimicaConfig.MaxEquipos));
                case MimicaConfigError.NombreVacio: return Loc.T("errorEquipoSinNombre");
                case MimicaConfigError.NombresRepetidos: return Loc.T("errorEquiposRepetidos");
                default: return Loc.T("errorSinCategorias");
            }
        }
    }

    /// <summary>
    /// El turno: cuenta atrás (cian, amarillo en los últimos 10 s, rojo en los 5 últimos, con pitidos), la categoría, el
    /// estilo en modo mezcla, la tarjeta en grande y Acierto / Pasar. Al acabar el tiempo suena la bocina y se pasa al
    /// resumen. El tiempo se para con los diálogos abiertos.
    /// </summary>
    public sealed class MimicaTurnScreen : GuessPlayScreen
    {
        private readonly MimicaModule _module;
        private readonly Countdown _countdown = new Countdown();
        private TMP_Text _timer;
        private TMP_Text _word;
        private TMP_Text _category;
        private TMP_Text _style;
        private TMP_Text _score;
        private NeonButton _pass;
        private NeonPanel _panel;

        public MimicaTurnScreen(MimicaModule module) : base(module.Host)
        {
            _module = module;
            _countdown.SecondChanged += ShowSeconds;
            _countdown.Finished += TimeUp;
        }

        protected override GuessFase Phase => GuessFase.Turno;

        public Countdown Countdown => _countdown;
        public string Word => _word != null ? _word.text : null;

        protected override string Subtitle() => TeamName;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var body = Ui.Column(scaffold.Body, 12f, new RectOffset(16, 16, 8, 16));
            body.Sized(flexHeight: 1f);
            _timer = Ui.FittedText(body, string.Empty, 56f, Palette.NeonCyan, 1f);
            _score = Ui.Text(body, string.Empty, TextRole.TitleMedium, Palette.NeonGreen, TextAlignmentOptions.Center, bold: true);
            Ui.Flex(body);
            _category = Ui.Text(body, string.Empty, TextRole.LabelLarge, Palette.NeonVioletText, TextAlignmentOptions.Center, maxLines: 1);
            _style = Ui.Text(body, string.Empty, TextRole.TitleLarge, Palette.NeonYellow, TextAlignmentOptions.Center, 0.8f, bold: true, maxLines: 1);
            _panel = Ui.Panel(body, Palette.NeonCyan, glow: 1f, padding: new RectOffset(16, 16, 28, 28), alignment: TextAnchor.MiddleCenter);
            _word = Ui.FittedText(_panel.Content, string.Empty, 44f, Palette.TextPrimary, 1f, minSize: 18f, lines: 3);
            Ui.Flex(body);
            var buttons = Ui.Row(body, 10f, alignment: TextAnchor.MiddleCenter);
            Ui.Button(buttons, Loc.T("acierto"), Correct, ButtonStyle.Filled, IconId.Check, Palette.NeonGreen, 0.7f).Sized(height: 72f, flexWidth: 1f);
            _pass = Ui.Button(buttons, Loc.T("pasar"), Pass, ButtonStyle.Outlined, IconId.SkipNext, Palette.NeonRed);
            _pass.Sized(height: 72f, flexWidth: 1f);
            _countdown.Start(Flow.Options.Segundos);
            Refresh();
        }

        public override void Tick(float deltaTime)
        {
            if (Dialogs.IsOpen) return;
            _countdown.Tick(deltaTime);
        }

        public override void OnHide() => _countdown.Stop();

        public void Correct()
        {
            Flow.Acierto();
            Host.Audio?.Play(AppSound.Correct, 0.6f);
            Refresh();
            FadeScaleIn.Play(_panel.gameObject, 0.2f, 0.95f);
        }

        public void Pass()
        {
            if (!Flow.PuedePasar) return;
            Flow.Pasar();
            Host.Audio?.Play(AppSound.Swipe, 0.6f);
            Refresh();
            FadeScaleIn.Play(_panel.gameObject, 0.2f, 0.95f);
        }

        /// <summary>Se acaba el tiempo (también lo usan los tests).</summary>
        public void TimeUp()
        {
            if (Flow.Fase != GuessFase.Turno) return;
            Host.Audio?.Play(AppSound.Buzzer);
            Haptics.Vibrate();
            Flow.TerminarTurno();
            Host.GoToPhase();
        }

        private void Refresh()
        {
            var card = Flow.Current;
            if (card == null) return;
            _word.text = card.Text.ToUpper(Loc.Culture);
            _panel.name = Loc.T("cartaSemantica", ("carta", card.Text));
            _category.text = CategoryOption.LabelOf(card.Category, _module.CustomCategoriesOrEmpty());
            var adult = WordCategories.IsAdult(card.Category);
            _panel.SetColors(adult ? Palette.NeonMagenta : Palette.NeonCyan);
            _style.gameObject.SetActive(Flow.CurrentTag >= 0);
            if (Flow.CurrentTag >= 0) _style.text = MimicaModule.StyleLabel((MimicaEstilo)Flow.CurrentTag).ToUpper(Loc.Culture);
            _score.text = Loc.T("aciertosTurno", ("n", Flow.Aciertos));
            _pass.Interactable = Flow.PuedePasar;
            _pass.SetLabel(Flow.PasesRestantes > 0 ? Loc.T("pasarQuedan", ("n", Flow.PasesRestantes)) : Loc.T("pasar"));
            if (Flow.ConsumirAvisoMazo()) Snackbar.Show(Loc.T("tarjetasAgotadas"), Palette.NeonYellow);
        }

        private void ShowSeconds(int seconds)
        {
            if (_timer == null) return;
            _timer.text = seconds.ToString();
            _timer.color = seconds > 10 ? Palette.NeonCyan : seconds > 5 ? Palette.NeonYellow : Palette.NeonRed;
            Ui.SetGlow((TextMeshProUGUI)_timer, _timer.color, 1f);
            _timer.name = Loc.T("tiempoRestante", ("n", seconds));
            if (seconds <= 5 && seconds > 0 && _countdown.Running) Host.Audio?.Play(AppSound.Beep, 0.8f);
        }
    }
}
