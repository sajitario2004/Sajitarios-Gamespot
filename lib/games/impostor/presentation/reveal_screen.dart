import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import 'package:sajitarios_gamespot/core/audio/audio_service.dart';
import 'package:sajitarios_gamespot/core/theme/app_theme.dart';
import 'package:sajitarios_gamespot/core/widgets/neon.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/role.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/abandon_game_dialog.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_routes.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Revelación del rol del jugador actual.
///
/// Muestra "Es el turno de {jugador}" y un botón grande "Revelar". El rol y la
/// palabra **no** son visibles hasta pulsar "Revelar":
/// - [Role.palabra]: muestra la palabra grande.
/// - [Role.impostor]: muestra "IMPOSTOR"; si la pista está activada
///   (`config.hintEnabled`) muestra además la pista.
///
/// El botón "Ocultar y pasar" llama a
/// `impostorFlowControllerProvider.notifier.avanzar()`: si era el último jugador
/// navega a `impostor-results`; si quedan jugadores vuelve a `impostor-pass`.
class RevealScreen extends ConsumerStatefulWidget {
  const RevealScreen({super.key});

  @override
  ConsumerState<RevealScreen> createState() => _RevealScreenState();
}

class _RevealScreenState extends ConsumerState<RevealScreen> {
  /// Si el rol del jugador actual ya está visible en pantalla.
  ///
  /// Es estado local de la pantalla: cada vez que se entra (un jugador nuevo)
  /// empieza oculto para no filtrar el rol al pasar el móvil.
  bool _revelado = false;

  /// Revela el rol del jugador actual y reproduce el SFX de revelación.
  ///
  /// El sonido respeta el toggle de silencio del [AudioService] y va envuelto
  /// (en el propio servicio) para no romper la UI si el audio falla.
  void _revelar() {
    setState(() => _revelado = true);
    ref.read(audioServiceProvider).playReveal();
  }

  void _ocultarYPasar() {
    final controller = ref.read(impostorFlowControllerProvider.notifier);
    final terminado = controller.avanzar();
    if (!mounted) return;
    if (terminado) {
      context.goNamed(kImpostorResultsRouteName);
    } else {
      context.goNamed(kImpostorPassRouteName);
    }
  }

  /// Gestiona el botón "atrás" del sistema durante la revelación.
  ///
  /// Dejar que el back navegue libremente dejaría el flujo en un estado
  /// incoherente (la pantalla cambiaría pero el `impostorFlowControllerProvider`
  /// seguiría apuntando al jugador actual). Por eso interceptamos el gesto:
  /// pedimos confirmación y, si el usuario quiere salir, reiniciamos el flujo y
  /// volvemos a la configuración. El avance hacia adelante ("Revelar" /
  /// "Ocultar y pasar") no se ve afectado.
  Future<void> _confirmarSalida() async {
    final salir = await abandonarPartidaDialog(context);
    if (salir != true || !mounted) return;
    ref.read(impostorFlowControllerProvider.notifier).reiniciar();
    if (!mounted) return;
    context.goNamed(kImpostorSetupRouteName);
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;
    final state = ref.watch(impostorFlowControllerProvider);
    final controller = ref.read(impostorFlowControllerProvider.notifier);

    final session = state.session;
    final jugador = controller.jugadorActual;

    // Defensa: si no hay sesión o jugador (estado inesperado), no hay nada que
    // revelar. Se muestra un mensaje neutro sin filtrar información.
    if (session == null || jugador == null) {
      return Scaffold(
        appBar: AppBar(
          title: NeonText(
            l10n.impostorTitulo,
            style:
                theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
          ),
        ),
        body: NeonBackground(
          child: SafeArea(
            child: Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(Icons.inbox_outlined, size: 64),
                    const SizedBox(height: 16),
                    Text(
                      l10n.noHayPartidaEnCurso,
                      textAlign: TextAlign.center,
                      style: theme.textTheme.titleLarge,
                    ),
                    const SizedBox(height: 24),
                    FilledButton.icon(
                      onPressed: () => context.goNamed(kImpostorSetupRouteName),
                      icon: const Icon(Icons.tune),
                      label: Text(l10n.configurarPartida),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      );
    }

    final role = session.roleOf(jugador) ?? Role.palabra;
    final hintEnabled = state.config?.hintEnabled ?? false;

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) {
        if (didPop) return;
        _confirmarSalida();
      },
      child: Scaffold(
        appBar: AppBar(
          title: NeonText(
            l10n.impostorTitulo,
            style:
                theme.appBarTheme.titleTextStyle ?? theme.textTheme.titleLarge,
          ),
        ),
        body: NeonBackground(
          child: SafeArea(
            child: Center(
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 520),
                // Hacemos el contenido scrolleable para absorber el excedente en
                // horizontal o con texto grande, manteniendo el centrado vertical
                // cuando hay espacio de sobra (ConstrainedBox(minHeight) +
                // IntrinsicHeight).
                child: LayoutBuilder(
                  builder: (context, constraints) {
                    return SingleChildScrollView(
                      child: ConstrainedBox(
                        constraints: BoxConstraints(
                          minHeight: constraints.maxHeight,
                        ),
                        child: IntrinsicHeight(
                          child: Padding(
                            padding: const EdgeInsets.all(24),
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Text(
                                  l10n.esElTurnoDe,
                                  textAlign: TextAlign.center,
                                  style: theme.textTheme.titleMedium?.copyWith(
                                    color: theme.colorScheme.onSurfaceVariant,
                                  ),
                                ),
                                const SizedBox(height: 8),
                                NeonText(
                                  jugador.name,
                                  textAlign: TextAlign.center,
                                  glowColor: AppTheme.neonCyan,
                                  style: theme.textTheme.headlineMedium
                                      ?.copyWith(
                                        color: AppTheme.textPrimary,
                                        fontWeight: FontWeight.w800,
                                      ),
                                ),
                                const SizedBox(height: 40),
                                Expanded(
                                  child: Center(
                                    // Transición suave (fundido + ligero
                                    // crecimiento) entre la carta oculta y la
                                    // revelación del rol. Es puramente visual: NO
                                    // altera la lógica de revelado (el rol sigue
                                    // oculto hasta pulsar "Revelar").
                                    child: AnimatedSwitcher(
                                      duration: const Duration(
                                        milliseconds: 350,
                                      ),
                                      switchInCurve: Curves.easeOutBack,
                                      switchOutCurve: Curves.easeIn,
                                      transitionBuilder: (child, animation) {
                                        return FadeTransition(
                                          opacity: animation,
                                          child: ScaleTransition(
                                            scale: Tween<double>(
                                              begin: 0.92,
                                              end: 1,
                                            ).animate(animation),
                                            child: child,
                                          ),
                                        );
                                      },
                                      child: _revelado
                                          ? _RevelacionRol(
                                              key: const ValueKey('revelado'),
                                              role: role,
                                              palabra: session.word.text,
                                              pista: session.word.hint,
                                              hintEnabled: hintEnabled,
                                            )
                                          : _CartaOculta(
                                              key: const ValueKey('oculta'),
                                              theme: theme,
                                            ),
                                    ),
                                  ),
                                ),
                                const SizedBox(height: 32),
                                SizedBox(
                                  width: double.infinity,
                                  child: _revelado
                                      ? NeonGlowWrapper(
                                          color: AppTheme.neonCyan,
                                          borderRadius: const BorderRadius.all(
                                            Radius.circular(24),
                                          ),
                                          child: FilledButton.icon(
                                            onPressed: _ocultarYPasar,
                                            icon: const Icon(
                                              Icons.visibility_off,
                                            ),
                                            label: Text(
                                              state.esUltimoJugador
                                                  ? l10n.ocultarYVerResultados
                                                  : l10n.ocultarYPasar,
                                            ),
                                            style: FilledButton.styleFrom(
                                              minimumSize:
                                                  const Size.fromHeight(56),
                                            ),
                                          ),
                                        )
                                      : NeonGlowWrapper(
                                          color: AppTheme.neonMagenta,
                                          intensity: 1.2,
                                          borderRadius: const BorderRadius.all(
                                            Radius.circular(24),
                                          ),
                                          child: FilledButton.icon(
                                            onPressed: _revelar,
                                            icon: const Icon(Icons.visibility),
                                            label: Text(l10n.revelar),
                                            style: FilledButton.styleFrom(
                                              minimumSize:
                                                  const Size.fromHeight(56),
                                            ),
                                          ),
                                        ),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                    );
                  },
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

/// Carta boca abajo: lo que se ve antes de pulsar "Revelar".
class _CartaOculta extends StatelessWidget {
  const _CartaOculta({required this.theme, super.key});

  final ThemeData theme;

  @override
  Widget build(BuildContext context) {
    return NeonPanel(
      borderColor: AppTheme.neonCyan,
      borderRadius: const BorderRadius.all(Radius.circular(24)),
      padding: const EdgeInsets.symmetric(vertical: 48, horizontal: 24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.help_outline, size: 72, color: AppTheme.neonCyan),
          const SizedBox(height: 16),
          Text(
            AppLocalizations.of(context)!.pulsaRevelar,
            textAlign: TextAlign.center,
            style: theme.textTheme.bodyLarge?.copyWith(
              color: AppTheme.textPrimary,
            ),
          ),
        ],
      ),
    );
  }
}

/// Contenido revelado del rol del jugador actual.
class _RevelacionRol extends StatelessWidget {
  const _RevelacionRol({
    required this.role,
    required this.palabra,
    required this.pista,
    required this.hintEnabled,
    super.key,
  });

  final Role role;
  final String palabra;
  final String pista;
  final bool hintEnabled;

  /// Resumen leído por lectores de pantalla al revelarse el rol.
  ///
  /// Va en un [Semantics] con `liveRegion`, de modo que la tecnología de apoyo
  /// anuncia el rol en cuanto aparece (sin necesidad de que el usuario navegue
  /// hasta el contenido).
  String _anuncio(AppLocalizations l10n) {
    if (role.esImpostor) {
      return hintEnabled
          ? l10n.tuRolEsImpostorConPista(pista)
          : l10n.tuRolEsImpostor;
    }
    return l10n.tuPalabraEsAnuncio(palabra);
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = AppLocalizations.of(context)!;

    if (role.esImpostor) {
      return Semantics(
        liveRegion: true,
        label: _anuncio(l10n),
        child: PulseGlow(
          minIntensity: 0.85,
          maxIntensity: 1.4,
          builder: (context, intensity) => NeonPanel(
            borderColor: AppTheme.neonMagenta,
            backgroundColor: AppTheme.surface,
            intensity: intensity,
            borderWidth: 2.0,
            borderRadius: const BorderRadius.all(Radius.circular(24)),
            padding: const EdgeInsets.symmetric(vertical: 40, horizontal: 24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(
                  Icons.theater_comedy,
                  size: 56,
                  color: AppTheme.neonMagenta,
                  shadows: neonTextShadows(
                    color: AppTheme.neonMagenta,
                    intensity: intensity,
                  ),
                ),
                const SizedBox(height: 16),
                NeonText(
                  l10n.impostorMayus,
                  textAlign: TextAlign.center,
                  glowColor: AppTheme.neonMagenta,
                  intensity: intensity * 1.5,
                  style: theme.textTheme.displayLarge?.copyWith(
                    color: AppTheme.textPrimary,
                    fontWeight: FontWeight.w900,
                    letterSpacing: 3.0,
                  ),
                ),
                if (hintEnabled) ...[
                  const SizedBox(height: 24),
                  Text(
                    l10n.setupPista,
                    style: theme.textTheme.labelLarge?.copyWith(
                      color: AppTheme.textMuted,
                    ),
                  ),
                  const SizedBox(height: 4),
                  NeonText(
                    pista,
                    textAlign: TextAlign.center,
                    glowColor: AppTheme.neonViolet,
                    style: theme.textTheme.headlineSmall?.copyWith(
                      color: AppTheme.textPrimary,
                    ),
                  ),
                ],
              ],
            ),
          ),
        ),
      );
    }

    return Semantics(
      liveRegion: true,
      label: _anuncio(l10n),
      child: PulseGlow(
        minIntensity: 0.8,
        maxIntensity: 1.2,
        builder: (context, intensity) => NeonPanel(
          borderColor: AppTheme.neonCyan,
          backgroundColor: AppTheme.surface,
          intensity: intensity,
          borderWidth: 2.0,
          borderRadius: const BorderRadius.all(Radius.circular(24)),
          padding: const EdgeInsets.symmetric(vertical: 40, horizontal: 24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                l10n.tuPalabraEs,
                style: theme.textTheme.labelLarge?.copyWith(
                  color: AppTheme.textMuted,
                ),
              ),
              const SizedBox(height: 12),
              FittedBox(
                fit: BoxFit.scaleDown,
                child: NeonText(
                  palabra,
                  textAlign: TextAlign.center,
                  glowColor: AppTheme.neonCyan,
                  intensity: intensity,
                  style: theme.textTheme.displayMedium?.copyWith(
                    color: AppTheme.textPrimary,
                    fontWeight: FontWeight.w900,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
