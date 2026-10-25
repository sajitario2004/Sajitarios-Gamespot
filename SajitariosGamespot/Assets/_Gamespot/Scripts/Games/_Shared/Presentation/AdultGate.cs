using System;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using UnityEngine;

namespace Gamespot.Games.Shared
{
    /// <summary>
    /// Confirmación de edad para el contenido +18 (V070): la primera vez en la sesión de la app se pregunta si todos son
    /// mayores de 18; si se confirma, ya no se vuelve a preguntar en ningún juego hasta cerrar la app.
    /// </summary>
    public static class AdultGate
    {
        public static bool Confirmed { get; set; }

        /// <summary>Ejecuta <paramref name="onConfirmed"/> ya o después de confirmar; si se cancela, no hace nada.</summary>
        public static void Request(Action onConfirmed, string titleKey = "adultosTitulo", string messageKey = "adultosMensaje",
            string yesKey = "adultosSi")
        {
            if (Confirmed)
            {
                onConfirmed?.Invoke();
                return;
            }
            Dialogs.Confirm(Loc.T(titleKey), Loc.T(messageKey), Loc.T("cancelar"), Loc.T(yesKey), ok =>
            {
                if (!ok) return;
                Confirmed = true;
                onConfirmed?.Invoke();
            }, Palette.NeonMagenta, Palette.NeonMagenta);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Confirmed = false;
    }
}
