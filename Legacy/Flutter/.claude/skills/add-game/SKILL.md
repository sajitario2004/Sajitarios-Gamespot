---
name: add-game
description: Scaffold a new game as an isolated bounded context under lib/games/ and register it in the GameRegistry, without touching the menu. Use when the user wants to add a new minigame to Sajitarios Gamespot.
disable-model-invocation: true
---

# Añadir un juego nuevo

Crea un juego como *bounded context* aislado siguiendo la Screaming Architecture del proyecto. El menú nunca se toca: solo se registra el juego en el `GameRegistry`.

Argumento: `$ARGUMENTS` = nombre del juego (ej. `tres_en_raya`). Si no se da, pregúntalo. Usa `snake_case` para carpetas/archivos y `PascalCase` para clases.

## Pasos

1. **Lee `plan.md`** (secciones "Estructura de carpetas" y "Game Registry") para alinearte con el patrón vigente, y mira un juego existente en `lib/games/` como plantilla.

2. **Pregunta el alcance** antes de generar: ¿persiste datos (necesita `data/` + tabla sqflite) o no? ¿qué pantallas tiene? ¿qué lógica de dominio (use cases)?

3. **Crea la estructura:**
   ```
   lib/games/<nombre>/
     domain/         # entidades + use cases puros (aleatoriedad vía RandomProvider inyectable)
     data/           # solo si persiste — repository sqflite + migración en AppDatabase
     presentation/   # screens + providers Riverpod
   ```

4. **Crea el `GameDescriptor`** del juego implementando el contrato de `lib/games/_shared/game_descriptor.dart` (`id`, `title`, `description`, `icon`, `buildEntryScreen`). Title/description/copy en **español**.

5. **Regístralo** añadiéndolo a la lista de `gameRegistryProvider` en `lib/games/_shared/game_registry.dart`. **No modifiques `MenuScreen`.**

6. **Tests:** crea `test/games/<nombre>/` con tests de los use cases de dominio (mockeando `RandomProvider` cuando haya aleatoriedad).

7. **Verifica:** `flutter analyze` (sin warnings) y `flutter test`.
