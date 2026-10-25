using System.Collections;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Localization;
using Gamespot.Core.Runtime.UI;
using Gamespot.Games.Shared;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>V070: gestión de cartas común (con tipos, niveles, categorías y segunda opción) y confirmación de edad común.</summary>
    public sealed class PromptCardsTests
    {
        private const string Table = "test_prompt_cards";
        private const string CategoriesTable = "test_prompt_categories";

        private AppTestHarness _app;

        [TearDown]
        public void TearDown()
        {
            Dialogs.CloseAll();
            _app?.Dispose();
        }

        private (PromptCardsScreen screen, PromptCardRepository cards, CustomCategoryRepository categories) Open(bool withB = false)
        {
            _app = new AppTestHarness();
            var db = _app.Bootstrap.Database;
            PromptCardRepository.CreateTable(db, Table);
            CustomCategoryRepository.CreateTable(db, CategoriesTable);
            PromptCardRepository.SeedIfEmpty(db, Table, new[]
            {
                new PromptCard(0, "¿Cuál es tu mayor miedo?", PromptLevel.Suave, "verdad", WordCategories.General),
                new PromptCard(0, "Haz diez sentadillas", PromptLevel.Suave, "reto", WordCategories.General),
                new PromptCard(0, "¿Qué es lo más atrevido que has hecho?", PromptLevel.Picante, "verdad", WordCategories.Cachondeo)
            });
            var cards = new PromptCardRepository(db, Table);
            var categories = new CustomCategoryRepository(db, CategoriesTable);
            var config = new PromptCardsConfig
            {
                Kinds = new[] { ("verdad", "nivelSuave"), ("reto", "nivelPicante") },
                SeedCategories = new[] { WordCategories.General, WordCategories.Cachondeo },
                CustomCategories = categories
            };
            if (withB)
            {
                config.Kinds = System.Array.Empty<(string, string)>();
                config.SeedCategories = null;
                config.CustomCategories = null;
                config.TextBLabelKey = "campoCarta";
            }
            var screen = new PromptCardsScreen(cards, config);
            _app.Router.Push(screen);
            return (screen, cards, categories);
        }

        [UnityTest]
        public IEnumerator Gestion_CrearFiltrarEditarYBorrar()
        {
            var (screen, cards, categories) = Open();
            yield return null;
            Assert.AreEqual(3, screen.Items.Count);
            Assert.IsTrue(screen.Items.All(i => i.IsSeed));

            Assert.AreEqual(PromptCardProblem.Vacia, screen.Save(null, "   ", null, PromptLevel.Suave, "reto", WordCategories.Propias));
            Assert.AreEqual(PromptCardProblem.Duplicada, screen.Save(null, "haz diez sentadillas.", null, PromptLevel.Suave, "reto", null));
            Assert.IsNull(screen.Save(null, "  Imita  a un famoso ", null, PromptLevel.Picante, "reto", null));
            var own = screen.Items.First();
            Assert.IsFalse(own.IsSeed, "Las propias, arriba");
            Assert.AreEqual("Imita a un famoso", own.Texto, "Se limpian los espacios");
            Assert.AreEqual(WordCategories.Propias, own.Categoria, "Sin categoría va a las propias");

            screen.SetFilter("own");
            Assert.AreEqual(1, screen.Items.Count);
            screen.SetFilter("lvl:picante");
            Assert.AreEqual(2, screen.Items.Count);
            screen.SetFilter("kind:verdad");
            Assert.AreEqual(2, screen.Items.Count);
            screen.SetFilter("cat:" + WordCategories.Cachondeo);
            Assert.AreEqual(1, screen.Items.Count);
            screen.SetFilter(null);
            screen.SetQuery("famoso");
            yield return new UnityEngine.WaitForSeconds(PromptCardsScreen.DebounceSeconds + 0.1f);
            Assert.AreEqual(1, screen.Items.Count, "La búsqueda espera y filtra");

            Assert.IsNull(CategoryNameDialog.TryCreate(categories, "Oficina", out var oficina));
            Assert.IsNull(screen.Save(own, "Imita a tu jefe", null, PromptLevel.Suave, "reto", oficina.CategoryId));
            Assert.AreEqual(oficina.CategoryId, cards.GetById(own.Id).Categoria);
            screen.SetQuery(string.Empty);
            yield return new UnityEngine.WaitForSeconds(PromptCardsScreen.DebounceSeconds + 0.1f);
            screen.SetFilter("cat:" + oficina.CategoryId);
            Assert.AreEqual("Imita a tu jefe", screen.Items.Single().Texto);

            screen.Delete(screen.Items.Single().Id);
            Assert.AreEqual(0, cards.CountOwn());
            screen.Delete(cards.Search().First().Id);
            Assert.AreEqual(3, cards.Search().Count, "Las de serie no se borran");
        }

        [UnityTest]
        public IEnumerator Gestion_ConSegundaOpcionObligatoria()
        {
            var (screen, cards, _) = Open(true);
            yield return null;
            Assert.AreEqual(PromptCardProblem.FaltaSegunda, screen.Save(null, "Volar", " ", PromptLevel.Suave, null, null));
            Assert.IsNull(screen.Save(null, "Volar", "Ser invisible", PromptLevel.Suave, null, null));
            var row = cards.Search("Volar").Single();
            Assert.AreEqual("Ser invisible", row.TextoB);
            Assert.AreEqual(string.Empty, row.Categoria, "Sin categorías en este juego");
        }

        [UnityTest]
        public IEnumerator AdultGate_PreguntaUnaVezPorSesion()
        {
            _app = new AppTestHarness();
            yield return null;
            Assert.IsFalse(AdultGate.Confirmed);
            var calls = 0;
            AdultGate.Request(() => calls++);
            Assert.IsTrue(Dialogs.IsOpen);
            Dialogs.HandleBack();
            Assert.AreEqual(0, calls, "Cancelar no hace nada");
            Assert.IsFalse(AdultGate.Confirmed);

            AdultGate.Request(() => calls++);
            var confirm = UnityEngine.Object.FindObjectsByType<NeonButton>(UnityEngine.FindObjectsSortMode.None)
                .First(b => b.isActiveAndEnabled && b.name == $"Button {Loc.T("adultosSi")}");
            confirm.Button.onClick.Invoke();
            yield return null;
            Assert.AreEqual(1, calls);
            Assert.IsTrue(AdultGate.Confirmed);
            AdultGate.Request(() => calls++);
            Assert.IsFalse(Dialogs.IsOpen, "Ya no vuelve a preguntar");
            Assert.AreEqual(2, calls);
        }
    }
}
