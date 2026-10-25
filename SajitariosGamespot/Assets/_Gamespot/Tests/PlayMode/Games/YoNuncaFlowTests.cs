using System.Collections;
using System.Linq;
using Gamespot.Core.Runtime.Db;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.YoNunca;
using Gamespot.Games.YoNunca.Domain;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    public sealed class YoNuncaFlowTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown() => _app?.Dispose();

        private YoNuncaModule Module => _app.Bootstrap.Games.OfType<YoNuncaGame>().Single().Module;

        private YoNuncaSetupScreen OpenSetup()
        {
            _app = new AppTestHarness();
            _app.Router.Go(YoNuncaModule.SetupRoute);
            return (YoNuncaSetupScreen)_app.Router.Current;
        }

        [UnityTest]
        public IEnumerator AvisoPicante_ApareceYDesaparece()
        {
            var setup = OpenSetup();
            yield return null;
            Assert.IsFalse(setup.WarningVisible, "Por defecto solo suave");
            setup.Toggle(Intensidad.Picante, true);
            Assert.IsTrue(setup.WarningVisible);
            setup.Toggle(Intensidad.Picante, false);
            Assert.IsFalse(setup.WarningVisible);
        }

        [UnityTest]
        public IEnumerator SinIntensidades_Aviso()
        {
            var setup = OpenSetup();
            yield return null;
            setup.Toggle(Intensidad.Suave, false);
            setup.Start();
            Assert.AreEqual(Loc.T("yoNuncaErrorSinIntensidades"), Snackbar.CurrentText);
        }

        [UnityTest]
        public IEnumerator Jugar_SiguienteCambiaLaFraseYAtrasPideConfirmar()
        {
            var setup = OpenSetup();
            yield return null;
            setup.Start();
            yield return null;
            var play = (YoNuncaPlayScreen)_app.Router.Current;
            var first = play.Statement;
            Assert.IsTrue(first.StartsWith("Yo nunca"));
            Assert.AreEqual(Intensidad.Suave, Module.Flow.Current.Intensidad);
            play.Next();
            Assert.AreNotEqual(first, play.Statement, "Sin repetir hasta agotar");
            _app.Router.Back();
            Assert.IsTrue(Dialogs.IsOpen, "Atrás pide confirmación");
        }

        [UnityTest]
        public IEnumerator Seed_ConLosMinimos()
        {
            _app = new AppTestHarness();
            yield return null;
            var repo = new NeverStatementRepository(_app.Bootstrap.Database);
            var suaves = repo.GetByIntensidades(new[] { Intensidad.Suave }).Count;
            var picantes = repo.GetByIntensidades(new[] { Intensidad.Picante }).Count;
            Assert.That(suaves, Is.GreaterThanOrEqualTo(90));
            Assert.That(picantes, Is.GreaterThanOrEqualTo(250), "Picante mucho más grande (V059q)");
            Assert.AreEqual(0, YoNuncaSchema.SeedIfEmpty(_app.Bootstrap.Database, new ResourcesSeedSource()));
        }

        /// <summary>V059q: el chip de Picante +18 pide confirmar la edad la primera vez; cancelar lo deja sin marcar.</summary>
        [UnityTest]
        public IEnumerator Picante_PideConfirmarQueTodosSonMayores()
        {
            var setup = OpenSetup();
            yield return null;
            Assert.AreEqual(Loc.T("yoNuncaIntensidadPicante"), "Picante +18");
            setup.PicanteChip.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Assert.IsTrue(Dialogs.IsOpen, "Pregunta antes de marcar");
            Assert.IsFalse(Module.Draft.Contains(Intensidad.Picante));
            Dialogs.HandleBack();
            Assert.IsFalse(Module.Draft.Contains(Intensidad.Picante), "Sin confirmar no se marca");
            Assert.IsFalse(setup.WarningVisible);

            setup.RequestPicante(true);
            var confirm = UnityEngine.Object.FindObjectsByType<NeonButton>(UnityEngine.FindObjectsSortMode.None)
                .First(b => b.isActiveAndEnabled && b.name == $"Button {Loc.T("yoNuncaConfirmarPicanteSi")}");
            confirm.Button.onClick.Invoke();
            yield return null;
            Assert.IsTrue(Module.Draft.Contains(Intensidad.Picante));
            Assert.IsTrue(setup.WarningVisible);
            Assert.IsTrue(Module.AdultsConfirmed);
            setup.RequestPicante(false);
            setup.RequestPicante(true);
            Assert.IsFalse(Dialogs.IsOpen, "Ya confirmado en la sesión: no vuelve a preguntar");
            Assert.IsTrue(Module.Draft.Contains(Intensidad.Picante));
        }

        /// <summary>V059q: una frase propia se crea desde «Frases» y se puede jugar solo con las propias.</summary>
        [UnityTest]
        public IEnumerator FrasesPropias_SeCreanYSeJuegaSoloConEllas()
        {
            _app = new AppTestHarness();
            _app.Router.Go(YoNuncaModule.SetupRoute);
            yield return null;
            Assert.IsFalse(((YoNuncaSetupScreen)_app.Router.Current).OriginVisible, "Sin frases propias no se elige el origen");

            _app.Router.Go(YoNuncaModule.PhrasesRoute);
            var phrases = (YoNuncaPhrasesScreen)_app.Router.Current;
            yield return null;
            Assert.That(phrases.Items.Count, Is.GreaterThanOrEqualTo(340), "Salen las de serie");
            Assert.AreEqual(YoNuncaPhraseProblem.Vacia, phrases.Save(null, "Yo nunca", Intensidad.Suave));
            Assert.IsNull(phrases.Save(null, "he cantado en un karaoke de Benidorm", Intensidad.Suave));
            Assert.AreEqual(YoNuncaPhraseProblem.Duplicada, phrases.Save(null, "Yo nunca he cantado en un karaoke de Benidorm.", Intensidad.Suave));
            phrases.SetFilter(YoNuncaPhraseFilter.Propias);
            var mine = phrases.Items.Single();
            Assert.AreEqual("Yo nunca he cantado en un karaoke de Benidorm", mine.Frase, "Con «Yo nunca» delante");

            _app.Router.Go(YoNuncaModule.SetupRoute);
            var setup = (YoNuncaSetupScreen)_app.Router.Current;
            yield return null;
            Assert.IsTrue(setup.OriginVisible);
            setup.SetOrigin(false, true);
            setup.Start();
            yield return null;
            var play = (YoNuncaPlayScreen)_app.Router.Current;
            Assert.AreEqual(mine.Frase, play.Statement, "Solo las propias");
            play.Next();
            Assert.AreEqual(mine.Frase, play.Statement, "Con una sola, se repite");
        }
    }
}
