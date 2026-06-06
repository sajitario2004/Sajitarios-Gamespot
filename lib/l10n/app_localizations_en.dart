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

  @override
  String get setupRondas => 'Rounds';

  @override
  String setupRondasAyuda(int min, int max) {
    return 'Voting chances to expel the impostors. From $min to $max with these players.';
  }

  @override
  String rondasContador(int n) {
    String _temp0 = intl.Intl.pluralLogic(
      n,
      locale: localeName,
      other: '$n rounds',
      one: '$n round',
    );
    return '$_temp0';
  }

  @override
  String get menosRondas => 'Fewer rounds';

  @override
  String get masRondas => 'More rounds';

  @override
  String get votacionTitulo => 'Voting';

  @override
  String get votacionInstruccion => 'Vote for who you think is the impostor';

  @override
  String votacionRondaXDeY(int actual, int total) {
    return 'Round $actual of $total';
  }

  @override
  String get votacionExpulsar => 'Expel';

  @override
  String votacionConfirmarExpulsion(String nombre) {
    return 'Expel $nombre?';
  }

  @override
  String get votacionImpostorSigue => 'The impostor is still among you';

  @override
  String get votacionJugadoresGanan => 'You won!';

  @override
  String votacionEraImpostor(String nombre) {
    return '$nombre was an impostor';
  }

  @override
  String get finDePartidaTitulo => 'Game over';

  @override
  String get sacandoCartaEn => 'Drawing card in...';

  @override
  String get triviaTitulo => 'Questions for points';

  @override
  String get triviaMenuTitulo => 'Questions for points';

  @override
  String get triviaMenuDescripcion =>
      'Answer general knowledge and other themed questions. The most right wins!';

  @override
  String get triviaSetupTematicas => 'Themes';

  @override
  String get triviaSetupTematicasAyuda =>
      'Choose at least one theme for the game.';

  @override
  String triviaSetupRangoJugadores(int min, int max) {
    return 'Enter from $min to $max players.';
  }

  @override
  String get triviaEmpezarPartida => 'Start game';

  @override
  String get triviaIniciando => 'Starting...';

  @override
  String get triviaNoHayPreguntasTitulo => 'No questions';

  @override
  String get triviaNoHayPreguntasMensaje =>
      'There are not enough questions for the chosen themes and players. Try selecting more themes.';

  @override
  String get triviaRankingVictorias => 'Win ranking';

  @override
  String get triviaSinVictorias => 'No wins recorded yet.';

  @override
  String triviaVictorias(int n) {
    String _temp0 = intl.Intl.pluralLogic(
      n,
      locale: localeName,
      other: '$n wins',
      one: '$n win',
    );
    return '$_temp0';
  }

  @override
  String get triviaAtribucion =>
      'Questions based on Open Trivia DB (CC BY-SA 4.0)';

  @override
  String get triviaPasaleElMovilA => 'Pass the phone to';

  @override
  String get triviaPasaleElMovilAyuda =>
      'Once they\'re holding it, tap continue to see your question in private.';

  @override
  String get triviaPregunta => 'Question';

  @override
  String triviaOpcionLetra(String letra, String texto) {
    return 'Option $letra: $texto';
  }

  @override
  String triviaRondaXDeY(int actual, int total) {
    return 'Round $actual of $total';
  }

  @override
  String get triviaFinDePartida => 'Game over';

  @override
  String get triviaGanadores => 'You won!';

  @override
  String get triviaNadieGano => 'Nobody won this game';

  @override
  String triviaGanadoresList(String nombres) {
    return 'Winners: $nombres';
  }

  @override
  String get triviaJugarOtra => 'Play again';

  @override
  String get triviaNoHayPartida => 'There is no game in progress.';

  @override
  String get wavelengthTitulo => 'Wavelength';

  @override
  String wavelengthSetupRangoJugadores(int min, int max) {
    return 'Enter from $min to $max players.';
  }

  @override
  String get wavelengthSetupRondas => 'Rounds';

  @override
  String wavelengthSetupRondasAyuda(int min, int max) {
    return 'Number of rounds to play. From $min to $max.';
  }

  @override
  String get wavelengthEmpezarPartida => 'Start game';

  @override
  String get wavelengthIniciando => 'Starting...';

  @override
  String get wavelengthSinEspectrosTitulo => 'No spectra';

  @override
  String get wavelengthSinEspectrosMensaje =>
      'No spectra available to play. Reinstall the app to load the sample spectra.';

  @override
  String get wavelengthClueScreenInstruccion =>
      'You are the PSYCHIC: only YOU see the target. Look at the dial, write a clue, and pass the phone to the group.';

  @override
  String get wavelengthCluePsicoEtiqueta => 'Psychic';

  @override
  String get wavelengthDialSemanticsClue => 'Wavelength dial, psychic mode';

  @override
  String get wavelengthDialSemanticsGuess => 'Wavelength dial, move the needle';

  @override
  String get wavelengthClueFieldLabel => 'Your clue';

  @override
  String get wavelengthClueFieldHint => 'Type a clue...';

  @override
  String get wavelengthConfirmarPista => 'Confirm clue and pass the phone';

  @override
  String get wavelengthPassDeviceInstruccion => 'Pass the phone to the GROUP';

  @override
  String get wavelengthPassDeviceAyuda =>
      'Don\'t look at the target. When the group has the phone, tap continue to move the dial.';

  @override
  String get wavelengthGuessInstruccion =>
      'GROUP: move the dial to where you think the target is based on the clue.';

  @override
  String get wavelengthPistaEtiqueta => 'Psychic\'s clue:';

  @override
  String get wavelengthConfirmarAdivinanza => 'Confirm dial position';

  @override
  String wavelengthRevealPuntos(int pts) {
    String _temp0 = intl.Intl.pluralLogic(
      pts,
      locale: localeName,
      other: '$pts points',
      one: '$pts point',
    );
    return '$_temp0 this round';
  }

  @override
  String wavelengthRevealTotalPuntos(int pts) {
    return 'Total: $pts points';
  }

  @override
  String wavelengthRevealRondaXDeY(int actual, int total) {
    return 'Round $actual of $total';
  }

  @override
  String get wavelengthSiguienteRonda => 'Next round';

  @override
  String get wavelengthVerResultado => 'See result';

  @override
  String get wavelengthFinDePartida => 'Game over';

  @override
  String get wavelengthPuntuacionFinal => 'Final score';

  @override
  String wavelengthPuntosTotales(int pts) {
    return '$pts points';
  }

  @override
  String get wavelengthJugarOtra => 'Play again';

  @override
  String get wavelengthNoHayPartida => 'There is no game in progress.';

  @override
  String wavelengthRondaXDeY(int actual, int total) {
    return 'Round $actual of $total';
  }

  @override
  String get wavelengthBandBlanco => 'Bullseye';

  @override
  String get wavelengthBandCerca => 'Close';

  @override
  String get wavelengthBandLejos => 'Far';

  @override
  String get wavelengthBandFallo => 'Miss';

  @override
  String wavelengthRevealScoreSemantica(String banda, int puntos) {
    return 'Score: $banda, $puntos points';
  }

  @override
  String get tabuAciertoHint => 'Adds one point to the turn';

  @override
  String get tabuSaltarHint => 'Moves to the next word without scoring';

  @override
  String get tabuFaltaHint => 'Marks a foul for saying a forbidden word';

  @override
  String get tabuTitulo => 'Taboo';

  @override
  String get tabuSetupEquipos => 'Teams';

  @override
  String get tabuEquipoA => 'Team A';

  @override
  String get tabuEquipoAHint => 'E.g.: The Fast Ones';

  @override
  String get tabuEquipoB => 'Team B';

  @override
  String get tabuEquipoBHint => 'E.g.: The Creative Ones';

  @override
  String get tabuSetupTurno => 'Turn duration';

  @override
  String get tabuSetupTurnoAyuda => 'Seconds each team has to describe words.';

  @override
  String tabuSegundos(int n) {
    return '$n sec';
  }

  @override
  String get tabuEmpezarPartida => 'Start game';

  @override
  String get tabuIniciando => 'Starting...';

  @override
  String get tabuSinPalabrasTitulo => 'No words';

  @override
  String get tabuSinPalabrasMensaje =>
      'No words available to play. Reinstall the app to load the sample words.';

  @override
  String get tabuErrorEquipoAVacio => 'Team A name cannot be empty.';

  @override
  String get tabuErrorEquipoBVacio => 'Team B name cannot be empty.';

  @override
  String get tabuErrorEquiposDuplicados => 'Teams must have different names.';

  @override
  String get tabuErrorTurnoInvalido => 'Turn duration is not valid.';

  @override
  String get tabuProhibidas => 'Forbidden words';

  @override
  String get tabuAcierto => 'Correct';

  @override
  String get tabuSaltar => 'Skip';

  @override
  String get tabuFalta => 'Foul';

  @override
  String tabuAciertosContador(int n) {
    String _temp0 = intl.Intl.pluralLogic(
      n,
      locale: localeName,
      other: '$n correct',
      one: '$n correct',
    );
    return '$_temp0';
  }

  @override
  String tabuTiempoRestante(int n) {
    return '$n seconds left';
  }

  @override
  String get tabuMarcador => 'Scoreboard';

  @override
  String tabuObjetivoVictorias(int n) {
    return 'Goal: $n round wins';
  }

  @override
  String get tabuSiguienteTurno => 'Next turn';

  @override
  String get tabuFinDePartida => 'Game over';

  @override
  String get tabuGanadorLabel => 'Winning team';

  @override
  String get tabuNoHayPartida => 'There is no game in progress.';

  @override
  String get yoNuncaTitulo => 'Never Have I Ever';

  @override
  String get yoNuncaSetupIntensidades => 'Intensity';

  @override
  String get yoNuncaSetupIntensidadesAyuda =>
      'Choose at least one intensity level for the statements.';

  @override
  String get yoNuncaIntensidadSuave => 'Mild';

  @override
  String get yoNuncaIntensidadPicante => 'Spicy';

  @override
  String get yoNuncaAdvertenciaPicante =>
      'Explicit content (+18). This option includes sexually explicit statements for adults only. Enable it only if all players are 18 or older and consent.';

  @override
  String get yoNuncaEmpezar => 'Start';

  @override
  String get yoNuncaErrorSinIntensidades =>
      'Choose at least one intensity level to play.';

  @override
  String get yoNuncaSinFrasesTitulo => 'No statements';

  @override
  String get yoNuncaSinFrasesMensaje =>
      'There are no statements available for the selected intensity levels. Try a different intensity.';

  @override
  String get yoNuncaSiguiente => 'Next';

  @override
  String yoNuncaFraseSemantica(String frase) {
    return 'Statement: $frase';
  }

  @override
  String get yoNuncaNoHaySesion => 'There is no active session.';

  @override
  String get bombaTitulo => 'La Bomba';

  @override
  String get bombaSetupModo => 'Game mode';

  @override
  String get bombaModoSilaba => 'Syllable';

  @override
  String get bombaModoCategoria => 'Category';

  @override
  String bombaSetupRangoJugadores(int min, int max) {
    return 'Enter from $min to $max players.';
  }

  @override
  String get bombaEmpezarPartida => 'Start game';

  @override
  String get bombaIniciando => 'Starting...';

  @override
  String get bombaSinPromptsTitulo => 'No prompts';

  @override
  String get bombaSinPromptsMensaje =>
      'No prompts available for the selected mode. Reinstall the app to load the sample data.';

  @override
  String get bombaPasar => 'PASS';

  @override
  String get bombaTipoSilaba => 'Syllable — say a word containing it';

  @override
  String get bombaTipoCategoria => 'Category — say a word from this category';

  @override
  String bombaPortadorSemantica(String nombre) {
    return 'Holder: $nombre';
  }

  @override
  String bombaPromptSemantica(String texto) {
    return 'Prompt: $texto';
  }

  @override
  String get bombaExplosionTitulo => 'BOOM!';

  @override
  String get bombaEliminado => 'has been eliminated';

  @override
  String get bombaNoHayPartida => 'There is no game in progress.';

  @override
  String get bombaFinDePartida => 'Game over';

  @override
  String get bombaGanadorLabel => 'Winner';

  @override
  String get comoSeJuega => 'How to play?';

  @override
  String get reglasEsUn10Pero1 => 'A player taps «Draw a card».';

  @override
  String get reglasEsUn10Pero2 => 'A 5-second countdown builds suspense.';

  @override
  String get reglasEsUn10Pero3 =>
      'A random card between Ace and 10 is revealed.';

  @override
  String get reglasEsUn10Pero4 =>
      'The group interprets the card however they like. No more rules!';

  @override
  String get reglasImpostor1 =>
      'Choose the number of players, impostors, and rounds.';

  @override
  String get reglasImpostor2 =>
      'Pass the phone in order: each player sees their role (citizen or IMPOSTOR) and closes the app.';

  @override
  String get reglasImpostor3 =>
      'Citizens see the secret word; impostors see «IMPOSTOR» (and a hint if enabled).';

  @override
  String get reglasImpostor4 =>
      'There is a debate round: everyone gives clues without revealing the word.';

  @override
  String get reglasImpostor5 =>
      'Each voting round, the group votes on who they think is the impostor.';

  @override
  String get reglasImpostor6 =>
      'Citizens win if they eliminate all impostors; impostors win if they survive all rounds.';

  @override
  String get reglasTrivia1 =>
      'Up to 6 players compete by answering questions in turns.';

  @override
  String get reglasTrivia2 =>
      'Questions range from easy to very hard throughout the rounds.';

  @override
  String get reglasTrivia3 =>
      'A player who answers incorrectly is eliminated from the game.';

  @override
  String get reglasTrivia4 => 'Players who survive all rounds tie as winners.';

  @override
  String get reglasTrivia5 => 'Wins are saved by name — build up your ranking!';

  @override
  String get reglasWavelength1 =>
      'One player secretly sees a target marked on a spectrum between two opposite concepts.';

  @override
  String get reglasWavelength2 =>
      'That player gives a single verbal clue that places the target on the spectrum.';

  @override
  String get reglasWavelength3 =>
      'The rest of the group moves the dial to show where they think the target is.';

  @override
  String get reglasWavelength4 =>
      'Points are scored based on closeness to the real target — the closer, the better!';

  @override
  String get reglasWavelength5 =>
      'It\'s cooperative: the group wins or loses together by accumulating points over all rounds.';

  @override
  String get reglasTabu1 =>
      'Form two teams. The active player sees a word and its forbidden words.';

  @override
  String get reglasTabu2 =>
      'They must describe the word WITHOUT saying any of the forbidden words.';

  @override
  String get reglasTabu3 => 'Their team must guess it before time runs out.';

  @override
  String get reglasTabu4 =>
      'A correct guess scores a point for the team; saying a forbidden word passes the turn to the other team.';

  @override
  String get reglasTabu5 => 'The first team to reach 3 wins the game.';

  @override
  String get reglasYoNunca1 =>
      'The phone shows a random «I have never…» statement.';

  @override
  String get reglasYoNunca2 =>
      'Anyone who has done it must admit it (or drink — whatever the group prefers!).';

  @override
  String get reglasYoNunca3 =>
      'Pass the phone to the next player for the next statement.';

  @override
  String get reglasYoNunca4 =>
      'Choose the level: mild for all ages, spicy for adults.';

  @override
  String get reglasBomba1 => 'A syllable or category is shown on screen.';

  @override
  String get reglasBomba2 =>
      'The current player says a valid word (containing the syllable or belonging to the category) and passes the phone quickly.';

  @override
  String get reglasBomba3 =>
      'A bomb with a hidden timer explodes at a random moment.';

  @override
  String get reglasBomba4 =>
      'The player holding the phone when it explodes is eliminated.';

  @override
  String get reglasBomba5 => 'The last player standing wins the game.';
}
