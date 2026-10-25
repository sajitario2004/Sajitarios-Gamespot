using System.Collections;
using Gamespot.App;
using Gamespot.Core.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gamespot.Tests.PlayMode
{
    public sealed class BootstrapTests
    {
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.Destroy(_root);
        }

        [UnityTest]
        public IEnumerator Arranque_CreaLaUiConSusCapas()
        {
            _root = new GameObject("AppRoot");
            var bootstrap = _root.AddComponent<Bootstrap>();
            yield return null;

            Assert.IsNotNull(bootstrap.Ui, "El Bootstrap crea la raíz de la UI");
            Assert.IsNotNull(bootstrap.Ui.ScreenLayer);
            Assert.IsNotNull(bootstrap.Ui.OverlayLayer);
            Assert.IsNotNull(bootstrap.Ui.BackgroundLayer.GetComponentInChildren<NeonBackground>());

            var scaler = bootstrap.Ui.GetComponent<CanvasScaler>();
            Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, scaler.uiScaleMode);
            Assert.AreEqual(UiRoot.ReferenceResolution, scaler.referenceResolution);
        }
    }
}
