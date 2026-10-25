using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.EsUn10Pero.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Games.EsUn10Pero
{
    /// <summary>
    /// Carta neón con volteo (port de <c>CardFlipGame</c> de Flame, ahora con uGUI). Proporción 5:7, radio 0,10 ×
    /// ancho. Volteo de 0,55 s con <i>easeInOutCubic</i>: escala X = |cos(e·π)| (mín. 0,06), escala Y =
    /// 1 + 0,06·sin(e·π), la cara nueva aparece cuando e ≥ 0,5 (antes se ve el dorso) y la sombra cian se aleja
    /// y difumina con sin(e·π). Palos rojos en magenta y negros en cian. Sin animación no hace nada por fotograma.
    /// </summary>
    public sealed class CardView : MonoBehaviour
    {
        public const float FlipSeconds = 0.55f;
        public const float Aspect = 5f / 7f;

        private RectTransform _rect;
        private RectTransform _pivot;
        private Image _shadow;
        private GameObject _back;
        private GameObject _face;
        private Image _backMask;
        private Image _backBorder;
        private Image _faceFill;
        private Image _faceGlow;
        private Image _faceBorder;
        private Corner _topLeft;
        private Corner _bottomRight;
        private Image _centerSuit;
        private Image _centerGlow;

        private Card _shown;
        private Card _target;
        private bool _flipping;
        private float _t;
        private float _lastWidth = -1f;

        /// <summary>Carta de la cara visible (nula si se ve el dorso).</summary>
        public Card Shown => _face.activeSelf ? _shown : null;

        public bool IsFlipping => _flipping;

        private sealed class Corner
        {
            public RectTransform Root;
            public TextMeshProUGUI Value;
            public Image Suit;
        }

        public static CardView Create(Transform parent)
        {
            var go = new GameObject("CardView", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiLayout.Stretch((RectTransform)go.transform);
            var view = go.AddComponent<CardView>();
            view.BuildHierarchy();
            return view;
        }

        /// <summary>Voltea hacia <paramref name="card"/> (la cara anterior pasa por el dorso, como en el Flutter).</summary>
        public void FlipTo(Card card)
        {
            _target = card;
            _t = 0f;
            _flipping = true;
            ShowBack();
            enabled = true;
        }

        /// <summary>Muestra una carta sin animación (al reconstruir la pantalla).</summary>
        public void ShowImmediate(Card card)
        {
            _flipping = false;
            _pivot.localScale = Vector3.one;
            if (card == null) ShowBack();
            else ShowFace(card);
            SetLift(0f);
            enabled = false;
        }

        private void Update()
        {
            if (!_flipping)
            {
                enabled = false;
                return;
            }
            _t = Mathf.Min(1f, _t + Time.unscaledDeltaTime / FlipSeconds);
            var e = EaseInOutCubic(_t);
            var sx = Mathf.Clamp(Mathf.Abs(Mathf.Cos(e * Mathf.PI)), 0.06f, 1f);
            var sy = 1f + 0.06f * Mathf.Sin(e * Mathf.PI);
            _pivot.localScale = new Vector3(sx, sy, 1f);
            if (e >= 0.5f && _target != null && !_face.activeSelf) ShowFace(_target);
            SetLift(Mathf.Sin(e * Mathf.PI));
            if (_t >= 1f)
            {
                _flipping = false;
                _pivot.localScale = Vector3.one;
                SetLift(0f);
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_rect != null) Relayout();
        }

        public static float EaseInOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 0.5f * Mathf.Pow(2f * t - 2f, 3f) + 1f;

        private void BuildHierarchy()
        {
            _rect = (RectTransform)transform;

            _shadow = Layer("Shadow", transform, NeonSprites.Glow(20f), Palette.NeonCyan.WithAlpha(0.55f * NeonPanel.GlowOpacity));
            _pivot = UiLayout.CreateStretched("Pivot", transform);

            // Dorso: imagen recortada por el rectángulo redondeado + borde cian.
            _back = UiLayout.CreateStretched("Back", _pivot).gameObject;
            _backMask = Layer("Mask", _back.transform, NeonSprites.RoundedRect(20f), Palette.SurfaceHigh);
            var mask = _backMask.gameObject.AddComponent<Mask>();
            var backSprite = Resources.Load<Sprite>("Images/card_back");
            if (backSprite != null)
            {
                mask.showMaskGraphic = false;
                var art = Layer("Art", _backMask.transform, backSprite, Color.white);
                art.type = Image.Type.Simple;
                art.preserveAspect = false;
            }
            else
            {
                // Sin imagen: dorso dibujado (relleno surfaceHigh y marco interior magenta).
                mask.showMaskGraphic = true;
                var inner = Layer("InnerFrame", _backMask.transform, NeonSprites.RoundedBorder(14f, 2f), Palette.NeonMagenta);
                inner.rectTransform.offsetMin = new Vector2(18f, 18f);
                inner.rectTransform.offsetMax = new Vector2(-18f, -18f);
            }
            _backBorder = Layer("Border", _back.transform, NeonSprites.RoundedBorder(20f, 2f), Palette.NeonCyan);

            // Cara.
            _face = UiLayout.CreateStretched("Face", _pivot).gameObject;
            _faceGlow = Layer("Glow", _face.transform, NeonSprites.Glow(20f), Palette.NeonCyan.WithAlpha(0.75f * NeonPanel.GlowOpacity));
            _faceFill = Layer("Fill", _face.transform, NeonSprites.RoundedRect(20f), Palette.Surface);
            _faceBorder = Layer("Border", _face.transform, NeonSprites.RoundedBorder(20f, 2f), Palette.NeonCyan);
            _centerGlow = Layer("CenterGlow", _face.transform, SuitSprites.Get(SuitShape.Spade), Color.white);
            _centerSuit = Layer("CenterSuit", _face.transform, SuitSprites.Get(SuitShape.Spade), Color.white);
            _topLeft = CreateCorner("TopLeft", false);
            _bottomRight = CreateCorner("BottomRight", true);

            ShowBack();
            SetLift(0f);
            enabled = false;
        }

        private Corner CreateCorner(string name, bool rotated)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(_face.transform, false);
            var rect = (RectTransform)root.transform;
            var anchor = rotated ? new Vector2(1f, 0f) : new Vector2(0f, 1f);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0f, 1f);
            if (rotated) rect.localEulerAngles = new Vector3(0f, 0f, 180f);

            var valueGo = new GameObject("Value", typeof(RectTransform));
            valueGo.transform.SetParent(root.transform, false);
            var value = valueGo.AddComponent<TextMeshProUGUI>();
            value.fontStyle = FontStyles.Bold;
            value.alignment = TextAlignmentOptions.Center;
            value.textWrappingMode = TextWrappingModes.NoWrap;
            value.raycastTarget = false;

            var suit = Layer("Suit", root.transform, SuitSprites.Get(SuitShape.Spade), Color.white);
            suit.preserveAspect = true;
            return new Corner { Root = rect, Value = value, Suit = suit };
        }

        private void ShowBack()
        {
            _back.SetActive(true);
            _face.SetActive(false);
        }

        private void ShowFace(Card card)
        {
            _shown = card;
            _back.SetActive(false);
            _face.SetActive(true);
            var color = card.Suit.IsRed() ? Palette.NeonMagenta : Palette.NeonCyan;
            var shape = ShapeOf(card.Suit);
            foreach (var corner in new[] { _topLeft, _bottomRight })
            {
                corner.Value.text = card.Value.Label();
                corner.Value.color = color;
                Ui.SetGlow(corner.Value, color, 0.9f);
                corner.Suit.sprite = SuitSprites.Get(shape);
                corner.Suit.color = color;
            }
            _centerSuit.sprite = SuitSprites.Get(shape);
            _centerSuit.color = color;
            _centerGlow.sprite = SuitSprites.Get(shape);
            _centerGlow.color = color.WithAlpha(0.35f);
            Relayout(true);
        }

        /// <summary>Sombra: baja 8 + 18·lift y se difumina 12 + 16·lift (lift = sin(e·π), 0 en reposo).</summary>
        private void SetLift(float lift)
        {
            var dy = 8f + 18f * lift;
            var spread = (12f + 16f * lift) * 0.5f;
            var margin = NeonSprites.GlowMargin * 0.6f;
            _shadow.rectTransform.offsetMin = new Vector2(-margin - spread, -margin - dy - spread);
            _shadow.rectTransform.offsetMax = new Vector2(margin + spread, margin - dy + spread);
            _shadow.color = Palette.NeonCyan.WithAlpha(0.55f * NeonPanel.GlowOpacity * Mathf.Lerp(1f, 0.8f, lift));
        }

        private void Relayout(bool force = false)
        {
            var w = _rect.rect.width;
            if (w <= 1f || (!force && Mathf.Approximately(w, _lastWidth))) return;
            _lastWidth = w;
            var radius = Mathf.Max(4f, Mathf.Round(0.10f * w));
            var border = Mathf.Max(1.5f, 0.014f * w);
            _backMask.sprite = NeonSprites.RoundedRect(radius);
            _backBorder.sprite = NeonSprites.RoundedBorder(radius, border);
            _faceFill.sprite = NeonSprites.RoundedRect(radius);
            _faceBorder.sprite = NeonSprites.RoundedBorder(radius, border);
            _faceGlow.sprite = NeonSprites.Glow(radius);
            _shadow.sprite = NeonSprites.Glow(radius);
            var margin = NeonSprites.GlowMargin * 0.5f;
            _faceGlow.rectTransform.offsetMin = new Vector2(-margin, -margin);
            _faceGlow.rectTransform.offsetMax = new Vector2(margin, margin);

            var m = 0.08f * w;
            var valueSize = 0.16f * w;
            var suitSize = 0.11f * w;
            foreach (var corner in new[] { _topLeft, _bottomRight })
            {
                corner.Root.anchoredPosition = corner == _topLeft ? new Vector2(m, -m) : new Vector2(-m, m);
                corner.Root.sizeDelta = new Vector2(valueSize, valueSize + suitSize);
                var valueRect = corner.Value.rectTransform;
                valueRect.anchorMin = new Vector2(0f, 1f);
                valueRect.anchorMax = new Vector2(1f, 1f);
                valueRect.pivot = new Vector2(0.5f, 1f);
                valueRect.sizeDelta = new Vector2(0f, valueSize);
                valueRect.anchoredPosition = Vector2.zero;
                corner.Value.fontSize = valueSize;
                var suitRect = corner.Suit.rectTransform;
                suitRect.anchorMin = suitRect.anchorMax = new Vector2(0.5f, 1f);
                suitRect.pivot = new Vector2(0.5f, 1f);
                suitRect.sizeDelta = new Vector2(suitSize, suitSize);
                suitRect.anchoredPosition = new Vector2(0f, -valueSize);
            }

            var center = 0.40f * w;
            SetCentered(_centerSuit.rectTransform, center);
            SetCentered(_centerGlow.rectTransform, center * 1.18f);
        }

        private static void SetCentered(RectTransform rect, float size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = Vector2.zero;
        }

        private static SuitShape ShapeOf(CardSuit suit)
        {
            switch (suit)
            {
                case CardSuit.Corazones: return SuitShape.Heart;
                case CardSuit.Diamantes: return SuitShape.Diamond;
                case CardSuit.Treboles: return SuitShape.Club;
                default: return SuitShape.Spade;
            }
        }

        private static Image Layer(string name, Transform parent, Sprite sprite, Color color)
        {
            var rect = UiLayout.CreateStretched(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sprite != null && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }
    }
}
