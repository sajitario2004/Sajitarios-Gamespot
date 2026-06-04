import 'package:flutter/material.dart';

import 'package:sajitarios_gamespot/games/_shared/game_descriptor.dart';
import 'package:sajitarios_gamespot/games/es_un_10_pero/presentation/es_un_10_pero_screen.dart';
import 'package:sajitarios_gamespot/l10n/app_localizations.dart';

/// Descriptor del juego "Es un 10 pero" para el catálogo del hub.
///
/// Saca una carta aleatoria de la baraja (A–10) con su palo. Se registra en
/// `gameRegistryProvider`; el menú lo descubre a través de este descriptor sin
/// conocer la pantalla concreta.
class EsUn10PeroGame extends GameDescriptor {
  const EsUn10PeroGame();

  @override
  String get id => 'es_un_10_pero';

  @override
  String get title => 'Es un 10 pero';

  @override
  String get description => 'Saca una carta al azar de la A al 10.';

  @override
  String localizedTitle(BuildContext context) =>
      AppLocalizations.of(context)!.esUn10PeroMenuTitulo;

  @override
  String localizedDescription(BuildContext context) =>
      AppLocalizations.of(context)!.esUn10PeroMenuDescripcion;

  @override
  IconData get icon => Icons.style;

  @override
  Widget buildEntryScreen(BuildContext context) => const EsUn10PeroScreen();
}
