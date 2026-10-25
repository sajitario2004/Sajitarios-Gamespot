using System.Collections.Generic;
using System.Linq;

namespace Gamespot.Games.HombresLobo.Domain
{
    /// <summary>Cómo ha muerto alguien.</summary>
    public enum LoboMuerte
    {
        Lobos,
        GranLobo,
        LoboBlanco,
        Veneno,
        Linchado,
        Cazador,

        /// <summary>De pena, porque murió su enamorado.</summary>
        Pena,

        /// <summary>Por la espada oxidada del caballero.</summary>
        Oxido,

        /// <summary>El chivo expiatorio muere en el empate.</summary>
        Chivo,

        /// <summary>Lo ha decidido el game master a mano.</summary>
        Master
    }

    /// <summary>Un jugador y todo lo que le ha pasado.</summary>
    public sealed class LoboPlayer
    {
        public LoboPlayer(int index, string name, LoboRol rol)
        {
            Index = index;
            Name = name;
            Rol = rol;
            RolInicial = rol;
            VidasExtra = rol == LoboRol.Anciano ? 1 : 0;
        }

        /// <summary>Su sitio en la mesa (los nombres van en el orden en que están sentados).</summary>
        public int Index { get; }
        public string Name { get; }

        /// <summary>La carta que tiene ahora (el ladrón y la sirvienta pueden cambiarla).</summary>
        public LoboRol Rol { get; internal set; }

        public LoboRol RolInicial { get; internal set; }
        public bool Vivo { get; internal set; } = true;

        /// <summary>Le infectó el padre de los lobos: ahora es lobo (y sigue teniendo su carta).</summary>
        public bool Infectado { get; internal set; }

        /// <summary>Le ha encantado el flautista.</summary>
        public bool Encantado { get; internal set; }

        /// <summary>Su enamorado (lo elige Cupido).</summary>
        public int? Pareja { get; internal set; }

        public bool Capitan { get; internal set; }

        /// <summary>Vidas de más (el anciano aguanta un ataque de los lobos).</summary>
        public int VidasExtra { get; internal set; }

        /// <summary>El tonto del pueblo ya se salvó una vez del linchamiento: sigue vivo pero ya no vota.</summary>
        public bool TontoRevelado { get; internal set; }

        /// <summary>Grupo del sectario (0 o 1); el sectario está en el 0.</summary>
        public int Grupo { get; internal set; }

        public LoboMuerte? Causa { get; internal set; }
        public int? RondaMuerte { get; internal set; }

        public bool PuedeVotar => Vivo && !TontoRevelado;

        public LoboPlayer Clone() => (LoboPlayer)MemberwiseClone();
    }

    /// <summary>Pasos de la noche, en el orden en que se despiertan.</summary>
    public enum LoboPaso
    {
        Ladron,
        Cupido,
        Enamorados,
        PerroLobo,
        NinoSalvaje,
        Hermanas,
        Hermanos,
        Protector,
        Vidente,
        Zorro,
        Lobos,
        PadreLobos,
        GranLoboMalo,
        LoboBlanco,
        BrujaSalva,
        BrujaEnvenena,
        Flautista,
        Encantados,
        Cuervo
    }

    /// <summary>Respuestas de los pasos con opciones.</summary>
    public enum LoboOpcion
    {
        Si,
        No,
        Pueblo,
        Lobos,
        Quedarse,
        Carta1,
        Carta2
    }

    /// <summary>
    /// Un paso de la noche tal como lo ve el game master: quién se despierta, a cuántos tiene que señalar (entre los
    /// candidatos) o qué opciones hay. Con <c>Max</c> 0 y sin opciones es solo un aviso («se reconocen»).
    /// </summary>
    public sealed class LoboStep
    {
        public LoboStep(LoboPaso paso, LoboRol rol, int min, int max, IReadOnlyList<int> candidatos, IReadOnlyList<LoboOpcion> opciones = null)
        {
            Paso = paso;
            Rol = rol;
            Min = min;
            Max = max;
            Candidatos = candidatos ?? new int[0];
            Opciones = opciones ?? new LoboOpcion[0];
        }

        public LoboPaso Paso { get; }

        /// <summary>El rol que se despierta (para el icono y el texto).</summary>
        public LoboRol Rol { get; }

        public int Min { get; }
        public int Max { get; }
        public IReadOnlyList<int> Candidatos { get; }
        public IReadOnlyList<LoboOpcion> Opciones { get; }

        public bool EsAviso => Max == 0 && Opciones.Count == 0;
    }

    /// <summary>Qué ha pasado (para el registro del game master).</summary>
    public enum LoboEventoTipo
    {
        Reparto,
        LadronCambia,
        Enamorados,
        PerroLoboElige,
        NinoSalvajeElige,
        Protege,
        VidenteVe,
        ZorroOlfatea,
        ZorroPierde,
        LobosEligen,
        Infecta,
        Envenena,
        Salva,
        Encanta,
        CuervoMarca,
        AncianoResiste,
        Protegido,
        Muere,
        NinoSalvajeSeConvierte,
        OsoGrune,
        Capitan,
        Sucesor,
        Linchado,
        TontoSeSalva,
        Empate,
        NadieMuere,
        SirvientaToma,
        PoderesPerdidos,
        JuezSegundaVotacion,
        Revive,
        Luna,
        Victoria
    }

    /// <summary>Una línea del registro de la partida.</summary>
    public sealed class LoboEvento
    {
        public LoboEvento(int ronda, bool noche, LoboEventoTipo tipo, int? jugador = null, int? otro = null, LoboRol? rol = null,
            LoboMuerte? causa = null, bool? si = null)
        {
            Ronda = ronda;
            Noche = noche;
            Tipo = tipo;
            Jugador = jugador;
            Otro = otro;
            Rol = rol;
            Causa = causa;
            Si = si;
        }

        public int Ronda { get; }
        public bool Noche { get; }
        public LoboEventoTipo Tipo { get; }
        public int? Jugador { get; }
        public int? Otro { get; }
        public LoboRol? Rol { get; }
        public LoboMuerte? Causa { get; }

        /// <summary>Para los sí o no (el zorro olfatea un lobo, el oso gruñe…).</summary>
        public bool? Si { get; }
    }

    /// <summary>Lo que hay que resolver antes de seguir (el disparo del cazador, el sucesor del capitán…).</summary>
    public enum LoboPendienteTipo
    {
        Cazador,
        Sucesor,

        /// <summary>Antes de revelar al linchado: ¿la sirvienta quiere quedarse su carta?</summary>
        Sirvienta
    }

    public sealed class LoboPendiente
    {
        public LoboPendiente(LoboPendienteTipo tipo, int jugador)
        {
            Tipo = tipo;
            Jugador = jugador;
        }

        public LoboPendienteTipo Tipo { get; }

        /// <summary>El cazador o el capitán muertos, o el linchado (con la sirvienta).</summary>
        public int Jugador { get; }
    }

    public enum LoboGanador
    {
        Pueblo,
        Lobos,
        Enamorados,
        LoboBlanco,
        Angel,
        Flautista,
        Sectario,

        /// <summary>No queda nadie vivo.</summary>
        Nadie
    }

    public sealed class LoboVictoria
    {
        public LoboVictoria(LoboGanador ganador, IReadOnlyList<int> jugadores)
        {
            Ganador = ganador;
            Jugadores = jugadores;
        }

        public LoboGanador Ganador { get; }

        /// <summary>Los que ganan (vivos o muertos, según el bando).</summary>
        public IReadOnlyList<int> Jugadores { get; }
    }

    public enum LoboFase
    {
        Setup,

        /// <summary>Se pasa el móvil y cada uno ve su carta.</summary>
        Reparto,

        Noche,

        /// <summary>Se anuncian las muertes de la noche (y lo que haya pendiente).</summary>
        Amanecer,

        Dia,
        Final
    }

    /// <summary>
    /// Todo el estado de una partida, copiable para poder deshacer (V076). Lo cambia solo <see cref="LoboGame"/>.
    /// </summary>
    internal sealed class LoboState
    {
        public LoboFase Fase = LoboFase.Setup;
        public int Ronda;
        public List<LoboPlayer> Jugadores = new List<LoboPlayer>();
        public List<LoboRol> CartasLadron = new List<LoboRol>();
        public int RepartoIndex;

        // Noche en curso.
        public LoboPaso? Paso;
        public int? VictimaLobos;
        public int? VictimaLuna;
        public int? VictimaGranLobo;
        public int? VictimaLoboBlanco;
        public int? Protegido;
        public bool Infectar;
        public bool BrujaSalva;
        public int? BrujaMata;
        public List<int> EncantadosEstaNoche = new List<int>();

        // Lo que dura toda la partida.
        public int? UltimoProtegido;
        public int? CuervoMarcado;
        public int? OxidoPendiente;
        public bool BrujaCuraUsada;
        public bool BrujaVenenoUsada;
        public bool PadreUsado;
        public bool? PerroLoboEsLobo;
        public int? NinoModelo;
        public bool NinoConvertido;
        public bool ZorroSinPoder;
        public bool PoderesPerdidos;
        public bool JuezUsado;
        public bool SirvientaUsada;
        public bool LoboFamiliaMuerta;

        // Último resultado para el game master.
        public LoboRol? VidenteVio;
        public bool? ZorroOlio;
        public bool? OsoGrune;

        // Día.
        public bool LinchamientoHecho;
        public int LinchamientosHoy;
        public int LinchamientosPermitidos = 1;
        public List<int> Empatados = new List<int>();

        public List<LoboPendiente> Pendientes = new List<LoboPendiente>();
        public List<int> MuertesRecientes = new List<int>();
        public List<LoboEvento> Registro = new List<LoboEvento>();
        public LoboVictoria Victoria;

        // Eventos de luna (V081).
        public LoboLuna? Luna;

        public LoboState Clone()
        {
            var copy = (LoboState)MemberwiseClone();
            copy.Jugadores = Jugadores.Select(p => p.Clone()).ToList();
            copy.CartasLadron = new List<LoboRol>(CartasLadron);
            copy.EncantadosEstaNoche = new List<int>(EncantadosEstaNoche);
            copy.Empatados = new List<int>(Empatados);
            copy.Pendientes = new List<LoboPendiente>(Pendientes);
            copy.MuertesRecientes = new List<int>(MuertesRecientes);
            copy.Registro = new List<LoboEvento>(Registro);
            return copy;
        }
    }
}
