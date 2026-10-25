using System;
using System.Collections.Generic;
using Gamespot.Core.Runtime.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>Un botón de un diálogo.</summary>
    public sealed class DialogAction
    {
        public DialogAction(string label, Action onClick = null, ButtonStyle style = ButtonStyle.Text, Color? accent = null, float glow = 0f)
        {
            Label = label;
            OnClick = onClick;
            Style = style;
            Accent = accent;
            Glow = glow;
        }

        /// <summary>
        /// Acción que decide si el diálogo se cierra (por ejemplo, un formulario con errores): devuelve falso
        /// para dejarlo abierto.
        /// </summary>
        public DialogAction(string label, Func<bool> tryRun, ButtonStyle style = ButtonStyle.Text, Color? accent = null, float glow = 0f)
            : this(label, (Action)null, style, accent, glow)
        {
            TryRun = tryRun;
        }

        public string Label { get; }
        public Action OnClick { get; }
        public Func<bool> TryRun { get; }
        public ButtonStyle Style { get; }
        public Color? Accent { get; }
        public float Glow { get; }
    }

    /// <summary>
    /// Diálogos modales sobre la capa de superposición (como <c>AlertDialog</c>): velo, panel con borde magenta
    /// al 45 % y radio 20, título, mensaje (o contenido propio) y botones abajo a la derecha. El botón atrás
    /// cierra el de arriba (como "cancelar").
    /// </summary>
    public static class Dialogs
    {
        private sealed class Entry
        {
            public GameObject Root;
            public Action OnDismiss;
        }

        private static RectTransform _layer;
        private static readonly List<Entry> Open = new List<Entry>();

        public static bool IsOpen => Open.Count > 0;

        /// <summary>Se lanza al abrir o cerrar un diálogo (para pausar temporizadores).</summary>
        public static event Action OpenChanged;

        public static void Initialize(RectTransform overlayLayer)
        {
            _layer = overlayLayer;
            Open.Clear();
        }

        /// <summary>
        /// Abre un diálogo. Cada botón lo cierra y después ejecuta su acción. <paramref name="onDismiss"/> se
        /// llama si se cierra con atrás o tocando fuera. <paramref name="buildContent"/> añade contenido propio.
        /// </summary>
        public static GameObject Show(string title, string message, IReadOnlyList<DialogAction> actions, Action onDismiss = null,
            Action<RectTransform> buildContent = null, Color? titleColor = null, bool dismissOnScrim = true)
        {
            if (_layer == null) throw new InvalidOperationException("Dialogs.Initialize no se ha llamado");
            var entry = new Entry { OnDismiss = onDismiss };

            var root = UiLayout.CreateStretched("Dialog", _layer);
            entry.Root = root.gameObject;
            var scrim = root.gameObject.AddComponent<Image>();
            scrim.color = Palette.Scrim;
            var scrimButton = root.gameObject.AddComponent<Button>();
            scrimButton.transition = Selectable.Transition.None;
            if (dismissOnScrim) scrimButton.onClick.AddListener(() => Dismiss(entry));

            // El velo cubre toda la pantalla; el diálogo, solo la zona segura (V061: notch y barra de gestos).
            var safe = UiLayout.CreateStretched("Safe", root);
            safe.gameObject.AddComponent<SafeArea>();
            var holder = new GameObject("Holder", typeof(RectTransform));
            holder.transform.SetParent(safe, false);
            var holderRect = (RectTransform)holder.transform;
            holderRect.anchorMin = new Vector2(0f, 0.5f);
            holderRect.anchorMax = new Vector2(1f, 0.5f);
            holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.offsetMin = new Vector2(24f, 0f);
            holderRect.offsetMax = new Vector2(-24f, 0f);
            var holderLayout = holder.AddComponent<VerticalLayoutGroup>();
            holderLayout.childControlWidth = true;
            holderLayout.childControlHeight = true;
            holderLayout.childForceExpandWidth = true;
            holderLayout.childForceExpandHeight = false;
            var fitter = holder.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var panel = NeonPanel.Create(holder.transform, Palette.NeonMagenta.WithAlpha(0.45f), Palette.Surface, 20f, 1.5f, 0.5f,
                Palette.NeonMagenta, new RectOffset(24, 24, 24, 16), 16f);
            // El panel absorbe los toques para que no cierren el diálogo.
            var blocker = panel.gameObject.AddComponent<Image>();
            blocker.color = Color.clear;

            if (!string.IsNullOrEmpty(title))
            {
                Ui.Text(panel.Content, title, TextRole.HeadlineSmall, titleColor ?? Palette.TextPrimary, glow: titleColor.HasValue ? 0.8f : 0f);
            }
            if (!string.IsNullOrEmpty(message))
            {
                var text = Ui.Text(panel.Content, message, TextRole.BodyMedium, Palette.TextMuted);
                text.lineSpacing = 8f;
            }
            buildContent?.Invoke(panel.Content);

            if (actions != null && actions.Count > 0)
            {
                var buttons = Ui.Row(panel.Content, 8f, alignment: TextAnchor.MiddleRight, name: "Actions");
                Ui.Flex(buttons);
                foreach (var action in actions)
                {
                    var a = action;
                    NeonButton.Create(buttons, a.Label, () =>
                    {
                        if (a.TryRun != null)
                        {
                            if (a.TryRun()) Close(entry);
                            return;
                        }
                        Close(entry);
                        a.OnClick?.Invoke();
                    }, a.Style, accent: a.Accent, glow: a.Glow, fontSize: 16f);
                }
            }

            Open.Add(entry);
            OpenChanged?.Invoke();
            return entry.Root;
        }

        /// <summary>Diálogo de confirmación: <paramref name="onResult"/> recibe verdadero solo al confirmar.</summary>
        public static void Confirm(string title, string message, string cancelLabel, string confirmLabel, Action<bool> onResult,
            Color? confirmAccent = null, Color? titleColor = null)
        {
            var accent = confirmAccent ?? Palette.NeonCyan;
            Show(title, message, new[]
            {
                new DialogAction(cancelLabel, () => onResult?.Invoke(false), ButtonStyle.Text, Palette.NeonCyan),
                new DialogAction(confirmLabel, () => onResult?.Invoke(true), ButtonStyle.Filled, accent, 0.6f)
            }, () => onResult?.Invoke(false), titleColor: titleColor);
        }

        /// <summary>Botón atrás: cierra el diálogo de arriba. Devuelve falso si no había ninguno.</summary>
        public static bool HandleBack()
        {
            if (Open.Count == 0) return false;
            Dismiss(Open[Open.Count - 1]);
            return true;
        }

        /// <summary>Cierra el diálogo que devolvió <see cref="Show"/> sin llamar a su <c>onDismiss</c> (p. ej. al elegir en una lista).</summary>
        public static void Close(GameObject dialog)
        {
            var entry = Open.Find(e => e.Root == dialog);
            if (entry != null) Close(entry);
        }

        public static void CloseAll()
        {
            if (Open.Count == 0) return;
            foreach (var entry in Open.ToArray()) Destroy(entry);
            Open.Clear();
            OpenChanged?.Invoke();
        }

        private static void Dismiss(Entry entry)
        {
            if (!Close(entry)) return;
            entry.OnDismiss?.Invoke();
        }

        private static bool Close(Entry entry)
        {
            if (!Open.Remove(entry)) return false;
            Destroy(entry);
            OpenChanged?.Invoke();
            return true;
        }

        private static void Destroy(Entry entry)
        {
            if (entry.Root == null) return;
            entry.Root.SetActive(false);
            UnityEngine.Object.Destroy(entry.Root);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _layer = null;
            Open.Clear();
            OpenChanged = null;
        }
    }
}
