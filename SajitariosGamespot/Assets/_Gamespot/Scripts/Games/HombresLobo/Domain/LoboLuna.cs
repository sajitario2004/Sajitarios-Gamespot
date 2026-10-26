using System.Collections.Generic;

namespace Gamespot.Games.HombresLobo.Domain
{
    /// <summary>
    /// Eventos de luna (V081): con la regla activada, cada mañana hay una probabilidad de que salga uno; cambia las
    /// reglas de ese día y de la noche siguiente.
    /// </summary>
    public enum LoboLuna
    {
        /// <summary>Luna llena: la noche siguiente los lobos eligen dos víctimas.</summary>
        LunaLlena,

        /// <summary>Noche en calma: la noche siguiente los lobos no matan.</summary>
        NocheEnCalma,

        /// <summary>Eclipse: hoy no hay debate, se vota directamente.</summary>
        Eclipse,

        /// <summary>Niebla: hoy no se revelan los roles de los que mueran.</summary>
        Niebla,

        /// <summary>Tormenta: hoy se vota dos veces.</summary>
        Tormenta,

        /// <summary>Luna roja: la noche siguiente la vidente no ve.</summary>
        LunaRoja,

        /// <summary>Hoy el voto del capitán vale como los demás.</summary>
        Rebelion,

        /// <summary>Hoy, con empate en la votación, mueren todos los empatados.</summary>
        LunaDeSangre
    }

    public static class LoboLunas
    {
        /// <summary>Probabilidad de que salga un evento cada mañana (con la regla activada).</summary>
        public const double Probabilidad = 1.0 / 3.0;

        public static readonly IReadOnlyList<LoboLuna> All = (LoboLuna[])System.Enum.GetValues(typeof(LoboLuna));
    }
}
