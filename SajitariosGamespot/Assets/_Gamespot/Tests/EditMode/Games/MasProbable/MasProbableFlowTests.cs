using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Games.MasProbable.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.MasProbable
{
    /// <summary>V072: configuración, texto de las frases y partida de ¿Quién es más probable que…?</summary>
    public sealed class MasProbableFlowTests
    {
        private sealed class ListSource : IPromptSource
        {
            private readonly List<PromptCard> _cards;

            public ListSource(IEnumerable<PromptCard> cards) => _cards = cards.ToList();

            public IReadOnlyList<PromptCard> GetCards(PromptQuery query) => _cards.Where(query.Matches).ToList();
        }

        private static ListSource Source(int n = 4) =>
            new ListSource(Enumerable.Range(1, n).Select(i => new PromptCard(i, $"frase {i}", i % 2 == 0 ? PromptLevel.Picante : PromptLevel.Suave)));

        private static MasProbableFlowController Started(MasProbableOptions options, params string[] names)
        {
            var (config, error) = MasProbableConfig.Create(PromptLevels.All, names, options);
            Assert.IsNull(error);
            var flow = new MasProbableFlowController(Source(), new SeededRandom(5));
            flow.Iniciar(config);
            return flow;
        }

        [Test]
        public void Texto_QuitaLaPreguntaSiSeEscribeEntera()
        {
            Assert.AreEqual("acabe en comisaría", MasProbableText.Normalize("¿Quién es más probable que acabe en comisaría?"));
            Assert.AreEqual("se case en Las Vegas", MasProbableText.Normalize("quien es mas probable que se case en Las Vegas"));
            Assert.AreEqual("llore en su boda", MasProbableText.Normalize("  llore   en su boda?  "));
            Assert.AreEqual("haga eso", MasProbableText.Normalize("¿Quién es más probable que… haga eso?"));
            Assert.AreEqual(MasProbableText.MaxLength, MasProbableText.Normalize(new string('a', 200)).Length);
            Assert.AreEqual(string.Empty, MasProbableText.Normalize("¿Quién es más probable que?"));
        }

        [Test]
        public void Config_SinCuentaNoPideNombres()
        {
            var (config, error) = MasProbableConfig.Create(new[] { PromptLevel.Suave }, null);
            Assert.IsNull(error);
            Assert.AreEqual(0, config.Jugadores.Count);
            Assert.AreEqual(MasProbableConfigError.SinNiveles, MasProbableConfig.Create(new PromptLevel[0], null).error);
            var counting = new MasProbableOptions { Modo = MasProbableModo.ConCuenta };
            Assert.AreEqual(MasProbableConfigError.PocosJugadores, MasProbableConfig.Create(new[] { PromptLevel.Suave }, new[] { "A", "B" }, counting).error);
            Assert.AreEqual(MasProbableConfigError.NombresRepetidos,
                MasProbableConfig.Create(new[] { PromptLevel.Suave }, new[] { "Ana", "Bea", "ana" }, counting).error);
            Assert.AreEqual(MasProbableConfigError.NombreVacio, MasProbableConfig.Create(new[] { PromptLevel.Suave }, new[] { "Ana", "Bea", " " }, counting).error);
        }

        [Test]
        public void SinCuenta_FrasesSinRepetirHastaElLimite()
        {
            var flow = Started(new MasProbableOptions { Frases = 4 });
            var seen = new List<long>();
            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(MasProbableFase.Frase, flow.Fase);
                Assert.AreEqual(i + 1, flow.Numero);
                seen.Add(flow.Current.Id);
                flow.Senalado();
            }
            Assert.AreEqual(MasProbableFase.Final, flow.Fase);
            Assert.AreEqual(4, seen.Distinct().Count(), "Sin repetir hasta agotar");
            Assert.AreEqual(4, flow.Rondas.Count);
        }

        [Test]
        public void Saltar_NoCuentaParaElLimite()
        {
            var flow = Started(new MasProbableOptions { Frases = 2 });
            flow.Saltar();
            Assert.AreEqual(1, flow.Numero);
            flow.Senalado();
            flow.Saltar();
            flow.Saltar();
            Assert.AreEqual(2, flow.Numero);
            Assert.AreEqual(MasProbableFase.Frase, flow.Fase);
            flow.Senalado();
            Assert.AreEqual(MasProbableFase.Final, flow.Fase);
            Assert.AreEqual(2, flow.Rondas.Count);
        }

        [Test]
        public void ConCuenta_ApuntaTitulosConEmpatesYRanking()
        {
            var flow = Started(new MasProbableOptions { Modo = MasProbableModo.ConCuenta }, "Ana", "Bea", "Carlos");
            flow.Senalado();
            Assert.AreEqual(MasProbableFase.Apuntar, flow.Fase);
            flow.Apuntar(new[] { 1 });
            Assert.AreEqual(MasProbableFase.Frase, flow.Fase);
            flow.Senalado();
            flow.Apuntar(new[] { 2, 1, 7, -1 });
            flow.Senalado();
            flow.Apuntar(null);
            CollectionAssert.AreEqual(new[] { 0, 2, 1 }, flow.Titulos);
            CollectionAssert.AreEqual(new[] { 1, 2, 0 }, flow.Ranking.Select(r => r.index));
            CollectionAssert.AreEqual(new[] { 1, 2 }, flow.Rondas[1].Winners, "Los empatados, todos; los que no existen, fuera");
            CollectionAssert.IsEmpty(flow.Rondas[2].Winners);
            flow.Terminar();
            Assert.AreEqual(MasProbableFase.Final, flow.Fase);
        }

        [Test]
        public void AccionesFueraDeSuFase_NoHacenNada()
        {
            var flow = Started(new MasProbableOptions { Modo = MasProbableModo.ConCuenta }, "Ana", "Bea", "Carlos");
            flow.Apuntar(new[] { 0 });
            Assert.AreEqual(0, flow.Titulos[0], "Sin señalar no se apunta");
            flow.Senalado();
            flow.Senalado();
            flow.Saltar();
            Assert.AreEqual(MasProbableFase.Apuntar, flow.Fase);
            flow.Reiniciar();
            Assert.AreEqual(MasProbableFase.Setup, flow.Fase);
            Assert.IsNull(flow.Config);
        }

        [Test]
        public void Niveles_SoloSalenLosElegidosYSinCartasError()
        {
            var (config, _) = MasProbableConfig.Create(new[] { PromptLevel.Picante }, null);
            var flow = new MasProbableFlowController(Source(6), new SeededRandom(2));
            flow.Iniciar(config);
            for (var i = 0; i < 9; i++)
            {
                Assert.AreEqual(PromptLevel.Picante, flow.Current.Level);
                flow.Senalado();
            }
            var empty = new MasProbableFlowController(new ListSource(new PromptCard[0]), new SeededRandom(2));
            empty.Iniciar(config);
            Assert.AreEqual(MasProbableFase.Error, empty.Fase);
        }

        [Test]
        public void UnaLlamadaAlRandomPorFrase()
        {
            var random = new ScriptedRandom(0.0, 0.0, 0.99);
            var (config, _) = MasProbableConfig.Create(PromptLevels.All, null);
            var flow = new MasProbableFlowController(Source(), random);
            flow.Iniciar(config);
            Assert.AreEqual(1, flow.Current.Id);
            flow.Senalado();
            Assert.AreEqual(2, flow.Current.Id);
            flow.Senalado();
            Assert.AreEqual(4, flow.Current.Id);
            Assert.AreEqual(3, random.Consumed);
        }
    }
}
