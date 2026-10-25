import 'package:flutter_test/flutter_test.dart';

import 'package:sajitarios_gamespot/games/_shared/presentation/rules_screen.dart';

import '../../../support/localized_app.dart';

void main() {
  group('RulesScreen', () {
    const title = 'El Impostor';
    const steps = ['Paso uno.', 'Paso dos.', 'Paso tres.'];

    testWidgets('muestra el título del juego', (tester) async {
      await tester.pumpWidget(
        localizedApp(RulesScreen(gameTitle: title, steps: steps)),
      );
      await tester.pump();

      expect(find.text(title), findsOneWidget);
    });

    testWidgets('muestra la cabecera ¿Cómo se juega?', (tester) async {
      await tester.pumpWidget(
        localizedApp(RulesScreen(gameTitle: title, steps: steps)),
      );
      await tester.pump();

      expect(find.text('¿Cómo se juega?'), findsOneWidget);
    });

    testWidgets('renderiza todos los pasos de las reglas', (tester) async {
      await tester.pumpWidget(
        localizedApp(RulesScreen(gameTitle: title, steps: steps)),
      );
      await tester.pump();

      for (final step in steps) {
        expect(find.text(step), findsOneWidget);
      }
    });

    testWidgets('renderiza los números de paso', (tester) async {
      await tester.pumpWidget(
        localizedApp(RulesScreen(gameTitle: title, steps: steps)),
      );
      await tester.pump();

      for (var i = 1; i <= steps.length; i++) {
        expect(find.text('$i'), findsOneWidget);
      }
    });

    testWidgets('funciona con un único paso', (tester) async {
      await tester.pumpWidget(
        localizedApp(
          const RulesScreen(gameTitle: 'Juego', steps: ['El único paso.']),
        ),
      );
      await tester.pump();

      expect(find.text('El único paso.'), findsOneWidget);
      expect(find.text('1'), findsOneWidget);
    });
  });
}
