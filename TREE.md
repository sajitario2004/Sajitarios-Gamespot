# TREE

Estructura del repo. Generado con `tools/Update-Tree.ps1` el 2026-10-26 00:48.

No se listan: Library, Temp, Logs, obj, UserSettings, .git, .vs, .idea, .vscode, .utmp, Build, Builds, MemoryCaptures, Recordings, node_modules, __pycache__, .venv, .dart_tool, build, coverage, *.meta, *.csproj, *.sln, *.slnx, *.user, *.pidb, *.unityproj, .DS_Store, Thumbs.db, *.apk, *.aab.
Plegadas: skills, TextMesh Pro, Flutter, Capturas (Flutter = la app original en Legacy/Flutter, solo lectura).
Se regenera cada vez que se crea, mueve o borra algo; no se edita a mano (CLAUDE.md, seccion 1).

```
Sajitarios-Gamespot/
├── .claude/
│   └── settings.json
├── Docs/
│   ├── Capturas/  (86 elementos, plegado)
│   └── PLAN_MEJORAS.md
├── Legacy/
│   └── Flutter/  (20 elementos, plegado)
├── SajitariosGamespot/
│   ├── Assets/
│   │   ├── _Gamespot/
│   │   │   ├── Art/
│   │   │   │   └── Icon/
│   │   │   │       ├── app_icon.png
│   │   │   │       ├── app_icon_background.png
│   │   │   │       └── app_icon_foreground.png
│   │   │   ├── Fonts/
│   │   │   │   ├── LICENSE-MaterialIcons.txt
│   │   │   │   ├── LICENSE-Roboto.txt
│   │   │   │   ├── MaterialIcons-Regular.ttf
│   │   │   │   ├── Roboto-Bold.ttf
│   │   │   │   └── Roboto-Regular.ttf
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
│   │   │   │   ├── Fonts/
│   │   │   │   │   ├── MaterialIcons SDF.asset
│   │   │   │   │   ├── Roboto Bold SDF.asset
│   │   │   │   │   └── Roboto SDF.asset
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
│   │   │   │       ├── mas_probable_cards.json
│   │   │   │       ├── mimica_cards.json
│   │   │   │       ├── que_prefieres_cards.json
│   │   │   │       ├── quien_soy_cards.json
│   │   │   │       ├── tabu_words.json
│   │   │   │       ├── trivia_questions.json
│   │   │   │       ├── verdad_reto_cards.json
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
│   │   │   │   │   ├── Content/
│   │   │   │   │   │   ├── PromptCard.cs
│   │   │   │   │   │   └── WordCategories.cs
│   │   │   │   │   ├── Json/
│   │   │   │   │   │   ├── Json.cs
│   │   │   │   │   │   └── JsonWriter.cs
│   │   │   │   │   ├── Party/
│   │   │   │   │   │   ├── GuessGame.cs
│   │   │   │   │   │   └── TeamMatch.cs
│   │   │   │   │   ├── Random/
│   │   │   │   │   │   ├── IRandomProvider.cs
│   │   │   │   │   │   ├── RandomExtensions.cs
│   │   │   │   │   │   ├── ScriptedRandom.cs
│   │   │   │   │   │   └── SeededRandom.cs
│   │   │   │   │   ├── Text/
│   │   │   │   │   │   ├── IcuMessageFormatter.cs
│   │   │   │   │   │   └── NameNormalizer.cs
│   │   │   │   │   ├── Time/
│   │   │   │   │   │   └── Countdown.cs
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
│   │   │   │   │   ├── Sensors/
│   │   │   │   │   │   └── MotionSensor.cs
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
│   │   │   │   │   │   ├── IdleRendering.cs
│   │   │   │   │   │   ├── LandscapeFrame.cs
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
│   │   │   │   │   │   ├── ResponsiveChipGrid.cs
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
│   │   │   │   │   │   ├── SoundLevel.cs
│   │   │   │   │   │   └── SoundSynth.cs
│   │   │   │   │   ├── Import/
│   │   │   │   │   │   └── GamespotAssetPostprocessor.cs
│   │   │   │   │   ├── Localization/
│   │   │   │   │   │   └── ArbImporter.cs
│   │   │   │   │   ├── Setup/
│   │   │   │   │   │   ├── AndroidPermissions.cs
│   │   │   │   │   │   ├── AppSceneBuilder.cs
│   │   │   │   │   │   ├── FontSetup.cs
│   │   │   │   │   │   └── ProjectSetup.cs
│   │   │   │   │   └── Gamespot.Editor.asmdef
│   │   │   │   ├── Games/
│   │   │   │   │   ├── _Shared/
│   │   │   │   │   │   ├── Data/
│   │   │   │   │   │   │   ├── CustomCategoryRepository.cs
│   │   │   │   │   │   │   └── PromptCardRepository.cs
│   │   │   │   │   │   ├── Presentation/
│   │   │   │   │   │   │   ├── AdultGate.cs
│   │   │   │   │   │   │   ├── CategoryNameDialog.cs
│   │   │   │   │   │   │   ├── CategoryOption.cs
│   │   │   │   │   │   │   ├── CategoryPicker.cs
│   │   │   │   │   │   │   ├── CategorySelectDialog.cs
│   │   │   │   │   │   │   ├── CustomCategoriesScreen.cs
│   │   │   │   │   │   │   ├── GameUi.cs
│   │   │   │   │   │   │   ├── GuessScreens.cs
│   │   │   │   │   │   │   ├── NotFoundScreen.cs
│   │   │   │   │   │   │   ├── PlayerListEditor.cs
│   │   │   │   │   │   │   ├── PromptCardsConfig.cs
│   │   │   │   │   │   │   ├── PromptCardsScreen.cs
│   │   │   │   │   │   │   ├── RulesScreen.cs
│   │   │   │   │   │   │   ├── ScoreList.cs
│   │   │   │   │   │   │   └── SetupWidgets.cs
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
│   │   │   │   │   ├── HombresLobo/
│   │   │   │   │   │   ├── Domain/
│   │   │   │   │   │   │   ├── Gamespot.Games.HombresLobo.Domain.asmdef
│   │   │   │   │   │   │   ├── LoboConfig.cs
│   │   │   │   │   │   │   ├── LoboGame.cs
│   │   │   │   │   │   │   ├── LoboLuna.cs
│   │   │   │   │   │   │   ├── LoboRoles.cs
│   │   │   │   │   │   │   └── LoboState.cs
│   │   │   │   │   │   ├── Gamespot.Games.HombresLobo.asmdef
│   │   │   │   │   │   ├── HombresLoboGame.cs
│   │   │   │   │   │   └── LoboSetupScreens.cs
│   │   │   │   │   ├── Impostor/
│   │   │   │   │   │   ├── Data/
│   │   │   │   │   │   │   ├── CategoryWordSource.cs
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
│   │   │   │   │   ├── MasProbable/
│   │   │   │   │   │   ├── Domain/
│   │   │   │   │   │   │   ├── Gamespot.Games.MasProbable.Domain.asmdef
│   │   │   │   │   │   │   └── MasProbableDomain.cs
│   │   │   │   │   │   ├── Gamespot.Games.MasProbable.asmdef
│   │   │   │   │   │   ├── MasProbableGame.cs
│   │   │   │   │   │   └── MasProbableScreens.cs
│   │   │   │   │   ├── Mimica/
│   │   │   │   │   │   ├── Domain/
│   │   │   │   │   │   │   ├── Gamespot.Games.Mimica.Domain.asmdef
│   │   │   │   │   │   │   └── MimicaDomain.cs
│   │   │   │   │   │   ├── Gamespot.Games.Mimica.asmdef
│   │   │   │   │   │   ├── MimicaGame.cs
│   │   │   │   │   │   └── MimicaScreens.cs
│   │   │   │   │   ├── QuePrefieres/
│   │   │   │   │   │   ├── Domain/
│   │   │   │   │   │   │   ├── Gamespot.Games.QuePrefieres.Domain.asmdef
│   │   │   │   │   │   │   └── QuePrefieresDomain.cs
│   │   │   │   │   │   ├── Gamespot.Games.QuePrefieres.asmdef
│   │   │   │   │   │   ├── QuePrefieresGame.cs
│   │   │   │   │   │   └── QuePrefieresScreens.cs
│   │   │   │   │   ├── QuienSoy/
│   │   │   │   │   │   ├── Domain/
│   │   │   │   │   │   │   ├── Gamespot.Games.QuienSoy.Domain.asmdef
│   │   │   │   │   │   │   └── QuienSoyDomain.cs
│   │   │   │   │   │   ├── Gamespot.Games.QuienSoy.asmdef
│   │   │   │   │   │   ├── QuienSoyGame.cs
│   │   │   │   │   │   └── QuienSoyScreens.cs
│   │   │   │   │   ├── Tabu/
│   │   │   │   │   │   ├── Data/
│   │   │   │   │   │   │   └── TabuWordRepository.cs
│   │   │   │   │   │   ├── Domain/
│   │   │   │   │   │   │   ├── Flow/
│   │   │   │   │   │   │   │   └── TabuFlowController.cs
│   │   │   │   │   │   │   ├── Gamespot.Games.Tabu.Domain.asmdef
│   │   │   │   │   │   │   ├── TabuCardRules.cs
│   │   │   │   │   │   │   └── TabuDomain.cs
│   │   │   │   │   │   ├── Presentation/
│   │   │   │   │   │   │   ├── TabuCardEditScreen.cs
│   │   │   │   │   │   │   ├── TabuScreens.cs
│   │   │   │   │   │   │   └── TabuWordsScreen.cs
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
│   │   │   │   │   ├── VerdadReto/
│   │   │   │   │   │   ├── Domain/
│   │   │   │   │   │   │   ├── Gamespot.Games.VerdadReto.Domain.asmdef
│   │   │   │   │   │   │   ├── VerdadRetoConfig.cs
│   │   │   │   │   │   │   └── VerdadRetoFlowController.cs
│   │   │   │   │   │   ├── Gamespot.Games.VerdadReto.asmdef
│   │   │   │   │   │   ├── VerdadRetoGame.cs
│   │   │   │   │   │   └── VerdadRetoScreens.cs
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
│   │   │   │   │       │   ├── NeverStatementText.cs
│   │   │   │   │       │   └── YoNuncaDomain.cs
│   │   │   │   │       ├── Gamespot.Games.YoNunca.asmdef
│   │   │   │   │       ├── YoNuncaGame.cs
│   │   │   │   │       └── YoNuncaPhrasesScreen.cs
│   │   │   │   └── Menu/
│   │   │   │       ├── Gamespot.Menu.asmdef
│   │   │   │       ├── LegalScreen.cs
│   │   │   │       ├── MenuScreen.cs
│   │   │   │       └── SettingsScreen.cs
│   │   │   ├── Settings/
│   │   │   │   └── GamespotVolume.asset
│   │   │   └── Tests/
│   │   │       ├── EditMode/
│   │   │       │   ├── Audio/
│   │   │       │   │   └── AudioClipsTests.cs
│   │   │       │   ├── Core/
│   │   │       │   │   ├── GuessGameTests.cs
│   │   │       │   │   ├── IcuMessageFormatterTests.cs
│   │   │       │   │   ├── JsonTests.cs
│   │   │       │   │   ├── NameNormalizerTests.cs
│   │   │       │   │   ├── PromptCardTests.cs
│   │   │       │   │   ├── RandomTests.cs
│   │   │       │   │   └── WordCategoriesTests.cs
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
│   │   │       │   │   ├── HombresLobo/
│   │   │       │   │   │   └── Domain/
│   │   │       │   │   │       ├── LoboConfigTests.cs
│   │   │       │   │   │       ├── LoboDayTests.cs
│   │   │       │   │   │       ├── LoboNightTests.cs
│   │   │       │   │   │       └── LoboTestKit.cs
│   │   │       │   │   ├── Impostor/
│   │   │       │   │   │   ├── Data/
│   │   │       │   │   │   │   ├── ImpostorDataTests.cs
│   │   │       │   │   │   │   └── ImpostorOwnCategoriesTests.cs
│   │   │       │   │   │   └── Domain/
│   │   │       │   │   │       ├── AssignRolesUseCaseTests.cs
│   │   │       │   │   │       ├── GameConfigTests.cs
│   │   │       │   │   │       ├── GameSessionTests.cs
│   │   │       │   │   │       ├── ImpostorFlowControllerTests.cs
│   │   │       │   │   │       ├── ImpostorStatsTests.cs
│   │   │       │   │   │       └── ImpostorTestData.cs
│   │   │       │   │   ├── MasProbable/
│   │   │       │   │   │   └── MasProbableFlowTests.cs
│   │   │       │   │   ├── Mimica/
│   │   │       │   │   │   └── MimicaConfigTests.cs
│   │   │       │   │   ├── QuePrefieres/
│   │   │       │   │   │   └── QuePrefieresFlowTests.cs
│   │   │       │   │   ├── QuienSoy/
│   │   │       │   │   │   └── QuienSoyTests.cs
│   │   │       │   │   ├── Shared/
│   │   │       │   │   │   ├── CustomCategoryRepositoryTests.cs
│   │   │       │   │   │   └── PromptCardRepositoryTests.cs
│   │   │       │   │   ├── Tabu/
│   │   │       │   │   │   ├── Domain/
│   │   │       │   │   │   │   ├── TabuCardRulesTests.cs
│   │   │       │   │   │   │   └── TabuDomainTests.cs
│   │   │       │   │   │   ├── TabuDataTests.cs
│   │   │       │   │   │   └── TabuOwnCardsTests.cs
│   │   │       │   │   ├── Trivia/
│   │   │       │   │   │   ├── Data/
│   │   │       │   │   │   │   └── TriviaDataTests.cs
│   │   │       │   │   │   └── Domain/
│   │   │       │   │   │       └── TriviaDomainTests.cs
│   │   │       │   │   ├── VerdadReto/
│   │   │       │   │   │   └── VerdadRetoFlowTests.cs
│   │   │       │   │   ├── Wavelength/
│   │   │       │   │   │   ├── Domain/
│   │   │       │   │   │   │   └── WavelengthDomainTests.cs
│   │   │       │   │   │   └── WavelengthDialAndDataTests.cs
│   │   │       │   │   ├── YoNunca/
│   │   │       │   │   │   ├── Domain/
│   │   │       │   │   │   │   ├── NeverStatementTextTests.cs
│   │   │       │   │   │   │   └── YoNuncaDomainTests.cs
│   │   │       │   │   │   └── YoNuncaPhrasesTests.cs
│   │   │       │   │   ├── PromptSeedFilesTests.cs
│   │   │       │   │   └── SeedFilesTests.cs
│   │   │       │   ├── Localization/
│   │   │       │   │   ├── GlyphCoverageTests.cs
│   │   │       │   │   └── LocTests.cs
│   │   │       │   ├── Theme/
│   │   │       │   │   └── ContrastTests.cs
│   │   │       │   ├── ArchitectureTests.cs
│   │   │       │   └── Gamespot.Tests.EditMode.asmdef
│   │   │       └── PlayMode/
│   │   │           ├── Games/
│   │   │           │   ├── BombaFlowTests.cs
│   │   │           │   ├── CategoryTests.cs
│   │   │           │   ├── EsUn10PeroScreenTests.cs
│   │   │           │   ├── ImpostorFlowTests.cs
│   │   │           │   ├── LoboSetupTests.cs
│   │   │           │   ├── MasProbableFlowTests.cs
│   │   │           │   ├── MimicaFlowTests.cs
│   │   │           │   ├── OwnCategoriesTests.cs
│   │   │           │   ├── PlayAgainTests.cs
│   │   │           │   ├── PlayerRosterTests.cs
│   │   │           │   ├── PromptCardsTests.cs
│   │   │           │   ├── QuePrefieresFlowTests.cs
│   │   │           │   ├── QuienSoyFlowTests.cs
│   │   │           │   ├── SoundTests.cs
│   │   │           │   ├── TabuFlowTests.cs
│   │   │           │   ├── TriviaFlowTests.cs
│   │   │           │   ├── VerdadRetoFlowTests.cs
│   │   │           │   ├── WavelengthFlowTests.cs
│   │   │           │   └── YoNuncaFlowTests.cs
│   │   │           ├── AccessibilityTests.cs
│   │   │           ├── AppTestHarness.cs
│   │   │           ├── BootstrapTests.cs
│   │   │           ├── Gamespot.Tests.PlayMode.asmdef
│   │   │           ├── PerformanceTests.cs
│   │   │           └── SafeAreaTests.cs
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
