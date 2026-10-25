# Sajitarios Gamespot

App móvil **Flutter** (Android + iOS), **offline**, que funciona como **menú/hub de
minijuegos locales** tipo party games. La persistencia es local con **SQLite**
(`sqflite`) y todo el copy de la interfaz está en **español**.

Versión actual: **0.30.0+30** (estabilización pre-release).

## Juegos incluidos

- **Es un 10 pero** — saca una carta aleatoria de la baraja entre **A y 10**
  (sin J/Q/K), con animación de volteo renderizada con **Flame**.
- **El Impostor** — juego de palabras por turnos: cada jugador revela su rol
  pasándose el móvil. Reglas probabilísticas especiales en el reparto de roles
  (10% todos impostores / 10% ninguno / 80% reparto normal), pista opcional y
  gestión (CRUD) del banco de palabras.

## Stack

- **Flutter 3.41.4 / Dart 3.11.1**
- **Riverpod 3.x** (`flutter_riverpod`) — gestión de estado.
- **go_router 17.x** — routing declarativo (`appRouterProvider`).
- **Flame 1.37.0** — superficie de juego 2D (animaciones, game loop).
- **sqflite** — SQLite local; `sqflite_common_ffi` para tests en memoria.

## Arquitectura

**Screaming Architecture**: la estructura del repo grita "app de juegos". Cada
juego es un *bounded context* aislado.

```
lib/
  core/
    db/          AppDatabase (sqflite, migraciones, seed loader)
    random/      RandomProvider inyectable (mockeable en tests)
    routing/     go_router (appRouterProvider)
    theme/       app_theme.dart (morado #7C3AED + ámbar)
    widgets/     widgets compartidos
  games/
    _shared/     GameDescriptor + GameRegistry (gameRegistryProvider)
    es_un_10_pero/{domain,data,presentation}/
    impostor/{domain,data,presentation}/
  menu/          MenuScreen (lee gameRegistryProvider, no conoce juegos concretos)
  main.dart
assets/
  seed/impostor_words.json
test/              unit + widget tests (en español)
integration_test/  flujos e2e (requieren dispositivo)
```

La lógica de negocio vive en `domain/` como use cases puros y testeables. La
aleatoriedad siempre se inyecta vía `RandomProvider` (nunca `Random()` directo)
para poder fijar seeds en los tests.

## Cómo ejecutar

```bash
flutter pub get
flutter run            # emulador Android o simulador iOS
```

## Cómo testear

```bash
flutter test                       # suite completa (130 tests)
flutter test --plain-name "nombre" # un test concreto
flutter test --coverage            # genera coverage/lcov.info
```

Cobertura actual: **~75% global** de líneas. Los tests de integración
(`integration_test/`) requieren un dispositivo Android/iOS real o emulado y se
lanzan con `flutter test integration_test` o `flutter drive`.

Antes de cada PR: `flutter analyze` debe quedar **sin warnings ni errores**.

## Cómo añadir un juego nuevo

La regla de oro: **el `MenuScreen` nunca conoce juegos concretos**, solo lee
`gameRegistryProvider`. Añadir un juego = crear `lib/games/<nuevo>/` + registrarlo
en `GameRegistry` (`lib/games/_shared/game_registry.dart`). No se toca el menú.

Pasos (automatizados por la skill **`/add-game`**):

1. Crear el bounded context `lib/games/<nuevo>/{domain,data,presentation}/`.
2. Implementar un `GameDescriptor` (id, título, descripción, icono,
   `buildEntryScreen`).
3. Registrarlo en la lista de `gameRegistryProvider`.
4. Añadir tests en `test/games/<nuevo>/`.

## Build Android

```bash
flutter build apk --debug      # APK de prueba (debug)
flutter build apk --release    # APK de release
flutter build appbundle        # AAB para Play Store
```

El icono y el splash se generan con:

```bash
dart run flutter_launcher_icons
dart run flutter_native_splash:create
```

## Build iOS

```bash
flutter build ios --no-codesign   # build de prueba sin firma
flutter build ipa                 # release firmado (requiere certificados)
```

> La firma de iOS (`codesign`) requiere certificados de Apple Developer
> configurados en Xcode; en CI sin firma se usa `--no-codesign`.
