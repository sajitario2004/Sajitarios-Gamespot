using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Bomba.Domain;

namespace Gamespot.Games.Bomba.Data
{
    /// <summary>
    /// Tablas <c>bomba_silabas</c> y <c>bomba_categorias</c> y sus seeds (listas planas de textos: 93 sílabas y
    /// 83 categorías). Cada tabla se siembra solo si está vacía.
    /// </summary>
    public static class BombaSchema
    {
        public const string SilabasTable = "bomba_silabas";
        public const string CategoriasTable = "bomba_categorias";
        public const string SilabasSeed = "bomba_silabas";
        public const string CategoriasSeed = "bomba_categorias";

        public static void Create(IDb db)
        {
            db.Execute("CREATE TABLE IF NOT EXISTS bomba_silabas (id INTEGER PRIMARY KEY AUTOINCREMENT, silaba TEXT NOT NULL, " +
                       "is_seed INTEGER NOT NULL DEFAULT 0)");
            db.Execute("CREATE TABLE IF NOT EXISTS bomba_categorias (id INTEGER PRIMARY KEY AUTOINCREMENT, categoria TEXT NOT NULL, " +
                       "is_seed INTEGER NOT NULL DEFAULT 0)");
        }

        public static int SeedIfEmpty(IDb db, ISeedSource seeds) =>
            SeedTable(db, SilabasTable, "silaba", seeds.Load(SilabasSeed)) + SeedTable(db, CategoriasTable, "categoria", seeds.Load(CategoriasSeed));

        /// <summary>Lista de textos no vacíos, recortados. Lanza si algún elemento no es texto o está vacío.</summary>
        public static List<string> Parse(string json)
        {
            var result = new List<string>();
            var items = Json.ParseArray(json);
            for (var i = 0; i < items.Count; i++)
            {
                if (!(items[i] is string text) || text.Trim().Length == 0)
                {
                    throw new FormatException($"El elemento {i} del seed de La Bomba no es un texto no vacío");
                }
                result.Add(text.Trim());
            }
            return result;
        }

        private static int SeedTable(IDb db, string table, string column, string json)
        {
            if (!AppDatabase.IsEmpty(db, table)) return 0;
            var inserted = 0;
            foreach (var text in Parse(json))
            {
                inserted += db.Execute($"INSERT INTO {table} ({column}, is_seed) VALUES (?, 1)", text);
            }
            return inserted;
        }
    }

    /// <summary>Sílabas y categorías de La Bomba, por id.</summary>
    public sealed class BombaPromptRepository : IBombaPromptSource
    {
        private sealed class Row
        {
            public long Id { get; set; }
            public string Texto { get; set; }
        }

        private readonly IDb _db;

        public BombaPromptRepository(IDb db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public IReadOnlyList<BombaPrompt> GetAllSilabas() =>
            _db.Query<Row>("SELECT id AS Id, silaba AS Texto FROM bomba_silabas ORDER BY id ASC")
                .Select(r => new BombaPrompt(r.Id, r.Texto, BombaMode.Silaba)).ToList();

        public IReadOnlyList<BombaPrompt> GetAllCategorias() =>
            _db.Query<Row>("SELECT id AS Id, categoria AS Texto FROM bomba_categorias ORDER BY id ASC")
                .Select(r => new BombaPrompt(r.Id, r.Texto, BombaMode.Categoria)).ToList();
    }
}
