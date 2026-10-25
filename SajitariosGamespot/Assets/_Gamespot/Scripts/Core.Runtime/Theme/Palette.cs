using UnityEngine;

namespace Gamespot.Core.Runtime.Theme
{
    /// <summary>
    /// Paleta neón de la app (port de <c>AppTheme</c> del Flutter). Ningún color suelto en el código:
    /// todo sale de aquí.
    /// </summary>
    public static class Palette
    {
        // Fondos y superficies
        public static readonly Color Background = Hex(0x0A0A14);
        public static readonly Color Surface = Hex(0x14141F);
        public static readonly Color SurfaceLow = Hex(0x11111B);
        public static readonly Color SurfaceHigh = Hex(0x1B1B2A);
        public static readonly Color SurfaceHighest = Hex(0x222234);

        // Neón
        public static readonly Color NeonCyan = Hex(0x00F0FF);
        public static readonly Color NeonMagenta = Hex(0xFF00E5);
        public static readonly Color NeonViolet = Hex(0xB026FF);
        public static readonly Color NeonVioletText = Hex(0xC77DFF);
        public static readonly Color NeonError = Hex(0xFF3D71);

        // Marcos de las respuestas de la trivia (A, B, C, D)
        public static readonly Color NeonBlue = Hex(0x00A8FF);
        public static readonly Color NeonGreen = Hex(0x00FF7F);
        public static readonly Color NeonRed = Hex(0xFF2C55);
        public static readonly Color NeonYellow = Hex(0xFFE600);

        // Texto
        public static readonly Color TextPrimary = Hex(0xE6F7FF);
        public static readonly Color TextMuted = Hex(0xA8B6C8);
        public static readonly Color OnPrimary = Hex(0x001417);
        public static readonly Color OnSecondary = Hex(0x1A0017);

        // Líneas y velos
        public static readonly Color Outline = Hex(0x3A3A55);
        public static readonly Color Divider = Hex(0x26263A);
        public static readonly Color Scrim = new Color(0f, 0f, 0f, 0.8f);

        /// <summary>Colores de los marcos de las 4 respuestas de la trivia, en orden A–D.</summary>
        public static readonly Color[] AnswerFrameColors = { NeonBlue, NeonGreen, NeonRed, NeonYellow };

        /// <summary>Color opaco a partir de 0xRRGGBB.</summary>
        public static Color Hex(uint rgb, float alpha = 1f)
        {
            return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, alpha);
        }

        /// <summary>El mismo color con otra opacidad.</summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        /// <summary>Mezcla <paramref name="top"/> con su alfa sobre <paramref name="bottom"/> (como <c>Color.alphaBlend</c>).</summary>
        public static Color AlphaBlend(Color top, Color bottom)
        {
            var a = top.a;
            return new Color(
                top.r * a + bottom.r * (1f - a),
                top.g * a + bottom.g * (1f - a),
                top.b * a + bottom.b * (1f - a),
                1f);
        }
    }
}
