using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.Tabu.Data;
using Gamespot.Games.Tabu.Domain;

namespace Gamespot.Games.Tabu
{
    /// <summary>Descriptor del Tabú: dos equipos, turnos con tiempo y primero a 3 victorias de ronda.</summary>
    public sealed class TabuGame : IGameDescriptor
    {
        public string Id => "tabu";
        public string TitleKey => "tabuMenuTitulo";
        public string DescriptionKey => "tabuMenuDescripcion";
        public IconId Icon => IconId.DoNotDisturb;
        public string EntryRoute => TabuModule.SetupRoute;

        public TabuModule Module { get; private set; }

        public void Register(GameContext context)
        {
            var module = new TabuModule(new TabuFlowController(new TabuWordRepository(context.Db), context.Random), context.Router, context.Audio);
            Module = module;
            context.Router.Register(TabuModule.SetupRoute, () => new TabuSetupScreen(module));
            context.Router.Register(TabuModule.TurnRoute, () => new TabuTurnScreen(module));
            context.Router.Register(TabuModule.ScoreboardRoute, () => new TabuScoreboardScreen(module));
            context.Router.Register(TabuModule.GameOverRoute, () => new TabuGameOverScreen(module));
        }

        public void OnCreate(IDb db) => TabuSchema.Create(db);

        public void OnUpgrade(IDb db, int oldVersion, int newVersion)
        {
        }

        public void SeedIfEmpty(IDb db, ISeedSource seeds) => TabuSchema.SeedIfEmpty(db, seeds);
    }
}
