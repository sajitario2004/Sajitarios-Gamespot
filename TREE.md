# TREE

Estructura del repo. Generado con `tools/Update-Tree.ps1` el 2026-10-25 08:58.

No se listan: Library, Temp, Logs, obj, UserSettings, .git, .vs, .idea, .vscode, .utmp, Build, Builds, MemoryCaptures, Recordings, node_modules, __pycache__, .venv, .dart_tool, build, coverage, *.meta, *.csproj, *.sln, *.slnx, *.user, *.pidb, *.unityproj, .DS_Store, Thumbs.db.
Plegadas: skills, TextMesh Pro, Flutter, Capturas (Flutter = la app original en Legacy/Flutter, solo lectura).
Se regenera cada vez que se crea, mueve o borra algo; no se edita a mano (CLAUDE.md, seccion 1).

```
Sajitarios-Gamespot/
├── .claude/
│   └── settings.json
├── Docs/
│   └── Capturas/  (1 elementos, plegado)
├── Legacy/
│   └── Flutter/  (20 elementos, plegado)
├── SajitariosGamespot/
│   ├── Assets/
│   │   ├── _Gamespot/
│   │   │   ├── Scenes/
│   │   │   │   └── App.unity
│   │   │   ├── Scripts/
│   │   │   │   ├── App/
│   │   │   │   │   ├── Bootstrap.cs
│   │   │   │   │   └── Gamespot.App.asmdef
│   │   │   │   ├── Core/
│   │   │   │   │   ├── Json/
│   │   │   │   │   │   └── Json.cs
│   │   │   │   │   ├── Random/
│   │   │   │   │   │   ├── IRandomProvider.cs
│   │   │   │   │   │   ├── RandomExtensions.cs
│   │   │   │   │   │   ├── ScriptedRandom.cs
│   │   │   │   │   │   └── SeededRandom.cs
│   │   │   │   │   ├── Text/
│   │   │   │   │   │   ├── IcuMessageFormatter.cs
│   │   │   │   │   │   └── NameNormalizer.cs
│   │   │   │   │   └── Gamespot.Core.asmdef
│   │   │   │   ├── Core.Runtime/
│   │   │   │   │   ├── Theme/
│   │   │   │   │   │   └── Palette.cs
│   │   │   │   │   ├── UI/
│   │   │   │   │   │   ├── NeonBackground.cs
│   │   │   │   │   │   ├── SafeArea.cs
│   │   │   │   │   │   ├── UiLayout.cs
│   │   │   │   │   │   └── UiRoot.cs
│   │   │   │   │   └── Gamespot.Core.Runtime.asmdef
│   │   │   │   ├── Editor/
│   │   │   │   │   ├── Setup/
│   │   │   │   │   │   ├── AppSceneBuilder.cs
│   │   │   │   │   │   └── ProjectSetup.cs
│   │   │   │   │   └── Gamespot.Editor.asmdef
│   │   │   │   └── Games/
│   │   │   │       ├── EsUn10Pero/
│   │   │   │       │   └── Domain/
│   │   │   │       │       ├── Card.cs
│   │   │   │       │       ├── DrawCardUseCase.cs
│   │   │   │       │       └── Gamespot.Games.EsUn10Pero.Domain.asmdef
│   │   │   │       └── Impostor/
│   │   │   │           └── Domain/
│   │   │   │               ├── Flow/
│   │   │   │               │   └── ImpostorFlowController.cs
│   │   │   │               ├── AssignRolesUseCase.cs
│   │   │   │               ├── GameConfig.cs
│   │   │   │               ├── GameSession.cs
│   │   │   │               ├── Gamespot.Games.Impostor.Domain.asmdef
│   │   │   │               ├── Player.cs
│   │   │   │               ├── Role.cs
│   │   │   │               └── Word.cs
│   │   │   ├── Settings/
│   │   │   │   └── GamespotVolume.asset
│   │   │   └── Tests/
│   │   │       ├── EditMode/
│   │   │       │   ├── Core/
│   │   │       │   │   ├── IcuMessageFormatterTests.cs
│   │   │       │   │   ├── JsonTests.cs
│   │   │       │   │   ├── NameNormalizerTests.cs
│   │   │       │   │   └── RandomTests.cs
│   │   │       │   ├── Games/
│   │   │       │   │   ├── EsUn10Pero/
│   │   │       │   │   │   └── Domain/
│   │   │       │   │   │       └── DrawCardUseCaseTests.cs
│   │   │       │   │   └── Impostor/
│   │   │       │   │       └── Domain/
│   │   │       │   │           ├── AssignRolesUseCaseTests.cs
│   │   │       │   │           ├── GameConfigTests.cs
│   │   │       │   │           ├── GameSessionTests.cs
│   │   │       │   │           ├── ImpostorFlowControllerTests.cs
│   │   │       │   │           └── ImpostorTestData.cs
│   │   │       │   └── Gamespot.Tests.EditMode.asmdef
│   │   │       └── PlayMode/
│   │   │           ├── BootstrapTests.cs
│   │   │           └── Gamespot.Tests.PlayMode.asmdef
│   │   ├── Scenes/
│   │   │   └── SampleScene.unity
│   │   └── Settings/
│   │       ├── Scenes/
│   │       │   └── URP2DSceneTemplate.unity
│   │       ├── DefaultVolumeProfile.asset
│   │       ├── InputSystem_Actions.inputactions
│   │       ├── Lit2DSceneTemplate.scenetemplate
│   │       ├── Renderer2D.asset
│   │       ├── UniversalRenderPipelineGlobalSettings.asset
│   │       └── UniversalRP.asset
│   ├── Packages/
│   │   ├── manifest.json
│   │   └── packages-lock.json
│   ├── ProjectSettings/
│   │   ├── AudioManager.asset
│   │   ├── ClusterInputManager.asset
│   │   ├── DynamicsManager.asset
│   │   ├── EditorBuildSettings.asset
│   │   ├── EditorSettings.asset
│   │   ├── GraphicsSettings.asset
│   │   ├── InputManager.asset
│   │   ├── MemorySettings.asset
│   │   ├── MultiplayerManager.asset
│   │   ├── NavMeshAreas.asset
│   │   ├── NetworkManager.asset
│   │   ├── PackageManagerSettings.asset
│   │   ├── Physics2DSettings.asset
│   │   ├── PresetManager.asset
│   │   ├── ProjectSettings.asset
│   │   ├── ProjectVersion.txt
│   │   ├── QualitySettings.asset
│   │   ├── SceneTemplateSettings.json
│   │   ├── ShaderGraphSettings.asset
│   │   ├── TagManager.asset
│   │   ├── TimeManager.asset
│   │   ├── UnityConnectSettings.asset
│   │   ├── URPProjectSettings.asset
│   │   ├── VersionControlSettings.asset
│   │   ├── VFXManager.asset
│   │   └── XRSettings.asset
│   └── .vsconfig
├── tools/
│   ├── Capture-GameView.ps1
│   ├── UnityCli.ps1
│   └── Update-Tree.ps1
├── .gitattributes
├── .gitignore
├── CLAUDE.md
├── CONTEXT.md
├── LEARNINGS.md
├── README.md
├── TREE.md
└── VERSIONS.md
```
