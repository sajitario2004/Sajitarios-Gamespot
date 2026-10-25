using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Navigation;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using Gamespot.Games.Tabu.Data;
using Gamespot.Games.Tabu.Domain;
using TMPro;
using UnityEngine;

namespace Gamespot.Games.Tabu
{
    /// <summary>
    /// Alta o edición de una tarjeta propia del Tabú (V059l). Es una pantalla con scroll y no un diálogo: son seis
    /// campos y en el móvil el teclado taparía la mitad. Palabra, cinco prohibidas (la quinta, opcional) y la
    /// categoría, que se elige en un diálogo con "Nueva categoría". Al guardar vuelve a la lista.
    /// </summary>
    public sealed class TabuCardEditScreen : AppScreen
    {
        private readonly TabuModule _module;
        private readonly TabuCard _editing;
        private readonly Action _saved;
        private readonly List<NeonTextField> _forbidden = new List<NeonTextField>();
        private List<CustomCategory> _custom = new List<CustomCategory>();
        private string _category;
        private NeonTextField _word;
        private NeonButton _categoryButton;
        private TMP_Text _error;

        public TabuCardEditScreen(TabuModule module, TabuCard editing, string category, Action saved)
        {
            _module = module;
            _editing = editing;
            _category = string.IsNullOrEmpty(category) ? WordCategories.Propias : category;
            _saved = saved;
        }

        public NeonTextField Word => _word;
        public IReadOnlyList<NeonTextField> Forbidden => _forbidden;
        public string Category => _category;

        /// <summary>Texto del error visible (vacío si no hay).</summary>
        public string ErrorShown => _error != null && _error.gameObject.activeSelf ? _error.text : string.Empty;

        public override void Build(RectTransform root)
        {
            _custom = _module.CustomCategoriesOrEmpty();
            var scaffold = ScreenScaffold.Create(root, Loc.T(_editing == null ? "nuevaTarjeta" : "editarTarjeta"));
            scaffold.AppBar.SetLeading(IconId.Home, () => Router.Pop()).name = Loc.T("volver");
            var content = Ui.Scroll(scaffold.Body, 12f, new RectOffset(16, 16, 12, 32));

            var panel = Ui.Panel(content, Palette.NeonCyan.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            _word = NeonTextField.Create(panel.Content, Loc.T("campoTarjetaPalabra"), Loc.T("campoTarjetaPalabraHint"), _editing?.Palabra,
                characterLimit: 40);

            var forbiddenPanel = Ui.Panel(content, Palette.NeonMagenta.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(forbiddenPanel.Content, Loc.T("tarjetaProhibidasTitulo"), TextRole.TitleMedium, Palette.NeonMagenta, glow: 0.6f, bold: true);
            Ui.Text(forbiddenPanel.Content, Loc.T("tarjetaProhibidasAyuda"), TextRole.BodySmall, Palette.TextMuted);
            _forbidden.Clear();
            for (var i = 0; i < TabuWord.MaxForbidden; i++)
            {
                var label = i < TabuWord.MinForbidden
                    ? Loc.T("campoProhibida", ("n", i + 1))
                    : Loc.T("campoProhibidaOpcional", ("n", i + 1));
                var value = _editing != null && i < _editing.Prohibidas.Count ? _editing.Prohibidas[i] : null;
                _forbidden.Add(NeonTextField.Create(forbiddenPanel.Content, label, null, value, characterLimit: 30));
            }

            var categoryPanel = Ui.Panel(content, Palette.NeonViolet.WithAlpha(0.5f), glow: 0.3f, spacing: 10f);
            Ui.Text(categoryPanel.Content, Loc.T("campoCategoriaPalabra"), TextRole.TitleMedium, Palette.NeonVioletText, glow: 0.6f, bold: true);
            // Con el texto ya puesto: un botón creado sin texto no tiene etiqueta que cambiar después.
            _categoryButton = Ui.Button(categoryPanel.Content, CategoryOption.LabelOf(_category, _custom), OpenCategoryPicker, ButtonStyle.Outlined,
                IconId.Category);
            _categoryButton.name = Loc.T("elegirCategoria");

            _error = Ui.Text(content, string.Empty, TextRole.BodyMedium, Palette.NeonError, TextAlignmentOptions.Center);
            _error.gameObject.SetActive(false);
            Ui.Button(content, Loc.T(_editing == null ? "agregar" : "guardar"), () => Save(), ButtonStyle.Filled, IconId.Check, glow: 0.8f);
            _word.Focus();
        }

        /// <summary>Elige la categoría (también lo usan los tests).</summary>
        public void SetCategory(string category)
        {
            _category = string.IsNullOrEmpty(category) ? WordCategories.Propias : category;
            ShowCategory();
        }

        /// <summary>Guarda y vuelve a la lista. Si algo no vale, enseña el error y se queda.</summary>
        public bool Save()
        {
            var forbidden = _forbidden.Select(f => f.Value).ToList();
            try
            {
                if (_editing == null)
                {
                    _module.Words.Insert(_word.Value, forbidden, _category);
                    Snackbar.Show(Loc.T("tarjetaAnadida"));
                }
                else
                {
                    _module.Words.Update(_editing.Id, _word.Value, forbidden, _category);
                    Snackbar.Show(Loc.T("tarjetaActualizada"));
                }
            }
            catch (TabuCardException e)
            {
                _error.text = ErrorText(e);
                _error.gameObject.SetActive(true);
                return false;
            }
            Router.Pop();
            _saved?.Invoke();
            return true;
        }

        /// <summary>Mensaje de cada motivo por el que no se pudo guardar o borrar.</summary>
        public static string ErrorText(TabuCardException e)
        {
            switch (e.Problem)
            {
                case TabuCardProblem.Duplicada: return Loc.T("yaExisteEsaTarjeta");
                case TabuCardProblem.NoExiste: return Loc.T("esaTarjetaYaNoExiste");
                case TabuCardProblem.Predefinida: return Loc.T("tarjetasPredefinidasSoloLectura");
            }
            switch (e.Validation)
            {
                case TabuCardError.PalabraVacia: return Loc.T("tarjetaPalabraObligatoria");
                case TabuCardError.ProhibidaRepetida: return Loc.T("tarjetaProhibidaRepetida");
                default: return Loc.T("tarjetaProhibidasMinimo", ("min", TabuWord.MinForbidden));
            }
        }

        private void OpenCategoryPicker()
        {
            CategorySelectDialog.Show(Loc.T("elegirCategoria"), CategoryOption.Assignable(_custom), _category, SetCategory,
                onCreate: select => CategoryNameDialog.ShowCreate(_module.Categories, created =>
                {
                    _custom = _module.CustomCategoriesOrEmpty();
                    select(new CategoryOption(created.CategoryId, created.Name));
                }));
        }

        private void ShowCategory() => _categoryButton?.SetLabel(CategoryOption.LabelOf(_category, _custom));
    }
}
