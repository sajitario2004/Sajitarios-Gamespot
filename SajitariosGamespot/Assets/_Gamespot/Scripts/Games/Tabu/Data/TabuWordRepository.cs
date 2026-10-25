using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Tabu.Domain;

namespace Gamespot.Games.Tabu.Data
{
    /// <summary>
    /// Tabla <c>tabu_words</c> (prohibidas en JSON y categoría, <see cref="WordCategories"/>) y su seed
    /// (<c>[{palabra, prohibidas[4..5], categoria}]</c>, 1065).
    /// </summary>
    public static class TabuSchema
    {
        public const string Table = "tabu_words";
        public const string SeedName = "tabu_words";

        /// <summary>
        /// Última versión de la BD en la que se renovó el seed: 2 (las 120 heredadas, sin tildes, por 607), 3 (1076,
        /// con famosos, sitios, fiesta y cosas de España) y 4 (categorías y fuera las difíciles).
        /// </summary>
        public const int SeedRenewedVersion = 4;

        /// <summary>
        /// Al migrar desde antes de <see cref="SeedRenewedVersion"/> se borran las filas de seed; el arranque las
        /// vuelve a sembrar con el banco nuevo (<see cref="SeedIfEmpty"/> mira solo las de seed, así que las
        /// palabras propias, si algún día las hay, se conservan y no lo impiden).
        /// </summary>
        public static void Upgrade(IDb db, int oldVersion)
        {
            if (oldVersion < SeedRenewedVersion) db.Execute("DELETE FROM tabu_words WHERE is_seed = 1");
        }

        public static void Create(IDb db)
        {
            db.Execute("CREATE TABLE IF NOT EXISTS tabu_words (id INTEGER PRIMARY KEY AUTOINCREMENT, palabra TEXT NOT NULL, " +
                       "prohibidas_json TEXT NOT NULL, is_seed INTEGER NOT NULL DEFAULT 0, categoria TEXT NOT NULL DEFAULT 'general')");
            AppDatabase.AddColumnIfMissing(db, Table, "categoria", "TEXT NOT NULL DEFAULT 'general'");
            db.Execute("CREATE INDEX IF NOT EXISTS idx_tabu_words_categoria ON tabu_words(categoria)");
        }

        public static int SeedIfEmpty(IDb db, ISeedSource seeds)
        {
            if (db.Scalar<long>("SELECT COUNT(*) FROM tabu_words WHERE is_seed = 1") > 0) return 0;
            var inserted = 0;
            foreach (var (word, categoria) in ParseEntries(seeds.Load(SeedName)))
            {
                inserted += db.Execute("INSERT INTO tabu_words (palabra, prohibidas_json, is_seed, categoria) VALUES (?, ?, 1, ?)", word.Palabra,
                    JsonWriter.WriteStringArray(word.Prohibidas), categoria);
            }
            return inserted;
        }

        public static List<TabuWord> Parse(string json) => ParseEntries(json).Select(e => e.word).ToList();

        /// <summary>Tarjetas con su categoría (si falta, <see cref="WordCategories.General"/>).</summary>
        public static List<(TabuWord word, string categoria)> ParseEntries(string json)
        {
            var result = new List<(TabuWord, string)>();
            foreach (var item in Json.ParseArray(json))
            {
                try
                {
                    var map = (Dictionary<string, object>)item;
                    var forbidden = ((List<object>)map["prohibidas"]).Select(o => (string)o).ToList();
                    var categoria = map.TryGetValue("categoria", out var c) && c is string text && text.Trim().Length > 0 ? text.Trim() : WordCategories.General;
                    result.Add((new TabuWord(0, (string)map["palabra"], forbidden), categoria));
                }
                catch (Exception e)
                {
                    throw new FormatException($"Palabra de Tabú no válida: {e.Message}");
                }
            }
            return result;
        }
    }

    /// <summary>Palabras del Tabú, por id.</summary>
    public sealed class TabuWordRepository : ITabuWordSource
    {
        private const string Select = "SELECT id AS Id, palabra AS Palabra, prohibidas_json AS ProhibidasJson FROM tabu_words";

        private sealed class Row
        {
            public long Id { get; set; }
            public string Palabra { get; set; }
            public string ProhibidasJson { get; set; }
        }

        private sealed class CategoryCount
        {
            public string Categoria { get; set; }
            public long Total { get; set; }
        }

        private readonly IDb _db;

        public TabuWordRepository(IDb db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public IReadOnlyList<TabuWord> GetAll() => ToWords(_db.Query<Row>(Select + " ORDER BY id ASC"));

        /// <summary>Las tarjetas de esas categorías.</summary>
        public IReadOnlyList<TabuWord> GetByCategorias(IReadOnlyCollection<string> categorias)
        {
            if (categorias == null || categorias.Count == 0) return Array.Empty<TabuWord>();
            var args = categorias.Cast<object>().ToArray();
            var placeholders = string.Join(", ", args.Select(_ => "?"));
            return ToWords(_db.Query<Row>(Select + $" WHERE categoria IN ({placeholders}) ORDER BY id ASC", args));
        }

        /// <summary>Cuántas tarjetas hay en cada categoría (solo las que tienen alguna).</summary>
        public Dictionary<string, long> CountByCategoria() =>
            _db.Query<CategoryCount>("SELECT categoria AS Categoria, COUNT(*) AS Total FROM tabu_words GROUP BY categoria")
                .ToDictionary(c => c.Categoria, c => c.Total);

        private static IReadOnlyList<TabuWord> ToWords(IEnumerable<Row> rows) =>
            rows.Select(r => new TabuWord(r.Id, r.Palabra, Json.ParseArray(r.ProhibidasJson).Select(o => (string)o).ToList())).ToList();
    }

    /// <summary>
    /// Fuente de tarjetas de la partida limitada a las categorías marcadas en el setup (V059i); nulo = todas. El
    /// dominio sigue recibiendo una lista de tarjetas.
    /// </summary>
    public sealed class CategoryTabuWordSource : ITabuWordSource
    {
        private readonly TabuWordRepository _words;

        public CategoryTabuWordSource(TabuWordRepository words) => _words = words;

        public IReadOnlyCollection<string> Categorias { get; set; }

        public IReadOnlyList<TabuWord> GetAll() => Categorias == null ? _words.GetAll() : _words.GetByCategorias(Categorias);
    }
}
