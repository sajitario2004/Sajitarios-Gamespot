using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.UI;
using UnityEngine;

namespace Gamespot.Core.Runtime.Audio
{
    /// <summary>Botón de silencio (icono volumen activado / silenciado), como el <c>MuteButton</c> del Flutter.</summary>
    public static class MuteButton
    {
        public static NeonButton Create(Transform parent, AudioService audio)
        {
            NeonButton button = null;
            button = NeonButton.CreateIcon(parent, audio.Enabled ? IconId.VolumeUp : IconId.VolumeOff, () =>
            {
                var enabled = audio.Toggle();
                button.Icon.Set(enabled ? IconId.VolumeUp : IconId.VolumeOff);
                button.name = Loc.T(enabled ? "silenciarSonido" : "activarSonido");
            });
            button.name = Loc.T(audio.Enabled ? "silenciarSonido" : "activarSonido");
            return button;
        }
    }
}
