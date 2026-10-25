using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using TMPro;
using UnityEngine;

namespace Gamespot.Core.Runtime.Localization
{
    /// <summary>
    /// Botón de idioma de la AppBar y su diálogo: "Idioma del sistema", "Español" e "Inglés" (el elegido lleva
    /// una marca) y "Cancelar". Elegir uno lo guarda y reconstruye las pantallas.
    /// </summary>
    public static class LanguageSelector
    {
        public static NeonButton CreateButton(Transform parent)
        {
            var button = NeonButton.CreateIcon(parent, IconId.Language, ShowDialog);
            button.name = Loc.T("cambiarIdioma");
            return button;
        }

        public static void ShowDialog()
        {
            var current = Loc.Preference;
            Dialogs.Show(Loc.T("idioma"), null, new[]
            {
                new DialogAction(Loc.T("cancelar"), (System.Action)null, ButtonStyle.Text, Palette.NeonCyan)
            }, buildContent: content =>
            {
                Option(content, Loc.T("idiomaDelSistema"), current == null, null);
                Option(content, Loc.T("espanol"), current == Loc.Spanish, Loc.Spanish);
                Option(content, Loc.T("ingles"), current == Loc.English, Loc.English);
            });
        }

        private static void Option(RectTransform parent, string label, bool selected, string language)
        {
            var button = NeonButton.Create(parent, label, () =>
            {
                Dialogs.HandleBack();
                Loc.SetPreference(language);
            }, ButtonStyle.Outlined, selected ? IconId.Check : (IconId?)null, selected ? Palette.NeonCyan : Palette.TextMuted, fontSize: 16f);
            if (button.Label != null) button.Label.alignment = TextAlignmentOptions.Left;
        }
    }
}
