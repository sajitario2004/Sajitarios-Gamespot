using UnityEngine;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Ajusta los anclajes del RectTransform a <see cref="Screen.safeArea"/> (muesca, barras del sistema). El padre
    /// tiene que ocupar toda la pantalla. Solo recalcula cuando cambia la zona segura o la resolución.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreen;

        /// <summary>
        /// Zona segura simulada en píxeles de pantalla (V061): para probar un notch en el Editor y en los tests.
        /// Nula = la del dispositivo.
        /// </summary>
        public static Rect? Simulated { get; set; }

        /// <summary>La zona segura que se aplica ahora.</summary>
        public static Rect Current => Simulated ?? Screen.safeArea;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            Apply();
        }

        private void Update()
        {
            if (_lastSafeArea != Current || _lastScreen.x != Screen.width || _lastScreen.y != Screen.height)
            {
                Apply();
            }
        }

        private void Apply()
        {
            _lastSafeArea = Current;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) return;

            var min = _lastSafeArea.position;
            var max = _lastSafeArea.position + _lastSafeArea.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;
            _rect.anchorMin = min;
            _rect.anchorMax = max;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Simulated = null;
    }
}
