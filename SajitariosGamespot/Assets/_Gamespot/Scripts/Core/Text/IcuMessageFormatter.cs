using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Gamespot.Core
{
    /// <summary>
    /// Formateador del subconjunto de ICU MessageFormat que usan los ARB del Flutter:
    /// <c>{nombre}</c> y <c>{n, plural, =0{…} one{…} other{…}}</c> (con <c>#</c> = el número y
    /// placeholders anidados). Las reglas de plural son las de español e inglés: <c>one</c> si n = 1.
    /// </summary>
    public static class IcuMessageFormatter
    {
        /// <summary>Sustituye los argumentos en <paramref name="pattern"/>. Un argumento que falta se deja tal cual.</summary>
        public static string Format(string pattern, IReadOnlyDictionary<string, object> args)
        {
            if (string.IsNullOrEmpty(pattern)) return pattern ?? string.Empty;
            var sb = new StringBuilder(pattern.Length + 16);
            AppendFormatted(sb, pattern, args, null);
            return sb.ToString();
        }

        private static void AppendFormatted(StringBuilder sb, string pattern, IReadOnlyDictionary<string, object> args, string pluralValue)
        {
            var i = 0;
            while (i < pattern.Length)
            {
                var c = pattern[i];
                if (c == '#' && pluralValue != null)
                {
                    sb.Append(pluralValue);
                    i++;
                    continue;
                }
                if (c != '{')
                {
                    sb.Append(c);
                    i++;
                    continue;
                }

                var end = FindMatchingBrace(pattern, i);
                if (end < 0)
                {
                    // Llave sin cerrar: se copia literal.
                    sb.Append(pattern, i, pattern.Length - i);
                    return;
                }
                var inner = pattern.Substring(i + 1, end - i - 1);
                AppendArgument(sb, inner, args, pattern.Substring(i, end - i + 1));
                i = end + 1;
            }
        }

        private static void AppendArgument(StringBuilder sb, string inner, IReadOnlyDictionary<string, object> args, string original)
        {
            var firstComma = inner.IndexOf(',');
            if (firstComma < 0)
            {
                var name = inner.Trim();
                if (args != null && args.TryGetValue(name, out var value)) sb.Append(ToText(value));
                else sb.Append(original);
                return;
            }

            var argName = inner.Substring(0, firstComma).Trim();
            var rest = inner.Substring(firstComma + 1);
            var secondComma = rest.IndexOf(',');
            var type = (secondComma < 0 ? rest : rest.Substring(0, secondComma)).Trim();
            if (type != "plural" || secondComma < 0 || args == null || !args.TryGetValue(argName, out var raw))
            {
                sb.Append(original);
                return;
            }

            var number = ToNumber(raw);
            var branches = ParseBranches(rest.Substring(secondComma + 1));
            var exact = "=" + number.ToString(CultureInfo.InvariantCulture);
            string chosen;
            if (!branches.TryGetValue(exact, out chosen))
            {
                var category = number == 1 ? "one" : "other";
                if (!branches.TryGetValue(category, out chosen) && !branches.TryGetValue("other", out chosen))
                {
                    sb.Append(original);
                    return;
                }
            }
            AppendFormatted(sb, chosen, args, ToText(raw));
        }

        /// <summary>Lee pares <c>selector{texto}</c> (el texto puede llevar llaves anidadas).</summary>
        private static Dictionary<string, string> ParseBranches(string text)
        {
            var result = new Dictionary<string, string>();
            var i = 0;
            while (i < text.Length)
            {
                while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
                var selectorStart = i;
                while (i < text.Length && text[i] != '{' && !char.IsWhiteSpace(text[i])) i++;
                var selector = text.Substring(selectorStart, i - selectorStart);
                while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
                if (i >= text.Length || text[i] != '{') break;
                var end = FindMatchingBrace(text, i);
                if (end < 0) break;
                if (selector.Length > 0) result[selector] = text.Substring(i + 1, end - i - 1);
                i = end + 1;
            }
            return result;
        }

        private static int FindMatchingBrace(string text, int open)
        {
            var depth = 0;
            for (var i = open; i < text.Length; i++)
            {
                if (text[i] == '{') depth++;
                else if (text[i] == '}' && --depth == 0) return i;
            }
            return -1;
        }

        private static long ToNumber(object value)
        {
            switch (value)
            {
                case int v: return v;
                case long v: return v;
                case double v: return (long)Math.Round(v);
                case float v: return (long)Math.Round(v);
                case string s when long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed): return parsed;
                default: return Convert.ToInt64(value, CultureInfo.InvariantCulture);
            }
        }

        private static string ToText(object value)
        {
            switch (value)
            {
                case null: return string.Empty;
                case string s: return s;
                case IFormattable f: return f.ToString(null, CultureInfo.InvariantCulture);
                default: return value.ToString();
            }
        }
    }
}
