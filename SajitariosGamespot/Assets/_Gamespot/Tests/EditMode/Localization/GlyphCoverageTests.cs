using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gamespot.Core.Runtime.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine.TextCore.LowLevel;

namespace Gamespot.Tests.EditMode.Localization
{
    /// <summary>
    /// Cada carácter de los textos y los seeds tiene glifo en la fuente por defecto (Roboto desde V014) o en su
    /// cadena de respaldo: en el atlas estático de alguna o en el .ttf de una dinámica, que lo saca en el móvil. Así
    /// no vuelve a salir un cuadradito como con «10⁻¹¹» en la trivia (V059l). Y cada icono sale de la fuente de iconos.
    /// </summary>
    public sealed class GlyphCoverageTests
    {
        [Test]
        public void TodosLosCaracteresTienenGlifo()
        {
            var font = TMP_Settings.defaultFontAsset;
            Assert.IsNotNull(font, "Fuente por defecto de TextMeshPro");
            Assert.AreEqual("Roboto SDF", font.name);
            Assert.IsNotNull(font.fontWeightTable[7].regularTypeface, "Roboto lleva su negrita real");

            var files = Directory.GetFiles("Assets/_Gamespot/Resources/Localization", "*.json")
                .Concat(Directory.GetFiles("Assets/_Gamespot/Resources/Seed", "*.json")).ToList();
            Assert.That(files.Count, Is.GreaterThanOrEqualTo(4));
            var missing = new List<string>();
            foreach (var file in files)
            {
                foreach (var c in File.ReadAllText(file).Where(c => c >= 32).Distinct())
                {
                    if (!Covered(font, c, new HashSet<TMP_FontAsset>())) missing.Add($"U+{(int)c:X4} en {Path.GetFileName(file)}");
                }
            }
            CollectionAssert.IsEmpty(missing, "Caracteres sin glifo");
        }

        [Test]
        public void TodosLosIconosSalenDeLaFuenteDeIconos()
        {
            Assert.IsNotNull(Icons.IconFont, Icons.IconFontResource);
            foreach (IconId id in Enum.GetValues(typeof(IconId)))
            {
                if (id.ToString().StartsWith("Suit", StringComparison.Ordinal)) continue;
                Assert.IsTrue(Icons.Glyph(id).usesIconFont, $"El icono {id} sale como sustituto de texto");
            }
        }

        private static bool Covered(TMP_FontAsset asset, char c, HashSet<TMP_FontAsset> seen)
        {
            if (asset == null || !seen.Add(asset)) return false;
            if (asset.characterTable.Any(x => x.unicode == c)) return true;
            if (asset.atlasPopulationMode != AtlasPopulationMode.Static && asset.sourceFontFile != null
                && FontEngine.LoadFontFace(asset.sourceFontFile, 90) == FontEngineError.Success
                && FontEngine.TryGetGlyphIndex(c, out var index) && index != 0)
            {
                return true;
            }
            return (asset.fallbackFontAssetTable ?? new List<TMP_FontAsset>()).Any(f => Covered(f, c, seen));
        }
    }
}
