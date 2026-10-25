using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Impostor;
using Gamespot.Games.Impostor.Data;
using Gamespot.Games.Impostor.Domain;
using Gamespot.Tests.EditMode.Db;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Games.Impostor
{
    public sealed class ImpostorDataTests
    {
        private static readonly DictionarySeedSource SmallSeed = new DictionarySeedSource(new Dictionary<string, string>
        {
            ["impostor_words"] = "[{\"word\": \"playa\", \"hint\": \"arena\"}, {\"word\": \"Pirata\", \"hint\": \"barco\"}, {\"word\": \"PLAYA\", \"hint\": \"sol\"}]"
        });

        private TempDatabase _temp;
        private WordRepository _words;
        private GameHistoryRepository _history;
        private long _clock = 1000;

        [SetUp]
        public void SetUp()
        {
            _temp = new TempDatabase();
            var db = _temp.Open(new IDbSchema[] { new ImpostorGame() }, SmallSeed);
            _words = new WordRepository(db, () => ++_clock);
            _history = new GameHistoryRepository(db, () => DateTimeOffset.FromUnixTimeMilliseconds(++_clock));
        }

        [TearDown]
        public void TearDown() => _temp.Dispose();

        [Test]
        public void MigracionA3_AnadeLasPalabrasNuevasYRespetaLasPropias()
        {
            using (var temp = new TempDatabase())
            {
                var db = temp.Open(new IDbSchema[] { new ImpostorGame() }, SmallSeed, version: 2);
                db.Execute("INSERT INTO impostor_words (word, hint, is_seed, created_at) VALUES ('Ibai', 'mi pista', 0, 1)");
                temp.Close();

                var bigger = new DictionarySeedSource(new Dictionary<string, string>
                {
                    ["impostor_words"] = "[{\"word\": \"playa\", \"hint\": \"arena\"}, {\"word\": \"Pirata\", \"hint\": \"barco\"}, " +
                                         "{\"word\": \"Ibai\", \"hint\": \"Kings League\"}, {\"word\": \"Rosalía\", \"hint\": \"Motomami\"}]"
                });
                db = temp.Open(new IDbSchema[] { new ImpostorGame() }, bigger, version: ImpostorSchema.SeedExpandedVersion);
                var words = new WordRepository(db, () => 1).GetAll();
                Assert.IsTrue(words.Any(w => w.Word == "Rosalía" && w.IsSeed), "La palabra nueva entra como predefinida");
                var ibai = words.Single(w => w.Word == "Ibai");
                Assert.IsFalse(ibai.IsSeed, "La que ya había creado el usuario se queda suya");
                Assert.AreEqual("mi pista", ibai.Hint);
                Assert.AreEqual(4, words.Count, "playa, Pirata, Ibai (propia) y Rosalía");

                temp.Close();
                db = temp.Open(new IDbSchema[] { new ImpostorGame() }, bigger, version: ImpostorSchema.SeedExpandedVersion);
                Assert.AreEqual(4, new WordRepository(db, () => 1).GetAll().Count, "Sin migración no se vuelve a sembrar");
            }
        }

        [Test]
        public void Seed_SeCargaConIsSeedEIgnoraDuplicadosSinMayusculas()
        {
            var all = _words.GetAll();
            CollectionAssert.AreEqual(new[] { "Pirata", "playa" }, all.Select(w => w.Word));
            Assert.IsTrue(all.All(w => w.IsSeed));
        }

        [Test]
        public void Seed_NoSeRecargaSiYaHayPalabras()
        {
            Assert.AreEqual(0, ImpostorSchema.SeedIfEmpty(_temp.Db, SmallSeed));
        }

        [Test]
        public void Insertar_RecortaYEsDelUsuario()
        {
            var id = _words.Insert("  montaña ", " nieve ");
            var word = _words.GetById(id);
            Assert.AreEqual("montaña", word.Word);
            Assert.AreEqual("nieve", word.Hint);
            Assert.IsFalse(word.IsSeed);
        }

        [Test]
        public void Insertar_DuplicadoSinMayusculas_Lanza()
        {
            Assert.Throws<DuplicateWordException>(() => _words.Insert("PIRATA", "loro"));
        }

        [Test]
        public void Insertar_Vacio_Lanza()
        {
            Assert.Throws<ArgumentException>(() => _words.Insert("  ", "pista"));
            Assert.Throws<ArgumentException>(() => _words.Insert("palabra", ""));
        }

        [Test]
        public void Editar_ErroresEnOrden()
        {
            var seedId = _words.GetAll().First(w => w.Word == "playa").Id;
            var mine = _words.Insert("montaña", "nieve");
            Assert.Throws<ArgumentException>(() => _words.Update(seedId, "", "x"), "vacío antes que seed");
            Assert.Throws<WordNotFoundException>(() => _words.Update(9999, "a", "b"));
            Assert.Throws<ReadOnlySeedWordException>(() => _words.Update(seedId, "otra", "pista"));
            Assert.Throws<DuplicateWordException>(() => _words.Update(mine, "Playa", "pista"));
            _words.Update(mine, "Montaña rusa", "atracción");
            Assert.AreEqual("Montaña rusa", _words.GetById(mine).Word);
            _words.Update(mine, "montaña RUSA", "atracción");
        }

        [Test]
        public void Borrar_SoloLasPropias()
        {
            var seedId = _words.GetAll().First().Id;
            var mine = _words.Insert("montaña", "nieve");
            Assert.Throws<ReadOnlySeedWordException>(() => _words.Delete(seedId));
            Assert.Throws<WordNotFoundException>(() => _words.Delete(9999));
            _words.Delete(mine);
            Assert.IsNull(_words.GetById(mine));
        }

        [Test]
        public void Buscar_ContieneSinMayusculasYEscapaComodines()
        {
            _words.Insert("100% real", "porcentaje");
            _words.Insert("guion_bajo", "símbolo");
            CollectionAssert.AreEqual(new[] { "playa" }, _words.Search("LAY").Select(w => w.Word));
            CollectionAssert.AreEqual(new[] { "100% real" }, _words.Search("%").Select(w => w.Word));
            CollectionAssert.AreEqual(new[] { "guion_bajo" }, _words.Search("_").Select(w => w.Word));
            Assert.AreEqual(_words.GetAll().Count, _words.Search("   ").Count);
        }

        [Test]
        public void FuenteDePalabras_DevuelveDominio()
        {
            var words = _words.GetAllWords();
            Assert.AreEqual(2, words.Count);
            Assert.AreEqual(new Word("Pirata", "barco"), words[0]);
        }

        [Test]
        public void Historial_GuardaLeeYBorra()
        {
            var players = new List<Player> { new Player("Nacho"), new Player("Iker"), new Player("Lucía") };
            var roles = new Dictionary<Player, Role> { [players[0]] = Role.Impostor, [players[1]] = Role.Palabra, [players[2]] = Role.Palabra };
            _history.InsertFromSession(new GameSession(new Word("playa", "arena"), players, roles), true);
            _history.InsertFromSession(new GameSession(new Word("pirata", "barco"), players, roles), false);

            var all = _history.GetAll();
            Assert.AreEqual(2, all.Count);
            Assert.AreEqual("pirata", all[0].Word, "La más reciente primero");
            var first = all[1];
            Assert.AreEqual(3, first.NPlayers);
            Assert.AreEqual(1, first.NImpostors);
            Assert.IsTrue(first.HintEnabled);
            CollectionAssert.AreEqual(new[] { "Nacho", "Iker", "Lucía" }, first.Players.Select(p => p.Name));
            CollectionAssert.AreEqual(new[] { true, false, false }, first.Players.Select(p => p.WasImpostor));

            _history.DeleteAll();
            Assert.AreEqual(0L, _history.Count());
        }

        [Test]
        public void JugadoresJson_IdaYVueltaConComillas()
        {
            var json = GameHistoryRepository.PlayersToJson(new[] { new GameRecordPlayer("Ana \"la\" grande", true) });
            var back = GameHistoryRepository.PlayersFromJson(json);
            Assert.AreEqual("Ana \"la\" grande", back[0].Name);
            Assert.IsTrue(back[0].WasImpostor);
        }
    }
}
