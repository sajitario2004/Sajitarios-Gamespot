# TREE

Estructura del repo. Generado con `tools/Update-Tree.ps1` el 2026-10-25 16:14.

No se listan: Library, Temp, Logs, obj, UserSettings, .git, .vs, .idea, .vscode, .utmp, Build, Builds, MemoryCaptures, Recordings, node_modules, __pycache__, .venv, .dart_tool, build, coverage, *.meta, *.csproj, *.sln, *.slnx, *.user, *.pidb, *.unityproj, .DS_Store, Thumbs.db.
Plegadas: skills, TextMesh Pro, Flutter, Capturas (Flutter = la app original en Legacy/Flutter, solo lectura).
Se regenera cada vez que se crea, mueve o borra algo; no se edita a mano (CLAUDE.md, seccion 1).

```
Sajitarios-Gamespot/
├── .claude/
│   └── settings.json
├── Docs/
│   ├── Capturas/  (24 elementos, plegado)
│   └── PLAN_MEJORAS.md
├── Legacy/
│   └── Flutter/  (20 elementos, plegado)
├── SajitariosGamespot/
│   ├── Assets/
│   │   ├── _Gamespot/
│   │   │   ├── Resources/
│   │   │   │   ├── Audio/
│   │   │   │   │   ├── beep.wav
│   │   │   │   │   ├── buzzer.wav
│   │   │   │   │   ├── card_flip.wav
│   │   │   │   │   ├── chime.wav
│   │   │   │   │   ├── correct.wav
│   │   │   │   │   ├── explosion.wav
│   │   │   │   │   ├── game_over.wav
│   │   │   │   │   ├── reveal.wav
│   │   │   │   │   ├── swipe.wav
│   │   │   │   │   ├── tick.wav
│   │   │   │   │   ├── tock.wav
│   │   │   │   │   └── wrong.wav
│   │   │   │   ├── Images/
│   │   │   │   │   ├── card_back.png
│   │   │   │   │   └── menu_header.png
│   │   │   │   ├── Localization/
│   │   │   │   │   ├── en.json
│   │   │   │   │   └── es.json
│   │   │   │   └── Seed/
│   │   │   │       ├── bomba_categorias.json
│   │   │   │       ├── bomba_silabas.json
│   │   │   │       ├── impostor_words.json
│   │   │   │       ├── tabu_words.json
│   │   │   │       ├── trivia_questions.json
│   │   │   │       ├── wavelength_spectra.json
│   │   │   │       └── yo_nunca_statements.json
│   │   │   ├── Scenes/
│   │   │   │   └── App.unity
│   │   │   ├── Scripts/
│   │   │   │   ├── App/
│   │   │   │   │   ├── Bootstrap.cs
│   │   │   │   │   ├── GameRegistry.cs
│   │   │   │   │   └── Gamespot.App.asmdef
│   │   │   │   ├── Core/
│   │   │   │   │   ├── Json/
│   │   │   │   │   │   ├── Json.cs
│   │   │   │   │   │   └── JsonWriter.cs
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
│   │   │   │   │   ├── Audio/
│   │   │   │   │   │   ├── AudioService.cs
│   │   │   │   │   │   ├── Haptics.cs
│   │   │   │   │   │   └── MuteButton.cs
│   │   │   │   │   ├── Db/
│   │   │   │   │   │   ├── AppDatabase.cs
│   │   │   │   │   │   ├── IDb.cs
│   │   │   │   │   │   ├── IDbSchema.cs
│   │   │   │   │   │   ├── ISeedSource.cs
│   │   │   │   │   │   └── SqliteDb.cs
│   │   │   │   │   ├── Localization/
│   │   │   │   │   │   ├── LanguageSelector.cs
│   │   │   │   │   │   └── Loc.cs
│   │   │   │   │   ├── Navigation/
│   │   │   │   │   │   ├── AppScreen.cs
│   │   │   │   │   │   └── Router.cs
│   │   │   │   │   ├── Settings/
│   │   │   │   │   │   └── AppSettings.cs
│   │   │   │   │   ├── Theme/
│   │   │   │   │   │   └── Palette.cs
│   │   │   │   │   ├── UI/
│   │   │   │   │   │   ├── AppBar.cs
│   │   │   │   │   │   ├── CoverImage.cs
│   │   │   │   │   │   ├── Dialogs.cs
│   │   │   │   │   │   ├── FadeScaleIn.cs
│   │   │   │   │   │   ├── IconId.cs
│   │   │   │   │   │   ├── Icons.cs
│   │   │   │   │   │   ├── IconView.cs
│   │   │   │   │   │   ├── NeonBackground.cs
│   │   │   │   │   │   ├── NeonButton.cs
│   │   │   │   │   │   ├── NeonChip.cs
│   │   │   │   │   │   ├── NeonPanel.cs
│   │   │   │   │   │   ├── NeonSprites.cs
│   │   │   │   │   │   ├── NeonStepper.cs
│   │   │   │   │   │   ├── NeonSwitch.cs
│   │   │   │   │   │   ├── NeonTextField.cs
│   │   │   │   │   │   ├── NeonTextMaterials.cs
│   │   │   │   │   │   ├── PulseGlow.cs
│   │   │   │   │   │   ├── ResponsiveGrid.cs
│   │   │   │   │   │   ├── SafeArea.cs
│   │   │   │   │   │   ├── ScreenScaffold.cs
│   │   │   │   │   │   ├── Snackbar.cs
│   │   │   │   │   │   ├── SuitSprites.cs
│   │   │   │   │   │   ├── Typography.cs
│   │   │   │   │   │   ├── Ui.cs
│   │   │   │   │   │   ├── UiExtensions.cs
│   │   │   │   │   │   ├── UiLayout.cs
│   │   │   │   │   │   ├── UiRoot.cs
│   │   │   │   │   │   └── VirtualList.cs
│   │   │   │   │   └── Gamespot.Core.Runtime.asmdef
│   │   │   │   ├── Editor/
│   │   │   │   │   ├── Audio/
│   │   │   │   │   │   └── SoundSynth.cs
│   │   │   │   │   ├── Import/
│   │   │   │   │   │   └── GamespotAssetPostprocessor.cs
│   │   │   │   │   ├── Localization/
│   │   │   │   │   │   └── ArbImporter.cs
│   │   │   │   │   ├── Setup/
│   │   │   │   │   │   ├── AppSceneBuilder.cs
│   │   │   │   │   │   └── ProjectSetup.cs
│   │   │   │   │   └── Gamespot.Editor.asmdef
│   │   │   │   ├── Games/
│   │   │   │   │   ├── _Shared/
│   │   │   │   │   │   ├── Presentation/
│   │   │   │   │   │   │   ├── GameUi.cs
│   │   │   │   │   │   │   ├── NotFoundScreen.cs
│   │   │   │   │   │   │   ├── PlayerListEditor.cs
│   │   │   │   │   │   │   └── RulesScreen.cs
│   │   │   │   │   │   ├── Gamespot.Games.Shared.asmdef
│   │   │   │   │   │   └── IGameDescriptor.cs
│   │   │   │   │   ├── Bomba/
│   │   │   │   │   │   ├── Data/
│   │   │   │   │   │   │   └── BombaPromptRepository.cs
│   │   │   │   │   │   ├── Domain/
│   │   │   │   │   │   │   ├── Flow/
│   │   │   │   │   │   │   │   └── BombaFlowController.cs
│   │   │   │   │   │   │   ├── BombaDomain.cs
│   │   │   │   │   │   │   └── Gamespot.Games.Bomba.Domain.asmdef
│   │   │   │   │   │   ├── Presentation/
│   │   │   │   │   │   │   └── BombaScreens.cs
│   │   │   │   │   │   ├── BombaGame.cs
│   │   │   │   │   │   └── Gamespot.Games.Bomba.asmdef
│   │   │   │   │   ├── EsUn10Pero/
│   │   │   │   │   │   ├── Domain/
│   │   │   │   │   │   │   ├── Card.cs
│   │   │   │   │   │   │   ├── DrawCardUseCase.cs
│   │   │   │   │   │   │   └── Gamespot.Games.EsUn10Pero.Domain.asmdef
│   │   │   │   │   │   ├── Presentation/
│   │   │   │   │   │   │   ├── CardView.cs
│   │   │   │   │   │   │   └── EsUn10PeroScreen.cs
│   │   │   │   │   │   ├── EsUn10PeroGame.cs
│   │   │   │   │   │   └── Gamespot.Games.EsUn10Pero.asmdef
│   │   │   │   │   ├── Impostor/
│   │   │   │   │   │   ├── Data/
│   │   │   │   │   │   │   ├── GameHistoryRepository.cs
│   │   │   │   │   │   │   ├── ImpostorSchema.cs
│   │   │   │   │   │   │   └── WordRepository.cs
│   │   │   │   │   │   ├── Domain/
│   │   │   │   │   │   │   ├── Flow/
│   │   │   │   │   │   │   │   └── ImpostorFlowController.cs
│   │   │   │   │   │   │   ├── AssignRolesUseCase.cs
│   │   │   │   │   │   │   ├── GameConfig.cs
│   │   │   │   │   │   │   ├── GameRecord.cs
│   │   │   │   │   │   │   ├── GameSession.cs
│   │   │   │   │   │   │   ├── Gamespot.Games.Impostor.Domain.asmdef
│   │   │   │   │   │   │   ├── Player.cs
│   │   │   │   │   │   │   ├── Role.cs
│   │   │   │   │   │   │   └── Word.cs
│   │   │   │   │   │   ├── Presentation/
│   │   │   │   │   │   │   ├── GameOverScreen.cs
│   │   │   │   │   │   │   ├── HistoryScreen.cs
│   │   │   │   │   │   │   ├── ImpostorModule.cs
│   │   │   │   │   │   │   ├── ImpostorPlayScreen.cs
│   │   │   │   │   │   │   ├── ImpostorSetupScreen.cs
│   │   │   │   │   │   │   ├── PassDeviceScreen.cs
│   │   │   │   │   │   │   ├── RevealScreen.cs
│   │   │   │   │   │   │   ├── VotingScreen.cs
│   │   │   │   │   │   │   └── WordsScreen.cs
│   │   │   │   │   │   ├── Gamespot.Games.Impostor.asmdef
│   │   │   │   │   │   └── ImpostorGame.cs
│   │   │   │   │   ├── Tabu/
│   │   │   │   │   │   ├── Data/
│   │   │   │   │   │   │   └── TabuWordRepository.cs
│   │   │   │   │   │   ├── Domain/
│   │   │   │   │   │   │   ├── Flow/
│   │   │   │   │   │   │   │   └── TabuFlowController.cs
│   │   │   │   │   │   │   ├── Gamespot.Games.Tabu.Domain.asmdef
│   │   │   │   │   │   │   └── TabuDomain.cs
│   │   │   │   │   │   ├── Presentation/
│   │   │   │   │   │   │   └── TabuScreens.cs
│   │   │   │   │   │   ├── Gamespot.Games.Tabu.asmdef
│   │   │   │   │   │   └── TabuGame.cs
│   │   │   │   │   ├── Trivia/
│   │   │   │   │   │   ├── Data/
│   │   │   │   │   │   │   └── TriviaData.cs
│   │   │   │   │   │   ├── Domain/
│   │   │   │   │   │   │   ├── Flow/
│   │   │   │   │   │   │   │   └── TriviaFlowController.cs
│   │   │   │   │   │   │   ├── Gamespot.Games.Trivia.Domain.asmdef
│   │   │   │   │   │   │   ├── Question.cs
│   │   │   │   │   │   │   └── TriviaSession.cs
│   │   │   │   │   │   ├── Presentation/
│   │   │   │   │   │   │   └── TriviaScreens.cs
│   │   │   │   │   │   ├── Gamespot.Games.Trivia.asmdef
│   │   │   │   │   │   └── TriviaGame.cs
│   │   │   │   │   ├── Wavelength/
│   │   │   │   │   │   ├── Data/
│   │   │   │   │   │   │   └── SpectrumRepository.cs
│   │   │   │   │   │   ├── Domain/
│   │   │   │   │   │   │   ├── Flow/
│   │   │   │   │   │   │   │   └── WavelengthFlowController.cs
│   │   │   │   │   │   │   ├── Gamespot.Games.Wavelength.Domain.asmdef
│   │   │   │   │   │   │   └── WavelengthDomain.cs
│   │   │   │   │   │   ├── Presentation/
│   │   │   │   │   │   │   ├── WavelengthDial.cs
│   │   │   │   │   │   │   └── WavelengthScreens.cs
│   │   │   │   │   │   ├── Gamespot.Games.Wavelength.asmdef
│   │   │   │   │   │   └── WavelengthGame.cs
│   │   │   │   │   └── YoNunca/
│   │   │   │   │       ├── Domain/
│   │   │   │   │       │   ├── Gamespot.Games.YoNunca.Domain.asmdef
│   │   │   │   │       │   └── YoNuncaDomain.cs
│   │   │   │   │       ├── Gamespot.Games.YoNunca.asmdef
│   │   │   │   │       └── YoNuncaGame.cs
│   │   │   │   └── Menu/
│   │   │   │       ├── Gamespot.Menu.asmdef
│   │   │   │       └── MenuScreen.cs
│   │   │   ├── Settings/
│   │   │   │   └── GamespotVolume.asset
│   │   │   └── Tests/
│   │   │       ├── EditMode/
│   │   │       │   ├── Audio/
│   │   │       │   │   └── AudioClipsTests.cs
│   │   │       │   ├── Core/
│   │   │       │   │   ├── IcuMessageFormatterTests.cs
│   │   │       │   │   ├── JsonTests.cs
│   │   │       │   │   ├── NameNormalizerTests.cs
│   │   │       │   │   └── RandomTests.cs
│   │   │       │   ├── Db/
│   │   │       │   │   ├── AppDatabaseTests.cs
│   │   │       │   │   └── TempDatabase.cs
│   │   │       │   ├── Games/
│   │   │       │   │   ├── Bomba/
│   │   │       │   │   │   ├── Domain/
│   │   │       │   │   │   │   └── BombaDomainTests.cs
│   │   │       │   │   │   └── BombaDataTests.cs
│   │   │       │   │   ├── EsUn10Pero/
│   │   │       │   │   │   └── Domain/
│   │   │       │   │   │       └── DrawCardUseCaseTests.cs
│   │   │       │   │   ├── Impostor/
│   │   │       │   │   │   ├── Data/
│   │   │       │   │   │   │   └── ImpostorDataTests.cs
│   │   │       │   │   │   └── Domain/
│   │   │       │   │   │       ├── AssignRolesUseCaseTests.cs
│   │   │       │   │   │       ├── GameConfigTests.cs
│   │   │       │   │   │       ├── GameSessionTests.cs
│   │   │       │   │   │       ├── ImpostorFlowControllerTests.cs
│   │   │       │   │   │       ├── ImpostorStatsTests.cs
│   │   │       │   │   │       └── ImpostorTestData.cs
│   │   │       │   │   ├── Tabu/
│   │   │       │   │   │   ├── Domain/
│   │   │       │   │   │   │   └── TabuDomainTests.cs
│   │   │       │   │   │   └── TabuDataTests.cs
│   │   │       │   │   ├── Trivia/
│   │   │       │   │   │   ├── Data/
│   │   │       │   │   │   │   └── TriviaDataTests.cs
│   │   │       │   │   │   └── Domain/
│   │   │       │   │   │       └── TriviaDomainTests.cs
│   │   │       │   │   ├── Wavelength/
│   │   │       │   │   │   ├── Domain/
│   │   │       │   │   │   │   └── WavelengthDomainTests.cs
│   │   │       │   │   │   └── WavelengthDialAndDataTests.cs
│   │   │       │   │   ├── YoNunca/
│   │   │       │   │   │   └── Domain/
│   │   │       │   │   │       └── YoNuncaDomainTests.cs
│   │   │       │   │   └── SeedFilesTests.cs
│   │   │       │   ├── Localization/
│   │   │       │   │   └── LocTests.cs
│   │   │       │   ├── ArchitectureTests.cs
│   │   │       │   └── Gamespot.Tests.EditMode.asmdef
│   │   │       └── PlayMode/
│   │   │           ├── Games/
│   │   │           │   ├── BombaFlowTests.cs
│   │   │           │   ├── EsUn10PeroScreenTests.cs
│   │   │           │   ├── ImpostorFlowTests.cs
│   │   │           │   ├── PlayerRosterTests.cs
│   │   │           │   ├── SoundTests.cs
│   │   │           │   ├── TabuFlowTests.cs
│   │   │           │   ├── TriviaFlowTests.cs
│   │   │           │   ├── WavelengthFlowTests.cs
│   │   │           │   └── YoNuncaFlowTests.cs
│   │   │           ├── AppTestHarness.cs
│   │   │           ├── BootstrapTests.cs
│   │   │           └── Gamespot.Tests.PlayMode.asmdef
│   │   ├── Scenes/
│   │   │   └── SampleScene.unity
│   │   ├── Settings/
│   │   │   ├── Scenes/
│   │   │   │   └── URP2DSceneTemplate.unity
│   │   │   ├── DefaultVolumeProfile.asset
│   │   │   ├── InputSystem_Actions.inputactions
│   │   │   ├── Lit2DSceneTemplate.scenetemplate
│   │   │   ├── Renderer2D.asset
│   │   │   ├── UniversalRenderPipelineGlobalSettings.asset
│   │   │   └── UniversalRP.asset
│   │   └── TextMesh Pro/  (8 elementos, plegado)
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
