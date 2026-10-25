/// Intensidad de una declaración "Yo nunca…".
///
/// Pure domain enum — sin imports de Flutter ni persistencia.
library;

/// Nivel de intensidad de una declaración.
///
/// [suave] es apta para todos los públicos; [picante] es contenido adulto
/// y requiere advertencia en la UI.
enum Intensidad {
  /// Apta para todos los públicos: situaciones cotidianas, viajes, comida, etc.
  suave,

  /// Contenido adulto/picante. La UI debe mostrar una advertencia previa.
  picante;

  /// Etiqueta legible en español.
  String get displayName => switch (this) {
    Intensidad.suave => 'Suave',
    Intensidad.picante => 'Picante',
  };
}
