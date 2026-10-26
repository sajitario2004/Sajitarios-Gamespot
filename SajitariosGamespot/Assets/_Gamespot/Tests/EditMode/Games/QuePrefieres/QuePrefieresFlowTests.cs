using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Games.QuePrefieres.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.QuePrefieres
{
    /// <summary>V073: configuración, votos, porcentajes, minoría y partida de ¿Qué prefieres?</summary>
    public sealed class QuePrefieresFlowTests
    {
        private sealed class ListSource : IPromptSource
        {
            private readonly List<PromptCard> _cards;

            public ListSource(IEnumerable<PromptCard> cards) => _cards = cards.ToList();

            public IReadOnlyList<PromptCard> GetCards(PromptQuery query) => _cards.Where(query.Matches).ToList();
        }

        private static ListSource Source(int n = 4) =>
            new ListSource(Enumerable.Range(1, n).Select(i => new PromptCard(i, $"a{i}", PromptLevel.Suave, textB: $"b{i}")));

        private static QuePrefieresFlowController Started(QuePrefieresOptions options, ListSource source = null)
        {
            var flow = new QuePrefieresFlowController(source ?? Source(), new SeededRandom(9));
            flow.Iniciar(QuePrefieresConfig.Create(new[] { PromptLevel.Suave }, options));
            return flow;
        }

        [Test]
        public void Config_SinNivelesNulaYCopiaLasOpciones()
        {
            Assert.IsNull(QuePrefieresConfig.Create(new PromptLevel[0]));
            var options = new QuePrefieresOptions { IncluirSerie = false, IncluirPropias = false, Preguntas = -1 };
            var config = QuePrefieresConfig.Create(new[] { PromptLevel.Suave }, options);
            Assert.IsTrue(config.Options.IncluirSerie);
            Assert.AreEqual(0, config.Options.Preguntas);
            options.Modo = QuePrefieresModo.ContarVotos;
            Assert.AreEqual(QuePrefieresModo.SoloPreguntas, config.Options.Modo);
        }

        [Test]
        public void Ronda_PorcentajesQueSuman100MinoriaYDivision()
        {
            var card = new PromptCard(1, "a", textB: "b");
            var round = new QuePrefieresRound(card, 1, 2);
            Assert.AreEqual(33, round.PorcentajeA);
            Assert.AreEqual(67, round.PorcentajeB);
            Assert.AreEqual(QuePrefieresOpcion.A, round.Minoria);
            Assert.IsNull(new QuePrefieresRound(card, 2, 2).Minoria, "Empate: nadie");
            Assert.IsNull(new QuePrefieresRound(card, 0, 0).Minoria, "Sin votos: nadie");
            Assert.AreEqual(0, new QuePrefieresRound(card, 0, 0).PorcentajeA);
            Assert.AreEqual(1.0, new QuePrefieresRound(card, 3, 3).Division);
            Assert.AreEqual(0.0, new QuePrefieresRound(card, 4, 0).Division);
            Assert.AreEqual(50, new QuePrefieresRound(card, 1, 1).PorcentajeA);
        }

        [Test]
        public void SoloPreguntas_SinVotosYHastaElLimite()
        {
            var flow = Started(new QuePrefieresOptions { Preguntas = 3 });
            flow.Votar(QuePrefieresOpcion.A);
            Assert.AreEqual(0, flow.VotosA, "Sin contar votos no se apuntan");
            var seen = new HashSet<long>();
            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(QuePrefieresFase.Pregunta, flow.Fase);
                Assert.IsTrue(seen.Add(flow.Current.Id));
                flow.Revelar();
            }
            Assert.AreEqual(QuePrefieresFase.Final, flow.Fase);
            Assert.AreEqual(3, flow.Rondas.Count);
            Assert.IsNull(flow.MasDividida);
        }

        [Test]
        public void ContarVotos_ResultadoYResumen()
        {
            var flow = Started(new QuePrefieresOptions { Modo = QuePrefieresModo.ContarVotos });
            flow.Votar(QuePrefieresOpcion.A, 3);
            flow.Votar(QuePrefieresOpcion.B, 2);
            flow.Votar(QuePrefieresOpcion.B, -5);
            Assert.AreEqual(0, flow.VotosB, "Sin bajar de 0");
            flow.Votar(QuePrefieresOpcion.B, 1);
            flow.Revelar();
            Assert.AreEqual(QuePrefieresFase.Resultado, flow.Fase);
            Assert.AreEqual(QuePrefieresOpcion.B, flow.Ultima.Minoria);
            Assert.AreEqual(75, flow.Ultima.PorcentajeA);
            flow.Siguiente();
            Assert.AreEqual(0, flow.VotosA, "Cada pregunta empieza a cero");
            flow.Votar(QuePrefieresOpcion.A, 2);
            flow.Votar(QuePrefieresOpcion.B, 2);
            flow.Revelar();
            flow.Siguiente();
            flow.Revelar();
            flow.Terminar();
            Assert.AreEqual(QuePrefieresFase.Final, flow.Fase);
            Assert.AreSame(flow.Rondas[1], flow.MasDividida);
            Assert.AreSame(flow.Rondas[0], flow.MasUnanime);
            Assert.AreEqual(0, flow.Rondas[2].Total, "La que no se votó no cuenta en el resumen");
        }

        [Test]
        public void Votos_TopeYFueraDeFase()
        {
            var flow = Started(new QuePrefieresOptions { Modo = QuePrefieresModo.ContarVotos });
            flow.Votar(QuePrefieresOpcion.A, 500);
            Assert.AreEqual(QuePrefieresConfig.MaxVotos, flow.VotosA);
            flow.Revelar();
            flow.Votar(QuePrefieresOpcion.B);
            Assert.AreEqual(0, flow.Ultima.VotosB, "En el resultado ya no se vota");
            flow.Saltar();
            Assert.AreEqual(QuePrefieresFase.Resultado, flow.Fase, "Saltar solo en la pregunta");
        }

        [Test]
        public void Saltar_NoCuentaYSinSegundaOpcionNoSale()
        {
            var cards = new ListSource(new[] { new PromptCard(1, "sin b"), new PromptCard(2, "a", textB: "b") });
            var flow = Started(new QuePrefieresOptions { Preguntas = 1 }, cards);
            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(2, flow.Current.Id, "Las que no tienen segunda opción no se juegan");
                flow.Saltar();
            }
            Assert.AreEqual(1, flow.Numero);
            flow.Revelar();
            Assert.AreEqual(QuePrefieresFase.Final, flow.Fase);
            var none = Started(null, new ListSource(new[] { new PromptCard(1, "sin b") }));
            Assert.AreEqual(QuePrefieresFase.Error, none.Fase);
        }
    }
}
