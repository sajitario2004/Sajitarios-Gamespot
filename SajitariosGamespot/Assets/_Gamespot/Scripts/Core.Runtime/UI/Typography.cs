using TMPro;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>Escala tipográfica de Material 3 (la que usaba el Flutter), en unidades del Canvas.</summary>
    public enum TextRole
    {
        DisplayLarge,
        DisplayMedium,
        DisplaySmall,
        HeadlineLarge,
        HeadlineMedium,
        HeadlineSmall,
        TitleLarge,
        TitleMedium,
        TitleSmall,
        BodyLarge,
        BodyMedium,
        BodySmall,
        LabelLarge,
        LabelMedium,
        LabelSmall
    }

    /// <summary>Tamaños y pesos de cada <see cref="TextRole"/>.</summary>
    public static class Typography
    {
        /// <summary>
        /// Puntos suspensivos si se pueden pintar; si no, corte simple. TextMeshPro no encuentra "…" en un texto
        /// en negrita cuando la fuente no tiene variante bold real (LiberationSans solo trae la regular) y avisa
        /// en consola cada vez.
        /// </summary>
        public static void SetEllipsis(TMP_Text text)
        {
            var font = text.font != null ? text.font : TMP_Settings.defaultFontAsset;
            var bold = (text.fontStyle & FontStyles.Bold) == FontStyles.Bold;
            var hasBoldTypeface = font != null && font.fontWeightTable != null && font.fontWeightTable.Length > 7
                                  && font.fontWeightTable[7].regularTypeface != null;
            text.overflowMode = !bold || hasBoldTypeface ? TextOverflowModes.Ellipsis : TextOverflowModes.Truncate;
        }

        public static float Size(TextRole role)
        {
            switch (role)
            {
                case TextRole.DisplayLarge: return 57f;
                case TextRole.DisplayMedium: return 45f;
                case TextRole.DisplaySmall: return 36f;
                case TextRole.HeadlineLarge: return 32f;
                case TextRole.HeadlineMedium: return 28f;
                case TextRole.HeadlineSmall: return 24f;
                case TextRole.TitleLarge: return 22f;
                case TextRole.TitleMedium: return 16f;
                case TextRole.TitleSmall: return 14f;
                case TextRole.BodyLarge: return 16f;
                case TextRole.BodyMedium: return 14f;
                case TextRole.BodySmall: return 12f;
                case TextRole.LabelLarge: return 14f;
                case TextRole.LabelMedium: return 12f;
                case TextRole.LabelSmall: return 11f;
                default: return 14f;
            }
        }

        /// <summary>El tema del Flutter ponía en negrita los display, headline, titleLarge y labelLarge.</summary>
        public static bool IsBold(TextRole role)
        {
            switch (role)
            {
                case TextRole.DisplayLarge:
                case TextRole.DisplayMedium:
                case TextRole.DisplaySmall:
                case TextRole.HeadlineLarge:
                case TextRole.HeadlineMedium:
                case TextRole.HeadlineSmall:
                case TextRole.TitleLarge:
                case TextRole.LabelLarge:
                    return true;
                default:
                    return false;
            }
        }
    }
}
