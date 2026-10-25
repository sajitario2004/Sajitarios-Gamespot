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
        public IEnumerator Seed_192FrasesConLosMinimos()
        {
            _app = new AppTestHarness();
            yield return null;
            var repo = new NeverStatementRepository(_app.Bootstrap.Database);
            var suaves = repo.GetByIntensidades(new[] { Intensidad.Suave }).Count;
            var picantes = repo.GetByIntensidades(new[] { Intensidad.Picante }).Count;
            Assert.That(suaves + picantes, Is.GreaterThanOrEqualTo(140));
            Assert.That(suaves, Is.GreaterThanOrEqualTo(50));
            Assert.That(picantes, Is.GreaterThanOrEqualTo(30));
            Assert.AreEqual(0, YoNuncaSchema.SeedIfEmpty(_app.Bootstrap.Database, new ResourcesSeedSource()));
        }
    }
}
