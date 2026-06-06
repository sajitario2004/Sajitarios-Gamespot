import 'dart:io';

import 'package:integration_test/integration_test_driver_extended.dart';

/// Driver host para la auditoría visual: guarda en `/tmp/audit/NOMBRE.png` cada
/// captura que el test de integración pide vía `binding.takeScreenshot(name)`.
///
/// Uso:
/// ```sh
/// flutter drive \
///   --driver=test_driver/screenshot_driver.dart \
///   --target=integration_test/audit_screens_test.dart \
///   -d UDID_DEL_SIMULADOR
/// ```
Future<void> main() async {
  await integrationDriver(
    onScreenshot:
        (String name, List<int> bytes, [Map<String, Object?>? args]) async {
          final file = File('/tmp/audit/$name.png');
          await file.create(recursive: true);
          await file.writeAsBytes(bytes);
          return true;
        },
  );
}
