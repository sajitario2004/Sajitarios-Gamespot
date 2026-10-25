using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamespot.Games.HombresLobo.Domain
{
    /// <summary>Con quién gana cada uno.</summary>
    public enum LoboBando
    {
        Pueblo,
        Lobos,

        /// <summary>Gana solo, con su propio objetivo (Ángel, Flautista, Sectario, Lobo blanco).</summary>
        Solitario
    }

    /// <summary>Paquetes de roles: el básico y las expansiones (V076).</summary>
    public enum LoboPaquete
    {
        Basico,

        /// <summary>Los del pueblo con poderes nuevos (protector, anciano, zorro…).</summary>
        Aldea,

        /// <summary>Lobos especiales y los que pueden acabar de lobos (perro lobo, niño salvaje).</summary>
        Lobos,

        /// <summary>Los que juegan solos (ángel, flautista, sectario).</summary>
        Solitarios
    }

    /// <summary>
    /// Roles de Hombres Lobo (V076). Los nombres son los tradicionales del juego de roles ocultos, sin marcas. El orden
    /// es el del catálogo (y el de los chips del setup).
    /// </summary>
    public enum LoboRol
    {
        Aldeano,
        Lobo,
        Vidente,
        Bruja,
        Cazador,
        Cupido,
        Nina,
        Ladron,
        Protector,
        Anciano,
        ChivoExpiatorio,
        TontoDelPueblo,
        Hermana,
        Hermano,
        Zorro,
        Cuervo,
        OsoDomador,
        Caballero,
        JuezTartamudo,
        Sirvienta,
        PerroLobo,
        NinoSalvaje,
        LoboBlanco,
        GranLoboMalo,
        PadreLobos,
        Angel,
        Flautista,
        Sectario
    }

    /// <summary>Lo que hay que saber de un rol para repartirlo y llevar la partida.</summary>
    public sealed class LoboRoleDef
    {
        public LoboRoleDef(LoboRol rol, LoboPaquete paquete, LoboBando bando, int max = 1, int grupo = 0, bool despiertaConLobos = false)
        {
            Rol = rol;
            Paquete = paquete;
            Bando = bando;
            Max = max;
            Grupo = grupo;
            DespiertaConLobos = despiertaConLobos;
        }

        public LoboRol Rol { get; }
        public LoboPaquete Paquete { get; }

        /// <summary>El bando al repartir (el perro lobo, el niño salvaje y los infectados pueden cambiar).</summary>
        public LoboBando Bando { get; }

        /// <summary>Cuántas cartas puede haber como mucho.</summary>
        public int Max { get; }

        /// <summary>Si es mayor que 0, van en grupo de exactamente ese número (hermanas 2, hermanos 3) o ninguno.</summary>
        public int Grupo { get; }

        /// <summary>Se despierta con los lobos para elegir víctima.</summary>
        public bool DespiertaConLobos { get; }

        /// <summary>Clave del nombre: <c>loboRolVidente</c>…</summary>
        public string NameKey => "loboRol" + Rol;

        /// <summary>Clave de lo que hace: <c>loboRolDescVidente</c>…</summary>
        public string DescriptionKey => "loboRolDesc" + Rol;
    }

    /// <summary>Catálogo de roles por paquetes (V076).</summary>
    public static class LoboCatalog
    {
        public static readonly IReadOnlyList<LoboRoleDef> All = new[]
        {
            new LoboRoleDef(LoboRol.Aldeano, LoboPaquete.Basico, LoboBando.Pueblo, 30),
            new LoboRoleDef(LoboRol.Lobo, LoboPaquete.Basico, LoboBando.Lobos, 10, despiertaConLobos: true),
            new LoboRoleDef(LoboRol.Vidente, LoboPaquete.Basico, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.Bruja, LoboPaquete.Basico, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.Cazador, LoboPaquete.Basico, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.Cupido, LoboPaquete.Basico, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.Nina, LoboPaquete.Basico, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.Ladron, LoboPaquete.Basico, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.Protector, LoboPaquete.Aldea, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.Anciano, LoboPaquete.Aldea, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.ChivoExpiatorio, LoboPaquete.Aldea, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.TontoDelPueblo, LoboPaquete.Aldea, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.Hermana, LoboPaquete.Aldea, LoboBando.Pueblo, 2, 2),
            new LoboRoleDef(LoboRol.Hermano, LoboPaquete.Aldea, LoboBando.Pueblo, 3, 3),
            new LoboRoleDef(LoboRol.Zorro, LoboPaquete.Aldea, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.Cuervo, LoboPaquete.Aldea, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.OsoDomador, LoboPaquete.Aldea, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.Caballero, LoboPaquete.Aldea, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.JuezTartamudo, LoboPaquete.Aldea, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.Sirvienta, LoboPaquete.Aldea, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.PerroLobo, LoboPaquete.Lobos, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.NinoSalvaje, LoboPaquete.Lobos, LoboBando.Pueblo),
            new LoboRoleDef(LoboRol.LoboBlanco, LoboPaquete.Lobos, LoboBando.Solitario, despiertaConLobos: true),
            new LoboRoleDef(LoboRol.GranLoboMalo, LoboPaquete.Lobos, LoboBando.Lobos, despiertaConLobos: true),
            new LoboRoleDef(LoboRol.PadreLobos, LoboPaquete.Lobos, LoboBando.Lobos, despiertaConLobos: true),
            new LoboRoleDef(LoboRol.Angel, LoboPaquete.Solitarios, LoboBando.Solitario),
            new LoboRoleDef(LoboRol.Flautista, LoboPaquete.Solitarios, LoboBando.Solitario),
            new LoboRoleDef(LoboRol.Sectario, LoboPaquete.Solitarios, LoboBando.Solitario)
        };

        private static readonly Dictionary<LoboRol, LoboRoleDef> ByRole = All.ToDictionary(r => r.Rol);

        public static LoboRoleDef Get(LoboRol rol) => ByRole[rol];

        public static IReadOnlyList<LoboRoleDef> Of(LoboPaquete paquete) => All.Where(r => r.Paquete == paquete).ToList();

        /// <summary>Lobos de carta: se despiertan juntos de noche y cuentan como lobos para los demás.</summary>
        public static bool EsLoboDeCarta(LoboRol rol) => Get(rol).DespiertaConLobos;

        static LoboCatalog()
        {
            if (All.Count != Enum.GetValues(typeof(LoboRol)).Length) throw new InvalidOperationException("Falta algún rol en el catálogo");
        }
    }
}
