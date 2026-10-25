using System;
using System.Collections.Generic;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;

namespace Gamespot.Games.Shared
{
    /// <summary>Piezas de UI comunes a todos los juegos (botón de volver al menú, reglas, abandonar).</summary>
    public static class GameUi
    {
        /// <summary>
        /// Botón "volver al menú" (icono casa) a la izquierda de la AppBar. Sin partida en curso va directo a
        /// <c>/</c>; con <paramref name="confirmAndReset"/> pide confirmación y, al confirmar, lo ejecuta antes.
        /// </summary>
        public static NeonButton HomeButton(AppBar appBar, Router router, Action confirmAndReset = null)
        {
            var button = appBar.SetLeading(IconId.Home, () =>
            {
                if (confirmAndReset == null)
                {
                    router.Go(Router.Home);
                    return;
                }
                AbandonGameDialog.Show(() =>
                {
                    confirmAndReset();
                    router.Go(Router.Home);
                });
            });
            button.name = Loc.T("volverAlMenu");
            return button;
        }

        /// <summary>Botón "¿Cómo se juega?" que apila la pantalla de reglas del juego.</summary>
        public static NeonButton RulesButton(AppBar appBar, Router router, string gameTitleKey, IReadOnlyList<string> stepKeys)
        {
            var button = appBar.AddAction(IconId.Help, () => router.Push(new RulesScreen(gameTitleKey, stepKeys)));
            button.name = Loc.T("comoSeJuega");
            return button;
        }

        /// <summary>
        /// Botón «Terminar partida» (bandera) de los juegos sin final fijo (V071): pide confirmación y, al confirmar,
        /// ejecuta <paramref name="onConfirm"/> (que lleva a la clasificación).
        /// </summary>
        public static NeonButton FinishButton(AppBar appBar, Action onConfirm)
        {
            var button = appBar.AddAction(IconId.Flag, () => Dialogs.Confirm(Loc.T("terminarPartidaTitulo"), Loc.T("terminarPartidaMensaje"),
                Loc.T("seguirJugando"), Loc.T("terminar"), ok =>
                {
                    if (ok) onConfirm?.Invoke();
                }, Palette.NeonMagenta));
            button.name = Loc.T("terminarPartida");
            return button;
        }

        /// <summary>Claves <c>prefijo1..N</c> de los pasos de las reglas.</summary>
        public static IReadOnlyList<string> RuleKeys(string prefix, int count)
        {
            var keys = new string[count];
            for (var i = 0; i < count; i++) keys[i] = $"{prefix}{i + 1}";
            return keys;
        }
    }

    /// <summary>
    /// Diálogo "¿Salir de la partida?" (port de <c>abandonarPartidaDialog</c>, ahora en lo compartido:
    /// corrige el acoplamiento entre juegos del Flutter). Solo llama a <paramref name="onConfirm"/> al salir.
    /// </summary>
    public static class AbandonGameDialog
    {
        public static void Show(Action onConfirm, Action onCancel = null)
        {
            Dialogs.Confirm(Loc.T("salirDeLaPartidaTitulo"), Loc.T("salirDeLaPartidaMensaje"), Loc.T("seguirJugando"), Loc.T("salir"),
                confirmed =>
                {
                    if (confirmed) onConfirm?.Invoke();
                    else onCancel?.Invoke();
                }, Palette.NeonMagenta, Palette.NeonMagenta);
        }
    }
}
