using System.Collections.Generic;
using Gamespot.Games.Bomba;
using Gamespot.Games.EsUn10Pero;
using Gamespot.Games.HombresLobo;
using Gamespot.Games.Impostor;
using Gamespot.Games.MasProbable;
using Gamespot.Games.Mimica;
using Gamespot.Games.QuePrefieres;
using Gamespot.Games.QuienSoy;
using Gamespot.Games.Trivia;
using Gamespot.Games.Wavelength;
using Gamespot.Games.YoNunca;
using Gamespot.Games.Shared;
using Gamespot.Games.Tabu;
using Gamespot.Games.VerdadReto;

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
            new TriviaGame(),
            new WavelengthGame(),
            new TabuGame(),
            new YoNuncaGame(),
            new BombaGame(),
            new VerdadRetoGame(),
            new MasProbableGame(),
            new QuePrefieresGame(),
            new MimicaGame(),
            new QuienSoyGame(),
            new HombresLoboGame(),
        };
    }
}
