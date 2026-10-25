using System;
using Gamespot.Core.Runtime.Audio;
using NUnit.Framework;
using UnityEngine;

namespace Gamespot.Tests.EditMode.Core
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
    }
}
