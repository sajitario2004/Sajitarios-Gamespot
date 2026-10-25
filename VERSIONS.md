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
- [x] **V013 · Paleta y kit neón.** `Palette`, `NeonText`, `NeonPanel`, `NeonButton`, `NeonBackground`, `PulseGlow`, layouts (columna, fila, scroll), campos de texto, diálogos y snackbar.
  - Notas: Kit en `Core.Runtime/UI`: `NeonSprites` (rectángulos redondeados, bordes, círculos y halos generados por código, 9-slice a 2x), `Ui` (Column, Row, Space, Flex, Scroll, Text con halo por *underlay* de TMP, FittedText, Panel, Button, IconButton, Image, Divider, NumberBadge), `NeonPanel`, `NeonButton` (Filled/Outlined/Text, mín. 48), `NeonTextField`, `NeonSwitch`, `NeonChip`, `NeonStepper`, `AppBar`, `ScreenScaffold`, `PulseGlow` (respeta `AppSettings.ReduceMotion`), `CoverImage`, `ResponsiveGrid`, `Dialogs`, `Snackbar`. Recursos esenciales de TMP importados por código (LiberationSans SDF). En negrita no hay «…» hasta tener una fuente con variante bold real (V014): se corta sin puntos.
- [ ] **V014 · Tipografía e iconos.** Fuente TMP con glifos del español e iconos (Material Symbols, Apache-2.0). **Pedir permiso antes de descargar.** Palos de la carta como sprites propios.
- [x] **V015 · Router.** Rutas por texto, pila, botón atrás de Android, ruta desconocida, `BackToMenuButton`.
  - Notas: `AppScreen` + `Router` (Go/Push/Pop/Back, ruta padre, Escape = atrás de Android, salir en el menú, `NotFoundScreen`, reconstruye al cambiar de idioma). Atrás cierra antes el diálogo abierto.
- [x] **V016 · Audio.** `AudioService` con los 3 WAV, mute en `PlayerPrefs`, `MuteButton`.
  - Notas: `AudioService` (los 3 WAV del Flutter en `Resources/Audio`, precarga en el arranque, `audio_enabled` en PlayerPrefs, errores ignorados) y `MuteButton`. Un `AssetPostprocessor` importa `Resources/Images` como sprites y el audio como clips cortos.
- [x] **V017 · Compartidos.** `RulesScreen`, `AbandonGameDialog`, normalización de nombres, filas de jugadores reutilizables.
  - Notas: En `Games/_Shared`: `IGameDescriptor` (también `IDbSchema`) + `GameContext`, `GameUi.HomeButton/RulesButton/RuleKeys`, `AbandonGameDialog` (ya fuera del Impostor), `RulesScreen`, `PlayerListEditor` (filas numeradas, quitar en el mínimo deshabilitado, «Máximo de jugadores alcanzado») y `NotFoundScreen`. `LanguageSelector` en `Core.Runtime/Localization`.

## Fase 3 — Menú

- [x] **V018 · MenuScreen** (cabecera, rejilla 2/3 columnas, tarjetas cian/magenta, idioma, mute, catálogo vacío).
  - Notas: `MenuScreen` (solo recibe la lista de descriptores): título con PulseGlow 0.8–1.3, idioma y silencio, cabecera `menu_header` (cover, 140), rejilla 2/3 columnas proporción 0.9 con tarjetas cian/magenta, catálogo vacío. `Bootstrap` completo: Loc → UI → BD con todos los esquemas y seeds → audio → router → registro de juegos → menú. Captura `Docs/Capturas/V018_menu_vacio.png`.
- [x] **V019 · Tests del menú:** tantas tarjetas como descriptores; el asmdef del menú no referencia juegos.
  - Notas: PlayMode: arranque y menú, una tarjeta por juego y su ruta, atrás (padre y salir), ruta desconocida, reglas apiladas, atrás cierra el diálogo. EditMode `ArchitectureTests`: el menú no referencia juegos, los dominios son puros, ningún juego referencia a otro. 118 EditMode + 6 PlayMode en verde.

## Fase 4 — Es un 10 pero
- [x] **V020 · Dominio** `Card`, `DrawCardUseCase` + tests (las 40 cartas salen; misma seed, misma secuencia).
  - Notas: Hecho junto con V010 (dominio puro + 5 tests).
- [x] **V021 · Pantalla y cuenta atrás** de 5 s (botón deshabilitado, overlay, etiqueta "Sacar otra carta").
  - Notas: `EsUn10PeroScreen`: cuenta atrás 5 → 1 con `Tick` del router (un paso por segundo), botón deshabilitado, overlay al 72 % con «Sacando carta en...» y el número a 96 con halo; la carta se saca al terminar; «Sacar otra carta» tras la primera. Capturas `V021_cuenta_atras.png`.
- [x] **V022 · Carta neón con flip** 0.55 s *easeInOutCubic*, sombra, cara y dorso; SFX al revelar.
  - Notas: `CardView` en uGUI: dorso `card_back` recortado por un rectángulo redondeado (o dibujado si falta), cara con valor y palo en las esquinas (girada 180° abajo) y palo grande en el centro; volteo 0,55 s *easeInOutCubic* con la cara nueva a mitad y sombra cian que sube y se difumina; rojos en magenta y negros en cian. Palos dibujados por código (`SuitSprites`), sin emojis ni glifos. Captura `V022_carta.png`.
- [x] **V023 · Registro, reglas y test PlayMode.**
  - Notas: `EsUn10PeroGame` (ruta `/es-un-10-pero`, sin tablas) registrado en `GameRegistry`. Tests PlayMode: tarjeta en el menú, cuenta atrás, carta a los 5 s, sonido una vez y nada si está silenciado. 118 EditMode + 10 PlayMode en verde. Captura `V023_menu.png`.

## Fase 5 — El Impostor
- [x] **V024 · Dominio:** `Player`, `Role`, `Word`, `GameConfig`, `GameSession` + tests.
  - Notas: Adelantada (código puro): `Player` (igualdad exacta), `Role`, `Word` (recorta, pista obligatoria), `GameConfig` (errores en orden, impostores min(5, n−1), rondas max(1, n−3), nulo = máximo) y `GameSession` (orden de revelación = de entrada). Tests de configuración y sesión.
- [x] **V025 · `AssignRolesUseCase`** + tests (10.000 iteraciones 10/10/80, casos especiales con `FakeRandom`, capado, orden de revelación).
  - Notas: Adelantada: palabra → un solo `NextDouble()` → caso; reparto normal con Fisher-Yates sobre una copia. Tests: 10.000 iteraciones semilla 20240615 (±2/±2/±3 %), misma semilla = mismos conteos, bordes 0.10/0.20 y casos especiales con `ScriptedRandom`, capado, orden de revelación, palabra y pista.
- [x] **V026 · Datos:** esquema, `WordRepository` (CRUD, `NOCASE`, búsqueda con `ESCAPE`), seed de 2060 palabras + tests.
  - Notas: `ImpostorSchema` (tablas finales del Flutter, `IF NOT EXISTS`), `WordRepository` (orden NOCASE, `LIKE … ESCAPE '\'`, unicidad sin mayúsculas comprobada antes de escribir, errores en el orden del Flutter, seed de solo lectura) y seed de 2060 palabras validado (sin duplicados, `word ≠ hint`). **Arreglo en el núcleo:** `AppDatabase` asegura las tablas de todos los juegos en cada arranque; la versión solo sirve para cambiar tablas existentes (en Play, la BD creada antes de añadir el Impostor no tenía sus tablas).
- [x] **V027 · `GameHistoryRepository`** + tests.
  - Notas: `GameHistoryRepository` (`players_json` con `name`/`role`, la más reciente primero, borrar todo) e `ImpostorStats` puras (palabra más repetida sin mayúsculas con la grafía más reciente, ranking de impostores). Tests de datos con BD temporal.
- [x] **V028 · `ImpostorFlowController`** (fases y votación) + tests.
  - Notas: Adelantada: `ImpostorFlowController` puro en `Domain/Flow` (Iniciar con `IImpostorWordSource`, Revelar, Avanzar, Votar, Reiniciar; error SinPalabras/Desconocido). Tests de votación: pillar al único impostor, cada voto gasta ronda, hay que pillarlos a todos, re-votar eliminado no hace nada, sin impostores gana el impostor.
- [x] **V029 · Setup.**
  - Notas: `ImpostorSetupScreen` con `PlayerListEditor` (3–15), contadores de impostores y rondas que solo se recortan, pista, validación con avisos y diálogo «No hay palabras». El borrador del setup se conserva al ir a palabras o historial (mejora: en el Flutter se perdía). Captura `V029_impostor_setup.png`.
- [x] **V030 · Pasar el móvil y Revelar.**
  - Notas: `PassDeviceScreen` (círculo que late, nombre grande, Continuar) y `RevealScreen` (rol oculto hasta Revelar, sonido, impostor magenta con pista violeta, palabra cian; aparición 350 ms). Captura `V030_impostor_revelar.png`.
- [x] **V031 · Votación y Fin de partida.**
  - Notas: `VotingScreen` (ronda X de Y, candidatos, impostores pillados tachados, confirmación, aviso tras cada voto) y `GameOverScreen` (solo el desenlace, suena y guarda el historial una vez). Atrás interceptado en votación y fin. Captura `V031_impostor_votacion.png`.
- [x] **V032 · Gestión de palabras** (lista virtualizada, búsqueda con *debounce* de 280 ms, formulario).
  - Notas: `WordsScreen` con `VirtualList` (solo las filas visibles de 2060), búsqueda con espera de 280 ms, formulario que no se cierra con campos vacíos, borrado con confirmación y avisos de cada error. Captura `V032_impostor_palabras.png`.
- [x] **V033 · Historial y estadísticas.**
  - Notas: `HistoryScreen`: estadísticas, partidas desplegables con pista y rol de cada jugador, borrar todo con confirmación, estados vacío y de error.
- [x] **V034 · Registro, reglas y test PlayMode** del flujo completo.
  - Notas: `ImpostorGame` en `GameRegistry` (7 rutas). PlayMode: partida completa (5 revelaciones con sonido, votación, fin, historial guardado una vez, atrás vuelve al setup), validación, recorte de contadores, atrás con confirmación y borrador conservado. 135 EditMode + 15 PlayMode en verde.

## Fase 6 — Preguntas por puntos
- [x] **V035 · Dominio** + tests.
  - Notas: Dominio puro: `Difficulty` (nombres de BD y OpenTDB), `Tematicas`, `Question` (4 opciones, índice válido), `TriviaConfig` (2–6, nombres recortados, al menos una temática), `TriviaSession` inmutable (9 rondas, tramos de 3, eliminación, empate) y `DealQuestionsUseCase` (lista ordenada). Tests en el arnés .NET y en Unity.
- [x] **V036 · Datos** (preguntas y victorias) + seed de 1151 + tests.
  - Notas: `TriviaSchema` (`IF NOT EXISTS`, opciones en JSON), `QuestionRepository` (pool por temáticas IN (…) y dificultad, por id) y `WinnerRepository` (sumar sin mayúsculas en una transacción, ranking). Seed de 1151 con preguntas en las 9 temáticas × 3 dificultades.
- [x] **V037 · `TriviaFlowController`** + tests.
  - Notas: `TriviaFlowController` puro: reparte por ronda, el siguiente es el primer vivo tras el actual en el orden de la ronda, recarga pools al cerrar ronda, +1 victoria a cada superviviente, cualquier fallo de carga → SinPreguntas, acciones fuera de fase no hacen nada.
- [x] **V038 · Setup** (temáticas, ranking, atribución).
  - Notas: `TriviaSetupScreen`: jugadores 2–6 en violeta, chips de temáticas en rejilla de 2 (la última no se desmarca), ranking de victorias y atribución OpenTDB. Captura `V038_trivia_setup.png`.
- [x] **V039 · Pasar el móvil y Pregunta** (rejilla 2×2 con los 4 colores, 800 ms).
  - Notas: `TriviaPassScreen` y `TriviaQuestionScreen` (panel violeta, rejilla 2×2 azul/verde/rojo/amarillo, verde la correcta y rojo la elegida errónea, resto atenuado, 800 ms y avanza; bloqueada tras la primera pulsación). Arreglo del kit: un tamaño fijo con `Sized` ya no es flexible en ese eje (el círculo salía como píldora). Captura `V039_trivia_pasar.png`.
- [x] **V040 · Fin, registro, reglas y test PlayMode.**
  - Notas: `TriviaGameOverScreen` (ganadores o «Nadie ganó esta partida»), `TriviaGame` registrado. PlayMode: temáticas, todos fallan → nadie gana sin victorias, todo acertado → 18 respuestas y +1 victoria a los dos. 158 EditMode + 18 PlayMode en verde.

## Fase 7 — Wavelength
> El autor recuerda (2026-10-25) que **Wavelength está roto** en la app Flutter, aunque la v0.72 decía haberlo arreglado. No se porta su código tal cual: se reescribe desde la especificación de `CONTEXT.md` §6.4 con los arreglos de §8.1, y no se da por cerrado sin un test PlayMode de partida completa (pista → pasar → arrastrar el dial → revelado → siguiente ronda → fin) y una prueba en Play arrastrando el dial de verdad.

- [x] **V041 · Dominio** (puntos con bordes inclusivos) + tests.
  - Notas: Dominio puro: `Spectrum`, `WavelengthScoring` (bordes inclusivos con tolerancia 1e-9: en coma flotante 0,5 + 0,06 no daba «en el blanco»), `WavelengthRound`, `WavelengthConfig` (2–8, 1–20 rondas, 8 por defecto), `WavelengthSession` (psíquico rotatorio, marcador) y `PickRoundUseCase`.
- [x] **V042 · Datos** + seed de 80 + tests.
  - Notas: `WavelengthSchema` + `SpectrumRepository` y seed de 80 espectros (validado, idempotente).
- [x] **V043 · Dial** (`Graphic` uGUI con malla propia, arrastre, revelado 0.7 s).
  - Notas: `WavelengthDial`: `MaskableGraphic` con malla propia (fondo, 3 bandas, objetivo, arco con halo, 5 marcas, aguja con pomo). Recibe toque y arrastre directamente (`useDragThreshold = false`); bajo la base va al extremo más cercano; sin aguja en el modo del psíquico; revelado de 0,7 s con la aguja del grupo. **Gotcha:** el `CanvasRenderer` va explícito al crear el objeto (sin él la malla no se pintaba).
- [x] **V044 · `WavelengthFlowController`** + tests.
  - Notas: `WavelengthFlowController` puro (pista, pasar, adivinar, revelar, siguiente; fallo al cargar → fin). `ScoreIncludingCurrent` para enseñar el total con la ronda revelada.
- [x] **V045 · Pantallas** (setup, pista, pasar, adivinar, revelado, fin).
  - Notas: 6 pantallas; las del dial en columna SIN scroll (el dial no compite con ningún arrastre). Pista vacía avisa en vez de ignorarse; «Siguiente» con doble toque no salta rondas; media por ronda localizada; textos sin tilde corregidos en `es.json`. Capturas `V045_wavelength_pista/adivinar/revelado.png`.
- [x] **V046 · Registro, reglas y test PlayMode.**
  - Notas: `WavelengthGame` registrado. PlayMode: partida completa de 2 rondas arrastrando el dial con eventos de puntero reales (pantalla → punto local), aguja del revelado = la del grupo, banda y total con la ronda, animación terminada, y el arrastre lo recibe el dial (no hay scroll). 188 EditMode + 20 PlayMode en verde.

## Fase 8 — Tabú
- [x] **V047 · Dominio** + tests.
  - Notas: Dominio puro: `TabuWord` (4–5 prohibidas), `TabuConfig` (equipos, 30–120 s, objetivo 3), `TabuSession` inmutable (falta −1 sin bajar de 0 y sin cortar el turno, victoria de ronda con ≥1 acierto, sin repetir palabras) y `PickTabuWordUseCase`. Tests en el arnés y en Unity.
- [x] **V048 · Datos** + seed de 120 + tests.
  - Notas: `TabuSchema` + `TabuWordRepository` y seed de 120 palabras (4–5 prohibidas, sin duplicados). Ojo: el seed heredado trae palabras sin tildes («Volcan», «erupcion»): va al plan de mejoras.
- [x] **V049 · `TabuFlowController`** + tests.
  - Notas: `TabuFlowController` puro: acierto/saltar/falta pasan a una palabra nueva (si se acaban, se queda la actual), fin de turno → marcador o fin, acciones fuera de fase ignoradas.
- [x] **V050 · Pantallas** (setup, turno con temporizador, marcador, fin).
  - Notas: Setup (equipos + chips 30/60/90), turno (palabra en mayúsculas, prohibidas, cuenta atrás cian/amarillo/rojo, aciertos en la barra, botones compactos), marcador y fin. El temporizador se pausa con un diálogo abierto (arreglo). `NeonButton.Compact()` para botones estrechos en fila. Captura `V050_tabu_turno.png`.
- [x] **V051 · Registro, reglas y test PlayMode.**
  - Notas: `TabuGame` registrado. PlayMode: equipos iguales dan aviso, el tiempo termina el turno y da la victoria, pausa con el diálogo, primero a 3 gana. 190 EditMode + 24 PlayMode en verde.

## Fase 9 — Yo Nunca
- [x] **V052 · Dominio, datos y seed de 192** + tests.
  - Notas: Dominio puro (`Intensidad`, `NeverStatement`, `YoNuncaConfig`, `DrawStatementUseCase` sin repetir hasta agotar y luego vuelve a empezar, `YoNuncaFlowController`), esquema, repositorio por intensidades y seed de 192 frases (98 suaves, 94 picantes) validado.
- [x] **V053 · Flow y pantallas** (aviso +18).
  - Notas: Setup con chips Suave/Picante y aviso +18 fijo mientras Picante está activo; juego con la frase centrada (sin scroll, que recortaba el halo) y «Siguiente». Atrás y casa piden confirmación (arreglo). Capturas `V053_yo_nunca_setup/juego.png`.
- [x] **V054 · Registro, reglas y test PlayMode.**
  - Notas: `YoNuncaGame` registrado. PlayMode: aviso aparece y desaparece, sin intensidades da aviso, «Siguiente» cambia la frase, atrás confirma, mínimos del seed. 196 EditMode + 28 PlayMode en verde.

## Fase 10 — La Bomba
- [x] **V055 · Dominio** (mecha [10,60), rotación, explosión) + tests.
  - Notas: Dominio puro en `BombaDomain.cs`: `BombaMode`, `BombaPrompt`, `BombaConfig` (2–12 jugadores, mecha dentro de [10,60] y min < max), `BombaSession` inmutable (empieza el primero, pasar rota sin tocar la mecha, al explotar la bomba pasa al siguiente, gana el último) y `PickPromptUseCase` (sin repetir por modo hasta agotar). Tests portados del Flutter, también en el arnés .NET (168 en verde).
- [x] **V056 · Datos** + seeds (93 sílabas, 83 categorías) + tests.
  - Notas: `BombaSchema` (dos tablas, idempotente) y `BombaPromptRepository`; seeds de 93 sílabas y 83 categorías, cada tabla se siembra solo si está vacía y un elemento vacío o que no sea texto da error. Ojo: las categorías heredadas vienen sin tildes («paises»): va al plan de mejoras.
- [x] **V057 · `BombaFlowController`** + tests.
  - Notas: `BombaFlowController` puro: fases Setup → Jugando ⇄ Explotando → GameOver y Error(SinPrompts) si el modo no tiene contenido; pasar no reinicia la mecha; tras la explosión, ronda nueva con mecha y sílaba o categoría nuevas; acciones fuera de fase ignoradas.
- [x] **V058 · Pantallas** (setup, juego con mecha oculta y overlay de 1,5 s, fin).
  - Notas: Setup (chips Sílaba/Categoría en rojo, 2–12 jugadores con `PlayerListEditor`, nombres normalizados: arreglo), juego (portador en mayúsculas, prompt en panel rojo que late, «PASAR» de 64, pastilla de vivos con `AppBar.AddInfo`), aviso «¡BOOM!» a pantalla completa 1,5 s y fin con el ganador. La mecha se cuenta por frame y se pausa con un diálogo abierto (arreglo). Capturas `V058_bomba_setup/juego/explosion/fin.png`.
- [x] **V059 · Registro, reglas y test PlayMode.**
  - Notas: `BombaGame` registrado: el menú ya tiene los 7 juegos. PlayMode: errores del setup, partida completa (pasar, explotar, aviso de 1,5 s, ganador, salir reinicia) y mecha pausada con el diálogo de salir. 213 EditMode + 31 PlayMode en verde.

## Fase 10b — Plan de mejoras y juegos nuevos (petición del autor, 2026-10-25)
- [x] **V059a · Plan de mejoras** de los 7 juegos una vez portados (jugabilidad, contenido, UX y deuda de `CONTEXT.md` §8.2), con prioridades, para que el autor elija.
  - Notas: Plan en `Docs/PLAN_MEJORAS.md`: 11 mejoras comunes (C1–C11) y mejoras por juego (E, I, T, W, B, Y, L) con prioridad y esfuerzo; orden recomendado arriba. Basado en el análisis del contenido (tildes que faltan, tamaño de los bancos) y la deuda de `CONTEXT.md` §8.2. Pendiente de que el autor elija.
- [x] **V059b · Cinco juegos nuevos** propuestos (reglas, por qué encajan en un hub local de pasar el móvil, esfuerzo estimado), para que el autor elija cuáles añadir.
  - Notas: Cinco juegos propuestos en el mismo archivo (§3): Verdad o Reto (S), ¿Quién soy? (M), Dibuja y adivina (M), Código secreto (M–L) y Hombres Lobo con el móvil de narrador (L), más tres ideas pequeñas en el banquillo. Pendiente de que el autor elija.

## Fase 10c — Mejoras elegidas del plan (el autor, 2026-10-25: «C2, C3+L1, B1+B2, C4 e I1/T1/W2, sin descargar fuentes»)
- [x] **V059c · C2 · Jugadores guardados entre juegos.** Al empezar una partida se guardan los nombres; los setups con lista de jugadores (Impostor, Trivia, Wavelength, Bomba) arrancan con ellos si están vacíos, y "Usar los de la última partida" los recupera.
  - Acepta: test PlayMode que juega en un juego y abre otro con los nombres puestos; los tests no dejan nombres guardados.
  - Notas: `AppSettings.LastPlayers` (preferencia `last_players`, lista JSON) se guarda al empezar en Impostor, Trivia, Wavelength y Bomba. `PlayerListEditor` arranca con esos nombres si no hay ninguno escrito (recortados al máximo del juego) y enseña «Usar los de la última partida» cuando no coinciden. PlayMode `PlayerRosterTests` (4); el arnés restaura la preferencia. 35 PlayMode en verde. Captura `V059c_jugadores_guardados.png`.
- [x] **V059d · C3 + L1 · Sonido y vibración.** Sonidos sintetizados propios (sin descargas): tic-tac de La Bomba que acelera, explosión y vibración; pitidos de los últimos 5 s y bocina en Tabú; acierto y fallo en Trivia; revelado en Wavelength; pasar en Yo Nunca.
  - Acepta: los WAV se generan desde el Editor y suenan en su momento (test con el contador de `AudioService`).
  - Notas: `SoundSynth` (menú Gamespot → Audio → Generar sonidos sintetizados) crea tick, tock, explosion, beep, buzzer, correct, wrong, chime y swipe (44,1 kHz, mono, 16 bit, semilla fija). `AudioService` con 12 sonidos y `CountOf`; `Haptics` + preferencia `vibration_enabled`. La Bomba: tic-tac que acelera (0,9 → 0,18 s) y explosión con vibración; Tabú: pitidos de 5 a 1 y bocina; Trivia: acierto/fallo; Wavelength: campanas al confirmar; Yo Nunca: pasar. EditMode `AudioClipsTests` y PlayMode `SoundTests` (5). 214 EditMode + 40 PlayMode en verde. Sin escuchar: falta probarlos en el móvil.
- [x] **V059e · B1 + B2 · Tabú con 500+ palabras y tildes.** Seed ampliado y corregido; si se agotan las palabras, aviso y vuelven a salir; la BD sube a la versión 2 y renueva el seed del Tabú en las instalaciones existentes.
  - Acepta: seed ≥ 500 sin duplicados ni prohibidas repetidas; test de la migración 1 → 2; test del aviso.
  - Notas: Seed de 607 tarjetas con 5 prohibidas (las 120 heredadas corregidas y 487 nuevas por temas: animales, comida, casa, profesiones, deportes, lugares, transporte, naturaleza, cuerpo, fiestas, ocio, ropa, música, fantasía, acciones y colegio), generado desde `tabu_cards.txt` con validación. `TabuSession.ReiniciarPalabras` + aviso `tabuPalabrasAgotadas` una vez. `AppDatabase.CurrentVersion = 2` y `TabuSchema.Upgrade` renueva el seed; el seed del Tabú mira solo filas de seed. Tests: dominio (agotar y una sola palabra), datos (≥ 500, prohibidas, tildes, migración 1 → 2 conservando propias). 219 EditMode + 40 PlayMode; arnés .NET 170. Captura `V059e_tabu_tildes.png`.
- [x] **V059f · C4 · Jugar otra con los mismos jugadores** en Tabú, Trivia, Wavelength y La Bomba (empieza al momento con los mismos ajustes), más "Cambiar ajustes" para volver al setup.
  - Acepta: test PlayMode por juego.
  - Notas: `PlayAgain()` y `ResetToSetup()` en los módulos de Tabú, Trivia, Wavelength y La Bomba; sus pantallas de fin tienen «Jugar otra» (principal), «Cambiar ajustes» y «Volver al menú». Trivia y Wavelength ya tenían «Jugar otra», pero llevaba al setup. PlayMode `PlayAgainTests` (4: misma configuración, marcador o mecha a cero, cambiar ajustes conserva el borrador). 44 PlayMode en verde. Captura `V059f_bomba_jugar_otra.png`.
- [x] **V059g · I1 / T1 / W2.** Rondas del Impostor: empiezan en el máximo y lo siguen hasta que se tocan. Trivia sin preguntas repetidas en la partida y Wavelength sin espectros repetidos (si no quedan, vuelven a salir).
  - Acepta: tests de dominio y de pantalla.
  - Notas: I1: `ImpostorSetupDraft.RoundsTouched`; las rondas empiezan en max(1, n−3) y lo siguen hasta que se pulsa el contador, luego solo se recortan. T1: `TriviaFlowController` reparte con las preguntas no salidas (si no llegan para los vivos, con todo el pool). W2: `WavelengthFlowController` elige entre los espectros no salidos (si se agotan, todos). Tests: Trivia (sin repetir en 27 preguntas; con pool corto repite sin error), Wavelength (3 espectros antes de repetir), Impostor PlayMode (rondas siguen y se recortan). 222 EditMode + 45 PlayMode; arnés .NET 173. Captura `V059g_impostor_rondas.png`.

## Fase 10d — Palabras de fiesta y de España (el autor, 2026-10-25)
- [x] **V059h · Impostor y Tabú con famosos, sitios y fiesta.** Petición: «más palabras, a ser posible de España: políticos, streamers, juegos famosos, creadores y creadoras de contenido, tipos de alcohol… y otras cosas graciosas de fiesta; también en el Tabú», y después «también personas famosas o sitios famosos».
  - Acepta: bancos ampliados sin duplicados; la BD sube a la versión 3 y lleva las palabras nuevas a instalaciones existentes sin tocar las propias; los nombres largos se leen enteros.
  - Notas: Impostor 2060 → 2724 (+664) y Tabú 607 → 1076 (+469): política y realeza, streamers y creadoras/es, música, tele, deporte, famosos del mundo, ficción, videojuegos, juegos de mesa, alcohol, fiesta, ligar y redes, canciones, series, memes, marcas y sitios de España y del mundo. Pistas neutras para personas reales. BD v3: el Tabú renueva el seed y el Impostor completa el suyo sin tocar las propias (`TopUpSeed`). Arreglo de UI para nombres largos (`Ui.Text` de una línea, `FittedText(lines: 2)`, lista de palabras). Tests: migración a 3, contenido de los seeds. 225 EditMode + 45 PlayMode. Capturas `V059h_*`.

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
