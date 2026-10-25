using System;

namespace Gamespot.Core
{
    /// <summary>
    /// <see cref="IRandomProvider"/> sobre <see cref="System.Random"/>. Con semilla es determinista
    /// (misma semilla, misma secuencia); sin semilla usa la del sistema.
    /// Ojo: la secuencia no coincide con la de <c>Random(seed)</c> de Dart.
    /// </summary>
    public sealed class SeededRandom : IRandomProvider
    {
        private readonly System.Random _random;

        /// <summary>Generador sin semilla fija (el de la app).</summary>
        public SeededRandom()
        {
            _random = new System.Random();
        }

        /// <summary>Generador determinista para tests.</summary>
        public SeededRandom(int seed)
        {
            _random = new System.Random(seed);
        }

        public int NextInt(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max), max, "max tiene que ser mayor que 0");
            return _random.Next(max);
        }

        public double NextDouble() => _random.NextDouble();

        public bool NextBool() => _random.Next(2) == 1;
    }
}
