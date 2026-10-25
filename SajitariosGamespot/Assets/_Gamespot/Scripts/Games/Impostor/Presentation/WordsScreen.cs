using System;
using System.Collections.Generic;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Impostor.Data;
using Gamespot.Games.Shared;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Impostor
{
    /// <summary>
    /// Gestión de palabras (port de <c>WordsManagementScreen</c>): búsqueda (la consulta espera 280 ms desde la
    /// última tecla), lista virtualizada (2724 palabras de seed), alta, edición y borrado de las propias. Las predefinidas
    /// llevan borde violeta y candado y no se pueden editar ni borrar.
    /// </summary>
    public sealed class WordsScreen : AppScreen
    {
        public const float DebounceSeconds = 0.28f;
        private const float RowHeight = 76f;

        private readonly ImpostorModule _module;
        private List<ImpostorWord> _items = new List<ImpostorWord>();
        private string _query = string.Empty;
        private float _debounce = -1f;
        private VirtualList _list;
        private NeonButton _clear;
        private GameObject _empty;
        private TMP_Text _emptyText;

        public WordsScreen(ImpostorModule module) => _module = module;

        public IReadOnlyList<ImpostorWord> Items => _items;
        public VirtualList List => _list;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("gestionarPalabrasTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);

            var searchRow = Ui.Row(scaffold.Body, 8f, new RectOffset(16, 16, 8, 8), name: "Search");
            var search = NeonTextField.Create(searchRow, null, Loc.T("buscarPalabra"), _query);
            search.Sized(flexWidth: 1f);
            search.Changed += SetQuery;
            _clear = NeonButton.CreateIcon(searchRow, IconId.Close, () =>
            {
                search.Value = string.Empty;
                SetQuery(string.Empty);
            }, Palette.TextMuted);
            _clear.name = Loc.T("limpiarBusqueda");

            var listArea = Ui.Column(scaffold.Body, 0f, name: "ListArea");
            listArea.Sized(flexHeight: 1f);
            _list = VirtualList.Create(listArea, RowHeight, CreateRow, BindRow, 10f);
            _empty = UiLayout.CreateStretched("Empty", listArea).gameObject;
            _emptyText = Ui.Text(_empty.transform, string.Empty, TextRole.BodyLarge, Palette.TextMuted, TextAlignmentOptions.Center);
            UiLayout.Stretch(_emptyText.rectTransform);
            _emptyText.margin = new Vector4(32f, 0f, 32f, 0f);
            _empty.AddComponent<UnityEngine.UI.LayoutElement>().ignoreLayout = true;

            var bottom = Ui.Row(scaffold.Body, 0f, new RectOffset(16, 16, 8, 16), TextAnchor.MiddleRight, "Bottom");
            Ui.Flex(bottom);
            Ui.Button(bottom, Loc.T("agregar"), () => OpenForm(null), ButtonStyle.Filled, IconId.Add, glow: 0.8f);

            Reload();
        }

        public override void Tick(float deltaTime)
        {
            if (_debounce < 0f) return;
            _debounce -= deltaTime;
            if (_debounce <= 0f)
            {
                _debounce = -1f;
                Reload();
            }
        }

        /// <summary>Cambia la búsqueda: el texto se actualiza ya; la consulta, a los 280 ms.</summary>
        public void SetQuery(string value)
        {
            _query = value?.Trim() ?? string.Empty;
            _clear.gameObject.SetActive(_query.Length > 0);
            _debounce = DebounceSeconds;
        }

        /// <summary>Vuelve a consultar la base de datos.</summary>
        public void Reload()
        {
            try
            {
                _items = _query.Length == 0 ? _module.Words.GetAll() : _module.Words.Search(_query);
                _emptyText.text = _query.Length == 0 ? Loc.T("sinPalabrasAun") : Loc.T("sinCoincidencias");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Impostor] No se pudieron cargar las palabras: {e.Message}");
                _items = new List<ImpostorWord>();
                _emptyText.text = Loc.T("noSePudieronCargarPalabras");
            }
            _clear.gameObject.SetActive(_query.Length > 0);
            _empty.SetActive(_items.Count == 0);
            _list.SetCount(_items.Count, true);
        }

        private RectTransform CreateRow(RectTransform parent)
        {
            var panel = NeonPanel.Create(parent, Palette.NeonCyan, Palette.Surface, 16f, 1.5f, 0f, padding: new RectOffset(16, 8, 8, 8),
                name: "WordRow");
            var view = panel.gameObject.AddComponent<WordRowView>();
            var row = Ui.Row(panel.Content, 6f, alignment: TextAnchor.MiddleLeft);
            row.Sized(flexHeight: 1f);
            var texts = Ui.Column(row, 2f, alignment: TextAnchor.MiddleLeft);
            texts.Sized(flexWidth: 1f);
            // Hasta dos líneas que encogen en una caja fija: con los famosos y los sitios hay nombres largos
            // («José Luis Rodríguez Zapatero») y la pista tiene que seguir viéndose debajo.
            view.Word = Ui.Text(texts, string.Empty, TextRole.TitleMedium, alignment: TextAlignmentOptions.BottomLeft, maxLines: 2, bold: true);
            view.Word.enableAutoSizing = true;
            view.Word.fontSizeMax = view.Word.fontSize;
            view.Word.fontSizeMin = 11f;
            view.Word.Sized(height: 36f);
            view.Hint = Ui.Text(texts, string.Empty, TextRole.BodySmall, Palette.TextMuted, maxLines: 1);
            view.Seed = Ui.Row(row, 4f, new RectOffset(8, 8, 2, 2), TextAnchor.MiddleCenter, "SeedChip").gameObject;
            Ui.Icon(view.Seed.transform, IconId.Lock, 14f, Palette.NeonVioletText);
            Ui.Text(view.Seed.transform, Loc.T("predefinida"), TextRole.LabelSmall, Palette.NeonVioletText).textWrappingMode =
                TextWrappingModes.NoWrap;
            view.Edit = NeonButton.CreateIcon(row, IconId.Edit, () => OnEdit(view.Index), Palette.NeonCyan, 22f);
            view.Delete = NeonButton.CreateIcon(row, IconId.Delete, () => OnDelete(view.Index), Palette.NeonError, 22f);
            view.Panel = panel;
            return (RectTransform)panel.transform;
        }

        private void BindRow(RectTransform rowRect, int index)
        {
            var view = rowRect.GetComponent<WordRowView>();
            var item = _items[index];
            view.Index = index;
            view.Word.text = item.Word;
            view.Hint.text = Loc.T("pistaConValor", ("pista", item.Hint));
            view.Seed.SetActive(item.IsSeed);
            view.Panel.SetColors(item.IsSeed ? Palette.NeonViolet : Palette.NeonCyan);
            view.Edit.Interactable = !item.IsSeed;
            view.Delete.Interactable = !item.IsSeed;
            view.Edit.name = Loc.T(item.IsSeed ? "palabrasPredefinidasNoEditar" : "editar");
            view.Delete.name = Loc.T(item.IsSeed ? "palabrasPredefinidasNoBorrar" : "borrar");
        }

        private void OnEdit(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            OpenForm(_items[index]);
        }

        private void OnDelete(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            var item = _items[index];
            Dialogs.Confirm(Loc.T("borrarPalabraTitulo"), Loc.T("borrarPalabraMensaje", ("palabra", item.Word)), Loc.T("cancelar"), Loc.T("borrar"),
                ok =>
                {
                    if (!ok) return;
                    Run(() => _module.Words.Delete(item.Id), "palabraBorrada");
                }, Palette.NeonError);
        }

        /// <summary>Formulario de alta (<paramref name="editing"/> nulo) o edición. Ambos campos son obligatorios.</summary>
        private void OpenForm(ImpostorWord editing)
        {
            NeonTextField word = null;
            NeonTextField hint = null;
            TMP_Text wordError = null;
            TMP_Text hintError = null;
            Dialogs.Show(Loc.T(editing == null ? "nuevaPalabra" : "editarPalabra"), null, new[]
            {
                new DialogAction(Loc.T("cancelar"), (Action)null, ButtonStyle.Text, Palette.NeonCyan),
                new DialogAction(Loc.T(editing == null ? "agregar" : "guardar"), () =>
                {
                    var w = word.Value.Trim();
                    var h = hint.Value.Trim();
                    wordError.gameObject.SetActive(w.Length == 0);
                    hintError.gameObject.SetActive(h.Length == 0);
                    if (w.Length == 0 || h.Length == 0) return false;
                    if (editing == null) Run(() => _module.Words.Insert(w, h), "palabraAnadida");
                    else Run(() => _module.Words.Update(editing.Id, w, h), "palabraActualizada");
                    return true;
                }, ButtonStyle.Filled, Palette.NeonCyan, 0.6f)
            }, buildContent: content =>
            {
                word = NeonTextField.Create(content, Loc.T("campoPalabra"), Loc.T("campoPalabraHint"), editing?.Word, characterLimit: 60);
                wordError = Ui.Text(content, Loc.T("campoObligatorio"), TextRole.BodySmall, Palette.NeonError);
                wordError.gameObject.SetActive(false);
                hint = NeonTextField.Create(content, Loc.T("campoPista"), Loc.T("campoPistaHint"), editing?.Hint, characterLimit: 80);
                hintError = Ui.Text(content, Loc.T("campoObligatorio"), TextRole.BodySmall, Palette.NeonError);
                hintError.gameObject.SetActive(false);
                word.Focus();
            });
        }

        /// <summary>Ejecuta una operación del repositorio y avisa del resultado (o del error concreto).</summary>
        private void Run(Action operation, string successKey)
        {
            try
            {
                operation();
                Snackbar.Show(Loc.T(successKey));
            }
            catch (DuplicateWordException)
            {
                Snackbar.Show(Loc.T("yaExisteEsaPalabra"), Palette.NeonError);
            }
            catch (ReadOnlySeedWordException)
            {
                Snackbar.Show(Loc.T("palabrasPredefinidasSoloLectura"), Palette.NeonError);
            }
            catch (WordNotFoundException)
            {
                Snackbar.Show(Loc.T("esaPalabraYaNoExiste"), Palette.NeonError);
            }
            catch (ArgumentException)
            {
                Snackbar.Show(Loc.T("palabraYPistaObligatorias"), Palette.NeonError);
            }
            Reload();
        }
    }

    /// <summary>Referencias de una fila reciclable de la lista de palabras.</summary>
    public sealed class WordRowView : MonoBehaviour
    {
        public int Index;
        public NeonPanel Panel;
        public TMP_Text Word;
        public TMP_Text Hint;
        public GameObject Seed;
        public NeonButton Edit;
        public NeonButton Delete;
    }
}
