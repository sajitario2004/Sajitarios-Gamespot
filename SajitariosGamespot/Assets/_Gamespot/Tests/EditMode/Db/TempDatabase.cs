using System;
using System.Collections.Generic;
using System.IO;
using Gamespot.Core.Runtime.Db;

namespace Gamespot.Tests.EditMode.Db
{
    /// <summary>Base de datos en un archivo temporal que se borra al terminar el test.</summary>
    public sealed class TempDatabase : IDisposable
    {
        public TempDatabase()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"gamespot_test_{Guid.NewGuid():N}.db");
        }

        public string Path { get; }

        public SqliteDb Db { get; private set; }

        /// <summary>Abre con <see cref="AppDatabase.Open"/> (esquemas + seeds).</summary>
        public SqliteDb Open(IReadOnlyList<IDbSchema> schemas, ISeedSource seeds, int version = AppDatabase.CurrentVersion)
        {
            Db?.Dispose();
            Db = AppDatabase.Open(Path, schemas, seeds, version);
            return Db;
        }

        /// <summary>Abre el archivo sin esquemas (para tests de repositorios con DDL directo).</summary>
        public SqliteDb OpenRaw()
        {
            Db?.Dispose();
            Db = SqliteDb.Open(Path);
            return Db;
        }

        public void Close()
        {
            Db?.Dispose();
            Db = null;
        }

        public void Dispose()
        {
            Close();
            try
            {
                if (File.Exists(Path)) File.Delete(Path);
            }
            catch (IOException)
            {
                // Si el archivo sigue bloqueado se queda en la carpeta temporal del sistema.
            }
        }
    }

    /// <summary>Seeds en memoria para los tests.</summary>
    public sealed class DictionarySeedSource : ISeedSource
    {
        private readonly Dictionary<string, string> _seeds;

        public DictionarySeedSource(Dictionary<string, string> seeds) => _seeds = seeds;

        public string Load(string name) =>
            _seeds.TryGetValue(name, out var text) ? text : throw new FileNotFoundException($"No hay seed {name} en el test");
    }
}
