# CLAUDE.md — Reglas del proyecto Sajitarios Gamespot

Reglas obligatorias para cualquier IA o persona que trabaje en este repo. Si una regla choca con una petición, pregunta antes de saltártela.

**Qué es:** hub **offline** de minijuegos tipo *party games* para móvil (Android + iOS): un menú desde el que se elige el juego y se juega en un solo dispositivo pasándose el móvil. Siete juegos: Es un 10 pero, El Impostor, Preguntas por puntos, Wavelength, Tabú, Yo Nunca y La Bomba.

**Estado:** migración de la app Flutter (v0.75, ya terminada, en `Legacy/Flutter/`) a **Unity**. La hoja de ruta está en `VERSIONS.md`.

| Qué | Dónde |
|---|---|
| Proyecto Unity | `SajitariosGamespot/` (Unity **6000.3.27f1**, URP, uGUI + TextMeshPro) |
| Código del juego | `SajitariosGamespot/Assets/_Gamespot/` (estructura en `CONTEXT.md` §4) |
| App Flutter original (solo lectura, referencia del port) | `Legacy/Flutter/` (código en `lib/`, tests en `test/`, docs antiguos en `docs/`) |
| Contexto, especificación de los juegos, decisiones y registro de trabajo | `CONTEXT.md` |
| Gustos y forma de trabajar del autor | `LEARNINGS.md` |
| Árbol de carpetas | `TREE.md` (se regenera con `tools/Update-Tree.ps1`) |
| Hoja de ruta (V001–V069) | `VERSIONS.md` |
| Funciones para la CLI de Unity | `tools/UnityCli.ps1` (ver §3) |
| Capturas del Game view | `tools/Capture-GameView.ps1 -Name Vxxx_algo` → `Docs/Capturas/` |

---

## 0. Antes de empezar cualquier tarea
1. Lee `CONTEXT.md` (qué es, decisiones y estado) y `LEARNINGS.md` (gustos del autor).
2. De `VERSIONS.md`, lee **solo** la versión en curso y su criterio de aceptación.
3. Usa `TREE.md` para localizar archivos en vez de explorar el disco.
4. `git status`: nunca trabajes directamente en `main`. La rama de la migración es `feature/unity-port`.
5. Si vas a portar algo, busca primero su original en `Legacy/Flutter/lib/` y sus tests en `Legacy/Flutter/test/`: son la especificación.

## 1. Los archivos .md (mantenerlos al día es parte de cada tarea)
| Archivo | Contenido | Cuándo se actualiza |
|---|---|---|
| `CLAUDE.md` | Estas reglas | Cuando el autor fije una regla nueva |
| `CONTEXT.md` | Todo el contexto: qué es, intención original, especificación de cada juego, arquitectura, decisiones, licencias, deuda y registro de trabajo | Al terminar cada versión (cabecera + entrada nueva arriba del registro) y cuando cambie una decisión o una dependencia |
| `LEARNINGS.md` | Gustos y preferencias del autor, con fecha | Cada vez que exprese un gusto, una corrección o una preferencia |
| `TREE.md` | **Solo** el árbol de carpetas y archivos | Cuando se cree, mueva o borre algo: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Update-Tree.ps1` |
| `VERSIONS.md` | Fases, versiones y criterio de aceptación de cada una | Al empezar y al terminar cada versión (`[ ]` → `[x]`) |

No crees otros .md de documentación sin que el autor lo pida (los `SKILL.md` y el `README.md` sí están permitidos).

## 2. Git
- Rama de trabajo: `feature/unity-port`. Nunca commits directos en `main`.
- **Un commit por versión terminada**, en español y con prefijo convencional: `feat:`, `fix:`, `docs:`, `chore:`, `refactor:`, `test:` o `build:`. Ejemplo: `feat(impostor): asignación de roles 10/10/80 (V025)`.
- **Pide permiso al autor antes de hacer `push`, de fusionar en `main` o de borrar `Legacy/Flutter/`.**
- Nunca `--no-verify`, `push --force` ni reescribir historia.
- Binarios nuevos por **Git LFS** según `.gitattributes`. Las carpetas vacías llevan `.gitkeep`.
- Nunca entran al repo: `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`, `Builds/`, `.apk`, `.aab`, proyectos de Xcode generados.

## 3. Cómo se trabaja con Unity
- **Primero**, el Editor abierto con el proyecto `SajitariosGamespot/` y `U editor status` en estado `ready`.
- En cada llamada de PowerShell se carga `. tools/UnityCli.ps1`. Da `U <grupo> <nombre> …` (comando de la CLI → `data.result`), `E <snippet.cs>` (C# en vivo con `script eval_file`), `Sync-Unity` (Refresh + recompilar + esperar), `Get-UnityConsole`, `Invoke-UnityTests EditMode|PlayMode`, `Start-UnityPlay`, `Stop-UnityPlay` y `Test-UnityPlay`.
  - El ejecutable es `%LOCALAPPDATA%\Unity\bin\unity.exe`; la lista de comandos sale con `unity command --json --detail compact --limit 500`.
  - C# en vivo: el código va en un `.cs` del scratchpad y se manda con `E`. Con `script eval --code` PowerShell 5.1 rompe las comillas.
  - Argumentos con JSON (`settings.* set --settings '{…}'`) desde **Bash**: PowerShell 5.1 rompe las comillas. Primero `--dry_run true`, después `--confirm true`.
- **Nunca** editar a mano `.unity`, `.prefab` ni `.asset` con el Editor abierto: se usan los comandos de la CLI o código del Editor. Los `.cs`, `.asmdef` y `.json` sí se escriben directamente; después, `Sync-Unity` y consola sin errores.
- **Ajustes del proyecto** cambiados por API solo cambian en memoria: `EditorUtility.SetDirty(asset)` + `AssetDatabase.SaveAssets()` y comprobar con `git diff` que están en disco. `SaveAssets` guarda todo lo pendiente: revisar `git status` después. El control de versiones tiene que seguir en *Visible Meta Files*.
- **Paquetes** (`package add/remove`): la recarga de dominio corta el comando y la CLI da «Network error». Es lo esperado: se comprueba `Packages/manifest.json` y se limpia la consola.
- **Si no conecta**, puede ser Safe Mode por errores de compilación: corregir los scripts y reiniciar Unity.
- **Abrir el Editor desde Unity Hub** (o con `Unity.exe -projectPath`), no con `unity open`: este deja un auxiliar `unity.exe --internal-identity-serve` que puede dejar de responder, y entonces el Editor se cuelga en la siguiente recarga de dominio (el log se llena de `IPCStream (hubIPCService): IPC stream failed to write (Timed out)` y la CLI deja de verlo). Si pasa: cerrar ese auxiliar; el Editor termina la recarga solo al cabo de unos minutos (pasó el 2026-10-25).
- **Color lineal:** el proyecto está en espacio de color lineal y la UI mezcla las transparencias en lineal. Los alfas del Flutter (que mezcla en sRGB) salen más brillantes: los velos y brillos se ajustan mirando capturas.
- **Play con el Editor sin foco:** entrar con `Start-UnityPlay` (pone `runInBackground`) y salir siempre con `Stop-UnityPlay`, que lo restaura. Comprobar después `git diff ProjectSettings/`.
- **Recarga de dominio desactivada al entrar en Play** (Unity 6 por defecto): todo estado estático se reinicia con `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`.
- **Tests:** EditMode con `Invoke-UnityTests EditMode`; PlayMode con `Invoke-UnityTests PlayMode` (asíncrono, esperar a `completed`). Los tests que tocan datos guardados (BD, PlayerPrefs) usan una BD temporal o restauran lo que había.
- **Capturas:** nunca con `--save_path` (Unity lo importaría dentro de `Assets/`). Se usa `tools/Capture-GameView.ps1`.
- **Builds:** `build run --target Android --outputPath Builds/Android/... --confirm true` y esperar a `build status` = `completed`. iOS genera el proyecto de Xcode (compilar y firmar requiere un Mac).
- **Cerrar una versión:** recompilación sin errores, consola limpia, tests verdes, captura en Play si la versión tiene pantalla, y el criterio de aceptación de `VERSIONS.md` cumplido.
- Antes de quitar paquetes o hacer algo destructivo en el proyecto, preguntar al autor.

## 4. Arquitectura: reglas de código
- **Screaming Architecture.** Cada juego es un *bounded context* aislado en `Assets/_Gamespot/Scripts/Games/<Juego>/{Domain,Data,Presentation}` con sus propios asmdef. Lo compartido va en `Core/`, `Core.Runtime/` o `Games/_Shared/`. Un juego **nunca** referencia a otro.
- **Regla de oro:** añadir un juego = crear `Games/<Juego>/` + registrarlo en `App/GameRegistry.cs`. **El `MenuScreen` y el `Router` nunca conocen juegos concretos**; solo reciben la lista de `IGameDescriptor`. El esquema de BD de cada juego vive en su juego (hooks del descriptor), no en `Core`.
- **Dominio puro:** los asmdef `*.Domain` llevan `noEngineReferences: true` (nada de `UnityEngine`, ni de BD, ni de UI). Use cases pequeños y testeables en EditMode.
- **Aleatoriedad** siempre vía `IRandomProvider` inyectado (nunca `System.Random` ni `UnityEngine.Random` directos en el dominio), para poder fijarla en los tests. El orden de las llamadas al random forma parte de la especificación (ver `CONTEXT.md`).
- **Textos:** ningún texto de UI fijo en el código; todo pasa por `Loc.T("clave")` (`Resources/Localization/es.json` y `en.json`, mismas claves en los dos). Español por defecto.
- **Sin emojis** en código ni en textos: solo iconos (fuente de iconos o sprites propios).
- **Colores** solo desde `Palette` (la paleta neón de `CONTEXT.md` §5); nada de valores sueltos.
- **UI:** uGUI + TextMeshPro construida por código con el kit neón de `Core.Runtime/UI` (como los widgets de Flutter). Canvas en *Screen Space – Camera* con Bloom de URP.
- **Datos:** SQLite con sqlite-net (`gilzoide/unity-sqlite-net`). Consultas siempre parametrizadas. Las filas de seed (`is_seed = 1`) son de solo lectura.
- **Estilo:** identificadores en **inglés** (clases, métodos, archivos); **comentarios en español**. Un tipo principal por archivo (sus enums y ayudas pequeñas pueden ir con él). Los nombres del dominio que vienen del Flutter se conservan en español (`Iniciar`, `Votar`, `Rondas`…) para que el port se pueda comparar línea a línea.
- **Lógica de partida pura:** los *flow controllers* (fases y acciones de cada juego) viven en `Domain/Flow/`, sin Unity, y se prueban como el dominio. Las pantallas solo los leen y les llaman.
- **Comprobación fuera de Unity:** el código puro (Core + `Games/*/Domain`) también compila y pasa sus tests con el SDK de .NET (netstandard2.1, C# 9) usando el `nunit.framework.dll` de Unity; sirve cuando el Editor no está disponible, pero no sustituye a la tanda de Unity.

## 5. Reglas críticas del Impostor (NO improvisar)
`AssignRolesUseCase` encapsula las reglas probabilísticas, el núcleo testeado del proyecto. Orden exacto de llamadas al random: primero se elige la palabra (`Pick`), después **un solo `NextDouble()`** decide el caso:
- `< 0.10` → **TODOS** impostores.
- `< 0.20` → **NINGUNO** impostor.
- resto (80 %) → asignación normal: se baraja una copia de los jugadores (Fisher-Yates) y los primeros `nImpostores` son impostores.

Otras reglas: jugadores mín. 3 / máx. 15; impostores 1–5 capado a `jugadores − 1`; **toda palabra lleva pista obligatoria**; el **orden de revelación = orden de introducción de los nombres** (se baraja para asignar roles, NO para revelar); las palabras seed son de solo lectura y las del usuario, editables y borrables; la unicidad de palabras no distingue mayúsculas. La distribución 10/10/80 se valida con 10.000 iteraciones y seed fija (±2 % / ±2 % / ±3 %): no cambies esos tests sin entender por qué.

## 6. Skills por ámbito
Las skills del plugin de Unity ya están disponibles en la sesión. Antes de un ámbito nuevo, elige la skill y anótala en `CONTEXT.md`.
| Ámbito | Skill |
|---|---|
| Crear el proyecto | `unity:new-unity-project` |
| CLI de Unity | `unity:unity-cli` |
| UI (uGUI) | `unity:ui-ugui`, `unity:ui` |
| Texto (TextMeshPro) | `unity:optimize-text-mesh-pro` |
| Paquetes | `unity:unity-package-management` |
| Post-procesado (Bloom) | `unity:urp-postprocessing` |
| Audio | `unity:optimize-audio`, `unity:audio-setup-mixers` |
| Revisión de código | `code-review`, `simplify` |

Antes de instalar una skill o un paquete de terceros: revisar su código, autor, estrellas y licencia, y anotarlo en el registro de licencias de `CONTEXT.md`.

## 7. Comandos habituales
```powershell
. tools/UnityCli.ps1
U editor status
Sync-Unity
Get-UnityConsole -Tail 10
Invoke-UnityTests EditMode
Invoke-UnityTests PlayMode
Start-UnityPlay ; Stop-UnityPlay
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Update-Tree.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Capture-GameView.ps1 -Name V020_carta
```

## 8. Comunicación con el autor
- Siempre en **español**.
- Ante decisiones de diseño, de alcance o de dependencias nuevas: pregunta con opciones y recomienda una. Si el autor dice que sigas sin preguntar, elige la opción recomendada y anótala como «elegida por Claude, revisable».
- Al terminar cada versión, di qué queda por probar a mano (se apunta en la cabecera de `CONTEXT.md`).
