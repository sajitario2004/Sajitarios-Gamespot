using System.Collections.Generic;
using System.Linq;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.Trivia.Data;
using Gamespot.Games.Trivia.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Games.Trivia
{
    /// <summary>Estado compartido de las pantallas de la trivia.</summary>
    public sealed class TriviaModule
    {
        public const string SetupRoute = "/trivia";
        public const string PassRoute = "/trivia/pass";
        public const string QuestionRoute = "/trivia/question";
        public const string GameOverRoute = "/trivia/game-over";

        public TriviaModule(TriviaFlowController flow, QuestionRepository questions, WinnerRepository winners, Router router, AudioService audio)
        {
            Flow = flow;
            Questions = questions;
            Winners = winners;
            Router = router;
            Audio = audio;
        }

        public AudioService Audio { get; }

        public TriviaFlowController Flow { get; }
        public QuestionRepository Questions { get; }
        public WinnerRepository Winners { get; }
        public Router Router { get; }

        /// <summary>Borrador del setup (nombres y temáticas marcadas).</summary>
        public List<string> DraftNames { get; } = new List<string>();
        public HashSet<string> DraftThemes { get; } = new HashSet<string>(Tematicas.Default);

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

        /// <summary>Clave del nombre localizado de una temática.</summary>
        public static string ThemeKey(string id)
        {
            var parts = id.Split('_');
            return "triviaTematica" + string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
        }
    }

    /// <summary>
    /// Setup de la trivia (port de <c>TriviaSetupScreen</c>): 2–6 jugadores (violeta), temáticas en chips (por defecto
    /// cultura general e historia; la última marcada no se puede desmarcar), ranking de victorias y atribución OpenTDB.
    /// </summary>
    public sealed class TriviaSetupScreen : AppScreen
    {
        private readonly TriviaModule _module;
        private PlayerListEditor _players;
        private readonly Dictionary<string, NeonChip> _chips = new Dictionary<string, NeonChip>();

        public TriviaSetupScreen(TriviaModule module) => _module = module;

        public PlayerListEditor Players => _players;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("triviaTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "triviaTitulo", GameUi.RuleKeys("reglasTrivia", 5));
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 8, 32));

            var playersPanel = Ui.Panel(content, Palette.NeonViolet.WithAlpha(0.6f), glow: 0.3f, spacing: 10f);
            Ui.Text(playersPanel.Content, Loc.T("setupJugadores"), TextRole.TitleMedium, Palette.NeonVioletText, glow: 0.6f, bold: true);
            Ui.Text(playersPanel.Content, Loc.T("triviaSetupRangoJugadores", ("min", TriviaConfig.MinPlayers), ("max", TriviaConfig.MaxPlayers)),
                TextRole.BodySmall, Palette.TextMuted);
            _players = PlayerListEditor.Create(playersPanel.Content, TriviaConfig.MinPlayers, TriviaConfig.MaxPlayers, Palette.NeonViolet,
                _module.DraftNames);
            _players.NamesChanged += SaveDraft;
            _players.CountChanged += SaveDraft;

            var themesPanel = Ui.Panel(content, Palette.NeonCyan.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(themesPanel.Content, Loc.T("triviaSetupTematicas"), TextRole.TitleMedium, Palette.NeonCyan, glow: 0.6f, bold: true);
            Ui.Text(themesPanel.Content, Loc.T("triviaSetupTematicasAyuda"), TextRole.BodySmall, Palette.TextMuted);
            var wrap = new GameObject("Chips", typeof(RectTransform));
            wrap.transform.SetParent(themesPanel.Content, false);
            var grid = wrap.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(150f, 42f);
            grid.spacing = new Vector2(8f, 8f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            _chips.Clear();
            foreach (var id in Tematicas.All)
            {
                var themeId = id;
                var chip = NeonChip.Create(wrap.transform, Loc.T(TriviaModule.ThemeKey(themeId)), _module.DraftThemes.Contains(themeId), Palette.NeonViolet);
                chip.ToggleOnClick = false;
                chip.Clicked += () => ToggleTheme(themeId);
                _chips[themeId] = chip;
            }
            var fitter = wrap.AddComponent<ResponsiveChipGrid>();
            fitter.Columns = 2;

            BuildRanking(content);
            Ui.Text(content, Loc.T("triviaAtribucion"), TextRole.BodySmall, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Button(content, Loc.T("triviaEmpezarPartida"), Start, ButtonStyle.Filled, IconId.ArrowForward, Palette.NeonViolet, 0.8f);
        }

        /// <summary>Marca o desmarca una temática; la última marcada no se puede quitar.</summary>
        public void ToggleTheme(string id)
        {
            var themes = _module.DraftThemes;
            if (themes.Contains(id))
            {
                if (themes.Count == 1) return;
                themes.Remove(id);
            }
            else
            {
                themes.Add(id);
            }
            foreach (var pair in _chips) pair.Value.Selected = themes.Contains(pair.Key);
        }

        private void BuildRanking(RectTransform content)
        {
            List<WinnerRepository.Entry> ranking;
            try
            {
                ranking = _module.Winners.GetAllRanked();
            }
            catch (System.Exception)
            {
                return;
            }
            var panel = Ui.Panel(content, Palette.NeonViolet.WithAlpha(0.6f), spacing: 6f);
            Ui.Text(panel.Content, Loc.T("triviaRankingVictorias"), TextRole.TitleMedium, Palette.NeonVioletText, glow: 0.6f, bold: true);
            if (ranking.Count == 0) Ui.Text(panel.Content, Loc.T("triviaSinVictorias"), TextRole.BodyMedium, Palette.TextMuted);
            foreach (var entry in ranking)
            {
                var row = Ui.Row(panel.Content, 8f);
                Ui.Text(row, entry.Name, TextRole.BodyMedium, maxLines: 1).Sized(flexWidth: 1f);
                Ui.Text(row, Loc.T("triviaVictorias", ("n", entry.Wins)), TextRole.BodyMedium, Palette.NeonVioletText, TextAlignmentOptions.Right, bold: true);
            }
        }

        private void SaveDraft()
        {
            _module.DraftNames.Clear();
            _module.DraftNames.AddRange(_players.RawNames);
        }

        public void Start()
        {
            SaveDraft();
            var (config, error) = TriviaConfig.Create(_players.NormalizedNames, _module.DraftThemes);
            if (error.HasValue)
            {
                Snackbar.Show(ErrorText(error.Value), Palette.NeonError);
                return;
            }
            _players.Remember();
            _module.Flow.Iniciar(config);
            if (_module.Flow.Fase == TriviaFase.Pass)
            {
                Router.Go(TriviaModule.PassRoute);
                return;
            }
            Dialogs.Show(Loc.T("triviaNoHayPreguntasTitulo"), Loc.T("triviaNoHayPreguntasMensaje"), new[]
            {
                new DialogAction(Loc.T("aceptar"), (System.Action)null, ButtonStyle.Filled, Palette.NeonViolet)
            });
        }

        private static string ErrorText(TriviaConfigError error)
        {
            switch (error)
            {
                case TriviaConfigError.PocosJugadores: return Loc.T("errorPocosJugadores", ("min", TriviaConfig.MinPlayers));
                case TriviaConfigError.DemasiadosJugadores: return Loc.T("errorDemasiadosJugadores", ("max", TriviaConfig.MaxPlayers));
                case TriviaConfigError.NombreVacio: return Loc.T("errorNombreVacio");
                case TriviaConfigError.NombresDuplicados: return Loc.T("errorNombresDuplicados");
                default: return Loc.T("triviaSetupTematicasAyuda");
            }
        }
    }

    /// <summary>Rejilla de chips que reparte el ancho en N columnas.</summary>
    public sealed class ResponsiveChipGrid : MonoBehaviour
    {
        public int Columns = 2;
        private GridLayoutGroup _grid;
        private float _lastWidth = -1f;

        private void Awake() => _grid = GetComponent<GridLayoutGroup>();

        private void LateUpdate()
        {
            var width = ((RectTransform)transform).rect.width;
            if (_grid == null || width <= 0f || Mathf.Approximately(width, _lastWidth)) return;
            _lastWidth = width;
            _grid.cellSize = new Vector2((width - _grid.spacing.x * (Columns - 1)) / Columns, 44f);
        }
    }

    /// <summary>Base de las pantallas con partida en curso: casa y atrás piden confirmación.</summary>
    public abstract class TriviaPlayScreen : AppScreen
    {
        protected TriviaPlayScreen(TriviaModule module) => Module = module;

        protected TriviaModule Module { get; }

        public sealed override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("triviaTitulo"), Subtitle());
            GameUi.HomeButton(scaffold.AppBar, Router, () => Module.Flow.Reiniciar());
            if (Module.Flow.Session == null)
            {
                var column = Ui.Column(scaffold.Body, 16f, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter);
                column.Sized(flexHeight: 1f);
                Ui.Text(column, Loc.T("triviaNoHayPartida"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                Ui.Button(column, Loc.T("configurarPartida"), () => Router.Go(TriviaModule.SetupRoute));
                return;
            }
            BuildGame(scaffold);
        }

        protected virtual string Subtitle() => null;

        protected abstract void BuildGame(ScreenScaffold scaffold);

        public override bool OnBack()
        {
            if (Module.Flow.Session == null) return false;
            AbandonGameDialog.Show(Module.ResetToSetup);
            return true;
        }
    }

    /// <summary>"Pásale el móvil a…" en violeta.</summary>
    public sealed class TriviaPassScreen : TriviaPlayScreen
    {
        public TriviaPassScreen(TriviaModule module) : base(module)
        {
        }

        protected override void BuildGame(ScreenScaffold scaffold)
        {
            var flow = Module.Flow;
            var content = Ui.Scroll(scaffold.Body, 18f, new RectOffset(24, 24, 24, 24));
            Ui.Text(content, Loc.T("jugadorXDeY", ("posicion", flow.CurrentPlayerIndex + 1), ("total", flow.AlivePlayers.Count)), TextRole.LabelLarge,
                Palette.TextMuted, TextAlignmentOptions.Center);
            var row = Ui.Row(content, 0f, alignment: TextAnchor.MiddleCenter);
            var circle = Ui.Panel(row, Palette.NeonViolet, Palette.Surface, 0.9f, new RectOffset(28, 28, 28, 28), radius: 64f, alignment: TextAnchor.MiddleCenter);
            circle.Sized(128f, 128f);
            Ui.Icon(circle.Content, IconId.Quiz, 72f, Palette.NeonVioletText, 0.9f);
            PulseGlow.On(circle);
            Ui.Text(content, Loc.T("triviaPasaleElMovilA"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Text(content, flow.CurrentPlayer?.Name ?? string.Empty, TextRole.DisplayMedium, Palette.TextPrimary, TextAlignmentOptions.Center, 1f,
                Palette.NeonViolet, 2, true);
            Ui.Text(content, Loc.T("triviaPasaleElMovilAyuda"), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Button(content, Loc.T("continuar"), Continue, ButtonStyle.Filled, IconId.ArrowForward, Palette.NeonViolet, 0.8f).Sized(height: 64f);
        }

        public void Continue()
        {
            Module.Flow.PasarDispositivo();
            Router.Go(TriviaModule.QuestionRoute);
        }
    }

    /// <summary>
    /// Pregunta (port de <c>TriviaQuestionScreen</c>): enunciado en panel violeta y 4 respuestas en rejilla 2×2 con
    /// marcos azul, verde, rojo y amarillo. Al tocar una, la correcta se pone verde, la elegida errónea roja y el
    /// resto se atenúa; a los 800 ms responde y pasa al siguiente.
    /// </summary>
    public sealed class TriviaQuestionScreen : TriviaPlayScreen
    {
        public const float FeedbackSeconds = 0.8f;
        private static readonly string[] Letters = { "A", "B", "C", "D" };

        private readonly List<NeonPanel> _cells = new List<NeonPanel>();
        private readonly List<TMP_Text> _letters = new List<TMP_Text>();
        private readonly List<TMP_Text> _texts = new List<TMP_Text>();
        private int _chosen = -1;
        private float _wait;

        public TriviaQuestionScreen(TriviaModule module) : base(module)
        {
        }

        public bool Locked => _chosen >= 0;

        protected override string Subtitle() =>
            Loc.T("triviaRondaXDeY", ("actual", Module.Flow.CurrentRound + 1), ("total", TriviaSession.RoundCount));

        protected override void BuildGame(ScreenScaffold scaffold)
        {
            _cells.Clear();
            _letters.Clear();
            _texts.Clear();
            var question = Module.Flow.CurrentQuestion;
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 12, 24));
            Ui.Text(content, Module.Flow.CurrentPlayer?.Name ?? string.Empty, TextRole.TitleLarge, Palette.NeonVioletText, TextAlignmentOptions.Center, 0.8f,
                Palette.NeonViolet, 1, true);
            if (question == null)
            {
                Ui.Text(content, Loc.T("triviaNoHayPartida"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                return;
            }
            var panel = Ui.Panel(content, Palette.NeonViolet, glow: 0.9f, padding: new RectOffset(20, 20, 20, 20), borderWidth: 2f,
                alignment: TextAnchor.MiddleCenter);
            var statement = Ui.Text(panel.Content, question.Enunciado, TextRole.TitleMedium, alignment: TextAlignmentOptions.Center, bold: true);
            statement.lineSpacing = 16f;
            panel.name = $"{Loc.T("triviaPregunta")}: {question.Enunciado}";

            var gridGo = new GameObject("Answers", typeof(RectTransform));
            gridGo.transform.SetParent(content, false);
            var grid = gridGo.AddComponent<GridLayoutGroup>();
            grid.spacing = new Vector2(12f, 12f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            var responsive = gridGo.AddComponent<AnswerGridSizer>();
            responsive.Aspect = 1.3f;

            for (var i = 0; i < Question.OptionCount; i++)
            {
                var index = i;
                var color = Palette.AnswerFrameColors[i];
                var cell = NeonPanel.Create(gridGo.transform, color, Palette.Surface, 14f, 2f, 0.7f, padding: new RectOffset(10, 10, 10, 10),
                    spacing: 4f, alignment: TextAnchor.MiddleCenter, name: $"Answer {Letters[i]}");
                var hit = cell.gameObject.AddComponent<Image>();
                hit.color = Color.clear;
                var button = cell.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => Choose(index));
                _letters.Add(Ui.Text(cell.Content, Letters[i], TextRole.LabelLarge, color, TextAlignmentOptions.Center, bold: true));
                var text = Ui.Text(cell.Content, question.Options[i], TextRole.BodySmall, Palette.TextPrimary, TextAlignmentOptions.Center, maxLines: 3,
                    bold: true);
                _texts.Add(text);
                cell.name = Loc.T("triviaOpcionLetra", ("letra", Letters[i]), ("texto", question.Options[i]));
                _cells.Add(cell);
            }
        }

        /// <summary>Elige la respuesta <paramref name="index"/> (solo la primera vez).</summary>
        public void Choose(int index)
        {
            var question = Module.Flow.CurrentQuestion;
            if (Locked || question == null) return;
            _chosen = index;
            _wait = FeedbackSeconds;
            Module.Audio?.Play(question.IsCorrect(index) ? AppSound.Correct : AppSound.Wrong, 0.8f);
            for (var i = 0; i < _cells.Count; i++)
            {
                var frame = Palette.AnswerFrameColors[i];
                if (question.IsCorrect(i))
                {
                    _cells[i].SetColors(Palette.NeonGreen, Palette.AlphaBlend(Palette.NeonGreen.WithAlpha(0.18f), Palette.Surface));
                    _cells[i].GlowIntensity = 1.2f;
                }
                else if (i == index)
                {
                    _cells[i].SetColors(Palette.NeonRed, Palette.AlphaBlend(Palette.NeonRed.WithAlpha(0.18f), Palette.Surface));
                    _cells[i].GlowIntensity = 1.2f;
                }
                else
                {
                    _cells[i].SetColors(frame.WithAlpha(0.25f), Palette.Surface.WithAlpha(0.5f));
                    _cells[i].GlowIntensity = 0f;
                    _texts[i].color = Palette.TextMuted;
                    _letters[i].color = frame.WithAlpha(0.4f);
                }
            }
        }

        public override void Tick(float deltaTime)
        {
            if (!Locked || _wait <= 0f) return;
            _wait -= deltaTime;
            if (_wait > 0f) return;
            Module.Flow.Responder(_chosen);
            Router.Go(Module.Flow.Fase == TriviaFase.GameOver ? TriviaModule.GameOverRoute
                : Module.Flow.Fase == TriviaFase.Pass ? TriviaModule.PassRoute : TriviaModule.SetupRoute);
        }
    }

    /// <summary>Celdas de la rejilla 2×2 de respuestas: ancho repartido y proporción fija.</summary>
    public sealed class AnswerGridSizer : MonoBehaviour
    {
        public float Aspect = 1.3f;
        private float _lastWidth = -1f;

        private void LateUpdate()
        {
            var grid = GetComponent<GridLayoutGroup>();
            var width = ((RectTransform)transform).rect.width;
            if (grid == null || width <= 0f || Mathf.Approximately(width, _lastWidth)) return;
            _lastWidth = width;
            var cell = (width - grid.spacing.x) / 2f;
            grid.cellSize = new Vector2(cell, Mathf.Max(56f, cell / Aspect));
        }
    }

    /// <summary>Fin de la trivia: empate de supervivientes o "Nadie ganó esta partida".</summary>
    public sealed class TriviaGameOverScreen : AppScreen
    {
        private readonly TriviaModule _module;

        public TriviaGameOverScreen(TriviaModule module) => _module = module;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("triviaFinDePartida"));
            GameUi.HomeButton(scaffold.AppBar, Router).Clicked += () => _module.Flow.Reiniciar();
            var content = Ui.Scroll(scaffold.Body, 20f, new RectOffset(24, 24, 40, 24));
            var winners = _module.Flow.Winners;
            var won = winners.Count > 0;
            var accent = won ? Palette.NeonCyan : Palette.NeonMagenta;
            var panel = Ui.Panel(content, accent, glow: 1f, padding: new RectOffset(24, 24, 32, 32), spacing: 16f, alignment: TextAnchor.UpperCenter);
            PulseGlow.On(panel, 0.8f, 1.3f);
            Ui.Icon(panel.Content, won ? IconId.Trophy : IconId.SentimentDissatisfied, 72f, accent, 1f);
            Ui.Text(panel.Content, Loc.T(won ? "triviaGanadores" : "triviaNadieGano"), TextRole.HeadlineSmall, Palette.TextPrimary,
                TextAlignmentOptions.Center, 1f, accent, bold: true);
            if (won)
            {
                Ui.Text(panel.Content, Loc.T("triviaGanadoresList", ("nombres", string.Join(", ", winners.Select(w => w.Name)))), TextRole.TitleMedium,
                    Palette.TextPrimary, TextAlignmentOptions.Center);
            }
            Ui.Button(content, Loc.T("triviaJugarOtra"), _module.ResetToSetup, ButtonStyle.Filled, IconId.Refresh, Palette.NeonViolet, 0.8f);
            Ui.Button(content, Loc.T("volverAlMenu"), _module.ResetToMenu, ButtonStyle.Outlined, IconId.Home);
        }

        public override bool OnBack()
        {
            _module.ResetToSetup();
            return true;
        }
    }
}
