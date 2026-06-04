// Generador de efectos de sonido PLACEHOLDER — Sajitarios Gamespot.
//
// PLACEHOLDER: estos sonidos son marcadores de posición sintetizados
// programáticamente (tonos sine simples), NO audio final. Sustituye los .wav
// de assets/audio/ por sonido profesional cuando esté disponible.
//
// Ejecuta este generador con:  dart run tool/generate_sounds.dart
//
// Produce WAV PCM 16-bit mono (44.1 kHz) en assets/audio/, según las rutas que
// fija lib/core/assets/assets.dart (AppAudio):
//   assets/audio/card_flip.wav  (clic/tono corto descendente)
//   assets/audio/reveal.wav     (tono ascendente "ta-da" corto)
//   assets/audio/game_over.wav  (acorde menor descendente)
//
// Dart puro (dart:io + dart:typed_data + dart:math): no depende de Flutter.

import 'dart:io';
import 'dart:math' as math;
import 'dart:typed_data';

/// Frecuencia de muestreo (Hz). 44.1 kHz: estándar de audio.
const int sampleRate = 44100;

void main() {
  final dir = Directory('assets/audio');
  if (!dir.existsSync()) dir.createSync(recursive: true);

  // card_flip: clic corto, dos tonos descendentes rápidos (~120 ms).
  final cardFlip = <double>[
    ..._tone(frequency: 880, durationMs: 50, volume: 0.55),
    ..._tone(frequency: 620, durationMs: 70, volume: 0.45),
  ];
  _writeWav('assets/audio/card_flip.wav', _applyEnvelope(cardFlip));

  // reveal: tono ascendente "ta-da" corto (~260 ms).
  final reveal = <double>[
    ..._tone(frequency: 523, durationMs: 90, volume: 0.5), // C5
    ..._tone(frequency: 659, durationMs: 90, volume: 0.5), // E5
    ..._tone(frequency: 784, durationMs: 120, volume: 0.55), // G5
  ];
  _writeWav('assets/audio/reveal.wav', _applyEnvelope(reveal));

  // game_over: acorde menor descendente (~500 ms), tono más grave/sombrío.
  final gameOver = <double>[
    ..._chord(frequencies: [440, 523, 622], durationMs: 200, volume: 0.4), // Am
    ..._chord(frequencies: [349, 415, 523], durationMs: 300, volume: 0.4), // Fm
  ];
  _writeWav('assets/audio/game_over.wav', _applyEnvelope(gameOver));

  stdout.writeln('Sonidos generados en assets/audio/ (PLACEHOLDER).');
}

/// Genera muestras de una onda sine de [frequency] Hz durante [durationMs] ms.
List<double> _tone({
  required double frequency,
  required int durationMs,
  required double volume,
}) {
  final count = (sampleRate * durationMs / 1000).round();
  final samples = List<double>.filled(count, 0);
  for (var i = 0; i < count; i++) {
    final t = i / sampleRate;
    samples[i] = volume * math.sin(2 * math.pi * frequency * t);
  }
  return samples;
}

/// Genera un acorde sumando varias ondas sine (normalizado por nº de notas).
List<double> _chord({
  required List<double> frequencies,
  required int durationMs,
  required double volume,
}) {
  final count = (sampleRate * durationMs / 1000).round();
  final samples = List<double>.filled(count, 0);
  for (var i = 0; i < count; i++) {
    final t = i / sampleRate;
    var sum = 0.0;
    for (final f in frequencies) {
      sum += math.sin(2 * math.pi * f * t);
    }
    samples[i] = volume * (sum / frequencies.length);
  }
  return samples;
}

/// Aplica un envolvente attack/release suave para evitar clics en los bordes.
List<double> _applyEnvelope(List<double> samples) {
  final n = samples.length;
  if (n == 0) return samples;
  // Rampas de ~5 ms (o menos si el clip es muy corto).
  final ramp = math.min((sampleRate * 0.005).round(), n ~/ 2);
  final out = List<double>.of(samples);
  for (var i = 0; i < ramp; i++) {
    final gain = i / ramp;
    out[i] *= gain;
    out[n - 1 - i] *= gain;
  }
  return out;
}

/// Escribe [samples] (rango [-1, 1]) como WAV PCM 16-bit mono.
void _writeWav(String path, List<double> samples) {
  const channels = 1;
  const bitsPerSample = 16;
  final byteRate = sampleRate * channels * bitsPerSample ~/ 8;
  final blockAlign = channels * bitsPerSample ~/ 8;
  final dataSize = samples.length * blockAlign;

  final bytes = BytesBuilder();

  void writeString(String s) => bytes.add(s.codeUnits);
  void writeUint32(int v) {
    final b = ByteData(4)..setUint32(0, v, Endian.little);
    bytes.add(b.buffer.asUint8List());
  }

  void writeUint16(int v) {
    final b = ByteData(2)..setUint16(0, v, Endian.little);
    bytes.add(b.buffer.asUint8List());
  }

  // Cabecera RIFF.
  writeString('RIFF');
  writeUint32(36 + dataSize); // tamaño del chunk RIFF
  writeString('WAVE');

  // Sub-chunk fmt.
  writeString('fmt ');
  writeUint32(16); // tamaño del sub-chunk fmt (PCM)
  writeUint16(1); // formato de audio: 1 = PCM
  writeUint16(channels);
  writeUint32(sampleRate);
  writeUint32(byteRate);
  writeUint16(blockAlign);
  writeUint16(bitsPerSample);

  // Sub-chunk data.
  writeString('data');
  writeUint32(dataSize);

  final pcm = ByteData(dataSize);
  for (var i = 0; i < samples.length; i++) {
    final clamped = samples[i].clamp(-1.0, 1.0);
    final value = (clamped * 32767).round();
    pcm.setInt16(i * 2, value, Endian.little);
  }
  bytes.add(pcm.buffer.asUint8List());

  File(path).writeAsBytesSync(bytes.toBytes());
}
