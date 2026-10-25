using System;
using Gamespot.Core.Runtime.Settings;
using UnityEngine;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Brillo que late (port de <c>PulseGlow</c>): la intensidad va de <see cref="Min"/> a <see cref="Max"/>
    /// en 1,6 s, ida y vuelta, con ease-in-out. Con "reducir animaciones" se queda fija en (min + max) / 2.
    /// </summary>
    public sealed class PulseGlow : MonoBehaviour
    {
        public const float HalfPeriodSeconds = 1.6f;

        private Action<float> _apply;
        private float _time;

        public float Min { get; private set; } = 0.7f;
        public float Max { get; private set; } = 1.2f;

        /// <summary>Intensidad actual.</summary>
        public float Intensity { get; private set; }

        /// <summary>Anima el halo de un panel.</summary>
        public static PulseGlow On(NeonPanel panel, float min = 0.7f, float max = 1.2f) =>
            Attach(panel.gameObject, min, max, v => panel.GlowIntensity = v);

        /// <summary>Anima cualquier valor (por ejemplo, el halo de un texto).</summary>
        public static PulseGlow Attach(GameObject target, float min, float max, Action<float> apply)
        {
            var pulse = target.GetOrAdd<PulseGlow>();
            pulse.Min = min;
            pulse.Max = max;
            pulse._apply = apply;
            pulse.Refresh();
            return pulse;
        }

        private void OnEnable()
        {
            AppSettings.ReduceMotionChanged += Refresh;
            Refresh();
        }

        private void OnDisable() => AppSettings.ReduceMotionChanged -= Refresh;

        private void Update()
        {
            if (AppSettings.ReduceMotion || _apply == null) return;
            _time += Time.unscaledDeltaTime;
            var t = Mathf.PingPong(_time / HalfPeriodSeconds, 1f);
            Set(Mathf.Lerp(Min, Max, EaseInOut(t)));
        }

        private void Refresh()
        {
            if (_apply == null) return;
            if (AppSettings.ReduceMotion) Set((Min + Max) * 0.5f);
            else Set(Min);
        }

        private void Set(float value)
        {
            Intensity = value;
            _apply?.Invoke(value);
        }

        /// <summary>Curva ease-in-out cúbica (como <c>Curves.easeInOut</c>).</summary>
        public static float EaseInOut(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
}
