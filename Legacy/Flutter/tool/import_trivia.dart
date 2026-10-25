// import_trivia.dart — Infrastructure tool for fetching trivia questions
// from the Open Trivia Database (OpenTDB) and emitting them in the format
// expected by assets/seed/trivia_questions.json.
//
// USAGE:
//   dart tool/import_trivia.dart [--category=<id>] [--difficulty=<easy|medium|hard>] [--amount=<n>]
//
// EXAMPLES:
//   dart tool/import_trivia.dart --category=11 --difficulty=easy --amount=50
//   dart tool/import_trivia.dart --category=15 --difficulty=hard --amount=20
//
// OUTPUT:
//   Writes a JSON array to stdout. Redirect to a file if needed:
//     dart tool/import_trivia.dart --category=11 > /tmp/opentdb_result.json
//
// ─────────────────────────────────────────────────────────────────────────────
// IMPORTANT NOTES:
//
// 1. LANGUAGE: OpenTDB content is in ENGLISH. Every question and answer
//    returned by this tool MUST be translated to neutral Spanish and reviewed
//    for factual accuracy before merging into assets/seed/trivia_questions.json.
//
// 2. LICENSE: OpenTDB is licensed under CC BY-SA 4.0.
//    Attribution is required in the app UI (to be added in the UI slice,
//    v0.58). When questions from OpenTDB are included in the seed, add a
//    visible notice in the app: "Some questions sourced from Open Trivia DB
//    (opentdb.com) — CC BY-SA 4.0".
//
// 3. SCHEMA: The emitted JSON uses the same shape as trivia_questions.json:
//    { "tematica": "id", "difficulty": "facil|dificil|muyDificil",
//      "enunciado": "...", "opciones": ["a","b","c","d"], "correcta": 0-3 }
//    The tematica id defaults to "cultura_general"; pass --tematica to override.
//
// 4. OPTION ORDER: options are placed in the order: correct answer first,
//    then the three wrong answers in the order returned by OpenTDB.
//    "correcta" is therefore always 0 in the raw output. Shuffle manually
//    if needed before merging, updating "correcta" accordingly.
//
// 5. ENCODING: OpenTDB returns base64-encoded text when using encode=base64.
//    This tool decodes it via dart:convert. No HTML entities to handle.
//
// 6. NO NEW DEPENDENCIES: This tool uses dart:io's HttpClient to avoid
//    adding the `http` package to pubspec.yaml (which would affect app build).
//
// ─────────────────────────────────────────────────────────────────────────────
// OPENTDB CATEGORY IDS (partial reference):
//   9  = General Knowledge      10 = Books
//   11 = Film                   12 = Music
//   14 = Television             15 = Video Games
//   17 = Science & Nature       18 = Science: Computers
//   19 = Science: Mathematics   20 = Mythology
//   21 = Sports                 22 = Geography
//   23 = History                27 = Animals
//
// Full list: https://opentdb.com/api_category.php
// ─────────────────────────────────────────────────────────────────────────────

// ignore_for_file: avoid_print

import 'dart:convert';
import 'dart:io';

Future<void> main(List<String> args) async {
  // ── Parse arguments ────────────────────────────────────────────────────────
  final parsedArgs = _parseArgs(args);

  final int? category = parsedArgs['category'] != null
      ? int.tryParse(parsedArgs['category']!)
      : null;
  final String difficulty = parsedArgs['difficulty'] ?? 'easy';
  final int amount = int.tryParse(parsedArgs['amount'] ?? '50') ?? 50;
  final String tematica = parsedArgs['tematica'] ?? 'cultura_general';

  if (!{'easy', 'medium', 'hard'}.contains(difficulty)) {
    stderr.writeln(
      'ERROR: --difficulty must be one of: easy, medium, hard. Got: $difficulty',
    );
    exitCode = 1;
    return;
  }

  // ── Build URL ──────────────────────────────────────────────────────────────
  final queryParams = <String, String>{
    'amount': '$amount',
    'type': 'multiple',
    'difficulty': difficulty,
    // Use base64 encoding to avoid URL encoding issues with special characters.
    'encode': 'base64',
  };
  if (category != null) {
    queryParams['category'] = '$category';
  }

  final uri = Uri.https('opentdb.com', '/api.php', queryParams);
  stderr.writeln('Fetching: $uri');

  // ── HTTP request using dart:io HttpClient (no extra dependencies) ──────────
  final List<dynamic> results;
  try {
    results = await _fetchOpenTdb(uri);
  } on HttpException catch (e) {
    stderr.writeln('HTTP error: $e');
    exitCode = 1;
    return;
  } on FormatException catch (e) {
    stderr.writeln('JSON parse error: $e');
    exitCode = 1;
    return;
  }

  if (results.isEmpty) {
    stderr.writeln(
      'WARNING: OpenTDB returned 0 results. '
      'Try a smaller --amount or a different --category/--difficulty.',
    );
    print('[]');
    return;
  }

  // ── Map to seed schema ─────────────────────────────────────────────────────
  final output = <Map<String, Object?>>[];

  for (final raw in results) {
    final item = raw as Map<String, dynamic>;

    final enunciado = _decodeBase64Text(item['question'] as String? ?? '');
    final correctAnswer = _decodeBase64Text(
      item['correct_answer'] as String? ?? '',
    );
    final incorrectAnswers = (item['incorrect_answers'] as List<dynamic>)
        .map((e) => _decodeBase64Text(e as String))
        .toList();

    // Correct answer is placed first (index 0); shuffle when merging if needed.
    final opciones = <String>[correctAnswer, ...incorrectAnswers];

    output.add(<String, Object?>{
      'tematica': tematica,
      'difficulty': _mapDifficulty(difficulty),
      'enunciado': enunciado,
      'opciones': opciones,
      'correcta': 0, // correct_answer is always at index 0 (see note 4 above)
    });
  }

  // ── Emit JSON ──────────────────────────────────────────────────────────────
  final encoder = JsonEncoder.withIndent('  ');
  print(encoder.convert(output));

  stderr.writeln(
    'Done. ${output.length} questions fetched '
    '(category=${category ?? "any"}, difficulty=$difficulty).',
  );
  stderr.writeln(
    'REMINDER: Translate to Spanish and review before merging into the seed.',
  );
}

// ── Helpers ────────────────────────────────────────────────────────────────

/// Fetches the OpenTDB API and returns the `results` array from the response.
///
/// Throws [HttpException] on non-200 status or if the OpenTDB response code
/// indicates an error. Throws [FormatException] if the body is not valid JSON.
Future<List<dynamic>> _fetchOpenTdb(Uri uri) async {
  final client = HttpClient();
  client.connectionTimeout = const Duration(seconds: 15);

  try {
    final request = await client.getUrl(uri);
    final response = await request.close();

    if (response.statusCode != 200) {
      throw HttpException('Unexpected status ${response.statusCode} for $uri');
    }

    final body = await response.transform(utf8.decoder).join();
    final decoded = jsonDecode(body) as Map<String, dynamic>;

    final responseCode = decoded['response_code'] as int?;
    if (responseCode != 0) {
      // OpenTDB response codes:
      //   0 = Success
      //   1 = No results (not enough questions for the query)
      //   2 = Invalid parameter
      //   3 = Token not found
      //   4 = Token empty
      //   5 = Rate limit
      throw HttpException(
        'OpenTDB response_code=$responseCode. '
        'If 1: reduce --amount. If 5: wait ~5 seconds and retry.',
      );
    }

    return decoded['results'] as List<dynamic>;
  } finally {
    client.close();
  }
}

/// Decodes a base64-encoded string returned by OpenTDB when using
/// `encode=base64`. Falls back to raw string if decoding fails.
String _decodeBase64Text(String encoded) {
  try {
    final bytes = base64.decode(encoded);
    return utf8.decode(bytes);
  } on FormatException {
    // Already plain text or unknown encoding — return as-is.
    return encoded;
  }
}

/// Maps OpenTDB difficulty strings to the app's Difficulty enum names.
String _mapDifficulty(String openTdbDifficulty) => switch (openTdbDifficulty) {
  'easy' => 'facil',
  'medium' => 'dificil',
  'hard' => 'muyDificil',
  _ => 'facil',
};

/// Minimal CLI argument parser that handles `--key=value` and `--key value`.
Map<String, String> _parseArgs(List<String> args) {
  final result = <String, String>{};
  var i = 0;
  while (i < args.length) {
    final arg = args[i];
    if (arg.startsWith('--')) {
      final withoutDashes = arg.substring(2);
      final eqIndex = withoutDashes.indexOf('=');
      if (eqIndex != -1) {
        final key = withoutDashes.substring(0, eqIndex);
        final value = withoutDashes.substring(eqIndex + 1);
        result[key] = value;
      } else if (i + 1 < args.length && !args[i + 1].startsWith('--')) {
        result[withoutDashes] = args[i + 1];
        i++;
      }
    }
    i++;
  }
  return result;
}
