using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.VerdadReto.Domain;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.VerdadReto
{
    /// <summary>
    /// Setup de Verdad o Reto (V071): jugadores (2–20), nivel (suave / picante +18 con confirmación de edad), de
    /// dónde salen las cartas (si hay propias) y las reglas: orden de los turnos, quién elige, rondas, puntos,
    /// rajarse con prenda, cambios de carta, máximo de verdades seguidas y tiempo para los retos. La barra abre la
    /// gestión de cartas.
    /// </summary>
    public sealed class VerdadRetoSetupScreen : AppScreen
    {
        private readonly VerdadRetoModule _module;

        public VerdadRetoSetupScreen(VerdadRetoModule module) => _module = module;

        public PlayerListEditor Players { get; private set; }
        public PromptLevelPicker Levels { get; private set; }
        public PromptOriginPicker Origin { get; private set; }

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("verdadRetoTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "verdadRetoTitulo", GameUi.RuleKeys("reglasVerdadReto", 6));
            scaffold.AppBar.AddAction(IconId.MenuBook, () => Router.Go(VerdadRetoModule.CardsRoute)).name = Loc.T("gestionarCartas");
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 8, 32));
            var draft = _module.Draft;

            var players = SetupSection.Create(content, Loc.T("jugadores"), Loc.T("verdadRetoJugadoresAyuda"), Palette.NeonCyan);
            Players = PlayerListEditor.Create(players.Content, VerdadRetoConfig.MinJugadores, VerdadRetoConfig.MaxJugadores, Palette.NeonCyan,
                _module.DraftPlayers);
            Players.NamesChanged += SaveNames;
            Players.CountChanged += SaveNames;

            Levels = PromptLevelPicker.Create(content, _module.DraftLevels);
            Origin = PromptOriginPicker.Create(content, _module.OwnCountOrZero(), draft.IncluirSerie, draft.IncluirPropias, (seed, own) =>
            {
                draft.IncluirSerie = seed;
                draft.IncluirPropias = own;
            });

            var turns = SetupSection.Create(content, Loc.T("verdadRetoTurnos"), null, Palette.NeonViolet);
            Label(turns.Content, "verdadRetoOrden");
            ChoiceChips<OrdenTurnos>.Create(turns.Content, new[]
            {
                (OrdenTurnos.EnOrden, Loc.T("verdadRetoOrdenNombres")),
                (OrdenTurnos.AlAzar, Loc.T("verdadRetoOrdenAzar"))
            }, draft.Orden, Palette.NeonViolet, v => draft.Orden = v);
            Label(turns.Content, "verdadRetoQuienElige");
            ChoiceChips<ModoEleccion>.Create(turns.Content, new[]
            {
                (ModoEleccion.ElJugador, Loc.T("verdadRetoEligeJugador")),
                (ModoEleccion.AlAzar, Loc.T("verdadRetoEligeMovil"))
            }, draft.Eleccion, Palette.NeonViolet, v => draft.Eleccion = v);
            Label(turns.Content, "verdadRetoRondas");
            ChoiceChips<int>.Create(turns.Content, VerdadRetoConfig.RondasOptions.Select(n => (n, n == 0 ? Loc.T("sinLimite") : n.ToString())).ToList(),
                draft.Rondas, Palette.NeonViolet, v => draft.Rondas = v);
            var points = NeonSwitch.Create(turns.Content, Loc.T("verdadRetoPuntos"), Loc.T("verdadRetoPuntosAyuda"), draft.Puntos);
            points.Changed += v => draft.Puntos = v;

            var rules = SetupSection.Create(content, Loc.T("verdadRetoReglas"), null, Palette.NeonMagenta);
            var chicken = NeonSwitch.Create(rules.Content, Loc.T("verdadRetoPermitirRajarse"), Loc.T("verdadRetoPermitirRajarseAyuda"),
                draft.PermitirRajarse);
            chicken.Changed += v => draft.PermitirRajarse = v;
            Label(rules.Content, "verdadRetoCambios");
            ChoiceChips<int>.Create(rules.Content, VerdadRetoConfig.CambiosOptions.Select(n => (n, n == 0 ? Loc.T("ninguno") : n.ToString())).ToList(),
                draft.CambiosPorTurno, Palette.NeonMagenta, v => draft.CambiosPorTurno = v);
            Label(rules.Content, "verdadRetoMaxVerdades");
            ChoiceChips<int>.Create(rules.Content, VerdadRetoConfig.MaxVerdadesOptions.Select(n => (n, n == 0 ? Loc.T("sinLimite") : n.ToString())).ToList(),
                draft.MaxVerdadesSeguidas, Palette.NeonMagenta, v => draft.MaxVerdadesSeguidas = v);
            Label(rules.Content, "verdadRetoTiempoReto");
            ChoiceChips<int>.Create(rules.Content,
                VerdadRetoConfig.SegundosRetoOptions.Select(n => (n, n == 0 ? Loc.T("sinTiempo") : Loc.T("segundosCorto", ("n", n)))).ToList(),
                draft.SegundosReto, Palette.NeonMagenta, v => draft.SegundosReto = v);

            Ui.Text(content, Loc.T("verdadRetoNadieObligado"), TextRole.BodySmall, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Button(content, Loc.T("empezarPartida"), Start, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f);
        }

        private static void Label(Transform parent, string key) => Ui.Text(parent, Loc.T(key), TextRole.LabelLarge, Palette.TextMuted);

        private void SaveNames()
        {
            _module.DraftPlayers.Clear();
            _module.DraftPlayers.AddRange(Players.RawNames);
        }

        public void Start()
        {
            SaveNames();
            var (config, error) = VerdadRetoConfig.Create(Players.NormalizedNames, Levels.Selected, _module.Draft);
            if (error.HasValue)
            {
                Snackbar.Show(ErrorText(error.Value), Palette.NeonError);
                return;
            }
            Players.Remember();
            _module.Flow.Iniciar(config);
            if (_module.Flow.Fase == VerdadRetoFase.Eleccion)
            {
                _module.GoToPhase();
                return;
            }
            Dialogs.Show(Loc.T("sinCartasTitulo"), Loc.T("sinCartasMensaje"), new[]
            {
                new DialogAction(Loc.T("aceptar"), (Action)null, ButtonStyle.Filled, Palette.NeonCyan)
            });
        }

        public static string ErrorText(VerdadRetoConfigError error)
        {
            switch (error)
            {
                case VerdadRetoConfigError.PocosJugadores: return Loc.T("errorMinJugadores", ("n", VerdadRetoConfig.MinJugadores));
                case VerdadRetoConfigError.DemasiadosJugadores: return Loc.T("errorMaxJugadores", ("n", VerdadRetoConfig.MaxJugadores));
                case VerdadRetoConfigError.NombreVacio: return Loc.T("errorNombreVacio");
                case VerdadRetoConfigError.NombresRepetidos: return Loc.T("errorNombresDuplicados");
                default: return Loc.T("errorSinNiveles");
            }
        }
    }

    /// <summary>
    /// Base de las pantallas con partida en curso: casa y atrás piden confirmación; la barra tiene el marcador (si
    /// hay puntos) y «Terminar». Sin partida, «No hay ninguna partida en curso».
    /// </summary>
    public abstract class VerdadRetoPlayScreen : AppScreen
    {
        protected VerdadRetoPlayScreen(VerdadRetoModule module) => Module = module;

        protected VerdadRetoModule Module { get; }

        protected VerdadRetoFlowController Flow => Module.Flow;

        /// <summary>La fase que pinta esta pantalla.</summary>
        protected abstract VerdadRetoFase Phase { get; }

        public sealed override void Build(RectTransform root)
        {
            var playing = Flow.Config != null && Flow.Fase == Phase;
            var subtitle = playing && Phase != VerdadRetoFase.Final ? RoundText() : null;
            var scaffold = ScreenScaffold.Create(root, Loc.T("verdadRetoTitulo"), subtitle);
            if (!playing)
            {
                GameUi.HomeButton(scaffold.AppBar, Router);
                var column = Ui.Column(scaffold.Body, 16f, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter);
                column.Sized(flexHeight: 1f);
                Ui.Text(column, Loc.T("noHayPartidaEnCurso"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                Ui.Button(column, Loc.T("configurarPartida"), () => Router.Go(VerdadRetoModule.SetupRoute));
                return;
            }
            if (Phase == VerdadRetoFase.Final)
            {
                GameUi.HomeButton(scaffold.AppBar, Router, null);
            }
            else
            {
                GameUi.HomeButton(scaffold.AppBar, Router, () => Flow.Reiniciar());
                if (Flow.Config.Options.Puntos) scaffold.AppBar.AddAction(IconId.Trophy, ShowScores).name = Loc.T("marcador");
                GameUi.FinishButton(scaffold.AppBar, Finish);
            }
            BuildPhase(scaffold);
        }

        protected abstract void BuildPhase(ScreenScaffold scaffold);

        public override bool OnBack()
        {
            if (Flow.Config == null || Flow.Fase == VerdadRetoFase.Setup) return false;
            if (Flow.Fase == VerdadRetoFase.Final)
            {
                Module.ResetToMenu();
                return true;
            }
            AbandonGameDialog.Show(Module.ResetToSetup);
            return true;
        }

        /// <summary>«Terminar»: a la clasificación.</summary>
        public void Finish()
        {
            Flow.Terminar();
            Module.GoToPhase();
        }

        private string RoundText()
        {
            var rounds = Flow.Config.Options.Rondas;
            return rounds > 0 ? Loc.T("rondaXDeY", ("ronda", Flow.Ronda), ("total", rounds)) : Loc.T("rondaX", ("ronda", Flow.Ronda));
        }

        private void ShowScores()
        {
            Dialogs.Show(Loc.T("marcador"), null, new[]
            {
                new DialogAction(Loc.T("cerrar"), (Action)null, ButtonStyle.Filled, Palette.NeonCyan)
            }, buildContent: content =>
            {
                var list = Ui.Scroll(content, 0f, new RectOffset(2, 2, 2, 2), "ScoresScroll");
                ((RectTransform)list.parent.parent).Sized(height: Mathf.Min(360f, Flow.Jugadores.Count * 72f + 8f), flexHeight: 0f);
                ScoreList.Create(list, VerdadRetoEndScreen.Rows(Flow));
            });
        }

        protected static RectTransform CenteredColumn(RectTransform body)
        {
            var content = Ui.Scroll(body, 16f, new RectOffset(20, 20, 20, 24));
            return content;
        }
    }

    /// <summary>Le toca a alguien: su nombre en grande y Verdad / Reto (o «Sortear», si elige el móvil).</summary>
    public sealed class VerdadRetoTurnScreen : VerdadRetoPlayScreen
    {
        public VerdadRetoTurnScreen(VerdadRetoModule module) : base(module)
        {
        }

        protected override VerdadRetoFase Phase => VerdadRetoFase.Eleccion;

        public NeonButton VerdadButton { get; private set; }
        public NeonButton RetoButton { get; private set; }

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var content = CenteredColumn(scaffold.Body);
            Ui.Space(content, 12f);
            Ui.Text(content, Loc.T("leTocaA"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Text(content, Flow.JugadorActual.Name, TextRole.DisplayMedium, Palette.TextPrimary, TextAlignmentOptions.Center, 1f, Palette.NeonCyan, 2,
                true);
            Ui.Space(content, 12f);
            if (Flow.Config.Options.Eleccion == ModoEleccion.AlAzar)
            {
                Ui.Text(content, Loc.T("verdadRetoSorteoAyuda"), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
                Ui.Button(content, Loc.T("verdadRetoSortear"), Draw, ButtonStyle.Filled, IconId.Casino, Palette.NeonViolet, 0.9f).Sized(height: 72f);
                return;
            }
            VerdadButton = Ui.Button(content, Loc.T("verdadRetoVerdad"), ChooseVerdad, ButtonStyle.Filled, IconId.Help, Palette.NeonCyan, 0.9f);
            VerdadButton.Sized(height: 80f);
            VerdadButton.Interactable = Flow.PuedeElegir(VerdadRetoKinds.Verdad);
            RetoButton = Ui.Button(content, Loc.T("verdadRetoReto"), ChooseReto, ButtonStyle.Filled, IconId.Whatshot, Palette.NeonMagenta, 0.9f);
            RetoButton.Sized(height: 80f);
            RetoButton.Interactable = Flow.PuedeElegir(VerdadRetoKinds.Reto);
            var reason = Reason();
            if (reason != null) Ui.Text(content, reason, TextRole.BodyMedium, Palette.NeonYellow, TextAlignmentOptions.Center);
        }

        private string Reason()
        {
            if (Flow.VerdadBloqueada) return Loc.T("verdadRetoTocaReto", ("n", Flow.Config.Options.MaxVerdadesSeguidas));
            if (!Flow.Disponible(VerdadRetoKinds.Verdad)) return Loc.T("verdadRetoSinVerdades");
            if (!Flow.Disponible(VerdadRetoKinds.Reto)) return Loc.T("verdadRetoSinRetos");
            return null;
        }

        public void ChooseVerdad() => Choose(() => Flow.Elegir(VerdadRetoKinds.Verdad));
        public void ChooseReto() => Choose(() => Flow.Elegir(VerdadRetoKinds.Reto));
        public void Draw() => Choose(Flow.Sortear);

        private void Choose(Action action)
        {
            action();
            if (Flow.Fase != VerdadRetoFase.Carta) return;
            Module.Audio?.Play(AppSound.CardFlip, 0.7f);
            Module.GoToPhase();
        }
    }

    /// <summary>
    /// La carta: VERDAD (cian) o RETO (magenta), el texto en grande y ¡Hecho! / Me rajo / Otra carta. Con tiempo
    /// para los retos, una cuenta atrás que se para con los diálogos y suena al acabar (sin cortar el turno).
    /// </summary>
    public sealed class VerdadRetoCardScreen : VerdadRetoPlayScreen
    {
        private readonly Countdown _countdown = new Countdown();
        private TMP_Text _text;
        private TMP_Text _timer;
        private NeonPanel _panel;
        private NeonButton _change;

        public VerdadRetoCardScreen(VerdadRetoModule module) : base(module)
        {
            _countdown.SecondChanged += ShowSeconds;
            _countdown.Finished += () =>
            {
                Module.Audio?.Play(AppSound.Buzzer);
                Haptics.Vibrate();
                if (_timer != null) _timer.text = Loc.T("verdadRetoTiempo");
            };
        }

        protected override VerdadRetoFase Phase => VerdadRetoFase.Carta;

        public string CardText => _text != null ? _text.text : null;
        public Countdown Countdown => _countdown;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var truth = Flow.TipoActual == VerdadRetoKinds.Verdad;
            var color = truth ? Palette.NeonCyan : Palette.NeonMagenta;
            var content = CenteredColumn(scaffold.Body);
            Ui.Text(content, Flow.JugadorActual.Name, TextRole.TitleLarge, Palette.TextPrimary, TextAlignmentOptions.Center, maxLines: 1, bold: true);
            Ui.Text(content, Loc.T(truth ? "verdadRetoVerdad" : "verdadRetoReto").ToUpper(Loc.Culture), TextRole.HeadlineMedium, color,
                TextAlignmentOptions.Center, 1f, color, 1, true);
            _panel = Ui.Panel(content, color, glow: 1f, padding: new RectOffset(22, 22, 32, 32), alignment: TextAnchor.MiddleCenter);
            PulseGlow.On(_panel, 0.8f, 1.2f);
            _text = Ui.Text(_panel.Content, string.Empty, TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, 1f, color, bold: true);
            _text.lineSpacing = 8f;
            var spicy = Flow.CartaActual.Level == PromptLevel.Picante;
            if (spicy) Ui.Text(content, Loc.T("nivelPicante"), TextRole.LabelLarge, Palette.NeonMagenta, TextAlignmentOptions.Center);

            _timer = null;
            if (!truth && Flow.Config.Options.SegundosReto > 0)
            {
                _timer = Ui.FittedText(content, string.Empty, 44f, Palette.NeonCyan, 1f);
                _countdown.Start(Flow.Config.Options.SegundosReto);
            }

            var points = Flow.Config.Options.Puntos ? (truth ? VerdadRetoConfig.PuntosVerdad : VerdadRetoConfig.PuntosReto) : 0;
            var done = Ui.Button(content, points > 0 ? Loc.T("verdadRetoHechoPuntos", ("n", points)) : Loc.T("verdadRetoHecho"), Done, ButtonStyle.Filled,
                IconId.Check, Palette.NeonGreen, 0.8f);
            done.Sized(height: 64f);
            if (Flow.Config.Options.PermitirRajarse)
            {
                Ui.Button(content, Loc.T("verdadRetoMeRajo"), Chicken, ButtonStyle.Outlined, IconId.Block, Palette.NeonRed);
            }
            // Con el texto ya puesto: un botón creado sin texto no tiene etiqueta que cambiar después.
            _change = Ui.Button(content, Loc.T("verdadRetoOtraCarta", ("n", Flow.CambiosRestantes)), Change, ButtonStyle.Text, IconId.Refresh,
                Palette.NeonCyan);
            Refresh();
        }

        public override void Tick(float deltaTime)
        {
            if (Dialogs.IsOpen) return;
            _countdown.Tick(deltaTime);
        }

        public override void OnHide() => _countdown.Stop();

        public void Done()
        {
            Flow.Cumplir();
            Module.Audio?.Play(AppSound.Correct, 0.6f);
            Module.GoToPhase();
        }

        public void Chicken()
        {
            Flow.Rajarse();
            if (Flow.Fase != VerdadRetoFase.Castigo) return;
            Module.Audio?.Play(AppSound.Wrong, 0.6f);
            Module.GoToPhase();
        }

        public void Change()
        {
            Flow.OtraCarta();
            Module.Audio?.Play(AppSound.Swipe, 0.6f);
            Refresh();
            FadeScaleIn.Play(_panel.gameObject, 0.25f, 0.96f);
            if (_timer != null) _countdown.Start(Flow.Config.Options.SegundosReto);
        }

        private void Refresh()
        {
            _text.text = Flow.TextoCarta;
            _panel.name = Loc.T("cartaSemantica", ("carta", _text.text));
            _change.gameObject.SetActive(Flow.CambiosRestantes > 0);
            _change.SetLabel(Loc.T("verdadRetoOtraCarta", ("n", Flow.CambiosRestantes)));
            if (Flow.ConsumirAvisoMazo()) Snackbar.Show(Loc.T("cartasAgotadas"), Palette.NeonYellow);
        }

        private void ShowSeconds(int seconds)
        {
            if (_timer == null) return;
            _timer.text = seconds.ToString();
            _timer.color = seconds > 10 ? Palette.NeonCyan : seconds > 5 ? Palette.NeonYellow : Palette.NeonRed;
            Ui.SetGlow((TextMeshProUGUI)_timer, _timer.color, 1f);
            _timer.name = Loc.T("tiempoRestante", ("n", seconds));
            if (seconds <= 5 && seconds > 0) Module.Audio?.Play(AppSound.Beep, 0.8f);
        }
    }

    /// <summary>Se ha rajado: la prenda (o «el grupo elige la prenda» si no hay cartas de castigo) y «Prenda cumplida».</summary>
    public sealed class VerdadRetoPenaltyScreen : VerdadRetoPlayScreen
    {
        private TMP_Text _text;

        public VerdadRetoPenaltyScreen(VerdadRetoModule module) : base(module)
        {
        }

        protected override VerdadRetoFase Phase => VerdadRetoFase.Castigo;

        public string PenaltyText => _text != null ? _text.text : null;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var content = CenteredColumn(scaffold.Body);
            Ui.Icon(content, IconId.Block, 56f, Palette.NeonRed, 0.9f);
            Ui.Text(content, Loc.T("verdadRetoSeRaja", ("nombre", Flow.JugadorActual.Name)), TextRole.HeadlineSmall, Palette.NeonRed,
                TextAlignmentOptions.Center, 0.8f, bold: true);
            Ui.Text(content, Loc.T("verdadRetoPrenda"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            var panel = Ui.Panel(content, Palette.NeonRed, glow: 0.9f, padding: new RectOffset(22, 22, 28, 28), alignment: TextAnchor.MiddleCenter);
            _text = Ui.Text(panel.Content, Flow.TextoCastigo ?? Loc.T("verdadRetoPrendaLibre"), TextRole.HeadlineSmall, Palette.TextPrimary,
                TextAlignmentOptions.Center, 0.8f, Palette.NeonRed, bold: true);
            panel.name = Loc.T("cartaSemantica", ("carta", _text.text));
            Ui.Button(content, Loc.T("verdadRetoPrendaCumplida"), Done, ButtonStyle.Filled, IconId.Check, Palette.NeonCyan, 0.8f).Sized(height: 64f);
        }

        public void Done()
        {
            Flow.CastigoCumplido();
            Module.GoToPhase();
        }
    }

    /// <summary>Clasificación: el ganador (con puntos) y todos con sus verdades, retos y rajadas; jugar otra, ajustes o menú.</summary>
    public sealed class VerdadRetoEndScreen : VerdadRetoPlayScreen
    {
        public VerdadRetoEndScreen(VerdadRetoModule module) : base(module)
        {
        }

        protected override VerdadRetoFase Phase => VerdadRetoFase.Final;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(20, 20, 20, 24));
            var panel = Ui.Panel(content, Palette.NeonCyan, glow: 1f, padding: new RectOffset(24, 24, 24, 24), spacing: 10f, alignment: TextAnchor.UpperCenter);
            PulseGlow.On(panel, 0.8f, 1.3f);
            Ui.Icon(panel.Content, IconId.Trophy, 64f, Palette.NeonCyan, 1f);
            var ranking = Flow.Ranking;
            if (Flow.Config.Options.Puntos && ranking.Count > 0)
            {
                var top = ranking.Where(p => p.Puntos == ranking[0].Puntos && p.Rajadas == ranking[0].Rajadas).Select(p => p.Name).ToList();
                Ui.Text(panel.Content, Loc.T(top.Count > 1 ? "empateEntre" : "ganador"), TextRole.TitleMedium, Palette.TextMuted, TextAlignmentOptions.Center);
                Ui.FittedText(panel.Content, string.Join(" · ", top), 36f, Palette.TextPrimary, 1f, lines: 2);
            }
            else
            {
                Ui.Text(panel.Content, Loc.T("partidaTerminada"), TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, bold: true);
            }
            Ui.Text(content, Loc.T("verdadRetoTurnosJugados", ("n", Flow.Turnos)), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
            ScoreList.Create(content, Rows(Flow));
            Ui.Button(content, Loc.T("jugarOtra"), Module.PlayAgain, ButtonStyle.Filled, IconId.Refresh, glow: 0.8f);
            Ui.Button(content, Loc.T("cambiarAjustes"), Module.ResetToSetup, ButtonStyle.Outlined, IconId.Tune);
            Ui.Button(content, Loc.T("volverAlMenu"), Module.ResetToMenu, ButtonStyle.Text, IconId.Home);
        }

        public override void OnShow()
        {
            if (Flow.Fase == VerdadRetoFase.Final) Module.Audio?.Play(AppSound.GameOver, 0.7f);
        }

        /// <summary>Filas de la clasificación (también las usa el marcador de la barra).</summary>
        public static List<ScoreRow> Rows(VerdadRetoFlowController flow)
        {
            var ranking = flow.Ranking;
            var points = flow.Config.Options.Puntos;
            var positions = ScoreList.Positions(ranking, p => p.Puntos * 1000 - p.Rajadas);
            return ranking.Select((p, i) => new ScoreRow(positions[i], p.Name, points ? Loc.T("puntosCorto", ("n", p.Puntos)) : null,
                Loc.T("verdadRetoResumenJugador", ("verdades", p.Verdades), ("retos", p.Retos), ("rajadas", p.Rajadas)),
                i == 0 ? Palette.NeonCyan : p.Rajadas > p.Verdades + p.Retos ? Palette.NeonRed : Palette.NeonViolet)).ToList();
        }
    }
}
