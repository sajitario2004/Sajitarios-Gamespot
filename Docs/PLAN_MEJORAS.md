# Plan de mejoras y juegos nuevos (V059a / V059b)

> 2026-10-25. Escrito al terminar de portar los 7 juegos a Unity, a petición del autor. **Nada de esto está
> hecho todavía:** es una carta para elegir. Marca lo que quieras (o di los códigos, p. ej. "C1, C2, B1 y
> Verdad o Reto") y se convierte en versiones de `VERSIONS.md`.

**Estado (2026-10-25):** hechas **C2, C3 + L1, B1 + B2, C4, I1, T1 y W2** (fase 10c de `VERSIONS.md`,
V059c–V059g) y, fuera del plan, **I4** (categorías en el Impostor y también en el Tabú) y el banco de
fiesta y de España sin las palabras difíciles (V059h–V059i). Lo demás sigue pendiente de que elijas. **Nuevo (V059r):** sección 4, diez juegos para elegir cinco. Corrección: en C4 decía que Trivia y Wavelength no
tenían "Jugar otra"; sí lo tenían, pero llevaba al setup. Ahora los cuatro empiezan al momento con los mismos
ajustes y además tienen "Cambiar ajustes".

**Cómo leerlo**
- **Prioridad:** Alta (se nota en la primera partida), Media, Baja.
- **Esfuerzo:** **S** = 1–2 versiones (como Yo Nunca), **M** = 3–5 (como Tabú), **L** = 6 o más (como el Impostor).
- **Ojo regla:** toca una regla tuya y no se hace sin tu OK expreso.

---

## 0. Lo que recomiendo hacer primero

Antes de añadir nada:
1. **Probar la app en un móvil de verdad** (fase 12: build Android). Es lo más importante. Hasta ahora todo
   se ha probado en el Editor.
2. **C1 · Fuentes e iconos reales.** Es lo que más cambia el aspecto. Hoy los iconos son letras: «<», «?»,
   «oo».

Después, por este orden:

| # | Mejora | Por qué |
|---|---|---|
| 1 | **C2** · Jugadores guardados entre juegos | Los nombres se escriben una vez por noche, no en cada juego |
| 2 | **C3 + L1** · Sonido y vibración (sobre todo el tic-tac de La Bomba) | Hoy La Bomba es silencio total, y la tensión del juego es el tic-tac |
| 3 | **B1 + B2** · Tabú con 500+ palabras y con tildes | Con 120 una partida larga casi lo agota; además, 1 de cada 4 tarjetas va sin tildes |
| 4 | **C4** · "Jugar otra" en todos los juegos | Hoy solo el Impostor la tiene; los demás te sacan al menú |
| 5 | **I1, T1, W2** · Las tres deudas pequeñas que se notan al jugar | Rondas del Impostor, preguntas y espectros repetidos |

---

## 1. Mejoras comunes a todos los juegos

| Código | Mejora | Detalle | Prioridad | Esfuerzo |
|---|---|---|---|---|
| **C1** | Fuentes e iconos reales | Material Symbols (Apache-2.0) para los iconos y una fuente con negrita real (OFL). **Necesito tu permiso para descargarlas.** | Alta | S |
| **C2** | Jugadores guardados | Se guardan los últimos nombres y los setups (Impostor, Trivia, Wavelength, Bomba) arrancan con ellos. Botón "Usar los de la última partida". | Alta | S |
| **C3** | Sonido y vibración por juego | Hoy hay 3 sonidos en 2 juegos. Propuesta: Bomba (tic-tac, explosión y vibración), Tabú (pitido en los últimos 5 s y bocina al final), Trivia (acierto y fallo), Wavelength (revelado) y Yo Nunca (pasar). Sonidos sintetizados propios, o CC0 con tu permiso. | Alta | M |
| **C4** | "Jugar otra" con los mismos jugadores | En Tabú, Trivia, Wavelength y La Bomba. | Alta | S |
| **C5** | Pantalla de ajustes | Idioma, sonido, vibración y "Reducir animaciones". Esta última ya existe en el código, pero no tiene interruptor. | Media | S |
| **C6** | Contenido en inglés | La app está en español e inglés, pero todos los bancos de contenido están solo en español: en inglés se juega con palabras en español. **Decides tú:** ocultar el inglés hasta tener bancos (S), o crearlos (L). | Media | S / L |
| **C7** | Contenido propio en todos los juegos | Como las palabras del Impostor: añadir y editar en Tabú, Yo Nunca, La Bomba y Wavelength, con una pantalla común. Lo de serie sigue siendo de solo lectura. | Media | M |
| **C8** | Accesibilidad | El dial de Wavelength no anuncia la posición de la aguja (pendiente heredado del Flutter, 0.79). Además: contraste del texto atenuado y letra grande en todas las pantallas. | Media | M |
| **C9** | Revisión del contenido | Todos los bancos los generó una máquina y hay que revisar los datos. Faltan tildes en unas 100 palabras del Impostor, 29 de las 120 tarjetas de Tabú y 10 categorías de La Bomba. | Media | M |
| **C10** | Reglas la primera vez | Al abrir un juego por primera vez se muestran sus reglas, que ya existen. | Baja | S |
| **C11** | Estadísticas por juego | Hoy solo guardan algo el Impostor (historial) y la Trivia (victorias). | Baja | M |

---

## 2. Mejoras por juego

### Es un 10 pero
- **E0 · Falta tu contexto.** Hoy solo saca una carta de A a 10. En su día dijiste que darías más contexto;
  antes de tocarlo necesito saber cómo se juega en la mesa.
- Ideas mientras tanto (A–10 se mantiene, es regla tuya):
  - **E1** · Mazo sin repetir: no vuelve a salir una carta hasta sacar las 40, y se ven las que quedan. (S)
  - **E2** · Cuenta atrás configurable: 0, 3 o 5 s. (S)
  - **E3** · Banco opcional de "peros" para inspirar la ronda, p. ej. "Es un 10 pero… ronca". (S)

### El Impostor
| Código | Mejora | Prioridad | Esfuerzo |
|---|---|---|---|
| **I1** | El selector de rondas empieza en 1 y no sube al añadir jugadores (deuda). Que empiece en el máximo recomendado y lo siga. | Alta | S |
| **I2** | **Ojo regla.** En los casos "todos impostores" y "ninguno" (10 % + 10 %) el grupo hoy no puede ganar: vota sin poder acertar. Propuesta: botón "¡No hay impostor!" en la votación, y al final decir que eran todos. **No toca el reparto 10/10/80**, solo cómo se cierra la votación. | Alta | M |
| **I3** | Temporizador de debate opcional antes de votar (2–5 min). | Media | S |
| **I4** | Categorías de palabras para elegir (comida, lugares, famosos…). Hay que etiquetar las 2060 palabras. | Media | M |

### Preguntas por puntos (Trivia)
| Código | Mejora | Prioridad | Esfuerzo |
|---|---|---|---|
| **T1** | Que no se repita una pregunta en la misma partida (deuda). | Alta | S |
| **T2** | El tramo "muy difícil" tiene 218 preguntas frente a unas 470 de los otros. Ampliarlo y revisar los datos. | Media | M |
| **T3** | Comodín 50:50, uno por jugador y partida, para que no caiga nadie a la primera. | Media | S |
| **T4** | Tiempo por pregunta opcional (15 o 30 s). | Media | S |
| **T5** | Modo por puntos, sin eliminación (opcional). | Baja | M |

### Wavelength (ya arreglado en el port)
| Código | Mejora | Prioridad | Esfuerzo |
|---|---|---|---|
| **W1** | Los dos conceptos junto a los extremos del dial (hoy van en una fila encima). | Media | S |
| **W2** | Que no se repita un espectro en la partida (deuda). | Alta | S |
| **W3** | Modo competitivo por equipos, como el original: el otro equipo apuesta si la respuesta queda a la izquierda o a la derecha. | Media | M |
| **W4** | Pasar de 80 a más de 200 espectros. | Media | S |

### Tabú
| Código | Mejora | Prioridad | Esfuerzo |
|---|---|---|---|
| **B1** | **El banco se queda corto.** Un turno de 60 s puede gastar fácilmente 10–15 palabras (cada acierto, salto o falta pasa a otra); una partida larga casi agota las 120 y, cuando se acaban, se queda siempre la misma tarjeta. Ampliar a 500 o más y avisar si se agota. | Alta | M |
| **B2** | Tildes: 29 de las 120 tarjetas («Volcan», «erupcion», «Brujula»). | Alta | S |
| **B3** | Objetivo configurable (3, 5 o 7 victorias) (deuda). "Jugar otra" va en C4. | Media | S |
| **B4** | Límite de saltos opcional y resumen del turno (acertadas, saltadas y faltas). | Media | S |
| **B5** | Hasta 4 equipos. | Baja | M |

### Yo Nunca
| Código | Mejora | Prioridad | Esfuerzo |
|---|---|---|---|
| **Y1** | Ampliar el banco: hay 98 frases suaves y en una noche se agotan. | Media | S |
| **Y2** | Nivel intermedio entre suave y picante. | Media | S |
| **Y3** | Frases propias (va con C7). | Media | M |
| **Y4** | Modo con jugadores y marcador de quién "lo ha hecho" más (opcional). | Baja | M |

### La Bomba
| Código | Mejora | Prioridad | Esfuerzo |
|---|---|---|---|
| **L1** | Tic-tac que acelera según se acerca la explosión, sonido de explosión y vibración (va con C3). | Alta | S |
| **L2** | Mecha configurable (deuda): corta 10–30 s, normal 10–60 s o larga 30–90 s. | Media | S |
| **L3** | Modo mixto: sílaba o categoría al azar en cada ronda. Además, ampliar los bancos y poner las tildes que faltan («paises», «Espana»). | Media | S |
| **L4** | Vidas opcionales: 2 o 3 explosiones antes de quedar fuera, para grupos que quieren partidas más largas. | Baja | S |

---

## 3. Cinco juegos nuevos (V059b)

Todos cumplen lo mismo que los actuales: un solo móvil, sin internet, se juega pasándolo o dejándolo en el
centro, y estética neón. Cada uno es una carpeta nueva `Games/<Juego>/` más una línea en `GameRegistry`.

| Juego | Jugadores | Tipo | Qué reutiliza | Esfuerzo |
|---|---|---|---|---|
| **Verdad o Reto** | 2–15 | Atrevido, por turnos | Yo Nunca (niveles y aviso +18), lista de jugadores | **S** |
| **¿Quién soy?** (en la frente) | 4+ en equipos | Mímica y descripción, contrarreloj | Tabú (temporizador, equipos, marcador), palabras del Impostor | **M** |
| **Dibuja y adivina** | 4+ en equipos | Dibujo, contrarreloj | Tabú (temporizador, equipos), palabras del Impostor | **M** |
| **Código secreto** | 4–10 en 2 equipos | Palabras y deducción | Las 2060 palabras del Impostor y la rejilla del menú | **M–L** |
| **Hombres Lobo** (el móvil narra) | 6–15 | Roles ocultos y deducción | Pasar y revelar del Impostor, votación, diálogos | **L** |

### Verdad o Reto
- Por turnos y en el orden de los nombres, cada jugador elige "Verdad" o "Reto" y sale una tarjeta.
- Niveles suave y picante. Con picante, el mismo aviso +18 de Yo Nunca.
- Opcional: si alguien no quiere responder o hacer el reto, "Paso" con prenda.
- **Por qué encaja:** es el clásico de fiesta que le falta al menú, y casi todo existe ya. Lo grande es el
  contenido: unas 150 verdades y 150 retos por nivel.

### ¿Quién soy? (en la frente)
- Un jugador se pone el móvil en la frente con la palabra hacia fuera; su equipo la describe o la hace con
  mímica.
- Si acierta, inclina el móvil hacia abajo; si pasa, hacia arriba. Se detecta con el acelerómetro y también
  hay botones por si acaso.
- Turnos de 60 s y gana el equipo con más aciertos.
- **Por qué encaja:** es el party game de móvil por excelencia y usa el teléfono como pieza del juego.
- Hace falta un banco nuevo de famosos y personajes; para el resto vale el del Impostor.

### Dibuja y adivina
- El que dibuja ve una palabra y la dibuja con el dedo en el móvil; su equipo tiene 60 s para adivinarla.
- Colores básicos, deshacer y borrar todo.
- Al acabar el turno se pasa el móvil al otro equipo.
- **Por qué encaja:** es muy distinto de lo que hay en el menú (es visual, no verbal) y cabe en un solo móvil.
- Lo nuevo es el lienzo de dibujo (trazos suaves sobre una textura).

### Código secreto (palabras por equipos)
- Una rejilla de 5×5 palabras. Cada equipo tiene un "jefe de espías", el único que puede abrir la clave de
  colores, que está en una pantalla protegida.
- El jefe da una pista de una palabra y un número; su equipo toca palabras en el móvil. Si toca la palabra
  "asesina", pierde.
- **Por qué encaja:** reutiliza el banco de 2060 palabras y deja el móvil en el centro de la mesa.
- El nombre y el diseño son propios: la mecánica es libre, pero el nombre original es una marca registrada.

### Hombres Lobo (el móvil hace de narrador)
- Reparte roles en secreto pasando el móvil, como el Impostor: lobos, aldeanos, vidente y bruja (se empieza
  con pocos roles).
- **Noche:** todos cierran los ojos y el móvil va narrando por pantalla, con sonido opcional. Los lobos tocan
  a su víctima y la vidente investiga.
- **Día:** el móvil anuncia quién ha muerto, hay un tiempo de debate y se vota tocando.
- **Por qué encaja:** los roles ocultos son lo que mejor hace ya la app, y quita la necesidad de un
  moderador humano.
- Es el más grande de los cinco.

**Mi orden recomendado:** primero Verdad o Reto (poco esfuerzo y completa el menú), después ¿Quién soy? y,
como proyecto grande, Hombres Lobo.

**En el banquillo** (ideas más pequeñas por si alguna te gusta más):
- **¿Quién es más probable que…?** Sale una frase y, a la de tres, todos señalan a alguien. (S)
- **Mente colmena.** Sale una categoría y gana quien coincida con la mayoría. (S)
- **Cadena de palabras.** Cada palabra empieza por la última sílaba de la anterior, con la mecha de La
  Bomba. (S)

---

## 4. Diez juegos nuevos para elegir cinco (V059r, 2026-10-25)

El autor pidió unos 10 juegos nuevos de fiesta para elegir 5 (con todo lo demás ya probado y funcionando). Incluye
los 5 de la sección 3 y 5 nuevos, varios como variantes de los que ya hay. Todos: un solo móvil, sin internet,
pasándolo o en el centro de la mesa, estética neón, y los que tienen nivel +18 con el mismo aviso y confirmación
de edad que Yo Nunca.

| # | Juego | Jugadores | Cómo se juega | +18 | Reutiliza | Esfuerzo |
|---|---|---|---|---|---|---|
| 1 | **Verdad o Reto** ★ | 2–15 | Por turnos, cada uno elige verdad o reto y sale una tarjeta; quien se raja, bebe o paga prenda | Sí | Yo Nunca: niveles, frases propias, aviso +18 | **S** |
| 2 | **¿Quién es más probable que…?** ★ | 3–15 | Sale una frase («…acabe en comisaría»); a la de tres todos señalan; el más señalado bebe | Sí | Yo Nunca casi entero | **S** |
| 3 | **¿Qué prefieres?** | 2–15 | Dos opciones imposibles; cada uno vota con la mano; la minoría bebe | Sí | Yo Nunca | **S** |
| 4 | **Mímica por equipos** | 4+ en 2 equipos | Como el Tabú pero sin hablar: películas, acciones, famosos… contra reloj | Sí (categoría) | Tabú: equipos, tiempo, marcador, categorías y tarjetas propias | **S–M** |
| 5 | **¿Quién soy?** (en la frente) ★ | 4+ en equipos | El móvil en la frente; tu equipo describe; inclinas abajo si aciertas y arriba si pasas | Sí (categoría) | Tabú (tiempo, equipos), palabras del Impostor | **M** |
| 6 | **El Espía** (variante del Impostor) ★ | 4–12 | Todos saben el lugar y su papel menos el espía; preguntas por turnos durante 8 min; el espía intenta adivinar el lugar | No | Impostor: pasar, revelar, votación, historial | **M** |
| 7 | **Dibuja y adivina** | 4+ en equipos | El que dibuja ve una palabra y la dibuja con el dedo; su equipo tiene 60 s | Sí (categoría) | Tabú (tiempo, equipos), palabras del Impostor | **M** |
| 8 | **Duelo de reflejos** (pantalla partida) | 2–4 a la vez | El móvil en el centro, cada uno en su esquina: pulsar el primero cuando salga el color, no pulsar antes… el que pierde, bebe | No | Sonidos, vibración | **M** |
| 9 | **Código secreto** | 4–10 en 2 equipos | Rejilla de 5×5 palabras; el jefe de cada equipo ve la clave y da una pista y un número | No | Palabras del Impostor | **M–L** |
| 10 | **Hombres Lobo** (el móvil narra) ★ | 6–15 | Roles secretos pasando el móvil; de noche el móvil narra, de día se debate y se vota | No | Impostor: pasar, revelar, votación | **L** |

★ = los 5 que recomiendo: los dos baratos que más juego dan en una fiesta (1 y 2), el que usa el móvil como pieza
(5), la variante del juego que más gusta (6) y el grande de roles ocultos (10).

**Detalle de los nuevos de esta ronda** (los de la sección 3 siguen igual):
- **¿Quién es más probable que…?** Sin turnos: sale la frase, cuenta atrás de 3 s y todos señalan a la vez. El
  móvil no cuenta votos (se ve en la mesa); solo saca frases sin repetir, con nivel suave y +18 y frases propias.
- **¿Qué prefieres?** Dos tarjetas grandes, A y B, y «Votad»; se puede marcar cuántos eligieron cada una para ver
  el porcentaje del grupo al final. Banco de preguntas suaves y +18.
- **Mímica por equipos.** El Tabú sin prohibidas: la palabra a hacer con mímica y Acierto / Pasar. Categorías:
  películas y series, acciones, famosos, animales, oficios, Cachondeo +18.
- **El Espía.** Banco de lugares (playa, discoteca, hospital, nave espacial…) con papeles (socorrista, DJ,
  enfermera…). Reloj de 8 min visible; en cualquier momento se puede parar para votar al espía. Si el espía acierta
  el lugar, gana él.
- **Duelo de reflejos.** Minijuegos cortos con la pantalla dividida en 2–4 zonas: «pulsa cuando sea verde»,
  «¿coinciden el color y la palabra?», «toca más rápido». Quien pulsa antes de tiempo pierde la ronda.

**Para elegir:** di los 5 números (p. ej. «1, 2, 5, 6 y 10») y se convierten en versiones de `VERSIONS.md`.
