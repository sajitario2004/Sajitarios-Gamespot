using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Gamespot.Core
{
    /// <summary>
    /// Lector JSON mínimo y sin dependencias para los seeds y los textos de localización
    /// (<c>JsonUtility</c> de Unity no lee diccionarios ni listas sueltas).
    /// Devuelve <see cref="Dictionary{TKey,TValue}"/> (objetos), <see cref="List{T}"/> (listas),
    /// <see cref="string"/>, <see cref="double"/>, <see cref="bool"/> o <c>null</c>.
    /// </summary>
    public static class Json
    {
        /// <summary>Lee un documento JSON completo. Lanza <see cref="FormatException"/> si no es válido.</summary>
        public static object Parse(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            var reader = new Reader(text);
            reader.SkipWhitespace();
            var value = reader.ReadValue();
            reader.SkipWhitespace();
            if (!reader.AtEnd) throw reader.Error("Hay texto de más después del valor JSON");
            return value;
        }

        /// <summary>Lee un objeto JSON. Lanza si la raíz no es un objeto.</summary>
        public static Dictionary<string, object> ParseObject(string text) =>
            Parse(text) as Dictionary<string, object> ?? throw new FormatException("La raíz del JSON no es un objeto");

        /// <summary>Lee una lista JSON. Lanza si la raíz no es una lista.</summary>
        public static List<object> ParseArray(string text) =>
            Parse(text) as List<object> ?? throw new FormatException("La raíz del JSON no es una lista");

        private sealed class Reader
        {
            private readonly string _s;
            private int _i;

            public Reader(string s) => _s = s;

            public bool AtEnd => _i >= _s.Length;

            public FormatException Error(string message) => new FormatException($"{message} (posición {_i})");

            public void SkipWhitespace()
            {
                while (!AtEnd && char.IsWhiteSpace(_s[_i])) _i++;
            }

            public object ReadValue()
            {
                if (AtEnd) throw Error("Se esperaba un valor");
                var c = _s[_i];
                switch (c)
                {
                    case '{': return ReadObject();
                    case '[': return ReadArray();
                    case '"': return ReadString();
                    case 't': Expect("true"); return true;
                    case 'f': Expect("false"); return false;
                    case 'n': Expect("null"); return null;
                    default:
                        if (c == '-' || (c >= '0' && c <= '9')) return ReadNumber();
                        throw Error($"Carácter inesperado '{c}'");
                }
            }

            private Dictionary<string, object> ReadObject()
            {
                var result = new Dictionary<string, object>();
                _i++; // {
                SkipWhitespace();
                if (Peek('}')) { _i++; return result; }
                while (true)
                {
                    SkipWhitespace();
                    if (AtEnd || _s[_i] != '"') throw Error("Se esperaba una clave entre comillas");
                    var key = ReadString();
                    SkipWhitespace();
                    if (!Peek(':')) throw Error("Se esperaba ':'");
                    _i++;
                    SkipWhitespace();
                    result[key] = ReadValue();
                    SkipWhitespace();
                    if (Peek(',')) { _i++; continue; }
                    if (Peek('}')) { _i++; return result; }
                    throw Error("Se esperaba ',' o '}'");
                }
            }

            private List<object> ReadArray()
            {
                var result = new List<object>();
                _i++; // [
                SkipWhitespace();
                if (Peek(']')) { _i++; return result; }
                while (true)
                {
                    SkipWhitespace();
                    result.Add(ReadValue());
                    SkipWhitespace();
                    if (Peek(',')) { _i++; continue; }
                    if (Peek(']')) { _i++; return result; }
                    throw Error("Se esperaba ',' o ']'");
                }
            }

            private string ReadString()
            {
                _i++; // "
                var sb = new StringBuilder();
                while (true)
                {
                    if (AtEnd) throw Error("Cadena sin cerrar");
                    var c = _s[_i++];
                    if (c == '"') return sb.ToString();
                    if (c != '\\')
                    {
                        sb.Append(c);
                        continue;
                    }
                    if (AtEnd) throw Error("Escape incompleto");
                    var e = _s[_i++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (_i + 4 > _s.Length) throw Error("Escape \\u incompleto");
                            sb.Append((char)int.Parse(_s.Substring(_i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            _i += 4;
                            break;
                        default: throw Error($"Escape desconocido '\\{e}'");
                    }
                }
            }

            private double ReadNumber()
            {
                var start = _i;
                while (!AtEnd && "+-0123456789.eE".IndexOf(_s[_i]) >= 0) _i++;
                var token = _s.Substring(start, _i - start);
                if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    throw Error($"Número no válido '{token}'");
                }
                return value;
            }

            private void Expect(string word)
            {
                if (string.CompareOrdinal(_s, _i, word, 0, word.Length) != 0) throw Error($"Se esperaba '{word}'");
                _i += word.Length;
            }

            private bool Peek(char c) => !AtEnd && _s[_i] == c;
        }
    }
}
