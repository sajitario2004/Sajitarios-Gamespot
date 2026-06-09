import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/games/impostor/domain/game_config.dart';
import 'package:sajitarios_gamespot/games/impostor/domain/game_session.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/assign_roles_provider.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/impostor_flow_controller.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/pass_device_screen.dart';
import 'package:sajitarios_gamespot/games/impostor/presentation/reveal_screen.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

import 'support/fake_assign_roles_coordinator.dart';

/// Tests de accesibilidad: las pantallas del flujo del Impostor no desbordan con
/// `textScaler` alto (2.0). Las pantallas son scrollables (v0.36), así que el
/// excedente vertical se absorbe sin "RenderFlex overflowed".
void main() {
  /// Envuelve [child] en un MaterialApp localizado (es) con textScaler 2.0.
  Widget scaledApp(ProviderContainer container, Widget child) {
    return UncontrolledProviderScope(
      container: container,
      child: MaterialApp(
        locale: const Locale('es'),
        localizationsDelegates: AppLocalizations.localizationsDelegates,
        supportedLocales: AppLocalizations.supportedLocales,
        home: MediaQuery(
          data: const MediaQueryData(textScaler: TextScaler.linear(2.0)),
          child: child,
        ),
      ),
    );
  }

  Future<ProviderContainer> containerEnPass(GameSession session) async {
    final container = ProviderContainer(
      overrides: [
        assignRolesCoordinatorProvider.overrideWithValue(
          FakeAssignRolesCoordinator(session: session),
        ),
      ],
    );
    final config = GameConfig.create(
      players: session.players,
      nImpostores: 1,
    ).config!;
    await container
        .read(impostorFlowControllerProvider.notifier)
        .iniciar(config);
    return container;
  }

  group('Flujo del Impostor con textScaler 2.0 (sin overflow)', () {
    testWidgets('PassDeviceScreen no desborda con texto grande', (
      tester,
    ) async {
      final session = buildSession(
        nombres: ['Nacho', 'Iker', 'Lucía'],
        impostores: {'Nacho'},
      );
      final container = await containerEnPass(session);
      addTearDown(container.dispose);

      await tester.pumpWidget(scaledApp(container, const PassDeviceScreen()));
      await tester.pump();

      expect(tester.takeException(), isNull);
      expect(find.text('Nacho'), findsOneWidget);
    });

    testWidgets('RevealScreen no desborda con texto grande (rol IMPOSTOR)', (
      tester,
    ) async {
      final session = buildSession(
        nombres: ['Nacho', 'Iker', 'Lucía'],
        impostores: {'Nacho'},
        palabra: 'pirata',
        pista: 'barco',
      );
      final container = ProviderContainer(
        overrides: [
          assignRolesCoordinatorProvider.overrideWithValue(
            FakeAssignRolesCoordinator(session: session),
          ),
        ],
      );
      addTearDown(container.dispose);
      final config = GameConfig.create(
        players: session.players,
        nImpostores: 1,
        hintEnabled: true,
      ).config!;
      final notifier = container.read(impostorFlowControllerProvider.notifier);
      await notifier.iniciar(config);
      notifier.revelar();

      await tester.pumpWidget(scaledApp(container, const RevealScreen()));
      await tester.pump();

      // Revela el rol gigante "IMPOSTOR" + pista, el caso de más texto. El
      // botón puede quedar bajo el pliegue con textScaler 2.0, así que se
      // asegura su visibilidad antes de pulsar. Tras revelar se bombea una
      // duración fija (la revelación usa PulseGlow, animación infinita: no se
      // puede usar pumpAndSettle).
      final revelar = find.widgetWithText(FilledButton, 'Revelar');
      await tester.ensureVisible(revelar);
      await tester.pump();
      await tester.tap(revelar);
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 300));

      expect(tester.takeException(), isNull);
      expect(find.text('IMPOSTOR'), findsOneWidget);
    });
  });
}
