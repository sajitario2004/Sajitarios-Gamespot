using Gamespot.Core.Runtime.Settings;
using UnityEngine;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Aparición con fundido y escala (como el <c>AnimatedSwitcher</c> del Impostor: 350 ms, de 0,92 a 1 con
    /// easeOutBack). Con "reducir animaciones" aparece sin animar. Se quita sola al terminar.
    /// </summary>
    public sealed class FadeScaleIn : MonoBehaviour
    {
        private CanvasGroup _group;
        private float _duration;
        private float _from;
        private float _t;

        public static void Play(GameObject target, float duration = 0.35f, float fromScale = 0.92f)
        {
            if (AppSettings.ReduceMotion || duration <= 0f) return;
            var anim = target.GetOrAdd<FadeScaleIn>();
            anim._group = target.GetOrAdd<CanvasGroup>();
            anim._duration = duration;
            anim._from = fromScale;
            anim._t = 0f;
            anim.Apply(0f);
            anim.enabled = true;
        }

        private void Update()
        {
            _t += Time.unscaledDeltaTime / _duration;
            if (_t >= 1f)
            {
                Apply(1f);
                Destroy(this);
                return;
            }
            Apply(_t);
        }

        private void Apply(float t)
        {
            _group.alpha = Mathf.Clamp01(t * 1.4f);
            var s = Mathf.LerpUnclamped(_from, 1f, EaseOutBack(t));
            transform.localScale = new Vector3(s, s, 1f);
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
