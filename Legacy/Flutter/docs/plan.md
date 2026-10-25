# Plan — Sajitarios Gamespot

## Contexto

Construir una aplicación móvil (Android + iOS) que funcione como **menú/hub de minijuegos locales** tipo party games. La app es offline, persiste datos en SQLite y arranca con dos juegos:

1. **"Es un 10 pero"** — revelador de carta random A–10 (MVP simple, extensible más adelante).
2. **"El Impostor"** — juego clásico de palabras donde unos jugadores conocen la palabra y los impostores no, con pista opcional y reglas probabilísticas especiales.

La arquitectura debe permitir **agregar nuevos juegos fácilmente** sin tocar el resto.

## Stack confirmado

- **Flutter** (Dart) — UI multiplataforma, hot reload.
- **Flame** — motor de juegos 2D para Flutter (game loop, componentes, sprites, animaciones). Se usa para la superficie de juego (ej. flip de carta de "Es un 10 pero", animaciones de reveal). Los menús y formularios siguen en Flutter/Material; Flame se embebe con `GameWidget` dentro de la `presentation/` de cada juego que lo requiera.
- **sqflite** — SQLite local.
- **Plataformas:** Android e iOS.
- **State management:** `Riverpod` (simple, testeable, escala bien).
- **Routing:** `go_router` (rutas declarativas, deep links a futuro).
- **Estructura:** **Screaming Architecture** — la estructura del repo grita "esto es una app de juegos", no "esto es Flutter".

## Estructura de carpetas

```
lib/
  core/
    db/                  # AppDatabase, migraciones, helpers sqflite
    random/              # RandomProvider (inyectable, mockeable en tests)
    routing/             # go_router config
    theme/               # Material theme, colores, tipografía
    widgets/             # Botones, cards, layouts compartidos
  games/
    _shared/             # GameDescriptor, GameRegistry, interfaces comunes
    es_un_10_pero/
      domain/            # Card, Deck, DrawCardUseCase
      data/              # (vacío en MVP, no persiste)
      presentation/      # EsUn10PeroScreen, providers
    impostor/
      domain/            # Word, GameConfig, Player, Role, AssignRolesUseCase
      data/              # WordRepository (sqflite), seed loader
      presentation/      # setup → reveal → results screens, providers
  menu/                  # MenuScreen (lista juegos desde GameRegistry)
  main.dart
assets/
  seed/impostor_words.json
test/
  games/impostor/        # tests de AssignRolesUseCase (probabilidades, edge cases)
  games/es_un_10_pero/   # tests de DrawCardUseCase
```

**Por qué así:** cada juego es un **bounded context** independiente. Para agregar un tercer juego se crea `lib/games/nuevo_juego/` y se registra en `GameRegistry`. El menú no se toca.

## Modelo de datos (SQLite)

```sql
-- Palabras del Impostor
CREATE TABLE impostor_words (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  word TEXT NOT NULL UNIQUE,
  hint TEXT NOT NULL,           -- requerida siempre (regla del juego)
  is_seed INTEGER NOT NULL DEFAULT 0,  -- 1 si vino del JSON inicial
  created_at INTEGER NOT NULL
);
CREATE INDEX idx_impostor_words_word ON impostor_words(word);
```

Migraciones versionadas en `AppDatabase` (campo `version` de sqflite). El seed inicial se carga desde `assets/seed/impostor_words.json` la primera vez que la tabla está vacía.

## Game Registry (clave de la extensibilidad)

```dart
// lib/games/_shared/game_descriptor.dart
abstract class GameDescriptor {
  String get id;
  String get title;
  String get description;
  IconData get icon;
  Widget buildEntryScreen(BuildContext context);
}

// lib/games/_shared/game_registry.dart
final gameRegistryProvider = Provider<List<GameDescriptor>>((ref) => [
  EsUn10PeroGame(),
  ImpostorGame(),
  // futuros juegos acá
]);
```

El `MenuScreen` solo lee `gameRegistryProvider` y pinta una grid/lista. **No conoce ningún juego en concreto.**

## Juego 1 — "Es un 10 pero" (MVP)

**Alcance MVP:**
- Pantalla con botón grande "Sacar carta".
- Tira una carta random: valor (A, 2–10) + palo (♠♥♦♣).
- Animación simple de flip (render con **Flame**: la carta es un componente Flame dentro de un `GameWidget`).
- Sin persistencia, sin historial.

**Implementación:**
- `Card` (value: enum 1..10 con label "A"/"2".../"10", suit: enum).
- `DrawCardUseCase(RandomProvider)` → devuelve `Card`. **Testeable inyectando un Random fijo.**

## Juego 2 — "El Impostor" (núcleo del proyecto)

### Flujo de pantallas

```
SetupScreen (nombres + nº impostores + pista on/off)
  → PassDeviceScreen (instrucción "pasale el móvil a {jugador}")
  → RevealScreen (botón "Revelar" → muestra palabra/IMPOSTOR + pista opcional)
  → siguiente jugador… hasta el último
  → ResultsScreen (muestra a todos, fin de partida)
```

### Configuración pre-partida

- **Jugadores:** lista ordenada, mínimo 3, **máximo 15** (validación en UI).
- **Nº impostores:** slider/stepper de **1 a 5**, capado a `players.length - 1`.
- **Pista activa:** switch on/off.

### Asignación de roles — `AssignRolesUseCase`

Reglas exactas (ESTO ES CRÍTICO, lo encapsulamos y lo testeamos a fondo):

1. Tirar `rng.nextDouble()`:
   - `< 0.10` → **TODOS son impostores** (caso especial).
   - `< 0.20` (es decir, siguiente 10%) → **NINGUNO es impostor** (todos saben la palabra).
   - resto (80%) → asignación normal: elegir `nImpostores` jugadores random.
2. Elegir **una palabra random** de `impostor_words` (con su pista).
3. Devolver `GameSession { word, hint, assignments: Map<Player, Role> }` donde `Role = palabra | impostor`.

**Orden de revelación:** el del array de jugadores tal como fueron introducidos (NO se baraja para revelar — sí para asignar roles).

### RevealScreen — comportamiento

- Muestra: "Es el turno de **{jugador}**" + botón grande "Revelar".
- Al pulsar:
  - Si rol = palabra → muestra la palabra grande.
  - Si rol = impostor:
    - Si `hintEnabled` = false → muestra solo "IMPOSTOR".
    - Si `hintEnabled` = true → muestra "IMPOSTOR" + la pista.
- Botón "Ocultar y pasar" → vuelve a `PassDeviceScreen` del siguiente jugador.

### Gestión de palabras (pantalla CRUD)

Accesible desde el menú del Impostor:
- Listar palabras (seed + propias), búsqueda por texto.
- Agregar nueva (word + hint requeridos, validar unicidad).
- Editar/borrar **solo las del usuario** (`is_seed = 0`).
- Las seed son de solo lectura para no romper la base.

## Archivos críticos a crear

| Archivo | Responsabilidad |
|---|---|
| `pubspec.yaml` | deps: flutter_riverpod, go_router, flame, sqflite, path, path_provider |
| `lib/main.dart` | `ProviderScope` + arranque |
| `lib/core/db/app_database.dart` | Singleton sqflite, migraciones, seed loader |
| `lib/core/random/random_provider.dart` | Wrapper inyectable sobre `Random` |
| `lib/games/_shared/game_descriptor.dart` | Contrato de juego |
| `lib/games/_shared/game_registry.dart` | Provider con la lista de juegos |
| `lib/menu/menu_screen.dart` | Grid del menú principal |
| `lib/games/es_un_10_pero/domain/draw_card_use_case.dart` | Lógica de carta random |
| `lib/games/es_un_10_pero/presentation/es_un_10_pero_screen.dart` | UI carta |
| `lib/games/impostor/data/word_repository.dart` | CRUD sqflite + seed |
| `lib/games/impostor/domain/assign_roles_use_case.dart` | **Reglas de probabilidad (lo más testeado)** |
| `lib/games/impostor/presentation/setup_screen.dart` | Config de partida |
| `lib/games/impostor/presentation/pass_device_screen.dart` | Pantalla intermedia |
| `lib/games/impostor/presentation/reveal_screen.dart` | Revelar rol |
| `lib/games/impostor/presentation/results_screen.dart` | Fin de partida |
| `lib/games/impostor/presentation/words_management_screen.dart` | CRUD palabras |
| `assets/seed/impostor_words.json` | Semilla de palabras+pistas |

## Fases de implementación

1. **F1 — Scaffold:** `flutter create`, deps, theme, routing, `MenuScreen` vacío con `GameRegistry`.
2. **F2 — Es un 10 pero:** `DrawCardUseCase` + pantalla + tests del use case.
3. **F3 — Base SQLite Impostor:** `AppDatabase`, `impostor_words`, seed loader, `WordRepository`.
4. **F4 — Impostor lógica:** `AssignRolesUseCase` + **suite de tests dedicada** (probabilidades 10/10/80, edge cases: todos impostores, ninguno, capado por nº jugadores).
5. **F5 — Impostor UI:** Setup → Pass → Reveal → Results.
6. **F6 — CRUD palabras:** pantalla de gestión.
7. **F7 — Pulido:** iconos, splash, animaciones de flip de carta (con Flame), copy.

## Verificación

- **Tests unitarios:**
  - `DrawCardUseCase`: con `Random` mockeado, comprobar que devuelve cartas válidas (1..10, 4 palos).
  - `AssignRolesUseCase`: ejecutar 10.000 iteraciones con seed fija para verificar distribución ~10/10/80; tests específicos para los dos casos especiales; test de "nImpostores capado a players-1".
  - `WordRepository`: CRUD + carga de seed (en sqflite in-memory).
- **Manual end-to-end** (`flutter run` en emulador Android + simulador iOS):
  - Menú muestra los dos juegos.
  - Es un 10 pero: sacar carta varias veces → ver que cambia.
  - Impostor: crear partida con 3 jugadores, 1 impostor, sin pista → recorrer reveal → verificar que solo uno ve "IMPOSTOR".
  - Repetir con pista activa → impostor ve la pista.
  - Agregar una palabra propia desde el CRUD → jugar y ver que puede salir.
  - Editar/borrar una propia: OK. Intentar borrar una seed: deshabilitado.
- **`flutter analyze`** sin warnings antes de cada PR.

## Fuera de alcance (por ahora)

- Multijugador online / sincronización.
- Cuentas, login, cloud.
- Animaciones complejas (más allá del flip de carta).
- Internacionalización (arrancamos en español).
- Estadísticas/historial de partidas.
