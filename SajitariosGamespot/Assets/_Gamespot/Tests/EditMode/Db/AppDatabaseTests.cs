using System;
using System.Collections.Generic;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Db
{
    public sealed class AppDatabaseTests
    {
        /// <summary>Esquema de prueba: una tabla con UNIQUE COLLATE NOCASE y un seed JSON.</summary>
        private sealed class FakeSchema : IDbSchema
        {
            public int Creates;
            public readonly List<(int from, int to)> Upgrades = new List<(int, int)>();
            public int Seeds;
            public bool ThrowOnCreate;

            public void OnCreate(IDb db)
            {
                Creates++;
                db.Execute("CREATE TABLE IF NOT EXISTS things (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL UNIQUE COLLATE NOCASE, is_seed INTEGER NOT NULL DEFAULT 0)");
                if (ThrowOnCreate) throw new InvalidOperationException("fallo a mitad");
            }

            public void OnUpgrade(IDb db, int oldVersion, int newVersion) => Upgrades.Add((oldVersion, newVersion));

            public void SeedIfEmpty(IDb db, ISeedSource seeds)
            {
                if (!AppDatabase.IsEmpty(db, "things")) return;
                Seeds++;
                foreach (var item in Json.ParseArray(seeds.Load("things")))
                {
                    db.Execute("INSERT OR IGNORE INTO things (name, is_seed) VALUES (?, 1)", (string)item);
                }
            }
        }

        private sealed class ThingRow
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public bool IsSeed { get; set; }
        }

        private static readonly DictionarySeedSource Seeds = new DictionarySeedSource(new Dictionary<string, string>
        {
            ["things"] = "[\"Uno\", \"Dos\", \"uno\"]"
        });

        private TempDatabase _temp;

        [SetUp]
        public void SetUp() => _temp = new TempDatabase();

        [TearDown]
        public void TearDown() => _temp.Dispose();

        [Test]
        public void BaseNueva_CreaElEsquemaYPoneLaVersion()
        {
            var schema = new FakeSchema();
            var db = _temp.Open(new IDbSchema[] { schema }, Seeds);
            Assert.AreEqual(1, schema.Creates);
            Assert.AreEqual(AppDatabase.CurrentVersion, db.UserVersion);
        }

        [Test]
        public void Seeds_SeCarganUnaVezEIgnoranDuplicadosSinMayusculas()
        {
            var schema = new FakeSchema();
            var db = _temp.Open(new IDbSchema[] { schema }, Seeds);
            Assert.AreEqual(2L, db.Scalar<long>("SELECT COUNT(*) FROM things"), "'uno' choca con 'Uno' (NOCASE)");

            _temp.Close();
            var again = new FakeSchema();
            db = _temp.Open(new IDbSchema[] { again }, Seeds);
            Assert.AreEqual(1, again.Creates, "Las tablas se aseguran en cada arranque (idempotente)");
            Assert.AreEqual(0, again.Seeds, "Con datos no se vuelve a sembrar");
            Assert.AreEqual(2L, db.Scalar<long>("SELECT COUNT(*) FROM things"), "Los datos siguen ahí");
        }

        /// <summary>Esquema de un juego añadido en una versión posterior de la app.</summary>
        private sealed class LateGameSchema : IDbSchema
        {
            public void OnCreate(IDb db) => db.Execute("CREATE TABLE IF NOT EXISTS late_things (id INTEGER PRIMARY KEY, name TEXT)");
            public void OnUpgrade(IDb db, int oldVersion, int newVersion) { }
            public void SeedIfEmpty(IDb db, ISeedSource seeds)
            {
                if (AppDatabase.IsEmpty(db, "late_things")) db.Execute("INSERT INTO late_things (name) VALUES ('nuevo')");
            }
        }

        [Test]
        public void JuegoNuevoEnUnaInstalacionExistente_CreaSusTablasSinSubirLaVersion()
        {
            _temp.Open(new IDbSchema[] { new FakeSchema() }, Seeds);
            _temp.Close();
            var db = _temp.Open(new IDbSchema[] { new FakeSchema(), new LateGameSchema() }, Seeds);
            Assert.AreEqual(1L, db.Scalar<long>("SELECT COUNT(*) FROM late_things"));
            Assert.AreEqual(2L, db.Scalar<long>("SELECT COUNT(*) FROM things"));
            Assert.AreEqual(AppDatabase.CurrentVersion, db.UserVersion);
        }

        [Test]
        public void VersionMasAlta_LlamaAOnUpgrade()
        {
            _temp.Open(new IDbSchema[] { new FakeSchema() }, Seeds, version: 1);
            _temp.Close();
            var schema = new FakeSchema();
            var db = _temp.Open(new IDbSchema[] { schema }, Seeds, version: 3);
            CollectionAssert.AreEqual(new[] { (1, 3) }, schema.Upgrades);
            Assert.AreEqual(3, db.UserVersion);
        }

        [Test]
        public void VersionDelArchivoMasNueva_Lanza()
        {
            _temp.Open(new IDbSchema[] { new FakeSchema() }, Seeds, version: 2);
            _temp.Close();
            Assert.Throws<InvalidOperationException>(() => _temp.Open(new IDbSchema[] { new FakeSchema() }, Seeds, version: 1));
        }

        [Test]
        public void FalloAlCrear_DeshaceLaTransaccion()
        {
            Assert.Throws<InvalidOperationException>(() => _temp.Open(new IDbSchema[] { new FakeSchema { ThrowOnCreate = true } }, Seeds));
            var db = _temp.OpenRaw();
            Assert.AreEqual(0, db.UserVersion);
            Assert.AreEqual(0L, db.Scalar<long>("SELECT COUNT(*) FROM sqlite_master WHERE name = 'things'"));
        }

        [Test]
        public void Query_MapeaColumnasConAlias()
        {
            var db = _temp.Open(new IDbSchema[] { new FakeSchema() }, Seeds);
            var rows = db.Query<ThingRow>("SELECT id AS Id, name AS Name, is_seed AS IsSeed FROM things ORDER BY name COLLATE NOCASE");
            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual("Dos", rows[0].Name);
            Assert.IsTrue(rows[0].IsSeed);
        }

        [Test]
        public void LastInsertRowId()
        {
            var db = _temp.Open(new IDbSchema[] { new FakeSchema() }, Seeds);
            db.Execute("INSERT INTO things (name) VALUES (?)", "Tres");
            Assert.AreEqual(db.Scalar<long>("SELECT id FROM things WHERE name = 'Tres'"), db.LastInsertRowId);
        }
    }
}
