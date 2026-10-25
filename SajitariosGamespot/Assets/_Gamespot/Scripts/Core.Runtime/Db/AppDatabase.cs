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
        /// Versión del esquema en Unity. Empieza en 1 con el esquema final (v8) de la app Flutter: no hay
        /// instalaciones Unity anteriores que migrar. Cada cambio de esquema futuro sube este número.
        /// </summary>
        public const int CurrentVersion = 1;

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
                if (stored == 0)
                {
                    db.InTransaction(() =>
                    {
                        foreach (var schema in schemas) schema.OnCreate(db);
                        db.UserVersion = version;
                    });
                }
                else if (stored < version)
                {
                    db.InTransaction(() =>
                    {
                        foreach (var schema in schemas) schema.OnUpgrade(db, stored, version);
                        db.UserVersion = version;
                    });
                }
                else if (stored > version)
                {
                    throw new InvalidOperationException($"La base de datos es de una versión más nueva ({stored}) que la app ({version})");
                }

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
    }
}
