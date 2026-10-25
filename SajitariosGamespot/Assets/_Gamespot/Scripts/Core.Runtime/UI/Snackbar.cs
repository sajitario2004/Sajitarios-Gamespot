using System.Collections;
using Gamespot.Core.Runtime.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Aviso flotante abajo (como <c>SnackBar</c> flotante, radio 12). Solo hay uno a la vez: uno nuevo
    /// sustituye al anterior. Desaparece a los 3 s.
    /// </summary>
    public sealed class Snackbar : MonoBehaviour
    {
        public const float DurationSeconds = 3f;

        private static RectTransform _layer;
        private static Snackbar _current;

        /// <summary>Texto del aviso visible (para los tests).</summary>
        public static string CurrentText => _current != null ? _current._text.text : null;

        private TextMeshProUGUI _text;

        public static void Initialize(RectTransform overlayLayer)
        {
            _layer = overlayLayer;
            _current = null;
        }

        public static void Show(string message, Color? accent = null)
        {
            if (_layer == null) return;
            Hide();

            var go = new GameObject("Snackbar", typeof(RectTransform));
            go.transform.SetParent(_layer, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(16f, 24f);
            rect.offsetMax = new Vector2(-16f, 24f);
            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;

            var panel = NeonPanel.Create(go.transform, (accent ?? Palette.NeonViolet).WithAlpha(0.8f), Palette.SurfaceHighest, 12f, 1.2f,
                0.4f, padding: new RectOffset(16, 16, 14, 14));
            var snackbar = go.AddComponent<Snackbar>();
            snackbar._text = Ui.Text(panel.Content, message, TextRole.BodyMedium, Palette.TextPrimary);
            _current = snackbar;
            snackbar.StartCoroutine(snackbar.AutoHide());
        }

        public static void Hide()
        {
            if (_current == null) return;
            _current.gameObject.SetActive(false);
            Destroy(_current.gameObject);
            _current = null;
        }

        private IEnumerator AutoHide()
        {
            yield return new WaitForSecondsRealtime(DurationSeconds);
            if (_current == this) Hide();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _layer = null;
            _current = null;
        }
    }
}
