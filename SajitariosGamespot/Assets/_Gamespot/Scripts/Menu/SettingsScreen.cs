using Gamespot.Core.Runtime.Audio;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Settings;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;

namespace Gamespot.Menu
{
    /// <summary>
    /// Ajustes de la app (V059p, petición del autor; sustituye al diálogo de V062), desde el engranaje del menú:
    /// sonido y vibración, idioma y reducir animaciones, y la información legal (términos y condiciones de uso,
    /// política de privacidad, política de cookies y licencias), cada documento en su <see cref="LegalScreen"/>.
    /// Cada interruptor se guarda al momento.
    /// </summary>
    public sealed class SettingsScreen : AppScreen
    {
        public const string Route = "/ajustes";

        /// <summary>Documentos legales: clave del título y del texto.</summary>
        public static readonly (string title, string body)[] LegalDocuments =
        {
            ("legalTerminosTitulo", "legalTerminosTexto"),
            ("legalPrivacidadTitulo", "legalPrivacidadTexto"),
            ("legalCookiesTitulo", "legalCookiesTexto"),
            ("legalLicenciasTitulo", "legalLicenciasTexto")
        };

        private readonly AudioService _audio;

        public SettingsScreen(AudioService audio) => _audio = audio;

        public NeonSwitch Sound { get; private set; }
        public NeonSwitch Vibration { get; private set; }
        public NeonSwitch ReduceMotion { get; private set; }

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("ajustes"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            var content = Ui.Scroll(scaffold.Body, 16f, new RectOffset(16, 16, 8, 32));

            var soundPanel = Ui.Panel(content, Palette.NeonCyan.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(soundPanel.Content, Loc.T("ajustesSonidoVibracion"), TextRole.TitleMedium, Palette.NeonCyan, glow: 0.6f, bold: true);
            if (_audio != null)
            {
                Sound = NeonSwitch.Create(soundPanel.Content, Loc.T("ajustesSonido"), Loc.T("ajustesSonidoSubtitulo"), _audio.Enabled);
                Sound.Changed += v => _audio.Enabled = v;
            }
            Vibration = NeonSwitch.Create(soundPanel.Content, Loc.T("ajustesVibracion"), Loc.T("ajustesVibracionSubtitulo"), AppSettings.VibrationEnabled);
            Vibration.Changed += v => AppSettings.VibrationEnabled = v;

            var screenPanel = Ui.Panel(content, Palette.NeonViolet.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(screenPanel.Content, Loc.T("ajustesPantalla"), TextRole.TitleMedium, Palette.NeonVioletText, glow: 0.6f, bold: true);
            ReduceMotion = NeonSwitch.Create(screenPanel.Content, Loc.T("ajustesReducirAnimaciones"), Loc.T("ajustesReducirAnimacionesSubtitulo"),
                AppSettings.ReduceMotion);
            ReduceMotion.Changed += v => AppSettings.ReduceMotion = v;
            Ui.Button(screenPanel.Content, Loc.T("ajustesIdioma"), LanguageSelector.ShowDialog, ButtonStyle.Outlined, IconId.Language,
                accent: Palette.NeonViolet);

            var legalPanel = Ui.Panel(content, Palette.NeonMagenta.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(legalPanel.Content, Loc.T("ajustesInformacionLegal"), TextRole.TitleMedium, Palette.NeonMagenta, glow: 0.6f, bold: true);
            foreach (var (title, body) in LegalDocuments)
            {
                var t = title;
                var b = body;
                Ui.Button(legalPanel.Content, Loc.T(t), () => OpenLegal(t, b), ButtonStyle.Outlined, IconId.MenuBook, accent: Palette.NeonMagenta)
                    .name = Loc.T(t);
            }

            Ui.Text(content, Loc.T("ajustesVersion", ("version", Application.version)), TextRole.BodySmall, Palette.TextMuted,
                TextAlignmentOptions.Center);
        }

        /// <summary>Abre un documento legal (también lo usan los tests).</summary>
        public LegalScreen OpenLegal(string titleKey, string bodyKey)
        {
            var screen = new LegalScreen(titleKey, bodyKey);
            Router.Push(screen);
            return screen;
        }
    }
}
