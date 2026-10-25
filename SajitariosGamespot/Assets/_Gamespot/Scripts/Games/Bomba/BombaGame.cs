using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Bomba.Data;
using Gamespot.Games.Bomba.Domain;
using Gamespot.Games.Shared;

namespace Gamespot.Games.Bomba
{
    /// <summary>Descriptor de La Bomba: sílaba o categoría, mecha oculta y eliminación hasta que queda uno.</summary>
    public sealed class BombaGame : IGameDescriptor
    {
        public string Id => "bomba";
        public string TitleKey => "bombaMenuTitulo";
        public string DescriptionKey => "bombaMenuDescripcion";
        public IconId Icon => IconId.Fire;
        public string EntryRoute => BombaModule.SetupRoute;

        public BombaModule Module { get; private set; }

        public void Register(GameContext context)
        {
            var module = new BombaModule(new BombaFlowController(new BombaPromptRepository(context.Db), context.Random), context.Router);
            Module = module;
            context.Router.Register(BombaModule.SetupRoute, () => new BombaSetupScreen(module));
            context.Router.Register(BombaModule.PlayRoute, () => new BombaPlayScreen(module));
            context.Router.Register(BombaModule.GameOverRoute, () => new BombaGameOverScreen(module));
        }

        public void OnCreate(IDb db) => BombaSchema.Create(db);

        public void OnUpgrade(IDb db, int oldVersion, int newVersion)
        {
        }

        public void SeedIfEmpty(IDb db, ISeedSource seeds) => BombaSchema.SeedIfEmpty(db, seeds);
    }
}
