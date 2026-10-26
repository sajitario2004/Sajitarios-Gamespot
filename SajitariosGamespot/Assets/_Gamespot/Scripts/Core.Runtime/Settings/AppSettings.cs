using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using UnityEngine;

namespace Gamespot.Core.Runtime.Settings
{
    /// <summary>Preferencias de la app guardadas en <c>PlayerPrefs</c> (mismas claves que el Flutter).</summary>
    public static class AppSettings
    {
        public const string AudioEnabledKey = "audio_enabled";
        public const string ReduceMotionKey = "reduce_motion";
        public const string LastPlayersKey = "last_players";
        public const string VibrationEnabledKey = "vibration_enabled";

        /// <summary>Se lanza al cambiar "reducir animaciones".</summary>
        public static event Action ReduceMotionChanged;

        /// <summary>Sonido activado (por defecto sí).</summary>
        public static bool AudioEnabled
        {
            get => PlayerPrefs.GetInt(AudioEnabledKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(AudioEnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary>Vibración activada (por defecto sí). Aún sin interruptor: llegará con la pantalla de ajustes (C5).</summary>
        public static bool VibrationEnabled
        {
            get => PlayerPrefs.GetInt(VibrationEnabledKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(VibrationEnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// "Reducir animaciones": los brillos que laten se quedan fijos (el Flutter seguía la opción del
        /// sistema; en Unity es una opción de la app). Por defecto no.
        /// </summary>
        public static bool ReduceMotion
        {
            // Sin elegir en la app, sigue al sistema (V062). Se guarda en memoria: los brillos lo miran en cada
            // fotograma y PlayerPrefs es una llamada nativa (V063).
            get
            {
                if (!_reduceMotion.HasValue)
                {
                    _reduceMotion = PlayerPrefs.HasKey(ReduceMotionKey) ? PlayerPrefs.GetInt(ReduceMotionKey, 0) == 1 : SystemReduceMotion;
                }
                return _reduceMotion.Value;
            }
            set
            {
                if (value == ReduceMotion) return;
                PlayerPrefs.SetInt(ReduceMotionKey, value ? 1 : 0);
                PlayerPrefs.Save();
                _reduceMotion = value;
                ReduceMotionChanged?.Invoke();
            }
        }

        private static bool? _reduceMotion;

        /// <summary>Olvida la elección de "reducir animaciones": vuelve a seguir al sistema.</summary>
        public static void ClearReduceMotion()
        {
            var before = ReduceMotion;
            PlayerPrefs.DeleteKey(ReduceMotionKey);
            PlayerPrefs.Save();
            _reduceMotion = null;
            if (ReduceMotion != before) ReduceMotionChanged?.Invoke();
        }

        /// <summary>
        /// Nombres de la última partida empezada, en orden (los comparten todos los setups con lista de jugadores).
        /// Vacío si no hay o si lo guardado está mal; asignar una lista vacía lo borra.
        /// </summary>
        public static IReadOnlyList<string> LastPlayers
        {
            get
            {
                var raw = PlayerPrefs.GetString(LastPlayersKey, string.Empty);
                if (string.IsNullOrEmpty(raw)) return Array.Empty<string>();
                try
                {
                    return Json.ParseArray(raw).OfType<string>().Select(n => n.Trim()).Where(n => n.Length > 0).ToList();
                }
                catch (Exception)
                {
                    return Array.Empty<string>();
                }
            }
            set
            {
                var names = (value ?? Array.Empty<string>()).Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).ToList();
                if (names.Count == 0) PlayerPrefs.DeleteKey(LastPlayersKey);
                else PlayerPrefs.SetString(LastPlayersKey, JsonWriter.WriteStringArray(names));
                PlayerPrefs.Save();
            }
        }

        private static bool? _systemReduceMotion;

        /// <summary>
        /// Si el sistema pide menos movimiento: en Android, «Quitar animaciones» (escala de animación a 0). En iOS haría
        /// falta un plugin nativo: por ahora, no.
        /// </summary>
        public static bool SystemReduceMotion
        {
            get
            {
                if (_systemReduceMotion.HasValue) return _systemReduceMotion.Value;
                _systemReduceMotion = false;
#if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    using (var resolver = UnityEngine.Android.AndroidApplication.currentContext.Call<AndroidJavaObject>("getContentResolver"))
                    using (var global = new AndroidJavaClass("android.provider.Settings$Global"))
                    {
                        _systemReduceMotion = global.CallStatic<float>("getFloat", resolver, "animator_duration_scale", 1f) == 0f;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Ajustes] No se pudo leer la escala de animación del sistema: {e.Message}");
                }
#endif
                return _systemReduceMotion.Value;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ReduceMotionChanged = null;
            _reduceMotion = null;
        }
    }
}
