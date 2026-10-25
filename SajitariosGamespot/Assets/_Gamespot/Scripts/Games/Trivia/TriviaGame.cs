using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.Trivia.Data;
using Gamespot.Games.Trivia.Domain;

namespace Gamespot.Games.Trivia
{
    /// <summary>Descriptor de "Preguntas por puntos": trivia por eliminación de 9 rondas con 1151 preguntas de seed.</summary>
    public sealed class TriviaGame : IGameDescriptor
    {
        public string Id => "trivia";
        public string TitleKey => "triviaMenuTitulo";
        public string DescriptionKey => "triviaMenuDescripcion";
        public IconId Icon => IconId.Quiz;
        public string EntryRoute => TriviaModule.SetupRoute;

        public TriviaModule Module { get; private set; }

        public void Register(GameContext context)
        {
            var questions = new QuestionRepository(context.Db);
            var winners = new WinnerRepository(context.Db);
            var module = new TriviaModule(new TriviaFlowController(questions, winners, context.Random), questions, winners, context.Router, context.Audio);
            Module = module;
            context.Router.Register(TriviaModule.SetupRoute, () => new TriviaSetupScreen(module));
            context.Router.Register(TriviaModule.PassRoute, () => new TriviaPassScreen(module));
            context.Router.Register(TriviaModule.QuestionRoute, () => new TriviaQuestionScreen(module));
            context.Router.Register(TriviaModule.GameOverRoute, () => new TriviaGameOverScreen(module));
        }

        public void OnCreate(IDb db) => TriviaSchema.Create(db);

        public void OnUpgrade(IDb db, int oldVersion, int newVersion) => TriviaSchema.Upgrade(db, oldVersion);

        public void SeedIfEmpty(IDb db, ISeedSource seeds) => TriviaSchema.SeedIfEmpty(db, seeds);
    }
}
