using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Gamespot.Core
{
    /// <summary>Escritura JSON mínima para los archivos que genera la app o el Editor (textos de localización).</summary>
    public static class JsonWriter
    {
        /// <summary>Objeto plano clave → texto, una clave por línea y en el orden recibido.</summary>
        public static string WriteStringMap(IEnumerable<KeyValuePair<string, string>> entries)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            var first = true;
            foreach (var entry in entries)
            {
                if (!first) sb.Append(",\n");
                first = false;
                sb.Append("  ");
                AppendString(sb, entry.Key);
                sb.Append(": ");
                AppendString(sb, entry.Value);
            }
            sb.Append(first ? "}\n" : "\n}\n");
            return sb.ToString();
        }

        /// <summary>Cadena JSON entre comillas con los escapes necesarios (las tildes se dejan tal cual).</summary>
        public static void AppendString(StringBuilder sb, string value)
        {
            sb.Append('"');
            foreach (var c in value ?? string.Empty)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }
    }
}
