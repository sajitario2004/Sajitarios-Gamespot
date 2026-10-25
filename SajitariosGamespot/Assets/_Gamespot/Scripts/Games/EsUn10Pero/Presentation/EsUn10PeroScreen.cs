using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.EsUn10Pero.Domain;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Games.EsUn10Pero
{
    /// <summary>
    /// Pantalla de "Es un 10 pero" (port de <c>EsUn10PeroScreen</c>). Al pulsar "Sacar carta" empieza una cuenta
    /// atrás 5 → 1 (un paso por segundo, botón deshabilitado) y a los 5 s se saca la carta EN ESE MOMENTO, se
    /// voltea y suena <see cref="AppSound.CardFlip"/>. Pulsar durante la cuenta atrás no hace nada.
    /// </summary>
    public sealed class EsUn10PeroScreen : AppScreen
    {
        public const int CountdownStart = 5;

        private readonly DrawCardUseCase _draw;
        private readonly AudioService _audio;

        private int _countdown;
        private float _accumulated;
        private Card _card;

        private CardView _cardView;
        private NeonButton _button;
        private GameObject _hint;
        private GameObject _overlay;
        private TextMeshProUGUI _countText;

        public EsUn10PeroScreen(DrawCardUseCase draw, AudioService audio)
        {
            _draw = draw;
            _audio = audio;
        }

        /// <summary>Segundos que quedan de la cuenta atrás (0 = no hay cuenta atrás).</summary>
        public int Countdown => _countdown;

        /// <summary>Última carta sacada (nula si aún no hay).</summary>
        public Card Card => _card;

        public NeonButton DrawButton => _button;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("esUn10PeroTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            GameUi.RulesButton(scaffold.AppBar, Router, "esUn10PeroTitulo", GameUi.RuleKeys("reglasEsUn10Pero", 4));
            if (_audio != null) MuteButton.Create(scaffold.AppBar.Actions, _audio).Sized(NeonButton.MinTouchSize, NeonButton.MinTouchSize);
            scaffold.AppBar.Actions.Sized(NeonButton.MinTouchSize * scaffold.AppBar.Actions.childCount, NeonButton.MinTouchSize);
            scaffold.AppBar.Leading.Sized(NeonButton.MinTouchSize * scaffold.AppBar.Actions.childCount, NeonButton.MinTouchSize);

            var column = Ui.Column(scaffold.Body, 24f, new RectOffset(24, 24, 24, 24));
            column.Sized(flexHeight: 1f);

            var area = new GameObject("CardArea", typeof(RectTransform));
            area.transform.SetParent(column, false);
            ((RectTransform)area.transform).Sized(flexHeight: 1f);

            var frame = UiLayout.CreateStretched("CardFrame", area.transform);
            var fitter = frame.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = CardView.Aspect;

            _cardView = CardView.Create(frame);
            _cardView.ShowImmediate(_card);

            _hint = BuildHint(frame);
            _overlay = BuildOverlay(frame, out _countText);

            _button = Ui.Button(column, Loc.T("sacarCarta"), OnDrawPressed, ButtonStyle.Filled, IconId.Casino, glow: 0.8f);
            Refresh();
        }

        public override void Tick(float deltaTime)
        {
            if (_countdown <= 0) return;
            _accumulated += deltaTime;
            while (_accumulated >= 1f && _countdown > 0)
            {
                _accumulated -= 1f;
                Step();
            }
        }

        /// <summary>Pulsar "Sacar carta" (también lo usan los tests).</summary>
        public void OnDrawPressed()
        {
            if (_countdown > 0) return;
            _countdown = CountdownStart;
            _accumulated = 0f;
            Refresh();
        }

        private void Step()
        {
            if (_countdown > 1)
            {
                _countdown--;
                Refresh();
                return;
            }
            _countdown = 0;
            Reveal();
        }

        private void Reveal()
        {
            _card = _draw.Execute();
            _cardView.FlipTo(_card);
            _audio?.Play(AppSound.CardFlip);
            Refresh();
        }

        private void Refresh()
        {
            var counting = _countdown > 0;
            _button.Interactable = !counting;
            _button.SetLabel(Loc.T(_card == null ? "sacarCarta" : "sacarOtraCarta"));
            _hint.SetActive(_card == null && !counting);
            _overlay.SetActive(counting);
            _countText.text = counting ? _countdown.ToString() : string.Empty;
            _cardView.name = counting
                ? Loc.T("sacandoCartaEn")
                : _card == null
                    ? Loc.T("cartaSinSacarSemantica")
                    : Loc.T("cartaSemantica", ("valor", _card.Value.Label()), ("palo", Loc.T(SuitKey(_card.Suit))));
        }

        /// <summary>Clave del nombre localizado del palo (en el Flutter estaba fijo en español).</summary>
        public static string SuitKey(CardSuit suit)
        {
            switch (suit)
            {
                case CardSuit.Corazones: return "paloCorazones";
                case CardSuit.Diamantes: return "paloDiamantes";
                case CardSuit.Treboles: return "paloTreboles";
                default: return "paloEspadas";
            }
        }

        private static GameObject BuildHint(Transform parent)
        {
            var rect = UiLayout.CreateStretched("Hint", parent);
            var column = Ui.Column(rect, 0f, new RectOffset(24, 24, 24, 24), TextAnchor.LowerCenter);
            UiLayout.Stretch(column);
            Ui.Text(column, Loc.T("pistaCartaVacia"), TextRole.TitleMedium, Palette.TextPrimary, TextAlignmentOptions.Center, 0.8f,
                Palette.NeonCyan, 3, true);
            return rect.gameObject;
        }

        private static GameObject BuildOverlay(Transform parent, out TextMeshProUGUI count)
        {
            var rect = UiLayout.CreateStretched("Countdown", parent);
            var background = rect.gameObject.AddComponent<Image>();
            background.sprite = NeonSprites.RoundedRect(24f);
            background.type = Image.Type.Sliced;
            background.color = Palette.Background.WithAlpha(0.72f);
            background.raycastTarget = false;

            var column = Ui.Column(rect, 16f, new RectOffset(16, 16, 16, 16), TextAnchor.MiddleCenter);
            UiLayout.Stretch(column);
            Ui.Text(column, Loc.T("sacandoCartaEn"), TextRole.TitleMedium, Palette.TextPrimary, TextAlignmentOptions.Center, 0.8f,
                Palette.NeonCyan, bold: true);
            count = Ui.FittedText(column, "5", 96f, Palette.NeonCyan, 1f);
            return rect.gameObject;
        }
    }
}
