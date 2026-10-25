using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.QuienSoy.Domain;
using Gamespot.Games.Shared;
using UnityEngine;

namespace Gamespot.Games.QuienSoy
{
    /// <summary>
    /// Tablas de ¿Quién soy? (V075): <c>quien_soy_cards</c> (la común de cartas, con categoría y sin nivel) y
    /// <c>quien_soy_categories</c> (categorías propias). Seed <c>[{texto, categoria}]</c>.
    /// </summary>
    public static class QuienSoySchema
    {
        public const string Table = "quien_soy_cards";
        public const string CategoriesTable = "quien_soy_categories";
        public const string SeedName = "quien_soy_cards";

        public static void Create(IDb db)
        {
            PromptCardRepository.CreateTable(db, Table);
            CustomCategoryRepository.CreateTable(db, CategoriesTable);
        }

        public static int SeedIfEmpty(IDb db, ISeedSource seeds) =>
            PromptCardRepository.SeedIfEmpty(db, Table, PromptCardRepository.ParseSeed(seeds.Load(SeedName)));
    }

    /// <summary>Estado compartido de las pantallas de ¿Quién soy?</summary>
    public sealed class QuienSoyModule
    {
        public const string SetupRoute = "/quien-soy";
        public const string ReadyRoute = "/quien-soy/preparados";
        public const string TurnRoute = "/quien-soy/turno";
        public const string SummaryRoute = "/quien-soy/resumen";
        public const string EndRoute = "/quien-soy/final";
        public const string CardsRoute = "/quien-soy/tarjetas";

        public QuienSoyModule(GuessGameController flow, Router router, AudioService audio, PromptCardRepository cards,
            CustomCategoryRepository categories)
        {
            Flow = flow;
            Router = router;
            Audio = audio;
            Cards = cards;
            Categories = categories;
            Host = new GuessGameHost(flow, router, audio, "quienSoyTitulo", SetupRoute, ReadyRoute, TurnRoute, SummaryRoute, EndRoute)
            {
                ReadyHelpKey = "quienSoyPreparadoAyuda",
                StartTurnKey = "quienSoyEmpezar",
                PlayAgain = PlayAgain
            };
        }

        public GuessGameController Flow { get; }
        public Router Router { get; }
        public AudioService Audio { get; }
        public PromptCardRepository Cards { get; }
        public CustomCategoryRepository Categories { get; }
        public GuessGameHost Host { get; }
        public QuienSoyConfig Config { get; private set; }

        public List<string> DraftTeams { get; } = new List<string>();
        public List<string> DraftPlayers { get; } = new List<string>();
        public HashSet<string> DraftCategories { get; } = new HashSet<string>(QuienSoyCategories.Default);
        public HashSet<string> KnownCategories { get; } = new HashSet<string>(QuienSoyCategories.Seed.Concat(new[] { WordCategories.Propias }));
        public QuienSoyOptions Draft { get; } = new QuienSoyOptions();

        public PromptCardsConfig CardsConfig => new PromptCardsConfig
        {
            TitleKey = "quienSoyTarjetasTitulo",
            SearchHintKey = "buscarTarjeta",
            TextLabelKey = "quienSoyCampoTarjeta",
            TextHintKey = "quienSoyCampoTarjetaHint",
            HasLevels = false,
            SeedCategories = QuienSoyCategories.Seed,
            CustomCategories = Categories,
            MaxLength = 50
        };

        public List<CustomCategory> CustomCategoriesOrEmpty()
        {
            try
            {
                return Categories.GetAll();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Quién soy] No se pudieron cargar las categorías: {e.Message}");
                return new List<CustomCategory>();
            }
        }

        public IReadOnlyList<CategoryOption> AvailableCategories()
        {
            var custom = CustomCategoriesOrEmpty();
            var ids = QuienSoyCategories.Seed.Concat(new[] { WordCategories.Propias }).Concat(custom.Select(c => c.CategoryId));
            try
            {
                var counts = Cards.CountByCategory();
                return CategoryOption.For(ids.Where(id => counts.TryGetValue(id, out var n) && n > 0), custom);
            }
            catch (Exception)
            {
                return CategoryOption.For(QuienSoyCategories.Seed, custom);
            }
        }

        public void Start(QuienSoyConfig config)
        {
            Config = config;
            Flow.Iniciar(config.Nombres, config.Query, config.GuessOptions);
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
    /// Descriptor de ¿Quién soy? (V075): el móvil en la frente con la palabra hacia fuera; los demás la describen y se
    /// inclina el móvil para acertar (abajo) o pasar (arriba), con botones de respaldo. Por equipos o cada uno a lo
    /// suyo, por rondas, con categorías (también +18 y propias) y tarjetas propias.
    /// </summary>
    public sealed class QuienSoyGame : IGameDescriptor
    {
        public string Id => "quien_soy";
        public string TitleKey => "quienSoyMenuTitulo";
        public string DescriptionKey => "quienSoyMenuDescripcion";
        public IconId Icon => IconId.PsychologyAlt;
        public string EntryRoute => QuienSoyModule.SetupRoute;

        public QuienSoyModule Module { get; private set; }

        public void Register(GameContext context)
        {
            var cards = new PromptCardRepository(context.Db, QuienSoySchema.Table);
            var categories = new CustomCategoryRepository(context.Db, QuienSoySchema.CategoriesTable);
            var module = new QuienSoyModule(new GuessGameController(cards, context.Random), context.Router, context.Audio, cards, categories);
            Module = module;
            context.Router.Register(QuienSoyModule.SetupRoute, () => new QuienSoySetupScreen(module));
            context.Router.Register(QuienSoyModule.ReadyRoute, () => new GuessReadyScreen(module.Host));
            context.Router.Register(QuienSoyModule.TurnRoute, () => new QuienSoyTurnScreen(module));
            context.Router.Register(QuienSoyModule.SummaryRoute, () => new GuessSummaryScreen(module.Host));
            context.Router.Register(QuienSoyModule.EndRoute, () => new GuessEndScreen(module.Host));
            context.Router.Register(QuienSoyModule.CardsRoute, () => new PromptCardsScreen(cards, module.CardsConfig));
        }

        public void OnCreate(IDb db) => QuienSoySchema.Create(db);

        public void OnUpgrade(IDb db, int oldVersion, int newVersion)
        {
        }

        public void SeedIfEmpty(IDb db, ISeedSource seeds) => QuienSoySchema.SeedIfEmpty(db, seeds);
    }
}
