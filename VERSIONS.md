# VERSIONS.md — Hoja de ruta de la migración a Unity

Cada versión es una pieza pequeña con su criterio de aceptación. Al empezar se lee **solo** la versión en curso (la primera sin marcar). Al terminar: `[x]`, notas de lo que haya cambiado, `CONTEXT.md` actualizado, `TREE.md` regenerado si hay archivos nuevos, y commit.

Cierre técnico de cada versión con código Unity: recompilación sin errores, consola limpia, tests verdes y, si hay pantalla, captura en Play.

---

## Fase 0 — Forma de trabajo y reorganización del repo

- [x] **V001 · Rama y Flutter a Legacy.** Rama `feature/unity-port`; todo el Flutter movido con `git mv` a `Legacy/Flutter/` (docs antiguos en `Legacy/Flutter/docs/`); hook `dart format` fuera de `.claude/settings.json`.
  - Acepta: la raíz solo tiene `.claude/`, `Legacy/`, los .md nuevos y la config de git; `git status` muestra renombres, no borrados.
- [x] **V002 · Archivos .md.** `CLAUDE.md`, `CONTEXT.md` (con la especificación de los 7 juegos), `LEARNINGS.md`, `VERSIONS.md`, `README.md`.
  - Acepta: una IA nueva entiende qué hacer leyendo solo esos archivos.
- [x] **V003 · Git y herramientas.** `.gitignore` (Unity + Legacy), `.gitattributes` (LFS + YAML de Unity), `tools/UnityCli.ps1`, `tools/Update-Tree.ps1`, `tools/Capture-GameView.ps1` (adaptados de AZÚCAR), `TREE.md` generado.
  - Acepta: `Update-Tree.ps1` genera `TREE.md` sin `Library/` ni `*.meta`.
- [x] **V004 · Memoria y commit.** Notas en la memoria de Claude del proyecto; commit de la Fase 0.
  - Notas: `.gitattributes` en la raíz (las macros solo funcionan ahí) con `Legacy/**` fuera de LFS para no tocar los binarios ya versionados del Flutter. `.gitignore` en la raíz con las rutas de Unity ancladas a `SajitariosGamespot/`.

## Fase 1 — Proyecto Unity base

- [x] **V005 · Crear el proyecto** `SajitariosGamespot/` con Unity 6000.3.27f1 y URP (skill `unity:new-unity-project`). *Visible Meta Files*, *Force Text*.
  - Acepta: el proyecto abre sin errores y la CLI conecta (`U editor status` → `ready`).
  - Notas: plantilla `com.unity.template.universal-2d` (URP con Renderer 2D), creada con `--with-pipeline --no-cloud` (sin proyecto en Unity Cloud).
- [x] **V006 · Paquetes.** Input System, Test Framework, uGUI (TMP), `com.unity.pipeline`, sqlite-net (`https://github.com/gilzoide/unity-sqlite-net.git#1.3.3`).
  - Acepta: `manifest.json` con los paquetes; recompila sin errores; licencias anotadas en `CONTEXT.md` §9.
  - Notas: Input System 1.20.1, Test Framework 1.6.0, uGUI 2.0.0, Pipeline 0.9.1-exp.1 y URP 17.3.0 venían con la plantilla. sqlite-net con `package add` (asmdef `Gilzoide.SqliteNet`, namespace `SQLite`). La plantilla trae paquetes 2D, Visual Scripting, Multiplayer Center y `collab-proxy` que no se usan: se pueden quitar con permiso del autor.
- [x] **V007 · Player Settings por código** (`Gamespot → Configuración → Aplicar todo`): compañía, producto, id `com.sajitarios.sajitarios_gamespot`, vertical, IL2CPP + ARM64, sin splash de Unity, fondo `#0A0A14`.
  - Acepta: `git diff ProjectSettings/` muestra los valores; repetir el menú no cambia nada.
  - Notas: ids iguales a los del Flutter: Android `com.sajitarios.sajitarios_gamespot`, iOS `com.sajitarios.sajitariosGamespot` (iOS no admite `_`). Versión 0.76.0 (76), siguiendo la numeración del Flutter.
- [x] **V008 · Estructura y assemblies** de `CONTEXT.md` §4, con `Gamespot.Core` y los `*.Domain` sin referencias a UnityEngine, y los asmdef de tests.
  - Acepta: compila; un test EditMode vacío pasa.
  - Notas: asmdef: `Gamespot.Core` (puro), `Gamespot.Core.Runtime`, `Gamespot.App`, `Gamespot.Editor`, `Gamespot.Tests.EditMode/PlayMode` y los `Gamespot.Games.<Juego>.Domain` (puros) según se añaden juegos.
- [x] **V009 · Escena App y arranque.** `App.unity` con cámara URP, EventSystem (Input System), Canvas *Screen Space – Camera* con `CanvasScaler` 390×844 / 0.5, `SafeArea`, Bloom, `Bootstrap`.
  - Acepta: Play muestra el fondo neón vacío sin errores en consola.
  - Notas: Captura `Docs/Capturas/V009_fondo.png`. La UI la crea el código (`UiRoot`): la escena solo tiene cámara, volumen con Bloom, EventSystem y `AppRoot`.

## Fase 2 — Núcleo

- [x] **V010 · Random.** `IRandomProvider`, `SeededRandom`, `FakeRandom`, `Shuffle` (Fisher-Yates del Flutter). Tests.
  - Notas: 101 tests EditMode en verde (con los dominios de V020/V024/V025/V028).
- [x] **V011 · AppDatabase** (sqlite-net): archivo, `user_version`, hooks de esquema por descriptor, seed loader genérico idempotente. Tests con BD temporal.
  - Notas: `IDb` (Execute/Scalar/Query con alias/InTransaction) sobre sqlite-net; `AppDatabase.Open` crea o migra con `PRAGMA user_version` en una transacción y siembra cada juego solo si su tabla está vacía. 7 tests con archivo temporal (108 EditMode en total).
- [x] **V012 · Localización.** `Loc.T`, placeholders y plurales ICU, importador de ARB → `es.json`/`en.json`, idioma en `PlayerPrefs`, selector. Tests (paridad de claves, plurales).
  - Notas: `Loc.T(clave, (nombre, valor)…)` estático (se reinicia en `SubsystemRegistration`), español por defecto, inglés cae al español si falta una clave. `Gamespot → i18n → Importar ARB del Flutter` añade solo las claves que faltan (305 por idioma). 26 claves nuevas de Unity (títulos del menú de 5 juegos, temáticas, palos, media de Wavelength, cargando, reducir animaciones) y la regla 4 del Tabú corregida: 331 claves por idioma. El selector de idioma va en V017.
- [ ] **V013 · Paleta y kit neón.** `Palette`, `NeonText`, `NeonPanel`, `NeonButton`, `NeonBackground`, `PulseGlow`, layouts (columna, fila, scroll), campos de texto, diálogos y snackbar.
- [ ] **V014 · Tipografía e iconos.** Fuente TMP con glifos del español e iconos (Material Symbols, Apache-2.0). **Pedir permiso antes de descargar.** Palos de la carta como sprites propios.
- [ ] **V015 · Router.** Rutas por texto, pila, botón atrás de Android, ruta desconocida, `BackToMenuButton`.
- [ ] **V016 · Audio.** `AudioService` con los 3 WAV, mute en `PlayerPrefs`, `MuteButton`.
- [ ] **V017 · Compartidos.** `RulesScreen`, `AbandonGameDialog`, normalización de nombres, filas de jugadores reutilizables.

## Fase 3 — Menú

- [ ] **V018 · MenuScreen** (cabecera, rejilla 2/3 columnas, tarjetas cian/magenta, idioma, mute, catálogo vacío).
- [ ] **V019 · Tests del menú:** tantas tarjetas como descriptores; el asmdef del menú no referencia juegos.

## Fase 4 — Es un 10 pero
- [ ] **V020 · Dominio** `Card`, `DrawCardUseCase` + tests (las 40 cartas salen; misma seed, misma secuencia).
- [ ] **V021 · Pantalla y cuenta atrás** de 5 s (botón deshabilitado, overlay, etiqueta "Sacar otra carta").
- [ ] **V022 · Carta neón con flip** 0.55 s *easeInOutCubic*, sombra, cara y dorso; SFX al revelar.
- [ ] **V023 · Registro, reglas y test PlayMode.**

## Fase 5 — El Impostor
- [ ] **V024 · Dominio:** `Player`, `Role`, `Word`, `GameConfig`, `GameSession` + tests.
- [ ] **V025 · `AssignRolesUseCase`** + tests (10.000 iteraciones 10/10/80, casos especiales con `FakeRandom`, capado, orden de revelación).
- [ ] **V026 · Datos:** esquema, `WordRepository` (CRUD, `NOCASE`, búsqueda con `ESCAPE`), seed de 2060 palabras + tests.
- [ ] **V027 · `GameHistoryRepository`** + tests.
- [ ] **V028 · `ImpostorFlowController`** (fases y votación) + tests.
- [ ] **V029 · Setup.**
- [ ] **V030 · Pasar el móvil y Revelar.**
- [ ] **V031 · Votación y Fin de partida.**
- [ ] **V032 · Gestión de palabras** (lista virtualizada, búsqueda con *debounce* de 280 ms, formulario).
- [ ] **V033 · Historial y estadísticas.**
- [ ] **V034 · Registro, reglas y test PlayMode** del flujo completo.

## Fase 6 — Preguntas por puntos
- [ ] **V035 · Dominio** + tests.
- [ ] **V036 · Datos** (preguntas y victorias) + seed de 1151 + tests.
- [ ] **V037 · `TriviaFlowController`** + tests.
- [ ] **V038 · Setup** (temáticas, ranking, atribución).
- [ ] **V039 · Pasar el móvil y Pregunta** (rejilla 2×2 con los 4 colores, 800 ms).
- [ ] **V040 · Fin, registro, reglas y test PlayMode.**

## Fase 7 — Wavelength
- [ ] **V041 · Dominio** (puntos con bordes inclusivos) + tests.
- [ ] **V042 · Datos** + seed de 80 + tests.
- [ ] **V043 · Dial** (`Graphic` uGUI con malla propia, arrastre, revelado 0.7 s).
- [ ] **V044 · `WavelengthFlowController`** + tests.
- [ ] **V045 · Pantallas** (setup, pista, pasar, adivinar, revelado, fin).
- [ ] **V046 · Registro, reglas y test PlayMode.**

## Fase 8 — Tabú
- [ ] **V047 · Dominio** + tests.
- [ ] **V048 · Datos** + seed de 120 + tests.
- [ ] **V049 · `TabuFlowController`** + tests.
- [ ] **V050 · Pantallas** (setup, turno con temporizador, marcador, fin).
- [ ] **V051 · Registro, reglas y test PlayMode.**

## Fase 9 — Yo Nunca
- [ ] **V052 · Dominio, datos y seed de 192** + tests.
- [ ] **V053 · Flow y pantallas** (aviso +18).
- [ ] **V054 · Registro, reglas y test PlayMode.**

## Fase 10 — La Bomba
- [ ] **V055 · Dominio** (mecha [10,60), rotación, explosión) + tests.
- [ ] **V056 · Datos** + seeds (93 sílabas, 83 categorías) + tests.
- [ ] **V057 · `BombaFlowController`** + tests.
- [ ] **V058 · Pantallas** (setup, juego con mecha oculta y overlay de 1,5 s, fin).
- [ ] **V059 · Registro, reglas y test PlayMode.**

## Fase 11 — Pulido y paridad
- [ ] **V060 · Icono y arranque** (placeholders con la paleta neón, mismos nombres).
- [ ] **V061 · Barrido responsive** (pantallas pequeñas, notch, textos largos).
- [ ] **V062 · Accesibilidad básica** (objetivos táctiles ≥ 48 dp, contraste, "reducir animaciones").
- [ ] **V063 · Rendimiento en reposo** (sin `Update` vacíos, Bloom ligero en móvil).
- [ ] **V064 · Lista de paridad** pantalla a pantalla contra el Flutter.

## Fase 12 — Builds y cierre
- [ ] **V065 · Build Android** (`.apk` release y desarrollo).
- [ ] **V066 · Prueba en emulador o dispositivo:** una partida de cada juego; palabras propias e historial persisten tras cerrar.
- [ ] **V067 · Proyecto de Xcode** para iOS (compilar y firmar en un Mac).
- [ ] **V068 · Validación del autor.**
- [ ] **V069 · Cierre** (con permiso): borrar `Legacy/Flutter/`, `TREE.md`, PR de `feature/unity-port` a `main`.
