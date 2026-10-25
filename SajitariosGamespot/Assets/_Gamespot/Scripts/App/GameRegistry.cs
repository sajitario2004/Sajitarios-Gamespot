using System.Collections.Generic;
using Gamespot.Games.EsUn10Pero;
using Gamespot.Games.Impostor;
using Gamespot.Games.Shared;

namespace Gamespot.App
{
    /// <summary>
    /// Registro de juegos (port de <c>gameRegistryProvider</c>): el ÚNICO sitio que conoce los juegos concretos.
    /// Añadir un juego = crear <c>Games/&lt;Juego&gt;/</c> y añadir aquí su descriptor. El orden es el del menú
    /// y el de creación de las tablas.
    /// </summary>
    public static class GameRegistry
    {
        public static IReadOnlyList<IGameDescriptor> CreateAll() => new IGameDescriptor[]
        {
            new EsUn10PeroGame(),
            new ImpostorGame(),
            // Los demás juegos se añaden aquí según se portan (VERSIONS.md, fases 5–10).
        };
    }
}
