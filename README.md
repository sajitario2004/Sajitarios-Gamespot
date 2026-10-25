# Sajitarios Gamespot

Hub **offline** de minijuegos tipo *party games* para móvil (Android + iOS): eliges un juego en el menú y jugáis en un solo móvil, pasándooslo. Interfaz en español (e inglés) con estética neón.

**Juegos:** Es un 10 pero · El Impostor · Preguntas por puntos · Wavelength · Tabú · Yo Nunca · La Bomba.

> **Estado:** migración de la app Flutter original a **Unity** en curso (rama `feature/unity-port`). La app Flutter está en `Legacy/Flutter/` como referencia. Hoja de ruta en [`VERSIONS.md`](VERSIONS.md).

## Abrir el proyecto
1. Unity Hub → *Add project from disk* → carpeta `SajitariosGamespot/`.
2. Versión de Unity: **6000.3.27f1** (con los módulos de Android e iOS).
3. Escena principal: `Assets/_Gamespot/Scenes/App.unity`.

## Tests
Unity → *Window → General → Test Runner* → EditMode y PlayMode. Desde la CLI: `Invoke-UnityTests EditMode` (ver `CLAUDE.md` §3).

## Builds
- Android: *File → Build Profiles → Android → Build* (salida en `SajitariosGamespot/Builds/Android/`, ignorada por git).
- iOS: genera el proyecto de Xcode; compilar y firmar requiere un Mac.

## Documentación del proyecto
- [`CLAUDE.md`](CLAUDE.md): reglas de trabajo.
- [`CONTEXT.md`](CONTEXT.md): contexto, especificación de los juegos y decisiones.
- [`LEARNINGS.md`](LEARNINGS.md): preferencias del autor.
- [`TREE.md`](TREE.md): árbol de carpetas.
