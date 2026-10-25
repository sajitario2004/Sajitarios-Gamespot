using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Setup de Hombres Lobo (V077): jugadores (5–30, en el orden en que se sientan), el reparto (un preset o a medida,
    /// con la pantalla de roles por paquetes) y todas las reglas. «Empezar» reparte las cartas.
    /// </summary>
    public sealed class LoboSetupScreen : AppScreen
    {
        private readonly HombresLoboModule _module;
        private TMP_Text _summary;
        private TMP_Text _cards;
        private ChoiceChips<LoboPreset> _presets;

        public LoboSetupScreen(HombresLoboModule module) => _module = module;

        public PlayerListEditor Players { get; private set; }

        /// <summary>Si las cartas cuadran con los jugadores.</summary>
        public bool CardsOk => LoboConfig.TotalCartas(_module.DraftRoles) == LoboConfig.CartasNecesarias(Players.Count, _module.DraftRoles);

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("loboTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "loboTitulo", GameUi.RuleKeys("reglasLobo", 8));
            scaffold.AppBar.AddAction(IconId.MenuBook, OpenRoles).name = Loc.T("loboElegirRoles");
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 8, 32));
            var rules = _module.DraftRules;

            var players = SetupSection.Create(content, Loc.T("jugadores"), Loc.T("loboJugadoresAyuda"), Palette.NeonCyan);
            Players = PlayerListEditor.Create(players.Content, LoboConfig.MinJugadores, LoboConfig.MaxJugadores, Palette.NeonCyan, _module.DraftPlayers);
            Players.NamesChanged += SaveNames;
            Players.CountChanged += () =>
            {
                SaveNames();
                if (_module.DraftPreset.HasValue) _module.ApplyPreset(_module.DraftPreset.Value, Players.Count);
                RefreshRoles();
            };
            if (_module.DraftPreset.HasValue && _module.PresetPlayers != Players.Count) _module.ApplyPreset(_module.DraftPreset.Value, Players.Count);

            var deal = SetupSection.Create(content, Loc.T("loboReparto"), Loc.T("loboRepartoAyuda"), Palette.NeonRed);
            _presets = ChoiceChips<LoboPreset>.Create(deal.Content, LoboPresets.All.Select(p => (p, Loc.T("loboPreset" + p))).ToList(),
                _module.DraftPreset ?? (LoboPreset)(-1), Palette.NeonRed, SelectPreset, 2);
            _summary = Ui.Text(deal.Content, string.Empty, TextRole.BodyMedium, Palette.TextPrimary);
            _cards = Ui.Text(deal.Content, string.Empty, TextRole.LabelLarge, Palette.NeonGreen, bold: true);
            var buttons = Ui.Row(deal.Content, 8f, alignment: TextAnchor.MiddleCenter);
            Ui.Button(buttons, Loc.T("loboElegirRoles"), OpenRoles, ButtonStyle.Outlined, IconId.Tune, Palette.NeonRed).Sized(flexWidth: 1f);
            Ui.Button(buttons, Loc.T("loboAjustarAldeanos"), Fill, ButtonStyle.Text, IconId.Face, Palette.NeonCyan).Sized(flexWidth: 1f);

            var day = SetupSection.Create(content, Loc.T("loboReglasDia"), null, Palette.NeonYellow);
            Switch(day.Content, "loboReglaRevelar", rules.RevelarRolAlMorir, v => rules.RevelarRolAlMorir = v);
            Switch(day.Content, "loboReglaCapitan", rules.Capitan, v => rules.Capitan = v);
            Switch(day.Content, "loboReglaDesempate", rules.DesempateSegundaVotacion, v => rules.DesempateSegundaVotacion = v);
            Ui.Text(day.Content, Loc.T("loboReglaDebate"), TextRole.LabelLarge, Palette.TextMuted);
            ChoiceChips<int>.Create(day.Content, new[] { 0, 60, 120, 180, 300 }.Select(s => (s, s == 0 ? Loc.T("sinTiempo") : Loc.T("minutosCorto", ("n", s / 60))))
                .ToList(), rules.DebateSegundos, Palette.NeonYellow, v => rules.DebateSegundos = v);

            var night = SetupSection.Create(content, Loc.T("loboReglasNoche"), null, Palette.NeonViolet);
            Switch(night.Content, "loboReglaLobosSeConocen", rules.LobosSeConocenAlRepartir, v => rules.LobosSeConocenAlRepartir = v);
            Switch(night.Content, "loboReglaPrimeraNoche", rules.PrimeraNocheSinMuertes, v => rules.PrimeraNocheSinMuertes = v);
            Switch(night.Content, "loboReglaProtegerse", rules.ProtectorPuedeProtegerse, v => rules.ProtectorPuedeProtegerse = v);
            Switch(night.Content, "loboReglaProtectorRepite", rules.ProtectorRepetir, v => rules.ProtectorRepetir = v);
            Switch(night.Content, "loboReglaBrujaSeSalva", rules.BrujaPuedeSalvarse, v => rules.BrujaPuedeSalvarse = v);

            var win = SetupSection.Create(content, Loc.T("loboReglasPartida"), null, Palette.NeonMagenta);
            Switch(win.Content, "loboReglaIgualar", rules.LobosGananAlIgualar, v => rules.LobosGananAlIgualar = v);
            Switch(win.Content, "loboReglaAnciano", rules.AncianoQuitaPoderes, v => rules.AncianoQuitaPoderes = v);
            Switch(win.Content, "loboReglaLuna", rules.EventosLuna, v => rules.EventosLuna = v);

            Ui.Text(content, Loc.T("loboMasterAyuda"), TextRole.BodySmall, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Button(content, Loc.T("loboRepartir"), Start, ButtonStyle.Filled, IconId.Masks, Palette.NeonRed, 0.8f);
            RefreshRoles();
        }

        private static void Switch(Transform parent, string key, bool value, Action<bool> changed)
        {
            var sw = NeonSwitch.Create(parent, Loc.T(key), Loc.T(key + "Ayuda"), value);
            sw.Changed += changed;
        }

        private void SaveNames()
        {
            _module.DraftPlayers.Clear();
            _module.DraftPlayers.AddRange(Players.RawNames);
        }

        public void SelectPreset(LoboPreset preset)
        {
            _module.ApplyPreset(preset, Players.Count);
            RefreshRoles();
        }

        public void Fill()
        {
            _module.FillWithVillagers(Players.Count);
            RefreshRoles();
        }

        private void OpenRoles() => Router.Push(new LoboRolesScreen(_module, Players.Count, RefreshRoles));

        /// <summary>Vuelve a pintar el resumen del reparto (al cambiar de preset, de jugadores o volver de los roles).</summary>
        public void RefreshRoles()
        {
            if (_summary == null) return;
            var roles = _module.DraftRoles;
            _summary.text = roles.Count == 0 ? Loc.T("loboSinRoles") : LoboUi.RolesSummary(roles);
            var total = LoboConfig.TotalCartas(roles);
            var needed = LoboConfig.CartasNecesarias(Players.Count, roles);
            _cards.text = Loc.T(roles.ContainsKey(LoboRol.Ladron) ? "loboCartasConLadron" : "loboCartas", ("n", total), ("total", needed));
            _cards.color = total == needed ? Palette.NeonGreen : Palette.NeonError;
            if (_module.DraftPreset.HasValue) _presets.Show(_module.DraftPreset.Value);
            else _presets.ShowNone();
        }

        public void Start()
        {
            SaveNames();
            var (config, error) = LoboConfig.Create(Players.NormalizedNames, _module.DraftRoles, _module.DraftRules);
            if (error.HasValue)
            {
                Snackbar.Show(ErrorText(error.Value, Players.Count, _module.DraftRoles), Palette.NeonError);
                return;
            }
            Players.Remember();
            _module.Game.Iniciar(config);
            _module.GoToPhase();
        }

        public static string ErrorText(LoboConfigError error, int players, IReadOnlyDictionary<LoboRol, int> roles)
        {
            switch (error)
            {
                case LoboConfigError.PocosJugadores: return Loc.T("errorMinJugadores", ("n", LoboConfig.MinJugadores));
                case LoboConfigError.DemasiadosJugadores: return Loc.T("errorMaxJugadores", ("n", LoboConfig.MaxJugadores));
                case LoboConfigError.NombreVacio: return Loc.T("errorNombreVacio");
                case LoboConfigError.NombresRepetidos: return Loc.T("errorNombresDuplicados");
                case LoboConfigError.SinLobos: return Loc.T("loboErrorSinLobos");
                case LoboConfigError.GrupoIncompleto: return Loc.T("loboErrorGrupo");
                case LoboConfigError.DemasiadasCopias: return Loc.T("loboErrorCopias");
                default:
                    return Loc.T("loboErrorCartas", ("n", LoboConfig.TotalCartas(roles)), ("total", LoboConfig.CartasNecesarias(players, roles)));
            }
        }
    }

    /// <summary>
    /// Elegir las cartas (V077): cada paquete con sus roles, lo que hace cada uno y cuántas cartas hay; arriba, si
    /// cuadran con los jugadores. Las hermanas van de dos en dos y los hermanos de tres en tres.
    /// </summary>
    public sealed class LoboRolesScreen : AppScreen
    {
        private readonly HombresLoboModule _module;
        private readonly int _players;
        private readonly Action _changed;
        private readonly Dictionary<LoboRol, NeonStepper> _steppers = new Dictionary<LoboRol, NeonStepper>();
        private TMP_Text _cards;

        public LoboRolesScreen(HombresLoboModule module, int players, Action changed)
        {
            _module = module;
            _players = players;
            _changed = changed;
        }

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("loboElegirRoles"));
            scaffold.AppBar.SetLeading(IconId.Check, Close).name = Loc.T("listo");
            var content = Ui.Scroll(scaffold.Body, 14f, new RectOffset(16, 16, 8, 32));
            _cards = Ui.Text(content, string.Empty, TextRole.TitleMedium, Palette.NeonGreen, TextAlignmentOptions.Center, 0.6f, bold: true);
            _steppers.Clear();
            foreach (var pack in (LoboPaquete[])Enum.GetValues(typeof(LoboPaquete)))
            {
                var section = SetupSection.Create(content, LoboUi.PackName(pack), Loc.T("loboPaqueteAyuda" + pack),
                    pack == LoboPaquete.Lobos ? Palette.NeonRed : pack == LoboPaquete.Solitarios ? Palette.NeonYellow : Palette.NeonCyan);
                foreach (var def in LoboCatalog.Of(pack)) RoleRow(section.Content, def);
            }
            var bottom = Ui.Row(content, 8f, alignment: TextAnchor.MiddleCenter);
            Ui.Button(bottom, Loc.T("loboAjustarAldeanos"), Fill, ButtonStyle.Outlined, IconId.Face, Palette.NeonCyan).Sized(flexWidth: 1f);
            Ui.Button(bottom, Loc.T("listo"), Close, ButtonStyle.Filled, IconId.Check, glow: 0.8f).Sized(flexWidth: 1f);
            Refresh();
        }

        private void RoleRow(Transform parent, LoboRoleDef def)
        {
            // Arriba, icono, nombre y contador (de ancho fijo); debajo, lo que hace, a todo lo ancho.
            var block = Ui.Column(parent, 2f, name: def.Rol.ToString());
            var row = Ui.Row(block, 10f, alignment: TextAnchor.MiddleLeft);
            var color = LoboUi.Color(def.Rol);
            Ui.Icon(row, LoboUi.Icon(def.Rol), 28f, color, 0.6f);
            var name = Ui.Text(row, LoboUi.Name(def.Rol), TextRole.TitleSmall, color, bold: true, maxLines: 2);
            name.enableAutoSizing = true;
            name.fontSizeMax = name.fontSize;
            name.fontSizeMin = 11f;
            name.Sized(width: 0f, flexWidth: 1f);
            var current = _module.DraftRoles.TryGetValue(def.Rol, out var n) ? n : 0;
            var stepper = NeonStepper.Create(row, current, 0, def.Max, v => v.ToString(), color);
            stepper.Sized(width: 156f, flexWidth: 0f);
            Ui.Text(block, LoboUi.Description(def.Rol), TextRole.BodySmall, Palette.TextMuted);
            stepper.SetButtonNames(Loc.T("loboQuitarCarta", ("rol", LoboUi.Name(def.Rol))), Loc.T("loboAnadirCarta", ("rol", LoboUi.Name(def.Rol))));
            stepper.Changed += v => SetCount(def, v);
            _steppers[def.Rol] = stepper;
        }

        /// <summary>Cambia las cartas de un rol (los grupos saltan de 0 a su número y al revés).</summary>
        public void SetCount(LoboRoleDef def, int value)
        {
            var count = value;
            if (def.Grupo > 0)
            {
                var before = _module.DraftRoles.TryGetValue(def.Rol, out var b) ? b : 0;
                count = value > before ? def.Grupo : value < before ? 0 : before;
                if (_steppers.TryGetValue(def.Rol, out var stepper)) stepper.Set(count, false);
            }
            _module.SetRole(def.Rol, count);
            Refresh();
        }

        public void Fill()
        {
            _module.FillWithVillagers(_players);
            if (_steppers.TryGetValue(LoboRol.Aldeano, out var villagers))
            {
                villagers.Set(_module.DraftRoles.TryGetValue(LoboRol.Aldeano, out var n) ? n : 0, false);
            }
            Refresh();
        }

        private void Refresh()
        {
            var total = LoboConfig.TotalCartas(_module.DraftRoles);
            var needed = LoboConfig.CartasNecesarias(_players, _module.DraftRoles);
            _cards.text = Loc.T(_module.DraftRoles.ContainsKey(LoboRol.Ladron) ? "loboCartasConLadron" : "loboCartas", ("n", total), ("total", needed));
            _cards.color = total == needed ? Palette.NeonGreen : Palette.NeonError;
            _changed?.Invoke();
        }

        private void Close() => Router.Pop();
    }
}
