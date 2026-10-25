using System.Collections;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Impostor;
using Gamespot.Games.Shared;
using Gamespot.Games.Tabu;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>
    /// V059l: categorías propias en el Impostor y el Tabú (crear, usar en palabras y tarjetas, elegirlas en el setup,
    /// jugar solo con ellas y borrarlas sin perder nada) y tarjetas propias del Tabú.
    /// </summary>
    public sealed class OwnCategoriesTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown()
        {
            Dialogs.CloseAll();
            _app?.Dispose();
        }

        private T Game<T>() => _app.Bootstrap.Games.OfType<T>().Single();

        [UnityTest]
        public IEnumerator Impostor_CategoriaPropiaSaleMarcadaYSeJuegaSoloConElla()
        {
            _app = new AppTestHarness();
            var module = Game<ImpostorGame>().Module;
            Assert.IsNull(CategoryNameDialog.TryCreate(module.Categories, "Mi pueblo", out var pueblo));
            Assert.AreEqual(CategoryNameError.Duplicado, CategoryNameDialog.TryCreate(module.Categories, Loc.T("categoriaFiesta"), out _),
                "No puede llamarse como una de serie");
            module.Words.Insert("La plaza", "fuente", pueblo.CategoryId);

            _app.Router.Go(ImpostorModule.SetupRoute);
            var setup = (ImpostorSetupScreen)_app.Router.Current;
            yield return null;
            Assert.AreEqual(pueblo.CategoryId, setup.Categories.Shown.Last(), "Las propias van al final");
            Assert.IsTrue(setup.Categories.IsSelected(pueblo.CategoryId), "La primera vez sale marcada");
            setup.Categories.SelectOnly(pueblo.CategoryId);
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Bea");
            setup.Players.SetName(2, "Carlos");
            setup.Start();
            yield return null;
            Assert.AreEqual("La plaza", module.Flow.Session.Word.Text);

            // Al volver, la elección se conserva: no se vuelve a marcar sola nada.
            module.ResetToSetup();
            yield return null;
            setup = (ImpostorSetupScreen)_app.Router.Current;
            CollectionAssert.AreEqual(new[] { pueblo.CategoryId }, setup.Categories.Selected);
        }

        [UnityTest]
        public IEnumerator Impostor_FiltroYBorrarCategoriaPasaLasPalabrasAMisPalabras()
        {
            _app = new AppTestHarness();
            var module = Game<ImpostorGame>().Module;
            CategoryNameDialog.TryCreate(module.Categories, "Clase", out var clase);
            module.Words.Insert("La profe", "examen", clase.CategoryId);
            module.Words.Insert("El recreo", "patio", clase.CategoryId);

            _app.Router.Go(ImpostorModule.WordsRoute);
            var words = (WordsScreen)_app.Router.Current;
            yield return null;
            words.SetCategoryFilter(clase.CategoryId);
            CollectionAssert.AreEqual(new[] { "El recreo", "La profe" }, words.Items.Select(w => w.Word));

            var screen = new CustomCategoriesScreen(module.Categories, () => module.Words.CountByCategory(),
                from => module.Words.MoveCategory(from, WordCategories.Propias), "categoriaContadorPalabras", words.Reload);
            _app.Router.Push(screen);
            yield return null;
            Assert.AreEqual("Clase", screen.Items.Single().Name);
            Assert.IsNull(screen.Rename(clase.Id, "Mi clase"));
            Assert.AreEqual("Mi clase", module.Categories.GetById(clase.Id).Name);
            screen.Delete(clase.Id);
            CollectionAssert.IsEmpty(screen.Items);
            Assert.IsNull(words.CategoryFilter, "La categoría del filtro se borró: el filtro se quita");
            Assert.AreEqual(2, module.Words.CountByCategory()[WordCategories.Propias], "Las palabras siguen, en Mis palabras");
            _app.Router.Pop();
        }

        [UnityTest]
        public IEnumerator Tabu_TarjetaPropiaEnCategoriaNuevaYPartidaSoloConElla()
        {
            _app = new AppTestHarness();
            var module = Game<TabuGame>().Module;
            CategoryNameDialog.TryCreate(module.Categories, "Mi pueblo", out var pueblo);

            _app.Router.Go(TabuModule.WordsRoute);
            var list = (TabuWordsScreen)_app.Router.Current;
            yield return null;
            Assert.That(list.Items.Count, Is.GreaterThanOrEqualTo(1000), "Salen las de seed");

            var editor = list.OpenEditor(null);
            yield return null;
            Assert.AreEqual(WordCategories.Propias, editor.Category, "Una nueva va a Mis palabras");
            editor.Word.Value = "Villarriba";
            editor.Forbidden[0].Value = "plaza";
            editor.Forbidden[1].Value = "fiestas";
            editor.Forbidden[2].Value = "bar";
            Assert.IsFalse(editor.Save(), "Con tres prohibidas no se guarda");
            Assert.AreEqual(Loc.T("tarjetaProhibidasMinimo", ("min", 4)), editor.ErrorShown);
            editor.Forbidden[3].Value = "Plaza";
            Assert.IsFalse(editor.Save());
            Assert.AreEqual(Loc.T("tarjetaProhibidaRepetida"), editor.ErrorShown);
            editor.Forbidden[3].Value = "iglesia";
            editor.SetCategory(pueblo.CategoryId);
            Assert.IsTrue(editor.Save());
            yield return null;
            Assert.AreSame(list, _app.Router.Current, "Al guardar vuelve a la lista");
            list.SetCategoryFilter(pueblo.CategoryId);
            Assert.AreEqual("Villarriba", list.Items.Single().Palabra);

            _app.Router.Go(TabuModule.SetupRoute);
            var setup = (TabuSetupScreen)_app.Router.Current;
            yield return null;
            Assert.IsTrue(setup.Categories.IsSelected(pueblo.CategoryId), "La categoría propia sale marcada");
            setup.TeamA.Value = "Rojos";
            setup.TeamB.Value = "Azules";
            setup.Categories.SelectOnly(pueblo.CategoryId);
            setup.Start();
            yield return null;
            Assert.AreEqual("Villarriba", module.Flow.Session.PalabraActual.Palabra);
        }

        [UnityTest]
        public IEnumerator Tabu_EditarYBorrarTarjetaPropia()
        {
            _app = new AppTestHarness();
            var module = Game<TabuGame>().Module;
            var id = module.Words.Insert("Villarriba", new[] { "plaza", "fiestas", "bar", "iglesia" });
            _app.Router.Go(TabuModule.WordsRoute);
            var list = (TabuWordsScreen)_app.Router.Current;
            yield return null;
            list.SetCategoryFilter(WordCategories.Propias);
            var card = list.Items.Single();
            var editor = list.OpenEditor(card);
            yield return null;
            Assert.AreEqual("Villarriba", editor.Word.Value);
            Assert.AreEqual("iglesia", editor.Forbidden[3].Value);
            Assert.AreEqual(string.Empty, editor.Forbidden[4].Value);
            editor.Word.Value = "Villabajo";
            editor.Forbidden[4].Value = "campo";
            Assert.IsTrue(editor.Save());
            yield return null;
            Assert.AreEqual("Villabajo", module.Words.GetById(id).Palabra);
            Assert.AreEqual(5, module.Words.GetById(id).Prohibidas.Count);

            list.Delete(id);
            CollectionAssert.IsEmpty(list.Items);
            Assert.IsNull(module.Words.GetById(id));
        }
    }
}
