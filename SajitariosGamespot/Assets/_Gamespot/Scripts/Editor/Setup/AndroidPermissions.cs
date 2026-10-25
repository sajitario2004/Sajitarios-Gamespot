using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Gamespot.Editor.Setup
{
    /// <summary>
    /// La app no usa internet (V059p: lo dice la política de privacidad), pero algún paquete de Unity hace que el
    /// manifiesto final pida <c>INTERNET</c>. En los builds normales se quita con <c>tools:node="remove"</c> en el
    /// manifiesto de <c>unityLibrary</c>, que gana a cualquier librería al fusionarlos; en los de desarrollo se deja,
    /// porque lo usa el profiler.
    /// </summary>
    public sealed class AndroidPermissions : IPreprocessBuildWithReport, IPostGenerateGradleAndroidProject
    {
        public const string Internet = "android.permission.INTERNET";
        private const string ToolsNamespace = "http://schemas.android.com/tools";

        private static bool _development;

        public int callbackOrder => 999;

        public void OnPreprocessBuild(BuildReport report) => _development = (report.summary.options & BuildOptions.Development) != 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            if (_development) return;
            var manifest = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            if (!File.Exists(manifest))
            {
                Debug.LogWarning($"[Android] No encuentro {manifest}: el permiso {Internet} se queda");
                return;
            }
            File.WriteAllText(manifest, RemovePermission(File.ReadAllText(manifest), Internet));
        }

        /// <summary>Quita el permiso del manifiesto y deja la marca para que el fusionado lo quite también de las librerías.</summary>
        public static string RemovePermission(string manifest, string permission)
        {
            var escaped = Regex.Escape(permission);
            manifest = Regex.Replace(manifest, $@"\s*<uses-permission[^>]*android:name=""{escaped}""[^>]*/>", string.Empty);
            if (!manifest.Contains($"xmlns:tools=\"{ToolsNamespace}\""))
            {
                manifest = new Regex(@"<manifest\b").Replace(manifest, $"<manifest xmlns:tools=\"{ToolsNamespace}\"", 1);
            }
            var marker = $"<uses-permission android:name=\"{permission}\" tools:node=\"remove\" />";
            var close = manifest.IndexOf('>', manifest.IndexOf("<manifest", System.StringComparison.Ordinal));
            return manifest.Insert(close + 1, "\n  " + marker);
        }
    }
}
