using System.Linq;
using Gamespot.Core;
using Gamespot.Games.HombresLobo.Domain;
using NUnit.Framework;
using static Gamespot.Tests.EditMode.Games.HombresLobo.Domain.LoboTestKit;

namespace Gamespot.Tests.EditMode.Games.HombresLobo.Domain
{
    /// <summary>V076: el día de Hombres Lobo (capitán, votación, empates, roles del día), la victoria y los eventos de luna.</summary>
    public sealed class LoboDayTests
    {
        private const LoboRol A = LoboRol.Aldeano;
        private const LoboRol L = LoboRol.Lobo;

        [Test]
        public void Pueblo_GanaAlLincharAlUltimoLobo()
        {
            var game = Game(L, A, A, A, A);
            Step(game, LoboPaso.Lobos, 1);
            ToDay(game);
            Assert.IsTrue(game.Linchar(0));
            Assert.AreEqual(LoboGanador.Pueblo, game.Victoria.Ganador);
            Assert.AreEqual(LoboFase.Final, game.Fase);
            Assert.IsFalse(game.EmpezarNoche());
        }

        [Test]
        public void Lobos_GananAlQuedarseSolosOAlIgualarConLaRegla()
        {
            var parity = Game(new LoboRules { LobosGananAlIgualar = true }, L, L, A, A, A);
            Step(parity, LoboPaso.Lobos, 2);
            Assert.AreEqual(LoboGanador.Lobos, parity.Victoria.Ganador, "Dos lobos y dos aldeanos");
            CollectionAssert.AreEquivalent(new[] { 0, 1 }, parity.Victoria.Jugadores);

            var game = Game(L, L, A, A, A);
            Step(game, LoboPaso.Lobos, 2);
            Assert.IsNull(game.Victoria, "Sin la regla, hasta que no quede nadie más");
            ToDay(game);
            game.Linchar(3);
            game.EmpezarNoche();
            Step(game, LoboPaso.Lobos, 4);
            Assert.AreEqual(LoboGanador.Lobos, game.Victoria.Ganador);
        }

        [Test]
        public void Enamorados_GananSiSonLosDosUltimos()
        {
            var game = Game(L, LoboRol.Cupido, A, A, A);
            Step(game, LoboPaso.Cupido, 0, 2);
            Step(game, LoboPaso.Enamorados);
            Step(game, LoboPaso.Lobos, 1);
            ToDay(game);
            game.Linchar(3);
            game.EmpezarNoche();
            Step(game, LoboPaso.Lobos, 4);
            Assert.AreEqual(LoboGanador.Enamorados, game.Victoria.Ganador);
            CollectionAssert.AreEquivalent(new[] { 0, 2 }, game.Victoria.Jugadores);
        }

        [Test]
        public void Capitan_SeEligeYAlMorirNombraSucesor()
        {
            var game = Game(L, A, A, A, A, A);
            Step(game, LoboPaso.Lobos, 5);
            ToDay(game);
            Assert.IsTrue(game.NecesitaCapitan);
            Assert.IsTrue(game.ElegirCapitan(1));
            Assert.IsFalse(game.NecesitaCapitan);
            Assert.IsTrue(game.VotoDobleCapitan);
            game.NadieLinchado();
            game.EmpezarNoche();
            Step(game, LoboPaso.Lobos, 1);
            Assert.AreEqual(LoboPendienteTipo.Sucesor, game.PendienteActual.Tipo);
            Assert.IsFalse(game.EmpezarDia());
            Assert.IsFalse(game.NombrarSucesor(1), "Un muerto no");
            Assert.IsTrue(game.NombrarSucesor(2));
            Assert.AreSame(game.Jugadores[2], game.Capitan);
            Assert.IsTrue(game.EmpezarDia());
        }

        [Test]
        public void Tonto_SeSalvaUnaVezYYaNoVota()
        {
            var game = Game(L, LoboRol.TontoDelPueblo, A, A, A);
            Step(game, LoboPaso.Lobos, 2);
            ToDay(game);
            Assert.IsTrue(game.Linchar(1));
            Assert.IsTrue(game.Jugadores[1].Vivo);
            Assert.IsTrue(game.Jugadores[1].TontoRevelado);
            Assert.IsFalse(game.Jugadores[1].PuedeVotar);
            Assert.IsTrue(game.LinchamientoHecho);
            Assert.IsFalse(game.Linchar(3), "Una votación por día");
        }

        [Test]
        public void Empate_MuereElChivo()
        {
            var game = Game(L, LoboRol.ChivoExpiatorio, A, A, A);
            Step(game, LoboPaso.Lobos, 2);
            ToDay(game);
            Assert.IsTrue(game.Empate(new[] { 0, 3 }));
            Assert.IsFalse(game.Jugadores[1].Vivo);
            Assert.AreEqual(LoboMuerte.Chivo, game.Jugadores[1].Causa);
            Assert.IsTrue(game.LinchamientoHecho);
        }

        [Test]
        public void Empate_SegundaVotacionEntreLosEmpatadosYLuegoNadie()
        {
            var game = Game(L, A, A, A, A, A);
            Step(game, LoboPaso.Lobos, 5);
            ToDay(game);
            Assert.IsFalse(game.Empate(new[] { 1 }), "Hacen falta dos");
            Assert.IsTrue(game.Empate(new[] { 0, 1 }));
            CollectionAssert.AreEqual(new[] { 0, 1 }, game.Empatados);
            Assert.IsFalse(game.Linchar(2), "Solo entre los empatados");
            Assert.IsTrue(game.Empate(new[] { 0, 1 }));
            Assert.IsTrue(game.LinchamientoHecho);
            Assert.AreEqual(5, game.Vivos.Count(), "Segundo empate: no muere nadie");

            var direct = Game(new LoboRules { DesempateSegundaVotacion = false }, L, A, A, A, A);
            Step(direct, LoboPaso.Lobos, 4);
            ToDay(direct);
            direct.Empate(new[] { 0, 1 });
            Assert.IsTrue(direct.LinchamientoHecho, "Sin segunda votación: nadie");
        }

        [Test]
        public void Sirvienta_SeQuedaConLaCartaDelLinchado()
        {
            var game = Game(L, LoboRol.Sirvienta, LoboRol.Vidente, A, A);
            Step(game, LoboPaso.Vidente, 0);
            Step(game, LoboPaso.Lobos, 3);
            ToDay(game);
            Assert.IsTrue(game.Linchar(2));
            Assert.AreEqual(LoboPendienteTipo.Sirvienta, game.PendienteActual.Tipo);
            Assert.IsTrue(game.Jugadores[2].Vivo, "Aún no se ha revelado");
            Assert.IsTrue(game.SirvientaDecide(true));
            Assert.IsFalse(game.Jugadores[2].Vivo);
            Assert.AreEqual(LoboRol.Vidente, game.Jugadores[1].Rol);
            game.EmpezarNoche();
            Assert.AreEqual(LoboPaso.Vidente, game.Paso.Paso, "La nueva vidente se despierta");
        }

        [Test]
        public void Juez_PideOtraVotacionUnaVez()
        {
            var game = Game(L, LoboRol.JuezTartamudo, A, A, A, A, A);
            Step(game, LoboPaso.Lobos, 2);
            ToDay(game);
            Assert.IsFalse(game.JuezDisponible, "Después de la primera votación");
            game.Linchar(3);
            Assert.IsTrue(game.JuezDisponible);
            Assert.IsTrue(game.PedirSegundaVotacion());
            Assert.IsFalse(game.LinchamientoHecho);
            Assert.IsTrue(game.Linchar(4));
            Assert.IsFalse(game.JuezDisponible, "Solo una vez");
        }

        [Test]
        public void GameMaster_MataYReviveAMano()
        {
            var game = Game(L, LoboRol.Cazador, A, A, A);
            Step(game, LoboPaso.Lobos, 2);
            ToDay(game);
            Assert.IsTrue(game.MatarManual(1));
            Assert.AreEqual(LoboMuerte.Master, game.Jugadores[1].Causa);
            Assert.AreEqual(LoboPendienteTipo.Cazador, game.PendienteActual.Tipo, "El cazador dispara igual");
            Assert.IsTrue(game.NoDisparar());
            Assert.IsTrue(game.RevivirManual(2));
            Assert.IsTrue(game.Jugadores[2].Vivo);
            game.Terminar();
            Assert.AreEqual(LoboFase.Final, game.Fase);
            Assert.IsNull(game.Victoria);
        }

        [Test]
        public void Sectario_GanaCuandoMuereElOtroGrupo()
        {
            // Con el sectario, los demás se reparten en dos grupos al azar (Fisher-Yates); muertos los del otro grupo, gana.
            var game = Game(null, new ScriptedRandom(true, 0.0), null, LoboRol.Sectario, L, A, A, A);
            var other = game.Jugadores.Where(p => p.Grupo == 1).Select(p => p.Index).ToList();
            Assert.That(other.Count, Is.EqualTo(2));
            foreach (var i in other)
            {
                if (game.Fase == LoboFase.Final) break;
                game.MatarManual(i);
                while (game.PendienteActual != null) game.NoDisparar();
            }
            Assert.AreEqual(LoboGanador.Sectario, game.Victoria?.Ganador);
        }

        [Test]
        public void Luna_LlenaDosVictimasYTormentaDosVotaciones()
        {
            // Primer amanecer: NextDouble 0,1 (< 1/3) y NextInt(8) con 0,0 → luna llena; segundo: 0,1 y 0,5 → tormenta.
            var game = Game(new LoboRules { EventosLuna = true }, new ScriptedRandom(0.1, 0.0, 0.1, 0.5, 0.9), null, L, A, A, A, A, A, A);
            Step(game, LoboPaso.Lobos, 1);
            Assert.AreEqual(LoboLuna.LunaLlena, game.Luna);
            ToDay(game);
            game.NadieLinchado();
            game.EmpezarNoche();
            Assert.AreEqual(2, game.Paso.Max, "Con luna llena, dos víctimas");
            Step(game, LoboPaso.Lobos, 2, 3);
            CollectionAssert.AreEquivalent(new[] { 2, 3 }, game.MuertesRecientes);
            Assert.AreEqual(LoboLuna.Tormenta, game.Luna);
            ToDay(game);
            game.Linchar(4);
            Assert.IsFalse(game.LinchamientoHecho, "Con tormenta se vota dos veces");
            game.Linchar(5);
            Assert.IsTrue(game.LinchamientoHecho);
        }

        [Test]
        public void Luna_DeSangreMuerenLosEmpatadosYNieblaOcultaRoles()
        {
            var game = Game(new LoboRules { EventosLuna = true }, new ScriptedRandom(0.1, 0.9, 0.1, 0.4), null, L, A, A, A, A, A);
            Step(game, LoboPaso.Lobos, 5);
            Assert.AreEqual(LoboLuna.LunaDeSangre, game.Luna);
            ToDay(game);
            game.Empate(new[] { 1, 2 });
            Assert.IsFalse(game.Jugadores[1].Vivo);
            Assert.IsFalse(game.Jugadores[2].Vivo);
            game.EmpezarNoche();
            Step(game, LoboPaso.Lobos, 3);
            Assert.AreEqual(LoboLuna.Niebla, game.Luna);
            Assert.IsFalse(game.RevelarRoles);
        }
    }
}
