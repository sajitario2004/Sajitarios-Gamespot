using System.Collections.Generic;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Bomba.Domain;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Bomba
{
    /// <summary>Estado compartido de las pantallas de La Bomba.</summary>
    public sealed class BombaModule
    {
        public const string SetupRoute = "/bomba";
        public const string PlayRoute = "/bomba/play";
        public const string GameOverRoute = "/bomba/game-over";

        public BombaModule(BombaFlowController flow, Router router, AudioService audio)
        {
            Flow = flow;
            Router = router;
            Audio = audio;
        }

        public BombaFlowController Flow { get; }
        public Router Router { get; }
        public AudioService Audio { get; }
        public BombaMode DraftMode { get; set; } = BombaMode.Silaba;
        public List<string> DraftNames { get; } = new List<string>();

        public void ResetToMenu()
        {
            Flow.Reiniciar();
            Router.Go(Router.Home);
        }

        /// <summary>Título de la barra con el halo rojo del juego (como el Flutter).</summary>
        public static ScreenScaffold Scaffold(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("bombaTitulo"));
            Ui.SetGlow(scaffold.AppBar.Title, Palette.NeonError, 0.8f);
            return scaffold;
        }
    }

    /// <summary>
    /// Setup: modo Sílaba (por defecto) o Categoría y 2–12 jugadores en orden de juego. La mecha siempre va de
    /// 10 a 60 s. Los nombres se normalizan como en el resto de juegos (en el Flutter solo se recortaban).
    /// </summary>
    public sealed class BombaSetupScreen : AppScreen
    {
        private readonly BombaModule _module;
        private readonly Dictionary<BombaMode, NeonChip> _chips = new Dictionary<BombaMode, NeonChip>();
        private PlayerListEditor _players;

        public BombaSetupScreen(BombaModule module) => _module = module;

        public PlayerListEditor Players => _players;

        public override void Build(RectTransform root)
        {
            var scaffold = BombaModule.Scaffold(root);
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "bombaTitulo", GameUi.RuleKeys("reglasBomba", 5));
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 8, 32));

            var modePanel = Ui.Panel(content, Palette.NeonError.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(modePanel.Content, Loc.T("bombaSetupModo"), TextRole.TitleMedium, Palette.NeonError, glow: 0.6f, bold: true);
            var row = Ui.Row(modePanel.Content, 10f, alignment: TextAnchor.MiddleCenter);
            _chips.Clear();
            AddModeChip(row, BombaMode.Silaba, "bombaModoSilaba");
            AddModeChip(row, BombaMode.Categoria, "bombaModoCategoria");

            var playersPanel = Ui.Panel(content, Palette.NeonCyan.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(playersPanel.Content, Loc.T("setupJugadores"), TextRole.TitleMedium, Palette.NeonCyan, glow: 0.6f, bold: true);
            Ui.Text(playersPanel.Content, Loc.T("bombaSetupRangoJugadores", ("min", BombaConfig.MinPlayers), ("max", BombaConfig.MaxPlayers)),
                TextRole.BodySmall, Palette.TextMuted);
            _players = PlayerListEditor.Create(playersPanel.Content, BombaConfig.MinPlayers, BombaConfig.MaxPlayers, Palette.NeonCyan, _module.DraftNames);
            _players.NamesChanged += SaveDraft;
            _players.CountChanged += SaveDraft;

            Ui.Button(content, Loc.T("bombaEmpezarPartida"), Start, ButtonStyle.Filled, IconId.Fire, Palette.NeonError, 0.8f);
        }

        private void AddModeChip(Transform row, BombaMode mode, string labelKey)
        {
            var chip = NeonChip.Create(row, Loc.T(labelKey), mode == _module.DraftMode, Palette.NeonError);
            chip.ToggleOnClick = false;
            chip.Clicked += () => SelectMode(mode);
            chip.Sized(flexWidth: 1f);
            _chips[mode] = chip;
        }

        public void SelectMode(BombaMode mode)
        {
            _module.DraftMode = mode;
            foreach (var pair in _chips) pair.Value.Selected = pair.Key == mode;
        }

        private void SaveDraft()
        {
            _module.DraftNames.Clear();
            _module.DraftNames.AddRange(_players.RawNames);
        }

        public void Start()
        {
            SaveDraft();
            var (config, error) = BombaConfig.Create(_module.DraftMode, _players.NormalizedNames);
            if (error.HasValue)
            {
                Snackbar.Show(ErrorText(error.Value), Palette.NeonError);
                return;
            }
            _players.Remember();
            _module.Flow.Iniciar(config);
            if (_module.Flow.Fase == BombaFase.Jugando)
            {
                Router.Go(BombaModule.PlayRoute);
                return;
            }
            Dialogs.Show(Loc.T("bombaSinPromptsTitulo"), Loc.T("bombaSinPromptsMensaje"), new[]
            {
                new DialogAction(Loc.T("aceptar"), (System.Action)null, ButtonStyle.Filled, Palette.NeonCyan)
            });
        }

        private static string ErrorText(BombaConfigError error)
        {
            switch (error)
            {
                case BombaConfigError.PocosJugadores: return Loc.T("errorPocosJugadores", ("min", BombaConfig.MinPlayers));
                case BombaConfigError.DemasiadosJugadores: return Loc.T("errorDemasiadosJugadores", ("max", BombaConfig.MaxPlayers));
                case BombaConfigError.NombreVacio: return Loc.T("errorNombreVacio");
                case BombaConfigError.NombresDuplicados: return Loc.T("errorNombresDuplicados");
                default: return Loc.T("errorNoSePudoIniciar");
            }
        }
    }

    /// <summary>
    /// Juego (port de <c>BombaPlayScreen</c>): quién tiene la bomba, la sílaba o categoría en un panel rojo que
    /// late y "PASAR". La mecha es oculta (sin cuenta atrás) y se cuenta aquí, frame a frame; al acabarse sale
    /// "¡BOOM!" con el eliminado durante 1,5 s y sigue sola. Las dos esperas se PAUSAN mientras hay un diálogo
    /// abierto (en el Flutter la bomba podía explotar detrás del diálogo de salir).
    /// Sonido (V059d, L1): tic-tac que acelera según se consume la mecha, y explosión con vibración.
    /// </summary>
    public sealed class BombaPlayScreen : AppScreen
    {
        public const float ExplosionSeconds = 1.5f;
        public const float SlowestTick = 0.9f;
        public const float FastestTick = 0.18f;

        private readonly BombaModule _module;
        private float _elapsed;
        private float _explosionLeft;
        private float _nextTick;
        private bool _tock;
        private ScreenScaffold _scaffold;
        private RectTransform _overlay;
        private TMP_Text _holder;
        private TMP_Text _prompt;
        private TMP_Text _alive;
        private TMP_Text _eliminated;

        public BombaPlayScreen(BombaModule module) => _module = module;

        /// <summary>Segundos de mecha consumidos en la ronda actual.</summary>
        public float Elapsed => _elapsed;
        public bool ExplosionVisible => _overlay != null && _overlay.gameObject.activeSelf;
        public string HolderText => _holder != null ? _holder.text : null;
        public string PromptText => _prompt != null ? _prompt.text : null;
        public string EliminatedText => _eliminated != null ? _eliminated.text : null;

        /// <summary>Segundos entre tic y tac según lo consumido de la mecha (0..1): de 0,9 s a 0,18 s.</summary>
        public static float TickInterval(float progress) => Mathf.Lerp(SlowestTick, FastestTick, Mathf.Clamp01(progress));

        public override void Build(RectTransform root)
        {
            var flow = _module.Flow;
            _scaffold = BombaModule.Scaffold(root);
            GameUi.HomeButton(_scaffold.AppBar, Router, () => flow.Reiniciar());
            if (flow.Session == null || flow.CurrentPrompt == null)
            {
                var empty = Ui.Column(_scaffold.Body, 16f, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter);
                empty.Sized(flexHeight: 1f);
                Ui.Text(empty, Loc.T("bombaNoHayPartida"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                Ui.Button(empty, Loc.T("configurarPartida"), () => Router.Go(BombaModule.SetupRoute));
                return;
            }
            _alive = _scaffold.AppBar.AddInfo(IconId.Group, string.Empty, Palette.NeonCyan);

            var body = Ui.Column(_scaffold.Body, 0f, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter);
            body.Sized(flexHeight: 1f);
            Ui.Flex(body);
            _holder = Ui.Text(body, string.Empty, TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, 1f, Palette.NeonCyan, 1, true);
            Ui.Space(body, 32f);
            var panel = Ui.Panel(body, Palette.NeonError, glow: 1f, padding: new RectOffset(32, 32, 24, 24), alignment: TextAnchor.MiddleCenter);
            PulseGlow.On(panel, 0.5f, 1.2f);
            _prompt = Ui.FittedText(panel.Content, string.Empty, 56f, Palette.TextPrimary, 1f);
            Ui.SetGlow((TextMeshProUGUI)_prompt, Palette.NeonError, 1f);
            Ui.Space(body, 16f);
            Ui.Text(body, Loc.T(flow.Config.Mode == BombaMode.Silaba ? "bombaTipoSilaba" : "bombaTipoCategoria"), TextRole.BodyMedium, Palette.TextMuted,
                TextAlignmentOptions.Center);
            Ui.Flex(body);
            Ui.Button(body, Loc.T("bombaPasar"), Pass, ButtonStyle.Filled, IconId.SkipNext, Palette.NeonCyan, 0.8f).Sized(height: 64f);

            BuildOverlay(root);
            Refresh();
            if (flow.Fase == BombaFase.Explotando) ShowExplosion();
        }

        private void BuildOverlay(RectTransform root)
        {
            _overlay = Ui.Column(root, 16f, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter, name: "Explosion");
            UiLayout.Stretch(_overlay);
            Ui.Icon(_overlay, IconId.Fire, 80f, Palette.NeonError, 1f);
            Ui.Text(_overlay, Loc.T("bombaExplosionTitulo"), TextRole.HeadlineMedium, Palette.TextPrimary, TextAlignmentOptions.Center, 1f,
                Palette.NeonError, bold: true);
            var panel = Ui.Panel(_overlay, Palette.NeonError, glow: 1f, padding: new RectOffset(20, 20, 20, 20), alignment: TextAnchor.MiddleCenter);
            _eliminated = Ui.FittedText(panel.Content, string.Empty, 44f, Palette.TextPrimary, 1f);
            Ui.SetGlow((TextMeshProUGUI)_eliminated, Palette.NeonError, 1f);
            Ui.Text(_overlay, Loc.T("bombaEliminado"), TextRole.BodyLarge, Palette.TextMuted, TextAlignmentOptions.Center);
            _overlay.gameObject.SetActive(false);
        }

        public void Pass()
        {
            _module.Flow.Pasar();
            Refresh();
        }

        public override void Tick(float deltaTime)
        {
            if (Dialogs.IsOpen) return;
            var flow = _module.Flow;
            if (flow.Fase == BombaFase.Jugando && flow.Session != null)
            {
                _elapsed += deltaTime;
                if (_elapsed >= flow.FuseSeconds)
                {
                    Explode();
                    return;
                }
                _nextTick -= deltaTime;
                if (_nextTick <= 0f)
                {
                    _module.Audio?.Play(_tock ? AppSound.Tock : AppSound.Tick, 0.7f);
                    _tock = !_tock;
                    _nextTick = TickInterval(_elapsed / (float)flow.FuseSeconds);
                }
            }
            else if (flow.Fase == BombaFase.Explotando && _overlay != null)
            {
                _explosionLeft -= deltaTime;
                if (_explosionLeft <= 0f) ContinueAfterExplosion();
            }
        }

        public override bool OnBack()
        {
            if (_module.Flow.Session == null) return false;
            AbandonGameDialog.Show(_module.ResetToMenu);
            return true;
        }

        private void Explode()
        {
            _module.Flow.Explotar();
            if (_module.Flow.Fase != BombaFase.Explotando) return;
            _explosionLeft = ExplosionSeconds;
            _module.Audio?.Play(AppSound.Explosion);
            Haptics.Vibrate();
            ShowExplosion();
        }

        private void ShowExplosion()
        {
            _eliminated.text = _module.Flow.Eliminado ?? string.Empty;
            _eliminated.name = _eliminated.text;
            _scaffold.Root.gameObject.SetActive(false);
            _overlay.gameObject.SetActive(true);
            FadeScaleIn.Play(_overlay.gameObject, 0.3f, 0.85f);
            if (_explosionLeft <= 0f) _explosionLeft = ExplosionSeconds;
        }

        private void ContinueAfterExplosion()
        {
            _module.Flow.ContinuarTrasExplosion();
            if (_module.Flow.Fase == BombaFase.GameOver)
            {
                Router.Go(BombaModule.GameOverRoute);
                return;
            }
            _elapsed = 0f;
            _nextTick = 0f;
            _overlay.gameObject.SetActive(false);
            _scaffold.Root.gameObject.SetActive(true);
            Refresh();
        }

        private void Refresh()
        {
            var flow = _module.Flow;
            if (flow.Session == null || flow.CurrentPrompt == null) return;
            _holder.text = flow.CurrentHolder.ToUpper(Loc.Culture);
            _holder.name = Loc.T("bombaPortadorSemantica", ("nombre", flow.CurrentHolder));
            _prompt.text = flow.CurrentPrompt.Texto.ToUpper(Loc.Culture);
            _prompt.name = Loc.T("bombaPromptSemantica", ("texto", flow.CurrentPrompt.Texto));
            _alive.text = flow.AlivePlayers.Count.ToString();
        }
    }

    /// <summary>Fin: trofeo, "Fin de la partida" y el ganador. Casa, atrás y "Volver al menú" reinician.</summary>
    public sealed class BombaGameOverScreen : AppScreen
    {
        private readonly BombaModule _module;

        public BombaGameOverScreen(BombaModule module) => _module = module;

        public override void Build(RectTransform root)
        {
            var scaffold = BombaModule.Scaffold(root);
            scaffold.AppBar.SetLeading(IconId.Home, _module.ResetToMenu).name = Loc.T("volverAlMenu");
            var winner = _module.Flow.Winner;
            if (winner == null)
            {
                var empty = Ui.Column(scaffold.Body, 16f, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter);
                empty.Sized(flexHeight: 1f);
                Ui.Text(empty, Loc.T("bombaNoHayPartida"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                Ui.Button(empty, Loc.T("configurarPartida"), () => Router.Go(BombaModule.SetupRoute));
                return;
            }
            var content = Ui.Scroll(scaffold.Body, 18f, new RectOffset(24, 24, 40, 24));
            var header = Ui.Column(content, 12f, alignment: TextAnchor.UpperCenter);
            Ui.Icon(header, IconId.Trophy, 64f, Palette.NeonCyan, 1f);
            Ui.Text(header, Loc.T("bombaFinDePartida"), TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, 0.8f, Palette.NeonCyan);
            var panel = Ui.Panel(content, Palette.NeonCyan, glow: 1f, padding: new RectOffset(24, 24, 24, 24), spacing: 8f, alignment: TextAnchor.UpperCenter);
            PulseGlow.On(panel, 0.8f, 1.3f);
            Ui.Text(panel.Content, Loc.T("bombaGanadorLabel"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.FittedText(panel.Content, winner, 44f, Palette.TextPrimary, 1f).name = winner;
            Ui.Space(content, 16f);
            Ui.Button(content, Loc.T("volverAlMenu"), _module.ResetToMenu, ButtonStyle.Filled, IconId.Home, glow: 0.8f);
        }

        public override bool OnBack()
        {
            _module.ResetToMenu();
            return true;
        }
    }
}
