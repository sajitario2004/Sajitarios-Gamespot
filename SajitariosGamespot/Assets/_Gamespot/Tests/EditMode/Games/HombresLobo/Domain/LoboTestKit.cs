using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Games.HombresLobo.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.HombresLobo.Domain
{
    /// <summary>Ayudas de los tests de Hombres Lobo: partidas con los roles fijados y pasos de la noche.</summary>
    internal static class LoboTestKit
    {
        /// <summary>Una partida ya repartida (en la primera noche) con un rol por jugador, J0, J1…</summary>
        public static LoboGame Game(LoboRules rules, IRandomProvider random, IReadOnlyList<LoboRol> extras, params LoboRol[] roles)
        {
            var names = roles.Select((_, i) => $"J{i}").ToList();
            var counts = roles.Concat(extras ?? new LoboRol[0]).GroupBy(r => r).ToDictionary(g => g.Key, g => g.Count());
            var (config, error) = LoboConfig.Create(names, counts, rules ?? new LoboRules());
            Assert.IsNull(error, "Configuración de prueba");
            var game = new LoboGame(random ?? new SeededRandom(1));
            game.IniciarConReparto(config, roles, extras);
            for (var i = 0; i < roles.Length; i++) game.SiguienteReparto();
            Assert.AreEqual(LoboFase.Noche, game.Fase);
            return game;
        }

        public static LoboGame Game(params LoboRol[] roles) => Game(null, null, null, roles);

        public static LoboGame Game(LoboRules rules, params LoboRol[] roles) => Game(rules, null, null, roles);

        /// <summary>Resuelve el paso en curso, que tiene que ser <paramref name="paso"/>.</summary>
        public static void Step(LoboGame game, LoboPaso paso, params int[] picks)
        {
            Assert.IsNotNull(game.Paso, $"Se esperaba el paso {paso} y no hay paso");
            Assert.AreEqual(paso, game.Paso.Paso);
            Assert.IsTrue(game.ResolverPaso(picks), $"El paso {paso} no acepta {string.Join(",", picks)}");
        }

        public static void Option(LoboGame game, LoboPaso paso, LoboOpcion opcion)
        {
            Assert.IsNotNull(game.Paso, $"Se esperaba el paso {paso} y no hay paso");
            Assert.AreEqual(paso, game.Paso.Paso);
            Assert.IsTrue(game.ResolverPaso(null, opcion), $"El paso {paso} no acepta {opcion}");
        }

        /// <summary>Los pasos que quedan esta noche, resolviéndolos con lo mínimo (avisos, sin elegir o el primero).</summary>
        public static List<LoboPaso> FinishNight(LoboGame game)
        {
            var seen = new List<LoboPaso>();
            while (game.Fase == LoboFase.Noche && game.Paso != null)
            {
                var step = game.Paso;
                seen.Add(step.Paso);
                if (step.Opciones.Count > 0) game.ResolverPaso(null, step.Opciones.Contains(LoboOpcion.No) ? LoboOpcion.No : step.Opciones[0]);
                else game.ResolverPaso(step.Candidatos.Take(step.Min).ToList());
            }
            return seen;
        }

        public static void ToDay(LoboGame game)
        {
            Assert.AreEqual(LoboFase.Amanecer, game.Fase);
            Assert.IsTrue(game.EmpezarDia());
        }
    }
}
