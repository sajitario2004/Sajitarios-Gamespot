using Gamespot.Core.Runtime.Settings;
using UnityEngine;

namespace Gamespot.Core.Runtime.Audio
{
    /// <summary>
    /// Vibración del móvil (V059d). Solo vibra en Android e iOS y si <see cref="AppSettings.VibrationEnabled"/>;
    /// en el Editor solo cuenta, para los tests.
    /// </summary>
    public static class Haptics
    {
        /// <summary>Cuántas vibraciones se han pedido con la vibración activada.</summary>
        public static int VibrateCount { get; private set; }

        public static void Vibrate()
        {
            if (!AppSettings.VibrationEnabled) return;
            VibrateCount++;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => VibrateCount = 0;
    }
}
