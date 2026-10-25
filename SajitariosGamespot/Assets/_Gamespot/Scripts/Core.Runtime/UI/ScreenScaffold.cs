using UnityEngine;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Andamio de pantalla (como <c>Scaffold</c>): barra superior arriba y cuerpo debajo, que ocupa el resto.
    /// </summary>
    public sealed class ScreenScaffold
    {
        private ScreenScaffold(AppBar appBar, RectTransform body)
        {
            AppBar = appBar;
            Body = body;
        }

        public AppBar AppBar { get; }

        /// <summary>Zona bajo la barra (una columna sin padding que ocupa el alto restante).</summary>
        public RectTransform Body { get; }

        public static ScreenScaffold Create(RectTransform root, string title, string subtitle = null)
        {
            var column = Ui.Column(root, 0f, name: "Scaffold");
            UiLayout.Stretch(column);
            var appBar = AppBar.Create(column, title, subtitle);
            var body = Ui.Column(column, 0f, name: "Body");
            body.Sized(flexWidth: 1f, flexHeight: 1f);
            return new ScreenScaffold(appBar, body);
        }
    }
}
