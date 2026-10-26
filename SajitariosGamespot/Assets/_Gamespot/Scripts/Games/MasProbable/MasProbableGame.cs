using System;
using System.Collections.Generic;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.MasProbable.Domain;
using Gamespot.Games.Shared;
using UnityEngine;

namespace Gamespot.Games.MasProbable
{
    /// <summary>
    /// Tabla <c>mas_probable_cards</c> (la común de cartas, V070) y su seed <c>[{texto, nivel}]</c>: lo que va detrás de
    /// «¿Quién es más probable que…», suave o picante (+18). Las del usuario, en la misma tabla con <c>is_seed = 0</c>.
    /// </summary>
    public static class MasProbableSchema
    {
        public const string Table = "mas_probable_cards";
        public const string SeedName = "mas_probable_cards";

        public static void Create(IDb db) => PromptCardRepository.CreateTable(db, Table);

        public static int SeedIfEmpty(IDb db, ISeedSource seeds) =>
            PromptCardRepository.SeedIfEmpty(db, Table, PromptCardRepository.ParseSeed(seeds.Load(SeedName)));
    }

    /// <summary>Estado compartido de las pantallas de ¿Quién es más probable que…?</summary>
    public sealed class MasProbableModule
    {
        public const string SetupRoute = "/mas-probable";
        public const string PhraseRoute = "/mas-probable/frase";
        public const string PointRoute = "/mas-probable/apuntar";
        public const string EndRoute = "/mas-probable/final";
        public const string CardsRoute = "/mas-probable/frases";

        public MasProbableModule(MasProbableFlowController flow, Router router, AudioService audio, PromptCardRepository cards)
        {
            Flow = flow;
            Router = router;
            Audio = audio;
            Cards = cards;
        }

        public MasProbableFlowController Flow { get; }
        public Router Router { get; }
        public AudioService Audio { get; }
        public PromptCardRepository Cards { get; }

        public List<string> DraftPlayers { get; } = new List<string>();
        public HashSet<PromptLevel> DraftLevels { get; } = new HashSet<PromptLevel> { PromptLevel.Suave };
        public MasProbableOptions Draft { get; } = new MasProbableOptions();

        /// <summary>La gestión de frases: solo el final de la pregunta y su nivel.</summary>
        public static PromptCardsConfig CardsConfig => new PromptCardsConfig
        {
            TitleKey = "masProbableFrasesTitulo",
            SearchHintKey = "masProbableBuscarFrase",
            TextLabelKey = "masProbableCampoFrase",
            TextHintKey = "masProbableCampoFraseHint",
            Normalize = MasProbableText.Normalize,
            MaxLength = MasProbableText.MaxLength
        };

        public long OwnCountOrZero()
        {
            try
            {
                return Cards.CountOwn();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Más probable] No se pudieron contar las frases propias: {e.Message}");
                return 0;
            }
        }

        public string RouteForPhase()
        {
            switch (Flow.Fase)
            {
                case MasProbableFase.Frase: return PhraseRoute;
                case MasProbableFase.Apuntar: return PointRoute;
                case MasProbableFase.Final: return EndRoute;
                default: return SetupRoute;
            }
        }

        public void GoToPhase() => Router.Go(RouteForPhase());

        public void ResetToMenu()
        {
            Flow.Reiniciar();
            Router.Go(Router.Home);
        }

        public void ResetToSetup()
        {
            Flow.Reiniciar();
            Router.Go(SetupRoute);
        }

        public void PlayAgain()
        {
            var config = Flow.Config;
            if (config == null)
            {
                ResetToSetup();
                return;
            }
            Flow.Iniciar(config);
            if (Flow.Fase == MasProbableFase.Frase) GoToPhase();
            else ResetToSetup();
        }
    }

    /// <summary>
    /// Descriptor de ¿Quién es más probable que…? (V072): sale una frase, cuenta atrás y todos señalan a la vez; si se
    /// quiere, se apunta al más señalado y al final se ve quién es «el más probable de todo». Suave y picante +18.
    /// </summary>
    public sealed class MasProbableGame : IGameDescriptor
    {
        public string Id => "mas_probable";
        public string TitleKey => "masProbableMenuTitulo";
        public string DescriptionKey => "masProbableMenuDescripcion";
        public IconId Icon => IconId.TouchApp;
        public string EntryRoute => MasProbableModule.SetupRoute;

        public MasProbableModule Module { get; private set; }

        public void Register(GameContext context)
        {
            var cards = new PromptCardRepository(context.Db, MasProbableSchema.Table);
            var module = new MasProbableModule(new MasProbableFlowController(cards, context.Random), context.Router, context.Audio, cards);
            Module = module;
            context.Router.Register(MasProbableModule.SetupRoute, () => new MasProbableSetupScreen(module));
            context.Router.Register(MasProbableModule.PhraseRoute, () => new MasProbablePhraseScreen(module));
            context.Router.Register(MasProbableModule.PointRoute, () => new MasProbablePointScreen(module));
            context.Router.Register(MasProbableModule.EndRoute, () => new MasProbableEndScreen(module));
            context.Router.Register(MasProbableModule.CardsRoute, () => new PromptCardsScreen(cards, MasProbableModule.CardsConfig));
        }

        public void OnCreate(IDb db) => MasProbableSchema.Create(db);

        public void OnUpgrade(IDb db, int oldVersion, int newVersion)
        {
        }

        public void SeedIfEmpty(IDb db, ISeedSource seeds) => MasProbableSchema.SeedIfEmpty(db, seeds);
    }
}
