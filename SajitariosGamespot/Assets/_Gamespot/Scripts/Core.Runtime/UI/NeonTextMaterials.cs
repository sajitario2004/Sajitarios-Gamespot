using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Materiales de TextMeshPro con halo neón (port de <c>neonTextShadows</c>): usa el <i>underlay</i>
    /// suavizado del shader móvil de distancia, sin desplazamiento, del color del brillo. Se comparten por
    /// (fuente, color, intensidad) para no crear un material por texto.
    /// </summary>
    public static class NeonTextMaterials
    {
        private static readonly Dictionary<(int, Color32, int), Material> Cache = new Dictionary<(int, Color32, int), Material>();

        /// <summary>Material con halo de <paramref name="glow"/> a la intensidad dada (0 = sin halo).</summary>
        public static Material Get(TMP_FontAsset font, Color glow, float intensity)
        {
            if (font == null) return null;
            if (intensity <= 0.01f) return font.material;
            var bucket = Mathf.RoundToInt(intensity * 10f);
            var key = (font.GetInstanceID(), (Color32)glow, bucket);
            if (Cache.TryGetValue(key, out var material) && material != null) return material;

            material = new Material(font.material) { name = $"{font.name} Neon {ColorUtility.ToHtmlStringRGB(glow)} x{bucket}" };
            var strength = Mathf.Clamp01(intensity);
            material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(glow.r, glow.g, glow.b, 0.75f * strength));
            material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, Mathf.Lerp(0.35f, 0.8f, Mathf.Clamp01(intensity / 1.5f)));
            material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.25f);
            material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0f);
            material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, 0f);
            Cache[key] = material;
            return material;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Cache.Clear();
    }
}
