using System;
using System.Linq;
using Gamespot.Core;
using Gamespot.Games.Impostor.Domain;
using NUnit.Framework;
using static Gamespot.Tests.EditMode.Games.Impostor.ImpostorTestData;

namespace Gamespot.Tests.EditMode.Games.Impostor
{
    public sealed class ImpostorFlowControllerTests
    {
        private static readonly Word[] OneWord = { new Word("playa", "arena") };

        /// <summary>
        /// Controller con un guion que deja como impostores a los <paramref name="impostores"/> primeros
        /// jugadores (barajado sin cambios: cada NextInt(i+1) devuelve i).
        /// </summary>
        private static ImpostorFlowController Started(int players, int impostores, double roll = 0.5)
        {
            var script = new[] { 0.0, roll }.Concat(Enumerable.Repeat(0.999, players)).ToArray();
            var flow = new ImpostorFlowController(new FakeWordSource(OneWord), new ScriptedRandom(script));
            flow.Iniciar(Config(Players(players), impostores));
            return flow;
        }

        private static void RevealAll(ImpostorFlowController flow)
        {
            for (var i = 0; i < flow.TotalPlayers; i++)
            {
                flow.Revelar();
                flow.Avanzar();
            }
        }

        [Test]
        public void EstadoInicial_SetupSinPartida()
        {
            var flow = new ImpostorFlowController(new FakeWordSource(OneWord), new SeededRandom(1));
            Assert.AreEqual(ImpostorPhase.Setup, flow.Phase);
            Assert.IsNull(flow.Session);
            Assert.IsNull(flow.JugadorActual);
        }

        [Test]
        public void Iniciar_ReparteYPasaAlPrimerJugador()
        {
            var flow = Started(4, 1);
            Assert.AreEqual(ImpostorPhase.Pass, flow.Phase);
            Assert.AreEqual(0, flow.CurrentIndex);
            Assert.AreEqual("J1", flow.JugadorActual.Name);
            Assert.AreEqual(4, flow.TotalPlayers);
        }

        [Test]
        public void Iniciar_SinPalabras_ErrorSinPalabras()
        {
            var flow = new ImpostorFlowController(new FakeWordSource(Array.Empty<Word>()), new SeededRandom(1));
            flow.Iniciar(Config(Players(3), 1));
            Assert.AreEqual(ImpostorPhase.Error, flow.Phase);
            Assert.AreEqual(ImpostorErrorKind.SinPalabras, flow.ErrorKind);
            Assert.IsNull(flow.Session);
        }

        [Test]
        public void Iniciar_ErrorInesperado_ErrorDesconocido()
        {
            var flow = new ImpostorFlowController(new FakeWordSource(OneWord, throws: true), new SeededRandom(1));
            flow.Iniciar(Config(Players(3), 1));
            Assert.AreEqual(ImpostorPhase.Error, flow.Phase);
            Assert.AreEqual(ImpostorErrorKind.Desconocido, flow.ErrorKind);
        }

        [Test]
        public void RevelarYAvanzar_RecorrenElOrdenYEmpiezanLaVotacion()
        {
            var flow = Started(5, 1);
            for (var i = 0; i < 4; i++)
            {
                flow.Revelar();
                Assert.AreEqual(ImpostorPhase.Reveal, flow.Phase);
                Assert.AreEqual($"J{i + 1}", flow.JugadorActual.Name);
                Assert.IsFalse(flow.Avanzar());
                Assert.AreEqual(ImpostorPhase.Pass, flow.Phase);
            }
            flow.Revelar();
            Assert.IsTrue(flow.EsUltimoJugador);
            Assert.IsTrue(flow.Avanzar());
            Assert.AreEqual(ImpostorPhase.Voting, flow.Phase);
            Assert.AreEqual(1, flow.RondaActual);
            Assert.AreEqual(2, flow.RondasTotales, "5 jugadores → máx. 2 rondas");
            Assert.AreEqual(LastVote.Ninguno, flow.LastVote);
        }

        [Test]
        public void SinPartida_RevelarYAvanzarNoHacenNada()
        {
            var flow = new ImpostorFlowController(new FakeWordSource(OneWord), new SeededRandom(1));
            flow.Revelar();
            Assert.AreEqual(ImpostorPhase.Setup, flow.Phase);
            Assert.IsFalse(flow.Avanzar());
        }

        [Test]
        public void Votar_PillarAlUnicoImpostor_GananLosJugadores()
        {
            var flow = Started(5, 1);
            RevealAll(flow);
            flow.Votar(new Player("J1"));
            Assert.AreEqual(ImpostorPhase.GameOver, flow.Phase);
            Assert.AreEqual(VotingOutcome.JugadoresGanan, flow.Outcome);
            Assert.AreEqual(LastVote.EraImpostor, flow.LastVote);
        }

        [Test]
        public void Votar_CadaVotoGastaUnaRonda_YAlAcabarGanaElImpostor()
        {
            var flow = Started(5, 1);
            RevealAll(flow);
            flow.Votar(new Player("J2"));
            Assert.AreEqual(ImpostorPhase.Voting, flow.Phase);
            Assert.AreEqual(2, flow.RondaActual);
            Assert.AreEqual(LastVote.NoEraImpostor, flow.LastVote);
            Assert.IsTrue(flow.Candidatos.Contains(new Player("J2")), "Los inocentes votados siguen siendo votables");

            flow.Votar(new Player("J2"));
            Assert.AreEqual(ImpostorPhase.GameOver, flow.Phase);
            Assert.AreEqual(VotingOutcome.ImpostorGana, flow.Outcome);
        }

        [Test]
        public void Votar_HayQuePillarATodosLosImpostores()
        {
            var flow = Started(6, 2);
            RevealAll(flow);
            flow.Votar(new Player("J1"));
            Assert.AreEqual(ImpostorPhase.Voting, flow.Phase, "Queda otro impostor");
            Assert.AreEqual(LastVote.EraImpostor, flow.LastVote);
            Assert.IsFalse(flow.Candidatos.Contains(new Player("J1")));
            CollectionAssert.AreEqual(new[] { new Player("J2") }, flow.ImpostoresVivos);

            flow.Votar(new Player("J2"));
            Assert.AreEqual(VotingOutcome.JugadoresGanan, flow.Outcome);
        }

        [Test]
        public void Votar_AUnEliminado_NoHaceNada()
        {
            var flow = Started(6, 2);
            RevealAll(flow);
            flow.Votar(new Player("J1"));
            var ronda = flow.RondaActual;
            flow.Votar(new Player("J1"));
            Assert.AreEqual(ronda, flow.RondaActual);
            Assert.AreEqual(ImpostorPhase.Voting, flow.Phase);
        }

        [Test]
        public void Votar_SinImpostores_GanaElImpostor()
        {
            var flow = Started(3, 1, roll: 0.15);
            Assert.AreEqual(0, flow.Session.ImpostorCount);
            RevealAll(flow);
            flow.Votar(new Player("J1"));
            Assert.AreEqual(VotingOutcome.ImpostorGana, flow.Outcome);
        }

        [Test]
        public void Votar_FueraDeLaVotacion_NoHaceNada()
        {
            var flow = Started(3, 1);
            flow.Votar(new Player("J1"));
            Assert.AreEqual(ImpostorPhase.Pass, flow.Phase);
            Assert.IsNull(flow.Outcome);
        }

        [Test]
        public void Reiniciar_VuelveAlEstadoInicial()
        {
            var flow = Started(3, 1);
            var changes = 0;
            flow.Changed += () => changes++;
            flow.Reiniciar();
            Assert.AreEqual(ImpostorPhase.Setup, flow.Phase);
            Assert.IsNull(flow.Session);
            Assert.IsNull(flow.Config);
            Assert.AreEqual(1, changes);
        }
    }
}
