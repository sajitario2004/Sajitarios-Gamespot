using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;

namespace Gamespot.Games.HombresLobo.Domain
{
    /// <summary>
    /// Motor de una partida de Hombres Lobo llevada por un game master que no juega (V076). Reparte las cartas, va
    /// diciendo qué rol se despierta en cada paso de la noche (solo los que están vivos y tienen poder), resuelve las
    /// muertes al amanecer (protector, pociones, infección, anciano, enamorados, cazador, caballero, niño salvaje…), lleva
    /// el día (capitán, votación, chivo, tonto, sirvienta, juez) y comprueba quién gana. Todo se puede deshacer.
    /// Orden de las llamadas al random: al empezar, Fisher-Yates del mazo (las cartas en el orden del catálogo) y, con
    /// sectario, Fisher-Yates de los demás jugadores para los grupos; con eventos de luna, cada mañana
    /// <c>NextDouble()</c> y, si sale evento, <c>NextInt</c> para elegirlo.
    /// </summary>
    public sealed class LoboGame
    {
        private const int MaxDeshacer = 80;

        private static readonly LoboPaso[] Orden = (LoboPaso[])Enum.GetValues(typeof(LoboPaso));

        private readonly IRandomProvider _random;
        private readonly List<LoboState> _undo = new List<LoboState>();
        private LoboState _s = new LoboState();

        public LoboGame(IRandomProvider random) => _random = random ?? throw new ArgumentNullException(nameof(random));

        public event Action Changed;

        public LoboConfig Config { get; private set; }
        public LoboRules Rules => Config?.Rules;
        public LoboFase Fase => _s.Fase;

        /// <summary>Ronda en curso: la noche n y el día n.</summary>
        public int Ronda => _s.Ronda;

        public IReadOnlyList<LoboPlayer> Jugadores => _s.Jugadores;
        public IEnumerable<LoboPlayer> Vivos => _s.Jugadores.Where(p => p.Vivo);

        /// <summary>Las dos cartas que se le ofrecen al ladrón la primera noche.</summary>
        public IReadOnlyList<LoboRol> CartasLadron => _s.CartasLadron;

        /// <summary>A quién le toca ver su carta en el reparto.</summary>
        public int RepartoIndex => _s.RepartoIndex;

        /// <summary>El paso de la noche en curso (nulo fuera de la noche).</summary>
        public LoboStep Paso => _s.Fase == LoboFase.Noche && _s.Paso.HasValue ? CrearPaso(_s.Paso.Value) : null;

        /// <summary>El rol que vio la vidente en su paso (nulo si no ha mirado).</summary>
        public LoboRol? VidenteVio => _s.VidenteVio;

        /// <summary>Si el zorro olió un lobo en su paso.</summary>
        public bool? ZorroOlio => _s.ZorroOlio;

        /// <summary>Si el oso gruñó esta mañana.</summary>
        public bool? OsoGrune => _s.OsoGrune;

        public IReadOnlyList<int> MuertesRecientes => _s.MuertesRecientes;
        public IReadOnlyList<LoboPendiente> Pendientes => _s.Pendientes;
        public LoboPendiente PendienteActual => _s.Pendientes.FirstOrDefault();
        public IReadOnlyList<LoboEvento> Registro => _s.Registro;
        public LoboVictoria Victoria => _s.Victoria;
        public LoboLuna? Luna => _s.Luna;
        public bool PoderesPerdidos => _s.PoderesPerdidos;
        public int? CuervoMarcado => _s.CuervoMarcado;
        public bool LinchamientoHecho => _s.LinchamientoHecho;

        /// <summary>Con empate, los que van a la segunda votación (vacío si no la hay).</summary>
        public IReadOnlyList<int> Empatados => _s.Empatados;

        public bool PuedeDeshacer => _undo.Count > 0;

        /// <summary>Pasos ya resueltos esta noche (0 = acaba de empezar).</summary>
        public int PasosResueltos => _s.PasosNoche;

        /// <summary>Si hoy se dice el rol de los que mueren (la regla, salvo con niebla).</summary>
        public bool RevelarRoles => Rules != null && Rules.RevelarRolAlMorir && _s.Luna != LoboLuna.Niebla;

        /// <summary>Si hoy hay debate (no lo hay con eclipse).</summary>
        public bool HayDebate => Rules != null && Rules.DebateSegundos > 0 && _s.Luna != LoboLuna.Eclipse;

        /// <summary>Si el voto del capitán vale doble hoy.</summary>
        public bool VotoDobleCapitan => Rules != null && Rules.Capitan && _s.Luna != LoboLuna.Rebelion;

        /// <summary>El primer día, o si el capitán murió sin sucesor, hay que elegir capitán.</summary>
        public bool NecesitaCapitan => Rules != null && Rules.Capitan && _s.Fase == LoboFase.Dia && !Vivos.Any(p => p.Capitan);

        public bool JuezDisponible => _s.Fase == LoboFase.Dia && _s.LinchamientoHecho && _s.Pendientes.Count == 0 && !_s.JuezUsado
                                      && TienePoder(LoboRol.JuezTartamudo) && _s.Victoria == null;

        public LoboPlayer Capitan => Vivos.FirstOrDefault(p => p.Capitan);

        // ------------------------------------------------------------------ Consultas

        /// <summary>Si cuenta como lobo (para los lobos, la vidente no: ella ve la carta).</summary>
        public bool EsLobo(int index)
        {
            var p = _s.Jugadores[index];
            return LoboCatalog.EsLoboDeCarta(p.Rol) || p.Infectado || (p.Rol == LoboRol.PerroLobo && _s.PerroLoboEsLobo == true)
                   || (p.Rol == LoboRol.NinoSalvaje && _s.NinoConvertido);
        }

        /// <summary>Con quién gana ahora.</summary>
        public LoboBando BandoDe(int index)
        {
            var p = _s.Jugadores[index];
            if (EsLobo(index)) return p.Rol == LoboRol.LoboBlanco ? LoboBando.Solitario : LoboBando.Lobos;
            if (p.Rol == LoboRol.Angel) return _s.Ronda <= 1 ? LoboBando.Solitario : LoboBando.Pueblo;
            return p.Rol == LoboRol.Flautista || p.Rol == LoboRol.Sectario ? LoboBando.Solitario : LoboBando.Pueblo;
        }

        /// <summary>Los vecinos vivos más cercanos a cada lado (uno solo si no queda nadie más).</summary>
        public IReadOnlyList<int> Vecinos(int index)
        {
            var n = _s.Jugadores.Count;
            var result = new List<int>();
            for (var k = 1; k < n; k++)
            {
                var left = (index - k + n) % n;
                if (_s.Jugadores[left].Vivo)
                {
                    result.Add(left);
                    break;
                }
            }
            for (var k = 1; k < n; k++)
            {
                var right = (index + k) % n;
                if (_s.Jugadores[right].Vivo)
                {
                    if (!result.Contains(right)) result.Add(right);
                    break;
                }
            }
            return result;
        }

        public bool TienePoder(LoboRol rol)
        {
            var holder = Vivos.FirstOrDefault(p => p.Rol == rol);
            if (holder == null) return false;
            return !_s.PoderesPerdidos || LoboCatalog.Get(rol).Bando != LoboBando.Pueblo;
        }

        // ------------------------------------------------------------------ Reparto

        public void Iniciar(LoboConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _undo.Clear();
            _s = new LoboState();
            var deck = new List<LoboRol>();
            foreach (var def in LoboCatalog.All)
            {
                for (var i = 0; i < config.Count(def.Rol); i++) deck.Add(def.Rol);
            }
            _random.Shuffle(deck);
            Deal(deck);
        }

        /// <summary>
        /// Reparto a mano (si el game master reparte cartas de verdad, o en los tests): el rol de cada jugador en orden y,
        /// con ladrón, las dos cartas que sobran.
        /// </summary>
        public void IniciarConReparto(LoboConfig config, IReadOnlyList<LoboRol> roles, IReadOnlyList<LoboRol> cartasLadron = null)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            if (roles == null || roles.Count != config.Jugadores.Count) throw new ArgumentException("Hace falta un rol por jugador", nameof(roles));
            _undo.Clear();
            _s = new LoboState();
            Deal(roles.Concat(cartasLadron ?? Array.Empty<LoboRol>()).ToList());
        }

        private void Deal(IReadOnlyList<LoboRol> deck)
        {
            var config = Config;
            for (var i = 0; i < config.Jugadores.Count; i++) _s.Jugadores.Add(new LoboPlayer(i, config.Jugadores[i], deck[i]));
            _s.CartasLadron = deck.Skip(config.Jugadores.Count).ToList();
            var sectario = _s.Jugadores.FirstOrDefault(p => p.Rol == LoboRol.Sectario);
            if (sectario != null)
            {
                var others = _random.Shuffled(_s.Jugadores.Where(p => p != sectario).Select(p => p.Index));
                for (var k = 0; k < others.Count; k++) _s.Jugadores[others[k]].Grupo = k % 2 == 0 ? 1 : 0;
                sectario.Grupo = 0;
            }
            _s.Ronda = 1;
            _s.Fase = LoboFase.Reparto;
            Log(LoboEventoTipo.Reparto);
            Changed?.Invoke();
        }

        /// <summary>El jugador del reparto ya ha visto su carta: al siguiente (o, tras el último, empieza la noche).</summary>
        public void SiguienteReparto()
        {
            // Pasar el móvil no se deshace: el game master empieza a deshacer desde la primera noche.
            if (_s.Fase != LoboFase.Reparto) return;
            _s.RepartoIndex++;
            if (_s.RepartoIndex >= _s.Jugadores.Count) StartNight();
            Changed?.Invoke();
        }

        /// <summary>Los demás lobos (para enseñárselos a un lobo al ver su carta, si la regla lo dice).</summary>
        public IReadOnlyList<int> CompanerosLobos(int index) =>
            _s.Jugadores.Where(p => p.Index != index && LoboCatalog.EsLoboDeCarta(p.Rol)).Select(p => p.Index).ToList();

        /// <summary>Los de su grupo (para enseñárselos al sectario).</summary>
        public IReadOnlyList<int> GrupoDe(int index) =>
            _s.Jugadores.Where(p => p.Index != index && p.Grupo == _s.Jugadores[index].Grupo).Select(p => p.Index).ToList();

        // ------------------------------------------------------------------ Noche

        /// <summary>
        /// Resuelve el paso en curso con los jugadores señalados y/o la opción elegida y pasa al siguiente (o al
        /// amanecer). Devuelve falso (y no hace nada) si la respuesta no vale para el paso.
        /// </summary>
        public bool ResolverPaso(IReadOnlyList<int> jugadores = null, LoboOpcion? opcion = null)
        {
            var step = Paso;
            if (step == null) return false;
            var picked = (jugadores ?? Array.Empty<int>()).Distinct().ToList();
            if (picked.Count < step.Min || picked.Count > step.Max || picked.Any(i => !step.Candidatos.Contains(i))) return false;
            if (step.Opciones.Count > 0 && (!opcion.HasValue || !step.Opciones.Contains(opcion.Value))) return false;
            Save();
            _s.PasosNoche++;
            Apply(step, picked, opcion);
            Advance(step.Paso);
            Changed?.Invoke();
            return true;
        }

        private void StartNight()
        {
            _s.Fase = LoboFase.Noche;
            _s.VictimaLobos = null;
            _s.VictimaLuna = null;
            _s.VictimaGranLobo = null;
            _s.VictimaLoboBlanco = null;
            _s.Protegido = null;
            _s.Infectar = false;
            _s.BrujaSalva = false;
            _s.BrujaMata = null;
            _s.EncantadosEstaNoche.Clear();
            _s.VidenteVio = null;
            _s.ZorroOlio = null;
            _s.OsoGrune = null;
            _s.CuervoMarcado = null;
            _s.MuertesRecientes.Clear();
            _s.Paso = null;
            _s.PasosNoche = 0;
            Advance(null);
        }

        private void Advance(LoboPaso? from)
        {
            var start = from.HasValue ? Array.IndexOf(Orden, from.Value) + 1 : 0;
            for (var i = start; i < Orden.Length; i++)
            {
                if (!Aplicable(Orden[i])) continue;
                _s.Paso = Orden[i];
                return;
            }
            _s.Paso = null;
            Dawn();
        }

        private LoboPlayer Holder(LoboRol rol) => Vivos.FirstOrDefault(p => p.Rol == rol);

        private bool FirstNight => _s.Ronda == 1;

        private bool Aplicable(LoboPaso paso)
        {
            switch (paso)
            {
                case LoboPaso.Ladron:
                    return FirstNight && _s.CartasLadron.Count == LoboConfig.CartasExtraLadron && Vivos.Any(p => p.Rol == LoboRol.Ladron);
                case LoboPaso.Cupido: return FirstNight && Holder(LoboRol.Cupido) != null && Vivos.Count() >= 2;
                case LoboPaso.Enamorados: return FirstNight && Vivos.Any(p => p.Pareja.HasValue);
                case LoboPaso.PerroLobo: return FirstNight && Holder(LoboRol.PerroLobo) != null && !_s.PerroLoboEsLobo.HasValue;
                case LoboPaso.NinoSalvaje: return FirstNight && Holder(LoboRol.NinoSalvaje) != null && !_s.NinoModelo.HasValue;
                case LoboPaso.Hermanas: return FirstNight && Vivos.Count(p => p.Rol == LoboRol.Hermana) >= 2;
                case LoboPaso.Hermanos: return FirstNight && Vivos.Count(p => p.Rol == LoboRol.Hermano) >= 2;
                case LoboPaso.Protector: return TienePoder(LoboRol.Protector) && CrearPaso(paso).Candidatos.Count > 0;
                case LoboPaso.Vidente: return TienePoder(LoboRol.Vidente) && _s.Luna != LoboLuna.LunaRoja;
                case LoboPaso.Zorro: return TienePoder(LoboRol.Zorro) && !_s.ZorroSinPoder;
                case LoboPaso.Lobos: return Vivos.Any(p => EsLobo(p.Index));
                case LoboPaso.PadreLobos: return Holder(LoboRol.PadreLobos) != null && !_s.PadreUsado && _s.VictimaLobos.HasValue;
                case LoboPaso.GranLoboMalo:
                    return Holder(LoboRol.GranLoboMalo) != null && !_s.LoboFamiliaMuerta && LobosMatan && CrearPaso(paso).Candidatos.Count > 0;
                case LoboPaso.LoboBlanco: return Holder(LoboRol.LoboBlanco) != null && _s.Ronda % 2 == 0 && CrearPaso(paso).Candidatos.Count > 0;
                case LoboPaso.BrujaSalva:
                    return TienePoder(LoboRol.Bruja) && !_s.BrujaCuraUsada && _s.VictimaLobos.HasValue && !_s.Infectar
                           && (Rules.BrujaPuedeSalvarse || _s.VictimaLobos != Holder(LoboRol.Bruja).Index);
                case LoboPaso.BrujaEnvenena: return TienePoder(LoboRol.Bruja) && !_s.BrujaVenenoUsada;
                case LoboPaso.Flautista: return Holder(LoboRol.Flautista) != null && CrearPaso(paso).Candidatos.Count > 0;
                case LoboPaso.Encantados: return _s.EncantadosEstaNoche.Count > 0;
                case LoboPaso.Cuervo: return TienePoder(LoboRol.Cuervo);
                default: return false;
            }
        }

        /// <summary>Si los lobos matan esta noche (la primera, según la regla, y con noche en calma, no).</summary>
        private bool LobosMatan => !(FirstNight && Rules.PrimeraNocheSinMuertes) && _s.Luna != LoboLuna.NocheEnCalma;

        private List<int> AliveWhere(Func<LoboPlayer, bool> filter) => Vivos.Where(filter).Select(p => p.Index).ToList();

        private LoboStep CrearPaso(LoboPaso paso)
        {
            switch (paso)
            {
                case LoboPaso.Ladron:
                {
                    var bothWolves = _s.CartasLadron.Count == 2 && _s.CartasLadron.All(LoboCatalog.EsLoboDeCarta);
                    var options = bothWolves
                        ? new[] { LoboOpcion.Carta1, LoboOpcion.Carta2 }
                        : new[] { LoboOpcion.Quedarse, LoboOpcion.Carta1, LoboOpcion.Carta2 };
                    return new LoboStep(paso, LoboRol.Ladron, 0, 0, null, options);
                }
                case LoboPaso.Cupido: return new LoboStep(paso, LoboRol.Cupido, 2, 2, AliveWhere(_ => true));
                case LoboPaso.Enamorados: return new LoboStep(paso, LoboRol.Cupido, 0, 0, null);
                case LoboPaso.PerroLobo:
                    return new LoboStep(paso, LoboRol.PerroLobo, 0, 0, null, new[] { LoboOpcion.Pueblo, LoboOpcion.Lobos });
                case LoboPaso.NinoSalvaje:
                {
                    var self = Holder(LoboRol.NinoSalvaje).Index;
                    return new LoboStep(paso, LoboRol.NinoSalvaje, 1, 1, AliveWhere(p => p.Index != self));
                }
                case LoboPaso.Hermanas: return new LoboStep(paso, LoboRol.Hermana, 0, 0, null);
                case LoboPaso.Hermanos: return new LoboStep(paso, LoboRol.Hermano, 0, 0, null);
                case LoboPaso.Protector:
                {
                    var self = Holder(LoboRol.Protector)?.Index ?? -1;
                    var candidates = AliveWhere(p => (Rules.ProtectorPuedeProtegerse || p.Index != self)
                                                     && (Rules.ProtectorRepetir || p.Index != _s.UltimoProtegido));
                    return new LoboStep(paso, LoboRol.Protector, 1, 1, candidates);
                }
                case LoboPaso.Vidente:
                {
                    var self = Holder(LoboRol.Vidente)?.Index ?? -1;
                    return new LoboStep(paso, LoboRol.Vidente, 1, 1, AliveWhere(p => p.Index != self));
                }
                case LoboPaso.Zorro: return new LoboStep(paso, LoboRol.Zorro, 0, 1, AliveWhere(_ => true));
                case LoboPaso.Lobos:
                {
                    var candidates = AliveWhere(p => !EsLobo(p.Index));
                    if (!LobosMatan || candidates.Count == 0) return new LoboStep(paso, LoboRol.Lobo, 0, 0, null);
                    var max = _s.Luna == LoboLuna.LunaLlena ? Math.Min(2, candidates.Count) : 1;
                    return new LoboStep(paso, LoboRol.Lobo, 1, max, candidates);
                }
                case LoboPaso.PadreLobos:
                    return new LoboStep(paso, LoboRol.PadreLobos, 0, 0, null, new[] { LoboOpcion.Si, LoboOpcion.No });
                case LoboPaso.GranLoboMalo:
                    return new LoboStep(paso, LoboRol.GranLoboMalo, 0, 1,
                        AliveWhere(p => !EsLobo(p.Index) && p.Index != _s.VictimaLobos && p.Index != _s.VictimaLuna));
                case LoboPaso.LoboBlanco:
                {
                    var self = Holder(LoboRol.LoboBlanco)?.Index ?? -1;
                    return new LoboStep(paso, LoboRol.LoboBlanco, 0, 1, AliveWhere(p => p.Index != self && EsLobo(p.Index)));
                }
                case LoboPaso.BrujaSalva: return new LoboStep(paso, LoboRol.Bruja, 0, 0, null, new[] { LoboOpcion.Si, LoboOpcion.No });
                case LoboPaso.BrujaEnvenena:
                {
                    var self = Holder(LoboRol.Bruja)?.Index ?? -1;
                    return new LoboStep(paso, LoboRol.Bruja, 0, 1, AliveWhere(p => p.Index != self));
                }
                case LoboPaso.Flautista:
                {
                    var self = Holder(LoboRol.Flautista)?.Index ?? -1;
                    var candidates = AliveWhere(p => p.Index != self && !p.Encantado);
                    var count = Math.Min(2, candidates.Count);
                    return new LoboStep(paso, LoboRol.Flautista, count, count, candidates);
                }
                case LoboPaso.Encantados: return new LoboStep(paso, LoboRol.Flautista, 0, 0, null);
                case LoboPaso.Cuervo: return new LoboStep(paso, LoboRol.Cuervo, 0, 1, AliveWhere(_ => true));
                default: throw new ArgumentOutOfRangeException(nameof(paso), paso, null);
            }
        }

        private void Apply(LoboStep step, IReadOnlyList<int> picked, LoboOpcion? option)
        {
            var first = picked.Count > 0 ? picked[0] : (int?)null;
            switch (step.Paso)
            {
                case LoboPaso.Ladron:
                {
                    var thief = Vivos.First(p => p.Rol == LoboRol.Ladron);
                    if (option == LoboOpcion.Quedarse)
                    {
                        Log(LoboEventoTipo.LadronCambia, thief.Index, rol: LoboRol.Ladron, si: false);
                        break;
                    }
                    var k = option == LoboOpcion.Carta1 ? 0 : 1;
                    var taken = _s.CartasLadron[k];
                    _s.CartasLadron[k] = LoboRol.Ladron;
                    thief.Rol = taken;
                    if (taken == LoboRol.Anciano) thief.VidasExtra = 1;
                    Log(LoboEventoTipo.LadronCambia, thief.Index, rol: taken, si: true);
                    break;
                }
                case LoboPaso.Cupido:
                    _s.Jugadores[picked[0]].Pareja = picked[1];
                    _s.Jugadores[picked[1]].Pareja = picked[0];
                    Log(LoboEventoTipo.Enamorados, picked[0], picked[1]);
                    break;
                case LoboPaso.PerroLobo:
                    _s.PerroLoboEsLobo = option == LoboOpcion.Lobos;
                    Log(LoboEventoTipo.PerroLoboElige, Holder(LoboRol.PerroLobo).Index, si: _s.PerroLoboEsLobo);
                    break;
                case LoboPaso.NinoSalvaje:
                    _s.NinoModelo = first;
                    Log(LoboEventoTipo.NinoSalvajeElige, Holder(LoboRol.NinoSalvaje).Index, first);
                    break;
                case LoboPaso.Protector:
                    _s.Protegido = first;
                    _s.UltimoProtegido = first;
                    Log(LoboEventoTipo.Protege, Holder(LoboRol.Protector).Index, first);
                    break;
                case LoboPaso.Vidente:
                    _s.VidenteVio = _s.Jugadores[first.Value].Rol;
                    Log(LoboEventoTipo.VidenteVe, Holder(LoboRol.Vidente).Index, first, _s.VidenteVio);
                    break;
                case LoboPaso.Zorro:
                    if (!first.HasValue) break;
                    var sniffed = new[] { first.Value }.Concat(Vecinos(first.Value)).Distinct();
                    _s.ZorroOlio = sniffed.Any(EsLobo);
                    Log(LoboEventoTipo.ZorroOlfatea, Holder(LoboRol.Zorro).Index, first, si: _s.ZorroOlio);
                    if (_s.ZorroOlio == false)
                    {
                        _s.ZorroSinPoder = true;
                        Log(LoboEventoTipo.ZorroPierde, Holder(LoboRol.Zorro).Index);
                    }
                    break;
                case LoboPaso.Lobos:
                    _s.VictimaLobos = first;
                    _s.VictimaLuna = picked.Count > 1 ? picked[1] : (int?)null;
                    if (first.HasValue) Log(LoboEventoTipo.LobosEligen, first, _s.VictimaLuna);
                    break;
                case LoboPaso.PadreLobos:
                    _s.Infectar = option == LoboOpcion.Si;
                    if (_s.Infectar) _s.PadreUsado = true;
                    break;
                case LoboPaso.GranLoboMalo:
                    _s.VictimaGranLobo = first;
                    break;
                case LoboPaso.LoboBlanco:
                    _s.VictimaLoboBlanco = first;
                    break;
                case LoboPaso.BrujaSalva:
                    _s.BrujaSalva = option == LoboOpcion.Si;
                    if (_s.BrujaSalva) _s.BrujaCuraUsada = true;
                    break;
                case LoboPaso.BrujaEnvenena:
                    if (!first.HasValue) break;
                    _s.BrujaMata = first;
                    _s.BrujaVenenoUsada = true;
                    break;
                case LoboPaso.Flautista:
                    foreach (var i in picked)
                    {
                        _s.Jugadores[i].Encantado = true;
                        _s.EncantadosEstaNoche.Add(i);
                    }
                    Log(LoboEventoTipo.Encanta, first, picked.Count > 1 ? picked[1] : (int?)null);
                    break;
                case LoboPaso.Cuervo:
                    _s.CuervoMarcado = first;
                    if (first.HasValue) Log(LoboEventoTipo.CuervoMarca, first);
                    break;
            }
        }

        // ------------------------------------------------------------------ Amanecer

        private void Dawn()
        {
            // Las muertes se apuntan aún en la noche; después, amanecer (salvo que ya haya ganado alguien).
            _s.MuertesRecientes.Clear();
            if (_s.OxidoPendiente.HasValue)
            {
                var rusty = _s.OxidoPendiente.Value;
                _s.OxidoPendiente = null;
                Kill(rusty, LoboMuerte.Oxido);
            }
            if (_s.VictimaLobos.HasValue) Attack(_s.VictimaLobos.Value, LoboMuerte.Lobos, true);
            if (_s.VictimaLuna.HasValue) Attack(_s.VictimaLuna.Value, LoboMuerte.Lobos, false);
            if (_s.VictimaGranLobo.HasValue) Attack(_s.VictimaGranLobo.Value, LoboMuerte.GranLobo, false);
            if (_s.BrujaMata.HasValue)
            {
                Log(LoboEventoTipo.Envenena, _s.BrujaMata);
                KillByVillage(_s.BrujaMata.Value, LoboMuerte.Veneno);
            }
            if (_s.VictimaLoboBlanco.HasValue) Kill(_s.VictimaLoboBlanco.Value, LoboMuerte.LoboBlanco);
            if (_s.Fase != LoboFase.Final) _s.Fase = LoboFase.Amanecer;
            if (TienePoder(LoboRol.OsoDomador))
            {
                var bear = Holder(LoboRol.OsoDomador).Index;
                _s.OsoGrune = Vecinos(bear).Any(EsLobo);
                Log(LoboEventoTipo.OsoGrune, bear, si: _s.OsoGrune);
            }
            if (Rules.EventosLuna && _s.Victoria == null)
            {
                _s.Luna = _random.NextDouble() < LoboLunas.Probabilidad ? _random.Pick(LoboLunas.All) : (LoboLuna?)null;
                if (_s.Luna.HasValue) Log(LoboEventoTipo.Luna, otro: (int)_s.Luna.Value);
            }
            else
            {
                _s.Luna = null;
            }
            CheckVictory();
        }

        /// <summary>Un ataque de lobo: protector, infección (solo la víctima principal), poción (ídem) y la vida del anciano.</summary>
        private void Attack(int index, LoboMuerte cause, bool primary)
        {
            var p = _s.Jugadores[index];
            if (!p.Vivo) return;
            if (_s.Protegido == index)
            {
                Log(LoboEventoTipo.Protegido, index);
                return;
            }
            if (primary && _s.Infectar)
            {
                p.Infectado = true;
                Log(LoboEventoTipo.Infecta, index);
                return;
            }
            if (primary && _s.BrujaSalva)
            {
                Log(LoboEventoTipo.Salva, index);
                return;
            }
            if (p.VidasExtra > 0)
            {
                p.VidasExtra--;
                Log(LoboEventoTipo.AncianoResiste, index);
                return;
            }
            Kill(index, cause);
            if (p.Rol == LoboRol.Caballero && !_s.PoderesPerdidos) _s.OxidoPendiente = FirstWolfAfter(index);
        }

        /// <summary>El primer lobo vivo siguiendo la mesa desde <paramref name="index"/> (a su izquierda).</summary>
        private int? FirstWolfAfter(int index)
        {
            var n = _s.Jugadores.Count;
            for (var k = 1; k < n; k++)
            {
                var j = (index + k) % n;
                if (_s.Jugadores[j].Vivo && EsLobo(j)) return j;
            }
            return null;
        }

        /// <summary>Una muerte que causa el pueblo (veneno, linchamiento, cazador): si es el anciano, pueden perderse los poderes.</summary>
        private void KillByVillage(int index, LoboMuerte cause)
        {
            var p = _s.Jugadores[index];
            if (!p.Vivo) return;
            Kill(index, cause);
            if (p.Rol == LoboRol.Anciano && Rules.AncianoQuitaPoderes && !_s.PoderesPerdidos)
            {
                _s.PoderesPerdidos = true;
                Log(LoboEventoTipo.PoderesPerdidos, index);
            }
        }

        private void Kill(int index, LoboMuerte cause)
        {
            var p = _s.Jugadores[index];
            if (!p.Vivo) return;
            var wasWolf = EsLobo(index);
            p.Vivo = false;
            p.Causa = cause;
            p.RondaMuerte = _s.Ronda;
            _s.MuertesRecientes.Add(index);
            Log(LoboEventoTipo.Muere, index, rol: p.Rol, causa: cause);
            if (wasWolf) _s.LoboFamiliaMuerta = true;
            if (p.Rol == LoboRol.Angel && _s.Ronda == 1 && _s.Victoria == null) SetVictory(LoboGanador.Angel, new[] { index });
            if (_s.NinoModelo == index && !_s.NinoConvertido)
            {
                var child = Holder(LoboRol.NinoSalvaje);
                if (child != null)
                {
                    _s.NinoConvertido = true;
                    Log(LoboEventoTipo.NinoSalvajeSeConvierte, child.Index);
                }
            }
            if (p.Rol == LoboRol.Cazador && !_s.PoderesPerdidos && Vivos.Any()) _s.Pendientes.Add(new LoboPendiente(LoboPendienteTipo.Cazador, index));
            if (p.Capitan)
            {
                p.Capitan = false;
                if (Vivos.Any()) _s.Pendientes.Add(new LoboPendiente(LoboPendienteTipo.Sucesor, index));
            }
            if (p.Pareja.HasValue && _s.Jugadores[p.Pareja.Value].Vivo) Kill(p.Pareja.Value, LoboMuerte.Pena);
        }

        // ------------------------------------------------------------------ Pendientes

        /// <summary>El cazador muerto dispara a alguien vivo.</summary>
        public bool Disparar(int objetivo)
        {
            var pending = PendienteActual;
            if (pending == null || pending.Tipo != LoboPendienteTipo.Cazador || !IsAlive(objetivo)) return false;
            Save();
            _s.Pendientes.RemoveAt(0);
            KillByVillage(objetivo, LoboMuerte.Cazador);
            AfterPending();
            Changed?.Invoke();
            return true;
        }

        /// <summary>El cazador no quiere (o no puede) disparar.</summary>
        public bool NoDisparar()
        {
            var pending = PendienteActual;
            if (pending == null || pending.Tipo != LoboPendienteTipo.Cazador) return false;
            Save();
            _s.Pendientes.RemoveAt(0);
            AfterPending();
            Changed?.Invoke();
            return true;
        }

        /// <summary>El capitán muerto nombra sucesor.</summary>
        public bool NombrarSucesor(int sucesor)
        {
            var pending = PendienteActual;
            if (pending == null || pending.Tipo != LoboPendienteTipo.Sucesor || !IsAlive(sucesor)) return false;
            Save();
            _s.Pendientes.RemoveAt(0);
            foreach (var p in _s.Jugadores) p.Capitan = false;
            _s.Jugadores[sucesor].Capitan = true;
            Log(LoboEventoTipo.Sucesor, sucesor, pending.Jugador);
            AfterPending();
            Changed?.Invoke();
            return true;
        }

        /// <summary>Antes de revelar al linchado, la sirvienta decide si se queda con su carta.</summary>
        public bool SirvientaDecide(bool toma)
        {
            var pending = PendienteActual;
            if (pending == null || pending.Tipo != LoboPendienteTipo.Sirvienta) return false;
            Save();
            _s.Pendientes.RemoveAt(0);
            var lynched = pending.Jugador;
            if (toma)
            {
                var servant = Holder(LoboRol.Sirvienta);
                var role = _s.Jugadores[lynched].Rol;
                servant.Rol = role;
                servant.VidasExtra = role == LoboRol.Anciano ? 1 : 0;
                _s.SirvientaUsada = true;
                Log(LoboEventoTipo.SirvientaToma, servant.Index, lynched, role);
            }
            ResolveLynch(lynched);
            AfterPending();
            Changed?.Invoke();
            return true;
        }

        private void AfterPending()
        {
            if (_s.Pendientes.Count == 0) CheckVictory();
        }

        private bool IsAlive(int index) => index >= 0 && index < _s.Jugadores.Count && _s.Jugadores[index].Vivo;

        // ------------------------------------------------------------------ Día

        /// <summary>Tras los anuncios del amanecer (y lo pendiente), empieza el día.</summary>
        public bool EmpezarDia()
        {
            if (_s.Fase != LoboFase.Amanecer || _s.Pendientes.Count > 0 || _s.Victoria != null) return false;
            Save();
            _s.Fase = LoboFase.Dia;
            _s.LinchamientoHecho = false;
            _s.LinchamientosHoy = 0;
            _s.LinchamientosPermitidos = _s.Luna == LoboLuna.Tormenta ? 2 : 1;
            _s.Empatados.Clear();
            Changed?.Invoke();
            return true;
        }

        public bool ElegirCapitan(int index)
        {
            if (_s.Fase != LoboFase.Dia || !Rules.Capitan || !IsAlive(index)) return false;
            Save();
            foreach (var p in _s.Jugadores) p.Capitan = false;
            _s.Jugadores[index].Capitan = true;
            Log(LoboEventoTipo.Capitan, index);
            Changed?.Invoke();
            return true;
        }

        /// <summary>El pueblo lincha al más votado (con segunda votación, solo puede ser uno de los empatados).</summary>
        public bool Linchar(int index)
        {
            if (_s.Fase != LoboFase.Dia || _s.LinchamientoHecho || _s.Pendientes.Count > 0 || !IsAlive(index)) return false;
            if (_s.Empatados.Count > 0 && !_s.Empatados.Contains(index)) return false;
            Save();
            var servant = Holder(LoboRol.Sirvienta);
            if (servant != null && servant.Index != index && !_s.SirvientaUsada && TienePoder(LoboRol.Sirvienta))
            {
                _s.Pendientes.Add(new LoboPendiente(LoboPendienteTipo.Sirvienta, index));
                Changed?.Invoke();
                return true;
            }
            ResolveLynch(index);
            AfterPending();
            Changed?.Invoke();
            return true;
        }

        private void ResolveLynch(int index)
        {
            _s.Empatados.Clear();
            var p = _s.Jugadores[index];
            Log(LoboEventoTipo.Linchado, index, rol: p.Rol);
            if (p.Rol == LoboRol.TontoDelPueblo && !p.TontoRevelado && !_s.PoderesPerdidos)
            {
                p.TontoRevelado = true;
                Log(LoboEventoTipo.TontoSeSalva, index);
                if (p.Capitan)
                {
                    p.Capitan = false;
                    _s.Pendientes.Add(new LoboPendiente(LoboPendienteTipo.Sucesor, index));
                }
            }
            else
            {
                KillByVillage(index, LoboMuerte.Linchado);
            }
            CountLynch();
        }

        private void CountLynch()
        {
            _s.LinchamientosHoy++;
            _s.LinchamientoHecho = _s.LinchamientosHoy >= _s.LinchamientosPermitidos;
        }

        /// <summary>
        /// Empate en la votación: con luna de sangre mueren todos los empatados; si hay chivo expiatorio, muere él; si no,
        /// según la regla, segunda votación entre los empatados (una vez) o no muere nadie.
        /// </summary>
        public bool Empate(IReadOnlyList<int> empatados)
        {
            if (_s.Fase != LoboFase.Dia || _s.LinchamientoHecho || _s.Pendientes.Count > 0) return false;
            var tied = (empatados ?? Array.Empty<int>()).Where(IsAlive).Distinct().ToList();
            if (tied.Count < 2) return false;
            Save();
            Log(LoboEventoTipo.Empate, tied[0], tied[1]);
            var goat = Holder(LoboRol.ChivoExpiatorio);
            if (_s.Luna == LoboLuna.LunaDeSangre)
            {
                _s.Empatados.Clear();
                foreach (var i in tied) KillByVillage(i, LoboMuerte.Linchado);
                CountLynch();
            }
            else if (goat != null)
            {
                _s.Empatados.Clear();
                Kill(goat.Index, LoboMuerte.Chivo);
                CountLynch();
            }
            else if (Rules.DesempateSegundaVotacion && _s.Empatados.Count == 0)
            {
                _s.Empatados.AddRange(tied);
            }
            else
            {
                _s.Empatados.Clear();
                Log(LoboEventoTipo.NadieMuere);
                CountLynch();
            }
            AfterPending();
            Changed?.Invoke();
            return true;
        }

        /// <summary>Hoy no se lincha a nadie.</summary>
        public bool NadieLinchado()
        {
            if (_s.Fase != LoboFase.Dia || _s.LinchamientoHecho || _s.Pendientes.Count > 0) return false;
            Save();
            _s.Empatados.Clear();
            Log(LoboEventoTipo.NadieMuere);
            CountLynch();
            Changed?.Invoke();
            return true;
        }

        /// <summary>El juez tartamudo pide (una vez en la partida) otra votación hoy mismo.</summary>
        public bool PedirSegundaVotacion()
        {
            if (!JuezDisponible) return false;
            Save();
            _s.JuezUsado = true;
            _s.LinchamientosPermitidos++;
            _s.LinchamientoHecho = false;
            Log(LoboEventoTipo.JuezSegundaVotacion, Holder(LoboRol.JuezTartamudo).Index);
            Changed?.Invoke();
            return true;
        }

        /// <summary>Se acaba el día: empieza la noche siguiente.</summary>
        public bool EmpezarNoche()
        {
            if (_s.Fase != LoboFase.Dia || _s.Pendientes.Count > 0 || _s.Victoria != null) return false;
            Save();
            _s.Ronda++;
            _s.Empatados.Clear();
            StartNight();
            Changed?.Invoke();
            return true;
        }

        // ------------------------------------------------------------------ Game master

        /// <summary>El game master mata a alguien a mano (p. ej. si se ha equivocado o alguien abandona).</summary>
        public bool MatarManual(int index)
        {
            if (!IsAlive(index) || _s.Fase == LoboFase.Setup || _s.Fase == LoboFase.Final) return false;
            Save();
            Kill(index, LoboMuerte.Master);
            AfterPending();
            Changed?.Invoke();
            return true;
        }

        /// <summary>El game master revive a alguien a mano.</summary>
        public bool RevivirManual(int index)
        {
            if (index < 0 || index >= _s.Jugadores.Count || _s.Jugadores[index].Vivo || _s.Fase == LoboFase.Final) return false;
            Save();
            var p = _s.Jugadores[index];
            p.Vivo = true;
            p.Causa = null;
            p.RondaMuerte = null;
            _s.MuertesRecientes.Remove(index);
            Log(LoboEventoTipo.Revive, index);
            Changed?.Invoke();
            return true;
        }

        /// <summary>El game master da la partida por terminada (sin ganador si no lo hay).</summary>
        public void Terminar()
        {
            if (_s.Fase == LoboFase.Setup || _s.Fase == LoboFase.Final) return;
            Save();
            _s.Fase = LoboFase.Final;
            Changed?.Invoke();
        }

        public bool Deshacer()
        {
            if (_undo.Count == 0) return false;
            _s = _undo[_undo.Count - 1];
            _undo.RemoveAt(_undo.Count - 1);
            Changed?.Invoke();
            return true;
        }

        public void Reiniciar()
        {
            Config = null;
            _s = new LoboState();
            _undo.Clear();
            Changed?.Invoke();
        }

        // ------------------------------------------------------------------ Victoria

        /// <summary>Quién ganaría ahora (nulo si la partida sigue).</summary>
        public LoboVictoria CalcularVictoria()
        {
            var alive = Vivos.ToList();
            if (alive.Count == 0) return new LoboVictoria(LoboGanador.Nadie, Array.Empty<int>());
            if (alive.Count == 2 && alive[0].Pareja == alive[1].Index) return new LoboVictoria(LoboGanador.Enamorados, new[] { alive[0].Index, alive[1].Index });
            var white = alive.FirstOrDefault(p => p.Rol == LoboRol.LoboBlanco);
            if (alive.Count == 1 && white != null) return new LoboVictoria(LoboGanador.LoboBlanco, new[] { white.Index });
            var piper = alive.FirstOrDefault(p => p.Rol == LoboRol.Flautista);
            if (piper != null && alive.All(p => p == piper || p.Encantado)) return new LoboVictoria(LoboGanador.Flautista, new[] { piper.Index });
            var sectarian = alive.FirstOrDefault(p => p.Rol == LoboRol.Sectario);
            if (sectarian != null && alive.All(p => p.Grupo == sectarian.Grupo)) return new LoboVictoria(LoboGanador.Sectario, new[] { sectarian.Index });
            var wolves = alive.Count(p => EsLobo(p.Index));
            if (wolves == 0) return new LoboVictoria(LoboGanador.Pueblo, Team(LoboBando.Pueblo));
            if (white != null) return null;
            var others = alive.Count - wolves;
            if (others == 0 || (Rules.LobosGananAlIgualar && wolves >= others)) return new LoboVictoria(LoboGanador.Lobos, Team(LoboBando.Lobos));
            return null;
        }

        private IReadOnlyList<int> Team(LoboBando bando) => _s.Jugadores.Where(p => BandoDe(p.Index) == bando).Select(p => p.Index).ToList();

        private void CheckVictory()
        {
            if (_s.Victoria == null)
            {
                var victory = CalcularVictoria();
                if (victory != null) SetVictory(victory.Ganador, victory.Jugadores);
            }
            if (_s.Victoria != null && _s.Pendientes.Count == 0) _s.Fase = LoboFase.Final;
        }

        private void SetVictory(LoboGanador ganador, IReadOnlyList<int> jugadores)
        {
            _s.Victoria = new LoboVictoria(ganador, jugadores);
            Log(LoboEventoTipo.Victoria);
            if (_s.Pendientes.Count == 0) _s.Fase = LoboFase.Final;
        }

        // ------------------------------------------------------------------ Ayudas

        private void Save()
        {
            _undo.Add(_s.Clone());
            if (_undo.Count > MaxDeshacer) _undo.RemoveAt(0);
        }

        private void Log(LoboEventoTipo tipo, int? jugador = null, int? otro = null, LoboRol? rol = null, LoboMuerte? causa = null, bool? si = null) =>
            _s.Registro.Add(new LoboEvento(_s.Ronda, _s.Fase == LoboFase.Noche || _s.Fase == LoboFase.Reparto, tipo, jugador, otro, rol, causa, si));
    }
}
