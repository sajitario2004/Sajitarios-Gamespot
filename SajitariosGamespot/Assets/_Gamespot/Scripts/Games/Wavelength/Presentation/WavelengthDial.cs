using System;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Games.Wavelength.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gamespot.Games.Wavelength
{
    /// <summary>Qué enseña el dial.</summary>
    public enum DialMode
    {
        /// <summary>El psíquico ve las bandas y el objetivo; no hay aguja.</summary>
        Clue,
        /// <summary>El grupo mueve la aguja; bandas y objetivo ocultos.</summary>
        Guess,
        /// <summary>Aparecen bandas y objetivo (0,7 s) con la aguja donde la dejó el grupo.</summary>
        Reveal
    }

    /// <summary>
    /// Dial semicircular de Wavelength dibujado como malla de uGUI (sustituye al dial de Flame, que estaba roto).
    /// Geometría (como el Flutter): radio = min(0,44·ancho, 0,82·alto), centro en la base a 0,22·alto; la posición
    /// p ∈ [0, 1] va de izquierda (0) a derecha (1) pasando por arriba (0,5). Bandas cian superpuestas: lejos ±0,20,
    /// cerca ±0,12 y en el blanco ±0,06. El propio dial recibe el toque y el arrastre (no compite con ningún scroll)
    /// y un toque bajo la base va al extremo más cercano.
    /// </summary>
    public sealed class WavelengthDial : MaskableGraphic, IPointerDownHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler
    {
        public const float RevealSeconds = 0.7f;
        private const int ArcSegments = 72;

        private DialMode _mode = DialMode.Guess;
        private double _target = 0.5;
        private float _needle = 0.5f;
        private float _revealProgress = 1f;

        /// <summary>Se lanza al mover la aguja (solo en modo Guess).</summary>
        public event Action<float> NeedleChanged;

        public DialMode Mode => _mode;
        public float Needle => _needle;
        public float RevealProgress => _revealProgress;

        public static WavelengthDial Create(Transform parent)
        {
            // El CanvasRenderer va explícito: al añadir un Graphic propio por código no siempre se crea solo, y sin
            // él la malla no se pinta (el dial salía vacío aunque la lógica funcionaba).
            var go = new GameObject("WavelengthDial", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var dial = go.AddComponent<WavelengthDial>();
            dial.raycastTarget = true;
            dial.color = Color.white;
            return dial;
        }

        /// <summary>Configura el dial de una vez (no hay estado "pendiente" que se pierda, como pasaba con Flame).</summary>
        public void Setup(DialMode mode, double target, float needle)
        {
            _mode = mode;
            _target = Math.Max(0.0, Math.Min(1.0, target));
            _needle = Mathf.Clamp01(needle);
            _revealProgress = mode == DialMode.Reveal ? 0f : 1f;
            enabled = true;
            SetVerticesDirty();
        }

        /// <summary>Pone la aguja (lo usan el toque, el arrastre y los tests).</summary>
        public void SetNeedle(float value)
        {
            if (_mode != DialMode.Guess) return;
            _needle = Mathf.Clamp01(value);
            SetVerticesDirty();
            NeedleChanged?.Invoke(_needle);
        }

        /// <summary>Termina ya la animación de revelado.</summary>
        public void CompleteReveal()
        {
            _revealProgress = 1f;
            SetVerticesDirty();
        }

        private void Update()
        {
            if (_mode != DialMode.Reveal || _revealProgress >= 1f) return;
            _revealProgress = Mathf.Min(1f, _revealProgress + Time.unscaledDeltaTime / RevealSeconds);
            SetVerticesDirty();
        }

        // ------------------------------------------------------------------ Entrada

        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        public void OnPointerDown(PointerEventData eventData) => HandlePointer(eventData);

        public void OnBeginDrag(PointerEventData eventData) => HandlePointer(eventData);

        public void OnDrag(PointerEventData eventData) => HandlePointer(eventData);

        private void HandlePointer(PointerEventData eventData)
        {
            if (_mode != DialMode.Guess) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var local)) return;
            SetNeedle(PositionFromLocalPoint(local));
        }

        /// <summary>Posición 0..1 del punto local. Bajo la base: el extremo del lado del toque.</summary>
        public float PositionFromLocalPoint(Vector2 local)
        {
            var g = Geometry();
            var dx = local.x - g.center.x;
            var dy = local.y - g.center.y;
            if (dy < 0f) return dx < 0f ? 0f : 1f;
            var angle = Mathf.Atan2(dy, dx); // 0 a la derecha, π a la izquierda
            return Mathf.Clamp01(1f - angle / Mathf.PI);
        }

        /// <summary>Punto local de la posición <paramref name="p"/> a la distancia <paramref name="radius"/>.</summary>
        public Vector2 LocalPointOf(float p, float radius)
        {
            var g = Geometry();
            var angle = Mathf.PI * (1f - p);
            return g.center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private (Vector2 center, float radius) Geometry()
        {
            var rect = rectTransform.rect;
            var radius = Mathf.Max(1f, Mathf.Min(0.44f * rect.width, 0.82f * rect.height));
            var center = new Vector2(rect.center.x, rect.yMin + 0.22f * rect.height);
            return (center, radius);
        }

        // ------------------------------------------------------------------ Dibujo

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var (center, r) = Geometry();
            if (r <= 1f) return;
            var cyan = Palette.NeonCyan;

            // Fondo del semicírculo para que las bandas se lean.
            Wedge(vh, center, r, 0f, 1f, Palette.SurfaceHigh.WithAlpha(0.55f));

            // Bandas y objetivo: el psíquico los ve; en el revelado aparecen poco a poco.
            var show = _mode == DialMode.Clue ? 1f : _mode == DialMode.Reveal ? _revealProgress : 0f;
            if (show > 0f)
            {
                var t = (float)_target;
                Wedge(vh, center, r, t - (float)WavelengthScoring.FarHalfWidth, t + (float)WavelengthScoring.FarHalfWidth, cyan.WithAlpha(0.20f * show));
                Wedge(vh, center, r, t - (float)WavelengthScoring.NearHalfWidth, t + (float)WavelengthScoring.NearHalfWidth, cyan.WithAlpha(0.30f * show));
                Wedge(vh, center, r, t - (float)WavelengthScoring.BullseyeHalfWidth, t + (float)WavelengthScoring.BullseyeHalfWidth, cyan.WithAlpha(0.45f * show));
                Line(vh, LocalPointOf(t, 0.15f * r), LocalPointOf(t, 0.98f * r), 0.05f * r, cyan.WithAlpha(0.35f * show));
                Line(vh, LocalPointOf(t, 0.15f * r), LocalPointOf(t, 0.98f * r), 0.018f * r, Palette.TextPrimary.WithAlpha(show));
            }

            // Arco con halo y marcas en 0, ¼, ½, ¾ y 1.
            Arc(vh, center, r, 0.055f * r, cyan.WithAlpha(0.30f));
            Arc(vh, center, r, 0.025f * r, cyan);
            for (var i = 0; i <= 4; i++)
            {
                var p = i / 4f;
                Line(vh, LocalPointOf(p, 0.94f * r), LocalPointOf(p, 1.06f * r), 0.014f * r, cyan);
            }

            // Aguja magenta (no en el modo del psíquico).
            if (_mode != DialMode.Clue)
            {
                var magenta = Palette.NeonMagenta;
                var tip = LocalPointOf(_needle, 0.92f * r);
                Line(vh, center, tip, 0.045f * r, magenta.WithAlpha(0.35f));
                Line(vh, center, tip, 0.016f * r, magenta);
                Disc(vh, tip, 0.06f * r, magenta.WithAlpha(0.35f));
                Disc(vh, tip, 0.045f * r, magenta);
                Disc(vh, center, 0.05f * r, magenta);
            }
        }

        private void Wedge(VertexHelper vh, Vector2 center, float r, float from, float to, Color color)
        {
            from = Mathf.Clamp01(from);
            to = Mathf.Clamp01(to);
            if (to <= from || color.a <= 0f) return;
            var segments = Mathf.Max(2, Mathf.CeilToInt(ArcSegments * (to - from)));
            var start = vh.currentVertCount;
            vh.AddVert(center, color, Vector4.zero);
            for (var i = 0; i <= segments; i++)
            {
                vh.AddVert(LocalPointOf(Mathf.Lerp(from, to, i / (float)segments), r), color, Vector4.zero);
            }
            for (var i = 0; i < segments; i++) vh.AddTriangle(start, start + 1 + i, start + 2 + i);
        }

        private void Arc(VertexHelper vh, Vector2 center, float r, float width, Color color)
        {
            var start = vh.currentVertCount;
            for (var i = 0; i <= ArcSegments; i++)
            {
                var p = i / (float)ArcSegments;
                vh.AddVert(LocalPointOf(p, r - width * 0.5f), color, Vector4.zero);
                vh.AddVert(LocalPointOf(p, r + width * 0.5f), color, Vector4.zero);
            }
            for (var i = 0; i < ArcSegments; i++)
            {
                var a = start + i * 2;
                vh.AddTriangle(a, a + 1, a + 3);
                vh.AddTriangle(a, a + 3, a + 2);
            }
        }

        private static void Line(VertexHelper vh, Vector2 from, Vector2 to, float width, Color color)
        {
            var dir = (to - from).normalized;
            var normal = new Vector2(-dir.y, dir.x) * (width * 0.5f);
            var start = vh.currentVertCount;
            vh.AddVert(from - normal, color, Vector4.zero);
            vh.AddVert(from + normal, color, Vector4.zero);
            vh.AddVert(to + normal, color, Vector4.zero);
            vh.AddVert(to - normal, color, Vector4.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }

        private static void Disc(VertexHelper vh, Vector2 center, float radius, Color color)
        {
            const int segments = 24;
            var start = vh.currentVertCount;
            vh.AddVert(center, color, Vector4.zero);
            for (var i = 0; i <= segments; i++)
            {
                var a = i / (float)segments * Mathf.PI * 2f;
                vh.AddVert(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, color, Vector4.zero);
            }
            for (var i = 0; i < segments; i++) vh.AddTriangle(start, start + 1 + i, start + 2 + i);
        }
    }
}
