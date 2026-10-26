using Gamespot.Core.Runtime.Theme;
using NUnit.Framework;
using UnityEngine;

namespace Gamespot.Tests.EditMode.Theme
{
    /// <summary>
    /// V062: contraste WCAG 2.1 de los colores de texto de la paleta sobre los fondos de la app. Texto normal, 4,5:1
    /// (AA); etiquetas de botón (18 en negrita, texto grande), 3:1. Los colores de la paleta son sRGB.
    /// </summary>
    public sealed class ContrastTests
    {
        private static readonly Color[] Backgrounds = { Palette.Background, Palette.Surface, Palette.SurfaceHigh, Palette.SurfaceHighest };

        // Colores con los que se escribe texto de cualquier tamaño.
        private static readonly (string name, Color color)[] TextColors =
        {
            ("TextPrimary", Palette.TextPrimary), ("TextMuted", Palette.TextMuted), ("NeonCyan", Palette.NeonCyan),
            ("NeonMagenta", Palette.NeonMagenta), ("NeonVioletText", Palette.NeonVioletText), ("NeonError", Palette.NeonError),
            ("NeonBlue", Palette.NeonBlue), ("NeonGreen", Palette.NeonGreen), ("NeonYellow", Palette.NeonYellow)
        };

        // Acentos de botón: en los rellenos la etiqueta va en OnPrimary; en los de borde, del color del acento.
        private static readonly (string name, Color color)[] ButtonAccents =
        {
            ("NeonCyan", Palette.NeonCyan), ("NeonMagenta", Palette.NeonMagenta), ("NeonViolet", Palette.NeonViolet),
            ("NeonError", Palette.NeonError), ("NeonGreen", Palette.NeonGreen), ("NeonRed", Palette.NeonRed)
        };

        private static float Luminance(Color c)
        {
            float Channel(float v) => v <= 0.04045f ? v / 12.92f : Mathf.Pow((v + 0.055f) / 1.055f, 2.4f);
            return 0.2126f * Channel(c.r) + 0.7152f * Channel(c.g) + 0.0722f * Channel(c.b);
        }

        public static float Ratio(Color a, Color b)
        {
            var la = Luminance(a);
            var lb = Luminance(b);
            return (Mathf.Max(la, lb) + 0.05f) / (Mathf.Min(la, lb) + 0.05f);
        }

        [Test]
        public void ElTextoSeLeeSobreTodosLosFondos()
        {
            foreach (var (name, color) in TextColors)
            {
                foreach (var background in Backgrounds)
                {
                    Assert.That(Ratio(color, background), Is.GreaterThanOrEqualTo(4.5f), $"{name} sobre #{ColorUtility.ToHtmlStringRGB(background)}");
                }
            }
        }

        [Test]
        public void LasEtiquetasDeLosBotonesSeLeen()
        {
            foreach (var (name, accent) in ButtonAccents)
            {
                Assert.That(Ratio(Palette.OnPrimary, accent), Is.GreaterThanOrEqualTo(3f), $"Botón relleno {name}");
                Assert.That(Ratio(accent, Palette.Surface), Is.GreaterThanOrEqualTo(3f), $"Botón de borde {name}");
            }
        }
    }
}
