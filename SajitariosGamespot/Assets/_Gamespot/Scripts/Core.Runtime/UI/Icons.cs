using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Glifos de <see cref="IconId"/>. Con <c>Resources/Fonts/MaterialIcons SDF</c> (V014, la fuente de iconos de la
    /// app Flutter) se usan sus puntos de código; si faltara, un sustituto ASCII legible.
    /// Los palos de la carta usan U+2660–U+2666 de la fuente de texto (glifos, no emojis).
    /// </summary>
    public static class Icons
    {
        public const string IconFontResource = "Fonts/MaterialIcons SDF";

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
            [IconId.Cancel] = 0xE5C9,
            [IconId.Category] = 0xE574,
            [IconId.FilterList] = 0xE152,
            [IconId.Settings] = 0xE8B8,
            // Juegos de la fase 13 (V071-V081).
            [IconId.Whatshot] = 0xE80E,
            [IconId.TouchApp] = 0xE913,
            [IconId.AltRoute] = 0xF184,
            [IconId.SignLanguage] = 0xEBE5,
            [IconId.PsychologyAlt] = 0xF8EA,
            [IconId.NightsStay] = 0xEA46,
            [IconId.Flag] = 0xE153,
            [IconId.Pets] = 0xE91D,
            [IconId.Science] = 0xEA4B,
            [IconId.GpsFixed] = 0xE1B3,
            [IconId.Favorite] = 0xE87D,
            [IconId.Shield] = 0xE9E0,
            [IconId.Star] = 0xE838,
            [IconId.WbSunny] = 0xE430,
            [IconId.Bedtime] = 0xEF44,
            [IconId.PersonOff] = 0xE510,
            [IconId.Gavel] = 0xE90E,
            [IconId.Undo] = 0xE166,
            [IconId.PlayArrow] = 0xE037,
            [IconId.Pause] = 0xE034,
            [IconId.ArrowUpward] = 0xE5D8,
            [IconId.ArrowDownward] = 0xE5DB,
            [IconId.ScreenRotation] = 0xE1C1,
            [IconId.Bolt] = 0xEA0B,
            [IconId.RecordVoiceOver] = 0xE91F,
            [IconId.Masks] = 0xF218,
            [IconId.HeartBroken] = 0xEAC2,
            [IconId.Face] = 0xE87C,
            [IconId.MilitaryTech] = 0xEA3F,
            [IconId.MusicNote] = 0xE405,
            [IconId.AutoFixHigh] = 0xE663,
            [IconId.RemoveRedEye] = 0xE417,
            [IconId.Elderly] = 0xF21A,
            [IconId.SwapHoriz] = 0xE8D4,
            [IconId.Forest] = 0xEA99,
            [IconId.Campaign] = 0xEF49,
            [IconId.Dangerous] = 0xE99A,
            [IconId.Groups] = 0xF233,
            [IconId.CompareArrows] = 0xE915,
            [IconId.HourglassTop] = 0xEA5B,
            [IconId.ChildCare] = 0xEB41,
            [IconId.ThumbUp] = 0xE8DC,
            [IconId.ThumbDown] = 0xE8DB,
            [IconId.Savings] = 0xE2EB,
            [IconId.LocalBar] = 0xE540
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
            [IconId.Cancel] = "x",
            [IconId.Category] = "#",
            [IconId.FilterList] = "=",
            [IconId.Settings] = "*",
            [IconId.Whatshot] = "*",
            [IconId.TouchApp] = "!",
            [IconId.AltRoute] = "Y",
            [IconId.SignLanguage] = "~",
            [IconId.PsychologyAlt] = "?",
            [IconId.NightsStay] = "C",
            [IconId.Flag] = "F",
            [IconId.Pets] = "W",
            [IconId.Science] = "&",
            [IconId.GpsFixed] = "+",
            [IconId.Favorite] = "<3",
            [IconId.Shield] = "U",
            [IconId.Star] = "*",
            [IconId.WbSunny] = "O",
            [IconId.Bedtime] = "C",
            [IconId.PersonOff] = "x",
            [IconId.Gavel] = "T",
            [IconId.Undo] = "<-",
            [IconId.PlayArrow] = ">",
            [IconId.Pause] = "||",
            [IconId.ArrowUpward] = "^",
            [IconId.ArrowDownward] = "v",
            [IconId.ScreenRotation] = "@",
            [IconId.Bolt] = "!",
            [IconId.RecordVoiceOver] = "\"",
            [IconId.Masks] = "M",
            [IconId.HeartBroken] = "</3",
            [IconId.Face] = "o",
            [IconId.MilitaryTech] = "*",
            [IconId.MusicNote] = "d",
            [IconId.AutoFixHigh] = "*",
            [IconId.RemoveRedEye] = "o",
            [IconId.Elderly] = "A",
            [IconId.SwapHoriz] = "<>",
            [IconId.Forest] = "^",
            [IconId.Campaign] = "<|",
            [IconId.Dangerous] = "X",
            [IconId.Groups] = "ooo",
            [IconId.CompareArrows] = "<>",
            [IconId.HourglassTop] = "8",
            [IconId.ChildCare] = "o",
            [IconId.ThumbUp] = "+1",
            [IconId.ThumbDown] = "-1",
            [IconId.Savings] = "$",
            [IconId.LocalBar] = "Y"
        };

        /// <summary>Todos los puntos de código que usa la app (para crear el atlas de la fuente de iconos).</summary>
        public static IEnumerable<int> AllCodepoints => Codepoints.Values;

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
