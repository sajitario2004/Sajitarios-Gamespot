using System.Collections.Generic;
using UnityEngine;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>Palos de la baraja francesa.</summary>
    public enum SuitShape
    {
        Spade,
        Heart,
        Diamond,
        Club
    }

    /// <summary>
    /// Palos de la carta dibujados por código como sprites blancos (se tiñen con <c>Image.color</c>). Nada de
    /// emojis ni de glifos de fuente: así se ven igual en todos los móviles. Se generan con suavizado a partir
    /// de formas simples (círculos, triángulos y rombos) y se guardan en caché.
    /// </summary>
    public static class SuitSprites
    {
        private const int Size = 128;
        private static readonly Dictionary<SuitShape, Sprite> Cache = new Dictionary<SuitShape, Sprite>();

        public static Sprite Get(SuitShape shape)
        {
            if (Cache.TryGetValue(shape, out var sprite) && sprite != null) return sprite;
            sprite = Build(shape);
            Cache[shape] = sprite;
            return sprite;
        }

        private static Sprite Build(SuitShape shape)
        {
            var pixels = new Color32[Size * Size];
            const int samples = 3;
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    // Supermuestreo 3×3 para bordes suaves.
                    var hits = 0;
                    for (var sy = 0; sy < samples; sy++)
                    {
                        for (var sx = 0; sx < samples; sx++)
                        {
                            var px = (x + (sx + 0.5f) / samples) / Size * 2f - 1f;
                            var py = (y + (sy + 0.5f) / samples) / Size * 2f - 1f;
                            if (Inside(shape, px, py)) hits++;
                        }
                    }
                    var alpha = (byte)Mathf.RoundToInt(255f * hits / (samples * samples));
                    pixels[y * Size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = $"Suit{shape}"
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = $"Suit{shape}";
            return sprite;
        }

        /// <summary>¿El punto (x, y) en [−1, 1] (y hacia arriba) está dentro del palo?</summary>
        private static bool Inside(SuitShape shape, float x, float y)
        {
            switch (shape)
            {
                case SuitShape.Diamond:
                    return Mathf.Abs(x) / 0.62f + Mathf.Abs(y) / 0.92f <= 1f;
                case SuitShape.Heart:
                    return Heart(x, y);
                case SuitShape.Spade:
                    // Corazón invertido y un poco más arriba, con su pie.
                    return Heart(x, -(y - 0.18f) * 1.05f) || Stem(x, y);
                case SuitShape.Club:
                    return Circle(x, y, 0f, 0.42f, 0.36f) || Circle(x, y, -0.4f, -0.08f, 0.36f) || Circle(x, y, 0.4f, -0.08f, 0.36f)
                           || Circle(x, y, 0f, 0.05f, 0.2f) || Stem(x, y);
                default:
                    return false;
            }
        }

        private static bool Heart(float x, float y)
        {
            // Dos lóbulos y un triángulo hacia abajo.
            if (Circle(x, y, -0.42f, 0.38f, 0.46f) || Circle(x, y, 0.42f, 0.38f, 0.46f)) return true;
            if (y > 0.42f || y < -0.92f) return false;
            var halfWidth = (y + 0.92f) / (0.42f + 0.92f) * 0.86f;
            return Mathf.Abs(x) <= halfWidth;
        }

        private static bool Stem(float x, float y)
        {
            if (y > 0f || y < -0.92f) return false;
            var halfWidth = Mathf.Lerp(0.36f, 0.08f, Mathf.InverseLerp(-0.92f, -0.35f, y));
            return Mathf.Abs(x) <= halfWidth && y < -0.2f;
        }

        private static bool Circle(float x, float y, float cx, float cy, float r) => (x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Cache.Clear();
    }
}
