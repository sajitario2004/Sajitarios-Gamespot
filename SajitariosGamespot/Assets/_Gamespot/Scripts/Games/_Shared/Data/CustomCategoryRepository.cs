using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;

namespace Gamespot.Games.Shared
{
    /// <summary>Fila de una tabla de categorías propias.</summary>
    public sealed class CustomCategory
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long CreatedAt { get; set; }

        /// <summary>El id que se guarda en las palabras (<c>custom_N</c>).</summary>
        public string CategoryId => WordCategories.CustomId(Id);
    }

    public enum CategoryNameError
    {
        Vacio,
        Duplicado
    }

    /// <summary>Nombre de categoría vacío o repetido.</summary>
    public sealed class CategoryNameException : Exception
    {
        public CategoryNameException(CategoryNameError error) : base($"Nombre de categoría no válido: {error}") => Error = error;

        public CategoryNameError Error { get; }
    }

    /// <summary>
    /// Categorías que crea el usuario (V059l). Cada juego tiene su propia tabla (<c>impostor_categories</c>,
    /// <c>tabu_categories</c>) y la crea en su esquema con <see cref="CreateTable"/>: aquí solo está el código común.
    /// Los nombres no se repiten sin distinguir mayúsculas, ni entre ellos ni con las categorías de serie
    /// (<c>reservedNames</c>). Borrar una categoría no toca sus palabras: el juego las mueve antes a "Mis palabras".
    /// </summary>
    public sealed class CustomCategoryRepository
    {
        public const int MaxNameLength = 24;

        private readonly IDb _db;
        private readonly string _table;
        private readonly Func<long> _now;

        public CustomCategoryRepository(IDb db, string table, Func<long> nowMillis = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _table = table ?? throw new ArgumentNullException(nameof(table));
            _now = nowMillis ?? (() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        /// <summary>Crea la tabla si falta (el nombre es una constante del juego, nunca un texto del usuario).</summary>
        public static void CreateTable(IDb db, string table) =>
            db.Execute($"CREATE TABLE IF NOT EXISTS {table} (id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                       "name TEXT NOT NULL UNIQUE COLLATE NOCASE, created_at INTEGER NOT NULL)");

        /// <summary>Todas, por nombre.</summary>
        public List<CustomCategory> GetAll() =>
            _db.Query<CustomCategory>($"SELECT id AS Id, name AS Name, created_at AS CreatedAt FROM {_table} ORDER BY name COLLATE NOCASE ASC, id ASC");

        public CustomCategory GetById(long id) =>
            _db.Query<CustomCategory>($"SELECT id AS Id, name AS Name, created_at AS CreatedAt FROM {_table} WHERE id = ?", id).FirstOrDefault();

        /// <summary>Crea una categoría y devuelve su fila. Lanza <see cref="CategoryNameException"/>.</summary>
        public CustomCategory Insert(string name, IEnumerable<string> reservedNames = null)
        {
            var clean = Validate(name, null, reservedNames);
            _db.Execute($"INSERT INTO {_table} (name, created_at) VALUES (?, ?)", clean, _now());
            return GetById(_db.LastInsertRowId);
        }

        /// <summary>Cambia el nombre (puede quedarse igual con otras mayúsculas).</summary>
        public void Rename(long id, string name, IEnumerable<string> reservedNames = null)
        {
            var clean = Validate(name, id, reservedNames);
            _db.Execute($"UPDATE {_table} SET name = ? WHERE id = ?", clean, id);
        }

        public void Delete(long id) => _db.Execute($"DELETE FROM {_table} WHERE id = ?", id);

        /// <summary>Nombre recortado, con los espacios colapsados y como mucho <see cref="MaxNameLength"/> caracteres.</summary>
        public static string Clean(string name)
        {
            var clean = NameNormalizer.Normalize(name);
            return clean.Length > MaxNameLength ? clean.Substring(0, MaxNameLength).TrimEnd() : clean;
        }

        private string Validate(string name, long? exceptId, IEnumerable<string> reservedNames)
        {
            var clean = Clean(name);
            if (clean.Length == 0) throw new CategoryNameException(CategoryNameError.Vacio);
            // En C# y no con NOCASE: SQLite solo iguala mayúsculas ASCII y "Ñoños" tiene que chocar con "ñoños".
            var taken = GetAll().Where(c => c.Id != exceptId).Select(c => c.Name).Concat(reservedNames ?? Array.Empty<string>());
            if (taken.Any(other => string.Equals(NameNormalizer.Normalize(other), clean, StringComparison.OrdinalIgnoreCase)))
            {
                throw new CategoryNameException(CategoryNameError.Duplicado);
            }
            return clean;
        }
    }
}
