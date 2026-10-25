using System.Collections;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode
{
    /// <summary>
    /// V061: con un notch arriba y la barra de gestos abajo (zona segura simulada), las pantallas, los diálogos y los
    /// avisos quedan dentro de la zona segura y el velo del diálogo cubre toda la pantalla.
    /// </summary>
    public sealed class SafeAreaTests
    {
        private const float TopInset = 0.06f;
        private const float BottomInset = 0.03f;

        private AppTestHarness _app;

        [SetUp]
        public void SetUp()
        {
            SafeArea.Simulated = new Rect(0f, Screen.height * BottomInset, Screen.width, Screen.height * (1f - TopInset - BottomInset));
        }

        [TearDown]
        public void TearDown()
        {
            SafeArea.Simulated = null;
            _app?.Dispose();
        }

        /// <summary>Esquinas en píxeles de pantalla.</summary>
        private Rect ScreenRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var camera = _app.Bootstrap.Ui.Canvas.worldCamera;
            var min = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
            var max = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static void AssertInside(Rect inner, Rect outer, string what)
        {
            Assert.That(inner.yMax, Is.LessThanOrEqualTo(outer.yMax + 1f), $"{what}: se mete en el notch");
            Assert.That(inner.yMin, Is.GreaterThanOrEqualTo(outer.yMin - 1f), $"{what}: se mete en la barra de gestos");
        }

        [UnityTest]
        public IEnumerator PantallasDialogosYAvisosDentroDeLaZonaSegura()
        {
            _app = new AppTestHarness();
            _app.Router.Go("/");
            yield return AppTestHarness.Frames();
            var safe = SafeArea.Current;

            var screens = ScreenRect(_app.Bootstrap.Ui.ScreenLayer);
            Assert.AreEqual(safe.yMax, screens.yMax, 1f, "La capa de pantallas empieza bajo el notch");
            Assert.AreEqual(safe.yMin, screens.yMin, 1f, "Y acaba sobre la barra de gestos");

            Snackbar.Show(Loc.T("palabraAnadida"));
            Dialogs.Confirm(Loc.T("borrarPalabraTitulo"), Loc.T("borrarPalabraMensaje", ("palabra", "Pirata")), Loc.T("cancelar"), Loc.T("borrar"), _ => { });
            yield return AppTestHarness.Frames();

            var overlay = _app.Bootstrap.Ui.OverlayLayer;
            var snackbar = overlay.Find("SnackbarArea/Snackbar") as RectTransform;
            Assert.IsNotNull(snackbar);
            AssertInside(ScreenRect(snackbar), safe, "Aviso");

            var dialog = overlay.Find("Dialog") as RectTransform;
            Assert.IsNotNull(dialog);
            var scrim = ScreenRect(dialog);
            Assert.AreEqual(Screen.height, scrim.yMax, 1f, "El velo cubre también el notch");
            AssertInside(ScreenRect((RectTransform)dialog.Find("Safe/Holder")), safe, "Diálogo");

            Snackbar.Hide();
            yield return null;
            Assert.IsNull(overlay.Find("SnackbarArea"), "El aviso se va con su contenedor");
        }
    }
}
