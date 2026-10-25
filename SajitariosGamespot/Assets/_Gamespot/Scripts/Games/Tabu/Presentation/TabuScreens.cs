using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.Tabu.Data;
using Gamespot.Games.Tabu.Domain;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Tabu
{
    /// <summary>Estado compartido de las pantallas del Tabú.</summary>
    public sealed class TabuModule
    {
        public const string SetupRoute = "/tabu";
        public const string TurnRoute = "/tabu/turn";
        public const string ScoreboardRoute = "/tabu/scoreboard";
        public const string GameOverRoute = "/tabu/game-over";

        public TabuModule(TabuFlowController flow, Router router, AudioService audio, TabuWordRepository words, CategoryTabuWordSource source)
        {
            Flow = flow;
            Router = router;
            Audio = audio;
            Words = words;
            WordSource = source;
        }

        public TabuWordRepository Words { get; }

        /// <summary>De dónde saca tarjetas la partida (con las categorías del setup).</summary>
        public CategoryTabuWordSource WordSource { get; }

        /// <summary>Categorías marcadas en el setup (por defecto todas menos Cachondeo, +18).</summary>
        public HashSet<string> DraftCategorias { get; } = new HashSet<string>(WordCategories.Default);

        public TabuFlowController Flow { get; }
        public Router Router { get; }
        public AudioService Audio { get; }
        public string DraftTeamA { get; set; } = string.Empty;
        public string DraftTeamB { get; set; } = string.Empty;
        public int DraftSeconds { get; set; } = TabuConfig.DefaultTurnSeconds;

        public void ResetToMenu()
        {
            Flow.Reiniciar();
            Router.Go(Router.Home);
        }

        /// <summary>"Cambiar ajustes": al setup con los equipos y el tiempo de antes.</summary>
        public void ResetToSetup()
        {
            Flow.Reiniciar();
            Router.Go(SetupRoute);
        }

        /// <summary>
        /// "Jugar otra" (C4): partida nueva al momento con la misma configuración. Si no se puede (p. ej. sin
        /// contenido), vuelve al setup, que conserva el borrador.
        /// </summary>
        public void PlayAgain()
        {
            var config = Flow.Config;
            if (config == null)
            {
                ResetToSetup();
                return;
            }
            Flow.Iniciar(config);
            if (Flow.Fase == TabuFase.Turno) Router.Go(TurnRoute);
            else ResetToSetup();
        }
    }

    /// <summary>
    /// Setup: nombres de los dos equipos, duración del turno (30/60/90 s, 60 por defecto) y categorías de las
    /// tarjetas (V059i). Objetivo fijo: 3.
    /// </summary>
    public sealed class TabuSetupScreen : AppScreen
    {
        private readonly TabuModule _module;
        private NeonTextField _teamA;
        private NeonTextField _teamB;
        private CategoryPicker _categories;
        private readonly System.Collections.Generic.Dictionary<int, NeonChip> _chips = new System.Collections.Generic.Dictionary<int, NeonChip>();

        public TabuSetupScreen(TabuModule module) => _module = module;

        public NeonTextField TeamA => _teamA;
        public NeonTextField TeamB => _teamB;
        public CategoryPicker Categories => _categories;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("tabuTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "tabuTitulo", GameUi.RuleKeys("reglasTabu", 5));
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 8, 32));

            var teams = Ui.Panel(content, Palette.NeonCyan.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(teams.Content, Loc.T("tabuSetupEquipos"), TextRole.TitleMedium, Palette.NeonCyan, glow: 0.6f, bold: true);
            _teamA = NeonTextField.Create(teams.Content, Loc.T("tabuEquipoA"), Loc.T("tabuEquipoAHint"), _module.DraftTeamA, characterLimit: 30);
            _teamA.Changed += v => _module.DraftTeamA = v;
            _teamB = NeonTextField.Create(teams.Content, Loc.T("tabuEquipoB"), Loc.T("tabuEquipoBHint"), _module.DraftTeamB, characterLimit: 30);
            _teamB.Changed += v => _module.DraftTeamB = v;

            var turn = Ui.Panel(content, Palette.NeonMagenta.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(turn.Content, Loc.T("tabuSetupTurno"), TextRole.TitleMedium, Palette.NeonMagenta, glow: 0.6f, bold: true);
            Ui.Text(turn.Content, Loc.T("tabuSetupTurnoAyuda"), TextRole.BodySmall, Palette.TextMuted);
            var row = Ui.Row(turn.Content, 8f, alignment: TextAnchor.MiddleCenter);
            _chips.Clear();
            foreach (var seconds in TabuConfig.TurnOptions)
            {
                var s = seconds;
                var chip = NeonChip.Create(row, Loc.T("tabuSegundos", ("n", s)), s == _module.DraftSeconds, Palette.NeonMagenta);
                chip.ToggleOnClick = false;
                chip.Clicked += () => SelectSeconds(s);
                chip.Sized(flexWidth: 1f);
                _chips[s] = chip;
            }

            _categories = CategoryPicker.Create(content, AvailableCategories(), _module.DraftCategorias, Palette.NeonCyan);

            Ui.Button(content, Loc.T("tabuEmpezarPartida"), Start, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f);
        }

        private IReadOnlyList<string> AvailableCategories()
        {
            try
            {
                var counts = _module.Words.CountByCategoria();
                return WordCategories.Seed.Where(id => counts.TryGetValue(id, out var n) && n > 0).ToList();
            }
            catch (System.Exception)
            {
                return WordCategories.Seed;
            }
        }

        public void SelectSeconds(int seconds)
        {
            _module.DraftSeconds = seconds;
            foreach (var pair in _chips) pair.Value.Selected = pair.Key == seconds;
        }

        public void Start()
        {
            var (config, error) = TabuConfig.Create(Core.NameNormalizer.Normalize(_teamA.Value), Core.NameNormalizer.Normalize(_teamB.Value),
                _module.DraftSeconds);
            if (error.HasValue)
            {
                Snackbar.Show(ErrorText(error.Value), Palette.NeonError);
                return;
            }
            _module.WordSource.Categorias = _categories.Selected;
            _module.Flow.Iniciar(config);
            if (_module.Flow.Fase == TabuFase.Turno)
            {
                Router.Go(TabuModule.TurnRoute);
                return;
            }
            Dialogs.Show(Loc.T("tabuSinPalabrasTitulo"), Loc.T("tabuSinPalabrasMensaje"), new[]
            {
                new DialogAction(Loc.T("aceptar"), (System.Action)null, ButtonStyle.Filled, Palette.NeonCyan)
            });
        }

        private static string ErrorText(TabuConfigError error)
        {
            switch (error)
            {
                case TabuConfigError.EquipoAVacio: return Loc.T("tabuErrorEquipoAVacio");
                case TabuConfigError.EquipoBVacio: return Loc.T("tabuErrorEquipoBVacio");
                case TabuConfigError.EquiposDuplicados: return Loc.T("tabuErrorEquiposDuplicados");
                case TabuConfigError.TurnoSegundosInvalido: return Loc.T("tabuErrorTurnoInvalido");
                default: return Loc.T("errorNoSePudoIniciar");
            }
        }
    }

    /// <summary>Base con partida en curso: casa y atrás confirman y vuelven al menú.</summary>
    public abstract class TabuPlayScreen : AppScreen
    {
        protected TabuPlayScreen(TabuModule module) => Module = module;

        protected TabuModule Module { get; }

        protected ScreenScaffold BuildScaffold(RectTransform root, string title, string subtitle = null)
        {
            var scaffold = ScreenScaffold.Create(root, title, subtitle);
            GameUi.HomeButton(scaffold.AppBar, Router, () => Module.Flow.Reiniciar());
            return scaffold;
        }

        protected bool BuildNoGameIfNeeded(RectTransform body)
        {
            if (Module.Flow.Session != null) return false;
            var column = Ui.Column(body, 16f, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter);
            column.Sized(flexHeight: 1f);
            Ui.Text(column, Loc.T("tabuNoHayPartida"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
            Ui.Button(column, Loc.T("configurarPartida"), () => Router.Go(TabuModule.SetupRoute));
            return true;
        }

        public override bool OnBack()
        {
            if (Module.Flow.Session == null) return false;
            AbandonGameDialog.Show(Module.ResetToMenu);
            return true;
        }
    }

    /// <summary>
    /// Turno (port de <c>TabuTurnScreen</c>): palabra en mayúsculas, prohibidas, equipo en violeta, cuenta atrás
    /// (cian &gt; 10, amarillo 6–10, rojo ≤ 5) y Acierto / Saltar / Falta. El temporizador vive aquí y se PAUSA
    /// mientras hay un diálogo abierto (en el Flutter seguía corriendo detrás del diálogo de salir).
    /// Sonido (V059d): un pitido en cada uno de los últimos 5 segundos y bocina al acabarse.
    /// </summary>
    public sealed class TabuTurnScreen : TabuPlayScreen
    {
        /// <summary>Desde qué segundo suena el pitido de aviso.</summary>
        public const int BeepSeconds = 5;

        private float _remaining;
        private TMP_Text _word;
        private RectTransform _forbidden;
        private TMP_Text _countdown;
        private TMP_Text _correct;

        public TabuTurnScreen(TabuModule module) : base(module)
        {
        }

        public float Remaining => _remaining;
        public int SecondsShown => Mathf.CeilToInt(_remaining);

        public override void Build(RectTransform root)
        {
            var session = Module.Flow.Session;
            var scaffold = BuildScaffold(root, Loc.T("tabuTitulo"), session != null ? Loc.T("tabuAciertosContador", ("n", session.AciertosTurno)) : null);
            if (BuildNoGameIfNeeded(scaffold.Body)) return;
            if (_remaining <= 0f) _remaining = Module.Flow.TurnDurationSeconds;
            _correct = scaffold.AppBar.Subtitle;

            var body = Ui.Column(scaffold.Body, 12f, new RectOffset(16, 16, 8, 16));
            body.Sized(flexHeight: 1f);
            Ui.Text(body, Module.Flow.CurrentTeamName, TextRole.TitleLarge, Palette.NeonVioletText, TextAlignmentOptions.Center, 0.8f, Palette.NeonViolet,
                1, true);
            _countdown = Ui.FittedText(body, string.Empty, 56f, Palette.NeonCyan, 1f);

            var wordPanel = Ui.Panel(body, Palette.NeonCyan, glow: 1f, padding: new RectOffset(16, 16, 20, 20), alignment: TextAnchor.MiddleCenter);
            _word = Ui.FittedText(wordPanel.Content, string.Empty, 40f, Palette.TextPrimary, 1f, minSize: 18f, lines: 2);

            var forbiddenPanel = Ui.Panel(body, Palette.NeonError.WithAlpha(0.7f), padding: new RectOffset(16, 16, 12, 12), spacing: 6f,
                alignment: TextAnchor.UpperCenter);
            Ui.Text(forbiddenPanel.Content, Loc.T("tabuProhibidas"), TextRole.LabelLarge, Palette.NeonError, TextAlignmentOptions.Center, bold: true);
            _forbidden = Ui.Column(forbiddenPanel.Content, 4f, name: "Forbidden");
            Ui.Flex(body);

            var buttons = Ui.Row(body, 8f, alignment: TextAnchor.MiddleCenter);
            Ui.Button(buttons, Loc.T("tabuAcierto"), Correct, ButtonStyle.Filled, null, Palette.NeonGreen, 0.6f).Compact().Sized(flexWidth: 1f).name =
                Loc.T("tabuAciertoHint");
            Ui.Button(buttons, Loc.T("tabuSaltar"), Skip, ButtonStyle.Outlined, null, Palette.NeonCyan).Compact().Sized(flexWidth: 1f).name =
                Loc.T("tabuSaltarHint");
            Ui.Button(buttons, Loc.T("tabuFalta"), Foul, ButtonStyle.Outlined, null, Palette.NeonRed).Compact().Sized(flexWidth: 1f).name =
                Loc.T("tabuFaltaHint");
            Refresh();
        }

        public void Correct() => Do(Module.Flow.Acierto);
        public void Skip() => Do(Module.Flow.Saltar);
        public void Foul() => Do(Module.Flow.Falta);

        private void Do(System.Action action)
        {
            action();
            Refresh();
        }

        public override void Tick(float deltaTime)
        {
            if (Module.Flow.Fase != TabuFase.Turno || Module.Flow.Session == null || Dialogs.IsOpen) return;
            var before = SecondsShown;
            _remaining -= deltaTime;
            if (_remaining <= 0f)
            {
                _remaining = 0f;
                Module.Audio?.Play(AppSound.Buzzer);
                Module.Flow.TerminarTurno();
                Router.Go(Module.Flow.Fase == TabuFase.GameOver ? TabuModule.GameOverRoute : TabuModule.ScoreboardRoute);
                return;
            }
            var now = SecondsShown;
            if (now < before && now <= BeepSeconds) Module.Audio?.Play(AppSound.Beep, 0.8f);
            UpdateCountdown();
        }

        private void Refresh()
        {
            var session = Module.Flow.Session;
            if (session == null) return;
            _word.text = session.PalabraActual.Palabra.ToUpper(Loc.Culture);
            _forbidden.ClearChildren();
            foreach (var word in session.PalabraActual.Prohibidas)
            {
                Ui.Text(_forbidden, word, TextRole.TitleMedium, Palette.TextPrimary, TextAlignmentOptions.Center, maxLines: 1);
            }
            if (Module.Flow.ConsumirAvisoPalabras()) Snackbar.Show(Loc.T("tabuPalabrasAgotadas"), Palette.NeonYellow);
            if (_correct != null)
            {
                _correct.text = Loc.T("tabuAciertosContador", ("n", session.AciertosTurno));
                _correct.gameObject.SetActive(true);
            }
            UpdateCountdown();
        }

        private void UpdateCountdown()
        {
            var seconds = SecondsShown;
            _countdown.text = seconds.ToString();
            _countdown.color = seconds > 10 ? Palette.NeonCyan : seconds > 5 ? Palette.NeonYellow : Palette.NeonRed;
            Ui.SetGlow((TextMeshProUGUI)_countdown, _countdown.color, 1f);
            _countdown.name = Loc.T("tabuTiempoRestante", ("n", seconds));
        }
    }

    /// <summary>Marcador entre turnos: "A: x / 3" (cian) y "B: y / 3" (violeta) y "Siguiente turno".</summary>
    public sealed class TabuScoreboardScreen : TabuPlayScreen
    {
        public TabuScoreboardScreen(TabuModule module) : base(module)
        {
        }

        public override void Build(RectTransform root)
        {
            var scaffold = BuildScaffold(root, Loc.T("tabuMarcador"));
            if (BuildNoGameIfNeeded(scaffold.Body)) return;
            var session = Module.Flow.Session;
            var config = Module.Flow.Config;
            var content = Ui.Scroll(scaffold.Body, 18f, new RectOffset(24, 24, 32, 24));
            Ui.Text(content, Loc.T("tabuObjetivoVictorias", ("n", config.ObjetivoVictorias)), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            ScoreCard(content, config.EquipoA, session.VictoriasA, config.ObjetivoVictorias, Palette.NeonCyan);
            ScoreCard(content, config.EquipoB, session.VictoriasB, config.ObjetivoVictorias, Palette.NeonViolet);
            Ui.Button(content, Loc.T("tabuSiguienteTurno"), Next, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f);
        }

        public void Next()
        {
            Module.Flow.SiguienteTurno();
            Router.Go(TabuModule.TurnRoute);
        }

        internal static void ScoreCard(RectTransform parent, string team, int wins, int target, Color color)
        {
            var panel = Ui.Panel(parent, color, glow: 0.7f, padding: new RectOffset(20, 20, 16, 16));
            var row = Ui.Row(panel.Content, 10f);
            Ui.Text(row, team, TextRole.TitleLarge, Palette.TextPrimary, maxLines: 1, bold: true).Sized(flexWidth: 1f);
            Ui.Text(row, $"{wins} / {target}", TextRole.HeadlineSmall, color, TextAlignmentOptions.Right, 0.8f, bold: true);
        }
    }

    /// <summary>Fin: equipo ganador, marcador final, "Jugar otra" (mismos equipos y tiempo), "Cambiar ajustes" y menú.</summary>
    public sealed class TabuGameOverScreen : TabuPlayScreen
    {
        public TabuGameOverScreen(TabuModule module) : base(module)
        {
        }

        public override void Build(RectTransform root)
        {
            var scaffold = BuildScaffold(root, Loc.T("tabuFinDePartida"));
            if (BuildNoGameIfNeeded(scaffold.Body)) return;
            var session = Module.Flow.Session;
            var config = Module.Flow.Config;
            var content = Ui.Scroll(scaffold.Body, 18f, new RectOffset(24, 24, 32, 24));
            var panel = Ui.Panel(content, Palette.NeonCyan, glow: 1f, padding: new RectOffset(24, 24, 28, 28), spacing: 12f, alignment: TextAnchor.UpperCenter);
            PulseGlow.On(panel, 0.8f, 1.3f);
            Ui.Icon(panel.Content, IconId.Trophy, 72f, Palette.NeonCyan, 1f);
            Ui.Text(panel.Content, Loc.T("tabuGanadorLabel"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.FittedText(panel.Content, session.Ganador.HasValue ? config.NameOf(session.Ganador.Value) : "—", 40f, Palette.TextPrimary, 1f);
            TabuScoreboardScreen.ScoreCard(content, config.EquipoA, session.VictoriasA, config.ObjetivoVictorias, Palette.NeonCyan);
            TabuScoreboardScreen.ScoreCard(content, config.EquipoB, session.VictoriasB, config.ObjetivoVictorias, Palette.NeonViolet);
            Ui.Button(content, Loc.T("jugarOtra"), Module.PlayAgain, ButtonStyle.Filled, IconId.Refresh, glow: 0.8f);
            Ui.Button(content, Loc.T("cambiarAjustes"), Module.ResetToSetup, ButtonStyle.Outlined, IconId.Tune);
            Ui.Button(content, Loc.T("volverAlMenu"), Module.ResetToMenu, ButtonStyle.Text, IconId.Home);
        }

        public override bool OnBack()
        {
            Module.ResetToMenu();
            return true;
        }
    }
}
