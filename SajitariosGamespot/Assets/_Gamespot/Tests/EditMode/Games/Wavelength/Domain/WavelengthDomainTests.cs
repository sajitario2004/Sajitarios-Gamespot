using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Games.Wavelength.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Wavelength
{
    internal sealed class FakeSpectrumSource : ISpectrumSource
    {
        public IReadOnlyList<Spectrum> Pool = new[] { new Spectrum(1, "frío", "caliente"), new Spectrum(2, "barato", "caro") };
        public bool FailOnNext;
        private int _calls;

        public IReadOnlyList<Spectrum> GetAll()
        {
            _calls++;
            if (FailOnNext && _calls > 1) throw new InvalidOperationException("BD caída");
            return Pool;
        }
    }

    public sealed class WavelengthDomainTests
    {
        [TestCase(0.5, 0.5, 4)]
        [TestCase(0.55, 0.5, 4)]
        [TestCase(0.60, 0.5, 3)]
        [TestCase(0.68, 0.5, 2)]
        [TestCase(0.71, 0.5, 0)]
        [TestCase(-0.5, 0.0, 4)]
        [TestCase(1.5, 1.0, 4)]
        public void Puntos_BordesInclusivosYRecorte(double guess, double target, int expected)
        {
            Assert.AreEqual(expected, WavelengthScoring.ScoreFor(guess, target));
        }

        [Test]
        public void Puntos_BordesExactosComoElFlutter()
        {
            // Mismas operaciones que los tests del Flutter: el borde exacto puntúa la banda mejor.
            Assert.AreEqual(4, WavelengthScoring.ScoreFor(0.5 + 0.06, 0.5));
            Assert.AreEqual(3, WavelengthScoring.ScoreFor(0.5 + 0.12, 0.5));
            Assert.AreEqual(2, WavelengthScoring.ScoreFor(0.5 + 0.20, 0.5));
        }

        [Test]
        public void Ronda_ValidaObjetivoPistaYRespuesta()
        {
            var spectrum = new Spectrum(1, "frío", "caliente");
            Assert.Throws<ArgumentOutOfRangeException>(() => WavelengthRound.Start(spectrum, 1.01));
            Assert.DoesNotThrow(() => WavelengthRound.Start(spectrum, 1.0));
            var round = WavelengthRound.Start(spectrum, 0.3);
            Assert.IsNull(round.Score);
            Assert.Throws<ArgumentException>(() => round.WithClue("  "));
            round = round.WithClue(" playa ").WithGuess(2.0);
            Assert.AreEqual("playa", round.Clue);
            Assert.AreEqual(1.0, round.Guess);
            Assert.AreEqual(0, round.Score);
        }

        [Test]
        public void Config_Limites()
        {
            Assert.AreEqual(WavelengthConfigError.PocosJugadores, WavelengthConfig.Create(new[] { "Ana" }).error);
            Assert.AreEqual(WavelengthConfigError.DemasiadosJugadores, WavelengthConfig.Create(Enumerable.Range(0, 9).Select(i => $"J{i}").ToList()).error);
            Assert.AreEqual(WavelengthConfigError.NombresDuplicados, WavelengthConfig.Create(new[] { "Ana", "ana" }).error);
            Assert.AreEqual(WavelengthConfigError.RondasFueraDeRango, WavelengthConfig.Create(new[] { "Ana", "Bea" }, 0).error);
            Assert.AreEqual(WavelengthConfigError.RondasFueraDeRango, WavelengthConfig.Create(new[] { "Ana", "Bea" }, 21).error);
            Assert.AreEqual(8, WavelengthConfig.Create(new[] { "Ana", "Bea" }).config.Rounds);
            Assert.IsNull(WavelengthConfig.Create(Enumerable.Range(0, 8).Select(i => $"J{i}").ToList(), 20).error);
        }

        [Test]
        public void Sesion_RotaElPsiquicoYAcumula()
        {
            var session = WavelengthSession.Start(new[] { "Ana", "Bob", "Carlos" }, 4);
            var spectrum = new Spectrum(1, "a", "b");
            var psychics = new List<string>();
            var scores = new List<int>();
            while (!session.IsOver)
            {
                psychics.Add(session.CurrentPsychic);
                session = session.RecordRound(WavelengthRound.Start(spectrum, 0.5).WithGuess(session.CurrentRoundIndex == 1 ? 0.6 : 0.5));
                scores.Add(session.CumulativeScore);
            }
            CollectionAssert.AreEqual(new[] { "Ana", "Bob", "Carlos", "Ana" }, psychics);
            CollectionAssert.AreEqual(new[] { 4, 7, 11, 15 }, scores);
            Assert.IsNull(session.CurrentPsychic);
            Assert.Throws<InvalidOperationException>(() => session.RecordRound(WavelengthRound.Start(spectrum, 0.5).WithGuess(0.5)));
        }

        [Test]
        public void ElegirRonda_EspectroYLuegoObjetivo()
        {
            var source = new FakeSpectrumSource();
            var round = new PickRoundUseCase(new ScriptedRandom(0.99, 0.25)).Execute(source.Pool);
            Assert.AreEqual(source.Pool[1], round.Spectrum);
            Assert.AreEqual(0.25, round.Target);
        }

        [Test]
        public void Flow_PartidaCompletaYNoOpsFueraDeFase()
        {
            var flow = new WavelengthFlowController(new FakeSpectrumSource(), new SeededRandom(5));
            flow.SubmitGuess(0.5);
            flow.Next();
            Assert.AreEqual(WavelengthFase.Setup, flow.Fase);

            flow.Iniciar(WavelengthConfig.Create(new[] { "Ana", "Luis" }, 2).config);
            Assert.AreEqual(WavelengthFase.Clue, flow.Fase);
            Assert.AreEqual("Ana", flow.CurrentPsychic);
            flow.PasarDispositivo();
            Assert.AreEqual(WavelengthFase.Clue, flow.Fase, "Pasar en Clue no hace nada");
            flow.ConfirmarPista("   ");
            Assert.AreEqual(WavelengthFase.Clue, flow.Fase, "Pista vacía no avanza");
            flow.ConfirmarPista("café");
            flow.PasarDispositivo();
            flow.SubmitGuess(flow.CurrentRound.Target);
            Assert.AreEqual(WavelengthFase.Reveal, flow.Fase);
            Assert.AreEqual(0, flow.Session.CumulativeScore, "Aún no suma");
            Assert.AreEqual(4, flow.ScoreIncludingCurrent, "El revelado enseña el total con esta ronda");
            flow.Next();
            Assert.AreEqual(WavelengthFase.Clue, flow.Fase);
            Assert.AreEqual("Luis", flow.CurrentPsychic);
            Assert.AreEqual(4, flow.Session.CumulativeScore);

            flow.ConfirmarPista("hielo");
            flow.PasarDispositivo();
            flow.SubmitGuess(flow.CurrentRound.Target);
            flow.Next();
            Assert.AreEqual(WavelengthFase.GameOver, flow.Fase);
            Assert.AreEqual(8, flow.Session.CumulativeScore);
        }

        [Test]
        public void Flow_SinEspectros_Error()
        {
            var flow = new WavelengthFlowController(new FakeSpectrumSource { Pool = new Spectrum[0] }, new SeededRandom(1));
            flow.Iniciar(WavelengthConfig.Create(new[] { "Ana", "Bea" }).config);
            Assert.AreEqual(WavelengthFase.Error, flow.Fase);
            Assert.AreEqual(WavelengthErrorKind.SinEspectros, flow.ErrorKind);
        }

        [Test]
        public void Flow_FalloAlCargarLaSiguienteRonda_TerminaLaPartida()
        {
            var flow = new WavelengthFlowController(new FakeSpectrumSource { FailOnNext = true }, new SeededRandom(1));
            flow.Iniciar(WavelengthConfig.Create(new[] { "Ana", "Bea" }, 3).config);
            flow.ConfirmarPista("x");
            flow.PasarDispositivo();
            flow.SubmitGuess(0.5);
            flow.Next();
            Assert.AreEqual(WavelengthFase.GameOver, flow.Fase);
            Assert.AreEqual(1, flow.Session.CompletedRounds.Count);
        }
    }
}
