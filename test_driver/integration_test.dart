import 'package:integration_test/integration_test_driver.dart';

/// Script host para ejecutar los tests de `integration_test/` en un
/// dispositivo/simulador con `flutter drive`:
///
/// ```sh
/// flutter drive \
///   --driver=test_driver/integration_test.dart \
///   --target=integration_test/impostor_flow_test.dart
/// ```
///
/// (En este entorno no hay dispositivo Android/iOS conectado ni plataforma
/// macOS/web generada, por lo que la ejecución se documenta pero no se corre
/// aquí; los tests son válidos y `flutter analyze` pasa sin avisos.)
Future<void> main() => integrationDriver();
