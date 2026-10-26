using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.HombresLobo.Domain;
using Gamespot.Games.Shared;
using UnityEngine;

namespace Gamespot.Games.HombresLobo
{
    /// <summary>Estado compartido de las pantallas de Hombres Lobo: la partida y el borrador del setup.</summary>
    public sealed class HombresLoboModule
    {
        public const string SetupRoute = "/hombres-lobo";
        public const string RevealRoute = "/hombres-lobo/reparto";
        public const string NightRoute = "/hombres-lobo/noche";
        public const string DawnRoute = "/hombres-lobo/amanecer";
        public const string DayRoute = "/hombres-lobo/dia";
        public const string EndRoute = "/hombres-lobo/final";

        public HombresLoboModule(LoboGame game, Router router, AudioService audio)
        {
            Game = game;
            Router = router;
            Audio = audio;
            ApplyPreset(LoboPreset.Clasica);
        }

        public LoboGame Game { get; }
        public Router Router { get; }
        public AudioService Audio { get; }

        public List<string> DraftPlayers { get; } = new List<string>();

        /// <summary>Las cartas elegidas (por preset o a mano).</summary>
        public Dictionary<LoboRol, int> DraftRoles { get; } = new Dictionary<LoboRol, int>();

        /// <summary>El preset elegido (nulo = a medida): si cambia el número de jugadores, se recalcula.</summary>
        public LoboPreset? DraftPreset { get; private set; }

        public LoboRules DraftRules { get; } = new LoboRules();

        /// <summary>Jugadores para los que está hecho el reparto del preset.</summary>
        public int PresetPlayers { get; private set; } = 8;

        public void ApplyPreset(LoboPreset preset, int? players = null)
        {
            DraftPreset = preset;
            PresetPlayers = Mathf.Max(LoboConfig.MinJugadores, players ?? PresetPlayers);
            DraftRoles.Clear();
            foreach (var pair in LoboPresets.Roles(preset, PresetPlayers)) DraftRoles[pair.Key] = pair.Value;
        }

        /// <summary>Cambia una carta a mano: el reparto pasa a ser a medida.</summary>
        public void SetRole(LoboRol rol, int count)
        {
            DraftPreset = null;
            if (count <= 0) DraftRoles.Remove(rol);
            else DraftRoles[rol] = count;
        }

        /// <summary>Completa o recorta con aldeanos hasta que las cartas cuadren con <paramref name="players"/>.</summary>
        public void FillWithVillagers(int players)
        {
            var needed = LoboConfig.CartasNecesarias(players, DraftRoles);
            var others = LoboConfig.TotalCartas(DraftRoles) - (DraftRoles.TryGetValue(LoboRol.Aldeano, out var v) ? v : 0);
            SetRole(LoboRol.Aldeano, Mathf.Max(0, needed - others));
        }

        public void PlayAgain()
        {
            var config = Game.Config;
            if (config == null)
            {
                ResetToSetup();
                return;
            }
            Game.Iniciar(config);
            GoToPhase();
        }

        public string RouteForPhase()
        {
            switch (Game.Fase)
            {
                case LoboFase.Reparto: return RevealRoute;
                case LoboFase.Noche: return NightRoute;
                case LoboFase.Amanecer: return DawnRoute;
                case LoboFase.Dia: return DayRoute;
                case LoboFase.Final: return EndRoute;
                default: return SetupRoute;
            }
        }

        public void GoToPhase() => Router.Go(RouteForPhase());

        public void ResetToMenu()
        {
            Game.Reiniciar();
            Router.Go(Router.Home);
        }

        public void ResetToSetup()
        {
            Game.Reiniciar();
            Router.Go(SetupRoute);
        }
    }

    /// <summary>Iconos, colores y textos de los roles (la parte de pantalla del catálogo).</summary>
    public static class LoboUi
    {
        private static readonly Dictionary<LoboRol, IconId> RoleIcons = new Dictionary<LoboRol, IconId>
        {
            [LoboRol.Aldeano] = IconId.Face,
            [LoboRol.Lobo] = IconId.Pets,
            [LoboRol.Vidente] = IconId.Visibility,
            [LoboRol.Bruja] = IconId.Science,
            [LoboRol.Cazador] = IconId.GpsFixed,
            [LoboRol.Cupido] = IconId.Favorite,
            [LoboRol.Nina] = IconId.RemoveRedEye,
            [LoboRol.Ladron] = IconId.SwapHoriz,
            [LoboRol.Protector] = IconId.Shield,
            [LoboRol.Anciano] = IconId.Elderly,
            [LoboRol.ChivoExpiatorio] = IconId.Dangerous,
            [LoboRol.TontoDelPueblo] = IconId.SentimentDissatisfied,
            [LoboRol.Hermana] = IconId.Group,
            [LoboRol.Hermano] = IconId.Groups,
            [LoboRol.Zorro] = IconId.Forest,
            [LoboRol.Cuervo] = IconId.Campaign,
            [LoboRol.OsoDomador] = IconId.RecordVoiceOver,
            [LoboRol.Caballero] = IconId.MilitaryTech,
            [LoboRol.JuezTartamudo] = IconId.Gavel,
            [LoboRol.Sirvienta] = IconId.CompareArrows,
            [LoboRol.PerroLobo] = IconId.Bolt,
            [LoboRol.NinoSalvaje] = IconId.ChildCare,
            [LoboRol.LoboBlanco] = IconId.Pets,
            [LoboRol.GranLoboMalo] = IconId.Pets,
            [LoboRol.PadreLobos] = IconId.AutoFixHigh,
            [LoboRol.Angel] = IconId.Star,
            [LoboRol.Flautista] = IconId.MusicNote,
            [LoboRol.Sectario] = IconId.Masks
        };

        public static IconId Icon(LoboRol rol) => RoleIcons[rol];

        public static string Name(LoboRol rol) => Loc.T(LoboCatalog.Get(rol).NameKey);
        public static string Description(LoboRol rol) => Loc.T(LoboCatalog.Get(rol).DescriptionKey);

        /// <summary>Color del bando con el que empieza el rol (el lobo blanco, en violeta; los solitarios, en amarillo).</summary>
        public static Color Color(LoboRol rol)
        {
            if (rol == LoboRol.LoboBlanco) return Palette.NeonVioletText;
            switch (LoboCatalog.Get(rol).Bando)
            {
                case LoboBando.Lobos: return Palette.NeonRed;
                case LoboBando.Solitario: return Palette.NeonYellow;
                default: return Palette.NeonCyan;
            }
        }

        public static Color ColorOf(LoboBando bando) =>
            bando == LoboBando.Lobos ? Palette.NeonRed : bando == LoboBando.Solitario ? Palette.NeonYellow : Palette.NeonCyan;

        public static string PackName(LoboPaquete paquete) => Loc.T("loboPaquete" + paquete);

        /// <summary>«2 lobos · vidente · bruja · 4 aldeanos».</summary>
        public static string RolesSummary(IReadOnlyDictionary<LoboRol, int> roles) =>
            string.Join(" · ", LoboCatalog.All.Where(d => roles.TryGetValue(d.Rol, out var n) && n > 0)
                .Select(d => roles[d.Rol] > 1 ? Loc.T("loboRolesVarios", ("n", roles[d.Rol]), ("rol", Name(d.Rol))) : Name(d.Rol)));

        public static string Cause(LoboMuerte cause) => Loc.T("loboMuerte" + cause);
    }

    /// <summary>
    /// Descriptor de Hombres Lobo (V076–V081): roles ocultos repartidos pasando el móvil y después un game master que no
    /// juega lleva la partida con el móvil, paso a paso. Todos los roles de las expansiones y muchas reglas a elegir.
    /// </summary>
    public sealed class HombresLoboGame : IGameDescriptor
    {
        public string Id => "hombres_lobo";
        public string TitleKey => "loboMenuTitulo";
        public string DescriptionKey => "loboMenuDescripcion";
        public IconId Icon => IconId.NightsStay;
        public string EntryRoute => HombresLoboModule.SetupRoute;

        public HombresLoboModule Module { get; private set; }

        public void Register(GameContext context)
        {
            var module = new HombresLoboModule(new LoboGame(context.Random), context.Router, context.Audio);
            Module = module;
            context.Router.Register(HombresLoboModule.SetupRoute, () => new LoboSetupScreen(module));
            context.Router.Register(HombresLoboModule.RevealRoute, () => new LoboRevealScreen(module));
            context.Router.Register(HombresLoboModule.NightRoute, () => new LoboNightScreen(module));
        }

        // Hombres Lobo no guarda nada en la base de datos.
        public void OnCreate(IDb db)
        {
        }

        public void OnUpgrade(IDb db, int oldVersion, int newVersion)
        {
        }

        public void SeedIfEmpty(IDb db, ISeedSource seeds)
        {
        }
    }
}
