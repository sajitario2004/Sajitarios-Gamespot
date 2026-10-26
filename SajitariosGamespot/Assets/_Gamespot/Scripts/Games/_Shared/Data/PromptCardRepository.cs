using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;

namespace Gamespot.Games.Shared
{
    /// <summary>Fila de una tabla de cartas.</summary>
    public sealed class PromptCardRow
    {
        public long Id { get; set; }
        public string Texto { get; set; }
        public string TextoB { get; set; }
        public string Tipo { get; set; }
        public string Nivel { get; set; }
        public string Categoria { get; set; }
        public bool IsSeed { get; set; }

        public PromptLevel Level => PromptLevels.FromStorageName(Nivel);

        public PromptCard ToCard() => new PromptCard(Id, Texto, Level, Tipo, Categoria, TextoB, !IsSeed);
    }

    public enum PromptCardProblem
    {
        Vacia,
        FaltaSegunda,
        Duplicada,
        NoExiste,
        Predefinida
    }

    /// <summary>No se pudo guardar o borrar una carta propia.</summary>
    public sealed class PromptCardException : Exception
    {
        public PromptCardException(PromptCardProblem problem) : base($"Carta no válida: {problem}") => Problem = problem;

        public PromptCardProblem Problem { get; }
    }

    /// <summary>Filtro de la gestión de cartas. Nulo o vacío = sin filtrar por eso.</summary>
    public sealed class PromptCardFilter
    {
        public bool OwnOnly { get; set; }
        public PromptLevel? Level { get; set; }
        public string Kind { get; set; }
        public string Category { get; set; }
    }

    /// <summary>
    /// Cartas de un juego (V070): una tabla por juego (<c>verdad_reto_cards</c>, <c>mimica_cards</c>…) que el juego crea
    /// en su esquema con <see cref="CreateTable"/>; aquí solo está el código común. Sirve las cartas de la partida
    /// (<see cref="IPromptSource"/>) y gestiona las propias: búsqueda, alta, edición y borrado, sin repetir otra carta
    /// (sin distinguir mayúsculas ni el punto final). Las del seed son de solo lectura.
    /// Seed: <c>[{texto, texto_b?, tipo?, nivel?, categoria?}]</c>.
    /// </summary>
    public sealed class PromptCardRepository : IPromptSource
    {
        private readonly IDb _db;
        private readonly string _table;
        private readonly string _select;

        public PromptCardRepository(IDb db, string table)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _table = table ?? throw new ArgumentNullException(nameof(table));
            _select = $"SELECT id AS Id, texto AS Texto, texto_b AS TextoB, tipo AS Tipo, nivel AS Nivel, categoria AS Categoria, is_seed AS IsSeed FROM {_table}";
        }

        /// <summary>Crea la tabla si falta (el nombre es una constante del juego).</summary>
        public static void CreateTable(IDb db, string table)
        {
            db.Execute($"CREATE TABLE IF NOT EXISTS {table} (id INTEGER PRIMARY KEY AUTOINCREMENT, texto TEXT NOT NULL, texto_b TEXT, " +
                       "tipo TEXT NOT NULL DEFAULT '', nivel TEXT NOT NULL DEFAULT 'suave', categoria TEXT NOT NULL DEFAULT '', " +
                       "is_seed INTEGER NOT NULL DEFAULT 0)");
            db.Execute($"CREATE INDEX IF NOT EXISTS idx_{table}_tipo_nivel ON {table}(tipo, nivel)");
        }

        /// <summary>Lee y valida el seed (lanza <see cref="FormatException"/> con entradas mal formadas).</summary>
        public static List<PromptCard> ParseSeed(string json)
        {
            var result = new List<PromptCard>();
            foreach (var item in Json.ParseArray(json))
            {
                if (!(item is Dictionary<string, object> map) || !(map.TryGetValue("texto", out var t) && t is string texto))
                {
                    throw new FormatException("Carta del seed sin 'texto'");
                }
                string Field(string key) => map.TryGetValue(key, out var v) && v is string s ? s : null;
                result.Add(new PromptCard(0, texto, PromptLevels.FromStorageName(Field("nivel")), Field("tipo"), Field("categoria"), Field("texto_b")));
            }
            return result;
        }

        /// <summary>Carga el seed si no hay cartas de seed (las propias no lo impiden). Devuelve cuántas entraron.</summary>
        public static int SeedIfEmpty(IDb db, string table, IEnumerable<PromptCard> cards)
        {
            if (db.Scalar<long>($"SELECT COUNT(*) FROM {table} WHERE is_seed = 1") > 0) return 0;
            var inserted = 0;
            foreach (var c in cards)
            {
                inserted += db.Execute($"INSERT INTO {table} (texto, texto_b, tipo, nivel, categoria, is_seed) VALUES (?, ?, ?, ?, ?, 1)",
                    c.Text, c.TextB, c.Kind, c.Level.StorageName(), c.Category);
            }
            return inserted;
        }

        /// <summary>Borra las cartas de seed (para renovarlas al migrar); las propias se quedan.</summary>
        public static void DeleteSeed(IDb db, string table) => db.Execute($"DELETE FROM {table} WHERE is_seed = 1");

        public IReadOnlyList<PromptCard> GetCards(PromptQuery query)
        {
            query = query ?? new PromptQuery();
            return _db.Query<PromptCardRow>(_select + " ORDER BY id ASC").Select(r => r.ToCard()).Where(query.Matches).ToList();
        }

        public long CountOwn() => _db.Scalar<long>($"SELECT COUNT(*) FROM {_table} WHERE is_seed = 0");

        /// <summary>Cartas por categoría (solo las que tienen alguna).</summary>
        public Dictionary<string, long> CountByCategory() =>
            _db.Query<PromptCardRow>($"SELECT categoria AS Categoria FROM {_table}").GroupBy(r => r.Categoria ?? string.Empty)
                .ToDictionary(g => g.Key, g => (long)g.Count());

        /// <summary>Cartas que contienen <paramref name="text"/> (en cualquiera de los dos textos) y pasan el filtro: las propias primero.</summary>
        public List<PromptCardRow> Search(string text = null, PromptCardFilter filter = null)
        {
            var q = text?.Trim() ?? string.Empty;
            var filters = new List<string>();
            var args = new List<object>();
            if (q.Length > 0)
            {
                var like = $"%{q.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")}%";
                filters.Add("(texto LIKE ? ESCAPE '\\' OR texto_b LIKE ? ESCAPE '\\')");
                args.Add(like);
                args.Add(like);
            }
            if (filter != null)
            {
                if (filter.OwnOnly) filters.Add("is_seed = 0");
                if (filter.Level.HasValue)
                {
                    filters.Add("nivel = ?");
                    args.Add(filter.Level.Value.StorageName());
                }
                if (!string.IsNullOrEmpty(filter.Kind))
                {
                    filters.Add("tipo = ?");
                    args.Add(filter.Kind);
                }
                if (!string.IsNullOrEmpty(filter.Category))
                {
                    filters.Add("categoria = ?");
                    args.Add(filter.Category);
                }
            }
            var where = filters.Count == 0 ? string.Empty : " WHERE " + string.Join(" AND ", filters);
            return _db.Query<PromptCardRow>(_select + where + " ORDER BY is_seed ASC, id DESC", args.ToArray());
        }

        public PromptCardRow GetById(long id) => _db.Query<PromptCardRow>(_select + " WHERE id = ?", id).FirstOrDefault();

        /// <summary>Añade una carta propia. <paramref name="requireB"/>: el juego necesita segunda opción.</summary>
        public long Insert(PromptCard card, bool requireB = false)
        {
            Check(card, null, requireB);
            _db.Execute($"INSERT INTO {_table} (texto, texto_b, tipo, nivel, categoria, is_seed) VALUES (?, ?, ?, ?, ?, 0)",
                card.Text, card.TextB, card.Kind, card.Level.StorageName(), card.Category);
            return _db.LastInsertRowId;
        }

        public void Update(long id, PromptCard card, bool requireB = false)
        {
            var current = GetById(id) ?? throw new PromptCardException(PromptCardProblem.NoExiste);
            if (current.IsSeed) throw new PromptCardException(PromptCardProblem.Predefinida);
            Check(card, id, requireB);
            _db.Execute($"UPDATE {_table} SET texto = ?, texto_b = ?, tipo = ?, nivel = ?, categoria = ? WHERE id = ? AND is_seed = 0",
                card.Text, card.TextB, card.Kind, card.Level.StorageName(), card.Category, id);
        }

        public void Delete(long id)
        {
            var current = GetById(id) ?? throw new PromptCardException(PromptCardProblem.NoExiste);
            if (current.IsSeed) throw new PromptCardException(PromptCardProblem.Predefinida);
            _db.Execute($"DELETE FROM {_table} WHERE id = ? AND is_seed = 0", id);
        }

        /// <summary>Pasa las cartas propias de una categoría a otra (al borrar una categoría propia).</summary>
        public int MoveCategory(string from, string to) =>
            _db.Execute($"UPDATE {_table} SET categoria = ? WHERE categoria = ? AND is_seed = 0", to ?? string.Empty, from);

        /// <summary>
        /// Texto obligatorio (y segunda opción si el juego la pide), sin repetir otra carta del mismo tipo. En C# y no
        /// con NOCASE: SQLite solo iguala mayúsculas ASCII.
        /// </summary>
        private void Check(PromptCard card, long? exceptId, bool requireB)
        {
            if (card == null || card.Text.Length == 0) throw new PromptCardException(PromptCardProblem.Vacia);
            if (requireB && string.IsNullOrEmpty(card.TextB)) throw new PromptCardException(PromptCardProblem.FaltaSegunda);
            var key = Key(card.Text, card.TextB, card.Kind);
            var taken = _db.Query<PromptCardRow>($"SELECT id AS Id, texto AS Texto, texto_b AS TextoB, tipo AS Tipo FROM {_table}")
                .Any(r => r.Id != exceptId && Key(r.Texto, r.TextoB, r.Tipo) == key);
            if (taken) throw new PromptCardException(PromptCardProblem.Duplicada);
        }

        private static string Key(string text, string textB, string kind) =>
            Clean(text) + "|" + Clean(textB) + "|" + (kind ?? string.Empty);

        private static string Clean(string text) => NameNormalizer.DuplicateKey(text ?? string.Empty).TrimEnd('.', '!', '?', '…', ' ');
    }
}
