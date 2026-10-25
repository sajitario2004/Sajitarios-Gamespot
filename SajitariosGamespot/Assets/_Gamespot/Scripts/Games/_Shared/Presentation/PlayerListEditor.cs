using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Settings;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using UnityEngine;

namespace Gamespot.Games.Shared
{
    /// <summary>
    /// Lista ordenada de nombres de jugadores de los setups (la UX del Impostor que copian Trivia, Wavelength y
    /// La Bomba): fila con avatar numerado, campo "Jugador n" y botón de quitar (deshabilitado en el mínimo), y
    /// "Añadir jugador" hasta el máximo (entonces dice "Máximo de jugadores alcanzado"). Sin reordenar: el orden
    /// de las filas es el orden de juego.
    /// Los nombres de la última partida empezada (<see cref="AppSettings.LastPlayers"/>, compartidos entre juegos)
    /// rellenan la lista si llega vacía, y "Usar los de la última partida" los recupera cuando no coinciden.
    /// </summary>
    public sealed class PlayerListEditor : MonoBehaviour
    {
        private readonly List<string> _values = new List<string>();
        private RectTransform _rows;
        private NeonButton _add;
        private NeonButton _useLast;
        private int _min;
        private int _max;
        private Color _accent;

        /// <summary>Se lanza al añadir o quitar un jugador (los contadores dependientes se recortan).</summary>
        public event Action CountChanged;

        /// <summary>Se lanza al escribir en cualquier campo.</summary>
        public event Action NamesChanged;

        public int Count => _values.Count;

        /// <summary>Nombres tal cual se escribieron.</summary>
        public IReadOnlyList<string> RawNames => _values;

        /// <summary>Nombres recortados y con los espacios colapsados (lo que se pasa a la configuración).</summary>
        public IReadOnlyList<string> NormalizedNames => _values.Select(NameNormalizer.Normalize).ToList();

        /// <summary>Si se ve el botón "Usar los de la última partida".</summary>
        public bool CanUseLastPlayers => _useLast != null && _useLast.gameObject.activeSelf;

        public static PlayerListEditor Create(Transform parent, int min, int max, Color accent, IReadOnlyList<string> initial = null)
        {
            var column = Ui.Column(parent, 10f, name: "PlayerList");
            var editor = column.gameObject.AddComponent<PlayerListEditor>();
            editor._min = min;
            editor._max = max;
            editor._accent = accent;
            editor._useLast = Ui.Button(column, Loc.T("usarUltimosJugadores"), editor.UseLastPlayers, ButtonStyle.Text, IconId.History, accent);
            editor._rows = Ui.Column(column, 10f, name: "Rows");
            editor._add = Ui.Button(column, Loc.T("anadirJugador"), editor.AddPlayer, ButtonStyle.Outlined, IconId.PersonAdd, accent);

            // Sin ningún nombre escrito (primera vez en la sesión) se empieza con los de la última partida.
            var hasNames = initial != null && initial.Any(n => !string.IsNullOrWhiteSpace(n));
            editor._values.AddRange(hasNames ? initial : editor.LastPlayers());
            while (editor._values.Count < min) editor._values.Add(string.Empty);
            editor.Rebuild(-1);
            return editor;
        }

        /// <summary>Pone los nombres de la última partida (recortados al máximo de este juego).</summary>
        public void UseLastPlayers()
        {
            var last = LastPlayers();
            if (last.Count == 0) return;
            _values.Clear();
            _values.AddRange(last);
            while (_values.Count < _min) _values.Add(string.Empty);
            Rebuild(-1);
            CountChanged?.Invoke();
            NamesChanged?.Invoke();
        }

        /// <summary>Guarda los nombres actuales como los de la última partida (se llama al empezar la partida).</summary>
        public void Remember() => AppSettings.LastPlayers = NormalizedNames;

        private List<string> LastPlayers() => AppSettings.LastPlayers.Take(_max).ToList();

        private void RefreshUseLast()
        {
            var last = LastPlayers();
            var current = NormalizedNames.Where(n => n.Length > 0);
            _useLast.gameObject.SetActive(last.Count > 0 && !last.SequenceEqual(current));
        }

        public void AddPlayer()
        {
            if (_values.Count >= _max) return;
            _values.Add(string.Empty);
            Rebuild(_values.Count - 1);
            CountChanged?.Invoke();
        }

        public void RemovePlayer(int index)
        {
            if (_values.Count <= _min || index < 0 || index >= _values.Count) return;
            _values.RemoveAt(index);
            Rebuild(-1);
            CountChanged?.Invoke();
        }

        /// <summary>Pone un nombre (para los tests).</summary>
        public void SetName(int index, string name)
        {
            _values[index] = name ?? string.Empty;
            Rebuild(-1);
            NamesChanged?.Invoke();
        }

        private void Rebuild(int focusIndex)
        {
            _rows.ClearChildren();
            for (var i = 0; i < _values.Count; i++)
            {
                var index = i;
                var row = Ui.Row(_rows, 10f, alignment: TextAnchor.MiddleLeft, name: $"Player {i + 1}");
                Ui.NumberBadge(row, (i + 1).ToString(), _accent, 32f);
                var field = NeonTextField.Create(row, Loc.T("jugadorNumero", ("n", i + 1)), Loc.T("nombreDelJugadorNumero", ("n", i + 1)),
                    _values[i], characterLimit: 40);
                field.Sized(flexWidth: 1f);
                field.Changed += value =>
                {
                    _values[index] = value;
                    RefreshUseLast();
                    NamesChanged?.Invoke();
                };
                var remove = NeonButton.CreateIcon(row, IconId.RemoveCircle, () => RemovePlayer(index), Palette.TextMuted);
                remove.name = Loc.T("quitarJugador");
                remove.Interactable = _values.Count > _min;
                if (i == focusIndex) field.Focus();
            }

            var full = _values.Count >= _max;
            _add.Interactable = !full;
            _add.SetLabel(Loc.T(full ? "maximoJugadoresAlcanzado" : "anadirJugador"));
            RefreshUseLast();
        }
    }
}
