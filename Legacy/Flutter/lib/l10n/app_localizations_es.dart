// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for Spanish Castilian (`es`).
class AppLocalizationsEs extends AppLocalizations {
  AppLocalizationsEs([String locale = 'es']) : super(locale);

  @override
  String get appTitle => 'Sajitarios Gamespot';

  @override
  String get idioma => 'Idioma';

  @override
  String get idiomaDelSistema => 'Idioma del sistema';

  @override
  String get espanol => 'Español';

  @override
  String get ingles => 'Inglés';

  @override
  String get cambiarIdioma => 'Cambiar idioma';

  @override
  String get aceptar => 'Aceptar';

  @override
  String get cancelar => 'Cancelar';

  @override
  String get menuVacioTitulo => 'Todavía no hay juegos';

  @override
  String get menuVacioMensaje =>
      'Pronto podrás elegir entre varios juegos para jugar en grupo.';

  @override
  String jugarA(String titulo, String descripcion) {
    return 'Jugar a $titulo. $descripcion';
  }

  @override
  String get rutaNoEncontradaTitulo => 'Pantalla no encontrada';

  @override
  String get rutaNoEncontradaMensaje =>
      'No encontramos la pantalla que buscabas.';

  @override
  String get rutaNoEncontradaAyuda => 'Vuelve al menú para seguir jugando.';

  @override
  String get volverAlMenu => 'Volver al menú';

  @override
  String get esUn10PeroTitulo => 'Es un 10 pero';

  @override
  String get sacarCarta => 'Sacar carta';

  @override
  String get sacarOtraCarta => 'Sacar otra carta';

  @override
  String get pistaCartaVacia => 'Pulsa \"Sacar carta\"\npara revelar una carta';

  @override
  String get cartaSinSacarSemantica => 'Sin carta, pulsa Sacar carta';

  @override
  String cartaSemantica(String valor, String palo) {
    return 'Carta: $valor de $palo';
  }

  @override
  String get impostorTitulo => 'El Impostor';

  @override
  String get impostorMenuTitulo => 'El Impostor';

  @override
  String get impostorMenuDescripcion =>
      'Todos conocen la palabra... menos los impostores. ¿Los descubrirás?';

  @override
  String get esUn10PeroMenuTitulo => 'Es un 10 pero';

  @override
  String get esUn10PeroMenuDescripcion =>
      'Saca una carta al azar de la A al 10.';

  @override
  String get historial => 'Historial';

  @override
  String get gestionarPalabras => 'Gestionar palabras';

  @override
  String get setupJugadores => 'Jugadores';

  @override
  String setupRangoJugadores(int min, int max) {
    return 'Introduce de $min a $max jugadores. El orden es el orden de revelación.';
  }

  @override
  String get anadirJugador => 'Añadir jugador';

  @override
  String get maximoJugadoresAlcanzado => 'Máximo de jugadores alcanzado';

  @override
  String get setupImpostores => 'Impostores';

  @override
  String get setupPista => 'Pista';

  @override
  String get iniciandoPartida => 'Iniciando...';

  @override
  String get empezarPartida => 'Empezar partida';

  @override
  String jugadorNumero(int n) {
    return 'Jugador $n';
  }

  @override
  String nombreDelJugadorNumero(int n) {
    return 'Nombre del jugador $n';
  }

  @override
  String get quitarJugador => 'Quitar jugador';

  @override
  String impostoresContador(int n) {
    String _temp0 = intl.Intl.pluralLogic(
      n,
      locale: localeName,
      other: '$n impostores',
      one: '$n impostor',
    );
    return '$_temp0';
  }

  @override
  String maximoImpostoresPara(int max, int jugadores) {
    return 'Máximo $max para $jugadores jugadores.';
  }

  @override
  String get menosImpostores => 'Menos impostores';

  @override
  String get masImpostores => 'Más impostores';

  @override
  String get darPistaAlImpostor => 'Dar pista al impostor';

  @override
  String get darPistaAlImpostorSubtitulo =>
      'Si está activa, el impostor verá una pista sobre la palabra.';

  @override
  String errorPocosJugadores(int min) {
    return 'Se necesitan al menos $min jugadores.';
  }

  @override
  String errorDemasiadosJugadores(int max) {
    return 'El máximo es de $max jugadores.';
  }

  @override
  String get errorNombresDuplicados => 'Hay nombres de jugador repetidos.';

  @override
  String get errorNombreVacio => 'Todos los jugadores deben tener un nombre.';

  @override
  String get errorNoSePudoIniciar => 'No se pudo iniciar la partida.';

  @override
  String get noHayPalabrasTitulo => 'No hay palabras';

  @override
  String get noHayPalabrasMensaje =>
      'Necesitas al menos una palabra para jugar. Añade tu primera palabra desde la gestión de palabras.';

  @override
  String get ahoraNo => 'Ahora no';

  @override
  String get pasaleElMovilA => 'Pásale el móvil a';

  @override
  String jugadorXDeY(int posicion, int total) {
    return 'Jugador $posicion de $total';
  }

  @override
  String get pasaleElMovilAyuda =>
      'Cuando lo tenga en sus manos, pulsa continuar y revela tu rol en privado.';

  @override
  String get continuar => 'Continuar';

  @override
  String get esElTurnoDe => 'Es el turno de';

  @override
  String get pulsaRevelar =>
      'Pulsa \"Revelar\" cuando seas tú quien mira la pantalla.';

  @override
  String get revelar => 'Revelar';

  @override
  String get ocultarYPasar => 'Ocultar y pasar';

  @override
  String get ocultarYVerResultados => 'Ocultar y ver resultados';

  @override
  String get impostorMayus => 'IMPOSTOR';

  @override
  String get tuPalabraEs => 'Tu palabra es';

  @override
  String get noHayPartidaEnCurso => 'No hay ninguna partida en curso.';

  @override
  String get configurarPartida => 'Configurar partida';

  @override
  String get tuRolEsImpostor => 'Tu rol es IMPOSTOR';

  @override
  String tuRolEsImpostorConPista(String pista) {
    return 'Tu rol es IMPOSTOR. Pista: $pista';
  }

  @override
  String tuPalabraEsAnuncio(String palabra) {
    return 'Tu palabra es $palabra';
  }

  @override
  String get salirDeLaPartidaTitulo => '¿Salir de la partida?';

  @override
  String get salirDeLaPartidaMensaje =>
      'Si sales ahora se perderá la partida actual y tendrás que configurarla de nuevo.';

  @override
  String get seguirJugando => 'Seguir jugando';

  @override
  String get salir => 'Salir';

  @override
  String get resultadoDeLaPartida => 'Resultado de la partida';

  @override
  String get noHayPartidaQueMostrar => 'No hay ninguna partida que mostrar.';

  @override
  String get laPalabraEra => 'La palabra era';

  @override
  String pistaConValor(String pista) {
    return 'Pista: $pista';
  }

  @override
  String resumenImpostores(int count) {
    String _temp0 = intl.Intl.pluralLogic(
      count,
      locale: localeName,
      other: 'Había $count impostores.',
      one: 'Había 1 impostor.',
      zero: 'No había ningún impostor: todos sabían la palabra.',
    );
    return '$_temp0';
  }

  @override
  String get sabiaLaPalabra => 'Sabía la palabra';

  @override
  String jugadorEraImpostor(String nombre) {
    return '$nombre: era impostor';
  }

  @override
  String jugadorSabiaLaPalabra(String nombre) {
    return '$nombre: sabía la palabra';
  }

  @override
  String get jugarOtra => 'Jugar otra';

  @override
  String get gestionarPalabrasTitulo => 'Gestionar palabras';

  @override
  String get agregar => 'Agregar';

  @override
  String get buscarPalabra => 'Buscar palabra';

  @override
  String get limpiarBusqueda => 'Limpiar búsqueda';

  @override
  String get noSePudieronCargarPalabras =>
      'No se pudieron cargar las palabras.';

  @override
  String get sinPalabrasAun =>
      'Aún no hay palabras. Añade la primera con el botón \"Agregar\".';

  @override
  String get sinCoincidencias =>
      'No hay palabras que coincidan con tu búsqueda.';

  @override
  String get palabraAnadida => 'Palabra añadida.';

  @override
  String get yaExisteEsaPalabra => 'Ya existe esa palabra.';

  @override
  String get palabraYPistaObligatorias =>
      'La palabra y la pista son obligatorias.';

  @override
  String get palabraActualizada => 'Palabra actualizada.';

  @override
  String get palabrasPredefinidasSoloLectura =>
      'Las palabras predefinidas son de solo lectura.';

  @override
  String get esaPalabraYaNoExiste => 'Esa palabra ya no existe.';

  @override
  String get palabraBorrada => 'Palabra borrada.';

  @override
  String get borrarPalabraTitulo => 'Borrar palabra';

  @override
  String borrarPalabraMensaje(String palabra) {
    return '¿Seguro que quieres borrar \"$palabra\"?';
  }

  @override
  String get borrar => 'Borrar';

  @override
  String get predefinida => 'Predefinida';

  @override
  String get palabrasPredefinidasNoEditar =>
      'Las palabras predefinidas no se pueden editar';

  @override
  String get editar => 'Editar';

  @override
  String get palabrasPredefinidasNoBorrar =>
      'Las palabras predefinidas no se pueden borrar';

  @override
  String get editarPalabra => 'Editar palabra';

  @override
  String get nuevaPalabra => 'Nueva palabra';

  @override
  String get campoPalabra => 'Palabra';

  @override
  String get campoPalabraHint => 'Ej.: pirata';

  @override
  String get campoPista => 'Pista';

  @override
  String get campoPistaHint => 'Ej.: barco';

  @override
  String get campoObligatorio => 'Este campo es obligatorio.';

  @override
  String get guardar => 'Guardar';

  @override
  String get borrarHistorialTitulo => 'Borrar historial';

  @override
  String get borrarHistorialMensaje =>
      'Se borrarán todas las partidas guardadas. Esta acción no se puede deshacer.';

  @override
  String get borrarTodo => 'Borrar todo';

  @override
  String get historialBorrado => 'Historial borrado.';

  @override
  String get noSePudoBorrarHistorial => 'No se pudo borrar el historial.';

  @override
  String get noSePudoCargarHistorial => 'No se pudo cargar el historial.';

  @override
  String get reintentar => 'Reintentar';

  @override
  String get historialVacioTitulo => 'Todavía no hay partidas guardadas.';

  @override
  String get historialVacioMensaje =>
      'Cuando termines una partida del Impostor aparecerá aquí.';

  @override
  String get partidas => 'Partidas';

  @override
  String get estadisticas => 'Estadísticas';

  @override
  String get partidasJugadas => 'Partidas jugadas';

  @override
  String get palabraMasRepetida => 'Palabra más repetida';

  @override
  String palabraMasRepetidaValor(String palabra, int veces) {
    return '$palabra ($veces)';
  }

  @override
  String get vecesQueFueImpostor => 'Veces que cada jugador fue impostor';

  @override
  String get nadieFueImpostor => 'Nadie ha sido impostor todavía.';

  @override
  String partidaSubtitulo(String fecha, int jugadores, String impostores) {
    return '$fecha · $jugadores jugadores · $impostores';
  }

  @override
  String impostoresTexto(int n) {
    String _temp0 = intl.Intl.pluralLogic(
      n,
      locale: localeName,
      other: '$n impostores',
      one: '1 impostor',
    );
    return '$_temp0';
  }

  @override
  String pistaActivadaConValor(String pista) {
    return 'Pista activada: $pista';
  }

  @override
  String get pistaActivada => 'Pista activada';

  @override
  String get pistaDesactivada => 'Pista desactivada';

  @override
  String get silenciarSonido => 'Silenciar sonido';

  @override
  String get activarSonido => 'Activar sonido';

  @override
  String get setupRondas => 'Rondas';

  @override
  String setupRondasAyuda(int min, int max) {
    return 'Oportunidades de voto para expulsar a los impostores. De $min a $max con estos jugadores.';
  }

  @override
  String rondasContador(int n) {
    String _temp0 = intl.Intl.pluralLogic(
      n,
      locale: localeName,
      other: '$n rondas',
      one: '$n ronda',
    );
    return '$_temp0';
  }

  @override
  String get menosRondas => 'Menos rondas';

  @override
  String get masRondas => 'Más rondas';

  @override
  String get votacionTitulo => 'Votación';

  @override
  String get votacionInstruccion => 'Votad a quien creáis impostor';

  @override
  String votacionRondaXDeY(int actual, int total) {
    return 'Ronda $actual de $total';
  }

  @override
  String get votacionExpulsar => 'Expulsar';

  @override
  String votacionConfirmarExpulsion(String nombre) {
    return '¿Expulsar a $nombre?';
  }

  @override
  String get votacionImpostorSigue => 'El impostor sigue entre vosotros';

  @override
  String get votacionJugadoresGanan => '¡Habéis ganado!';

  @override
  String get finDePartidaTitulo => 'Fin de la partida';

  @override
  String get sacandoCartaEn => 'Sacando carta en...';

  @override
  String get triviaTitulo => 'Preguntas por puntos';

  @override
  String get triviaSetupTematicas => 'Temáticas';

  @override
  String get triviaSetupTematicasAyuda =>
      'Elige al menos una temática para la partida.';

  @override
  String triviaSetupRangoJugadores(int min, int max) {
    return 'Introduce de $min a $max jugadores.';
  }

  @override
  String get triviaEmpezarPartida => 'Empezar partida';

  @override
  String get triviaIniciando => 'Iniciando...';

  @override
  String get triviaNoHayPreguntasTitulo => 'Sin preguntas';

  @override
  String get triviaNoHayPreguntasMensaje =>
      'No hay suficientes preguntas para las temáticas y jugadores elegidos. Prueba con más temáticas.';

  @override
  String get triviaRankingVictorias => 'Ranking de victorias';

  @override
  String get triviaSinVictorias => 'Aún no hay victorias registradas.';

  @override
  String triviaVictorias(int n) {
    String _temp0 = intl.Intl.pluralLogic(
      n,
      locale: localeName,
      other: '$n victorias',
      one: '$n victoria',
    );
    return '$_temp0';
  }

  @override
  String get triviaAtribucion =>
      'Preguntas basadas en Open Trivia DB (CC BY-SA 4.0)';

  @override
  String get triviaPasaleElMovilA => 'Pásale el móvil a';

  @override
  String get triviaPasaleElMovilAyuda =>
      'Cuando lo tenga, pulsa continuar para ver tu pregunta en privado.';

  @override
  String get triviaPregunta => 'Pregunta';

  @override
  String triviaOpcionLetra(String letra, String texto) {
    return 'Opción $letra: $texto';
  }

  @override
  String triviaRondaXDeY(int actual, int total) {
    return 'Ronda $actual de $total';
  }

  @override
  String get triviaFinDePartida => 'Fin de la partida';

  @override
  String get triviaGanadores => '¡Habéis ganado!';

  @override
  String get triviaNadieGano => 'Nadie ganó esta partida';

  @override
  String triviaGanadoresList(String nombres) {
    return 'Ganadores: $nombres';
  }

  @override
  String get triviaJugarOtra => 'Jugar otra';

  @override
  String get triviaNoHayPartida => 'No hay ninguna partida en curso.';

  @override
  String get wavelengthTitulo => 'Wavelength';

  @override
  String wavelengthSetupRangoJugadores(int min, int max) {
    return 'Introduce de $min a $max jugadores.';
  }

  @override
  String get wavelengthSetupRondas => 'Rondas';

  @override
  String wavelengthSetupRondasAyuda(int min, int max) {
    return 'Número de rondas a jugar. De $min a $max.';
  }

  @override
  String get wavelengthEmpezarPartida => 'Empezar partida';

  @override
  String get wavelengthIniciando => 'Iniciando...';

  @override
  String get wavelengthSinEspectrosTitulo => 'Sin espectros';

  @override
  String get wavelengthSinEspectrosMensaje =>
      'No hay espectros disponibles para jugar. Instala de nuevo la app para cargar los espectros de ejemplo.';

  @override
  String get wavelengthClueScreenInstruccion =>
      'Eres el PSIQUICO: solo TU ves el objetivo. Observa el dial, escribe una pista y pasa el movil al grupo.';

  @override
  String get wavelengthCluePsicoEtiqueta => 'Psiquico';

  @override
  String get wavelengthDialSemanticsClue => 'Dial de Wavelength, modo psiquico';

  @override
  String get wavelengthDialSemanticsGuess =>
      'Dial de Wavelength, mueve la aguja';

  @override
  String get wavelengthClueFieldLabel => 'Tu pista';

  @override
  String get wavelengthClueFieldHint => 'Escribe una pista...';

  @override
  String get wavelengthConfirmarPista => 'Confirmar pista y pasar el movil';

  @override
  String get wavelengthPassDeviceInstruccion => 'Pasale el movil al GRUPO';

  @override
  String get wavelengthPassDeviceAyuda =>
      'No mires el objetivo. Cuando el grupo tenga el movil, pulsa continuar para que adivinen con el dial.';

  @override
  String get wavelengthGuessInstruccion =>
      'GRUPO: movéis el dial hacia donde creéis que está el objetivo segun la pista.';

  @override
  String get wavelengthPistaEtiqueta => 'Pista del psiquico:';

  @override
  String get wavelengthConfirmarAdivinanza => 'Confirmar posicion del dial';

  @override
  String wavelengthRevealPuntos(int pts) {
    String _temp0 = intl.Intl.pluralLogic(
      pts,
      locale: localeName,
      other: '$pts puntos',
      one: '$pts punto',
    );
    return '$_temp0 esta ronda';
  }

  @override
  String wavelengthRevealTotalPuntos(int pts) {
    return 'Total: $pts puntos';
  }

  @override
  String wavelengthRevealRondaXDeY(int actual, int total) {
    return 'Ronda $actual de $total';
  }

  @override
  String get wavelengthSiguienteRonda => 'Siguiente ronda';

  @override
  String get wavelengthVerResultado => 'Ver resultado';

  @override
  String get wavelengthFinDePartida => 'Fin de la partida';

  @override
  String get wavelengthPuntuacionFinal => 'Puntuación final';

  @override
  String wavelengthPuntosTotales(int pts) {
    return '$pts puntos';
  }

  @override
  String get wavelengthJugarOtra => 'Jugar otra';

  @override
  String wavelengthRondaXDeY(int actual, int total) {
    return 'Ronda $actual de $total';
  }

  @override
  String get wavelengthBandBlanco => 'En el blanco';

  @override
  String get wavelengthBandCerca => 'Cerca';

  @override
  String get wavelengthBandLejos => 'Lejos';

  @override
  String get wavelengthBandFallo => 'Fallo';

  @override
  String wavelengthRevealScoreSemantica(String banda, int puntos) {
    return 'Puntuación: $banda, $puntos puntos';
  }

  @override
  String get tabuAciertoHint => 'Suma un punto al turno';

  @override
  String get tabuSaltarHint => 'Pasa a la siguiente palabra sin puntuar';

  @override
  String get tabuFaltaHint => 'Marca una falta por decir una palabra prohibida';

  @override
  String get tabuTitulo => 'Tabú';

  @override
  String get tabuSetupEquipos => 'Equipos';

  @override
  String get tabuEquipoA => 'Equipo A';

  @override
  String get tabuEquipoAHint => 'Ej.: Los Rápidos';

  @override
  String get tabuEquipoB => 'Equipo B';

  @override
  String get tabuEquipoBHint => 'Ej.: Los Creativos';

  @override
  String get tabuSetupTurno => 'Duración del turno';

  @override
  String get tabuSetupTurnoAyuda =>
      'Segundos que tiene cada equipo para describir palabras.';

  @override
  String tabuSegundos(int n) {
    return '$n seg';
  }

  @override
  String get tabuEmpezarPartida => 'Empezar partida';

  @override
  String get tabuIniciando => 'Iniciando...';

  @override
  String get tabuSinPalabrasTitulo => 'Sin palabras';

  @override
  String get tabuSinPalabrasMensaje =>
      'No hay palabras disponibles para jugar. Instala de nuevo la app para cargar las palabras de ejemplo.';

  @override
  String get tabuErrorEquipoAVacio =>
      'El nombre del equipo A no puede estar vacío.';

  @override
  String get tabuErrorEquipoBVacio =>
      'El nombre del equipo B no puede estar vacío.';

  @override
  String get tabuErrorEquiposDuplicados =>
      'Los equipos deben tener nombres distintos.';

  @override
  String get tabuErrorTurnoInvalido => 'La duración del turno no es válida.';

  @override
  String get tabuProhibidas => 'Palabras prohibidas';

  @override
  String get tabuAcierto => 'Acierto';

  @override
  String get tabuSaltar => 'Saltar';

  @override
  String get tabuFalta => 'Falta';

  @override
  String tabuAciertosContador(int n) {
    String _temp0 = intl.Intl.pluralLogic(
      n,
      locale: localeName,
      other: '$n aciertos',
      one: '$n acierto',
    );
    return '$_temp0';
  }

  @override
  String tabuTiempoRestante(int n) {
    return '$n segundos restantes';
  }

  @override
  String get tabuMarcador => 'Marcador';

  @override
  String tabuObjetivoVictorias(int n) {
    return 'Objetivo: $n victorias de ronda';
  }

  @override
  String get tabuSiguienteTurno => 'Siguiente turno';

  @override
  String get tabuFinDePartida => 'Fin de la partida';

  @override
  String get tabuGanadorLabel => 'Equipo ganador';

  @override
  String get tabuNoHayPartida => 'No hay ninguna partida en curso.';

  @override
  String get yoNuncaTitulo => 'Yo Nunca';

  @override
  String get yoNuncaSetupIntensidades => 'Intensidades';

  @override
  String get yoNuncaSetupIntensidadesAyuda =>
      'Elige al menos una intensidad para las frases.';

  @override
  String get yoNuncaIntensidadSuave => 'Suave';

  @override
  String get yoNuncaIntensidadPicante => 'Picante';

  @override
  String get yoNuncaAdvertenciaPicante =>
      'Contenido explícito (+18). Esta opción incluye frases de contenido sexual explícito para adultos. Solo actívala si todos los jugadores son mayores de 18 años y dan su consentimiento.';

  @override
  String get yoNuncaEmpezar => 'Empezar';

  @override
  String get yoNuncaErrorSinIntensidades =>
      'Elige al menos una intensidad para jugar.';

  @override
  String get yoNuncaSinFrasesTitulo => 'Sin frases';

  @override
  String get yoNuncaSinFrasesMensaje =>
      'No hay frases disponibles para las intensidades elegidas. Prueba con otra intensidad.';

  @override
  String get yoNuncaSiguiente => 'Siguiente';

  @override
  String yoNuncaFraseSemantica(String frase) {
    return 'Frase: $frase';
  }

  @override
  String get yoNuncaNoHaySesion => 'No hay ninguna sesión en curso.';

  @override
  String get bombaTitulo => 'La Bomba';

  @override
  String get bombaSetupModo => 'Modo de juego';

  @override
  String get bombaModoSilaba => 'Sílaba';

  @override
  String get bombaModoCategoria => 'Categoría';

  @override
  String bombaSetupRangoJugadores(int min, int max) {
    return 'Introduce de $min a $max jugadores.';
  }

  @override
  String get bombaEmpezarPartida => 'Empezar partida';

  @override
  String get bombaIniciando => 'Iniciando...';

  @override
  String get bombaSinPromptsTitulo => 'Sin prompts';

  @override
  String get bombaSinPromptsMensaje =>
      'No hay prompts disponibles para el modo elegido. Reinstala la app para cargar los datos de ejemplo.';

  @override
  String get bombaPasar => 'PASAR';

  @override
  String get bombaTipoSilaba => 'Sílaba — di una palabra que la contenga';

  @override
  String get bombaTipoCategoria =>
      'Categoría — di una palabra de esta categoría';

  @override
  String bombaPortadorSemantica(String nombre) {
    return 'Portador: $nombre';
  }

  @override
  String bombaPromptSemantica(String texto) {
    return 'Prompt: $texto';
  }

  @override
  String get bombaExplosionTitulo => '¡BOOM!';

  @override
  String get bombaEliminado => 'ha sido eliminado';

  @override
  String get bombaNoHayPartida => 'No hay ninguna partida en curso.';

  @override
  String get bombaFinDePartida => 'Fin de la partida';

  @override
  String get bombaGanadorLabel => 'Ganador';

  @override
  String get comoSeJuega => '¿Cómo se juega?';

  @override
  String get reglasEsUn10Pero1 => 'Un jugador pulsa «Sacar carta».';

  @override
  String get reglasEsUn10Pero2 =>
      'Una cuenta atrás de 5 segundos genera suspenso.';

  @override
  String get reglasEsUn10Pero3 => 'Se revela una carta al azar entre As y 10.';

  @override
  String get reglasEsUn10Pero4 =>
      'El grupo interpreta la carta como quiera. ¡Sin más reglas!';

  @override
  String get reglasImpostor1 =>
      'Decide cuántos jugadores, impostores y rondas habrá.';

  @override
  String get reglasImpostor2 =>
      'Pásate el móvil en orden: cada jugador ve su rol (ciudadano o IMPOSTOR) y lo cierra.';

  @override
  String get reglasImpostor3 =>
      'Los ciudadanos ven la palabra secreta; los impostores ven «IMPOSTOR» (y una pista si está activada).';

  @override
  String get reglasImpostor4 =>
      'Hay una ronda de debate: todos dan pistas sin revelar la palabra.';

  @override
  String get reglasImpostor5 =>
      'Cada ronda de votación, el grupo vota a quien crea impostor.';

  @override
  String get reglasImpostor6 =>
      'Los ciudadanos ganan si eliminan a todos los impostores; los impostores ganan si sobreviven todas las rondas.';

  @override
  String get reglasTrivia1 =>
      'Hasta 6 jugadores compiten respondiendo preguntas por turnos.';

  @override
  String get reglasTrivia2 =>
      'Las preguntas van de fácil a muy difícil a lo largo de las rondas.';

  @override
  String get reglasTrivia3 =>
      'Quien falla una pregunta queda eliminado de la partida.';

  @override
  String get reglasTrivia4 =>
      'Los jugadores que sobrevivan todas las rondas empatan como ganadores.';

  @override
  String get reglasTrivia5 =>
      'Las victorias se guardan por nombre: ¡acumula puntos en el ranking!';

  @override
  String get reglasWavelength1 =>
      'Un jugador ve en secreto un objetivo marcado en un espectro entre dos conceptos opuestos.';

  @override
  String get reglasWavelength2 =>
      'Ese jugador da una sola pista verbal que sitúe el objetivo en el espectro.';

  @override
  String get reglasWavelength3 =>
      'El resto del grupo mueve el dial para indicar dónde creen que está el objetivo.';

  @override
  String get reglasWavelength4 =>
      'Se puntúa según la cercanía al objetivo real: ¡cuanto más cerca, mejor!';

  @override
  String get reglasWavelength5 =>
      'Es cooperativo: el grupo gana o pierde junto acumulando puntos en todas las rondas.';

  @override
  String get reglasTabu1 =>
      'Se forman dos equipos. Un jugador de turno ve una palabra y sus palabras prohibidas.';

  @override
  String get reglasTabu2 =>
      'Debe describir la palabra SIN decir ninguna de las palabras prohibidas.';

  @override
  String get reglasTabu3 =>
      'Su equipo tiene que adivinarla antes de que acabe el tiempo.';

  @override
  String get reglasTabu4 =>
      'Si describe correctamente, el equipo anota un punto; si dice una prohibida, el turno pasa al equipo contrario.';

  @override
  String get reglasTabu5 =>
      'El primer equipo en llegar a 3 victorias gana la partida.';

  @override
  String get reglasYoNunca1 =>
      'El móvil muestra una frase «Yo nunca…» al azar.';

  @override
  String get reglasYoNunca2 =>
      'Quien haya hecho eso debe reconocerlo (o beber, ¡como prefiera el grupo!).';

  @override
  String get reglasYoNunca3 =>
      'Se pasa el móvil al siguiente jugador para sacar la siguiente frase.';

  @override
  String get reglasYoNunca4 =>
      'Elige el nivel: suave para todas las edades, picante para mayores.';

  @override
  String get reglasBomba1 => 'Se muestra una sílaba o categoría en pantalla.';

  @override
  String get reglasBomba2 =>
      'El jugador actual dice una palabra válida (que contenga la sílaba o pertenezca a la categoría) y pasa el móvil rápido.';

  @override
  String get reglasBomba3 =>
      'Una bomba con temporizador oculto explota al azar en cualquier momento.';

  @override
  String get reglasBomba4 =>
      'El jugador que tenga el móvil cuando explote queda eliminado.';

  @override
  String get reglasBomba5 => 'El último jugador en pie gana la partida.';
}
