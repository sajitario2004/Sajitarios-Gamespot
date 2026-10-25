using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Gamespot.Editor.Audio
{
    /// <summary>
    /// Genera por código los efectos de sonido nuevos (V059d, sin descargas): 44,1 kHz, mono, 16 bit, en
    /// <c>Resources/Audio</c>. Son sustitutos propios, como los WAV que heredó el Flutter; se pueden cambiar por
    /// sonidos finales con el mismo nombre. El ruido usa una semilla fija: el resultado es siempre el mismo.
    /// Desde V059n todos salen al mismo volumen (<see cref="SoundLevel"/>), también los tres heredados del Flutter,
    /// que se vuelven a nivelar en su sitio (nivelar dos veces da lo mismo).
    /// Menú: Gamespot → Audio → Generar y nivelar los sonidos.
    /// </summary>
    public static class SoundSynth
    {
        public const int SampleRate = 44100;
        private const string Folder = "Assets/_Gamespot/Resources/Audio";

        /// <summary>Los WAV que heredó el Flutter: no se generan, solo se nivelan.</summary>
        public static readonly IReadOnlyList<string> Inherited = new[] { "card_flip", "reveal", "game_over" };

        private static readonly Dictionary<string, Func<float[]>> Sounds = new Dictionary<string, Func<float[]>>
        {
            ["tick"] = () => Click(2200f, 0.05f, 11),
            ["tock"] = () => Click(1500f, 0.05f, 12),
            ["explosion"] = Explosion,
            ["beep"] = Beep,
            ["buzzer"] = Buzzer,
            ["correct"] = Correct,
            ["wrong"] = Wrong,
            ["chime"] = Chime,
            ["swipe"] = Swipe
        };

        [MenuItem("Gamespot/Audio/Generar y nivelar los sonidos")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(Folder);
            foreach (var pair in Sounds)
            {
                Write($"{Folder}/{pair.Key}.wav", Finish(pair.Value()));
            }
            foreach (var name in Inherited)
            {
                var path = $"{Folder}/{name}.wav";
                Write(path, SoundLevel.Normalize(SoundLevel.ReadWav(path)));
            }
            Debug.Log($"[Audio] Generados {Sounds.Count} sonidos y nivelados {Inherited.Count} heredados en {Folder}");
        }

        private static void Write(string path, float[] samples)
        {
            File.WriteAllBytes(path, SoundLevel.ToWav(samples));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        // ------------------------------------------------------------------ Sonidos

        /// <summary>Tic o tac del reloj de La Bomba: chasquido de ruido y un tono que se apaga enseguida.</summary>
        private static float[] Click(float freq, float seconds, int seed)
        {
            var buffer = Buffer(seconds);
            var rng = new System.Random(seed);
            var low = 0f;
            for (var i = 0; i < buffer.Length; i++)
            {
                var t = Time(i);
                var noise = (float)(rng.NextDouble() * 2 - 1);
                low += 0.35f * (noise - low);
                var highPassed = noise - low;
                buffer[i] = 0.7f * highPassed * Mathf.Exp(-t / 0.004f) + 0.6f * Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Exp(-t / 0.012f);
            }
            return buffer;
        }

        /// <summary>Explosión: golpe grave que baja de tono y ruido que se oscurece al apagarse.</summary>
        private static float[] Explosion()
        {
            var buffer = Buffer(1.4f);
            var rng = new System.Random(21);
            var low = 0f;
            var phase = 0f;
            for (var i = 0; i < buffer.Length; i++)
            {
                var t = Time(i);
                var cutoff = Mathf.Lerp(3500f, 120f, Mathf.Clamp01(t / 1.1f));
                low += Alpha(cutoff) * ((float)(rng.NextDouble() * 2 - 1) - low);
                var noise = low * Attack(t, 0.004f) * Mathf.Exp(-t / 0.38f) * 2.2f;
                phase += 2f * Mathf.PI * Mathf.Lerp(95f, 32f, Mathf.Clamp01(t / 0.5f)) / SampleRate;
                var thump = Mathf.Sin(phase) * Attack(t, 0.002f) * Mathf.Exp(-t / 0.22f);
                buffer[i] = (float)Math.Tanh(1.6f * (noise + 0.9f * thump));
            }
            return buffer;
        }

        /// <summary>Pitido de los últimos segundos del Tabú.</summary>
        private static float[] Beep()
        {
            var buffer = Buffer(0.15f);
            for (var i = 0; i < buffer.Length; i++)
            {
                var t = Time(i);
                var tone = Mathf.Sin(2f * Mathf.PI * 880f * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * 1760f * t);
                buffer[i] = tone * Attack(t, 0.005f) * Release(t, 0.15f, 0.05f);
            }
            return buffer;
        }

        /// <summary>Bocina de fin de turno: dos ondas casi iguales que baten (suena áspera a propósito).</summary>
        private static float[] Buzzer()
        {
            var buffer = Buffer(0.8f);
            for (var i = 0; i < buffer.Length; i++)
            {
                var t = Time(i);
                var wave = Saw(180f, t, 6) + Saw(191f, t, 6);
                buffer[i] = 0.5f * wave * Attack(t, 0.01f) * Release(t, 0.8f, 0.15f);
            }
            return buffer;
        }

        /// <summary>Acierto: dos notas que suben (sol y re).</summary>
        private static float[] Correct()
        {
            var buffer = Buffer(0.5f);
            AddBell(buffer, 783.99f, 0f, 0.12f);
            AddBell(buffer, 1174.66f, 0.11f, 0.2f);
            return buffer;
        }

        /// <summary>Fallo: dos notas graves que bajan, con armónicos impares (más "cuadrada").</summary>
        private static float[] Wrong()
        {
            var buffer = Buffer(0.55f);
            for (var i = 0; i < buffer.Length; i++)
            {
                var t = Time(i);
                var freq = t < 0.18f ? 311.13f : 233.08f;
                var local = t < 0.18f ? t : t - 0.18f;
                var square = Mathf.Sin(2f * Mathf.PI * freq * t) + Mathf.Sin(6f * Mathf.PI * freq * t) / 3f + Mathf.Sin(10f * Mathf.PI * freq * t) / 5f;
                buffer[i] = 0.5f * square * Attack(local, 0.006f) * Mathf.Exp(-local / 0.16f);
            }
            return buffer;
        }

        /// <summary>Revelado de Wavelength: arpegio de do mayor con timbre de campana.</summary>
        private static float[] Chime()
        {
            var buffer = Buffer(1.1f);
            var notes = new[] { 523.25f, 659.25f, 783.99f, 1046.5f };
            for (var n = 0; n < notes.Length; n++) AddBell(buffer, notes[n], 0.08f * n, 0.35f);
            return buffer;
        }

        /// <summary>Pasar de frase en Yo Nunca: barrido de ruido suave, como una página.</summary>
        private static float[] Swipe()
        {
            const float seconds = 0.3f;
            var buffer = Buffer(seconds);
            var rng = new System.Random(31);
            var low = 0f;
            var lower = 0f;
            for (var i = 0; i < buffer.Length; i++)
            {
                var t = Time(i);
                var noise = (float)(rng.NextDouble() * 2 - 1);
                low += Alpha(Mathf.Lerp(600f, 5000f, t / seconds)) * (noise - low);
                lower += Alpha(300f) * (low - lower);
                buffer[i] = 1.6f * (low - lower) * Mathf.Sin(Mathf.PI * t / seconds);
            }
            return buffer;
        }

        // ------------------------------------------------------------------ Ayudas

        private static float[] Buffer(float seconds) => new float[Mathf.CeilToInt(seconds * SampleRate)];

        private static float Time(int sample) => (float)sample / SampleRate;

        private static float Alpha(float cutoff) => 1f - Mathf.Exp(-2f * Mathf.PI * cutoff / SampleRate);

        private static float Attack(float t, float seconds) => t <= 0f ? 0f : Mathf.Clamp01(t / seconds);

        private static float Release(float t, float total, float seconds) => Mathf.Clamp01((total - t) / seconds);

        private static float Saw(float freq, float t, int harmonics)
        {
            var sum = 0f;
            for (var k = 1; k <= harmonics; k++) sum += Mathf.Sin(2f * Mathf.PI * freq * k * t) / k;
            return sum;
        }

        /// <summary>Suma una nota de campana: fundamental, octava suave y un parcial inarmónico que se apaga antes.</summary>
        private static void AddBell(float[] buffer, float freq, float start, float decay)
        {
            var first = Mathf.FloorToInt(start * SampleRate);
            for (var i = Math.Max(first, 0); i < buffer.Length; i++)
            {
                var t = Time(i) - start;
                var env = Attack(t, 0.004f);
                buffer[i] += env * (Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Exp(-t / decay)
                                    + 0.25f * Mathf.Sin(4f * Mathf.PI * freq * t) * Mathf.Exp(-t / (decay * 0.6f))
                                    + 0.12f * Mathf.Sin(2f * Mathf.PI * freq * 2.76f * t) * Mathf.Exp(-t / (decay * 0.3f)));
            }
        }

        /// <summary>
        /// Suaviza 2 ms al principio y al final para que no chasquee y lo deja al volumen común (antes se normalizaba
        /// a 0,85 de pico, casi el máximo, y los pitidos y las ondas cuadradas sonaban demasiado fuerte).
        /// </summary>
        private static float[] Finish(float[] buffer)
        {
            var fade = Mathf.Min(buffer.Length / 2, Mathf.RoundToInt(0.002f * SampleRate));
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] *= Mathf.Min(1f, Mathf.Min(i, buffer.Length - 1 - i) / (float)Mathf.Max(1, fade));
            }
            return SoundLevel.Normalize(buffer);
        }
    }
}
