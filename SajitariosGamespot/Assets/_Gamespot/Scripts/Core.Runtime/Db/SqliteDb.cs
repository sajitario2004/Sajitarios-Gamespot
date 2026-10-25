using System;
using System.Collections.Generic;
using SQLite;

namespace Gamespot.Core.Runtime.Db
{
    /// <summary><see cref="IDb"/> sobre una conexión de sqlite-net (gilzoide/unity-sqlite-net).</summary>
    public sealed class SqliteDb : IDb, IDisposable
    {
        private readonly SQLiteConnection _connection;

        private SqliteDb(SQLiteConnection connection)
        {
            _connection = connection;
        }

        /// <summary>Ruta del archivo abierto.</summary>
        public string Path => _connection.DatabasePath;

        /// <summary>Abre (o crea) el archivo y activa las claves foráneas, como el Flutter.</summary>
        public static SqliteDb Open(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Falta la ruta de la base de datos", nameof(path));
            var connection = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
            connection.Execute("PRAGMA foreign_keys = ON");
            return new SqliteDb(connection);
        }

        public int Execute(string sql, params object[] args) => _connection.Execute(sql, args);

        public T Scalar<T>(string sql, params object[] args) => _connection.ExecuteScalar<T>(sql, args);

        public List<T> Query<T>(string sql, params object[] args) where T : new() => _connection.Query<T>(sql, args);

        public void InTransaction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            // sqlite-net usa savepoints, así que también vale anidada.
            _connection.RunInTransaction(action);
        }

        public long LastInsertRowId => _connection.ExecuteScalar<long>("SELECT last_insert_rowid()");

        /// <summary>Versión del esquema guardada en el archivo (<c>PRAGMA user_version</c>).</summary>
        public int UserVersion
        {
            get => _connection.ExecuteScalar<int>("PRAGMA user_version");
            set => _connection.Execute($"PRAGMA user_version = {value}");
        }

        public void Dispose() => _connection.Dispose();
    }
}
