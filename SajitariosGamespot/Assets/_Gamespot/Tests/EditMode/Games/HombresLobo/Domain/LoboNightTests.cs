using System.Linq;
using Gamespot.Core;
using Gamespot.Games.HombresLobo.Domain;
using NUnit.Framework;
using static Gamespot.Tests.EditMode.Games.HombresLobo.Domain.LoboTestKit;

namespace Gamespot.Tests.EditMode.Games.HombresLobo.Domain
{
    /// <summary>V076: la noche de Hombres Lobo, paso a paso, y lo que pasa al amanecer.</summary>
    public sealed class LoboNightTests
    {
        private const LoboRol A = LoboRol.Aldeano;
        private const LoboRol L = LoboRol.Lobo;

        [Test]
        public void Basica_VidenteLobosBrujaYMuerteAlAmanecer()
        {
            var game = Game(L, LoboRol.Vidente, LoboRol.Bruja, A, A);
            Step(game, LoboPaso.Vidente, 0);
            Assert.AreEqual(LoboRol.Lobo, game.VidenteVio);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4 }, game.Paso.Candidatos, "Los lobos eligen entre los que no son lobos");
            Step(game, LoboPaso.Lobos, 3);
            Option(game, LoboPaso.BrujaSalva, LoboOpcion.No);
            Step(game, LoboPaso.BrujaEnvenena);
            Assert.AreEqual(LoboFase.Amanecer, game.Fase);
            CollectionAssert.AreEqual(new[] { 3 }, game.MuertesRecientes);
            Assert.AreEqual(LoboMuerte.Lobos, game.Jugadores[3].Causa);
            Assert.IsNull(game.Victoria);
            Assert.IsFalse(game.ResolverPaso(new[] { 1 }), "Ya no hay paso");
        }

        [Test]
        public void Bruja_SalvaUnaVezYEnvenena()
        {
            var game = Game(L, LoboRol.Bruja, A, A, A, A);
            Step(game, LoboPaso.Lobos, 2);
            Option(game, LoboPaso.BrujaSalva, LoboOpcion.Si);
            Step(game, LoboPaso.BrujaEnvenena, 3);
            Assert.IsTrue(game.Jugadores[2].Vivo, "Salvada por la poción");
            Assert.IsFalse(game.Jugadores[3].Vivo);
            Assert.AreEqual(LoboMuerte.Veneno, game.Jugadores[3].Causa);
            ToDay(game);
            game.NadieLinchado();
            game.EmpezarNoche();
            Step(game, LoboPaso.Lobos, 2);
            Assert.AreEqual(LoboFase.Amanecer, game.Fase, "Sin pociones, la bruja ya no se despierta");
            Assert.IsFalse(game.Jugadores[2].Vivo);
        }

        [Test]
        public void Protector_SalvaYNoRepite()
        {
            var game = Game(L, LoboRol.Protector, A, A, A);
            CollectionAssert.Contains(game.Paso.Candidatos, 1, "Puede protegerse a sí mismo");
            Step(game, LoboPaso.Protector, 2);
            Step(game, LoboPaso.Lobos, 2);
            Assert.IsTrue(game.Jugadores[2].Vivo);
            Assert.IsTrue(game.Registro.Any(e => e.Tipo == LoboEventoTipo.Protegido && e.Jugador == 2));
            ToDay(game);
            game.NadieLinchado();
            game.EmpezarNoche();
            Assert.AreEqual(LoboPaso.Protector, game.Paso.Paso);
            CollectionAssert.DoesNotContain(game.Paso.Candidatos, 2, "No dos noches seguidas a la misma");
        }

        [Test]
        public void Cupido_LosEnamoradosMuerenJuntos()
        {
            var game = Game(L, LoboRol.Cupido, A, A, A, A);
            Step(game, LoboPaso.Cupido, 2, 3);
            Step(game, LoboPaso.Enamorados);
            Step(game, LoboPaso.Lobos, 2);
            CollectionAssert.AreEqual(new[] { 2, 3 }, game.MuertesRecientes);
            Assert.AreEqual(LoboMuerte.Pena, game.Jugadores[3].Causa);
        }

        [Test]
        public void Cazador_DisparaAlMorirYGanaElPueblo()
        {
            var game = Game(L, LoboRol.Cazador, A, A, A);
            Step(game, LoboPaso.Lobos, 1);
            Assert.AreEqual(LoboPendienteTipo.Cazador, game.PendienteActual.Tipo);
            Assert.IsFalse(game.EmpezarDia(), "Primero dispara");
            Assert.IsFalse(game.Disparar(1), "No a un muerto");
            Assert.IsTrue(game.Disparar(0));
            Assert.AreEqual(LoboFase.Final, game.Fase);
            Assert.AreEqual(LoboGanador.Pueblo, game.Victoria.Ganador);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4 }, game.Victoria.Jugadores);
        }

        [Test]
        public void Anciano_AguantaUnAtaqueYSiLoMataElPuebloSePierdenLosPoderes()
        {
            var game = Game(L, LoboRol.Anciano, LoboRol.Vidente, A, A);
            Step(game, LoboPaso.Vidente, 1);
            Step(game, LoboPaso.Lobos, 1);
            Assert.IsTrue(game.Jugadores[1].Vivo);
            Assert.AreEqual(0, game.Jugadores[1].VidasExtra);
            ToDay(game);
            Assert.IsTrue(game.Linchar(1));
            Assert.IsTrue(game.PoderesPerdidos);
            game.EmpezarNoche();
            Assert.AreEqual(LoboPaso.Lobos, game.Paso.Paso, "La vidente ya no se despierta");
        }

        [Test]
        public void PadreLobos_InfectaEnVezDeMatarUnaVez()
        {
            var game = Game(LoboRol.PadreLobos, LoboRol.Bruja, A, A, A);
            Step(game, LoboPaso.Lobos, 2);
            Option(game, LoboPaso.PadreLobos, LoboOpcion.Si);
            Assert.AreEqual(LoboPaso.BrujaEnvenena, game.Paso.Paso, "Infectada: no hay a quién salvar");
            Step(game, LoboPaso.BrujaEnvenena);
            Assert.IsTrue(game.Jugadores[2].Vivo);
            Assert.IsTrue(game.Jugadores[2].Infectado);
            Assert.IsTrue(game.EsLobo(2));
            Assert.AreEqual(LoboBando.Lobos, game.BandoDe(2));
            ToDay(game);
            game.NadieLinchado();
            game.EmpezarNoche();
            Step(game, LoboPaso.Lobos, 3);
            Assert.AreEqual(LoboPaso.BrujaSalva, game.Paso.Paso, "El padre solo infecta una vez");
        }

        [Test]
        public void GranLoboMalo_SegundaVictimaHastaQueMuereUnLobo()
        {
            var game = Game(LoboRol.GranLoboMalo, L, A, A, A, A, A);
            Step(game, LoboPaso.Lobos, 2);
            CollectionAssert.DoesNotContain(game.Paso.Candidatos, 2);
            Step(game, LoboPaso.GranLoboMalo, 3);
            CollectionAssert.AreEquivalent(new[] { 2, 3 }, game.MuertesRecientes);
            ToDay(game);
            game.Linchar(1);
            game.EmpezarNoche();
            Step(game, LoboPaso.Lobos, 4);
            Assert.AreEqual(LoboFase.Amanecer, game.Fase, "Muerto un lobo, ya no hay segunda víctima");
        }

        [Test]
        public void LoboBlanco_LasNochesParesYGanaSolo()
        {
            var game = Game(LoboRol.LoboBlanco, L, A, A, A);
            Step(game, LoboPaso.Lobos, 2);
            ToDay(game);
            game.NadieLinchado();
            game.EmpezarNoche();
            Step(game, LoboPaso.Lobos, 3);
            CollectionAssert.AreEqual(new[] { 1 }, game.Paso.Candidatos);
            Step(game, LoboPaso.LoboBlanco, 1);
            CollectionAssert.AreEquivalent(new[] { 3, 1 }, game.MuertesRecientes);
            Assert.IsNull(game.Victoria, "El lobo blanco tiene que quedarse solo");
            ToDay(game);
            game.Linchar(4);
            Assert.AreEqual(LoboGanador.LoboBlanco, game.Victoria.Ganador);
        }

        [Test]
        public void Ladron_CambiaSuCartaYConDosLobosTieneQueCoger()
        {
            var game = Game(null, null, new[] { LoboRol.Vidente, A }, LoboRol.Ladron, L, A, A, A);
            Assert.AreEqual(LoboPaso.Ladron, game.Paso.Paso);
            CollectionAssert.Contains(game.Paso.Opciones, LoboOpcion.Quedarse);
            Option(game, LoboPaso.Ladron, LoboOpcion.Carta1);
            Assert.AreEqual(LoboRol.Vidente, game.Jugadores[0].Rol);
            Assert.AreEqual(LoboRol.Ladron, game.CartasLadron[0]);
            Assert.AreEqual(LoboPaso.Vidente, game.Paso.Paso, "Ahora es la vidente");

            var wolves = Game(null, null, new[] { L, L }, LoboRol.Ladron, LoboRol.Vidente, A, A, A);
            CollectionAssert.DoesNotContain(wolves.Paso.Opciones, LoboOpcion.Quedarse, "Con dos lobos no se puede quedar");
            Option(wolves, LoboPaso.Ladron, LoboOpcion.Carta2);
            Assert.IsTrue(wolves.EsLobo(0));
        }

        [Test]
        public void PerroLoboYNinoSalvaje()
        {
            var dog = Game(LoboRol.PerroLobo, L, A, A, A);
            Option(dog, LoboPaso.PerroLobo, LoboOpcion.Lobos);
            Assert.IsTrue(dog.EsLobo(0));
            CollectionAssert.DoesNotContain(dog.Paso.Candidatos, 0, "Ya es de los lobos");

            var child = Game(LoboRol.NinoSalvaje, L, A, A, A);
            Step(child, LoboPaso.NinoSalvaje, 2);
            Assert.IsFalse(child.EsLobo(0));
            Step(child, LoboPaso.Lobos, 2);
            Assert.IsTrue(child.EsLobo(0), "Murió su modelo: ahora es lobo");
            Assert.IsTrue(child.Registro.Any(e => e.Tipo == LoboEventoTipo.NinoSalvajeSeConvierte));
        }

        [Test]
        public void Zorro_PierdeElOlfatoSiNoHayLobo()
        {
            var game = Game(LoboRol.Zorro, L, A, A, A);
            Step(game, LoboPaso.Zorro, 2);
            Assert.AreEqual(true, game.ZorroOlio, "Al lado de J2 está el lobo J1");
            Step(game, LoboPaso.Lobos, 4);
            ToDay(game);
            game.NadieLinchado();
            game.EmpezarNoche();
            Step(game, LoboPaso.Zorro, 3);
            Assert.AreEqual(false, game.ZorroOlio, "J2, J3 y J0: ningún lobo");
            Step(game, LoboPaso.Lobos, 2);
            ToDay(game);
            game.NadieLinchado();
            game.EmpezarNoche();
            Assert.AreEqual(LoboPaso.Lobos, game.Paso.Paso, "Sin olfato ya no se despierta");
        }

        [Test]
        public void OsoGruneConUnLoboAlLado()
        {
            var game = Game(LoboRol.OsoDomador, L, A, A, A);
            Step(game, LoboPaso.Lobos, 3);
            Assert.AreEqual(true, game.OsoGrune);
            CollectionAssert.AreEquivalent(new[] { 4, 1 }, game.Vecinos(0));
        }

        [Test]
        public void Caballero_SuEspadaMataAlLoboDeSuIzquierdaLaNocheSiguiente()
        {
            var game = Game(LoboRol.Caballero, A, L, A, A, L);
            Step(game, LoboPaso.Lobos, 0);
            Assert.IsTrue(game.Jugadores[2].Vivo, "Aún no");
            ToDay(game);
            game.NadieLinchado();
            game.EmpezarNoche();
            Step(game, LoboPaso.Lobos, 1);
            Assert.IsFalse(game.Jugadores[2].Vivo);
            Assert.AreEqual(LoboMuerte.Oxido, game.Jugadores[2].Causa);
            Assert.IsTrue(game.Jugadores[5].Vivo);
        }

        [Test]
        public void Flautista_GanaCuandoTodosLosVivosEstanEncantados()
        {
            var game = Game(LoboRol.Flautista, L, A, A, A);
            Step(game, LoboPaso.Lobos, 2);
            Step(game, LoboPaso.Flautista, 3, 4);
            Step(game, LoboPaso.Encantados);
            Assert.IsNull(game.Victoria);
            ToDay(game);
            game.NadieLinchado();
            game.EmpezarNoche();
            Step(game, LoboPaso.Lobos, 3);
            Assert.AreEqual(1, game.Paso.Max, "Solo queda uno por encantar");
            Step(game, LoboPaso.Flautista, 1);
            Step(game, LoboPaso.Encantados);
            Assert.AreEqual(LoboGanador.Flautista, game.Victoria.Ganador);
            Assert.AreEqual(LoboFase.Final, game.Fase);
        }

        [Test]
        public void Angel_GanaSiMuereLaPrimeraNoche()
        {
            var game = Game(LoboRol.Angel, L, A, A, A);
            Step(game, LoboPaso.Lobos, 0);
            Assert.AreEqual(LoboGanador.Angel, game.Victoria.Ganador);
            Assert.AreEqual(LoboFase.Final, game.Fase);
        }

        [Test]
        public void PrimeraNocheSinMuertes()
        {
            var game = Game(new LoboRules { PrimeraNocheSinMuertes = true }, L, A, A, A, A);
            Assert.AreEqual(LoboPaso.Lobos, game.Paso.Paso);
            Assert.IsTrue(game.Paso.EsAviso);
            Step(game, LoboPaso.Lobos);
            Assert.AreEqual(0, game.MuertesRecientes.Count);
        }

        [Test]
        public void HermanasYHermanosSeReconocenLaPrimeraNoche()
        {
            var game = Game(L, LoboRol.Hermana, LoboRol.Hermana, LoboRol.Hermano, LoboRol.Hermano, LoboRol.Hermano);
            var steps = FinishNight(game);
            CollectionAssert.AreEqual(new[] { LoboPaso.Hermanas, LoboPaso.Hermanos, LoboPaso.Lobos }, steps);
        }

        [Test]
        public void Deshacer_VuelveAlPasoAnterior()
        {
            var game = Game(L, LoboRol.Vidente, A, A, A);
            Step(game, LoboPaso.Vidente, 0);
            Step(game, LoboPaso.Lobos, 2);
            Assert.AreEqual(LoboFase.Amanecer, game.Fase);
            Assert.IsTrue(game.Deshacer());
            Assert.AreEqual(LoboPaso.Lobos, game.Paso.Paso);
            Assert.IsTrue(game.Jugadores[2].Vivo);
            Assert.IsTrue(game.Deshacer());
            Assert.AreEqual(LoboPaso.Vidente, game.Paso.Paso);
            Assert.IsNull(game.VidenteVio);
        }

        [Test]
        public void PasoInvalido_NoHaceNada()
        {
            var game = Game(L, LoboRol.Cupido, A, A, A);
            Assert.IsFalse(game.ResolverPaso(new[] { 1 }), "Cupido elige a dos");
            Assert.IsFalse(game.ResolverPaso(new[] { 1, 1 }), "Dos distintos");
            Assert.IsFalse(game.ResolverPaso(new[] { 1, 9 }), "Que existan");
            Assert.AreEqual(LoboPaso.Cupido, game.Paso.Paso);
            Assert.IsFalse(game.PuedeDeshacer);
        }
    }
}
