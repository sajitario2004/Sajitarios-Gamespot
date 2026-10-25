using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Shared;
using Gamespot.Games.Tabu.Domain;

namespace Gamespot.Games.Tabu.Data
{
    /// <summary>
    /// Tabla <c>tabu_words</c> (prohibidas en JSON y categoría, <see cref="WordCategories"/>), las categorías propias
    /// (<c>tabu_categories</c>, V059l) y el seed (<c>[{palabra, prohibidas[4..5], categoria}]</c>, 1161).
    /// </summary>
    public static class TabuSchema
    {
        public const string Table = "tabu_words";
        public const string CategoriesTable = "tabu_categories";
        public const string SeedName = "tabu_words";

        /// <summary>
        /// Última versión de la BD en la que se renovó el seed: 2 (las 120 heredadas, sin tildes, por 607), 3 (1076,
        /// con famosos, sitios, fiesta y cosas de España), 4 (categorías y fuera las difíciles) y 5 (Cachondeo).
        /// </summary>
        public const int SeedRenewedVersion = 5;

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
            CustomCategoryRepository.CreateTable(db, CategoriesTable);
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

    /// <summary>Una tarjeta tal como está en la BD (para la gestión de tarjetas).</summary>
    public sealed class TabuCard
    {
        public long Id { get; set; }
        public string Palabra { get; set; }
        public IReadOnlyList<string> Prohibidas { get; set; }
        public bool IsSeed { get; set; }
        public string Categoria { get; set; }
    }

    public enum TabuCardProblem
    {
        Invalida,
        Duplicada,
        NoExiste,
        Predefinida
    }

    /// <summary>No se pudo guardar o borrar una tarjeta propia (con el motivo de validación si no era válida).</summary>
    public sealed class TabuCardException : Exception
    {
        public TabuCardException(TabuCardProblem problem, TabuCardError? validation = null) : base($"Tarjeta no válida: {problem} {validation}")
        {
            Problem = problem;
            Validation = validation;
        }

        public TabuCardProblem Problem { get; }
        public TabuCardError? Validation { get; }
    }

    /// <summary>
    /// Tarjetas del Tabú: la fuente de la partida (por id o por categorías) y, desde V059l, la gestión de las propias:
    /// búsqueda (también por categoría), alta, edición y borrado, con las reglas de <see cref="TabuCardRules"/> y sin
    /// repetir la palabra de otra tarjeta. Las del seed son de solo lectura.
    /// </summary>
    public sealed class TabuWordRepository : ITabuWordSource
    {
        private const string Select = "SELECT id AS Id, palabra AS Palabra, prohibidas_json AS ProhibidasJson, is_seed AS IsSeed, " +
                                      "categoria AS Categoria FROM tabu_words";
        private const string ByPalabra = " ORDER BY palabra COLLATE NOCASE ASC, id ASC";

        private sealed class Row
        {
            public long Id { get; set; }
            public string Palabra { get; set; }
            public string ProhibidasJson { get; set; }
            public bool IsSeed { get; set; }
            public string Categoria { get; set; }
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

        /// <summary>Todas las tarjetas ordenadas por palabra, o las que contienen <paramref name="query"/> y/o son de <paramref name="categoria"/>.</summary>
        public List<TabuCard> Search(string query = null, string categoria = null)
        {
            var q = query?.Trim() ?? string.Empty;
            var filters = new List<string>();
            var args = new List<object>();
            if (q.Length > 0)
            {
                filters.Add("palabra LIKE ? ESCAPE '\\'");
                args.Add($"%{q.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")}%");
            }
            if (!string.IsNullOrEmpty(categoria))
            {
                filters.Add("categoria = ?");
                args.Add(categoria);
            }
            var where = filters.Count == 0 ? string.Empty : " WHERE " + string.Join(" AND ", filters);
            return _db.Query<Row>(Select + where + ByPalabra, args.ToArray()).Select(ToCard).ToList();
        }

        public TabuCard GetById(long id) => _db.Query<Row>(Select + " WHERE id = ?", id).Select(ToCard).FirstOrDefault();

        /// <summary>Añade una tarjeta propia en <paramref name="categoria"/> (vacía = "Mis palabras"). Lanza <see cref="TabuCardException"/>.</summary>
        public long Insert(string palabra, IEnumerable<string> prohibidas, string categoria = WordCategories.Propias)
        {
            var (p, forbidden) = Clean(palabra, prohibidas);
            if (Exists(p, null)) throw new TabuCardException(TabuCardProblem.Duplicada);
            _db.Execute("INSERT INTO tabu_words (palabra, prohibidas_json, is_seed, categoria) VALUES (?, ?, 0, ?)", p,
                JsonWriter.WriteStringArray(forbidden), CleanCategory(categoria));
            return _db.LastInsertRowId;
        }

        /// <summary>Edita una tarjeta propia (<paramref name="categoria"/> nula = la que tenía). Errores: no válida, no existe, seed, duplicada.</summary>
        public void Update(long id, string palabra, IEnumerable<string> prohibidas, string categoria = null)
        {
            var (p, forbidden) = Clean(palabra, prohibidas);
            var current = GetById(id) ?? throw new TabuCardException(TabuCardProblem.NoExiste);
            if (current.IsSeed) throw new TabuCardException(TabuCardProblem.Predefinida);
            if (Exists(p, id)) throw new TabuCardException(TabuCardProblem.Duplicada);
            _db.Execute("UPDATE tabu_words SET palabra = ?, prohibidas_json = ?, categoria = ? WHERE id = ? AND is_seed = 0", p,
                JsonWriter.WriteStringArray(forbidden), categoria == null ? current.Categoria : CleanCategory(categoria), id);
        }

        /// <summary>Borra una tarjeta propia. Errores: no existe, seed.</summary>
        public void Delete(long id)
        {
            var current = GetById(id) ?? throw new TabuCardException(TabuCardProblem.NoExiste);
            if (current.IsSeed) throw new TabuCardException(TabuCardProblem.Predefinida);
            _db.Execute("DELETE FROM tabu_words WHERE id = ? AND is_seed = 0", id);
        }

        /// <summary>Pasa las tarjetas propias de una categoría a otra (al borrar una categoría propia). Devuelve cuántas.</summary>
        public int MoveCategory(string from, string to) =>
            _db.Execute("UPDATE tabu_words SET categoria = ? WHERE categoria = ? AND is_seed = 0", CleanCategory(to), from);

        private static (string palabra, List<string> prohibidas) Clean(string palabra, IEnumerable<string> prohibidas)
        {
            var error = TabuCardRules.Validate(palabra, prohibidas, out var p, out var forbidden);
            if (error != null) throw new TabuCardException(TabuCardProblem.Invalida, error);
            return (p, forbidden);
        }

        private static string CleanCategory(string categoria) => string.IsNullOrWhiteSpace(categoria) ? WordCategories.Propias : categoria.Trim();

        /// <summary>
        /// Si otra tarjeta ya tiene esa palabra. En C# y no con NOCASE: SQLite solo iguala mayúsculas ASCII
        /// ("Ñandú" = "ñandú").
        /// </summary>
        private bool Exists(string palabra, long? exceptId)
        {
            var key = NameNormalizer.DuplicateKey(palabra);
            return _db.Query<Row>("SELECT id AS Id, palabra AS Palabra FROM tabu_words")
                .Any(r => r.Id != exceptId && NameNormalizer.DuplicateKey(r.Palabra) == key);
        }

        private static TabuCard ToCard(Row r) => new TabuCard
        {
            Id = r.Id,
            Palabra = r.Palabra,
            Prohibidas = Json.ParseArray(r.ProhibidasJson).Select(o => (string)o).ToList(),
            IsSeed = r.IsSeed,
            Categoria = r.Categoria
        };

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
