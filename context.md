# Contexto del proyecto — Sajitarios Gamespot

Este documento recoge el contexto original que dio el usuario sobre el proyecto, tal como lo expuso al inicio. Sirve como fuente de verdad de la intención y las reglas de los juegos antes de cualquier decisión técnica.

---

## Visión general

Una aplicación **local** (de momento, sin backend) que funciona como **menú de juegos simples**. Desde el menú el usuario selecciona el juego al que quiere jugar. Los juegos también se construirán **desde cero**. La persistencia local se hace con **SQLite**.

La aplicación debe estar pensada para que sea fácil **agregar nuevos juegos** en el futuro.

---

## Juegos planificados al inicio

### Juego 1 — "Es un 10 pero"

- El juego saca **una carta random** de una baraja de cartas normal.
- La carta tiene que estar entre **"A" y "10"** (es decir, valores 1 a 10, sin J, Q, K).
- El juego solo muestra una carta random, **no tiene más complicación** por ahora.
- El usuario indicará más contexto/funcionalidad sobre este juego más adelante.

### Juego 2 — "El Impostor"

Funcionamiento descrito por el usuario:

**Antes de la partida (configuración):**
1. Se introducen los **nombres de los jugadores** que van a jugar. **Máximo 15** jugadores.
2. Se selecciona el **número de impostores** que va a tener la partida, **entre 1 y 5**.
3. Sin embargo, existen dos reglas probabilísticas especiales:
   - **10% de probabilidad** de que **TODOS** los jugadores sean impostores.
   - **10% de probabilidad** de que **NINGUNO** sea impostor.
4. Antes de empezar también se puede activar/desactivar la opción de **pista**:
   - Si la pista está activa, cuando a un jugador le toca ser impostor, además de saber que es impostor recibe una **pista** sobre la palabra.
   - Ejemplo dado por el usuario: si la palabra es **"pirata"**, la pista podría ser **"barco"**.
5. **Importante:** todas las palabras de la base de datos deben tener **una pista asociada** obligatoriamente, por si la opción de pista se activa.

**Durante la partida:**
1. El sistema elige una **palabra aleatoria** de la lista de palabras.
2. Se procede a **pasar el móvil de persona en persona**.
3. El **orden de revelación** es el mismo orden en el que se introdujeron los nombres de los jugadores.
4. Cada jugador, cuando le toca, pulsa un botón **"Revelar"**:
   - Si **NO es impostor** → ve la **palabra**.
   - Si **es impostor** y la opción pista está **desactivada** → ve solo el texto **"impostor"**.
   - Si **es impostor** y la opción pista está **activada** → ve **"impostor"** + la pista asociada a la palabra.

**Ejemplo concreto dado por el usuario:**
- Palabra elegida: **"playa"**.
- Jugadores introducidos: **Nacho, Iker y Lucía** (en ese orden).
- A Nacho le toca ser **impostor**, la pista está **desactivada**.
- Orden de revelación:
  1. **Nacho** coge el móvil primero (porque fue el primero introducido), pulsa "Revelar" → aparece solo **"impostor"** (sin pista, porque la opción está desactivada).
  2. Después se pasa el móvil a **Iker**, pulsa "Revelar" → aparece la palabra **"playa"**.
  3. Finalmente **Lucía**, pulsa "Revelar" → aparece la palabra **"playa"**.

---

## Decisiones tomadas durante la conversación

### Stack tecnológico

- El usuario dudaba entre **Flutter** y **Unity**.
- Recomendación dada y **aceptada**: **Flutter + sqflite**.
- Razones principales:
  - Los juegos son UI + lógica + persistencia, no requieren física, 3D ni render en tiempo real.
  - sqflite ofrece SQLite local nativo.
  - Hot reload, binario pequeño, UI nativa multiplataforma.
  - Unity sería overkill (runtime pesado, fricción con SQLite, UI menos cómoda).

### Plataformas objetivo

- **Android** e **iOS** (decidido por el usuario).

### Gestión de palabras del Impostor

- **Decisión:** seed inicial + el usuario puede agregar/editar palabras propias desde la app.
- Las palabras seed son de solo lectura; las añadidas por el usuario son editables/borrables.

### Alcance de "Es un 10 pero"

- **Decisión:** MVP puro — solo revelar carta random A–10. Sin historial, sin reglas asociadas. Extensible para añadir más adelante lo que el usuario decida.

---

## Reglas y constraints clave (resumen rápido)

- App **local**, sin backend.
- Persistencia con **SQLite**.
- **Menú** principal con selección de juegos.
- Arquitectura **extensible** para añadir juegos nuevos sin tocar el resto.
- Juego del Impostor:
  - Jugadores: mín. 3, máx. 15.
  - Impostores: 1 a 5 (capado a `jugadores - 1` en modo normal).
  - 10% prob. todos impostores / 10% prob. ninguno impostor / 80% asignación normal.
  - Pista opcional, siempre asociada obligatoriamente a cada palabra.
  - Orden de revelación = orden de introducción de nombres.

---

# Estado actual del proyecto (contexto acumulado)

> Lo de arriba es la intención original del usuario. Esta sección documenta el
> estado REAL del proyecto tras su construcción completa. Donde haya discrepancia,
> manda esta sección. Roadmap detallado en `version.md`; guía operativa en `CLAUDE.md`.

## Resumen

App Flutter **construida al completo** (versiones 0.1 → 0.49, fases F1–F13),
verificada con `flutter analyze` limpio y **211 tests** en verde. Local, sin
backend. Dos juegos: "Es un 10 pero" y "El Impostor". Construida mediante
orquestación multi-agente (un workflow por fase, agentes especialistas + puerta QA
+ verificación adversarial en lo crítico). App lanzada y validada en simulador iOS.

## Stack y versiones

- Flutter 3.41.4 / Dart 3.11.1. Proyecto `sajitarios_gamespot` (org `com.sajitarios`).
- **Riverpod 3.x** (`flutter_riverpod ^3.3.1`): `Notifier`/`NotifierProvider`,
  `Provider`, `ProviderScope`; en tests, overrides con `overrideWithValue`.
- **go_router ^17.3.0** (`appRouterProvider`); rutas montadas iterando
  `gameRegistryProvider` (`for (g in registry) ...g.routes()`) — el router no
  menciona juegos concretos.
- **Flame 1.37.0** (+ `flame_audio`) para la carta y SFX.
- **sqflite** (+ `sqflite_common` para tipos puros y `sqflite_common_ffi` en tests).
- **i18n**: `flutter_localizations` + `intl`, ARB es/en en `lib/l10n`, `generate: true`.
- `shared_preferences` (idioma + toggle de audio). `image` (generadores de assets).
- Versión actual del `pubspec`: `0.49.0+49`.

## Arquitectura (Screaming Architecture)

```
lib/
  core/        db, random, routing, theme, widgets (neon), assets, audio, i18n
  games/
    _shared/   GameDescriptor + gameRegistryProvider
    es_un_10_pero/  domain (Card puro), presentation (Flame CardFlipGame)
    impostor/  domain, data, presentation
  menu/        MenuScreen (lee el registry; no conoce juegos)
  l10n/        ARB + AppLocalizations generado
```

- **Extensibilidad (regla de oro):** añadir juego = crear `lib/games/<juego>/` +
  registrar en `GameRegistry`. El descriptor aporta `routes()` y, si persiste,
  `onCreateTables`/`onUpgradeTables` (el esquema de cada juego vive en su contexto,
  NO en `core/db`). Skill `/add-game` para el scaffolding.
- Dominio puro y testeable; aleatoriedad SIEMPRE vía `RandomProvider`
  (`lib/core/random`), nunca `dart:math Random` directo.

## Reglas duras de estilo (preferencias del usuario)

- **Idioma:** todo el copy de UI en **español**, vía `AppLocalizations` (no
  hardcodear strings). i18n es/en con selector e idioma persistido.
- **NADA de emojis** en código ni en textos de UI: solo iconos de Flutter
  (`Icons.*` / `CupertinoIcons.*`). Los palos de la carta son `CupertinoIcons.suit_*_fill`.
- No commits/push salvo que el usuario lo pida.

## Juego 1 — "Es un 10 pero" (estado actual)

- Dominio puro: `Card` con `CardValue` (A–10) y `CardSuit` (espadas/corazones/
  diamantes/tréboles; `displayName`, `isRed`; el icono se mapea en presentación).
- `DrawCardUseCase(RandomProvider)`.
- UI: `EsUn10PeroScreen` con carta renderizada por **Flame** (`CardFlipGame`,
  animación de flip, look neón con halo tipo tubo, motor en pausa en reposo).
- **Cuenta atrás de 5 s** (F13): al pulsar sacar carta, cuenta 5→1 (botón
  deshabilitado) y luego revela la carta (flip + SFX).

## Juego 2 — "El Impostor" (estado actual — IMPORTANTE: el flujo cambió)

- Reglas de asignación INTACTAS (auditadas): `AssignRolesUseCase` con un solo
  `nextDouble()` (`<0.10` todos / `<0.20` ninguno / resto normal capado a
  `players-1`), orden de revelación = orden de introducción, pista obligatoria.
- **CAMBIO DE FLUJO (F13):** tras revelar todos los roles **YA NO se muestra una
  pantalla de resultados con los roles**. En su lugar hay una **VOTACIÓN por rondas**:
  - En el setup se elige nº de **rondas** (oportunidades): mín 1, máx
    `max(1, jugadores − 3)` (ej. 6 jugadores → máx 3).
  - `VotingScreen`: una sola pantalla compartida; el grupo elige a quién expulsar
    cada ronda (selección única, sin conteo individual).
  - Regla: **hay que pillar a TODOS los impostores**. Si el votado es impostor,
    queda eliminado; al no quedar impostores vivos → "¡Habéis ganado!". Si no, "el
    impostor sigue entre vosotros". Cada voto consume una ronda; si se agotan con
    impostores vivos → **gana el impostor SIN revelar** identidades.
  - `GameOverScreen` muestra solo el desenlace (sin roles) y **guarda el historial**.
- Flujo: Menú → Setup → (PassDevice → Reveal)×jugador → **Voting** → GameOver.
  Estado en `ImpostorFlowController` (fases setup/pass/reveal/**voting**/**gameOver**;
  `results` quedó como legado sin uso).
- **CRUD de palabras** (`WordsManagementScreen`): listar/buscar (con debounce),
  agregar (unicidad `COLLATE NOCASE`), editar/borrar solo las del usuario (las
  `is_seed` son de solo lectura). Seed de 60 palabras en `assets/seed/impostor_words.json`.
- **Historial/estadísticas** (`HistoryScreen`): partidas guardadas en `game_history`
  (sqflite) + estadísticas (total, palabra más repetida, ranking de impostores).

## Diseño visual — NEÓN cyberpunk (F11)

- Tema oscuro principal (`ThemeMode.dark`). Colores en `lib/core/theme/app_theme.dart`:
  fondo `#0A0A14`, cian `#00F0FF` (primary), magenta `#FF00E5` (secondary), violeta
  `#B026FF`, texto claro `#E6F7FF`. "Full neón": glow, bordes brillantes, rejilla.
- Widgets reutilizables en `lib/core/widgets/neon.dart`: `NeonText`, `NeonPanel`,
  `NeonGlowWrapper` (envuelve botones sin cambiar su tipo), `NeonBackground`,
  `PulseGlow` (respeta "reduce motion"; **no** colocarlo visible bajo
  `pumpAndSettle` en tests — cuelga por animación infinita).

## Persistencia (sqflite)

- `AppDatabase` (`lib/core/db`) genérico/versionado (`kAppDatabaseVersion` actual = 3),
  delega esquema/migraciones a cada juego vía `GameDescriptor`. Migraciones:
  v2 = `word UNIQUE COLLATE NOCASE` (dedup), v3 = tabla `game_history`.
- Esquema del Impostor en `lib/games/impostor/data/impostor_schema.dart`.

## Calidad y auditorías

- **2 auditorías multi-agente** realizadas (7 dimensiones + verificación adversarial).
  La 2ª (tras F8–F11) dio **0 critical / 0 high**. Todos los hallazgos accionables
  (medium/low + info reales) corregidos en F8 y F12.
- `flutter analyze` limpio; `flutter test` 211 verdes; cobertura ~75%+ (no versionada,
  `coverage/` en `.gitignore`).

## Pendientes / deuda conocida (no bloqueante)

- **Assets son placeholders** generados por código (`tool/generate_icon.dart`,
  `generate_images.dart`, `generate_sounds.dart`): icono, splash, `card_back.png`,
  `menu_header.png`, y los WAV. Sustituir por arte/audio final (mismos nombres) y
  regenerar; la integración no requiere cambios.
- **`integration_test/`** existe y se ha ejecutado en simulador, pero **necesita
  dispositivo/emulador** (no corre en CI sin device).
- UX menor: el selector de rondas arranca en el mínimo (1) y no auto-sube al máximo
  al añadir jugadores (ajustable a mano).
- Excepción de contraste documentada: `inversePrimary` (neonViolet) en rol de borde
  de snackbar ~4.18:1 (no es texto; aclararlo empeoraría sobre fondo claro).

## Fuera de alcance (requieren decisión/backend) — 0.50+

- Multijugador online, cuentas/cloud (necesitan backend; rompen el diseño local).
- Nuevos juegos vía `GameRegistry` (falta decidir cuáles).

## Mapa de versiones (resumen)

| Fase | Versiones | Contenido |
|---|---|---|
| F1 | 0.1–0.5 | Scaffold, theme, routing, GameRegistry, RandomProvider |
| F2 | 0.6–0.9 | "Es un 10 pero" + flip Flame |
| F3 | 0.10–0.12 | Base SQLite + WordRepository |
| F4 | 0.13–0.15 | Lógica Impostor (reglas 10/10/80) |
| F5 | 0.16–0.21 | UI Impostor (setup→pass→reveal→results) |
| F6 | 0.22–0.24 | CRUD de palabras |
| F7 | 0.25–0.30 | Pulido + estabilización |
| F8 | 0.31–0.38 | Correcciones 1ª auditoría |
| F9 | 0.39–0.41 | Assets (imágenes y sonidos) |
| F10 | 0.42–0.45 | Estadísticas/historial + i18n |
| F11 | 0.46–0.47 | Rediseño visual neón |
| F12 | 0.48 | Correcciones 2ª auditoría |
| F13 | 0.49 | Votación del Impostor + cuenta atrás de carta |

## Herramientas y configuración del repo

- Skills instaladas en `.agents/skills/` (oficiales de Flutter/Dart + Flame, a11y,
  seguridad, etc.) y skill de proyecto `/add-game` en `.claude/skills/`.
- Hook `dart format` en `.claude/settings.json` (PostToolUse Write|Edit) y modo
  `bypassPermissions` activo (compartido).
