using Gamespot.Core.Runtime.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Fondo neón de todas las pantallas (port de <c>NeonBackground</c>): degradado radial del 10 % de
    /// magenta sobre el fondo, centrado en (−0.6, −0.8) con radio 1.4 × el lado corto, más una rejilla
    /// de líneas cian cada 36 unidades. El Flutter usaba un 5 % de alfa mezclado en sRGB; Unity mezcla en
    /// espacio lineal y ese 5 % sale mucho más brillante, así que aquí va más bajo para el mismo aspecto.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class NeonBackground : MonoBehaviour
    {
        private const float GridSpacing = 36f;
        private const float GridAlpha = 0.018f;
        private const float GradientRadius = 1.4f;
        private static readonly Vector2 GradientCenter = new Vector2(-0.6f, -0.8f);
        private const int GradientDownscale = 6;

        private static Texture2D _gridTile;

        private RectTransform _rect;
        private RawImage _gradient;
        private RawImage _grid;
        private Texture2D _gradientTexture;
        private Vector2 _lastSize;

        /// <summary>Crea el fondo estirado a todo el padre.</summary>
        public static NeonBackground Create(Transform parent)
        {
            var go = new GameObject("NeonBackground", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiLayout.Stretch((RectTransform)go.transform);
            return go.AddComponent<NeonBackground>();
        }

        private void Awake()
        {
            _rect = (RectTransform)transform;

            _gradient = CreateLayer("Gradient");
            _gradient.color = Color.white;

            _grid = CreateLayer("Grid");
            _grid.texture = GridTile();
            _grid.color = Palette.NeonCyan.WithAlpha(GridAlpha);
        }

        private void OnEnable() => Refresh();

        private void OnRectTransformDimensionsChange()
        {
            if (_rect != null) Refresh();
        }

        private void OnDestroy()
        {
            if (_gradientTexture != null) Destroy(_gradientTexture);
        }

        private RawImage CreateLayer(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            UiLayout.Stretch((RectTransform)go.transform);
            var image = go.AddComponent<RawImage>();
            image.raycastTarget = false;
            return image;
        }

        private void Refresh()
        {
            var size = _rect.rect.size;
            if (size.x <= 1f || size.y <= 1f || size == _lastSize) return;
            _lastSize = size;
            _grid.uvRect = new Rect(0f, 0f, size.x / GridSpacing, size.y / GridSpacing);
            RebuildGradient(size);
        }

        private void RebuildGradient(Vector2 size)
        {
            var w = Mathf.Max(8, Mathf.CeilToInt(size.x / GradientDownscale));
            var h = Mathf.Max(8, Mathf.CeilToInt(size.y / GradientDownscale));
            if (_gradientTexture == null || _gradientTexture.width != w || _gradientTexture.height != h)
            {
                if (_gradientTexture != null) Destroy(_gradientTexture);
                _gradientTexture = new Texture2D(w, h, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    name = "NeonBackgroundGradient"
                };
            }

            var inner = Palette.AlphaBlend(Palette.NeonMagenta.WithAlpha(0.10f), Palette.Background);
            var outer = Palette.Background;
            // Alignment de Flutter: (-1,-1) es arriba a la izquierda. En la textura, y crece hacia arriba.
            var cx = (GradientCenter.x + 1f) * 0.5f * size.x;
            var cyFromTop = (GradientCenter.y + 1f) * 0.5f * size.y;
            var cy = size.y - cyFromTop;
            var radius = GradientRadius * Mathf.Min(size.x, size.y);

            var pixels = new Color32[w * h];
            for (var y = 0; y < h; y++)
            {
                var py = (y + 0.5f) / h * size.y;
                for (var x = 0; x < w; x++)
                {
                    var px = (x + 0.5f) / w * size.x;
                    var t = Mathf.Clamp01(Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy)) / radius);
                    pixels[y * w + x] = Color.Lerp(inner, outer, t);
                }
            }
            _gradientTexture.SetPixels32(pixels);
            _gradientTexture.Apply(false);
            _gradient.texture = _gradientTexture;
        }

        private static Texture2D GridTile()
        {
            if (_gridTile != null) return _gridTile;
            const int size = 36;
            _gridTile = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point,
                name = "NeonBackgroundGrid"
            };
            var pixels = new Color32[size * size];
            var line = new Color32(255, 255, 255, 255);
            var clear = new Color32(255, 255, 255, 0);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    pixels[y * size + x] = x == 0 || y == size - 1 ? line : clear;
                }
            }
            _gridTile.SetPixels32(pixels);
            _gridTile.Apply(false);
            return _gridTile;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _gridTile = null;
    }
}
