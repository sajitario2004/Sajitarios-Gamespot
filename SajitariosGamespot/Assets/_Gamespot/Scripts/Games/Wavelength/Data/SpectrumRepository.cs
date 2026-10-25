using System;
using System.Collections.Generic;
using System.Linq;
using Gamespot.Core;
using Gamespot.Core.Runtime.Db;
using Gamespot.Games.Wavelength.Domain;

namespace Gamespot.Games.Wavelength.Data
{
    /// <summary>Tabla <c>wavelength_spectra</c> y su seed (<c>[{izquierda, derecha}]</c>, 80 pares).</summary>
    public static class WavelengthSchema
    {
        public const string Table = "wavelength_spectra";
        public const string SeedName = "wavelength_spectra";

        public static void Create(IDb db) =>
            db.Execute("CREATE TABLE IF NOT EXISTS wavelength_spectra (id INTEGER PRIMARY KEY AUTOINCREMENT, izquierda TEXT NOT NULL, " +
                       "derecha TEXT NOT NULL, is_seed INTEGER NOT NULL DEFAULT 0)");

        public static int SeedIfEmpty(IDb db, ISeedSource seeds)
        {
            if (!AppDatabase.IsEmpty(db, Table)) return 0;
            var inserted = 0;
            foreach (var (left, right) in Parse(seeds.Load(SeedName)))
            {
                inserted += db.Execute("INSERT INTO wavelength_spectra (izquierda, derecha, is_seed) VALUES (?, ?, 1)", left, right);
            }
            return inserted;
        }

        public static List<(string left, string right)> Parse(string json)
        {
            var result = new List<(string, string)>();
            foreach (var item in Json.ParseArray(json))
            {
                if (!(item is Dictionary<string, object> map)
                    || !(map.TryGetValue("izquierda", out var l) && l is string left && left.Trim().Length > 0)
                    || !(map.TryGetValue("derecha", out var r) && r is string right && right.Trim().Length > 0))
                {
                    throw new FormatException("Espectro de Wavelength sin 'izquierda' o 'derecha'");
                }
                result.Add((left.Trim(), right.Trim()));
            }
            return result;
        }
    }

    /// <summary>Espectros de la partida, por id.</summary>
    public sealed class SpectrumRepository : ISpectrumSource
    {
        private sealed class Row
        {
            public long Id { get; set; }
            public string Left { get; set; }
            public string Right { get; set; }
        }

        private readonly IDb _db;

        public SpectrumRepository(IDb db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public IReadOnlyList<Spectrum> GetAll() =>
            _db.Query<Row>("SELECT id AS Id, izquierda AS Left, derecha AS Right FROM wavelength_spectra ORDER BY id ASC")
                .Select(r => new Spectrum(r.Id, r.Left, r.Right)).ToList();
    }
}
