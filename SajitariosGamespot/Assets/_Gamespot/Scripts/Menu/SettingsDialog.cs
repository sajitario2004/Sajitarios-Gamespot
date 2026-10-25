using System;
using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Settings;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using UnityEngine;

namespace Gamespot.Menu
{
    /// <summary>
    /// Ajustes de la app (V062), desde la barra del menú: sonido (el mismo que el botón de silencio), vibración y
    /// "reducir animaciones" (sin brillos que laten ni transiciones; en Android, por defecto, sigue al ajuste
    /// "Quitar animaciones" del sistema). Cada interruptor se guarda al momento.
    /// </summary>
    public static class SettingsDialog
    {
        /// <summary>Abre el diálogo. Devuelve los interruptores (para los tests).</summary>
        public static (NeonSwitch sound, NeonSwitch vibration, NeonSwitch reduceMotion) Show(AudioService audio)
        {
            NeonSwitch sound = null;
            NeonSwitch vibration = null;
            NeonSwitch reduceMotion = null;
            Dialogs.Show(Loc.T("ajustes"), null, new[]
            {
                new DialogAction(Loc.T("cerrar"), (Action)null, ButtonStyle.Filled, Palette.NeonCyan, 0.6f)
            }, buildContent: content =>
            {
                if (audio != null)
                {
                    sound = NeonSwitch.Create(content, Loc.T("ajustesSonido"), Loc.T("ajustesSonidoSubtitulo"), audio.Enabled);
                    sound.Changed += v => audio.Enabled = v;
                }
                vibration = NeonSwitch.Create(content, Loc.T("ajustesVibracion"), Loc.T("ajustesVibracionSubtitulo"), AppSettings.VibrationEnabled);
                vibration.Changed += v => AppSettings.VibrationEnabled = v;
                reduceMotion = NeonSwitch.Create(content, Loc.T("ajustesReducirAnimaciones"), Loc.T("ajustesReducirAnimacionesSubtitulo"),
                    AppSettings.ReduceMotion);
                reduceMotion.Changed += v => AppSettings.ReduceMotion = v;
            });
            return (sound, vibration, reduceMotion);
        }
    }
}
