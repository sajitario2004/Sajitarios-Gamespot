using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.QuePrefieres.Domain;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Games.QuePrefieres
{
    /// <summary>
    /// Setup (V073): nivel (picante con la confirmación de edad), origen si hay preguntas propias, modo (solo preguntas
    /// / contar votos), tiempo para pensar, preguntas por partida y si paga la minoría.
    /// </summary>
    public sealed class QuePrefieresSetupScreen : AppScreen
    {
        private readonly QuePrefieresModule _module;

        public QuePrefieresSetupScreen(QuePrefieresModule module) => _module = module;

        public PromptLevelPicker Levels { get; private set; }
        public PromptOriginPicker Origin { get; private set; }

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("quePrefieresTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "quePrefieresTitulo", GameUi.RuleKeys("reglasQuePrefieres", 5));
            scaffold.AppBar.AddAction(IconId.MenuBook, () => Router.Go(QuePrefieresModule.CardsRoute)).name = Loc.T("gestionarPreguntas");
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 8, 32));
            var draft = _module.Draft;

            Levels = PromptLevelPicker.Create(content, _module.DraftLevels);
            Origin = PromptOriginPicker.Create(content, _module.OwnCountOrZero(), draft.IncluirSerie, draft.IncluirPropias, (seed, own) =>
            {
                draft.IncluirSerie = seed;
                draft.IncluirPropias = own;
            }, "origenTituloPreguntas", "origenAyudaPreguntas");

            var mode = SetupSection.Create(content, Loc.T("quePrefieresComoSeVota"), Loc.T("quePrefieresModoAyuda"), Palette.NeonViolet);
            ChoiceChips<QuePrefieresModo>.Create(mode.Content, new[]
            {
                (QuePrefieresModo.SoloPreguntas, Loc.T("quePrefieresModoSolo")),
                (QuePrefieresModo.ContarVotos, Loc.T("quePrefieresModoContar"))
            }, draft.Modo, Palette.NeonViolet, v => draft.Modo = v);
            var minority = NeonSwitch.Create(mode.Content, Loc.T("quePrefieresPagaMinoria"), Loc.T("quePrefieresPagaMinoriaAyuda"), draft.PagaMinoria);
            minority.Changed += v => draft.PagaMinoria = v;

            var game = SetupSection.Create(content, Loc.T("masProbablePartida"), null, Palette.NeonMagenta);
            Ui.Text(game.Content, Loc.T("quePrefieresPensar"), TextRole.LabelLarge, Palette.TextMuted);
            ChoiceChips<int>.Create(game.Content,
                QuePrefieresConfig.PensarOptions.Select(n => (n, n == 0 ? Loc.T("sinCuentaAtras") : Loc.T("segundosCorto", ("n", n)))).ToList(),
                draft.SegundosPensar, Palette.NeonMagenta, v => draft.SegundosPensar = v);
            Ui.Text(game.Content, Loc.T("quePrefieresPreguntasPartida"), TextRole.LabelLarge, Palette.TextMuted);
            ChoiceChips<int>.Create(game.Content,
                QuePrefieresConfig.PreguntasOptions.Select(n => (n, n == 0 ? Loc.T("sinLimite") : n.ToString())).ToList(),
                draft.Preguntas, Palette.NeonMagenta, v => draft.Preguntas = v);

            Ui.Button(content, Loc.T("empezarPartida"), Start, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f);
        }

        public void Start()
        {
            var config = QuePrefieresConfig.Create(Levels.Selected, _module.Draft);
            if (config == null)
            {
                Snackbar.Show(Loc.T("errorSinNiveles"), Palette.NeonError);
                return;
            }
            _module.Flow.Iniciar(config);
            if (_module.Flow.Fase == QuePrefieresFase.Pregunta)
            {
                _module.GoToPhase();
                return;
            }
            Dialogs.Show(Loc.T("sinPreguntasTitulo"), Loc.T("sinPreguntasMensaje"), new[]
            {
                new DialogAction(Loc.T("aceptar"), (Action)null, ButtonStyle.Filled, Palette.NeonCyan)
            });
        }
    }

    /// <summary>Base con partida en curso: casa y atrás confirman; la barra lleva «Terminar».</summary>
    public abstract class QuePrefieresPlayScreen : AppScreen
    {
        protected QuePrefieresPlayScreen(QuePrefieresModule module) => Module = module;

        protected QuePrefieresModule Module { get; }
        protected QuePrefieresFlowController Flow => Module.Flow;
        protected abstract QuePrefieresFase Phase { get; }

        public sealed override void Build(RectTransform root)
        {
            var playing = Flow.Config != null && Flow.Fase == Phase;
            string subtitle = null;
            if (playing && Phase != QuePrefieresFase.Final)
            {
                subtitle = Flow.Config.Options.Preguntas > 0
                    ? Loc.T("preguntaXDeY", ("n", Flow.Numero), ("total", Flow.Config.Options.Preguntas))
                    : Loc.T("preguntaX", ("n", Flow.Numero));
            }
            var scaffold = ScreenScaffold.Create(root, Loc.T("quePrefieresTitulo"), subtitle);
            if (!playing)
            {
                GameUi.HomeButton(scaffold.AppBar, Router);
                var column = Ui.Column(scaffold.Body, 16f, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter);
                column.Sized(flexHeight: 1f);
                Ui.Text(column, Loc.T("noHayPartidaEnCurso"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                Ui.Button(column, Loc.T("configurarPartida"), () => Router.Go(QuePrefieresModule.SetupRoute));
                return;
            }
            if (Phase == QuePrefieresFase.Final)
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
            if (Flow.Config == null || Flow.Fase == QuePrefieresFase.Setup) return false;
            if (Flow.Fase == QuePrefieresFase.Final)
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

        protected static Color ColorOf(QuePrefieresOpcion option) => option == QuePrefieresOpcion.A ? Palette.NeonCyan : Palette.NeonMagenta;

        /// <summary>Tarjeta de una opción: la letra y el texto.</summary>
        protected static NeonPanel OptionPanel(Transform parent, QuePrefieresOpcion option, string text, float glow = 0.9f)
        {
            var color = ColorOf(option);
            var panel = Ui.Panel(parent, color, glow: glow, padding: new RectOffset(18, 18, 18, 18), spacing: 6f, alignment: TextAnchor.MiddleCenter);
            Ui.Text(panel.Content, option.ToString(), TextRole.TitleLarge, color, TextAlignmentOptions.Center, 0.8f, bold: true);
            Ui.Text(panel.Content, text, TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, 0.6f, color, bold: true);
            panel.name = Loc.T("opcionSemantica", ("opcion", option.ToString()), ("texto", text));
            return panel;
        }
    }

    /// <summary>
    /// La pregunta: A (cian) «o» B (magenta). Con tiempo para pensar, una cuenta atrás y «¡Votad!»; contando votos, un
    /// contador debajo de cada opción y «Ver resultado»; si no, «Siguiente pregunta». «Otra pregunta» la cambia.
    /// </summary>
    public sealed class QuePrefieresQuestionScreen : QuePrefieresPlayScreen
    {
        private readonly Countdown _countdown = new Countdown();
        private TMP_Text _count;
        private NeonStepper _a;
        private NeonStepper _b;

        public QuePrefieresQuestionScreen(QuePrefieresModule module) : base(module)
        {
            _countdown.SecondChanged += ShowCount;
            _countdown.Finished += Vote;
        }

        protected override QuePrefieresFase Phase => QuePrefieresFase.Pregunta;

        public Countdown Countdown => _countdown;
        public NeonStepper StepperA => _a;
        public NeonStepper StepperB => _b;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var content = Ui.Scroll(scaffold.Body, 12f, new RectOffset(20, 20, 12, 24));
            Ui.Text(content, Loc.T("quePrefieresPregunta"), TextRole.TitleLarge, Palette.TextMuted, TextAlignmentOptions.Center, bold: true);
            if (Flow.Current.Level == PromptLevel.Picante)
            {
                Ui.Text(content, Loc.T("nivelPicante"), TextRole.LabelLarge, Palette.NeonMagenta, TextAlignmentOptions.Center);
            }
            OptionPanel(content, QuePrefieresOpcion.A, Flow.Current.Text);
            if (Flow.ContarVotos) _a = Stepper(content, QuePrefieresOpcion.A);
            Ui.Text(content, Loc.T("quePrefieresO"), TextRole.HeadlineMedium, Palette.NeonViolet, TextAlignmentOptions.Center, 1f, bold: true);
            OptionPanel(content, QuePrefieresOpcion.B, Flow.Current.TextB);
            if (Flow.ContarVotos) _b = Stepper(content, QuePrefieresOpcion.B);

            _count = Ui.FittedText(content, string.Empty, 48f, Palette.NeonYellow, 1f);
            _count.gameObject.SetActive(false);
            Ui.Button(content, Loc.T(Flow.ContarVotos ? "quePrefieresVerResultado" : "quePrefieresSiguiente"), Reveal, ButtonStyle.Filled,
                Flow.ContarVotos ? IconId.Vote : IconId.ArrowForward, glow: 0.8f).Sized(height: 64f);
            Ui.Button(content, Loc.T("quePrefieresOtraPregunta"), Skip, ButtonStyle.Text, IconId.Refresh, Palette.NeonCyan);
            if (Flow.ConsumirAvisoMazo()) Snackbar.Show(Loc.T("preguntasAgotadas"), Palette.NeonYellow);

            var seconds = Flow.Config.Options.SegundosPensar;
            if (seconds > 0)
            {
                _count.gameObject.SetActive(true);
                _countdown.Start(seconds);
            }
        }

        private NeonStepper Stepper(Transform parent, QuePrefieresOpcion option)
        {
            var stepper = NeonStepper.Create(parent, 0, 0, QuePrefieresConfig.MaxVotos, n => Loc.T("votosContador", ("n", n)), ColorOf(option));
            stepper.SetButtonNames(Loc.T("quitarVoto", ("opcion", option.ToString())), Loc.T("sumarVoto", ("opcion", option.ToString())));
            stepper.Changed += v =>
            {
                var current = option == QuePrefieresOpcion.A ? Flow.VotosA : Flow.VotosB;
                Flow.Votar(option, v - current);
            };
            return stepper;
        }

        public override void Tick(float deltaTime)
        {
            if (Dialogs.IsOpen) return;
            _countdown.Tick(deltaTime);
        }

        public override void OnHide() => _countdown.Stop();

        /// <summary>Suma un voto a una opción (también lo usan los tests).</summary>
        public void AddVote(QuePrefieresOpcion option, int delta = 1)
        {
            Flow.Votar(option, delta);
            _a?.Set(Flow.VotosA, false);
            _b?.Set(Flow.VotosB, false);
        }

        public void Reveal()
        {
            Flow.Revelar();
            Module.Audio?.Play(Flow.ContarVotos ? AppSound.Chime : AppSound.Swipe, 0.6f);
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
            _count.text = Loc.T("quePrefieresPensadlo", ("n", seconds));
            _count.name = Loc.T("tiempoRestante", ("n", seconds));
            if (seconds <= 3) Module.Audio?.Play(AppSound.Beep, 0.8f);
        }

        private void Vote()
        {
            if (_count == null) return;
            _count.gameObject.SetActive(true);
            _count.text = Loc.T("quePrefieresVotad");
            _count.color = Palette.NeonMagenta;
            Ui.SetGlow((TextMeshProUGUI)_count, Palette.NeonMagenta, 1f);
            Module.Audio?.Play(AppSound.Buzzer, 0.8f);
            Haptics.Vibrate();
        }
    }

    /// <summary>Contando votos: porcentaje y votos de cada opción con su barra y quién paga (la minoría) o empate.</summary>
    public sealed class QuePrefieresResultScreen : QuePrefieresPlayScreen
    {
        public QuePrefieresResultScreen(QuePrefieresModule module) : base(module)
        {
        }

        protected override QuePrefieresFase Phase => QuePrefieresFase.Resultado;

        public string Verdict { get; private set; }

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var round = Flow.Ultima;
            var content = Ui.Scroll(scaffold.Body, 14f, new RectOffset(20, 20, 16, 24));
            Ui.Text(content, Loc.T("quePrefieresResultado"), TextRole.TitleLarge, Palette.TextMuted, TextAlignmentOptions.Center, bold: true);
            ResultPanel(content, QuePrefieresOpcion.A, round.Card.Text, round.VotosA, round.PorcentajeA, round.Total);
            ResultPanel(content, QuePrefieresOpcion.B, round.Card.TextB, round.VotosB, round.PorcentajeB, round.Total);
            Verdict = VerdictText(round, Flow.Config.Options.PagaMinoria);
            var color = round.Minoria.HasValue ? ColorOf(round.Minoria.Value) : Palette.NeonYellow;
            var verdict = Ui.Panel(content, color, glow: 0.8f, padding: new RectOffset(18, 18, 14, 14), alignment: TextAnchor.MiddleCenter);
            Ui.Text(verdict.Content, Verdict, TextRole.TitleMedium, Palette.TextPrimary, TextAlignmentOptions.Center, bold: true);
            Ui.Button(content, Loc.T("quePrefieresSiguiente"), Next, ButtonStyle.Filled, IconId.ArrowForward, glow: 0.8f).Sized(height: 64f);
        }

        public static string VerdictText(QuePrefieresRound round, bool minorityPays)
        {
            if (round.Total == 0) return Loc.T("quePrefieresSinVotos");
            if (!round.Minoria.HasValue) return Loc.T("quePrefieresEmpate");
            var option = round.Minoria.Value;
            var text = option == QuePrefieresOpcion.A ? round.Card.Text : round.Card.TextB;
            return Loc.T(minorityPays ? "quePrefieresPagaLaMinoria" : "quePrefieresMinoriaEs", ("opcion", option.ToString()), ("texto", text));
        }

        private static void ResultPanel(Transform parent, QuePrefieresOpcion option, string text, int votes, int percent, int total)
        {
            var color = ColorOf(option);
            var panel = Ui.Panel(parent, color, glow: 0.6f, padding: new RectOffset(16, 16, 14, 14), spacing: 8f);
            var row = Ui.Row(panel.Content, 10f, alignment: TextAnchor.MiddleLeft);
            Ui.Text(row, option.ToString(), TextRole.HeadlineSmall, color, TextAlignmentOptions.Center, 0.8f, bold: true).Sized(width: 32f);
            var label = Ui.Text(row, text, TextRole.TitleMedium, Palette.TextPrimary, maxLines: 3, bold: true);
            label.Sized(width: 0f, flexWidth: 1f);
            Ui.Text(row, Loc.T("porcentaje", ("n", percent)), TextRole.HeadlineSmall, color, TextAlignmentOptions.Right, 0.8f, bold: true, maxLines: 1);
            Bar(panel.Content, total == 0 ? 0f : votes / (float)total, color);
            Ui.Text(panel.Content, Loc.T("votosContador", ("n", votes)), TextRole.BodySmall, Palette.TextMuted);
            panel.name = Loc.T("resultadoSemantica", ("opcion", option.ToString()), ("texto", text), ("porcentaje", percent));
        }

        /// <summary>Barra de porcentaje: fondo apagado y relleno del color de la opción.</summary>
        private static void Bar(Transform parent, float fraction, Color color)
        {
            var track = new GameObject("Bar", typeof(RectTransform));
            track.transform.SetParent(parent, false);
            ((RectTransform)track.transform).Sized(height: 14f, flexWidth: 1f);
            var back = track.AddComponent<Image>();
            back.sprite = NeonSprites.RoundedRect(7f);
            back.type = Image.Type.Sliced;
            back.color = Palette.SurfaceHighest;
            back.raycastTarget = false;
            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(track.transform, false);
            var rect = (RectTransform)fill.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = fill.AddComponent<Image>();
            image.sprite = NeonSprites.RoundedRect(7f);
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
        }

        public void Next()
        {
            Flow.Siguiente();
            Module.Audio?.Play(AppSound.Swipe, 0.6f);
            Module.GoToPhase();
        }
    }

    /// <summary>Final: cuántas preguntas y, contando votos, la más dividida, la más unánime y todas con sus porcentajes.</summary>
    public sealed class QuePrefieresEndScreen : QuePrefieresPlayScreen
    {
        public QuePrefieresEndScreen(QuePrefieresModule module) : base(module)
        {
        }

        protected override QuePrefieresFase Phase => QuePrefieresFase.Final;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(20, 20, 20, 24));
            var panel = Ui.Panel(content, Palette.NeonCyan, glow: 1f, padding: new RectOffset(24, 24, 24, 24), spacing: 10f, alignment: TextAnchor.UpperCenter);
            PulseGlow.On(panel, 0.8f, 1.3f);
            Ui.Icon(panel.Content, IconId.AltRoute, 64f, Palette.NeonCyan, 1f);
            Ui.Text(panel.Content, Loc.T("partidaTerminada"), TextRole.HeadlineSmall, Palette.TextPrimary, TextAlignmentOptions.Center, bold: true);
            Ui.Text(panel.Content, Loc.T("quePrefieresPreguntasJugadas", ("n", Flow.Rondas.Count)), TextRole.BodyMedium, Palette.TextMuted,
                TextAlignmentOptions.Center);
            var voted = Flow.Rondas.Where(r => r.Total > 0).ToList();
            if (Flow.ContarVotos && voted.Count > 0)
            {
                Highlight(content, "quePrefieresMasDividida", Flow.MasDividida, Palette.NeonYellow);
                Highlight(content, "quePrefieresMasUnanime", Flow.MasUnanime, Palette.NeonGreen);
                Ui.Text(content, Loc.T("quePrefieresTodas"), TextRole.TitleMedium, Palette.NeonCyan, glow: 0.6f, bold: true);
                ScoreList.Create(content, Rows(Flow));
            }
            Ui.Button(content, Loc.T("jugarOtra"), Module.PlayAgain, ButtonStyle.Filled, IconId.Refresh, glow: 0.8f);
            Ui.Button(content, Loc.T("cambiarAjustes"), Module.ResetToSetup, ButtonStyle.Outlined, IconId.Tune);
            Ui.Button(content, Loc.T("volverAlMenu"), Module.ResetToMenu, ButtonStyle.Text, IconId.Home);
        }

        public override void OnShow()
        {
            if (Flow.Fase == QuePrefieresFase.Final) Module.Audio?.Play(AppSound.GameOver, 0.7f);
        }

        private static void Highlight(Transform parent, string titleKey, QuePrefieresRound round, Color color)
        {
            if (round == null) return;
            var panel = Ui.Panel(parent, color, glow: 0.7f, padding: new RectOffset(18, 18, 14, 14), spacing: 6f);
            Ui.Text(panel.Content, Loc.T(titleKey), TextRole.LabelLarge, color, bold: true);
            Ui.Text(panel.Content, Loc.T("quePrefieresParejaOpciones", ("a", round.Card.Text), ("b", round.Card.TextB)), TextRole.TitleMedium,
                Palette.TextPrimary, bold: true);
            Ui.Text(panel.Content, Loc.T("quePrefieresParejaPorcentajes", ("a", round.PorcentajeA), ("b", round.PorcentajeB)), TextRole.BodyMedium,
                Palette.TextMuted);
        }

        /// <summary>Todas las preguntas votadas, en el orden en que salieron.</summary>
        public static List<ScoreRow> Rows(QuePrefieresFlowController flow) =>
            flow.Rondas.Where(r => r.Total > 0).Select((r, i) => new ScoreRow(i + 1,
                Loc.T("quePrefieresParejaOpciones", ("a", r.Card.Text), ("b", r.Card.TextB)),
                r.Minoria.HasValue ? (r.Minoria.Value == QuePrefieresOpcion.A ? "B" : "A") : "=",
                Loc.T("quePrefieresParejaPorcentajes", ("a", r.PorcentajeA), ("b", r.PorcentajeB)),
                r.Minoria.HasValue ? ColorOf(r.Minoria.Value == QuePrefieresOpcion.A ? QuePrefieresOpcion.B : QuePrefieresOpcion.A) : Palette.NeonYellow))
                .ToList();
    }
}
