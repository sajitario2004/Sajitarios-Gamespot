using System;
using System.Collections.Generic;
using Gamespot.Core;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.VerdadReto.Domain;
using UnityEngine;

namespace Gamespot.Games.VerdadReto
{
    /// <summary>
    /// Tabla <c>verdad_reto_cards</c> (la común de cartas, V070) y su seed <c>[{texto, tipo, nivel}]</c>: verdades,
    /// retos y castigos, suaves y picantes (+18). Las cartas del usuario van en la misma tabla con <c>is_seed = 0</c>.
    /// </summary>
    public static class VerdadRetoSchema
    {
        public const string Table = "verdad_reto_cards";
        public const string SeedName = "verdad_reto_cards";

        public static void Create(IDb db) => PromptCardRepository.CreateTable(db, Table);

        public static int SeedIfEmpty(IDb db, ISeedSource seeds) =>
            PromptCardRepository.SeedIfEmpty(db, Table, PromptCardRepository.ParseSeed(seeds.Load(SeedName)));
    }

    /// <summary>Estado compartido de las pantallas de Verdad o Reto.</summary>
    public sealed class VerdadRetoModule
    {
        public const string SetupRoute = "/verdad-reto";
        public const string TurnRoute = "/verdad-reto/turno";
        public const string CardRoute = "/verdad-reto/carta";
        public const string PenaltyRoute = "/verdad-reto/castigo";
        public const string EndRoute = "/verdad-reto/final";
        public const string CardsRoute = "/verdad-reto/cartas";

        public VerdadRetoModule(VerdadRetoFlowController flow, Router router, AudioService audio, PromptCardRepository cards)
        {
            Flow = flow;
            Router = router;
            Audio = audio;
            Cards = cards;
        }

        public VerdadRetoFlowController Flow { get; }
        public Router Router { get; }
        public AudioService Audio { get; }
        public PromptCardRepository Cards { get; }

        /// <summary>Nombres del setup (se conservan al ir y volver).</summary>
        public List<string> DraftPlayers { get; } = new List<string>();

        public HashSet<PromptLevel> DraftLevels { get; } = new HashSet<PromptLevel> { PromptLevel.Suave };
        public VerdadRetoOptions Draft { get; } = new VerdadRetoOptions();

        /// <summary>La gestión de cartas: verdades, retos y castigos, con nivel y sin categorías.</summary>
        public static PromptCardsConfig CardsConfig => new PromptCardsConfig
        {
            TitleKey = "verdadRetoCartasTitulo",
            TextHintKey = "verdadRetoCampoCartaHint",
            Kinds = new[]
            {
                (VerdadRetoKinds.Verdad, "verdadRetoVerdad"),
                (VerdadRetoKinds.Reto, "verdadRetoReto"),
                (VerdadRetoKinds.Castigo, "verdadRetoCastigo")
            }
        };

        public long OwnCountOrZero()
        {
            try
            {
                return Cards.CountOwn();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Verdad o Reto] No se pudieron contar las cartas propias: {e.Message}");
                return 0;
            }
        }

        /// <summary>La pantalla de la fase en curso.</summary>
        public string RouteForPhase()
        {
            switch (Flow.Fase)
            {
                case VerdadRetoFase.Eleccion: return TurnRoute;
                case VerdadRetoFase.Carta: return CardRoute;
                case VerdadRetoFase.Castigo: return PenaltyRoute;
                case VerdadRetoFase.Final: return EndRoute;
                default: return SetupRoute;
            }
        }

        /// <summary>Lleva a la pantalla de la fase en curso.</summary>
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

        /// <summary>«Jugar otra»: los mismos jugadores y reglas, con el marcador a cero.</summary>
        public void PlayAgain()
        {
            var config = Flow.Config;
            if (config == null)
            {
                ResetToSetup();
                return;
            }
            Flow.Iniciar(config);
            if (Flow.Fase == VerdadRetoFase.Eleccion) GoToPhase();
            else ResetToSetup();
        }
    }

    /// <summary>
    /// Descriptor de Verdad o Reto (V071): por turnos, cada jugador elige verdad o reto (o lo sortea el móvil); quien
    /// se raja cumple una prenda. Suave y picante +18, cartas propias y muchas reglas a elegir.
    /// </summary>
    public sealed class VerdadRetoGame : IGameDescriptor
    {
        public string Id => "verdad_reto";
        public string TitleKey => "verdadRetoMenuTitulo";
        public string DescriptionKey => "verdadRetoMenuDescripcion";
        public IconId Icon => IconId.Whatshot;
        public string EntryRoute => VerdadRetoModule.SetupRoute;

        public VerdadRetoModule Module { get; private set; }

        public void Register(GameContext context)
        {
            var cards = new PromptCardRepository(context.Db, VerdadRetoSchema.Table);
            var module = new VerdadRetoModule(new VerdadRetoFlowController(cards, context.Random), context.Router, context.Audio, cards);
            Module = module;
            context.Router.Register(VerdadRetoModule.SetupRoute, () => new VerdadRetoSetupScreen(module));
            context.Router.Register(VerdadRetoModule.TurnRoute, () => new VerdadRetoTurnScreen(module));
            context.Router.Register(VerdadRetoModule.CardRoute, () => new VerdadRetoCardScreen(module));
            context.Router.Register(VerdadRetoModule.PenaltyRoute, () => new VerdadRetoPenaltyScreen(module));
            context.Router.Register(VerdadRetoModule.EndRoute, () => new VerdadRetoEndScreen(module));
            context.Router.Register(VerdadRetoModule.CardsRoute, () => new PromptCardsScreen(cards, VerdadRetoModule.CardsConfig));
        }

        public void OnCreate(IDb db) => VerdadRetoSchema.Create(db);

        public void OnUpgrade(IDb db, int oldVersion, int newVersion)
        {
        }

        public void SeedIfEmpty(IDb db, ISeedSource seeds) => VerdadRetoSchema.SeedIfEmpty(db, seeds);
    }
}
