using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.HombresLobo.Domain
{
    /// <summary>Reglas de la partida que se eligen en el setup (V077), con sus valores por defecto.</summary>
    public sealed class LoboRules
    {
        /// <summary>Al morir, se dice el rol del muerto.</summary>
        public bool RevelarRolAlMorir { get; set; } = true;

        /// <summary>El primer día se elige capitán: su voto vale doble y al morir nombra sucesor.</summary>
        public bool Capitan { get; set; } = true;

        /// <summary>Segundos del debate del día (0 = sin tiempo).</summary>
        public int DebateSegundos { get; set; } = 180;

        /// <summary>Los lobos ganan en cuanto son tantos como los demás (si no, al quedarse solos).</summary>
        public bool LobosGananAlIgualar { get; set; }

        /// <summary>Al ver su carta, cada lobo ve quiénes son los demás lobos.</summary>
        public bool LobosSeConocenAlRepartir { get; set; } = true;

        public bool ProtectorPuedeProtegerse { get; set; } = true;

        /// <summary>El protector puede proteger a la misma persona dos noches seguidas.</summary>
        public bool ProtectorRepetir { get; set; }

        public bool BrujaPuedeSalvarse { get; set; } = true;

        /// <summary>Si el pueblo mata al anciano, los del pueblo pierden sus poderes.</summary>
        public bool AncianoQuitaPoderes { get; set; } = true;

        /// <summary>La primera noche los lobos no matan.</summary>
        public bool PrimeraNocheSinMuertes { get; set; }

        /// <summary>Con empate en la votación (y sin chivo expiatorio) se vota otra vez entre los empatados; si no, no muere nadie.</summary>
        public bool DesempateSegundaVotacion { get; set; } = true;

        /// <summary>Eventos de luna: cada mañana, a veces, pasa algo que cambia las reglas de ese día o de esa noche (V081).</summary>
        public bool EventosLuna { get; set; }

        public LoboRules Clone() => (LoboRules)MemberwiseClone();
    }

    public enum LoboConfigError
    {
        PocosJugadores,
        DemasiadosJugadores,
        NombreVacio,
        NombresRepetidos,
        SinLobos,
        CartasNoCuadran,
        GrupoIncompleto,
        DemasiadasCopias
    }

    /// <summary>
    /// Partida de Hombres Lobo (V076): 5–30 jugadores (en el orden en que están sentados, que cuenta para el zorro, el
    /// oso y el caballero), las cartas de cada rol y las reglas. Hace falta una carta por jugador y, con ladrón, dos más
    /// (las que se le ofrecen la primera noche).
    /// </summary>
    public sealed class LoboConfig
    {
        public const int MinJugadores = 5;
        public const int MaxJugadores = 30;
        public const int CartasExtraLadron = 2;

        private LoboConfig(IReadOnlyList<string> jugadores, IReadOnlyDictionary<LoboRol, int> roles, LoboRules rules)
        {
            Jugadores = jugadores;
            Roles = roles;
            Rules = rules;
        }

        public IReadOnlyList<string> Jugadores { get; }
        public IReadOnlyDictionary<LoboRol, int> Roles { get; }
        public LoboRules Rules { get; }

        public int Count(LoboRol rol) => Roles.TryGetValue(rol, out var n) ? n : 0;

        public static int CartasNecesarias(int jugadores, IReadOnlyDictionary<LoboRol, int> roles) =>
            jugadores + (roles != null && roles.TryGetValue(LoboRol.Ladron, out var n) && n > 0 ? CartasExtraLadron : 0);

        public static int TotalCartas(IReadOnlyDictionary<LoboRol, int> roles) => roles?.Values.Where(v => v > 0).Sum() ?? 0;

        public static (LoboConfig config, LoboConfigError? error) Create(IEnumerable<string> jugadores, IReadOnlyDictionary<LoboRol, int> roles,
            LoboRules rules = null)
        {
            var names = (jugadores ?? Array.Empty<string>()).Select(NameNormalizer.Normalize).ToList();
            if (names.Count < MinJugadores) return (null, LoboConfigError.PocosJugadores);
            if (names.Count > MaxJugadores) return (null, LoboConfigError.DemasiadosJugadores);
            if (names.Any(n => n.Length == 0)) return (null, LoboConfigError.NombreVacio);
            if (names.Select(NameNormalizer.DuplicateKey).Distinct().Count() != names.Count) return (null, LoboConfigError.NombresRepetidos);
            var counts = (roles ?? new Dictionary<LoboRol, int>()).Where(p => p.Value > 0).ToDictionary(p => p.Key, p => p.Value);
            foreach (var pair in counts)
            {
                var def = LoboCatalog.Get(pair.Key);
                if (pair.Value > def.Max) return (null, LoboConfigError.DemasiadasCopias);
                if (def.Grupo > 0 && pair.Value != def.Grupo) return (null, LoboConfigError.GrupoIncompleto);
            }
            if (!counts.Keys.Any(LoboCatalog.EsLoboDeCarta)) return (null, LoboConfigError.SinLobos);
            if (TotalCartas(counts) != CartasNecesarias(names.Count, counts)) return (null, LoboConfigError.CartasNoCuadran);
            var copy = (rules ?? new LoboRules()).Clone();
            copy.DebateSegundos = Math.Max(0, copy.DebateSegundos);
            return (new LoboConfig(names.AsReadOnly(), counts, copy), null);
        }
    }

    /// <summary>Repartos ya hechos para empezar rápido (V077). Siempre cuadran con el número de jugadores.</summary>
    public enum LoboPreset
    {
        /// <summary>Lobos, vidente, bruja, cazador y aldeanos: para aprender.</summary>
        Iniciacion,

        /// <summary>El juego básico completo.</summary>
        Clasica,

        /// <summary>Básico más el pueblo con poderes (protector, anciano, zorro, cuervo…).</summary>
        PuebloConPoderes,

        /// <summary>De todo: lobos especiales y solitarios según los jugadores que haya.</summary>
        Caos
    }

    public static class LoboPresets
    {
        public static readonly IReadOnlyList<LoboPreset> All = (LoboPreset[])Enum.GetValues(typeof(LoboPreset));

        /// <summary>Lobos de carta recomendados: uno por cada cuatro jugadores, al menos uno.</summary>
        public static int LobosPara(int jugadores) => Math.Max(1, jugadores / 4);

        /// <summary>Las cartas del preset para <paramref name="jugadores"/> (con ladrón, dos aldeanos más).</summary>
        public static Dictionary<LoboRol, int> Roles(LoboPreset preset, int jugadores)
        {
            var n = Math.Max(LoboConfig.MinJugadores, jugadores);
            var roles = new Dictionary<LoboRol, int>();
            var wolves = LobosPara(n);
            var specials = new List<LoboRol>();
            switch (preset)
            {
                case LoboPreset.Iniciacion:
                    specials.Add(LoboRol.Vidente);
                    if (n >= 6) specials.Add(LoboRol.Bruja);
                    if (n >= 7) specials.Add(LoboRol.Cazador);
                    break;
                case LoboPreset.Clasica:
                    specials.AddRange(new[] { LoboRol.Vidente, LoboRol.Bruja, LoboRol.Cazador, LoboRol.Cupido });
                    if (n >= 8) specials.Add(LoboRol.Nina);
                    if (n >= 9) specials.Add(LoboRol.Ladron);
                    break;
                case LoboPreset.PuebloConPoderes:
                    specials.AddRange(new[] { LoboRol.Vidente, LoboRol.Bruja, LoboRol.Cazador, LoboRol.Protector });
                    foreach (var (minimum, rol) in new[]
                             {
                                 (7, LoboRol.Anciano), (8, LoboRol.Cupido), (9, LoboRol.Zorro), (10, LoboRol.Cuervo), (11, LoboRol.OsoDomador),
                                 (12, LoboRol.TontoDelPueblo), (13, LoboRol.Caballero), (14, LoboRol.ChivoExpiatorio), (15, LoboRol.JuezTartamudo)
                             })
                    {
                        if (n >= minimum) specials.Add(rol);
                    }
                    break;
                case LoboPreset.Caos:
                    specials.AddRange(new[] { LoboRol.Vidente, LoboRol.Bruja, LoboRol.Cazador });
                    foreach (var (minimum, rol) in new[]
                             {
                                 (6, LoboRol.Protector), (7, LoboRol.Cupido), (8, LoboRol.Angel), (9, LoboRol.PerroLobo), (10, LoboRol.Flautista),
                                 (11, LoboRol.NinoSalvaje), (12, LoboRol.Zorro), (13, LoboRol.Anciano), (14, LoboRol.Cuervo)
                             })
                    {
                        if (n >= minimum) specials.Add(rol);
                    }
                    if (n >= 10)
                    {
                        wolves--;
                        roles[LoboRol.LoboBlanco] = 1;
                    }
                    if (n >= 9)
                    {
                        wolves--;
                        roles[LoboRol.GranLoboMalo] = 1;
                    }
                    if (n >= 13)
                    {
                        wolves--;
                        roles[LoboRol.PadreLobos] = 1;
                    }
                    break;
            }
            if (wolves > 0) roles[LoboRol.Lobo] = wolves;
            // Los especiales entran por orden de prioridad mientras quepan (con muchos lobos especiales, no caben todos).
            foreach (var rol in specials)
            {
                var with = new Dictionary<LoboRol, int>(roles) { [rol] = 1 };
                if (LoboConfig.TotalCartas(with) > LoboConfig.CartasNecesarias(n, with)) break;
                roles[rol] = 1;
            }
            var needed = LoboConfig.CartasNecesarias(n, roles);
            var villagers = needed - LoboConfig.TotalCartas(roles);
            if (villagers > 0) roles[LoboRol.Aldeano] = villagers;
            return roles;
        }
    }
}
