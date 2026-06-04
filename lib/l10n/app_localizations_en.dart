// ignore: unused_import
import 'package:intl/intl.dart' as intl;
import 'app_localizations.dart';

// ignore_for_file: type=lint

/// The translations for English (`en`).
class AppLocalizationsEn extends AppLocalizations {
  AppLocalizationsEn([String locale = 'en']) : super(locale);

  @override
  String get appTitle => 'Sajitarios Gamespot';

  @override
  String get idioma => 'Language';

  @override
  String get idiomaDelSistema => 'System language';

  @override
  String get espanol => 'Spanish';

  @override
  String get ingles => 'English';

  @override
  String get cambiarIdioma => 'Change language';

  @override
  String get aceptar => 'OK';

  @override
  String get cancelar => 'Cancel';

  @override
  String get menuVacioTitulo => 'No games yet';

  @override
  String get menuVacioMensaje =>
      'Soon you\'ll be able to choose from several games to play in a group.';

  @override
  String jugarA(String titulo, String descripcion) {
    return 'Play $titulo. $descripcion';
  }

  @override
  String get rutaNoEncontradaTitulo => 'Screen not found';

  @override
  String get rutaNoEncontradaMensaje =>
      'We couldn\'t find the screen you were looking for.';

  @override
  String get rutaNoEncontradaAyuda => 'Go back to the menu to keep playing.';

  @override
  String get volverAlMenu => 'Back to menu';

  @override
  String get esUn10PeroTitulo => 'Es un 10 pero';

  @override
  String get sacarCarta => 'Draw a card';

  @override
  String get sacarOtraCarta => 'Draw another card';

  @override
  String get pistaCartaVacia => 'Tap \"Draw a card\"\nto reveal a card';

  @override
  String get cartaSinSacarSemantica => 'No card, tap Draw a card';

  @override
  String cartaSemantica(String valor, String palo) {
    return 'Card: $valor of $palo';
  }

  @override
  String get impostorTitulo => 'The Impostor';

  @override
  String get impostorMenuTitulo => 'The Impostor';

  @override
  String get impostorMenuDescripcion =>
      'Everyone knows the word... except the impostors. Will you find them?';

  @override
  String get esUn10PeroMenuTitulo => 'Es un 10 pero';

  @override
  String get esUn10PeroMenuDescripcion => 'Draw a random card from A to 10.';

  @override
  String get historial => 'History';

  @override
  String get gestionarPalabras => 'Manage words';

  @override
  String get setupJugadores => 'Players';

  @override
  String setupRangoJugadores(int min, int max) {
    return 'Enter from $min to $max players. The order is the reveal order.';
  }

  @override
  String get anadirJugador => 'Add player';

  @override
  String get maximoJugadoresAlcanzado => 'Maximum number of players reached';

  @override
  String get setupImpostores => 'Impostors';

  @override
  String get setupPista => 'Hint';

  @override
  String get iniciandoPartida => 'Starting...';

  @override
  String get empezarPartida => 'Start game';

  @override
  String jugadorNumero(int n) {
    return 'Player $n';
  }

  @override
  String nombreDelJugadorNumero(int n) {
    return 'Name of player $n';
  }

  @override
  String get quitarJugador => 'Remove player';

  @override
  String impostoresContador(int n) {
    String _temp0 = intl.Intl.pluralLogic(
      n,
      locale: localeName,
      other: '$n impostors',
      one: '$n impostor',
    );
    return '$_temp0';
  }

  @override
  String maximoImpostoresPara(int max, int jugadores) {
    return 'Maximum $max for $jugadores players.';
  }

  @override
  String get menosImpostores => 'Fewer impostors';

  @override
  String get masImpostores => 'More impostors';

  @override
  String get darPistaAlImpostor => 'Give the impostor a hint';

  @override
  String get darPistaAlImpostorSubtitulo =>
      'If enabled, the impostor will see a hint about the word.';

  @override
  String errorPocosJugadores(int min) {
    return 'At least $min players are needed.';
  }

  @override
  String errorDemasiadosJugadores(int max) {
    return 'The maximum is $max players.';
  }

  @override
  String get errorNombresDuplicados => 'There are duplicate player names.';

  @override
  String get errorNombreVacio => 'All players must have a name.';

  @override
  String get errorNoSePudoIniciar => 'The game could not be started.';

  @override
  String get errorSinPalabras =>
      'There are no words available to start the Impostor game.';

  @override
  String get noHayPalabrasTitulo => 'No words';

  @override
  String get noHayPalabrasMensaje =>
      'You need at least one word to play. Add your first word from the word management screen.';

  @override
  String get ahoraNo => 'Not now';

  @override
  String get pasaleElMovilA => 'Pass the phone to';

  @override
  String jugadorXDeY(int posicion, int total) {
    return 'Player $posicion of $total';
  }

  @override
  String get pasaleElMovilAyuda =>
      'Once they\'re holding it, tap continue and reveal your role in private.';

  @override
  String get continuar => 'Continue';

  @override
  String get esElTurnoDe => 'It\'s the turn of';

  @override
  String get pulsaRevelar =>
      'Tap \"Reveal\" when you\'re the one looking at the screen.';

  @override
  String get revelar => 'Reveal';

  @override
  String get ocultarYPasar => 'Hide and pass';

  @override
  String get ocultarYVerResultados => 'Hide and see results';

  @override
  String get impostorMayus => 'IMPOSTOR';

  @override
  String get tuPalabraEs => 'Your word is';

  @override
  String get noHayPartidaEnCurso => 'There is no game in progress.';

  @override
  String get configurarPartida => 'Set up game';

  @override
  String get tuRolEsImpostor => 'Your role is IMPOSTOR';

  @override
  String tuRolEsImpostorConPista(String pista) {
    return 'Your role is IMPOSTOR. Hint: $pista';
  }

  @override
  String tuPalabraEsAnuncio(String palabra) {
    return 'Your word is $palabra';
  }

  @override
  String get salirDeLaPartidaTitulo => 'Leave the game?';

  @override
  String get salirDeLaPartidaMensaje =>
      'If you leave now the current game will be lost and you\'ll have to set it up again.';

  @override
  String get seguirJugando => 'Keep playing';

  @override
  String get salir => 'Leave';

  @override
  String get resultadoDeLaPartida => 'Game result';

  @override
  String get noHayPartidaQueMostrar => 'There is no game to show.';

  @override
  String get laPalabraEra => 'The word was';

  @override
  String pistaConValor(String pista) {
    return 'Hint: $pista';
  }

  @override
  String resumenImpostores(int count) {
    String _temp0 = intl.Intl.pluralLogic(
      count,
      locale: localeName,
      other: 'There were $count impostors.',
      one: 'There was 1 impostor.',
      zero: 'There were no impostors: everyone knew the word.',
    );
    return '$_temp0';
  }

  @override
  String get sabiaLaPalabra => 'Knew the word';

  @override
  String jugadorEraImpostor(String nombre) {
    return '$nombre: was an impostor';
  }

  @override
  String jugadorSabiaLaPalabra(String nombre) {
    return '$nombre: knew the word';
  }

  @override
  String get jugarOtra => 'Play again';

  @override
  String get gestionarPalabrasTitulo => 'Manage words';

  @override
  String get agregar => 'Add';

  @override
  String get buscarPalabra => 'Search word';

  @override
  String get limpiarBusqueda => 'Clear search';

  @override
  String get noSePudieronCargarPalabras => 'The words could not be loaded.';

  @override
  String get sinPalabrasAun =>
      'No words yet. Add the first one with the \"Add\" button.';

  @override
  String get sinCoincidencias => 'No words match your search.';

  @override
  String get palabraAnadida => 'Word added.';

  @override
  String get yaExisteEsaPalabra => 'That word already exists.';

  @override
  String get palabraYPistaObligatorias => 'The word and the hint are required.';

  @override
  String get palabraActualizada => 'Word updated.';

  @override
  String get palabrasPredefinidasSoloLectura =>
      'Predefined words are read-only.';

  @override
  String get esaPalabraYaNoExiste => 'That word no longer exists.';

  @override
  String get palabraBorrada => 'Word deleted.';

  @override
  String get borrarPalabraTitulo => 'Delete word';

  @override
  String borrarPalabraMensaje(String palabra) {
    return 'Are you sure you want to delete \"$palabra\"?';
  }

  @override
  String get borrar => 'Delete';

  @override
  String get predefinida => 'Predefined';

  @override
  String get palabrasPredefinidasNoEditar =>
      'Predefined words cannot be edited';

  @override
  String get editar => 'Edit';

  @override
  String get palabrasPredefinidasNoBorrar =>
      'Predefined words cannot be deleted';

  @override
  String get editarPalabra => 'Edit word';

  @override
  String get nuevaPalabra => 'New word';

  @override
  String get campoPalabra => 'Word';

  @override
  String get campoPalabraHint => 'E.g.: pirate';

  @override
  String get campoPista => 'Hint';

  @override
  String get campoPistaHint => 'E.g.: ship';

  @override
  String get campoObligatorio => 'This field is required.';

  @override
  String get guardar => 'Save';

  @override
  String get borrarHistorialTitulo => 'Delete history';

  @override
  String get borrarHistorialMensaje =>
      'All saved games will be deleted. This action cannot be undone.';

  @override
  String get borrarTodo => 'Delete all';

  @override
  String get historialBorrado => 'History deleted.';

  @override
  String get noSePudoBorrarHistorial => 'The history could not be deleted.';

  @override
  String get noSePudoCargarHistorial => 'The history could not be loaded.';

  @override
  String get reintentar => 'Retry';

  @override
  String get historialVacioTitulo => 'No saved games yet.';

  @override
  String get historialVacioMensaje =>
      'When you finish an Impostor game it will appear here.';

  @override
  String get partidas => 'Games';

  @override
  String get estadisticas => 'Statistics';

  @override
  String get partidasJugadas => 'Games played';

  @override
  String get palabraMasRepetida => 'Most frequent word';

  @override
  String palabraMasRepetidaValor(String palabra, int veces) {
    return '$palabra ($veces)';
  }

  @override
  String get vecesQueFueImpostor => 'Times each player was an impostor';

  @override
  String get nadieFueImpostor => 'No one has been an impostor yet.';

  @override
  String partidaSubtitulo(String fecha, int jugadores, String impostores) {
    return '$fecha · $jugadores players · $impostores';
  }

  @override
  String impostoresTexto(int n) {
    String _temp0 = intl.Intl.pluralLogic(
      n,
      locale: localeName,
      other: '$n impostors',
      one: '1 impostor',
    );
    return '$_temp0';
  }

  @override
  String pistaActivadaConValor(String pista) {
    return 'Hint enabled: $pista';
  }

  @override
  String get pistaActivada => 'Hint enabled';

  @override
  String get pistaDesactivada => 'Hint disabled';

  @override
  String get silenciarSonido => 'Mute sound';

  @override
  String get activarSonido => 'Unmute sound';
}
