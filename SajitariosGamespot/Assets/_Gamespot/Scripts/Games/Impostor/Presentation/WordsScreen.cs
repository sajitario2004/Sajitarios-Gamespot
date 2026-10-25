using System;
using System.Collections.Generic;
using Gamespot.Core;
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
    /// última tecla), lista virtualizada (2525 palabras de seed), alta, edición y borrado de las propias. Las predefinidas
    /// llevan borde violeta y candado y no se pueden editar ni borrar. V059l: cada palabra enseña su categoría, se
    /// puede filtrar por categoría, las propias van a la categoría que se elija (también a una nueva) y "Mis
    /// categorías" se abre desde la barra.
    /// </summary>
    public sealed class WordsScreen : AppScreen
    {
        public const float DebounceSeconds = 0.28f;
        private const float RowHeight = 92f;

        private readonly ImpostorModule _module;
        private List<ImpostorWord> _items = new List<ImpostorWord>();
        private List<CustomCategory> _custom = new List<CustomCategory>();
        private string _query = string.Empty;
        private string _category;
        private float _debounce = -1f;
        private VirtualList _list;
        private NeonButton _clear;
        private GameObject _filterRow;
        private TMP_Text _filterText;
        private GameObject _empty;
        private TMP_Text _emptyText;

        public WordsScreen(ImpostorModule module) => _module = module;

        public IReadOnlyList<ImpostorWord> Items => _items;
        public VirtualList List => _list;

        /// <summary>Categoría del filtro (nula = todas).</summary>
        public string CategoryFilter => _category;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("gestionarPalabrasTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            scaffold.AppBar.AddAction(IconId.Category, OpenCategories).name = Loc.T("categoriasPropiasTitulo");

            var searchRow = Ui.Row(scaffold.Body, 8f, new RectOffset(16, 16, 8, 4), name: "Search");
            var search = NeonTextField.Create(searchRow, null, Loc.T("buscarPalabra"), _query);
            search.Sized(flexWidth: 1f);
            search.Changed += SetQuery;
            _clear = NeonButton.CreateIcon(searchRow, IconId.Close, () =>
            {
                search.Value = string.Empty;
                SetQuery(string.Empty);
            }, Palette.TextMuted);
            _clear.name = Loc.T("limpiarBusqueda");
            NeonButton.CreateIcon(searchRow, IconId.FilterList, OpenFilter, Palette.NeonCyan).name = Loc.T("filtrarPorCategoria");

            var filterRow = Ui.Row(scaffold.Body, 6f, new RectOffset(20, 16, 0, 4), TextAnchor.MiddleLeft, "Filter");
            _filterRow = filterRow.gameObject;
            _filterText = Ui.Text(filterRow, string.Empty, TextRole.LabelMedium, Palette.NeonCyan, maxLines: 1);
            _filterText.Sized(flexWidth: 1f);
            NeonButton.CreateIcon(filterRow, IconId.Close, () => SetCategoryFilter(null), Palette.TextMuted, 18f).name = Loc.T("quitarFiltro");

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

        /// <summary>Filtra por categoría (nula = todas) y vuelve a consultar al momento.</summary>
        public void SetCategoryFilter(string category)
        {
            _category = string.IsNullOrEmpty(category) ? null : category;
            Reload();
        }

        /// <summary>Vuelve a consultar la base de datos.</summary>
        public void Reload()
        {
            _custom = _module.CustomCategoriesOrEmpty();
            // Si la categoría del filtro se borró, el filtro se quita.
            if (WordCategories.IsCustom(_category) && !_custom.Exists(c => c.CategoryId == _category)) _category = null;
            try
            {
                _items = _query.Length == 0 && _category == null ? _module.Words.GetAll() : _module.Words.Search(_query, _category);
                _emptyText.text = _query.Length == 0 && _category == null ? Loc.T("sinPalabrasAun") : Loc.T("sinCoincidencias");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Impostor] No se pudieron cargar las palabras: {e.Message}");
                _items = new List<ImpostorWord>();
                _emptyText.text = Loc.T("noSePudieronCargarPalabras");
            }
            _clear.gameObject.SetActive(_query.Length > 0);
            _filterRow.SetActive(_category != null);
            if (_category != null) _filterText.text = Loc.T("categoriaConValor", ("categoria", CategoryOption.LabelOf(_category, _custom)));
            _empty.SetActive(_items.Count == 0);
            _list.SetCount(_items.Count, true);
        }

        private void OpenCategories()
        {
            Router.Push(new CustomCategoriesScreen(_module.Categories, () => _module.Words.CountByCategory(),
                from => _module.Words.MoveCategory(from, WordCategories.Propias), "categoriaContadorPalabras", Reload));
        }

        private void OpenFilter()
        {
            CategorySelectDialog.Show(Loc.T("filtrarPorCategoria"), CategoryOption.For(CategoryOption.AllIds(_custom), _custom), _category,
                SetCategoryFilter, Loc.T("todasLasCategorias"));
        }

        private RectTransform CreateRow(RectTransform parent)
        {
            var panel = NeonPanel.Create(parent, Palette.NeonCyan, Palette.Surface, 16f, 1.5f, 0f, padding: new RectOffset(16, 8, 8, 8),
                name: "WordRow");
            var view = panel.gameObject.AddComponent<WordRowView>();
            var row = Ui.Row(panel.Content, 6f, alignment: TextAnchor.MiddleLeft);
            row.Sized(flexHeight: 1f);
            var texts = Ui.Column(row, 2f, alignment: TextAnchor.MiddleLeft);
            // Ancho preferido 0: la columna se queda lo que sobra y no aplasta el chip ni los botones de la derecha.
            texts.Sized(width: 0f, flexWidth: 1f);
            // Hasta dos líneas que encogen en una caja fija: con los famosos y los sitios hay nombres largos
            // («José Luis Rodríguez Zapatero») y la pista tiene que seguir viéndose debajo.
            view.Word = Ui.Text(texts, string.Empty, TextRole.TitleMedium, alignment: TextAlignmentOptions.BottomLeft, maxLines: 2, bold: true);
            view.Word.enableAutoSizing = true;
            view.Word.fontSizeMax = view.Word.fontSize;
            view.Word.fontSizeMin = 11f;
            view.Word.Sized(height: 36f);
            view.Hint = Ui.Text(texts, string.Empty, TextRole.BodySmall, Palette.TextMuted, maxLines: 1);
            view.Category = Ui.Text(texts, string.Empty, TextRole.LabelSmall, Palette.NeonCyan, maxLines: 1);
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
            view.Category.text = CategoryOption.LabelOf(item.Category, _custom);
            view.Category.color = item.IsSeed ? Palette.NeonVioletText : Palette.NeonCyan;
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

        /// <summary>
        /// Una palabra nueva va a la categoría del filtro si se puede elegir, y si no a "Mis palabras".
        /// </summary>
        private string DefaultCategoryForNew()
        {
            var options = CategoryOption.Assignable(_custom);
            return _category != null && options.Exists(o => o.Id == _category) ? _category : WordCategories.Propias;
        }

        /// <summary>
        /// Formulario de alta (<paramref name="editing"/> nulo) o edición: palabra y pista obligatorias y la
        /// categoría, que se elige en otro diálogo (con "Nueva categoría" para crearla sin salir).
        /// </summary>
        private void OpenForm(ImpostorWord editing)
        {
            NeonTextField word = null;
            NeonTextField hint = null;
            TMP_Text wordError = null;
            TMP_Text hintError = null;
            NeonButton categoryButton = null;
            var category = editing?.Category ?? DefaultCategoryForNew();

            void ShowCategory() => categoryButton.SetLabel(CategoryOption.LabelOf(category, _custom));

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
                    if (editing == null) Run(() => _module.Words.Insert(w, h, category), "palabraAnadida");
                    else Run(() => _module.Words.Update(editing.Id, w, h, category), "palabraActualizada");
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
                Ui.Text(content, Loc.T("campoCategoriaPalabra"), TextRole.LabelLarge, Palette.TextMuted);
                // Con el texto ya puesto: un botón creado sin texto no tiene etiqueta que cambiar después.
                categoryButton = Ui.Button(content, CategoryOption.LabelOf(category, _custom), () => CategorySelectDialog.Show(Loc.T("elegirCategoria"),
                    CategoryOption.Assignable(_custom), category, picked =>
                    {
                        category = picked ?? WordCategories.Propias;
                        ShowCategory();
                    }, onCreate: select => CategoryNameDialog.ShowCreate(_module.Categories, created =>
                    {
                        _custom = _module.CustomCategoriesOrEmpty();
                        select(new CategoryOption(created.CategoryId, created.Name));
                    })), ButtonStyle.Outlined, IconId.Category);
                categoryButton.name = Loc.T("elegirCategoria");
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
        public TMP_Text Category;
        public GameObject Seed;
        public NeonButton Edit;
        public NeonButton Delete;
    }
}
