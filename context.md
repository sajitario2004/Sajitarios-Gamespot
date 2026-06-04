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
