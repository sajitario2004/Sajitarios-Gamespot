using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Games.Tabu.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Tabu
{
    internal sealed class FakeTabuSource : ITabuWordSource
    {
        public IReadOnlyList<TabuWord> Pool;

        public FakeTabuSource(int count) => Pool = Enumerable.Range(1, count).Select(Word).ToList();

        public static TabuWord Word(int i) => new TabuWord(i, $"palabra{i}", new[] { "a", "b", "c", "d" });

        public IReadOnlyList<TabuWord> GetAll() => Pool;
    }

    public sealed class TabuDomainTests
    {
        private static TabuConfig Config(int target = 3) => TabuConfig.Create("Rojos", "Azules", 60, target).config;

        [Test]
        public void Palabra_De4A5Prohibidas()
        {
            Assert.DoesNotThrow(() => new TabuWord(1, "sol", new[] { "a", "b", "c", "d" }));
            Assert.DoesNotThrow(() => new TabuWord(1, "sol", new[] { "a", "b", "c", "d", "e" }));
            Assert.Throws<ArgumentException>(() => new TabuWord(1, "sol", new[] { "a", "b", "c" }));
            Assert.Throws<ArgumentException>(() => new TabuWord(1, "sol", new[] { "a", "b", "c", "d", "e", "f" }));
            Assert.Throws<ArgumentException>(() => new TabuWord(1, " ", new[] { "a", "b", "c", "d" }));
            Assert.Throws<ArgumentException>(() => new TabuWord(1, "sol", new[] { "a", "b", " ", "d" }));
        }

        [Test]
        public void Config_Limites()
        {
            Assert.AreEqual(TabuConfigError.EquipoAVacio, TabuConfig.Create(" ", "B").error);
            Assert.AreEqual(TabuConfigError.EquipoBVacio, TabuConfig.Create("A", "").error);
            Assert.AreEqual(TabuConfigError.EquiposDuplicados, TabuConfig.Create("Rojos", " rojos ").error);
            Assert.AreEqual(TabuConfigError.TurnoSegundosInvalido, TabuConfig.Create("A", "B", 29).error);
            Assert.AreEqual(TabuConfigError.TurnoSegundosInvalido, TabuConfig.Create("A", "B", 121).error);
            Assert.IsNull(TabuConfig.Create("A", "B", 30).error);
            Assert.IsNull(TabuConfig.Create("A", "B", 120).error);
            Assert.AreEqual(TabuConfigError.ObjetivoVictoriasInvalido, TabuConfig.Create("A", "B", 60, 0).error);
            Assert.AreEqual(3, TabuConfig.Create("A", "B").config.ObjetivoVictorias);
        }

        [Test]
        public void Sesion_FaltaNoBajaDeCeroYGanaSoloConAciertos()
        {
            var s = TabuSession.Start(Config(), FakeTabuSource.Word(1)).RegistrarFalta();
            Assert.AreEqual(0, s.AciertosTurno);
            s = s.TerminarTurno(FakeTabuSource.Word(2));
            Assert.AreEqual(0, s.VictoriasA, "Sin aciertos no hay victoria");
            Assert.AreEqual(TabuEquipo.B, s.EquipoActual);
            s = s.RegistrarAcierto().RegistrarAcierto().RegistrarFalta().TerminarTurno(FakeTabuSource.Word(3));
            Assert.AreEqual(1, s.VictoriasB);
            Assert.AreEqual(TabuEquipo.A, s.EquipoActual);
        }

        [Test]
        public void Sesion_NoRepitePalabrasYTerminaAlObjetivo()
        {
            var s = TabuSession.Start(Config(1), FakeTabuSource.Word(1));
            Assert.Throws<ArgumentException>(() => s.AvanzarPalabra(FakeTabuSource.Word(1)));
            s = s.RegistrarAcierto().TerminarTurno(FakeTabuSource.Word(2));
            Assert.IsTrue(s.IsOver);
            Assert.AreEqual(TabuEquipo.A, s.Ganador);
            Assert.Throws<InvalidOperationException>(() => s.RegistrarAcierto());
        }

        [Test]
        public void Elegir_SinRepetir()
        {
            var pool = new FakeTabuSource(3).Pool;
            var pick = new PickTabuWordUseCase(new SeededRandom(2));
            Assert.AreEqual(3L, pick.Execute(pool, new long[] { 1, 2 }).Id);
            Assert.Throws<ArgumentException>(() => pick.Execute(pool, new long[] { 1, 2, 3 }));
            Assert.Throws<ArgumentException>(() => pick.Execute(new TabuWord[0], new long[0]));
        }

        [Test]
        public void Flow_EquipoAGanaTrasTresTurnosConAciertos()
        {
            var flow = new TabuFlowController(new FakeTabuSource(50), new SeededRandom(1));
            flow.Iniciar(Config());
            var seen = new HashSet<long> { flow.Session.PalabraActual.Id };
            for (var turn = 0; turn < 5; turn++)
            {
                Assert.AreEqual(TabuFase.Turno, flow.Fase);
                if (flow.Session.EquipoActual == TabuEquipo.A)
                {
                    flow.Acierto();
                    Assert.IsTrue(seen.Add(flow.Session.PalabraActual.Id), "Palabra nueva tras acertar");
                }
                else
                {
                    flow.Saltar();
                    flow.Falta();
                }
                flow.TerminarTurno();
                if (flow.Fase == TabuFase.GameOver) break;
                Assert.AreEqual(TabuFase.FinRonda, flow.Fase);
                flow.SiguienteTurno();
            }
            Assert.AreEqual(TabuFase.GameOver, flow.Fase);
            Assert.AreEqual(TabuEquipo.A, flow.Session.Ganador);
            Assert.AreEqual(3, flow.Session.VictoriasA);
            Assert.AreEqual(0, flow.Session.VictoriasB);
            flow.Acierto();
            Assert.AreEqual(TabuFase.GameOver, flow.Fase, "Acciones ignoradas al terminar");
        }

        [Test]
        public void Flow_SeAcabanLasPalabras_SeQuedaLaActual()
        {
            var flow = new TabuFlowController(new FakeTabuSource(1), new SeededRandom(1));
            flow.Iniciar(Config());
            flow.Acierto();
            flow.Acierto();
            Assert.AreEqual(2, flow.Session.AciertosTurno);
            Assert.AreEqual(1L, flow.Session.PalabraActual.Id);
        }

        [Test]
        public void Flow_SinPalabras_Error()
        {
            var flow = new TabuFlowController(new FakeTabuSource(0), new SeededRandom(1));
            flow.Iniciar(Config());
            Assert.AreEqual(TabuFase.Error, flow.Fase);
            Assert.AreEqual(TabuErrorKind.SinPalabras, flow.ErrorKind);
        }

        [Test]
        public void Flow_FueraDeFaseNoHaceNada()
        {
            var flow = new TabuFlowController(new FakeTabuSource(5), new SeededRandom(1));
            flow.Acierto();
            flow.TerminarTurno();
            flow.SiguienteTurno();
            Assert.AreEqual(TabuFase.Setup, flow.Fase);
            flow.Iniciar(Config());
            flow.SiguienteTurno();
            Assert.AreEqual(TabuFase.Turno, flow.Fase);
        }
    }
}
