using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Core
{
    /// <summary>V074: marcador por equipos y partida común de adivinar (Mímica y ¿Quién soy?).</summary>
    public sealed class GuessGameTests
    {
        private sealed class ListSource : IPromptSource
        {
            private readonly List<PromptCard> _cards;

            public ListSource(IEnumerable<PromptCard> cards) => _cards = cards.ToList();

            public IReadOnlyList<PromptCard> GetCards(PromptQuery query) => _cards.Where(query.Matches).ToList();
        }

        private static ListSource Cards(int n = 10) =>
            new ListSource(Enumerable.Range(1, n).Select(i => new PromptCard(i, $"carta {i}", category: i % 2 == 0 ? "par" : "impar")));

        private static readonly string[] Teams = { "Rojos", "Azules" };

        [Test]
        public void Marcador_GanaAlAcabarLaRondaEnElObjetivo()
        {
            var match = new TeamMatch(Teams, objetivo: 3);
            match.CerrarTurno(3);
            Assert.IsFalse(match.Terminada, "El otro equipo aún juega su turno");
            Assert.AreEqual(1, match.Actual);
            match.CerrarTurno(1);
            Assert.IsTrue(match.Terminada);
            CollectionAssert.AreEqual(new[] { 0 }, match.Ganadores);
        }

        [Test]
        public void Marcador_EmpateArribaJuegaOtraRonda()
        {
            var match = new TeamMatch(Teams, objetivo: 3);
            match.CerrarTurno(3);
            match.CerrarTurno(4);
            Assert.IsTrue(match.Terminada, "El segundo llega más alto");
            CollectionAssert.AreEqual(new[] { 1 }, match.Ganadores);

            var tie = new TeamMatch(Teams, objetivo: 3);
            tie.CerrarTurno(3);
            tie.CerrarTurno(3);
            Assert.IsFalse(tie.Terminada);
            Assert.IsTrue(tie.Desempate);
            Assert.AreEqual(2, tie.Ronda);
            tie.CerrarTurno(0);
            tie.CerrarTurno(1);
            Assert.IsTrue(tie.Terminada);
            CollectionAssert.AreEqual(new[] { 1 }, tie.Ganadores);
        }

        [Test]
        public void Marcador_PorRondasYSinBajarDeCero()
        {
            var match = new TeamMatch(new[] { "A", "B", "C" }, rondas: 2);
            match.CerrarTurno(-5);
            Assert.AreEqual(0, match.Puntos[0], "No baja de 0");
            match.CerrarTurno(2);
            match.CerrarTurno(1);
            Assert.AreEqual(2, match.Ronda);
            match.CerrarTurno(4);
            match.CerrarTurno(1);
            Assert.IsFalse(match.Terminada);
            match.CerrarTurno(1);
            Assert.IsTrue(match.Terminada);
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, match.Clasificacion);
            var open = new TeamMatch(Teams);
            for (var i = 0; i < 20; i++) open.CerrarTurno(5);
            Assert.IsFalse(open.Terminada, "Sin objetivo ni rondas, hasta terminar a mano");
            open.Terminar();
            Assert.AreEqual(2, open.Ganadores.Count, "Terminada a mano con empate: los dos");
        }

        [Test]
        public void Partida_TurnoConAciertosPasesYResumenCorregible()
        {
            var flow = new GuessGameController(Cards(), new SeededRandom(4));
            flow.Iniciar(Teams, new PromptQuery(), new GuessGameOptions { Objetivo = 2, Pases = 1, PenalizarPases = true });
            Assert.AreEqual(GuessFase.Preparado, flow.Fase);
            flow.Acierto();
            Assert.AreEqual(GuessFase.Preparado, flow.Fase, "Sin empezar el turno no se juega");
            flow.EmpezarTurno();
            Assert.IsNotNull(flow.Current);
            var first = flow.Current;
            flow.Acierto();
            Assert.AreNotSame(first, flow.Current);
            Assert.IsTrue(flow.PuedePasar);
            flow.Pasar();
            Assert.AreEqual(0, flow.PasesRestantes);
            Assert.IsFalse(flow.PuedePasar);
            var count = flow.Turno.Count;
            flow.Pasar();
            Assert.AreEqual(count, flow.Turno.Count, "Sin pases no se pasa");
            flow.Acierto();
            flow.TerminarTurno();
            Assert.AreEqual(GuessFase.Resumen, flow.Fase);
            Assert.IsNull(flow.Current, "La que estaba en juego no cuenta");
            Assert.AreEqual(2, flow.Aciertos);
            Assert.AreEqual(1, flow.Fallos);
            Assert.AreEqual(1, flow.PuntosTurno, "El pase resta");
            flow.Corregir(1);
            Assert.AreEqual(3, flow.PuntosTurno);
            flow.ConfirmarTurno();
            Assert.AreEqual(3, flow.Match.Puntos[0]);
            Assert.AreEqual(GuessFase.Preparado, flow.Fase);
            Assert.AreEqual(1, flow.Match.Actual);
            flow.EmpezarTurno();
            Assert.AreEqual(1, flow.PasesRestantes, "Los pases se reponen cada turno");
            flow.TerminarTurno();
            flow.ConfirmarTurno();
            Assert.AreEqual(GuessFase.Final, flow.Fase);
            Assert.AreEqual(3, flow.Jugadas, "Dos aciertos y un pase");
        }

        [Test]
        public void Partida_SinRepetirCategoriasYEtiquetas()
        {
            var random = new ScriptedRandom(true, 0.0, 0.5);
            var flow = new GuessGameController(Cards(), random);
            flow.Iniciar(Teams, new PromptQuery { Categories = new[] { "par" } }, new GuessGameOptions { Etiquetas = 4 });
            flow.EmpezarTurno();
            var ids = new List<long>();
            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual("par", flow.Current.Category);
                Assert.AreEqual(2, flow.CurrentTag, "La etiqueta: NextInt(4) con 0,5");
                ids.Add(flow.Current.Id);
                flow.Acierto();
            }
            Assert.AreEqual(5, ids.Distinct().Count(), "Sin repetir hasta agotar");
            Assert.IsTrue(flow.ConsumirAvisoMazo(), "La sexta vuelve a empezar");
            Assert.AreEqual(2, flow.Turno.Last().Etiqueta, "Cada carta guarda su etiqueta");
        }

        [Test]
        public void Partida_SinCartasErrorYTerminar()
        {
            var flow = new GuessGameController(Cards(), new SeededRandom(1));
            flow.Iniciar(Teams, new PromptQuery { Categories = new[] { "nada" } }, null);
            Assert.AreEqual(GuessFase.Error, flow.Fase);
            flow.Iniciar(Teams, null, null);
            flow.EmpezarTurno();
            flow.Terminar();
            Assert.AreEqual(GuessFase.Final, flow.Fase);
            flow.Reiniciar();
            Assert.AreEqual(GuessFase.Setup, flow.Fase);
            Assert.IsNull(flow.Match);
        }
    }
}
