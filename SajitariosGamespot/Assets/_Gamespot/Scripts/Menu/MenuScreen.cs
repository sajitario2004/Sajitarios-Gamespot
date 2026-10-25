using System.Collections.Generic;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamespot.Menu
{
    /// <summary>
    /// Menú principal (port de <c>MenuScreen</c>). Solo conoce la lista de <see cref="IGameDescriptor"/>: nunca un
    /// juego concreto. Cabecera, rejilla de tarjetas (2 columnas, 3 si el ancho supera 600) con acento cian en
    /// las pares y magenta en las impares; pulsar una abre la ruta de entrada del juego.
    /// </summary>
    public sealed class MenuScreen : AppScreen
    {
        public const float HeaderHeight = 140f;
        public const float CardAspect = 0.9f;

        private readonly IReadOnlyList<IGameDescriptor> _games;
        private readonly AudioService _audio;

        public MenuScreen(IReadOnlyList<IGameDescriptor> games, AudioService audio)
        {
            _games = games;
            _audio = audio;
        }

        /// <summary>Tarjetas creadas (para los tests).</summary>
        public List<NeonButton> Cards { get; } = new List<NeonButton>();

        public override void Build(RectTransform root)
        {
            Cards.Clear();
            var scaffold = ScreenScaffold.Create(root, Loc.T("appTitle"));
            var title = scaffold.AppBar.Title;
            PulseGlow.Attach(title.gameObject, 0.8f, 1.3f, v => Ui.SetGlow(title, Palette.NeonCyan, v));
            if (_audio != null) scaffold.AppBar.Actions.Sized(NeonButton.MinTouchSize * 2f, NeonButton.MinTouchSize);
            LanguageSelector.CreateButton(scaffold.AppBar.Actions);
            if (_audio != null) MuteButton.Create(scaffold.AppBar.Actions, _audio);
            // El menú no tiene botón a la izquierda: el título usa ese hueco (como Flutter cuando no cabe centrado).
            scaffold.AppBar.Leading.Sized(8f, NeonButton.MinTouchSize);

            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 0, 24));
            var header = Resources.Load<Texture2D>("Images/menu_header");
            if (header != null) CoverImage.Create(content, header, HeaderHeight);

            if (_games == null || _games.Count == 0)
            {
                BuildEmpty(content);
                return;
            }

            var grid = ResponsiveGrid.Create(content, 16f, CardAspect);
            for (var i = 0; i < _games.Count; i++)
            {
                Cards.Add(BuildCard(grid.transform, _games[i], i % 2 == 0 ? Palette.NeonCyan : Palette.NeonMagenta));
            }
        }

        private NeonButton BuildCard(Transform parent, IGameDescriptor game, Color accent)
        {
            var title = Loc.T(game.TitleKey);
            var description = Loc.T(game.DescriptionKey);
            var card = NeonButton.Create(parent, null, () => Router.Go(game.EntryRoute), ButtonStyle.Outlined, accent: accent, glow: 0.6f,
                radius: 20f, name: $"Card {game.Id}", vertical: true);
            card.name = Loc.T("jugarA", ("titulo", title), ("descripcion", description));

            // La tarjeta es una columna centrada: icono, título y descripción.
            var column = card.GetComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(16, 16, 16, 16);
            column.childForceExpandWidth = true;

            var icon = Ui.Icon(card.transform, game.Icon, 56f, accent, 0.9f);
            icon.Text.alignment = TextAlignmentOptions.Center;
            Ui.Space(card.transform, 8f);
            Ui.Text(card.transform, title, TextRole.TitleMedium, Palette.TextPrimary, TextAlignmentOptions.Center, 0.8f, accent, 2, true);
            Ui.Text(card.transform, description, TextRole.BodySmall, Palette.TextMuted, TextAlignmentOptions.Center, maxLines: 3);
            return card;
        }

        private static void BuildEmpty(RectTransform content)
        {
            var panel = Ui.Panel(content, Palette.NeonCyan, glow: 0.6f, padding: new RectOffset(28, 28, 28, 28), spacing: 12f,
                alignment: TextAnchor.UpperCenter);
            Ui.Icon(panel.Content, IconId.VideogameAsset, 72f, Palette.NeonCyan, 0.8f);
            Ui.Text(panel.Content, Loc.T("menuVacioTitulo"), TextRole.TitleLarge, alignment: TextAlignmentOptions.Center);
            Ui.Text(panel.Content, Loc.T("menuVacioMensaje"), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
        }
    }
}
