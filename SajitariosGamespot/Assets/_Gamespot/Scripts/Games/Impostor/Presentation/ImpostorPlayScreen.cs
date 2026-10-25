using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Impostor
{
    /// <summary>
    /// Base de las pantallas con partida en curso (pasar, revelar, votar): la casa y el botón atrás piden
    /// confirmación ("¿Salir de la partida?"); al confirmar, la casa va al menú y atrás vuelve al setup.
    /// Sin partida muestra "No hay ninguna partida en curso" con un botón para configurarla.
    /// </summary>
    public abstract class ImpostorPlayScreen : AppScreen
    {
        protected ImpostorPlayScreen(ImpostorModule module) => Module = module;

        protected ImpostorModule Module { get; }

        protected virtual string TitleKey => "impostorTitulo";

        public sealed override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T(TitleKey));
            GameUi.HomeButton(scaffold.AppBar, Router, () => Module.Flow.Reiniciar());
            if (Module.Flow.Session == null)
            {
                BuildNoGame(scaffold.Body);
                return;
            }
            BuildGame(scaffold);
        }

        protected abstract void BuildGame(ScreenScaffold scaffold);

        public override bool OnBack()
        {
            if (Module.Flow.Session == null) return false;
            AbandonGameDialog.Show(Module.ResetToSetup);
            return true;
        }

        private void BuildNoGame(RectTransform body)
        {
            var column = Ui.Column(body, 16f, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter);
            column.Sized(flexHeight: 1f);
            Ui.Text(column, Loc.T("noHayPartidaEnCurso"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
            Ui.Button(column, Loc.T("configurarPartida"), () => Router.Go(ImpostorModule.SetupRoute));
        }

        /// <summary>Columna centrada que ocupa el cuerpo, con scroll por si la letra es grande.</summary>
        protected static RectTransform CenteredBody(RectTransform body, float spacing = 16f)
        {
            var content = Ui.Scroll(body, spacing, new RectOffset(24, 24, 24, 24));
            return content;
        }
    }
}
