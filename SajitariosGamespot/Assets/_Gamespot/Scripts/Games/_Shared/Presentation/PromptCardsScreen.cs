using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Shared
{
    /// <summary>
    /// Gestión de cartas común (V070) de Verdad o Reto, ¿Quién es más probable…?, ¿Qué prefieres?, Mímica y ¿Quién soy?
    /// (como la de palabras del Impostor): búsqueda con espera de 280 ms, filtro (todas, mías, nivel, tipo, categoría),
    /// lista virtualizada con las propias arriba y las de serie con candado, y alta, edición y borrado de las propias
    /// en un diálogo con los campos que tenga el juego (<see cref="PromptCardsConfig"/>).
    /// </summary>
    public sealed class PromptCardsScreen : AppScreen
    {
        public const float DebounceSeconds = 0.28f;
        private const float RowHeight = 104f;
        private const string FilterOwn = "own";
        private const string LevelPrefix = "lvl:";
        private const string KindPrefix = "kind:";
        private const string CategoryPrefix = "cat:";

        private readonly PromptCardRepository _cards;
        private readonly PromptCardsConfig _config;
        private List<PromptCardRow> _items = new List<PromptCardRow>();
        private List<CustomCategory> _custom = new List<CustomCategory>();
        private string _query = string.Empty;
        private string _filter;
        private float _debounce = -1f;
        private VirtualList _list;
        private NeonButton _clear;
        private GameObject _filterRow;
        private TMP_Text _filterText;
        private GameObject _empty;
        private TMP_Text _emptyText;

        public PromptCardsScreen(PromptCardRepository cards, PromptCardsConfig config)
        {
            _cards = cards;
            _config = config ?? new PromptCardsConfig();
        }

        public IReadOnlyList<PromptCardRow> Items => _items;

        /// <summary>Filtro actual (nulo = todas; <c>own</c>, <c>lvl:…</c>, <c>kind:…</c>, <c>cat:…</c>).</summary>
        public string FilterId => _filter;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T(_config.TitleKey));
            GameUi.HomeButton(scaffold.AppBar, Router);
            if (_config.CustomCategories != null)
            {
                scaffold.AppBar.AddAction(IconId.Category, OpenCategories).name = Loc.T("categoriasPropiasTitulo");
            }

            var searchRow = Ui.Row(scaffold.Body, 8f, new RectOffset(16, 16, 8, 4), name: "Search");
            var search = NeonTextField.Create(searchRow, null, Loc.T(_config.SearchHintKey), _query);
            search.Sized(flexWidth: 1f);
            search.Changed += SetQuery;
            _clear = NeonButton.CreateIcon(searchRow, IconId.Close, () =>
            {
                search.Value = string.Empty;
                SetQuery(string.Empty);
            }, Palette.TextMuted);
            _clear.name = Loc.T("limpiarBusqueda");
            NeonButton.CreateIcon(searchRow, IconId.FilterList, OpenFilter, Palette.NeonCyan).name = Loc.T("filtrar");

            var filterRow = Ui.Row(scaffold.Body, 6f, new RectOffset(20, 16, 0, 4), TextAnchor.MiddleLeft, "Filter");
            _filterRow = filterRow.gameObject;
            _filterText = Ui.Text(filterRow, string.Empty, TextRole.LabelMedium, Palette.NeonCyan, maxLines: 1);
            _filterText.Sized(width: 0f, flexWidth: 1f);
            NeonButton.CreateIcon(filterRow, IconId.Close, () => SetFilter(null), Palette.TextMuted, 18f).name = Loc.T("quitarFiltro");

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

        public void SetQuery(string value)
        {
            _query = value?.Trim() ?? string.Empty;
            _clear.gameObject.SetActive(_query.Length > 0);
            _debounce = DebounceSeconds;
        }

        public void SetFilter(string filterId)
        {
            _filter = string.IsNullOrEmpty(filterId) ? null : filterId;
            Reload();
        }

        public void Reload()
        {
            _custom = CustomOrEmpty();
            if (_filter != null && _filter.StartsWith(CategoryPrefix, StringComparison.Ordinal))
            {
                var category = _filter.Substring(CategoryPrefix.Length);
                if (WordCategories.IsCustom(category) && !_custom.Exists(c => c.CategoryId == category)) _filter = null;
            }
            try
            {
                _items = _cards.Search(_query, ToFilter(_filter));
                _emptyText.text = _filter == FilterOwn && _query.Length == 0 ? Loc.T("sinCartasPropias") : Loc.T("sinCartasCoincidentes");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Cartas] No se pudieron cargar: {e.Message}");
                _items = new List<PromptCardRow>();
                _emptyText.text = Loc.T("noSePudieronCargarCartas");
            }
            _clear.gameObject.SetActive(_query.Length > 0);
            _filterRow.SetActive(_filter != null);
            if (_filter != null) _filterText.text = Loc.T("filtroConValor", ("filtro", FilterLabel(_filter)));
            _empty.SetActive(_items.Count == 0);
            _list.SetCount(_items.Count, true);
        }

        /// <summary>Guarda una carta propia nueva o editada (también lo usan los tests). Devuelve el error, o nulo.</summary>
        public PromptCardProblem? Save(PromptCardRow editing, string text, string textB, PromptLevel level, string kind, string category)
        {
            var clean = Clean(text);
            var cleanB = _config.HasTextB ? Clean(textB) : null;
            try
            {
                if (clean.Length == 0) throw new PromptCardException(PromptCardProblem.Vacia);
                var card = new PromptCard(0, clean, _config.HasLevels ? level : PromptLevel.Suave, kind ?? string.Empty,
                    _config.HasCategories ? category ?? WordCategories.Propias : string.Empty, cleanB);
                if (editing == null) _cards.Insert(card, _config.HasTextB);
                else _cards.Update(editing.Id, card, _config.HasTextB);
            }
            catch (PromptCardException e)
            {
                return e.Problem;
            }
            Snackbar.Show(Loc.T(editing == null ? "cartaAnadida" : "cartaActualizada"));
            Reload();
            return null;
        }

        /// <summary>Borra una carta propia sin confirmar (también lo usan los tests).</summary>
        public void Delete(long id)
        {
            try
            {
                _cards.Delete(id);
                Snackbar.Show(Loc.T("cartaBorrada"));
            }
            catch (PromptCardException e)
            {
                Snackbar.Show(ErrorText(e.Problem), Palette.NeonError);
            }
            Reload();
        }

        public static string ErrorText(PromptCardProblem problem)
        {
            switch (problem)
            {
                case PromptCardProblem.Vacia: return Loc.T("cartaObligatoria");
                case PromptCardProblem.FaltaSegunda: return Loc.T("cartaFaltaSegunda");
                case PromptCardProblem.Duplicada: return Loc.T("cartaRepetida");
                case PromptCardProblem.Predefinida: return Loc.T("cartasPredefinidasSoloLectura");
                default: return Loc.T("esaCartaYaNoExiste");
            }
        }

        private string Clean(string text)
        {
            if (_config.Normalize != null) return _config.Normalize(text ?? string.Empty) ?? string.Empty;
            var clean = NameNormalizer.Normalize(text);
            return clean.Length > _config.MaxLength ? clean.Substring(0, _config.MaxLength).TrimEnd() : clean;
        }

        private List<CustomCategory> CustomOrEmpty()
        {
            if (_config.CustomCategories == null) return new List<CustomCategory>();
            try
            {
                return _config.CustomCategories.GetAll();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Cartas] No se pudieron cargar las categorías: {e.Message}");
                return new List<CustomCategory>();
            }
        }

        private static PromptCardFilter ToFilter(string id)
        {
            if (id == null) return null;
            if (id == FilterOwn) return new PromptCardFilter { OwnOnly = true };
            if (id.StartsWith(LevelPrefix, StringComparison.Ordinal))
            {
                return new PromptCardFilter { Level = PromptLevels.FromStorageName(id.Substring(LevelPrefix.Length)) };
            }
            if (id.StartsWith(KindPrefix, StringComparison.Ordinal)) return new PromptCardFilter { Kind = id.Substring(KindPrefix.Length) };
            if (id.StartsWith(CategoryPrefix, StringComparison.Ordinal)) return new PromptCardFilter { Category = id.Substring(CategoryPrefix.Length) };
            return null;
        }

        private string FilterLabel(string id)
        {
            if (id == FilterOwn) return Loc.T("filtroMias");
            if (id.StartsWith(LevelPrefix, StringComparison.Ordinal)) return LevelLabel(PromptLevels.FromStorageName(id.Substring(LevelPrefix.Length)));
            if (id.StartsWith(KindPrefix, StringComparison.Ordinal)) return KindLabel(id.Substring(KindPrefix.Length));
            if (id.StartsWith(CategoryPrefix, StringComparison.Ordinal)) return CategoryOption.LabelOf(id.Substring(CategoryPrefix.Length), _custom);
            return id;
        }

        private static string LevelLabel(PromptLevel level) => Loc.T(level == PromptLevel.Picante ? "nivelPicante" : "nivelSuave");

        private string KindLabel(string kind)
        {
            foreach (var (id, key) in _config.Kinds) if (id == kind) return Loc.T(key);
            return kind;
        }

        private IReadOnlyList<string> CategoryIds() =>
            _config.SeedCategories.Concat(new[] { WordCategories.Propias }).Concat(_custom.Select(c => c.CategoryId)).ToList();

        private void OpenFilter()
        {
            var options = new List<CategoryOption> { new CategoryOption(FilterOwn, Loc.T("filtroMias")) };
            if (_config.HasLevels)
            {
                foreach (var level in PromptLevels.All) options.Add(new CategoryOption(LevelPrefix + level.StorageName(), LevelLabel(level)));
            }
            foreach (var (id, key) in _config.Kinds) options.Add(new CategoryOption(KindPrefix + id, Loc.T(key)));
            if (_config.HasCategories)
            {
                options.AddRange(CategoryIds().Select(c => new CategoryOption(CategoryPrefix + c, CategoryOption.LabelOf(c, _custom))));
            }
            CategorySelectDialog.Show(Loc.T("filtrar"), options, _filter, SetFilter, Loc.T("todasLasCategorias"));
        }

        private void OpenCategories()
        {
            Router.Push(new CustomCategoriesScreen(_config.CustomCategories, () => _cards.CountByCategory(),
                from => _cards.MoveCategory(from, WordCategories.Propias), "categoriaContadorTarjetas", Reload));
        }

        private RectTransform CreateRow(RectTransform parent)
        {
            var panel = NeonPanel.Create(parent, Palette.NeonCyan, Palette.Surface, 16f, 1.5f, 0f, padding: new RectOffset(16, 8, 8, 8),
                name: "CardRow");
            var view = panel.gameObject.AddComponent<PromptCardRowView>();
            var row = Ui.Row(panel.Content, 6f, alignment: TextAnchor.MiddleLeft);
            row.Sized(flexHeight: 1f);
            var texts = Ui.Column(row, 2f, alignment: TextAnchor.MiddleLeft);
            // Ancho preferido 0: la columna se queda lo que sobra y no aplasta el candado ni los botones.
            texts.Sized(width: 0f, flexWidth: 1f);
            view.Text = Ui.Text(texts, string.Empty, TextRole.BodyMedium, maxLines: 3, bold: true);
            view.Text.enableAutoSizing = true;
            view.Text.fontSizeMax = view.Text.fontSize;
            view.Text.fontSizeMin = 10f;
            view.Text.Sized(height: _config.HasTextB ? 36f : 56f);
            view.TextB = Ui.Text(texts, string.Empty, TextRole.BodySmall, Palette.TextMuted, maxLines: 2);
            view.TextB.enableAutoSizing = true;
            view.TextB.fontSizeMax = view.TextB.fontSize;
            view.TextB.fontSizeMin = 9f;
            view.TextB.Sized(height: 28f);
            view.TextB.gameObject.SetActive(_config.HasTextB);
            view.Info = Ui.Text(texts, string.Empty, TextRole.LabelSmall, Palette.NeonCyan, maxLines: 1);
            view.Seed = Ui.Icon(row, IconId.Lock, 18f, Palette.NeonVioletText).gameObject;
            view.Seed.name = Loc.T("predefinida");
            view.Edit = NeonButton.CreateIcon(row, IconId.Edit, () => OnEdit(view.Index), Palette.NeonCyan, 22f);
            view.Delete = NeonButton.CreateIcon(row, IconId.Delete, () => OnDelete(view.Index), Palette.NeonError, 22f);
            view.Panel = panel;
            return (RectTransform)panel.transform;
        }

        private void BindRow(RectTransform rowRect, int index)
        {
            var view = rowRect.GetComponent<PromptCardRowView>();
            var item = _items[index];
            view.Index = index;
            view.Text.text = item.Texto;
            if (_config.HasTextB) view.TextB.text = Loc.T("cartaOpcionB", ("texto", item.TextoB ?? string.Empty));
            var parts = new List<string>();
            if (!item.IsSeed) parts.Add(Loc.T("filtroMias"));
            if (_config.HasKinds) parts.Add(KindLabel(item.Tipo));
            if (_config.HasLevels) parts.Add(LevelLabel(item.Level));
            if (_config.HasCategories) parts.Add(CategoryOption.LabelOf(item.Categoria, _custom));
            view.Info.text = string.Join(" · ", parts);
            var spicy = item.Level == PromptLevel.Picante || WordCategories.IsAdult(item.Categoria);
            view.Info.color = spicy ? Palette.NeonMagenta : item.IsSeed ? Palette.NeonVioletText : Palette.NeonCyan;
            view.Seed.SetActive(item.IsSeed);
            view.Panel.SetColors(item.IsSeed ? Palette.NeonViolet : spicy ? Palette.NeonMagenta : Palette.NeonCyan);
            view.Edit.gameObject.SetActive(!item.IsSeed);
            view.Delete.gameObject.SetActive(!item.IsSeed);
            view.Edit.name = Loc.T("editar");
            view.Delete.name = Loc.T("borrar");
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
            Dialogs.Confirm(Loc.T("borrarCartaTitulo"), Loc.T("borrarCartaMensaje", ("carta", item.Texto)), Loc.T("cancelar"), Loc.T("borrar"),
                ok =>
                {
                    if (ok) Delete(item.Id);
                }, Palette.NeonError);
        }

        /// <summary>Formulario de alta (<paramref name="editing"/> nulo) o edición con los campos que tenga el juego.</summary>
        private void OpenForm(PromptCardRow editing)
        {
            NeonTextField field = null;
            NeonTextField fieldB = null;
            TMP_Text error = null;
            NeonButton categoryButton = null;
            var levelChips = new Dictionary<PromptLevel, NeonChip>();
            var kindChips = new Dictionary<string, NeonChip>();
            var level = editing?.Level ?? (_filter == LevelPrefix + PromptLevel.Picante.StorageName() ? PromptLevel.Picante : PromptLevel.Suave);
            var kind = editing?.Tipo ?? (_filter != null && _filter.StartsWith(KindPrefix, StringComparison.Ordinal)
                ? _filter.Substring(KindPrefix.Length)
                : _config.HasKinds ? _config.Kinds[0].id : string.Empty);
            var category = editing?.Categoria ?? (_filter != null && _filter.StartsWith(CategoryPrefix, StringComparison.Ordinal)
                ? _filter.Substring(CategoryPrefix.Length)
                : WordCategories.Propias);

            void Refresh()
            {
                foreach (var pair in levelChips) pair.Value.Selected = pair.Key == level;
                foreach (var pair in kindChips) pair.Value.Selected = pair.Key == kind;
                categoryButton?.SetLabel(CategoryOption.LabelOf(category, _custom));
            }

            Dialogs.Show(Loc.T(editing == null ? "nuevaCarta" : "editarCarta"), null, new[]
            {
                new DialogAction(Loc.T("cancelar"), (Action)null, ButtonStyle.Text, Palette.NeonCyan),
                new DialogAction(Loc.T(editing == null ? "agregar" : "guardar"), () =>
                {
                    var problem = Save(editing, field.Value, fieldB?.Value, level, kind, category);
                    if (problem == null) return true;
                    error.text = ErrorText(problem.Value);
                    error.gameObject.SetActive(true);
                    return false;
                }, ButtonStyle.Filled, Palette.NeonCyan, 0.6f)
            }, buildContent: content =>
            {
                field = NeonTextField.Create(content, Loc.T(_config.TextLabelKey), _config.TextHintKey != null ? Loc.T(_config.TextHintKey) : null,
                    editing?.Texto, characterLimit: _config.MaxLength);
                if (_config.HasTextB)
                {
                    fieldB = NeonTextField.Create(content, Loc.T(_config.TextBLabelKey), _config.TextBHintKey != null ? Loc.T(_config.TextBHintKey) : null,
                        editing?.TextoB, characterLimit: _config.MaxLength);
                }
                error = Ui.Text(content, string.Empty, TextRole.BodySmall, Palette.NeonError);
                error.gameObject.SetActive(false);
                if (_config.HasKinds)
                {
                    var kindRow = Ui.Row(content, 10f, alignment: TextAnchor.MiddleCenter);
                    foreach (var (id, key) in _config.Kinds)
                    {
                        var k = id;
                        var chip = NeonChip.Create(kindRow, Loc.T(key), k == kind, Palette.NeonViolet);
                        chip.Sized(flexWidth: 1f);
                        chip.ToggleOnClick = false;
                        chip.Clicked += () =>
                        {
                            kind = k;
                            Refresh();
                        };
                        kindChips[k] = chip;
                    }
                }
                if (_config.HasLevels)
                {
                    var levelRow = Ui.Row(content, 10f, alignment: TextAnchor.MiddleCenter);
                    foreach (var l in PromptLevels.All)
                    {
                        var lv = l;
                        var chip = NeonChip.Create(levelRow, LevelLabel(lv), lv == level, lv == PromptLevel.Picante ? Palette.NeonMagenta : Palette.NeonCyan);
                        chip.Sized(flexWidth: 1f);
                        chip.ToggleOnClick = false;
                        chip.Clicked += () =>
                        {
                            level = lv;
                            Refresh();
                        };
                        levelChips[lv] = chip;
                    }
                }
                if (_config.HasCategories)
                {
                    categoryButton = Ui.Button(content, CategoryOption.LabelOf(category, _custom), () => CategorySelectDialog.Show(Loc.T("elegirCategoria"),
                        CategoryOption.For(CategoryIds(), _custom), category, picked =>
                        {
                            category = picked ?? WordCategories.Propias;
                            Refresh();
                        }, onCreate: _config.CustomCategories == null
                            ? (Action<Action<CategoryOption>>)null
                            : select => CategoryNameDialog.ShowCreate(_config.CustomCategories, created =>
                            {
                                _custom = CustomOrEmpty();
                                select(new CategoryOption(created.CategoryId, created.Name));
                            })), ButtonStyle.Outlined, IconId.Category);
                    categoryButton.name = Loc.T("elegirCategoria");
                }
                field.Focus();
            });
        }
    }

    /// <summary>Referencias de una fila reciclable de la lista de cartas.</summary>
    public sealed class PromptCardRowView : MonoBehaviour
    {
        public int Index;
        public NeonPanel Panel;
        public TMP_Text Text;
        public TMP_Text TextB;
        public TMP_Text Info;
        public GameObject Seed;
        public NeonButton Edit;
        public NeonButton Delete;
    }
}
