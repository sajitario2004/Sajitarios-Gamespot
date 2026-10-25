using System;
using System.Collections.Generic;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Shared;

namespace Gamespot.Games.Impostor.Data
{
    /// <summary>
    /// Tablas del Impostor (esquema final del Flutter + categoría): palabras con pista obligatoria, unicidad sin
    /// mayúsculas y categoría (<see cref="WordCategories"/>), categorías propias (V059l) e historial de partidas. Seed:
    /// <c>Resources/Seed/impostor_words.json</c> (<c>[{word, hint, category}]</c>).
    /// </summary>
    public static class ImpostorSchema
    {
        public const string WordsTable = "impostor_words";
        public const string HistoryTable = "game_history";
        public const string CategoriesTable = "impostor_categories";
        public const string SeedName = "impostor_words";

        /// <summary>
        /// Última versión de la BD en la que cambió el seed: 3 (+664 de fiesta y de España), 4 (categorías y fuera
        /// las palabras difíciles) y 5 (Cachondeo y vuelven las fáciles).
        /// </summary>
        public const int SeedRenewedVersion = 5;

        /// <summary>Si una BD de <paramref name="oldVersion"/> tiene que renovar las palabras del seed.</summary>
        public static bool NeedsSeedRefresh(int oldVersion) => oldVersion < SeedRenewedVersion;

        public static void Create(IDb db)
        {
            db.Execute("CREATE TABLE IF NOT EXISTS impostor_words (" +
                       "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                       "word TEXT NOT NULL UNIQUE COLLATE NOCASE, " +
                       "hint TEXT NOT NULL, " +
                       "is_seed INTEGER NOT NULL DEFAULT 0, " +
                       "created_at INTEGER NOT NULL, " +
                       "category TEXT NOT NULL DEFAULT 'propias')");
            // Las BD de antes de las categorías reciben la columna aquí; sus palabras propias quedan como "propias".
            AppDatabase.AddColumnIfMissing(db, WordsTable, "category", "TEXT NOT NULL DEFAULT 'propias'");
            db.Execute("CREATE INDEX IF NOT EXISTS idx_impostor_words_word ON impostor_words(word)");
            db.Execute("CREATE INDEX IF NOT EXISTS idx_impostor_words_category ON impostor_words(category)");
            CustomCategoryRepository.CreateTable(db, CategoriesTable);
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
        public static int SeedIfEmpty(IDb db, ISeedSource seeds) => AppDatabase.IsEmpty(db, WordsTable) ? InsertSeed(db, seeds) : 0;

        /// <summary>
        /// Al migrar: borra las palabras del seed (son de solo lectura: nadie las ha cambiado) y carga el seed
        /// actual, así salen las quitadas y entran las nuevas con su categoría. Las del usuario no se tocan: si
        /// alguien había creado una igual a una del seed, se queda la suya (UNIQUE NOCASE + OR IGNORE).
        /// </summary>
        public static int RefreshSeed(IDb db, ISeedSource seeds)
        {
            db.Execute("DELETE FROM impostor_words WHERE is_seed = 1");
            return InsertSeed(db, seeds);
        }

        private static int InsertSeed(IDb db, ISeedSource seeds)
        {
            var entries = Parse(seeds.Load(SeedName));
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var inserted = 0;
            foreach (var (word, hint, category) in entries)
            {
                inserted += db.Execute("INSERT OR IGNORE INTO impostor_words (word, hint, is_seed, created_at, category) VALUES (?, ?, 1, ?, ?)",
                    word, hint, now, category);
            }
            return inserted;
        }

        /// <summary>
        /// Lee y valida el seed: cada entrada con <c>word</c> y <c>hint</c> no vacíos (recortados) y, opcional,
        /// <c>category</c> (si falta, <see cref="WordCategories.General"/>).
        /// </summary>
        public static List<(string word, string hint, string category)> Parse(string json)
        {
            var result = new List<(string, string, string)>();
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
                var category = map.TryGetValue("category", out var c) && c is string text && text.Trim().Length > 0 ? text.Trim() : WordCategories.General;
                result.Add((word, hint, category));
            }
            return result;
        }
    }
}
