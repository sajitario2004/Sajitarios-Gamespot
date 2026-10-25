using System;
using System.Collections.Generic;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;

namespace Gamespot.Games.Impostor.Data
{
    /// <summary>
    /// Tablas del Impostor (esquema final del Flutter, v2 + v3): palabras con pista obligatoria y unicidad sin
    /// mayúsculas, e historial de partidas. Seed: <c>Resources/Seed/impostor_words.json</c> (<c>[{word, hint}]</c>).
    /// </summary>
    public static class ImpostorSchema
    {
        public const string WordsTable = "impostor_words";
        public const string HistoryTable = "game_history";
        public const string SeedName = "impostor_words";

        public static void Create(IDb db)
        {
            db.Execute("CREATE TABLE IF NOT EXISTS impostor_words (" +
                       "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                       "word TEXT NOT NULL UNIQUE COLLATE NOCASE, " +
                       "hint TEXT NOT NULL, " +
                       "is_seed INTEGER NOT NULL DEFAULT 0, " +
                       "created_at INTEGER NOT NULL)");
            db.Execute("CREATE INDEX IF NOT EXISTS idx_impostor_words_word ON impostor_words(word)");
            db.Execute("CREATE TABLE IF NOT EXISTS game_history (" +
                       "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                       "created_at INTEGER NOT NULL, " +
                       "word TEXT NOT NULL, " +
                       "hint TEXT, " +
                       "n_players INTEGER NOT NULL, " +
                       "n_impostors INTEGER NOT NULL, " +
                       "hint_enabled INTEGER NOT NULL, " +
                       "players_json TEXT NOT NULL)");
            db.Execute("CREATE INDEX IF NOT EXISTS idx_game_history_created_at ON game_history(created_at)");
        }

        /// <summary>Carga las palabras iniciales si la tabla está vacía. Devuelve cuántas filas entraron.</summary>
        public static int SeedIfEmpty(IDb db, ISeedSource seeds)
        {
            if (!AppDatabase.IsEmpty(db, WordsTable)) return 0;
            var entries = Parse(seeds.Load(SeedName));
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var inserted = 0;
            foreach (var (word, hint) in entries)
            {
                inserted += db.Execute("INSERT OR IGNORE INTO impostor_words (word, hint, is_seed, created_at) VALUES (?, ?, 1, ?)", word, hint, now);
            }
            return inserted;
        }

        /// <summary>Lee y valida el seed: cada entrada con <c>word</c> y <c>hint</c> no vacíos (recortados).</summary>
        public static List<(string word, string hint)> Parse(string json)
        {
            var result = new List<(string, string)>();
            foreach (var item in Json.ParseArray(json))
            {
                if (!(item is Dictionary<string, object> map)
                    || !(map.TryGetValue("word", out var w) && w is string word)
                    || !(map.TryGetValue("hint", out var h) && h is string hint))
                {
                    throw new FormatException("Entrada del seed del Impostor sin 'word' o 'hint'");
                }
                word = word.Trim();
                hint = hint.Trim();
                if (word.Length == 0 || hint.Length == 0) throw new FormatException("Palabra o pista vacía en el seed del Impostor");
                result.Add((word, hint));
            }
            return result;
        }
    }
}
