using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Impostor.Domain;

namespace Gamespot.Games.Impostor.Data
{
    /// <summary>Fila de <c>impostor_words</c>.</summary>
    public sealed class ImpostorWord
    {
        public long Id { get; set; }
        public string Word { get; set; }
        public string Hint { get; set; }
        public bool IsSeed { get; set; }
        public long CreatedAt { get; set; }
    }

    /// <summary>Ya existe una palabra igual (sin distinguir mayúsculas).</summary>
    public sealed class DuplicateWordException : Exception
    {
        public DuplicateWordException(string word) : base($"Ya existe la palabra '{word}'") { }
    }

    /// <summary>La palabra ya no existe (se borró).</summary>
    public sealed class WordNotFoundException : Exception
    {
        public WordNotFoundException(long id) : base($"No existe la palabra {id}") { }
    }

    /// <summary>Las palabras predefinidas (seed) son de solo lectura.</summary>
    public sealed class ReadOnlySeedWordException : Exception
    {
        public ReadOnlySeedWordException(long id) : base($"La palabra {id} es predefinida") { }
    }

    /// <summary>
    /// Palabras del Impostor (port de <c>WordRepository</c>): listado ordenado sin mayúsculas, búsqueda con
    /// <c>LIKE … ESCAPE '\'</c>, alta, edición y borrado de las del usuario. Las seed no se tocan.
    /// También es la fuente de palabras de la partida.
    /// </summary>
    public sealed class WordRepository : IImpostorWordSource
    {
        private const string Select = "SELECT id AS Id, word AS Word, hint AS Hint, is_seed AS IsSeed, created_at AS CreatedAt FROM impostor_words";
        private const string Order = " ORDER BY word COLLATE NOCASE ASC, id ASC";

        private readonly IDb _db;
        private readonly Func<long> _now;

        public WordRepository(IDb db, Func<long> nowMillis = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _now = nowMillis ?? (() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        public List<ImpostorWord> GetAll() => _db.Query<ImpostorWord>(Select + Order);

        /// <summary>Busca por texto contenido (recortado; vacío = todas). <c>%</c>, <c>_</c> y <c>\</c> se buscan literales.</summary>
        public List<ImpostorWord> Search(string query)
        {
            var q = query?.Trim() ?? string.Empty;
            if (q.Length == 0) return GetAll();
            var escaped = q.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            return _db.Query<ImpostorWord>(Select + " WHERE word LIKE ? ESCAPE '\\'" + Order, $"%{escaped}%");
        }

        public ImpostorWord GetById(long id) => _db.Query<ImpostorWord>(Select + " WHERE id = ?", id).FirstOrDefault();

        public long Count() => _db.Scalar<long>("SELECT COUNT(*) FROM impostor_words");

        /// <summary>Añade una palabra del usuario. Lanza <see cref="ArgumentException"/> o <see cref="DuplicateWordException"/>.</summary>
        public long Insert(string word, string hint)
        {
            var (w, h) = Clean(word, hint);
            if (Exists(w, null)) throw new DuplicateWordException(w);
            _db.Execute("INSERT INTO impostor_words (word, hint, is_seed, created_at) VALUES (?, ?, 0, ?)", w, h, _now());
            return _db.LastInsertRowId;
        }

        /// <summary>Edita una palabra del usuario. Errores en orden: vacía, no existe, es seed, duplicada.</summary>
        public void Update(long id, string word, string hint)
        {
            var (w, h) = Clean(word, hint);
            var current = GetById(id) ?? throw new WordNotFoundException(id);
            if (current.IsSeed) throw new ReadOnlySeedWordException(id);
            if (Exists(w, id)) throw new DuplicateWordException(w);
            _db.Execute("UPDATE impostor_words SET word = ?, hint = ? WHERE id = ? AND is_seed = 0", w, h, id);
        }

        /// <summary>Borra una palabra del usuario. Errores: no existe, es seed.</summary>
        public void Delete(long id)
        {
            var current = GetById(id) ?? throw new WordNotFoundException(id);
            if (current.IsSeed) throw new ReadOnlySeedWordException(id);
            _db.Execute("DELETE FROM impostor_words WHERE id = ? AND is_seed = 0", id);
        }

        /// <summary>Todas las palabras (seed y propias) como dominio, para la partida.</summary>
        public IReadOnlyList<Word> GetAllWords() => GetAll().Select(r => new Word(r.Word, r.Hint)).ToList();

        private bool Exists(string word, long? exceptId) =>
            exceptId.HasValue
                ? _db.Scalar<long>("SELECT COUNT(*) FROM impostor_words WHERE word = ? COLLATE NOCASE AND id <> ?", word, exceptId.Value) > 0
                : _db.Scalar<long>("SELECT COUNT(*) FROM impostor_words WHERE word = ? COLLATE NOCASE", word) > 0;

        private static (string word, string hint) Clean(string word, string hint)
        {
            var w = word?.Trim() ?? string.Empty;
            var h = hint?.Trim() ?? string.Empty;
            if (w.Length == 0 || h.Length == 0) throw new ArgumentException("La palabra y la pista son obligatorias");
            return (w, h);
        }
    }
}
