using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gamespot.Core.Runtime.Db
{
    /// <summary>
    /// Abre la base de datos de la app y la deja lista: crea o migra el esquema de todos los juegos
    /// (en el orden del registro) y carga sus seeds si las tablas están vacías.
    /// </summary>
    public static class AppDatabase
    {
        public const string FileName = "sajitarios_gamespot.db";

        /// <summary>
        /// Versión del esquema en Unity. Empezó en 1 con el esquema final (v8) de la app Flutter. Cada cambio de
        /// esquema o de seed que haya que llevar a instalaciones existentes sube este número:
        /// 2 · el Tabú renueva su seed (607 palabras con tildes, V059e).
        /// 3 · palabras de fiesta y de España: el Impostor añade las nuevas a su seed y el Tabú lo renueva (1076).
        /// 4 · categorías y fuera las palabras difíciles: el Impostor y el Tabú renuevan su seed (columna de categoría).
        /// </summary>
        public const int CurrentVersion = 4;

        /// <summary>Ruta del archivo en el dispositivo.</summary>
        public static string DefaultPath => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        /// <summary>Abre, crea o migra el esquema y carga los seeds.</summary>
        public static SqliteDb Open(string path, IReadOnlyList<IDbSchema> schemas, ISeedSource seeds, int version = CurrentVersion)
        {
            if (schemas == null) throw new ArgumentNullException(nameof(schemas));
            if (version < 1) throw new ArgumentOutOfRangeException(nameof(version));

            var db = SqliteDb.Open(path);
            try
            {
                var stored = db.UserVersion;
                if (stored > version)
                {
                    throw new InvalidOperationException($"La base de datos es de una versión más nueva ({stored}) que la app ({version})");
                }
                db.InTransaction(() =>
                {
                    // Las tablas de cada juego se aseguran en cada arranque (CREATE ... IF NOT EXISTS): un juego
                    // nuevo trae sus tablas sin subir la versión. La versión solo sirve para cambiar tablas que ya
                    // existían (OnUpgrade).
                    foreach (var schema in schemas) schema.OnCreate(db);
                    if (stored > 0 && stored < version)
                    {
                        foreach (var schema in schemas) schema.OnUpgrade(db, stored, version);
                    }
                    if (stored != version) db.UserVersion = version;
                });

                if (seeds != null)
                {
                    foreach (var schema in schemas)
                    {
                        db.InTransaction(() => schema.SeedIfEmpty(db, seeds));
                    }
                }
                return db;
            }
            catch
            {
                db.Dispose();
                throw;
            }
        }

        /// <summary>Verdadero si la tabla no tiene filas (para los seeds idempotentes).</summary>
        public static bool IsEmpty(IDb db, string table) => db.Scalar<long>($"SELECT COUNT(*) FROM {table}") == 0;

        private sealed class ColumnInfo
        {
            public string Name { get; set; }
        }

        /// <summary>Si <paramref name="table"/> tiene la columna (nombres fijos del código, nunca del usuario).</summary>
        public static bool HasColumn(IDb db, string table, string column)
        {
            foreach (var info in db.Query<ColumnInfo>($"PRAGMA table_info({table})"))
            {
                if (string.Equals(info.Name, column, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        /// <summary>
        /// Añade la columna si falta (para llamar desde <c>OnCreate</c>, que corre en cada arranque: así una tabla
        /// creada por una versión anterior la recibe antes de los índices y los seeds).
        /// </summary>
        public static void AddColumnIfMissing(IDb db, string table, string column, string definition)
        {
            if (!HasColumn(db, table, column)) db.Execute($"ALTER TABLE {table} ADD COLUMN {column} {definition}");
        }
    }
}
