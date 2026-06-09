# Versiones — Sajitarios Gamespot

Roadmap incremental del desarrollo, dividido en **30 versiones (0.1 → 0.30)**. Cada versión es un hito coherente y entregable. Es un plan vivo y "de momento": de la 0.31 en adelante quedan los temas marcados como *fuera de alcance* en `plan.md` (multijugador online, cuentas/cloud, internacionalización, estadísticas/historial).

Cada versión indica:
- **Objetivo** — qué problema resuelve.
- **Alcance** — qué entra.
- **Hecho cuando** — criterio de cierre.

Stack y reglas según `plan.md` y `context.md`: Flutter + **Flame** + Riverpod + go_router + sqflite, Screaming Architecture, copy en español.

---

## Cimientos (Fase F1)

### 0.1 — Scaffold del proyecto
- **Objetivo:** tener una app Flutter que arranca con la estructura del proyecto lista.
- **Alcance:** `flutter create`, dependencias (`flutter_riverpod`, `go_router`, `flame`, `sqflite`, `path`, `path_provider`), árbol de carpetas Screaming Architecture (`lib/core`, `lib/games/_shared`, `lib/menu`) vacío.
- **Hecho cuando:** `flutter run` levanta una app vacía en Android e iOS sin errores.

### 0.2 — Theme base y arranque
- **Objetivo:** identidad visual mínima y bootstrap de estado.
- **Alcance:** `core/theme` (Material theme, colores, tipografía); `main.dart` envuelto en `ProviderScope`.
- **Hecho cuando:** la app aplica el theme propio y arranca bajo Riverpod.

### 0.3 — Routing y menú placeholder
- **Objetivo:** navegación declarativa lista para crecer.
- **Alcance:** `core/routing` con `go_router` (`MaterialApp.router`); `MenuScreen` placeholder.
- **Hecho cuando:** la ruta inicial muestra el menú y la navegación funciona.

### 0.4 — Game Registry (extensibilidad)
- **Objetivo:** el núcleo que permite añadir juegos sin tocar el menú.
- **Alcance:** `GameDescriptor`, `GameRegistry`, `gameRegistryProvider`; `MenuScreen` pinta una grid leyendo el registry (sin acoplar juegos concretos).
- **Hecho cuando:** el menú renderiza la lista de juegos desde el provider (aún vacío o con placeholder).

### 0.5 — RandomProvider inyectable
- **Objetivo:** aleatoriedad mockeable para tests deterministas.
- **Alcance:** `core/random/RandomProvider` (wrapper sobre `Random`) + tests unitarios.
- **Hecho cuando:** un test puede inyectar un `Random` con seed fija y obtener resultados reproducibles.

---

## Juego 1 — "Es un 10 pero" (Fase F2 + Flame)

### 0.6 — Dominio de la carta
- **Objetivo:** modelar la carta y su obtención.
- **Alcance:** `Card` (value A/2–10, suit ♠♥♦♣) y `DrawCardUseCase(RandomProvider)`.
- **Hecho cuando:** el use case devuelve una carta válida.

### 0.7 — Tests del DrawCardUseCase
- **Objetivo:** garantizar cartas siempre válidas.
- **Alcance:** tests con `Random` mockeado (valores 1–10, 4 palos, sin J/Q/K).
- **Hecho cuando:** la suite pasa y cubre los límites.

### 0.8 — UI del juego de cartas
- **Objetivo:** poder sacar cartas desde el menú.
- **Alcance:** pantalla con botón grande "Sacar carta" que muestra la carta; registro del juego en `GameRegistry`.
- **Hecho cuando:** desde el menú se entra al juego y cada pulsación muestra una carta nueva.

### 0.9 — Animación de flip con Flame
- **Objetivo:** dar vida a la carta.
- **Alcance:** la carta como componente **Flame** dentro de un `GameWidget` con animación de volteo.
- **Hecho cuando:** al sacar carta se ve la animación de flip fluida.

---

## Base de datos del Impostor (Fase F3)

### 0.10 — AppDatabase y migraciones
- **Objetivo:** persistencia local versionada.
- **Alcance:** `core/db/AppDatabase` singleton sqflite + esquema de migraciones por `version`.
- **Hecho cuando:** la base se crea/abre y soporta migraciones.

### 0.11 — Tabla de palabras y seed
- **Objetivo:** datos iniciales del Impostor.
- **Alcance:** tabla `impostor_words` (+ índice); seed loader desde `assets/seed/impostor_words.json` cuando la tabla está vacía.
- **Hecho cuando:** primer arranque carga las palabras seed con su pista.

### 0.12 — WordRepository
- **Objetivo:** acceso a datos de palabras.
- **Alcance:** `WordRepository` (CRUD sqflite) + tests con sqflite in-memory.
- **Hecho cuando:** CRUD y carga de seed verificados por tests.

---

## Lógica del Impostor (Fase F4)

### 0.13 — Dominio del Impostor
- **Objetivo:** modelar la partida.
- **Alcance:** `Word`, `GameConfig`, `Player`, `Role`, `GameSession`.
- **Hecho cuando:** los tipos representan una partida completa.

### 0.14 — AssignRolesUseCase
- **Objetivo:** encapsular las reglas probabilísticas (núcleo del proyecto).
- **Alcance:** reglas con un `rng.nextDouble()`: `<0.10` todos impostores, `<0.20` ninguno, resto asignación normal de `nImpostores`; elección de palabra random + pista.
- **Hecho cuando:** el use case devuelve un `GameSession` correcto en los tres casos.

### 0.15 — Suite de tests de asignación
- **Objetivo:** blindar las reglas críticas.
- **Alcance:** ~10.000 iteraciones con seed fija (distribución ~10/10/80), tests de los dos casos especiales y del capado a `players-1`.
- **Hecho cuando:** la distribución y los edge cases pasan de forma estable.

---

## UI del Impostor (Fase F5)

### 0.16 — SetupScreen: jugadores
- **Objetivo:** introducir la lista de jugadores.
- **Alcance:** lista ordenada, mín. 3 / máx. 15, con validación en UI.
- **Hecho cuando:** no se puede continuar fuera de los límites.

### 0.17 — SetupScreen: impostores y pista
- **Objetivo:** completar la configuración de partida.
- **Alcance:** stepper/slider de nº impostores (1–5, capado a `players-1`) + switch de pista on/off.
- **Hecho cuando:** la configuración queda lista para iniciar la partida.

### 0.18 — PassDeviceScreen
- **Objetivo:** pantalla intermedia de paso de móvil.
- **Alcance:** instrucción "pásale el móvil a {jugador}" antes de cada revelación.
- **Hecho cuando:** aparece el jugador correcto según el orden de introducción.

### 0.19 — RevealScreen
- **Objetivo:** revelar el rol de cada jugador.
- **Alcance:** botón "Revelar" → palabra, o "IMPOSTOR" (+ pista si está activa); botón "Ocultar y pasar".
- **Hecho cuando:** el comportamiento coincide con las reglas de `context.md`.

### 0.20 — Flujo de partida completo
- **Objetivo:** recorrer toda la ronda.
- **Alcance:** navegación setup → pass → reveal por cada jugador en orden de introducción.
- **Hecho cuando:** una partida fluye de principio a fin sin saltos de orden.

### 0.21 — ResultsScreen
- **Objetivo:** cierre de partida.
- **Alcance:** pantalla final que muestra a todos los jugadores y su rol.
- **Hecho cuando:** al terminar el último jugador se ve el resultado completo.

---

## CRUD de palabras (Fase F6)

### 0.22 — Listado y búsqueda
- **Objetivo:** gestionar el banco de palabras.
- **Alcance:** listar palabras (seed + propias) con búsqueda por texto.
- **Hecho cuando:** se ven todas las palabras y el filtro funciona.

### 0.23 — Agregar palabra
- **Objetivo:** ampliar el banco desde la app.
- **Alcance:** alta con `word` + `hint` requeridos y validación de unicidad.
- **Hecho cuando:** se crea una palabra propia válida y puede salir en partida.

### 0.24 — Editar y borrar
- **Objetivo:** mantener las palabras propias.
- **Alcance:** editar/borrar solo `is_seed = 0`; las seed quedan en solo lectura.
- **Hecho cuando:** las propias se editan/borran y las seed no.

---

## Pulido y estabilización (Fase F7)

### 0.25 — Iconos y splash
- **Objetivo:** primera impresión cuidada.
- **Alcance:** iconos por juego en el menú + splash screen.
- **Hecho cuando:** la app tiene icono, splash y menú con iconos.

### 0.26 — Pulido de animaciones (Flame)
- **Objetivo:** transiciones agradables.
- **Alcance:** refinar el flip de carta y las transiciones de reveal con Flame.
- **Hecho cuando:** las animaciones son fluidas y sin parpadeos.

### 0.27 — Copy y UX en español- **Objetivo:** lenguaje claro y consistente.
- **Alcance:** mensajes, estados vacíos, confirmaciones y textos revisados.
- **Hecho cuando:** todos los textos están en español y son coherentes.
- **Notas de implementación:**
  - Revisado todo el copy (menú, "Es un 10 pero", flujo Impostor, gestión de
    palabras): eliminado el anglicismo "party games" del estado vacío del menú;
    pantalla de ruta no encontrada con título y mensaje más claros (sin volcar
    el error técnico al usuario).
  - FIX: en `ResultsScreen`, "Volver al menú" ahora llama a `flow.reiniciar()`
    antes de salir, para no dejar la sesión terminada viva en el controlador.
  - FIX: botón "atrás" del sistema en `PassDeviceScreen` y `RevealScreen`
    interceptado con `PopScope` + diálogo de confirmación
    (`abandonarPartidaDialog`): al confirmar reinicia el flujo y vuelve a setup;
    al cancelar permanece en la partida. Evita estados incoherentes y filtrado
    de rol. El avance hacia adelante no se ve afectado.

### 0.28 — Tests de widget e integración- **Objetivo:** cubrir la UI y los flujos.
- **Alcance:** tests de widget de pantallas clave + integración e2e (flujo Impostor y flujo carta).
- **Hecho cuando:** los flujos principales pasan en CI/local.
- **Notas de implementación:**
  - INTEGRACIÓN: añadida la dependencia `integration_test` (dev) y la carpeta
    `integration_test/`. `integration_test/impostor_flow_test.dart` recorre el
    flujo end-to-end del Impostor por la **UI y el router reales** (menú →
    setup → pass → reveal de cada jugador → results) con datos deterministas:
    `randomProvider.overrideWithValue(RandomProvider.seeded(1))` +
    `wordRepositoryProvider` sustituido por un `FakeWordRepository.single()`
    (una sola palabra, sin SQLite/path_provider). Con una sola palabra el
    `pick` consume una posición fija y la tirada de rama queda determinada:
    seed 1 → rama normal (1 impostor). Verifica que **exactamente uno** ve
    "IMPOSTOR" y se llega a resultados; una variante activa la pista y verifica
    que el impostor ve "Pista" + el valor de la pista y nunca la palabra. Script
    host `test_driver/integration_test.dart` para `flutter drive`.
  - LIMITACIÓN DE ENTORNO: `flutter test integration_test` / `flutter drive`
    requieren un dispositivo Android/iOS (o plataforma macOS/web generada). En
    este entorno solo hay macOS/Chrome no soportados por el proyecto, así que la
    suite e2e se documenta pero no se ejecutó aquí; el test compila y
    `flutter analyze` pasa sin avisos.
  - WIDGET: `test/games/es_un_10_pero/presentation/es_un_10_pero_screen_test.dart`
    verifica que sacar carta cambia el estado (la pista de carta vacía
    desaparece y el botón pasa de "Sacar carta" a "Sacar otra carta").
  - LÓGICA (observación del revisor de F4): nueva suite
    `test/games/impostor/domain/assign_roles_branches_test.dart` que distingue
    las ramas **por RAMA** y no solo por recuento. Fija cada rama por seed y
    ancla la tirada (`roll`): seed 8 → TODOS (roll < 0.10), seed 0 → NINGUNO
    (0.10 ≤ roll < 0.20), seed 1 → NORMAL (roll ≥ 0.20), comprobando además el
    rol concreto de cada jugador para que "todos" y "ninguno" no se confundan.
  - TESTS: +6 en `test/` (124 → 130 verdes) + 2 tests e2e en
    `integration_test/`. `flutter analyze` limpio.

### 0.29 — Responsive, accesibilidad y errores- **Objetivo:** robustez y alcance de dispositivos.
- **Alcance:** layouts responsive (móvil/tablet), accesibilidad básica y manejo de errores/edge cases.
- **Hecho cuando:** la app se ve bien en varios tamaños y no rompe en casos límite.
- **Notas de implementación:**
  - RESPONSIVE: `MenuScreen` ahora centra y limita la grid a `maxWidth: 900`,
    y adapta el `childAspectRatio` al `textScaler` (clamp 1.0–2.0) para no
    desbordar con texto grande; títulos/descripciones en `Flexible` con
    `maxLines`. El resto de pantallas ya usaban `ConstrainedBox` +
    `LayoutBuilder`.
  - ACCESIBILIDAD: tarjetas del menú envueltas en `Semantics(button: true,
    label: 'Jugar a {título}. {descripción}')` con `ExcludeSemantics` interno
    para no duplicar la lectura. Tiles de jugador en `ResultsScreen` con un
    único nodo semántico "{nombre}: era impostor / sabía la palabra". Campos de
    jugador con `labelText`. Botones de revelar/ocultar con
    `minimumSize: Size.fromHeight(56)` (objetivo táctil cómodo). Avatar de
    número excluido de semántica.
  - ERRORES/EDGE CASES: BD sin palabras al iniciar el Impostor ya no se
    resuelve con un snackbar fugaz: el flow controller expone un
    `ImpostorErrorKind.sinPalabras` y `SetupScreen` muestra un diálogo claro
    que ofrece ir a "Gestionar palabras". Nombres con espacios internos
    repetidos se normalizan (`\s+` → un espacio) para que la detección de
    duplicados funcione. `RevealScreen` en estado sin partida ofrece un botón
    "Configurar partida" en vez de dejar al usuario atascado.
  - TESTS: +5 (124 verdes). 2 para el diálogo "No hay palabras" y su
    navegación, 1 para la normalización de nombres con espacios, 2 para la
    accesibilidad del menú (Semantics de botón y ausencia de overflow con
    textScaler 2.0).

### 0.30 — Estabilización pre-release- **Objetivo:** dejar la app lista para un primer release.
- **Alcance:** `flutter analyze` sin warnings, cobertura de tests, versionado, README de build y builds de prueba Android/iOS.
- **Hecho cuando:** se generan builds instalables en ambas plataformas sin warnings.
- **Notas de implementación:**
  - ANALYZE: `flutter analyze` limpio, 0 issues (no hizo falta `dart fix`).
  - TESTS: `flutter test` 130/130 verdes. Cobertura global ~75.5% de líneas
    (impostor 78.8%, es_un_10_pero 71.9%, menu 69.4%, core 60.2%; los archivos
    más bajos son `app_database.dart` 0% y `pass_device_screen.dart` 2%, ambos
    difíciles de cubrir sin dispositivo/SQLite real). LCOV en `coverage/lcov.info`.
  - VERSIÓN: `pubspec.yaml` actualizado a `0.30.0+30`.
  - README: reescrito con descripción, stack, ejecución, testing, estructura,
    cómo añadir un juego (skill `/add-game`) y pasos de build Android/iOS.
  - BUILDS: `flutter build apk --debug` OK (app-debug.apk) y
    `flutter build ios --no-codesign` OK (Runner.app 16.9MB). iOS sin firma por
    diseño (no es bloqueo de código).

---

## Correcciones de auditoría (Fase F8)

Versiones derivadas de la auditoría multi-agente (7 dimensiones + verificación
adversarial). Cada una resuelve un hallazgo o clúster de hallazgos. Tras cada
una: `flutter analyze` limpio y `flutter test` verde.

### 0.31 — Accesibilidad de la carta y de la revelación- **Objetivo:** que los lectores de pantalla puedan usar la app.
- **Alcance:** la carta de "Es un 10 pero" se dibuja con Canvas/Flame y no genera
  nodo semántico (hallazgo HIGH); envolver el `GameWidget` en `Semantics` con un
  label "Carta: {valor} de {palo}" que se actualiza al sacar carta. Además,
  anunciar la revelación del rol del Impostor (live region / merge semántico) en
  `RevealScreen`.
- **Hecho cuando:** TalkBack/VoiceOver leen la carta sacada y el rol revelado.
- **Notas:** `GameWidget` envuelto en `Semantics(liveRegion: true)` con label
  dinámico ("Carta: {valor} de {palo}" / "Sin carta, pulsa Sacar carta").
  `RevealScreen` envuelve la revelación en `Semantics(liveRegion)` ("Tu rol es
  IMPOSTOR. Pista: …" / "Tu palabra es …") sin tocar los textos que verifican
  los tests.

### 0.32 — Unicidad de palabras case-insensitive- **Objetivo:** evitar duplicados lógicos ("Pirata" vs "pirata").
- **Alcance:** la BD impone `UNIQUE` sensible a mayúsculas pero búsqueda/orden son
  `NOCASE`; alinear con `word TEXT NOT NULL UNIQUE COLLATE NOCASE` y añadir una
  migración de BD (version 2) que recree el índice/columna conservando datos.
- **Hecho cuando:** insertar la misma palabra con distinta capitalización falla y
  hay test que lo cubre.
- **Notas:** `kAppDatabaseVersion`=2; migración v1→v2 en transacción que recrea la
  tabla con NOCASE y **deduplica** case-insensitive (gana la seed/más antigua).
  Test de migración vía `onUpgradeForTest` + sqflite_ffi; insertar "PIRATA" tras
  migrar falla con `DuplicateWordException`.

### 0.33 — Romper la inversión de dependencia dominio→datos- **Objetivo:** que `domain/` no conozca `data/`.
- **Alcance:** eliminar `Word.fromImpostorWord` de `domain/word.dart` y mover la
  conversión a la frontera de datos (extensión en `data/` o en el coordinador).
- **Hecho cuando:** `domain/` no importa nada de `data/` y los tests siguen verdes.
- **Notas:** conversión movida a `extension ImpostorWordX on ImpostorWord`
  (`data/impostor_word.dart`); el coordinador usa `word.toDomain()`.
  `domain/word.dart` quedó **sin imports** (verificado).

### 0.34 — Eliminar `WordRepository.getRandom` (código muerto)- **Objetivo:** quitar duplicación de responsabilidad.
- **Alcance:** la elección de palabra vive en el dominio; `getRandom` solo lo usan
  los tests. Eliminarlo (y su test) dejando la selección en el use case.
- **Hecho cuando:** no queda selección aleatoria en la capa de datos.
- **Notas:** `getRandom` eliminado; `WordRepository` ya no depende de
  `RandomProvider` (constructor/campo/provider limpiados). Sin usos en `lib/`.

### 0.35 — Rendimiento del componente Flame- **Objetivo:** no malgastar CPU/GPU en reposo.
- **Alcance:** `card_flip_game.dart` recrea `Paint`/`Path`/`TextPainter` cada frame
  y `render()` no corta en reposo; añadir early-out cuando no hay flip y reutilizar
  instancias inmutables/cachear el dibujo estático.
- **Hecho cuando:** en reposo no hay reasignaciones por frame y la animación se ve
  igual o mejor.
- **Notas:** 6 `Paint` reutilizables (recoloreados solo al cambiar paleta); dorso y
  cara horneados en `ui.Picture` cacheadas (se regraban solo si cambia
  tamaño/paleta/carta); `render()` con early-out en reposo (`drawPicture` directo).
  API `flipTo`/`updatePalette` intacta; `==`/`hashCode` en `CardFlipPalette`.

### 0.36 — Layout defensivo en el flujo del Impostor- **Objetivo:** evitar overflows en horizontal / texto grande.
- **Alcance:** `PassDeviceScreen` y `RevealScreen` tienen `Column` central sin
  scroll; envolver en `SingleChildScrollView`/`Expanded` para absorber el excedente.
- **Hecho cuando:** no hay "RenderFlex overflowed" en pantallas pequeñas, horizontal
  ni con `textScaler` alto.
- **Notas:** ambas pantallas con `SingleChildScrollView` + centrado cuando hay
  espacio; sin romper widget tests.

### 0.37 — Cerrar huecos de cobertura de tests- **Objetivo:** cubrir la lógica hoy sin pruebas.
- **Alcance:** tests directos de `ImpostorFlowController` (iniciar ok/sinPalabras,
  avanzar hasta results, reiniciar, límites de índice), del `AssignRolesCoordinator`
  real (glue BD→dominio + `NoWordsAvailableException`), de `AppDatabase`
  (onCreate/onUpgrade/singleton vía sqflite_common_ffi) y de `PassDeviceScreen`.
- **Hecho cuando:** sube la cobertura de esos archivos y los tests pasan.
- **Notas:** 4 ficheros de test nuevos; suite total **130 → 150 tests** verdes.

### 0.38 — Extensibilidad: rutas vía GameRegistry- **Objetivo:** un único punto de extensión por juego.
- **Alcance:** `GameDescriptor` aporta sus rutas (`List<RouteBase> routes()`) y
  `appRouterProvider` las agrega iterando `gameRegistryProvider`, para que el router
  deje de mencionar juegos concretos; clarificar la entrada única (routeName vs
  buildEntryScreen) para no dejar caminos divergentes.
- **Hecho cuando:** registrar un juego en el registry basta también para sus rutas.
- **Notas:** `routes()` añadido al contrato; `ImpostorGame.routes()` → `impostorRoutes()`;
  `app_router.dart` monta rutas con `for (g in registry) ...g.routes()` y ya **no
  importa juegos concretos** (verificado). Entrada única documentada.

---

## Assets: imágenes y sonidos (Fase F9)

Infraestructura y uso de assets (imágenes y audio) para enriquecer la app. Los
assets concretos se generan como **placeholders** (mismo enfoque que el icono de
0.25) para que todo funcione de extremo a extremo; deben sustituirse por arte y
audio finales. Tras cada versión: `flutter analyze` limpio y `flutter test` verde.

### 0.39 — Infraestructura de assets (hecho)
- **Objetivo:** base común para imágenes y sonidos.
- **Alcance:** carpetas `assets/images/` y `assets/audio/` declaradas en
  `pubspec.yaml`; dependencia de audio (`flame_audio`); una clase `Assets` con las
  rutas centralizadas (sin strings sueltos); un `AudioService` + provider Riverpod
  con precarga y un toggle de sonido (activar/silenciar) consultable desde la UI.
- **Hecho cuando:** la app declara y resuelve assets, y el servicio de audio se
  inyecta vía Riverpod (mockeable en tests).
- **Notas:** `flame_audio` añadido; clase `Assets` (rutas de imágenes/audio);
  `AudioService` abstracto (impl `FlameAudioService` con try/catch + `NoopAudioService`
  para tests) con toggle `enabled`; `audioServiceProvider` y `audioEnabledProvider`;
  `MuteButton` reutilizable (`Icons.volume_up`/`volume_off`).

### 0.40 — Imágenes en la app (hecho)
- **Objetivo:** usar imágenes reales en la UI/juego.
- **Alcance:** generar placeholders PNG por código (tool) e integrarlos: dorso de
  la carta de "Es un 10 pero" como sprite/imagen en el componente Flame y/o una
  imagen de cabecera en el menú. Carga robusta con fallback si falta el asset.
- **Hecho cuando:** se ve al menos una imagen cargada desde `assets/images/` sin
  romper la animación de la carta.
- **Notas:** `tool/generate_images.dart` genera `card_back.png` y `menu_header.png`
  (placeholders). El dorso se carga como imagen en el componente Flame con fallback
  al dibujo programático; el menú muestra cabecera con `errorBuilder` de fallback.

### 0.41 — Sonidos en la app (hecho)
- **Objetivo:** efectos de sonido en eventos clave.
- **Alcance:** generar SFX placeholder por código (tonos WAV cortos) y
  reproducirlos en: voltear carta ("Es un 10 pero"), revelar rol y fin de partida
  (Impostor), respetando el toggle de silencio del `AudioService`. Sin bloquear la
  UI ni romper tests (audio desactivado/mock en el entorno de test).
- **Hecho cuando:** los eventos disparan sonido cuando está activado y silencio
  cuando no, con tests del `AudioService`/toggle.
- **Notas:** `tool/generate_sounds.dart` sintetiza WAV PCM 16-bit (placeholders):
  `card_flip.wav`, `reveal.wav`, `game_over.wav`. SFX vía `audioServiceProvider`
  (respeta el toggle) en voltear carta, revelar rol y fin de partida; `MuteButton`
  en el AppBar de "Es un 10 pero". Tests con `CountingAudioService` (suena solo si
  `enabled`). Suite total 150 → 163 verde.

---

## Estadísticas e internacionalización (Fase F10)

Dos funcionalidades que encajan en la arquitectura local actual (sin backend):
historial/estadísticas de partidas del Impostor en sqflite, e internacionalización
(español + inglés). Tras cada versión: `flutter analyze` limpio y `flutter test`
verde. Sin emojis: solo iconos de Flutter.

### 0.42 — Persistencia de historial de partidas (hecho)
- **Objetivo:** guardar el resultado de cada partida del Impostor.
- **Alcance:** tabla `game_history` en sqflite (migración v3 de `AppDatabase`):
  fecha, palabra, lista de jugadores y sus roles, nº de impostores, pista on/off.
  `GameHistoryRepository` (insertar, listar, borrar, agregados para estadísticas)
  + provider. Guardar la partida al llegar a `ResultsScreen`. Tests in-memory.
- **Hecho cuando:** terminar una partida persiste un registro y el repositorio lo
  lee, verificado por tests.
- **Notas:** `kAppDatabaseVersion`=3, tabla `game_history` (+índice) en onCreate y
  migración v2→v3; `GameRecord`/`GameRecordPlayer` (JSON) y `GameHistoryRepository`
  (insertFromSession/getAll/deleteAll/count/mostFrequentWord/impostorCountsByPlayer);
  `ResultsScreen` guarda la partida una vez. Tests in-memory.

### 0.43 — Pantalla de historial y estadísticas (hecho)
- **Objetivo:** ver partidas pasadas y métricas.
- **Alcance:** `HistoryScreen` accesible desde el flujo del Impostor: lista de
  partidas (fecha, palabra, nº jugadores/impostores) y un resumen de estadísticas
  (total de partidas, palabra más repetida, veces que cada jugador fue impostor…).
  Opción de borrar el historial. Estados de carga/vacío. Tests de widget.
- **Hecho cuando:** se ven las partidas guardadas y las estadísticas agregadas.
- **Notas:** `HistoryScreen` (ExpansionTile por partida + Card de stats: total,
  palabra más repetida, ranking de impostores) con borrar historial y estados
  carga/vacío; accesible desde el flujo del Impostor. Tests de widget con repo fake.

### 0.44 — Infraestructura de internacionalización (i18n) (hecho)
- **Objetivo:** base para múltiples idiomas.
- **Alcance:** `flutter_localizations` + `intl`, `l10n.yaml`, ficheros ARB
  (`app_es.arb`, `app_en.arb`) y `AppLocalizations` generado; `MaterialApp.router`
  con `localizationsDelegates`/`supportedLocales` (es por defecto, en secundario);
  provider de `Locale` con selector de idioma persistido y un control en la UI.
- **Hecho cuando:** la app arranca localizada y se puede cambiar de idioma en
  caliente.
- **Notas:** `flutter_localizations`+`intl`+`shared_preferences`, `generate: true`,
  `l10n.yaml`, ARB es/en, `AppLocalizations` generado; `localeProvider`
  (`LocaleController`, null = idioma del sistema, persistido); `MaterialApp.router`
  con delegates/supportedLocales (es por defecto) y `LanguageSelectorButton`
  (`Icons.language`) en el menú.

### 0.45 — Migración de textos a i18n (es + en) (hecho)
- **Objetivo:** todo el copy traducible.
- **Alcance:** mover los textos hardcodeados de todas las pantallas (menú, "Es un
  10 pero", flujo Impostor, gestión de palabras, historial) a las ARB con
  traducción española e inglesa. Ajustar los tests de widget para fijar el locale
  español y conservar las aserciones. Sin strings de UI sueltos.
- **Hecho cuando:** cambiando el idioma, la UI se muestra en es o en, y los tests
  pasan bajo locale español.
- **Notas:** todo el copy de pantallas/diálogos/errores externalizado a ARB es/en
  (placeholders y plurales ICU); valores españoles idénticos a los previos. Helper
  `test/support/localized_app.dart` fija locale `es` + delegates en los widget tests.
  Excepción intencional: `GameDescriptor.title/description` (metadatos de catálogo)
  no se localizan (no tienen contexto y son nombres de marca).

---

## Rediseño visual neón (Fase F11)

Rediseño visual completo con estética **cyberpunk cian + magenta** sobre fondo casi
negro, "full neón" (resplandores, bordes brillantes, texto con glow, pulsos). El
neón solo luce sobre oscuro, así que el tema oscuro neón pasa a ser la experiencia
principal. Sin emojis: solo iconos de Flutter. Sin romper los tests (se conservan
textos y tipos de widget que las pruebas verifican; el glow se añade por decoración).

### 0.46 — Sistema de diseño neón (hecho)
- **Objetivo:** base visual reutilizable.
- **Alcance:** reescribir `core/theme/app_theme.dart` a un tema oscuro cyberpunk
  (fondo `#0A0A14`, cian `#00F0FF`, magenta `#FF00E5`, violeta de apoyo `#B026FF`);
  widgets neón reutilizables en `core/widgets/` (texto con glow, panel/tarjeta de
  borde brillante, botones neón, helper de sombra-resplandor, fondo con rejilla
  sutil, pulso animado). Forzar el tema neón oscuro en `main.dart`.
- **Hecho cuando:** existen el tema y los widgets neón, documentada su API.
- **Notas:** `AppTheme` (colores `background #0A0A14`, `neonCyan #00F0FF`,
  `neonMagenta #FF00E5`, `neonViolet #B026FF`, superficies y texto claro);
  `lib/core/widgets/neon.dart` con `neonGlow`/`neonTextShadows`, `NeonText`,
  `NeonPanel`, `NeonGlowWrapper` (envuelve botones sin cambiar su tipo),
  `NeonBackground` (fondo + rejilla) y `PulseGlow`. `main.dart` fuerza
  `ThemeMode.dark`. Aviso: `PulseGlow` (animación infinita) no debe quedar visible
  bajo `pumpAndSettle` en tests.

### 0.47 — Aplicar el neón a todas las pantallas (hecho)
- **Objetivo:** que toda la app se vea neón.
- **Alcance:** aplicar el sistema de 0.46 a menú, "Es un 10 pero" (carta Flame con
  bordes/resplandor neón y palos cian/magenta), flujo Impostor (setup, pass,
  reveal con texto gigante glow, results), gestión de palabras e historial.
  Preservar textos y tipos de widget usados por los tests.
- **Hecho cuando:** todas las pantallas usan el lenguaje neón y los tests pasan.
- **Notas:** menú (tarjetas neón alternando cian/magenta + título glow pulsante),
  "Es un 10 pero" (carta Flame con marco/halo neón estilo tubo y palos cian/magenta),
  flujo Impostor (reveal con "IMPOSTOR" gigante con glow magenta), palabras e
  historial. Se conservaron tipos de widget y textos (i18n); el glow se aplica por
  decoración. 182/182 tests verdes.

---

## Correcciones de la 2ª auditoría (Fase F12)

### 0.48 — Correcciones de auditoría (post F8-F11) (hecho)
- **Objetivo:** resolver los hallazgos de la segunda auditoría (0 critical/high; 8
  medium + 12 low + info accionables).
- **Alcance (por área):**
  - **Rendimiento:** `PulseGlow` no reconstruye subárboles caros por frame y
    respeta "reduce motion"; `card_flip_game` pausa el motor Flame en reposo
    (`pauseEngine`/`resumeEngine`); búsqueda de palabras con debounce; `neonGlow`
    cacheado; `RepaintBoundary` en la rejilla de fondo; historial carga la tabla
    una sola vez.
  - **Layout:** `Expanded`/`maxLines`/`FittedBox` en nombre de jugador, palabra de
    la ronda y resumen de estadísticas (sin overflow).
  - **i18n:** títulos/descripciones de los juegos y mensajes de error del flujo
    localizados (es/en); fecha del historial con `DateFormat` del locale; validar
    el locale persistido contra los soportados.
  - **Arquitectura:** el esquema/seed del Impostor deja de vivir en `core/db`
    (contrato de aportación de esquema por juego vía `GameRegistry`); icono de palo
    movido fuera del dominio `Card`; contador del seed loader exacto.
  - **Audio:** persistir el toggle de silencio.
  - **Testing:** tests de `PulseGlow`, selector de idioma, migración NOCASE
    (`Pirata`/`pirata`), `textScaler` en reveal/pass/results; `coverage/` regenerado
    y gitignorado.
- **Hecho cuando:** `flutter analyze` limpio, todos los tests verdes y los
  hallazgos cerrados.
- **Notas:** `flutter analyze` 0 issues, 182 → **198 tests** verdes. Refactor de
  esquema: `GameDescriptor.onCreateTables/onUpgradeTables` (tipos `sqflite_common`
  puros), esquema del Impostor en `lib/games/impostor/data/impostor_schema.dart`,
  `core/db` sin tablas concretas. Excepción documentada: `inversePrimary`
  (neonViolet) sobre `inverseSurface` queda en ~4.18:1 (rol de borde/acento de
  snackbar, no texto); aclararlo empeoraría el contraste sobre fondo claro.

---

## Votación del Impostor y cuenta atrás (Fase F13)

### 0.49 — Mecánica de votación por rondas + cuenta atrás de carta (hecho)
- **Objetivo:** cambiar el final del Impostor (ya no se muestran los roles) por una
  votación por rondas, y añadir una cuenta atrás al sacar carta.
- **Alcance:**
  - **Rondas (setup):** selector de número de rondas; mínimo 1, máximo
    `max(1, jugadores − 3)` (ej. 6 jugadores → máx 3). Campo `rounds` en `GameConfig`.
  - **Votación:** tras revelar todos los roles se va a una `VotingScreen` (una sola
    pantalla; el grupo elige a quién expulsar cada ronda). Regla: **hay que pillar a
    todos los impostores**; si el votado es impostor, queda eliminado; se gana al no
    quedar impostores ("¡Habéis ganado!"); si no es impostor, "el impostor sigue
    entre vosotros". Cada voto consume una ronda; si se agotan las rondas con
    impostores vivos, **gana el impostor sin revelar** identidades.
  - **Sin pantalla de roles** al terminar los reveals (se elimina ese resultado).
    El historial se sigue guardando al acabar la partida (con el desenlace).
  - **Reveal:** reducir el tamaño del texto "IMPOSTOR".
  - **Es un 10 pero:** al pulsar sacar carta, **cuenta atrás de 5 segundos** (5→1)
    antes de revelar la carta; botón deshabilitado durante la cuenta.
- **Hecho cuando:** una partida llega a votación, se resuelve por rondas con los
  mensajes correctos, y la carta se revela tras 5s. `flutter analyze` limpio y
  tests verdes.
- **Notas:** `GameConfig.rounds` + `maxRoundsFor(p)=max(1,p-3)`; flow controller con
  fases `voting`/`gameOver`, `votar(Player)` (must-catch-all), `eliminados`,
  `rondaActual/rondasTotales`, `outcome`. `VotingScreen` (selección única por ronda)
  + `GameOverScreen` (mensaje sin roles, guarda historial). Reveal navega a votación
  y "IMPOSTOR" reducido a tamaño menor. "Es un 10 pero" con cuenta atrás de 5s
  (Timer, botón deshabilitado) antes del flip. 182 → **211 tests** verdes; e2e
  ejecutado en simulador. Nota UX: el selector de rondas arranca en el mínimo (1);
  no auto-sube al máximo al añadir jugadores (ajustable en setup).

---

## Nuevos juegos: Trivia y Wavelength (Fases F14–F15)

Dos juegos nuevos que **caben en la arquitectura local actual** (sin backend, un
solo dispositivo, SQLite, extensibilidad vía `GameRegistry`). Se construyen como
*bounded contexts* aislados en `lib/games/<juego>/{domain,data,presentation}` y se
registran en `gameRegistryProvider` SIN tocar el `MenuScreen` (regla de oro).
Aleatoriedad SIEMPRE vía `RandomProvider`. Copy de UI en español vía
`AppLocalizations` (es/en). Sin emojis: solo iconos de Flutter. Tras cada versión:
`flutter analyze` limpio y `flutter test` verde.

> Decisiones de planificación (acordadas con el usuario):
> - **Origen de preguntas:** importar de **Open Trivia DB** (OpenTDB, opentdb.com,
>   licencia CC BY-SA 4.0) y adaptar/traducir al español a un seed JSON. Sus
>   dificultades `easy/medium/hard` mapean 1:1 a **fácil/difícil/muy difícil** y ya
>   son multiple-choice de 4 opciones (1 correcta + 3 incorrectas). OpenTDB **no
>   tiene categoría de cocina**: esa temática se cura aparte.
> - **Flujo de la trivia:** por rondas, todos los jugadores a la vez (pasando el
>   móvil); quien falla queda eliminado; los supervivientes de las 9 preguntas
>   **empatan**.
> - **Contador de victorias:** tabla SQLite `nombre → victorias`; introducir un
>   nombre distinto = registro nuevo desde cero.
> - **Orden de construcción:** primero la trivia (F14), después Wavelength (F15).
> - *Pendiente de decidir más adelante:* temporizador por pregunta (v1 arranca SIN
>   cuenta atrás; se puede añadir como mejora posterior).

### F14 — "Preguntas por puntos" (trivia por eliminación)

Juego de trivia para **hasta 6 jugadores** (mín. 2). Cada jugador responde 9
preguntas en dificultad creciente (**3 fáciles → 3 difíciles → 3 muy difíciles**),
elegidas por temática (cultura general, videojuegos, cocina…). Se avanza **por
rondas**: en cada ronda los jugadores vivos responden, y **quien falla se elimina**.
Los que sobreviven a las 9 preguntas **empatan y ganan**, sumando una victoria a su
contador personal (por nombre). Estética obligatoria: caja de pregunta en **morado
neón**; las 4 respuestas en cuadrados con **marcos azul, verde, rojo y amarillo**.

#### 0.50 — Dominio de la trivia (puro y testeable)
- **Objetivo:** modelar pregunta, dificultad, temática y partida sin UI ni BD.
- **Alcance:** `Question` (id, temática, dificultad, enunciado, 4 opciones, índice
  de la correcta), `Difficulty {facil, dificil, muyDificil}`, `Theme`,
  `TriviaConfig` (jugadores 2–6 + temáticas elegidas) y `TriviaSession`/reglas de
  eliminación por rondas. Selección de preguntas vía `RandomProvider` (mockeable);
  cada jugador recibe una pregunta DISTINTA por ronda.
- **Hecho cuando:** los tipos representan una partida completa y los tests cubren
  eliminación, empate de supervivientes y el avance fácil→difícil→muy difícil.
- **Notas (hecho):** `lib/games/trivia/domain/` con `Difficulty {facil,dificil,muyDificil}`
  (+`fromOpenTdb`), `Tematica` (nombre español para no chocar con `ThemeData`),
  `Question` (4 opciones + `correctIndex` validados, `isCorrect`), `TriviaConfig`
  (2–6 jugadores + temáticas), `TriviaPlayer`, `TriviaSession` (rondas 0–8, tramo por
  ronda vía función pura `difficultyForRound`, vivos/eliminados, `winners`=supervivientes;
  vacío si todos caen) y `DealQuestionsUseCase` (Fisher-Yates vía `RandomProvider`,
  pregunta distinta por jugador). `flutter analyze` 0 issues.

#### 0.51 — Persistencia y esquema (SQLite)
- **Objetivo:** banco de preguntas y contador de victorias persistentes.
- **Alcance:** `lib/games/trivia/data/trivia_schema.dart` con tablas
  `trivia_questions` (enunciado, opciones JSON, índice correcto, temática,
  dificultad, `is_seed`) y `trivia_winners` (`name` UNIQUE COLLATE NOCASE, `wins`),
  aportadas vía `GameDescriptor.onCreateTables`/`onUpgradeTables` (migración
  `kAppDatabaseVersion` → 4, sin tablas concretas en `core/db`). `QuestionRepository`
  (consulta por temática/dificultad, random) y `WinnerRepository` (incrementar por
  nombre, listar ranking). Tests in-memory (sqflite_common_ffi).
- **Hecho cuando:** las tablas se crean/migran y los repositorios pasan tests.
- **Notas (hecho):** `lib/games/trivia/data/` con `TriviaSchema` (DDL `trivia_questions`
  +índice y `trivia_winners` con `name UNIQUE COLLATE NOCASE`, misma firma que
  `impostor_schema` para wiring trivial en 0.57), `QuestionRepository` (insert/bulk/count/
  pools por temática+dificultad, opera sobre `DatabaseExecutor`, sin random) y
  `WinnerRepository` (upsert `incrementWins`, `getWins`, `getAllRanked`). Aún NO se
  toca `kAppDatabaseVersion` ni el registry (wiring en 0.57); tests aplican el DDL
  directo sobre sqflite ffi in-memory. Suite 211 → **294 verdes**. `pubspec` 0.51.0+51.

#### 0.52 — Importación OpenTDB y seed inicial
- **Objetivo:** poblar el banco con preguntas reales de calidad.
- **Alcance:** `tool/import_trivia.dart` que toma preguntas de OpenTDB
  (type=multiple), mapea `easy/medium/hard` → `facil/dificil/muyDificil`, decodifica
  entidades HTML, mezcla las 4 opciones y registra el índice correcto; adaptación/
  traducción al español a `assets/seed/trivia_questions.json`. Seed inicial de
  **~150–200** preguntas balanceadas para arrancar el juego jugable. Atribución
  **CC BY-SA 4.0** en la app (sobre OpenTDB). La temática **cocina** se cura a mano
  (no existe en OpenTDB).
- **Hecho cuando:** primer arranque carga el seed y hay preguntas en las 3
  dificultades por cada temática inicial.
- **Notas (hecho):** `assets/seed/trivia_questions.json` con **172 preguntas** en 9
  temáticas (cultura_general, videojuegos, cocina con 24 c/u —las prioritarias—, más
  ciencia, cine, historia, música, deportes, geografía), todas con 4 opciones e índice
  correcto y repartidas en las 3 dificultades. `tool/import_trivia.dart` (OpenTDB vía
  `dart:io` HttpClient + `encode=base64`, mapea easy/medium/hard, nota CC BY-SA 4.0).
  `trivia_questions_seed_loader.dart` idempotente (carga si la tabla está vacía).
  Suite 294 → **307 verdes**. `pubspec` 0.52.0+52.

#### 0.53 — SetupScreen de la trivia
- **Objetivo:** configurar la partida.
- **Alcance:** añadir **hasta 6 jugadores** (mín. 2) con la misma UX del Impostor
  (lista ordenada, validación, neón) + selección de **temáticas** a incluir.
  Diálogo claro si el banco no tiene suficientes preguntas para la config elegida.
- **Hecho cuando:** no se puede iniciar fuera de límites y la config queda lista.
- **Notas (hecho):** `TriviaSetupScreen` (2–6 jugadores con la UX del Impostor +
  chips multi-selección de temáticas + ranking de victorias por nombre + diálogo
  `sinPreguntas` + atribución OpenTDB CC BY-SA). Construida en la rebanada de
  integración (v0.57).

#### 0.54 — Pantalla de pregunta (UI neón obligatoria)
- **Objetivo:** la superficie de juego con la estética pedida.
- **Alcance:** caja del enunciado con **morado neón** (`AppTheme.neonViolet`); 4
  respuestas en cuadrados (grid 2×2) con **marcos de color azul / verde / rojo /
  amarillo** (añadir esos 4 colores neón al theme). Feedback visual de acierto/
  fallo al pulsar. Layout responsive + accesible (sin overflow, objetivos táctiles).
- **Hecho cuando:** se ve la pregunta en morado y las 4 opciones con sus 4 marcos
  de color, y pulsar marca acierto/fallo.
- **Notas (hecho):** `TriviaQuestionScreen` con `NeonPanel` de borde `neonViolet`
  (morado) para el enunciado y grid 2×2 de respuestas con marco
  `answerFrameColors[i]` (azul/verde/rojo/amarillo); al pulsar, feedback 800ms
  (verde acierto / rojo fallo) y avanza. Sin `PulseGlow` (seguro para tests).

#### 0.55 — Flow controller: mecánica por rondas
- **Objetivo:** orquestar la partida completa.
- **Alcance:** `TriviaFlowController` (Riverpod `Notifier`) con fases
  setup→pass→pregunta→(siguiente jugador/ronda)→fin; pasar el móvil entre jugadores
  vivos (`TriviaPassDeviceScreen`), 9 preguntas en 3 tramos de dificultad,
  eliminación al fallar, registro de supervivientes. `RandomProvider` inyectado.
- **Hecho cuando:** una partida fluye por rondas, elimina a quien falla y termina
  con el conjunto de supervivientes correcto.
- **Notas (hecho):** `TriviaFlowController` (Riverpod `Notifier`) con fases
  `setup/iniciando/pass/question/gameOver/error`, `iniciar(config)` (carga pools por
  temática, valida pool suficiente o `TriviaErrorKind.sinPreguntas`, reparte pregunta
  distinta por jugador vía `DealQuestionsUseCase`+`RandomProvider`), `responder(index)`
  (elimina al fallar, avanza jugador/ronda, recalcula tramo de dificultad),
  `pasarDispositivo()`, `reiniciar()`; al `gameOver` suma `+1` a cada superviviente en
  `WinnerRepository` por nombre. Providers `FutureProvider` para los repos. Sin strings
  de UI (solo enums). Groundwork de 0.54: colores neón `neonBlue/Green/Red/Yellow` +
  `answerFrameColors` en `app_theme.dart` (la pantalla se construye en la rebanada de
  integración). Suite 307 → **324 verdes**. `pubspec` 0.55.0+55.

#### 0.56 — Fin de partida y contador de victorias
- **Objetivo:** cerrar la partida y persistir las victorias por nombre.
- **Alcance:** pantalla de desenlace que muestra el **empate de supervivientes**
  (sin “ganador único” si hay varios) e incrementa `+1` en `trivia_winners` para
  cada superviviente (por nombre, COLLATE NOCASE). Al volver al menú/setup se
  muestra el **contador/ranking de victorias**; introducir un nombre distinto
  arranca desde cero. Caso límite documentado: si TODOS fallan una ronda, no hay
  ganadores esa partida.
- **Hecho cuando:** terminar persiste las victorias y el contador se ve al volver.
- **Notas (hecho):** `TriviaGameOverScreen` muestra el empate de supervivientes
  (o "nadie ganó") SIN roles; las victorias ya las persiste el controller en
  `gameOver`; "Volver al menú" llama a `reiniciar()`. El ranking se ve en el setup.

#### 0.57 — Registro en GameRegistry, rutas, i18n y tests
- **Objetivo:** integrar el juego en la app sin acoplar el menú.
- **Alcance:** `TriviaGame` (`GameDescriptor` con `routes()` y aporte de esquema),
  registro en `gameRegistryProvider`; `trivia_routes.dart`; todo el copy en ARB
  es/en (sin strings sueltos); tests de widget (pregunta, setup) y de flujo. El
  router monta las rutas iterando el registry (sin mencionar el juego concreto).
- **Hecho cuando:** el juego aparece en el menú, se juega de principio a fin,
  `flutter analyze` limpio y tests verdes.
- **Notas (hecho):** `TriviaGame` (`GameDescriptor`: título "Preguntas por puntos",
  `Icons.quiz`, `routes()`, aporta esquema vía `onCreateTables`/`onUpgradeTables`)
  registrado en `gameRegistryProvider` (menú intacto). `trivia_routes.dart`
  (`/trivia` + pass/question/game-over). `kAppDatabaseVersion` 3→**4** con migración
  que crea las tablas de trivia en instalaciones existentes; seed en primer arranque
  igual que el Impostor. **27 claves** ARB es/en nuevas. Tests de migración v4 +
  widget (setup/pregunta/fin). Suite 324 → **344 verdes**. `pubspec` 0.57.0+57.

#### 0.58 — Ampliación de contenido a 1000+ preguntas
- **Objetivo:** alcanzar el banco mínimo de **1000+ preguntas**.
- **Alcance:** lotes sucesivos de importación/curación (puede ocupar varias
  sub-versiones de puro contenido) hasta superar 1000 preguntas, balanceadas por
  temática y por las 3 dificultades; revisión de calidad de la traducción al
  español y curación de la temática de cocina.
- **Hecho cuando:** el seed supera 1000 preguntas válidas y repartidas, sin romper
  el rendimiento de carga.
- **Notas (hecho):** banco final **1151 preguntas** (generadas en 5 tandas paralelas,
  fusionadas, validadas y deduplicadas por enunciado normalizado: 50 duplicados
  fuera, 0 inválidas). Reparto: facil 471 / dificil 462 / muyDificil 218; por temática
  102–176 cada una (deportes 176, videojuegos 144, cocina 135…). El test de seed exige
  ahora **≥1000**. `flutter analyze` 0 issues, **344 tests verdes**. `pubspec` 0.58.0+58.
  AVISO: el contenido en volumen es generado por máquina; conviene una revisión factual
  posterior (las palabras/preguntas del usuario son editables desde la app a futuro).

### F15 — "Wavelength" (sintonía / lectura de mente)

Juego cooperativo: hay un **espectro entre dos conceptos opuestos** (p. ej.
*frío ↔ caliente*) con una **zona objetivo oculta**; un jugador ve el objetivo y da
una **pista**, y el resto **mueve un dial** para adivinar dónde cae. Se puntúa según
la cercanía. Es el juego donde **Flame luce de verdad** (dial animado con estética
neón). Se construye DESPUÉS de la trivia.

#### 0.59 — Dominio de Wavelength (puro y testeable)
- **Objetivo:** modelar espectro, objetivo y puntuación.
- **Alcance:** `Spectrum` (par de conceptos opuestos), objetivo oculto en un rango,
  cálculo de **puntos por cercanía** de la respuesta al objetivo; objetivo y
  espectro elegidos vía `RandomProvider`. Sin UI ni BD.
- **Hecho cuando:** los tests verifican la puntuación por cercanía y la elección
  determinista con seed fija.
- **Notas (hecho):** `lib/games/wavelength/domain/` con `Spectrum` (izquierda/derecha),
  `WavelengthConfig` (2–8 jugadores + rondas), `WavelengthRound` + puntuación por bandas
  concéntricas sobre eje 0..1 (bullseye `±0.06`=4, near `±0.12`=3, far `±0.20`=2, fuera=0;
  constantes públicas para que el dial las pinte), `PickRoundUseCase` (espectro + objetivo
  random vía `RandomProvider`) y `WavelengthSession` (rondas, score acumulado, rota el
  clue-giver). Puro y determinista con seed.

#### 0.60 — Persistencia/seed de espectros
- **Objetivo:** banco de pares de conceptos en español.
- **Alcance:** seed de espectros (`assets/seed/wavelength_spectra.json` y/o tabla
  `wavelength_spectra` aportada por `GameDescriptor`, migración → 5). Carga inicial.
- **Hecho cuando:** el primer arranque dispone de espectros para jugar.
- **Notas (hecho):** `wavelength_schema.dart` (tabla `wavelength_spectra`, misma firma
  que trivia para wiring en 0.63), `SpectrumRepository` (sobre `DatabaseExecutor`),
  `assets/seed/wavelength_spectra.json` con **80 pares** de conceptos opuestos en español,
  y seed loader idempotente. Sin bump de `kAppDatabaseVersion` aún (wiring en 0.63);
  tests con DDL directo sobre ffi. Suite 344 → **428 verdes**. `pubspec` 0.60.0+60.

#### 0.61 — Dial Flame neón (superficie de juego)
- **Objetivo:** el render del dial con animación.
- **Alcance:** componente Flame del dial/aguja (semicírculo) con estética neón
  (resplandor, marcas), arrastrable para fijar la respuesta y animación de
  revelado de la zona objetivo. Reutiliza el patrón de pausa del motor en reposo.
- **Hecho cuando:** el dial se ve neón, se arrastra y revela el objetivo con animación.
- **Notas (hecho):** `wavelength_dial_game.dart` (FlameGame + `_DialComponent`,
  semicírculo neón con etiquetas de los conceptos, aguja arrastrable
  `needlePosition` 0..1 + `onGuessChanged`, modo clue/guess/reveal con bandas
  concéntricas y animación de revelado 0.7s; `pauseEngine` en reposo igual que la
  carta). Gotcha Flame 1.37: `DragUpdateEvent.localEndPosition` (no `localPosition`).

#### 0.62 — Flujo de partida de Wavelength
- **Objetivo:** recorrer una ronda completa.
- **Alcance:** setup (jugadores/equipos), un jugador ve el objetivo y escribe la
  **pista**, se pasa el móvil, el resto mueve el dial, se revela y se **puntúa**;
  rondas sucesivas. Estados en un flow controller Riverpod.
- **Hecho cuando:** una ronda fluye de pista→adivinanza→puntuación sin saltos.
- **Notas (hecho):** `WavelengthFlowController` (Notifier) fases
  `setup/clue/pass/guess/reveal/gameOver` + error `sinEspectros`; `iniciar` carga pool
  y elige ronda (`PickRoundUseCase`+`RandomProvider`), el psíquico ve el objetivo y
  escribe la pista, el grupo arrastra el dial, `submitGuess` puntúa por bandas y suma
  al score acumulado, rota el clue-giver cada ronda. 6 pantallas neón (setup, clue,
  pass, guess, reveal, game-over). Sin strings de UI en el controller.

#### 0.63 — Registro en GameRegistry, i18n y tests
- **Objetivo:** integrar Wavelength en la app.
- **Alcance:** `WavelengthGame` (`GameDescriptor` + `routes()`), registro en el
  registry; copy ARB es/en; tests de dominio y de flujo. Router desacoplado.
- **Hecho cuando:** el juego aparece en el menú y se juega completo; `flutter
  analyze` limpio y tests verdes.
- **Notas (hecho):** `WavelengthGame` (`GameDescriptor`: título "Wavelength",
  `Icons.tune`, `routes()`, esquema vía `onCreateTables`/`onUpgradeTables`) registrado
  en `gameRegistryProvider` (menú intacto). `wavelength_routes.dart` (`/wavelength` +
  clue/pass/guess/reveal/game-over). `kAppDatabaseVersion` 4→**5** con migración que
  crea `wavelength_spectra` + seed en instalaciones existentes. **28 claves** ARB es/en.
  Tests de migración v5 + flow + widget. Suite 428 → **469 verdes**. `pubspec` 0.63.0+63.

---

## Más juegos + reglas + auditoría (Fases F16–F20)

Tres juegos nuevos de fiesta (Tabú, Yo Nunca, La Bomba), una pantalla de reglas
"¿Cómo se juega?" por juego, y una auditoría multi-skill final. Mismas reglas de
arquitectura: cada juego es un *bounded context* en `lib/games/<juego>/{domain,data,
presentation}` registrado en `gameRegistryProvider` SIN tocar el `MenuScreen`;
aleatoriedad vía `RandomProvider`; copy es/en vía `AppLocalizations`; sin emojis
(solo iconos de Flutter). Tras cada versión: `flutter analyze` limpio y `flutter
test` verde.

> Decisiones de planificación (acordadas con el usuario):
> - **Tabú:** por **equipos** (2) + **temporizador** por turno; **contador de
>   victorias por equipo**: el primer equipo en llegar a **3 victorias** gana la
>   partida.
> - **Yo Nunca:** con **niveles** de intensidad **suave / picante** (picante con
>   aviso), elegibles en el setup.
> - **La Bomba:** **dos modos** elegibles (por **sílaba** / por **categoría**);
>   temporizador **oculto** que explota al azar + **eliminación** hasta que queda uno.
> - **Reglas:** botón **"¿Cómo se juega?"** en el **setup de cada juego**, que abre
>   una pantalla de reglas por juego (para los 7 juegos del catálogo).
> - Contenido (bancos de palabras/frases/sílabas) generado en español; revisable y
>   editable a futuro.

### F16 — Tabú (v0.64–0.65)

Un jugador de un equipo **describe una palabra sin decir las prohibidas**; su equipo
adivina contrarreloj mientras el otro equipo vigila. Marcador por equipos; gana la
partida el primer equipo en alcanzar **3 victorias de ronda**.

#### 0.64 — Dominio + datos + seed (Tabú)
- **Objetivo:** modelar palabra-tabú, configuración por equipos y puntuación; banco
  de palabras con prohibidas.
- **Alcance:** `lib/games/tabu/domain/` (`TabuWord` = palabra + lista de prohibidas;
  `TabuConfig` = 2 equipos con nombres, duración del turno en segundos, objetivo de
  victorias = 3; reglas de turno y de fin de partida puras y testeables; selección de
  palabra vía `RandomProvider`). `lib/games/tabu/data/` (`tabu_schema.dart` tabla
  `tabu_words` con `palabra` + `prohibidas_json`, `is_seed`; `TabuWordRepository`;
  seed loader). `assets/seed/tabu_words.json` con **~120 palabras**, cada una con
  **4–5 prohibidas**. Tests de dominio + datos.
- **Hecho cuando:** los tipos modelan una partida por equipos y los repos pasan tests.
- **Notas (hecho):** `TabuWord` (palabra+prohibidas), `TabuConfig` (2 equipos,
  `turnoSegundos`, `objetivoVictorias`=3), `TabuSession` (regla: cada turno con ≥1
  acierto da 1 victoria al equipo; primero a 3 gana) y `PickTabuWordUseCase` (sin
  repetir). `tabu_words` (`prohibidas_json`) + repo + seed loader. Seed **120 palabras**
  (4–5 prohibidas c/u). Run limpio: suite → **674 verdes**.

#### 0.65 — Presentación + integración (Tabú)
- **Objetivo:** Tabú jugable y en el menú.
- **Alcance:** `TabuFlowController` (Notifier; fases setup/turno/fin de ronda/fin de
  partida; temporizador del turno; acierto/saltar/falta; marcador por equipos; primer
  equipo a 3 gana). Pantallas neón: setup (2 equipos + nombres + duración del turno),
  pantalla de turno (palabra grande + lista de prohibidas + cuenta atrás + botones
  acierto/saltar/falta), marcador y desenlace. `TabuGame` (`GameDescriptor` + rutas +
  esquema), registro en el registry, migración `kAppDatabaseVersion` → **6**, i18n
  es/en, tests de flujo y widget.
- **Hecho cuando:** una partida por equipos fluye con temporizador y marcador, gana
  el primero a 3; `analyze` limpio y tests verdes.
- **Notas (hecho):** `TabuGame` (`GameDescriptor`: título "Tabú", `Icons.do_not_disturb_on_outlined`,
  `routes()`, aporta esquema vía `onCreateTables`/`onUpgradeTables`) registrado en
  `gameRegistryProvider` (menú intacto). `tabu_routes.dart` (`/tabu` + turn/scoreboard/
  game-over). `kAppDatabaseVersion` 5→**6** con migración que crea `tabu_words` + seed
  en instalaciones existentes (sin tocar impostor/trivia/wavelength). Timer del turno
  vive en `TabuTurnScreen` (igual que "Es un 10 pero") — el controlador es puro y
  testeable. **29 claves** ARB es/en (prefijo `tabu*`). Tests: migración v6 (5 casos),
  widget setup (5), turn screen (3), game-over (2). Suite 674 → **689 verdes**.
  `pubspec` 0.65.0+65.

### F17 — Yo Nunca (v0.66–0.67)

El móvil saca una frase **"Yo nunca…"** al azar y se pasa. Con niveles de intensidad
**suave** (todo público) y **picante** (adulto, con aviso).

#### 0.66 — Dominio + datos + seed (Yo Nunca)
- **Objetivo:** modelar frase con nivel y banco por intensidad.
- **Alcance:** `lib/games/yo_nunca/domain/` (`NeverStatement` = frase + `Intensidad
  {suave, picante}`; config con el/los niveles elegidos; selección al azar sin repetir
  vía `RandomProvider`). `data/` (`yo_nunca_schema.dart` tabla `yo_nunca_statements`
  con `frase` + `intensidad` + `is_seed`; repo; seed loader). `assets/seed/
  yo_nunca_statements.json` con **~150 frases** (mezcla suave/picante). Tests.
- **Hecho cuando:** el banco carga por nivel y la selección no repite hasta agotar.
- **Notas (hecho):** `Intensidad {suave,picante}`, `NeverStatement`, `YoNuncaConfig`
  (Set de intensidades, ≥1) y `DrawStatementUseCase` (filtra por nivel, sin repetir
  hasta agotar). `yo_nunca_statements` (+índice por intensidad) + repo
  `getByIntensidades` + seed loader. Seed **154 frases** (98 suave / 56 picante).
  Fix: `hashCode` de Set con XOR-fold (orden-independiente).

#### 0.67 — Presentación + integración (Yo Nunca)
- **Objetivo:** Yo Nunca jugable y en el menú.
- **Alcance:** `YoNuncaFlowController` (saca frase, pasa, evita repetición). Pantallas
  neón: setup (selector de nivel con **aviso** claro al activar picante), pantalla de
  revelado (frase al azar + "siguiente"/"pasar"). `YoNuncaGame` (`GameDescriptor` +
  rutas + esquema), registro, migración → **7**, i18n es/en, tests.
- **Hecho cuando:** se revelan frases del nivel elegido con el aviso en picante;
  `analyze` limpio y tests verdes.
- **Notas (hecho):** `YoNuncaGame` (`GameDescriptor`: título "Yo Nunca",
  `Icons.front_hand`, `routes()`, esquema vía hooks) registrado en el registry.
  `yo_nunca_routes.dart` (`/yo-nunca` + play). `YoNuncaFlowController` (setup/jugando/
  error; `iniciar`, `siguiente`). `YoNuncaSetupScreen` (multi-selección suave/picante
  con **aviso** al activar picante) + `YoNuncaPlayScreen` (frase grande + "Siguiente").
  `kAppDatabaseVersion` 6→**7** con migración de `yo_nunca_statements`. **14 claves**
  ARB es/en. Tests migración v7 + widget. Suite 689 → **705 verdes**. `pubspec` 0.67.0+67.

### F18 — La Bomba (v0.68–0.69)

Se muestra una **sílaba** o **categoría**; cada jugador dice una palabra válida y
pasa el móvil; un **temporizador oculto** explota al azar y quien lo tiene queda
**eliminado**. Se juega hasta que queda uno.

#### 0.68 — Dominio + datos + seed (La Bomba)
- **Objetivo:** modelar los dos modos, el temporizador oculto y la eliminación.
- **Alcance:** `lib/games/bomba/domain/` (`BombaMode {silaba, categoria}`;
  `BombaConfig` = modo + jugadores + rango de tiempo de explosión; el **instante de
  explosión** se decide vía `RandomProvider` dentro del rango —testeable sin reloj
  real—; lógica de eliminación y "último en pie"). `data/` (`bomba_schema.dart` tablas
  `bomba_silabas` y `bomba_categorias` + `is_seed`; repos; seed loader).
  `assets/seed/bomba_silabas.json` (**~80 sílabas**) y `bomba_categorias.json`
  (**~80 categorías**). Tests.
- **Hecho cuando:** la lógica decide explosión/eliminación de forma determinista con
  seed y los repos pasan tests.
- **Notas (hecho):** `BombaMode {silaba,categoria}`, `BombaPrompt`, `BombaConfig`
  (modo + jugadores 2–12 + rango fusible 10–60s, min<max), `BombaSession`
  (`pickFuseSeconds(rng,config)` puro en rango, `pasar`/`explode`/último en pie) y
  `PickPromptUseCase` (seen-set por modo). `bomba_silabas` + `bomba_categorias` +
  repo + seed loader. Seed **80 sílabas + 80 categorías**.

#### 0.69 — Presentación + integración (La Bomba)
- **Objetivo:** La Bomba jugable y en el menú.
- **Alcance:** `BombaFlowController` (reparte sílaba/categoría, gestiona el
  temporizador real en la UI sobre el instante objetivo, "pasar", explosión,
  eliminación, fin). Pantallas neón: setup (modo + jugadores), pantalla de juego
  (sílaba/categoría grande + botón "pasar" + animación/sonido de bomba al explotar +
  jugador eliminado), desenlace (último en pie). `BombaGame` (`GameDescriptor` + rutas
  + esquema), registro, migración → **8**, i18n es/en, tests.
- **Hecho cuando:** una partida elimina por explosión hasta dejar un ganador en ambos
  modos; `analyze` limpio y tests verdes.
- **Notas (hecho):** `BombaGame` (`GameDescriptor`: título "La Bomba",
  `Icons.local_fire_department`, `routes()`, esquema vía hooks) registrado.
  `bomba_routes.dart` (`/bomba` + play/game-over). `BombaFlowController` (setup/jugando/
  explotando/gameOver; `iniciar`, `pasar`, `explotar`, `continuarTrasExplosion`). La
  **mecha es un `Timer` oculto** propiedad de `BombaPlayScreen` (sin cuenta atrás
  visible); el controlador es puro/determinista. `kAppDatabaseVersion` 7→**8** con
  migración de `bomba_silabas` + `bomba_categorias`. Tests migración v8 + widget.
  **Incidente resuelto:** los tests de la play screen colgaban (cascadeo de timers en
  `pump(15s)` + sqflite_ffi en zona fake-async); arreglado desmontando el widget para
  drenar timers + `tester.runAsync` para el ffi. Suite 705 → **731 verdes**. `pubspec`
  0.69.0+69.

### F19 — "¿Cómo se juega?" — reglas por juego (v0.70)

#### 0.70 — Infraestructura de reglas + pantallas
- **Objetivo:** que cada juego explique cómo se juega desde su setup.
- **Alcance:** el `GameDescriptor` aporta su explicación (pasos/cómo se juega,
  localizada es/en) sin acoplar el menú; una `RulesScreen` reutilizable que pinta esas
  reglas con estética neón; un botón/icono **"¿Cómo se juega?"** en el setup de los
  **7 juegos** (Es un 10 pero, Impostor, Trivia, Wavelength, Tabú, Yo Nunca, La Bomba).
  i18n es/en de todas las reglas; tests de widget de la `RulesScreen` y de la presencia
  del botón.
- **Hecho cuando:** desde el setup de cada juego se abre su pantalla de reglas en el
  idioma activo; `analyze` limpio y tests verdes.
- **Notas (hecho):** `lib/games/_shared/presentation/rules_screen.dart` (`RulesScreen`
  neón reutilizable: `NeonBackground`+`NeonPanel`, pasos numerados, scrollable y
  accesible). Botón `IconButton(Icons.help_outline)` con tooltip "¿Cómo se juega?" en
  la AppBar del setup de los **7 juegos**, que hace `Navigator.push(MaterialPageRoute)`
  a la `RulesScreen` (sin ruta go_router). **39 claves** ARB es/en (`comoSeJuega` +
  `reglas<Juego><N>`). Tests de `RulesScreen` (5) + presencia del botón (impostor/
  trivia/bomba). Suite 731 → **739 verdes**. `pubspec` 0.70.0+70.

#### 0.71 — Auditoría con todas las skills relevantes + correcciones
- **Objetivo:** revisar la calidad de todo el proyecto (con foco en lo nuevo: F14–F19)
  usando las skills del proyecto y corregir los hallazgos accionables.
- **Alcance:** auditoría por dimensiones aplicando las skills relevantes —
  `flutter-apply-architecture-best-practices` (límites de capas/bounded contexts),
  `flutter-build-responsive-layout` + `flutter-fix-layout-issues` (overflows/
  responsive), `accessibility-compliance` (a11y de las pantallas nuevas),
  `security-review` (entrada de usuario/SQL/persistencia), `flutter-flame-games`
  (dial y componentes Flame), `dart-run-static-analysis` (analyze + `dart fix`),
  `dart-collect-coverage` (cobertura LCOV) y `flutter-add-widget-test`/
  `flutter-add-integration-test` (huecos de test) — más una **revisión adversarial**
  (`judgment-day`) sobre lo crítico. Se corrigen los hallazgos accionables.
- **Hecho cuando:** auditoría documentada con hallazgos por severidad, correcciones
  aplicadas, `analyze` 0 issues, tests verdes y cobertura reportada.
- **Notas (hecho):** auditoría con **6 revisores paralelos** (arquitectura, layout/
  responsive, accesibilidad, seguridad, Flame, lógica/cobertura), cada uno aplicando su
  skill, con justificación adversarial por hallazgo. **Limpio de fondo:** arquitectura
  (dominios puros, sin fugas domain→data, registry desacoplado) y seguridad (todas las
  queries parametrizadas, 0 inyección). **Correcciones aplicadas:**
  - *Lógica (bug real):* `TriviaFlowController._finishRound` sin try/catch podía congelar
    el controller si el pool encogía a media partida → ahora cae a `TriviaFase.error`.
  - *Layout/textScaler:* `tabu_turn_screen` (ListView prohibidas sin `shrinkWrap`/
    `NeverScrollable`), `bomba_play_screen` (prompt con `FittedBox`, holder `maxLines:1`,
    overlay de explosión scrollable), nombres de equipo `maxLines:1`, frase de Yo Nunca
    scrollable, `GameWidget` del dial con `minHeight`.
  - *Accesibilidad:* respuestas de trivia `InkWell`+`minHeight:56` (foco/teclado), hints
    semánticos en botones de Tabú, `liveRegion` en eliminado de Bomba y contador de Tabú,
    `minimumSize` en "Siguiente" de Yo Nunca, etiqueta de banda (texto) en revelado de
    Wavelength.
  - *Flame (rendimiento):* dial sin allocations por frame (`Paint`/`Path`/geometría
    cacheados), `resumeEngine` en modo clue, `pauseEngine` una vez por transición, y la
    pantalla de adivinar ya no invalida el `Picture` cache en cada tick de arrastre.
  - *Robustez:* `bomba_seed_loader` valida cada elemento del JSON (no `cast` ciego).
  - **Cobertura de tests:** +36 tests (flow controllers de Tabú/Bomba/Yo Nunca que
    faltaban + camino de error de trivia + edge cases). Cobertura global **74.1%**
    (impostor 85.8%, yo_nunca 88.1%, trivia 79.2%, tabu 78.3%, bomba 76.4%).
  - *Aceptados sin cambio (documentado):* el campo `_pickPromptUseCase` de Bomba es
    intencional (preserva el no-repetir entre rondas); `pickFuseSeconds` mantiene rango
    `[min,max)`.
  - Suite 739 → **775 verdes**, `analyze` 0 issues. `pubspec` 0.71.0+71.

---

## Correcciones de UX y contenido (Fase F21)

Lote de mejoras pedidas por el usuario tras probar la app: responsive global,
arreglo a fondo de Wavelength, Yo Nunca más explícito y banco del Impostor a 2000+.
Tras cada versión: `flutter analyze` 0 issues y `flutter test` verde.

### 0.72 — Arreglo a fondo de Wavelength (hecho)
- **Objetivo:** corregir los 3 problemas reportados: texto cortado, cuelgues/crashes
  y flujo confuso.
- **Hecho cuando:** se puede jugar una partida completa sin cuelgues, con texto
  legible y el rol/turno claro.
- **Notas (hecho):** **BUG RAÍZ del cuelgue:** los setters del dial Flame
  (`setMode/setTarget/setConceptLabels`) tenían un guard `if (isLoaded)` que
  **descartaba en silencio** las llamadas hechas en el primer `build()` (antes de que
  Flame completara `onLoad()`), dejando el dial congelado/en blanco. Corregido con
  **estado pendiente** que se aplica al terminar `onLoad`. **Texto:** etiquetas de
  conceptos reposicionadas y además renderizadas como widgets Flutter (`Wavelength
  ConceptLabelsRow`) con `maxLines`/ellipsis. **Flujo:** instrucciones claras es/en
  (psíquico ve el objetivo y da pista / pasar al grupo / el grupo mueve el dial /
  puntuar). +12 tests (recorrido completo setup→…→gameOver). Suite → **791 verdes**.
  `pubspec` 0.72.0+72.

### 0.73 — Responsive global (cortes de texto) (hecho)
- **Objetivo:** que no se corte ninguna letra en ninguna pantalla (móvil pequeño,
  horizontal, textScaler hasta 2.0).
- **Hecho cuando:** las pantallas clave no desbordan a textScaler 2.0 en 320×600.
- **Notas (hecho):** barrido por las pantallas de los 7 juegos + menú + reglas:
  `maxLines`/`overflow`/`Flexible`/`FittedBox`/scroll donde faltaba; `NeonText` ahora
  reenvía `maxLines`/`overflow`/`textAlign` a su `Text` interno. **Menú:** icono de
  tarjeta responsive (se encoge con el textScale) + suelo del `childAspectRatio` más
  bajo para que las tarjetas no desborden con texto grande. **Fin de Bomba:** nombre
  de ganador en `FittedBox(scaleDown)` (un nombre larguísimo se reduce hasta caber).
  Tests de regresión de overflow a textScaler 2.0 (reveal con palabra larga, trivia
  ganadores, fin de Bomba, menú). `pubspec` 0.73.0+73.

### 0.74 — Yo Nunca explícito (+18) (hecho)
- **Objetivo:** subir el nivel "picante" a contenido explícito para adultos.
- **Hecho cuando:** el nivel picante es +18 explícito y el aviso lo deja claro.
- **Notas (hecho):** se sustituyeron las frases picantes suaves por **94 frases
  explícitas +18** (se conservan 98 suaves; total 192). Aviso reforzado: "Contenido
  explícito (+18)… solo si todos son mayores de 18 y dan su consentimiento" (es/en).
  Límites mantenidos (nada con menores, no consentido, ni odio).

### 0.75 — Banco del Impostor a 2000+ palabras (hecho)
- **Objetivo:** ampliar el banco del Impostor a ~2000 palabras (con pista obligatoria).
- **Hecho cuando:** el seed supera 2000 palabras válidas, cada una con su pista.
- **Notas (hecho):** banco a **2060 palabras** (generadas en 7 tandas paralelas por
  categorías + 1 de relleno de nichos; fusionadas, validadas —cada una con pista no
  trivial— y deduplicadas: 240 duplicados cruzados fuera). Test de seed que exige
  **≥2000** con `word`/`hint` no vacíos, `word != hint` y sin duplicados. Suite total
  → **795 verdes**. `pubspec` 0.75.0+75.

---

## F22 — Auditoría completa con testeo (backlog de correcciones)

> Auditoría multidimensional ejecutada con las skills del proyecto
> (`flutter-apply-architecture-best-practices`, `hexagonal-architecture`,
> `accessibility-compliance`, `flutter-flame-games`, `flutter-build-responsive-layout`,
> `dart-collect-coverage`, `dart-run-static-analysis`). **Estado base sano:**
> `flutter analyze` sin issues, **823 tests verdes**, **cobertura de líneas 78,0 %**
> (5923/7591), paridad i18n es/en perfecta (310 = 310 claves), dominio puro (sin
> `Random()` directo, sin Flutter/sqflite en `domain/`), menú desacoplado, cero strings
> de UI hardcodeados, sin APIs deprecadas (`withOpacity`), sin TODOs reales.
>
> Los hallazgos siguientes quedan registrados **para corregir a futuro**, por prioridad.
> Cada uno propone una versión destino.

### Prioridad ALTA

- **0.76 — [ARQUITECTURA] Mover `abandon_game_dialog` a `_shared/`.**
  `lib/games/impostor/presentation/abandon_game_dialog.dart` vive dentro del *bounded
  context* del Impostor pero lo importan **5 juegos (10 ficheros)**: trivia
  (pass_device, question), tabu (turn, scoreboard), wavelength (clue, guess, reveal,
  pass_device), yo_nunca (play), bomba (play). Viola la regla de Screaming Architecture
  (aislamiento de contextos: lo compartido va en `lib/core/` o `lib/games/_shared/`).
  **Fix:** mover a `lib/games/_shared/presentation/abandon_game_dialog.dart` y actualizar
  los 10 imports. Es el **único** acoplamiento cruzado entre juegos del proyecto.

### Prioridad MEDIA

- **0.77 — [TESTING] Cobertura de las dos superficies Flame puras.**
  `bomba_play_screen.dart` (52 %) y `es_un_10_pero/card_flip_game.dart` (48 %) no tienen
  test de widget/overflow fiable (Flame `GameWidget` + timers no se pueden bombear en
  `testWidgets`). Riesgo: cortes de texto o regresiones de layout no detectados.
  **Fix:** tests que aíslen el *chrome* Flutter (stub del `GameWidget`) o golden tests
  del canvas. Son el único hueco de la auditoría de overflow de 28 pantallas.

- **0.78 — [FLAME/RESPONSIVE] Verificar el gesto del dial de Wavelength dentro del scroll.**
  Las pantallas `clue`/`guess` se hicieron *scrollables* para no cortar texto con letra
  grande; el dial Flame (con `DragCallbacks`) quedó dentro de un `SingleChildScrollView`.
  Por las reglas del *gesture arena* el reconocedor inmediato de Flame debería ganar al
  arrastrar sobre el dial, y el toque fija la aguja — pero **falta confirmarlo en
  dispositivo físico**. **Fix si se confirma conflicto:** condicionar el scroll a escalas
  de texto grandes, o aislar el gesto del dial del scroll.

- **0.79 — [ACCESIBILIDAD] El dial no anuncia la posición de la aguja.**
  Las pantallas del dial usan `Semantics(excludeSemantics: true)` con etiqueta estática,
  así que un lector de pantalla no recibe feedback al mover/fijar la aguja.
  **Fix:** añadir un `Semantics(liveRegion: true, value: '<posición>%')` que refleje
  `needlePosition` fuera del `GameWidget`.

- **0.80 — [TESTING] Tests de integración/flujo por juego + routing.**
  Solo el Impostor tiene test de integración (`integration_test/impostor_flow_test.dart`).
  Cobertura de routing baja: `core/routing/app_router.dart` 28 %, `*_routes.dart` 55–66 %.
  **Fix:** un test de flujo por juego (navegación de entrada, ronda completa, y el nuevo
  botón "volver al menú" → `/`).

### Prioridad BAJA / NIT

- **0.81 — [TESTING] Regresión del botón "volver al menú".**
  Se añadió el botón superior-izquierda a las 26 pantallas, pero no hay test que garantice
  su presencia: el bug de "quedar atrapado" podría volver en silencio.
  **Fix:** test por pantalla de entrada que verifique `VolverAlMenuButton` y su navegación
  a `/`.

- **0.82 — [TESTING] Ramas de dominio sin cubrir.**
  Modelos puros con ramas sin test (baratos y de alto valor): `trivia/domain/tematica`
  (14 %), `es_un_10_pero/domain/card` (31 %), `bomba/domain/bomba_prompt` (38 %),
  `impostor/data/impostor_word` (40 %), `bomba/domain/bomba_config` (57 %).
  **Fix:** unit tests de los casos límite (`fromMap`/`copyWith`/validaciones).

- **0.83 — [CALIDAD] Abstracción mínima de logging.**
  `core/audio/audio_service.dart` usa `debugPrint` en dos `catch` (líneas 157, 260). Es
  aceptable (se elimina en release) pero conviene un pequeño helper de logging para
  consistencia futura. NIT.

> **Nota:** `app_localizations_en.dart` aparece con 0 % de cobertura — es **código
> generado** y la app corre en `es` por defecto; no es un hallazgo, se excluye.

---

## Mapeo con `plan.md`

| Fase (`plan.md`) | Versiones |
|---|---|
| F1 — Scaffold | 0.1 – 0.5 |
| F2 — Es un 10 pero | 0.6 – 0.9 |
| F3 — Base SQLite Impostor | 0.10 – 0.12 |
| F4 — Impostor lógica | 0.13 – 0.15 |
| F5 — Impostor UI | 0.16 – 0.21 |
| F6 — CRUD palabras | 0.22 – 0.24 |
| F7 — Pulido | 0.25 – 0.30 |
| F8 — Correcciones de auditoría | 0.31 – 0.38 |
| F9 — Assets (imágenes y sonidos) | 0.39 – 0.41 |
| F10 — Estadísticas e i18n | 0.42 – 0.45 |
| F11 — Rediseño visual neón | 0.46 – 0.47 |
| F12 — Correcciones 2ª auditoría | 0.48 |
| F13 — Votación Impostor + cuenta atrás | 0.49 |
| F14 — Trivia "Preguntas por puntos" | 0.50 – 0.58 |
| F15 — Wavelength (sintonía) | 0.59 – 0.63 |
| F16 — Tabú | 0.64 – 0.65 |
| F17 — Yo Nunca | 0.66 – 0.67 |
| F18 — La Bomba | 0.68 – 0.69 |
| F19 — "¿Cómo se juega?" (reglas por juego) | 0.70 |
| F20 — Auditoría multi-skill | 0.71 |
| F21 — Correcciones de UX y contenido | 0.72 – 0.75 |
| F22 — Auditoría completa con testeo (backlog) | 0.76 – 0.83 |

> **F14–F18** son cinco juegos nuevos vía `GameRegistry`, **F19** añade las pantallas
> de reglas por juego y **F20** es una auditoría multi-skill — todo dentro del diseño
> local (sin backend). Las funcionalidades que quedan *fuera de alcance* por requerir
> backend o decisiones de producto (multijugador online, cuentas/cloud) entran a
> partir de **0.72**.
