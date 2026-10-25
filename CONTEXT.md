# CONTEXT.md — Sajitarios Gamespot

> **Estado (2026-10-25):** migración de Flutter a Unity en curso, rama `feature/unity-port`. Hechas las fases 0–10:
> base, núcleo, menú y **los 7 juegos portados y jugables** (Wavelength reescrito y arreglado). Versión en curso:
> V059a/V059b (plan de mejoras y juegos nuevos, en `Docs/PLAN_MEJORAS.md`); después, fases 11–12 (pulido y builds).
> **Pendiente de decisión del autor:** permiso para descargar las fuentes (V014): iconos Material Symbols y una
> fuente de texto con negrita real. Hasta entonces los iconos son sustitutos de texto ("<", "?", "))").
> Qué mejoras y cuáles de los 5 juegos nuevos se hacen (V059a/V059b).
> **Pendiente de probar a mano por el autor:** abrir `SajitariosGamespot/` en Unity 6000.3.27f1, Play en la escena
> `App` y jugar una partida de cada juego (sobre todo Wavelength, que estaba roto, y la mecha de La Bomba).

Documento vivo con todo el contexto del proyecto. Una IA nueva (o el mismo proyecto en otro PC) tiene que poder retomar el trabajo leyendo solo `CLAUDE.md`, este archivo, `LEARNINGS.md` y la versión en curso de `VERSIONS.md`.

---

## 1. Qué es

App **móvil (Android + iOS), offline y local** que funciona como **menú de minijuegos tipo party games**: desde el menú se elige un juego y se juega en un solo móvil, pasándoselo entre los jugadores. Sin backend, sin cuentas. Persistencia local con **SQLite**. Toda la UI en **español** (con inglés como segundo idioma). Estética **neón cyberpunk**. Tiene que ser fácil **añadir juegos nuevos** sin tocar el resto.

Historia: otra IA construyó la app completa en **Flutter** (v0.1 → v0.75, fases F1–F21, 823 tests). El 2026-10-25 el autor decidió **convertirla entera a Unity**. El Flutter queda en `Legacy/Flutter/` como especificación de solo lectura hasta que el autor valide la paridad; después se borra.

## 2. Intención original del autor (antes de cualquier decisión técnica)

- App local que funciona como menú de juegos simples, construidos desde cero, con SQLite, pensada para añadir juegos.
- **"Es un 10 pero":** saca una carta random de una baraja normal, solo entre **A y 10** (sin J, Q, K). Solo muestra la carta; el autor dará más contexto más adelante.
- **"El Impostor":**
  - Antes de la partida: nombres de los jugadores (**máximo 15**), número de impostores **entre 1 y 5**, y opción de **pista** activable.
  - Reglas especiales: **10 % de probabilidad de que TODOS sean impostores** y **10 % de que NINGUNO lo sea**.
  - Con pista activa, el impostor ve "impostor" + una pista de la palabra (ejemplo del autor: palabra "pirata", pista "barco"). **Todas las palabras de la BD tienen pista obligatoria.**
  - Durante la partida: palabra aleatoria; se pasa el móvil en el **mismo orden en que se introdujeron los nombres**; cada jugador pulsa **"Revelar"**: si no es impostor ve la palabra; si lo es, ve "impostor" (y la pista si está activa).
  - Ejemplo del autor: palabra "playa"; jugadores Nacho, Iker y Lucía; Nacho es impostor sin pista → Nacho ve "impostor", Iker y Lucía ven "playa".
- Decisiones de entonces: Flutter + sqflite (Unity se descartó por "overkill"; el autor cambia de opinión el 2026-10-25), Android e iOS, seed de palabras + palabras propias editables (las seed de solo lectura), "Es un 10 pero" como MVP puro.
- Después pidió (fases F13–F21 del Flutter): votación por rondas en el Impostor, cuenta atrás de 5 s en la carta, cinco juegos nuevos (Trivia, Wavelength, Tabú, Yo Nunca, La Bomba), pantalla "¿Cómo se juega?" en cada juego, responsive sin cortes de texto, Yo Nunca picante **+18 explícito** y bancos grandes (2000+ palabras del Impostor, 1000+ preguntas).

## 3. Decisiones

| Fecha | Decisión | Quién |
|---|---|---|
| 2026-10-25 | Convertir el proyecto entero a **Unity** con paridad funcional con la app Flutter v0.75 | Autor |
| 2026-10-25 | Forma de trabajo con `CLAUDE.md`, `CONTEXT.md`, `LEARNINGS.md`, `TREE.md` (+ `VERSIONS.md` como hoja de ruta, igual que en sus otros proyectos) | Autor |
| 2026-10-25 | **Mismo repo**: Unity en `SajitariosGamespot/`; Flutter a `Legacy/Flutter/` hasta validar la paridad, luego se borra | Autor (opción recomendada) |
| 2026-10-25 | **UI: uGUI + TextMeshPro construida por código** con un kit neón propio; Canvas *Screen Space – Camera* + Bloom de URP | Autor (opción recomendada) |
| 2026-10-25 | **Persistencia: SQLite con sqlite-net** (`gilzoide/unity-sqlite-net` 1.3.3) | Autor (opción recomendada) |
| 2026-10-25 | **Git:** rama `feature/unity-port`, commit por versión, permiso antes de push/merge a `main` | Autor (opción recomendada) |
| 2026-10-25 | Unity **6000.3.27f1** (la única instalada con módulos Android + iOS) | Claude |
| 2026-10-25 | Orientación **vertical** fija (juegos de pasar el móvil) | Claude, revisable |
| 2026-10-25 | Localización con un servicio propio ligero (JSON convertido de los ARB, plurales ICU), sin el paquete Localization ni Addressables | Claude, revisable |
| 2026-10-25 | BD Unity arranca en versión 1 con el esquema final (v8) del Flutter: no hay instalaciones Unity previas que migrar | Claude |
| 2026-10-25 | Se corrigen de paso los fallos heredados de §8.1; el resto se porta 1:1 | Claude, revisable |

## 4. Arquitectura Unity

```
SajitariosGamespot/Assets/_Gamespot/
  Scenes/App.unity                 única escena: cámara URP, EventSystem, AppRoot
  Scripts/
    Core/            Gamespot.Core            noEngineReferences: IRandomProvider, tipos puros
    Core.Runtime/    Gamespot.Core.Runtime    Db, Audio, Localization, Theme, UI kit, Navigation, Settings
    Games/_Shared/   Gamespot.Games.Shared    IGameDescriptor, RulesScreen, AbandonGameDialog, BackToMenuButton
    Games/<Juego>/Domain/              Gamespot.Games.<Juego>.Domain   puro (noEngineReferences)
    Games/<Juego>/{Data,Presentation}/ Gamespot.Games.<Juego>          repos, schema, seed, flow, pantallas
    Menu/            Gamespot.Menu            MenuScreen: solo recibe la lista de descriptores
    App/             Gamespot.App             composition root: GameRegistry + Bootstrap
    Editor/          Gamespot.Editor          configuración, importadores, builders
  Resources/Seed/*.json   Resources/Localization/{es,en}.json
  Audio/  Art/  Fonts/
  Tests/EditMode/  Tests/PlayMode/
```

- **Composition root:** `App/GameRegistry.cs` es el único sitio que conoce los juegos (equivale a `gameRegistryProvider`). El menú y el router reciben `IReadOnlyList<IGameDescriptor>`.
- **Contrato `IGameDescriptor`** (port de `GameDescriptor`): `Id`, `TitleKey`, `DescriptionKey`, `Icon`, `EntryRoute`, `Routes()` (pantallas del juego), `OnCreateTables(db)`, `OnUpgradeTables(db, old, new)`, `SeedIfEmpty(db)`, `RulesKeys` (pasos de "¿Cómo se juega?").
- **Navegación:** `Router` con rutas por texto (`/`, `/impostor`, `/impostor/pass`…), pila propia, botón atrás de Android, rutas desconocidas → pantalla de error con "Volver al menú".
- **Estado de partida:** un `FlowController` por juego (port del `Notifier` de Riverpod): fases como enum, acciones que no hacen nada en la fase equivocada, sin textos de UI. Las pantallas leen el controller; las rutas no llevan parámetros.

## 5. Núcleo (especificación heredada del Flutter)

### 5.1 Arranque y menú
- En Flutter todo era perezoso (BD al primer acceso, audio precargado solo en "Es un 10 pero"). En Unity el `Bootstrap` abre la BD, aplica esquema y seeds, precarga audio y carga el idioma antes de mostrar el menú.
- **Orden del registro:** 1 Es un 10 pero (`es_un_10_pero`, icono "style"), 2 El Impostor (`impostor`, "theater_comedy", `/impostor`), 3 Preguntas por puntos (`trivia`, "quiz", `/trivia`), 4 Wavelength (`wavelength`, "tune", `/wavelength`), 5 Tabú (`tabu`, "do_not_disturb_on", `/tabu`), 6 Yo Nunca (`yo_nunca`, "front_hand", `/yo-nunca`), 7 La Bomba (`bomba`, "local_fire_department", `/bomba`).
- **Menú:** AppBar con el título de la app en NeonText con PulseGlow (0.8–1.3) y el selector de idioma. Cuerpo: NeonBackground › SafeArea › cabecera `menu_header.png` (alto máx. 140, cover, oculta a lectores) + rejilla. Rejilla de 3 columnas si el ancho > 600, si no 2; ancho máx. 900, padding 16, separación 16. Tarjeta: acento cian (índice par) o magenta (impar), radio 20, borde 1.5 px, icono con glow, título (máx. 2 líneas) y descripción (máx. 3 líneas, color atenuado). Lector de pantalla: "Jugar a {titulo}. {descripcion}". Catálogo vacío: panel con icono, `menuVacioTitulo` y `menuVacioMensaje`.

### 5.2 Base de datos
- Archivo `sajitarios_gamespot.db` (en Unity, en `Application.persistentDataPath`). Flutter: versión 8, `PRAGMA foreign_keys = ON` (ninguna tabla tiene claves foráneas).
- `onCreate` llama a `OnCreateTables` de cada descriptor en orden de registro; `onUpgrade`, a `OnUpgradeTables` de cada uno, que aplica solo sus pasos.
- **Seeds:** si `COUNT(*)` de la tabla > 0 no hace nada; si no, lee el JSON, valida (entrada inválida → error), recorta textos, inserta en lote con `is_seed = 1` e ignorando conflictos, y devuelve cuántas filas entraron.

| Tabla | Columnas | Índice |
|---|---|---|
| `impostor_words` | `id` PK AI, `word` TEXT NN **UNIQUE COLLATE NOCASE**, `hint` TEXT NN, `is_seed` INT NN DEF 0, `created_at` INT NN (epoch ms) | `idx_impostor_words_word(word)` |
| `game_history` | `id` PK AI, `created_at` INT NN, `word` TEXT NN, `hint` TEXT nullable, `n_players` INT NN, `n_impostors` INT NN, `hint_enabled` INT NN, `players_json` TEXT NN | `idx_game_history_created_at(created_at)` |
| `trivia_questions` | `id` PK AI, `tematica_id` TEXT NN, `difficulty` TEXT NN, `enunciado` TEXT NN, `options_json` TEXT NN, `correct_index` INT NN, `is_seed` INT NN DEF 0 | `idx_trivia_questions_tema_diff(tematica_id, difficulty)` |
| `trivia_winners` | `id` PK AI, `name` TEXT NN **UNIQUE COLLATE NOCASE**, `wins` INT NN DEF 0 | — |
| `wavelength_spectra` | `id` PK AI, `izquierda` TEXT NN, `derecha` TEXT NN, `is_seed` | — |
| `tabu_words` | `id` PK AI, `palabra` TEXT NN, `prohibidas_json` TEXT NN, `is_seed` | — |
| `yo_nunca_statements` | `id` PK AI, `frase` TEXT NN, `intensidad` TEXT NN, `is_seed` | `idx_yo_nunca_statements_intensidad(intensidad)` |
| `bomba_silabas` | `id` PK AI, `silaba` TEXT NN, `is_seed` | — |
| `bomba_categorias` | `id` PK AI, `categoria` TEXT NN, `is_seed` | — |

### 5.3 Seeds (`Resources/Seed/`, copiados de `Legacy/Flutter/assets/seed/`)
| Archivo | Forma | Elementos | Mínimo de los tests |
|---|---|---|---|
| `impostor_words.json` | `[{word, hint}]` | 2060 | ≥ 2000, sin duplicados sin distinguir mayúsculas, `word ≠ hint` |
| `trivia_questions.json` | `[{tematica, difficulty, enunciado, opciones[4], correcta}]` | 1151 (fácil 471 / difícil 462 / muy difícil 218) | ≥ 1000 |
| `wavelength_spectra.json` | `[{izquierda, derecha}]` | 80 | ≥ 70 |
| `tabu_words.json` | `[{palabra, prohibidas[]}]` | 120 (5 prohibidas cada una) | ≥ 100, 4–5 prohibidas |
| `yo_nunca_statements.json` | `[{frase, intensidad}]` | 192 (suave 98 / picante 94) | ≥ 140, suave ≥ 50, picante ≥ 30 |
| `bomba_silabas.json` | `["CA", …]` | 93 | ≥ 70 |
| `bomba_categorias.json` | `["nombres de animales", …]` | 83 | ≥ 70 |

Temáticas de la trivia: deportes 176, videojuegos 144, cocina 135, cultura_general 126, musica 122, cine 118, historia 115, ciencia 113, geografia 102. Las preguntas vienen de Open Trivia DB (CC BY-SA 4.0, atribución obligatoria en la app) y de contenido generado; conviene una revisión factual.

### 5.4 Aleatoriedad
- Flutter: `RandomProvider` con `nextInt(max)` → [0,max), `nextDouble()` → [0,1), `nextBool()`, `pick(list)` = `list[nextInt(len)]` (lanza con lista vacía). **No** tiene `shuffle`: cada juego hace su Fisher-Yates `for i = n−1..1: j = nextInt(i+1); swap`.
- La secuencia de `Random(seed)` de Dart **no** coincide con la de C#: los tests que dependen de un valor concreto de una seed (p. ej. "seed 8 da 0.017") se reescriben con un `FakeRandom` guionizado; los de distribución se portan como estadísticos.

### 5.5 Audio
| Sonido | Archivo | Cuándo |
|---|---|---|
| `CardFlip` | `card_flip.wav` (0.12 s) | Es un 10 pero: al revelarse la carta (fin de la cuenta atrás) |
| `Reveal` | `reveal.wav` (0.30 s) | Impostor: al pulsar "Revelar" |
| `GameOver` | `game_over.wav` (0.50 s) | Impostor: al abrir el fin de partida (si hay sesión) |
- Ningún otro juego tiene sonido. Mute guardado en la preferencia `audio_enabled` (por defecto activado); errores silenciados. Botón: icono volumen on/off con tooltip `silenciarSonido` / `activarSonido`. Los WAV son placeholders sintetizados (16 bit, mono, 44.1 kHz).

### 5.6 Idiomas
- Preferencia `app_locale` con el código de idioma; sin valor = idioma del sistema; si no es es/en, español. Diálogo con "Idioma del sistema", "Español", "Inglés" y Cancelar.
- **305 claves** por idioma, mismas en los dos. Nombres en lowerCamelCase en español. Prefijos: `trivia*`, `wavelength*`, `tabu*`, `yoNunca*`, `bomba*`, `impostor*`, `esUn10Pero*`, `reglas<Juego>N`, `historial*`, `palabras*`, `menu*`; ~131 sin prefijo (compartidas y del Impostor: `setup*`, `error*`, `votacion*`…).
- 46 mensajes con placeholders y 7 con plurales: `impostoresContador`, `resumenImpostores` (con caso `=0`), `impostoresTexto`, `rondasContador`, `triviaVictorias`, `wavelengthRevealPuntos`, `tabuAciertosContador`.

### 5.7 Tema neón
| Nombre | Hex | Nombre | Hex |
|---|---|---|---|
| background | `#0A0A14` | textPrimary | `#E6F7FF` |
| surface | `#14141F` | textMuted | `#A8B6C8` |
| surfaceHigh | `#1B1B2A` | neonError | `#FF3D71` |
| neonCyan (primary) | `#00F0FF` | neonBlue (respuesta A) | `#00A8FF` |
| neonMagenta (secondary) | `#FF00E5` | neonGreen (respuesta B) | `#00FF7F` |
| neonViolet (decorativo) | `#B026FF` | neonRed (respuesta C) | `#FF2C55` |
| neonVioletText | `#C77DFF` | neonYellow (respuesta D) | `#FFE600` |
- Otros: outline `#3A3A55`, divider `#26263A`, surface low `#11111B`, highest `#222234`, scrim `#CC000000`.
- **Glow de cajas:** dos sombras, color con alfa 0.55·i y desenfoque 12·i, y alfa 0.30·i con desenfoque 28·i (+1 de extensión). **Glow de texto:** tres sombras (0.9/8i, 0.6/18i, 0.4/32i).
- **NeonPanel:** relleno `surface`, borde cian 1.5 px, radio 20, padding 16, glow opcional. **NeonBackground:** degradado radial centrado en (−0.6, −0.8), radio 1.4, del 10 % de magenta sobre el fondo al fondo, + rejilla cian al 5 % cada 36 px.
- **PulseGlow:** intensidad entre min y max (0.7–1.2 por defecto) en 1600 ms, ida y vuelta, ease-in-out; con "reducir animaciones" se queda en (min+max)/2.
- Botón principal: fondo cian, texto 18 w800, padding 24×16, radio 16. Campos de texto: relleno `surfaceHigh`, radio 14, borde cian al 30 % (al enfocar, cian 2 px). Diálogo: `surface`, radio 20, borde magenta al 45 %. Título de AppBar: 22 px w800 con glow cian.

### 5.8 Pantallas compartidas
- **RulesScreen(título, pasos):** título "¿Cómo se juega?", nombre del juego con glow, panel con filas numeradas (círculo con el número en cian + texto). Pasos: Impostor 6; Trivia, Wavelength, Tabú y Bomba 5; Es un 10 pero y Yo Nunca 4.
- **Botón "volver al menú"** (icono casa, tooltip `volverAlMenu`) en todas las pantallas. En setup y fin de partida va directo a `/`; con partida en curso pide confirmación.
- **Diálogo de abandonar:** título `salirDeLaPartidaTitulo`, mensaje `salirDeLaPartidaMensaje`, botones `seguirJugando` (falso) y `salir` (verdadero, glow magenta). Confirmar reinicia el controller del juego (y cancela temporizadores) y va a `/`. El botón atrás del sistema con partida en curso abre el mismo diálogo y, al confirmar, vuelve al setup del juego.
- **Normalización de nombres** (setups): recortar y colapsar espacios internos a uno; duplicados sin distinguir mayúsculas.

## 6. Especificación de los juegos

### 6.1 Es un 10 pero (`es_un_10_pero`)
- **Dominio:** `CardValue` A(1)…10 (sin J/Q/K); `CardSuit` espadas, corazones, diamantes, tréboles (`IsRed` = corazones y diamantes). `DrawCardUseCase`: `Pick(valores)` y después `Pick(palos)` (dos llamadas, en ese orden; 1/40 cada carta). No persiste nada.
- **Pantalla:** botón "Sacar carta" (icono casino, ancho completo, glow). Al pulsar: cuenta atrás 5→1 (un tic por segundo, botón deshabilitado, overlay con "Sacando carta en..." y el número a 96 px con glow cian, fondo al 72 %); a los 5 s se **saca la carta en ese momento**, se voltea y suena `CardFlip`. La etiqueta pasa a "Sacar otra carta" tras la primera carta. Pulsar durante la cuenta atrás no hace nada. Sin carta: pista "Pulsa "Sacar carta" para revelar una carta". Lector de pantalla: "Carta: {valor} de {palo}". AppBar: casa, título, reglas, mute. Ancho máx. 480, carta en proporción 5:7.
- **Carta:** tamaño que cabe en 5:7; radio 0.10·ancho. Flip de **0.55 s** con *easeInOutCubic*; escalaX = |cos(e·π)| (mín. 0.06), escalaY = 1 + 0.06·sin(e·π); la cara nueva aparece cuando e ≥ 0.5 (antes se ve el dorso). Sombra cian al 55 % que baja 8 + 18·sin(e·π) y se desenfoca 12 + 16·sin(e·π). **Cara:** relleno `surface`, contorno con glow cian; valor (0.16·ancho, w900) y palo (0.11·ancho) arriba a la izquierda y girados 180° abajo a la derecha; palo grande (0.40·ancho) en el centro; rojos en **magenta**, negros en **cian**. **Dorso:** `card_back.png` o, si falla, dibujado (relleno `surfaceHigh`, marco cian, marco interior magenta y rombos magenta).

### 6.2 El Impostor (`impostor`)
- **Dominio:** `Player(name)` (igualdad exacta), `Role {Palabra, Impostor}`, `Word(text, hint)` (recortados, no vacíos).
  - `GameConfig.Create(players, nImpostores, hintEnabled = false, rounds?)`: errores en orden `pocosJugadores` (< 3), `demasiadosJugadores` (> 15), `nombreVacio`, `nombresDuplicados` (trim + minúsculas). Impostores capados a [1, min(5, n−1)]. Rondas capadas a [1, max(1, n−3)]; si es nulo, el máximo (3–4 jugadores → 1; 5 → 2; 15 → 12).
  - `GameSession`: `RevealOrder` = orden de entrada; `Impostores`, `ImpostorCount`, `IsImpostor`, `RoleOf`, `Hint`.
  - `AssignRolesUseCase`: ver `CLAUDE.md` §5 (palabra → un `NextDouble()` → caso). Sin palabras → `NoWordsAvailable`.
- **Datos:** `WordRepository`: `GetAll` ordenado por `word COLLATE NOCASE, id`; `Search(q)` con `LIKE '%q%' ESCAPE '\'` (escapando `\ % _`); `Insert` (recorta, vacío → error, duplicado → `DuplicateWord`); `Update`/`Delete` con errores en orden: vacío, no existe, es seed (`ReadOnlySeedWord`), duplicado. `GameHistoryRepository`: `InsertFromSession`, `GetAll` (más reciente primero), `DeleteAll`, `Count`; `players_json` = `[{"name", "role": "impostor"|"palabra"}]`.
- **Flow:** fases `Setup, Iniciando, Pass, Reveal, Voting, GameOver, Error(sinPalabras | desconocido)`.
  - `Iniciar(config)` → `Pass` con índice 0. `Revelar()` → `Reveal`. `Avanzar()` → siguiente `Pass` o, tras el último, `Voting` (ronda 1 de `config.Rounds`).
  - `Votar(p)`: si `p` es impostor sale de los candidatos; si no quedan impostores → `GameOver` (ganan los jugadores); si se acaban las rondas → `GameOver` (gana el impostor); si no, ronda + 1. **Cada voto gasta una ronda**; los inocentes votados siguen en la lista; hay que pillar a **todos** los impostores. Con "todos" o "ninguno" el grupo no puede ganar (se conserva).
- **Pantallas:**
  - **Setup:** 3 filas de nombre al empezar (avatar numerado cian, campo, quitar deshabilitado con 3), "Añadir jugador" hasta 15 (luego "Máximo de jugadores alcanzado"), sin reordenar. Contador de impostores 1..min(5, n−1) (se recorta al quitar jugadores). Contador de rondas que empieza en 1 y solo se recorta. Interruptor de pista (apagado). Errores en snackbar. "Empezar" con spinner mientras inicia. Sin palabras → diálogo con "Ahora no" / "Gestionar palabras". AppBar: casa, reglas, historial, palabras.
  - **Pasar el móvil:** "Jugador X de Y", círculo cian con un móvil (72 px, 96 si ancho > 600), "Pásale el móvil a", nombre grande, ayuda, "Continuar" (64 px).
  - **Revelar:** "Es el turno de {nombre}", rol oculto hasta pulsar "Revelar" (glow magenta, suena `Reveal`); animación 350 ms (fade + escala 0.92 → 1). Impostor: panel magenta con pulso 0.85–1.4, "IMPOSTOR" y, con pista, "Pista" + la pista con glow violeta. Palabra: panel cian con pulso 0.8–1.2, "Tu palabra es" + la palabra. Botón "Ocultar y pasar" ("Ocultar y ver resultados" en el último).
  - **Votación:** "Ronda X de Y" (magenta), instrucción, tras cada voto "El impostor sigue entre vosotros"; candidatos en tarjetas cian; impostores pillados tachados en gris; confirmar "¿Expulsar a {nombre}?".
  - **Fin:** suena `GameOver` y se guarda el historial (una vez). **Nunca muestra roles ni la palabra.** "¡Habéis ganado!" (trofeo cian) o "El impostor sigue entre vosotros" (máscara magenta). Botones "Jugar otra" y "Volver al menú". Las partidas abandonadas no se guardan.
  - **Gestión de palabras:** búsqueda con *debounce* de **280 ms**; seed con borde violeta, chip "Predefinida" con candado y sin editar/borrar; propias con borde cian; formulario con palabra y pista obligatorias; confirmación al borrar; snackbars de resultado; estados vacío/sin coincidencias/error. Lista virtualizada (2060 filas).
  - **Historial:** estadísticas calculadas en la pantalla: total de partidas, palabra más repetida (agrupada en minúsculas, desempate por la menor, se muestra la grafía más reciente) y ranking de impostores (nombre exacto, más veces primero). Cada partida, desplegable: palabra, fecha corta + HH:mm, nº de jugadores e impostores, pista activa o no y el rol de cada jugador (el historial sí revela roles). "Borrar todo" con confirmación.

### 6.3 Preguntas por puntos (`trivia`)
- **Dominio:** `Difficulty {Facil, Dificil, MuyDificil}` (se guarda el nombre del enum; OpenTDB easy/medium/hard). Temáticas: cultura_general, videojuegos, cocina, cine, ciencia, geografia, historia, deportes, musica. `Question` con 4 opciones y `CorrectIndex` en [0,4). `TriviaConfig.Create(nombres, temáticas)`: 2–6 jugadores, nombres no vacíos ni duplicados, al menos una temática; guarda los nombres recortados.
  - `TriviaSession`: **9 rondas**, 3 por tramo: rondas 0–2 fácil, 3–5 difícil, 6–8 muy difícil. Fallar elimina. Termina a las 9 rondas o si no queda nadie; ganadores = los vivos al terminar (empate de supervivientes; si caen todos, nadie gana).
  - `DealQuestionsUseCase(vivos, pool)`: baraja una copia del pool (Fisher-Yates) y da `shuffled[i]` al jugador `i` (pregunta distinta para cada uno). El resultado es una **lista ordenada**, no un diccionario.
- **Datos:** `QuestionRepository.GetPool(temáticas, dificultad)` ordenado por id. `WinnerRepository.IncrementWins(name)` (insertar o ignorar + sumar 1, sin distinguir mayúsculas), `GetWins`, `GetAllRanked` (más victorias primero, desempate por nombre).
- **Flow:** fases `Setup, Iniciando, Pass, Question, GameOver, Error(sinPreguntas)`. `Iniciar`: si algún pool de las 3 dificultades tiene menos preguntas que jugadores → error. Reparte la ronda 0. `PasarDispositivo` → `Question`. `Responder(i)`: registra y pasa al siguiente jugador vivo de la ronda; al cerrar la ronda, avanza, recarga los pools y reparte la dificultad siguiente. Al terminar, +1 victoria a cada superviviente. Las preguntas pueden repetirse entre rondas.
- **Pantallas:** setup con 2 filas de nombre (máx. 6, avatares violeta), chips de temáticas (por defecto cultura general e historia; la última marcada no se puede desmarcar), ranking de victorias y atribución "Preguntas basadas en Open Trivia DB (CC BY-SA 4.0)". Pasar el móvil en violeta. Pregunta: "Ronda X de 9", nombre en violeta, enunciado en panel **violeta**, rejilla 2×2 con marcos **azul / verde / rojo / amarillo** y letras A–D; al pulsar, la correcta en verde, la elegida errónea en rojo, el resto atenuadas (250 ms), y a los **800 ms** avanza. Fin: "¡Habéis ganado!" + nombres, o "Nadie ha ganado". Sin sonido.

### 6.4 Wavelength (`wavelength`)
- **Dominio:** `Spectrum(left, right)`. `WavelengthConfig.Create(nombres, rondas = 8)`: 2–8 jugadores, rondas 1–20. Puntos (en `double`, bordes inclusivos): d = |respuesta − objetivo|; d ≤ 0.06 → 4, ≤ 0.12 → 3, ≤ 0.20 → 2, si no 0; la respuesta se recorta a [0,1]. `PickRoundUseCase`: `Pick(pool)` y objetivo = `NextDouble()` (puede repetirse espectro). `WavelengthSession`: cooperativo con un solo marcador; psíquico = `players[ronda % n]`.
- **Flow:** `Setup, Iniciando, Clue, Pass, Guess, Reveal, GameOver, Error(sinEspectros)`. `ConfirmarPista(texto)` (no vacío) → `Pass` → `Guess`; `SubmitGuess(pos)` → `Reveal` (aún no suma); `Next()` suma la ronda y va a `GameOver` o a una ronda nueva.
- **Pantallas:** setup (2–8 nombres, rondas ±1 en [1,20]). Pista: "Ronda X de Y", "Psíquico: nombre", instrucciones, fila de conceptos, dial en modo pista (ve el objetivo), campo de pista. Pasar el móvil. Adivinar: pista del psíquico + dial arrastrable (la aguja empieza en 0.5). Revelado: dial con bandas y objetivo apareciendo en **0.7 s**, texto de banda ("En el blanco" / "Cerca" / "Lejos" / "Fallo"; cian para 4 y 3, magenta para 2 y 0), "{n} puntos esta ronda" y "Total: {n} puntos" (**el total no incluye la ronda actual**, fijado por test). Fin: puntuación final y media por ronda.
- **Dial:** radio = min(0.44·ancho, 0.82·alto), centro en (ancho/2, 0.78·alto desde arriba); posición p ∈ [0,1] → ángulo θ = π + p·π (p = 0 izquierda, 0.5 arriba, 1 derecha). Arco cian (glow 0.055r + núcleo 0.025r), 5 marcas en p = 0, .25, .5, .75, 1. Bandas como cuñas cian superpuestas: lejos ±0.20 (alfa 0.20), cerca ±0.12 (0.30), blanco ±0.06 (0.45). Marcador del objetivo de 0.15r a 0.98r. Aguja magenta del centro a 0.92r con pomo de 0.045r. Entrada solo en modo adivinar: p = 1 − clamp(atan2(−(y−cy), x−cx), 0, π)/π. Alto del dial en pista y adivinar: clamp(0.42·alto disponible, 180, 320).

### 6.5 Tabú (`tabu`)
- **Dominio:** `TabuWord(palabra, prohibidas[4..5])`. `TabuConfig.Create(equipoA, equipoB, turnoSegundos = 60, objetivoVictorias = 3)`: errores en orden equipo A vacío, equipo B vacío, duplicados, turno fuera de [30,120], objetivo ≤ 0.
  - `TabuSession`: empieza el equipo A. Acierto +1; Saltar sin límite (no se muestra); Falta −1 sin bajar de 0 y **sin cortar el turno**. `TerminarTurno`: si los aciertos > 0, **+1 victoria de ronda** para el equipo; cambia de equipo y reinicia contadores. Gana el primero en llegar al objetivo. Palabras sin repetir en la partida (`PickTabuWordUseCase`; si se agotan, se queda la actual).
- **Pantallas:** setup (nombres de los dos equipos, chips 30/60/90 s, 60 por defecto; objetivo siempre 3). Turno: palabra en MAYÚSCULAS, lista de prohibidas, equipo en violeta, contador de aciertos en la AppBar, cuenta atrás (cian > 10, amarillo 6–10, rojo ≤ 5) y botones Acierto (verde), Saltar (cian), Falta (rojo). El temporizador vive en la pantalla (tic de 1 s; al llegar a 0 termina el turno). Marcador: "Objetivo: 3 victorias de ronda", "A: x / 3" (cian) y "B: y / 3" (violeta), "Siguiente turno". Fin: equipo ganador, marcadores y "Volver al menú" (no hay "jugar otra"). Sin sonido.

### 6.6 Yo Nunca (`yo_nunca`)
- **Dominio:** `Intensidad {Suave, Picante}`, `NeverStatement(frase, intensidad)`, `YoNuncaConfig` (al menos una intensidad). `DrawStatementUseCase(pool, intensidades, vistas)`: filtra, quita las vistas; si ya se vieron todas, vacía las vistas y vuelve a empezar (puede repetirse la última); elige con `NextInt`.
- **Flow:** `Setup, Jugando, Error(sinFrases)`; partida infinita, `Siguiente()`.
- **Pantallas:** setup con chips Suave (cian, marcado por defecto) y Picante (magenta). Con Picante activo aparece un **aviso fijo** (borde magenta, icono de aviso): contenido sexual explícito, solo para adultos; actívalo solo si todos tienen 18 o más y dan su consentimiento. Juego: la frase grande en cian en un panel con scroll y "Siguiente".

### 6.7 La Bomba (`bomba`)
- **Dominio:** `BombaMode {Silaba, Categoria}`, `BombaPrompt(texto, modo)` (clave = modo + id). `BombaConfig.Create(modo, nombres, minSeg, maxSeg)`: 2–12 jugadores, nombres válidos, min ≥ 10, max ≤ 60, min < max (el setup pasa siempre 10 y 60).
  - Mecha = min + `NextDouble()`·(max − min) → **[10, 60) segundos**.
  - `BombaSession`: empieza con la bomba el primer jugador; `Pasar()` → siguiente (circular); `Explode()` elimina al que la tiene y la bomba pasa al **siguiente** (vuelve al 0 si era el último); termina con ≤ 1 vivo, ganador el que queda. `NewRound` mantiene el índice y saca mecha nueva.
  - `PickPromptUseCase`: sin repetir por modo hasta agotar; luego se vacía.
- **Flow:** `Setup, Iniciando, Jugando, Explotando, GameOver, Error(sinPrompts)`. `Pasar()` no reinicia la mecha. `Explotar()` → `Explotando`; `ContinuarTrasExplosion()` → `GameOver` o ronda nueva con mecha y prompt nuevos (mismo modo).
- **Pantallas:** setup con chips Sílaba (por defecto) / Categoría y 2–12 nombres. Juego: nombre del que tiene la bomba en MAYÚSCULAS, sílaba o categoría en MAYÚSCULAS en un panel con borde rojo que **late** (0.7–1.2, 1600 ms), tipo de prompt, botón **PASAR** (64 px) y chip con los vivos. **Sin cuenta atrás visible**; la mecha se comprueba cada 50 ms (en Unity, en cada frame). Al explotar: overlay a pantalla completa **1,5 s** con fuego, "¡BOOM!", el eliminado y "ha sido eliminado", y sigue solo. Fin: trofeo, "Ganador" y el nombre (encogido para caber en una línea), "Volver al menú". Sin sonido.

## 7. Estado de la app Flutter (referencia)
- v0.75.0+75, Flutter 3.41.4 / Dart 3.11.1, Riverpod 3, go_router 17, Flame 1.37, sqflite. `flutter analyze` limpio, **823 tests**, cobertura 78 %.
- Código en `Legacy/Flutter/lib/`, tests en `Legacy/Flutter/test/`, roadmap completo (F1–F22) en `Legacy/Flutter/docs/version.md`, su contexto en `Legacy/Flutter/docs/context.md`.
- Assets de imagen y sonido son **placeholders** generados por código (`Legacy/Flutter/tool/generate_*.dart`) con el morado `#7C3AED` y ámbar antiguos.

## 8. Diferencias con el Flutter

### 8.1 Fallos heredados que se corrigen en el port (Claude, revisable)
- Títulos y descripciones del menú de 5 juegos, nombres de las temáticas de la trivia y de los palos de la carta, y la media de Wavelength estaban fijos en español → localizados.
- `AbandonGameDialog` vivía dentro del Impostor y lo importaban 5 juegos (hallazgo ALTA 0.76) → en `Games/_Shared/`.
- Impostor: el botón atrás del sistema no estaba interceptado en Votación ni en Fin de partida → interceptado.
- Yo Nunca: el botón atrás del sistema salía sin confirmar → diálogo de abandonar.
- Tabú y La Bomba: el temporizador seguía con el diálogo de salir abierto → se pausa.
- Tabú: la regla 4 decía que una prohibida pasa el turno, pero el código resta un acierto → se conserva el código y se corrige el texto.
- Wavelength: la aguja del revelado salía en 0.5 → muestra la respuesta del grupo; etiquetas duplicadas (lienzo + fila) → solo la fila; tocar bajo la base saltaba al polo derecho → polo más cercano.
- La Bomba: los nombres solo se recortaban → misma normalización que el resto. El setup pedía "¿Salir de la partida?" al volver sin haber partida → va directo, como los demás setups.
- Yo Nunca: la frase iba en un panel con scroll cuya máscara recortaba el halo → centrada sin scroll (las frases son cortas).
- BD y audio perezosos → preparados en el arranque. Botón de silencio también en el menú.
- Icono, splash, dorso y cabecera en morado/ámbar → placeholders con la paleta neón.

### 8.2 Deuda que se conserva (decide el autor)
- El selector de rondas del Impostor empieza en 1 y no sube solo al añadir jugadores.
- Las preguntas de la trivia y los espectros de Wavelength pueden repetirse entre rondas.
- Tabú no tiene "jugar otra"; el objetivo de victorias no se puede cambiar.
- La mecha de La Bomba no se puede configurar (siempre 10–60 s).
- Contenido de los bancos generado por máquina: conviene una revisión factual.

## 9. Registro de licencias
| Componente | Licencia | Uso |
|---|---|---|
| Open Trivia DB (preguntas base de la trivia) | CC BY-SA 4.0 | Atribución en el setup de la trivia |
| `gilzoide/unity-sqlite-net` 1.3.3 (SQLite-net 1.9 + SQLite3 Multiple Ciphers) | MIT | Persistencia SQLite. Revisado el 2026-10-25: 299 estrellas, activo (último push 2026-10-14), no archivado |
| Paquetes de Unity (URP, uGUI/TextMeshPro, Input System, Test Framework, Pipeline) | Unity Companion License | Motor, UI, entrada, tests y puente de la CLI |

## 10. Registro de trabajo (lo más reciente arriba)

### 2026-10-25 · V055–V059 · La Bomba (los 7 juegos portados)
- Dominio puro (mecha [10,60), pasar sin tocar la mecha, explosión que pasa la bomba al siguiente, sílabas y categorías sin repetir por modo), datos (93 + 83), flujo y 3 pantallas: setup, juego con mecha oculta y aviso «¡BOOM!» de 1,5 s, fin.
- La mecha y el aviso se cuentan en `Tick` y se pausan con un diálogo abierto. Kit: `AppBar.AddInfo` (pastilla de vivos), `ScreenScaffold.Root` (para tapar la pantalla con un aviso a pantalla completa) e icono `Group`.
- Gotcha de test: el router también llama a `Tick` en cada frame de un `[UnityTest]`; los tests de tiempo comparan contra lo ya consumido.
- Tests: 213 EditMode + 31 PlayMode en verde; arnés .NET 168.

### 2026-10-25 · V052–V054 · Yo Nunca
- Dominio, seed de 192 frases, setup con aviso +18 fijo mientras Picante está activo y juego infinito sin repetir hasta agotar. Atrás y casa piden confirmación (arreglo). La frase va centrada sin scroll: la máscara recortaba el halo.

### 2026-10-25 · V047–V051 · Tabú
- Dominio, seed de 120, setup (equipos y 30/60/90 s), turno con cuenta atrás que se pausa con el diálogo de salir, marcador y fin. `NeonButton.Compact()` para tres botones en una fila sin partir la etiqueta.

### 2026-10-25 · V041–V046 · Wavelength reescrito y arreglado
- El autor avisó de que Wavelength estaba roto. El dominio del Flutter era correcto; lo roto era el dial de Flame (estado perdido antes de cargar, aguja en 0,5 al revelar, etiquetas duplicadas, toque bajo la base al polo derecho, dial dentro de un scroll) y la UX (total sin la ronda actual, pista vacía ignorada en silencio, textos sin tildes).
- Reescrito: dial como `Graphic` de uGUI con malla propia, pantallas del dial sin scroll, aguja del grupo en el revelado, total con la ronda, tolerancia en los bordes de puntuación, textos corregidos.
- Gotcha de uGUI: un `Graphic` propio creado por código necesita el `CanvasRenderer` en el `new GameObject(...)`; si no, no pinta nada y no da error.
- `Invoke-UnityTests` ahora espera a que termine la recarga (si no, la tanda corre solo una parte).
- Tests: 188 EditMode + 20 PlayMode, incluida una partida completa arrastrando el dial con eventos de puntero.

### 2026-10-25 · V035–V040 · Preguntas por puntos
- Dominio, datos (1151 preguntas, victorias por nombre), 4 pantallas (rejilla 2×2 con los 4 colores y 800 ms de respuesta) y registro. `Sized` ya no deja flexible un eje con tamaño fijo.

### 2026-10-25 · V026–V034 · El Impostor completo
- Datos (palabras con seed de 2060, historial), estadísticas puras, 7 pantallas y registro en el menú. Detalle en `VERSIONS.md`.
- **Arreglo de arquitectura en la BD:** `IDbSchema.OnCreate` se llama en cada arranque y tiene que ser idempotente (`IF NOT EXISTS`). Así un juego añadido más tarde crea sus tablas en instalaciones existentes sin subir la versión. Se descubrió en Play: la BD de antes del Impostor no tenía sus tablas.
- Kit: `VirtualList` (recicla filas en `LateUpdate`: hacerlo dentro del layout de uGUI da «inside a rebuild loop»), `FadeScaleIn`, diálogos con validación (`DialogAction` con `Func<bool>`), `Ui.Flex` flexible solo en el eje del padre (si no, la fila se estira en vertical) y la AppBar equilibra como mucho dos botones.
- Mejora sobre el Flutter: el borrador del setup se conserva al ir a palabras o historial.
- Tests: 135 EditMode + 15 PlayMode en verde. Capturas V029–V032.

### 2026-10-25 · V011–V023 · Núcleo, menú y "Es un 10 pero"
- **BD (V011):** `IDb` sobre sqlite-net; `AppDatabase.Open` crea o migra con `PRAGMA user_version` y siembra cada juego si su tabla está vacía. Funciona en el Editor (plugin nativo de Windows).
- **Localización (V012):** `Loc.T` con ICU; importador `Gamespot → i18n → Importar ARB del Flutter` (solo añade claves que faltan); 331 claves es/en (26 nuevas de Unity) y regla 4 del Tabú corregida.
- **Kit neón (V013):** todo por código sobre uGUI + TMP; sprites redondeados, bordes y halos generados en tiempo de ejecución; halo de texto con el *underlay* del shader móvil de TMP. Recursos esenciales de TMP importados por código.
  - Gotcha de TMP: en negrita no encuentra «…» si la fuente no tiene variante bold real y avisa en consola; `Typography.SetEllipsis` usa corte simple en ese caso hasta tener una fuente con negrita (V014).
- **Navegación, audio y compartidos (V015–V017):** `Router` con pila, Escape = atrás de Android, diálogos que se cierran primero; `AudioService` + `MuteButton`; `IGameDescriptor`, reglas, abandonar, lista de jugadores, ruta no encontrada, selector de idioma.
- **Menú (V018–V019):** solo conoce los descriptores; tests de arquitectura sobre los asmdef (menú sin juegos, dominios puros, juegos aislados).
- **Es un 10 pero (V020–V023):** primer juego jugable. Palos dibujados por código (`SuitSprites`) en vez de glifos o emojis.
- **Tests:** 118 EditMode + 10 PlayMode en verde. Capturas en `Docs/Capturas/` (V018, V021, V022, V023).
- Skills usadas: `unity:unity-cli`, `unity:new-unity-project` (y la guía de `unity:ui-ugui` para el kit).

### 2026-10-25 · V007–V010 (+ dominios de V020, V024, V025, V028) · Base del proyecto y dominios puros
- Assemblies de `CONTEXT.md` §4; `Gamespot.Core` y los dominios con `noEngineReferences`.
- Player Settings por código (`Gamespot → Configuración → Aplicar todo`) con los mismos ids que el Flutter (Android `com.sajitarios.sajitarios_gamespot`, iOS `com.sajitarios.sajitariosGamespot`).
- Escena `App.unity` por código (`Gamespot → Construir → Escena App`): cámara URP con post-procesado, volumen global con Bloom (`Settings/GamespotVolume.asset`), EventSystem del Input System y `AppRoot` con `Bootstrap`. La UI (Canvas, fondo neón, capas) la crea el código.
- Core puro: random inyectable (Fisher-Yates del Flutter), normalización de nombres, lector JSON y formateador ICU.
- Dominio y *flow controllers* puros de Es un 10 pero y del Impostor (reglas 10/10/80 con un solo `NextDouble()`, votación por rondas).
- **Tests:** 101 EditMode y 1 PlayMode en verde dentro de Unity. Captura `Docs/Capturas/V009_fondo.png`.
- **Incidente:** tras `unity open`, el Editor se colgó en una recarga de dominio esperando al auxiliar `unity.exe --internal-identity-serve` (IPC de Hub). Al cerrar ese auxiliar se recuperó solo. Mientras, el código puro se comprobó con el SDK de .NET (netstandard2.1, C# 9) y el NUnit de Unity. Lección en `CLAUDE.md` §3.
- La mezcla de transparencias en espacio lineal hace los alfas del Flutter más brillantes: la rejilla del fondo baja del 5 % al 1,8 %.

### 2026-10-25 · V005–V006 · Proyecto Unity y paquetes
- Skills: `unity:new-unity-project` y `unity:unity-cli`.
- `unity projects create SajitariosGamespot --editor-version 6000.3.27f1 --template com.unity.template.universal-2d --with-pipeline --no-cloud`. Ya trae *Force Text* y *Visible Meta Files*. Abierto con `unity open`; la CLI conecta (`ready`).
- sqlite-net 1.3.3 instalado con `U package add --identifier <git>#1.3.3 --confirm true --wait true`.
- Aviso esperado en consola: el buscador del Package Manager falla sin credenciales («lacks valid authentication credentials») porque la sesión de Unity está caducada; no afecta a la compilación.

### 2026-10-25 · V003–V004 · Git, herramientas y memoria
- `.gitignore` (Unity anclado a `SajitariosGamespot/` + Flutter anclado a `Legacy/Flutter/`) y `.gitattributes` (plantilla Unity de AZÚCAR: LFS para binarios, YAML de Unity con `unityyamlmerge`; `Legacy/**` fuera de LFS).
- `tools/UnityCli.ps1`, `tools/Update-Tree.ps1` y `tools/Capture-GameView.ps1` adaptados de AZÚCAR (proyecto `SajitariosGamespot`, tests `Gamespot.Tests.*`, capturas en `Docs/Capturas/`). `TREE.md` generado.
- Memoria persistente de Claude con la forma de trabajar, la migración, la CLI de Unity y el perfil del autor.

### 2026-10-25 · V001–V002 · Forma de trabajo y Flutter a Legacy
- Análisis completo del repo (7 juegos, núcleo, tests) con tres exploradores; especificación volcada en §5–§6.
- Rama `feature/unity-port`. Todo el Flutter (código, tests, plataformas, assets, skills de Flutter, docs antiguos) movido con `git mv` a `Legacy/Flutter/`.
- Nuevos `CLAUDE.md`, `CONTEXT.md`, `LEARNINGS.md`, `VERSIONS.md`, `README.md`. Quitado el hook `dart format` de `.claude/settings.json`.
