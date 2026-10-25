using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Impostor.Data;
using Gamespot.Games.Impostor.Domain;
using Gamespot.Games.Shared;

namespace Gamespot.Games.Impostor
{
    /// <summary>
    /// Descriptor de "El Impostor": palabras (2525 de seed por categorías + las del usuario) e historial en SQLite; setup, pasar
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
            var source = new CategoryWordSource(words);
            var flow = new ImpostorFlowController(source, context.Random);
            var module = new ImpostorModule(flow, words, history, context.Audio, context.Router, source);
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

        // Lo marca OnUpgrade y lo usa SeedIfEmpty en el mismo arranque: AppDatabase.Open llama a los dos sobre este
        // descriptor, y OnUpgrade no recibe los seeds.
        private bool _refreshSeed;

        public void OnCreate(IDb db) => ImpostorSchema.Create(db);

        public void OnUpgrade(IDb db, int oldVersion, int newVersion) => _refreshSeed = ImpostorSchema.NeedsSeedRefresh(oldVersion);

        public void SeedIfEmpty(IDb db, ISeedSource seeds)
        {
            if (_refreshSeed) ImpostorSchema.RefreshSeed(db, seeds);
            else ImpostorSchema.SeedIfEmpty(db, seeds);
            _refreshSeed = false;
        }
    }
}
