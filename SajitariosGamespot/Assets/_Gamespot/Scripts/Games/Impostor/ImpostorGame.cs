using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Impostor.Data;
using Gamespot.Games.Impostor.Domain;
using Gamespot.Games.Shared;

namespace Gamespot.Games.Impostor
{
    /// <summary>
    /// Descriptor de "El Impostor": palabras (2060 de seed + las del usuario) e historial en SQLite; setup, pasar
    /// el móvil, revelar, votación, fin, gestión de palabras e historial.
    /// </summary>
    public sealed class ImpostorGame : IGameDescriptor
    {
        public string Id => "impostor";
        public string TitleKey => "impostorMenuTitulo";
        public string DescriptionKey => "impostorMenuDescripcion";
        public IconId Icon => IconId.TheaterComedy;
        public string EntryRoute => ImpostorModule.SetupRoute;

        /// <summary>Estado del juego tras registrarse (para los tests).</summary>
        public ImpostorModule Module { get; private set; }

        public void Register(GameContext context)
        {
            var words = new WordRepository(context.Db);
            var history = new GameHistoryRepository(context.Db);
            var flow = new ImpostorFlowController(words, context.Random);
            var module = new ImpostorModule(flow, words, history, context.Audio, context.Router);
            Module = module;

            var router = context.Router;
            router.Register(ImpostorModule.SetupRoute, () => new ImpostorSetupScreen(module));
            router.Register(ImpostorModule.PassRoute, () => new PassDeviceScreen(module));
            router.Register(ImpostorModule.RevealRoute, () => new RevealScreen(module));
            router.Register(ImpostorModule.VotingRoute, () => new VotingScreen(module));
            router.Register(ImpostorModule.GameOverRoute, () => new GameOverScreen(module));
            router.Register(ImpostorModule.WordsRoute, () => new WordsScreen(module));
            router.Register(ImpostorModule.HistoryRoute, () => new HistoryScreen(module));
        }

        public void OnCreate(IDb db) => ImpostorSchema.Create(db);

        public void OnUpgrade(IDb db, int oldVersion, int newVersion)
        {
            // Versión 1 en Unity = esquema final del Flutter: aún no hay migraciones.
        }

        public void SeedIfEmpty(IDb db, ISeedSource seeds) => ImpostorSchema.SeedIfEmpty(db, seeds);
    }
}
