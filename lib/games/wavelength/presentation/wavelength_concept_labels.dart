/// Shared widget that renders the two spectrum concept labels (left/right poles)
/// as Flutter [Text] widgets so they are never clipped by canvas bounds.
library;

import 'package:flutter/material.dart';

import 'package:sajitarios_gamespot/core/theme/app_theme.dart';

/// A [Row] that displays [left] and [right] concept labels for a Wavelength
/// spectrum. Each label occupies half the available width, wraps up to 2 lines,
/// and never overflows regardless of text length or screen width.
///
/// Rendered outside the Flame [GameWidget] so that long Spanish concepts
/// (e.g. "muy peligroso" / "completamente inofensivo") always display in full.
class WavelengthConceptLabelsRow extends StatelessWidget {
  const WavelengthConceptLabelsRow({
    super.key,
    required this.left,
    required this.right,
  });

  final String left;
  final String right;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final style = theme.textTheme.labelMedium?.copyWith(
      color: AppTheme.neonCyan,
      fontWeight: FontWeight.w700,
    );
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Expanded(
          child: Text(
            left,
            style: style,
            textAlign: TextAlign.left,
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
          ),
        ),
        const SizedBox(width: 8),
        Expanded(
          child: Text(
            right,
            style: style,
            textAlign: TextAlign.right,
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
          ),
        ),
      ],
    );
  }
}
