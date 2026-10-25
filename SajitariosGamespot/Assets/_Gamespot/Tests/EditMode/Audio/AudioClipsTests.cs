using System;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Editor.Audio;
using NUnit.Framework;
using UnityEngine;

namespace Gamespot.Tests.EditMode.Audio
{
    public sealed class AudioClipsTests
    {
        [Test]
        public void CadaSonidoTieneSuClip()
        {
            foreach (AppSound sound in Enum.GetValues(typeof(AppSound)))
            {
                var clip = Resources.Load<AudioClip>($"{AudioService.ResourcesFolder}/{AudioService.FileOf(sound)}");
                Assert.IsNotNull(clip, $"Falta Resources/Audio/{AudioService.FileOf(sound)}.wav");
                Assert.That(clip.length, Is.InRange(0.03f, 2f), sound.ToString());
                Assert.AreEqual(1, clip.channels, "Mono");
            }
        }

        /// <summary>V059n: todos al mismo volumen, bajo, y sin picos cerca del máximo (sonaban demasiado fuerte).</summary>
        [Test]
        public void TodosAlMismoVolumenYSinPicos()
        {
            foreach (AppSound sound in Enum.GetValues(typeof(AppSound)))
            {
                var samples = SoundLevel.ReadWav($"Assets/_Gamespot/Resources/{AudioService.ResourcesFolder}/{AudioService.FileOf(sound)}.wav");
                Assert.That(SoundLevel.PeakDb(samples), Is.LessThanOrEqualTo(SoundLevel.PeakCeilingDb + 0.1f), $"Pico de {sound}");
                // Solo puede quedar por debajo del objetivo si lo limita el pico (los chasquidos del reloj).
                Assert.That(SoundLevel.LoudnessDb(samples), Is.InRange(SoundLevel.TargetDb - 4f, SoundLevel.TargetDb + 0.5f), $"Volumen de {sound}");
            }
        }
    }
}
