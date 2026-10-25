using System;
using System.Collections.Generic;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Impostor.Domain;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Impostor
{
    /// <summary>
    /// Historial y estadísticas (port de <c>HistoryScreen</c>): partidas jugadas, palabra más repetida y veces que
    /// cada jugador fue impostor; cada partida se despliega con la pista y el rol de cada jugador (el historial sí
    /// revela roles, porque la partida ya terminó). "Borrar todo" pide confirmación.
    /// </summary>
    public sealed class HistoryScreen : AppScreen
    {
        private readonly ImpostorModule _module;
        private readonly HashSet<long> _expanded = new HashSet<long>();
        private RectTransform _content;
        private List<GameRecord> _records = new List<GameRecord>();

        public HistoryScreen(ImpostorModule module) => _module = module;

        public IReadOnlyList<GameRecord> Records => _records;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("historial"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            scaffold.AppBar.AddAction(IconId.Delete, ConfirmDeleteAll, Palette.NeonError).name = Loc.T("borrarHistorialTitulo");
            _content = Ui.Scroll(scaffold.Body, 14f, new RectOffset(16, 16, 8, 32));
            Reload();
        }

        public void Reload()
        {
            _content.ClearChildren();
            try
            {
                _records = _module.History.GetAll();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Impostor] No se pudo cargar el historial: {e.Message}");
                Ui.Text(_content, Loc.T("noSePudoCargarHistorial"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center);
                Ui.Button(_content, Loc.T("reintentar"), Reload, ButtonStyle.Outlined, IconId.Refresh);
                return;
            }

            if (_records.Count == 0)
            {
                var empty = Ui.Panel(_content, Palette.NeonCyan.WithAlpha(0.6f), glow: 0.4f, padding: new RectOffset(24, 24, 28, 28), spacing: 10f,
                    alignment: TextAnchor.UpperCenter);
                Ui.Icon(empty.Content, IconId.History, 56f, Palette.NeonCyan, 0.8f);
                Ui.Text(empty.Content, Loc.T("historialVacioTitulo"), TextRole.TitleMedium, alignment: TextAlignmentOptions.Center, bold: true);
                Ui.Text(empty.Content, Loc.T("historialVacioMensaje"), TextRole.BodyMedium, Palette.TextMuted, TextAlignmentOptions.Center);
                return;
            }

            BuildStats(ImpostorStats.Compute(_records));
            Ui.Text(_content, Loc.T("partidas"), TextRole.TitleMedium, Palette.NeonCyan, glow: 0.6f, bold: true);
            foreach (var record in _records) BuildRecord(record);
        }

        private void BuildStats(ImpostorStats stats)
        {
            var panel = Ui.Panel(_content, Palette.NeonViolet, glow: 0.5f, spacing: 8f);
            Ui.Text(panel.Content, Loc.T("estadisticas"), TextRole.TitleMedium, Palette.NeonVioletText, glow: 0.6f, bold: true);
            Line(panel.Content, Loc.T("partidasJugadas"), stats.Total.ToString());
            Line(panel.Content, Loc.T("palabraMasRepetida"), stats.MostRepeatedWord == null
                ? "—"
                : Loc.T("palabraMasRepetidaValor", ("palabra", stats.MostRepeatedWord), ("veces", stats.MostRepeatedCount)));
            Ui.Text(panel.Content, Loc.T("vecesQueFueImpostor"), TextRole.LabelLarge, Palette.TextMuted);
            if (stats.ImpostorRanking.Count == 0)
            {
                Ui.Text(panel.Content, Loc.T("nadieFueImpostor"), TextRole.BodyMedium, Palette.TextMuted);
            }
            foreach (var (name, count) in stats.ImpostorRanking) Line(panel.Content, name, count.ToString());
        }

        private void BuildRecord(GameRecord record)
        {
            var expanded = _expanded.Contains(record.Id);
            var button = NeonButton.Create(_content, null, () => Toggle(record.Id), ButtonStyle.Outlined, accent: Palette.NeonCyan.WithAlpha(0.7f),
                radius: 16f, name: $"Record {record.Id}", vertical: true);
            var layout = button.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 12, 12);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childForceExpandWidth = true;

            var header = Ui.Row(button.transform, 10f);
            var texts = Ui.Column(header, 2f, alignment: TextAnchor.MiddleLeft);
            texts.Sized(flexWidth: 1f);
            Ui.Text(texts, record.Word, TextRole.TitleMedium, maxLines: 1, bold: true);
            var date = $"{record.CreatedAt.ToString("d", Loc.Culture)} {record.CreatedAt.ToString("HH:mm", Loc.Culture)}";
            Ui.Text(texts, Loc.T("partidaSubtitulo", ("fecha", date), ("jugadores", record.NPlayers),
                ("impostores", Loc.T("impostoresTexto", ("n", record.NImpostors)))), TextRole.BodySmall, Palette.TextMuted);
            Ui.Icon(header, IconId.Lightbulb, 22f, record.HintEnabled ? Palette.NeonCyan : Palette.TextMuted.WithAlpha(0.4f));

            if (!expanded) return;
            Ui.Divider(button.transform);
            var hintText = record.HintEnabled
                ? string.IsNullOrEmpty(record.Hint) ? Loc.T("pistaActivada") : Loc.T("pistaActivadaConValor", ("pista", record.Hint))
                : Loc.T("pistaDesactivada");
            Ui.Text(button.transform, hintText, TextRole.BodyMedium, Palette.TextMuted);
            foreach (var player in record.Players)
            {
                var row = Ui.Row(button.transform, 8f);
                var name = Ui.Text(row, player.Name, TextRole.BodyLarge);
                name.Sized(flexWidth: 1f);
                Ui.Text(row, player.WasImpostor ? Loc.T("impostorMayus") : Loc.T("sabiaLaPalabra"), TextRole.LabelLarge,
                    player.WasImpostor ? Palette.NeonError : Palette.TextMuted, TextAlignmentOptions.Right, bold: player.WasImpostor);
            }
        }

        private void Toggle(long id)
        {
            if (!_expanded.Remove(id)) _expanded.Add(id);
            Reload();
        }

        private void ConfirmDeleteAll()
        {
            Dialogs.Confirm(Loc.T("borrarHistorialTitulo"), Loc.T("borrarHistorialMensaje"), Loc.T("cancelar"), Loc.T("borrarTodo"), ok =>
            {
                if (!ok) return;
                try
                {
                    _module.History.DeleteAll();
                    Snackbar.Show(Loc.T("historialBorrado"));
                }
                catch (Exception)
                {
                    Snackbar.Show(Loc.T("noSePudoBorrarHistorial"), Palette.NeonError);
                }
                Reload();
            }, Palette.NeonError);
        }

        private static void Line(Transform parent, string label, string value)
        {
            var row = Ui.Row(parent, 8f);
            var left = Ui.Text(row, label, TextRole.BodyMedium);
            left.Sized(flexWidth: 1f);
            Ui.Text(row, value, TextRole.BodyMedium, Palette.NeonCyan, TextAlignmentOptions.Right, bold: true);
        }
    }
}
