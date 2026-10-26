using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Games.VerdadReto.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.VerdadReto
{
    /// <summary>V071: configuración y partida de Verdad o Reto (turnos, cartas, rajarse, puntos, rondas y orden del random).</summary>
    public sealed class VerdadRetoFlowTests
    {
        private sealed class ListSource : IPromptSource
        {
            private readonly List<PromptCard> _cards;

            public ListSource(IEnumerable<PromptCard> cards) => _cards = cards.ToList();

            public IReadOnlyList<PromptCard> GetCards(PromptQuery query) => _cards.Where(query.Matches).ToList();
        }

        private static ListSource Source(int perKind = 3, bool castigos = true)
        {
            var cards = new List<PromptCard>();
            var id = 1;
            foreach (var kind in castigos ? VerdadRetoKinds.All : new[] { VerdadRetoKinds.Verdad, VerdadRetoKinds.Reto })
            {
                foreach (var level in PromptLevels.All)
                {
                    for (var i = 0; i < perKind; i++) cards.Add(new PromptCard(id++, $"{kind} {level} {i}", level, kind));
                }
            }
            return new ListSource(cards);
        }

        private static VerdadRetoConfig Config(VerdadRetoOptions options = null, params string[] names)
        {
            var (config, error) = VerdadRetoConfig.Create(names.Length > 0 ? names : new[] { "Ana", "Bea", "Carlos" }, new[] { PromptLevel.Suave },
                options);
            Assert.IsNull(error);
            return config;
        }

        private static VerdadRetoFlowController Started(VerdadRetoOptions options = null, IRandomProvider random = null, ListSource source = null,
            params string[] names)
        {
            var flow = new VerdadRetoFlowController(source ?? Source(), random ?? new SeededRandom(3));
            flow.Iniciar(Config(options, names));
            return flow;
        }

        [Test]
        public void Config_ValidaJugadoresYNiveles()
        {
            Assert.AreEqual(VerdadRetoConfigError.PocosJugadores, VerdadRetoConfig.Create(new[] { "Ana" }, new[] { PromptLevel.Suave }).error);
            Assert.AreEqual(VerdadRetoConfigError.DemasiadosJugadores,
                VerdadRetoConfig.Create(Enumerable.Range(0, 21).Select(i => $"J{i}"), new[] { PromptLevel.Suave }).error);
            Assert.AreEqual(VerdadRetoConfigError.NombreVacio, VerdadRetoConfig.Create(new[] { "Ana", "  " }, new[] { PromptLevel.Suave }).error);
            Assert.AreEqual(VerdadRetoConfigError.NombresRepetidos, VerdadRetoConfig.Create(new[] { "Ana", "ANA " }, new[] { PromptLevel.Suave }).error);
            Assert.AreEqual(VerdadRetoConfigError.SinNiveles, VerdadRetoConfig.Create(new[] { "Ana", "Bea" }, new PromptLevel[0]).error);
            var options = new VerdadRetoOptions { IncluirSerie = false, IncluirPropias = false, Rondas = -3 };
            var (config, _) = VerdadRetoConfig.Create(new[] { "  Ana  María ", "Bea" }, new[] { PromptLevel.Suave }, options);
            Assert.AreEqual("Ana María", config.Jugadores[0]);
            Assert.IsTrue(config.Options.IncluirSerie, "Sin origen = de serie");
            Assert.AreEqual(0, config.Options.Rondas);
            options.Puntos = false;
            Assert.IsTrue(config.Options.Puntos, "La configuración es una copia");
        }

        [Test]
        public void Turnos_EnOrdenConPuntosDeVerdadYReto()
        {
            var flow = Started();
            Assert.AreEqual(VerdadRetoFase.Eleccion, flow.Fase);
            Assert.AreEqual("Ana", flow.JugadorActual.Name);
            flow.Elegir(VerdadRetoKinds.Verdad);
            Assert.AreEqual(VerdadRetoFase.Carta, flow.Fase);
            Assert.AreEqual(VerdadRetoKinds.Verdad, flow.CartaActual.Kind);
            flow.Cumplir();
            Assert.AreEqual(VerdadRetoConfig.PuntosVerdad, flow.Jugadores[0].Puntos);
            Assert.AreEqual("Bea", flow.JugadorActual.Name);
            flow.Elegir(VerdadRetoKinds.Reto);
            Assert.AreEqual(VerdadRetoKinds.Reto, flow.CartaActual.Kind);
            flow.Cumplir();
            Assert.AreEqual(VerdadRetoConfig.PuntosReto, flow.Jugadores[1].Puntos);
            Assert.AreEqual(1, flow.Jugadores[1].Retos);
            flow.Elegir(VerdadRetoKinds.Verdad);
            flow.Cumplir();
            Assert.AreEqual(2, flow.Ronda, "Tras los tres, ronda 2");
            Assert.AreEqual("Ana", flow.JugadorActual.Name);
            Assert.AreEqual(3, flow.Turnos);
        }

        [Test]
        public void SinPuntos_SoloCuentaLoQueHace()
        {
            var flow = Started(new VerdadRetoOptions { Puntos = false });
            flow.Elegir(VerdadRetoKinds.Reto);
            flow.Cumplir();
            Assert.AreEqual(0, flow.Jugadores[0].Puntos);
            Assert.AreEqual(1, flow.Jugadores[0].Retos);
        }

        [Test]
        public void Rajarse_PrendaSinPuntosYSiNoSePuedeNoHaceNada()
        {
            var flow = Started();
            flow.Elegir(VerdadRetoKinds.Reto);
            flow.Rajarse();
            Assert.AreEqual(VerdadRetoFase.Castigo, flow.Fase);
            Assert.AreEqual(VerdadRetoKinds.Castigo, flow.CastigoActual.Kind);
            Assert.AreEqual(1, flow.Jugadores[0].Rajadas);
            flow.CastigoCumplido();
            Assert.AreEqual(0, flow.Jugadores[0].Puntos);
            Assert.AreEqual("Bea", flow.JugadorActual.Name);

            var strict = Started(new VerdadRetoOptions { PermitirRajarse = false });
            strict.Elegir(VerdadRetoKinds.Verdad);
            strict.Rajarse();
            Assert.AreEqual(VerdadRetoFase.Carta, strict.Fase, "Sin rajarse, la carta sigue");

            var noPenalties = Started(source: Source(castigos: false));
            noPenalties.Elegir(VerdadRetoKinds.Verdad);
            noPenalties.Rajarse();
            Assert.AreEqual(VerdadRetoFase.Castigo, noPenalties.Fase);
            Assert.IsNull(noPenalties.CastigoActual, "Sin cartas de castigo, la prenda la elige el grupo");
        }

        [Test]
        public void OtraCarta_HastaGastarLosCambiosYSinRepetir()
        {
            var flow = Started(new VerdadRetoOptions { CambiosPorTurno = 2 });
            flow.Elegir(VerdadRetoKinds.Verdad);
            var seen = new HashSet<long> { flow.CartaActual.Id };
            flow.OtraCarta();
            flow.OtraCarta();
            seen.Add(flow.CartaActual.Id);
            Assert.AreEqual(0, flow.CambiosRestantes);
            var last = flow.CartaActual;
            flow.OtraCarta();
            Assert.AreSame(last, flow.CartaActual, "Sin cambios no hace nada");
            Assert.AreEqual(VerdadRetoKinds.Verdad, flow.CartaActual.Kind);
        }

        [Test]
        public void MaxVerdadesSeguidas_ObligaARetoYSePoneACeroConUnReto()
        {
            var flow = Started(new VerdadRetoOptions { MaxVerdadesSeguidas = 2 }, names: new[] { "Ana", "Bea" });
            for (var i = 0; i < 2; i++)
            {
                flow.Elegir(VerdadRetoKinds.Verdad);
                flow.Cumplir();
                flow.Elegir(VerdadRetoKinds.Reto);
                flow.Cumplir();
            }
            Assert.AreEqual("Ana", flow.JugadorActual.Name);
            Assert.IsTrue(flow.VerdadBloqueada);
            Assert.IsFalse(flow.PuedeElegir(VerdadRetoKinds.Verdad));
            flow.Elegir(VerdadRetoKinds.Verdad);
            Assert.AreEqual(VerdadRetoFase.Eleccion, flow.Fase, "No deja elegir verdad");
            flow.Elegir(VerdadRetoKinds.Reto);
            flow.Cumplir();
            flow.Elegir(VerdadRetoKinds.Reto);
            flow.Cumplir();
            Assert.IsFalse(flow.VerdadBloqueada, "Tras un reto vuelve a poder");
        }

        [Test]
        public void SinRetos_LaVerdadNuncaSeBloquea()
        {
            var verdades = new ListSource(Enumerable.Range(1, 3).Select(i => new PromptCard(i, $"v{i}", kind: VerdadRetoKinds.Verdad)));
            var flow = Started(new VerdadRetoOptions { MaxVerdadesSeguidas = 2 }, source: verdades, names: new[] { "Ana", "Bea" });
            for (var i = 0; i < 6; i++)
            {
                Assert.IsTrue(flow.PuedeElegir(VerdadRetoKinds.Verdad), $"turno {i}");
                flow.Elegir(VerdadRetoKinds.Verdad);
                flow.Cumplir();
            }
            Assert.IsFalse(flow.Disponible(VerdadRetoKinds.Reto));
        }

        [Test]
        public void Rondas_AcabaTrasLaUltimaYRankingPorPuntos()
        {
            var flow = Started(new VerdadRetoOptions { Rondas = 2 }, names: new[] { "Ana", "Bea" });
            flow.Elegir(VerdadRetoKinds.Verdad);
            flow.Cumplir();
            flow.Elegir(VerdadRetoKinds.Reto);
            flow.Cumplir();
            Assert.IsTrue(flow.UltimaRonda);
            flow.Elegir(VerdadRetoKinds.Reto);
            flow.Rajarse();
            flow.CastigoCumplido();
            flow.Elegir(VerdadRetoKinds.Verdad);
            flow.Cumplir();
            Assert.AreEqual(VerdadRetoFase.Final, flow.Fase);
            CollectionAssert.AreEqual(new[] { "Bea", "Ana" }, flow.Ranking.Select(p => p.Name));
            Assert.AreEqual(3, flow.Ranking[0].Puntos);
        }

        [Test]
        public void Terminar_LlevaAlFinalDesdeCualquierFase()
        {
            var flow = Started();
            flow.Elegir(VerdadRetoKinds.Reto);
            flow.Terminar();
            Assert.AreEqual(VerdadRetoFase.Final, flow.Fase);
            flow.Reiniciar();
            Assert.AreEqual(VerdadRetoFase.Setup, flow.Fase);
            Assert.IsNull(flow.Config);
        }

        [Test]
        public void AlAzar_TodosJueganUnaVezPorRondaYSinRepetirAlCambiar()
        {
            var flow = Started(new VerdadRetoOptions { Orden = OrdenTurnos.AlAzar }, new SeededRandom(11), names: new[] { "A", "B", "C", "D" });
            var previous = -1;
            for (var round = 0; round < 25; round++)
            {
                var played = new HashSet<int>();
                for (var t = 0; t < 4; t++)
                {
                    Assert.AreNotEqual(previous, flow.TurnoIndex, "Nunca el mismo dos turnos seguidos");
                    Assert.IsTrue(played.Add(flow.TurnoIndex), "Cada uno una vez por ronda");
                    previous = flow.TurnoIndex;
                    flow.Elegir(VerdadRetoKinds.Verdad);
                    flow.Cumplir();
                }
                Assert.AreEqual(round + 2, flow.Ronda);
            }
        }

        [Test]
        public void OrdenDelRandom_EmpiezaSorteaYCarta()
        {
            // AlAzar: NextInt(3) → 0,7 → Carlos (2); Sortear: NextInt(2) → 0,6 → reto; carta: NextInt(3 retos) → 0,0 → la primera.
            var random = new ScriptedRandom(0.7, 0.6, 0.0);
            var flow = Started(new VerdadRetoOptions { Orden = OrdenTurnos.AlAzar, Eleccion = ModoEleccion.AlAzar }, random);
            Assert.AreEqual("Carlos", flow.JugadorActual.Name);
            flow.Sortear();
            Assert.AreEqual(VerdadRetoKinds.Reto, flow.TipoActual);
            Assert.AreEqual("reto Suave 0", flow.CartaActual.Text);
            Assert.AreEqual(3, random.Consumed);
        }

        [Test]
        public void Otro_PoneElNombreDeOtroJugador()
        {
            var cards = new ListSource(new[] { new PromptCard(1, "Imita a {otro}", kind: VerdadRetoKinds.Reto),
                new PromptCard(2, "Verdad", kind: VerdadRetoKinds.Verdad) });
            for (var seed = 0; seed < 20; seed++)
            {
                var flow = Started(source: cards, random: new SeededRandom(seed));
                flow.Elegir(VerdadRetoKinds.Reto);
                Assert.IsNotNull(flow.OtroJugador);
                Assert.AreNotEqual("Ana", flow.OtroJugador, "Nunca el que juega");
                Assert.AreEqual($"Imita a {flow.OtroJugador}", flow.TextoCarta);
            }
        }

        [Test]
        public void SinCartas_Error()
        {
            var flow = new VerdadRetoFlowController(new ListSource(new PromptCard[0]), new SeededRandom(1));
            flow.Iniciar(Config());
            Assert.AreEqual(VerdadRetoFase.Error, flow.Fase);
            var onlyPicante = new VerdadRetoFlowController(new ListSource(new[] { new PromptCard(1, "x", PromptLevel.Picante, VerdadRetoKinds.Reto) }),
                new SeededRandom(1));
            onlyPicante.Iniciar(Config());
            Assert.AreEqual(VerdadRetoFase.Error, onlyPicante.Fase, "Las picantes no salen si solo se eligió suave");
        }

        [Test]
        public void Mazo_NoRepiteHastaAgotarYAvisa()
        {
            var flow = Started(new VerdadRetoOptions { CambiosPorTurno = 0 }, names: new[] { "Ana", "Bea" });
            var ids = new List<long>();
            for (var i = 0; i < 3; i++)
            {
                flow.Elegir(VerdadRetoKinds.Verdad);
                ids.Add(flow.CartaActual.Id);
                Assert.IsFalse(flow.ConsumirAvisoMazo());
                flow.Cumplir();
            }
            Assert.AreEqual(3, ids.Distinct().Count());
            flow.Elegir(VerdadRetoKinds.Verdad);
            Assert.IsTrue(flow.ConsumirAvisoMazo());
            Assert.IsFalse(flow.ConsumirAvisoMazo(), "Una sola vez");
        }
    }
}
