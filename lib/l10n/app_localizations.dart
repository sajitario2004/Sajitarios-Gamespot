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

  /// Encabezado de la sección de rondas (oportunidades de voto) en la configuración.
  ///
  /// In es, this message translates to:
  /// **'Rondas'**
  String get setupRondas;

  /// Texto de ayuda con el rango de rondas permitido según el número de jugadores.
  ///
  /// In es, this message translates to:
  /// **'Oportunidades de voto para expulsar a los impostores. De {min} a {max} con estos jugadores.'**
  String setupRondasAyuda(int min, int max);

  /// Número de rondas seleccionado (singular/plural).
  ///
  /// In es, this message translates to:
  /// **'{n, plural, one{{n} ronda} other{{n} rondas}}'**
  String rondasContador(int n);

  /// Tooltip del botón para reducir el número de rondas.
  ///
  /// In es, this message translates to:
  /// **'Menos rondas'**
  String get menosRondas;

  /// Tooltip del botón para aumentar el número de rondas.
  ///
  /// In es, this message translates to:
  /// **'Más rondas'**
  String get masRondas;

  /// Título de la barra superior de la pantalla de votación.
  ///
  /// In es, this message translates to:
  /// **'Votación'**
  String get votacionTitulo;

  /// Instrucción principal de la pantalla de votación.
  ///
  /// In es, this message translates to:
  /// **'Votad a quien creáis impostor'**
  String get votacionInstruccion;

  /// Indicador de la ronda de votación en curso.
  ///
  /// In es, this message translates to:
  /// **'Ronda {actual} de {total}'**
  String votacionRondaXDeY(int actual, int total);

  /// Botón para expulsar al jugador seleccionado en la votación.
  ///
  /// In es, this message translates to:
  /// **'Expulsar'**
  String get votacionExpulsar;

  /// Mensaje de confirmación antes de expulsar a un jugador.
  ///
  /// In es, this message translates to:
  /// **'¿Expulsar a {nombre}?'**
  String votacionConfirmarExpulsion(String nombre);

  /// Mensaje cuando el jugador expulsado no era impostor o cuando gana el impostor al agotar las rondas.
  ///
  /// In es, this message translates to:
  /// **'El impostor sigue entre vosotros'**
  String get votacionImpostorSigue;

  /// Mensaje cuando los jugadores pillan a todos los impostores.
  ///
  /// In es, this message translates to:
  /// **'¡Habéis ganado!'**
  String get votacionJugadoresGanan;

  /// Feedback cuando el jugador expulsado sí era impostor.
  ///
  /// In es, this message translates to:
  /// **'{nombre} era impostor'**
  String votacionEraImpostor(String nombre);

  /// Título de la pantalla de desenlace (fin de partida sin revelar roles).
  ///
  /// In es, this message translates to:
  /// **'Fin de la partida'**
  String get finDePartidaTitulo;

  /// Texto de la cuenta atrás del caso especial 'Es un 10 pero' antes de sacar la carta.
  ///
  /// In es, this message translates to:
  /// **'Sacando carta en...'**
  String get sacandoCartaEn;

  /// Título de la barra superior en las pantallas del flujo de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Preguntas por puntos'**
  String get triviaTitulo;

  /// Título del juego 'Preguntas por puntos' en la tarjeta del menú.
  ///
  /// In es, this message translates to:
  /// **'Preguntas por puntos'**
  String get triviaMenuTitulo;

  /// Descripción del juego 'Preguntas por puntos' en la tarjeta del menú.
  ///
  /// In es, this message translates to:
  /// **'Responde preguntas de cultura general y otras temáticas. ¡El que más acierte gana!'**
  String get triviaMenuDescripcion;

  /// Encabezado de la sección de selección de temáticas en la configuración de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Temáticas'**
  String get triviaSetupTematicas;

  /// Texto de ayuda de la sección de temáticas.
  ///
  /// In es, this message translates to:
  /// **'Elige al menos una temática para la partida.'**
  String get triviaSetupTematicasAyuda;

  /// Texto de ayuda con el rango de jugadores de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Introduce de {min} a {max} jugadores.'**
  String triviaSetupRangoJugadores(int min, int max);

  /// Botón para empezar la partida de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Empezar partida'**
  String get triviaEmpezarPartida;

  /// Texto del botón de empezar mientras se inicia la partida de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Iniciando...'**
  String get triviaIniciando;

  /// Título del diálogo cuando no hay preguntas suficientes para la partida.
  ///
  /// In es, this message translates to:
  /// **'Sin preguntas'**
  String get triviaNoHayPreguntasTitulo;

  /// Mensaje del diálogo cuando no hay preguntas suficientes.
  ///
  /// In es, this message translates to:
  /// **'No hay suficientes preguntas para las temáticas y jugadores elegidos. Prueba con más temáticas.'**
  String get triviaNoHayPreguntasMensaje;

  /// Encabezado del ranking de victorias en la pantalla de setup de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Ranking de victorias'**
  String get triviaRankingVictorias;

  /// Mensaje cuando el ranking de victorias está vacío.
  ///
  /// In es, this message translates to:
  /// **'Aún no hay victorias registradas.'**
  String get triviaSinVictorias;

  /// Número de victorias de un jugador (singular/plural).
  ///
  /// In es, this message translates to:
  /// **'{n, plural, one{{n} victoria} other{{n} victorias}}'**
  String triviaVictorias(int n);

  /// Atribución de OpenTDB en la pantalla de setup de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Preguntas basadas en Open Trivia DB (CC BY-SA 4.0)'**
  String get triviaAtribucion;

  /// Instrucción de la pantalla de paso de móvil en Trivia.
  ///
  /// In es, this message translates to:
  /// **'Pásale el móvil a'**
  String get triviaPasaleElMovilA;

  /// Texto de ayuda en la pantalla de paso de móvil de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Cuando lo tenga, pulsa continuar para ver tu pregunta en privado.'**
  String get triviaPasaleElMovilAyuda;

  /// Etiqueta semántica de accesibilidad para el enunciado de la pregunta.
  ///
  /// In es, this message translates to:
  /// **'Pregunta'**
  String get triviaPregunta;

  /// Etiqueta semántica de una opción de respuesta.
  ///
  /// In es, this message translates to:
  /// **'Opción {letra}: {texto}'**
  String triviaOpcionLetra(String letra, String texto);

  /// Indicador de ronda en la pantalla de pregunta de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Ronda {actual} de {total}'**
  String triviaRondaXDeY(int actual, int total);

  /// Título de la pantalla de fin de partida de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Fin de la partida'**
  String get triviaFinDePartida;

  /// Mensaje cuando hay supervivientes que ganan en Trivia.
  ///
  /// In es, this message translates to:
  /// **'¡Habéis ganado!'**
  String get triviaGanadores;

  /// Mensaje cuando todos los jugadores fueron eliminados en Trivia.
  ///
  /// In es, this message translates to:
  /// **'Nadie ganó esta partida'**
  String get triviaNadieGano;

  /// Lista de ganadores separados por coma.
  ///
  /// In es, this message translates to:
  /// **'Ganadores: {nombres}'**
  String triviaGanadoresList(String nombres);

  /// Botón para jugar otra partida desde el fin de la partida de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Jugar otra'**
  String get triviaJugarOtra;

  /// Mensaje cuando la pantalla de Trivia no tiene partida.
  ///
  /// In es, this message translates to:
  /// **'No hay ninguna partida en curso.'**
  String get triviaNoHayPartida;

  /// Título de la barra superior en las pantallas del flujo de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Wavelength'**
  String get wavelengthTitulo;

  /// Texto de ayuda con el rango de jugadores de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Introduce de {min} a {max} jugadores.'**
  String wavelengthSetupRangoJugadores(int min, int max);

  /// Encabezado de la sección de rondas en la configuración de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Rondas'**
  String get wavelengthSetupRondas;

  /// Texto de ayuda del rango de rondas en Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Número de rondas a jugar. De {min} a {max}.'**
  String wavelengthSetupRondasAyuda(int min, int max);

  /// Botón para empezar la partida de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Empezar partida'**
  String get wavelengthEmpezarPartida;

  /// Texto del botón de empezar mientras se inicia la partida de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Iniciando...'**
  String get wavelengthIniciando;

  /// Título del diálogo cuando no hay espectros disponibles.
  ///
  /// In es, this message translates to:
  /// **'Sin espectros'**
  String get wavelengthSinEspectrosTitulo;

  /// Mensaje del diálogo cuando no hay espectros disponibles.
  ///
  /// In es, this message translates to:
  /// **'No hay espectros disponibles para jugar. Instala de nuevo la app para cargar los espectros de ejemplo.'**
  String get wavelengthSinEspectrosMensaje;

  /// Instrucción en la pantalla de pista para el psíquico.
  ///
  /// In es, this message translates to:
  /// **'Eres el PSIQUICO: solo TU ves el objetivo. Observa el dial, escribe una pista y pasa el movil al grupo.'**
  String get wavelengthClueScreenInstruccion;

  /// Etiqueta que indica quién es el psíquico en la ronda.
  ///
  /// In es, this message translates to:
  /// **'Psiquico'**
  String get wavelengthCluePsicoEtiqueta;

  /// Etiqueta de accesibilidad del dial en la pantalla del psíquico.
  ///
  /// In es, this message translates to:
  /// **'Dial de Wavelength, modo psiquico'**
  String get wavelengthDialSemanticsClue;

  /// Etiqueta de accesibilidad del dial en la pantalla del grupo.
  ///
  /// In es, this message translates to:
  /// **'Dial de Wavelength, mueve la aguja'**
  String get wavelengthDialSemanticsGuess;

  /// Etiqueta del campo de texto de la pista del psíquico.
  ///
  /// In es, this message translates to:
  /// **'Tu pista'**
  String get wavelengthClueFieldLabel;

  /// Texto de ejemplo del campo de pista.
  ///
  /// In es, this message translates to:
  /// **'Escribe una pista...'**
  String get wavelengthClueFieldHint;

  /// Botón para confirmar la pista y pasar el móvil al grupo.
  ///
  /// In es, this message translates to:
  /// **'Confirmar pista y pasar el movil'**
  String get wavelengthConfirmarPista;

  /// Instrucción principal de la pantalla de paso de móvil en Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Pasale el movil al GRUPO'**
  String get wavelengthPassDeviceInstruccion;

  /// Texto de ayuda en la pantalla de paso de móvil de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'No mires el objetivo. Cuando el grupo tenga el movil, pulsa continuar para que adivinen con el dial.'**
  String get wavelengthPassDeviceAyuda;

  /// Instrucción en la pantalla de adivinanza para el grupo.
  ///
  /// In es, this message translates to:
  /// **'GRUPO: movéis el dial hacia donde creéis que está el objetivo segun la pista.'**
  String get wavelengthGuessInstruccion;

  /// Etiqueta previa a la pista del psíquico en la pantalla de adivinanza.
  ///
  /// In es, this message translates to:
  /// **'Pista del psiquico:'**
  String get wavelengthPistaEtiqueta;

  /// Botón para confirmar la posición del dial y revelar el resultado.
  ///
  /// In es, this message translates to:
  /// **'Confirmar posicion del dial'**
  String get wavelengthConfirmarAdivinanza;

  /// Puntos obtenidos en esta ronda en la pantalla de revelación.
  ///
  /// In es, this message translates to:
  /// **'{pts, plural, one{{pts} punto} other{{pts} puntos}} esta ronda'**
  String wavelengthRevealPuntos(int pts);

  /// Puntuación acumulada mostrada en la pantalla de revelación.
  ///
  /// In es, this message translates to:
  /// **'Total: {pts} puntos'**
  String wavelengthRevealTotalPuntos(int pts);

  /// Indicador de ronda en la pantalla de revelación.
  ///
  /// In es, this message translates to:
  /// **'Ronda {actual} de {total}'**
  String wavelengthRevealRondaXDeY(int actual, int total);

  /// Botón para avanzar a la siguiente ronda tras la revelación.
  ///
  /// In es, this message translates to:
  /// **'Siguiente ronda'**
  String get wavelengthSiguienteRonda;

  /// Botón para ir a la pantalla de fin de partida desde la revelación.
  ///
  /// In es, this message translates to:
  /// **'Ver resultado'**
  String get wavelengthVerResultado;

  /// Título de la pantalla de fin de partida de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Fin de la partida'**
  String get wavelengthFinDePartida;

  /// Etiqueta de la puntuación final en el fin de partida de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Puntuación final'**
  String get wavelengthPuntuacionFinal;

  /// Puntuación total al final de la partida.
  ///
  /// In es, this message translates to:
  /// **'{pts} puntos'**
  String wavelengthPuntosTotales(int pts);

  /// Botón para jugar otra partida desde el fin de partida de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Jugar otra'**
  String get wavelengthJugarOtra;

  /// Mensaje cuando la pantalla de Wavelength no tiene partida.
  ///
  /// In es, this message translates to:
  /// **'No hay ninguna partida en curso.'**
  String get wavelengthNoHayPartida;

  /// Indicador de ronda en las pantallas del flujo de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Ronda {actual} de {total}'**
  String wavelengthRondaXDeY(int actual, int total);

  /// Etiqueta de banda de puntuación de Wavelength: máxima cercanía.
  ///
  /// In es, this message translates to:
  /// **'En el blanco'**
  String get wavelengthBandBlanco;

  /// Etiqueta de banda de puntuación de Wavelength: cerca.
  ///
  /// In es, this message translates to:
  /// **'Cerca'**
  String get wavelengthBandCerca;

  /// Etiqueta de banda de puntuación de Wavelength: lejos.
  ///
  /// In es, this message translates to:
  /// **'Lejos'**
  String get wavelengthBandLejos;

  /// Etiqueta de banda de puntuación de Wavelength: fallo.
  ///
  /// In es, this message translates to:
  /// **'Fallo'**
  String get wavelengthBandFallo;

  /// Etiqueta semántica del panel de puntuación en la revelación de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Puntuación: {banda}, {puntos} puntos'**
  String wavelengthRevealScoreSemantica(String banda, int puntos);

  /// Pista de accesibilidad para el botón Acierto de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Suma un punto al turno'**
  String get tabuAciertoHint;

  /// Pista de accesibilidad para el botón Saltar de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Pasa a la siguiente palabra sin puntuar'**
  String get tabuSaltarHint;

  /// Pista de accesibilidad para el botón Falta de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Marca una falta por decir una palabra prohibida'**
  String get tabuFaltaHint;

  /// Título de la barra superior en las pantallas del flujo de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Tabú'**
  String get tabuTitulo;

  /// Encabezado de la sección de equipos en la configuración de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Equipos'**
  String get tabuSetupEquipos;

  /// Etiqueta del campo del nombre del equipo A.
  ///
  /// In es, this message translates to:
  /// **'Equipo A'**
  String get tabuEquipoA;

  /// Texto de ejemplo del campo del equipo A.
  ///
  /// In es, this message translates to:
  /// **'Ej.: Los Rápidos'**
  String get tabuEquipoAHint;

  /// Etiqueta del campo del nombre del equipo B.
  ///
  /// In es, this message translates to:
  /// **'Equipo B'**
  String get tabuEquipoB;

  /// Texto de ejemplo del campo del equipo B.
  ///
  /// In es, this message translates to:
  /// **'Ej.: Los Creativos'**
  String get tabuEquipoBHint;

  /// Encabezado de la sección de duración del turno en la configuración de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Duración del turno'**
  String get tabuSetupTurno;

  /// Texto de ayuda de la sección de duración del turno.
  ///
  /// In es, this message translates to:
  /// **'Segundos que tiene cada equipo para describir palabras.'**
  String get tabuSetupTurnoAyuda;

  /// Duración del turno en segundos.
  ///
  /// In es, this message translates to:
  /// **'{n} seg'**
  String tabuSegundos(int n);

  /// Botón para empezar la partida de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Empezar partida'**
  String get tabuEmpezarPartida;

  /// Texto del botón de empezar mientras se inicia la partida de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Iniciando...'**
  String get tabuIniciando;

  /// Título del diálogo cuando no hay palabras de Tabú disponibles.
  ///
  /// In es, this message translates to:
  /// **'Sin palabras'**
  String get tabuSinPalabrasTitulo;

  /// Mensaje del diálogo cuando no hay palabras de Tabú disponibles.
  ///
  /// In es, this message translates to:
  /// **'No hay palabras disponibles para jugar. Instala de nuevo la app para cargar las palabras de ejemplo.'**
  String get tabuSinPalabrasMensaje;

  /// Error cuando el nombre del equipo A está vacío.
  ///
  /// In es, this message translates to:
  /// **'El nombre del equipo A no puede estar vacío.'**
  String get tabuErrorEquipoAVacio;

  /// Error cuando el nombre del equipo B está vacío.
  ///
  /// In es, this message translates to:
  /// **'El nombre del equipo B no puede estar vacío.'**
  String get tabuErrorEquipoBVacio;

  /// Error cuando los dos equipos tienen el mismo nombre.
  ///
  /// In es, this message translates to:
  /// **'Los equipos deben tener nombres distintos.'**
  String get tabuErrorEquiposDuplicados;

  /// Error cuando la duración del turno está fuera de rango.
  ///
  /// In es, this message translates to:
  /// **'La duración del turno no es válida.'**
  String get tabuErrorTurnoInvalido;

  /// Etiqueta de la lista de palabras prohibidas en la pantalla de turno.
  ///
  /// In es, this message translates to:
  /// **'Palabras prohibidas'**
  String get tabuProhibidas;

  /// Botón de acierto en la pantalla de turno de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Acierto'**
  String get tabuAcierto;

  /// Botón de saltar en la pantalla de turno de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Saltar'**
  String get tabuSaltar;

  /// Botón de falta en la pantalla de turno de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Falta'**
  String get tabuFalta;

  /// Contador de aciertos del turno actual (singular/plural).
  ///
  /// In es, this message translates to:
  /// **'{n, plural, one{{n} acierto} other{{n} aciertos}}'**
  String tabuAciertosContador(int n);

  /// Etiqueta semántica del tiempo restante del turno.
  ///
  /// In es, this message translates to:
  /// **'{n} segundos restantes'**
  String tabuTiempoRestante(int n);

  /// Título de la pantalla de marcador entre turnos.
  ///
  /// In es, this message translates to:
  /// **'Marcador'**
  String get tabuMarcador;

  /// Texto informativo del objetivo de victorias en la pantalla de marcador.
  ///
  /// In es, this message translates to:
  /// **'Objetivo: {n} victorias de ronda'**
  String tabuObjetivoVictorias(int n);

  /// Botón para comenzar el siguiente turno desde el marcador.
  ///
  /// In es, this message translates to:
  /// **'Siguiente turno'**
  String get tabuSiguienteTurno;

  /// Título de la pantalla de fin de partida de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Fin de la partida'**
  String get tabuFinDePartida;

  /// Etiqueta previa al nombre del equipo ganador.
  ///
  /// In es, this message translates to:
  /// **'Equipo ganador'**
  String get tabuGanadorLabel;

  /// Mensaje cuando la pantalla de Tabú no tiene partida activa.
  ///
  /// In es, this message translates to:
  /// **'No hay ninguna partida en curso.'**
  String get tabuNoHayPartida;

  /// Título de la barra superior en las pantallas del flujo de Yo Nunca.
  ///
  /// In es, this message translates to:
  /// **'Yo Nunca'**
  String get yoNuncaTitulo;

  /// Encabezado de la sección de selección de intensidades en la configuración de Yo Nunca.
  ///
  /// In es, this message translates to:
  /// **'Intensidades'**
  String get yoNuncaSetupIntensidades;

  /// Texto de ayuda de la sección de intensidades.
  ///
  /// In es, this message translates to:
  /// **'Elige al menos una intensidad para las frases.'**
  String get yoNuncaSetupIntensidadesAyuda;

  /// Etiqueta de la intensidad suave (apta para todos los públicos).
  ///
  /// In es, this message translates to:
  /// **'Suave'**
  String get yoNuncaIntensidadSuave;

  /// Etiqueta de la intensidad picante (contenido adulto).
  ///
  /// In es, this message translates to:
  /// **'Picante'**
  String get yoNuncaIntensidadPicante;

  /// Advertencia de contenido adulto mostrada al activar la intensidad picante.
  ///
  /// In es, this message translates to:
  /// **'Contenido explícito (+18). Esta opción incluye frases de contenido sexual explícito para adultos. Solo actívala si todos los jugadores son mayores de 18 años y dan su consentimiento.'**
  String get yoNuncaAdvertenciaPicante;

  /// Botón para empezar la sesión de Yo Nunca.
  ///
  /// In es, this message translates to:
  /// **'Empezar'**
  String get yoNuncaEmpezar;

  /// Error cuando no se ha seleccionado ninguna intensidad.
  ///
  /// In es, this message translates to:
  /// **'Elige al menos una intensidad para jugar.'**
  String get yoNuncaErrorSinIntensidades;

  /// Título del diálogo cuando no hay frases disponibles para las intensidades elegidas.
  ///
  /// In es, this message translates to:
  /// **'Sin frases'**
  String get yoNuncaSinFrasesTitulo;

  /// Mensaje del diálogo cuando no hay frases disponibles.
  ///
  /// In es, this message translates to:
  /// **'No hay frases disponibles para las intensidades elegidas. Prueba con otra intensidad.'**
  String get yoNuncaSinFrasesMensaje;

  /// Botón para sacar la siguiente frase en la pantalla de juego.
  ///
  /// In es, this message translates to:
  /// **'Siguiente'**
  String get yoNuncaSiguiente;

  /// Etiqueta semántica accesible de la frase actual.
  ///
  /// In es, this message translates to:
  /// **'Frase: {frase}'**
  String yoNuncaFraseSemantica(String frase);

  /// Mensaje cuando la pantalla de juego no tiene sesión activa.
  ///
  /// In es, this message translates to:
  /// **'No hay ninguna sesión en curso.'**
  String get yoNuncaNoHaySesion;

  /// Título de la barra superior en las pantallas del flujo de La Bomba.
  ///
  /// In es, this message translates to:
  /// **'La Bomba'**
  String get bombaTitulo;

  /// Encabezado de la sección de selección de modo en la configuración de La Bomba.
  ///
  /// In es, this message translates to:
  /// **'Modo de juego'**
  String get bombaSetupModo;

  /// Opción de modo sílaba en la configuración de La Bomba.
  ///
  /// In es, this message translates to:
  /// **'Sílaba'**
  String get bombaModoSilaba;

  /// Opción de modo categoría en la configuración de La Bomba.
  ///
  /// In es, this message translates to:
  /// **'Categoría'**
  String get bombaModoCategoria;

  /// Texto de ayuda con el rango de jugadores de La Bomba.
  ///
  /// In es, this message translates to:
  /// **'Introduce de {min} a {max} jugadores.'**
  String bombaSetupRangoJugadores(int min, int max);

  /// Botón para empezar la partida de La Bomba.
  ///
  /// In es, this message translates to:
  /// **'Empezar partida'**
  String get bombaEmpezarPartida;

  /// Texto del botón de empezar mientras se inicia la partida de La Bomba.
  ///
  /// In es, this message translates to:
  /// **'Iniciando...'**
  String get bombaIniciando;

  /// Título del diálogo cuando no hay prompts disponibles para el modo elegido.
  ///
  /// In es, this message translates to:
  /// **'Sin prompts'**
  String get bombaSinPromptsTitulo;

  /// Mensaje del diálogo cuando no hay prompts disponibles.
  ///
  /// In es, this message translates to:
  /// **'No hay prompts disponibles para el modo elegido. Reinstala la app para cargar los datos de ejemplo.'**
  String get bombaSinPromptsMensaje;

  /// Botón para pasar el móvil al siguiente jugador en La Bomba.
  ///
  /// In es, this message translates to:
  /// **'PASAR'**
  String get bombaPasar;

  /// Subtítulo explicativo cuando el modo es sílaba.
  ///
  /// In es, this message translates to:
  /// **'Sílaba — di una palabra que la contenga'**
  String get bombaTipoSilaba;

  /// Subtítulo explicativo cuando el modo es categoría.
  ///
  /// In es, this message translates to:
  /// **'Categoría — di una palabra de esta categoría'**
  String get bombaTipoCategoria;

  /// Etiqueta semántica accesible del portador actual de la bomba.
  ///
  /// In es, this message translates to:
  /// **'Portador: {nombre}'**
  String bombaPortadorSemantica(String nombre);

  /// Etiqueta semántica accesible del prompt activo.
  ///
  /// In es, this message translates to:
  /// **'Prompt: {texto}'**
  String bombaPromptSemantica(String texto);

  /// Título de la pantalla de explosión cuando la mecha expira.
  ///
  /// In es, this message translates to:
  /// **'¡BOOM!'**
  String get bombaExplosionTitulo;

  /// Texto que acompaña al nombre del jugador eliminado por la bomba.
  ///
  /// In es, this message translates to:
  /// **'ha sido eliminado'**
  String get bombaEliminado;

  /// Mensaje cuando la pantalla de La Bomba no tiene partida activa.
  ///
  /// In es, this message translates to:
  /// **'No hay ninguna partida en curso.'**
  String get bombaNoHayPartida;

  /// Título de la pantalla de fin de partida de La Bomba.
  ///
  /// In es, this message translates to:
  /// **'Fin de la partida'**
  String get bombaFinDePartida;

  /// Etiqueta previa al nombre del ganador en la pantalla de fin de partida.
  ///
  /// In es, this message translates to:
  /// **'Ganador'**
  String get bombaGanadorLabel;

  /// Tooltip del botón de reglas y título de la pantalla de reglas.
  ///
  /// In es, this message translates to:
  /// **'¿Cómo se juega?'**
  String get comoSeJuega;

  /// Regla 1 de Es un 10 pero.
  ///
  /// In es, this message translates to:
  /// **'Un jugador pulsa «Sacar carta».'**
  String get reglasEsUn10Pero1;

  /// Regla 2 de Es un 10 pero.
  ///
  /// In es, this message translates to:
  /// **'Una cuenta atrás de 5 segundos genera suspenso.'**
  String get reglasEsUn10Pero2;

  /// Regla 3 de Es un 10 pero.
  ///
  /// In es, this message translates to:
  /// **'Se revela una carta al azar entre As y 10.'**
  String get reglasEsUn10Pero3;

  /// Regla 4 de Es un 10 pero.
  ///
  /// In es, this message translates to:
  /// **'El grupo interpreta la carta como quiera. ¡Sin más reglas!'**
  String get reglasEsUn10Pero4;

  /// Regla 1 de El Impostor.
  ///
  /// In es, this message translates to:
  /// **'Decide cuántos jugadores, impostores y rondas habrá.'**
  String get reglasImpostor1;

  /// Regla 2 de El Impostor.
  ///
  /// In es, this message translates to:
  /// **'Pásate el móvil en orden: cada jugador ve su rol (ciudadano o IMPOSTOR) y lo cierra.'**
  String get reglasImpostor2;

  /// Regla 3 de El Impostor.
  ///
  /// In es, this message translates to:
  /// **'Los ciudadanos ven la palabra secreta; los impostores ven «IMPOSTOR» (y una pista si está activada).'**
  String get reglasImpostor3;

  /// Regla 4 de El Impostor.
  ///
  /// In es, this message translates to:
  /// **'Hay una ronda de debate: todos dan pistas sin revelar la palabra.'**
  String get reglasImpostor4;

  /// Regla 5 de El Impostor.
  ///
  /// In es, this message translates to:
  /// **'Cada ronda de votación, el grupo vota a quien crea impostor.'**
  String get reglasImpostor5;

  /// Regla 6 de El Impostor.
  ///
  /// In es, this message translates to:
  /// **'Los ciudadanos ganan si eliminan a todos los impostores; los impostores ganan si sobreviven todas las rondas.'**
  String get reglasImpostor6;

  /// Regla 1 de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Hasta 6 jugadores compiten respondiendo preguntas por turnos.'**
  String get reglasTrivia1;

  /// Regla 2 de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Las preguntas van de fácil a muy difícil a lo largo de las rondas.'**
  String get reglasTrivia2;

  /// Regla 3 de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Quien falla una pregunta queda eliminado de la partida.'**
  String get reglasTrivia3;

  /// Regla 4 de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Los jugadores que sobrevivan todas las rondas empatan como ganadores.'**
  String get reglasTrivia4;

  /// Regla 5 de Trivia.
  ///
  /// In es, this message translates to:
  /// **'Las victorias se guardan por nombre: ¡acumula puntos en el ranking!'**
  String get reglasTrivia5;

  /// Regla 1 de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Un jugador ve en secreto un objetivo marcado en un espectro entre dos conceptos opuestos.'**
  String get reglasWavelength1;

  /// Regla 2 de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Ese jugador da una sola pista verbal que sitúe el objetivo en el espectro.'**
  String get reglasWavelength2;

  /// Regla 3 de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'El resto del grupo mueve el dial para indicar dónde creen que está el objetivo.'**
  String get reglasWavelength3;

  /// Regla 4 de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Se puntúa según la cercanía al objetivo real: ¡cuanto más cerca, mejor!'**
  String get reglasWavelength4;

  /// Regla 5 de Wavelength.
  ///
  /// In es, this message translates to:
  /// **'Es cooperativo: el grupo gana o pierde junto acumulando puntos en todas las rondas.'**
  String get reglasWavelength5;

  /// Regla 1 de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Se forman dos equipos. Un jugador de turno ve una palabra y sus palabras prohibidas.'**
  String get reglasTabu1;

  /// Regla 2 de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Debe describir la palabra SIN decir ninguna de las palabras prohibidas.'**
  String get reglasTabu2;

  /// Regla 3 de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Su equipo tiene que adivinarla antes de que acabe el tiempo.'**
  String get reglasTabu3;

  /// Regla 4 de Tabú.
  ///
  /// In es, this message translates to:
  /// **'Si describe correctamente, el equipo anota un punto; si dice una prohibida, el turno pasa al equipo contrario.'**
  String get reglasTabu4;

  /// Regla 5 de Tabú.
  ///
  /// In es, this message translates to:
  /// **'El primer equipo en llegar a 3 victorias gana la partida.'**
  String get reglasTabu5;

  /// Regla 1 de Yo Nunca.
  ///
  /// In es, this message translates to:
  /// **'El móvil muestra una frase «Yo nunca…» al azar.'**
  String get reglasYoNunca1;

  /// Regla 2 de Yo Nunca.
  ///
  /// In es, this message translates to:
  /// **'Quien haya hecho eso debe reconocerlo (o beber, ¡como prefiera el grupo!).'**
  String get reglasYoNunca2;

  /// Regla 3 de Yo Nunca.
  ///
  /// In es, this message translates to:
  /// **'Se pasa el móvil al siguiente jugador para sacar la siguiente frase.'**
  String get reglasYoNunca3;

  /// Regla 4 de Yo Nunca.
  ///
  /// In es, this message translates to:
  /// **'Elige el nivel: suave para todas las edades, picante para mayores.'**
  String get reglasYoNunca4;

  /// Regla 1 de La Bomba.
  ///
  /// In es, this message translates to:
  /// **'Se muestra una sílaba o categoría en pantalla.'**
  String get reglasBomba1;

  /// Regla 2 de La Bomba.
  ///
  /// In es, this message translates to:
  /// **'El jugador actual dice una palabra válida (que contenga la sílaba o pertenezca a la categoría) y pasa el móvil rápido.'**
  String get reglasBomba2;

  /// Regla 3 de La Bomba.
  ///
  /// In es, this message translates to:
  /// **'Una bomba con temporizador oculto explota al azar en cualquier momento.'**
  String get reglasBomba3;

  /// Regla 4 de La Bomba.
  ///
  /// In es, this message translates to:
  /// **'El jugador que tenga el móvil cuando explote queda eliminado.'**
  String get reglasBomba4;

  /// Regla 5 de La Bomba.
  ///
  /// In es, this message translates to:
  /// **'El último jugador en pie gana la partida.'**
  String get reglasBomba5;
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
