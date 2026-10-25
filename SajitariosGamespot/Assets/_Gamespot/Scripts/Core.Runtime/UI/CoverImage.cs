using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Imagen que rellena su caja recortando lo que sobra (como <c>BoxFit.cover</c>), centrada.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public sealed class CoverImage : MonoBehaviour
    {
        private RawImage _image;

        public static CoverImage Create(Transform parent, Texture texture, float height)
        {
            var go = new GameObject("CoverImage", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var raw = go.AddComponent<RawImage>();
            raw.texture = texture;
            raw.raycastTarget = false;
            var cover = go.AddComponent<CoverImage>();
            cover.Sized(height: height);
            return cover;
        }

        private void Awake() => _image = GetComponent<RawImage>();

        private void OnRectTransformDimensionsChange() => Refresh();

        private void OnEnable() => Refresh();

        private void Refresh()
        {
            if (_image == null || _image.texture == null) return;
            var box = ((RectTransform)transform).rect.size;
            if (box.x <= 0f || box.y <= 0f) return;
            var textureAspect = (float)_image.texture.width / _image.texture.height;
            var boxAspect = box.x / box.y;
            if (boxAspect > textureAspect)
            {
                var h = textureAspect / boxAspect;
                _image.uvRect = new Rect(0f, (1f - h) * 0.5f, 1f, h);
            }
            else
            {
                var w = boxAspect / textureAspect;
                _image.uvRect = new Rect((1f - w) * 0.5f, 0f, w, 1f);
            }
        }
    }
}
