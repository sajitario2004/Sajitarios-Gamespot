using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.EsUn10Pero.Domain;
using Gamespot.Games.Shared;

namespace Gamespot.Games.EsUn10Pero
{
    /// <summary>
    /// Descriptor de "Es un 10 pero": sacar una carta al azar entre A y 10. No guarda nada en la base de datos.
    /// (En el Flutter no tenía ruta propia; aquí entra por <see cref="Route"/>.)
    /// </summary>
    public sealed class EsUn10PeroGame : IGameDescriptor
    {
        public const string Route = "/es-un-10-pero";

        public string Id => "es_un_10_pero";
        public string TitleKey => "esUn10PeroMenuTitulo";
        public string DescriptionKey => "esUn10PeroMenuDescripcion";
        public IconId Icon => IconId.Style;
        public string EntryRoute => Route;

        public void Register(GameContext context)
        {
            context.Router.Register(Route, () => new EsUn10PeroScreen(new DrawCardUseCase(context.Random), context.Audio));
        }

        public void OnCreate(IDb db)
        {
        }

        public void OnUpgrade(IDb db, int oldVersion, int newVersion)
        {
        }

        public void SeedIfEmpty(IDb db, ISeedSource seeds)
        {
        }
    }
}
