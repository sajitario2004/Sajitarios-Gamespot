using System;
using System.Collections.Generic;
using Gamespot.Core;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.Tabu.Data;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Tabu
{
    /// <summary>
    /// Gestión de tarjetas del Tabú (V059l, como la de palabras del Impostor): búsqueda con espera de 280 ms, filtro
    /// por categoría, lista virtualizada (1161 de seed) y alta, edición y borrado de las propias, que se editan en
    /// <see cref="TabuCardEditScreen"/>. Las predefinidas llevan candado. "Mis categorías" se abre desde la barra.
    /// </summary>
    public sealed class TabuWordsScreen : AppScreen
    {
        public const float DebounceSeconds = 0.28f;
        private const float RowHeight = 104f;

        private readonly TabuModule _module;
        private List<TabuCard> _items = new List<TabuCard>();
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

        public TabuWordsScreen(TabuModule module) => _module = module;

        public IReadOnlyList<TabuCard> Items => _items;
        public string CategoryFilter => _category;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("gestionarTarjetasTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);
            scaffold.AppBar.AddAction(IconId.Category, OpenCategories).name = Loc.T("categoriasPropiasTitulo");

            var searchRow = Ui.Row(scaffold.Body, 8f, new RectOffset(16, 16, 8, 4), name: "Search");
            var search = NeonTextField.Create(searchRow, null, Loc.T("buscarTarjeta"), _query);
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
            Ui.Button(bottom, Loc.T("agregar"), () => OpenEditor(null), ButtonStyle.Filled, IconId.Add, glow: 0.8f);

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

        public void SetQuery(string value)
        {
            _query = value?.Trim() ?? string.Empty;
            _clear.gameObject.SetActive(_query.Length > 0);
            _debounce = DebounceSeconds;
        }

        public void SetCategoryFilter(string category)
        {
            _category = string.IsNullOrEmpty(category) ? null : category;
            Reload();
        }

        public void Reload()
        {
            _custom = _module.CustomCategoriesOrEmpty();
            if (WordCategories.IsCustom(_category) && !_custom.Exists(c => c.CategoryId == _category)) _category = null;
            try
            {
                _items = _module.Words.Search(_query, _category);
                _emptyText.text = _query.Length == 0 && _category == null ? Loc.T("sinTarjetasAun") : Loc.T("sinTarjetasCoincidentes");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Tabú] No se pudieron cargar las tarjetas: {e.Message}");
                _items = new List<TabuCard>();
                _emptyText.text = Loc.T("noSePudieronCargarTarjetas");
            }
            _clear.gameObject.SetActive(_query.Length > 0);
            _filterRow.SetActive(_category != null);
            if (_category != null) _filterText.text = Loc.T("categoriaConValor", ("categoria", CategoryOption.LabelOf(_category, _custom)));
            _empty.SetActive(_items.Count == 0);
            _list.SetCount(_items.Count, true);
        }

        /// <summary>Abre el editor de una tarjeta nueva (nula) o propia. Devuelve la pantalla (para los tests).</summary>
        public TabuCardEditScreen OpenEditor(TabuCard editing)
        {
            var category = editing?.Categoria ?? (_category != null && CategoryOption.Assignable(_custom).Exists(o => o.Id == _category)
                ? _category
                : WordCategories.Propias);
            var editor = new TabuCardEditScreen(_module, editing, category, Reload);
            Router.Push(editor);
            return editor;
        }

        private void OpenCategories()
        {
            Router.Push(new CustomCategoriesScreen(_module.Categories, () => _module.Words.CountByCategoria(),
                from => _module.Words.MoveCategory(from, WordCategories.Propias), "categoriaContadorTarjetas", Reload));
        }

        private void OpenFilter()
        {
            CategorySelectDialog.Show(Loc.T("filtrarPorCategoria"), CategoryOption.For(CategoryOption.AllIds(_custom), _custom), _category,
                SetCategoryFilter, Loc.T("todasLasCategorias"));
        }

        private RectTransform CreateRow(RectTransform parent)
        {
            var panel = NeonPanel.Create(parent, Palette.NeonCyan, Palette.Surface, 16f, 1.5f, 0f, padding: new RectOffset(16, 8, 8, 8),
                name: "CardRow");
            var view = panel.gameObject.AddComponent<TabuCardRowView>();
            var row = Ui.Row(panel.Content, 6f, alignment: TextAnchor.MiddleLeft);
            row.Sized(flexHeight: 1f);
            var texts = Ui.Column(row, 2f, alignment: TextAnchor.MiddleLeft);
            // Ancho preferido 0: la columna se queda lo que sobra y no aplasta el chip ni los botones de la derecha.
            texts.Sized(width: 0f, flexWidth: 1f);
            view.Word = Ui.Text(texts, string.Empty, TextRole.TitleMedium, maxLines: 1, bold: true);
            // Las prohibidas en hasta tres líneas que encogen: cinco palabras largas no caben en una.
            view.Forbidden = Ui.Text(texts, string.Empty, TextRole.BodySmall, Palette.TextMuted, maxLines: 3);
            view.Forbidden.enableAutoSizing = true;
            view.Forbidden.fontSizeMax = view.Forbidden.fontSize;
            view.Forbidden.fontSizeMin = 9f;
            view.Forbidden.Sized(height: 44f);
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
            var view = rowRect.GetComponent<TabuCardRowView>();
            var item = _items[index];
            view.Index = index;
            view.Word.text = item.Palabra;
            view.Forbidden.text = Loc.T("prohibidasConValor", ("lista", string.Join(", ", item.Prohibidas)));
            view.Category.text = CategoryOption.LabelOf(item.Categoria, _custom);
            view.Category.color = item.IsSeed ? Palette.NeonVioletText : Palette.NeonCyan;
            view.Seed.SetActive(item.IsSeed);
            view.Panel.SetColors(item.IsSeed ? Palette.NeonViolet : Palette.NeonCyan);
            view.Edit.Interactable = !item.IsSeed;
            view.Delete.Interactable = !item.IsSeed;
            view.Edit.name = Loc.T(item.IsSeed ? "tarjetasPredefinidasNoEditar" : "editar");
            view.Delete.name = Loc.T(item.IsSeed ? "tarjetasPredefinidasNoBorrar" : "borrar");
        }

        private void OnEdit(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            OpenEditor(_items[index]);
        }

        private void OnDelete(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            var item = _items[index];
            Dialogs.Confirm(Loc.T("borrarTarjetaTitulo"), Loc.T("borrarTarjetaMensaje", ("palabra", item.Palabra)), Loc.T("cancelar"), Loc.T("borrar"),
                ok =>
                {
                    if (ok) Delete(item.Id);
                }, Palette.NeonError);
        }

        /// <summary>Borra una tarjeta propia sin confirmar (también lo usan los tests).</summary>
        public void Delete(long id)
        {
            try
            {
                _module.Words.Delete(id);
                Snackbar.Show(Loc.T("tarjetaBorrada"));
            }
            catch (TabuCardException e)
            {
                Snackbar.Show(TabuCardEditScreen.ErrorText(e), Palette.NeonError);
            }
            Reload();
        }
    }

    /// <summary>Referencias de una fila reciclable de la lista de tarjetas.</summary>
    public sealed class TabuCardRowView : MonoBehaviour
    {
        public int Index;
        public NeonPanel Panel;
        public TMP_Text Word;
        public TMP_Text Forbidden;
        public TMP_Text Category;
        public GameObject Seed;
        public NeonButton Edit;
        public NeonButton Delete;
    }
}
