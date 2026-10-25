using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Games.YoNunca.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.YoNunca
{
    internal sealed class FakeStatements : INeverStatementSource
    {
        public readonly List<NeverStatement> All = new List<NeverStatement>
        {
            new NeverStatement(1, "Yo nunca A", Intensidad.Suave),
            new NeverStatement(2, "Yo nunca B", Intensidad.Suave),
            new NeverStatement(3, "Yo nunca C", Intensidad.Suave),
            new NeverStatement(4, "Yo nunca P", Intensidad.Picante)
        };

        public IReadOnlyList<NeverStatement> GetByIntensidades(IReadOnlyCollection<Intensidad> intensidades) =>
            All.Where(s => intensidades.Contains(s.Intensidad)).ToList();
    }

    public sealed class YoNuncaDomainTests
    {
        [Test]
        public void Config_AlMenosUnaIntensidad()
        {
            Assert.IsNull(YoNuncaConfig.Create(new Intensidad[0]));
            Assert.AreEqual(2, YoNuncaConfig.Create(new[] { Intensidad.Suave, Intensidad.Picante, Intensidad.Suave }).Intensidades.Count);
        }

        [Test]
        public void Sacar_FiltraPorNivelYNoRepiteHastaAgotar()
        {
            var draw = new DrawStatementUseCase(new SeededRandom(3));
            var pool = new FakeStatements().All;
            var seen = new HashSet<long>();
            var first = Enumerable.Range(0, 3).Select(_ => draw.Execute(pool, new[] { Intensidad.Suave }, seen).Id).ToList();
            CollectionAssert.AreEquivalent(new long[] { 1, 2, 3 }, first, "Las 3 suaves sin repetir");
            Assert.AreEqual(3, seen.Count);
            draw.Execute(pool, new[] { Intensidad.Suave }, seen);
            Assert.AreEqual(1, seen.Count, "Al agotarse se vuelve a empezar");
        }

        [Test]
        public void Sacar_SinFrasesDelNivel_Lanza()
        {
            var draw = new DrawStatementUseCase(new SeededRandom(1));
            var pool = new FakeStatements().All.Where(s => s.Intensidad == Intensidad.Suave).ToList();
            Assert.Throws<ArgumentException>(() => draw.Execute(pool, new[] { Intensidad.Picante }, new HashSet<long>()));
        }

        [Test]
        public void Intensidad_NombresGuardados()
        {
            Assert.AreEqual("picante", Intensidad.Picante.StorageName());
            Assert.AreEqual(Intensidad.Suave, Intensidades.FromStorageName("suave"));
            Assert.Throws<FormatException>(() => Intensidades.FromStorageName("fuerte"));
        }

        [Test]
        public void Flow_SacaYSigue()
        {
            var flow = new YoNuncaFlowController(new FakeStatements(), new SeededRandom(2));
            flow.Siguiente();
            Assert.AreEqual(YoNuncaFase.Setup, flow.Fase);
            flow.Iniciar(YoNuncaConfig.Create(new[] { Intensidad.Picante }));
            Assert.AreEqual(YoNuncaFase.Jugando, flow.Fase);
            Assert.AreEqual(4L, flow.Current.Id);
            flow.Siguiente();
            Assert.AreEqual(4L, flow.Current.Id, "Con una sola frase se repite al agotarse");
        }

        [Test]
        public void Flow_SinFrases_Error()
        {
            var source = new FakeStatements();
            source.All.RemoveAll(s => s.Intensidad == Intensidad.Picante);
            var flow = new YoNuncaFlowController(source, new SeededRandom(2));
            flow.Iniciar(YoNuncaConfig.Create(new[] { Intensidad.Picante }));
            Assert.AreEqual(YoNuncaFase.Error, flow.Fase);
        }
    }
}
