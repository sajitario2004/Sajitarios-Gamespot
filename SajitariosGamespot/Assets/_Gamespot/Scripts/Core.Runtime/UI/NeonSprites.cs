using System.Collections.Generic;
using UnityEngine;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Sprites blancos generados por código (sin archivos) para la UI: rectángulos redondeados, bordes,
    /// círculos y halos de brillo. Se tiñen con <c>Image.color</c> y se usan en modo <i>Sliced</i>.
    /// Se generan al doble de resolución (2 texels por unidad del Canvas) y se guardan en caché.
    /// </summary>
    public static class NeonSprites
    {
        private const int Scale = 2;
        private const float ReferencePixelsPerUnit = 100f;

        // Halo: dos sombras como el Flutter (desenfoque 12 y 28, opacidad 0.55 y 0.30).
        private const float GlowBlurNear = 12f;
        private const float GlowBlurFar = 28f;
        private const float GlowAlphaNear = 0.55f;
        private const float GlowAlphaFar = 0.30f;

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>Margen (unidades) que el halo sobresale del rectángulo.</summary>
        public static float GlowMargin => Mathf.Ceil(GlowBlurFar * 1.6f);

        /// <summary>Rectángulo relleno con esquinas de <paramref name="radius"/> unidades.</summary>
        public static Sprite RoundedRect(float radius) => Get($"fill:{radius}", () => BuildRounded(radius, 0f));

        /// <summary>Solo el borde, de <paramref name="thickness"/> unidades.</summary>
        public static Sprite RoundedBorder(float radius, float thickness) => Get($"ring:{radius}:{thickness}", () => BuildRounded(radius, thickness));

        /// <summary>Círculo relleno (se estira como rectángulo redondeado con radio = la mitad del lado).</summary>
        public static Sprite Circle(float diameter) => RoundedRect(diameter * 0.5f);

        /// <summary>Halo suave alrededor de un rectángulo de esquinas <paramref name="radius"/>.</summary>
        public static Sprite Glow(float radius) => Get($"glow:{radius}", () => BuildGlow(radius));

        private static Sprite Get(string key, System.Func<Sprite> build)
        {
            if (Cache.TryGetValue(key, out var sprite) && sprite != null) return sprite;
            sprite = build();
            Cache[key] = sprite;
            return sprite;
        }

        private static Sprite BuildRounded(float radius, float thickness)
        {
            var r = Mathf.Max(1, Mathf.CeilToInt(radius * Scale));
            var t = thickness * Scale;
            var size = r * 2 + 2;
            var pixels = new Color32[size * size];
            var half = size * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var sd = RoundedRectDistance(x + 0.5f - half, y + 0.5f - half, half, r);
                    var outer = Mathf.Clamp01(0.5f - sd);
                    var alpha = t > 0f ? outer - Mathf.Clamp01(0.5f - (sd + t)) : outer;
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f));
                }
            }
            return CreateSprite(pixels, size, r, thickness > 0f ? "NeonRing" : "NeonRounded");
        }

        private static Sprite BuildGlow(float radius)
        {
            var r = Mathf.Max(1, Mathf.CeilToInt(radius * Scale));
            var margin = Mathf.CeilToInt(GlowMargin * Scale);
            var size = (r + margin) * 2 + 2;
            var half = size * 0.5f;
            var innerHalf = half - margin;
            var sigmaNear = GlowBlurNear * 0.57735f * Scale;
            var sigmaFar = GlowBlurFar * 0.57735f * Scale;
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var sd = RoundedRectDistance(x + 0.5f - half, y + 0.5f - half, innerHalf, r);
                    var d = Mathf.Max(0f, sd);
                    var alpha = GlowAlphaNear * Mathf.Exp(-(d * d) / (2f * sigmaNear * sigmaNear))
                                + GlowAlphaFar * Mathf.Exp(-(d * d) / (2f * sigmaFar * sigmaFar));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f));
                }
            }
            return CreateSprite(pixels, size, r + margin, "NeonGlow");
        }

        /// <summary>Distancia con signo a un rectángulo redondeado centrado (negativa dentro).</summary>
        private static float RoundedRectDistance(float px, float py, float halfExtent, float radius)
        {
            var qx = Mathf.Abs(px) - (halfExtent - radius);
            var qy = Mathf.Abs(py) - (halfExtent - radius);
            var outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            var inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
            return outside + inside - radius;
        }

        private static Sprite CreateSprite(Color32[] pixels, int size, int border, string name)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = name
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                ReferencePixelsPerUnit * Scale, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
            sprite.name = name;
            return sprite;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Cache.Clear();
    }
}
