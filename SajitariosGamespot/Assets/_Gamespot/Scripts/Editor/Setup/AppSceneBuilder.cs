using Gamespot.App;
using Gamespot.Core.Runtime.Theme;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Gamespot.Editor.Setup
{
    /// <summary>
    /// Construye la única escena de la app (menú <b>Gamespot → Construir → Escena App</b>): cámara URP con
    /// post-procesado, volumen global con Bloom, EventSystem del Input System y el <c>AppRoot</c> con el
    /// <see cref="Bootstrap"/>. La UI no vive en la escena: la crea el código al arrancar.
    /// </summary>
    public static class AppSceneBuilder
    {
        public const string ScenePath = "Assets/_Gamespot/Scenes/App.unity";
        public const string VolumeProfilePath = "Assets/_Gamespot/Settings/GamespotVolume.asset";

        // Bloom suave: realza el neón sin emborronar el texto (se ajusta mirando capturas).
        private const float BloomThreshold = 0.9f;
        private const float BloomIntensity = 0.6f;
        private const float BloomScatter = 0.65f;

        [MenuItem("Gamespot/Construir/Escena App", priority = 0)]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = CreateCamera();
            CreateVolume();
            CreateEventSystem();

            var root = new GameObject("AppRoot");
            root.AddComponent<Bootstrap>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Selection.activeObject = camera;
            Debug.Log($"[Gamespot] Escena construida: {ScenePath}");
        }

        private static GameObject CreateCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 0f, -10f);
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Palette.Background;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 100f;
            go.AddComponent<AudioListener>();
            var data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.None;
            return go;
        }

        private static void CreateVolume()
        {
            var go = new GameObject("Global Volume");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.sharedProfile = GetOrCreateVolumeProfile();
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        private static VolumeProfile GetOrCreateVolumeProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (profile == null)
            {
                EnsureFolder("Assets/_Gamespot", "Settings");
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, VolumeProfilePath);
            }

            if (!profile.TryGet(out Bloom bloom))
            {
                bloom = profile.Add<Bloom>(true);
                bloom.name = "Bloom";
                AssetDatabase.AddObjectToAsset(bloom, profile);
            }
            bloom.active = true;
            bloom.threshold.Override(BloomThreshold);
            bloom.intensity.Override(BloomIntensity);
            bloom.scatter.Override(BloomScatter);
            bloom.highQualityFiltering.Override(false);

            EditorUtility.SetDirty(bloom);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}")) AssetDatabase.CreateFolder(parent, name);
        }
    }
}
