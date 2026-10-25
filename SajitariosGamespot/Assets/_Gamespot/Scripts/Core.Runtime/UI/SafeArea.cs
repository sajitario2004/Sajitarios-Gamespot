using UnityEngine;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Ajusta los anclajes del RectTransform a <see cref="Screen.safeArea"/> (muesca, barras del sistema).
    /// Solo recalcula cuando cambia la zona segura o la resolución.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreen;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            Apply();
        }

        private void Update()
        {
            if (_lastSafeArea != Screen.safeArea || _lastScreen.x != Screen.width || _lastScreen.y != Screen.height)
            {
                Apply();
            }
        }

        private void Apply()
        {
            _lastSafeArea = Screen.safeArea;
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
    }
}
