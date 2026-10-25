using System.IO;
using UnityEngine;

namespace Gamespot.Core.Runtime.Db
{
    /// <summary>De dónde salen los JSON de datos iniciales (seeds).</summary>
    public interface ISeedSource
    {
        /// <summary>Texto del seed <paramref name="name"/> (sin extensión). Lanza si no existe.</summary>
        string Load(string name);
    }

    /// <summary>Seeds empaquetados en <c>Resources/Seed/</c> (funciona en Android, iOS y el Editor).</summary>
    public sealed class ResourcesSeedSource : ISeedSource
    {
        public const string Folder = "Seed";

        public string Load(string name)
        {
            var asset = Resources.Load<TextAsset>($"{Folder}/{name}");
            if (asset == null) throw new FileNotFoundException($"No existe el seed Resources/{Folder}/{name}.json");
            var text = asset.text;
            Resources.UnloadAsset(asset);
            return text;
        }
    }
}
