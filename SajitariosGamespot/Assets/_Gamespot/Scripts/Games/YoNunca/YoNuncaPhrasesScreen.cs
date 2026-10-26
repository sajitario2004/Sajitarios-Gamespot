using System;
using System.Collections.Generic;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.YoNunca.Domain;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.YoNunca
{
    /// <summary>
    /// Gestión de frases de Yo Nunca (V059q, como la de palabras del Impostor): búsqueda con espera de 280 ms, filtro
    /// (todas, suaves, picantes, mías), lista virtualizada con las propias arriba y las de serie con candado, y alta,
    /// edición y borrado de las propias. Al escribir no hace falta poner «Yo nunca»: se añade solo.
    /// </summary>
    public sealed class YoNuncaPhrasesScreen : AppScreen
    {
        public const float DebounceSeconds = 0.28f;
        private const float RowHeight = 92f;

        private static readonly (YoNuncaPhraseFilter filter, string key)[] Filters =
        {
            (YoNuncaPhraseFilter.Todas, "yoNuncaFiltroTodas"),
            (YoNuncaPhraseFilter.Suaves, "yoNuncaFiltroSuaves"),
            (YoNuncaPhraseFilter.Picantes, "yoNuncaFiltroPicantes"),
            (YoNuncaPhraseFilter.Propias, "yoNuncaFiltroPropias")
        };

        private readonly YoNuncaModule _module;
        private readonly Dictionary<YoNuncaPhraseFilter, NeonChip> _filterChips = new Dictionary<YoNuncaPhraseFilter, NeonChip>();
        private List<NeverStatementRow> _items = new List<NeverStatementRow>();
        private string _query = string.Empty;
        private YoNuncaPhraseFilter _filter = YoNuncaPhraseFilter.Todas;
        private float _debounce = -1f;
        private VirtualList _list;
        private NeonButton _clear;
        private GameObject _empty;
        private TMP_Text _emptyText;

        public YoNuncaPhrasesScreen(YoNuncaModule module) => _module = module;

        public IReadOnlyList<NeverStatementRow> Items => _items;
        public YoNuncaPhraseFilter Filter => _filter;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("yoNuncaFrasesTitulo"));
            GameUi.HomeButton(scaffold.AppBar, Router);

            var searchRow = Ui.Row(scaffold.Body, 8f, new RectOffset(16, 16, 8, 4), name: "Search");
            var search = NeonTextField.Create(searchRow, null, Loc.T("yoNuncaBuscarFrase"), _query);
            search.Sized(flexWidth: 1f);
            search.Changed += SetQuery;
            _clear = NeonButton.CreateIcon(searchRow, IconId.Close, () =>
            {
                search.Value = string.Empty;
                SetQuery(string.Empty);
            }, Palette.TextMuted);
            _clear.name = Loc.T("limpiarBusqueda");

            var filterRow = Ui.Row(scaffold.Body, 6f, new RectOffset(16, 16, 0, 4), TextAnchor.MiddleCenter, "Filters");
            _filterChips.Clear();
            foreach (var (filter, key) in Filters)
            {
                var f = filter;
                var chip = NeonChip.Create(filterRow, Loc.T(key), f == _filter, f == YoNuncaPhraseFilter.Picantes ? Palette.NeonMagenta : Palette.NeonCyan);
                chip.ToggleOnClick = false;
                chip.Clicked += () => SetFilter(f);
                chip.Sized(flexWidth: 1f);
                _filterChips[f] = chip;
            }

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

        public void SetFilter(YoNuncaPhraseFilter filter)
        {
            _filter = filter;
            foreach (var pair in _filterChips) pair.Value.Selected = pair.Key == filter;
            Reload();
        }

        public void Reload()
        {
            try
            {
                _items = _module.Phrases.Search(_query, _filter);
                _emptyText.text = _filter == YoNuncaPhraseFilter.Propias && _query.Length == 0
                    ? Loc.T("yoNuncaSinFrasesPropias")
                    : Loc.T("yoNuncaSinFrasesCoincidentes");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Yo Nunca] No se pudieron cargar las frases: {e.Message}");
                _items = new List<NeverStatementRow>();
                _emptyText.text = Loc.T("yoNuncaNoSePudieronCargarFrases");
            }
            _clear.gameObject.SetActive(_query.Length > 0);
            _empty.SetActive(_items.Count == 0);
            _list.SetCount(_items.Count, true);
        }

        /// <summary>Guarda una frase propia nueva o editada (también lo usan los tests). Devuelve el error, o nulo.</summary>
        public YoNuncaPhraseProblem? Save(NeverStatementRow editing, string text, Intensidad intensidad)
        {
            var frase = NeverStatementText.Normalize(text, Loc.T("yoNuncaPrefijo"));
            try
            {
                if (frase.Length == 0) throw new YoNuncaPhraseException(YoNuncaPhraseProblem.Vacia);
                if (editing == null) _module.Phrases.Insert(frase, intensidad);
                else _module.Phrases.Update(editing.Id, frase, intensidad);
            }
            catch (YoNuncaPhraseException e)
            {
                return e.Problem;
            }
            Snackbar.Show(Loc.T(editing == null ? "yoNuncaFraseAnadida" : "yoNuncaFraseActualizada"));
            Reload();
            return null;
        }

        /// <summary>Borra una frase propia sin confirmar (también lo usan los tests).</summary>
        public void Delete(long id)
        {
            try
            {
                _module.Phrases.Delete(id);
                Snackbar.Show(Loc.T("yoNuncaFraseBorrada"));
            }
            catch (YoNuncaPhraseException e)
            {
                Snackbar.Show(ErrorText(e.Problem), Palette.NeonError);
            }
            Reload();
        }

        public static string ErrorText(YoNuncaPhraseProblem problem)
        {
            switch (problem)
            {
                case YoNuncaPhraseProblem.Vacia: return Loc.T("yoNuncaFraseObligatoria");
                case YoNuncaPhraseProblem.Duplicada: return Loc.T("yoNuncaFraseRepetida");
                case YoNuncaPhraseProblem.Predefinida: return Loc.T("yoNuncaFrasesPredefinidasSoloLectura");
                default: return Loc.T("yoNuncaEsaFraseYaNoExiste");
            }
        }

        private RectTransform CreateRow(RectTransform parent)
        {
            var panel = NeonPanel.Create(parent, Palette.NeonCyan, Palette.Surface, 16f, 1.5f, 0f, padding: new RectOffset(16, 8, 8, 8),
                name: "PhraseRow");
            var view = panel.gameObject.AddComponent<YoNuncaPhraseRowView>();
            var row = Ui.Row(panel.Content, 6f, alignment: TextAnchor.MiddleLeft);
            row.Sized(flexHeight: 1f);
            var texts = Ui.Column(row, 2f, alignment: TextAnchor.MiddleLeft);
            // Ancho preferido 0: la columna se queda lo que sobra y no aplasta el candado ni los botones.
            texts.Sized(width: 0f, flexWidth: 1f);
            view.Phrase = Ui.Text(texts, string.Empty, TextRole.BodyMedium, maxLines: 3, bold: true);
            view.Phrase.enableAutoSizing = true;
            view.Phrase.fontSizeMax = view.Phrase.fontSize;
            view.Phrase.fontSizeMin = 10f;
            view.Phrase.Sized(height: 52f);
            view.Level = Ui.Text(texts, string.Empty, TextRole.LabelSmall, Palette.NeonCyan, maxLines: 1);
            view.Seed = Ui.Icon(row, IconId.Lock, 18f, Palette.NeonVioletText).gameObject;
            view.Seed.name = Loc.T("predefinida");
            view.Edit = NeonButton.CreateIcon(row, IconId.Edit, () => OnEdit(view.Index), Palette.NeonCyan, 22f);
            view.Delete = NeonButton.CreateIcon(row, IconId.Delete, () => OnDelete(view.Index), Palette.NeonError, 22f);
            view.Panel = panel;
            return (RectTransform)panel.transform;
        }

        private void BindRow(RectTransform rowRect, int index)
        {
            var view = rowRect.GetComponent<YoNuncaPhraseRowView>();
            var item = _items[index];
            view.Index = index;
            view.Phrase.text = item.Frase;
            var spicy = item.Level == Intensidad.Picante;
            var level = Loc.T(spicy ? "yoNuncaIntensidadPicante" : "yoNuncaIntensidadSuave");
            view.Level.text = item.IsSeed ? level : Loc.T("yoNuncaFrasePropiaNivel", ("nivel", level));
            view.Level.color = spicy ? Palette.NeonMagenta : Palette.NeonCyan;
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
            Dialogs.Confirm(Loc.T("yoNuncaBorrarFraseTitulo"), Loc.T("yoNuncaBorrarFraseMensaje", ("frase", item.Frase)), Loc.T("cancelar"),
                Loc.T("borrar"), ok =>
                {
                    if (ok) Delete(item.Id);
                }, Palette.NeonError);
        }

        /// <summary>Formulario de alta (<paramref name="editing"/> nulo) o edición: la frase y su nivel.</summary>
        private void OpenForm(NeverStatementRow editing)
        {
            NeonTextField field = null;
            TMP_Text error = null;
            NeonChip suave = null;
            NeonChip picante = null;
            var level = editing?.Level ?? (_filter == YoNuncaPhraseFilter.Picantes ? Intensidad.Picante : Intensidad.Suave);

            void ShowLevel()
            {
                suave.Selected = level == Intensidad.Suave;
                picante.Selected = level == Intensidad.Picante;
            }

            Dialogs.Show(Loc.T(editing == null ? "yoNuncaNuevaFrase" : "yoNuncaEditarFrase"), null, new[]
            {
                new DialogAction(Loc.T("cancelar"), (Action)null, ButtonStyle.Text, Palette.NeonCyan),
                new DialogAction(Loc.T(editing == null ? "agregar" : "guardar"), () =>
                {
                    var problem = Save(editing, field.Value, level);
                    if (problem == null) return true;
                    error.text = ErrorText(problem.Value);
                    error.gameObject.SetActive(true);
                    return false;
                }, ButtonStyle.Filled, Palette.NeonCyan, 0.6f)
            }, buildContent: content =>
            {
                field = NeonTextField.Create(content, Loc.T("yoNuncaCampoFrase"), Loc.T("yoNuncaCampoFraseHint"), editing?.Frase,
                    characterLimit: NeverStatementText.MaxLength);
                Ui.Text(content, Loc.T("yoNuncaCampoFraseAyuda"), TextRole.BodySmall, Palette.TextMuted);
                error = Ui.Text(content, string.Empty, TextRole.BodySmall, Palette.NeonError);
                error.gameObject.SetActive(false);
                Ui.Text(content, Loc.T("yoNuncaSetupIntensidades"), TextRole.LabelLarge, Palette.TextMuted);
                var row = Ui.Row(content, 10f, alignment: TextAnchor.MiddleCenter);
                suave = NeonChip.Create(row, Loc.T("yoNuncaIntensidadSuave"), level == Intensidad.Suave, Palette.NeonCyan);
                suave.Sized(flexWidth: 1f);
                suave.ToggleOnClick = false;
                suave.Clicked += () =>
                {
                    level = Intensidad.Suave;
                    ShowLevel();
                };
                picante = NeonChip.Create(row, Loc.T("yoNuncaIntensidadPicante"), level == Intensidad.Picante, Palette.NeonMagenta);
                picante.Sized(flexWidth: 1f);
                picante.ToggleOnClick = false;
                picante.Clicked += () =>
                {
                    level = Intensidad.Picante;
                    ShowLevel();
                };
                field.Focus();
            });
        }
    }

    /// <summary>Referencias de una fila reciclable de la lista de frases.</summary>
    public sealed class YoNuncaPhraseRowView : MonoBehaviour
    {
        public int Index;
        public NeonPanel Panel;
        public TMP_Text Phrase;
        public TMP_Text Level;
        public GameObject Seed;
        public NeonButton Edit;
        public NeonButton Delete;
    }
}
