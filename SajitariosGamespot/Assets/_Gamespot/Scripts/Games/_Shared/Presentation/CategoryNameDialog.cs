using System;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.Theme;
using Gamespot.Core.Runtime.UI;
using TMPro;

namespace Gamespot.Games.Shared
{
    /// <summary>Diálogo con el nombre de una categoría propia (crear o renombrar), con su error debajo del campo.</summary>
    public static class CategoryNameDialog
    {
        /// <param name="tryApply">Guarda el nombre: devuelve nulo si fue bien (se cierra) o el error que enseñar.</param>
        public static void Show(string title, string initial, string confirmLabel, Func<string, CategoryNameError?> tryApply)
        {
            NeonTextField field = null;
            TMP_Text error = null;
            Dialogs.Show(title, null, new[]
            {
                new DialogAction(Loc.T("cancelar"), (Action)null, ButtonStyle.Text, Palette.NeonCyan),
                new DialogAction(confirmLabel, () =>
                {
                    var result = tryApply(field.Value);
                    if (result == null) return true;
                    error.text = Loc.T(result == CategoryNameError.Vacio ? "campoObligatorio" : "yaExisteEsaCategoria");
                    error.gameObject.SetActive(true);
                    return false;
                }, ButtonStyle.Filled, Palette.NeonCyan, 0.6f)
            }, buildContent: content =>
            {
                field = NeonTextField.Create(content, Loc.T("campoCategoria"), Loc.T("campoCategoriaHint"), initial,
                    characterLimit: CustomCategoryRepository.MaxNameLength);
                error = Ui.Text(content, string.Empty, TextRole.BodySmall, Palette.NeonError);
                error.gameObject.SetActive(false);
                field.Focus();
            });
        }

        /// <summary>"Nueva categoría": la crea en <paramref name="repository"/> y avisa con la fila nueva.</summary>
        public static void ShowCreate(CustomCategoryRepository repository, Action<CustomCategory> onCreated)
        {
            Show(Loc.T("nuevaCategoria"), null, Loc.T("agregar"), name =>
            {
                var error = TryCreate(repository, name, out var created);
                if (error != null) return error;
                Snackbar.Show(Loc.T("categoriaCreada"));
                onCreated?.Invoke(created);
                return null;
            });
        }

        /// <summary>Crea la categoría sin diálogo (también lo usan los tests). Devuelve el error, o nulo.</summary>
        public static CategoryNameError? TryCreate(CustomCategoryRepository repository, string name, out CustomCategory created)
        {
            created = null;
            try
            {
                created = repository.Insert(name, CategoryOption.BuiltInNames());
                return null;
            }
            catch (CategoryNameException e)
            {
                return e.Error;
            }
        }
    }
}
