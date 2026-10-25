using System;
using System.Collections.Generic;
using Gamespot.Core.Runtime.Settings;
using UnityEngine;

namespace Gamespot.Core.Runtime.Audio
{
    /// <summary>Efectos de sonido de la app (los mismos WAV que el Flutter).</summary>
    public enum AppSound
    {
        /// <summary>Es un 10 pero: al revelarse la carta.</summary>
        CardFlip,
        /// <summary>Impostor: al pulsar "Revelar".</summary>
        Reveal,
        /// <summary>Impostor: al abrir el fin de partida.</summary>
        GameOver
    }

    /// <summary>
    /// Sonidos de la app (port de <c>AudioService</c>): se precargan en el arranque, se disparan sin esperar y
    /// no suenan si el usuario los silenció (<c>PlayerPrefs["audio_enabled"]</c>, activado por defecto).
    /// Cualquier error se ignora: el sonido nunca rompe una partida.
    /// </summary>
    public sealed class AudioService : MonoBehaviour
    {
        public const string ResourcesFolder = "Audio";

        private static readonly Dictionary<AppSound, string> Files = new Dictionary<AppSound, string>
        {
            [AppSound.CardFlip] = "card_flip",
            [AppSound.Reveal] = "reveal",
            [AppSound.GameOver] = "game_over"
        };

        private readonly Dictionary<AppSound, AudioClip> _clips = new Dictionary<AppSound, AudioClip>();
        private AudioSource _source;

        /// <summary>Se lanza al activar o silenciar el sonido.</summary>
        public event Action<bool> EnabledChanged;

        /// <summary>Cuántas veces ha sonado cada efecto (para los tests).</summary>
        public int PlayCount { get; private set; }

        public AppSound? LastPlayed { get; private set; }

        public bool Enabled
        {
            get => AppSettings.AudioEnabled;
            set
            {
                AppSettings.AudioEnabled = value;
                EnabledChanged?.Invoke(value);
            }
        }

        public static AudioService Create(GameObject host)
        {
            var service = host.AddComponent<AudioService>();
            service._source = host.AddComponent<AudioSource>();
            service._source.playOnAwake = false;
            service._source.spatialBlend = 0f;
            return service;
        }

        /// <summary>Carga los tres clips (se llama en el arranque).</summary>
        public void Preload()
        {
            foreach (var pair in Files)
            {
                if (_clips.ContainsKey(pair.Key)) continue;
                try
                {
                    var clip = Resources.Load<AudioClip>($"{ResourcesFolder}/{pair.Value}");
                    if (clip != null)
                    {
                        clip.LoadAudioData();
                        _clips[pair.Key] = clip;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Audio] No se pudo cargar {pair.Value}: {e.Message}");
                }
            }
        }

        /// <summary>Cambia entre activado y silenciado; devuelve el estado nuevo.</summary>
        public bool Toggle()
        {
            Enabled = !Enabled;
            return Enabled;
        }

        public void Play(AppSound sound, float volume = 1f)
        {
            if (!Enabled) return;
            try
            {
                if (!_clips.TryGetValue(sound, out var clip))
                {
                    Preload();
                    if (!_clips.TryGetValue(sound, out clip)) return;
                }
                PlayCount++;
                LastPlayed = sound;
                _source.PlayOneShot(clip, Mathf.Clamp01(volume));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Audio] No se pudo reproducir {sound}: {e.Message}");
            }
        }
    }
}
