# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Qué es esto

App móvil **Flutter** (Android + iOS), offline, que funciona como **menú/hub de minijuegos locales** tipo party games. Persiste en SQLite vía `sqflite`. Arranca con dos juegos: "Es un 10 pero" (revelar carta random A–10) y "El Impostor". Spec completa en `@plan.md`; contexto e intención original del usuario en `@context.md`. **Lee ambos antes de implementar.**

> Estado: en planificación. El scaffold de Flutter (`lib/`, `pubspec.yaml`) aún no existe.

## Stack y convenciones

- **Flutter / Dart**, state management con **Riverpod** (`flutter_riverpod`), routing con **go_router**, persistencia con **sqflite**.
- **Motor de juegos: Flame** (`flame`) para el render y la lógica de juego (loop, componentes, animaciones, sprites). Cada juego que necesite render/animación se construye como un `FlameGame` embebido en su `presentation/` vía `GameWidget`. La UI de menús, setup y formularios sigue siendo Flutter/Material normal — Flame es solo para la superficie de juego.
- **Screaming Architecture**: cada juego es un *bounded context* aislado en `lib/games/<juego>/{domain,data,presentation}`. Código compartido en `lib/core/` y `lib/games/_shared/`.
- **Extensibilidad (regla de oro):** añadir un juego = crear `lib/games/<nuevo>/` + registrarlo en `GameRegistry` (`lib/games/_shared/game_registry.dart`). **El `MenuScreen` nunca conoce juegos concretos** — solo lee `gameRegistryProvider`. No acoples el menú a un juego.
- **Idioma:** la app y todo el copy de UI van en **español**.
- Lógica de negocio en `domain/` use cases puros y testeables; aleatoriedad siempre vía un `RandomProvider` inyectable (nunca `Random()` directo en use cases) para poder mockearla en tests.

## Reglas críticas del Impostor (NO improvisar)

`AssignRolesUseCase` encapsula las reglas probabilísticas — son el núcleo testeado del proyecto. Con un solo `rng.nextDouble()`:
- `< 0.10` → **TODOS** impostores.
- `< 0.20` → **NINGUNO** impostor.
- resto (80%) → asignación normal de `nImpostores` jugadores random.

Otras reglas: jugadores mín. 3 / máx. 15; impostores 1–5 capado a `players.length - 1`; **toda palabra lleva pista obligatoria**; el **orden de revelación = orden de introducción de nombres** (se baraja para asignar roles, NO para revelar); palabras seed (`is_seed = 1`) son solo lectura, las del usuario son editables/borrables.

## Comandos

- Tests: `flutter test` — un solo test: `flutter test --plain-name "nombre"`.
- **`flutter analyze` sin warnings antes de cada PR** (requisito).
- Correr: `flutter run` (emulador Android / simulador iOS).
- La distribución 10/10/80 se valida con ~10.000 iteraciones y seed fija — no cambies esos tests sin entender por qué.

## Etiqueta de repo

- Trabaja en una rama, no en `main`. Ejecuta `flutter analyze` y `flutter test` antes de abrir PR.
