using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gamespot.Core;
using UnityEditor;
using UnityEngine;

namespace Gamespot.Editor.Localization
{
    /// <summary>
    /// Pasa los ARB de la app Flutter (<c>Legacy/Flutter/lib/l10n/app_{es,en}.arb</c>) a
    /// <c>Resources/Localization/{es,en}.json</c> (menú <b>Gamespot → i18n → Importar ARB del Flutter</b>).
    /// Solo añade las claves que faltan: lo que ya está en el JSON (traducciones corregidas o claves
    /// nuevas de Unity) no se toca. Se quitan los metadatos <c>@clave</c>.
    /// </summary>
    public static class ArbImporter
    {
        public const string OutputFolder = "Assets/_Gamespot/Resources/Localization";
        private static readonly string ArbFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Legacy", "Flutter", "lib", "l10n"));

        [MenuItem("Gamespot/i18n/Importar ARB del Flutter", priority = 0)]
        public static void ImportAll()
        {
            var added = Import("es") + Import("en");
            AssetDatabase.Refresh();
            Debug.Log($"[Gamespot] ARB importados: {added} claves nuevas.");
        }

        /// <summary>Importa un idioma y devuelve cuántas claves ha añadido.</summary>
        public static int Import(string language)
        {
            var arbPath = Path.Combine(ArbFolder, $"app_{language}.arb");
            if (!File.Exists(arbPath)) throw new FileNotFoundException($"No existe {arbPath}");
            var arb = Json.ParseObject(File.ReadAllText(arbPath));

            Directory.CreateDirectory(OutputFolder);
            var outputPath = Path.Combine(OutputFolder, $"{language}.json");
            var entries = new List<KeyValuePair<string, string>>();
            var existing = new HashSet<string>();
            if (File.Exists(outputPath))
            {
                foreach (var pair in Json.ParseObject(File.ReadAllText(outputPath)))
                {
                    entries.Add(new KeyValuePair<string, string>(pair.Key, pair.Value as string ?? string.Empty));
                    existing.Add(pair.Key);
                }
            }

            var added = 0;
            foreach (var pair in arb.Where(p => !p.Key.StartsWith("@")))
            {
                if (existing.Contains(pair.Key) || !(pair.Value is string text)) continue;
                entries.Add(new KeyValuePair<string, string>(pair.Key, text));
                added++;
            }

            File.WriteAllText(outputPath, JsonWriter.WriteStringMap(entries), new System.Text.UTF8Encoding(false));
            return added;
        }
    }
}
