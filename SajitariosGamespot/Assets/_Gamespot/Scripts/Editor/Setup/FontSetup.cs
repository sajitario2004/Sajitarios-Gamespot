using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gamespot.Core.Runtime.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Gamespot.Editor.Setup
{
    /// <summary>
    /// Fuentes de la app (V014, con permiso del autor): Roboto normal y negrita para el texto y Material Icons para
    /// los iconos, las mismas que usaba la app Flutter (Apache-2.0, en <c>Assets/_Gamespot/Fonts</c> con sus
    /// licencias). Crea los assets SDF de TextMeshPro en <c>Resources/Fonts</c> con todos los caracteres de los
    /// textos y los seeds ya en el atlas y los deja estáticos (así no cambian al jugar en el Editor); lo que falte
    /// (un carácter raro en una palabra propia) lo pinta LiberationSans, que queda de respaldo con su atlas dinámico.
    /// Roboto lleva su negrita real (tabla de pesos) y pasa a ser la fuente por defecto. Es idempotente.
    /// Menú: Gamespot → Configuración → Fuentes.
    /// </summary>
    public static class FontSetup
    {
        public const string SourceFolder = "Assets/_Gamespot/Fonts/";
        public const string AssetFolder = "Assets/_Gamespot/Resources/Fonts/";
        public const string TextAssetPath = AssetFolder + "Roboto SDF.asset";
        public const string BoldAssetPath = AssetFolder + "Roboto Bold SDF.asset";
        public const string IconAssetPath = AssetFolder + "MaterialIcons SDF.asset";
        public const string FallbackAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        private const int SamplingPointSize = 56;
        private const int AtlasPadding = 6;
        private const int TextAtlasSize = 1024;
        // 44 iconos caben de sobra en 512 × 512.
        private const int IconAtlasSize = 512;

        // Además de lo que sale en los textos y los seeds: ASCII, Latin-1 y la puntuación habitual del español.
        private const string Extra = "…«»¿¡–—‘’“”€•·×";

        [MenuItem("Gamespot/Configuración/Fuentes", priority = 15)]
        public static void Apply()
        {
            Directory.CreateDirectory(AssetFolder);
            var fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackAssetPath);
            var shader = fallback != null ? fallback.material.shader : Shader.Find("TextMeshPro/Mobile/Distance Field");
            var text = TextCharacters();
            var bold = CreateOrUpdate(SourceFolder + "Roboto-Bold.ttf", BoldAssetPath, text, shader, TextAtlasSize);
            var regular = CreateOrUpdate(SourceFolder + "Roboto-Regular.ttf", TextAssetPath, text, shader, TextAtlasSize);
            var icons = CreateOrUpdate(SourceFolder + "MaterialIcons-Regular.ttf", IconAssetPath,
                new string(Icons.AllCodepoints.Select(c => (char)c).ToArray()), shader, IconAtlasSize);

            var fallbacks = fallback != null ? new List<TMP_FontAsset> { fallback } : new List<TMP_FontAsset>();
            regular.fallbackFontAssetTable = fallbacks;
            bold.fallbackFontAssetTable = new List<TMP_FontAsset>(fallbacks);
            var weights = new SerializedObject(regular);
            weights.FindProperty("m_FontWeightTable.Array.data[7].regularTypeface").objectReferenceValue = bold;
            weights.ApplyModifiedPropertiesWithoutUndo();

            var settings = new SerializedObject(AssetDatabase.LoadAssetAtPath<TMP_Settings>(ProjectSetup.TmpSettingsPath));
            settings.FindProperty("m_defaultFontAsset").objectReferenceValue = regular;
            settings.FindProperty("m_defaultFontAssetPath").stringValue = "Fonts/";
            settings.ApplyModifiedPropertiesWithoutUndo();

            foreach (var asset in new Object[] { regular, bold, icons, settings.targetObject }) EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Fuentes] Roboto ({regular.characterTable.Count} caracteres, negrita {bold.characterTable.Count}) y " +
                      $"Material Icons ({icons.characterTable.Count} iconos); Roboto es la fuente por defecto.");
        }

        /// <summary>Todos los caracteres de los textos y los seeds, más ASCII, Latin-1 y <see cref="Extra"/>.</summary>
        public static string TextCharacters()
        {
            var set = new SortedSet<char>();
            for (var c = (char)32; c < 127; c++) set.Add(c);
            for (var c = (char)160; c <= 255; c++) set.Add(c);
            foreach (var c in Extra) set.Add(c);
            var files = Directory.GetFiles("Assets/_Gamespot/Resources/Localization", "*.json")
                .Concat(Directory.GetFiles("Assets/_Gamespot/Resources/Seed", "*.json"));
            foreach (var file in files)
            {
                foreach (var c in File.ReadAllText(file))
                {
                    if (c >= 32 && !char.IsSurrogate(c)) set.Add(c);
                }
            }
            return new string(set.ToArray());
        }

        /// <summary>
        /// Crea el asset (o reutiliza el que hay, para no cambiar su GUID), añade los caracteres que falten y lo deja
        /// estático. Los que la fuente no tenga se ignoran: los pinta el respaldo.
        /// </summary>
        private static TMP_FontAsset CreateOrUpdate(string fontPath, string assetPath, string characters, Shader shader, int atlasSize)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (font == null) throw new FileNotFoundException($"Falta la fuente {fontPath}");
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (asset == null)
            {
                asset = TMP_FontAsset.CreateFontAsset(font, SamplingPointSize, AtlasPadding, GlyphRenderMode.SDFAA, atlasSize, atlasSize,
                    AtlasPopulationMode.Dynamic, true);
                asset.name = Path.GetFileNameWithoutExtension(assetPath);
                AssetDatabase.CreateAsset(asset, assetPath);
                asset.material.name = asset.name + " Material";
                AssetDatabase.AddObjectToAsset(asset.material, asset);
            }
            else
            {
                // Para añadir caracteres tiene que poder leer la fuente.
                asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            }
            if (shader != null) asset.material.shader = shader;
            asset.TryAddCharacters(characters, out _);
            for (var i = 0; i < asset.atlasTextures.Length; i++)
            {
                var texture = asset.atlasTextures[i];
                if (texture == null || AssetDatabase.Contains(texture)) continue;
                texture.name = $"{asset.name} Atlas {i}";
                AssetDatabase.AddObjectToAsset(texture, asset);
            }
            asset.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(asset);
            return asset;
        }
    }
}
