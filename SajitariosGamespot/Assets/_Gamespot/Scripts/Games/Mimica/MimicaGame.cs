using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Mimica.Domain;
using Gamespot.Games.Shared;
using UnityEngine;

namespace Gamespot.Games.Mimica
{
    /// <summary>
    /// Tablas de Mímica (V074): <c>mimica_cards</c> (la común de cartas, con categoría y sin nivel) y
    /// <c>mimica_categories</c> (categorías propias). Seed <c>[{texto, categoria}]</c>.
    /// </summary>
    public static class MimicaSchema
    {
        public const string Table = "mimica_cards";
        public const string CategoriesTable = "mimica_categories";
        public const string SeedName = "mimica_cards";

        public static void Create(IDb db)
        {
            PromptCardRepository.CreateTable(db, Table);
            CustomCategoryRepository.CreateTable(db, CategoriesTable);
        }

        public static int SeedIfEmpty(IDb db, ISeedSource seeds) =>
            PromptCardRepository.SeedIfEmpty(db, Table, PromptCardRepository.ParseSeed(seeds.Load(SeedName)));
    }

    /// <summary>Estado compartido de las pantallas de Mímica.</summary>
    public sealed class MimicaModule
    {
        public const string SetupRoute = "/mimica";
        public const string ReadyRoute = "/mimica/preparados";
        public const string TurnRoute = "/mimica/turno";
        public const string SummaryRoute = "/mimica/resumen";
        public const string EndRoute = "/mimica/final";
        public const string CardsRoute = "/mimica/tarjetas";

        public MimicaModule(GuessGameController flow, Router router, AudioService audio, PromptCardRepository cards, CustomCategoryRepository categories)
        {
            Flow = flow;
            Router = router;
            Audio = audio;
            Cards = cards;
            Categories = categories;
            Host = new GuessGameHost(flow, router, audio, "mimicaTitulo", SetupRoute, ReadyRoute, TurnRoute, SummaryRoute, EndRoute)
            {
                ReadyHelpKey = "mimicaPreparadoAyuda",
                TagLabel = tag => StyleLabel((MimicaEstilo)tag),
                PlayAgain = PlayAgain
            };
        }

        public GuessGameController Flow { get; }
        public Router Router { get; }
        public AudioService Audio { get; }
        public PromptCardRepository Cards { get; }
        public CustomCategoryRepository Categories { get; }
        public GuessGameHost Host { get; }

        /// <summary>La configuración de la partida en curso (para «Jugar otra»).</summary>
        public MimicaConfig Config { get; private set; }

        public List<string> DraftTeams { get; } = new List<string>();
        public HashSet<string> DraftCategories { get; } = new HashSet<string>(MimicaCategories.Default);
        public HashSet<string> KnownCategories { get; } = new HashSet<string>(MimicaCategories.Seed.Concat(new[] { WordCategories.Propias }));
        public MimicaOptions Draft { get; } = new MimicaOptions();

        public PromptCardsConfig CardsConfig => new PromptCardsConfig
        {
            TitleKey = "mimicaTarjetasTitulo",
            SearchHintKey = "buscarTarjeta",
            TextLabelKey = "mimicaCampoTarjeta",
            TextHintKey = "mimicaCampoTarjetaHint",
            HasLevels = false,
            SeedCategories = MimicaCategories.Seed,
            CustomCategories = Categories,
            MaxLength = 60
        };

        public static string StyleLabel(MimicaEstilo style) => Loc.T("mimicaEstilo" + style);

        public List<CustomCategory> CustomCategoriesOrEmpty()
        {
            try
            {
                return Categories.GetAll();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Mímica] No se pudieron cargar las categorías: {e.Message}");
                return new List<CustomCategory>();
            }
        }

        /// <summary>Las categorías con alguna tarjeta: las de serie, «Mis palabras» y las propias.</summary>
        public IReadOnlyList<CategoryOption> AvailableCategories()
        {
            var custom = CustomCategoriesOrEmpty();
            var ids = MimicaCategories.Seed.Concat(new[] { WordCategories.Propias }).Concat(custom.Select(c => c.CategoryId));
            try
            {
                var counts = Cards.CountByCategory();
                return CategoryOption.For(ids.Where(id => counts.TryGetValue(id, out var n) && n > 0), custom);
            }
            catch (Exception)
            {
                return CategoryOption.For(MimicaCategories.Seed, custom);
            }
        }

        public void Start(MimicaConfig config)
        {
            Config = config;
            Flow.Iniciar(config.Equipos, config.Query, config.GuessOptions);
        }

        public void PlayAgain()
        {
            if (Config == null)
            {
                Host.ResetToSetup();
                return;
            }
            Start(Config);
            if (Flow.Fase == GuessFase.Preparado) Host.GoToPhase();
            else Host.ResetToSetup();
        }
    }

    /// <summary>
    /// Descriptor de Mímica por equipos (V074): como el Tabú pero sin hablar, con categorías (también +18 y propias),
    /// tarjetas propias, tiempo, objetivo, pases y un modo mezcla (mímica, sonidos, dibujar en el aire o una palabra).
    /// </summary>
    public sealed class MimicaGame : IGameDescriptor
    {
        public string Id => "mimica";
        public string TitleKey => "mimicaMenuTitulo";
        public string DescriptionKey => "mimicaMenuDescripcion";
        public IconId Icon => IconId.SignLanguage;
        public string EntryRoute => MimicaModule.SetupRoute;

        public MimicaModule Module { get; private set; }

        public void Register(GameContext context)
        {
            var cards = new PromptCardRepository(context.Db, MimicaSchema.Table);
            var categories = new CustomCategoryRepository(context.Db, MimicaSchema.CategoriesTable);
            var module = new MimicaModule(new GuessGameController(cards, context.Random), context.Router, context.Audio, cards, categories);
            Module = module;
            context.Router.Register(MimicaModule.SetupRoute, () => new MimicaSetupScreen(module));
            context.Router.Register(MimicaModule.ReadyRoute, () => new GuessReadyScreen(module.Host));
            context.Router.Register(MimicaModule.TurnRoute, () => new MimicaTurnScreen(module));
            context.Router.Register(MimicaModule.SummaryRoute, () => new GuessSummaryScreen(module.Host));
            context.Router.Register(MimicaModule.EndRoute, () => new GuessEndScreen(module.Host));
            context.Router.Register(MimicaModule.CardsRoute, () => new PromptCardsScreen(cards, module.CardsConfig));
        }

        public void OnCreate(IDb db) => MimicaSchema.Create(db);

        public void OnUpgrade(IDb db, int oldVersion, int newVersion)
        {
        }

        public void SeedIfEmpty(IDb db, ISeedSource seeds) => MimicaSchema.SeedIfEmpty(db, seeds);
    }
}
