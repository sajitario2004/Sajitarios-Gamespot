using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Glifos de <see cref="IconId"/>. Si existe <c>Resources/Fonts/MaterialSymbols SDF</c> se usan los
    /// puntos de código de Material Symbols; si no, un sustituto ASCII legible (provisional, V014).
    /// Los palos de la carta usan U+2660–U+2666 de la fuente de texto (glifos, no emojis).
    /// </summary>
    public static class Icons
    {
        public const string IconFontResource = "Fonts/MaterialSymbols SDF";

        private static TMP_FontAsset _iconFont;
        private static bool _searched;

        private static readonly Dictionary<IconId, int> Codepoints = new Dictionary<IconId, int>
        {
            [IconId.Home] = 0xE88A,
            [IconId.Help] = 0xE8FD,
            [IconId.Language] = 0xE894,
            [IconId.VolumeUp] = 0xE050,
            [IconId.VolumeOff] = 0xE04F,
            [IconId.History] = 0xE889,
            [IconId.MenuBook] = 0xEA19,
            [IconId.Add] = 0xE145,
            [IconId.Remove] = 0xE15B,
            [IconId.RemoveCircle] = 0xE15D,
            [IconId.PersonAdd] = 0xE7FE,
            [IconId.Casino] = 0xEB40,
            [IconId.Style] = 0xE41D,
            [IconId.TheaterComedy] = 0xEA66,
            [IconId.Quiz] = 0xF04C,
            [IconId.Tune] = 0xE429,
            [IconId.DoNotDisturb] = 0xE644,
            [IconId.FrontHand] = 0xE769,
            [IconId.Fire] = 0xEF55,
            [IconId.Trophy] = 0xEA23,
            [IconId.Phone] = 0xE324,
            [IconId.Check] = 0xE5CA,
            [IconId.Lock] = 0xE897,
            [IconId.Edit] = 0xE3C9,
            [IconId.Delete] = 0xE872,
            [IconId.Search] = 0xE8B6,
            [IconId.Close] = 0xE5CD,
            [IconId.Block] = 0xE14B,
            [IconId.Vote] = 0xE175,
            [IconId.Lightbulb] = 0xE0F0,
            [IconId.Warning] = 0xE002,
            [IconId.SentimentDissatisfied] = 0xE811,
            [IconId.ArrowForward] = 0xE5C8,
            [IconId.Refresh] = 0xE5D5,
            [IconId.Timer] = 0xE425,
            [IconId.SkipNext] = 0xE044,
            [IconId.Group] = 0xE7EF,
            [IconId.Error] = 0xE000,
            [IconId.VideogameAsset] = 0xE338,
            [IconId.Visibility] = 0xE8F4,
            [IconId.CheckCircle] = 0xE86C,
            [IconId.Cancel] = 0xE5C9
        };

        private static readonly Dictionary<IconId, string> Fallbacks = new Dictionary<IconId, string>
        {
            [IconId.Home] = "<",
            [IconId.Help] = "?",
            [IconId.Language] = "Aa",
            [IconId.VolumeUp] = "))",
            [IconId.VolumeOff] = "x",
            [IconId.History] = "H",
            [IconId.MenuBook] = "Ab",
            [IconId.Add] = "+",
            [IconId.Remove] = "-",
            [IconId.RemoveCircle] = "-",
            [IconId.PersonAdd] = "+",
            [IconId.Casino] = "::",
            [IconId.Style] = "A",
            [IconId.TheaterComedy] = "?!",
            [IconId.Quiz] = "Q",
            [IconId.Tune] = "~",
            [IconId.DoNotDisturb] = "(-)",
            [IconId.FrontHand] = "1",
            [IconId.Fire] = "*",
            [IconId.Trophy] = "#1",
            [IconId.Phone] = "[ ]",
            [IconId.Check] = "v",
            [IconId.Lock] = "L",
            [IconId.Edit] = "/",
            [IconId.Delete] = "x",
            [IconId.Search] = "o",
            [IconId.Close] = "x",
            [IconId.Block] = "(/)",
            [IconId.Vote] = "v",
            [IconId.Lightbulb] = "i",
            [IconId.Warning] = "!",
            [IconId.SentimentDissatisfied] = ":(",
            [IconId.ArrowForward] = ">",
            [IconId.Refresh] = "@",
            [IconId.Timer] = "T",
            [IconId.SkipNext] = ">>",
            [IconId.Group] = "oo",
            [IconId.Error] = "!",
            [IconId.VideogameAsset] = "[+]",
            [IconId.Visibility] = "o",
            [IconId.CheckCircle] = "v",
            [IconId.Cancel] = "x"
        };

        /// <summary>Fuente de iconos, o nula si aún no está en el proyecto.</summary>
        public static TMP_FontAsset IconFont
        {
            get
            {
                if (!_searched)
                {
                    _searched = true;
                    _iconFont = Resources.Load<TMP_FontAsset>(IconFontResource);
                }
                return _iconFont;
            }
        }

        /// <summary>Texto a pintar y si va con la fuente de iconos.</summary>
        public static (string text, bool usesIconFont) Glyph(IconId id)
        {
            switch (id)
            {
                case IconId.SuitSpade: return ("♠", false);
                case IconId.SuitHeart: return ("♥", false);
                case IconId.SuitDiamond: return ("♦", false);
                case IconId.SuitClub: return ("♣", false);
            }
            var font = IconFont;
            if (font != null && Codepoints.TryGetValue(id, out var codepoint) && font.HasCharacter((char)codepoint, true, true))
            {
                return (char.ConvertFromUtf32(codepoint), true);
            }
            return (Fallbacks.TryGetValue(id, out var fallback) ? fallback : "?", false);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _iconFont = null;
            _searched = false;
        }
    }
}
