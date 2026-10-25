using Gamespot.Core.Runtime.Theme;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Gamespot.Editor.Setup
{
    /// <summary>
    /// Configuración del proyecto en código (menú <b>Gamespot → Configuración → Aplicar todo</b>).
    /// Es idempotente: repetirlo no cambia nada.
    /// </summary>
    public static class ProjectSetup
    {
        public const string CompanyName = "Sajitarios";
        public const string ProductName = "Sajitarios Gamespot";
        // Los mismos ids que la app Flutter: iOS no admite guiones bajos en el bundle id.
        public const string AndroidApplicationId = "com.sajitarios.sajitarios_gamespot";
        public const string AppleBundleId = "com.sajitarios.sajitariosGamespot";
        // Continúa la numeración de la app Flutter (0.75.0+75).
        public const string Version = "0.76.0";
        public const int BuildNumber = 76;

        public const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        [MenuItem("Gamespot/Configuración/Aplicar todo", priority = 0)]
        public static void ApplyAll()
        {
            ImportTmpEssentials();
            ApplyPlayerSettings();
            Debug.Log("[Gamespot] Configuración del proyecto aplicada.");
        }

        /// <summary>
        /// Importa los recursos esenciales de TextMeshPro (ajustes, fuente LiberationSans SDF y shaders) sin
        /// diálogo, si aún no están en <c>Assets/TextMesh Pro</c>.
        /// </summary>
        [MenuItem("Gamespot/Configuración/Recursos de TextMeshPro", priority = 10)]
        public static void ImportTmpEssentials()
        {
            if (AssetDatabase.LoadMainAssetAtPath(TmpSettingsPath) != null) return;
            var package = System.IO.Path.GetFullPath("Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage");
            AssetDatabase.ImportPackage(package, false);
            AssetDatabase.Refresh();
        }

        [MenuItem("Gamespot/Configuración/Player Settings", priority = 20)]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = Version;
            PlayerSettings.Android.bundleVersionCode = BuildNumber;
            PlayerSettings.iOS.buildNumber = BuildNumber.ToString();

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidApplicationId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, AppleBundleId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, AppleBundleId);

            // Juegos de pasar el móvil: solo vertical (decisión de Claude, revisable).
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);

            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.SplashScreen.backgroundColor = Palette.Background;
            PlayerSettings.runInBackground = false;

            Save();
        }

        /// <summary>Los cambios por API solo están en memoria: se marca el asset y se guarda en disco.</summary>
        private static void Save()
        {
            var playerSettings = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
            if (playerSettings != null) EditorUtility.SetDirty(playerSettings);
            AssetDatabase.SaveAssets();
        }
    }
}
