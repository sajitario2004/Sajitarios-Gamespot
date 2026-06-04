import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:intl/intl.dart' as intl;

import 'app_localizations_en.dart';
import 'app_localizations_es.dart';

// ignore_for_file: type=lint

/// Callers can lookup localized strings with an instance of AppLocalizations
/// returned by `AppLocalizations.of(context)`.
///
/// Applications need to include `AppLocalizations.delegate()` in their app's
/// `localizationDelegates` list, and the locales they support in the app's
/// `supportedLocales` list. For example:
///
/// ```dart
/// import 'l10n/app_localizations.dart';
///
/// return MaterialApp(
///   localizationsDelegates: AppLocalizations.localizationsDelegates,
///   supportedLocales: AppLocalizations.supportedLocales,
///   home: MyApplicationHome(),
/// );
/// ```
///
/// ## Update pubspec.yaml
///
/// Please make sure to update your pubspec.yaml to include the following
/// packages:
///
/// ```yaml
/// dependencies:
///   # Internationalization support.
///   flutter_localizations:
///     sdk: flutter
///   intl: any # Use the pinned version from flutter_localizations
///
///   # Rest of dependencies
/// ```
///
/// ## iOS Applications
///
/// iOS applications define key application metadata, including supported
/// locales, in an Info.plist file that is built into the application bundle.
/// To configure the locales supported by your app, you’ll need to edit this
/// file.
///
/// First, open your project’s ios/Runner.xcworkspace Xcode workspace file.
/// Then, in the Project Navigator, open the Info.plist file under the Runner
/// project’s Runner folder.
///
/// Next, select the Information Property List item, select Add Item from the
/// Editor menu, then select Localizations from the pop-up menu.
///
/// Select and expand the newly-created Localizations item then, for each
/// locale your application supports, add a new item and select the locale
/// you wish to add from the pop-up menu in the Value field. This list should
/// be consistent with the languages listed in the AppLocalizations.supportedLocales
/// property.
abstract class AppLocalizations {
  AppLocalizations(String locale)
    : localeName = intl.Intl.canonicalizedLocale(locale.toString());

  final String localeName;

  static AppLocalizations? of(BuildContext context) {
    return Localizations.of<AppLocalizations>(context, AppLocalizations);
  }

  static const LocalizationsDelegate<AppLocalizations> delegate =
      _AppLocalizationsDelegate();

  /// A list of this localizations delegate along with the default localizations
  /// delegates.
  ///
  /// Returns a list of localizations delegates containing this delegate along with
  /// GlobalMaterialLocalizations.delegate, GlobalCupertinoLocalizations.delegate,
  /// and GlobalWidgetsLocalizations.delegate.
  ///
  /// Additional delegates can be added by appending to this list in
  /// MaterialApp. This list does not have to be used at all if a custom list
  /// of delegates is preferred or required.
  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates =
      <LocalizationsDelegate<dynamic>>[
        delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
      ];

  /// A list of this localizations delegate's supported locales.
  static const List<Locale> supportedLocales = <Locale>[
    Locale('es'),
    Locale('en'),
  ];

  /// Título de la aplicación, mostrado en la barra superior del menú.
  ///
  /// In es, this message translates to:
  /// **'Sajitarios Gamespot'**
  String get appTitle;

  /// Etiqueta del control de selección de idioma.
  ///
  /// In es, this message translates to:
  /// **'Idioma'**
  String get idioma;

  /// Opción que sigue el idioma configurado en el dispositivo (locale nulo).
  ///
  /// In es, this message translates to:
  /// **'Idioma del sistema'**
  String get idiomaDelSistema;

  /// Nombre del idioma español en el selector de idioma.
  ///
  /// In es, this message translates to:
  /// **'Español'**
  String get espanol;

  /// Nombre del idioma inglés en el selector de idioma.
  ///
  /// In es, this message translates to:
  /// **'Inglés'**
  String get ingles;

  /// Tooltip/etiqueta accesible del botón que abre el selector de idioma.
  ///
  /// In es, this message translates to:
  /// **'Cambiar idioma'**
  String get cambiarIdioma;

  /// Texto del botón de confirmación genérico.
  ///
  /// In es, this message translates to:
  /// **'Aceptar'**
  String get aceptar;

  /// Texto del botón para cerrar/descartar un diálogo.
  ///
  /// In es, this message translates to:
  /// **'Cancelar'**
  String get cancelar;

  /// Título del estado vacío del menú cuando no hay juegos registrados.
  ///
  /// In es, this message translates to:
  /// **'Todavía no hay juegos'**
  String get menuVacioTitulo;

  /// Mensaje del estado vacío del menú cuando no hay juegos registrados.
  ///
  /// In es, this message translates to:
  /// **'Pronto podrás elegir entre varios juegos para jugar en grupo.'**
  String get menuVacioMensaje;

  /// Etiqueta semántica accesible de cada tarjeta de juego del menú.
  ///
  /// In es, this message translates to:
  /// **'Jugar a {titulo}. {descripcion}'**
  String jugarA(String titulo, String descripcion);

  /// Título de la barra superior de la pantalla de ruta no encontrada.
  ///
  /// In es, this message translates to:
  /// **'Pantalla no encontrada'**
  String get rutaNoEncontradaTitulo;

  /// Mensaje principal de la pantalla de ruta no encontrada.
  ///
  /// In es, this message translates to:
  /// **'No encontramos la pantalla que buscabas.'**
  String get rutaNoEncontradaMensaje;

  /// Mensaje secundario de la pantalla de ruta no encontrada.
  ///
  /// In es, this message translates to:
  /// **'Vuelve al menú para seguir jugando.'**
  String get rutaNoEncontradaAyuda;

  /// Texto del botón que regresa al menú principal.
  ///
  /// In es, this message translates to:
  /// **'Volver al menú'**
  String get volverAlMenu;

  /// Título de la pantalla del juego 'Es un 10 pero'.
  ///
  /// In es, this message translates to:
  /// **'Es un 10 pero'**
  String get esUn10PeroTitulo;

  /// Botón para sacar la primera carta en 'Es un 10 pero'.
  ///
  /// In es, this message translates to:
  /// **'Sacar carta'**
  String get sacarCarta;

  /// Botón para sacar una nueva carta tras haber sacado una en 'Es un 10 pero'.
  ///
  /// In es, this message translates to:
  /// **'Sacar otra carta'**
  String get sacarOtraCarta;

  /// Texto superpuesto sobre la carta mientras no se ha sacado ninguna.
  ///
  /// In es, this message translates to:
  /// **'Pulsa \"Sacar carta\"\npara revelar una carta'**
  String get pistaCartaVacia;

  /// Etiqueta semántica de la carta cuando todavía no se ha sacado ninguna.
  ///
  /// In es, this message translates to:
  /// **'Sin carta, pulsa Sacar carta'**
  String get cartaSinSacarSemantica;

  /// Etiqueta semántica dinámica de la carta actual en 'Es un 10 pero'.
  ///
  /// In es, this message translates to:
  /// **'Carta: {valor} de {palo}'**
  String cartaSemantica(String valor, String palo);

  /// Título de la barra superior en las pantallas del flujo del Impostor.
  ///
  /// In es, this message translates to:
  /// **'El Impostor'**
  String get impostorTitulo;

  /// Título del juego 'El Impostor' en la tarjeta del menú.
  ///
  /// In es, this message translates to:
  /// **'El Impostor'**
  String get impostorMenuTitulo;

  /// Descripción del juego 'El Impostor' en la tarjeta del menú.
  ///
  /// In es, this message translates to:
  /// **'Todos conocen la palabra... menos los impostores. ¿Los descubrirás?'**
  String get impostorMenuDescripcion;

  /// Título del juego 'Es un 10 pero' en la tarjeta del menú.
  ///
  /// In es, this message translates to:
  /// **'Es un 10 pero'**
  String get esUn10PeroMenuTitulo;

  /// Descripción del juego 'Es un 10 pero' en la tarjeta del menú.
  ///
  /// In es, this message translates to:
  /// **'Saca una carta al azar de la A al 10.'**
  String get esUn10PeroMenuDescripcion;

  /// Etiqueta/tooltip del acceso al historial de partidas.
  ///
  /// In es, this message translates to:
  /// **'Historial'**
  String get historial;

  /// Etiqueta/tooltip del acceso a la gestión del banco de palabras.
  ///
  /// In es, this message translates to:
  /// **'Gestionar palabras'**
  String get gestionarPalabras;

  /// Encabezado de la sección de jugadores en la configuración de la partida.
  ///
  /// In es, this message translates to:
  /// **'Jugadores'**
  String get setupJugadores;

  /// Texto de ayuda con el rango de jugadores permitido en la configuración.
  ///
  /// In es, this message translates to:
  /// **'Introduce de {min} a {max} jugadores. El orden es el orden de revelación.'**
  String setupRangoJugadores(int min, int max);

  /// Botón para añadir un nuevo jugador en la configuración.
  ///
  /// In es, this message translates to:
  /// **'Añadir jugador'**
  String get anadirJugador;

  /// Texto del botón de añadir jugador cuando se llegó al máximo.
  ///
  /// In es, this message translates to:
  /// **'Máximo de jugadores alcanzado'**
  String get maximoJugadoresAlcanzado;

  /// Encabezado de la sección de impostores en la configuración.
  ///
  /// In es, this message translates to:
  /// **'Impostores'**
  String get setupImpostores;

  /// Encabezado de la sección de la pista en la configuración.
  ///
  /// In es, this message translates to:
  /// **'Pista'**
  String get setupPista;

  /// Texto del botón de empezar mientras se inicia la partida.
  ///
  /// In es, this message translates to:
  /// **'Iniciando...'**
  String get iniciandoPartida;

  /// Botón para empezar la partida del Impostor.
  ///
  /// In es, this message translates to:
  /// **'Empezar partida'**
  String get empezarPartida;

  /// Etiqueta del campo de nombre del jugador n.
  ///
  /// In es, this message translates to:
  /// **'Jugador {n}'**
  String jugadorNumero(int n);

  /// Texto de pista (hint) del campo de nombre del jugador n.
  ///
  /// In es, this message translates to:
  /// **'Nombre del jugador {n}'**
  String nombreDelJugadorNumero(int n);

  /// Tooltip del botón para quitar un jugador.
  ///
  /// In es, this message translates to:
  /// **'Quitar jugador'**
  String get quitarJugador;

  /// Número de impostores seleccionado (singular/plural).
  ///
  /// In es, this message translates to:
  /// **'{n, plural, one{{n} impostor} other{{n} impostores}}'**
  String impostoresContador(int n);

  /// Texto de ayuda con el máximo de impostores para la cantidad de jugadores.
  ///
  /// In es, this message translates to:
  /// **'Máximo {max} para {jugadores} jugadores.'**
  String maximoImpostoresPara(int max, int jugadores);

  /// Tooltip del botón para reducir el número de impostores.
  ///
  /// In es, this message translates to:
  /// **'Menos impostores'**
  String get menosImpostores;

  /// Tooltip del botón para aumentar el número de impostores.
  ///
  /// In es, this message translates to:
  /// **'Más impostores'**
  String get masImpostores;

  /// Título del interruptor que activa la pista para el impostor.
  ///
  /// In es, this message translates to:
  /// **'Dar pista al impostor'**
  String get darPistaAlImpostor;

  /// Subtítulo del interruptor de pista.
  ///
  /// In es, this message translates to:
  /// **'Si está activa, el impostor verá una pista sobre la palabra.'**
  String get darPistaAlImpostorSubtitulo;

  /// Error cuando hay menos jugadores del mínimo permitido.
  ///
  /// In es, this message translates to:
  /// **'Se necesitan al menos {min} jugadores.'**
  String errorPocosJugadores(int min);

  /// Error cuando hay más jugadores del máximo permitido.
  ///
  /// In es, this message translates to:
  /// **'El máximo es de {max} jugadores.'**
  String errorDemasiadosJugadores(int max);

  /// Error cuando hay nombres de jugador repetidos.
  ///
  /// In es, this message translates to:
  /// **'Hay nombres de jugador repetidos.'**
  String get errorNombresDuplicados;

  /// Error cuando algún jugador no tiene nombre.
  ///
  /// In es, this message translates to:
  /// **'Todos los jugadores deben tener un nombre.'**
  String get errorNombreVacio;

  /// Error genérico al no poder iniciar la partida.
  ///
  /// In es, this message translates to:
  /// **'No se pudo iniciar la partida.'**
  String get errorNoSePudoIniciar;

  /// Error cuando la base de datos no tiene palabras para iniciar la partida del Impostor.
  ///
  /// In es, this message translates to:
  /// **'No hay palabras disponibles para iniciar la partida del Impostor.'**
  String get errorSinPalabras;

  /// Título del diálogo cuando la BD no tiene palabras.
  ///
  /// In es, this message translates to:
  /// **'No hay palabras'**
  String get noHayPalabrasTitulo;

  /// Mensaje del diálogo cuando la BD no tiene palabras.
  ///
  /// In es, this message translates to:
  /// **'Necesitas al menos una palabra para jugar. Añade tu primera palabra desde la gestión de palabras.'**
  String get noHayPalabrasMensaje;

  /// Botón para descartar el diálogo de sin palabras.
  ///
  /// In es, this message translates to:
  /// **'Ahora no'**
  String get ahoraNo;

  /// Instrucción de la pantalla de paso de móvil.
  ///
  /// In es, this message translates to:
  /// **'Pásale el móvil a'**
  String get pasaleElMovilA;

  /// Indicador de posición del jugador actual.
  ///
  /// In es, this message translates to:
  /// **'Jugador {posicion} de {total}'**
  String jugadorXDeY(int posicion, int total);

  /// Texto de ayuda en la pantalla de paso de móvil.
  ///
  /// In es, this message translates to:
  /// **'Cuando lo tenga en sus manos, pulsa continuar y revela tu rol en privado.'**
  String get pasaleElMovilAyuda;

  /// Botón para continuar a la revelación del rol.
  ///
  /// In es, this message translates to:
  /// **'Continuar'**
  String get continuar;

  /// Texto previo al nombre del jugador en la pantalla de revelación.
  ///
  /// In es, this message translates to:
  /// **'Es el turno de'**
  String get esElTurnoDe;

  /// Texto de la carta oculta antes de revelar el rol.
  ///
  /// In es, this message translates to:
  /// **'Pulsa \"Revelar\" cuando seas tú quien mira la pantalla.'**
  String get pulsaRevelar;

  /// Botón para revelar el rol del jugador actual.
  ///
  /// In es, this message translates to:
  /// **'Revelar'**
  String get revelar;

  /// Botón para ocultar el rol y pasar al siguiente jugador.
  ///
  /// In es, this message translates to:
  /// **'Ocultar y pasar'**
  String get ocultarYPasar;

  /// Botón para ocultar el rol del último jugador y ver los resultados.
  ///
  /// In es, this message translates to:
  /// **'Ocultar y ver resultados'**
  String get ocultarYVerResultados;

  /// Etiqueta del rol impostor mostrada en mayúsculas.
  ///
  /// In es, this message translates to:
  /// **'IMPOSTOR'**
  String get impostorMayus;

  /// Texto previo a la palabra para los jugadores no impostores.
  ///
  /// In es, this message translates to:
  /// **'Tu palabra es'**
  String get tuPalabraEs;

  /// Mensaje cuando la pantalla de revelación no tiene partida.
  ///
  /// In es, this message translates to:
  /// **'No hay ninguna partida en curso.'**
  String get noHayPartidaEnCurso;

  /// Botón para ir a la configuración cuando no hay partida en curso.
  ///
  /// In es, this message translates to:
  /// **'Configurar partida'**
  String get configurarPartida;

  /// Anuncio accesible base cuando el jugador es impostor.
  ///
  /// In es, this message translates to:
  /// **'Tu rol es IMPOSTOR'**
  String get tuRolEsImpostor;

  /// Anuncio accesible cuando el impostor tiene pista activada.
  ///
  /// In es, this message translates to:
  /// **'Tu rol es IMPOSTOR. Pista: {pista}'**
  String tuRolEsImpostorConPista(String pista);

  /// Anuncio accesible de la palabra para jugadores no impostores.
  ///
  /// In es, this message translates to:
  /// **'Tu palabra es {palabra}'**
  String tuPalabraEsAnuncio(String palabra);

  /// Título del diálogo de confirmación para abandonar la partida.
  ///
  /// In es, this message translates to:
  /// **'¿Salir de la partida?'**
  String get salirDeLaPartidaTitulo;

  /// Mensaje del diálogo de confirmación para abandonar la partida.
  ///
  /// In es, this message translates to:
  /// **'Si sales ahora se perderá la partida actual y tendrás que configurarla de nuevo.'**
  String get salirDeLaPartidaMensaje;

  /// Botón para cancelar la salida y seguir en la partida.
  ///
  /// In es, this message translates to:
  /// **'Seguir jugando'**
  String get seguirJugando;

  /// Botón para confirmar la salida de la partida.
  ///
  /// In es, this message translates to:
  /// **'Salir'**
  String get salir;

  /// Título de la pantalla de resultados.
  ///
  /// In es, this message translates to:
  /// **'Resultado de la partida'**
  String get resultadoDeLaPartida;

  /// Mensaje en resultados cuando no hay una sesión válida.
  ///
  /// In es, this message translates to:
  /// **'No hay ninguna partida que mostrar.'**
  String get noHayPartidaQueMostrar;

  /// Texto previo a la palabra en la tarjeta de resultados.
  ///
  /// In es, this message translates to:
  /// **'La palabra era'**
  String get laPalabraEra;

  /// Pista con su valor, mostrada en resultados y en la lista de palabras.
  ///
  /// In es, this message translates to:
  /// **'Pista: {pista}'**
  String pistaConValor(String pista);

  /// Resumen del número de impostores de la partida en resultados.
  ///
  /// In es, this message translates to:
  /// **'{count, plural, =0{No había ningún impostor: todos sabían la palabra.} one{Había 1 impostor.} other{Había {count} impostores.}}'**
  String resumenImpostores(int count);

  /// Etiqueta del rol de quien sabía la palabra.
  ///
  /// In es, this message translates to:
  /// **'Sabía la palabra'**
  String get sabiaLaPalabra;

  /// Etiqueta semántica del jugador que fue impostor.
  ///
  /// In es, this message translates to:
  /// **'{nombre}: era impostor'**
  String jugadorEraImpostor(String nombre);

  /// Etiqueta semántica del jugador que sabía la palabra.
  ///
  /// In es, this message translates to:
  /// **'{nombre}: sabía la palabra'**
  String jugadorSabiaLaPalabra(String nombre);

  /// Botón para jugar otra partida desde resultados.
  ///
  /// In es, this message translates to:
  /// **'Jugar otra'**
  String get jugarOtra;

  /// Título de la pantalla de gestión de palabras.
  ///
  /// In es, this message translates to:
  /// **'Gestionar palabras'**
  String get gestionarPalabrasTitulo;

  /// Botón para agregar una palabra y confirmación del alta.
  ///
  /// In es, this message translates to:
  /// **'Agregar'**
  String get agregar;

  /// Texto de pista del campo de búsqueda de palabras.
  ///
  /// In es, this message translates to:
  /// **'Buscar palabra'**
  String get buscarPalabra;

  /// Tooltip del botón para limpiar la búsqueda.
  ///
  /// In es, this message translates to:
  /// **'Limpiar búsqueda'**
  String get limpiarBusqueda;

  /// Mensaje de error al cargar el banco de palabras.
  ///
  /// In es, this message translates to:
  /// **'No se pudieron cargar las palabras.'**
  String get noSePudieronCargarPalabras;

  /// Estado vacío del banco de palabras sin búsqueda.
  ///
  /// In es, this message translates to:
  /// **'Aún no hay palabras. Añade la primera con el botón \"Agregar\".'**
  String get sinPalabrasAun;

  /// Estado vacío del banco de palabras con búsqueda sin coincidencias.
  ///
  /// In es, this message translates to:
  /// **'No hay palabras que coincidan con tu búsqueda.'**
  String get sinCoincidencias;

  /// Confirmación tras añadir una palabra.
  ///
  /// In es, this message translates to:
  /// **'Palabra añadida.'**
  String get palabraAnadida;

  /// Error al intentar crear/editar una palabra duplicada.
  ///
  /// In es, this message translates to:
  /// **'Ya existe esa palabra.'**
  String get yaExisteEsaPalabra;

  /// Error cuando faltan la palabra o la pista al guardar.
  ///
  /// In es, this message translates to:
  /// **'La palabra y la pista son obligatorias.'**
  String get palabraYPistaObligatorias;

  /// Confirmación tras actualizar una palabra.
  ///
  /// In es, this message translates to:
  /// **'Palabra actualizada.'**
  String get palabraActualizada;

  /// Error al intentar editar/borrar una palabra predefinida.
  ///
  /// In es, this message translates to:
  /// **'Las palabras predefinidas son de solo lectura.'**
  String get palabrasPredefinidasSoloLectura;

  /// Error cuando la palabra que se intenta editar/borrar ya no existe.
  ///
  /// In es, this message translates to:
  /// **'Esa palabra ya no existe.'**
  String get esaPalabraYaNoExiste;

  /// Confirmación tras borrar una palabra.
  ///
  /// In es, this message translates to:
  /// **'Palabra borrada.'**
  String get palabraBorrada;

  /// Título del diálogo de confirmación para borrar una palabra.
  ///
  /// In es, this message translates to:
  /// **'Borrar palabra'**
  String get borrarPalabraTitulo;

  /// Mensaje del diálogo de confirmación para borrar una palabra.
  ///
  /// In es, this message translates to:
  /// **'¿Seguro que quieres borrar \"{palabra}\"?'**
  String borrarPalabraMensaje(String palabra);

  /// Botón para confirmar el borrado de una palabra.
  ///
  /// In es, this message translates to:
  /// **'Borrar'**
  String get borrar;

  /// Chip que marca una palabra como predefinida (solo lectura).
  ///
  /// In es, this message translates to:
  /// **'Predefinida'**
  String get predefinida;

  /// Tooltip del botón editar deshabilitado para palabras predefinidas.
  ///
  /// In es, this message translates to:
  /// **'Las palabras predefinidas no se pueden editar'**
  String get palabrasPredefinidasNoEditar;

  /// Tooltip del botón editar de una palabra de usuario.
  ///
  /// In es, this message translates to:
  /// **'Editar'**
  String get editar;

  /// Tooltip del botón borrar deshabilitado para palabras predefinidas.
  ///
  /// In es, this message translates to:
  /// **'Las palabras predefinidas no se pueden borrar'**
  String get palabrasPredefinidasNoBorrar;

  /// Título del diálogo de edición de palabra.
  ///
  /// In es, this message translates to:
  /// **'Editar palabra'**
  String get editarPalabra;

  /// Título del diálogo de alta de palabra.
  ///
  /// In es, this message translates to:
  /// **'Nueva palabra'**
  String get nuevaPalabra;

  /// Etiqueta del campo de la palabra en el formulario.
  ///
  /// In es, this message translates to:
  /// **'Palabra'**
  String get campoPalabra;

  /// Texto de ejemplo del campo de la palabra.
  ///
  /// In es, this message translates to:
  /// **'Ej.: pirata'**
  String get campoPalabraHint;

  /// Etiqueta del campo de la pista en el formulario.
  ///
  /// In es, this message translates to:
  /// **'Pista'**
  String get campoPista;

  /// Texto de ejemplo del campo de la pista.
  ///
  /// In es, this message translates to:
  /// **'Ej.: barco'**
  String get campoPistaHint;

  /// Error de validación de un campo obligatorio del formulario.
  ///
  /// In es, this message translates to:
  /// **'Este campo es obligatorio.'**
  String get campoObligatorio;

  /// Botón para guardar la edición de una palabra.
  ///
  /// In es, this message translates to:
  /// **'Guardar'**
  String get guardar;

  /// Título del diálogo de confirmación y tooltip para borrar el historial.
  ///
  /// In es, this message translates to:
  /// **'Borrar historial'**
  String get borrarHistorialTitulo;

  /// Mensaje del diálogo de confirmación para borrar el historial.
  ///
  /// In es, this message translates to:
  /// **'Se borrarán todas las partidas guardadas. Esta acción no se puede deshacer.'**
  String get borrarHistorialMensaje;

  /// Botón para confirmar el borrado de todo el historial.
  ///
  /// In es, this message translates to:
  /// **'Borrar todo'**
  String get borrarTodo;

  /// Confirmación tras borrar el historial.
  ///
  /// In es, this message translates to:
  /// **'Historial borrado.'**
  String get historialBorrado;

  /// Error al intentar borrar el historial.
  ///
  /// In es, this message translates to:
  /// **'No se pudo borrar el historial.'**
  String get noSePudoBorrarHistorial;

  /// Mensaje de error al cargar el historial.
  ///
  /// In es, this message translates to:
  /// **'No se pudo cargar el historial.'**
  String get noSePudoCargarHistorial;

  /// Botón para reintentar la carga del historial.
  ///
  /// In es, this message translates to:
  /// **'Reintentar'**
  String get reintentar;

  /// Título del estado vacío del historial.
  ///
  /// In es, this message translates to:
  /// **'Todavía no hay partidas guardadas.'**
  String get historialVacioTitulo;

  /// Mensaje del estado vacío del historial.
  ///
  /// In es, this message translates to:
  /// **'Cuando termines una partida del Impostor aparecerá aquí.'**
  String get historialVacioMensaje;

  /// Encabezado de la lista de partidas del historial.
  ///
  /// In es, this message translates to:
  /// **'Partidas'**
  String get partidas;

  /// Encabezado del resumen de estadísticas del historial.
  ///
  /// In es, this message translates to:
  /// **'Estadísticas'**
  String get estadisticas;

  /// Etiqueta de la estadística de total de partidas jugadas.
  ///
  /// In es, this message translates to:
  /// **'Partidas jugadas'**
  String get partidasJugadas;

  /// Etiqueta de la estadística de la palabra más repetida.
  ///
  /// In es, this message translates to:
  /// **'Palabra más repetida'**
  String get palabraMasRepetida;

  /// Valor de la palabra más repetida con su número de repeticiones.
  ///
  /// In es, this message translates to:
  /// **'{palabra} ({veces})'**
  String palabraMasRepetidaValor(String palabra, int veces);

  /// Encabezado del ranking de cuántas veces cada jugador fue impostor.
  ///
  /// In es, this message translates to:
  /// **'Veces que cada jugador fue impostor'**
  String get vecesQueFueImpostor;

  /// Mensaje cuando ningún jugador ha sido impostor en el historial.
  ///
  /// In es, this message translates to:
  /// **'Nadie ha sido impostor todavía.'**
  String get nadieFueImpostor;

  /// Subtítulo de una partida en el historial (fecha, jugadores, impostores).
  ///
  /// In es, this message translates to:
  /// **'{fecha} · {jugadores} jugadores · {impostores}'**
  String partidaSubtitulo(String fecha, int jugadores, String impostores);

  /// Texto del número de impostores de una partida (singular/plural).
  ///
  /// In es, this message translates to:
  /// **'{n, plural, one{1 impostor} other{{n} impostores}}'**
  String impostoresTexto(int n);

  /// Detalle de partida con la pista activada y su valor.
  ///
  /// In es, this message translates to:
  /// **'Pista activada: {pista}'**
  String pistaActivadaConValor(String pista);

  /// Detalle de partida con la pista activada sin valor.
  ///
  /// In es, this message translates to:
  /// **'Pista activada'**
  String get pistaActivada;

  /// Detalle de partida con la pista desactivada.
  ///
  /// In es, this message translates to:
  /// **'Pista desactivada'**
  String get pistaDesactivada;

  /// Tooltip del botón de sonido cuando el audio está activo.
  ///
  /// In es, this message translates to:
  /// **'Silenciar sonido'**
  String get silenciarSonido;

  /// Tooltip del botón de sonido cuando el audio está silenciado.
  ///
  /// In es, this message translates to:
  /// **'Activar sonido'**
  String get activarSonido;
}

class _AppLocalizationsDelegate
    extends LocalizationsDelegate<AppLocalizations> {
  const _AppLocalizationsDelegate();

  @override
  Future<AppLocalizations> load(Locale locale) {
    return SynchronousFuture<AppLocalizations>(lookupAppLocalizations(locale));
  }

  @override
  bool isSupported(Locale locale) =>
      <String>['en', 'es'].contains(locale.languageCode);

  @override
  bool shouldReload(_AppLocalizationsDelegate old) => false;
}

AppLocalizations lookupAppLocalizations(Locale locale) {
  // Lookup logic when only language code is specified.
  switch (locale.languageCode) {
    case 'en':
      return AppLocalizationsEn();
    case 'es':
      return AppLocalizationsEs();
  }

  throw FlutterError(
    'AppLocalizations.delegate failed to load unsupported locale "$locale". This is likely '
    'an issue with the localizations generation tool. Please file an issue '
    'on GitHub with a reproducible sample app and the gen-l10n configuration '
    'that was used.',
  );
}
