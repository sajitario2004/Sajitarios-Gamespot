using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.HombresLobo.Domain;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.HombresLobo
{
    /// <summary>
    /// Base de la consola del game master (V079): en la barra, deshacer, los jugadores (con sus cartas, para matar o
    /// revivir a mano) y el registro. Casa y atrás piden confirmación. Si la fase cambia, va a su pantalla.
    /// </summary>
    public abstract class LoboMasterScreen : AppScreen
    {
        /// <summary>Letras que caben en un chip de media fila sin encoger demasiado.</summary>
        private const int MaxChipLabel = 26;

        protected LoboMasterScreen(HombresLoboModule module) => Module = module;

        protected HombresLoboModule Module { get; }
        protected LoboGame Game => Module.Game;
        protected abstract LoboFase Phase { get; }

        public sealed override void Build(RectTransform root)
        {
            var playing = Game.Config != null && Game.Fase == Phase;
            var scaffold = ScreenScaffold.Create(root, Loc.T("loboTitulo"), playing ? Subtitle() : null);
            if (!playing)
            {
                GameUi.HomeButton(scaffold.AppBar, Router);
                var column = Ui.Column(scaffold.Body, 16f, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter);
                column.Sized(flexHeight: 1f);
                Ui.Text(column, Loc.T("noHayPartidaEnCurso"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                Ui.Button(column, Loc.T("configurarPartida"), () => Router.Go(HombresLoboModule.SetupRoute));
                return;
            }
            if (Phase == LoboFase.Final) GameUi.HomeButton(scaffold.AppBar, Router);
            else GameUi.HomeButton(scaffold.AppBar, Router, () => Game.Reiniciar());
            if (Phase != LoboFase.Final)
            {
                var undo = scaffold.AppBar.AddAction(IconId.Undo, Undo);
                undo.name = Loc.T("deshacer");
                undo.Interactable = Game.PuedeDeshacer;
            }
            scaffold.AppBar.AddAction(IconId.Groups, () => Router.Push(new LoboPlayersScreen(Module, Refresh))).name = Loc.T("loboJugadoresMaster");
            scaffold.AppBar.AddAction(IconId.History, () => Router.Push(new LoboLogScreen(Module))).name = Loc.T("loboRegistro");
            BuildPhase(scaffold);
        }

        protected virtual string Subtitle() => Loc.T(Phase == LoboFase.Noche ? "loboNocheN" : "loboDiaN", ("n", Game.Ronda));

        protected abstract void BuildPhase(ScreenScaffold scaffold);

        public override bool OnBack()
        {
            if (Game.Config == null) return false;
            if (Game.Fase == LoboFase.Final)
            {
                Module.ResetToMenu();
                return true;
            }
            AbandonGameDialog.Show(Module.ResetToSetup);
            return true;
        }

        /// <summary>Deshace la última acción del game master.</summary>
        public void Undo()
        {
            if (!Game.Deshacer()) return;
            Snackbar.Show(Loc.T("loboDeshecho"));
            Refresh();
        }

        /// <summary>Tras cambiar la partida: la misma pantalla de nuevo o la de la fase nueva.</summary>
        protected void Refresh()
        {
            if (Module.RouteForPhase() != Path)
            {
                Module.GoToPhase();
                return;
            }
            Root.ClearChildren();
            Build(Root);
        }

        /// <summary>Rejilla de chips de jugadores para elegir; devuelve los chips por índice.</summary>
        protected Dictionary<int, NeonChip> PlayerChips(Transform parent, IEnumerable<int> candidates, ICollection<int> picked, int max, Color accent,
            Action changed, bool showRoles = true)
        {
            var labels = candidates.ToDictionary(i => i, i => showRoles
                ? Loc.T("loboJugadorConRol", ("nombre", Game.Jugadores[i].Name), ("rol", LoboUi.Name(Game.Jugadores[i].Rol)))
                : Game.Jugadores[i].Name);
            // Con nombres y cartas largos («María José Rodríguez · Lobo blanco») no caben dos por fila sin encoger.
            var grid = ResponsiveChipGrid.Create(parent, labels.Values.Any(l => l.Length > MaxChipLabel) ? 1 : 2);
            var chips = new Dictionary<int, NeonChip>();
            foreach (var pair in labels)
            {
                var i = pair.Key;
                var player = Game.Jugadores[i];
                var label = pair.Value;
                var chip = NeonChip.Create(grid, label, picked.Contains(i), accent);
                chip.ToggleOnClick = false;
                chip.name = player.Name;
                chip.Clicked += () =>
                {
                    if (picked.Contains(i)) picked.Remove(i);
                    else
                    {
                        if (max == 1) picked.Clear();
                        if (picked.Count >= max) return;
                        picked.Add(i);
                    }
                    foreach (var pair in chips) pair.Value.Selected = picked.Contains(pair.Key);
                    changed?.Invoke();
                };
                chips[i] = chip;
            }
            return chips;
        }

        protected static NeonPanel InfoPanel(Transform parent, string text, Color color, IconId icon)
        {
            var panel = Ui.Panel(parent, color, glow: 0.6f, padding: new RectOffset(14, 14, 12, 12));
            var row = Ui.Row(panel.Content, 10f, alignment: TextAnchor.UpperLeft);
            Ui.Icon(row, icon, 24f, color, 0.6f);
            Ui.Text(row, text, TextRole.BodyMedium, Palette.TextPrimary).Sized(width: 0f, flexWidth: 1f);
            return panel;
        }

        /// <summary>Panel de «lee en voz alta» con su primera línea; se le añaden más con <see cref="ReadLine"/>.</summary>
        protected static NeonPanel ReadAloud(Transform parent, string text)
        {
            var read = Ui.Panel(parent, Palette.NeonCyan, glow: 0.5f, padding: new RectOffset(16, 16, 14, 14), spacing: 6f);
            Ui.Text(read.Content, Loc.T("loboLeeEnVozAlta"), TextRole.LabelMedium, Palette.NeonCyan, bold: true);
            ReadLine(read, text);
            return read;
        }

        protected static void ReadLine(NeonPanel read, string text, Color? color = null) =>
            Ui.Text(read.Content, text, TextRole.BodyLarge, color ?? Palette.TextPrimary);

        /// <summary>
        /// Un muerto para anunciar: su nombre, su carta (si hoy se revela o si <paramref name="reveal"/>) y, solo para
        /// el game master, cómo murió.
        /// </summary>
        protected void DeathRow(Transform parent, int index, bool? reveal = null)
        {
            var player = Game.Jugadores[index];
            if (player.Vivo) return;
            var show = reveal ?? Game.RevelarRoles;
            var color = show ? LoboUi.Color(player.Rol) : Palette.NeonRed;
            var panel = Ui.Panel(parent, Palette.NeonRed, glow: 0.4f, padding: new RectOffset(12, 12, 10, 10));
            panel.name = Loc.T("loboHaMuerto", ("nombre", player.Name));
            var row = Ui.Row(panel.Content, 10f, alignment: TextAnchor.MiddleLeft);
            Ui.Icon(row, show ? LoboUi.Icon(player.Rol) : IconId.PersonOff, 32f, color, 0.6f);
            var texts = Ui.Column(row, 2f, alignment: TextAnchor.MiddleLeft);
            texts.Sized(width: 0f, flexWidth: 1f);
            Ui.Text(texts, player.Name, TextRole.TitleSmall, Palette.TextPrimary, bold: true, maxLines: 2);
            Ui.Text(texts, show ? Loc.T("loboSuCartaEra", ("rol", LoboUi.Name(player.Rol))) : Loc.T("loboCartaOculta"), TextRole.BodyMedium, color);
            if (player.Causa.HasValue)
            {
                Ui.Text(texts, Loc.T("loboSoloParaTi", ("texto", LoboUi.Cause(player.Causa.Value))), TextRole.BodySmall, Palette.TextMuted);
            }
        }

        /// <summary>El evento de luna de hoy, con lo que cambia.</summary>
        protected void MoonPanel(Transform parent)
        {
            if (!Game.Luna.HasValue) return;
            var luna = Game.Luna.Value;
            var title = Loc.T("loboLunaHoy", ("luna", Loc.T("loboLuna" + luna)));
            var panel = Ui.Panel(parent, Palette.NeonViolet, glow: 0.7f, padding: new RectOffset(14, 14, 12, 12), spacing: 4f);
            panel.name = title;
            var row = Ui.Row(panel.Content, 10f, alignment: TextAnchor.MiddleLeft);
            Ui.Icon(row, IconId.NightsStay, 28f, Palette.NeonVioletText, 0.8f);
            Ui.Text(row, title, TextRole.TitleSmall, Palette.NeonVioletText, bold: true, maxLines: 2).Sized(width: 0f, flexWidth: 1f);
            Ui.Text(panel.Content, Loc.T("loboLunaDesc" + luna), TextRole.BodyMedium, Palette.TextPrimary);
        }

        // ------------------------------------------------------------------ Pendientes

        private readonly List<int> _pendingPick = new List<int>();
        private NeonButton _pendingConfirm;

        /// <summary>
        /// Lo que hay que resolver antes de seguir (el disparo del cazador, el sucesor del capitán o la decisión de la
        /// sirvienta). Devuelve si hay algo pendiente.
        /// </summary>
        protected bool PendingSection(Transform parent)
        {
            _pendingPick.Clear();
            _pendingConfirm = null;
            var pending = Game.PendienteActual;
            if (pending == null) return false;
            var name = Game.Jugadores[pending.Jugador].Name;
            var alive = Game.Vivos.Select(p => p.Index).ToList();
            switch (pending.Tipo)
            {
                case LoboPendienteTipo.Cazador:
                {
                    var panel = PendingPanel(parent, LoboUi.Icon(LoboRol.Cazador), Palette.NeonRed, Loc.T("loboPendienteCazador", ("nombre", name)));
                    PlayerChips(panel.Content, alive, _pendingPick, 1, Palette.NeonRed, RefreshPending, false);
                    _pendingConfirm = Ui.Button(panel.Content, Loc.T("loboDisparar"), () => Shoot(_pendingPick.FirstOrDefault()), ButtonStyle.Filled,
                        IconId.GpsFixed, Palette.NeonRed, 0.8f);
                    Ui.Button(panel.Content, Loc.T("loboNoDispara"), SkipShot, ButtonStyle.Text, null, Palette.TextMuted);
                    break;
                }
                case LoboPendienteTipo.Sucesor:
                {
                    var panel = PendingPanel(parent, IconId.MilitaryTech, Palette.NeonYellow, Loc.T("loboPendienteSucesor", ("nombre", name)));
                    PlayerChips(panel.Content, alive, _pendingPick, 1, Palette.NeonYellow, RefreshPending, false);
                    _pendingConfirm = Ui.Button(panel.Content, Loc.T("loboNombrarSucesor"), () => Successor(_pendingPick.FirstOrDefault()),
                        ButtonStyle.Filled, IconId.MilitaryTech, Palette.NeonYellow, 0.8f);
                    break;
                }
                default:
                {
                    var servant = Game.Vivos.FirstOrDefault(p => p.Rol == LoboRol.Sirvienta);
                    var panel = PendingPanel(parent, LoboUi.Icon(LoboRol.Sirvienta), Palette.NeonCyan,
                        Loc.T("loboPendienteSirvienta", ("nombre", name), ("sirvienta", servant != null ? servant.Name : "-")));
                    Ui.Button(panel.Content, Loc.T("loboSirvientaSi"), () => Servant(true), ButtonStyle.Filled, IconId.CompareArrows, Palette.NeonCyan, 0.8f);
                    Ui.Button(panel.Content, Loc.T("loboSirvientaNo"), () => Servant(false), ButtonStyle.Outlined, null, Palette.NeonCyan);
                    break;
                }
            }
            RefreshPending();
            return true;
        }

        private static NeonPanel PendingPanel(Transform parent, IconId icon, Color color, string text)
        {
            var panel = Ui.Panel(parent, color, glow: 0.8f, padding: new RectOffset(14, 14, 14, 14), spacing: 10f);
            PulseGlow.On(panel, 0.6f, 1f);
            var row = Ui.Row(panel.Content, 10f, alignment: TextAnchor.MiddleLeft);
            Ui.Icon(row, icon, 32f, color, 0.8f);
            Ui.Text(row, Loc.T("loboAntesDeSeguir"), TextRole.TitleSmall, color, bold: true).Sized(width: 0f, flexWidth: 1f);
            Ui.Text(panel.Content, text, TextRole.BodyLarge, Palette.TextPrimary);
            return panel;
        }

        private void RefreshPending()
        {
            if (_pendingConfirm != null) _pendingConfirm.Interactable = _pendingPick.Count == 1;
        }

        /// <summary>El cazador dispara (también lo usan los tests).</summary>
        public void Shoot(int target)
        {
            if (!Game.Disparar(target)) return;
            Module.Audio?.Play(AppSound.Explosion, 0.5f);
            Refresh();
        }

        public void SkipShot()
        {
            if (Game.NoDisparar()) Refresh();
        }

        public void Successor(int index)
        {
            if (Game.NombrarSucesor(index)) Refresh();
        }

        public void Servant(bool takes)
        {
            if (Game.SirvientaDecide(takes)) Refresh();
        }
    }

    /// <summary>
    /// La noche (V079): el paso en curso con el rol que se despierta y quién lo tiene, lo que hay que leer en voz alta,
    /// a quién señala (chips con el nombre y la carta) u opciones, y «Hecho». La vidente y el zorro enseñan su resultado
    /// en un diálogo para que el game master se lo muestre. Al acabar, al amanecer.
    /// </summary>
    public sealed class LoboNightScreen : LoboMasterScreen
    {
        private readonly List<int> _picked = new List<int>();
        private Dictionary<int, NeonChip> _chips = new Dictionary<int, NeonChip>();
        private NeonButton _confirm;

        public LoboNightScreen(HombresLoboModule module) : base(module)
        {
        }

        protected override LoboFase Phase => LoboFase.Noche;

        public LoboStep Step => Game.Paso;
        public IReadOnlyList<int> Picked => _picked;

        protected override void BuildPhase(ScreenScaffold scaffold)
        {
            _picked.Clear();
            _chips = new Dictionary<int, NeonChip>();
            var step = Game.Paso;
            var content = Ui.Scroll(scaffold.Body, 12f, new RectOffset(16, 16, 12, 24));
            if (step == null) return;
            if (IsFirstStep(step)) InfoPanel(content, Loc.T("loboTodosDuermen"), Palette.NeonViolet, IconId.Bedtime);

            var color = LoboUi.Color(step.Rol);
            var head = Ui.Row(content, 12f, alignment: TextAnchor.MiddleLeft);
            Ui.Icon(head, LoboUi.Icon(step.Rol), 44f, color, 0.9f);
            var title = Ui.Column(head, 2f, alignment: TextAnchor.MiddleLeft);
            title.Sized(width: 0f, flexWidth: 1f);
            Ui.Text(title, LoboTexts.StepTitle(step.Paso), TextRole.HeadlineSmall, color, glow: 0.6f, bold: true, maxLines: 2);
            var holders = LoboTexts.Holders(Game, step.Paso);
            if (holders.Count > 0)
            {
                Ui.Text(title, Loc.T("loboLoTiene", ("nombres", LoboTexts.Names(Game, holders))), TextRole.BodySmall, Palette.TextMuted);
            }

            ReadAloud(content, LoboTexts.Narration(step.Paso));
            foreach (var line in Extras(step)) InfoPanel(content, line, Palette.NeonYellow, IconId.Lightbulb);

            if (step.Max > 0)
            {
                Ui.Text(content, CountText(step), TextRole.LabelLarge, Palette.TextMuted);
                _chips = PlayerChips(content, step.Candidatos, _picked, step.Max, color, RefreshConfirm);
            }
            if (step.Opciones.Count > 0)
            {
                foreach (var option in step.Opciones)
                {
                    var o = option;
                    Ui.Button(content, LoboTexts.Option(Game, step.Paso, o), () => Choose(o), ButtonStyle.Outlined, null, color).name =
                        LoboTexts.Option(Game, step.Paso, o);
                }
            }
            else
            {
                _confirm = Ui.Button(content, Loc.T("loboHecho"), Confirm, ButtonStyle.Filled, IconId.Check, glow: 0.8f);
                _confirm.Sized(height: 64f);
                RefreshConfirm();
            }
            Ui.Text(content, LoboTexts.Hint(step.Paso), TextRole.BodySmall, Palette.TextMuted, TextAlignmentOptions.Center);
        }

        private bool IsFirstStep(LoboStep step) => Game.PasosResueltos == 0;

        private static string CountText(LoboStep step)
        {
            if (step.Min == step.Max) return Loc.T("loboEligeN", ("n", step.Max));
            return Loc.T("loboEligeHastaN", ("n", step.Max));
        }

        /// <summary>Lo que el game master tiene que saber en el paso (la víctima para la bruja y el padre, la niña…).</summary>
        private IEnumerable<string> Extras(LoboStep step)
        {
            var victim = Game.Registro.LastOrDefault(e => e.Tipo == LoboEventoTipo.LobosEligen && e.Ronda == Game.Ronda);
            switch (step.Paso)
            {
                case LoboPaso.Lobos:
                    if (Game.Vivos.Any(p => p.Rol == LoboRol.Nina)) yield return Loc.T("loboExtraNina");
                    if (Game.Luna == LoboLuna.LunaLlena) yield return Loc.T("loboExtraLunaLlena");
                    if (step.EsAviso) yield return Loc.T("loboExtraSinVictima");
                    break;
                case LoboPaso.PadreLobos:
                case LoboPaso.BrujaSalva:
                    if (victim != null) yield return Loc.T("loboExtraVictima", ("nombre", LoboTexts.PlayerName(Game, victim.Jugador)));
                    break;
                case LoboPaso.Ladron:
                    yield return Loc.T("loboExtraLadron", ("carta1", LoboUi.Name(Game.CartasLadron[0])), ("carta2", LoboUi.Name(Game.CartasLadron[1])));
                    break;
            }
        }

        private void RefreshConfirm()
        {
            var step = Game.Paso;
            if (_confirm == null || step == null) return;
            var ok = _picked.Count >= step.Min && _picked.Count <= step.Max;
            _confirm.Interactable = ok;
            _confirm.SetLabel(Loc.T(step.Max > 0 && step.Min == 0 && _picked.Count == 0 ? "loboNadie" : "loboHecho"));
        }

        /// <summary>Marca o desmarca a un jugador del paso (también lo usan los tests).</summary>
        public void Pick(int index)
        {
            var step = Game.Paso;
            if (step == null || !step.Candidatos.Contains(index)) return;
            if (_picked.Contains(index)) _picked.Remove(index);
            else
            {
                if (step.Max == 1) _picked.Clear();
                if (_picked.Count < step.Max) _picked.Add(index);
            }
            foreach (var pair in _chips) pair.Value.Selected = _picked.Contains(pair.Key);
            RefreshConfirm();
        }

        public void Confirm()
        {
            var step = Game.Paso;
            if (step == null) return;
            var target = _picked.FirstOrDefault();
            if (!Game.ResolverPaso(_picked.ToList())) return;
            Module.Audio?.Play(AppSound.Swipe, 0.5f);
            ShowResult(step, target);
            Refresh();
        }

        public void Choose(LoboOpcion option)
        {
            if (!Game.ResolverPaso(null, option)) return;
            Module.Audio?.Play(AppSound.Swipe, 0.5f);
            Refresh();
        }

        /// <summary>Lo que ve la vidente o lo que huele el zorro, en grande para enseñárselo.</summary>
        private void ShowResult(LoboStep step, int target)
        {
            if (step.Paso == LoboPaso.Vidente && Game.VidenteVio.HasValue)
            {
                var rol = Game.VidenteVio.Value;
                Dialogs.Show(Loc.T("loboVidenteVe", ("nombre", Game.Jugadores[target].Name)), null, new[]
                {
                    new DialogAction(Loc.T("loboYaLoHaVisto"), (Action)null, ButtonStyle.Filled, Palette.NeonCyan)
                }, buildContent: c =>
                {
                    Ui.Icon(c, LoboUi.Icon(rol), 72f, LoboUi.Color(rol), 1f);
                    Ui.FittedText(c, LoboUi.Name(rol), 36f, LoboUi.Color(rol), 1f, lines: 2);
                });
            }
            if (step.Paso == LoboPaso.Zorro && Game.ZorroOlio.HasValue)
            {
                Dialogs.Show(Loc.T(Game.ZorroOlio.Value ? "loboZorroSi" : "loboZorroNo"), Loc.T(Game.ZorroOlio.Value ? "loboZorroSiAyuda" : "loboZorroNoAyuda"),
                    new[] { new DialogAction(Loc.T("aceptar"), (Action)null, ButtonStyle.Filled, Palette.NeonCyan) });
            }
        }
    }

    /// <summary>
    /// Los jugadores para el game master: su carta, si están vivos (o cómo murieron) y lo que les afecta (capitán,
    /// enamorado, infectado, encantado, sin voto…). Desde aquí se mata o revive a mano.
    /// </summary>
    public sealed class LoboPlayersScreen : AppScreen
    {
        private readonly HombresLoboModule _module;
        private readonly Action _changed;

        public LoboPlayersScreen(HombresLoboModule module, Action changed)
        {
            _module = module;
            _changed = changed;
        }

        private LoboGame Game => _module.Game;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("loboJugadoresMaster"), Loc.T("loboVivosDeN", ("n", Game.Vivos.Count()), ("total", Game.Jugadores.Count)));
            scaffold.AppBar.SetLeading(IconId.Check, Close).name = Loc.T("volver");
            var content = Ui.Scroll(scaffold.Body, 10f, new RectOffset(16, 16, 12, 24));
            foreach (var player in Game.Jugadores) Row(content, player);
            if (Game.CartasLadron.Count > 0)
            {
                Ui.Text(content, Loc.T("loboCartasSobrantes", ("cartas", string.Join(", ", Game.CartasLadron.Select(LoboUi.Name)))), TextRole.BodySmall,
                    Palette.TextMuted);
            }
            if (Game.Fase != LoboFase.Final)
            {
                Ui.Button(content, Loc.T("loboTerminarAqui"), ConfirmEnd, ButtonStyle.Text, IconId.Flag, Palette.NeonRed);
            }
        }

        private void ConfirmEnd()
        {
            Dialogs.Confirm(Loc.T("loboTerminarTitulo"), Loc.T("loboTerminarMensaje"), Loc.T("cancelar"), Loc.T("terminar"), ok =>
            {
                if (ok) End();
            }, Palette.NeonRed);
        }

        /// <summary>Da la partida por terminada y vuelve (también lo usan los tests).</summary>
        public void End()
        {
            Game.Terminar();
            Close();
        }

        private void Row(Transform parent, LoboPlayer player)
        {
            var color = player.Vivo ? LoboUi.ColorOf(Game.BandoDe(player.Index)) : Palette.TextMuted;
            var panel = Ui.Panel(parent, color, glow: player.Vivo ? 0.4f : 0f, padding: new RectOffset(12, 10, 8, 8));
            var row = Ui.Row(panel.Content, 10f, alignment: TextAnchor.MiddleLeft);
            Ui.Icon(row, player.Vivo ? LoboUi.Icon(player.Rol) : IconId.PersonOff, 28f, color, 0.5f);
            var texts = Ui.Column(row, 2f, alignment: TextAnchor.MiddleLeft);
            texts.Sized(width: 0f, flexWidth: 1f);
            Ui.Text(texts, Loc.T("loboJugadorConRol", ("nombre", player.Name), ("rol", LoboUi.Name(player.Rol))), TextRole.TitleSmall,
                player.Vivo ? Palette.TextPrimary : Palette.TextMuted, bold: true, maxLines: 2);
            var status = Status(player);
            if (status.Length > 0) Ui.Text(texts, status, TextRole.BodySmall, Palette.TextMuted, maxLines: 3);
            if (Game.Fase == LoboFase.Final) return;
            var action = player.Vivo
                ? NeonButton.CreateIcon(row, IconId.PersonOff, () => ConfirmKill(player), Palette.NeonRed)
                : NeonButton.CreateIcon(row, IconId.Refresh, () => Revive(player.Index), Palette.NeonGreen);
            action.name = Loc.T(player.Vivo ? "loboMatarA" : "loboRevivirA", ("nombre", player.Name));
        }

        private string Status(LoboPlayer p)
        {
            var parts = new List<string>();
            if (!p.Vivo) parts.Add(Loc.T("loboMuertoPor", ("causa", p.Causa.HasValue ? LoboUi.Cause(p.Causa.Value) : string.Empty), ("n", p.RondaMuerte ?? 0)));
            if (p.Capitan) parts.Add(Loc.T("loboCapitan"));
            if (p.Pareja.HasValue) parts.Add(Loc.T("loboEnamoradoDe", ("nombre", Game.Jugadores[p.Pareja.Value].Name)));
            if (p.Infectado) parts.Add(Loc.T("loboInfectado"));
            if (p.Encantado) parts.Add(Loc.T("loboEncantado"));
            if (p.TontoRevelado) parts.Add(Loc.T("loboSinVoto"));
            if (p.VidasExtra > 0) parts.Add(Loc.T("loboVidaExtra"));
            if (p.Rol != p.RolInicial) parts.Add(Loc.T("loboEraAntes", ("rol", LoboUi.Name(p.RolInicial))));
            if (Game.CuervoMarcado == p.Index) parts.Add(Loc.T("loboMarcadoCuervo"));
            if (Game.Jugadores.Any(x => x.Rol == LoboRol.Sectario)) parts.Add(Loc.T("loboGrupoN", ("n", p.Grupo + 1)));
            return string.Join(" · ", parts);
        }

        private void ConfirmKill(LoboPlayer player)
        {
            Dialogs.Confirm(Loc.T("loboMatarTitulo", ("nombre", player.Name)), Loc.T("loboMatarMensaje"), Loc.T("cancelar"), Loc.T("loboMatar"), ok =>
            {
                if (!ok) return;
                Game.MatarManual(player.Index);
                Rebuild();
            }, Palette.NeonRed);
        }

        /// <summary>Revive a mano (también lo usan los tests).</summary>
        public void Revive(int index)
        {
            Game.RevivirManual(index);
            Rebuild();
        }

        private void Rebuild()
        {
            Root.ClearChildren();
            Build(Root);
        }

        private void Close()
        {
            Router.Pop();
            _changed?.Invoke();
        }
    }

    /// <summary>El registro de toda la partida, de lo más nuevo a lo más viejo.</summary>
    public sealed class LoboLogScreen : AppScreen
    {
        private readonly HombresLoboModule _module;

        public LoboLogScreen(HombresLoboModule module) => _module = module;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("loboRegistro"));
            scaffold.AppBar.SetLeading(IconId.Check, () => Router.Pop()).name = Loc.T("volver");
            var content = Ui.Scroll(scaffold.Body, 8f, new RectOffset(16, 16, 12, 24));
            var events = _module.Game.Registro.Where(e => e.Tipo != LoboEventoTipo.Reparto).Reverse().ToList();
            if (events.Count == 0) Ui.Text(content, Loc.T("loboRegistroVacio"), TextRole.BodyLarge, Palette.TextMuted, TextAlignmentOptions.Center);
            foreach (var e in events)
            {
                var color = e.Tipo == LoboEventoTipo.Muere ? Palette.NeonRed : e.Noche ? Palette.NeonVioletText : Palette.NeonYellow;
                Ui.Text(content, LoboTexts.Event(_module.Game, e), TextRole.BodyMedium, color);
            }
        }
    }
}
