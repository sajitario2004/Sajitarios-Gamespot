using System;
using System.Collections.Generic;
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
    /// "Mis categorías" (V059l), común al Impostor y al Tabú: lista de las categorías propias con cuántas palabras
    /// tiene cada una, crear, renombrar y borrar. Se apila sobre la gestión de palabras o de tarjetas. Al borrar una,
    /// el juego mueve antes sus palabras a "Mis palabras" (<c>moveToOwn</c>): no se pierde nada.
    /// </summary>
    public sealed class CustomCategoriesScreen : AppScreen
    {
        private readonly CustomCategoryRepository _repository;
        private readonly Func<IReadOnlyDictionary<string, long>> _counts;
        private readonly Action<string> _moveToOwn;
        private readonly string _countKey;
        private readonly Action _changed;
        private List<CustomCategory> _items = new List<CustomCategory>();
        private RectTransform _list;
        private TMP_Text _empty;

        /// <param name="counts">Palabras por id de categoría.</param>
        /// <param name="moveToOwn">Pasa las palabras de esa categoría a "Mis palabras" (antes de borrarla).</param>
        /// <param name="countKey">Clave del contador ("{n} palabras", "{n} tarjetas").</param>
        /// <param name="changed">Avisa a la pantalla de debajo de que algo cambió.</param>
        public CustomCategoriesScreen(CustomCategoryRepository repository, Func<IReadOnlyDictionary<string, long>> counts, Action<string> moveToOwn,
            string countKey, Action changed = null)
        {
            _repository = repository;
            _counts = counts;
            _moveToOwn = moveToOwn;
            _countKey = countKey;
            _changed = changed;
        }

        public IReadOnlyList<CustomCategory> Items => _items;

        public override void Build(RectTransform root)
        {
            var scaffold = ScreenScaffold.Create(root, Loc.T("categoriasPropiasTitulo"));
            scaffold.AppBar.SetLeading(IconId.Home, () => Router.Pop()).name = Loc.T("volver");
            var content = Ui.Scroll(scaffold.Body, 12f, new RectOffset(16, 16, 12, 24));
            Ui.Text(content, Loc.T("categoriasPropiasAyuda"), TextRole.BodySmall, Palette.TextMuted);
            _list = Ui.Column(content, 10f, name: "Categories");
            _empty = Ui.Text(content, Loc.T("sinCategoriasPropias"), TextRole.BodyLarge, Palette.TextMuted, TextAlignmentOptions.Center);
            Ui.Space(content, 8f);
            Ui.Button(content, Loc.T("nuevaCategoria"), () => CategoryNameDialog.ShowCreate(_repository, _ => Changed()), ButtonStyle.Filled,
                IconId.Add, glow: 0.8f);
            Reload();
        }

        /// <summary>Vuelve a leer las categorías y sus contadores.</summary>
        public void Reload()
        {
            IReadOnlyDictionary<string, long> counts;
            try
            {
                _items = _repository.GetAll();
                counts = _counts?.Invoke() ?? new Dictionary<string, long>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Categorías] No se pudieron cargar: {e.Message}");
                _items = new List<CustomCategory>();
                counts = new Dictionary<string, long>();
            }
            _list.ClearChildren();
            foreach (var item in _items)
            {
                BuildRow(item, counts.TryGetValue(item.CategoryId, out var n) ? n : 0);
            }
            _empty.gameObject.SetActive(_items.Count == 0);
        }

        /// <summary>Renombra sin diálogo (también lo usan los tests). Devuelve el error, o nulo.</summary>
        public CategoryNameError? Rename(long id, string name)
        {
            try
            {
                _repository.Rename(id, name, CategoryOption.BuiltInNames());
            }
            catch (CategoryNameException e)
            {
                return e.Error;
            }
            Snackbar.Show(Loc.T("categoriaRenombrada"));
            Changed();
            return null;
        }

        /// <summary>Borra sin confirmar (también lo usan los tests): sus palabras pasan antes a "Mis palabras".</summary>
        public void Delete(long id)
        {
            try
            {
                _moveToOwn?.Invoke(WordCategories.CustomId(id));
                _repository.Delete(id);
                Snackbar.Show(Loc.T("categoriaBorrada"));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Categorías] No se pudo borrar: {e.Message}");
                Snackbar.Show(Loc.T("errorNoSePudoGuardar"), Palette.NeonError);
            }
            Changed();
        }

        private void Changed()
        {
            Reload();
            _changed?.Invoke();
        }

        private void BuildRow(CustomCategory item, long count)
        {
            var panel = NeonPanel.Create(_list, Palette.NeonCyan, Palette.Surface, 16f, 1.5f, 0f, padding: new RectOffset(16, 8, 10, 10),
                name: $"Category {item.Name}");
            var row = Ui.Row(panel.Content, 6f, alignment: TextAnchor.MiddleLeft);
            var texts = Ui.Column(row, 2f, alignment: TextAnchor.MiddleLeft);
            // Ancho preferido 0: la columna se queda lo que sobra y no aplasta el chip ni los botones de la derecha.
            texts.Sized(width: 0f, flexWidth: 1f);
            Ui.Text(texts, item.Name, TextRole.TitleMedium, maxLines: 1, bold: true);
            Ui.Text(texts, Loc.T(_countKey, ("n", count)), TextRole.BodySmall, Palette.TextMuted, maxLines: 1);
            var id = item.Id;
            var name = item.Name;
            NeonButton.CreateIcon(row, IconId.Edit, () => CategoryNameDialog.Show(Loc.T("renombrarCategoria"), name, Loc.T("guardar"),
                value => Rename(id, value)), Palette.NeonCyan, 22f).name = Loc.T("renombrarCategoria");
            NeonButton.CreateIcon(row, IconId.Delete, () => Dialogs.Confirm(Loc.T("borrarCategoriaTitulo"),
                Loc.T("borrarCategoriaMensaje", ("nombre", name)), Loc.T("cancelar"), Loc.T("borrar"), ok =>
                {
                    if (ok) Delete(id);
                }, Palette.NeonError), Palette.NeonError, 22f).name = Loc.T("borrar");
        }
    }
}
