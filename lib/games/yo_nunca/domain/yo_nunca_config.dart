/// Configuración validada e inmutable para una sesión de Yo Nunca.
///
/// Solo obtenible vía [YoNuncaConfig.create], que valida las entradas y
/// devuelve un [YoNuncaConfigResult] (éxito o fallo) para que la capa de
/// presentación pueda reaccionar al error específico sin capturar excepciones.
library;

import 'package:sajitarios_gamespot/games/yo_nunca/domain/intensidad.dart';

/// Razones por las que no se pudo construir un [YoNuncaConfig].
enum YoNuncaConfigError {
  /// No se seleccionó ninguna [Intensidad].
  sinIntensidades,
}

/// Resultado de intentar construir un [YoNuncaConfig].
///
/// Contiene un [config] válido ([YoNuncaConfigResult.success]) o un [error]
/// que describe por qué falló la construcción ([YoNuncaConfigResult.failure]).
class YoNuncaConfigResult {
  const YoNuncaConfigResult._({this.config, this.error});

  /// Resultado exitoso que envuelve el [config] validado.
  const YoNuncaConfigResult.success(YoNuncaConfig config)
    : this._(config: config);

  /// Resultado fallido con el [error] correspondiente.
  const YoNuncaConfigResult.failure(YoNuncaConfigError error)
    : this._(error: error);

  /// La configuración válida, o `null` en caso de fallo.
  final YoNuncaConfig? config;

  /// La razón del fallo, o `null` en caso de éxito.
  final YoNuncaConfigError? error;

  /// `true` cuando la configuración se construyó con éxito.
  bool get isSuccess => config != null;
}

/// Configuración validada para una sesión de Yo Nunca.
///
/// Garantías: al menos una [Intensidad] seleccionada.
class YoNuncaConfig {
  const YoNuncaConfig._({required this.intensidades});

  /// Conjunto de intensidades seleccionadas para esta sesión. Al menos uno.
  final Set<Intensidad> intensidades;

  /// Construye un [YoNuncaConfig] desde entradas crudas, devolviendo un
  /// [YoNuncaConfigResult].
  ///
  /// Regla de validación: [intensidades] debe tener al menos un elemento.
  static YoNuncaConfigResult create({required Set<Intensidad> intensidades}) {
    if (intensidades.isEmpty) {
      return const YoNuncaConfigResult.failure(
        YoNuncaConfigError.sinIntensidades,
      );
    }
    return YoNuncaConfigResult.success(
      YoNuncaConfig._(intensidades: Set<Intensidad>.unmodifiable(intensidades)),
    );
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    if (other is! YoNuncaConfig) return false;
    if (other.intensidades.length != intensidades.length) return false;
    return other.intensidades.containsAll(intensidades);
  }

  @override
  int get hashCode {
    // XOR-fold so the result is independent of iteration order.
    var h = 0;
    for (final i in intensidades) {
      h ^= i.hashCode;
    }
    return h;
  }

  @override
  String toString() => 'YoNuncaConfig(intensidades: $intensidades)';
}
