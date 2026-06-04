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
  String get errorSinPalabras =>
      'No hay palabras disponibles para iniciar la partida del Impostor.';

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
}
