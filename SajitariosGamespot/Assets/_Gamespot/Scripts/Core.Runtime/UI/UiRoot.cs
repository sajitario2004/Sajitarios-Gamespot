using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Raíz de toda la UI: un Canvas en modo <i>Screen Space – Camera</i> (para que el Bloom de URP
    /// afecte a la UI) con tres capas: fondo neón, pantallas y superposiciones (diálogos y avisos).
    /// Las pantallas y superposiciones respetan la zona segura.
    /// </summary>
    public sealed class UiRoot : MonoBehaviour
    {
        /// <summary>Resolución de referencia (iPhone 13/14, unidades lógicas).</summary>
        public static readonly Vector2 ReferenceResolution = new Vector2(390f, 844f);

        public Canvas Canvas { get; private set; }
        public RectTransform BackgroundLayer { get; private set; }
        public RectTransform ScreenLayer { get; private set; }
        public RectTransform OverlayLayer { get; private set; }

        /// <summary>Crea el Canvas y sus capas, renderizado por <paramref name="camera"/>.</summary>
        public static UiRoot Create(Camera camera)
        {
            var go = new GameObject("UiRoot", typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            if (camera != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 10f;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            canvas.pixelPerfect = false;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            go.AddComponent<GraphicRaycaster>();

            var root = go.AddComponent<UiRoot>();
            root.Canvas = canvas;
            root.BackgroundLayer = UiLayout.CreateStretched("Background", go.transform);
            NeonBackground.Create(root.BackgroundLayer);

            root.ScreenLayer = UiLayout.CreateStretched("Screens", go.transform);
            root.ScreenLayer.gameObject.AddComponent<SafeArea>();

            root.OverlayLayer = UiLayout.CreateStretched("Overlays", go.transform);
            return root;
        }
    }
}
