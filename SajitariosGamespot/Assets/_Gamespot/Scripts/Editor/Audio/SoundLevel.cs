using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Gamespot.Editor.Audio
{
    /// <summary>
    /// Nivel de los efectos de sonido (V059n: el autor los oía «exageradamente fuertes»; V059o: «siguen demasiado
    /// altos, bájalos a la mitad», −6 dB más). Todos se dejan a la misma sonoridad, <see cref="TargetDb"/>, con el pico
    /// como mucho a <see cref="PeakCeilingDb"/>. La sonoridad es el RMS
    /// de la parte que suena (ventanas de 10 ms por encima de −60 dB) después de quitar los graves que el altavoz de
    /// un móvil no reproduce (paso alto de 200 Hz): así la explosión, que es casi todo graves, no se queda corta.
    /// WAV de 16 bit mono, como los de <c>Resources/Audio</c>.
    /// </summary>
    public static class SoundLevel
    {
        public const float TargetDb = -26f;
        public const float PeakCeilingDb = -12f;
        public const float HighPassHz = 200f;
        public const int SampleRate = 44100;

        /// <summary>Sonoridad (dBFS) tal como la mide <see cref="Normalize"/>.</summary>
        public static float LoudnessDb(float[] samples)
        {
            var filtered = HighPass(HighPass(samples));
            var window = SampleRate / 100;
            double sum = 0;
            var count = 0;
            for (var start = 0; start < filtered.Length; start += window)
            {
                var end = Math.Min(filtered.Length, start + window);
                double chunk = 0;
                for (var i = start; i < end; i++) chunk += filtered[i] * filtered[i];
                if (Math.Sqrt(chunk / (end - start)) < 0.001) continue;
                sum += chunk;
                count += end - start;
            }
            return count == 0 ? -120f : ToDb((float)Math.Sqrt(sum / count));
        }

        public static float PeakDb(float[] samples)
        {
            var peak = 0f;
            foreach (var s in samples) peak = Mathf.Max(peak, Mathf.Abs(s));
            return ToDb(peak);
        }

        /// <summary>Escala a <see cref="TargetDb"/> sin pasar de <see cref="PeakCeilingDb"/> de pico.</summary>
        public static float[] Normalize(float[] samples)
        {
            // Unas pocas vueltas: al bajar el volumen, la cola de algunos sonidos cae por debajo del umbral de "lo
            // que suena" y la medida cambia (la explosión se quedaba 0,9 dB por encima).
            for (var pass = 0; pass < 4; pass++)
            {
                var loudness = LoudnessDb(samples);
                if (loudness <= -120f) return samples;
                var gainDb = Mathf.Min(TargetDb - loudness, PeakCeilingDb - PeakDb(samples));
                if (Mathf.Abs(gainDb) < 0.05f) break;
                var gain = Mathf.Pow(10f, gainDb / 20f);
                for (var i = 0; i < samples.Length; i++) samples[i] *= gain;
            }
            return samples;
        }

        public static float[] ReadWav(string path)
        {
            var bytes = File.ReadAllBytes(path);
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                if (new string(reader.ReadChars(4)) != "RIFF") throw new FormatException($"{path} no es un WAV");
                reader.ReadInt32();
                if (new string(reader.ReadChars(4)) != "WAVE") throw new FormatException($"{path} no es un WAV");
                short channels = 0, bits = 0;
                while (reader.BaseStream.Position + 8 <= reader.BaseStream.Length)
                {
                    var id = new string(reader.ReadChars(4));
                    var size = reader.ReadInt32();
                    if (id == "fmt ")
                    {
                        reader.ReadInt16();
                        channels = reader.ReadInt16();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt16();
                        bits = reader.ReadInt16();
                        reader.BaseStream.Position += size - 16;
                    }
                    else if (id == "data")
                    {
                        if (channels != 1 || bits != 16) throw new FormatException($"{path}: se espera mono de 16 bit");
                        var samples = new List<float>(size / 2);
                        for (var i = 0; i < size / 2; i++) samples.Add(reader.ReadInt16() / 32768f);
                        return samples.ToArray();
                    }
                    else
                    {
                        reader.BaseStream.Position += size + (size & 1);
                    }
                }
            }
            throw new FormatException($"{path} no tiene datos");
        }

        public static byte[] ToWav(float[] samples)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                var dataBytes = samples.Length * 2;
                writer.Write(new[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + dataBytes);
                writer.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(SampleRate);
                writer.Write(SampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(new[] { 'd', 'a', 't', 'a' });
                writer.Write(dataBytes);
                foreach (var s in samples) writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(s, -1f, 1f) * short.MaxValue));
                writer.Flush();
                return stream.ToArray();
            }
        }

        private static float ToDb(float value) => value > 0f ? 20f * Mathf.Log10(value) : -120f;

        /// <summary>Paso alto de primer orden en <see cref="HighPassHz"/> (dos seguidos dan 12 dB por octava).</summary>
        private static float[] HighPass(float[] input)
        {
            var rc = 1f / (2f * Mathf.PI * HighPassHz);
            var dt = 1f / SampleRate;
            var alpha = rc / (rc + dt);
            var output = new float[input.Length];
            for (var i = 1; i < input.Length; i++) output[i] = alpha * (output[i - 1] + input[i] - input[i - 1]);
            return output;
        }
    }
}
