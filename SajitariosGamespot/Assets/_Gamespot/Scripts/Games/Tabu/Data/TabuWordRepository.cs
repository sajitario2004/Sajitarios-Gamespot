using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Tabu.Domain;

namespace Gamespot.Games.Tabu.Data
{
    /// <summary>Tabla <c>tabu_words</c> (prohibidas en JSON) y su seed (<c>[{palabra, prohibidas[4..5]}]</c>, 120).</summary>
    public static class TabuSchema
    {
        public const string Table = "tabu_words";
        public const string SeedName = "tabu_words";

        public static void Create(IDb db) =>
            db.Execute("CREATE TABLE IF NOT EXISTS tabu_words (id INTEGER PRIMARY KEY AUTOINCREMENT, palabra TEXT NOT NULL, " +
                       "prohibidas_json TEXT NOT NULL, is_seed INTEGER NOT NULL DEFAULT 0)");

        public static int SeedIfEmpty(IDb db, ISeedSource seeds)
        {
            if (!AppDatabase.IsEmpty(db, Table)) return 0;
            var inserted = 0;
            foreach (var word in Parse(seeds.Load(SeedName)))
            {
                var json = new System.Text.StringBuilder("[");
                for (var i = 0; i < word.Prohibidas.Count; i++)
                {
                    if (i > 0) json.Append(',');
                    JsonWriter.AppendString(json, word.Prohibidas[i]);
                }
                inserted += db.Execute("INSERT INTO tabu_words (palabra, prohibidas_json, is_seed) VALUES (?, ?, 1)", word.Palabra, json.Append(']').ToString());
            }
            return inserted;
        }

        public static List<TabuWord> Parse(string json)
        {
            var result = new List<TabuWord>();
            foreach (var item in Json.ParseArray(json))
            {
                try
                {
                    var map = (Dictionary<string, object>)item;
                    var forbidden = ((List<object>)map["prohibidas"]).Select(o => (string)o).ToList();
                    result.Add(new TabuWord(0, (string)map["palabra"], forbidden));
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
        private sealed class Row
        {
            public long Id { get; set; }
            public string Palabra { get; set; }
            public string ProhibidasJson { get; set; }
        }

        private readonly IDb _db;

        public TabuWordRepository(IDb db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public IReadOnlyList<TabuWord> GetAll() =>
            _db.Query<Row>("SELECT id AS Id, palabra AS Palabra, prohibidas_json AS ProhibidasJson FROM tabu_words ORDER BY id ASC")
                .Select(r => new TabuWord(r.Id, r.Palabra, Json.ParseArray(r.ProhibidasJson).Select(o => (string)o).ToList()))
                .ToList();
    }
}
