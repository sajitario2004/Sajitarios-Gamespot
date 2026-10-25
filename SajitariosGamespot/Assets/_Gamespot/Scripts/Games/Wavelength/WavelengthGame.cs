using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.Wavelength.Data;
using Gamespot.Games.Wavelength.Domain;

namespace Gamespot.Games.Wavelength
{
    /// <summary>Descriptor de Wavelength: cooperativo de pista y dial, con 80 espectros de seed.</summary>
    public sealed class WavelengthGame : IGameDescriptor
    {
        public string Id => "wavelength";
        public string TitleKey => "wavelengthMenuTitulo";
        public string DescriptionKey => "wavelengthMenuDescripcion";
        public IconId Icon => IconId.Tune;
        public string EntryRoute => WavelengthModule.SetupRoute;

        public WavelengthModule Module { get; private set; }

        public void Register(GameContext context)
        {
            var spectra = new SpectrumRepository(context.Db);
            var module = new WavelengthModule(new WavelengthFlowController(spectra, context.Random), spectra, context.Router);
            Module = module;
            var router = context.Router;
            router.Register(WavelengthModule.SetupRoute, () => new WavelengthSetupScreen(module));
            router.Register(WavelengthModule.ClueRoute, () => new WavelengthClueScreen(module));
            router.Register(WavelengthModule.PassRoute, () => new WavelengthPassScreen(module));
            router.Register(WavelengthModule.GuessRoute, () => new WavelengthGuessScreen(module));
            router.Register(WavelengthModule.RevealRoute, () => new WavelengthRevealScreen(module));
            router.Register(WavelengthModule.GameOverRoute, () => new WavelengthGameOverScreen(module));
        }

        public void OnCreate(IDb db) => WavelengthSchema.Create(db);

        public void OnUpgrade(IDb db, int oldVersion, int newVersion)
        {
        }

        public void SeedIfEmpty(IDb db, ISeedSource seeds) => WavelengthSchema.SeedIfEmpty(db, seeds);
    }
}
