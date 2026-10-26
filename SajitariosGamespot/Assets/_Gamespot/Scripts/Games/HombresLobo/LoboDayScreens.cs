using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.HombresLobo.Domain;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.HombresLobo
{
    /// <summary>
    /// El amanecer (V080): lo que el game master lee en voz alta (quién ha muerto y, según la regla, su carta; el oso),
    /// el evento de luna, lo que ha pasado en secreto durante la noche (solo para él) y lo pendiente (el disparo del
    /// cazador, el sucesor del capitán). Después, «Empezar el día».
    /// </summary>
    public sealed class LoboDawnScreen : LoboMasterScreen
    {
        /// <summary>Lo que pasa de noche sin que nadie se entere: el game master lo ve, pero no lo dice.</summary>
        private static readonly LoboEventoTipo[] Secret =
        {
            LoboEventoTipo.Protegido, LoboEventoTipo.Salva, LoboEventoTipo.Infecta, LoboEventoTipo.AncianoResiste,
            LoboEventoTipo.NinoSalvajeSeConvierte
        };

        private NeonButton _start;

        public LoboDawnScreen(HombresLoboModule module) : base(module)
        {
        }

        protected override LoboFase Phase => LoboFase.Amanecer;

        public bool CanStartDay => _start != null && _start.Interactable;

        protected override string Subtitle() => Loc.T("loboAmanecerN", ("n", Game.Ronda));

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var content = Ui.Scroll(scaffold.Body, 12f, new RectOffset(16, 16, 12, 24));
            var head = Ui.Row(content, 12f, alignment: TextAnchor.MiddleLeft);
            Ui.Icon(head, IconId.WbSunny, 44f, Palette.NeonYellow, 0.9f);
            Ui.Text(head, Loc.T("loboAmanece"), TextRole.HeadlineSmall, Palette.NeonYellow, glow: 0.6f, bold: true, maxLines: 2).Sized(width: 0f, flexWidth: 1f);

            // Los de la noche, para la frase; debajo, también los que mueran ahora (el disparo del cazador).
            var night = Tonight().Where(e => e.Tipo == LoboEventoTipo.Muere && e.Jugador.HasValue).Select(e => e.Jugador.Value).Distinct()
                .Where(i => !Game.Jugadores[i].Vivo).ToList();
            var deaths = Game.MuertesRecientes.Where(i => !Game.Jugadores[i].Vivo).ToList();
            var read = ReadAloud(content, Loc.T("loboNarrarAmanecer"));
            if (night.Count == 0) ReadLine(read, Loc.T("loboNadieMurio"), Palette.NeonGreen);
            else ReadLine(read, Loc.T("loboHanMuerto", ("n", night.Count), ("nombres", LoboTexts.Names(Game, night))));
            if (Game.OsoGrune.HasValue) ReadLine(read, Loc.T(Game.OsoGrune.Value ? "loboOsoGrune" : "loboOsoNoGrune"));
            if (Tonight().Any(e => e.Tipo == LoboEventoTipo.PoderesPerdidos)) ReadLine(read, Loc.T("loboEventoPoderesPerdidos"), Palette.NeonYellow);
            foreach (var index in deaths) DeathRow(content, index);
            MoonPanel(content);

            var secrets = Tonight().Where(e => Secret.Contains(e.Tipo)).ToList();
            if (secrets.Count > 0)
            {
                var panel = Ui.Panel(content, Palette.NeonViolet, glow: 0.3f, padding: new RectOffset(14, 14, 12, 12), spacing: 4f);
                Ui.Text(panel.Content, Loc.T("loboSoloParaTiTitulo"), TextRole.LabelMedium, Palette.NeonVioletText, bold: true);
                foreach (var e in secrets) Ui.Text(panel.Content, LoboTexts.EventText(Game, e), TextRole.BodyMedium, Palette.TextMuted);
            }

            var pending = PendingSection(content);
            _start = Ui.Button(content, Loc.T("loboEmpezarDia"), StartDay, ButtonStyle.Filled, IconId.WbSunny, Palette.NeonYellow, 0.8f);
            _start.Sized(height: 64f);
            _start.Interactable = !pending;
        }

        private IEnumerable<LoboEvento> Tonight() => Game.Registro.Where(e => e.Ronda == Game.Ronda && e.Noche);

        public void StartDay()
        {
            if (!Game.EmpezarDia()) return;
            Module.Audio?.Play(AppSound.Swipe, 0.5f);
            Refresh();
        }
    }

    /// <summary>
    /// El día (V080): si hace falta, primero se elige capitán; después, el debate con tiempo (empezar, pausar, +30 s) y
    /// la votación: los votos a mano alzada con + y − (el capitán vale doble, el marcado por el cuervo empieza con dos,
    /// el tonto descubierto no vota) y linchar al más votado, empate (segunda votación, chivo o luna de sangre) o nadie.
    /// Lo pendiente (sirvienta, cazador, sucesor) va antes que nada. Con el linchamiento hecho, el juez puede pedir otra
    /// votación y se pasa a la noche.
    /// </summary>
    public sealed class LoboDayScreen : LoboMasterScreen
    {
        private const int MasTiempo = 30;

        private readonly Countdown _debate = new Countdown();
        private readonly Dictionary<int, int> _votes = new Dictionary<int, int>();
        private readonly Dictionary<int, NeonStepper> _steppers = new Dictionary<int, NeonStepper>();
        private readonly List<int> _captain = new List<int>();
        private List<int> _candidates = new List<int>();
        private bool _debateStarted;
        private TMP_Text _timer;
        private NeonButton _play;
        private NeonButton _captainButton;
        private NeonButton _lynch;
        private NeonButton _tie;
        private TMP_Text _result;

        public LoboDayScreen(HombresLoboModule module) : base(module)
        {
            _debate.SecondChanged += ShowSeconds;
            _debate.Finished += () =>
            {
                Module.Audio?.Play(AppSound.Buzzer);
                Haptics.Vibrate();
                Snackbar.Show(Loc.T("loboTiempoAcabado"), Palette.NeonYellow);
                RefreshPlay();
            };
        }

        protected override LoboFase Phase => LoboFase.Dia;

        public Countdown Debate => _debate;

        /// <summary>A quién se puede votar ahora (en la segunda votación, solo a los empatados).</summary>
        public IReadOnlyList<int> Candidates => _candidates;

        public string ResultText => _result != null ? _result.text : null;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            _timer = null;
            _play = null;
            _captainButton = null;
            _lynch = null;
            _tie = null;
            _result = null;
            _steppers.Clear();
            _candidates = new List<int>();
            var content = Ui.Scroll(scaffold.Body, 12f, new RectOffset(16, 16, 12, 24));
            MoonPanel(content);
            Today(content);
            if (PendingSection(content)) return;
            if (Game.NecesitaCapitan)
            {
                CaptainSection(content);
                return;
            }
            if (!Game.LinchamientoHecho)
            {
                if (Game.HayDebate) DebateSection(content);
                VoteSection(content);
                return;
            }
            if (Game.JuezDisponible) JudgeSection(content);
            var night = Ui.Button(content, Loc.T("loboEmpezarNoche"), StartNight, ButtonStyle.Filled, IconId.Bedtime, Palette.NeonViolet, 0.8f);
            night.Sized(height: 64f);
        }

        public override void Tick(float deltaTime)
        {
            if (Dialogs.IsOpen) return;
            _debate.Tick(deltaTime);
        }

        public override void OnHide() => _debate.Stop();

        // ------------------------------------------------------------------ Lo que ha pasado hoy

        /// <summary>Lo de hoy, para anunciarlo: los muertos (con su carta según la regla), el tonto, el capitán, nadie.</summary>
        private void Today(Transform parent)
        {
            var shown = Game.Registro.Where(e => e.Ronda == Game.Ronda && !e.Noche && IsPublic(e.Tipo)).ToList();
            if (shown.Count == 0) return;
            Ui.Text(parent, Loc.T("loboHoy"), TextRole.LabelLarge, Palette.TextMuted);
            // Si la sirvienta se quedó la carta del linchado, esa carta no se enseña.
            var taken = new HashSet<int>(Game.Registro.Where(e => e.Ronda == Game.Ronda && e.Tipo == LoboEventoTipo.SirvientaToma && e.Otro.HasValue)
                .Select(e => e.Otro.Value));
            var dead = new HashSet<int>();
            foreach (var e in shown)
            {
                switch (e.Tipo)
                {
                    case LoboEventoTipo.Muere:
                        if (e.Jugador.HasValue && dead.Add(e.Jugador.Value)) DeathRow(parent, e.Jugador.Value, taken.Contains(e.Jugador.Value) ? false : (bool?)null);
                        break;
                    case LoboEventoTipo.TontoSeSalva:
                        InfoPanel(parent, LoboTexts.EventText(Game, e), Palette.NeonYellow, LoboUi.Icon(LoboRol.TontoDelPueblo));
                        break;
                    case LoboEventoTipo.Capitan:
                    case LoboEventoTipo.Sucesor:
                        InfoPanel(parent, LoboTexts.EventText(Game, e), Palette.NeonYellow, IconId.MilitaryTech);
                        break;
                    default:
                        InfoPanel(parent, LoboTexts.EventText(Game, e), Palette.NeonCyan, IconId.Lightbulb);
                        break;
                }
            }
        }

        private static bool IsPublic(LoboEventoTipo tipo) =>
            tipo == LoboEventoTipo.Muere || tipo == LoboEventoTipo.TontoSeSalva || tipo == LoboEventoTipo.NadieMuere || tipo == LoboEventoTipo.Capitan
            || tipo == LoboEventoTipo.Sucesor || tipo == LoboEventoTipo.PoderesPerdidos;

        // ------------------------------------------------------------------ Capitán

        private void CaptainSection(Transform parent)
        {
            var panel = Ui.Panel(parent, Palette.NeonYellow, glow: 0.6f, padding: new RectOffset(14, 14, 14, 14), spacing: 10f);
            var head = Ui.Row(panel.Content, 10f, alignment: TextAnchor.MiddleLeft);
            Ui.Icon(head, IconId.MilitaryTech, 32f, Palette.NeonYellow, 0.8f);
            Ui.Text(head, Loc.T("loboCapitanTitulo"), TextRole.TitleMedium, Palette.NeonYellow, bold: true).Sized(width: 0f, flexWidth: 1f);
            Ui.Text(panel.Content, Loc.T("loboCapitanAyuda"), TextRole.BodyMedium, Palette.TextPrimary);
            _captain.Clear();
            PlayerChips(panel.Content, Game.Vivos.Select(p => p.Index), _captain, 1, Palette.NeonYellow, RefreshCaptain, false);
            _captainButton = Ui.Button(panel.Content, Loc.T("loboNombrarCapitan"), () => ElectCaptain(_captain.FirstOrDefault()), ButtonStyle.Filled,
                IconId.MilitaryTech, Palette.NeonYellow, 0.8f);
            RefreshCaptain();
        }

        private void RefreshCaptain()
        {
            if (_captainButton != null) _captainButton.Interactable = _captain.Count == 1;
        }

        /// <summary>El pueblo elige capitán (también lo usan los tests).</summary>
        public void ElectCaptain(int index)
        {
            if (Game.ElegirCapitan(index)) Refresh();
        }

        // ------------------------------------------------------------------ Debate

        private void DebateSection(Transform parent)
        {
            var panel = Ui.Panel(parent, Palette.NeonCyan, glow: 0.5f, padding: new RectOffset(14, 14, 14, 14), spacing: 8f, alignment: TextAnchor.UpperCenter);
            Ui.Text(panel.Content, Loc.T("loboDebate"), TextRole.TitleMedium, Palette.NeonCyan, TextAlignmentOptions.Center, bold: true);
            Ui.Text(panel.Content, Loc.T("loboDebateAyuda"), TextRole.BodySmall, Palette.TextMuted, TextAlignmentOptions.Center);
            _timer = Ui.FittedText(panel.Content, string.Empty, 56f, Palette.NeonCyan, 1f);
            var row = Ui.Row(panel.Content, 8f, alignment: TextAnchor.MiddleCenter);
            _play = Ui.Button(row, Loc.T("loboEmpezarDebate"), ToggleDebate, ButtonStyle.Filled, IconId.Timer, Palette.NeonCyan, 0.6f);
            _play.Sized(width: 0f, flexWidth: 1f);
            Ui.Button(row, Loc.T("loboMasTiempo", ("n", MasTiempo)), () => AddTime(MasTiempo), ButtonStyle.Outlined, IconId.Add, Palette.NeonCyan)
                .Sized(width: 0f, flexWidth: 1f);
            ShowSeconds(_debateStarted ? _debate.SecondsShown : Game.Rules.DebateSegundos);
            RefreshPlay();
        }

        /// <summary>Empieza, pausa o sigue el debate (y lo vuelve a empezar si se ha acabado).</summary>
        public void ToggleDebate()
        {
            if (!_debateStarted || _debate.IsFinished)
            {
                _debateStarted = true;
                _debate.Start(Game.Rules.DebateSegundos);
            }
            else if (_debate.Running) _debate.Pause();
            else _debate.Resume();
            RefreshPlay();
        }

        public void AddTime(int seconds)
        {
            if (!_debateStarted)
            {
                _debateStarted = true;
                _debate.Start(Game.Rules.DebateSegundos + seconds);
                _debate.Pause();
            }
            else _debate.Add(seconds);
            RefreshPlay();
        }

        private void RefreshPlay()
        {
            if (_play == null) return;
            var key = !_debateStarted ? "loboEmpezarDebate" : _debate.IsFinished ? "loboOtraVez" : _debate.Running ? "loboPausar" : "loboSeguir";
            _play.SetLabel(Loc.T(key));
        }

        private void ShowSeconds(int seconds)
        {
            if (_timer == null) return;
            _timer.text = $"{seconds / 60}:{seconds % 60:00}";
            var color = !_debateStarted || seconds > 30 ? Palette.NeonCyan : seconds > 10 ? Palette.NeonYellow : Palette.NeonRed;
            _timer.color = color;
            Ui.SetGlow((TextMeshProUGUI)_timer, color, 1f);
            _timer.name = Loc.T("tiempoRestante", ("n", seconds));
            if (_debate.Running && seconds <= 5 && seconds > 0) Module.Audio?.Play(AppSound.Beep, 0.8f);
        }

        // ------------------------------------------------------------------ Votación

        private void VoteSection(Transform parent)
        {
            var second = Game.Empatados.Count > 0;
            _candidates = (second ? Game.Empatados : Game.Vivos.Select(p => p.Index)).Where(i => Game.Jugadores[i].Vivo).ToList();
            foreach (var key in _votes.Keys.Where(k => !_candidates.Contains(k)).ToList()) _votes.Remove(key);

            var panel = Ui.Panel(parent, Palette.NeonMagenta, glow: 0.5f, padding: new RectOffset(14, 14, 14, 14), spacing: 8f);
            var head = Ui.Row(panel.Content, 10f, alignment: TextAnchor.MiddleLeft);
            Ui.Icon(head, IconId.Vote, 30f, Palette.NeonMagenta, 0.8f);
            Ui.Text(head, Loc.T(second ? "loboSegundaVotacion" : "loboVotacion"), TextRole.TitleMedium, Palette.NeonMagenta, bold: true)
                .Sized(width: 0f, flexWidth: 1f);
            Ui.Text(panel.Content, second ? Loc.T("loboSegundaVotacionAyuda", ("nombres", LoboTexts.Names(Game, _candidates))) : Loc.T("loboVotacionAyuda"),
                TextRole.BodyMedium, Palette.TextPrimary);
            if (Game.LinchamientosPermitidos > 1)
            {
                Ui.Text(panel.Content, Loc.T("loboVotacionesQuedan", ("n", Game.LinchamientosRestantes)), TextRole.BodySmall, Palette.NeonVioletText);
            }
            var captain = Game.Capitan;
            if (captain != null && Game.VotoDobleCapitan)
            {
                Ui.Text(panel.Content, Loc.T("loboVotoCapitanDoble", ("nombre", captain.Name)), TextRole.BodySmall, Palette.NeonYellow);
            }
            var silent = Game.Vivos.Where(p => !p.PuedeVotar).Select(p => p.Index).ToList();
            if (silent.Count > 0) Ui.Text(panel.Content, Loc.T("loboNoVotan", ("nombres", LoboTexts.Names(Game, silent))), TextRole.BodySmall, Palette.TextMuted);

            var max = Game.Vivos.Count() * 2;
            foreach (var index in _candidates) VoteRow(panel.Content, index, max);
            _result = Ui.Text(panel.Content, string.Empty, TextRole.TitleSmall, Palette.TextPrimary, TextAlignmentOptions.Center, bold: true);
            _lynch = Ui.Button(panel.Content, Loc.T("loboLinchar"), Lynch, ButtonStyle.Filled, IconId.Gavel, Palette.NeonRed, 0.8f);
            _lynch.Sized(height: 60f);
            _tie = Ui.Button(panel.Content, Loc.T("loboHayEmpate"), DeclareTie, ButtonStyle.Outlined, IconId.CompareArrows, Palette.NeonYellow);
            Ui.Button(panel.Content, Loc.T("loboNadieHoy"), NoLynch, ButtonStyle.Text, IconId.Block, Palette.TextMuted);
            RefreshVotes();
        }

        private void VoteRow(Transform parent, int index, int max)
        {
            var player = Game.Jugadores[index];
            var bonus = Bonus(index);
            var row = Ui.Row(parent, 8f, alignment: TextAnchor.MiddleLeft);
            var texts = Ui.Column(row, 0f, alignment: TextAnchor.MiddleLeft);
            texts.Sized(width: 0f, flexWidth: 1f);
            Ui.Text(texts, player.Name, TextRole.TitleSmall, Palette.TextPrimary, bold: true, maxLines: 1);
            if (bonus > 0) Ui.Text(texts, Loc.T("loboVotosCuervo", ("n", bonus)), TextRole.BodySmall, Palette.NeonVioletText);
            var stepper = NeonStepper.Create(row, _votes.TryGetValue(index, out var v) ? v : 0, 0, max, n => (n + bonus).ToString(), Palette.NeonMagenta);
            stepper.Sized(width: 156f, flexWidth: 0f);
            stepper.SetButtonNames(Loc.T("loboQuitarVoto", ("nombre", player.Name)), Loc.T("loboDarVoto", ("nombre", player.Name)));
            stepper.Changed += n => SetVotes(index, n);
            _steppers[index] = stepper;
        }

        /// <summary>El cuervo marcó a alguien esta noche: empieza con dos votos.</summary>
        private int Bonus(int index) => Game.CuervoMarcado == index ? 2 : 0;

        private int Total(int index) => (_votes.TryGetValue(index, out var v) ? v : 0) + Bonus(index);

        /// <summary>Los más votados (vacío si nadie tiene votos).</summary>
        public IReadOnlyList<int> Leaders()
        {
            var top = _candidates.Select(Total).DefaultIfEmpty(0).Max();
            return top <= 0 ? new List<int>() : _candidates.Where(i => Total(i) == top).ToList();
        }

        /// <summary>Apunta los votos a mano alzada de un jugador (también lo usan los tests).</summary>
        public void SetVotes(int index, int votes)
        {
            if (!_candidates.Contains(index)) return;
            _votes[index] = Mathf.Max(0, votes);
            if (_steppers.TryGetValue(index, out var stepper)) stepper.Set(_votes[index], false);
            RefreshVotes();
        }

        private void RefreshVotes()
        {
            if (_result == null) return;
            var leaders = Leaders();
            var top = leaders.Count > 0 ? Total(leaders[0]) : 0;
            if (leaders.Count == 0) _result.text = Loc.T("loboSinVotos");
            else if (leaders.Count == 1) _result.text = Loc.T("loboMasVotado", ("nombre", Game.Jugadores[leaders[0]].Name), ("n", top));
            else _result.text = Loc.T("loboEmpateEntre", ("nombres", LoboTexts.Names(Game, leaders)), ("n", top));
            _lynch.Interactable = leaders.Count == 1;
            _lynch.SetLabel(leaders.Count == 1 ? Loc.T("loboLincharA", ("nombre", Game.Jugadores[leaders[0]].Name)) : Loc.T("loboLinchar"));
            _tie.gameObject.SetActive(leaders.Count > 1);
        }

        /// <summary>Lincha al más votado.</summary>
        public void Lynch()
        {
            var leaders = Leaders();
            if (leaders.Count != 1 || !Game.Linchar(leaders[0])) return;
            _votes.Clear();
            Module.Audio?.Play(AppSound.Wrong, 0.6f);
            Refresh();
        }

        /// <summary>Empate entre los más votados: lo resuelve el motor (segunda votación, chivo, luna de sangre o nadie).</summary>
        public void DeclareTie()
        {
            var leaders = Leaders();
            if (leaders.Count < 2 || !Game.Empate(leaders)) return;
            _votes.Clear();
            Refresh();
        }

        public void NoLynch()
        {
            if (!Game.NadieLinchado()) return;
            _votes.Clear();
            Refresh();
        }

        // ------------------------------------------------------------------ Juez y noche

        private void JudgeSection(Transform parent)
        {
            var judge = Game.Vivos.FirstOrDefault(p => p.Rol == LoboRol.JuezTartamudo);
            var panel = Ui.Panel(parent, Palette.NeonCyan, glow: 0.4f, padding: new RectOffset(14, 14, 12, 12), spacing: 8f);
            Ui.Text(panel.Content, Loc.T("loboJuezAyuda", ("nombre", judge != null ? judge.Name : "-")), TextRole.BodyMedium, Palette.TextPrimary);
            Ui.Button(panel.Content, Loc.T("loboJuezBoton"), Judge, ButtonStyle.Outlined, IconId.Gavel, Palette.NeonCyan);
        }

        public void Judge()
        {
            if (Game.PedirSegundaVotacion()) Refresh();
        }

        public void StartNight()
        {
            if (!Game.EmpezarNoche()) return;
            Module.Audio?.Play(AppSound.Swipe, 0.5f);
            Refresh();
        }
    }

    /// <summary>
    /// El final (V080): quién gana (o que no gana nadie si el game master la da por terminada), las últimas muertes para
    /// anunciarlas, todas las cartas con los ganadores resaltados, y jugar otra con las mismas cartas, cambiar los
    /// ajustes, ver el registro o volver al menú.
    /// </summary>
    public sealed class LoboEndScreen : LoboMasterScreen
    {
        public LoboEndScreen(HombresLoboModule module) : base(module)
        {
        }

        protected override LoboFase Phase => LoboFase.Final;

        protected override string Subtitle() => Loc.T("loboFinal");

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            var content = Ui.Scroll(scaffold.Body, 12f, new RectOffset(16, 16, 16, 24));
            var victory = Game.Victoria;
            var color = victory != null ? LoboTexts.WinnerColor(victory.Ganador) : Palette.TextMuted;
            var panel = Ui.Panel(content, color, glow: 1f, padding: new RectOffset(24, 24, 24, 24), spacing: 10f, alignment: TextAnchor.UpperCenter);
            PulseGlow.On(panel, 0.8f, 1.3f);
            Ui.Icon(panel.Content, IconId.Trophy, 64f, color, 1f);
            Ui.FittedText(panel.Content, victory != null ? Loc.T("loboGanador" + victory.Ganador) : Loc.T("loboSinGanador"), 36f, color, 1f, lines: 2);
            if (victory != null && victory.Jugadores.Count > 0)
            {
                Ui.Text(panel.Content, LoboTexts.Names(Game, victory.Jugadores), TextRole.TitleSmall, Palette.TextPrimary, TextAlignmentOptions.Center);
            }

            var last = LastDeaths();
            if (last.Count > 0)
            {
                Ui.Text(content, Loc.T("loboUltimasMuertes"), TextRole.LabelLarge, Palette.TextMuted);
                foreach (var index in last) DeathRow(content, index, true);
            }
            Ui.Text(content, Loc.T("loboTodasLasCartas"), TextRole.LabelLarge, Palette.TextMuted);
            foreach (var player in Game.Jugadores) CardRow(content, player, victory);

            Ui.Button(content, Loc.T("jugarOtra"), PlayAgain, ButtonStyle.Filled, IconId.Refresh, glow: 0.8f);
            Ui.Button(content, Loc.T("cambiarAjustes"), Module.ResetToSetup, ButtonStyle.Outlined, IconId.Tune);
            Ui.Button(content, Loc.T("loboVerRegistro"), () => Router.Push(new LoboLogScreen(Module)), ButtonStyle.Outlined, IconId.History);
            Ui.Button(content, Loc.T("volverAlMenu"), Module.ResetToMenu, ButtonStyle.Text, IconId.Home);
        }

        public override void OnShow()
        {
            if (Game.Fase == LoboFase.Final) Module.Audio?.Play(AppSound.GameOver, 0.7f);
        }

        /// <summary>
        /// Los muertos de la última tanda (la noche o el día en que acabó), que aún no se han anunciado. Las muertes de
        /// la noche se apuntan con la noche; las del amanecer y el día, con el día.
        /// </summary>
        private List<int> LastDeaths()
        {
            var lastDeath = Game.Registro.LastOrDefault(e => e.Tipo == LoboEventoTipo.Muere);
            if (lastDeath == null || lastDeath.Ronda != Game.Ronda) return new List<int>();
            return Game.Registro.Where(e => e.Tipo == LoboEventoTipo.Muere && e.Ronda == lastDeath.Ronda && e.Noche == lastDeath.Noche && e.Jugador.HasValue)
                .Select(e => e.Jugador.Value).Distinct().Where(i => !Game.Jugadores[i].Vivo).ToList();
        }

        private void CardRow(Transform parent, LoboPlayer player, LoboVictoria victory)
        {
            var winner = victory != null && victory.Jugadores.Contains(player.Index);
            var accent = winner ? LoboTexts.WinnerColor(victory.Ganador) : Palette.TextMuted;
            var panel = Ui.Panel(parent, accent, glow: winner ? 0.6f : 0f, padding: new RectOffset(12, 10, 8, 8));
            var row = Ui.Row(panel.Content, 10f, alignment: TextAnchor.MiddleLeft);
            Ui.Icon(row, LoboUi.Icon(player.Rol), 28f, LoboUi.Color(player.Rol), 0.5f);
            var texts = Ui.Column(row, 2f, alignment: TextAnchor.MiddleLeft);
            texts.Sized(width: 0f, flexWidth: 1f);
            Ui.Text(texts, Loc.T("loboJugadorConRol", ("nombre", player.Name), ("rol", LoboUi.Name(player.Rol))), TextRole.TitleSmall,
                player.Vivo ? Palette.TextPrimary : Palette.TextMuted, bold: true, maxLines: 2);
            var parts = new List<string>
            {
                player.Vivo
                    ? Loc.T("loboSigueVivo")
                    : Loc.T("loboMuertoPor", ("causa", player.Causa.HasValue ? LoboUi.Cause(player.Causa.Value) : string.Empty), ("n", player.RondaMuerte ?? 0))
            };
            if (player.Rol != player.RolInicial) parts.Add(Loc.T("loboEraAntes", ("rol", LoboUi.Name(player.RolInicial))));
            if (player.Infectado) parts.Add(Loc.T("loboInfectado"));
            if (player.Pareja.HasValue) parts.Add(Loc.T("loboEnamoradoDe", ("nombre", Game.Jugadores[player.Pareja.Value].Name)));
            Ui.Text(texts, string.Join(" · ", parts), TextRole.BodySmall, Palette.TextMuted, maxLines: 3);
            if (winner) Ui.Icon(row, IconId.Trophy, 22f, accent, 0.6f);
        }

        /// <summary>Otra partida con los mismos jugadores y las mismas cartas (también lo usan los tests).</summary>
        public void PlayAgain() => Module.PlayAgain();
    }
}
