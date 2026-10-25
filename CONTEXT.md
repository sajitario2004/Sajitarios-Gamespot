# CONTEXT.md — Sajitarios Gamespot

> **Estado (2026-10-25):** migración de Flutter a Unity en curso, rama `feature/unity-port`. Hechas las fases 0–10:
> base, núcleo, menú y **los 7 juegos portados y jugables** (Wavelength reescrito y arreglado), más el plan de
> mejoras y 5 juegos nuevos propuestos en `Docs/PLAN_MEJORAS.md` (V059a/V059b). Versión en curso: V060 (fase 11,
> pulido); lo que el autor elija del plan entra como versiones nuevas.
> **Fuentes (V014, con permiso del autor):** Roboto con negrita real y Material Icons, las de la app Flutter: los
> iconos ya son glifos de verdad.
> **Pendiente de decisión del autor:** qué mejoras y cuáles de los 5 juegos nuevos se hacen (V059a/V059b).
> **Hechas las mejoras elegidas del plan (fase 10c):** C2, C3+L1, B1+B2, C4, I1, T1, W2 (estado en `Docs/PLAN_MEJORAS.md`).
> **Hecho (V059h–V059j):** Impostor (2525) y Tabú (1161) por categorías elegibles en el setup, con Cachondeo +18 desmarcada por defecto.
> **Hecho (V059m):** categorías propias en el Impostor y el Tabú, palabras y tarjetas propias en la categoría que se elija y gestión de tarjetas del Tabú.
> **Pendiente de probar a mano (V059m):** crear una categoría, meter palabras o tarjetas en ella desde el móvil (con el teclado) y jugar solo con ella.
> **Pendiente del autor:** aclarar 7 palabras de Cachondeo (Pereira, Fabiana Sevillano, «dedear panchas», «padalustro», «amigo kebap»,
> «magic» y «virgen»; ver V059j en §10). Las 221 complicadas quedan fuera (V059k).
> **Ojo al probar en el móvil (V059l):** la captura de Wavelength roto que mandó el autor era de la **app Flutter antigua**,
> no de la de Unity. Las dos usan el mismo id (`com.sajitarios.sajitarios_gamespot`): si al instalar el APK el móvil dice
> que hay un conflicto con otra app, hay que desinstalar la antigua antes (sus palabras propias no pasan a la nueva).
> **Icono:** el de la app Flutter (V060). **APK de prueba:** `SajitariosGamespot-prueba.apk` en la raíz del repo (V065, en curso): instalarlo en el móvil y jugar.
> **Pendiente de probar a mano por el autor:** abrir `SajitariosGamespot/` en Unity 6000.3.27f1, Play en la escena
> `App` y jugar una partida de cada juego (sobre todo Wavelength, que estaba roto, y la mecha de La Bomba).
> **Sonidos:** el autor los oía demasiado fuertes: nivelados en V059n y a la mitad en V059o (comprobar en el móvil; si alguno sigue alto o bajo, se toca ese).
> **Escuchar los sonidos nuevos** (sintetizados, nadie los ha oído aún): tic-tac y explosión de La Bomba, pitidos
> y bocina del Tabú, acierto/fallo de la Trivia, campanas de Wavelength y pasar de Yo Nunca; la vibración solo se
> nota en el móvil.

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
| 2026-10-25 | Categorías propias (V059m): **una tabla por juego** (`impostor_categories`, `tabu_categories`) con el código común en `Games/_Shared`; ids `custom_N`; los nombres no se repiten (sin distinguir mayúsculas) ni con los de serie | Claude, revisable |
| 2026-10-25 | Borrar una categoría propia **no borra sus palabras**: pasan a «Mis palabras» | Claude, revisable |
| 2026-10-25 | Las palabras y tarjetas propias pueden ir a cualquier categoría: «Mis palabras», una propia o una de serie (también Cachondeo) | Claude, revisable |
| 2026-10-25 | El editor de tarjetas del Tabú es una **pantalla con scroll**, no un diálogo (seis campos y el teclado del móvil) | Claude, revisable |
| 2026-10-25 | «Reducir animaciones» sigue al ajuste «Quitar animaciones» de Android mientras no se elija en la app; en iOS, apagado (haría falta un plugin nativo) | Claude, revisable |

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
- **Menú:** AppBar con el título de la app en NeonText con PulseGlow (0.8–1.3) y el selector de idioma. Cuerpo: NeonBackground › SafeArea › cabecera `menu_header.png` (alto máx. 140, cover, oculta a lectores) + rejilla. Rejilla de 3 columnas si el ancho > 600, si no 2; ancho máx. 900, padding 16, separación 16. Tarjeta: acento cian (índice par) o magenta (impar), radio 20, borde 1.5 px, icono con glow, título (máx. 2 líneas) y descripción (máx. 3 líneas, color atenuado). En Unity (V059l, petición del autor): descripciones cortas (≤ 47 caracteres), letra de 11 que encoge hasta 9, título en un hueco fijo de dos líneas que encoge hasta 12 e icono de 44. Lector de pantalla: "Jugar a {titulo}. {descripcion}". Catálogo vacío: panel con icono, `menuVacioTitulo` y `menuVacioMensaje`. En Unity (V062) la barra del menú tiene también **Ajustes** (sonido, vibración y reducir animaciones).

### 5.2 Base de datos
- Archivo `sajitarios_gamespot.db` (en Unity, en `Application.persistentDataPath`). Flutter: versión 8, `PRAGMA foreign_keys = ON` (ninguna tabla tiene claves foráneas).
- **Versiones en Unity** (`AppDatabase.CurrentVersion`): 1 = esquema final del Flutter; **2 = el Tabú renueva su seed** (`TabuSchema.Upgrade` borra las filas `is_seed = 1` y el arranque siembra el banco nuevo; el seed del Tabú se carga si no hay filas de seed, no si la tabla está vacía); **3 = palabras de fiesta y de España**: el Tabú vuelve a renovar su seed y el Impostor añade las que falten; **4 = categorías y fuera las difíciles**: columna `category` (Impostor, por defecto `'propias'`) y `categoria` (Tabú), que `OnCreate` añade con `AppDatabase.AddColumnIfMissing` a las tablas antiguas, y los dos renuevan su seed: el Impostor borra sus filas `is_seed = 1` y carga el seed actual (`ImpostorSchema.RefreshSeed`, `INSERT OR IGNORE` sobre `UNIQUE COLLATE NOCASE`: las palabras propias, también las que se llaman igual que una del seed, se respetan). `ImpostorGame` recuerda en `OnUpgrade` que toca renovar y lo hace en `SeedIfEmpty`, porque `OnUpgrade` no recibe los seeds; **5 = Cachondeo y vuelven las fáciles**: los dos renuevan su seed otra vez; **6 = la trivia renueva su seed** (`TriviaSchema.Upgrade` borra las preguntas de seed; las victorias se quedan): los exponentes y subíndices que la fuente no tiene van como `<sup>`/`<sub>` de TextMeshPro.
- `onCreate` llama a `OnCreateTables` de cada descriptor en orden de registro; `onUpgrade`, a `OnUpgradeTables` de cada uno, que aplica solo sus pasos.
- **Seeds:** si `COUNT(*)` de la tabla > 0 no hace nada; si no, lee el JSON, valida (entrada inválida → error), recorta textos, inserta en lote con `is_seed = 1` e ignorando conflictos, y devuelve cuántas filas entraron.

| Tabla | Columnas | Índice |
|---|---|---|
| `impostor_words` | `id` PK AI, `word` TEXT NN **UNIQUE COLLATE NOCASE**, `hint` TEXT NN, `is_seed` INT NN DEF 0, `created_at` INT NN (epoch ms) | `idx_impostor_words_word(word)` |
| `impostor_categories` / `tabu_categories` (V059m) | `id` PK AI, `name` TEXT NN **UNIQUE COLLATE NOCASE**, `created_at` INT NN | — |
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
| `impostor_words.json` | `[{word, hint, category}]` | **2525** (1793 clásicas de las 2060 heredadas + 650 de fiesta y de España + 82 de Cachondeo; V059h–V059j) | ≥ 2100, ≥ 20 por categoría, sin duplicados sin distinguir mayúsculas, `word ≠ hint` |
| `trivia_questions.json` | `[{tematica, difficulty, enunciado, opciones[4], correcta}]` | 1151 (fácil 471 / difícil 462 / muy difícil 218) | ≥ 1000 |
| `wavelength_spectra.json` | `[{izquierda, derecha}]` | 80 | ≥ 70 |
| `tabu_words.json` | `[{palabra, prohibidas[], categoria}]` | **1161** (5 prohibidas cada una; 605 clásicas, 467 de fiesta y de España y 89 de Cachondeo; V059e–V059j) | ≥ 1000, 5 prohibidas distintas que no repiten la palabra |
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
- En el Flutter ningún otro juego tenía sonido. Mute guardado en la preferencia `audio_enabled` (por defecto activado); errores silenciados. Botón: icono volumen on/off con tooltip `silenciarSonido` / `activarSonido`. Los WAV son placeholders sintetizados (16 bit, mono, 44.1 kHz).
- **Sonidos nuevos en Unity (V059d, C3 + L1)**, sintetizados por código con el menú *Gamespot → Audio → Generar y nivelar los sonidos* (`Editor/Audio/SoundSynth.cs`, semilla fija; se pueden sustituir por sonidos finales con el mismo nombre):

| Sonido | Archivo | Cuándo |
|---|---|---|
| `Tick` / `Tock` | `tick.wav`, `tock.wav` (0.05 s) | La Bomba: alternos mientras corre la mecha, cada 0,9 s al principio y cada 0,18 s al final (lineal con lo consumido) |
| `Explosion` | `explosion.wav` (1.4 s) | La Bomba: al explotar, con **vibración** |
| `Beep` | `beep.wav` (0.15 s) | Tabú: en cada uno de los últimos 5 segundos |
| `Buzzer` | `buzzer.wav` (0.8 s) | Tabú: se acaba el turno |
| `Correct` / `Wrong` | `correct.wav`, `wrong.wav` | Trivia: al elegir respuesta |
| `Chime` | `chime.wav` (1.1 s) | Wavelength: al confirmar la adivinanza (revelado) |
| `Swipe` | `swipe.wav` (0.3 s) | Yo Nunca: siguiente frase |
- **Volumen (V059n):** el autor los oía «exageradamente fuertes». Todos los WAV (los 9 sintetizados y los 3 heredados) se dejan a **−26 dB de sonoridad** con el pico como mucho a **−12 dB** (V059o: «bájalos a la mitad»; en V059n eran −20 y −6) (`Editor/Audio/SoundLevel`: RMS de las ventanas de 10 ms que suenan, después de un paso alto de 200 Hz para no contar los graves que el altavoz del móvil no da). Menú *Gamespot → Audio → Generar y nivelar los sonidos*; nivelar dos veces da lo mismo. Las llamadas siguen bajando algunos un poco más (pitido 0,8, campanas 0,8, pasar 0,6). Los tests de PlayMode no se oyen (`AudioListener.volume = 0` en `AppTestHarness`). `AudioClipsTests` comprueba los niveles.
- **Vibración:** `Haptics.Vibrate()` (`Handheld.Vibrate` solo en Android/iOS; Unity añade el permiso `VIBRATE` al usarlo). Preferencia `vibration_enabled` (por defecto sí), independiente del silencio y aún sin interruptor (llegará con C5).

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
- **Jugadores guardados (Unity, C2):** al empezar una partida de Impostor, Trivia, Wavelength o La Bomba se guardan los nombres normalizados en la preferencia `last_players` (lista JSON). `PlayerListEditor` arranca con ellos si llega sin ningún nombre (recortados al máximo del juego) y muestra "Usar los de la última partida" cuando los de pantalla no coinciden. Tabú (equipos) y Yo Nunca no tienen lista de jugadores.

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
  - **Setup:** 3 filas de nombre al empezar (avatar numerado cian, campo, quitar deshabilitado con 3), "Añadir jugador" hasta 15 (luego "Máximo de jugadores alcanzado"), sin reordenar. Contador de impostores 1..min(5, n−1) (se recorta al quitar jugadores). Contador de rondas que en el Flutter empezaba en 1 y solo se recortaba; en Unity (I1) empieza en el máximo y lo sigue al añadir o quitar jugadores hasta que se toca, y después solo se recorta. Interruptor de pista (apagado). Errores en snackbar. "Empezar" con spinner mientras inicia. Sin palabras → diálogo con "Ahora no" / "Gestionar palabras". AppBar: casa, reglas, historial, palabras.
  - **Pasar el móvil:** "Jugador X de Y", círculo cian con un móvil (72 px, 96 si ancho > 600), "Pásale el móvil a", nombre grande, ayuda, "Continuar" (64 px).
  - **Revelar:** "Es el turno de {nombre}", rol oculto hasta pulsar "Revelar" (glow magenta, suena `Reveal`); animación 350 ms (fade + escala 0.92 → 1). Impostor: panel magenta con pulso 0.85–1.4, "IMPOSTOR" y, con pista, "Pista" + la pista con glow violeta. Palabra: panel cian con pulso 0.8–1.2, "Tu palabra es" + la palabra. Botón "Ocultar y pasar" ("Ocultar y ver resultados" en el último).
  - **Votación:** "Ronda X de Y" (magenta), instrucción, tras cada voto "El impostor sigue entre vosotros"; candidatos en tarjetas cian; impostores pillados tachados en gris; confirmar "¿Expulsar a {nombre}?".
  - **Fin:** suena `GameOver` y se guarda el historial (una vez). **Nunca muestra roles ni la palabra.** "¡Habéis ganado!" (trofeo cian) o "El impostor sigue entre vosotros" (máscara magenta). Botones "Jugar otra" y "Volver al menú". Las partidas abandonadas no se guardan.
  - **Gestión de palabras:** búsqueda con *debounce* de **280 ms**; seed con borde violeta, chip "Predefinida" con candado y sin editar/borrar; propias con borde cian; formulario con palabra y pista obligatorias; confirmación al borrar; snackbars de resultado; estados vacío/sin coincidencias/error. Lista virtualizada (2060 filas). En Unity (V059m): cada fila enseña su categoría; filtro por categoría junto a la búsqueda (con «Todas»); el formulario elige la categoría en otro diálogo, con «Nueva categoría» para crearla sin salir (una nueva va a la del filtro si se puede elegir, si no a «Mis palabras»); la barra abre «Mis categorías» (`CustomCategoriesScreen`: crear, renombrar, borrar, con cuántas palabras tiene cada una).
  - **Historial:** estadísticas calculadas en la pantalla: total de partidas, palabra más repetida (agrupada en minúsculas, desempate por la menor, se muestra la grafía más reciente) y ranking de impostores (nombre exacto, más veces primero). Cada partida, desplegable: palabra, fecha corta + HH:mm, nº de jugadores e impostores, pista activa o no y el rol de cada jugador (el historial sí revela roles). "Borrar todo" con confirmación.

### 6.3 Preguntas por puntos (`trivia`)
- **Dominio:** `Difficulty {Facil, Dificil, MuyDificil}` (se guarda el nombre del enum; OpenTDB easy/medium/hard). Temáticas: cultura_general, videojuegos, cocina, cine, ciencia, geografia, historia, deportes, musica. `Question` con 4 opciones y `CorrectIndex` en [0,4). `TriviaConfig.Create(nombres, temáticas)`: 2–6 jugadores, nombres no vacíos ni duplicados, al menos una temática; guarda los nombres recortados.
  - `TriviaSession`: **9 rondas**, 3 por tramo: rondas 0–2 fácil, 3–5 difícil, 6–8 muy difícil. Fallar elimina. Termina a las 9 rondas o si no queda nadie; ganadores = los vivos al terminar (empate de supervivientes; si caen todos, nadie gana).
  - `DealQuestionsUseCase(vivos, pool)`: baraja una copia del pool (Fisher-Yates) y da `shuffled[i]` al jugador `i` (pregunta distinta para cada uno). El resultado es una **lista ordenada**, no un diccionario.
- **Datos:** `QuestionRepository.GetPool(temáticas, dificultad)` ordenado por id. `WinnerRepository.IncrementWins(name)` (insertar o ignorar + sumar 1, sin distinguir mayúsculas), `GetWins`, `GetAllRanked` (más victorias primero, desempate por nombre).
- **Flow:** fases `Setup, Iniciando, Pass, Question, GameOver, Error(sinPreguntas)`. `Iniciar`: si algún pool de las 3 dificultades tiene menos preguntas que jugadores → error. Reparte la ronda 0. `PasarDispositivo` → `Question`. `Responder(i)`: registra y pasa al siguiente jugador vivo de la ronda; al cerrar la ronda, avanza, recarga los pools y reparte la dificultad siguiente. Al terminar, +1 victoria a cada superviviente. En el Flutter las preguntas podían repetirse entre rondas; en Unity (T1) se reparte con las que no han salido en la partida y, si no quedan para todos los vivos, con el pool completo.
- **Pantallas:** setup con 2 filas de nombre (máx. 6, avatares violeta), chips de temáticas (por defecto cultura general e historia; la última marcada no se puede desmarcar), ranking de victorias y atribución "Preguntas basadas en Open Trivia DB (CC BY-SA 4.0)". Pasar el móvil en violeta. Pregunta: "Ronda X de 9", nombre en violeta, enunciado en panel **violeta**, rejilla 2×2 con marcos **azul / verde / rojo / amarillo** y letras A–D; al pulsar, la correcta en verde, la elegida errónea en rojo, el resto atenuadas (250 ms), y a los **800 ms** avanza. Fin: "¡Habéis ganado!" + nombres, o "Nadie ha ganado". Sin sonido.

### 6.4 Wavelength (`wavelength`)
- **Dominio:** `Spectrum(left, right)`. `WavelengthConfig.Create(nombres, rondas = 8)`: 2–8 jugadores, rondas 1–20. Puntos (en `double`, bordes inclusivos): d = |respuesta − objetivo|; d ≤ 0.06 → 4, ≤ 0.12 → 3, ≤ 0.20 → 2, si no 0; la respuesta se recorta a [0,1]. `PickRoundUseCase`: `Pick(pool)` y objetivo = `NextDouble()`. En el Flutter podía repetirse espectro; en Unity (W2) el flujo le pasa solo los que no han salido y, si ya salieron todos, todos. `WavelengthSession`: cooperativo con un solo marcador; psíquico = `players[ronda % n]`.
- **Flow:** `Setup, Iniciando, Clue, Pass, Guess, Reveal, GameOver, Error(sinEspectros)`. `ConfirmarPista(texto)` (no vacío) → `Pass` → `Guess`; `SubmitGuess(pos)` → `Reveal` (aún no suma); `Next()` suma la ronda y va a `GameOver` o a una ronda nueva.
- **Pantallas:** setup (2–8 nombres, rondas ±1 en [1,20]). Pista: "Ronda X de Y", "Psíquico: nombre", instrucciones, fila de conceptos, dial en modo pista (ve el objetivo), campo de pista. Pasar el móvil. Adivinar: pista del psíquico + dial arrastrable (la aguja empieza en 0.5). Revelado: dial con bandas y objetivo apareciendo en **0.7 s**, texto de banda ("En el blanco" / "Cerca" / "Lejos" / "Fallo"; cian para 4 y 3, magenta para 2 y 0), "{n} puntos esta ronda" y "Total: {n} puntos" (**el total no incluye la ronda actual**, fijado por test). Fin: puntuación final y media por ronda.
- **Dial:** radio = min(0.44·ancho, 0.82·alto), centro en (ancho/2, 0.78·alto desde arriba); posición p ∈ [0,1] → ángulo θ = π + p·π (p = 0 izquierda, 0.5 arriba, 1 derecha). Arco cian (glow 0.055r + núcleo 0.025r), 5 marcas en p = 0, .25, .5, .75, 1. Bandas como cuñas cian superpuestas: lejos ±0.20 (alfa 0.20), cerca ±0.12 (0.30), blanco ±0.06 (0.45). Marcador del objetivo de 0.15r a 0.98r. Aguja magenta del centro a 0.92r con pomo de 0.045r. Entrada solo en modo adivinar: p = 1 − clamp(atan2(−(y−cy), x−cx), 0, π)/π. Alto del dial en pista y adivinar: clamp(0.42·alto disponible, 180, 320).

### 6.5 Tabú (`tabu`)
- **Dominio:** `TabuWord(palabra, prohibidas[4..5])`. `TabuConfig.Create(equipoA, equipoB, turnoSegundos = 60, objetivoVictorias = 3)`: errores en orden equipo A vacío, equipo B vacío, duplicados, turno fuera de [30,120], objetivo ≤ 0.
  - `TabuSession`: empieza el equipo A. Acierto +1; Saltar sin límite (no se muestra); Falta −1 sin bajar de 0 y **sin cortar el turno**. `TerminarTurno`: si los aciertos > 0, **+1 victoria de ronda** para el equipo; cambia de equipo y reinicia contadores. Gana el primero en llegar al objetivo. Palabras sin repetir en la partida (`PickTabuWordUseCase`). En el Flutter, si se agotaban se quedaba la actual; en Unity (V059e) vuelven a salir todas menos la actual y el turno avisa una vez (`tabuPalabrasAgotadas`).
- **Pantallas:** setup (nombres de los dos equipos, chips 30/60/90 s, 60 por defecto; objetivo siempre 3). Turno: palabra en MAYÚSCULAS, lista de prohibidas, equipo en violeta, contador de aciertos en la AppBar, cuenta atrás (cian > 10, amarillo 6–10, rojo ≤ 5) y botones Acierto (verde), Saltar (cian), Falta (rojo). El temporizador vive en la pantalla (tic de 1 s; al llegar a 0 termina el turno). Marcador: "Objetivo: 3 victorias de ronda", "A: x / 3" (cian) y "B: y / 3" (violeta), "Siguiente turno". Fin: equipo ganador, marcadores y "Volver al menú" (en el Flutter no había "jugar otra"; en Unity sí, ver §8.1). Sin sonido en el Flutter.
- **Tarjetas propias (V059m, nuevo en Unity):** el setup abre la gestión de tarjetas (`/tabu/words`): búsqueda, filtro por categoría, las de serie con candado y las propias editables. El editor (`TabuCardEditScreen`, apilado) pide la palabra, 5 prohibidas (la quinta opcional) y la categoría. Reglas (`TabuCardRules`, dominio): palabra obligatoria, 4–5 prohibidas, ninguna repetida ni igual a la palabra (sin distinguir mayúsculas); el repositorio, además, no deja repetir la palabra de otra tarjeta. Categorías propias como en el Impostor; el setup las enseña con «Mis palabras».

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
- **"Jugar otra" (C4, V059f):** en Tabú, Trivia, Wavelength y La Bomba empieza al momento otra partida con la misma configuración (equipos y tiempo; jugadores y temáticas; jugadores y rondas; jugadores y modo). En el Flutter, Trivia y Wavelength volvían al setup y Tabú y La Bomba no tenían el botón. "Cambiar ajustes" vuelve al setup con el borrador. El Impostor sigue como en el Flutter (vuelve al setup).
- Icono, splash, dorso y cabecera en morado/ámbar → placeholders con la paleta neón; el icono, al final, es el de la app Flutter (V060, lo pidió el autor).

### 8.3 Lista de paridad pantalla a pantalla (V064)
Las 29 pantallas del Flutter (`Legacy/Flutter/lib/**/*_screen.dart`) y su pantalla de ruta desconocida tienen su
equivalente en Unity. **Textos:** un script compara las claves de localización que usa cada pantalla del Flutter con
las que usa el código Unity: están todas, salvo los «Iniciando…» de los cinco setups (en Unity el arranque es
instantáneo, no hay espera que enseñar) y tres etiquetas solo para el lector de pantalla (dial de Wavelength y roles
del historial; ver §8.2). «Igual» = mismo contenido, reglas y flujo (especificación en §6); las diferencias son las
de §8.1 y las mejoras pedidas por el autor (fases 10c–10e).

| Flutter | Unity | Estado | Diferencias | Lo cubre |
|---|---|---|---|---|
| `menu_screen` | `MenuScreen` | Igual + mejoras | Silencio y Ajustes en la barra (V062); descripciones cortas (V059l); iconos de verdad (V014) | `BootstrapTests` |
| `rules_screen` | `RulesScreen` | Igual | — | `BootstrapTests` |
| `_RouteErrorScreen` | `NotFoundScreen` | Igual | — | `BootstrapTests` |
| `es_un_10_pero_screen` | `EsUn10PeroScreen` | Igual | Palos dibujados por código | `EsUn10PeroScreenTests` |
| `impostor/setup_screen` | `ImpostorSetupScreen` | Igual + mejoras | Rondas empiezan en el máximo (I1); últimos jugadores (C2); categorías, también propias (V059i, V059m) | `ImpostorFlowTests`, `CategoryTests`, `OwnCategoriesTests`, `PlayerRosterTests` |
| `impostor/pass_device_screen` | `PassDeviceScreen` | Igual | — | `ImpostorFlowTests` |
| `impostor/reveal_screen` | `RevealScreen` | Igual | Palabras largas en dos líneas que encogen | `ImpostorFlowTests` |
| `impostor/voting_screen` | `VotingScreen` | Igual | Atrás del sistema interceptado (§8.1) | `ImpostorFlowTests` |
| `impostor/game_over_screen` | `GameOverScreen` | Igual | Atrás interceptado; nunca revela roles (como el Flutter) | `ImpostorFlowTests` |
| `impostor/words_management_screen` | `WordsScreen` | Igual + mejoras | Categoría en cada fila, filtro y «Mis categorías» (V059m) | `ImpostorFlowTests`, `OwnCategoriesTests` |
| `impostor/history_screen` | `HistoryScreen` | Igual | — | `ImpostorFlowTests` (guardado), barrido de textos |
| `trivia/trivia_setup_screen` | `TriviaSetupScreen` | Igual + mejoras | Últimos jugadores (C2); chips de 48 | `TriviaFlowTests` |
| `trivia/trivia_pass_device_screen` | `TriviaPassScreen` | Igual | — | `TriviaFlowTests` |
| `trivia/trivia_question_screen` | `TriviaQuestionScreen` | Igual + mejoras | Preguntas sin repetir (T1); sonido de acierto y fallo (C3); respuestas largas que encogen; exponentes con `<sup>` (V059l) | `TriviaFlowTests`, `SoundTests` |
| `trivia/trivia_game_over_screen` | `TriviaGameOverScreen` | Igual + mejoras | «Jugar otra» al momento (C4) | `PlayAgainTests` |
| `wavelength/wavelength_setup_screen` | `WavelengthSetupScreen` | Igual | — | `WavelengthFlowTests` |
| `wavelength/wavelength_clue_screen` | `WavelengthClueScreen` | Arreglada | El dial del Flutter estaba roto (reescrito); conceptos solo en la fila (§8.1) | `WavelengthFlowTests` |
| `wavelength/wavelength_pass_device_screen` | `WavelengthPassScreen` | Igual | — | `WavelengthFlowTests` |
| `wavelength/wavelength_guess_screen` | `WavelengthGuessScreen` | Arreglada | Arrastre real; bajo la base va al polo más cercano (§8.1) | `WavelengthFlowTests` |
| `wavelength/wavelength_reveal_screen` | `WavelengthRevealScreen` | Arreglada | La aguja es la respuesta del grupo (en el Flutter, 0,5); «Ronda X de Y» en la barra; campanas (C3); espectros sin repetir (W2) | `WavelengthFlowTests` |
| `wavelength/wavelength_game_over_screen` | `WavelengthGameOverScreen` | Igual + mejoras | Media localizada; «Jugar otra» (C4) | `PlayAgainTests` |
| `tabu/tabu_setup_screen` | `TabuSetupScreen` | Igual + mejoras | Categorías y tarjetas propias (V059i, V059m) | `TabuFlowTests`, `CategoryTests`, `OwnCategoriesTests` |
| `tabu/tabu_turn_screen` | `TabuTurnScreen` | Igual + mejoras | Tiempo en pausa con el diálogo (§8.1); pitidos y bocina (C3); palabras agotadas avisan y vuelven (V059e) | `TabuFlowTests`, `SoundTests` |
| `tabu/tabu_scoreboard_screen` | `TabuScoreboardScreen` | Igual | Nombres largos en dos líneas | `TabuFlowTests` |
| `tabu/tabu_game_over_screen` | `TabuGameOverScreen` | Igual + mejoras | «Jugar otra» y «Cambiar ajustes» (C4) | `PlayAgainTests` |
| `yo_nunca/yo_nunca_setup_screen` | `YoNuncaSetupScreen` | Igual | Aviso +18 con picante | `YoNuncaFlowTests` |
| `yo_nunca/yo_nunca_play_screen` | `YoNuncaPlayScreen` | Igual + mejoras | Atrás confirma (§8.1); frase centrada sin scroll; sonido al pasar (C3) | `YoNuncaFlowTests` |
| `bomba/bomba_setup_screen` | `BombaSetupScreen` | Igual | Nombres normalizados; atrás sin diálogo (§8.1) | `BombaFlowTests` |
| `bomba/bomba_play_screen` | `BombaPlayScreen` | Igual + mejoras | Mecha en pausa con el diálogo (§8.1); tic-tac, explosión y vibración (C3, L1) | `BombaFlowTests`, `SoundTests` |
| `bomba/bomba_game_over_screen` | `BombaGameOverScreen` | Igual + mejoras | «Jugar otra» (C4) | `BombaFlowTests`, `PlayAgainTests` |

**Solo en Unity:** `CustomCategoriesScreen` (Mis categorías), `TabuWordsScreen` y `TabuCardEditScreen` (tarjetas propias,
V059m) y el diálogo de Ajustes (V062). **Comunes a todas:** zona segura y notch (V061), pulsables de 48 y contraste
(V062), sin textos cortados en 4 tamaños y 2 idiomas (V059l). Lo que queda para la validación del autor es jugarlo en
el móvil (V066, V068).

### 8.2 Deuda que se conserva (decide el autor)
- El objetivo de victorias del Tabú no se puede cambiar.
- La mecha de La Bomba no se puede configurar (siempre 10–60 s).
- Contenido de los bancos generado por máquina: conviene una revisión factual.
- Sin lector de pantalla (TalkBack/VoiceOver): uGUI no expone la jerarquía a la accesibilidad del sistema; haría falta el módulo de accesibilidad de Unity 6 (`AccessibilityHierarchy`) pantalla a pantalla. En el Flutter había `Semantics` (V062).

## 9. Registro de licencias
| Componente | Licencia | Uso |
|---|---|---|
| Open Trivia DB (preguntas base de la trivia) | CC BY-SA 4.0 | Atribución en el setup de la trivia |
| `gilzoide/unity-sqlite-net` 1.3.3 (SQLite-net 1.9 + SQLite3 Multiple Ciphers) | MIT | Persistencia SQLite. Revisado el 2026-10-25: 299 estrellas, activo (último push 2026-10-14), no archivado |
| Paquetes de Unity (URP, uGUI/TextMeshPro, Input System, Test Framework, Pipeline) | Unity Companion License | Motor, UI, entrada, tests y puente de la CLI |
| Roboto 3.005 Regular y Bold (`google/fonts`, `apache/roboto/static` en el commit `ff11ed915971`) | Apache-2.0 (`Fonts/LICENSE-Roboto.txt`) | Fuente de texto (V014, permiso del autor el 2026-10-25). Repo oficial de Google Fonts |
| Material Icons Regular (`google/material-design-icons`, `font/MaterialIcons-Regular.ttf`) | Apache-2.0 (`Fonts/LICENSE-MaterialIcons.txt`) | Iconos (V014). Repo oficial de Google, 54 000 estrellas |
| LiberationSans (recursos esenciales de TextMeshPro) | OFL 1.1 | Respaldo de los caracteres que Roboto no tenga |

## 10. Registro de trabajo (lo más reciente arriba)

### 2026-10-25 · V059o · Sonidos a la mitad
- El autor los seguía oyendo demasiado altos: «bájalos a la mitad». `SoundLevel.TargetDb` −26 (antes −20) y `PeakCeilingDb` −12 (antes −6): mitad de amplitud en los 12. La explosión bajaba 0,9 dB menos de lo previsto (al bajarla, su cola salía del umbral de «lo que suena» y la medida subía): `Normalize` repite la medida hasta acertar.

### 2026-10-25 · V065 y V067 · APK de desarrollo y proyecto de Xcode
- APK de desarrollo en `SajitariosGamespot/Builds/Android/SajitariosGamespot-dev.apk` (opciones `Development` y `DetailedBuildReport` del comando `build run`): 49 MB, sin errores ni avisos. El release de prueba sigue en la raíz.
- Proyecto de Xcode generado desde Windows (`build.target switch` a iOS, `build run --target iOS`, vuelta a Android): `SajitariosGamespot/Builds/iOS/Unity-iPhone.xcodeproj`, 57 s sin errores. Al cambiar de plataforma Unity añade al `ProjectSettings` la entrada de batching de iPhone. Compilar y firmar necesita un Mac.

### 2026-10-25 · V064 · Lista de paridad con el Flutter
- §8.3: las 29 pantallas del Flutter (y la de ruta desconocida) con su pantalla Unity, estado (igual, igual con mejoras o arreglada), diferencias y el test que la cubre; más las 4 que solo tiene Unity.
- Comprobación de textos por pantalla (script en el scratchpad: claves `l10n.*` de cada pantalla del Flutter frente a las que usa el código Unity): no falta ninguna; solo sobran los cinco «Iniciando…» (en Unity el arranque es instantáneo) y tres etiquetas de lector de pantalla (deuda en §8.2).
- `Legacy/Flutter/` se queda hasta que el autor valide la app en el móvil (decisión del autor, 2026-10-25).

### 2026-10-25 · V063 · Rendimiento en reposo
- **Medición:** los contadores del Profiler en Play dentro del Editor (`GC.Alloc`, `UGUI.Rendering.UpdateBatches`) daban miles de asignaciones por fotograma incluso con la UI apagada: son del propio Editor y no sirven. En su lugar, `PerformanceTests` comprueba con `AllocatingGCMemoryConstraint` que el trabajo de un fotograma (el `Tick` de la pantalla y los brillos) no asigna memoria en el menú, los siete setups, la lista de palabras, el turno del Tabú y la mecha de La Bomba.
- **Arreglos:** el turno del Tabú rehacía el texto del contador y su nombre accesible en cada fotograma (60 cadenas por segundo): ahora solo al cambiar el segundo. `PulseGlow` aplica el brillo por escalones de 0,02 (cada cambio rehace los lotes del Canvas: ≈ 15 por segundo en vez de 60, sin diferencia visible) y expone `Advance` para los tests. `AppSettings.ReduceMotion` se guarda en memoria (antes, una llamada a `PlayerPrefs` por brillo y fotograma); `ClearReduceMotion` vuelve a seguir al sistema.
- **Bloom ligero:** a un cuarto de resolución y 5 pasadas (antes, a la mitad y 6), sin filtrado de alta calidad. Menú *Gamespot → Configuración → Bloom*. Igual a la vista (`V063_menu_bloom_ligero.png`).
- **Reposo:** `IdleRendering` (en el `Bootstrap`): tras 3 s sin tocar la pantalla se dibuja un fotograma de cada dos (30 fps) con `OnDemandRendering`; al tocar o al cambiar de pantalla, todos. La lógica sigue a 60 (temporizadores, mecha y sonidos no cambian). Sin `Update` vacíos: todos los que hay salen enseguida si no tienen nada que hacer.

### 2026-10-25 · V062 · Accesibilidad básica
- **Objetivos táctiles:** barrido de todos los `Selectable` activos en las 88 pantallas: solo los chips bajaban de 48 (44 en las rejillas, 40 en las filas de tiempo, modo e intensidad). `NeonChip` y `ResponsiveChipGrid` usan `NeonButton.MinTouchSize` (48); las filas del selector de categorías, 56. `AccessibilityTests` lo comprueba en el menú, los siete setups y la lista de palabras.
- **Contraste:** `ContrastTests` calcula el contraste WCAG de la paleta: texto (principal, atenuado y acentos) ≥ 4,5:1 sobre fondo y superficies; etiquetas de botón (18 en negrita) ≥ 3:1, rellenas y de borde. Ya cumplía: el violeta saturado solo se usa de halo y de borde.
- **Reducir animaciones:** `AppSettings.ReduceMotion` existía (lo respetan `PulseGlow` y `FadeScaleIn`, igual que en el Flutter) pero no había dónde activarlo. Diálogo `Menu/SettingsDialog` (icono `Settings`, engranaje) con sonido, vibración (`vibration_enabled`, el interruptor que faltaba de C5) y reducir animaciones. Sin elegir, `SystemReduceMotion` lee `animator_duration_scale` de Android (0 = «Quitar animaciones»).
- 254 EditMode + 56 PlayMode; barridos de textos y pulsables limpios. Captura `V062_ajustes.png`.

### 2026-10-25 · V061 · Notch y zona segura
- La capa de pantallas respetaba `Screen.safeArea`; los diálogos y los avisos (capa de superposiciones), no, aunque el comentario de `UiRoot` lo decía: en un móvil con barra de gestos el aviso de abajo podía quedar debajo de ella. Ahora el velo del diálogo cubre toda la pantalla y su panel va en un hijo con `SafeArea`; cada aviso va en su contenedor `SnackbarArea` con `SafeArea` (y se destruye con él).
- `SafeArea.Simulated` (se reinicia al entrar en Play): zona segura de mentira para probar un notch en el Editor. `SafeAreaTests` simula un notch del 6 % arriba y una barra del 3 % abajo y comprueba pantallas, diálogo, velo y aviso. Con el notch simulado, barrido de textos limpio. 54 PlayMode. Captura `V061_notch_simulado.png`.
- El APK va a pantalla completa (`androidStartInFullscreen`) y se dibuja también bajo el notch (`androidRenderOutsideSafeArea`): el fondo neón llena la pantalla y el contenido queda dentro de la zona segura.

### 2026-10-25 · V014 · Fuentes: Roboto con negrita real y Material Icons
- Con permiso del autor («las de Flutter»): Roboto Regular y Bold y Material Icons, del GitHub oficial de Google (Apache-2.0, ≈ 650 KB, sumas SHA-256 comprobadas), en `Assets/_Gamespot/Fonts` con sus licencias. Roboto estático no está suelto en el repo actual (solo un zip de 29 MB o la variable): se tomó de `google/fonts` en el commit `ff11ed915971` (Roboto 3.005).
- `Editor/Setup/FontSetup` (menú *Fuentes*, también en *Aplicar todo*; idempotente, conserva los GUID): crea `Resources/Fonts/Roboto SDF`, `Roboto Bold SDF` y `MaterialIcons SDF` (SDFAA, 56 pt, atlas de 1024 y 512 para los iconos, con el shader móvil del halo) con todos los caracteres de los textos y los seeds (204) y los 44 iconos, y los deja **estáticos** para que el Editor no los cambie al jugar; lo que falte lo pinta LiberationSans (respaldo, con su atlas dinámico). Roboto es la fuente por defecto de TMP, con la negrita real en su tabla de pesos. `Icons` busca `Resources/Fonts/MaterialIcons SDF` y expone `AllCodepoints`.
- Con la negrita real, `Ui.Text` de una línea cortaba con «…» sin encoger (solo encogía cuando no había «…»): ahora encoge siempre hasta el 70 % antes. El nombre del equipo del marcador del Tabú va en dos líneas que encogen.
- Tests: `GlyphCoverageTests` sigue la cadena Roboto → LiberationSans y comprueba que todos los iconos salen de la fuente de iconos. Barrido de textos limpio (88 pantallas, es/en, 4 tamaños). 252 EditMode + 53 PlayMode. Capturas `V014_*`.

### 2026-10-25 · V059n · Sonidos más bajos
- El autor oía los sonidos «exageradamente fuertes» mientras corrían los tests. Medidos: los sintetizados se normalizaban a 0,85 de pico (−1,4 dB) y el pitido del Tabú y el fallo de la trivia llegaban a −6 y −9 dB de RMS; los tres heredados del Flutter (carta, revelar, fin), a −9. Ahora todos a −20 dB de sonoridad y como mucho −6 dB de pico (`SoundLevel`); bajan entre 2 dB (tic-tac, que ya era bajo) y 13 dB (pitido). Se mide sin los graves (paso alto de 200 Hz) para que la explosión, que es casi todo graves, no se quede corta en el altavoz del móvil.
- `SoundSynth` usa `SoundLevel` y nivela también los 3 heredados (menú *Generar y nivelar los sonidos*). Los tests de PlayMode ya no se oyen. Test de niveles en `AudioClipsTests`. 251 EditMode + 53 PlayMode.

### 2026-10-25 · V059m · Categorías y tarjetas propias en el Impostor y el Tabú
- **Común (`Games/_Shared`):** `CustomCategoryRepository` (tabla de cada juego, `CreateTable` desde su esquema; nombres limpios de hasta 24 caracteres, sin repetir sin distinguir mayúsculas, en C# porque NOCASE de SQLite solo iguala ASCII, ni con los nombres de serie del idioma actual); `CategoryOption` (id + nombre, «Mis palabras» si la propia ya no existe); `CategorySelectDialog` (chips en un scroll, «Todas» y «Nueva categoría» opcionales); `CategoryNameDialog`; `CustomCategoriesScreen`. `CategoryPicker` recibe opciones con nombre y un conjunto de «ya vistas»: una categoría que aparece por primera vez sale marcada (salvo las +18). `WordCategories.CustomId/IsCustom`. Iconos nuevos `Category` y `FilterList` (sustitutos «#» y «=» hasta que haya fuente de iconos). `Dialogs.Close`.
- **Impostor:** `WordRepository.Insert/Update` con categoría, `Search(texto, categoría)` y `MoveCategory`; la gestión de palabras enseña la categoría, filtra y elige categoría en el formulario. El setup enseña las propias con palabras (al final, con su nombre).
- **Tabú:** `TabuCard`, `TabuWordRepository.Search/GetById/Insert/Update/Delete/MoveCategory` con `TabuCardException` (no válida, repetida, no existe, de serie); `TabuWordsScreen` y `TabuCardEditScreen`; el setup abre las tarjetas desde la barra y enseña «Mis palabras» y las propias.
- **Arreglos que salieron al mirarlo:** un `NeonButton` creado sin texto no tiene etiqueta que cambiar luego (el botón de categoría salía vacío); los chips del selector necesitaban margen dentro del scroll; las columnas de texto de las filas usan ancho preferido 0 para no aplastar el chip «Predefinida» y las prohibidas largas van en tres líneas.
- Tests: repositorio de categorías, palabras y tarjetas con categoría, reglas de tarjeta (también en el arnés .NET), PlayMode del flujo completo (crear categoría, palabra o tarjeta en ella, sale marcada en el setup, partida solo con ella, filtro, renombrar, borrar sin perder palabras, validación del editor). Barrido de textos de las pantallas nuevas, con los diálogos abiertos y las tarjetas más largas: limpio. 250 EditMode + 53 PlayMode; arnés .NET 178. Capturas `V059m_*`.

### 2026-10-25 · V059l · Menú y textos que no caben
- **Wavelength:** la captura del móvil era de la app Flutter (textos sin tildes del ARB: «Psiquico», «movil»; icono de casa de Material; el dial descentrado es el fallo que ya se arregló en el port). En Unity, a 1080×2340, el dial sale centrado y con tildes (`V059l_wavelength_movil.png`). Hay que instalar el APK nuevo (y, si el móvil dice que hay conflicto, desinstalar antes la app vieja: mismo id).
- **Menú:** a tamaño de móvil faltaban títulos enteros (el VerticalLayout aplastaba el título hasta 0 y TextMeshPro, en modo Truncate, no pintaba nada) y las descripciones se cortaban. Ahora: descripciones cortas en los dos idiomas, letra de 11 que encoge hasta 9, título en un hueco fijo de dos líneas que encoge hasta 12, icono de 44 y menos margen (`V059l_menu_movil.png`; antes, `V059l_menu_movil_antes.png`).
- **Barrido automático de textos** (script de eval del Editor, en el scratchpad): recorre 88 pantallas de los 7 juegos (setups, reglas, partidas completas, fin, palabras, historial) y marca los textos truncados, que se salen por arriba o por los lados (ancho tipográfico, sin el relleno del halo) o que quedan fuera de la pantalla. Pasado en 720×1280, 1080×2340, 1440×3200 y 1600×2560, en español y en inglés. Encontró y se arregló: chips de categoría largos en inglés (ahora de una línea que encoge), la rejilla de respuestas de la trivia (su tamaño se calculaba en `LateUpdate`: el primer fotograma salía con celdas de 100×100; ahora también en `OnRectTransformDimensionsChange`, igual que `ResponsiveChipGrid`) y respuestas de hasta 79 caracteres (encogen hasta 9). Lo único que queda marcado es «IMPOSTOR» en el revelado, 3 px por lado en una carta con margen (no se ve: `V059l_impostor_revelado.png`).
- **Glifos:** la consola avisaba de «⁻» sin glifo: 8 preguntas de ciencia de la trivia usaban exponentes y subíndices Unicode (⁰⁴⁷⁸⁹⁻, ₂) que LiberationSans no tiene y salían como cuadraditos. Pasan a `<sup>`/`<sub>` (en las preguntas que tienen alguno, todas sus rachas, para que las cuatro opciones se vean iguales) y la BD sube a la v6 para renovarlas. `GlyphCoverageTests` comprueba que cada carácter de los textos y los seeds está en el atlas estático o en el .ttf (del que el atlas dinámico de respaldo lo saca en el móvil: así salen «Dvořák» o «Janáček»).
- Tests: migración a 6 (preguntas renovadas, victorias conservadas), exponentes con etiquetas, glifos. Tests en verde con V059m.

### 2026-10-25 · V060 · Icono de la app (el de la app Flutter)
- Al instalar el APK salía el icono de Unity: el proyecto no tenía icono. El autor quiere **el que tenía la app Flutter** (carta blanca con pica naranja sobre el morado `#7C3AED`), no uno nuevo con la paleta neón como decía el plan.
- `Assets/_Gamespot/Art/Icon/`: `app_icon.png` y `app_icon_foreground.png` copiados de `Legacy/Flutter/assets/icon/`, y `app_icon_background.png` (morado liso, generado). El importador los trata sin compresión ni mipmaps. Menú *Gamespot → Configuración → Icono de la app* (`ProjectSetup.ApplyIcons`, también en *Aplicar todo*): icono por defecto (Android 7.1 e iOS) y adaptativo de Android (fondo + carta). Los tipos "Legacy" y "Round" de Android están obsoletos en Unity 6.3 y se dejan vacíos.
- Comprobado dentro del APK (recursos `mipmap/app_icon`, `ic_launcher_background` y `ic_launcher_foreground`). APK rehecho sin errores ni avisos.

### 2026-10-25 · V065 (en curso) · Primer APK de prueba
- `SajitariosGamespot-prueba.apk` en la raíz del repo (git lo ignora): plataforma activa cambiada a Android con `build.target switch` y build con `build run --target Android --outputPath <raíz>/SajitariosGamespot-prueba.apk --confirm true` (3 min 18 s, sin errores ni avisos). Release con la firma de depuración de Unity (no vale para Google Play), IL2CPP ARM64, 38,4 MB, `com.sajitarios.sajitarios_gamespot` 0.76.0 (76), API 25–36, permisos INTERNET (el de serie de Unity) y VIBRATE (de `Handheld.Vibrate`).
- La build guarda ajustes en el proyecto (filtros de shaders de URP, batching de Android, el mapa del Input System en los precargados, la fuente de respaldo de TMP): se versionan para que la siguiente build no ensucie el árbol. La carpeta `*_BurstDebugInformation_DoNotShip/` que deja junto al APK se ignora.
- La plataforma activa del Editor se queda en Android.

### 2026-10-25 · V059k · Las 221 complicadas, fuera
- El autor decidió que las 221 palabras complicadas que seguían pendientes se quedan fuera. Ya no estaban en el seed, así que no cambia la BD (sigue en la versión 5); `SeedFilesTests` comprueba una muestra de cada grupo (actuario, úvula, talabartero, obenque, tuya, okapi, fenogreco, pizzicato, cromorno, yokai, feldespato, firmware, trébede, turbera, acuaplano, sustrato). La lista completa está en el registro de V059j y en el diff del seed de V059i.

### 2026-10-25 · V059j · Cachondeo (+18), vuelven las fáciles y fuera las que dice el autor
- **Cachondeo** (`WordCategories.Cachondeo`, `IsAdult`): lista del autor (política y polémica, streamers, porno y sexo explícito, humor bruto, dibujos de la infancia…), 82 palabras en el Impostor y 89 tarjetas en el Tabú. Las que ya existían se quedan en su categoría (Trump, Sánchez, Abascal, Ibai, Messi, Peppa Pig…); Marta Díaz vuelve dentro de Cachondeo. Va **desmarcada por defecto** (`WordCategories.Default`) y el `CategoryPicker` enseña un aviso +18 fijo mientras está marcada (elegido por Claude, como el modo picante de Yo Nunca; revisable).
- **No se metieron** (decisión de Claude, se le explica al autor): «menores de edad» y Jeffrey Epstein (con sexo en la misma categoría sería humor sobre abuso de menores), y «gitano», «rumano», «homosexual» y «macaco» (grupos de personas como chiste; «macaco» es el insulto racista contra Vinícius). **Pendientes de que el autor aclare:** Pereira, Fabiana Sevillano, «dedear panchas», «padalustro», «amigo kebap», «magic» y «virgen».
- **Vuelven 268 de las quitadas en V059i** (las que no eran complicadas, con ortografía corregida: tímpano, torreón, rosetón, gibón, laúd, punzón, carril bici, tienda de campaña…, y 19 de las de fiesta). **Fuera del todo:** las que dijo el autor, más feudo, Andreu Buenafuente y Eugenio, y las repetidas o con errata. **Pendientes de decisión:** 221 complicadas (el autor decidió en V059k que se quedan fuera).
- BD v5 (ver §5.2), probada en la BD real del Editor (4 → 5: 2525 y 1161). 230 EditMode + 49 PlayMode; arnés .NET 174. Captura `V059j_cachondeo_aviso.png`.

### 2026-10-25 · V059i · Categorías en el Impostor y el Tabú, y fuera las palabras difíciles
- **Categorías** (`Gamespot.Core.WordCategories`, compartidas por los dos dominios): Clásicas, Famosos, Política, Internet y redes, Tele y series, Personajes, Juegos, Fiesta, Muy de España, Sitios, y en el Impostor "Mis palabras" (las del usuario, `propias`), que solo sale si hay alguna. Panel `CategoryPicker` (Games/_Shared) en los dos setups: por defecto todas, al menos una, se conserva en el borrador. La partida las respeta a través de `CategoryWordSource` / `CategoryTabuWordSource` (el dominio sigue recibiendo una lista: no sabe de categorías), también en "Jugar otra". `ResponsiveChipGrid` pasa al kit (`Core.Runtime/UI`).
- **Palabras quitadas** (petición: «andreu, eugenio, feudo»): 516 de las 2060 heredadas del Impostor (náutica, arquitectura y minerales raros, especias e instrumentos antiguos, mitología poco conocida, oficios técnicos, latinoamericanismos, erratas y repetidas) y 32 de las de fiesta (famosos y cosas poco conocidas: Andreu Buenafuente, Eugenio, Martes y Trece, Los Morancos, streamers pequeños, Buzz, Dixit…); 11 tarjetas del Tabú (Candil, Desfiladero, Pedestal, Monzón, Tifón, Anfiteatro, Martes y Trece…). Errata corregida: «hamburgesa». La lista completa está en el diff de este commit (`git show` del seed); cualquier palabra se puede recuperar.
- BD v4 (ver §5.2), probada en la BD real del Editor (3 → 4: 2176 y 1065, sin «feudo»). Tests: migración a 4 (seed renovado, propias respetadas, tabla antigua sin columna), filtros y conteos, contenido de los seeds, PlayMode de categorías (solo Política, Mis palabras, Tabú solo Sitios y Jugar otra). 230 EditMode + 48 PlayMode; arnés .NET 174. Capturas `V059i_*`.

### 2026-10-25 · V059h · Palabras de fiesta y de España (Impostor y Tabú)
- Petición del autor: más palabras del Impostor «a ser posible de España» (políticos, streamers, juegos famosos, creadores y creadoras de contenido, tipos de alcohol, cosas graciosas de fiesta) y también en el Tabú; luego añadió personas y sitios famosos.
- **Impostor +664** (2724): política y realeza, creadoras y creadores, música, tele y cine, deporte, famosos del mundo, personajes de ficción, videojuegos, juegos de mesa, alcohol, fiesta y noche, ligar y redes, canciones de fiesta, series y programas, memes, marcas, sitios de España y del mundo, comida de bar. **Tabú +469** (1076) de los mismos temas. Escritas a mano y validadas por script (duplicados contra el banco sin tildes ni mayúsculas, pista ≠ palabra, prohibidas que no contienen la palabra, tildes).
- **Criterio con personas reales:** pistas y prohibidas neutras, solo de su vida pública (partido, cargo, programa, canción); nada de insultos ni de polémicas personales, y fuera las personas con acusaciones graves recientes. Conviene que el autor revise los nombres: el banco refleja lo conocido hasta 2025.
- BD v3 (ver §5.2), probada también en la BD real del Editor (2 → 3: 2724 y 1076).
- **Arreglo de UI que destaparon los nombres largos:** `Ui.Text` de una línea ya no parte por palabras (en negrita, sin «…», se perdían palabras enteras: «Andreu Buenafuente» salía «Andreu» y «fisioterapeuta deportivo», «fisioterapeuta»); ahora encoge hasta el 70 % y luego corta. `Ui.FittedText(lines: 2)` para la palabra del revelado del Impostor, la del turno del Tabú y la sílaba/categoría de La Bomba; la lista de palabras usa dos líneas que encogen. Capturas `V059h_*`.

### 2026-10-25 · Fase 10c · Mejoras elegidas por el autor (C2, C3+L1, B1+B2, C4, I1/T1/W2)
- **V059g · I1 / T1 / W2:** rondas del Impostor que siguen al máximo hasta tocarlas (`ImpostorSetupDraft.RoundsTouched`), Trivia sin preguntas repetidas en la partida y Wavelength sin espectros repetidos (con vuelta a empezar si se agotan). Se quitan de la deuda §8.2.
- **V059f · C4:** "Jugar otra" (`Module.PlayAgain`: misma configuración, partida nueva al momento; si falla, al setup) y "Cambiar ajustes" (`ResetToSetup`) en los fines de Tabú, Trivia, Wavelength y La Bomba. Ojo: el plan decía que Trivia y Wavelength no tenían "Jugar otra"; sí lo tenían, pero volvía al setup.
- **V059e · B1 + B2:** Tabú con 607 tarjetas (las 120 heredadas con tildes y erratas corregidas: «maze», «paddlear», «crocodilo», «greco», «bufo», duplicados como «Cofre del tesoro»/«Candelero»; y 487 nuevas por temas), validadas por script (duplicados sin tildes, prohibidas repetidas o que contienen la palabra, palabras que suelen llevar tilde). Al agotarse, vuelven a salir y se avisa. BD v2 con migración del seed (comprobada también en la BD real del Editor: 1 → 2, 607 palabras).
- **V059d · C3 + L1:** 9 sonidos sintetizados en el Editor (sin descargas), tic-tac de La Bomba que acelera, explosión con vibración, pitidos y bocina del Tabú, acierto/fallo de la Trivia, campanas del revelado de Wavelength y pasar página en Yo Nunca. Los módulos de esos juegos reciben el `AudioService`. No he podido escucharlos: se comprobó duración, pico y nivel de cada WAV; falta oírlos en el móvil.
- **V059c · C2:** jugadores guardados entre juegos (`AppSettings.LastPlayers` + `PlayerListEditor`). El arnés de PlayMode guarda y restaura la preferencia para que un test no rellene el siguiente.

### 2026-10-25 · V059a–V059b · Plan de mejoras y 5 juegos nuevos
- `Docs/PLAN_MEJORAS.md`: mejoras comunes y por juego con prioridad y esfuerzo, y 5 juegos nuevos (Verdad o Reto, ¿Quién soy?, Dibuja y adivina, Código secreto, Hombres Lobo). Hallazgos de contenido: faltan tildes en ~1 de cada 4 tarjetas de Tabú, 10 categorías de La Bomba y un centenar de palabras del Impostor; Tabú (120) se queda corto; los bancos solo existen en español aunque la app tenga inglés.
- Nada se implementa hasta que el autor elija.

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
