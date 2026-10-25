using UnityEditor;
using UnityEngine;

namespace Gamespot.Editor.Import
{
    /// <summary>
    /// Ajustes de importación automáticos: las imágenes de <c>Resources/Images</c> entran como sprites de UI
    /// (sin mipmaps, filtro bilineal), los iconos de <c>Art/Icon</c> sin comprimir y los sonidos de
    /// <c>Resources/Audio</c> como clips cortos descomprimidos.
    /// </summary>
    public sealed class GamespotAssetPostprocessor : AssetPostprocessor
    {
        private const string ImagesFolder = "Assets/_Gamespot/Resources/Images/";
        private const string AudioFolder = "Assets/_Gamespot/Resources/Audio/";
        private const string IconFolder = "Assets/_Gamespot/Art/Icon/";

        private void OnPreprocessTexture()
        {
            if (assetPath.StartsWith(IconFolder))
            {
                // Icono de la app: lo reescala el build de cada plataforma, así que entra sin compresión ni mipmaps.
                var icon = (TextureImporter)assetImporter;
                icon.textureType = TextureImporterType.Default;
                icon.mipmapEnabled = false;
                icon.alphaIsTransparency = true;
                icon.textureCompression = TextureImporterCompression.Uncompressed;
                icon.maxTextureSize = 1024;
                return;
            }
            if (!assetPath.StartsWith(ImagesFolder)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
        }

        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(AudioFolder)) return;
            var importer = (AudioImporter)assetImporter;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.ADPCM;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.forceToMono = true;
        }
    }
}
