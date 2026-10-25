using System;
using System.Collections.Generic;
using System.Globalization;
using Gamespot.Core;
using UnityEngine;

namespace Gamespot.Core.Runtime.Localization
{
    /// <summary>
    /// Textos de la app en español (por defecto) e inglés (port de <c>AppLocalizations</c> + <c>LocaleController</c>).
    /// Los textos están en <c>Resources/Localization/{es,en}.json</c> (clave → mensaje ICU). La preferencia del
    /// usuario se guarda en <c>PlayerPrefs["app_locale"]</c>; sin valor se sigue el idioma del sistema.
    /// Una clave que falta en inglés cae al español; una que no existe se muestra como <c>[clave]</c>.
    /// </summary>
    public static class Loc
    {
        public const string PreferenceKey = "app_locale";
        public const string Spanish = "es";
        public const string English = "en";
        public const string ResourcesFolder = "Localization";

        /// <summary>Idiomas soportados, en el orden del selector.</summary>
        public static readonly IReadOnlyList<string> Supported = new[] { Spanish, English };

        private static Dictionary<string, string> _table;
        private static Dictionary<string, string> _fallback;
        private static readonly HashSet<string> MissingReported = new HashSet<string>();

        /// <summary>Idioma activo ("es" o "en").</summary>
        public static string Language { get; private set; } = Spanish;

        /// <summary>Cultura para fechas y números del idioma activo.</summary>
        public static CultureInfo Culture => Language == English ? CultureInfo.GetCultureInfo("en-US") : CultureInfo.GetCultureInfo("es-ES");

        /// <summary>Se lanza al cambiar de idioma (las pantallas se reconstruyen).</summary>
        public static event Action LanguageChanged;

        /// <summary>Preferencia guardada: "es", "en" o nulo (idioma del sistema).</summary>
        public static string Preference
        {
            get
            {
                var value = PlayerPrefs.GetString(PreferenceKey, string.Empty);
                return IsSupported(value) ? value : null;
            }
        }

        /// <summary>Carga los textos según la preferencia guardada (o el sistema).</summary>
        public static void Initialize()
        {
            var saved = PlayerPrefs.GetString(PreferenceKey, string.Empty);
            if (saved.Length > 0 && !IsSupported(saved))
            {
                // Un código guardado que ya no se soporta se borra (como el Flutter).
                PlayerPrefs.DeleteKey(PreferenceKey);
                PlayerPrefs.Save();
            }
            Apply(Preference ?? SystemLanguage());
        }

        /// <summary>Cambia el idioma: "es", "en" o nulo para seguir al sistema. Se guarda al momento.</summary>
        public static void SetPreference(string language)
        {
            if (language == null)
            {
                PlayerPrefs.DeleteKey(PreferenceKey);
            }
            else
            {
                if (!IsSupported(language)) throw new ArgumentException($"Idioma no soportado: {language}", nameof(language));
                PlayerPrefs.SetString(PreferenceKey, language);
            }
            PlayerPrefs.Save();
            Apply(language ?? SystemLanguage());
        }

        /// <summary>Texto de <paramref name="key"/> sin argumentos.</summary>
        public static string T(string key) => Format(key, null);

        /// <summary>Texto de <paramref name="key"/> con argumentos con nombre: <c>Loc.T("jugadorNumero", ("n", 3))</c>.</summary>
        public static string T(string key, params (string name, object value)[] args)
        {
            if (args == null || args.Length == 0) return Format(key, null);
            var map = new Dictionary<string, object>(args.Length);
            foreach (var (name, value) in args) map[name] = value;
            return Format(key, map);
        }

        /// <summary>Verdadero si existe la clave en el idioma activo o en español.</summary>
        public static bool Has(string key)
        {
            EnsureLoaded();
            return _table.ContainsKey(key) || _fallback.ContainsKey(key);
        }

        /// <summary>Claves del idioma <paramref name="language"/> (para los tests de paridad).</summary>
        public static IReadOnlyCollection<string> KeysOf(string language) => LoadTable(language).Keys;

        /// <summary>Idioma del sistema si está soportado; si no, español.</summary>
        public static string SystemLanguage() => Application.systemLanguage == UnityEngine.SystemLanguage.English ? English : Spanish;

        private static bool IsSupported(string language) => language == Spanish || language == English;

        private static string Format(string key, IReadOnlyDictionary<string, object> args)
        {
            EnsureLoaded();
            if (!_table.TryGetValue(key, out var pattern) && !_fallback.TryGetValue(key, out pattern))
            {
                if (MissingReported.Add(key)) Debug.LogWarning($"[Loc] Falta la clave '{key}'");
                return $"[{key}]";
            }
            return IcuMessageFormatter.Format(pattern, args);
        }

        private static void EnsureLoaded()
        {
            if (_table == null) Apply(Preference ?? SystemLanguage());
        }

        private static void Apply(string language)
        {
            var changed = _table != null && language != Language;
            Language = language;
            _fallback = LoadTable(Spanish);
            _table = language == Spanish ? _fallback : LoadTable(language);
            if (changed) LanguageChanged?.Invoke();
        }

        private static Dictionary<string, string> LoadTable(string language)
        {
            var asset = Resources.Load<TextAsset>($"{ResourcesFolder}/{language}");
            var table = new Dictionary<string, string>();
            if (asset == null)
            {
                Debug.LogError($"[Loc] No existe Resources/{ResourcesFolder}/{language}.json");
                return table;
            }
            foreach (var pair in Json.ParseObject(asset.text))
            {
                if (pair.Value is string text) table[pair.Key] = text;
            }
            return table;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _table = null;
            _fallback = null;
            Language = Spanish;
            LanguageChanged = null;
            MissingReported.Clear();
        }
    }
}
