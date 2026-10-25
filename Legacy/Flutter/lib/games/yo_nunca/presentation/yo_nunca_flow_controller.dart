/// Controlador de flujo de una sesión de Yo Nunca.
///
/// Orquesta el recorrido setup → jugando (draw-and-pass) que las pantallas
/// consumen. No contiene UI: coordina la [YoNuncaConfig] actual, el pool de
/// [NeverStatement] y la [NeverStatement] actual.
///
/// Convenciones (Riverpod 3.x): se usa un [Notifier] expuesto con un
/// [NotifierProvider]. Las pantallas escuchan [yoNuncaFlowControllerProvider]
/// para leer el estado y llaman a los métodos del notifier vía `.notifier`.
library;

import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'package:sajitarios_gamespot/core/random/random_provider.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/draw_statement_use_case.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/never_statement.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/domain/yo_nunca_config.dart';
import 'package:sajitarios_gamespot/games/yo_nunca/presentation/yo_nunca_repositories_provider.dart';

export 'package:sajitarios_gamespot/games/yo_nunca/domain/intensidad.dart';
export 'package:sajitarios_gamespot/games/yo_nunca/domain/never_statement.dart';
export 'package:sajitarios_gamespot/games/yo_nunca/domain/yo_nunca_config.dart';

/// Fase actual del flujo de la sesión de Yo Nunca.
enum YoNuncaFase {
  /// Configuración inicial (selección de intensidades).
  setup,

  /// Jugando: una frase está visible y el grupo pasa el móvil.
  jugando,

  /// Error al iniciar (p. ej. pool vacío para las intensidades elegidas).
  error,
}

/// Tipo de error al iniciar la sesión, para que la UI pueda reaccionar de
/// forma específica sin depender de strings.
enum YoNuncaErrorKind {
  /// No hay declaraciones disponibles para las intensidades elegidas.
  sinFrases,
}

/// Estado inmutable del flujo de Yo Nunca.
class YoNuncaFlowState {
  const YoNuncaFlowState({
    required this.fase,
    this.config,
    this.pool = const [],
    this.fraseActual,
    this.seen = const {},
    this.errorKind,
  });

  /// Estado inicial: en configuración, sin sesión.
  const YoNuncaFlowState.initial() : this(fase: YoNuncaFase.setup);

  /// Fase actual del flujo.
  final YoNuncaFase fase;

  /// Configuración con la que se inició la sesión (o `null` antes de iniciar).
  final YoNuncaConfig? config;

  /// Pool completo de declaraciones para las intensidades seleccionadas.
  final List<NeverStatement> pool;

  /// Frase mostrada actualmente (solo en [YoNuncaFase.jugando]).
  final NeverStatement? fraseActual;

  /// IDs de frases ya mostradas en esta sesión (para el no-repeat).
  final Set<int> seen;

  /// Tipo de error (solo en [YoNuncaFase.error]).
  final YoNuncaErrorKind? errorKind;

  YoNuncaFlowState copyWith({
    YoNuncaFase? fase,
    YoNuncaConfig? config,
    List<NeverStatement>? pool,
    NeverStatement? fraseActual,
    Set<int>? seen,
    YoNuncaErrorKind? errorKind,
    bool clearError = false,
  }) {
    return YoNuncaFlowState(
      fase: fase ?? this.fase,
      config: config ?? this.config,
      pool: pool ?? this.pool,
      fraseActual: fraseActual ?? this.fraseActual,
      seen: seen ?? this.seen,
      errorKind: clearError ? null : (errorKind ?? this.errorKind),
    );
  }
}

/// Notifier que orquesta el flujo de una sesión de Yo Nunca.
class YoNuncaFlowController extends Notifier<YoNuncaFlowState> {
  @override
  YoNuncaFlowState build() => const YoNuncaFlowState.initial();

  // ── Iniciar ──────────────────────────────────────────────────────────────

  /// Inicia la sesión con [config]:
  /// 1. Carga el pool de frases del repositorio filtrado por intensidades.
  /// 2. Valida que el pool no esté vacío (→ [YoNuncaErrorKind.sinFrases]).
  /// 3. Saca la primera frase via [DrawStatementUseCase] y deja la fase en
  ///    [YoNuncaFase.jugando].
  Future<void> iniciar(YoNuncaConfig config) async {
    try {
      final repo = await ref.read(neverStatementRepositoryProvider.future);
      final pool = await repo.getByIntensidades(config.intensidades);

      if (pool.isEmpty) {
        state = YoNuncaFlowState(
          fase: YoNuncaFase.error,
          config: config,
          errorKind: YoNuncaErrorKind.sinFrases,
        );
        return;
      }

      final seen = <int>{};
      final primera = DrawStatementUseCase(ref.read(randomProvider))(
        pool: pool,
        intensidades: config.intensidades,
        seen: seen,
      );

      state = YoNuncaFlowState(
        fase: YoNuncaFase.jugando,
        config: config,
        pool: pool,
        fraseActual: primera,
        seen: Set<int>.unmodifiable(seen),
      );
    } catch (_) {
      state = YoNuncaFlowState(
        fase: YoNuncaFase.error,
        config: config,
        errorKind: YoNuncaErrorKind.sinFrases,
      );
    }
  }

  // ── Siguiente ─────────────────────────────────────────────────────────────

  /// Saca la siguiente frase del pool (sin repetir hasta agotarlo, luego
  /// rebaraja).
  ///
  /// No hace nada si no estamos en [YoNuncaFase.jugando].
  void siguiente() {
    if (state.fase != YoNuncaFase.jugando) return;
    final config = state.config;
    if (config == null) return;

    // seen es mutable internamente; creamos una copia mutable para el use case.
    final seen = Set<int>.of(state.seen);
    final frase = DrawStatementUseCase(ref.read(randomProvider))(
      pool: state.pool,
      intensidades: config.intensidades,
      seen: seen,
    );

    state = state.copyWith(
      fraseActual: frase,
      seen: Set<int>.unmodifiable(seen),
    );
  }

  // ── Reiniciar ─────────────────────────────────────────────────────────────

  /// Reinicia el flujo al estado inicial (setup, sin sesión).
  void reiniciar() {
    state = const YoNuncaFlowState.initial();
  }
}

/// Provider del controlador de flujo de Yo Nunca.
///
/// Las pantallas leen el estado con `ref.watch(yoNuncaFlowControllerProvider)`
/// y llaman a los métodos con
/// `ref.read(yoNuncaFlowControllerProvider.notifier)`.
final yoNuncaFlowControllerProvider =
    NotifierProvider<YoNuncaFlowController, YoNuncaFlowState>(
      YoNuncaFlowController.new,
    );
