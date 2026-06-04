/// Selector de idioma de la app.
///
/// Ofrece un [IconButton] ([LanguageSelectorButton], pensado para el `AppBar`)
/// que abre un diálogo con las opciones de idioma: "Idioma del sistema",
/// "Español" e "Inglés". Al elegir una, actualiza [localeProvider] (que persiste
/// la elección) y `MaterialApp.router` se reconstruye en caliente.
///
/// Solo iconos de Flutter ([Icons.language]); sin emojis. Los textos vienen de
/// [AppLocalizations].
library;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../l10n/app_localizations.dart';
import 'locale_controller.dart';

/// Botón de barra que abre el diálogo de selección de idioma.
class LanguageSelectorButton extends ConsumerWidget {
  const LanguageSelectorButton({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    return IconButton(
      icon: const Icon(Icons.language),
      tooltip: l10n.cambiarIdioma,
      onPressed: () => showLanguageDialog(context),
    );
  }
}

/// Muestra el diálogo de selección de idioma.
Future<void> showLanguageDialog(BuildContext context) {
  return showDialog<void>(
    context: context,
    builder: (context) => const _LanguageDialog(),
  );
}

class _LanguageDialog extends ConsumerWidget {
  const _LanguageDialog();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context)!;
    final current = ref.watch(localeProvider);
    final currentCode = current?.languageCode;

    return AlertDialog(
      title: Text(l10n.idioma),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          _LanguageOption(
            label: l10n.idiomaDelSistema,
            selected: currentCode == null,
            onTap: () => _select(context, ref, null),
          ),
          _LanguageOption(
            label: l10n.espanol,
            selected: currentCode == 'es',
            onTap: () => _select(context, ref, const Locale('es')),
          ),
          _LanguageOption(
            label: l10n.ingles,
            selected: currentCode == 'en',
            onTap: () => _select(context, ref, const Locale('en')),
          ),
        ],
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text(l10n.cancelar),
        ),
      ],
    );
  }

  void _select(BuildContext context, WidgetRef ref, Locale? locale) {
    ref.read(localeProvider.notifier).setLocale(locale);
    Navigator.of(context).pop();
  }
}

class _LanguageOption extends StatelessWidget {
  const _LanguageOption({
    required this.label,
    required this.selected,
    required this.onTap,
  });

  final String label;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return ListTile(
      title: Text(label),
      trailing: selected
          ? Icon(Icons.check, color: theme.colorScheme.primary)
          : null,
      onTap: onTap,
    );
  }
}
