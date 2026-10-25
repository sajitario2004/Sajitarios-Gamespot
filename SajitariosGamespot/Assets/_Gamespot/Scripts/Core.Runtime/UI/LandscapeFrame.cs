using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Marco girado 90° para pintar en horizontal dentro de la app vertical (V075, el móvil en la frente): ocupa el
    /// rectángulo del padre con el ancho y el alto cambiados y gira hacia el lado de <see cref="Angle"/> (+90 o −90).
    /// Su contenido se coloca con layouts normales, en los ejes ya girados. El tamaño se recalcula cuando cambia el del
    /// padre (así el primer fotograma ya sale bien) y, por si acaso, en <c>LateUpdate</c>.
    /// </summary>
    public sealed class LandscapeFrame : MonoBehaviour
    {
        private RectTransform _rect;
        private Vector2 _lastSize = new Vector2(-1f, -1f);
        private float _angle = -90f;

        public float Angle
        {
            get => _angle;
            set
            {
                var angle = value >= 0f ? 90f : -90f;
                if (Mathf.Approximately(angle, _angle)) return;
                _angle = angle;
                Apply(true);
            }
        }

        public RectTransform Content => _rect;

        public static LandscapeFrame Create(Transform parent, float angle = -90f)
        {
            var go = new GameObject("Landscape", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().ignoreLayout = true;
            var frame = go.AddComponent<LandscapeFrame>();
            frame._rect = (RectTransform)go.transform;
            frame._rect.anchorMin = frame._rect.anchorMax = new Vector2(0.5f, 0.5f);
            frame._rect.pivot = new Vector2(0.5f, 0.5f);
            frame._rect.anchoredPosition = Vector2.zero;
            frame._angle = angle >= 0f ? 90f : -90f;
            parent.gameObject.GetOrAdd<LandscapeFrameHost>().Frame = frame;
            frame.Apply(true);
            return frame;
        }

        private void OnEnable() => Apply(true);

        private void LateUpdate() => Apply(false);

        internal void Apply(bool force)
        {
            if (_rect == null) _rect = (RectTransform)transform;
            var parent = _rect.parent as RectTransform;
            if (parent == null) return;
            var size = parent.rect.size;
            if (!force && size == _lastSize) return;
            _lastSize = size;
            _rect.sizeDelta = new Vector2(size.y, size.x);
            _rect.localRotation = Quaternion.Euler(0f, 0f, _angle);
        }
    }

    /// <summary>Avisa al <see cref="LandscapeFrame"/> hijo cuando cambia el tamaño de su padre.</summary>
    public sealed class LandscapeFrameHost : MonoBehaviour
    {
        public LandscapeFrame Frame;

        private void OnRectTransformDimensionsChange()
        {
            if (Frame != null) Frame.Apply(false);
        }
    }
}
