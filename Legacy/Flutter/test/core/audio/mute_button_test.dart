import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/core/audio/mute_button.dart';

import '../../support/localized_app.dart';

void main() {
  testWidgets('MuteButton alterna el icono y el estado al pulsar', (
    tester,
  ) async {
    await tester.pumpWidget(
      ProviderScope(
        child: localizedApp(
          const Scaffold(appBar: null, body: Center(child: MuteButton())),
        ),
      ),
    );

    // Arranca con sonido activo: volume_up.
    expect(find.byIcon(Icons.volume_up), findsOneWidget);
    expect(find.byIcon(Icons.volume_off), findsNothing);

    await tester.tap(find.byType(MuteButton));
    await tester.pump();

    // Tras pulsar: silenciado.
    expect(find.byIcon(Icons.volume_off), findsOneWidget);
    expect(find.byIcon(Icons.volume_up), findsNothing);
  });
}
