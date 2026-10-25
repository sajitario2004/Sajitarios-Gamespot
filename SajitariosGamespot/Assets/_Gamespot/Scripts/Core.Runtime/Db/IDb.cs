using System;
using System.Collections.Generic;

namespace Gamespot.Core.Runtime.Db
{
    /// <summary>
    /// Acceso a la base de datos SQLite de la app. Los repositorios de cada juego solo ven esta interfaz
    /// (no sqlite-net). Todas las consultas van parametrizadas con <c>?</c>.
    /// </summary>
    public interface IDb
    {
        /// <summary>Ejecuta una sentencia y devuelve las filas afectadas.</summary>
        int Execute(string sql, params object[] args);

        /// <summary>Primera columna de la primera fila.</summary>
        T Scalar<T>(string sql, params object[] args);

        /// <summary>
        /// Filas mapeadas a <typeparamref name="T"/> por nombre de columna (sin distinguir mayúsculas):
        /// en el SQL se usan alias, por ejemplo <c>SELECT is_seed AS IsSeed</c>.
        /// </summary>
        List<T> Query<T>(string sql, params object[] args) where T : new();

        /// <summary>Ejecuta <paramref name="action"/> en una transacción (se deshace si lanza).</summary>
        void InTransaction(Action action);

        /// <summary>Rowid de la última fila insertada en esta conexión.</summary>
        long LastInsertRowId { get; }
    }
}
