using System.Collections;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Impostor;
using Gamespot.Games.Impostor.Data;
using Gamespot.Games.Tabu;
using Gamespot.Games.Tabu.Data;
using Gamespot.Games.Tabu.Domain;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gamespot.Tests.PlayMode.Games
{
    /// <summary>V059i: el Impostor y el Tabú sacan las palabras solo de las categorías marcadas.</summary>
    public sealed class CategoryTests
    {
        private AppTestHarness _app;

        [TearDown]
        public void TearDown() => _app?.Dispose();

        private T Game<T>() => _app.Bootstrap.Games.OfType<T>().Single();

        [UnityTest]
        public IEnumerator Impostor_SoloPolitica()
        {
            _app = new AppTestHarness();
            _app.Router.Go(ImpostorModule.SetupRoute);
            var setup = (ImpostorSetupScreen)_app.Router.Current;
            yield return null;
            CollectionAssert.AreEqual(WordCategories.Seed, setup.Categories.Shown, "Sin palabras propias no sale «Mis palabras»");
            Assert.AreEqual(WordCategories.Seed.Count, setup.Categories.Selected.Count, "Por defecto, todas");

            setup.Categories.SelectOnly(WordCategories.Politica);
            setup.Categories.Toggle(WordCategories.Politica);
            CollectionAssert.AreEqual(new[] { WordCategories.Politica }, setup.Categories.Selected, "La última no se puede quitar");

            var politics = ImpostorSchema.Parse(new ResourcesSeedSource().Load(ImpostorSchema.SeedName))
                .Where(e => e.category == WordCategories.Politica).Select(e => e.word).ToList();
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Bea");
            setup.Players.SetName(2, "Carlos");
            setup.Start();
            yield return null;
            var module = Game<ImpostorGame>().Module;
            CollectionAssert.Contains(politics, module.Flow.Session.Word.Text);
        }

        [UnityTest]
        public IEnumerator Impostor_MisPalabrasSaleSiHayPropias()
        {
            _app = new AppTestHarness();
            Game<ImpostorGame>().Module.Words.Insert("Nuestra frase", "grupo");
            _app.Router.Go(ImpostorModule.SetupRoute);
            var setup = (ImpostorSetupScreen)_app.Router.Current;
            yield return null;
            Assert.AreEqual(WordCategories.Propias, setup.Categories.Shown.Last());
            setup.Categories.SelectOnly(WordCategories.Propias);
            setup.Players.SetName(0, "Ana");
            setup.Players.SetName(1, "Bea");
            setup.Players.SetName(2, "Carlos");
            setup.Start();
            yield return null;
            Assert.AreEqual("Nuestra frase", Game<ImpostorGame>().Module.Flow.Session.Word.Text, "La única propia");
        }

        [UnityTest]
        public IEnumerator Tabu_SoloSitiosYJugarOtraLasConserva()
        {
            _app = new AppTestHarness();
            _app.Router.Go(TabuModule.SetupRoute);
            var setup = (TabuSetupScreen)_app.Router.Current;
            yield return null;
            setup.TeamA.Value = "Rojos";
            setup.TeamB.Value = "Azules";
            setup.Categories.SelectOnly(WordCategories.Sitios);
            setup.Start();
            yield return null;
            var module = Game<TabuGame>().Module;
            var sitios = TabuSchema.ParseEntries(new ResourcesSeedSource().Load(TabuSchema.SeedName))
                .Where(e => e.categoria == WordCategories.Sitios).Select(e => e.word.Palabra).ToList();
            for (var i = 0; i < 5; i++)
            {
                CollectionAssert.Contains(sitios, module.Flow.Session.PalabraActual.Palabra);
                module.Flow.Saltar();
            }

            while (module.Flow.Fase != TabuFase.GameOver)
            {
                if (module.Flow.Session.EquipoActual == TabuEquipo.A) module.Flow.Acierto();
                module.Flow.TerminarTurno();
                module.Flow.SiguienteTurno();
            }
            module.PlayAgain();
            yield return null;
            CollectionAssert.Contains(sitios, module.Flow.Session.PalabraActual.Palabra, "Jugar otra sigue con las mismas categorías");
        }
    }
}
