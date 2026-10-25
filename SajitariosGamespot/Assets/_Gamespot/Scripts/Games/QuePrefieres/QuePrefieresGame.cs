using System;
using System.Collections.Generic;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.QuePrefieres.Domain;
using Gamespot.Games.Shared;
using UnityEngine;

namespace Gamespot.Games.QuePrefieres
{
    /// <summary>
    /// Tabla <c>que_prefieres_cards</c> (la común de cartas, V070) y su seed <c>[{texto, texto_b, nivel}]</c>: las dos
    /// opciones de cada pregunta, suave o picante (+18). Las del usuario, en la misma tabla con <c>is_seed = 0</c>.
    /// </summary>
    public static class QuePrefieresSchema
    {
        public const string Table = "que_prefieres_cards";
        public const string SeedName = "que_prefieres_cards";

        public static void Create(IDb db) => PromptCardRepository.CreateTable(db, Table);

        public static int SeedIfEmpty(IDb db, ISeedSource seeds) =>
            PromptCardRepository.SeedIfEmpty(db, Table, PromptCardRepository.ParseSeed(seeds.Load(SeedName)));
    }

    /// <summary>Estado compartido de las pantallas de ¿Qué prefieres?</summary>
    public sealed class QuePrefieresModule
    {
        public const string SetupRoute = "/que-prefieres";
        public const string QuestionRoute = "/que-prefieres/pregunta";
        public const string ResultRoute = "/que-prefieres/resultado";
        public const string EndRoute = "/que-prefieres/final";
        public const string CardsRoute = "/que-prefieres/preguntas";

        public QuePrefieresModule(QuePrefieresFlowController flow, Router router, AudioService audio, PromptCardRepository cards)
        {
            Flow = flow;
            Router = router;
            Audio = audio;
            Cards = cards;
        }

        public QuePrefieresFlowController Flow { get; }
        public Router Router { get; }
        public AudioService Audio { get; }
        public PromptCardRepository Cards { get; }

        public HashSet<PromptLevel> DraftLevels { get; } = new HashSet<PromptLevel> { PromptLevel.Suave };
        public QuePrefieresOptions Draft { get; } = new QuePrefieresOptions();

        /// <summary>La gestión de preguntas: las dos opciones (obligatorias) y el nivel.</summary>
        public static PromptCardsConfig CardsConfig => new PromptCardsConfig
        {
            TitleKey = "quePrefieresPreguntasTitulo",
            SearchHintKey = "quePrefieresBuscarPregunta",
            TextLabelKey = "quePrefieresCampoA",
            TextHintKey = "quePrefieresCampoAHint",
            TextBLabelKey = "quePrefieresCampoB",
            TextBHintKey = "quePrefieresCampoBHint",
            MaxLength = 90
        };

        public long OwnCountOrZero()
        {
            try
            {
                return Cards.CountOwn();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qué prefieres] No se pudieron contar las preguntas propias: {e.Message}");
                return 0;
            }
        }

        public string RouteForPhase()
        {
            switch (Flow.Fase)
            {
                case QuePrefieresFase.Pregunta: return QuestionRoute;
                case QuePrefieresFase.Resultado: return ResultRoute;
                case QuePrefieresFase.Final: return EndRoute;
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
            if (Flow.Fase == QuePrefieresFase.Pregunta) GoToPhase();
            else ResetToSetup();
        }
    }

    /// <summary>
    /// Descriptor de ¿Qué prefieres? (V073): dos opciones imposibles, cada uno vota a mano y, si se quiere, se apuntan los
    /// votos para ver el porcentaje y quién es la minoría. Suave y picante +18, preguntas propias.
    /// </summary>
    public sealed class QuePrefieresGame : IGameDescriptor
    {
        public string Id => "que_prefieres";
        public string TitleKey => "quePrefieresMenuTitulo";
        public string DescriptionKey => "quePrefieresMenuDescripcion";
        public IconId Icon => IconId.AltRoute;
        public string EntryRoute => QuePrefieresModule.SetupRoute;

        public QuePrefieresModule Module { get; private set; }

        public void Register(GameContext context)
        {
            var cards = new PromptCardRepository(context.Db, QuePrefieresSchema.Table);
            var module = new QuePrefieresModule(new QuePrefieresFlowController(cards, context.Random), context.Router, context.Audio, cards);
            Module = module;
            context.Router.Register(QuePrefieresModule.SetupRoute, () => new QuePrefieresSetupScreen(module));
            context.Router.Register(QuePrefieresModule.QuestionRoute, () => new QuePrefieresQuestionScreen(module));
            context.Router.Register(QuePrefieresModule.ResultRoute, () => new QuePrefieresResultScreen(module));
            context.Router.Register(QuePrefieresModule.EndRoute, () => new QuePrefieresEndScreen(module));
            context.Router.Register(QuePrefieresModule.CardsRoute, () => new PromptCardsScreen(cards, QuePrefieresModule.CardsConfig));
        }

        public void OnCreate(IDb db) => QuePrefieresSchema.Create(db);

        public void OnUpgrade(IDb db, int oldVersion, int newVersion)
        {
        }

        public void SeedIfEmpty(IDb db, ISeedSource seeds) => QuePrefieresSchema.SeedIfEmpty(db, seeds);
    }
}
