using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.UI;

namespace Gamespot.Games.Shared
{
    /// <summary>
    /// Contrato de un juego (port de <c>GameDescriptor</c>). Es el único punto de extensión: añadir un juego =
    /// crear su carpeta y registrar su descriptor en <c>App/GameRegistry.cs</c>. El menú y el router nunca
    /// conocen juegos concretos. Como <see cref="IDbSchema"/>, el juego aporta sus tablas y sus seeds.
    /// </summary>
    public interface IGameDescriptor : IDbSchema
    {
        /// <summary>Identificador estable (<c>impostor</c>, <c>trivia</c>…).</summary>
        string Id { get; }

        /// <summary>Clave de localización del título del menú.</summary>
        string TitleKey { get; }

        /// <summary>Clave de localización de la descripción del menú.</summary>
        string DescriptionKey { get; }

        IconId Icon { get; }

        /// <summary>Ruta de entrada (su setup).</summary>
        string EntryRoute { get; }

        /// <summary>Registra sus rutas y prepara sus repositorios con los servicios de la app.</summary>
        void Register(GameContext context);
    }

    /// <summary>Servicios de la app que recibe cada juego al registrarse.</summary>
    public sealed class GameContext
    {
        public GameContext(Router router, IDb db, IRandomProvider random, AudioService audio)
        {
            Router = router;
            Db = db;
            Random = random;
            Audio = audio;
        }

        public Router Router { get; }
        public IDb Db { get; }
        public IRandomProvider Random { get; }
        public AudioService Audio { get; }
    }
}
