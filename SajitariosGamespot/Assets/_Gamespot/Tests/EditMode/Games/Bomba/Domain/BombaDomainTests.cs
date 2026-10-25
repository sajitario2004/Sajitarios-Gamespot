using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Games.Bomba.Domain;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Bomba
{
    internal sealed class FakePrompts : IBombaPromptSource
    {
        public List<BombaPrompt> Silabas = Enumerable.Range(1, 5).Select(i => new BombaPrompt(i, "S" + i, BombaMode.Silaba)).ToList();
        public List<BombaPrompt> Categorias = Enumerable.Range(1, 3).Select(i => new BombaPrompt(i, "C" + i, BombaMode.Categoria)).ToList();

        public IReadOnlyList<BombaPrompt> GetAllSilabas() => Silabas;
        public IReadOnlyList<BombaPrompt> GetAllCategorias() => Categorias;
    }

    public sealed class BombaDomainTests
    {
        private static readonly string[] Three = { "Ana", "Bob", "Carlos" };

        private static BombaConfig Config(BombaMode mode = BombaMode.Silaba, IReadOnlyList<string> names = null, int min = 10, int max = 60) =>
            BombaConfig.Create(mode, names ?? Three, min, max).config;

        [Test]
        public void Prompt_TextoRecortadoYNoVacio()
        {
            Assert.AreEqual("CA", new BombaPrompt(1, "  CA ", BombaMode.Silaba).Texto);
            Assert.Throws<ArgumentException>(() => new BombaPrompt(1, "   ", BombaMode.Silaba));
        }

        [Test]
        public void Config_Errores()
        {
            Assert.AreEqual(BombaConfigError.PocosJugadores, BombaConfig.Create(BombaMode.Silaba, new[] { "Ana" }).error);
            Assert.AreEqual(BombaConfigError.DemasiadosJugadores,
                BombaConfig.Create(BombaMode.Silaba, Enumerable.Range(0, 13).Select(i => "J" + i).ToList()).error);
            Assert.AreEqual(BombaConfigError.NombreVacio, BombaConfig.Create(BombaMode.Silaba, new[] { "Ana", "  " }).error);
            Assert.AreEqual(BombaConfigError.NombresDuplicados, BombaConfig.Create(BombaMode.Silaba, new[] { "Ana", "ana " }).error);
            Assert.AreEqual(BombaConfigError.MinSegundosFueraDeLimite, BombaConfig.Create(BombaMode.Silaba, Three, 9, 60).error);
            Assert.AreEqual(BombaConfigError.MaxSegundosFueraDeLimite, BombaConfig.Create(BombaMode.Silaba, Three, 10, 61).error);
            Assert.AreEqual(BombaConfigError.RangoSegundosInvalido, BombaConfig.Create(BombaMode.Silaba, Three, 30, 30).error);
        }

        [Test]
        public void Config_ValidaRecortaNombresYGuardaElModo()
        {
            var (config, error) = BombaConfig.Create(BombaMode.Categoria, new[] { " Ana ", "Bob" });
            Assert.IsNull(error);
            Assert.AreEqual(BombaMode.Categoria, config.Mode);
            CollectionAssert.AreEqual(new[] { "Ana", "Bob" }, config.PlayerNames);
            Assert.AreEqual(10, config.MinSegundos);
            Assert.AreEqual(60, config.MaxSegundos);
            Assert.IsNotNull(BombaConfig.Create(BombaMode.Silaba, Enumerable.Range(0, 12).Select(i => "J" + i).ToList()).config);
        }

        [Test]
        public void Mecha_DentroDelRangoYDeterminista()
        {
            var config = Config(min: 20, max: 50);
            var rng = new SeededRandom(7);
            for (var i = 0; i < 1000; i++)
            {
                var fuse = BombaSession.PickFuseSeconds(rng, config);
                Assert.That(fuse, Is.GreaterThanOrEqualTo(20d).And.LessThan(50d));
            }
            Assert.AreEqual(BombaSession.PickFuseSeconds(new SeededRandom(3), config), BombaSession.PickFuseSeconds(new SeededRandom(3), config));
            Assert.AreEqual(10d + 0.5 * 50d, BombaSession.PickFuseSeconds(new ScriptedRandom(0.5), Config()), 1e-9);
        }

        [Test]
        public void Sesion_EmpiezaEnElPrimerJugadorConTodosVivos()
        {
            var session = BombaSession.Start(Config(), new ScriptedRandom(0.25));
            Assert.AreEqual("Ana", session.CurrentHolder);
            CollectionAssert.AreEqual(Three, session.AlivePlayers);
            Assert.IsFalse(session.IsOver);
            Assert.IsNull(session.Winner);
            Assert.AreEqual(22.5d, session.FuseSeconds, 1e-9);
        }

        [Test]
        public void Pasar_RotaConVueltaYNoTocaLaMecha()
        {
            var session = BombaSession.Start(Config(), new ScriptedRandom(0.1));
            var fuse = session.FuseSeconds;
            session = session.Pasar();
            Assert.AreEqual("Bob", session.CurrentHolder);
            session = session.Pasar().Pasar();
            Assert.AreEqual("Ana", session.CurrentHolder, "Vuelta al primero");
            Assert.AreEqual(fuse, session.FuseSeconds);
            CollectionAssert.AreEqual(Three, session.AlivePlayers);
        }

        [Test]
        public void Explotar_EliminaAlPortadorYLaBombaPasaAlSiguiente()
        {
            var session = BombaSession.Start(Config(), new ScriptedRandom(0.1)).Pasar();
            session = session.Explode();
            CollectionAssert.AreEqual(new[] { "Ana", "Carlos" }, session.AlivePlayers);
            Assert.AreEqual("Carlos", session.CurrentHolder, "La tiene el que iba detrás de Bob");

            var last = BombaSession.Start(Config(), new ScriptedRandom(0.1)).Pasar().Pasar();
            Assert.AreEqual("Carlos", last.CurrentHolder);
            last = last.Explode();
            CollectionAssert.AreEqual(new[] { "Ana", "Bob" }, last.AlivePlayers);
            Assert.AreEqual("Ana", last.CurrentHolder, "Si explota el último, vuelve al primero");
        }

        [Test]
        public void Explotar_HastaQueQuedaUno_Gana()
        {
            var session = BombaSession.Start(Config(), new ScriptedRandom(0.1)).Explode();
            Assert.IsFalse(session.IsOver);
            session = session.Explode();
            Assert.IsTrue(session.IsOver);
            Assert.AreEqual("Carlos", session.Winner);
            Assert.Throws<InvalidOperationException>(() => session.Pasar());
            Assert.Throws<InvalidOperationException>(() => session.Explode());
            CollectionAssert.AreEqual(Three, session.OrderedPlayers, "El orden original se conserva");
        }

        [Test]
        public void RondaNueva_MismoPortadorYMechaNueva()
        {
            var config = Config();
            var rng = new ScriptedRandom(0.0, 0.9);
            var session = BombaSession.Start(config, rng).Pasar().Explode();
            var round = BombaSession.NewRound(session, config, rng);
            Assert.AreEqual(session.CurrentHolder, round.CurrentHolder);
            CollectionAssert.AreEqual(session.AlivePlayers, round.AlivePlayers);
            Assert.AreEqual(10d, session.FuseSeconds, 1e-9);
            Assert.AreEqual(55d, round.FuseSeconds, 1e-9);
        }

        [Test]
        public void ElegirPrompt_SinRepetirHastaAgotarYLuegoReinicia()
        {
            var pick = new PickPromptUseCase(new SeededRandom(11));
            var pool = new FakePrompts().Silabas;
            var first = Enumerable.Range(0, pool.Count).Select(_ => pick.Pick(BombaMode.Silaba, pool).Id).ToList();
            CollectionAssert.AreEquivalent(pool.Select(p => p.Id), first, "Las 5 antes de repetir");
            Assert.That(pool.Select(p => p.Id), Has.Member(pick.Pick(BombaMode.Silaba, pool).Id), "La sexta vuelve a empezar");
        }

        [Test]
        public void ElegirPrompt_CadaModoLlevaSuCuenta()
        {
            var pick = new PickPromptUseCase(new SeededRandom(5));
            var fake = new FakePrompts();
            var pool = fake.Silabas.Concat(fake.Categorias).ToList();
            var silabas = new HashSet<long>();
            var categorias = new HashSet<long>();
            for (var i = 0; i < 3; i++)
            {
                var s = pick.Pick(BombaMode.Silaba, pool);
                Assert.AreEqual(BombaMode.Silaba, s.Mode);
                Assert.IsTrue(silabas.Add(s.Id));
                var c = pick.Pick(BombaMode.Categoria, pool);
                Assert.AreEqual(BombaMode.Categoria, c.Mode);
                Assert.IsTrue(categorias.Add(c.Id), "Los ids se repiten entre modos pero no se mezclan");
            }
        }

        [Test]
        public void ElegirPrompt_SinPromptsDelModo_LanzaYResetVuelveAEmpezar()
        {
            var pick = new PickPromptUseCase(new ScriptedRandom(true, 0.0));
            var fake = new FakePrompts();
            Assert.Throws<ArgumentException>(() => pick.Pick(BombaMode.Categoria, fake.Silabas));
            Assert.Throws<ArgumentException>(() => pick.Pick(BombaMode.Silaba, new BombaPrompt[0]));
            Assert.AreEqual(1, pick.Pick(BombaMode.Silaba, fake.Silabas).Id);
            Assert.AreEqual(2, pick.Pick(BombaMode.Silaba, fake.Silabas).Id, "La 1 ya salió");
            pick.ResetMode(BombaMode.Silaba);
            Assert.AreEqual(1, pick.Pick(BombaMode.Silaba, fake.Silabas).Id);
            pick.ResetAll();
            Assert.AreEqual(1, pick.Pick(BombaMode.Silaba, fake.Silabas).Id);
        }

        [Test]
        public void Flujo_PartidaCompleta()
        {
            var flow = new BombaFlowController(new FakePrompts(), new SeededRandom(2));
            var changes = 0;
            flow.Changed += () => changes++;
            flow.Pasar();
            flow.Explotar();
            flow.ContinuarTrasExplosion();
            Assert.AreEqual(BombaFase.Setup, flow.Fase, "Fuera de fase no hace nada");
            Assert.AreEqual(0, changes);

            flow.Iniciar(Config());
            Assert.AreEqual(BombaFase.Jugando, flow.Fase);
            Assert.AreEqual(BombaMode.Silaba, flow.CurrentPrompt.Mode);
            Assert.AreEqual("Ana", flow.CurrentHolder);
            Assert.That(flow.FuseSeconds, Is.GreaterThanOrEqualTo(10d).And.LessThan(60d));
            var fuse = flow.FuseSeconds;

            flow.Pasar();
            Assert.AreEqual("Bob", flow.CurrentHolder);
            Assert.AreEqual(fuse, flow.FuseSeconds, "Pasar no reinicia la mecha");

            var prompt = flow.CurrentPrompt;
            flow.Explotar();
            Assert.AreEqual(BombaFase.Explotando, flow.Fase);
            Assert.AreEqual("Bob", flow.Eliminado);
            CollectionAssert.AreEqual(new[] { "Ana", "Carlos" }, flow.AlivePlayers);
            flow.Pasar();
            Assert.AreEqual("Carlos", flow.CurrentHolder, "Durante la explosión pasar no hace nada");

            flow.ContinuarTrasExplosion();
            Assert.AreEqual(BombaFase.Jugando, flow.Fase);
            Assert.IsNull(flow.Eliminado);
            Assert.AreNotEqual(prompt.Id, flow.CurrentPrompt.Id, "Sílaba nueva sin repetir");

            flow.Explotar();
            flow.ContinuarTrasExplosion();
            Assert.AreEqual(BombaFase.GameOver, flow.Fase);
            Assert.AreEqual("Ana", flow.Winner);

            flow.Reiniciar();
            Assert.AreEqual(BombaFase.Setup, flow.Fase);
            Assert.IsNull(flow.Session);
            Assert.IsNull(flow.Config);
            Assert.IsNull(flow.CurrentPrompt);
        }

        [Test]
        public void Flujo_SinPromptsDelModo_Error()
        {
            var fake = new FakePrompts { Categorias = new List<BombaPrompt>() };
            var flow = new BombaFlowController(fake, new SeededRandom(1));
            flow.Iniciar(Config(BombaMode.Categoria));
            Assert.AreEqual(BombaFase.Error, flow.Fase);
            Assert.AreEqual(BombaErrorKind.SinPrompts, flow.ErrorKind);
            Assert.IsNull(flow.Session);

            flow.Iniciar(Config(BombaMode.Silaba));
            Assert.AreEqual(BombaFase.Jugando, flow.Fase, "Con el otro modo sí puede");
            Assert.IsNull(flow.ErrorKind);
        }
    }
}
