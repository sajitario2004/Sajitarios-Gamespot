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

> A partir de **0.49** entran las funcionalidades hoy *fuera de alcance* (requieren backend o decisiones de producto): multijugador online, cuentas/cloud y nuevos juegos vía `GameRegistry`.
