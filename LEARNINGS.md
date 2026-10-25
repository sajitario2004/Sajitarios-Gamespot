# LEARNINGS.md — gustos y forma de trabajar del autor

Archivo vivo. Añade una línea cada vez que el autor exprese un gusto, una corrección o una preferencia, con la fecha. Lo más reciente va al final de cada sección.

## Forma de trabajar
- (2026-10-25) Trabaja igual en todos sus proyectos con `CLAUDE.md` (reglas), `CONTEXT.md` (todo el contexto), `LEARNINGS.md` (cómo trabaja el autor) y `TREE.md` (el árbol de carpetas, como el comando `tree`). En sus otros proyectos añade una hoja de ruta (`VERSIONS.md` en AZÚCAR, `ROADMAP.md` en SSG); aquí es `VERSIONS.md`.
- (2026-10-25) Quiere que una IA nueva pueda retomar el trabajo solo leyendo esos archivos.
- (2026-10-25) Le gusta planificar antes de construir (pidió `/plan`) y que se le pregunte con opciones antes de las decisiones grandes. En las cuatro preguntas de la migración eligió **todas las opciones recomendadas**.
- (2026-10-25) Avisa de que otra IA ya tocó el repo y dejó sus propios `.md`: hay que analizarlo todo antes de decidir, y no fiarse de los docs viejos sin comprobar el código (el `README.md` heredado decía v0.30 y la app estaba en v0.75).
- (2026-10-25) Git en este proyecto: rama de trabajo, commit por versión terminada, y permiso antes de `push` o de fusionar en `main`.
- (2026-10-25) Al terminar de portar todos los juegos quiere **un plan para mejorarlos** y **unas 5 propuestas de juegos nuevos** para elegir (VERSIONS.md, fase 10b).
- (2026-10-25) Recuerda que **Wavelength es un juego roto** en la app Flutter: hay que arreglarlo de verdad, no portar sus fallos.
- (2026-10-25) No contestó a la pregunta de descargar fuentes de iconos y texto: sin respuesta, no se descarga nada y se siguen usando los iconos provisionales.
- (2026-10-25) Del plan de mejoras eligió **exactamente el orden recomendado** (C2, C3+L1, B1+B2, C4, I1/T1/W2) y repitió «sin descargar fuentes todavía»: las cosas que necesitan descargas se dejan para cuando lo diga expresamente; mientras, se hace con recursos propios (los sonidos se sintetizaron en el Editor).

## Producto
- (2026-10-25) Lleva tiempo intentando hacer este juego. Pide convertir **todo el proyecto a Unity**, con los 7 juegos de la app Flutter.
- (heredado del Flutter, 2026) App **offline y local**, Android + iOS, persistencia con **SQLite**, fácil de ampliar con juegos nuevos.
- (heredado) Las reglas del Impostor son suyas y exactas (10 % todos impostores, 10 % ninguno, pista obligatoria, orden de revelación = orden de los nombres): no se improvisan.
- (heredado) Quiere bancos de contenido grandes: 2000+ palabras del Impostor y 1000+ preguntas de trivia.
- (heredado) Yo Nunca con nivel picante **+18 explícito**, con aviso claro de consentimiento entre adultos.
- (heredado) Cada juego explica sus reglas desde su setup ("¿Cómo se juega?") y todas las pantallas tienen un botón para volver al menú (no quiere quedarse "atrapado" en un juego).

- (2026-10-25) Quiere contenido **de España y de fiesta**: políticos, streamers, creadoras y creadores, juegos famosos, tipos de alcohol, personas y sitios famosos y «cosas graciosas de fiesta», tanto en el Impostor como en el Tabú. Pide los bancos grandes y con humor de grupo de amigos.

- (2026-10-25) Prefiere palabras **fáciles y conocidas para jugar en grupo**: fuera los términos raros («feudo») y los famosos que un grupo joven no conoce («Andreu» Buenafuente, Eugenio). Y quiere **elegir categorías** en el Impostor y el Tabú.

- (2026-10-25) Quiere una categoría **«Cachondeo»** con humor de fiesta muy bruto (política, porno, sexo, memes) en el Impostor y el Tabú. Se hace +18, desmarcada por defecto y con aviso; no se meten chistes sobre menores ni sobre grupos de personas (se le explicó). Prefiere decidir en persona qué palabras raras se quitan: hay que enseñarle solo las dudosas, no la lista entera.

- (2026-10-25) Quiere **conservar el icono de la app Flutter** (carta blanca con pica naranja sobre morado), aunque no sea de la paleta neón.

- (2026-10-25) Prueba la app **en su móvil** y manda capturas de lo que falla. Quiere las **descripciones del menú cortas y con la letra más pequeña**, y que se compruebe que **no se corta ningún texto** en ninguna pantalla.

- (2026-10-25) Quiere la app **lo más personalizable posible**, empezando por el Impostor y el Tabú: crear sus propias categorías, elegir la categoría antes de empezar y crear sus propias palabras y tarjetas.

- (2026-10-25) Los sonidos le parecían **demasiado fuertes**: prefiere efectos a un volumen moderado y parejo. Oye el Editor mientras Claude trabaja (los tests ya no suenan). Tras nivelarlos aún los quería **a la mitad**: mejor quedarse corto de volumen que pasarse.

- (2026-10-25) Dio permiso para descargar las fuentes y eligió **las mismas de la app Flutter** (Roboto y Material Icons) frente a otras nuevas. Quiere que se termine todo lo que se pueda hacer sin él, fase a fase, y un APK al final para probar.

- (2026-10-25) Quiere un **apartado de Ajustes** como el de cualquier app: quitar el sonido y la vibración y ver términos y condiciones, privacidad y cookies.

## Diseño
- (heredado) Estética **neón cyberpunk**: fondo casi negro, cian y magenta con brillo, rejilla de fondo.
- (heredado) **Nada de emojis**, ni en el código ni en los textos: solo iconos.
- (heredado) Toda la UI en **español** (con inglés como segundo idioma) y sin textos cortados en ninguna pantalla, también con letra grande.
- (heredado) Trivia: caja de la pregunta en morado neón y las 4 respuestas con marcos azul, verde, rojo y amarillo.

## Herramientas que ya usa
- (2026-10-25) Trabaja con Claude conectado a **Unity** por la CLI (`unity command …`), como en su proyecto AZÚCAR (`C:\Users\nacha\Documents\DEV\Unity\AzucarClaudio`), de donde se reutilizan los scripts de `tools/`.
- (2026-10-25) Tiene instalados Unity 6000.3.27f1 (con Android e iOS) y 6000.5.1f1, Unity Hub, git-lfs y GitHub CLI.
