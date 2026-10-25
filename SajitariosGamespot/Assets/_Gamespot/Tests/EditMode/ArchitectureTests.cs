using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gamespot.Core;
using NUnit.Framework;
using UnityEngine;

namespace Gamespot.Tests.EditMode
{
    /// <summary>Reglas de arquitectura de CLAUDE.md §4 comprobadas sobre los asmdef.</summary>
    public sealed class ArchitectureTests
    {
        private static readonly string ScriptsFolder = Path.Combine(Application.dataPath, "_Gamespot", "Scripts");

        private static IEnumerable<(string path, Dictionary<string, object> asmdef)> Asmdefs() =>
            Directory.GetFiles(ScriptsFolder, "*.asmdef", SearchOption.AllDirectories)
                .Select(p => (p, Json.ParseObject(File.ReadAllText(p))));

        private static List<string> References(Dictionary<string, object> asmdef) =>
            ((List<object>)asmdef["references"]).Select(r => (string)r).ToList();

        [Test]
        public void ElMenuNoConoceJuegosConcretos()
        {
            var menu = Asmdefs().Single(a => (string)a.asmdef["name"] == "Gamespot.Menu");
            var games = References(menu.asmdef).Where(r => r.StartsWith("Gamespot.Games.") && r != "Gamespot.Games.Shared").ToList();
            CollectionAssert.IsEmpty(games, "Regla de oro: el menú solo lee la lista de descriptores");
        }

        [Test]
        public void LosDominiosSonPuros()
        {
            foreach (var (path, asmdef) in Asmdefs())
            {
                var name = (string)asmdef["name"];
                if (name != "Gamespot.Core" && !name.EndsWith(".Domain")) continue;
                Assert.AreEqual(true, asmdef["noEngineReferences"], $"{name} no puede depender de UnityEngine");
                Assert.IsTrue(References(asmdef).All(r => r == "Gamespot.Core"), $"{name} solo puede referenciar Gamespot.Core");
            }
        }

        [Test]
        public void UnJuegoNoReferenciaAOtro()
        {
            foreach (var (path, asmdef) in Asmdefs())
            {
                var name = (string)asmdef["name"];
                if (!name.StartsWith("Gamespot.Games.") || name == "Gamespot.Games.Shared") continue;
                var game = name.Split('.')[2];
                var others = References(asmdef).Where(r => r.StartsWith("Gamespot.Games.") && r != "Gamespot.Games.Shared"
                                                           && r.Split('.')[2] != game).ToList();
                CollectionAssert.IsEmpty(others, $"{name} referencia a otro juego");
            }
        }
    }
}
