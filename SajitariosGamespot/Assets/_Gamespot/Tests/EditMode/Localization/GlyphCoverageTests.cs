using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Gamespot.Tests.EditMode.Localization
{
    /// <summary>
    /// Cada carácter de los textos y los seeds tiene glifo: o está en el atlas estático de la fuente o lo tiene el
    /// .ttf, del que el atlas dinámico de respaldo lo saca en el móvil. Así no vuelve a salir un cuadradito como con
    /// «10⁻¹¹» en la trivia (V059l).
    /// </summary>
    public sealed class GlyphCoverageTests
    {
        private const string SourceFont = "Assets/TextMesh Pro/Fonts/LiberationSans.ttf";

        [Test]
        public void TodosLosCaracteresTienenGlifo()
        {
            var atlas = TMP_Settings.defaultFontAsset;
            Assert.IsNotNull(atlas, "Fuente por defecto de TextMeshPro");
            var inAtlas = new HashSet<uint>(atlas.characterTable.Select(c => c.unicode));
            var font = AssetDatabase.LoadAssetAtPath<Font>(SourceFont);
            Assert.IsNotNull(font, SourceFont);
            Assert.AreEqual(FontEngineError.Success, FontEngine.LoadFontFace(font, 90));

            var files = Directory.GetFiles("Assets/_Gamespot/Resources/Localization", "*.json")
                .Concat(Directory.GetFiles("Assets/_Gamespot/Resources/Seed", "*.json")).ToList();
            Assert.That(files.Count, Is.GreaterThanOrEqualTo(4));
            var missing = new List<string>();
            foreach (var file in files)
            {
                foreach (var c in File.ReadAllText(file).Where(c => c >= 32).Distinct())
                {
                    if (inAtlas.Contains(c)) continue;
                    if (FontEngine.TryGetGlyphIndex(c, out var index) && index != 0) continue;
                    missing.Add($"U+{(int)c:X4} en {Path.GetFileName(file)}");
                }
            }
            CollectionAssert.IsEmpty(missing, "Caracteres sin glifo");
        }
    }
}
