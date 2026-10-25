using System.Collections;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Menu;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gamespot.Tests.PlayMode
{
    public sealed class BootstrapTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown() => _app?.Dispose();

        [UnityTest]
        public IEnumerator Arranque_CreaLaUiYMuestraElMenu()
        {
            _app = new AppTestHarness();
            yield return AppTestHarness.Frames();

            var ui = _app.Bootstrap.Ui;
            Assert.IsNotNull(ui.ScreenLayer);
            Assert.IsNotNull(ui.OverlayLayer);
            Assert.IsNotNull(ui.BackgroundLayer.GetComponentInChildren<NeonBackground>());
            var scaler = ui.GetComponent<CanvasScaler>();
            Assert.AreEqual(UiRoot.ReferenceResolution, scaler.referenceResolution);

            Assert.AreEqual(Router.Home, _app.Router.CurrentPath);
            Assert.IsInstanceOf<MenuScreen>(_app.Router.Current);
            Assert.IsNotNull(_app.Bootstrap.Database);
        }

        [UnityTest]
        public IEnumerator Menu_UnaTarjetaPorJuegoYAbreSuRuta()
        {
            _app = new AppTestHarness(() => new IGameDescriptor[] { new FakeGame("uno"), new FakeGame("dos"), new FakeGame("tres") });
            yield return AppTestHarness.Frames();

            var menu = (MenuScreen)_app.Router.Current;
            Assert.AreEqual(3, menu.Cards.Count);
            menu.Cards[1].Button.onClick.Invoke();
            yield return null;
            Assert.AreEqual("/dos", _app.Router.CurrentPath);
        }

        [UnityTest]
        public IEnumerator BotonAtras_VuelveAlPadreYEnElMenuSale()
        {
            _app = new AppTestHarness(() => new IGameDescriptor[] { new FakeGame("uno") });
            yield return AppTestHarness.Frames();

            _app.Router.Go("/uno");
            yield return null;
            _app.Router.Back();
            Assert.AreEqual(Router.Home, _app.Router.CurrentPath);
            _app.Router.Back();
            Assert.IsTrue(_app.QuitRequested, "Atrás en el menú sale de la app");
        }

        [UnityTest]
        public IEnumerator RutaDesconocida_PantallaDeError()
        {
            _app = new AppTestHarness();
            yield return AppTestHarness.Frames();
            _app.Router.Go("/no-existe");
            yield return null;
            Assert.IsInstanceOf<NotFoundScreen>(_app.Router.Current);
        }

        [UnityTest]
        public IEnumerator Reglas_SeApilanYAtrasLasQuita()
        {
            _app = new AppTestHarness();
            yield return AppTestHarness.Frames();
            _app.Router.Push(new RulesScreen("impostorTitulo", GameUi.RuleKeys("reglasImpostor", 6)));
            yield return null;
            Assert.IsInstanceOf<RulesScreen>(_app.Router.Current);
            _app.Router.Back();
            Assert.IsInstanceOf<MenuScreen>(_app.Router.Current);
        }

        [UnityTest]
        public IEnumerator Dialogo_AtrasLoCierraPrimero()
        {
            _app = new AppTestHarness();
            yield return AppTestHarness.Frames();
            var confirmado = (bool?)null;
            AbandonGameDialog.Show(() => confirmado = true, () => confirmado = false);
            Assert.IsTrue(Dialogs.IsOpen);
            _app.Router.Back();
            Assert.IsFalse(Dialogs.IsOpen);
            Assert.AreEqual(false, confirmado);
            Assert.IsFalse(_app.QuitRequested);
        }
    }
}
