using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.MasProbable.Domain;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.MasProbable
{
    /// <summary>
    /// Setup (V072): nivel (picante con la confirmación de edad), origen de las frases si hay propias, cómo se juega
    /// (todos señalan / llevar la cuenta, que pide 3–20 nombres), cuenta atrás (sin, 3 o 5 s) y frases por partida.
    /// </summary>
    public sealed class MasProbableSetupScreen : AppScreen
    {
        private readonly MasProbableModule _module;
        private GameObject _playersPanel;

        public MasProbableSetupScreen(MasProbableModule module) => _module = module;

        public PlayerListEditor Players { get; private set; }
        public PromptLevelPicker Levels { get; private set; }
        public PromptOriginPicker Origin { get; private set; }
        public ChoiceChips<MasProbableModo> Mode { get; private set; }

        /// <summary>Si se ve la lista de jugadores (solo llevando la cuenta).</summary>
        public bool PlayersVisible => _playersPanel != null && _playersPanel.activeSelf;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("masProbableTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "masProbableTitulo", GameUi.RuleKeys("reglasMasProbable", 5));
            scaffold.AppBar.AddAction(IconId.MenuBook, () => Router.Go(MasProbableModule.CardsRoute)).name = Loc.T("gestionarFrases");
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 8, 32));
            var draft = _module.Draft;

            Levels = PromptLevelPicker.Create(content, _module.DraftLevels);
            Origin = PromptOriginPicker.Create(content, _module.OwnCountOrZero(), draft.IncluirSerie, draft.IncluirPropias, (seed, own) =>
            {
                draft.IncluirSerie = seed;
                draft.IncluirPropias = own;
            }, "origenTituloFrases", "origenAyudaFrases");

            var mode = SetupSection.Create(content, Loc.T("masProbableComoSeJuega"), Loc.T("masProbableModoAyuda"), Palette.NeonViolet);
            Mode = ChoiceChips<MasProbableModo>.Create(mode.Content, new[]
            {
                (MasProbableModo.Senalar, Loc.T("masProbableModoSenalar")),
                (MasProbableModo.ConCuenta, Loc.T("masProbableModoCuenta"))
            }, draft.Modo, Palette.NeonViolet, SelectMode);

            var players = SetupSection.Create(content, Loc.T("jugadores"), Loc.T("masProbableJugadoresAyuda"), Palette.NeonCyan);
            _playersPanel = players.gameObject;
            Players = PlayerListEditor.Create(players.Content, MasProbableConfig.MinJugadores, MasProbableConfig.MaxJugadores, Palette.NeonCyan,
                _module.DraftPlayers);
            Players.NamesChanged += SaveNames;
            Players.CountChanged += SaveNames;
            _playersPanel.SetActive(draft.Modo == MasProbableModo.ConCuenta);

            var game = SetupSection.Create(content, Loc.T("masProbablePartida"), null, Palette.NeonMagenta);
            Ui.Text(game.Content, Loc.T("masProbableCuenta"), TextRole.LabelLarge, Palette.TextMuted);
            ChoiceChips<int>.Create(game.Content,
                MasProbableConfig.CuentaOptions.Select(n => (n, n == 0 ? Loc.T("sinCuentaAtras") : Loc.T("segundosCorto", ("n", n)))).ToList(),
                draft.SegundosCuenta, Palette.NeonMagenta, v => draft.SegundosCuenta = v);
            Ui.Text(game.Content, Loc.T("masProbableFrasesPartida"), TextRole.LabelLarge, Palette.TextMuted);
            ChoiceChips<int>.Create(game.Content, MasProbableConfig.FrasesOptions.Select(n => (n, n == 0 ? Loc.T("sinLimite") : n.ToString())).ToList(),
                draft.Frases, Palette.NeonMagenta, v => draft.Frases = v);

            Ui.Text(content, Loc.T("masProbableRespeto"), TextRole.BodySmall, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Button(content, Loc.T("empezarPartida"), Start, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f);
        }

        public void SelectMode(MasProbableModo mode)
        {
            _module.Draft.Modo = mode;
            if (Mode != null && Mode.Value != mode) Mode.Select(mode);
            if (_playersPanel != null) _playersPanel.SetActive(mode == MasProbableModo.ConCuenta);
        }

        private void SaveNames()
        {
            _module.DraftPlayers.Clear();
            _module.DraftPlayers.AddRange(Players.RawNames);
        }

        public void Start()
        {
            SaveNames();
            var (config, error) = MasProbableConfig.Create(Levels.Selected, Players.NormalizedNames, _module.Draft);
            if (error.HasValue)
            {
                Snackbar.Show(ErrorText(error.Value), Palette.NeonError);
                return;
            }
            if (config.Options.Modo == MasProbableModo.ConCuenta) Players.Remember();
            _module.Flow.Iniciar(config);
            if (_module.Flow.Fase == MasProbableFase.Frase)
            {
                _module.GoToPhase();
                return;
            }
            Dialogs.Show(Loc.T("sinFrasesTitulo"), Loc.T("sinFrasesMensaje"), new[]
            {
                new DialogAction(Loc.T("aceptar"), (Action)null, ButtonStyle.Filled, Palette.NeonCyan)
            });
        }

        public static string ErrorText(MasProbableConfigError error)
        {
            switch (error)
            {
                case MasProbableConfigError.PocosJugadores: return Loc.T("errorMinJugadores", ("n", MasProbableConfig.MinJugadores));
                case MasProbableConfigError.DemasiadosJugadores: return Loc.T("errorMaxJugadores", ("n", MasProbableConfig.MaxJugadores));
                case MasProbableConfigError.NombreVacio: return Loc.T("errorNombreVacio");
                case MasProbableConfigError.NombresRepetidos: return Loc.T("errorNombresDuplicados");
                default: return Loc.T("errorSinNiveles");
            }
        }
    }

    /// <summary>Base con partida en curso: casa y atrás confirman; la barra lleva «Terminar».</summary>
    public abstract class MasProbablePlayScreen : AppScreen
    {
        protected MasProbablePlayScreen(MasProbableModule module) => Module = module;

        protected MasProbableModule Module { get; }
        protected MasProbableFlowController Flow => Module.Flow;
        protected abstract MasProbableFase Phase { get; }

        public sealed override void Build(RectTransform root)
        {
            var playing = Flow.Config != null && Flow.Fase == Phase;
            string subtitle = null;
            if (playing && Phase != MasProbableFase.Final)
            {
                subtitle = Flow.Config.Options.Frases > 0
                    ? Loc.T("fraseXDeY", ("n", Flow.Numero), ("total", Flow.Config.Options.Frases))
                    : Loc.T("fraseX", ("n", Flow.Numero));
            }
            var scaffold = ScreenScaffold.Create(root, Loc.T("masProbableTitulo"), subtitle);
            if (!playing)
            {
                GameUi.HomeButton(scaffold.AppBar, Router);
                var column = Ui.Column(scaffold.Body, 16f, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter);
                column.Sized(flexHeight: 1f);
                Ui.Text(column, Loc.T("noHayPartidaEnCurso"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                Ui.Button(column, Loc.T("configurarPartida"), () => Router.Go(MasProbableModule.SetupRoute));
                return;
            }
            if (Phase == MasProbableFase.Final)
            {
                GameUi.HomeButton(scaffold.AppBar, Router);
            }
            else
            {
                GameUi.HomeButton(scaffold.AppBar, Router, () => Flow.Reiniciar());
                GameUi.FinishButton(scaffold.AppBar, Finish);
            }
            BuildPhase(scaffold);
        }

        protected abstract void BuildPhase(ScreenScaffold scaffold);

        public override bool OnBack()
        {
            if (Flow.Config == null || Flow.Fase == MasProbableFase.Setup) return false;
            if (Flow.Fase == MasProbableFase.Final)
            {
                Module.ResetToMenu();
                return true;
            }
            AbandonGameDialog.Show(Module.ResetToSetup);
            return true;
        }

        public void Finish()
        {
            Flow.Terminar();
            Module.GoToPhase();
        }

        /// <summary>La pregunta entera con la frase: «…acabe en comisaría?».</summary>
        public static string Question(PromptCard card) => card == null ? string.Empty : Loc.T("masProbableFraseCompleta", ("frase", card.Text));
    }

    /// <summary>
    /// La frase: «¿Quién es más probable que…» y el final en grande; «¡A la de tres!» lanza la cuenta atrás (si hay) y
    /// al acabar todos señalan. Después, siguiente frase o apuntar al más señalado.
    /// </summary>
    public sealed class MasProbablePhraseScreen : MasProbablePlayScreen
    {
        private readonly Countdown _countdown = new Countdown();
        private TMP_Text _text;
        private TMP_Text _count;
        private NeonPanel _panel;
        private NeonButton _go;
        private NeonButton _next;
        private NeonButton _skip;

        public MasProbablePhraseScreen(MasProbableModule module) : base(module)
        {
            _countdown.SecondChanged += ShowCount;
            _countdown.Finished += Pointed;
        }

        protected override MasProbableFase Phase => MasProbableFase.Frase;

        public string PhraseText => _text != null ? _text.text : null;
        public Countdown Countdown => _countdown;

        /// <summary>Si ya se ha señalado (se ve «Siguiente» o «Apuntar»).</summary>
        public bool Revealed => _next != null && _next.gameObject.activeSelf;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var body = Ui.Column(scaffold.Body, 14f, new RectOffset(20, 20, 16, 20));
            body.Sized(flexHeight: 1f);
            Ui.Flex(body);
            Ui.Text(body, Loc.T("masProbablePregunta"), TextRole.TitleLarge, Palette.TextMuted, TextAlignmentOptions.Center, bold: true);
            var spicy = Flow.Current.Level == PromptLevel.Picante;
            var color = spicy ? Palette.NeonMagenta : Palette.NeonCyan;
            _panel = Ui.Panel(body, color, glow: 1f, padding: new RectOffset(22, 22, 34, 34), alignment: TextAnchor.MiddleCenter);
            PulseGlow.On(_panel, 0.8f, 1.2f);
            _text = Ui.Text(_panel.Content, Question(Flow.Current), TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, 1f, color,
                bold: true);
            _text.lineSpacing = 8f;
            _panel.name = Loc.T("cartaSemantica", ("carta", Loc.T("masProbablePregunta") + " " + _text.text));
            _count = Ui.FittedText(body, string.Empty, 72f, Palette.NeonYellow, 1f);
            _count.gameObject.SetActive(false);
            Ui.Flex(body);

            var seconds = Flow.Config.Options.SegundosCuenta;
            _go = Ui.Button(body, Loc.T(seconds > 0 ? "masProbableALaDeTres" : "masProbableYaHemosSenalado"), StartCount, ButtonStyle.Filled,
                IconId.TouchApp, Palette.NeonViolet, 0.9f);
            _go.Sized(height: 64f);
            _next = Ui.Button(body, Loc.T(Flow.ConCuenta ? "masProbableApuntar" : "masProbableSiguiente"), Next, ButtonStyle.Filled,
                Flow.ConCuenta ? IconId.Check : IconId.ArrowForward, glow: 0.8f);
            _next.Sized(height: 64f);
            _next.gameObject.SetActive(false);
            _skip = Ui.Button(body, Loc.T("masProbableOtraFrase"), Skip, ButtonStyle.Text, IconId.Refresh, Palette.NeonCyan);
            if (Flow.ConsumirAvisoMazo()) Snackbar.Show(Loc.T("frasesAgotadas"), Palette.NeonYellow);
        }

        public override void Tick(float deltaTime)
        {
            if (Dialogs.IsOpen) return;
            _countdown.Tick(deltaTime);
        }

        public override void OnHide() => _countdown.Stop();

        /// <summary>«¡A la de tres!»: la cuenta atrás (o, sin cuenta, ya está).</summary>
        public void StartCount()
        {
            _go.gameObject.SetActive(false);
            _skip.gameObject.SetActive(false);
            var seconds = Flow.Config.Options.SegundosCuenta;
            if (seconds <= 0)
            {
                Pointed();
                return;
            }
            _count.gameObject.SetActive(true);
            _countdown.Start(seconds);
        }

        public void Next()
        {
            Flow.Senalado();
            Module.Audio?.Play(AppSound.Swipe, 0.6f);
            Module.GoToPhase();
        }

        public void Skip()
        {
            Flow.Saltar();
            Module.Audio?.Play(AppSound.Swipe, 0.6f);
            Module.GoToPhase();
        }

        private void ShowCount(int seconds)
        {
            if (_count == null || !_countdown.Running || seconds <= 0) return;
            _count.text = seconds.ToString();
            _count.name = Loc.T("tiempoRestante", ("n", seconds));
            Module.Audio?.Play(AppSound.Beep, 0.8f);
        }

        private void Pointed()
        {
            if (_count != null)
            {
                _count.gameObject.SetActive(true);
                _count.text = Loc.T("masProbableSenalad");
                _count.color = Palette.NeonMagenta;
                Ui.SetGlow((TextMeshProUGUI)_count, Palette.NeonMagenta, 1f);
            }
            if (Flow.Config.Options.SegundosCuenta > 0)
            {
                Module.Audio?.Play(AppSound.Buzzer, 0.8f);
                Haptics.Vibrate();
            }
            if (_next != null) _next.gameObject.SetActive(true);
            if (_go != null) _go.gameObject.SetActive(false);
            if (_skip != null) _skip.gameObject.SetActive(false);
        }
    }

    /// <summary>Con cuenta: se marca al más señalado (o a los empatados) y «Apuntar»; sin marcar a nadie, no suma nadie.</summary>
    public sealed class MasProbablePointScreen : MasProbablePlayScreen
    {
        private readonly HashSet<int> _picked = new HashSet<int>();
        private readonly List<NeonChip> _chips = new List<NeonChip>();
        private NeonButton _confirm;

        public MasProbablePointScreen(MasProbableModule module) : base(module)
        {
        }

        protected override MasProbableFase Phase => MasProbableFase.Apuntar;

        public IReadOnlyCollection<int> Picked => _picked;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var content = Ui.Scroll(scaffold.Body, 14f, new RectOffset(20, 20, 16, 24));
            Ui.Text(content, Loc.T("masProbablePregunta"), TextRole.TitleSmall, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Text(content, Question(Flow.Current), TextRole.TitleLarge, Palette.TextPrimary, TextAlignmentOptions.Center, 0.6f, Palette.NeonCyan,
                bold: true);
            Ui.Text(content, Loc.T("masProbableQuienMas"), TextRole.BodyMedium, Palette.NeonVioletText, TextAlignmentOptions.Center);
            var grid = ResponsiveChipGrid.Create(content);
            _chips.Clear();
            for (var i = 0; i < Flow.Config.Jugadores.Count; i++)
            {
                var index = i;
                var chip = NeonChip.Create(grid, Flow.Config.Jugadores[i], false, Palette.NeonViolet);
                chip.ToggleOnClick = false;
                chip.Clicked += () => Toggle(index);
                chip.name = Flow.Config.Jugadores[i];
                _chips.Add(chip);
            }
            _confirm = Ui.Button(content, Loc.T("masProbableNadie"), Confirm, ButtonStyle.Filled, IconId.Check, glow: 0.8f);
            _confirm.Sized(height: 64f);
        }

        /// <summary>Marca o desmarca a un jugador (los empatados se marcan todos).</summary>
        public void Toggle(int index)
        {
            if (!_picked.Remove(index)) _picked.Add(index);
            for (var i = 0; i < _chips.Count; i++) _chips[i].Selected = _picked.Contains(i);
            _confirm.SetLabel(Loc.T(_picked.Count == 0 ? "masProbableNadie" : "masProbableApuntar"));
        }

        public void Confirm()
        {
            Flow.Apuntar(_picked);
            Module.Audio?.Play(_picked.Count > 0 ? AppSound.Correct : AppSound.Swipe, 0.6f);
            Module.GoToPhase();
        }
    }

    /// <summary>Final: con cuenta, «el más probable de todo» y los títulos de cada uno; sin cuenta, cuántas frases se jugaron.</summary>
    public sealed class MasProbableEndScreen : MasProbablePlayScreen
    {
        public MasProbableEndScreen(MasProbableModule module) : base(module)
        {
        }

        protected override MasProbableFase Phase => MasProbableFase.Final;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(20, 20, 20, 24));
            var panel = Ui.Panel(content, Palette.NeonCyan, glow: 1f, padding: new RectOffset(24, 24, 24, 24), spacing: 10f, alignment: TextAnchor.UpperCenter);
            PulseGlow.On(panel, 0.8f, 1.3f);
            Ui.Icon(panel.Content, Flow.ConCuenta ? IconId.Trophy : IconId.TouchApp, 64f, Palette.NeonCyan, 1f);
            var ranking = Flow.Ranking;
            if (Flow.ConCuenta && ranking.Count > 0 && ranking[0].titulos > 0)
            {
                var top = ranking.Where(r => r.titulos == ranking[0].titulos).Select(r => Flow.Config.Jugadores[r.index]).ToList();
                Ui.Text(panel.Content, Loc.T("masProbableElMas"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
                Ui.FittedText(panel.Content, string.Join(" · ", top), 36f, Palette.TextPrimary, 1f, lines: 2);
            }
            else
            {
                Ui.Text(panel.Content, Loc.T("partidaTerminada"), TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, bold: true);
            }
            Ui.Text(content, Loc.T("masProbableFrasesJugadas", ("n", Flow.Rondas.Count)), TextRole.BodyMedium, Palette.TextMuted,
                TextAlignmentOptions.Center);
            if (Flow.ConCuenta) ScoreList.Create(content, Rows(Flow));
            Ui.Button(content, Loc.T("jugarOtra"), Module.PlayAgain, ButtonStyle.Filled, IconId.Refresh, glow: 0.8f);
            Ui.Button(content, Loc.T("cambiarAjustes"), Module.ResetToSetup, ButtonStyle.Outlined, IconId.Tune);
            Ui.Button(content, Loc.T("volverAlMenu"), Module.ResetToMenu, ButtonStyle.Text, IconId.Home);
        }

        public override void OnShow()
        {
            if (Flow.Fase == MasProbableFase.Final) Module.Audio?.Play(AppSound.GameOver, 0.7f);
        }

        /// <summary>Clasificación por títulos, con la última frase que «ganó» cada uno.</summary>
        public static List<ScoreRow> Rows(MasProbableFlowController flow)
        {
            var ranking = flow.Ranking;
            var positions = ScoreList.Positions(ranking, r => r.titulos);
            return ranking.Select((r, i) =>
            {
                var last = flow.Rondas.LastOrDefault(round => round.Winners.Contains(r.index));
                var detail = last != null ? Question(last.Card) : Loc.T("masProbableSinTitulos");
                return new ScoreRow(positions[i], flow.Config.Jugadores[r.index], Loc.T("masProbableTitulos", ("n", r.titulos)), detail,
                    positions[i] == 1 && r.titulos > 0 ? Palette.NeonCyan : Palette.NeonViolet);
            }).ToList();
        }
    }
}
