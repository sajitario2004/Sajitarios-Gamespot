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

        /// <summary>El icono de la app Flutter (carta con pica sobre el morado #7C3AED) y sus capas adaptativas.</summary>
        public const string IconFolder = "Assets/_Gamespot/Art/Icon/";

        [MenuItem("Gamespot/Configuración/Aplicar todo", priority = 0)]
        public static void ApplyAll()
        {
            ImportTmpEssentials();
            ApplyPlayerSettings();
            ApplyIcons();
            Debug.Log("[Gamespot] Configuración del proyecto aplicada.");
        }

        /// <summary>
        /// Icono de la app: el mismo que tenía la app Flutter. Por defecto (también iOS, que lo reescala) y Android
        /// adaptativo con el fondo morado y la carta delante.
        /// </summary>
        [MenuItem("Gamespot/Configuración/Icono de la app", priority = 30)]
        public static void ApplyIcons()
        {
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconFolder + "app_icon.png");
            var foreground = AssetDatabase.LoadAssetAtPath<Texture2D>(IconFolder + "app_icon_foreground.png");
            var background = AssetDatabase.LoadAssetAtPath<Texture2D>(IconFolder + "app_icon_background.png");
            if (icon == null || foreground == null || background == null)
            {
                Debug.LogWarning($"[Gamespot] Faltan los iconos en {IconFolder}");
                return;
            }

            // El icono por defecto sirve también para Android 7.1 (sin adaptativos: Unity 6.3 da por obsoletos los
            // iconos "Legacy" y "Round") y para iOS.
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
            var adaptive = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, UnityEditor.Android.AndroidPlatformIconKind.Adaptive);
            foreach (var platformIcon in adaptive) platformIcon.SetTextures(background, foreground);
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, UnityEditor.Android.AndroidPlatformIconKind.Adaptive, adaptive);
            Save();
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
